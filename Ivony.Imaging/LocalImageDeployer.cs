﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{


  /// <summary>
  /// 本地发布任务，将图片发布到本地文件系统。
  /// </summary>
  public class LocalImageDeployer : ImageDeployer
  {

    public LocalImageDeployer( string rootPath )
    {

      if ( rootPath == null )
        throw new ArgumentNullException( "rootPath" );

      RootPath = rootPath;
    }


    public string RootPath
    {
      get;
      private set;
    }


    public async override Task<string> DeployImageAsync( string filepath )
    {

      var destination = GetDestinationPath( filepath );

      using ( var sourceStream = File.OpenRead( filepath ) )
      {
        using ( var destinationStream = File.OpenWrite( destination ) )
        {
          await sourceStream.CopyToAsync( destinationStream );
        }
      }

      return destination;

    }

    protected virtual string GetDestinationPath( string filepath )
    {
      Directory.CreateDirectory( RootPath );
      return Path.Combine( RootPath, Path.GetFileName( filepath ) );
    }
  }
}
