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
using System.Data.Entity;

namespace Masonry.Data
{
  public class MasonryDataContext : DbContext
  {
    public virtual IDbSet<User> Users { get; set; }
    public virtual IDbSet<ProfilePicture> ProfilePictures { get; set; }
    public virtual IDbSet<Post> Posts { get; set; }
    public virtual IDbSet<Comment> Comments { get; set; }
    
    static MasonryDataContext()
    {
      /* 
       * This switches off any database schema checks for this context.
       * As a result you will be getting huge performance improvements and 
       * possibility using external database schemas created in the scope
       * of other products.
       */
      Database.SetInitializer<MasonryDataContext>(null);
    }

    public MasonryDataContext()
      : base("DefaultConnection")
    {

    }
    
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder
        .Entity<User>()
        .HasMany(u => u.Following)
        .WithMany(u => u.Followers)
        .Map(m =>
        {
          m.ToTable("Subscriptions");
          m.MapLeftKey("UserId");
          m.MapRightKey("TargetUserId");
        });
    }
  }
}
