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
namespace Masonry.Core.Hosting
{
  /// <summary>
  /// Used to locate files on disk for the <see cref="IVirtualFileProvider"/>.
  /// </summary>
  /// <example>
  /// <code>
  /// public class DiskFileLocator : IViewFileProvider
  /// {
  ///     string _startUri;
  ///     string _diskRoot;
  /// 
  ///     public DiskFileLocator(string startUri, string diskRoot)
  ///     {
  ///         _diskRoot = diskRoot;
  ///         _startUri =  startUri;
  ///     }
  /// 
  ///     public string GetFullPath(string uri)
  ///     {
  ///          if (!uri.ToLower().StartWith(_startUri))
  ///              return null;
  /// 
  ///          var path = uri.Remove(0, _startUri.Length).Replace('/', '\\');
  ///          path = Path.Combine(_diskRoot, path);
  ///          if (File.Exists(path))
  ///              return path;
  ///     }
  /// }
  /// </code>
  /// </example>
  public interface IViewFileLocator
  {
    /// <summary>
    /// Get full path to a file
    /// </summary>
    /// <param name="uri">Requested uri</param>
    /// <returns>Full disk path if found; otherwise null.</returns>
    string GetFullPath(string uri);
  }
}
