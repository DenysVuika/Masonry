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

namespace Masonry.Core.Hosting
{
  /// <summary>
  /// Used to correct external view files
  /// </summary>
  /// <remarks>The purpose of the class is to allow the external views to look exactly as regular views without @inherits or anything like that.</remarks>
  public interface IViewGenerator
  {
    /// <summary>
    /// Modify the view
    /// </summary>
    /// <param name="virtualPath">Path to view</param>
    /// <param name="stream">Stream containing the original view</param>
    /// <returns>Stream with modified contents</returns>
    Stream GenerateView(string virtualPath, Stream stream);
  }
}
