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
using System.Collections;
using System.Web.Caching;
using System.Web.Hosting;

namespace Masonry.Core.Hosting
{
  public interface IVirtualFileProvider
  {
    /// <summary>
    /// Checks if a file exits
    /// </summary>
    /// <param name="virtualPath">Virtual path like "~/Views/Home/Index.cshtml"</param>
    /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
    bool FileExists(string virtualPath);

    /// <summary>
    /// Creates a cache dependency based on the specified virtual paths
    /// </summary>
    /// <param name="virtualPath">Virtual path like "~/Views/Home/Index.cshtml"</param>
    /// <param name="virtualPathDependencies">An array of paths to other resources required by the primary virtual resource </param>
    /// <param name="utcStart">The UTC time at which the virtual resources were read </param>
    /// <returns>A CacheDependency if the file is found and caching should be used; <see cref="NoCache.Instance"/> if caching should be disabled for the file; <c>null</c> if file is not found.</returns>
    CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart);

    /// <summary>
    /// Returns a cache key to use for the specified virtual path
    /// </summary>
    /// <param name="virtualPath">Virtual path like "~/Views/Home/Index.cshtml"</param>
    /// <returns>CacheDependency if found; otherwise <c>false</c>.</returns>
    string GetCacheKey(string virtualPath);

    /// <summary>
    /// Get file hash.
    /// </summary>
    /// <param name="virtualPath">Virtual path like "~/Views/Home/Index.cshtml"</param>
    /// <param name="virtualPathDependencies">An array of paths to other virtual resources required by the primary virtual resource </param>
    /// <returns>a new hash each time the file have changed (if file is found); otherwise null</returns>
    string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies);

    /// <summary>
    /// Get the view
    /// </summary>
    /// <param name="virtualPath">Virtual path like "~/Views/Home/Index.cshtml"</param>
    /// <returns>File</returns>
    VirtualFile GetFile(string virtualPath);
  }
}
