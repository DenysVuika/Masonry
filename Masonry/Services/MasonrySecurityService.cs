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

using System;
using System.Composition;
using System.Web;
using System.Web.Helpers;
using WebMatrix.WebData;

namespace Masonry.Services
{
  [Export(typeof(IMasonrySecurityService))]
  public class MasonrySecurityService : IMasonrySecurityService
  {
    public bool IsAuthenticated
    {
      get { return WebSecurity.IsAuthenticated; }
    }

    public int CurrentUserId
    {
      get { return WebSecurity.CurrentUserId; }
    }

    public string CurrentUserName
    {
      get { return WebSecurity.CurrentUserName; }
    }

    public bool Login(string userName, string password, bool persistCookie = false)
    {
      return WebSecurity.Login(userName, password, persistCookie);
    }

    public void Logout()
    {
      WebSecurity.Logout();
    }

    public bool ChangePassword(string userName, string currentPassword, string newPassword)
    {
      return WebSecurity.ChangePassword(userName, currentPassword, newPassword);
    }

    public bool ConfirmAccount(string accountConfirmationToken)
    {
      return WebSecurity.ConfirmAccount(accountConfirmationToken);
    }

    public void CreateUserAndAccount(string account, string password, string email, string name, bool requireConfirmation = false)
    {
      var token = WebSecurity.CreateUserAndAccount(
        account, 
        password, 
        new
        {
          Email = email,
          Name = name
        }, 
        requireConfirmation);

      if (requireConfirmation)
        SendRegistrationConfirmationEmail(email, token);
    }

    // TODO: move to a separate notification service
    private static void SendRegistrationConfirmationEmail(string email, string confirmationToken)
    {
      var siteUrl = HttpContext.Current.Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
      var confirmationUrl = siteUrl + VirtualPathUtility.ToAbsolute("~/Account/Confirm?token=" + HttpUtility.UrlEncode(confirmationToken));
      var confirmationLink = String.Format("<a href=\"{0}\">Click to confirm your registration</a>", confirmationUrl);
      var emailBodyText =
        "<p>Thank you for signing up with us! "
        + "Please confirm your registration by clicking the following link:</p>"
        + "<p>" + confirmationLink + "</p>"
        + "<p>In case you need it, here's the confirmation code:<strong> "
        + confirmationToken
        + "</strong></p>";

      WebMail.Send(email, "Your account confirmation", emailBodyText);
    }
  }
}