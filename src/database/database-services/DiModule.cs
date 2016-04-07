/*
 * Copyright Matthew Cosand
 */
using System;
using System.Configuration;
using Kcsar.Database.Model;
using Ninject;
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

      var memberService = new MembersService(Kernel.Get<Func<IKcsarContext>>());
      Kernel.Bind<IMemberInfoService>().ToConstant(memberService).InSingletonScope();
      Kernel.Bind<IMembersService>().ToConstant(memberService).InSingletonScope();
    }
  }
}
