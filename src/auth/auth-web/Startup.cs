/*
 * Copyright Matthew Cosand
 */
using System;
using System.Configuration;
using System.Web;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using Microsoft.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OpenIdConnect;
using Ninject;
using Owin;
using Sar.Auth.Controllers;
using Sar.Auth.Services;
using Serilog;

[assembly: OwinStartup(typeof(Sar.Auth.Startup))]

namespace Sar.Auth
{
  public sealed class Startup
  {
    internal static IKernel kernel;

    public void Configuration(IAppBuilder app)
    {
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.RollingFile(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\log-{Date}.txt")
        .CreateLogger();

      Action<IAppBuilder> buildApp =
          coreApp =>
          {
            var userService = kernel.Get<SarUserService>();
            var clientStore = kernel.Get<IClientStore>();
            var corsService = new DefaultCorsPolicyService { AllowAll = true };

            var factory = new IdentityServerServiceFactory
            {
              UserService = new Registration<IUserService>(resolver => userService),
              ClientStore = new Registration<IClientStore>(resolver => clientStore),
              CorsPolicyService = new Registration<ICorsPolicyService>(resolver => corsService),
              ViewService = new Registration<IViewService, MvcViewService<AccountController>>()
            }
            .UseInMemoryScopes(Scopes.Get());

            // These registrations are also needed since these are dealt with using non-standard construction
            factory.Register(new Registration<HttpContext>(resolver => HttpContext.Current));
            factory.Register(new Registration<HttpContextBase>(resolver => new HttpContextWrapper(resolver.Resolve<HttpContext>())));
            factory.Register(new Registration<HttpRequestBase>(resolver => resolver.Resolve<HttpContextBase>().Request));
            factory.Register(new Registration<HttpResponseBase>(resolver => resolver.Resolve<HttpContextBase>().Response));
            factory.Register(new Registration<HttpServerUtilityBase>(resolver => resolver.Resolve<HttpContextBase>().Server));
            factory.Register(new Registration<HttpSessionStateBase>(resolver => resolver.Resolve<HttpContextBase>().Session));


            var options = new IdentityServerOptions
            {
              SiteName = "KCSARA Authentication Service",

              SigningCertificate = Cert.Load(),
              Factory = factory,
              CspOptions = new CspOptions
              {
                ImgSrc = "'self' data:"
              },
              AuthenticationOptions = new AuthenticationOptions
              {
                IdentityProviders = ConfigureIdentityProviders,
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
    public static void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
    {
      var googleId = ConfigurationManager.AppSettings["google:clientId"];
      var googleSecret = ConfigurationManager.AppSettings["google:clientSecret"];

      var google = new GoogleOAuth2AuthenticationOptions
      {
        AuthenticationType = "Google",
        Caption = "Google",
        SignInAsAuthenticationType = signInAsType,
        ClientId = googleId,
        ClientSecret = googleSecret
      };
      app.UseGoogleAuthentication(google);

      var facebookId = ConfigurationManager.AppSettings["facebook:appId"];
      var facebookSecret = ConfigurationManager.AppSettings["facebook:appSecret"];
      if (!string.IsNullOrWhiteSpace(facebookId) && !string.IsNullOrWhiteSpace(facebookSecret))
      {
        var fb = new FacebookAuthenticationOptions
        {
          AuthenticationType = "Facebook",
          Caption = "Facebook",
          SignInAsAuthenticationType = signInAsType,
          AppId = facebookId,
          AppSecret = facebookSecret
        };
        app.UseFacebookAuthentication(fb);
      }

      foreach (var openIdType in (ConfigurationManager.AppSettings["openId:providers"] ?? string.Empty).Split(','))
      {
        var type = openIdType.Trim();
        var prefix = "openid:" + type + ":";

        var openId = ConfigurationManager.AppSettings[prefix + "clientId"];
        var openSecret = ConfigurationManager.AppSettings[prefix + "clientSecret"];
        var caption = ConfigurationManager.AppSettings[prefix + "caption"];
        var authority = ConfigurationManager.AppSettings[prefix + "authority"];

        if (!string.IsNullOrWhiteSpace(openId)
          && !string.IsNullOrWhiteSpace(openSecret)
          && !string.IsNullOrWhiteSpace(caption)
          && !string.IsNullOrWhiteSpace(authority))
        {

          app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
          {
            AuthenticationType = type,
            ClientId = openId,
            ClientSecret = openSecret,
            Authority = authority,
            Caption = caption,
            SignInAsAuthenticationType = signInAsType
          });

        }
      }

    }
  }
}