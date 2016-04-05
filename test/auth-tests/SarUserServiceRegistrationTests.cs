using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer3.Core;
using Moq;
using Sar.Auth;
using Sar.Auth.Data;
using Sar.Auth.Services;
using Sar.Services;
using Serilog;
using Xunit;

namespace Test.Auth
{
  public class SarUserServiceRegistrationTests
  {
    [Fact]
    public async Task EmailNull()
    {
      var args = new UserServiceBuilder();

      var svc = args.Build();

      var claimsId = new ClaimsIdentity(new[] { new Claim(Constants.ClaimTypes.ExternalProviderUserId, "abcdefg", null, "facebook") });

      Func<Task<ProcessVerificationResult>> act = async () => await svc.SendExternalVerificationCode(claimsId, null);
      act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task NoMemberNoAccountWithEmail()
    {
      string email = "test@example.com";

      var args = new UserServiceBuilder();
      args.Members.Setup(f => f.FindMembersByEmail(email)).Returns(Task.FromResult<IList<Member>>(new List<Member>()));
      args.Logger.Setup(f => f.Warning("{email} does not exist in the database", It.IsAny<object[]>()));
      var svc = args.Build();

      var claimsId = new ClaimsIdentity(new[] { new Claim(Constants.ClaimTypes.ExternalProviderUserId, "abcdefg", null, "facebook") });

      var result = await svc.SendExternalVerificationCode(claimsId, email);
      result.Should().Be(ProcessVerificationResult.EmailNotAvailable);
    }

    [Fact]
    public async Task ManyMembersNoAccountWithEmail()
    {
      string email = "test@example.com";

      var args = new UserServiceBuilder();
      args.Members.Setup(f => f.FindMembersByEmail(email)).Returns(Task.FromResult<IList<Member>>(new List<Member> {
        new Member { Email = email, FirstName = "First", LastName = "Last", Id = Guid.NewGuid() },
        new Member { Email = email, FirstName = "Other First", LastName = "Other Last", Id = Guid.NewGuid() }
        }));
      args.Logger.Setup(f => f.Warning("{email} exists for multiple members: {@members}", It.IsAny<object[]>()));
      var svc = args.Build();

      var claimsId = new ClaimsIdentity(new[] { new Claim(Constants.ClaimTypes.ExternalProviderUserId, "abcdefg", null, "facebook") });

      var result = await svc.SendExternalVerificationCode(claimsId, email);
      result.Should().Be(ProcessVerificationResult.EmailNotAvailable);
    }

    [Fact]
    public async Task ManyAccountsWithEmail()
    {
      string email = "test@example.com";

      var args = new UserServiceBuilder();
      args.DB.AddRange(new[]
      {
        new AccountRow { Email = email, FirstName = "First", LastName = "Last", Id = Guid.NewGuid() },
        new AccountRow { Email = email, FirstName = "Other First", LastName = "Other Last", Id = Guid.NewGuid() }
      });
      args.Members.Setup(f => f.FindMembersByEmail(email)).Returns(Task.FromResult<IList<Member>>(new List<Member> {
        new Member { Email = email, FirstName = "First", LastName = "Last", Id = Guid.NewGuid() }
      }));
      args.Logger.Setup(f => f.Warning("{email} exists for multiple accounts: {@accounts}", It.IsAny<object[]>()));
      var svc = args.Build();

      var claimsId = new ClaimsIdentity(new[] { new Claim(Constants.ClaimTypes.ExternalProviderUserId, "abcdefg", null, "facebook") });

      var result = await svc.SendExternalVerificationCode(claimsId, email);
      result.Should().Be(ProcessVerificationResult.EmailNotAvailable);
    }

    [Fact]
    public async Task OneMemberNoAccountSendCode()
    {
      string email = "test@example.com";

      var args = new UserServiceBuilder();
      args.Members.Setup(f => f.FindMembersByEmail(email)).Returns(Task.FromResult<IList<Member>>(new List<Member> {
        new Member { Email = email, FirstName = "First", LastName = "Last", Id = Guid.NewGuid() }
        }));
      args.Logger.Setup(f => f.Information("Sending verification code to {email} for login {provider}:{providerId}", It.IsAny<object[]>()));
      args.EMails.Setup(f => f.SendEmail(email, "KCSARA Verification Code", It.IsAny<string>(), true)).Returns(Task.FromResult(0));
      var svc = args.Build();

      var claimsId = new ClaimsIdentity(new[] { new Claim(Constants.ClaimTypes.ExternalProviderUserId, "abcdefg", null, "facebook") });

      var result = await svc.SendExternalVerificationCode(claimsId, email);
      result.Should().Be(ProcessVerificationResult.Success);
    }




    class UserServiceBuilder
    {
      public TestDB<IAuthDbContext> DB { get; private set; }
      public Mock<IMemberInfoService> Members { get; private set; }
      public Mock<ISendEmailService> EMails { get; private set; }
      public Mock<IConfigService> Config { get; private set; }
      public Mock<ILogger> Logger { get; private set; }

      public UserServiceBuilder()
      {
        DB = new TestDB<IAuthDbContext>();
        Members = new Mock<IMemberInfoService>(MockBehavior.Strict);
        EMails = new Mock<ISendEmailService>(MockBehavior.Strict);
        Config = new Mock<IConfigService>(MockBehavior.Strict);
        Logger = new Mock<ILogger>(MockBehavior.Strict);
        Logger.Setup(f => f.ForContext<SarUserService>()).Returns(Logger.Object);
      }

      public SarUserService Build()
      {
        return new SarUserService(() => DB.Object, Members.Object, EMails.Object, Config.Object, Logger.Object);
      }
    }
  }
}
