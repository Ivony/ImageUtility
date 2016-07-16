using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace Ivony.Imaging
{
  public class VirtualPathDeployTask : ImageDeployer
  {

    public VirtualPathDeployTask( string virtualRoot, string physicalRoot = null, string host = null )
    {
      VirtualRoot = virtualRoot;
      Host = host;
    }


    /// <summary>
    /// 发布的根虚拟路径
    /// </summary>
    public string VirtualRoot { get; private set; }


    /// <summary>
    /// 发布的根虚拟路径
    /// </summary>
    public string PhysicalRoot { get; private set; }


    /// <summary>
    /// 发布站点地址
    /// </summary>
    public string Host { get; private set; }



    /// <summary>
    /// 发布指定位置的图片
    /// </summary>
    /// <param name="filepath">待发布的图片地址</param>
    /// <returns>发布后的 URL</returns>
    public async override Task DeployImageAsync( ImageWorkflowContext context, string filepath )
    {

      string virtualPath, physicalPath, deployPath = GetDeployPath( filepath );

      virtualPath = VirtualPathUtility.ToAbsolute( VirtualPathUtility.Combine( VirtualRoot, deployPath ) );

      if ( PhysicalRoot == null )
        physicalPath = HostingEnvironment.MapPath( virtualPath );

      else
        physicalPath = Path.Combine( PhysicalRoot, deployPath );



      Directory.CreateDirectory( Path.GetDirectoryName( physicalPath ) );

      using ( var sourceStream = File.OpenRead( filepath ) )
      {
        using ( var destinationStream = File.OpenWrite( physicalPath ) )
        {
          await sourceStream.CopyToAsync( destinationStream );
        }
      }


      string host = Host;

      if ( host == null )
      {
        if ( HttpContext.Current == null )
          throw new InvalidOperationException();

        host = HttpContext.Current.Request.Url.GetLeftPart( UriPartial.Authority );
      }
    }



    /// <summary>
    /// 获取目标文件虚拟路径
    /// </summary>
    /// <param name="filepath">要发布的文件的原路径</param>
    /// <returns>目标文件位置</returns>
    protected virtual string GetDeployPath( string filepath )
    {
      return Path.GetFileName( filepath );
    }


  }
}
