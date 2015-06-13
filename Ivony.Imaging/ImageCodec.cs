using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ivony.Imaging
{

  /// <summary>
  /// 定义某种图像格式编码信息
  /// </summary>
  public interface IImageCodec
  {

    /// <summary>
    /// 将图像编码保存为图像文件
    /// </summary>
    /// <param name="image">图像对象</param>
    /// <param name="filepath">要保存的路径</param>
    /// <returns></returns>
    Task SaveAsync( Image image, string filepath );


    /// <summary>
    /// 获取一般情况下文件的扩展名
    /// </summary>
    string FileExensions { get; }
  }



  /// <summary>
  /// 定义某种图像格式编码信息
  /// </summary>
  public class ImageCodec : IImageCodec
  {

    /// <summary>
    /// 创建 ImageCodec 对象
    /// </summary>
    /// <param name="codec"></param>
    /// <param name="parameters"></param>
    /// <param name="extensionName"></param>
    public ImageCodec( ImageCodecInfo codec, EncoderParameters parameters, string extensionName )
    {
      CodecInfo = codec;
      CodecParameters = parameters;
      FileExensions = extensionName;
    }



    /// <summary>
    /// 获取图像编码器
    /// </summary>
    public ImageCodecInfo CodecInfo { get; private set; }

    /// <summary>
    /// 获取编码器参数
    /// </summary>
    public EncoderParameters CodecParameters { get; private set; }

    /// <summary>
    /// 获取图片文件扩展名
    /// </summary>
    public string FileExensions { get; private set; }




    /// <summary>
    /// 图片文件另存为
    /// </summary>
    /// <param name="image">图像对象</param>
    /// <param name="filepath">要保存的路径</param>
    /// <returns></returns>
    public async Task SaveAsync( Image image, string filepath )
    {
      var stream = new MemoryStream();
      image.Save( stream, CodecInfo, CodecParameters );

      Directory.CreateDirectory( Path.GetDirectoryName( filepath ) );

      var imageData = stream.ToArray();

      using ( var fileStream = File.OpenWrite( filepath ) )
      {
        await fileStream.WriteAsync( imageData, 0, imageData.Length );
      }
    }



    public static ImageCodec Jpeg( int quality = 90 )
    {
      var parameters = new EncoderParameters();
      parameters.Param[0] = new EncoderParameter( Encoder.Quality, (long) quality );

      return new ImageCodec( ImageCodecInfo.GetImageEncoders().First( e => e.MimeType == "image/jpeg" ), parameters, ".jpg" );
    }

  }
}
