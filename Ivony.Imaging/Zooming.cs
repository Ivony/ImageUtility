using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Ivony.Imaging
{
  public static class Zooming
  {

    public static Image ZoomTo( this Image sourceImage, Size size )
    {
      return ZoomTo( sourceImage, size, Color.Transparent, InterpolationMode.HighQualityBicubic );
    }

    public static Image ZoomTo( this Image sourceImage, Size size, Color background )
    {
      return ZoomTo( sourceImage, size, background, InterpolationMode.HighQualityBicubic );
    }

    public static Image ZoomTo( this Image sourceImage, Size size, InterpolationMode interpolation )
    {
      return ZoomTo( sourceImage, size, Color.Transparent, interpolation );
    }


    /// <summary>
    /// 将图片保持高宽比的缩放到指定大小，并使用指定背景填充多余部分和使用指定缩放算法。
    /// </summary>
    /// <param name="sourceImage">要进行缩放的原图</param>
    /// <param name="size">缩放的目标尺寸</param>
    /// <param name="background">填充空白区域的颜色</param>
    /// <param name="mode">缩放算法</param>
    /// <returns></returns>
    public static Image ZoomTo( this Image sourceImage, Size size, Color background, InterpolationMode interpolation )
    {
      return ZoomTo( sourceImage, size, background, interpolation, SmoothingMode.Default );
    }

    public static Image ZoomTo( this Image sourceImage, Size size, Color background, InterpolationMode interpolation, SmoothingMode smoothing )
    {
      return ZoomTo( sourceImage, size, background, interpolation, smoothing, new Size( 0, 0 ) );
    }


    public static Image ZoomTo( this Image sourceImage, Size size, Color background, InterpolationMode interpolation, SmoothingMode smoothing, Size overflow )
    {

      Bitmap bitmap = new Bitmap( size.Width, size.Height );
      Graphics graphics = Graphics.FromImage( bitmap );
      graphics.InterpolationMode = interpolation;
      graphics.SmoothingMode = smoothing;
      Size _size = sourceImage.Size.ZoomTo( size, true );
      if ( background != Color.Empty )
      {
        graphics.Clear( background );
      }
      Rectangle rect = new Rectangle( (bitmap.Width - _size.Width) / 2, (bitmap.Height - _size.Height) / 2, _size.Width, _size.Height );
      rect.Inflate( overflow );
      graphics.DrawImage( sourceImage, rect );
      return bitmap;
    }

  }
}
