/*
 * Copyright Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
        return new Client
        {
          ClientId = row.ClientId,
          ClientName = row.DisplayName,
          ClientSecrets = string.IsNullOrWhiteSpace(row.Secret) ? new List<Secret>() : new List<Secret> { new Secret(row.Secret.Sha256()) },
          Enabled = row.Enabled,
          IdentityTokenLifetime = 60 * 30, // 30 minutes
          Flow = Flows.Hybrid,
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
          AccessTokenType = AccessTokenType.Jwt
        };
      }
    }
  }
}