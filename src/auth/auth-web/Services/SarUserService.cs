/*
 * Copyright Matthew Cosand
 */
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
    private readonly IRolesService _roles;
    private readonly Func<IAuthDbContext> _dbFactory;
    private readonly ISendEmailService _emailService;
    private readonly IConfigService _config;
    private readonly ILogger _log;

    public SarUserService(Func<IAuthDbContext> dbFactory, IMemberInfoService memberService, IRolesService roles, ISendEmailService email, IConfigService config, ILogger log)
    {
      _memberService = memberService;
      _roles = roles;
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
            context.AuthenticateResult = new AuthenticateResult(Strings.AccountLocked);
          }
          else
          {
            string name = account.FirstName + " " + account.LastName;
            if (account.MemberId.HasValue)
            {
              var member = await _memberService.GetMember(account.MemberId.Value);
              if (member == null)
              {
                throw new InvalidOperationException(LogStrings.MemberNotFound);
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

    public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
    {
      context.AuthenticateResult = new AuthenticateResult(Strings.UserPasswordNotCorrect);

      using (var db = _dbFactory())
      {
        var account = await db.Accounts
          .Where(f => f.Username == context.UserName)
          .SingleOrDefaultAsync();

        if (account != null && !string.IsNullOrWhiteSpace(account.PasswordHash))
        {
          if (!PasswordsMatch(context.Password, account.PasswordHash))
          {
            context.AuthenticateResult = new AuthenticateResult(Strings.UserPasswordNotCorrect);
            return;
          }

          if (account.Locked.HasValue)
          {
            _log.Warning(LogStrings.LockedAccountAttempt, account);
            context.AuthenticateResult = new AuthenticateResult(Strings.AccountLocked);
            return;
          }

          if (account.MemberId.HasValue)
          {
            var member = await _memberService.GetMember(account.MemberId.Value);
            if (member == null)
            {
              context.AuthenticateResult = new AuthenticateResult(Strings.AccountLocked);
              return;
            }
            if (account.FirstName != member.FirstName || account.LastName != member.LastName || account.Email != member.Email)
            {
              account.FirstName = member.FirstName;
              account.LastName = member.LastName;
              account.Email = member.Email;
              await db.SaveChangesAsync();
            }
          }

          if (string.IsNullOrWhiteSpace(account.FirstName) || string.IsNullOrWhiteSpace(account.LastName))
          {
            _log.Error(LogStrings.AccountHasNoName, account.Username);
          }
          context.AuthenticateResult = new AuthenticateResult(account.Id.ToString(), string.Format("{0} {1}", account.FirstName, account.LastName));
        }
      }
    }

    public const int PasswordSaltLength = 24;

    public static string HashPassword(string password, string salt)
    {
      byte[] bytes = Encoding.Unicode.GetBytes(password);
      byte[] src = Convert.FromBase64String(salt);
      byte[] dst = new byte[src.Length + bytes.Length];
      Buffer.BlockCopy(src, 0, dst, 0, src.Length);
      Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
      HashAlgorithm algorithm = HashAlgorithm.Create("SHA1");
      byte[] inArray = algorithm.ComputeHash(dst);
      return Convert.ToBase64String(inArray);
    }

    public static bool PasswordsMatch(string password, string hashedPassword)
    {
      var salt = hashedPassword.Substring(0, PasswordSaltLength);
      var hashed = HashPassword(password, salt);
      return string.Equals(hashedPassword.Substring(PasswordSaltLength), hashed);
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
            throw new InvalidOperationException(LogStrings.MemberNotFound);
          }
          account.Email = member.Email;
          account.FirstName = member.FirstName;
          account.LastName = member.LastName;

          var units = member.Units.Select(f => f.Name);
          foreach (var unit in units)
          {
            claims.Add(new Claim(Scopes.UnitsClaim, unit));
          }
          if (units.Any())
          {
            claims.Add(new Claim(Scopes.RolesClaim, "cdb.users"));
          }

          claims.Add(new Claim(Scopes.MemberIdClaim, member.Id.ToString()));

          string profileTemplate = _config["memberProfileTemplate"];
          if (!string.IsNullOrWhiteSpace(profileTemplate))
          {
            claims.Add(new Claim(Constants.ClaimTypes.Profile, string.Format(profileTemplate, member.Id)));
          }

          var roles = _roles.RolesForAccount(account.Id);
          foreach (var role in roles)
          {
            claims.Add(new Claim(Scopes.RolesClaim, role));
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
          _log.Information(LogStrings.VerificationCodeNotCorrect, code, email);
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
              account = new AccountRow { Email = email, MemberId = m.Id };
              db.Accounts.Add(account);
            }
            account.FirstName = m.FirstName;
            account.LastName = m.LastName;
          },
          a => { account = a; });
        if (processResult != ProcessVerificationResult.Success) return processResult;

        var login = new ExternalLoginRow { Account = account, Provider = provider, ProviderId = providerUserId };
        db.ExternalLogins.Add(login);
        db.Verifications.Remove(verification);
        _log.Information(LogStrings.AssociatingExternalLogin, provider, providerUserId, account.FirstName + " " + account.LastName);
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
        _log.Warning(LogStrings.AlreadyRegistered,
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
          _log.Warning(LogStrings.EmailNotFound, email);
          return ProcessVerificationResult.EmailNotAvailable;
        }
        else if (members.Count > 1)
        {
          _log.Warning(LogStrings.MultipleMembersForEmail, email, members.Select(f => new { Name = f.FirstName + " " + f.LastName, Id = f.Id }));
          return ProcessVerificationResult.EmailNotAvailable;
        }
        else if (memberAction != null)
        {
          memberAction(members[0]);
        }
      }
      else if (accounts.Count > 1)
      {
        _log.Warning(LogStrings.MultipleAccountsForEmail, email, accounts.Select(f => new { Name = f.FirstName + " " + f.LastName, Id = f.Id }));
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

        _log.Information(LogStrings.SendingVerifyCode, email, provider, providerUserId);
        await _emailService.SendEmail(email, Strings.VerifyMessageSubject, string.Format(Strings.VerifyMessageHtml, verification.Code));

        return ProcessVerificationResult.Success;
      }
    }
  }
}