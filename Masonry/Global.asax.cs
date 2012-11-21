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

using System.Web;
using Masonry.Controllers;
using Masonry.Core.Composition;
using System.Web.Mvc;
using System.Web.Routing;
using WebMatrix.WebData;
using Masonry.Composition.Verbs;

namespace Masonry
{
  // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
  // visit http://go.microsoft.com/?LinkId=9394801

  public class MvcApplication : HttpApplication
  {
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
      filters.Add(new HandleErrorAttribute());
    }

    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      // http://www.paraesthesia.com/archive/2011/07/21/running-static-files-through-virtualpathprovider-in-iis7.aspx
      routes.IgnoreRoute("{*staticfile}", new { staticfile = @".*\.(css|js|gif|jpg|png)(/.*)?" });
      routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });

      // Special route for Help navigation
      routes.MapRoute(
        "Help",
        "Help/{page}",
        new { controller = "Help", action = "Page" },
        new[] { typeof(HelpController).Namespace }
        );
      
      routes.MapRoute(
        "Default", // Route name
        "{controller}/{action}/{id}", // URL with parameters
        new { controller = "Home", action = "Index", id = UrlParameter.Optional }, // Parameter defaults
        new[] { typeof(HomeController).Namespace } // default Home controller to avoid naming conflicts
        );
    }

    protected void Application_Start()
    {
      // Initialize membership and role providers
      WebSecurity.InitializeDatabaseConnection("DefaultConnection", "Users", "Id", "Account", false);

      AreaRegistration.RegisterAllAreas();

      RegisterGlobalFilters(GlobalFilters.Filters);
      RegisterRoutes(RouteTable.Routes);

      CompositionProvider.AddAssembly(GetType().Assembly);
      CompositionProvider.AddAssembly(typeof(IActionVerb).Assembly);
    }

    // Alternatively can be configured within web.config: http://msdn.microsoft.com/en-us/library/bb763179.aspx
    //void Application_BeginRequest()
    //{
    //  // Basic click jacking protection
    //  HttpContext.Current.Response.AddHeader("X-Frame-Options", "DENY");
    //}
  }
}