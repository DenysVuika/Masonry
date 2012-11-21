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
using System.IO;
using System.Text;

namespace Masonry.Core.Hosting
{
  /// <summary>
  ///   Adds default usings, sets an inherits clause and specifies the layout name
  /// </summary>
  /// <remarks>
  /// Modifies embedded views so that they works like any other views. This includes the following
  /// 
  /// <list type="bullet">
  /// <item>Include a <c>@model</c> directive if missing</item>
  /// <item>Add a <c>@inherits</c> directive</item>
  /// <item>Add any missing @using statements (MVC and ASP.NET dependencies)</item>
  /// </list>
  /// </remarks>
  public class EmbeddedViewGenerator : IViewGenerator
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="EmbeddedViewGenerator" /> class.
    /// </summary>
    public EmbeddedViewGenerator()
    {
      WebViewPageClassName = "System.Web.Mvc.WebViewPage";
      LayoutPath = null;
    }

    /// <summary>
    ///   Base view class to inherit.
    /// </summary>
    /// <value> Default is System.Web.Mvc.WebViewPage </value>
    public string WebViewPageClassName { get; set; }

    /// <summary>
    ///   Gets or sets relative path to the layout file to use
    /// </summary>
    /// <example>
    ///   <code>fixer.LayoutPath = "~/Views/Shared/_Layout.cshtml";</code>
    /// </example>
    /// <value>Default is the one specified in _ViewStart</value>
    public string LayoutPath { get; set; }

    #region IEmbeddedViewFixer Members

    /// <summary>
    ///   Modify the view
    /// </summary>
    /// <param name="virtualPath"> Path to view </param>
    /// <param name="stream"> Stream containing the original view </param>
    /// <returns> Stream with modified contents </returns>
    public Stream GenerateView(string virtualPath, Stream stream)
    {
      var reader = new StreamReader(stream, Encoding.UTF8);
      var view = reader.ReadToEnd();
      stream.Close();
      var ourStream = new MemoryStream();
      var writer = new StreamWriter(ourStream, Encoding.UTF8);

      var modelString = "";
      var modelPos = view.IndexOf("@model", System.StringComparison.Ordinal);
      if (modelPos != -1)
      {
        writer.Write(view.Substring(0, modelPos));
        var modelEndPos = view.IndexOfAny(new[] { '\r', '\n' }, modelPos);
        modelString = view.Substring(modelPos, modelEndPos - modelPos);
        view = view.Remove(0, modelEndPos);
      }

      writer.WriteLine("@using System.Web.Mvc");
      writer.WriteLine("@using System.Web.Mvc.Ajax");
      writer.WriteLine("@using System.Web.Mvc.Html");
      writer.WriteLine("@using System.Web.Routing");

      var basePrefix = "@inherits " + WebViewPageClassName;

      if (virtualPath.ToLower().Contains("_viewstart"))
        writer.WriteLine("@inherits System.Web.WebPages.StartPage");
      else if (modelString == "@model object")
        writer.WriteLine(basePrefix + "<dynamic>");
      else if (!string.IsNullOrEmpty(modelString))
        writer.WriteLine(basePrefix + "<" + modelString.Substring(7) + ">");
      else
        writer.WriteLine(basePrefix);

      // partial views should not have a layout
      if (!string.IsNullOrEmpty(LayoutPath) && !virtualPath.Contains("/_"))
      {
        writer.WriteLine("@{{ Layout = \"{0}\"; }}", LayoutPath);
      }
      writer.Write(view);
      writer.Flush();
      ourStream.Position = 0;
      return ourStream;
    }

    #endregion
  }
}
