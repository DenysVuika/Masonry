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
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

namespace Masonry.Core.Composition
{
  /// <summary>
  /// Provides lifetime management for the <see cref="CompositionProvider"/> type.
  /// This module is automatically injected into the ASP.NET request processing
  /// pipeline at startup and should not be called by user code.
  /// </summary>
  public class RequestCompositionScopeModule : IHttpModule
  {
    static bool _isInitialized;

    /// <summary>
    /// Register the module. This method is automatically called
    /// at startup and should not be called by user code.
    /// </summary>
    public static void Register()
    {
      if (!_isInitialized)
      {
        _isInitialized = true;
        DynamicModuleUtility.RegisterModule(typeof(RequestCompositionScopeModule));
      }
    }

    /// <summary>
    /// Release resources used by the module.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Initialize the module.
    /// </summary>
    /// <param name="context">Application in which the module is running.</param>
    public void Init(HttpApplication context)
    {
      context.EndRequest += DisposeCompositionScope;

      CompositionProvider.PostStartDefaultInitialize();
    }

    static void DisposeCompositionScope(object sender, EventArgs e)
    {
      var scope = CompositionProvider.CurrentInitialisedScope;
      if (scope != null)
        scope.Dispose();
    }
  }
}
