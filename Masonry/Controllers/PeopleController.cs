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

using Masonry.Composition.Filters;
using Masonry.Models;
using System.Web.Mvc;

namespace Masonry.Controllers
{
  public class PeopleController : MasonryController
  {
    //
    // GET: /People/
    [SidebarElement]
    public ActionResult Index(int? page)
    {
      var profiles = Repository.GetUserProfiles(Security.CurrentUserId, page ?? 0);
      var model = new PeopleModel { Profiles = profiles };

      if (IsAjaxRequest())
        return Json(model, JsonRequestBehavior.AllowGet);

      return View(model);
    }

    [SidebarElement("people", "index")]
    public ActionResult Following(string uid, int? page)
    {
      var user = Repository.FindUser(uid);
      if (user == null)
      {
        // TODO: redirect to error page
        return null;
      }

      var model = new PeopleModel
      {
        Account = user.Account,
        Name = user.Name,
        Profiles = Repository.GetFollowings(user.Id, page ?? 0)
      };

      if (IsAjaxRequest())
        return Json(model, JsonRequestBehavior.AllowGet);

      return View(model);
    }

    [SidebarElement("people", "index")]
    public ActionResult Followers(string uid, int? page)
    {
      var user = Repository.FindUser(uid);
      if (user == null)
      {
        // TODO: redirect to error page
        return null;
      }

      var model = new PeopleModel
      {
        Account = user.Account,
        Name = user.Name,
        Profiles = Repository.GetFollowers(user.Id, page ?? 0)
      };

      if (IsAjaxRequest())
        return Json(model, JsonRequestBehavior.AllowGet);

      return View(model);
    }

    // TODO: needs direct SQL call instead (performance optimization)
    public ActionResult Follow(string uid)
    {
      // TODO: redirect to error page in case of invalid uid
      Repository.Follow(Security.CurrentUserId, uid);
      return RedirectToAction("Index", "People");
    }

    // TODO: needs direct SQL call instead (performance optimization)
    public ActionResult Unfollow(string uid)
    {
      // TODO: redirect to error page in case of invalid uid
      Repository.Unfollow(Security.CurrentUserId, uid);
      return RedirectToAction("Index", "People");
    }
  }
}
