using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{
  /// <summary>
  /// 定义缩放行为
  /// </summary>
  public abstract class ZoomingBehavior
  {


    /// <summary>
    /// 计算图片缩放后需要摆放的位置
    /// </summary>
    /// <param name="imageSize">图片大小</param>
    /// <param name="canvasSize">画布大小</param>
    /// <returns>摆放的位置</returns>
    public abstract Rectangle Zoom( Size imageSize, Size canvasSize );




    /// <summary>
    /// 适应缩放，同比例放大或缩小，图片居中
    /// </summary>
    public static ZoomingBehavior Fit { get; } = new FitZooming();


    /// <summary>
    /// 拉伸缩放，按照画布大小拉伸图片
    /// </summary>
    public static ZoomingBehavior Extrude { get; } = new ExtrudeZooming();



    /// <summary>
    /// 适应缩放行为
    /// </summary>
    private class FitZooming : ZoomingBehavior
    {
      /// <summary>
      /// 将指定图片适应指定的画布大小并居中摆放，获取摆放的位置
      /// </summary>
      /// <param name="imageSize">图片大小</param>
      /// <param name="canvasSize">画布大小</param>
      /// <returns>摆放的位置</returns>
      public override Rectangle Zoom( Size imageSize, Size canvasSize )
      {

        var width = canvasSize.Width;
        var height = canvasSize.Height;


        Size targetSize;

        var aspeticRatio = ((double) imageSize.Width) / (double) imageSize.Height;

        if ( aspeticRatio <= (double) (width / height) )
          targetSize = new Size( (int) ((double) imageSize.Width * height / (double) imageSize.Height), height );
        else
          targetSize = new Size( width, (int) ((double) imageSize.Height * width / (double) imageSize.Width) );


        return new Rectangle( (width - targetSize.Width) / 2, (height - targetSize.Height) / 2, targetSize.Width, targetSize.Height );

      }
    }

    /// <summary>
    /// 拉伸缩放行为
    /// </summary>
    private class ExtrudeZooming : ZoomingBehavior
    {
      /// <summary>
      /// 将指定图片拉伸成画布大小
      /// </summary>
      /// <param name="imageSize">图片大小</param>
      /// <param name="canvasSize">画布大小</param>
      /// <returns>画布大小</returns>
      public override Rectangle Zoom( Size imageSize, Size canvasSize )
      {
        return new Rectangle( new Point( 0, 0 ), canvasSize );
      }
    }
  }
}
