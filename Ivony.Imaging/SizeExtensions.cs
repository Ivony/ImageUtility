using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Ivony.Imaging
{
  public static class SizeExtensions
  {

    /// <summary>
    /// 将一个尺寸保持长宽比的缩放到指定尺寸所能容纳的最大尺寸
    /// </summary>
    /// <param name="size">要缩放的原始尺寸</param>
    /// <param name="spec">指定的尺寸</param>
    /// <returns>缩放后的尺寸</returns>
    public static Size ZoomTo( this Size size, Size spec, bool fitMode = true )
    {
      return ZoomTo( size, spec.AsSizeF() );
    }

    /// <summary>
    /// 将 Size 对象转换为 SizeF 对象
    /// </summary>
    /// <param name="size">要转换的 Size 对象</param>
    /// <returns>转换后的 SizeF 对象</returns>
    public static SizeF AsSizeF( this Size size )
    {
      return new SizeF( size.Width, size.Height );
    }

    /// <summary>
    /// 将 SizeF 对象转换为 Size 对象
    /// </summary>
    /// <param name="size">要转换的 SizeF 对象</param>
    /// <returns>转换后的 Size 对象</returns>
    public static Size AsSize( this SizeF size )
    {
      return new Size( (int) Math.Round( size.Width ), (int) Math.Round( size.Height ) );
    }



    /// <summary>
    /// 将一个尺寸保持长宽比的缩放到指定尺寸所能容纳的最大尺寸
    /// </summary>
    /// <param name="size">要缩放的原始尺寸</param>
    /// <param name="spec">指定的尺寸</param>
    /// <returns>缩放后的尺寸</returns>
    public static Size ZoomTo( this Size size, SizeF spec, bool fitMode = true )
    {

      double than = (double) size.Width / (double) size.Height;

      if ( than <= (spec.Width / spec.Height) )
        return new Size( (int) ((double) size.Width * spec.Height / (double) size.Height), (int) spec.Height );
      else
        return new Size( (int) spec.Width, (int) ((double) size.Height * spec.Width / (double) size.Width) );
    }

    /// <summary>
    /// 计算指定尺寸的面积
    /// </summary>
    /// <param name="size">要计算面积的尺寸</param>
    /// <returns>面积大小</returns>
    public static int Acreage( this Size size )
    {
      checked { return size.Height * size.Width; }
    }

    /// <summary>
    /// 判断指定的尺寸是否能够容纳另一个较小的尺寸
    /// </summary>
    /// <param name="containerSize">容纳另一个尺寸的尺寸</param>
    /// <param name="size">被容纳的尺寸</param>
    /// <returns>若 size 的长宽都小于或等于 container 的长宽，则返回 true ，否则返回 false</returns>
    public static bool CanContains( this Size containerSize, Size size )
    {
      return containerSize.Width >= size.Width && containerSize.Height >= size.Height;
    }
  }


  public enum ZoomMode
  {
    /// <summary>适应</summary>
    Fit,
    /// <summary>填充</summary>
    Fill,
    /// <summary>拉伸</summary>
    Extrude,
    /// <summary>平铺</summary>
    Tile,
    /// <summary>居中</summary>
    Center
  }
}
