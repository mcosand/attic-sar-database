/* 
 * Copyright Matthew Cosand
 */
namespace Sar.Auth
{
  using System;
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

      var processResult = await _userService.SendExternalVerificationCode(partial_login, request.Email);

      return ServiceResultToObject(request, processResult, new { Success = true });
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

      var processResult = await _userService.VerifyExternalCode(partial_login, request.Email, request.Code);

      var resumeUrl = await ctx.Environment.GetPartialLoginResumeUrlAsync();
      return ServiceResultToObject(request, processResult, new { Success = true, Url = resumeUrl });
    }

    private static object ServiceResultToObject(VerifyCodeRequest request, ProcessVerificationResult processResult, object success)
    {
      object result;
      switch (processResult)
      {
        case ProcessVerificationResult.Success:
          result = success;
          break;
        case ProcessVerificationResult.AlreadyRegistered:
          result = new { Success = false, Errors = new { _ = new[] { "Login is already registered" } } };
          break;
        case ProcessVerificationResult.EmailNotAvailable:
          result = new { Success = false, Errors = new { Email = new[] { request.Email + " is not available for registration" } } };
          break;
        case ProcessVerificationResult.InvalidVerifyCode:
          result = new { Success = false, Errors = new { Code = new[] { "Verification code not found or is invalid" } } };
          break;
        default:
          throw new NotImplementedException("Don't know how to handle verification result " + processResult);
      }

      return result;
    }
  }
}
