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

using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace Masonry.Core.Web
{
  // http://stackoverflow.com/questions/6642815/create-etag-filter-in-asp-net-mvc
  class ETagFilter : MemoryStream
  {
    private readonly HttpResponseBase _response;
    private readonly HttpRequestBase _request;
    private readonly Stream _filter;

    public ETagFilter(ControllerContext context)
    {
      _response = context.HttpContext.Response;
      _request = context.HttpContext.Request;
      _filter = context.HttpContext.Response.Filter;
    }

    public ETagFilter(HttpResponseBase response, HttpRequestBase request)
    {
      _response = response;
      _request = request;
      _filter = response.Filter;
    }

    private static string GetToken(Stream stream)
    {
      var checksum = MD5.Create().ComputeHash(stream);
      return Convert.ToBase64String(checksum, 0, checksum.Length);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      var data = new byte[count];
      Buffer.BlockCopy(buffer, offset, data, 0, count);
      var token = GetToken(new MemoryStream(data));

      var clientToken = _request.Headers["If-None-Match"];

      if (token != clientToken)
      {
        _response.Headers["ETag"] = token;
        _filter.Write(data, 0, count);
      }
      else
      {
        _response.SuppressContent = true;
        _response.StatusCode = 304;
        _response.StatusDescription = "Not Modified";
        _response.Headers["Content-Length"] = "0";
      }
    }
  }
}
