using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Masonry.WebApi.Models;

namespace Masonry.WebApi.Controllers
{
    public class PostsController : ApiController
    {
      private static readonly IPostRepository repository = new PostRepository();

      public IEnumerable<Post> GetAllPosts()
      {
        return repository.GetAll();
      }

      public Post GetPost(int id)
      {
        var item = repository.Get(id);
        if (item == null) throw new HttpResponseException(HttpStatusCode.NotFound);
        return item;
      }
    }
}
