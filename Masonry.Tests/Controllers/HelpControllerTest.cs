using Microsoft.VisualStudio.TestTools.UnitTesting;
using Masonry.Controllers;
using System.Web.Mvc;
using Moq;

namespace Masonry.Tests.Controllers
{
  [TestClass]
  public class HelpControllerTest
  {
    [TestMethod]
    public void Index()
    {
      var controllerMock = new Mock<HelpController> { CallBase = true };
      controllerMock.Setup(x => x.GetMarkdownContent(It.IsAny<string>())).Returns("content");

      var controller = controllerMock.Object;
      var result = controller.Index() as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.AreEqual("content", result.Model);

      controllerMock.Verify(x => x.GetMarkdownContent("Index"), Times.Once());
    }

    [TestMethod]
    public void Page()
    {
      var controllerMock = new Mock<HelpController> { CallBase = true };
      controllerMock.Setup(x => x.GetMarkdownContent(It.IsAny<string>())).Returns((string param) => param + "content");

      var controller = controllerMock.Object;
      var result = controller.Page("page1") as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual("Index", result.ViewName);
      Assert.AreEqual("page1", result.ViewBag.HelpPage);
      Assert.AreEqual("page1content", result.Model);

      controllerMock.Verify(x => x.GetMarkdownContent("page1"), Times.Once());
    }

    [TestMethod]
    public void PageIndexForMissing()
    {
      var controllerMock = new Mock<HelpController> { CallBase = true };
      controllerMock
        .Setup(x => x.GetMarkdownContent(It.IsAny<string>()))
        .Returns((string param) => param == "Index" ? "index_content" : null);

      var controller = controllerMock.Object;
      var result = controller.Page("page1") as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual("Index", result.ViewName);
      Assert.AreEqual("page1", result.ViewBag.HelpPage);
      Assert.AreEqual("index_content", result.Model);

      controllerMock.Verify(x => x.GetMarkdownContent("page1"), Times.Once());
      controllerMock.Verify(x => x.GetMarkdownContent("Index"), Times.Once());
    }
  }
}
