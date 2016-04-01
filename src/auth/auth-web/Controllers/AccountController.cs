/*
 * Copyright Matthew Cosand
 */
using System.Web.Mvc;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.ViewModels;
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
  }
}