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

using System.Configuration;
using System.Web.Mvc;

namespace Masonry.Core.Web
{
  // Required to be able to switch page in the Views\Web.Config
  public abstract class MasonryWebViewPage : WebViewPage
  {
  }

  // Inspired by http://haacked.com/archive/2011/02/21/changing-base-type-of-a-razor-view.aspx
  public abstract class MasonryWebViewPage<TModel> : WebViewPage<TModel>
  {
    public MasonryHtmlHelper<TModel> Masonry { get; private set; } 

    public override void Execute()
    {
      // do nothing
    }

    protected override void SetViewData(ViewDataDictionary viewData)
    {
      if (!viewData.ContainsKey("Brand"))
        viewData["Brand"] = ConfigurationManager.AppSettings["masonry.config.ui.brand"];

      if (!viewData.ContainsKey("Copyright"))
        viewData["Copyright"] = ConfigurationManager.AppSettings["masonry.config.ui.copyright"];

      base.SetViewData(viewData);
    }

    public override void InitHelpers()
    {
      base.InitHelpers();
      Masonry = new MasonryHtmlHelper<TModel>(ViewContext, this);
    }
  }
}