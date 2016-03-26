/*
 * Copyright Matthew Cosand
 */
using System.Configuration;

namespace Sar.Services
{
  public interface IConfigService
  {
    string this[string key] { get; }
  }
  public class ConfigService : IConfigService
  {
    public string this[string key]
    {
      get
      {
        return ConfigurationManager.AppSettings[key];
      }
    }
  }
}
