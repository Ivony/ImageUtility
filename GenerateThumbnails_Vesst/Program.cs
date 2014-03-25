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


namespace GenerateThumbnails
{
  class Program
  {

    private static readonly string sourcePath = @"D:\images.chiangcn.com\original";
    private static readonly string logPath = @"D:\Logs";

    public static SqlDbUtility dbUtility = new SqlDbUtility( "Data Source=10.21.0.9,6818;Initial Catalog=PRODUCTCENTER_DB;User ID=chiangdba;Password=ZbdSv75$Zfz?;MultipleActiveResultSets=True" );



    private static readonly Size tinySpec = new Size( 45, 64 );
    private static readonly Size smallSpec = new Size( 90, 126 );
    private static readonly Size mediumSpec = new Size( 170, 240 );
    private static readonly Size bigSpec = new Size( 333, 470 );
    private static readonly Size hugeSpec = new Size( 1332, 1880 );



    private static string logFilepath;

    private static bool rebuildMode = false;


    private static readonly Regex skuRegex = new Regex( @"^\s*[a-zA-Z]{2}[0-9]{3,4}[a-zA-Z]\s*$", RegexOptions.Compiled );


    static void Main( string[] args )
    {

      try
      {

        if ( args.Contains( "-rebuild" ) )
          rebuildMode = true;


        var sourceDirectory = new DirectoryInfo( sourcePath );



        logFilepath = Path.Combine( logPath, DateTime.UtcNow.ToString( "yyyyMM" ), "GenerateThumbnails_" + DateTime.UtcNow.ToString( "yyyyMMddHHmm" ) + ".log" );






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


        DataTable data;

        if ( rebuildMode )
          data = dbUtility.ExecuteData( "SELECT SKU, Generated FROM ImageGenerateTracking WHERE Type='original_new' AND SKU IN ( SELECT SKU FROM _VesstSKUs )" );

        else
          data = dbUtility.ExecuteData( "SELECT SKU, Generated FROM ImageGenerateTracking WHERE Type='original_new' AND SKU IN ( SELECT SKU FROM _VesstSKUs ) EXCEPT (SELECT SKU, Generated FROM ImageGenerateTracking WHERE Type='Vesst_Thumbnails')" );


        data.Rows.Cast<DataRow>()
        .ForAll( dataItem =>
        {
          var sku = (string) dataItem["SKU"];
          var images = (int) dataItem["Generated"];



          try
          {
            var files = GetFiles( sku, images ).ToArray();
            files.ForAll( GenerateThumbnail );


            var count = files.Count( f => f.Contains( '-' ) );

            dbUtility.ExecuteNonQuery( @"
DELETE ImageGenerateTracking WHERE SKU = {0} AND Type = {1};
INSERT INTO ImageGenerateTracking ( SKU, Type, Generated, LastUpdatedOn ) VALUES( {0}, {1}, {2}, {3} );"
             , sku, "Vesst_Thumbnails", count, DateTime.Now );

            Log( string.Format( "{0} has {1} image(s).", sku, count ) );

          }
          catch ( Exception e )
          {
            Console.WriteLine( e );

            Log( string.Format( "{0} has exceptions.", sku ) );
          }

        } );


      }
      catch ( Exception e )
      {
        Console.WriteLine( e );
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

            var path = GetPath( filepath, tinySpec );
            using ( var stream = File.OpenWrite( path ) )
            {
              GenerateThumbnail( image, tinySpec, stream );
            }
            Log( string.Format( "{0} => {1}", filepath, path ) );


            path = GetPath( filepath, smallSpec );
            using ( var stream = File.OpenWrite( path ) )
            {
              GenerateThumbnail( image, smallSpec, stream );
            }
            Log( string.Format( "{0} => {1}", filepath, path ) );


            path = GetPath( filepath, mediumSpec );
            using ( var stream = File.OpenWrite( path ) )
            {
              GenerateThumbnail( image, mediumSpec, stream );
            }
            Log( string.Format( "{0} => {1}", filepath, path ) );


            path = GetPath( filepath, bigSpec );
            using ( var stream = File.OpenWrite( path ) )
            {
              GenerateThumbnail( image, bigSpec, stream );
            }
            Log( string.Format( "{0} => {1}", filepath, path ) );

            path = GetPath( filepath, hugeSpec );
            using ( var stream = File.OpenWrite( path ) )
            {
              GenerateThumbnail( image, hugeSpec, stream );
            }
            Log( string.Format( "{0} => {1}", filepath, path ) );

          }
        }
      }
      catch ( OutOfMemoryException )
      {
        Log( filepath + " is not an image" );
        File.AppendAllLines( logFilepath, new[] { filepath + " is not an image" } );
      }
    }

    private static string GetPath( string filepath, Size specSize )
    {
      var directoryName = string.Format( "{0}x{1}", specSize.Width, specSize.Height );
      var path = filepath.Remove( 0, sourcePath.Length ).TrimStart( '\\' );
      var result = Path.Combine( @"D:\images.chiangcn.com\", directoryName, path );

      Directory.CreateDirectory( Path.GetDirectoryName( result ) );

      return result;
    }

    public static void GenerateThumbnail( Image source, Size specSize, Stream targetStream )
    {

      source.ZoomTo( specSize, Color.Empty, InterpolationMode.HighQualityBicubic, SmoothingMode.None, new Size( 2, 2 ) ).SaveAsJpeg( targetStream, 95 );
    }


    private static void Log( string message )
    {
      Console.WriteLine( message );

      Directory.CreateDirectory( Path.GetDirectoryName( logFilepath ) );

      File.AppendAllText( logFilepath, message + Environment.NewLine );
    }

  }
}
