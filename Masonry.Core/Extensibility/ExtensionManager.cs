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

using System.Web.Hosting;
using Masonry.Core.Hosting;
using Masonry.Core.Configuration;
using System.Linq;

namespace Masonry.Core.Extensibility
{
  public class ExtensionManager
  {
    private static ExtensionLoader _extensionLoader;

    public static void Register()
    {
      _extensionLoader = new ExtensionLoader();
      
      var viewProvider = new EmbeddedViewFileProvider();
      var contentProvider = new EmbeddedFileProvider();

      var provider = new AggregateVirtualPathProvider(viewProvider, contentProvider);
      HostingEnvironment.RegisterVirtualPathProvider(provider);

      var config = CoreConfiguration.Current.Extensibility;
      if (config == null) return;

      // automatic extension discovery with '*.Extension.dll' convention
      if (config.AutomaticExtensionDiscovery)
      {
        _extensionLoader.LoadExtensions();

        foreach (var assembly in _extensionLoader.Assemblies)
        {
          viewProvider.Add(assembly);
          contentProvider.Add(assembly);
        }
      }
      else
      {
        // controlled extension discovery based on web.config (without conventions)
        foreach (var ext in config.Extensions
          .OfType<ExtensionElement>()
          .Distinct(new ExtensionElementNameComparer()))
        {
          var assembly = _extensionLoader.TryLoadAssemblyByName(ext.Name);
          if (assembly == null) continue;

          var mapping = new NamespaceMapping(assembly, ext.Namespace);
          viewProvider.Add(mapping);
          contentProvider.Add(mapping);
        }
      }
    }
  }
}