/*
 * Copyright Matthew Cosand
 */
using System.Collections.Generic;
using IdentityServer3.Core.Models;

namespace Sar.Auth
{
  public static class Scopes
  {
    public static readonly string UnitsClaim = "units";
    public static readonly string MemberIdClaim = "memberId";
    public static readonly string RolesClaim = "roles";

    public static IEnumerable<Scope> Get()
    {
      return new List<Scope> {
            StandardScopes.OpenId,
            StandardScopes.Profile,
            StandardScopes.Email,
            new Scope { Name = "kcsara-profile", Type = ScopeType.Identity, Claims = new List<ScopeClaim>
            {
              new ScopeClaim(UnitsClaim),
              new ScopeClaim(MemberIdClaim),
              new ScopeClaim(RolesClaim)
            } },
            new Scope { Name = "truck-api" },
        };
    }
  }
}