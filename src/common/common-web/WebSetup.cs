/*
 * Copyright Matthew Cosand
 */
namespace Sar.Web
{
  using System;
  using System.Collections.Specialized;
  using System.Web;
  using System.Web.Http;
  using System.Web.Http.ExceptionHandling;
  using Newtonsoft.Json.Serialization;
  using Ninject;
  using Ninject.Web.Common;
  using Ninject.Web.WebApi;
  using Owin;
  using Serilog;

  public static class WebSetup
  {
    public static IKernel StartMvcAndWebApiWithNinject(IAppBuilder app, NameValueCollection configStrings, Action<IKernel> registerServices)
    {
      var kernel = SetupDependencyInjection(registerServices, configStrings);
      SetupWebApi(app, kernel);
      return kernel;
    }

    public static IKernel SetupDependencyInjection(Action<IKernel> registerServices, NameValueCollection configStrings)
    {
      var kernel = new StandardKernel();
      var bootstrapper = new Bootstrapper();
      bootstrapper.Initialize(() => kernel);
      kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

      var logConfig = new LoggerConfiguration()
          .MinimumLevel.Information()
          .WriteTo.RollingFile(AppDomain.CurrentDomain.BaseDirectory + "\\logs\\log-{Date}.txt");
      Log.Logger = logConfig.CreateLogger();

      kernel.Bind<ILogger>().ToConstant(Log.Logger);
      if (registerServices != null)
      {
        registerServices(kernel);
      }

      return kernel;
    }

    public static void SetupWebApi(IAppBuilder app, IKernel kernel)
    {
      var config = new HttpConfiguration();
      config.MapHttpAttributeRoutes();
      config.Services.Replace(typeof(IExceptionHandler), new ApiUserExceptionHandler());
      config.Services.Add(typeof(IExceptionLogger), kernel.Get<ApiExceptionLogger>());

      var formatter = config.Formatters.JsonFormatter;
      formatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

      config.DependencyResolver = new NinjectDependencyResolver(kernel);
      app.UseWebApi(config);
    }
  }
}
