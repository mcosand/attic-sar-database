/*
 * Copyright Matthew Cosand
 */
using System.Collections.Generic;
using IdentityServer3.Core.Models;

namespace Sar.Auth
{
  public static class Scopes
  {
    public static IEnumerable<Scope> Get()
    {
      return new List<Scope> {
            StandardScopes.OpenId,
            StandardScopes.Profile,
            StandardScopes.Email,
            new Scope { Name = "units", Type = ScopeType.Identity, Claims = new List<ScopeClaim> { new ScopeClaim("units") } },
            new Scope { Name = "truck-api" },
        };
    }
  }
}