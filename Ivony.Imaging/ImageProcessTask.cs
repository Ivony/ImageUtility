using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{

  /// <summary>
  /// 定义一个图像处理任务
  /// </summary>
  public abstract class ImageProcessTask
  {

    /// <summary>
    /// 执行图像处理任务
    /// </summary>
    /// <param name="image">图像对象</param>
    /// <returns>处理后的图像</returns>
    public Image ProcessImage( Image image )
    {
      return ProcessImage( ImageWorkflowContext.CreateEmpty(), image );
    }

    /// <summary>
    /// 执行图像处理任务
    /// </summary>
    /// <param name="image">图像对象</param>
    /// <returns>处理后的图像</returns>
    public abstract Image ProcessImage( ImageWorkflowContext context, Image image );



    /// <summary>
    /// 将两个图像处理任务串联起来
    /// </summary>
    /// <param name="task1"></param>
    /// <param name="task2"></param>
    /// <returns></returns>
    public static ImageProcessTask operator +( ImageProcessTask task1, ImageProcessTask task2 )
    {

      return new ImageProcessTaskChain( task1, task2 );

    }


    private class BlankImageProcessTask : ImageProcessTask
    {
      public override Image ProcessImage( ImageWorkflowContext context, Image image ) { return image; }
    }


    /// <summary>
    /// 获取一个空白的图片处理任务（即不对图片进行任何处理）
    /// </summary>
    public static ImageProcessTask Blank { get; } = new BlankImageProcessTask();


  }
}
