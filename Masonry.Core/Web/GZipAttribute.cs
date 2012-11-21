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

using System.IO.Compression;
using System.Web.Mvc;

namespace Masonry.Core.Web
{
  public sealed class GZipAttribute : ActionFilterAttribute
  {
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      var request = filterContext.HttpContext.Request;

      var acceptEncoding = request.Headers["Accept-Encoding"];
      if (string.IsNullOrWhiteSpace(acceptEncoding)) return;
      acceptEncoding = acceptEncoding.ToLowerInvariant();

      var response = filterContext.HttpContext.Response;

      if (acceptEncoding.Contains("gzip"))
      {
        response.AppendHeader("Content-encoding", "gzip");
        response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
      }
      else if (acceptEncoding.Contains("deflate"))
      {
        response.AppendHeader("Content-encoding", "deflate");
        response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
      }
    }
  }
}
