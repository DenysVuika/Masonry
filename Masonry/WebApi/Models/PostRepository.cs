using System;
using System.Collections.Generic;

namespace Masonry.WebApi.Models
{
  public class PostRepository : IPostRepository
  {
    private readonly List<Post> _posts = new List<Post>();
    private int _nextId = 1;

    public PostRepository()
    {
      Add(new Post {Content = "post 1"});
      Add(new Post {Content = "post 2"});
      Add(new Post {Content = "post 3"});
    }

    public IEnumerable<Post> GetAll()
    {
      return _posts;
    }

    public Post Get(int id)
    {
      return _posts.Find(p => p.Id == id);
    }

    public Post Add(Post item)
    {
      if (item == null) throw new ArgumentNullException("item");
      item.Id = _nextId++;
      _posts.Add(item);
      return item;
    }
  }
}