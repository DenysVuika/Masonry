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
using System.Data.Entity;
using System.Linq;
using Masonry.Data;
using System.Composition;
using Masonry.Data.Model;
using Masonry.Models;
using System.Web.Helpers;
using System.Web.Hosting;

namespace Masonry.Services
{
  [Export(typeof(IMasonryDataRepository))]
  public class MasonryDataRepository : IMasonryDataRepository
  {
    private readonly MasonryDataContext _context;
    public virtual MasonryDataContext DataContext
    {
      get { return _context; }
    }

    public MasonryDataRepository()
    {
      _context = new MasonryDataContext();
    }

    public MasonryDataRepository(MasonryDataContext context)
    {
      if (context == null) throw new ArgumentNullException("context");
      _context = context;
    }

    public virtual User FindUser(string account)
    {
      return string.IsNullOrWhiteSpace(account) ? null : _context.Users.FirstOrDefault(u => u.Account == account);
    }

    public virtual User FindUser(int accountId)
    {
      return _context.Users.FirstOrDefault(u => u.Id == accountId);
    }

    public virtual IEnumerable<CommentModel> GetComments(int postId)
    {
      var comments = _context.Comments
        .Include(c => c.User)
        .OrderBy(c => c.CreatedUtc)
        .Where(c => c.PostId == postId)
        .Select(c => new CommentModel
        {
          Id = c.Id,
          Account = c.User.Account,
          Name = c.User.Name,
          Content = c.Content,
          CreatedUtc = c.CreatedUtc,
          PostId = c.PostId
        }).ToList();

      return comments;
    }

    public virtual CommentModel CreateComment(int userId, int postId, string content)
    {
      var user = FindUser(userId);
      if (user == null) return null;

      var comment = new Comment
      {
        UserId = userId,
        PostId = postId,
        CreatedUtc = DateTime.UtcNow,
        Content = content
      };

      _context.Comments.Add(comment);
      _context.SaveChanges();
      
      return new CommentModel
      {
        Id = comment.Id,
        Account = user.Account,
        Content = comment.Content,
        CreatedUtc = comment.CreatedUtc,
        Name = user.Name,
        PostId = comment.PostId
      };
    }

    public virtual bool DeletePost(int accountId, int postId)
    {
      if (accountId <= 0 || postId <= 0) return false;

      var post = _context.Posts.FirstOrDefault(p => p.Id == postId);
      if (post == null || post.UserId != accountId) return false;

      _context.Posts.Remove(post);
      _context.SaveChanges();

      return true;
    }

    public virtual PostModel CreatePost(int userId, string content)
    {
      if (userId <= 0 || string.IsNullOrWhiteSpace(content)) return null;

      var user = FindUser(userId);
      if (user == null) return null;

      var post = new Post
      {
        UserId = userId,
        Content = content,
        CreatedUtc = DateTime.UtcNow
      };
      _context.Posts.Add(post);
      _context.SaveChanges();

      return new PostModel
      {
        Id = post.Id,
        Account = user.Account,
        Name = user.Name,
        Content = post.Content,
        CreatedUtc = post.CreatedUtc,
        CommentsCount = 0
      };
    }

    public virtual PostModel GetPost(int postId)
    {
      if (postId <= 0) return null;

      var post = _context.Posts
        .Select(p => new PostModel
        {
          Id = p.Id,
          Account = p.User.Account,
          Name = p.User.Name,
          Content = p.Content,
          CreatedUtc = p.CreatedUtc,
          CommentsCount = p.Comments.Count,
          Comments = p.Comments.Select(c => new CommentModel
          {
            Id = c.Id,
            Account = c.User.Account,
            Name = c.User.Name,
            Content = c.Content,
            CreatedUtc = c.CreatedUtc,
            PostId = c.PostId
          })
        })
        // materialize query
        .FirstOrDefault(p => p.Id == postId);

      return post;
    }

    public virtual bool Follow(int userId, string account)
    {
      if (userId <= 0 || string.IsNullOrWhiteSpace(account)) return false;

      // Modifying many:many data in EF actually sucks, 
      // it will be more efficient performing direct SQL call instead
      var targetUser = _context.Users
        .Where(u => u.Account == account && u.Id != userId)
        .Select(u => new
        {
          User = u,
          CanFollow = u.Followers.All(f => f.Id != userId)
        })
        .FirstOrDefault();

      if (targetUser == null || !targetUser.CanFollow) return false;

      var currentUser = _context.Users
        .Include(u => u.Following)
        .FirstOrDefault(u => u.Id == userId);

      if (currentUser == null) return false;

      if (currentUser.Following.All(u => u.Account != account))
      {
        currentUser.Following.Add(targetUser.User);
        _context.SaveChanges();
        return true; 
      }

      return false;
    }

    public virtual bool Unfollow(int userId, string account)
    {
      if (userId <= 0 || string.IsNullOrWhiteSpace(account)) return false;

      // Modifying many:many data in EF actually sucks, 
      // it will be more efficient performing direct SQL call instead
      var currentUser = _context.Users
        .Include(u => u.Following)
        .FirstOrDefault(u => u.Id == userId);

      if (currentUser == null) return false;
      
      var targetUser = currentUser.Following.FirstOrDefault(u => u.Account == account);
      if (targetUser == null) return false;

      if (currentUser.Following.Remove(targetUser))
      {
        _context.SaveChanges();
        return true;
      }

      return false;
    }

    public virtual IEnumerable<PostModel> GetTimelineEntries(int userId, int top)
    {
      if (userId <= 0 || top < 0) return Enumerable.Empty<PostModel>();

      var posts = _context.Users
        .Where(u => u.Id == userId)
        .SelectMany(u => u.Following.Concat(new[] { u }))
        .SelectMany(u => u.Posts)
        .OrderByDescending(p => p.CreatedUtc)
        .Where(p => top <= 0 || p.Id < top)
        .Take(Constants.PostsPerPage)
        .Select(p => new PostModel
        {
          Id = p.Id,
          Content = p.Content,
          CreatedUtc = p.CreatedUtc,
          Account = p.User.Account,
          Name = p.User.Name,
          CommentsCount = p.Comments.Count
        })
        // materialize result set
        .ToList();
      return posts;
    }

    public virtual int CheckTimelineUpdates(int userId, int top)
    {
      if (userId <= 0 || top <= 0) return 0;

      var result = _context.Users
        .Where(u => u.Id == userId)
        .SelectMany(u => u.Following.Concat(new[] { u }))
        .SelectMany(f => f.Posts)
        .OrderBy(p => p.Id)
        .Count(p => p.Id > top);

      return result;
    }

    public virtual IEnumerable<PostModel> GetTimelineUpdates(int userId, int top)
    {
      if (userId <= 0 || top <= 0) return Enumerable.Empty<PostModel>();

      var posts = _context.Users
        .Where(u => u.Id == userId)
        .SelectMany(u => u.Following.Concat(new[] { u }))
        .SelectMany(f => f.Posts)
        .OrderBy(p => p.CreatedUtc)
        .Where(p => p.Id > top)
        .Select(p => new PostModel
        {
          Id = p.Id,
          Content = p.Content,
          CreatedUtc = p.CreatedUtc,
          Account = p.User.Account,
          Name = p.User.Name,
          CommentsCount = p.Comments.Count
        })
        // materialize result set
        .ToList();
      return posts;
    }

    public virtual IEnumerable<PostModel> GetFeedEntries(int userId, int top)
    {
      if (userId <= 0 || top < 0) return Enumerable.Empty<PostModel>();

      var posts = _context.Posts
        .Where(p => p.UserId == userId)
        .OrderByDescending(p => p.CreatedUtc)
        .Where(p => top <= 0 || p.Id < top)
        .Take(Constants.PostsPerPage)
        .Select(p => new PostModel
        {
          Id = p.Id,
          Account = p.User.Account,
          Name = p.User.Name,
          Content = p.Content,
          CreatedUtc = p.CreatedUtc,
          CommentsCount = p.Comments.Count
        })
        // materialize result set
        .ToList();

      return posts;
    }

    public virtual IEnumerable<PostModel> GetMentions(string account, int top)
    {
      if (string.IsNullOrWhiteSpace(account)) return Enumerable.Empty<PostModel>();

      var mention = "@" + account;

      var posts = _context.Posts
        .Where(p => p.User.Account != account && p.Content.Contains(mention))
        // TODO: Add searching through comments as well?
        .OrderByDescending(p => p.CreatedUtc)
        .Where(p => top <= 0 || p.Id < top)
        .Take(Constants.PostsPerPage)
        .Select(p => new PostModel
        {
          Id = p.Id,
          Account = p.User.Account,
          Name = p.User.Name,
          Content = p.Content,
          CreatedUtc = p.CreatedUtc,
          CommentsCount = p.Comments.Count
        })
        // materialize result set
        .ToList();

      return posts;
    }

    //public UserProfileModel GetUserProfile(int userId, string account)
    //{
    //  if (string.IsNullOrWhiteSpace(account)) return null;

    //  var profile = _context.Users
    //    .Where(u => u.Account == account)
    //    .Select(u => new UserProfileModel
    //    {
    //      Account = u.Account,
    //      Name = u.Name,
    //      Website = u.Website,
    //      Bio = u.Bio,
    //      Posts = u.Posts.Count,
    //      Followers = u.Followers.Count,
    //      Following = u.Following.Count,
    //      IsFollowed = u.Followers.Any(user => user.Id == userId)
    //    })
    //    .FirstOrDefault();
      
    //  return profile;
    //}

    public virtual UserProfileModel GetUserProfile(string currentUserAccount, string account)
    {
      if (string.IsNullOrWhiteSpace(currentUserAccount) || string.IsNullOrWhiteSpace(account)) return null;

      var profile = _context.Users
        .Where(u => u.Account == account)
        .Select(u => new UserProfileModel
        {
          Account = u.Account,
          Name = u.Name,
          Website = u.Website,
          Bio = u.Bio,
          Posts = u.Posts.Count,
          Followers = u.Followers.Count,
          Following = u.Following.Count,
          IsFollowed = u.Followers.Any(user => user.Account == currentUserAccount)
        })
        .FirstOrDefault();

      if (profile != null)
        profile.IsOwnProfile = currentUserAccount.Equals(account, StringComparison.OrdinalIgnoreCase);

      return profile;
    }

    public virtual IEnumerable<UserProfileModel> GetUserProfiles(int userId, int page = 0)
    {
      if (userId <= 0 || page < 0) return Enumerable.Empty<UserProfileModel>();

      var skipRecords = page * Constants.UserProfilesPerPage;

      var profiles = _context.Users
          .Where(u => u.Id != userId)
          .OrderBy(u => u.JoinedUtc)
          .Skip(skipRecords)
          .Take(Constants.UserProfilesPerPage)
          .Select(u => new UserProfileModel
          {
            Account = u.Account,
            Name = u.Name,
            Website = u.Website,
            Bio = u.Bio,
            Posts = u.Posts.Count,
            Followers = u.Followers.Count,
            Following = u.Following.Count,
            IsFollowed = u.Followers.Any(user => user.Id == userId)
          })
          // materialize query
          .ToList();

      return profiles;
    }

    public virtual IEnumerable<UserProfileModel> GetFollowings(int userId, int page = 0)
    {
      if (userId <= 0) return Enumerable.Empty<UserProfileModel>();

      var skipRecords = page * Constants.UserProfilesPerPage;

      var profiles = _context.Users
        .Where(u => u.Id == userId)
        .SelectMany(u => u.Following)
        .OrderBy(u => u.JoinedUtc)
        .Skip(skipRecords)
        .Take(Constants.UserProfilesPerPage)
        .Select(u => new UserProfileModel
        {
          Account = u.Account,
          Name = u.Name,
          Website = u.Website,
          Bio = u.Bio,
          Posts = u.Posts.Count,
          Followers = u.Followers.Count,
          Following = u.Following.Count,
          IsFollowed = u.Followers.Any(user => user.Id == userId),
          IsOwnProfile = u.Id == userId
        })
        // materialize query
        .ToList();

      return profiles;
    }

    public virtual IEnumerable<UserProfileModel> GetFollowers(int userId, int page = 0)
    {
      if (userId <= 0) return Enumerable.Empty<UserProfileModel>();

      var skipRecords = page * Constants.UserProfilesPerPage;

      var profiles = _context.Users
          .Where(u => u.Id == userId)
          .SelectMany(u => u.Followers)
          .OrderBy(u => u.JoinedUtc)
          .Skip(skipRecords)
          .Take(Constants.UserProfilesPerPage)
          .Select(u => new UserProfileModel
          {
            Account = u.Account,
            Name = u.Name,
            Website = u.Website,
            Bio = u.Bio,
            Posts = u.Posts.Count,
            Followers = u.Followers.Count,
            Following = u.Following.Count,
            IsFollowed = u.Followers.Any(user => user.Id == userId),
            IsOwnProfile = u.Id == userId
          })
          // materialize query
          .ToList();

      return profiles;
    }

    public virtual UserPictureModel GetUserPictureNormal(string account)
    {
      if (string.IsNullOrWhiteSpace(account)) return GetDefaultPicture(Constants.PictureSizeNormal);

      var picture = _context.ProfilePictures
        .Where(p => p.User.Account == account)
        .Select(p => new UserPictureModel { Data = p.Image, ContentType = p.ContentType })
        .FirstOrDefault();

      return picture ?? GetDefaultPicture(Constants.PictureSizeNormal);
    }

    public virtual UserPictureModel GetUserPictureSmall(string account)
    {
      if (string.IsNullOrWhiteSpace(account)) return GetDefaultPicture(Constants.PictureSizeSmall);

      var picture = _context.ProfilePictures
        .Where(p => p.User.Account == account)
        .Select(p => new UserPictureModel { Data = p.ImageSmall, ContentType = p.ContentType })
        .FirstOrDefault();

      return picture ?? GetDefaultPicture(Constants.PictureSizeSmall);
    }

    public virtual UserPictureModel GetUserPictureTiny(string account)
    {
      if (string.IsNullOrWhiteSpace(account)) return GetDefaultPicture(Constants.PictureSizeTiny);

      var picture = _context.ProfilePictures
        .Where(p => p.User.Account == account)
        .Select(p => new UserPictureModel { Data = p.ImageTiny, ContentType = p.ContentType })
        .FirstOrDefault();

      return picture ?? GetDefaultPicture(Constants.PictureSizeTiny);
    }

    public virtual UserPictureModel GetDefaultPicture(int size)
    {
      var image = new WebImage(HostingEnvironment.MapPath(Constants.DefaultProfilePicturePath));
      image.Resize(size + 1, size + 1).Crop(1, 1);
      return new UserPictureModel { Data = image.GetBytes(), ContentType = image.ImageFormat };
    }

    public virtual bool UpdateAccountSettings(int userId, string name, string website, string location, string bio, byte[] picture)
    {
      if (userId <= 0) return false;

      var account = _context.Users.Find(userId);
      if (account == null) return false;

      account.Name = name;
      account.Website = string.IsNullOrWhiteSpace(website) ? null : website;
      account.Location = string.IsNullOrWhiteSpace(location) ? null : location;
      account.Bio = string.IsNullOrWhiteSpace(bio) ? null : bio;

      if (picture != null && picture.Length > 0)
      {
        var image = new WebImage(picture);

        var profilePicture = _context.ProfilePictures.FirstOrDefault(p => p.UserId == userId);
        if (profilePicture == null)
        {
          profilePicture = new ProfilePicture
          {
            UserId = userId,
            ContentType = image.ImageFormat
          };

          _context.ProfilePictures.Add(profilePicture);
        }

        // TODO: those size manipulations are required to workaround WebImage.Resize bug related to 1px black border
        profilePicture.Image = image.Resize(Constants.PictureSizeNormal + 1, Constants.PictureSizeNormal + 1).Crop(1, 1).GetBytes();
        profilePicture.ImageSmall = image.Resize(Constants.PictureSizeSmall + 1, Constants.PictureSizeSmall + 1).Crop(1, 1).GetBytes();
        profilePicture.ImageTiny = image.Resize(Constants.PictureSizeTiny + 1, Constants.PictureSizeTiny + 1).Crop(1, 1).GetBytes();
      }

      _context.SaveChanges();
      return true;
    }
  }
}