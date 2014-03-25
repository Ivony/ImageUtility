using Ivony.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImageBackup
{
  class Program
  {

    private static SqlDbUtility ImagesDb = SqlDbUtility.Create( "Images" );

    private static string imagesPathRoot = @"\\IMG\images.chiangcn.com2";

    static void Main( string[] args )
    {

      while ( true )
      {
        var images = ImagesDb.T( "SELECT TOP 100 SKU FROM ProductImageSummary WHERE Archived = 0 AND Modified = 0" ).ExecuteFirstColumn<string>();


        if ( images.Any() )
          images.AsParallel().ForAll( Backup );

        else
        {
          Console.WriteLine( "there is no data, sleeep." );
          Thread.Sleep( new TimeSpan( 0, 1, 0 ) );
        }

      }
    }

    private static void Backup( string sku )
    {
      try
      {
        string type = null;
        var availableTypes = ImagesDb.T( "SELECT Type FROM ProductImages WHERE SKU = {0} GROUP BY Type", sku ).ExecuteFirstColumn<string>();

        if ( availableTypes.Contains( "origin" ) )
          type = "origin";

        else if ( availableTypes.Contains( "1332x1880" ) )
          type = "1332x1880";

        else if ( availableTypes.Contains( "860x666" ) )
          type = "860x666";

        else
        {
          ImagesDb.T( "UPDATE ProductImageSummary SET Archived = 1, Images = 0 WHERE SKU = {0}", sku ).ExecuteNonQuery();
          Console.WriteLine( "sku {0} has no image", sku );
          return;
        }

        var images = ImagesDb.T( "SELECT ImagePath, [Index], LastModified FROM ProductImages WHERE SKU = {0} AND Type = {1}", sku, type ).ExecuteDynamics();

        foreach ( var dataItem in images )
        {

          int index = dataItem.Index;
          string source = dataItem.ImagePath;
          DateTime lastModified = dataItem.LastModified;

          var destination = string.Format( @"{0}\{1}\{2}\{3}\{4}{5}.jpg", imagesPathRoot, "original", sku.Remove( 2 ), sku.Remove( sku.Length - 1 ), sku, index == 0 ? "" : "-" + index );
          Directory.CreateDirectory( Path.GetDirectoryName( destination ) );
          File.Copy( source, destination, true );

          ImagesDb.T( "INSERT INTO ProductImageBackupLogs ( SourcePath, DestinationPath, LastModified, BackupDate ) VALUES ( {...} )", source, destination, lastModified, DateTime.UtcNow ).ExecuteNonQuery();
        }

        ImagesDb.T( "UPDATE ProductImageSummary SET Archived = 1, Images = {1} WHERE SKU = {0}", sku, ImagesDb.T( "SELECT MAX( [Index] ) FROM ProductImages WHERE SKU = {0} AND Type = {1}", sku, type ).ExecuteScalar<int>() ).ExecuteNonQuery();

        Console.WriteLine( "backup {0} OK.", sku );

      }
      catch ( Exception e )
      {
        Console.WriteLine( "backup {0} error.\n{1}", sku, e );
      }
    }
  }
}
