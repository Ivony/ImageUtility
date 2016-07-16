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
    /// <param name="context">图片处理上下文对象</param>
    /// <param name="filename">文件名</param>
    /// <returns>图片访问URL</returns>
    public abstract Task DeployImageAsync( ImageWorkflowContext context, string filepath );

    public static ImageDeployer operator +( ImageDeployer x, ImageDeployer y )
    {
      return new MulticastImageDeployer( x, y );
    }

  }
}
