using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;

namespace Sar.Auth.Services
{
  public class SarUserService : UserServiceBase
  {
    public override Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
    {
      return base.AuthenticateExternalAsync(context);
    }

    public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
    {
      return base.AuthenticateLocalAsync(context);
    }

    public override Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
      return base.GetProfileDataAsync(context);
    }
  }
}