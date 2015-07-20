using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
namespace Ivony.Imaging
{
  public abstract class ImageDeployer
  {
    /// <summary>
    /// 保存或者部署图片
    /// </summary>
    /// <param name="filename">文件名</param>
    /// <param name="image">图片对象</param>
    /// <returns>图片访问URL</returns>
    public abstract Task DeployImageAsync( string filepath );

    public static ImageDeployer operator +( ImageDeployer x, ImageDeployer y )
    {
      return new MulticastImageDeployer( x, y );
    }

  }
}
