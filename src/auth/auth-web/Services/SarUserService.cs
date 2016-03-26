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

namespace Sar.Auth.Services
{
  public class SarUserService : UserServiceBase
  {
    private readonly IMemberInfoService _memberService;
    private readonly Func<IAuthDbContext> _dbFactory;
    private readonly ISendEmailService _emailService;

    public SarUserService(Func<IAuthDbContext> dbFactory, IMemberInfoService memberService, ISendEmailService email)
    {
      _memberService = memberService;
      _dbFactory = dbFactory;
      _emailService = email;
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

          claims.Add(new Claim("units", string.Join(",", member.Units.Select(f => f.Name))));
        }

        claims.Add(new Claim(Constants.ClaimTypes.Email, account.Email));
        claims.Add(new Claim(Constants.ClaimTypes.GivenName, account.FirstName));
        claims.Add(new Claim(Constants.ClaimTypes.FamilyName, account.LastName));
        claims.Add(new Claim(Constants.ClaimTypes.Name, account.FirstName + " " + account.LastName));

        context.IssuedClaims = claims.Where(f => context.RequestedClaimTypes.Contains(f.Type));
      }


    }

    public async Task VerifyExternalCode(ClaimsIdentity identity, string email, string code)
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
        if (verification == null)
        {
          throw new InvalidOperationException("Verification code not found or is invalid");
        }
        if (verification.Code != code)
        {
          throw new InvalidOperationException("Verification code not found or is invalid");
        }

        AccountRow account = null;
        await ProcessVerification(email, provider, providerUserId,
          db,
          m =>
          {
            account = new AccountRow { FirstName = m.FirstName, LastName = m.LastName, Email = email, MemberId = m.Id };
            db.Accounts.Add(account);
          },
          a => { account = a; });

        var login = new ExternalLoginRow { Account = account, Provider = provider, ProviderId = providerUserId };
        db.ExternalLogins.Add(login);
        db.Verifications.Remove(verification);
        await db.SaveChangesAsync();

        
      }
    }

    private async Task ProcessVerification(string email, string provider, string providerUserId,
      IAuthDbContext db, Action<Member> memberAction, Action<AccountRow> accountAction)
    {
      var existingLogin = await db.ExternalLogins.FirstOrDefaultAsync(f => f.Provider == provider && f.ProviderId == providerUserId);
      if (existingLogin != null)
      {
        throw new InvalidOperationException("Login already registered");
      }

      var accounts = await db.Accounts.Where(f => f.Email == email).ToListAsync();
      if (accounts.Count == 0)
      {
        var members = await _memberService.FindMembersByEmail(email);
        if (members.Count == 0)
        {
          throw new NotFoundException();
        }
        else if (members.Count > 1)
        {
          throw new MultipleMatchesException();
        }
        else if (memberAction != null)
        {
          memberAction(members[0]);
        }
      }
      else if (accounts.Count > 1)
      {
        throw new MultipleMatchesException();
      }
      else if (accountAction != null)
      {
        accountAction(accounts[0]);
      }
    }

    public async Task SendExternalVerificationCode(ClaimsIdentity identity, string email)
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
        await ProcessVerification(email, provider, providerUserId, db, null, null);

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

        await _emailService.SendEmail(email, "KCSARA Verification Code", "Your code: " + verification.Code);
      }
    }
  }
}