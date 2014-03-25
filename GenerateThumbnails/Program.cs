using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Ivony.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using Ivony.Fluent;
using System.Text.RegularExpressions;
using Ivony.Data;
using System.Data;
using System.Configuration;


namespace GenerateThumbnails
{
  class Program
  {

    private static readonly string logPath = ConfigurationManager.AppSettings["LogPath"] ?? @"D:\Logs";
    private static readonly string localPath = ConfigurationManager.AppSettings["LocalFilePath"] ?? @"D:\Images";


    private static string sourcePath;


    public static SqlDbUtility dbUtility = SqlDbUtility.Create( "ProductCenter" );



    private static readonly Size tinySpec = new Size( 45, 35 );
    private static readonly Size smallSpec = new Size( 155, 120 );
    private static readonly Size mediumSpec = new Size( 348, 270 );
    private static readonly Size largeSpec = new Size( 450, 349 );
    private static readonly Size bigSpec = new Size( 860, 666 );
    private static readonly Size wsdealSpec = new Size( 142, 142 );
    private static readonly Size wsdealSpec1 = new Size( 142, 110 );
    private static readonly Size mobile1 = new Size( 105, 105 );
    private static readonly Size mobile2 = new Size( 160, 160 );
    private static readonly Size mobile3 = new Size( 210, 210 );
    private static readonly Size mobile4 = new Size( 235, 235 );
    private static readonly Size mobile5 = new Size( 310, 240 );
    private static readonly Size mobile6 = new Size( 470, 364 );
    private static readonly Size mobile7 = new Size( 630, 488 );
    private static readonly Size mobile8 = new Size( 710, 550 );
    private static readonly Size mobile9 = new Size( 400, 310 );
    private static readonly Size mobile10 = new Size( 607, 470 );
    private static readonly Size mobile11 = new Size( 810, 627 );
    private static readonly Size mobile12 = new Size( 917, 710 );
    private static readonly Size new1 = new Size( 550, 426 );
    private static readonly Size new2 = new Size( 220, 170 );
    private static readonly Size new3 = new Size( 94, 73 );
    private static readonly Size new4 = new Size( 59, 46 );

    private static readonly Size vesst_tinySpec   = new Size( 45, 64 );
    private static readonly Size vesst_smallSpec  = new Size( 90, 126 );
    private static readonly Size vesst_mediumSpec = new Size( 170, 240 );
    private static readonly Size vesst_bigSpec    = new Size( 333, 470 );
    private static readonly Size vesst_hugeSpec   = new Size( 1332, 1880 );



    private static string logFilepath;

    private static bool rebuildMode = false;

    private static bool forceMode = false;


    private static readonly string source_type = "Download";
    private static readonly string type = "Thumbnails";


    private static readonly Regex skuRegex = new Regex( @"^\s*[a-zA-Z]{2}[0-9]{3,4}[a-zA-Z]\s*$", RegexOptions.Compiled );


    static void Main( string[] args )
    {

      if ( args.Contains( "-rebuild" ) )
        rebuildMode = true;

      if ( args.Contains( "-force" ) )
        forceMode = true;



      logFilepath = Path.Combine( logPath, DateTime.UtcNow.ToString( "yyyyMM" ), "GenerateThumbnails_" + DateTime.UtcNow.ToString( "yyyyMMddHHmm" ) + ".log" );
      sourcePath = Path.Combine( localPath, "original" );

      Directory.CreateDirectory( Path.GetDirectoryName( logFilepath ) );



      var sourceDirectory = new DirectoryInfo( sourcePath );


      try
      {

        var products = args.Where( a => skuRegex.IsMatch( a ) );
        if ( products.Any() )
        {
          var files = new List<string>();

          foreach ( var sku in products )
          {
            var directory = new DirectoryInfo( string.Format( @"{0}\{1}\{2}", sourcePath, sku.Substring( 0, 2 ), sku.Substring( 0, sku.Length - 1 ) ) );
            files.AddRange( directory.EnumerateFiles( "*" ).Where( file => file.Name.StartsWith( sku ) ).Select( f => f.FullName ) );
          }

          files.ForAll( GenerateThumbnail );

          return;
        }


        DataRow[] data;

        if ( rebuildMode )
          data = dbUtility.Data( "SELECT SKU, Version, Images FROM ImagesTracking WHERE Type={0}", source_type ).Rows.Cast<DataRow>().ToArray();

        else
          data = dbUtility.Data( "SELECT SKU, Version, Images FROM ImagesTracking WHERE Type={0} EXCEPT (SELECT SKU, Version, Images FROM ImagesTracking WHERE Type = {1})", source_type, type ).Rows.Cast<DataRow>().ToArray();

        DateTime start = DateTime.Now;


        if ( forceMode )
          data.AsParallel().ForAll( dataItem => ProcessImage( start, dataItem ) );
        else
          data.ForAll( dataItem => ProcessImage( start, dataItem ) );

      }
      catch ( Exception e )
      {
        Console.WriteLine( e );
      }
    }

    private static void ProcessImage( DateTime start, DataRow dataItem )
    {
      var sku = (string) dataItem["SKU"];
      var version = (int) dataItem["Version"];
      var images = (int) dataItem["Images"];



      try
      {
        var files = GetFiles( sku, images ).ToArray();
        files.ForAll( GenerateThumbnail );


        var count = files.Count( f => f.Contains( '-' ) );

        dbUtility.NonQuery( @"
DELETE ImagesTracking WHERE SKU = {0} AND Type = {2};
INSERT INTO ImagesTracking ( SKU, Version, Type, Images ) VALUES( {0}, {1}, {2}, {3} );
UPDATE AuditedProducts SET ImagesGenerated = 1 WHERE ImageInfo = {1};",
        sku, version, type, count );

        Log( string.Format( "{0} has {1} image(s).", sku, count ) );



      }
      catch ( Exception e )
      {
        Console.WriteLine( e );

        Log( string.Format( "{0} has exceptions.", sku ) );
      }

      if ( !rebuildMode && !forceMode && DateTime.Now - start > TimeSpan.FromMinutes( 30 ) )
      {
        Log( "time out." );
        throw new Exception( "已执行半小时，自动结束进程" );
      }
    }


    private static IEnumerable<string> GetFiles( string sku, int images )
    {
      if ( images <= 0 )
        yield break;

      var defaultImagePath = string.Format( @"{0}\{1}\{2}\{3}.jpg", sourcePath, sku.Substring( 0, 2 ), sku.Substring( 0, sku.Length - 1 ), sku );
      if ( File.Exists( defaultImagePath ) )
        yield return defaultImagePath;
      else
        Log( defaultImagePath + " not found." );

      for ( int index = 1; index <= images; index++ )
      {
        var path = string.Format( @"{0}\{1}\{2}\{3}-{4}.jpg", sourcePath, sku.Substring( 0, 2 ), sku.Substring( 0, sku.Length - 1 ), sku, index );
        if ( File.Exists( path ) )
          yield return path;
        else
          Log( path + " not found." );
      }

    }



    private static object file_sync = new object();



    public static void GenerateThumbnail( string filepath )
    {
      try
      {

        using ( var sourceStream = File.OpenRead( filepath ) )
        {

          using ( var image = Image.FromStream( sourceStream ) )
          {

            GenerateThumbnail( image, tinySpec, filepath );
            GenerateThumbnail( image, smallSpec, filepath );
            GenerateThumbnail( image, mediumSpec, filepath );
            GenerateThumbnail( image, largeSpec, filepath );
            GenerateThumbnail( image, bigSpec, filepath );
            GenerateThumbnail( image, wsdealSpec, filepath );
            GenerateThumbnail( image, wsdealSpec1, filepath );
            GenerateThumbnail( image, mobile1, filepath );
            GenerateThumbnail( image, mobile2, filepath );
            GenerateThumbnail( image, mobile3, filepath );
            GenerateThumbnail( image, mobile4, filepath );
            GenerateThumbnail( image, mobile5, filepath );
            GenerateThumbnail( image, mobile6, filepath );
            GenerateThumbnail( image, mobile7, filepath );
            GenerateThumbnail( image, mobile8, filepath );
            GenerateThumbnail( image, mobile9, filepath );
            GenerateThumbnail( image, mobile10, filepath );
            GenerateThumbnail( image, mobile11, filepath );
            GenerateThumbnail( image, mobile12, filepath );
            GenerateThumbnail( image, new1, filepath );
            GenerateThumbnail( image, new2, filepath );
            GenerateThumbnail( image, new3, filepath );
            GenerateThumbnail( image, new4, filepath );

            GenerateThumbnail( image, vesst_tinySpec  , filepath );
            GenerateThumbnail( image, vesst_smallSpec , filepath );
            GenerateThumbnail( image, vesst_mediumSpec, filepath );
            GenerateThumbnail( image, vesst_bigSpec   , filepath );
            GenerateThumbnail( image, vesst_hugeSpec  , filepath );

          }
        }
      }
      catch ( OutOfMemoryException )
      {
        Log( filepath + " is not an image" );
        File.AppendAllLines( logFilepath, new[] { filepath + " is not an image" } );
      }
    }

    private static void GenerateThumbnail( Image image, Size spec, string filepath )
    {
      var path = GetPath( filepath, spec );
      using ( var stream = File.OpenWrite( path ) )
      {
        GenerateThumbnail( image, spec, stream );
      }

      if ( !forceMode )
        Log( string.Format( "{0} => {1}", filepath, path ) );
    }

    private static string GetPath( string filepath, Size specSize )
    {
      var directoryName = string.Format( "{0}x{1}", specSize.Width, specSize.Height );
      var path = filepath.Remove( 0, sourcePath.Length ).TrimStart( '\\' );
      var result = Path.Combine( localPath, directoryName, path );

      Directory.CreateDirectory( Path.GetDirectoryName( result ) );

      return result;
    }

    public static void GenerateThumbnail( Image source, Size specSize, Stream targetStream )
    {

      source.ZoomTo( specSize, Color.White, InterpolationMode.HighQualityBicubic, SmoothingMode.HighQuality ).SaveAsJpeg( targetStream, 95 );
    }


    private static void Log( string message )
    {
      Console.WriteLine( message );

      if ( forceMode )
        return;

      Directory.CreateDirectory( Path.GetDirectoryName( logFilepath ) );

      File.AppendAllText( logFilepath, message + Environment.NewLine );
    }

  }
}
