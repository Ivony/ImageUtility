using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Imaging
{
  public class WatermarkTask : ImageProcessTask
  {
    public WatermarkTask( string watermarkPath )
    {
      WatermarkPath = watermarkPath;
    }


    public string WatermarkPath
    {
      get;
      private set;
    }




    public override Image ProcessImage( ImageWorkflowContext context, Image image )
    {
      using ( var watermark = LoadWatermark() )
      {

        var graphic = Graphics.FromImage( image );
        graphic.DrawImage( watermark, GetRectangle( watermark.Size, image.Size ) );

        return image;
      }
    }

    private Rectangle GetRectangle( Size watermarkSize, Size imageSize )
    {
      var location = new Point( (imageSize.Width - watermarkSize.Width) / 2, (imageSize.Height - watermarkSize.Height) / 2 );
      return new Rectangle( location, watermarkSize );
    }

    private Image LoadWatermark()
    {
      return Image.FromFile( WatermarkPath );
    }

  }
}
