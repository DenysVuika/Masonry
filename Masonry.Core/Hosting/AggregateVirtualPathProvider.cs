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
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;

namespace Masonry.Core.Hosting
{
  /// <summary>
  ///   Virtual path provider used to provide resources from multiple sources.
  /// </summary>
  public class AggregateVirtualPathProvider : VirtualPathProvider
  {
    private readonly List<IVirtualFileProvider> _fileProviders = new List<IVirtualFileProvider>();

    public AggregateVirtualPathProvider()
    {
    }

    public AggregateVirtualPathProvider(params IVirtualFileProvider[] providers)
    {
      _fileProviders.AddRange(providers);
    }
    
    /// <summary>
    ///   Add a new file provider
    /// </summary>
    /// <param name="fileProvider"> </param>
    public void Add(IVirtualFileProvider fileProvider)
    {
      if (fileProvider == null) throw new ArgumentNullException("fileProvider");
      _fileProviders.Add(fileProvider);
    }

    /// <summary>
    ///   Gets a value that indicates whether a file exists in the virtual file system.
    /// </summary>
    /// <param name="virtualPath"> The path to the virtual file. </param>
    /// <returns> true if the file exists in the virtual file system; otherwise, false. </returns>
    public override bool FileExists(string virtualPath)
    {
      return _fileProviders.Any(provider => provider.FileExists(virtualPath)) || base.FileExists(virtualPath);
    }


    /// <summary>
    ///   Creates a cache dependency based on the specified virtual paths.
    /// </summary>
    /// <param name="virtualPath"> The path to the primary virtual resource. </param>
    /// <param name="virtualPathDependencies"> An array of paths to other resources required by the primary virtual resource. </param>
    /// <param name="utcStart"> The UTC time at which the virtual resources were read. </param>
    /// <returns> A <see cref="T:System.Web.Caching.CacheDependency" /> object for the specified virtual resources. </returns>
    public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
    {
      foreach (var provider in _fileProviders)
      {
        var result = provider.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        if (result is NoCache)
          return null;
        if (result != null)
          return result;
      }

      return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
    }

    /// <summary>
    ///   Returns a cache key to use for the specified virtual path.
    /// </summary>
    /// <param name="virtualPath"> The path to the virtual resource. </param>
    /// <returns> A cache key for the specified virtual resource. </returns>
    public override string GetCacheKey(string virtualPath)
    {
      foreach (
          var result in
              _fileProviders.Select(provider => provider.GetCacheKey(virtualPath)).Where(result => result != null)
          )
      {
        return result;
      }

      return base.GetCacheKey(virtualPath);
    }

    /// <summary>
    ///   Gets a virtual file from the virtual file system.
    /// </summary>
    /// <param name="virtualPath"> The path to the virtual file. </param>
    /// <returns> A descendent of the <see cref="T:System.Web.Hosting.VirtualFile" /> class that represents a file in the virtual file system. </returns>
    public override VirtualFile GetFile(string virtualPath)
    {
      foreach (
          var result in
              _fileProviders.Select(provider => provider.GetFile(virtualPath)).Where(result => result != null))
      {
        return result;
      }

      return base.GetFile(virtualPath);
    }

    /// <summary>
    ///   Returns a hash of the specified virtual paths.
    /// </summary>
    /// <param name="virtualPath"> The path to the primary virtual resource. </param>
    /// <param name="virtualPathDependencies"> An array of paths to other virtual resources required by the primary virtual resource. </param>
    /// <returns> A hash of the specified virtual paths. </returns>
    public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
    {
      foreach (
          var result in
              _fileProviders.Select(provider => provider.GetFileHash(virtualPath, virtualPathDependencies)).Where(
                  result => result != null))
      {
        return result;
      }

      return base.GetFileHash(virtualPath, virtualPathDependencies);
    }
  }
}
