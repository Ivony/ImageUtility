using System;
using System.Collections.Generic;
using System.IO.Compression;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{
  public class ArchiveDeployer : ImageDeployer
  {

    protected ZipArchive GetArchive()
    {
      throw new NotImplementedException();
    }

    public override async Task DeployImageAsync( string filepath )
    {

      GetArchive().CreateEntryFromFile( filepath, CreateName() );


      throw new NotImplementedException();
    }

    protected string CreateName()
    {
      throw new NotImplementedException();
    }
  }
}
