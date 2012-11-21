using Masonry.Controllers;
using Masonry.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Masonry.Tests.Controllers
{
  public abstract class MasonryControllerTest<TController> where TController : MasonryController
  {
    protected Mock<IMasonrySecurityService> Security { get; set; }
    protected Mock<IMasonryDataRepository> Repository { get; set; }
    protected Mock<IMasonrySettingsService> Settings { get; set; }
    protected Mock<TController> Controller { get; set; }

    [TestInitialize]
    public void Setup()
    {
      Security = new Mock<IMasonrySecurityService>();
      Security.Setup(x => x.CurrentUserId).Returns(1);

      Repository = new Mock<IMasonryDataRepository>();
      Settings = new Mock<IMasonrySettingsService>();

      Controller = new Mock<TController> { CallBase = true };
      Controller
        .SetupProperty(x => x.Security, Security.Object)
        .SetupProperty(x => x.Repository, Repository.Object)
        .SetupProperty(x => x.Settings, Settings.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
      Controller = null;
      Security = null;
      Repository = null;
      Settings = null;
    }
  }
}
