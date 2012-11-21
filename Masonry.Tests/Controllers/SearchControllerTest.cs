using Microsoft.VisualStudio.TestTools.UnitTesting;
using Masonry.Controllers;
using System.Web.Mvc;

namespace Masonry.Tests.Controllers
{
  [TestClass]
  public class SearchControllerTest
  {
    [TestMethod]
    public void Index()
    {
      var controller = new SearchController();
      var result = controller.Index(null) as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
    }
  }
}
