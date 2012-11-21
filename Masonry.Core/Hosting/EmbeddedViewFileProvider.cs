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
using System.Web;
using System.Web.Mvc;

namespace Masonry.Core.Hosting
{
  /// <summary>
  ///   Locates views that are embedded resources.
  /// </summary>
  public class EmbeddedViewFileProvider : EmbeddedFileProvider
  {
    private readonly IViewGenerator _viewGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedViewFileProvider"/> class.
    /// </summary>
    /// <param name="siteRoot">Root directory of the web site</param>
    /// <example>
    /// <code>
    /// var embeddedProvider = new EmbeddedViewFileProvider(VirtualPathUtility.ToAbsolute("~/"));
    /// </code>
    /// </example>
    public EmbeddedViewFileProvider(string siteRoot)
      : base(siteRoot)
    {
      _viewGenerator = DependencyResolver.Current.GetService<IViewGenerator>();
      AllowedFileExtensions = new[] { "cshtml", "ascx", "aspx" };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedViewFileProvider"/> class.
    /// </summary>
    /// <param name="siteRoot">Root directory of the web site</param>
    /// <param name="viewGenerator">View fixer</param>
    /// <example>
    /// <code>
    /// var embeddedProvider = new EmbeddedViewFileProvider(VirtualPathUtility.ToAbsolute("~/"), new ExternalViewFixer());
    /// </code>
    /// </example>
    public EmbeddedViewFileProvider(string siteRoot, IViewGenerator viewGenerator)
      : base(siteRoot)
    {
      _viewGenerator = viewGenerator;
      AllowedFileExtensions = new[] { "cshtml", "ascx", "aspx" };
    }

    public EmbeddedViewFileProvider()
      : this(VirtualPathUtility.ToAbsolute("~/"), new EmbeddedViewGenerator())
    {
      
    }

    private Stream CorrectView(string virtualPath, Stream stream)
    {
      if (_viewGenerator == null)
        return stream;

      var outStream = _viewGenerator.GenerateView(virtualPath, stream);
      stream.Close();
      return outStream;
    }

    /// <summary>
    /// Resource to load. Will correct the returned views (so that they work as regular non-embedded views)
    /// </summary>
    /// <param name="virtualPath">Requested virtual path</param>
    /// <param name="resource">Identified resource (i.e. the one to load)</param>
    /// <returns>
    /// Stream that can be returned to the Virtual Path Provider.
    /// </returns>
    protected override Stream LoadStream(string virtualPath, MappedResource resource)
    {
      var stream = base.LoadStream(virtualPath, resource);

      // embedded views need a @inherits instruction
      if (stream != null && resource.FullResourceName.EndsWith(".cshtml"))
      {
        stream = CorrectView(virtualPath, stream);
      }

      return stream;
    }
  }
}
