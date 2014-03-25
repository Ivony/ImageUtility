using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Ivony.Fluent;
using Ivony.Data;
using System.Data;
using System.Configuration;

namespace Watermark
{


  public class Program
  {

    public static string localPath = ConfigurationManager.AppSettings["LocalFilePath"] ?? @"D:\Images";


    public static void Main( string[] args )
    {
      string watermarks = Path.Combine( localPath, "watermarks" );

      Directory.EnumerateFiles( watermarks, "*", SearchOption.AllDirectories )
        .AsParallel()
        .ForAll( watermarkFilepath => new WaterMarker( watermarkFilepath, Path.Combine( localPath, "860x666" ), DateTime.Now.AddMinutes( 30 ) ).MakeWaterMark() );

    }
  }




  public class WaterMarker
  {


    private static readonly string logPath = ConfigurationManager.AppSettings["LogPath"] ?? @"D:\Logs";
    public static SqlDbUtility dbUtility = SqlDbUtility.Create( "ProductCenter" );


    private string _logFilepath;
    private string _watermarkName;
    private string _watermarkPath;

    private string _sourcePath;

    private DateTime _endTime;


    public WaterMarker( string watermarkFilepath, string sourcePath, DateTime endTime )
    {
      _watermarkPath = watermarkFilepath;
      _watermarkName = Path.GetFileNameWithoutExtension( watermarkFilepath );
      _logFilepath = Path.Combine( logPath, DateTime.UtcNow.ToString( "yyyyMM" ), "WaterMarker_" + _watermarkName + "_" + DateTime.UtcNow.ToString( "yyyyMMddHHmm" ) + ".log" );

      _sourcePath = sourcePath;

      _endTime = endTime;
    }


    public void MakeWaterMark()
    {
      using ( var image = Image.FromFile( _watermarkPath ) )
      {

        var begin = DateTime.Now;


        var data = dbUtility.Data( "SELECT SKU, Version, Images FROM ImagesTracking WHERE Type='Thumbnails' EXCEPT (SELECT SKU, Version, Images FROM ImagesTracking WHERE Type = {0} )", _watermarkName );

        data.Rows.Cast<DataRow>()
        .ForAll( dataItem =>
        {
          var sku = (string) dataItem["SKU"];
          var version = (int) dataItem["Version"];
          var images = (int) dataItem["Images"];


          try
          {
            var files = GetFiles( sku, images ).ToArray();
            files.ForAll( file =>
            {

              using ( var stream = File.OpenRead( file ) )
              {
                var targetFile = GetPath( _sourcePath, _watermarkName, file );
                MakeWatermark( stream, image, targetFile );

                Log( string.Format( "{0} => {1}", file, targetFile ) );
              }

            } );


            var count = files.Count( f => f.Contains( '-' ) );

            dbUtility.NonQuery( @"
DELETE ImagesTracking WHERE SKU = {0} AND Type = {2};
INSERT INTO ImagesTracking ( SKU, Version, Type, Images ) VALUES( {0}, {1}, {2}, {3} );"
 , sku, version, _watermarkName, count );

            Log( string.Format( "{0} has {1} image(s).", sku, count ) );


          }
          catch ( Exception e )
          {
            Console.WriteLine( e );

            Log( string.Format( "{0} has exceptions.", sku ) );
          }

          if ( DateTime.Now > _endTime )
            throw new Exception( "Time out." );

        } );
      }

    }


    private static string GetPath( string sourcePath, string watermarkName, string filepath )
    {
      var targetFile = Path.Combine( sourcePath.TrimEnd( '\\' ).Remove( sourcePath.LastIndexOf( '\\' ) ), watermarkName, filepath.Remove( 0, sourcePath.Length ).TrimStart( '\\' ) );
      return targetFile;
    }


    public static void MakeWatermark( Stream sourceFile, Image watermark, string targetFile )
    {
      using ( var image = Image.FromStream( sourceFile ) )
      {

        if ( image.Width < watermark.Width || image.Height < watermark.Height )
          throw new InvalidOperationException();

        var g = Graphics.FromImage( image );

        g.CompositingQuality = CompositingQuality.HighQuality;
        g.CompositingMode = CompositingMode.SourceOver;

        var targetRectangle = new Rectangle( new Point( (image.Width - watermark.Width) / 2, (image.Height - watermark.Height) / 2 ), watermark.Size );

        g.DrawImage( watermark, targetRectangle );


        var encoder = ImageCodecInfo.GetImageEncoders().First( e => e.MimeType == "image/jpeg" );
        var parameters = new EncoderParameters();
        parameters.Param[0] = new EncoderParameter( Encoder.Quality, 95L );

        Directory.CreateDirectory( Path.GetDirectoryName( targetFile ) );

        image.Save( targetFile, encoder, parameters );
      }
    }


    private IEnumerable<string> GetFiles( string sku, int images )
    {
      if ( images <= 0 )
        yield break;

      var defaultImagePath = string.Format( @"{0}\{1}\{2}\{3}.jpg", _sourcePath, sku.Substring( 0, 2 ), sku.Substring( 0, sku.Length - 1 ), sku );
      if ( File.Exists( defaultImagePath ) )
        yield return defaultImagePath;
      else
        Log( defaultImagePath + " not found." );

      for ( int index = 1; index <= images; index++ )
      {
        var path = string.Format( @"{0}\{1}\{2}\{3}-{4}.jpg", _sourcePath, sku.Substring( 0, 2 ), sku.Substring( 0, sku.Length - 1 ), sku, index );
        if ( File.Exists( path ) )
          yield return path;
        else
          Log( path + " not found." );
      }
    }



    private void Log( string message )
    {
      Console.WriteLine( message );

      Directory.CreateDirectory( Path.GetDirectoryName( _logFilepath ) );

      File.AppendAllText( _logFilepath, message + Environment.NewLine );
    }
  }

}
