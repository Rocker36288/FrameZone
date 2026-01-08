using FrameZone_WebApi.Models;
using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;

namespace FrameZone_WebApi.Socials.Services
{
    public class PostService
    {
        private readonly PostRepository _postRepository;
        public PostService(PostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        // ================= 取得多筆貼文 =================
        public async Task<List<PostReadDto>> GetPostsAsync(long? currentUserId)
        {
            var posts = await _postRepository.GetPostsAsync();
            if (posts == null)
            {
                return null;
            }

            return await PostReadMapper.MapPostsWithSharedAsync(_postRepository, posts, currentUserId);
        }

        // ================= 取得指定使用者貼文 =================
        public async Task<List<PostReadDto>> GetPostsByUserIdAsync(long userId, long? currentUserId)
        {
            var posts = await _postRepository.GetPostsByUserIdAsync(userId);
            if (posts == null)
            {
                return null;
            }

            return await PostReadMapper.MapPostsWithSharedAsync(_postRepository, posts, currentUserId);
        }

        // ================= 取得貼文 =================
        public async Task<PostReadDto?> GetPostByIdAsync(int postId, long? currentUserId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                return null;
            }

            var dto = PostReadMapper.MapToReadDto(post, currentUserId);
            if (post.PostType == "share" && post.PostTypeId.HasValue)
            {
                var shared = await _postRepository.GetPostByIdAsync(post.PostTypeId.Value);
                if (shared != null)
                {
                    dto.SharedPost = PostReadMapper.MapToReadDto(shared, currentUserId);
                }
            }
            return dto;
        }

        // ================= 取得使用者資料 =================
        public async Task<UserProfileSummaryDto?> GetUserProfileSummaryAsync(long userId)
        {
            var user = await _postRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var followerCount = await _postRepository.GetFollowerCountAsync(userId);
            var followingCount = await _postRepository.GetFollowingCountAsync(userId);
            return new UserProfileSummaryDto
            {
                UserId = user.UserId,
                DisplayName = user.UserProfile?.DisplayName ?? user.Account ?? "使用者",
                Avatar = user.UserProfile?.Avatar,
                FollowerCount = followerCount,
                FollowingCount = followingCount
            };
        }

        // ================= 新增貼文 =================
        public async Task<PostReadDto?> CreatePostAsync(PostDto dto, long userId)
        {
            var post = new Post
            {
                UserId = userId,
                PostType = dto.PostType ?? "default",
                PostTypeId = dto.PostTypeId,
                PostContent = dto.PostContent,
            };
            var created = await _postRepository.AddPostAsync(post);
            if (created == null)
            {
                return null;
            }

            var createdWithUser = await _postRepository.GetPostByIdAsync(created.PostId);
            return createdWithUser == null ? null : PostReadMapper.MapToReadDto(createdWithUser, userId);
        }

        // ================= 編輯貼文 =================
        public async Task<PostReadDto> EditPostAsync(long userId, int postId, PostDto dto)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);

            //貼文不存在
            if (post == null)
            {
                throw new KeyNotFoundException("貼文不存在");
            }

            if (post.UserId != userId)
            {
                throw new UnauthorizedAccessException("無權修改他人貼文");
            }

            post.PostContent = dto.PostContent;
            post.PostType = dto.PostType ?? post.PostType;
            post.PostTypeId = dto.PostTypeId ?? post.PostTypeId;

            var updatedPost = await _postRepository.UpdatePostAsync(post);
            if (updatedPost == null)
            {
                throw new InvalidOperationException("編輯貼文失敗");
            }

            return PostReadMapper.MapToReadDto(post, userId);
        }

        // ================= 刪除貼文 =================
        public async Task DeletePostAsync(long userId, int postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);

            //貼文不存在
            if (post == null)
            {
                throw new KeyNotFoundException("貼文不存在或已刪除");
            }

            if (post.UserId != userId)
            {
                throw new UnauthorizedAccessException("無權刪除他人貼文");
            }

            var success = await _postRepository.DeletePostAsync(post);
            if (!success)
            {
                throw new InvalidOperationException("刪除貼文失敗");
            }
        }

        public async Task<List<PostReadDto>> GetCommentedPostsAsync(long userId, int limit)
        {
            var posts = await _postRepository.GetCommentedPostsAsync(userId, limit);
            return await PostReadMapper.MapPostsWithSharedAsync(_postRepository, posts, userId);
        }
    }
}
