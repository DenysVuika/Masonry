using Masonry.Controllers;
using Masonry.Data.Model;
using Masonry.Models;
using Masonry.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Masonry.Tests.Controllers
{
  [TestClass]
  public class AccountControllerTest : MasonryControllerTest<AccountController>
  {
    [TestMethod]
    public void Index()
    {
      var controller = new AccountController();
      var result = controller.Index() as RedirectToRouteResult;
      Assert.IsNotNull(result);

      var routeValues = result.RouteValues;

      Assert.AreEqual("AccountSettings", routeValues["action"]);
      Assert.AreEqual("Account", routeValues["controller"]);
    }

    [TestMethod]
    public void AccountSettings()
    {
      var user = new User
      {
        Account = "johndoe",
        Name = "John Doe",
        Website = "http://google.com",
        Location = "Country",
        Bio = "Some bio"
      };
      
      Repository.Setup(mock => mock.FindUser(It.IsAny<int>())).Returns(user);
      
      var result = Controller.Object.AccountSettings() as ViewResult;
      Assert.IsNotNull(result);
      
      var model = result.Model as AccountSettingsModel;
      Assert.IsNotNull(model);

      Assert.AreEqual(user.Account, model.Account);
      Assert.AreEqual(user.Name, model.Name);
      Assert.AreEqual(user.Website, model.Website);
      Assert.AreEqual(user.Location, model.Location);
      Assert.AreEqual(user.Bio, model.Bio);
    }

    [TestMethod]
    public void AccountSettingsWithInvalidModel()
    {
      var model = new AccountSettingsModel { Bio = new string('A', 161) };

      var controller = new AccountController();
      controller.ValidateModel(model);
      var result = controller.AccountSettings(model, null) as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.IsFalse(result.ViewData.ModelState.IsValid);
    }

    [TestMethod]
    public void AccountSettingsWithException()
    {
      Security.Setup(mock => mock.CurrentUserId).Throws<NullReferenceException>();
      
      var result = Controller.Object.AccountSettings(new AccountSettingsModel(), null) as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.IsFalse(result.ViewData.ModelState.IsValid);
    }

    [TestMethod]
    public void AccountSettingsSuccessful()
    {
      var model = new AccountSettingsModel
      {
        Name = "john doe",
        Location = "country name",
        Website = "http://www.google.com",
        Bio = "some text"
      };

      var image = new Mock<HttpPostedFileBase>();
      var imageData = Encoding.UTF8.GetBytes("picture content");
      image.Setup(mock => mock.ContentLength).Returns(imageData.Length);
      image.Setup(mock => mock.InputStream).Returns(new MemoryStream(imageData));

      Security.Setup(mock => mock.CurrentUserId).Returns(1);
      Repository.Setup(mock => mock.UpdateAccountSettings(
        It.IsAny<int>(), // id
        It.IsAny<string>(), // name
        It.IsAny<string>(), // website
        It.IsAny<string>(), // location
        It.IsAny<string>(), // bio
        It.IsAny<byte[]>()) // image
        ).Returns(true);

      var controller = Controller.Object;
      var result = controller.AccountSettings(model, image.Object) as RedirectToRouteResult;
      Assert.IsNotNull(result);
      Assert.IsNotNull(controller.TempData["Notification"]);

      var routeValues = result.RouteValues;
      Assert.AreEqual("Index", routeValues["action"]);
      Assert.AreEqual("Account", routeValues["controller"]);
    }
    
    [TestMethod]
    public void Login()
    {
      var controller = new AccountController();
      var result = controller.Login() as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
    }
    
    [TestMethod]
    public void LoginRedirectsToHome()
    {
      Security.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(true);
      Controller.Setup(x => x.CanRedirect(It.IsAny<string>())).Returns(false);
      
      var model = new LogOnModel { Account = "account", Password = "password" };
      var result = Controller.Object.Login(model, null) as RedirectToRouteResult;

      Assert.IsNotNull(result);
      var routeValues = result.RouteValues;
      Assert.AreEqual("Index", routeValues["action"]);
      Assert.AreEqual("Home", routeValues["controller"]);

      Controller.Verify(c => c.CanRedirect(null), Times.Once());
      Security.Verify(service => service.Login("account", "password", false), Times.Once());
    }

    [TestMethod]
    public void LoginRedirectsToUrl()
    {
      Security.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(true);
      Controller.Setup(x => x.CanRedirect(It.IsAny<string>())).Returns(true);
      
      var result = Controller.Object.Login(new LogOnModel(), "/Home/About") as RedirectResult;

      Assert.IsNotNull(result);
      Assert.AreEqual("/Home/About", result.Url);

      Security.Verify(x => x.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once());
      Controller.Verify(x => x.CanRedirect("/Home/About"), Times.Once());
    }

    [TestMethod]
    public void LoginRedisplaysForm()
    {
      // login will fail
      Security.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(false);

      var controller = Controller.Object;
      // test invalid model state
      controller.ModelState.AddModelError("", "Error");
      var result = controller.Login(new LogOnModel(), null) as ViewResult;
      Assert.IsNotNull(result);

      // test login failure
      controller.ModelState.Clear();
      result = controller.Login(new LogOnModel(), null) as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(1, controller.ModelState.Count);

      Security.Verify(x => x.Login(null, null, false), Times.Once());
    }

    [TestMethod]
    public void LoginFailsOnInvalidModel()
    {
      var controller = new AccountController();
      var model = new LogOnModel { Account = null, Password = "password" };
      controller.ValidateModel(model);

      var result = controller.Login(model, null) as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.IsFalse(result.ViewData.ModelState.IsValid);
    }

    [TestMethod]
    public void CannotRedirect()
    {
      var request = new Mock<HttpRequestBase>();
      request.Setup(x => x.Url).Returns(new Uri("http://localhost:123"));

      var httpContext = new Mock<HttpContextBase>();
      httpContext.Setup(x => x.Request).Returns(request.Object);
      
      var requestContext = new RequestContext(httpContext.Object, new RouteData());
      var controller = new AccountController { Url = new UrlHelper(requestContext) };

      Assert.IsFalse(controller.CanRedirect("http://www.google.com"));
      Assert.IsFalse(controller.CanRedirect(null));
      Assert.IsFalse(controller.CanRedirect(""));
      Assert.IsFalse(controller.CanRedirect("//test"));
      Assert.IsFalse(controller.CanRedirect("/\\test"));
    }

    [TestMethod]
    public void CanRedirect()
    {
      var request = new Mock<HttpRequestBase>();
      request.Setup(x => x.Url).Returns(new Uri("http://localhost:123"));

      var httpContext = new Mock<HttpContextBase>();
      httpContext.Setup(x => x.Request).Returns(request.Object);

      var requestContext = new RequestContext(httpContext.Object, new RouteData());
      var controller = new AccountController { Url = new UrlHelper(requestContext) };

      Assert.IsTrue(controller.CanRedirect("/Home/Index"));
    }

    [TestMethod]
    public void Logout()
    {
      Security.Setup(x => x.Logout());
      
      var result = Controller.Object.Logout() as RedirectToRouteResult;
      Assert.IsNotNull(result);

      var routeValues = result.RouteValues;
      Assert.AreEqual("Index", routeValues["action"]);
      Assert.AreEqual("Home", routeValues["controller"]);

      Security.Verify(service => service.Logout(), Times.Once());
    }

    [TestMethod]
    public void Register()
    {
      var controller = new AccountController();
      var result = controller.Register() as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
    }

    [TestMethod]
    public void RegisterWithInvalidCaptcha()
    {
      var controller = new AccountController();
      var result = controller.Register(null, false) as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.IsFalse(result.ViewData.ModelState.IsValid);
    }

    [TestMethod]
    public void RegisterWithInvalidModel()
    {
      var controller = new AccountController();
      var model = new RegisterModel();
      controller.ValidateModel(model);
      var result = controller.Register(model, true) as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.IsFalse(result.ViewData.ModelState.IsValid);
    }

    [TestMethod]
    public void RegisterWithException()
    {
      Settings.Setup(x => x.RequireAccountConfirmation).Throws<NullReferenceException>();
      
      var result = Controller.Object.Register(new RegisterModel(), true) as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.IsFalse(result.ViewData.ModelState.IsValid);

      Settings.Verify(x => x.RequireAccountConfirmation, Times.Once());
    }

    [TestMethod]
    public void RegisterWithMembershipException()
    {
      Settings.Setup(x => x.RequireAccountConfirmation).Throws(new MembershipCreateUserException(MembershipCreateStatus.ProviderError));
      
      var result = Controller.Object.Register(new RegisterModel(), true) as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.IsFalse(result.ViewData.ModelState.IsValid);

      Settings.Verify(mock => mock.RequireAccountConfirmation, Times.Once());
    }

    [TestMethod]
    public void RegistersWithoutConfirmation()
    {
      Settings.Setup(x => x.RequireAccountConfirmation).Returns(false);
      Security.Setup(x => x.CreateUserAndAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));
      Security.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(true);
      
      var model = new RegisterModel
      {
        Account = "account1",
        Name = "user1",
        Email = "user@mail.com",
        Password = "password",
        ConfirmPassword = "password"
      };

      var result = Controller.Object.Register(model, true) as RedirectToRouteResult;

      Assert.IsNotNull(result);
      Assert.AreEqual("Index", result.RouteValues["action"]);
      Assert.AreEqual("Home", result.RouteValues["controller"]);

      Settings.VerifyGet(x => x.RequireAccountConfirmation, Times.Once());
      Security.Verify(x => x.CreateUserAndAccount(model.Account, model.Password, model.Email, model.Name, false), Times.Once());
      Security.Verify(x => x.Login(model.Account, model.Password, false), Times.Once());
    }

    [TestMethod]
    public void RegistersWithConfirmation()
    {
      Settings.Setup(x => x.RequireAccountConfirmation).Returns(true);
      Security.Setup(x => x.CreateUserAndAccount(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));
      
      var model = new RegisterModel
      {
        Account = "account1",
        Name = "user1",
        Email = "user@mail.com",
        Password = "password",
        ConfirmPassword = "password"
      };

      var result = Controller.Object.Register(model, true) as RedirectToRouteResult;

      Assert.IsNotNull(result);
      Assert.AreEqual("Thanks", result.RouteValues["action"]);
      Assert.AreEqual("Account", result.RouteValues["controller"]);

      Settings.VerifyGet(x => x.RequireAccountConfirmation, Times.Once());
      Security.Verify(x => x.CreateUserAndAccount(model.Account, model.Password, model.Email, model.Name, true), Times.Once());
      Security.Verify(x => x.Login(model.Account, model.Password, false), Times.Never());
    }

    [TestMethod]
    public void Thanks()
    {
      var controller = new AccountController();
      var result = controller.Thanks() as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
    }

    [TestMethod]
    public void ConfirmFails()
    {
      Security.Setup(x => x.ConfirmAccount(It.IsAny<string>())).Returns(false);

      var controller = Controller.Object;

      // test empty token
      var result = controller.Confirm(null) as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.AreEqual(Strings.ErrorAccountConfirm, result.ViewBag.Message);

      // test confirmation rejection
      result = controller.Confirm("token") as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.AreEqual(Strings.ErrorAccountConfirm, result.ViewBag.Message);

      Security.Verify(x => x.ConfirmAccount("token"), Times.Once());
    }

    [TestMethod]
    public void ConfirmSuccessful()
    {
      Security.Setup(x => x.ConfirmAccount(It.IsAny<string>())).Returns(true);
      var result = Controller.Object.Confirm("token") as ViewResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.AreEqual(Strings.MsgAccountConfirm, result.ViewBag.Message);
      Security.Verify(mock => mock.ConfirmAccount("token"), Times.Once());
    }

    [TestMethod]
    public void ChangePassword()
    {
      var controller = new AccountController();
      var result = controller.ChangePassword() as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
    }

    [TestMethod]
    public void ChangePasswordWithInvalidModel()
    {
      var model = new ChangePasswordModel();
      var controller = new AccountController();
      controller.ValidateModel(model);

      var result = controller.ChangePassword(model) as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.IsFalse(result.ViewData.ModelState.IsValid);
      Assert.IsNull(result.Model);
    }

    [TestMethod]
    public void ChangePasswordWithException()
    {
      Security.Setup(x => x.CurrentUserName).Returns("johndoe");
      Security
        .Setup(x => x.ChangePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .Throws<NullReferenceException>();

      var model = new ChangePasswordModel { OldPassword = "oldpwd", NewPassword = "newpwd", ConfirmPassword = "newpwd" };
      
      var result = Controller.Object.ChangePassword(model) as ViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual(string.Empty, result.ViewName);
      Assert.IsFalse(result.ViewData.ModelState.IsValid);
      Assert.IsNull(result.Model);

      Security.Verify(mock => mock.ChangePassword("johndoe", "oldpwd", "newpwd"), Times.Once());
    }

    [TestMethod]
    public void ChangesPassword()
    {
      Security.Setup(x => x.CurrentUserName).Returns("johndoe");
      Security
        .Setup(x => x.ChangePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns(true);

      var model = new ChangePasswordModel { OldPassword = "oldpwd", NewPassword = "newpwd", ConfirmPassword = "newpwd" };
      var result = Controller.Object.ChangePassword(model) as RedirectToRouteResult;

      Assert.IsNotNull(result);
      Assert.AreEqual("AccountSettings", result.RouteValues["action"]);
      Assert.AreEqual("Account", result.RouteValues["controller"]);
      
      Security.Verify(mock => mock.ChangePassword("johndoe", "oldpwd", "newpwd"), Times.Once());
    }
    
    [TestMethod]
    public void PublicProfile()
    {
      var model = new UserProfileModel { Account = "johndoe" };

      Security.Setup(x => x.CurrentUserName).Returns("johndoe");
      Repository.Setup(x => x.GetUserProfile(It.IsAny<string>(), It.IsAny<string>())).Returns(model);
      
      var result = Controller.Object.PublicProfile("user") as PartialViewResult;
      Assert.IsNotNull(result);
      Assert.AreEqual("_UserProfilePartial", result.ViewName);
      Assert.AreEqual(model, result.Model);

      Security.VerifyGet(mock => mock.CurrentUserName);
      Repository.Verify(mock => mock.GetUserProfile("johndoe", "user"));
    }

    [TestMethod]
    public void Picture()
    {
      var model = new UserPictureModel { ContentType = "image/png", Data = new byte[0] };
      Repository.Setup(x => x.GetUserPictureNormal(It.IsAny<string>())).Returns(model);
      
      var result = Controller.Object.Picture("user") as FileContentResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(model.ContentType, result.ContentType);
      Assert.AreEqual(model.Data, result.FileContents);

      Repository.Verify(mock => mock.GetUserPictureNormal("user"), Times.Once());
    }

    [TestMethod]
    public void PictureSmall()
    {
      var model = new UserPictureModel { ContentType = "image/png", Data = new byte[0] };
      Repository.Setup(x => x.GetUserPictureSmall(It.IsAny<string>())).Returns(model);
      
      var result = Controller.Object.PictureSmall("user") as FileContentResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(model.ContentType, result.ContentType);
      Assert.AreEqual(model.Data, result.FileContents);

      Repository.Verify(mock => mock.GetUserPictureSmall("user"), Times.Once());
    }

    [TestMethod]
    public void PictureTiny()
    {
      var model = new UserPictureModel { ContentType = "image/png", Data = new byte[0] };
      Repository.Setup(x => x.GetUserPictureTiny(It.IsAny<string>())).Returns(model);
      
      var result = Controller.Object.PictureTiny("user") as FileContentResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(model.ContentType, result.ContentType);
      Assert.AreEqual(model.Data, result.FileContents);

      Repository.Verify(mock => mock.GetUserPictureTiny("user"), Times.Once());
    }
  }
}
