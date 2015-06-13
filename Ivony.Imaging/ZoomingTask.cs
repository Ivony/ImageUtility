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

    public ZoomingTask( Size size, string name = null, SmoothingMode smoothingMode = SmoothingMode.Default, InterpolationMode interpolationMode = InterpolationMode.Default )
      : this( size, name, Color.White, smoothingMode, interpolationMode ) { }

    public ZoomingTask( Size size, Color background, SmoothingMode smoothingMode = SmoothingMode.Default, InterpolationMode interpolationMode = InterpolationMode.Default )
      : this( size, null, background, smoothingMode, interpolationMode ) { }

    public ZoomingTask( Size size, string name, Color background, SmoothingMode smoothingMode = SmoothingMode.Default, InterpolationMode interpolationMode = InterpolationMode.Default )
    {
      Size = size;

      _name = name ?? "Thumbnail_" + size.Width + "x" + size.Height;

      Background = background;
      SmoothingMode = smoothingMode;
      InterpolationMode = interpolationMode;
    }


    public Size Size
    {
      get;
      private set;
    }


    private string _name;

    public override string Name
    {
      get { return _name; }
    }


    public SmoothingMode SmoothingMode
    {
      get;
      private set;
    }


    public InterpolationMode InterpolationMode
    {
      get;
      private set;
    }


    public Color Background
    {
      get;
      private set;
    }


    public override Image ProcessImage( Image image )
    {

      var targetRectangle = GetFitZoomRectangle( image.Size, Size );

      var canvas = new Bitmap( Size.Width, Size.Height );
      var graphic = Graphics.FromImage( canvas );
      graphic.SmoothingMode = SmoothingMode;
      graphic.InterpolationMode = InterpolationMode;

      graphic.Clear( Background );
      graphic.DrawImage( image, targetRectangle );

      return canvas;
    }


    /// <summary>
    /// 将指定图片适应指定的画布大小并居中摆放，获取摆放的位置
    /// </summary>
    /// <param name="imageSize">图片大小</param>
    /// <param name="canvasSize">画布大小</param>
    /// <returns>摆放的位置</returns>
    public static Rectangle GetFitZoomRectangle( Size imageSize, Size canvasSize )
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



    private void CalculateThumbnailSize( Image image, out int width, out int height )
    {
      width = 750;
      height = 750;
      var targetWidth = 750;
      var targetHeight = 750;

      //图片为正方形，且尺寸大于给定的目标尺寸，需要进行压缩，打750*750水印
      if ( image.Height == image.Width && image.Height >= 750 )
      {
        width = 750;
        height = 750;

        return;
      }

      var maxEdge = image.Width > image.Height ? image.Width : image.Height;
      var maxTargetEdge = targetWidth > targetHeight ? targetWidth : targetHeight;

      //图片最大边小于目标图片最大边，不需要压缩，打500*500水印
      if ( maxEdge < maxTargetEdge )
      {
        width = image.Width;
        height = image.Height;

        return;
      }

      double thumbnailRate = 0;

      //等比例压缩高度
      if ( image.Width > image.Height )
      {
        thumbnailRate = (double) targetWidth / (double) image.Width;

        height = (int) (image.Height * thumbnailRate);
      }
      //等比例压缩长度
      else
      {
        thumbnailRate = (double) targetHeight / (double) image.Height;

        width = (int) (image.Width * thumbnailRate);
      }

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
