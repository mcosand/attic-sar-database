/*
 * Copyright 2016 Matthew Cosand
 */
namespace System.Web.Mvc
{
  using Newtonsoft.Json;

  public static class HtmlHelperExtensions
  {
    public static HtmlString ToJson(this HtmlHelper helper, object obj)
    {
      return new HtmlString(JsonConvert.SerializeObject(obj, Sar.Utils.GetJsonSettings()));
    }
  }
}
