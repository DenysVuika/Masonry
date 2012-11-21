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

using System.Collections.Generic;
using System.Linq;
using Masonry.Composition.Verbs;
using Masonry.Core.Composition;
using Masonry.Core.Web;
using WebMatrix.WebData;

namespace Masonry.Extensions
{
  public static class HtmlExtensions
  {
    public static IEnumerable<IHeaderActionVerb> GetHeaderActions(this MasonryHtmlHelper helper)
    {
      return GetHeaderActions().Where(v => string.IsNullOrWhiteSpace(v.GroupName));
    }

    private static IEnumerable<IHeaderActionVerb> GetHeaderActions()
    {
      var isAuthenticated = WebSecurity.IsAuthenticated;

      return CompositionProvider.Current.GetExports<IHeaderActionVerb>()
        .Where(v => v.IsPublic == !isAuthenticated);
    }

    public static IEnumerable<ISidebarActionVerb> GetSidebarActions(this MasonryHtmlHelper helper)
    {
      return CompositionProvider.Current.GetExports<ISidebarActionVerb>();
    }

    public static bool HasSidebarActions(this MasonryHtmlHelper helper)
    {
      return GetSidebarActions(helper).Any();
    }

    public static IEnumerable<HeaderActionGroup> GetHeaderActionGroups(this MasonryHtmlHelper helper)
    {
      var cache = new Dictionary<string, HeaderActionGroup>();

      var actions = GetHeaderActions().Where(v => !string.IsNullOrWhiteSpace(v.GroupName));

      foreach (var verb in actions)
      {
        HeaderActionGroup group;
        if (!cache.TryGetValue(verb.GroupName, out group))
        {
          group = new HeaderActionGroup { Name = verb.GroupName };
          cache[verb.GroupName] = group;
        }

        group.AddVerb(verb);
      }

      return cache.Values;
    }
  }
}