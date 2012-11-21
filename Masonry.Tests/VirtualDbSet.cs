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

using Masonry.Data.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Masonry.Tests
{
  public class VirtualDbSet<T> : IDbSet<T> where T: class, IMasonryEntity
  {
    private readonly HashSet<T> _data;

    public VirtualDbSet()
    {
      _data = new HashSet<T>();
    }

    public virtual T Find(params object[] keyValues)
    {
      return _data.FirstOrDefault(item => item.Id == (int)keyValues[0]);
    }

    public T Add(T item)
    {
      _data.Add(item);
      return item;
    }

    public T Remove(T item)
    {
      _data.Remove(item);
      return item;
    }

    public T Attach(T item)
    {
      _data.Add(item);
      return item;
    }

    public void Detach(T item)
    {
      _data.Remove(item);
    }

    Type IQueryable.ElementType
    {
      get { return _data.AsQueryable().ElementType; }
    }

    Expression IQueryable.Expression
    {
      get { return _data.AsQueryable().Expression; }
    }

    IQueryProvider IQueryable.Provider
    {
      get { return _data.AsQueryable().Provider; }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _data.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _data.GetEnumerator();
    }

    public T Create()
    {
      return Activator.CreateInstance<T>();
    }

    public ObservableCollection<T> Local
    {
      get { return new ObservableCollection<T>(_data); }
    }

    public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
    {
      return Activator.CreateInstance<TDerivedEntity>();
    }
  }
}
