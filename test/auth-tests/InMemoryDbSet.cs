namespace Test.Auth
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Data.Entity;
  using System.Linq;

  public interface IInMemoryDbSet : IEnumerable
  {
    bool Contains(object o);
    void Add(object newRow);
    void Clear();
    Type ElementType { get; }
  }

  // From https://gist.github.com/troufster/913659
  public class InMemoryDbSet<T> : IInMemoryDbSet, IDbSet<T> where T : class
  {

    readonly HashSet<T> _set;
    public IQueryable<T> QueryableSet { get; private set; }


    public InMemoryDbSet() : this(Enumerable.Empty<T>()) { }

    public InMemoryDbSet(IEnumerable<T> entities)
    {
      _set = new HashSet<T>();

      foreach (var entity in entities)
      {
        _set.Add(entity);
      }

      QueryableSet = new TestDbAsyncEnumerable<T>(_set); // _set.AsQueryable();
    }

    public T Add(T entity)
    {
      _set.Add(entity);
      return entity;

    }

    public void AddRange(IEnumerable<T> entities)
    {
      foreach (var e in entities)
      {
        _set.Add(e);
      }
    }

    public T Attach(T entity)
    {
      _set.Add(entity);
      return entity;
    }

    public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
    {
      throw new NotImplementedException();
    }

    public T Create()
    {
      throw new NotImplementedException();
    }

    public virtual T Find(params object[] keyValues)
    {
      throw new NotImplementedException();
    }

    public System.Collections.ObjectModel.ObservableCollection<T> Local
    {
      get { throw new NotImplementedException(); }
    }

    public T Remove(T entity)
    {
      _set.Remove(entity);
      return entity;
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _set.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public Type ElementType
    {
      get { return QueryableSet.ElementType; }
    }

    public System.Linq.Expressions.Expression Expression
    {
      get { return QueryableSet.Expression; }
    }

    public IQueryProvider Provider
    {
      get { return QueryableSet.Provider; }
    }

    public void Clear()
    {
      _set.Clear();
    }

    bool IInMemoryDbSet.Contains(object o)
    {
      return (o is T) && this.Contains((T)o);
    }

    void IInMemoryDbSet.Add(object newRow)
    {
      var value = newRow as T;
      if (value == null)
      {
        throw new ArgumentException("value is not an instance of " + typeof(T).Name, "newRow");
      }

      this.Add(value);
    }
  }
}