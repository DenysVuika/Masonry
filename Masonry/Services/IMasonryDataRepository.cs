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
using Masonry.Data.Model;
using Masonry.Models;

namespace Masonry.Services
{
  public interface IMasonryDataRepository
  {
    User FindUser(string account);
    User FindUser(int accountId);
    IEnumerable<CommentModel> GetComments(int postId);
    CommentModel CreateComment(int userId, int postId, string content);
    bool DeletePost(int accountId, int postId);
    PostModel CreatePost(int userId, string content);
    PostModel GetPost(int postId);
    bool Follow(int userId, string account);
    bool Unfollow(int userId, string account);

    IEnumerable<PostModel> GetTimelineEntries(int userId, int top);
    int CheckTimelineUpdates(int userId, int top);
    IEnumerable<PostModel> GetTimelineUpdates(int userId, int top);
    IEnumerable<PostModel> GetFeedEntries(int userId, int top);
    IEnumerable<PostModel> GetMentions(string account, int top);
    //UserProfileModel GetUserProfile(int userId, string account);
    UserProfileModel GetUserProfile(string currentUserAccount, string account);
    IEnumerable<UserProfileModel> GetUserProfiles(int userId, int page = 0);
    IEnumerable<UserProfileModel> GetFollowings(int userId, int page = 0);
    IEnumerable<UserProfileModel> GetFollowers(int userId, int page = 0);

    UserPictureModel GetUserPictureNormal(string account);
    UserPictureModel GetUserPictureSmall(string account);
    UserPictureModel GetUserPictureTiny(string account);

    bool UpdateAccountSettings(int userId, string name, string website, string location, string bio, byte[] picture);
  }
}
