using System;
using System.Web.Routing;

namespace Sar.Auth
{
  public class AuthWebApplication : System.Web.HttpApplication
  {
    public const string SITEROOT = "";

    protected void Application_Start(object sender, EventArgs e)
    {
      RouteConfig.RegisterRoutes(RouteTable.Routes);
    }
 }
}