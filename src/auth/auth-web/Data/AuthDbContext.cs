/*
 * Copyright Matthew Cosand
 */
using System;
using System.Configuration;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Sar.Auth.Data
{
  public interface IAuthDbContext : IDisposable
  {
    IDbSet<AccountRow> Accounts { get; set; }
    IDbSet<ExternalLoginRow> ExternalLogins { get; set; }
    IDbSet<VerificationRow> Verifications { get; set; }

    IDbSet<ClientRow> Clients { get; set; }
    Task<int> SaveChangesAsync();
  }

  public class AuthDbContext : DbContext, IAuthDbContext
  {
    public AuthDbContext() : base(ConfigurationManager.AppSettings["authStore"]) { }

    public IDbSet<AccountRow> Accounts { get; set; }
    public IDbSet<ExternalLoginRow> ExternalLogins { get; set; }
    public IDbSet<VerificationRow> Verifications { get; set; }
    public IDbSet<ClientRow> Clients { get; set; }
  }
}