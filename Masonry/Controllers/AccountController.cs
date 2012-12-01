/*
The MIT License (MIT)
Copyright (c) 2012 Denys Vuika

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Masonry.Extensibility;
using Masonry.Core.Web;
using Masonry.Models;
using Masonry.Resources;
using Recaptcha;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Masonry.Controllers
{
  public class AccountController : MasonryController
  {
    [Authorize]
    public ActionResult Index()
    {
      return RedirectToAction("AccountSettings", "Account"); 
    }

    [Authorize, SidebarElement]
    public ActionResult AccountSettings()
    {
      var model = new AccountSettingsModel();
      var user = Repository.FindUser(Security.CurrentUserId);

      if (user != null)
      {
        model.Account = user.Account;
        model.Name = user.Name;
        model.Website = user.Website;
        model.Location = user.Location;
        model.Bio = user.Bio;
      }

      return View(model);
    }

    [Authorize, HttpPost, SidebarElement, ValidateAntiForgeryToken]
    public ActionResult AccountSettings(AccountSettingsModel model, HttpPostedFileBase picture)
    {
      if (ModelState.IsValid)
      {
        try
        {
          byte[] image = null;

          if (picture != null && picture.ContentLength > 0)
          {
            image = new byte[picture.ContentLength];
            picture.InputStream.Read(image, 0, image.Length);
          }

          var result = Repository.UpdateAccountSettings(
            Security.CurrentUserId,
            model.Name,
            model.Website,
            model.Location,
            model.Bio,
            image);

          if (result)
            TempData["Notification"] = Strings.MsgAccountSettingsUpdated;
          
          return RedirectToAction("Index", "Account");
        }
        catch
        {
          ModelState.AddModelError("", Strings.ErrorAccountSettingsUpdate);
        }
      }

      return View(model);
    }

    //
    // GET: /Account/Login

    public ActionResult Login()
    {
      return View();
    }

    //
    // POST: /Account/Login

    [HttpPost, ValidateAntiForgeryToken]
    public ActionResult Login(LogOnModel model, string returnUrl)
    {
      if (ModelState.IsValid)
      {
        if (Security.Login(model.Account, model.Password, model.RememberMe))
        {
          if (CanRedirect(returnUrl))
            return Redirect(returnUrl);

          return RedirectToAction("Index", "Home");
        }
        
        ModelState.AddModelError("", Strings.ErrorLogin);
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

    public virtual bool CanRedirect(string returnUrl)
    {
      return !string.IsNullOrWhiteSpace(returnUrl) 
        && Url.IsLocalUrl(returnUrl) 
        && returnUrl.Length > 1 
        && returnUrl.StartsWith("/") 
        && !returnUrl.StartsWith("//") 
        && !returnUrl.StartsWith("/\\");
    }

    //
    // GET: /Account/Logout

    public ActionResult Logout()
    {
      Security.Logout();
      return RedirectToAction("Index", "Home");
    }

    //
    // GET: /Account/Register

    public ActionResult Register()
    {
      return View();
    }

    //
    // POST: /Account/Register

    [HttpPost, ValidateAntiForgeryToken]
    [RecaptchaControlMvc.CaptchaValidatorAttribute]
    public ActionResult Register(RegisterModel model, bool captchaValid)
    {
      if (!captchaValid)
      {
        ModelState.AddModelError("recaptcha", Strings.ErrorInvalidRecaptcha);
      }
      else if (ModelState.IsValid)
      {
        try
        {
          var requireConfirmation = Settings.RequireAccountConfirmation;

          Security.CreateUserAndAccount(model.Account, model.Password, model.Email, model.Name, requireConfirmation);

          if (!requireConfirmation)
          {
            Security.Login(model.Account, model.Password);
            return RedirectToAction("Index", "Home");
          }
          
          // Redirect to "thanks" page
          return RedirectToAction("Thanks", "Account");
        }
        catch (MembershipCreateUserException ex)
        {
          ModelState.AddModelError("", Utils.ErrorCodeToString(ex.StatusCode));
        }
        catch (Exception)
        {
          ModelState.AddModelError("", Strings.ErrorRegistration);
        }
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }
    
    public ActionResult Thanks()
    {
      return View();
    }

    // Account confirmation (triggered from email link)
    public ActionResult Confirm(string token)
    {
      if (!string.IsNullOrWhiteSpace(token) && Security.ConfirmAccount(token))
        ViewBag.Message = Strings.MsgAccountConfirm;
      else
        ViewBag.Message = Strings.ErrorAccountConfirm;

      return View();
    }

    //
    // GET: /Account/ChangePassword

    [Authorize, SidebarElement]
    public ActionResult ChangePassword()
    {
      return View();
    }

    //
    // POST: /Account/ChangePassword

    [Authorize, HttpPost, SidebarElement]
    public ActionResult ChangePassword(ChangePasswordModel model)
    {
      if (ModelState.IsValid)
      {
        // ChangePassword will throw an exception rather
        // than return false in certain failure scenarios.
        bool changePasswordSucceeded;
        try
        {
          changePasswordSucceeded = Security.ChangePassword(Security.CurrentUserName, model.OldPassword, model.NewPassword);
        }
        catch (Exception)
        {
          changePasswordSucceeded = false;
        }

        if (changePasswordSucceeded)
        {
          TempData["Notification"] = Strings.MsgChangePassword;
          return RedirectToAction("AccountSettings", "Account"); 
        }

        ModelState.AddModelError("", Strings.ErrorChangePassword);
      }

      // If we got this far, something failed, redisplay form
      return View();
    }

    [Authorize, OutputCache(Duration = 0)]
    public ActionResult PublicProfile(string uid)
    {
      var profile = Repository.GetUserProfile(Security.CurrentUserName, uid);
      return PartialView("_UserProfilePartial", profile);
    }

    [Authorize, HttpGet, ETag]
    public ActionResult Picture(string uid)
    {
      var picture = Repository.GetUserPictureNormal(uid);
      return File(picture.Data, picture.ContentType);
    }
    
    [Authorize, HttpGet, ETag]
    public ActionResult PictureSmall(string uid)
    {
      var picture = Repository.GetUserPictureSmall(uid);
      return File(picture.Data, picture.ContentType);
    }

    [Authorize, HttpGet, ETag]
    public ActionResult PictureTiny(string uid)
    {
      var picture = Repository.GetUserPictureTiny(uid);
      return File(picture.Data, picture.ContentType);
    }
  }
}
