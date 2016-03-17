using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;

namespace Sar.Auth
{
  public static class Users
  {
    public static List<InMemoryUser> Get()
    {
      return new List<InMemoryUser> {
            new InMemoryUser {
                Subject = "1",
                Username = "tim",
                Password = "5!",
                Claims = new List<Claim> {
                    new Claim(Constants.ClaimTypes.GivenName, "Tim"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Wizard"),
                    new Claim(Constants.ClaimTypes.Email, "sample@example.com")
                }
            }
        };
    }
  }
}