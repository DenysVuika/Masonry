using Masonry.Data;
using Masonry.Data.Model;
using Masonry.Models;
using Masonry.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Masonry.Tests.Services
{
  [TestClass]
  public class MasonryDataRepositoryTest
  {
    private VirtualDbSet<User> Users { get; set; }
    private VirtualDbSet<ProfilePicture> ProfilePictures { get; set; }
    private VirtualDbSet<Post> Posts { get; set; }
    private VirtualDbSet<Comment> Comments { get; set; }

    private Mock<MasonryDataContext> DataContext { get; set; }
    private Mock<MasonryDataRepository> Repository { get; set; }
 
    [TestInitialize]
    public void Setup()
    {
      Users = new VirtualDbSet<User>();
      ProfilePictures = new VirtualDbSet<ProfilePicture>();
      Posts = new VirtualDbSet<Post>();
      Comments = new VirtualDbSet<Comment>();

      DataContext = new Mock<MasonryDataContext>();
      DataContext.Setup(x => x.Users).Returns(Users);
      DataContext.Setup(x => x.ProfilePictures).Returns(ProfilePictures);
      DataContext.Setup(x => x.Posts).Returns(Posts);
      DataContext.Setup(x => x.Comments).Returns(Comments);
      Repository = new Mock<MasonryDataRepository>(DataContext.Object) { CallBase = true };
    }

    [TestCleanup]
    public void Cleanup()
    {
      Repository = null;
      DataContext = null;
      Comments = null;
      Posts = null;
      ProfilePictures = null;
      Users = null;
    }

    [TestMethod]
    public void CreatesNewDataContext()
    {
      var repository = new MasonryDataRepository();
      Assert.IsNotNull(repository.DataContext);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void RequiresDataContext()
    {
      var repository = new MasonryDataRepository(null);
      Assert.IsNotNull(repository);
    }

    [TestMethod]
    public void FindUserWithEmptyAccount()
    {
      var user = Repository.Object.FindUser(null);
      Assert.IsNull(user);

      DataContext.VerifyGet(x => x.Users, Times.Never());
    }

    [TestMethod]
    public void FindUserByAccount()
    {
      var expectedUser = new User { Account = "user2" };

      Users.Add(new User { Account = "user1" });
      Users.Add(expectedUser);
      Users.Add(new User { Account = "user3" });

      var user = Repository.Object.FindUser(expectedUser.Account);
      Assert.IsNotNull(user);
      Assert.AreEqual(expectedUser, user);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void FindUserByAccountId()
    {
      var expectedUser = new User { Id = 1 };

      Users.Add(new User { Id = 2 });
      Users.Add(expectedUser);
      Users.Add(new User { Id = 3});

      var user = Repository.Object.FindUser(1);
      Assert.IsNotNull(user);
      Assert.AreEqual(expectedUser, user);
      
      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetCommentsForPost()
    {
      var user = new User { Id = 1, Account = "jdoe", Name = "john doe" };
      var comment1 = new Comment { Id = 1, User = user, Content = "comment1", CreatedUtc = DateTime.UtcNow, PostId = 1 };
      var comment2 = new Comment { Id = 2, User = user, Content = "comment2", CreatedUtc = DateTime.UtcNow, PostId = 2 };
      
      Comments.Add(comment1);
      Comments.Add(comment2);

      var comments = Repository.Object.GetComments(1).ToList();
      Assert.IsNotNull(comments);
      Assert.AreEqual(1, comments.Count());

      var comment = comments.First();
      Assert.AreEqual(comment1.Id, comment.Id);
      Assert.AreEqual(user.Account, comment.Account);
      Assert.AreEqual(user.Name, comment.Name);
      Assert.AreEqual(comment1.Content, comment.Content);
      Assert.AreEqual(comment1.CreatedUtc, comment.CreatedUtc);
      Assert.AreEqual(comment1.PostId, comment.PostId);

      DataContext.VerifyGet(x => x.Comments, Times.Once());
    }

    [TestMethod]
    public void GetNoCommentsForPost()
    {
      var comments = Repository.Object.GetComments(100).ToList();
      Assert.IsNotNull(comments);
      Assert.AreEqual(0, comments.Count);

      DataContext.VerifyGet(x => x.Comments, Times.Once());
    }

    [TestMethod]
    public void CreateCommentWithMissingUser()
    {
      Repository.Setup(x => x.FindUser(It.IsAny<int>())).Returns((User)null);
      var comment = Repository.Object.CreateComment(1, 1, "content");
      Assert.IsNull(comment);

      Repository.Verify(x => x.FindUser(1), Times.Once());
      DataContext.VerifyGet(x => x.Comments, Times.Never());
    }

    [TestMethod]
    public void CreateComment()
    {
      var user = new User { Id = 1, Account = "jdoe", Name = "john doe" };
      Repository.Setup(x => x.FindUser(It.IsAny<int>())).Returns(user);

      var comment = Repository.Object.CreateComment(1, 2, "content1");
      Assert.IsNotNull(comment);
      Assert.AreEqual(user.Account, comment.Account);
      Assert.AreEqual("content1", comment.Content);
      Assert.AreEqual(user.Name, comment.Name);
      Assert.AreEqual(2, comment.PostId);

      Repository.Verify(x => x.FindUser(1), Times.Once());
      DataContext.VerifyGet(x => x.Comments, Times.Once());
    }

    [TestMethod]
    public void DeletePostWithInvalidArguments()
    {
      var result = Repository.Object.DeletePost(0, 0);
      Assert.IsFalse(result);

      DataContext.VerifyGet(x => x.Posts, Times.Never());
    }

    [TestMethod]
    public void DeleteMissingPost()
    {
      var result = Repository.Object.DeletePost(1, 1);
      Assert.IsFalse(result);

      DataContext.VerifyGet(x => x.Posts, Times.Once());
    }

    [TestMethod]
    public void DeletePostWithDifferentUser()
    {
      Posts.Add(new Post { Id = 1, UserId = 2 });
      var result = Repository.Object.DeletePost(1, 1);
      Assert.IsFalse(result);

      DataContext.VerifyGet(x => x.Posts, Times.Once());
    }

    [TestMethod]
    public void DeletePost()
    {
      var post = new Post { Id = 1, UserId = 1 };
      Posts.Add(post);
      var result = Repository.Object.DeletePost(1, 1);
      Assert.IsTrue(result);
      Assert.AreEqual(0, Posts.Count());
    }

    [TestMethod]
    public void CreatePostWithInvalidArguments()
    {
      var post = Repository.Object.CreatePost(0, null);
      Assert.IsNull(post);

      Repository.Verify(x => x.FindUser(0), Times.Never());
    }

    [TestMethod]
    public void CreatePostWithMissingUser()
    {
      var post = Repository.Object.CreatePost(1, "content");
      Assert.IsNull(post);

      Repository.Verify(x => x.FindUser(1), Times.Once());
      DataContext.VerifyGet(x => x.Posts, Times.Never());
    }

    [TestMethod]
    public void CreatePost()
    {
      var user = new User { Id = 1, Account = "jdoe", Name = "john doe" };
      Repository.Setup(x => x.FindUser(It.IsAny<int>())).Returns(user);

      var post = Repository.Object.CreatePost(1, "content1");
      Assert.IsNotNull(post);
      Assert.AreEqual(user.Account, post.Account);
      Assert.AreEqual(user.Name, post.Name);
      Assert.AreEqual("content1", post.Content);

      Repository.Verify(x => x.FindUser(1), Times.Once());
      DataContext.VerifyGet(x => x.Posts, Times.Once());
    }

    [TestMethod]
    public void GetPostWithInvalidArguments()
    {
      var post = Repository.Object.GetPost(0);
      Assert.IsNull(post);
      
      DataContext.VerifyGet(x => x.Posts, Times.Never());
    }

    [TestMethod]
    public void GetPost()
    {
      var user = new User { Account = "jdoe", Name = "john doe" };

      var post = new Post
      {
        Id = 1,
        User = user,
        Comments = new Collection<Comment>(),
        Content = "content1"
      };

      Posts.Add(post);

      var result = Repository.Object.GetPost(1);
      Assert.IsNotNull(post);
      Assert.AreEqual(post.Id, result.Id);
      Assert.AreEqual(user.Account, result.Account);
      Assert.AreEqual(user.Name, result.Name);
      Assert.AreEqual(post.Content, result.Content);
      Assert.AreEqual(0, result.CommentsCount);

      DataContext.VerifyGet(x => x.Posts, Times.Once());
    }

    [TestMethod]
    public void FollowWithInvalidArguments()
    {
      var result = Repository.Object.Follow(0, null);
      Assert.IsFalse(result);

      DataContext.VerifyGet(x => x.Users, Times.Never());
      DataContext.Verify(x => x.SaveChanges(), Times.Never());
    }

    [TestMethod]
    public void FollowSelf()
    {
      Users.Add(new User { Id = 1, Account = "jdoe" });
      var result = Repository.Object.Follow(1, "jdoe");
      Assert.IsFalse(result);

      DataContext.Verify(x => x.SaveChanges(), Times.Never());
    }

    [TestMethod]
    public void Follow()
    {
      var user1 = new User { Id = 1, Account = "user1", Followers = new Collection<User>(), Following = new Collection<User>() };
      var user2 = new User { Id = 2, Account = "user2", Followers = new Collection<User>(), Following = new Collection<User>() };

      Users.Add(user1);
      Users.Add(user2);

      var result = Repository.Object.Follow(1, "user2");
      Assert.IsTrue(result);
      Assert.AreEqual(1, user1.Following.Count);
      Assert.AreEqual(user2, user1.Following.First());

      DataContext.Verify(x => x.SaveChanges(), Times.Once());
    }

    [TestMethod]
    public void FollowWithMissingUser()
    {
      var user2 = new User { Id = 2, Account = "user2", Followers = new Collection<User>(), Following = new Collection<User>() };
      Users.Add(user2);

      var result = Repository.Object.Follow(1, "user2");
      Assert.IsFalse(result);

      DataContext.VerifyGet(x => x.Users, Times.Exactly(2));
      DataContext.Verify(x => x.SaveChanges(), Times.Never());
    }

    [TestMethod]
    public void FollowSecondTime()
    {
      var user1 = new User { Id = 1, Account = "user1", Followers = new Collection<User>(), Following = new Collection<User>() };
      var user2 = new User { Id = 2, Account = "user2", Followers = new Collection<User>(), Following = new Collection<User>() };
      user1.Following.Add(user2);

      Users.Add(user1);
      Users.Add(user2);
      
      var result = Repository.Object.Follow(1, "user2");
      Assert.IsFalse(result);

      DataContext.Verify(x => x.SaveChanges(), Times.Never());
    }

    [TestMethod]
    public void UnfollowWithInvalidArguments()
    {
      var result = Repository.Object.Unfollow(0, null);
      Assert.IsFalse(result);

      DataContext.VerifyGet(x => x.Users, Times.Never());
      DataContext.Verify(x => x.SaveChanges(), Times.Never());
    }

    [TestMethod]
    public void UnfollowSelf()
    {
      Users.Add(new User { Id = 1, Account = "jdoe", Following = new Collection<User>()});
      var result = Repository.Object.Unfollow(1, "jdoe");
      Assert.IsFalse(result);

      DataContext.Verify(x => x.SaveChanges(), Times.Never());
    }

    [TestMethod]
    public void Unfollow()
    {
      var user1 = new User { Id = 1, Account = "user1", Followers = new Collection<User>(), Following = new Collection<User>() };
      var user2 = new User { Id = 2, Account = "user2", Followers = new Collection<User>(), Following = new Collection<User>() };
      user1.Following.Add(user2);

      Users.Add(user1);
      Users.Add(user2);

      var result = Repository.Object.Unfollow(1, "user2");
      Assert.IsTrue(result);
      Assert.AreEqual(0, user1.Following.Count);

      DataContext.Verify(x => x.SaveChanges(), Times.Once());
    }

    [TestMethod]
    public void UnfollowForMissingUser()
    {
      var result = Repository.Object.Unfollow(1, "user1");
      Assert.IsFalse(result);

      DataContext.Verify(x => x.SaveChanges(), Times.Never());
    }

    [TestMethod]
    public void UnfollowSecondTime()
    {
      var user1 = new User { Id = 1, Account = "user1", Followers = new Collection<User>(), Following = new Collection<User>() };
      Users.Add(user1);

      var result = Repository.Object.Unfollow(1, "user2");
      Assert.IsFalse(result);

      DataContext.Verify(x => x.SaveChanges(), Times.Never());
    }

    private sealed class WriteonlyCollection<T> : Collection<T>, ICollection<T>
    {
      bool ICollection<T>.Remove(T item)
      {
        return false;
      }
    }

    [TestMethod]
    public void UnfollowWithRemoveRejection()
    {
      var user1 = new User { Id = 1, Account = "user1", Followers = new Collection<User>(), Following = new WriteonlyCollection<User>() };
      var user2 = new User { Id = 2, Account = "user2", Followers = new Collection<User>(), Following = new Collection<User>() };
      user1.Following.Add(user2);

      Users.Add(user1);
      Users.Add(user2);

      var result = Repository.Object.Unfollow(1, "user2");
      Assert.IsFalse(result);

      DataContext.VerifyGet(x => x.Users, Times.Once());
      DataContext.Verify(x => x.SaveChanges(), Times.Never());
    }

    [TestMethod]
    public void GetTimelineEntriesWithInvalidArguments()
    {
      var posts = Repository.Object.GetTimelineEntries(0, -1).ToList();
      Assert.IsNotNull(posts);
      Assert.AreEqual(0, posts.Count);

      DataContext.VerifyGet(x => x.Users, Times.Never());
    }

    [TestMethod]
    public void GetTimelineEntriesForMissingUser()
    {
      var posts = Repository.Object.GetTimelineEntries(1, 0).ToList();
      Assert.IsNotNull(posts);
      Assert.AreEqual(0, posts.Count);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetTimelineEntries()
    {
      // 1st user to be selected
      var user1 = new User { Id = 1, Following = new Collection<User>()};
      var post1 = new Post { Id = 1, Content = "post1", CreatedUtc = DateTime.UtcNow, User = user1, Comments = new Collection<Comment>() };
      user1.Posts = new Collection<Post> { post1 };

      // 2nd user to be skipped
      var user2 = new User { Id = 2, Following = new Collection<User>()};
      var post2 = new Post { Id = 2, Content = "post2", CreatedUtc = DateTime.UtcNow, User = user2, Comments = new Collection<Comment>() };
      user2.Posts = new Collection<Post> { post2 };

      Users.Add(user1);
      Users.Add(user2);

      var posts = Repository.Object.GetTimelineEntries(1, 0).ToList();
      Assert.IsNotNull(posts);
      Assert.AreEqual(1, posts.Count);

      var result = posts.First();
      Assert.AreEqual(post1.Id, result.Id);
      Assert.AreEqual(post1.Content, result.Content);
      Assert.AreEqual(post1.CreatedUtc, result.CreatedUtc);
      Assert.AreEqual(user1.Account, result.Account);
      Assert.AreEqual(user1.Name, result.Name);
      Assert.AreEqual(post1.Comments.Count, result.CommentsCount);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetTimelineEntriesFromFollowings()
    {
      // 1st user
      var user1 = new User { Id = 1, Following = new Collection<User>() };
      var post1 = new Post { Id = 1, Content = "post1", CreatedUtc = DateTime.UtcNow, User = user1, Comments = new Collection<Comment>() };
      user1.Posts = new Collection<Post> { post1 };

      // 2nd user
      var user2 = new User { Id = 2, Following = new Collection<User>() };
      var post2 = new Post { Id = 2, Content = "post2", CreatedUtc = DateTime.UtcNow, User = user2, Comments = new Collection<Comment>() };
      user2.Posts = new Collection<Post> { post2 };

      // user 1 follows user 2
      user1.Following.Add(user2);

      Users.Add(user1);
      Users.Add(user2);

      var posts = Repository.Object.GetTimelineEntries(1, 0).ToList();
      Assert.IsNotNull(posts);
      Assert.AreEqual(2, posts.Count);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetTimelineEntriesWithTop()
    {
      var user1 = new User { Id = 1, Following = new Collection<User>() };
      var post1 = new Post { Id = 1, Content = "post1", CreatedUtc = DateTime.UtcNow, User = user1, Comments = new Collection<Comment>() };
      var post2 = new Post { Id = 2, Content = "post2", CreatedUtc = DateTime.UtcNow, User = user1, Comments = new Collection<Comment>() };
      user1.Posts = new Collection<Post> { post1, post2 };

      Users.Add(user1);

      // as timeline uses descending datetime mode only posts with id lower than Top value will be selected
      var posts = Repository.Object.GetTimelineEntries(1, 2).ToList();
      Assert.IsNotNull(posts);
      Assert.AreEqual(1, posts.Count);

      var result = posts.First();
      Assert.AreEqual(post1.Id, result.Id);
      Assert.AreEqual(post1.Content, result.Content);
      Assert.AreEqual(post1.CreatedUtc, result.CreatedUtc);
      Assert.AreEqual(user1.Account, result.Account);
      Assert.AreEqual(user1.Name, result.Name);
      Assert.AreEqual(post1.Comments.Count, result.CommentsCount);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void CheckTimelineUpdatesWithInvalidArguments()
    {
      var result = Repository.Object.CheckTimelineUpdates(0, -1);
      Assert.AreEqual(0, result);

      DataContext.VerifyGet(x => x.Users, Times.Never());
    }

    [TestMethod]
    public void CheckTimelineUpdates()
    {
      var user1 = new User { Id = 1, Following = new Collection<User>() };
      var post1 = new Post { Id = 1, Content = "post1", CreatedUtc = DateTime.UtcNow, User = user1, Comments = new Collection<Comment>() };
      var post2 = new Post { Id = 2, Content = "post2", CreatedUtc = DateTime.UtcNow, User = user1, Comments = new Collection<Comment>() };
      user1.Posts = new Collection<Post> { post1, post2 };

      Users.Add(user1);

      var result = Repository.Object.CheckTimelineUpdates(1, 1);
      Assert.AreEqual(1, result);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void CheckTimelineUpdatesZero()
    {
      var user1 = new User { Id = 1, Following = new Collection<User>() };
      var post1 = new Post { Id = 1, Content = "post1", CreatedUtc = DateTime.UtcNow, User = user1, Comments = new Collection<Comment>() };
      user1.Posts = new Collection<Post> { post1 };

      Users.Add(user1);

      var result = Repository.Object.CheckTimelineUpdates(1, 1);
      Assert.AreEqual(0, result);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetTimelineUpdatesWithInvalidArguments()
    {
      var posts = Repository.Object.GetTimelineUpdates(0, 0).ToList();
      Assert.IsNotNull(posts);
      Assert.AreEqual(0, posts.Count);

      DataContext.VerifyGet(x => x.Users, Times.Never());
    }

    [TestMethod]
    public void GetTimelineUpdates()
    {
      var user1 = new User { Id = 1, Following = new Collection<User>() };
      var post1 = new Post { Id = 1, Content = "post1", CreatedUtc = DateTime.UtcNow, User = user1, Comments = new Collection<Comment>() };
      var post2 = new Post { Id = 2, Content = "post2", CreatedUtc = DateTime.UtcNow, User = user1, Comments = new Collection<Comment>() };
      user1.Posts = new Collection<Post> { post1, post2 };

      Users.Add(user1);

      var posts = Repository.Object.GetTimelineUpdates(1, 1).ToList();
      Assert.IsNotNull(posts);
      Assert.AreEqual(1, posts.Count);

      var result = posts.First();
      Assert.AreEqual(post2.Id, result.Id);
      Assert.AreEqual(post2.Content, result.Content);
      Assert.AreEqual(post2.CreatedUtc, result.CreatedUtc);
      Assert.AreEqual(user1.Account, result.Account);
      Assert.AreEqual(user1.Name, result.Name);
      Assert.AreEqual(post2.Comments.Count, result.CommentsCount);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetFeedEntriesWithInvalidArguments()
    {
      var result = Repository.Object.GetFeedEntries(0, -1).ToList();
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count);

      DataContext.VerifyGet(x => x.Posts, Times.Never());
    }

    [TestMethod]
    public void GetFeedEntries()
    {
      var user1 = new User { Id = 1, Following = new Collection<User>() };
      var post1 = new Post { Id = 1, Content = "post1", CreatedUtc = DateTime.UtcNow, User = user1, UserId = 1, Comments = new Collection<Comment>() };
      var post2 = new Post { Id = 2, Content = "post2", CreatedUtc = DateTime.UtcNow, User = user1, UserId = 1, Comments = new Collection<Comment>() };

      Posts.Add(post1);
      Posts.Add(post2);

      var posts = Repository.Object.GetFeedEntries(1, 2).ToList();
      Assert.IsNotNull(posts);
      Assert.AreEqual(1, posts.Count);

      var result = posts.First();
      Assert.AreEqual(post1.Id, result.Id);
      Assert.AreEqual(post1.Content, result.Content);
      Assert.AreEqual(post1.CreatedUtc, result.CreatedUtc);
      Assert.AreEqual(user1.Account, result.Account);
      Assert.AreEqual(user1.Name, result.Name);
      Assert.AreEqual(post1.Comments.Count, result.CommentsCount);

      DataContext.VerifyGet(x => x.Posts, Times.Once());
    }

    [TestMethod]
    public void GetMentionsWithInvalidArguments()
    {
      var result = Repository.Object.GetMentions(null, 0).ToList();
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count);

      DataContext.VerifyGet(x => x.Posts, Times.Never());
    }

    [TestMethod]
    public void GetMentions()
    {
      var user1 = new User { Id = 1, Following = new Collection<User>() };
      var post1 = new Post { Id = 1, Content = "post1 for @user2", CreatedUtc = DateTime.UtcNow, User = user1, UserId = 1, Comments = new Collection<Comment>() };
      var post2 = new Post { Id = 2, Content = "post2 for @user2", CreatedUtc = DateTime.UtcNow, User = user1, UserId = 1, Comments = new Collection<Comment>() };

      Posts.Add(post1);
      Posts.Add(post2);

      var posts = Repository.Object.GetMentions("user2", 2).ToList();
      Assert.IsNotNull(posts);
      Assert.AreEqual(1, posts.Count);

      var result = posts.First();
      Assert.AreEqual(post1.Id, result.Id);
      Assert.AreEqual(post1.Content, result.Content);
      Assert.AreEqual(post1.CreatedUtc, result.CreatedUtc);
      Assert.AreEqual(user1.Account, result.Account);
      Assert.AreEqual(user1.Name, result.Name);
      Assert.AreEqual(post1.Comments.Count, result.CommentsCount);

      DataContext.VerifyGet(x => x.Posts, Times.Once());
    }

    [TestMethod]
    public void GetUserProfileWithInvalidArguments()
    {
      var result = Repository.Object.GetUserProfile(null, null);
      Assert.IsNull(result);

      DataContext.VerifyGet(x => x.Users, Times.Never());
    }

    [TestMethod]
    public void GetUserProfile()
    {
      var user = new User
      {
        Id = 1,
        Account = "jdoe",
        Name = "john doe",
        Website = "http://www.google.com",
        Bio = "some bio",
        Posts = new Collection<Post> { new Post(), new Post() },
        Followers = new Collection<User> { new User { Account = "user1" } },
        Following = new Collection<User> { new User { Account = "user2" } }
      };

      Users.Add(user);

      var result = Repository.Object.GetUserProfile("jdoe", "jdoe");
      Assert.IsNotNull(result);
      Assert.AreEqual(user.Account, result.Account);
      Assert.AreEqual(user.Name, result.Name);
      Assert.AreEqual(user.Website, result.Website);
      Assert.AreEqual(user.Bio, result.Bio);
      Assert.AreEqual(user.Followers.Count, result.Followers);
      Assert.AreEqual(user.Following.Count, result.Following);
      Assert.IsFalse(result.IsFollowed);
      Assert.IsTrue(result.IsOwnProfile);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetUserProfilesWithInvalidArguments()
    {
      var result = Repository.Object.GetUserProfiles(0).ToList();
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count);

      DataContext.VerifyGet(x => x.Users, Times.Never());
    }

    [TestMethod]
    public void GetUserProfiles()
    {
      var user = new User
      {
        Id = 1,
        Account = "jdoe",
        Name = "john doe",
        Website = "http://www.google.com",
        Bio = "some bio",
        JoinedUtc = DateTime.UtcNow,
        Posts = new Collection<Post>(),
        Followers = new Collection<User>(),
        Following = new Collection<User>()
      };

      Users.Add(user);

      var results = Repository.Object.GetUserProfiles(2).ToList();
      Assert.IsNotNull(results);
      Assert.AreEqual(1, results.Count);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetUserProfilesWithoutCurrent()
    {
      var user1 = new User
      {
        Id = 1,
        Account = "jdoe",
        Name = "john doe",
        Website = "http://www.google.com",
        Bio = "some bio",
        JoinedUtc = DateTime.UtcNow,
        Posts = new Collection<Post>(),
        Followers = new Collection<User>(),
        Following = new Collection<User>()
      };

      var user2 = new User
      {
        Id = 2,
        Account = "user2",
        Name = "user two",
        Website = "http://www.bing.com",
        Bio = "some bio 2",
        JoinedUtc = DateTime.UtcNow,
        Posts = new Collection<Post>(),
        Followers = new Collection<User>(),
        Following = new Collection<User>()
      };

      Users.Add(user1);
      Users.Add(user2);

      var result = Repository.Object.GetUserProfiles(1).ToList();
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual(user2.Account, result.First().Account);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetUserProfilesWithPage()
    {
      for (var i = 0; i < Constants.UserProfilesPerPage + 1; i++)
      {
        var user = new User
        {
          Id = i + 1,
          Account = "user " + (i + 1),
          Name = "user " + (i + 1),
          Website = "http://www.google.com/user-" + (i + 1),
          Bio = "some bio for user " + (i + 1),
          JoinedUtc = DateTime.UtcNow,
          Posts = new Collection<Post>(),
          Followers = new Collection<User>(),
          Following = new Collection<User>()
        };
        Users.Add(user);
      }
      
      var result = Repository.Object.GetUserProfiles(1000, 1).ToList();
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual("user " + (Constants.UserProfilesPerPage + 1), result.First().Account);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetFollowingsWithInvalidArguments()
    {
      var result = Repository.Object.GetFollowings(0).ToList();
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count);

      DataContext.VerifyGet(x => x.Users, Times.Never());
    }

    [TestMethod]
    public void GetFollowings()
    {
      var user1 = new User { Id = 1, Following = new Collection<User>() };
      var user2 = new User
        {
          Id = 2,
          Account = "jdoe",
          Name = "john doe",
          Website = "http://www.google.com",
          Bio = "some bio",
          JoinedUtc = DateTime.UtcNow,
          Posts = new Collection<Post>(),
          Followers = new Collection<User>(),
          Following = new Collection<User>()
        };

      user1.Following.Add(user2);
      user2.Followers.Add(user1);
      Users.Add(user1);
      Users.Add(user2);

      var result = Repository.Object.GetFollowings(1).ToList();
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);

      var profile = result.First();
      Assert.AreEqual(user2.Account, profile.Account);
      Assert.AreEqual(user2.Name, profile.Name);
      Assert.AreEqual(user2.Website, profile.Website);
      Assert.AreEqual(user2.Bio, profile.Bio);
      Assert.AreEqual(user2.Posts.Count, profile.Posts);
      Assert.AreEqual(user2.Following.Count, profile.Following);
      Assert.AreEqual(user2.Followers.Count, profile.Followers);
      Assert.IsTrue(profile.IsFollowed);
      Assert.IsFalse(profile.IsOwnProfile);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetFollowingsWithPage()
    {
      var user1 = new User { Id = 1000, Following = new Collection<User>() };
      for (var i = 0; i < Constants.UserProfilesPerPage + 1; i++)
      {
        var user = new User
        {
          Id = i + 1,
          Account = "user " + (i + 1),
          Name = "user " + (i + 1),
          Website = "http://www.google.com/user-" + (i + 1),
          Bio = "some bio for user " + (i + 1),
          JoinedUtc = DateTime.UtcNow,
          Posts = new Collection<Post>(),
          Followers = new Collection<User>(),
          Following = new Collection<User>()
        };
        user1.Following.Add(user);
      }

      Users.Add(user1);

      var result = Repository.Object.GetFollowings(1000, 1).ToList();
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual("user " + (Constants.UserProfilesPerPage + 1), result.First().Account);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetFollowersWithInvalidArguments()
    {
      var result = Repository.Object.GetFollowers(0).ToList();
      Assert.IsNotNull(result);
      Assert.AreEqual(0, result.Count);

      DataContext.VerifyGet(x => x.Users, Times.Never());
    }

    [TestMethod]
    public void GetFollowers()
    {
      var user1 = new User { Id = 1, Following = new Collection<User>(), Followers = new Collection<User>() };
      var user2 = new User
      {
        Id = 2,
        Account = "jdoe",
        Name = "john doe",
        Website = "http://www.google.com",
        Bio = "some bio",
        JoinedUtc = DateTime.UtcNow,
        Posts = new Collection<Post>(),
        Followers = new Collection<User>(),
        Following = new Collection<User>()
      };
      
      user1.Followers.Add(user2);
      Users.Add(user1);

      var result = Repository.Object.GetFollowers(1).ToList();
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);

      var profile = result.First();
      Assert.AreEqual(user2.Account, profile.Account);
      Assert.AreEqual(user2.Name, profile.Name);
      Assert.AreEqual(user2.Website, profile.Website);
      Assert.AreEqual(user2.Bio, profile.Bio);
      Assert.AreEqual(user2.Posts.Count, profile.Posts);
      Assert.AreEqual(user2.Following.Count, profile.Following);
      Assert.AreEqual(user2.Followers.Count, profile.Followers);
      Assert.IsFalse(profile.IsFollowed);
      Assert.IsFalse(profile.IsOwnProfile);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetFollowersWithPage()
    {
      var user1 = new User { Id = 1000, Followers = new Collection<User>() };
      for (var i = 0; i < Constants.UserProfilesPerPage + 1; i++)
      {
        var user = new User
        {
          Id = i + 1,
          Account = "user " + (i + 1),
          Name = "user " + (i + 1),
          Website = "http://www.google.com/user-" + (i + 1),
          Bio = "some bio for user " + (i + 1),
          JoinedUtc = DateTime.UtcNow,
          Posts = new Collection<Post>(),
          Followers = new Collection<User>(),
          Following = new Collection<User>()
        };
        user1.Followers.Add(user);
      }

      Users.Add(user1);

      var result = Repository.Object.GetFollowers(1000, 1).ToList();
      Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual("user " + (Constants.UserProfilesPerPage + 1), result.First().Account);

      DataContext.VerifyGet(x => x.Users, Times.Once());
    }

    [TestMethod]
    public void GetUserPictureNormalWithInvalidArguments()
    {
      var defaultPicture = new UserPictureModel { ContentType = "image/png", Data = new byte[0] };
      Repository.Setup(x => x.GetDefaultPicture(It.IsAny<int>())).Returns(defaultPicture);

      var result = Repository.Object.GetUserPictureNormal(null);
      Assert.IsNotNull(result);
      Assert.AreEqual(defaultPicture, result);

      Repository.Verify(x => x.GetDefaultPicture(Constants.PictureSizeNormal), Times.Once());
      DataContext.VerifyGet(x => x.ProfilePictures, Times.Never());
    }

    [TestMethod]
    public void GetUserPictureNormal()
    {
      var user = new User { Account = "jdoe" };
      var picture = new ProfilePicture { ContentType = "image/png", Image = new byte[0], User = user };
      ProfilePictures.Add(picture);

      var result = Repository.Object.GetUserPictureNormal("jdoe");
      Assert.IsNotNull(result);
      Assert.AreEqual(picture.ContentType, result.ContentType);
      Assert.AreEqual(picture.Image, result.Data);

      DataContext.VerifyGet(x => x.ProfilePictures, Times.Once());
    }

    [TestMethod]
    public void GetUserPictureNormalMissing()
    {
      var defaultPicture = new UserPictureModel { ContentType = "image/png", Data = new byte[0] };
      Repository.Setup(x => x.GetDefaultPicture(It.IsAny<int>())).Returns(defaultPicture);

      var result = Repository.Object.GetUserPictureNormal("missing_user");
      Assert.IsNotNull(result);
      Assert.AreEqual(defaultPicture, result);

      Repository.Verify(x => x.GetDefaultPicture(Constants.PictureSizeNormal), Times.Once());
      DataContext.VerifyGet(x => x.ProfilePictures, Times.Once());
    }

    [TestMethod]
    public void GetUserPictureSmallWithInvalidArguments()
    {
      var defaultPicture = new UserPictureModel { ContentType = "image/png", Data = new byte[0] };
      Repository.Setup(x => x.GetDefaultPicture(It.IsAny<int>())).Returns(defaultPicture);

      var result = Repository.Object.GetUserPictureSmall(null);
      Assert.IsNotNull(result);
      Assert.AreEqual(defaultPicture, result);

      Repository.Verify(x => x.GetDefaultPicture(Constants.PictureSizeSmall), Times.Once());
      DataContext.VerifyGet(x => x.ProfilePictures, Times.Never());
    }

    [TestMethod]
    public void GetUserPictureSmall()
    {
      var user = new User { Account = "jdoe" };
      var picture = new ProfilePicture { ContentType = "image/png", ImageSmall = new byte[0], User = user };
      ProfilePictures.Add(picture);

      var result = Repository.Object.GetUserPictureSmall("jdoe");
      Assert.IsNotNull(result);
      Assert.AreEqual(picture.ContentType, result.ContentType);
      Assert.AreEqual(picture.ImageSmall, result.Data);

      DataContext.VerifyGet(x => x.ProfilePictures, Times.Once());
    }

    [TestMethod]
    public void GetUserPictureSmallMissing()
    {
      var defaultPicture = new UserPictureModel { ContentType = "image/png", Data = new byte[0] };
      Repository.Setup(x => x.GetDefaultPicture(It.IsAny<int>())).Returns(defaultPicture);

      var result = Repository.Object.GetUserPictureSmall("missing_user");
      Assert.IsNotNull(result);
      Assert.AreEqual(defaultPicture, result);

      Repository.Verify(x => x.GetDefaultPicture(Constants.PictureSizeSmall), Times.Once());
      DataContext.VerifyGet(x => x.ProfilePictures, Times.Once());
    }

    [TestMethod]
    public void GetUserPictureTinyWithInvalidArguments()
    {
      var defaultPicture = new UserPictureModel { ContentType = "image/png", Data = new byte[0] };
      Repository.Setup(x => x.GetDefaultPicture(It.IsAny<int>())).Returns(defaultPicture);

      var result = Repository.Object.GetUserPictureTiny(null);
      Assert.IsNotNull(result);
      Assert.AreEqual(defaultPicture, result);

      Repository.Verify(x => x.GetDefaultPicture(Constants.PictureSizeTiny), Times.Once());
      DataContext.VerifyGet(x => x.ProfilePictures, Times.Never());
    }

    [TestMethod]
    public void GetUserPictureTiny()
    {
      var user = new User { Account = "jdoe" };
      var picture = new ProfilePicture { ContentType = "image/png", ImageTiny = new byte[0], User = user };
      ProfilePictures.Add(picture);

      var result = Repository.Object.GetUserPictureTiny("jdoe");
      Assert.IsNotNull(result);
      Assert.AreEqual(picture.ContentType, result.ContentType);
      Assert.AreEqual(picture.ImageTiny, result.Data);

      DataContext.VerifyGet(x => x.ProfilePictures, Times.Once());
    }

    [TestMethod]
    public void GetUserPictureTinyMissing()
    {
      var defaultPicture = new UserPictureModel { ContentType = "image/png", Data = new byte[0] };
      Repository.Setup(x => x.GetDefaultPicture(It.IsAny<int>())).Returns(defaultPicture);

      var result = Repository.Object.GetUserPictureTiny("missing_user");
      Assert.IsNotNull(result);
      Assert.AreEqual(defaultPicture, result);

      Repository.Verify(x => x.GetDefaultPicture(Constants.PictureSizeTiny), Times.Once());
      DataContext.VerifyGet(x => x.ProfilePictures, Times.Once());
    }
  }
}
