using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer3.Core;
using Moq;
using Sar.Auth;
using Sar.Auth.Data;
using Xunit;

namespace Test.Auth.Registration
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
      args.Logger.Setup(f => f.Warning(LogStrings.EmailNotFound, It.IsAny<object[]>()));
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
      args.Logger.Setup(f => f.Warning(LogStrings.MultipleMembersForEmail, It.IsAny<object[]>()));
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
      args.Logger.Setup(f => f.Warning(LogStrings.MultipleAccountsForEmail, It.IsAny<object[]>()));
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
      args.Logger.Setup(f => f.Information(LogStrings.SendingVerifyCode, It.IsAny<object[]>()));
      args.EMails.Setup(f => f.SendEmail(email, Strings.VerifyMessageSubject, It.IsAny<string>(), true)).Returns(Task.FromResult(0));
      var svc = args.Build();

      var claimsId = new ClaimsIdentity(new[] { new Claim(Constants.ClaimTypes.ExternalProviderUserId, "abcdefg", null, "facebook") });

      var result = await svc.SendExternalVerificationCode(claimsId, email);
      result.Should().Be(ProcessVerificationResult.Success);
    }
  }
}
