using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{

  /// <summary>
  /// 定义一个图片工作流
  /// </summary>
  public class ImageWorkflow
  {
    /// <summary>
    /// 需要对图片执行的处理任务
    /// </summary>
    protected ImageProcessTask Task { get; private set; }

    /// <summary>
    /// 图片发布任务，用于将图片发布到一处或多处
    /// </summary>
    protected ImageDeployer Deployer { get; private set; }




    public ImageWorkflow( string name, IImageCodec codec, ImageProcessTask task, ImageDeployer deployer )
    {
      Name = name;
      Codec = codec;
      Task = task;
      Deployer = deployer;

    }




    /// <summary>
    /// 异步处理图片文件
    /// </summary>
    /// <param name="url">要处理的图片地址</param>
    /// <returns>图片发布后的 URL</returns>
    public async Task ProcessAsync( string url )
    {

      Uri location;

      if ( !Uri.TryCreate( url, UriKind.Absolute, out location ) )
        throw new ArgumentException( "url 参数必须为一个合法有效的绝对 URL", "url" );



      Stream data = await LoadFile( location );


      string tempFilepath;

      using ( var image = Bitmap.FromStream( data ) )
      {
        Image result;

        var task = GetProcessTask( image );
        if ( task == null )
        {
          result = image;
        }
        else
        {
          result = task.ProcessImage( image );
        }

        await Codec.SaveAsync( result, tempFilepath = GetFilepath( url ) );
      }


      await Deployer.DeployImageAsync( tempFilepath );
      File.Delete( tempFilepath );
    }



    /// <summary>
    /// 加载文件内容
    /// </summary>
    /// <param name="location">文件地址</param>
    /// <returns></returns>
    private static async Task<Stream> LoadFile( Uri location )
    {
      MemoryStream data;
      if ( location.IsFile )
      {
        using ( var stream = File.OpenRead( location.LocalPath ) )
        {
          data = new MemoryStream();
          await stream.CopyToAsync( data );
        }
      }
      else
      {
        using ( var client = new HttpClient() )
        {
          var response = await client.GetAsync( location.AbsoluteUri );
          data = new MemoryStream( await response.Content.ReadAsByteArrayAsync() );
        }
      }

      data.Seek( 0, SeekOrigin.Begin );

      return data;
    }

    protected virtual ImageProcessTask GetProcessTask( Image image )
    {
      return Task;
    }





    /// <summary>
    /// 获取文件保存路径，默认将使用随机文件名保存在临时目录
    /// </summary>
    /// <param name="filepath">上传文件名称</param>
    /// <returns></returns>
    protected virtual string GetFilepath( string filepath )
    {
      return Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString( "N" ) + Codec.FileExensions );
    }




    /// <summary>
    /// 获取工作流名称
    /// </summary>
    public string Name { get; private set; }


    /// <summary>
    /// 图片编码格式
    /// </summary>
    public IImageCodec Codec { get; private set; }
  }
}
