using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{
  public class HashbasedImageWorkflow : ImageWorkflow
  {

    protected HashAlgorithm Hash { get; }


    public HashbasedImageWorkflow( string name, IImageCodec codec, ImageProcessTask task, ImageDeployer deployer, HashAlgorithm hash = null ) : base( name, codec, task, deployer )
    {
      Hash = hash ?? new SHA256Managed();
    }


    protected override Task<string> SaveImage( ImageWorkflowContext context, Image image )
    {

      return base.SaveImage( context, image );
    }



    protected override string GetFilepath( ImageWorkflowContext context, byte[] imageData )
    {
      return Path.Combine( Path.GetTempPath(), ComputeHash( imageData ) + Codec.FileExensions );
    }


    protected string ComputeHash( byte[] imageData )
    {
      var result = Hash.ComputeHash( imageData );
      var builder = new StringBuilder( result.Length * 2 );

      foreach ( var b in result )
        builder.AppendFormat( "{0:x2}", b );

      return builder.ToString();
    }
  }
}
