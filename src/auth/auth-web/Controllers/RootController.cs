/*
 * Copyright Matthew Cosand
 */
using System.Web.Mvc;

namespace Sar.Auth.Controllers
{
  public class RootController : Controller
  {
    [Route(AuthWebApplication.SITEROOT + "jsconfig")]
    [HttpGet]
    public ContentResult JSConfig()
    {
      return Content(string.Format("window.appRoot = '{0}';", Url.Content("~/")), "text/javascript");
    }
  }
}