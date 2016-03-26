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
        return (await db.Clients.Where(f => f.ClientId == clientId)
          .ToListAsync())
          .Select(f => new Client
          {
            ClientId = f.ClientId,
            ClientName = f.DisplayName,
            Enabled = f.Enabled,
            Flow = Flows.Implicit,
            RequireConsent = false,
            AllowRememberConsent = false,
            RedirectUris = f.RedirectUris.Select(g => g.Uri).ToList(),
            PostLogoutRedirectUris = new List<string>(),
            AllowedScopes = new List<string> {
                Constants.StandardScopes.OpenId,
                Constants.StandardScopes.Profile,
                Constants.StandardScopes.Email,
            }.Concat((f.AddedScopes ?? "").Split(',')).ToList(),
            AllowAccessToAllScopes = true,
            AccessTokenType = AccessTokenType.Jwt
          }).SingleOrDefault();
      }
    }
  }
}