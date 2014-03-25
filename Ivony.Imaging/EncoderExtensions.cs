using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ivony.Imaging
{
  public static class EncoderExtensions
  {
    /// <summary>
    /// 将图片保存为 JPEG 格式，如果可能的话
    /// </summary>
    /// <param name="image">要保存的图片</param>
    /// <param name="filepath">保存的文件路径</param>
    /// <param name="quantity">图片质量</param>
    public static void SaveAsJpeg( this Image image, string filepath, int quality )
    {


      image.Save( filepath, JpegEncoder(), JpegEncoderParameters( quality ) );

    }


    /// <summary>
    /// 将图片保存为 JPEG 格式，如果可能的话
    /// </summary>
    /// <param name="image">要保存的图片</param>
    /// <param name="stream">保存的流</param>
    /// <param name="quantity">图片质量</param>
    public static void SaveAsJpeg( this Image image, Stream stream, int quality )
    {

      image.Save( stream, JpegEncoder(), JpegEncoderParameters( quality ) );

    }

    private static ImageCodecInfo JpegEncoder()
    {
      return ImageCodecInfo.GetImageEncoders().First( e => e.MimeType == "image/jpeg" );
    }

    private static EncoderParameters JpegEncoderParameters(int quality)
    {
      var parameters = new EncoderParameters();
      parameters.Param[0] = new EncoderParameter( Encoder.Quality, (long) quality );
      return parameters;
    }



  }
}
