using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{
  public sealed class MulticastImageDeployer : ImageDeployer
  {


    private ImageDeployer[] deployers;


    public MulticastImageDeployer( params ImageDeployer[] deployers )
    {

      deployers.SelectMany( item =>
        {
          var m = item as MulticastImageDeployer;
          if ( m == null )
            return new[] { item };

          else
            return m.deployers;

        } );
    }


    public async override Task DeployImageAsync( string filepath )
    {

      string result = null;
      foreach ( var item in deployers )
      {
        await item.DeployImageAsync( filepath );
      }
    }
  }
}
