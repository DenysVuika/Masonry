using System.Web.Mvc;
using Masonry.Controllers;
using Masonry.Data.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Masonry.Models;

namespace Masonry.Tests.Controllers
{
  [TestClass]
  public class TimelineControllerTest : MasonryControllerTest<TimelineController>
  {
    [TestMethod]
    public void Index()
    {
      var user = new User { Id = 1, Account = "johndoe", Name = "john doe" };
      var posts = new PostModel[0];

      Repository.Setup(x => x.FindUser(It.IsAny<int>())).Returns(user);
      Repository.Setup(x => x.GetTimelineEntries(It.IsAny<int>(), It.IsAny<int>())).Returns(posts);
      
      var controller = Controller.NonAjaxRequest().Object;
      var result = controller.Index(0) as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);

      var model = result.Model as TimelineModel;
      Assert.IsNotNull(model);
      Assert.AreEqual(user.Account, model.Account);
      Assert.AreEqual(user.Name, model.Name);
      Assert.AreEqual(posts, model.Posts);

      Repository.Verify(x => x.FindUser(1), Times.Once());
      Repository.Verify(x => x.GetTimelineEntries(1, 0), Times.Once());
    }

    [TestMethod]
    public void IndexAjax()
    {
      var user = new User { Id = 1, Account = "johndoe", Name = "john doe" };
      var posts = new PostModel[0];

      Repository.Setup(x => x.FindUser(It.IsAny<int>())).Returns(user);
      Repository.Setup(x => x.GetTimelineEntries(It.IsAny<int>(), It.IsAny<int>())).Returns(posts);

      var controller = Controller.AjaxRequest().Object;
      var result = controller.Index(null) as JsonResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);

      var model = result.Data as TimelineModel;
      Assert.IsNotNull(model);
      Assert.AreEqual(user.Account, model.Account);
      Assert.AreEqual(user.Name, model.Name);
      Assert.AreEqual(posts, model.Posts);

      Repository.Verify(x => x.FindUser(1), Times.Once());
      Repository.Verify(x => x.GetTimelineEntries(1, 0), Times.Once());
    }

    [TestMethod]
    public void CheckUpdates()
    {
      Repository.Setup(x => x.CheckTimelineUpdates(It.IsAny<int>(), It.IsAny<int>())).Returns(15);

      var result = Controller.Object.CheckUpdates(null);
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      Assert.AreEqual(15, result.Data);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.CheckTimelineUpdates(1, 0), Times.Once());
    }

    [TestMethod]
    public void CheckUpdatesWithTop()
    {
      Repository.Setup(x => x.CheckTimelineUpdates(It.IsAny<int>(), It.IsAny<int>())).Returns(15);

      var result = Controller.Object.CheckUpdates(20);
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      Assert.AreEqual(15, result.Data);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.CheckTimelineUpdates(1, 20), Times.Once());
    }

    [TestMethod]
    public void Updates()
    {
      var posts = new PostModel[0];
      Repository.Setup(x => x.GetTimelineUpdates(It.IsAny<int>(), It.IsAny<int>())).Returns(posts);

      var result = Controller.Object.Updates(null);
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      Assert.AreEqual(posts, result.Data);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.GetTimelineUpdates(1, 0), Times.Once());
    }

    [TestMethod]
    public void UpdatesWithTop()
    {
      var posts = new PostModel[0];
      Repository.Setup(x => x.GetTimelineUpdates(It.IsAny<int>(), It.IsAny<int>())).Returns(posts);

      var result = Controller.Object.Updates(20);
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      Assert.AreEqual(posts, result.Data);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.GetTimelineUpdates(1, 20), Times.Once());
    }

    [TestMethod]
    public void Mentions()
    {
      var user = new User { Id = 1, Account = "johndoe", Name = "john doe" };
      var posts = new PostModel[0];

      Repository.Setup(x => x.FindUser(It.IsAny<int>())).Returns(user);
      Repository.Setup(x => x.GetMentions(It.IsAny<string>(), It.IsAny<int>())).Returns(posts);

      var controller = Controller.NonAjaxRequest().Object;
      var result = controller.Mentions(null) as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);

      var model = result.Model as TimelineModel;
      Assert.IsNotNull(model);
      Assert.AreEqual(user.Account, model.Account);
      Assert.AreEqual(user.Name, model.Name);
      Assert.AreEqual(posts, model.Posts);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.FindUser(1), Times.Once());
      Repository.Verify(x => x.GetMentions("johndoe", 0), Times.Once());
    }

    [TestMethod]
    public void MentionsWithTop()
    {
      var user = new User { Id = 1, Account = "johndoe", Name = "john doe" };
      var posts = new PostModel[0];

      Repository.Setup(x => x.FindUser(It.IsAny<int>())).Returns(user);
      Repository.Setup(x => x.GetMentions(It.IsAny<string>(), It.IsAny<int>())).Returns(posts);

      var controller = Controller.NonAjaxRequest().Object;
      var result = controller.Mentions(100) as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);

      var model = result.Model as TimelineModel;
      Assert.IsNotNull(model);
      Assert.AreEqual(user.Account, model.Account);
      Assert.AreEqual(user.Name, model.Name);
      Assert.AreEqual(posts, model.Posts);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.FindUser(1), Times.Once());
      Repository.Verify(x => x.GetMentions("johndoe", 100), Times.Once());
    }

    [TestMethod]
    public void MentionsAjax()
    {
      var user = new User { Id = 1, Account = "johndoe", Name = "john doe" };
      var posts = new PostModel[0];

      Repository.Setup(x => x.FindUser(It.IsAny<int>())).Returns(user);
      Repository.Setup(x => x.GetMentions(It.IsAny<string>(), It.IsAny<int>())).Returns(posts);

      var controller = Controller.AjaxRequest().Object;
      var result = controller.Mentions(null) as JsonResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);

      var model = result.Data as TimelineModel;
      Assert.IsNotNull(model);
      Assert.AreEqual(user.Account, model.Account);
      Assert.AreEqual(user.Name, model.Name);
      Assert.AreEqual(posts, model.Posts);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.FindUser(1), Times.Once());
      Repository.Verify(x => x.GetMentions("johndoe", 0), Times.Once());
    }
    
    [TestMethod]
    public void Feed()
    {
      var user = new User { Id = 1, Account = "johndoe", Name = "john doe" };
      var posts = new PostModel[0];

      Repository.Setup(x => x.FindUser(It.IsAny<string>())).Returns(user);
      Repository.Setup(x => x.GetFeedEntries(It.IsAny<int>(), It.IsAny<int>())).Returns(posts);

      var controller = Controller.NonAjaxRequest().Object;
      var result = controller.Feed(user.Account, null) as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.AreEqual(user.Account, result.ViewBag.NavigationFeed);

      var model = result.Model as TimelineModel;
      Assert.IsNotNull(model);
      Assert.AreEqual(user.Account, model.Account);
      Assert.AreEqual(user.Name, model.Name);
      Assert.AreEqual(posts, model.Posts);

      Repository.Verify(x => x.FindUser(user.Account), Times.Once());
      Repository.Verify(x => x.GetFeedEntries(1, 0), Times.Once());
    }
    
    [TestMethod]
    public void FeedWithInvalidUser()
    {
      Repository.Setup(x => x.FindUser(It.IsAny<string>())).Returns((User)null);

      var controller = Controller.NonAjaxRequest().Object;
      var result = controller.Feed("user", null) as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.AreEqual("user", result.ViewBag.NavigationFeed);
      Assert.IsNull(result.Model);

      Repository.Verify(x => x.FindUser("user"), Times.Once());
      Repository.Verify(x => x.GetFeedEntries(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
    }
    
    [TestMethod]
    public void FeedAjax()
    {
      var user = new User { Id = 1, Account = "johndoe", Name = "john doe" };
      var posts = new PostModel[0];

      Repository.Setup(x => x.FindUser(It.IsAny<string>())).Returns(user);
      Repository.Setup(x => x.GetFeedEntries(It.IsAny<int>(), It.IsAny<int>())).Returns(posts);

      var controller = Controller.AjaxRequest().Object;
      var result = controller.Feed(user.Account, 10) as JsonResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      
      var model = result.Data as TimelineModel;
      Assert.IsNotNull(model);
      Assert.AreEqual(user.Account, model.Account);
      Assert.AreEqual(user.Name, model.Name);
      Assert.AreEqual(posts, model.Posts);

      Repository.Verify(x => x.FindUser(user.Account), Times.Once());
      Repository.Verify(x => x.GetFeedEntries(1, 10), Times.Once());
    }

    [TestMethod]
    public void Comments()
    {
      var comments = new CommentModel[0];
      Repository.Setup(x => x.GetComments(It.IsAny<int>())).Returns(comments);
      
      var result = Controller.Object.Comments(1);
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      Assert.AreEqual(comments, result.Data);

      Repository.Verify(x => x.GetComments(1), Times.Once());
    }

    [TestMethod]
    public void CommentsWithoutPostId()
    {
      var controller = Controller.Object;
      
      var result = controller.Comments(null);
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      Assert.IsNull(result.Data);

      result = controller.Comments(0);
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      Assert.IsNull(result.Data);

      Repository.Verify(x => x.GetComments(It.IsAny<int>()), Times.Never());
    }

    [TestMethod]
    public void Post()
    {
      var post = new PostModel();
      Repository.Setup(x => x.GetPost(It.IsAny<int>())).Returns(post);
      
      var result = Controller.Object.Post(10) as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.AreEqual(post, result.Model);

      Repository.Verify(x => x.GetPost(10), Times.Once());
    }

    [TestMethod]
    public void Comment()
    {
      var comment = new CommentModel { PostId = 2, Content = "post content" };
      Repository.Setup(x => x.CreateComment(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).Returns(comment);
      
      var result = Controller.Object.Comment(comment);
      Assert.IsNotNull(result);
      Assert.AreEqual(comment, result.Data);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.CreateComment(1, comment.PostId, comment.Content), Times.Once());
    }

    [TestMethod]
    public void DeletePost()
    {
      Repository.Setup(x => x.DeletePost(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

      var controller = Controller.NonAjaxRequest().Object;
      var result = controller.DeletePost(10) as RedirectToRouteResult;

      Assert.IsNotNull(result);
      Assert.AreEqual("Index", result.RouteValues["action"]);
      Assert.AreEqual("Timeline", result.RouteValues["controller"]);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.DeletePost(1, 10), Times.Once());
    }

    [TestMethod]
    public void DeletePostAjax()
    {
      Repository.Setup(x => x.DeletePost(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

      var controller = Controller.AjaxRequest().Object;
      var result = controller.DeletePost(10) as JsonResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
      Assert.IsTrue((bool)result.Data);
    }

    [TestMethod]
    public void UpdateStatus()
    {
      var post = new PostModel { Content = "content" };

      Repository.Setup(x => x.CreatePost(It.IsAny<int>(), It.IsAny<string>())).Returns(post);
      
      var result = Controller.Object.UpdateStatus(post);
      Assert.IsNotNull(result);
      Assert.AreEqual(post, result.Data);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.CreatePost(1, "content"), Times.Once());
    }

    [TestMethod]
    public void UpdateStatusWithoutModel()
    {
      var result = Controller.Object.UpdateStatus(null);
      Assert.IsNotNull(result);
      Assert.IsNull(result.Data);

      Security.Verify(x => x.CurrentUserId, Times.Never());
      Repository.Verify(x => x.CreatePost(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
    }

    [TestMethod]
    public void UpdateStatusWithoutContent()
    {
      var result = Controller.Object.UpdateStatus(new PostModel());
      Assert.IsNotNull(result);
      Assert.IsNull(result.Data);

      Security.Verify(x => x.CurrentUserId, Times.Never());
      Repository.Verify(x => x.CreatePost(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
    }

    [TestMethod]
    public void Follow()
    {
      Repository.Setup(x => x.Follow(It.IsAny<int>(), It.IsAny<string>()));
      
      var result = Controller.Object.Follow("johndoe") as RedirectToRouteResult;

      Assert.IsNotNull(result);
      Assert.AreEqual("Index", result.RouteValues["action"]);
      Assert.AreEqual("Timeline", result.RouteValues["controller"]);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.Follow(1, "johndoe"), Times.Once());
    }

    [TestMethod]
    public void Unfollow()
    {
      Repository.Setup(x => x.Unfollow(It.IsAny<int>(), It.IsAny<string>()));
      
      var result = Controller.Object.Unfollow("johndoe") as RedirectToRouteResult;

      Assert.IsNotNull(result);
      Assert.AreEqual("Index", result.RouteValues["action"]);
      Assert.AreEqual("Timeline", result.RouteValues["controller"]);

      Security.Verify(x => x.CurrentUserId, Times.Once());
      Repository.Verify(x => x.Unfollow(1, "johndoe"), Times.Once());
    }
  }
}
