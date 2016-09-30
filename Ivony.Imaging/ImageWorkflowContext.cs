using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{

  /// <summary>
  /// 定义一次图片处理流水线中的上下文，用于在处理过程中传递信息
  /// </summary>
  public class ImageWorkflowContext
  {


    internal ImageWorkflowContext( ImageWorkflow workflow, IDictionary<string, object> data )
    {
      Workflow = workflow;
      Data = new Dictionary<string, object>( data );
      SyncRoot = new object();
    }


    /// <summary>
    /// 用于同步的对象
    /// </summary>
    public object SyncRoot { get; private set; }

    /// <summary>
    /// 处理图片的流水线对象
    /// </summary>
    public ImageWorkflow Workflow { get; private set; }

    /// <summary>
    /// 处理过程中共享的数据
    /// </summary>
    public IDictionary<string, object> Data { get; private set; }



    /// <summary>
    /// 创建一个空白的 ImageWorkflowContext
    /// </summary>
    public static ImageWorkflowContext CreateEmpty()
    {
      return new ImageWorkflowContext( null, new Dictionary<string, object>() );
    }
  }
}
