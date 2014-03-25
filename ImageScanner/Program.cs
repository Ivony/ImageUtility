using Ivony.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivony.Fluent;
using System.IO;
using System.Threading;

namespace ImageBackup
{
  class Program
  {


    private static SqlDbUtility ImagesDb = SqlDbUtility.Create( "Images" );

    private static string imagesPathRoot = @"D:\images.chiangcn.com";


    public static void Main( string[] args )
    {

      var typeDirectories = Directory.EnumerateDirectories( imagesPathRoot ).ToArray();

      foreach ( var p in typeDirectories )
        Console.WriteLine( p );



      while ( true )
      {
        try
        {
          var data = ImagesDb.T( "SELECT TOP 1000 SKU FROM ProductImageSummary WHERE Modified = 1" ).ExecuteFirstColumn<string>();
          if ( !data.Any() )
            break;

          data.AsParallel().ForAll( sku => ScanProductImages( sku, typeDirectories ) );
        }
        catch ( Exception e )
        {

          Console.WriteLine( e );
          Console.WriteLine( "error, wait for 10 second" );

          Thread.Sleep( new TimeSpan( 0, 0, 10 ) );
        }
      }

    }

    private static void ScanProductImages( string sku, string[] typeDirectories )
    {
      var now = DateTime.UtcNow;
      foreach ( var path in typeDirectories )
      {

        for ( int i = 0; i < 20; i++ )
        {
          var imagePath = GetImagePath( path, sku, i );
          if ( File.Exists( imagePath ) )
          {
            ImagesDb.T( "DELETE ProductImages WHERE ImagePath = {0}", imagePath ).ExecuteNonQuery();
            ImagesDb.T( "INSERT INTO ProductImages ( ImagePath, LastModified, SKU, Type, [Index] ) VALUES ( {...} );", imagePath, File.GetLastWriteTimeUtc( imagePath ), sku, Path.GetFileName( path ), i ).ExecuteNonQuery();
            Console.WriteLine( imagePath );
          }

        }
      }

      ImagesDb.T( "UPDATE ProductImageSummary SET Modified = 0, LastScanned = {1} WHERE SKU = {0}", sku, now ).ExecuteNonQuery();
    }

    private static string GetImagePath( string path, string sku, int i )
    {
      if ( i == 0 )
        return string.Format( @"{0}\{1}\{2}\{3}.jpg", path, sku.Remove( 2 ), sku.Remove( sku.Length - 1 ), sku );

      return string.Format( @"{0}\{1}\{2}\{3}-{4}.jpg", path, sku.Remove( 2 ), sku.Remove( sku.Length - 1 ), sku, i );
    }
  }
}
