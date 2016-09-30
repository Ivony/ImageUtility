using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{

  /// <summary>
  /// 定义生成缩略图的任务
  /// </summary>
  public class ZoomingTask : ImageProcessTask
  {


    public ZoomingTask( int width, int height ) : this( new Size( width, height ) ) { }

    public ZoomingTask( Size size, SmoothingMode smoothingMode = SmoothingMode.Default, InterpolationMode interpolationMode = InterpolationMode.Default )
      : this( size, Color.White, smoothingMode, interpolationMode ) { }

    public ZoomingTask( Size size, Color background, SmoothingMode smoothingMode = SmoothingMode.Default, InterpolationMode interpolationMode = InterpolationMode.Default )
      : this( size, background, ZoomingBehavior.Fit, smoothingMode, interpolationMode ) { }

    public ZoomingTask( Size size, Color background, ZoomingBehavior behavior, SmoothingMode smoothingMode = SmoothingMode.Default, InterpolationMode interpolationMode = InterpolationMode.Default )
    {
      Size = size;


      Background = background;
      ZoomingBehavior = behavior;
      SmoothingMode = smoothingMode;
      InterpolationMode = interpolationMode;
    }


    /// <summary>
    /// 缩放的目标大小
    /// </summary>
    public Size Size
    {
      get;
      private set;
    }


    /// <summary>
    /// 是否进行平滑处理
    /// </summary>
    public SmoothingMode SmoothingMode
    {
      get;
      private set;
    }


    /// <summary>
    /// 缩放所使用的插值算法
    /// </summary>
    public InterpolationMode InterpolationMode
    {
      get;
      private set;
    }


    /// <summary>
    /// 填充的背景颜色
    /// </summary>
    public Color Background
    {
      get;
      private set;
    }



    /// <summary>
    /// 获取缩放行为
    /// </summary>
    public ZoomingBehavior ZoomingBehavior { get; }



    /// <summary>
    /// 缩放图片
    /// </summary>
    /// <param name="context">工作流上下文</param>
    /// <param name="image">要处理的图像</param>
    /// <returns>处理后的图像</returns>
    public override Image ProcessImage( ImageWorkflowContext context, Image image )
    {

      var targetRectangle = ZoomingBehavior.Zoom( image.Size, Size );

      var canvas = new Bitmap( Size.Width, Size.Height );
      var graphic = Graphics.FromImage( canvas );
      graphic.SmoothingMode = SmoothingMode;
      graphic.InterpolationMode = InterpolationMode;

      graphic.Clear( Background );
      graphic.DrawImage( image, targetRectangle );

      return canvas;
    }



  }



}
