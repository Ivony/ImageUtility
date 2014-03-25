using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ivony.Data;

namespace ImagesCounter
{
  public class Program
  {

    private static readonly string imageDirectory = @"D:\images.chiangcn.com\original";
    public static SqlDbUtility dbUtility = new SqlDbUtility( "Data Source=10.21.0.9,6818;Initial Catalog=SCM_MIRROR_DB;User ID=chiangdba;Password=ZbdSv75$Zfz?;MultipleActiveResultSets=True" );


    public static void Main( string[] args )
    {

      new DirectoryInfo( imageDirectory ).EnumerateDirectories( "*", SearchOption.AllDirectories ).AsParallel().ForAll( directory =>
        {
          var imagesCount = directory.EnumerateFiles()
            .Select( file => Path.GetFileNameWithoutExtension( file.Name ) )
            .Where( name => name.Contains( '-' ) )
            .Select( name => name.Substring( 0, name.LastIndexOf( '-' ) ) )
            .GroupBy( sku => sku )
            .Select( group => new
              {
                SKU = group.Key,
                Count = group.Count()
              } );


          foreach ( var item in imagesCount )
          {

            if ( dbUtility.ExecuteScalar( "SELECT SKU FROM GoodsData WHERE SKU = {0}", item.SKU ) == null )
              dbUtility.ExecuteNonQuery( "INSERT INTO GoodsData ( SKU, Images ) VALUES ( {0}, {1} )", item.SKU, item.Count );
            else
              dbUtility.ExecuteNonQuery( "UPDATE GoodsData SET Images = {1} WHERE SKU = {0}", item.SKU, item.Count );

            Console.WriteLine( "{0} has {1} images", item.SKU, item.Count );

          }
        } );


    }
  }
}
