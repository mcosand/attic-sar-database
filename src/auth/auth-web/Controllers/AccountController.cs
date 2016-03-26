/*
 * Copyright Matthew Cosand
 */
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Sar.Auth.Models;
using Sar.Auth.Services;

namespace Sar.Auth.Controllers
{
  public class AccountController : Controller
  {
    private readonly SarUserService _userService;

    public AccountController(SarUserService service)
    {
      _userService = service;
    }

    // GET: Account
    [Route(AuthWebApplication.SITEROOT + "register")]
    public ActionResult Register()
    {
      return View();
    }

    [HttpGet]
    [Route(AuthWebApplication.SITEROOT + "registerlogin")]
    public async Task<ActionResult> RegisterLogin()
    {
      // this verifies that we have a partial signin from idsvr
      var ctx = Request.GetOwinContext();
      var partial_login = await ctx.Environment.GetIdentityServerPartialLoginAsync();
      
      if (partial_login == null)
      {
        return View("Error");
      }

      ViewBag.Email = (partial_login.FindFirst(Constants.ClaimTypes.Email)
                   ?? partial_login.FindFirst("upn"))?.Value;
      ViewBag.Name = partial_login.FindFirst(Constants.ClaimTypes.Name)?.Value ?? "Unknown User";

      return View();
    }
    [HttpPost]
    [Route(AuthWebApplication.SITEROOT + "externalVerificationCode")]
    public async Task<ActionResult> ExternalVerificationCode(VerifyCodeRequest request)
    {
      var ctx = Request.GetOwinContext();
      var partial_login = await ctx.Environment.GetIdentityServerPartialLoginAsync();
      if (partial_login == null)
      {
        return View("Error");
      }

      await _userService.SendExternalVerificationCode(partial_login, request.Email);

      //using (var db = )
      return CamelJson(new { Success = true });
    }

    [HttpPost]
    [Route(AuthWebApplication.SITEROOT + "verifyExternalCode")]
    public async Task<ActionResult> VerifyExternalCode(VerifyCodeRequest request)
    {
      var ctx = Request.GetOwinContext();
      var partial_login = await ctx.Environment.GetIdentityServerPartialLoginAsync();
      if (partial_login == null)
      {
        return View("Error");
      }

      await _userService.VerifyExternalCode(partial_login, request.Email, request.Code);

      var resumeUrl = await ctx.Environment.GetPartialLoginResumeUrlAsync();
      return CamelJson(new { Success = true, Url = resumeUrl });
    }

    #region Login

    /// <summary>
    /// Loads the HTML for the login page.
    /// </summary>
    /// <param name="model">
    /// The model.
    /// </param>
    /// <param name="message">
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Login(LoginViewModel model, SignInMessage message)
    {
      return this.View(model);
    }

    #endregion

    #region Logout

    /// <summary>
    /// Loads the HTML for the logout prompt page.
    /// </summary>
    /// <param name="model">
    /// The model.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Logout(LogoutViewModel model)
    {
      return this.View(model);
    }

    #endregion

    #region LoggedOut

    /// <summary>
    /// Loads the HTML for the logged out page informing the user that they have successfully logged out.
    /// </summary>
    /// <param name="model">
    /// The model.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult LoggedOut(LoggedOutViewModel model)
    {
      return this.View(model);
    }

    #endregion

    #region Consent

    /// <summary>
    /// Loads the HTML for the user consent page.
    /// </summary>
    /// <param name="model">
    /// The model.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Consent(ConsentViewModel model)
    {
      return this.View(model);
    }

    #endregion

    #region Permissions

    /// <summary>
    /// Loads the HTML for the client permissions page.
    /// </summary>
    /// <param name="model">
    /// The model.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public ActionResult Permissions(ClientPermissionsViewModel model)
    {
      return this.View(model);
    }

    #endregion

    /// <summary>
    /// Loads the HTML for the error page.
    /// </summary>
    /// <param name="model">
    /// The model.
    /// </param>
    /// <returns>
    /// The <see cref="ActionResult"/>.
    /// </returns>
    public virtual ActionResult Error(ErrorViewModel model)
    {
      return this.View(model);
    }

    private ContentResult CamelJson(object o)
    {
      var camelCaseFormatter = new JsonSerializerSettings();
      camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
      return Content(JsonConvert.SerializeObject(o, camelCaseFormatter), "application/json");
    }
  }
}