/* 
 * Copyright Matthew Cosand
 */
namespace Sar.Auth
{
  using System.Net;
  using System.Net.Http;
  using System.Threading.Tasks;
  using System.Web;
  using System.Web.Http;
  using IdentityServer3.Core.Extensions;
  using Sar.Auth.Models;
  using Sar.Auth.Services;
  using Serilog;

  public class ApiController : System.Web.Http.ApiController
  {
    private readonly ILogger _log;
    private readonly SarUserService _userService;

    public ApiController(SarUserService service, ILogger log)
    {
      _userService = service;
      _log = log;
    }

    [HttpPost]
    [Route(AuthWebApplication.SITEROOT + "externalVerificationCode")]
    public async Task<object> ExternalVerificationCode(VerifyCodeRequest request)
    {
      var ctx = HttpContext.Current.GetOwinContext();
      var partial_login = await ctx.Environment.GetIdentityServerPartialLoginAsync();
      if (partial_login == null)
      {
        throw new UserErrorException("Not logged in with external login.");
      }

      await _userService.SendExternalVerificationCode(partial_login, request.Email);

      return new { Success = true };
    }

    [HttpPost]
    [Route(AuthWebApplication.SITEROOT + "verifyExternalCode")]
    public async Task<object> VerifyExternalCode(VerifyCodeRequest request)
    {
      var ctx = HttpContext.Current.GetOwinContext();
      var partial_login = await ctx.Environment.GetIdentityServerPartialLoginAsync();
      if (partial_login == null)
      {
        throw new UserErrorException("Not logged in with external login.");
      }

      await _userService.VerifyExternalCode(partial_login, request.Email, request.Code);

      var resumeUrl = await ctx.Environment.GetPartialLoginResumeUrlAsync();
      return new { Success = true, Url = resumeUrl };
    }

  }
}
