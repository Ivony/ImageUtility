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

    public VirtualPathDeployTask( string virtualRoot = null, string physicalRoot = null, string key = "virtualpath-deployer" )
    {
      VirtualRoot = virtualRoot ?? "~/";
      PhysicalRoot = physicalRoot;
      TargetPathKey = key;
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
    /// 目标路径存放的字典键位置
    /// </summary>
    public string TargetPathKey { get; }


    /// <summary>
    /// 发布指定位置的图片
    /// </summary>
    /// <param name="filepath">待发布的图片地址</param>
    /// <returns></returns>
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


      context.Data[TargetPathKey] = virtualPath;
      context.Data[TargetPathKey + ".physical"] = physicalPath;

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
