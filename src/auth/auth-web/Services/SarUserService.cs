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
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.Default;
using Sar.Auth.Data;
using Sar.Services;
using Serilog;

namespace Sar.Auth.Services
{
  public class SarUserService : UserServiceBase
  {
    private readonly IMemberInfoService _memberService;
    private readonly Func<IAuthDbContext> _dbFactory;
    private readonly ISendEmailService _emailService;
    private readonly IConfigService _config;
    private readonly ILogger _log;

    public SarUserService(Func<IAuthDbContext> dbFactory, IMemberInfoService memberService, ISendEmailService email, IConfigService config, ILogger log)
    {
      _memberService = memberService;
      _dbFactory = dbFactory;
      _emailService = email;
      _config = config;
      _log = log.ForContext<SarUserService>();
    }

    public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
    {
      using (var db = _dbFactory())
      {
        var account = await db.ExternalLogins
                        .Where(f => f.Provider == context.ExternalIdentity.Provider && f.ProviderId == context.ExternalIdentity.ProviderId)
                        .Select(f => f.Account).SingleOrDefaultAsync();

        if (account != null)
        {
          if (account.Locked.HasValue)
          {
            context.AuthenticateResult = new AuthenticateResult("Account is locked");
          }
          else
          {
            string name = account.FirstName + " " + account.LastName;
            if (account.MemberId.HasValue)
            {
              var member = await _memberService.GetMember(account.MemberId.Value);
              if (member == null)
              {
                throw new InvalidOperationException("Member not found in database");
              }
              name = member.FirstName + " " + member.LastName;
            }
            context.AuthenticateResult = new AuthenticateResult(account.Id.ToString(), name, null, context.ExternalIdentity.Provider);
          }
        }
        else
        {
          context.AuthenticateResult = new AuthenticateResult("~/registerlogin", context.ExternalIdentity);
        }
      }
    }

    public override Task AuthenticateLocalAsync(LocalAuthenticationContext context)
    {
      return base.AuthenticateLocalAsync(context);
    }

    public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
      List<Claim> claims = new List<Claim>();
      claims.Add(new Claim(Constants.ClaimTypes.ZoneInfo, "America/Los_Angeles"));

      using (var db = _dbFactory())
      {
        var accountId = new Guid(context.Subject.GetSubjectId());
        var account = await db.Accounts.SingleOrDefaultAsync(f => f.Id == accountId);

        if (account.MemberId.HasValue)
        {
          var member = await _memberService.GetMember(account.MemberId.Value);
          if (member == null)
          {
            throw new InvalidOperationException("Member not found in database");
          }
          account.Email = member.Email;
          account.FirstName = member.FirstName;
          account.LastName = member.LastName;

          claims.Add(new Claim(Scopes.UnitsClaim, string.Join(",", member.Units.Select(f => f.Name))));
          claims.Add(new Claim(Scopes.MemberIdClaim, member.Id.ToString()));
          string profileTemplate = _config["memberProfileTemplate"];
          if (!string.IsNullOrWhiteSpace(profileTemplate))
          {
            claims.Add(new Claim(Constants.ClaimTypes.Profile, string.Format(profileTemplate, member.Id)));
          }
        }

        claims.Add(new Claim(Constants.ClaimTypes.Email, account.Email));
        claims.Add(new Claim(Constants.ClaimTypes.GivenName, account.FirstName));
        claims.Add(new Claim(Constants.ClaimTypes.FamilyName, account.LastName));
        claims.Add(new Claim(Constants.ClaimTypes.Name, account.FirstName + " " + account.LastName));

        context.IssuedClaims = claims.Where(f => context.RequestedClaimTypes.Contains(f.Type));
      }


    }

    public async Task<ProcessVerificationResult> VerifyExternalCode(ClaimsIdentity identity, string email, string code)
    {
      if (string.IsNullOrWhiteSpace(code))
      {
        throw new ArgumentNullException(nameof(code));
      }

      var nameIdClaim = identity.Claims.First(x => x.Type == Constants.ClaimTypes.ExternalProviderUserId);
      var provider = nameIdClaim.Issuer;
      var providerUserId = nameIdClaim.Value;

      using (var db = _dbFactory())
      {
        var verification = await db.Verifications.FirstOrDefaultAsync(f => f.Provider == provider && f.ProviderId == providerUserId && f.Email == email);
        if (verification == null || verification.Code != code)
        {
          _log.Information("Verification code {verifyCode} for {email} is not correct", code, email);
          return ProcessVerificationResult.InvalidVerifyCode;
        }

        AccountRow account = null;
        var processResult = await ProcessVerification(email, provider, providerUserId,
          db,
          m =>
          {
            account = db.Accounts.Where(f => f.MemberId == m.Id).FirstOrDefault();
            if (account == null)
            {
              new AccountRow { FirstName = m.FirstName, LastName = m.LastName, Email = email, MemberId = m.Id };
              db.Accounts.Add(account);
            }
          },
          a => { account = a; });
        if (processResult != ProcessVerificationResult.Success) return processResult;

        var login = new ExternalLoginRow { Account = account, Provider = provider, ProviderId = providerUserId };
        db.ExternalLogins.Add(login);
        db.Verifications.Remove(verification);
        _log.Information("Associating login {provider}:{providerId} with {name}'s account", provider, providerUserId, account.FirstName + " " + account.LastName);
        await db.SaveChangesAsync();

        return ProcessVerificationResult.Success;
      }
    }

    private async Task<ProcessVerificationResult> ProcessVerification(string email, string provider, string providerUserId,
      IAuthDbContext db, Action<Member> memberAction, Action<AccountRow> accountAction)
    {
      var existingLogin = await db.ExternalLogins.FirstOrDefaultAsync(f => f.Provider == provider && f.ProviderId == providerUserId);
      if (existingLogin != null)
      {
        _log.Warning("{provider} login {providerId} already registered to account {account} ({first} {last}",
          existingLogin.Provider,
          existingLogin.ProviderId,
          existingLogin.AccountId,
          existingLogin.Account.FirstName,
          existingLogin.Account.LastName);
        return ProcessVerificationResult.AlreadyRegistered;
      }

      var accounts = await db.Accounts.Where(f => f.Email == email).ToListAsync();
      if (accounts.Count == 0)
      {
        var members = await _memberService.FindMembersByEmail(email);
        if (members.Count == 0)
        {
          _log.Warning("{email} does not exist in the database", email);
          return ProcessVerificationResult.EmailNotAvailable;
        }
        else if (members.Count > 1)
        {
          _log.Warning("{email} exists for multiple members: {@members}", email, members.Select(f => new { Name = f.FirstName + " " + f.LastName, Id = f.Id }));
          return ProcessVerificationResult.EmailNotAvailable;
        }
        else if (memberAction != null)
        {
          memberAction(members[0]);
        }
      }
      else if (accounts.Count > 1)
      {
        _log.Warning("{email} exists for multiple accounts: {@accounts}", email, accounts.Select(f => new { Name = f.FirstName + " " + f.LastName, Id = f.Id }));
        return ProcessVerificationResult.EmailNotAvailable;
      }
      else if (accountAction != null)
      {
        accountAction(accounts[0]);
      }

      return ProcessVerificationResult.Success;
    }

    public async Task<ProcessVerificationResult> SendExternalVerificationCode(ClaimsIdentity identity, string email)
    {
      if (string.IsNullOrWhiteSpace(email))
      {
        throw new ArgumentNullException(nameof(email));
      }

      var nameIdClaim = identity.Claims.First(x => x.Type == Constants.ClaimTypes.ExternalProviderUserId);
      var provider = nameIdClaim.Issuer;
      var providerUserId = nameIdClaim.Value;

      using (var db = _dbFactory())
      {
        var processresult = await ProcessVerification(email, provider, providerUserId, db, null, null);
        if (processresult != ProcessVerificationResult.Success) return processresult;

        var verification = await db.Verifications.SingleOrDefaultAsync(f => f.Provider == provider && f.ProviderId == providerUserId);
        if (verification == null)
        {
          verification = new VerificationRow { Provider = provider, ProviderId = providerUserId };
          db.Verifications.Add(verification);
        }

        verification.Created = DateTime.Now;
        verification.Code = Guid.NewGuid().ToString().ToLowerInvariant().Replace("-", string.Empty);
        verification.Email = email;
        await db.SaveChangesAsync();

        _log.Information("Sending verification code to {email} for login {provider}:{providerId}", email, provider, providerUserId);
        await _emailService.SendEmail(email, "KCSARA Verification Code", "Your code: " + verification.Code);

        return ProcessVerificationResult.Success;
      }
    }
  }
}