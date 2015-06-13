using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{


  /// <summary>
  /// 定义 ImageWorkflow 的容器
  /// </summary>
  public class ImageWorkflowCollection : KeyedCollection<string, ImageWorkflow>
  {

    public ImageWorkflowCollection() : base( StringComparer.OrdinalIgnoreCase ) { }

    protected override string GetKeyForItem( ImageWorkflow item )
    {
      return item.Name;
    }


    public IEnumerable<string> TaskNames
    {
      get { return Dictionary.Keys; }
    }

  }
}
