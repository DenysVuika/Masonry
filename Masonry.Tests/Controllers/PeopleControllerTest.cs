using Masonry.Data.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Masonry.Controllers;
using System.Web.Mvc;
using Masonry.Models;

namespace Masonry.Tests.Controllers
{
  [TestClass]
  public class PeopleControllerTest : MasonryControllerTest<PeopleController>
  {
    [TestMethod]
    public void Index()
    {
      var profiles = new UserProfileModel[0];
      Repository.Setup(x => x.GetUserProfiles(It.IsAny<int>(), It.IsAny<int>())).Returns(profiles);
      Security.Setup(x => x.CurrentUserId).Returns(1);

      var controller = Controller.NonAjaxRequest().Object;
      var result = controller.Index(null) as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);

      var model = result.Model as PeopleModel;
      Assert.IsNotNull(model);
      Assert.AreEqual(profiles, model.Profiles);

      Repository.Verify(x => x.GetUserProfiles(1, 0), Times.Once());
    }

    [TestMethod]
    public void IndexAjax()
    {
      var profiles = new UserProfileModel[0];
      Repository.Setup(x => x.GetUserProfiles(It.IsAny<int>(), It.IsAny<int>())).Returns(profiles);
      Security.Setup(x => x.CurrentUserId).Returns(1);

      var controller = Controller.AjaxRequest().Object;
      var result = controller.Index(1) as JsonResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      Assert.AreEqual(profiles, result.Data);

      Repository.Verify(x => x.GetUserProfiles(1, 1), Times.Once());
    }

    [TestMethod]
    public void Following()
    {
      var user = new User { Id = 1, Account = "johndoe", Name = "john doe" };
      var profiles = new UserProfileModel[0];
      Repository.Setup(x => x.FindUser(It.IsAny<string>())).Returns(user);
      Repository.Setup(x => x.GetFollowings(It.IsAny<int>(), It.IsAny<int>())).Returns(profiles);

      var controller = Controller.NonAjaxRequest().Object;
      var result = controller.Following("user", 0) as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);

      var model = result.Model as PeopleModel;
      Assert.IsNotNull(model);
      Assert.AreEqual(user.Account, model.Account);
      Assert.AreEqual(user.Name, model.Name);
      Assert.AreEqual(profiles, model.Profiles);

      Repository.Verify(x => x.FindUser("user"), Times.Once());
      Repository.Verify(x => x.GetFollowings(1, 0), Times.Once());
    }

    [TestMethod]
    public void FollowingAjax()
    {
      var user = new User { Id = 1, Account = "johndoe", Name = "john doe" };
      var profiles = new UserProfileModel[0];
      Repository.Setup(x => x.FindUser(It.IsAny<string>())).Returns(user);
      Repository.Setup(x => x.GetFollowings(It.IsAny<int>(), It.IsAny<int>())).Returns(profiles);

      var controller = Controller.AjaxRequest().Object;
      var result = controller.Following("user", null) as JsonResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      Assert.AreEqual(profiles, result.Data);

      Repository.Verify(x => x.FindUser("user"), Times.Once());
      Repository.Verify(x => x.GetFollowings(1, 0), Times.Once());
    }

    [TestMethod]
    public void FollowingInvalidUser()
    {
      Repository.Setup(x => x.FindUser(It.IsAny<string>())).Returns((User)null);

      var controller = Controller.NonAjaxRequest().Object;
      var result = controller.Following("none", 0);
      Assert.IsNull(result);

      Repository.Verify(x => x.FindUser("none"), Times.Once());
      Repository.Verify(x => x.GetFollowings(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
    }

    [TestMethod]
    public void Followers()
    {
      var user = new User { Id = 1, Account = "johndoe", Name = "john doe" };
      var profiles = new UserProfileModel[0];
      Repository.Setup(x => x.FindUser(It.IsAny<string>())).Returns(user);
      Repository.Setup(x => x.GetFollowers(It.IsAny<int>(), It.IsAny<int>())).Returns(profiles);

      var controller = Controller.NonAjaxRequest().Object;
      var result = controller.Followers("user", null) as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);

      var model = result.Model as PeopleModel;
      Assert.IsNotNull(model);
      Assert.AreEqual(user.Account, model.Account);
      Assert.AreEqual(user.Name, model.Name);
      Assert.AreEqual(profiles, model.Profiles);

      Repository.Verify(x => x.FindUser("user"), Times.Once());
      Repository.Verify(x => x.GetFollowers(1, 0), Times.Once());
    }

    [TestMethod]
    public void FollowersAjax()
    {
      var user = new User { Id = 1, Account = "johndoe", Name = "john doe" };
      var profiles = new UserProfileModel[0];
      Repository.Setup(x => x.FindUser(It.IsAny<string>())).Returns(user);
      Repository.Setup(x => x.GetFollowers(It.IsAny<int>(), It.IsAny<int>())).Returns(profiles);

      var controller = Controller.AjaxRequest().Object;
      var result = controller.Followers("user", 0) as JsonResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      Assert.AreEqual(profiles, result.Data);

      Repository.Verify(x => x.FindUser("user"), Times.Once());
      Repository.Verify(x => x.GetFollowers(1, 0), Times.Once());
    }

    [TestMethod]
    public void FollowersInvalidUser()
    {
      Repository.Setup(x => x.FindUser(It.IsAny<string>())).Returns((User)null);

      var controller = Controller.NonAjaxRequest().Object;
      var result = controller.Followers("none", 0);
      Assert.IsNull(result);

      Repository.Verify(x => x.FindUser("none"), Times.Once());
      Repository.Verify(x => x.GetFollowers(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
    }

    [TestMethod]
    public void Follow()
    {
      Security.Setup(x => x.CurrentUserId).Returns(1);
      Repository.Setup(x => x.Follow(It.IsAny<int>(), It.IsAny<string>()));

      var result = Controller.Object.Follow("johndoe") as RedirectToRouteResult;
      Assert.IsNotNull(result);
      Assert.AreEqual("Index", result.RouteValues["action"]);
      Assert.AreEqual("People", result.RouteValues["controller"]);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.Follow(1, "johndoe"), Times.Once());
    }

    [TestMethod]
    public void Unfollow()
    {
      Security.Setup(x => x.CurrentUserId).Returns(1);
      Repository.Setup(x => x.Unfollow(It.IsAny<int>(), It.IsAny<string>()));
      
      var result = Controller.Object.Unfollow("johndoe") as RedirectToRouteResult;
      Assert.IsNotNull(result);
      Assert.AreEqual("Index", result.RouteValues["action"]);
      Assert.AreEqual("People", result.RouteValues["controller"]);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.Unfollow(1, "johndoe"), Times.Once());
    }
  }
}
