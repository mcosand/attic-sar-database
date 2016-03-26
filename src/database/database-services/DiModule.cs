/*
 * Copyright Matthew Cosand
 */
using System;
using System.Configuration;
using Kcsar.Database.Model;
using Ninject.Modules;
using Sar.Auth;

namespace Sar.Database.Services
{
  public class DiModule : NinjectModule
  {
    public override void Load()
    {
      string connection = ConfigurationManager.AppSettings["dataStore"];
      Kernel.Bind<Func<IKcsarContext>>().ToMethod(ctx => () => new KcsarContext(connection));
      Kernel.Bind<IMemberInfoService>().To<MembersService>();
    }
  }
}
