using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

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