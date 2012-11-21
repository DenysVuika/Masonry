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

using System.Web.Mvc;
using System.Web.Routing;

namespace Masonry.Core.Web
{
  public class MasonryHtmlHelper
  {
    public ViewContext ViewContext { get; private set; }
    public IViewDataContainer ViewDataContainer { get; private set; }
    public RouteCollection RouteCollection { get; private set; }

    public ViewDataDictionary ViewData
    {
      get { return ViewDataContainer.ViewData; }
    }

    public MasonryHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
      : this(viewContext, viewDataContainer, RouteTable.Routes)
    {
    }

    public MasonryHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
    {
      ViewContext = viewContext;
      ViewDataContainer = viewDataContainer;
      RouteCollection = routeCollection;
    }
  }

  public class MasonryHtmlHelper<TModel> : MasonryHtmlHelper //: HtmlHelper
  {
    public new ViewDataDictionary<TModel> ViewData { get; private set; }

    public MasonryHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
      : this(viewContext, viewDataContainer, RouteTable.Routes)
    {
    }

    public MasonryHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
      : base(viewContext, viewDataContainer, routeCollection)
    {
      ViewData = new ViewDataDictionary<TModel>(viewDataContainer.ViewData);
    }
  }
}
