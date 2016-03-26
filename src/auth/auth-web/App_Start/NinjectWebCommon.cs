[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Sar.Auth.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Sar.Auth.App_Start.NinjectWebCommon), "Stop")]

namespace Sar.Auth.App_Start
{
  using System;
  using System.Configuration;
  using System.Linq;
  using System.Reflection;
  using System.Web;
  using Data;
  using IdentityServer3.Core.Services;
  using Microsoft.Web.Infrastructure.DynamicModuleHelper;

  using Ninject;
  using Ninject.Web.Common;
  using Sar.Services;
  using Services;
  public static class NinjectWebCommon
  {
    private static readonly Bootstrapper bootstrapper = new Bootstrapper();

    /// <summary>
    /// Starts the application
    /// </summary>
    public static void Start()
    {
      DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
      DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
      bootstrapper.Initialize(CreateKernel);
      Startup.kernel = bootstrapper.Kernel;
    }

    /// <summary>
    /// Stops the application.
    /// </summary>
    public static void Stop()
    {
      bootstrapper.ShutDown();
    }

    /// <summary>
    /// Creates the kernel that will manage your application.
    /// </summary>
    /// <returns>The created kernel.</returns>
    private static IKernel CreateKernel()
    {
      var kernel = new StandardKernel();
      try
      {
        kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
        kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

        RegisterServices(kernel);
        return kernel;
      }
      catch
      {
        kernel.Dispose();
        throw;
      }
    }

    /// <summary>
    /// Load your modules or register your services here!
    /// </summary>
    /// <param name="kernel">The kernel.</param>
    private static void RegisterServices(IKernel kernel)
    {
      kernel.Bind<SarUserService>().ToSelf();
      kernel.Bind<IClientStore>().To<SarClientStore>();
      kernel.Bind<Func<IAuthDbContext>>().ToMethod(ctx => () => new AuthDbContext());
      kernel.Bind<ISendEmailService>().To<DefaultSendMessageService>().InSingletonScope();
      kernel.Bind<IConfigService>().To<ConfigService>().InSingletonScope();

      string assemblyNames = ConfigurationManager.AppSettings["diAssemblies"] ?? string.Empty;
      foreach (var assemblyName in assemblyNames.Split(','))
      {
        kernel.Load(Assembly.Load(assemblyName));
      }

      if (!kernel.GetBindings(typeof(IMemberInfoService)).Any())
      {
        kernel.Bind<IMemberInfoService>().To<NullMemberInfoService>();
      }
    }
  }
}
