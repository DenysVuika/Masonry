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
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Masonry.Core.Composition
{
  /// <summary>
  /// A <see cref="ContainerConfiguration"/> that loads parts in the currently-executing ASP.NET MVC web application.
  /// Parts are detected by namespace - classes in a ".Parts" namespace will be considered to be parts. These classes,
  /// and any interfaces they implement, will be exported and shared on a per-HTTP-request basis. When resolving
  /// dependencies, the longest public constructor on a part type will be used. The <see cref="ImportAttribute"/>,
  /// <see cref="ExportAttribute"/> and associated MEF attributes can be applied to modify composition structure.
  /// </summary>
  /// <remarks>
  /// This implementation emulates CompositionContainer and the composition-container based MVC
  /// integration for all types under the Parts namespace, for controllers, and for model binders. This will
  /// aid migration from one composition engine to the other, but this decision should be revisited if it
  /// causes confusion.
  /// </remarks>
  public class MvcContainerConfiguration : ContainerConfiguration
  {
    MvcContainerConfiguration(IEnumerable<Assembly> assemblies, AttributedModelProvider reflectionContext)
    {
      if (assemblies == null) throw new ArgumentNullException("assemblies");
      if (reflectionContext == null) throw new ArgumentNullException("reflectionContext");

      WithDefaultConventions(reflectionContext);
      WithAssemblies(assemblies);
    }

    /// <summary>
    /// Construct an <see cref="MvcContainerConfiguration"/> using parts in the specified assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies in which to search for parts.</param>
    public MvcContainerConfiguration(IEnumerable<Assembly> assemblies)
      : this(assemblies, DefineConventions())
    {
    }

    /// <summary>
    /// Construct an <see cref="MvcContainerConfiguration"/> using parts in the main application assembly.
    /// In some applications this may not be the expected assembly - in those cases specify the
    /// assemblies explicitly using the other constructor.
    /// </summary>
    public MvcContainerConfiguration()
      : this(new[] { GuessGlobalApplicationAssembly() })
    {
    }

    internal static Assembly GuessGlobalApplicationAssembly()
    {
      return HttpContext.Current.ApplicationInstance.GetType().BaseType.Assembly;
    }

    private static AttributedModelProvider DefineConventions()
    {
      var rb = new ConventionBuilder();

      rb.ForTypesDerivedFrom<IController>().Export();

      //rb.ForTypesDerivedFrom<IHttpController>().Export();

      //rb.ForTypesMatching(IsAPart)
      //    .Export()
      //    .ExportInterfaces();

      return rb;
    }

    //private static bool IsAPart(Type t)
    //{
    //  return !t.IsAssignableFrom(typeof(Attribute)) && t.IsInNamespace("Parts");
    //}
  }
}
