using Moq;
using Sar.Auth;
using Sar.Auth.Data;
using Sar.Auth.Services;
using Sar.Services;
using Serilog;

namespace Test.Auth
{
  public class UserServiceBuilder
  {
    public TestDB<IAuthDbContext> DB { get; private set; }
    public Mock<IMemberInfoService> Members { get; private set; }
    public Mock<ISendEmailService> EMails { get; private set; }
    public Mock<IConfigService> Config { get; private set; }
    public Mock<ILogger> Logger { get; private set; }
    public Mock<IRolesService> Roles { get; private set; }

    public UserServiceBuilder()
    {
      DB = new TestDB<IAuthDbContext>();
      Members = new Mock<IMemberInfoService>(MockBehavior.Strict);
      Roles = new Mock<IRolesService>(MockBehavior.Strict);
      EMails = new Mock<ISendEmailService>(MockBehavior.Strict);
      Config = new Mock<IConfigService>(MockBehavior.Strict);
      Logger = new Mock<ILogger>(MockBehavior.Strict);
      Logger.Setup(f => f.ForContext<SarUserService>()).Returns(Logger.Object);
    }

    public SarUserService Build()
    {
      return new SarUserService(() => DB.Object, Members.Object,Roles.Object, EMails.Object, Config.Object, Logger.Object);
    }
  }
}
