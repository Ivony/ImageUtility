using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

[assembly: PreApplicationStartMethod( typeof( ImageRedirect.ImageRoute ), "Initialize" )]
namespace ImageRedirect
{
  public class ImageRoute : RouteBase
  {


    public static void Initialize()
    {
      RouteTable.Routes.Add( new ImageRoute() );
      RouteTable.Routes.RouteExistingFiles = false;
    }

    public override RouteData GetRouteData( HttpContextBase httpContext )
    {
      var path = httpContext.Request.Path;

      if ( path.EndsWith( ".jpg", StringComparison.OrdinalIgnoreCase ) )
      {
        httpContext.Response.Redirect( "http://img2.focalprice.com" + path );
      }

      return null;
    }

    public override VirtualPathData GetVirtualPath( RequestContext requestContext, RouteValueDictionary values )
    {
      return null;
    }
  }
}