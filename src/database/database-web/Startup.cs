using System.Configuration;
using System.IdentityModel.Tokens;
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
        Authority = ConfigurationManager.AppSettings["auth:authority"].Trim('/') + "/",
        ClientId = ConfigurationManager.AppSettings["auth:clientId"],
        RedirectUri = ConfigurationManager.AppSettings["auth:redirect"].Trim('/') + "/",
        ResponseType = "id_token",
        Scope = "openid email profile kcsara-profile",
        TokenValidationParameters = new TokenValidationParameters
        {
          NameClaimType = "name"
        },
        SignInAsAuthenticationType = "Cookies"
      });
    }
  }
}