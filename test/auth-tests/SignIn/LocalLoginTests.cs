/*
 * Copyright Matthew Cosand
 */
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using Sar.Auth;
using Sar.Auth.Data;
using Sar.Auth.Services;
using Xunit;

namespace Test.Auth.SignIn
{
  public class LocalLoginTests
  {
    [Fact]
    public async Task Login()
    {
      var acct = GetSampleAccount();

      var args = new UserServiceBuilder();
      args.DB.Add(acct);
      var svc = args.Build();

      var ctx = new LocalAuthenticationContext { UserName = Username, Password = Password };
      await svc.AuthenticateLocalAsync(ctx);

      ctx.AuthenticateResult.IsError.Should().BeFalse();
      ctx.AuthenticateResult.IsPartialSignIn.Should().BeFalse();
      var identity = ctx.AuthenticateResult.User as ClaimsPrincipal;
      identity.Identity.Name.Should().Be("First Last");
      identity.FindFirst(Constants.ClaimTypes.Subject).Value.Should().Be(acct.Id.ToString());
    }

    [Fact]
    public async Task InvalidUser()
    {
      var args = new UserServiceBuilder();
      var svc = args.Build();
      var ctx = new LocalAuthenticationContext { UserName = Username, Password = "Doesn't Matter" };
      await svc.AuthenticateLocalAsync(ctx);

      ctx.AuthenticateResult.IsError.Should().BeTrue();
      ctx.AuthenticateResult.ErrorMessage.Should().Be(Strings.UserPasswordNotCorrect);
    }

    [Fact]
    public async Task InvalidPassword()
    {
      var args = new UserServiceBuilder();
      var acct = GetSampleAccount();
      args.DB.Add(acct);
      var svc = args.Build();
      var ctx = new LocalAuthenticationContext { UserName = Username, Password = "Not this one" };
      await svc.AuthenticateLocalAsync(ctx);

      ctx.AuthenticateResult.IsError.Should().BeTrue();
      ctx.AuthenticateResult.ErrorMessage.Should().Be(Strings.UserPasswordNotCorrect);
    }

    [Fact]
    public async Task Locked()
    {
      var args = new UserServiceBuilder();
      var acct = GetSampleAccount();
      acct.Locked = DateTime.Now;
      acct.LockReason = "Some reason";
      args.DB.Add(acct);
      args.Logger.Setup(f => f.Warning(LogStrings.LockedAccountAttempt, acct));
      var svc = args.Build();
      var ctx = new LocalAuthenticationContext { UserName = Username, Password = Password };
      await svc.AuthenticateLocalAsync(ctx);

      ctx.AuthenticateResult.IsError.Should().BeTrue();
      ctx.AuthenticateResult.ErrorMessage.Should().Be(Strings.AccountLocked);
    }

    [Fact]
    public async Task UpdateFromMember()
    {
      var member = new Member { Id = Guid.NewGuid(), Email = "newemail@example.local", FirstName = "UpdatedFirst", LastName = "UpdatedLast" };
      var acct = GetSampleAccount();
      acct.MemberId = member.Id;

      var args = new UserServiceBuilder();
      args.DB.Add(acct);
      args.Members.Setup(f => f.GetMember(member.Id)).Returns(Task.FromResult(member));
      var svc = args.Build();

      var ctx = new LocalAuthenticationContext { UserName = Username, Password = Password };
      args.DB.SaveChangesCount = 0;
      await svc.AuthenticateLocalAsync(ctx);

      ctx.AuthenticateResult.IsError.Should().BeFalse();
      args.DB.SaveChangesCount.Should().Be(1);
      acct.Email.Should().Be(member.Email);
      acct.FirstName.Should().Be(member.FirstName);
      acct.LastName.Should().Be(member.LastName);
    }

    [Fact]
    public async Task MissingMember()
    {
      var member = new Member { Id = Guid.NewGuid(), Email = "newemail@example.local", FirstName = "UpdatedFirst", LastName = "UpdatedLast" };
      var acct = GetSampleAccount();
      acct.MemberId = member.Id;

      var args = new UserServiceBuilder();
      args.DB.Add(acct);
      args.Logger.Setup(f => f.Error(LogStrings.LinkedMemberNotFound, acct));
      args.Members.Setup(f => f.GetMember(member.Id)).Returns(Task.FromResult((Member)null));
      var svc = args.Build();

      var ctx = new LocalAuthenticationContext { UserName = Username, Password = Password };
      args.DB.SaveChangesCount = 0;
      await svc.AuthenticateLocalAsync(ctx);

      ctx.AuthenticateResult.IsError.Should().BeTrue();
      ctx.AuthenticateResult.ErrorMessage.Should().Be(Strings.AccountLocked);
    }


    private const string Salt = "LquQSo3N3G/vJSjp70j3bA==";
    private const string Username = "testuser";
    private const string Password = "IAmAPa$$word";
    private static AccountRow GetSampleAccount()
    {
      return new AccountRow
      {
        Id = Guid.NewGuid(),
        Username = Username,
        PasswordHash = Salt + SarUserService.HashPassword(Password, Salt),
        Email = "test@example.com",
        FirstName = "First",
        LastName = "Last"
      };
    }
  }
}
