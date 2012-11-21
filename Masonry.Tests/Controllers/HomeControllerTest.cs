using Masonry.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web.Mvc;

namespace Masonry.Tests.Controllers
{
  [TestClass]
  public class HomeControllerTest : MasonryControllerTest<HomeController>
  {
    [TestMethod]
    public void Index()
    {
      var result = Controller.Object.Index() as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
    }

    [TestMethod]
    public void IndexAuthenticated()
    {
      Security.Setup(x => x.IsAuthenticated).Returns(true);
      var result = Controller.Object.Index() as RedirectToRouteResult;

      Assert.IsNotNull(result);
      Assert.AreEqual("Index", result.RouteValues["action"]);
      Assert.AreEqual("Timeline", result.RouteValues["controller"]);

      Security.Verify(x => x.IsAuthenticated, Times.Once());
    }

    [TestMethod]
    public void About()
    {
      var controller = new HomeController();
      var result = controller.About() as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
    }
  }
}
