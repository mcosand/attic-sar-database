using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(Sar.Database.Website.Startup))]

namespace Sar.Database.Website
{
  public sealed class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      app.UseCookieAuthentication(new CookieAuthenticationOptions
      {
        AuthenticationType = "Cookies"
      });
      app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
      {
        Authority = "https://localhost:44300/identity",
        ClientId = "databaseweb",
        RedirectUri = "https://localhost:44301/",
        ResponseType = "id_token",
        Scope = "openid email profile",
        SignInAsAuthenticationType = "Cookies"
      });
    }
  }
}