using System;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Logging;
using Microsoft.Owin;
using Owin;
using Serilog;
using Serilog.Sinks;
[assembly: OwinStartup(typeof(Sar.Auth.Startup))]

namespace Sar.Auth
{
  public sealed class Startup
  {
    public void Configuration(IAppBuilder app)
    {
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.RollingFile(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\log-{Date}.txt")
        .CreateLogger();

      app.Map(
          "/identity",
          coreApp => {
            coreApp.UseIdentityServer(new IdentityServerOptions
            {
              SiteName = "Standalone Identity Server",
              SigningCertificate = Cert.Load(),
              Factory = new IdentityServerServiceFactory()
                          .UseInMemoryClients(Clients.Get())
                          .UseInMemoryScopes(Scopes.Get())
                          .UseInMemoryUsers(Users.Get()),
              RequireSsl = true
            });
          });
    }
  }
}