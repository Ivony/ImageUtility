using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ivony.Data;
using Ivony.Fluent;

namespace ImageFileIndex
{
  class Program
  {

    public static SqlDbUtility dbUtility = new SqlDbUtility( "Data Source=10.21.0.9,6818;Initial Catalog=SCM_MIRROR_DB;User ID=chiangdba;Password=ZbdSv75$Zfz?;MultipleActiveResultSets=True" );

    private static readonly string rootPath = @"D:\images.chiangcn.com";


    private static readonly string server = "IMG1";

    static void Main( string[] args )
    {
      try
      {

        var directories = new HashSet<string>();
        foreach ( var directory in Directory.EnumerateDirectories( rootPath, "*", SearchOption.AllDirectories ) )
        {
          var files = Directory.EnumerateFiles( directory, "*", SearchOption.TopDirectoryOnly ).ToArray();
          if ( files.Any() )
          {
            directories.Add( directory );
            dbUtility.NonQuery( "DELETE ImageFiles WHERE Directory = {0}", directory );
            foreach ( var file in files )
              dbUtility.NonQuery( "INSERT INTO ImageFiles ( Filename, Directory, Server, LastModified ) VALUES ( {...} )", Path.GetFileName( file ), directory, server, File.GetLastWriteTimeUtc( file ) );

            Console.WriteLine( directory );
          }
        }

        var removedDirectories = dbUtility.Data( "SELECT DISTINCT Directory FROM ImageFiles" ).Column<string>().Except( directories );
        foreach ( var directory in removedDirectories )
          dbUtility.NonQuery( "DELETE ImageFiles WHERE Directory = {0}", directory );


      }
      catch ( Exception e )
      {
        Console.WriteLine( e );
      }
    }
  }

  public class FileItem : IEquatable<FileItem>
  {

    public string Filepath { get; set; }

    public DateTime _last;
    public DateTime LastModified
    {
      get { return _last; }

      set { _last = new DateTime( value.Ticks / 10000000 * 10000000 ); }
    }

    public bool Equals( FileItem other )
    {
      if ( other == null )
        return false;

      return other.Filepath.EqualsIgnoreCase( Filepath ) && other.LastModified == LastModified;
    }

    public override bool Equals( object obj )
    {
      return Equals( obj as FileItem );
    }

    public override int GetHashCode()
    {
      return StringComparer.OrdinalIgnoreCase.GetHashCode( Filepath ) ^ LastModified.GetHashCode();
    }
  }

}
