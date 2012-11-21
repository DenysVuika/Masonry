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
using System.Reflection;

namespace Masonry.Core.Hosting
{
  /// <summary>
  ///   Maps a namespace to a virtual path
  /// </summary>
  public class NamespaceMapping
  {
    private readonly Assembly _assembly;
    private readonly string _folderNamespace;

    /// <summary>
    ///   Initializes a new instance of the <see cref="NamespaceMapping" /> class.
    /// </summary>
    /// <param name="assembly"> The assembly that the views are located in. </param>
    /// <param name="folderNamespace"> Namespace that should correspond to the virtual path "~/". Typically root namespace in your project.</param>
    public NamespaceMapping(Assembly assembly, string folderNamespace)
    {
      if (assembly == null) throw new ArgumentNullException("assembly");
      if (folderNamespace == null) throw new ArgumentNullException("folderNamespace");

      _assembly = assembly;
      _folderNamespace = folderNamespace;
    }

    /// <summary>
    ///   Gets assembly that the embedded views are located in
    /// </summary>
    public Assembly Assembly
    {
      get { return _assembly; }
    }

    /// <summary>
    ///   Gets namespace that corresponds to application root
    /// </summary>
    public string FolderNamespace
    {
      get { return _folderNamespace; }
    }
  }
}
