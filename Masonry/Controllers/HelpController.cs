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

using MarkdownSharp;
using System.IO;
using System.Web.Mvc;
using Masonry.Composition.Filters;

namespace Masonry.Controllers
{
  public class HelpController : MasonryController
  {
    //
    // GET: /Help/
    [SidebarElement, NavbarElement]
    public ActionResult Index()
    {
      return View(model: GetMarkdownContent("Index"));
    }

    // Any drill-down navigation will preserve Index selection state
    [SidebarElement("help", "index"), NavbarElement("nav_help_index")]
    public ActionResult Page(string page)
    {
      ViewBag.HelpPage = page;
      var model = GetMarkdownContent(page);
      return View("Index", model: model ?? GetMarkdownContent("Index"));
    }
    
    [NonAction]
    public virtual string GetMarkdownContent(string pageName)
    {
      if (string.IsNullOrWhiteSpace(pageName)) return null;

      var path = string.Concat(HttpContext.Request.PhysicalApplicationPath, "\\Content\\Help\\", pageName, ".md");
      if (!System.IO.File.Exists(path)) return null;

      string pageContent;

      using (var reader = new StreamReader(path))
      {
        var md = new Markdown();
        pageContent = md.Transform(reader.ReadToEnd());
      }

      return pageContent;
    }

  }
}
