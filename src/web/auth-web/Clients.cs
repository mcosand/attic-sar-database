using System.Collections.Generic;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;

namespace Sar.Auth
{
  public static class Clients
  {
    public static IEnumerable<Client> Get()
    {
      return new List<Client> {
        new Client {
            ClientId = "databaseweb",
            ClientName = "KCSARA Database",
            Enabled = true,
            Flow = Flows.Implicit,
            RequireConsent = false,
            AllowRememberConsent = false,
            RedirectUris = new List<string> { "https://localhost:44301/" },
            PostLogoutRedirectUris = new List<string>(),
            AllowedScopes = new List<string> {
                Constants.StandardScopes.OpenId,
                Constants.StandardScopes.Profile,
                Constants.StandardScopes.Email
            },
            AllowAccessToAllScopes = true,
            AccessTokenType = AccessTokenType.Jwt
        }
      };
    }
  }
}