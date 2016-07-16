using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{

  /// <summary>
  /// 代表多个图片处理任务串联起来的任务链
  /// </summary>
  internal class ImageProcessTaskChain : ImageProcessTask
  {


    /// <summary>
    /// 创建 ImageTaskChain 对象
    /// </summary>
    /// <param name="tasks">图片任务列表</param>
    public ImageProcessTaskChain( params ImageProcessTask[] tasks )
    {

      ImageTasks = tasks.SelectMany( t => OpenTaskChain( t ) ).ToArray();
    }


    /// <summary>
    /// 如果图片任务是一个任务链，则将该任务链解开成多个任务
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    private IEnumerable<ImageProcessTask> OpenTaskChain( ImageProcessTask task )
    {
      var chain = task as ImageProcessTaskChain;
      if ( chain != null )
        return chain.ImageTasks;

      else
        return new[] { task };
    }


    protected ImageProcessTask[] ImageTasks { get; private set; }


    public override string Name
    {
      get { return string.Join( "+", ImageTasks.Select( task => task.Name ) ); }
    }

    public override Image ProcessImage( ImageWorkflowContext context, Image image )
    {
      foreach ( ImageProcessTask task in ImageTasks )
        image = task.ProcessImage( context, image );

      return image;
    }
  }
}
