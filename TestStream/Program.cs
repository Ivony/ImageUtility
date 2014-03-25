using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TestStream
{
  class Program
  {
    static void Main( string[] args )
    {
      using ( var stream = File.OpenWrite( @"C:\test.txt:test.xml" ) )
      {
        using ( var writer = new StreamWriter( stream ) )
        {
          writer.Write( "abc" );

          writer.Flush();
        }
      }

    }
  }
}
