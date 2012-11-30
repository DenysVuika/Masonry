using Masonry.Core.Composition;
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;

namespace Masonry.Core.Extensibility
{
  /// <summary>
  /// Finds all extensions and load them into the app domain (or use previously loaded assemblies)
  /// </summary>
  /// <remarks>Loads all assembles that have names ending with ".Extension.dll"</remarks>
  class ExtensionLoader
  {
    private readonly List<Assembly> _assemblies = new List<Assembly>();
    private readonly string _path;

    /// <summary>
    ///   Initializes the <see cref="ExtensionLoader" /> class.
    /// </summary>
    /// <param name="virtualPath"> App relative path to plugin folder </param>
    /// <example>
    ///   <code>var loader = new ExtensionLoader("~/"); // all plugins are located in the root folder.</code>
    /// </example>
    public ExtensionLoader(string virtualPath = "~/bin/")
    {
      if (virtualPath == null) throw new ArgumentNullException("virtualPath");
      var path = virtualPath.StartsWith("~")
                     ? HostingEnvironment.MapPath(virtualPath)
                     : virtualPath;
      if (path == null)
        throw new InvalidOperationException(string.Format("Failed to map path '{0}'.", virtualPath));

      _path = path;
    }

    /// <summary>
    ///   Gets all loaded plugin assemblies.
    /// </summary>
    public IEnumerable<Assembly> Assemblies
    {
      get { return _assemblies; }
    }

    /// <summary>
    ///   Called during startup to scan for all plugin assemblies
    /// </summary>
    public void LoadExtensions(string searchPattern = "*.Extension.dll")
    {
      foreach (var file in Directory.GetFiles(_path, searchPattern))
        LoadAssembly(file);
    }

    internal Assembly TryLoadAssemblyByName(string fileName, bool throwOnError = false)
    {
      if (string.IsNullOrWhiteSpace(fileName))
      {
        if (throwOnError)
          throw new ArgumentNullException("fileName");
        
        return null;
      }

      var fullPath = Path.Combine(_path, fileName);
      if (!File.Exists(fullPath))
      {
        if (throwOnError)
          throw new FileNotFoundException("file not found", fullPath);

        return null;
      }

      try
      {
        var assembly = Assembly.LoadFrom(fullPath);
        BuildManager.AddReferencedAssembly(assembly);
        CompositionProvider.AddAssembly(assembly);
        _assemblies.Add(assembly);
        return assembly;
      }
      catch (Exception err)
      {
        Trace.TraceWarning("Failed to load " + fullPath + ".", err);

        var loaderEx = err as ReflectionTypeLoadException;
        if (loaderEx != null)
        {
          foreach (var exception in loaderEx.LoaderExceptions)
            Trace.TraceWarning(string.Format("Loader exception for file '{0}'.", fullPath), exception);
        }

        if (throwOnError)
          throw;

        return null;
      }
    }

    private void LoadAssembly(string fullPath)
    {
      if (fullPath == null) throw new ArgumentNullException("fullPath");

      try
      {
        var assembly = Assembly.LoadFrom(fullPath);
        BuildManager.AddReferencedAssembly(assembly);
        CompositionProvider.AddAssembly(assembly);
        _assemblies.Add(assembly);
      }
      catch (Exception err)
      {
        Trace.TraceWarning("Failed to load " + fullPath + ".", err);

        var loaderEx = err as ReflectionTypeLoadException;
        if (loaderEx != null)
        {
          foreach (var exception in loaderEx.LoaderExceptions)
            Trace.TraceWarning(string.Format("Loader exception for file '{0}'.", fullPath), exception);
        }

        throw;
      }
    }
  }
}
