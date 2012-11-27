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

using System.Web.Security;
using Masonry.Data;
using Masonry.Resources;
using WebMatrix.WebData;
using System.Security.Principal;

namespace Masonry.Controllers
{
  public static class Utils
  {
    public static string GetDisplayName(this IIdentity identity)
    {
      if (!WebSecurity.IsAuthenticated) return null;

      using (var context = new MasonryDataContext())
      {
        var user = context.Users.Find(WebSecurity.CurrentUserId);
        return user.Name;
      }
    }

    public static string ErrorCodeToString(MembershipCreateStatus createStatus)
    {
      // See http://go.microsoft.com/fwlink/?LinkID=177550 for
      // a full list of status codes.
      switch (createStatus)
      {
        case MembershipCreateStatus.DuplicateUserName:
          return Strings.ErrorMembershipDuplicateUserName;

        case MembershipCreateStatus.DuplicateEmail:
          return Strings.ErrorMembershipDuplicateEmail;

        case MembershipCreateStatus.InvalidPassword:
          return Strings.ErrorMembershipInvalidPassword;

        case MembershipCreateStatus.InvalidEmail:
          return Strings.ErrorMembershipInvalidEmail;

        case MembershipCreateStatus.InvalidAnswer:
          return Strings.ErrorMembershipInvalidAnswer;

        case MembershipCreateStatus.InvalidQuestion:
          return Strings.ErrorMembershipInvalidQuestion;

        case MembershipCreateStatus.InvalidUserName:
          return Strings.ErrorMembershipInvalidUserName;

        case MembershipCreateStatus.ProviderError:
          return Strings.ErrorMembershipProviderError;

        case MembershipCreateStatus.UserRejected:
          return Strings.ErrorMembershipUserRejected;

        default:
          return Strings.ErrorMembership;
      }
    }
  }
}