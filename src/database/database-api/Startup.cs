/*
 * Copyright Matthew Cosand
 */
using System.IdentityModel.Tokens;
using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin;
using Ninject;
using Owin;
using Sar.Services;
using Sar.Web;

[assembly: OwinStartup(typeof(Sar.Database.Api.Startup))]

namespace Sar.Database.Api
{
  public partial class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      var kernel = WebSetup.SetupDependencyInjection(RegisterServices);
      var config = kernel.Get<IConfigService>();

      JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();

      app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
      {
        Authority = config["auth:authority"],
        RequiredScopes = new [] { "database-api" }
      });

      WebSetup.SetupWebApi(app, kernel);
    }

    public void RegisterServices(IKernel kernel)
    {
      kernel.Bind<IConfigService>().To<ConfigService>();
      kernel.Load<Services.DiModule>();
    }
  }
}
