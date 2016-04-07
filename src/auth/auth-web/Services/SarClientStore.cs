/*
 * Copyright Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Sar.Auth.Data;

namespace Sar.Auth.Services
{
  public class SarClientStore : IClientStore
  {
    private readonly Func<IAuthDbContext> _dbFactory;  

    public SarClientStore(Func<IAuthDbContext> dbFactory)
    {
      _dbFactory = dbFactory;
    }

    public async Task<Client> FindClientByIdAsync(string clientId)
    {
      using (var db = _dbFactory())
      {
        var row = await db.Clients.Where(f => f.ClientId == clientId).SingleOrDefaultAsync();
        if (row == null) return null;
        if (string.IsNullOrWhiteSpace(row.Secret))
        {
          return new Client
          {
            ClientId = row.ClientId,
            ClientName = row.DisplayName,
            Enabled = row.Enabled,
            Flow = Flows.Implicit,
            RequireConsent = false,
            AllowRememberConsent = false,
            RedirectUris = row.RedirectUris.Select(g => g.Uri).ToList(),
            PostLogoutRedirectUris = new List<string>(),
            AllowedScopes = new List<string> {
                Constants.StandardScopes.OpenId,
                Constants.StandardScopes.Profile,
                Constants.StandardScopes.Email,
                "kcsara-profile"
            }.Concat((row.AddedScopes ?? "").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToList(),
            AllowAccessToAllScopes = true,
            AccessTokenType = AccessTokenType.Jwt
          };
        }
        else
        {
          return new Client
          {
            ClientId = row.ClientId,
            ClientName = row.DisplayName,
            ClientSecrets = new List<Secret> { new Secret(row.Secret.Sha256()) },
            Enabled = row.Enabled,
            Flow = Flows.ClientCredentials,
            AllowedScopes = (row.AddedScopes ?? "").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList(),
            Claims = row.Roles.Select(f => new Claim(Scopes.RolesClaim, f.Id)).ToList(),
            PrefixClientClaims = false,
            AccessTokenType = AccessTokenType.Jwt
          };
        }
      }
    }
  }
}