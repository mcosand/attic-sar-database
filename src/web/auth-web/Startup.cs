using System;
using System.Web;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Services;
using Microsoft.Owin;
using Owin;
using Sar.Auth.Controllers;
using Sar.Auth.Services;
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

      Action<IAppBuilder> buildApp = 
          coreApp => {
            var factory = new IdentityServerServiceFactory()
                .UseInMemoryClients(Clients.Get())
                .UseInMemoryScopes(Scopes.Get());
            factory.UseInMemoryUsers(Users.Get());
            var userService = new SarUserService();
          //  factory.UserService = new Registration<IUserService>(resolver => userService);

            factory.ViewService = new Registration<IViewService, MvcViewService<AccountController>>();

            // These registrations are also needed since these are dealt with using non-standard construction
            factory.Register(new Registration<HttpContext>(resolver => HttpContext.Current));
            factory.Register(new Registration<HttpContextBase>(resolver => new HttpContextWrapper(resolver.Resolve<HttpContext>())));
            factory.Register(new Registration<HttpRequestBase>(resolver => resolver.Resolve<HttpContextBase>().Request));
            factory.Register(new Registration<HttpResponseBase>(resolver => resolver.Resolve<HttpContextBase>().Response));
            factory.Register(new Registration<HttpServerUtilityBase>(resolver => resolver.Resolve<HttpContextBase>().Server));
            factory.Register(new Registration<HttpSessionStateBase>(resolver => resolver.Resolve<HttpContextBase>().Session));


            var options = new IdentityServerOptions
            {
              SiteName = "IdentityServer3 - CustomUserService",

              SigningCertificate = Cert.Load(),
              Factory = factory,
              CspOptions = new CspOptions
              {
                ImgSrc = "'self' data:"
              },
              AuthenticationOptions = new AuthenticationOptions
              {
               // IdentityProviders = ConfigureAdditionalIdentityProviders,
                LoginPageLinks = new LoginPageLink[] {
                            new LoginPageLink{
                                Text = "Register",
                                //Href = "~/localregistration"
                                Href = "localregistration"
                            }
                        }
              },

              EventsOptions = new EventsOptions
              {
                RaiseSuccessEvents = true,
                RaiseErrorEvents = true,
                RaiseFailureEvents = true,
                RaiseInformationEvents = true
              }
            };

            coreApp.UseIdentityServer(options);
          };

      if (string.IsNullOrWhiteSpace(AuthWebApplication.SITEROOT))
      {
        buildApp(app);
      }
      else
      {
        app.Map("/" + AuthWebApplication.SITEROOT.Trim('/'), buildApp);
      }
    }
  }
}