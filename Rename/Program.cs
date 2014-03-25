using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivony.Data;
using System.IO;
using System.Data;
using System.Configuration;
using System.Net;

namespace Rename
{
  public class Program
  {


    private static readonly string type = "Download";

    private static SqlDbUtility dbUtility = SqlDbUtility.Create( "ProductCenter" );
    private static readonly string logPath = ConfigurationManager.AppSettings["LogPath"] ?? @"D:\Logs";
    private static readonly string imageDownloadUrl = ConfigurationManager.AppSettings["DownloadUrl"] ?? "http://img-products.chiangcn.com/";
    private static readonly string localFilePath = ConfigurationManager.AppSettings["LocalFilePath"] ?? @"D:\Images\original";



    private static string logFilepath;


    public static void Main( string[] args )
    {


      Directory.CreateDirectory( logPath );
      Directory.CreateDirectory( localFilePath );


      try
      {

        logFilepath = Path.Combine( logPath, DateTime.UtcNow.ToString( "yyyyMM" ), "Rename_" + DateTime.UtcNow.ToString( "yyyyMMddHHmm" ) + ".log" );


        dbUtility.NonQuery( "DELETE ImagesTracking WHERE Version NOT IN ( SELECT ImageInfo FROM AuditedProducts )" );


        DownloadImages();
      }
      catch ( Exception e )
      {
        Console.WriteLine( e );
      }
    }

    private static void DownloadImages()
    {

      var products = dbUtility.Data( "SELECT SKU, ImageInfo FROM AuditedProducts WHERE ImageInfo <> 0 AND ImageInfo NOT IN ( SELECT Version FROM ImagesTracking WHERE Type = {0} )", type ).AsView();


      foreach ( var dataItem in products )
      {

        var sku = (string) dataItem["SKU"];
        var version = (int) dataItem["ImageInfo"];

        if ( string.IsNullOrWhiteSpace( sku ) || version == 0 )
        {
          Console.WriteLine( "{0} skipped.", sku );
          continue;
        }

        var count = DownloadImages( sku, version );

        if ( count > 0 )
          dbUtility.NonQuery( "INSERT INTO ImagesTracking ( Version, SKU, Type, Images ) VALUES ( {...} )", version, sku, type, count );
      }
    }

    private static int DownloadImages( string sku, int version )
    {
      try
      {
        var client = new WebClient();


        client.DownloadData( DownloadUrl( sku, version, 1 ) );//先尝试下载第一张图，如果第一张图就下载不了，则直接退出。
        DeleteFiles( sku );

        int i = 0;

        while ( true )
        {
          i++;

          try
          {
            var url = DownloadUrl( sku, version, i );
            var filepath = FileName( sku, i );
            Directory.CreateDirectory( Path.GetDirectoryName( filepath ) );

            client.DownloadFile( url, filepath );
            Log( string.Format( "{0} => {1}", url, filepath ) );

            if ( i == 1 )
            {
              filepath = FileName( sku );
              client.DownloadFile( url, filepath );
              Log( string.Format( "{0} => {1}", url, filepath ) );
            }



          }
          catch ( WebException e )
          {
            var response = (HttpWebResponse) e.Response;
            if ( response != null && response.StatusCode == HttpStatusCode.NotFound )
              break;

            else
              throw;
          }
        }

        return i - 1;
      }
      catch ( Exception e )//发生异常后继续处理后面的
      {
        Console.WriteLine( e );
        return 0;
      }
    }

    private static Uri DownloadUrl( string sku, int version, int i )
    {
      var baseUrl =  new Uri( imageDownloadUrl );
      var path = string.Format( "{0}/{1}-{2}.jpg", version, sku, i );
      return new Uri( baseUrl, path );
    }


    private static string FileName( string sku )
    {
      var path = string.Format( @"{0}\{1}", sku.Substring( 0, 2 ), sku.Substring( 0, sku.Length - 1 ) );
      return Path.Combine( localFilePath, path, string.Format( "{0}.jpg", sku ) );
    }

    private static string FileName( string sku, int i )
    {
      var path = string.Format( @"{0}\{1}", sku.Substring( 0, 2 ), sku.Substring( 0, sku.Length - 1 ) );
      return Path.Combine( localFilePath, path, string.Format( "{0}-{1}.jpg", sku, i ) );
    }





    private static void DeleteFiles( string sku )
    {
      var directory = new DirectoryInfo( Path.Combine( localFilePath, GetPath( sku ) ) );

      if ( !directory.Exists )
        return;

      foreach ( var file in directory.EnumerateFiles( sku + "-*.*" ) )
        file.Delete();

      foreach ( var file in directory.EnumerateFiles( sku + ".*" ) )
        file.Delete();
    }


    private static void Log( string message )
    {
      Console.WriteLine( message );

      Directory.CreateDirectory( Path.GetDirectoryName( logFilepath ) );

      File.AppendAllText( logFilepath, message + Environment.NewLine );
    }

    private static string GetPath( string sku )
    {
      return string.Format( @"{0}\{1}", sku.Substring( 0, 2 ), sku.Substring( 0, sku.Length - 1 ) );
    }

  }
}
