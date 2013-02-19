using System.Collections.Generic;

namespace Masonry.WebApi.Models
{
  public interface IPostRepository
  {
    IEnumerable<Post> GetAll();
    Post Get(int id);
    Post Add(Post item);
  }
}
