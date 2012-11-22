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
using System.Web.Mvc;
using Masonry.Core.Web;
using Masonry.Models;

namespace Masonry.Controllers
{
  [Authorize]
  public class TimelineController : MasonryController
  {
    [OutputCache(Duration = 0), SidebarElement]
    public ActionResult Index(int? top)
    {
      var user = Repository.FindUser(Security.CurrentUserId);
      var model = new TimelineModel
      {
        Account = user.Account,
        Name = user.Name,
        Posts = Repository.GetTimelineEntries(user.Id, top ?? 0)
      };

      if (IsAjaxRequest())
        return Json(model.Posts, JsonRequestBehavior.AllowGet);
      
      return View(model);
    }

    public JsonResult CheckUpdates(int? top)
    {
      var count = Repository.CheckTimelineUpdates(Security.CurrentUserId, top ?? 0);
      return Json(count, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Updates(int? top)
    {
      var posts = Repository.GetTimelineUpdates(Security.CurrentUserId, top ?? 0);
      return Json(posts, JsonRequestBehavior.AllowGet);
    }

    [SidebarElement]
    public ActionResult Mentions(int? top)
    {
      var user = Repository.FindUser(Security.CurrentUserId);
      var model = new TimelineModel
      {
        Account = user.Account,
        Name = user.Name,
        Posts = Repository.GetMentions(user.Account, top ?? 0)
      };

      if (IsAjaxRequest())
        return Json(model.Posts, JsonRequestBehavior.AllowGet);
      
      return View(model);
    }

    [OutputCache(Duration = 0), SidebarElement]
    public ActionResult Feed(string uid, int? top)
    {
      var user = Repository.FindUser(uid);
      var model = user == null ? null : new TimelineModel
      {
        Account = user.Account,
        Name = user.Name,
        Posts = Repository.GetFeedEntries(user.Id, top ?? 0)
      };
      
      if (IsAjaxRequest())
        return Json(model != null ? model.Posts : new PostModel[0], JsonRequestBehavior.AllowGet);

      ViewBag.NavigationFeed = uid;
      return View(model);
    }

    [OutputCache(Duration = 0), GZip]
    public JsonResult Comments(int? id)
    {
      var postId = id ?? 0;
      if (postId == 0) return Json(null, JsonRequestBehavior.AllowGet);
      var comments = Repository.GetComments(postId);
      return Json(comments, JsonRequestBehavior.AllowGet);
    }

    // TODO: add safe checks
    [SidebarElement("timeline", "index")]
    public ActionResult Post(int pid)
    {
      var post = Repository.GetPost(pid);
      return View(post);
    }

    [HttpPost]
    public JsonResult Comment(CommentModel model)
    {
      var comment = Repository.CreateComment(Security.CurrentUserId, model.PostId, model.Content);
      return Json(comment);
    }

    public ActionResult DeletePost(int id)
    {
      var result = Repository.DeletePost(Security.CurrentUserId, id);

      if (IsAjaxRequest())
        return Json(result, JsonRequestBehavior.AllowGet);

      return RedirectToAction("Index", "Timeline");
    }

    [HttpPost]
    public JsonResult UpdateStatus(PostModel model)
    {
      if (model != null && !string.IsNullOrWhiteSpace(model.Content))
      {
        var post = Repository.CreatePost(Security.CurrentUserId, model.Content);
        return Json(post);
      }

      return Json(null);
    }

    // TODO: needs direct SQL call instead (performance optimization)
    // TODO: temp to solve redirects issue
    public ActionResult Follow(string uid)
    {
      Repository.Follow(Security.CurrentUserId, uid);
      return RedirectToAction("Index", "Timeline");
    }

    // TODO: needs direct SQL call instead (performance optimization)
    // TODO: temp to solve redirects issue
    public ActionResult Unfollow(string uid)
    {
      Repository.Unfollow(Security.CurrentUserId, uid);
      return RedirectToAction("Index", "Timeline");
    }
  }
}
