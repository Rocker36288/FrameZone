using FrameZone_WebApi.Models;
using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;
using System.Linq;

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

            return await MapPostsWithSharedAsync(posts, currentUserId);
        }

        // ================= 取得指定使用者貼文 =================
        public async Task<List<PostReadDto>> GetPostsByUserIdAsync(long userId, long? currentUserId)
        {
            var posts = await _postRepository.GetPostsByUserIdAsync(userId);
            if (posts == null)
            {
                return null;
            }

            return await MapPostsWithSharedAsync(posts, currentUserId);
        }

        // ================= 取得貼文 =================
        public async Task<PostReadDto?> GetPostByIdAsync(int postId, long? currentUserId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                return null;
            }

            var dto = MapToReadDto(post, currentUserId);
            if (post.PostType == "share" && post.PostTypeId.HasValue)
            {
                var shared = await _postRepository.GetPostByIdAsync(post.PostTypeId.Value);
                if (shared != null)
                {
                    dto.SharedPost = MapToReadDto(shared, currentUserId);
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
            return createdWithUser == null ? null : MapToReadDto(createdWithUser, userId);
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

            return MapToReadDto(post, userId);
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

        public async Task<bool> AddLikeAsync(long userId, int postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("貼文不存在");
            }

            var existing = await _postRepository.GetPostLikeAsync(userId, postId);
            if (existing != null)
            {
                return false;
            }

            var like = new PostLike
            {
                UserId = userId,
                PostId = postId,
                CreatedAt = DateTime.UtcNow
            };

            var saved = await _postRepository.AddPostLikeAsync(like);
            return saved != null;
        }

        public async Task<bool> RemoveLikeAsync(long userId, int postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("貼文不存在");
            }

            var existing = await _postRepository.GetPostLikeAsync(userId, postId);
            if (existing == null)
            {
                return false;
            }

            return await _postRepository.RemovePostLikeAsync(existing);
        }

        public async Task<PostReadDto?> CreateSharePostAsync(long userId, int postId, string? postContent)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("貼文不存在");
            }

            var existing = await _postRepository.GetPostShareAsync(userId, postId);
            if (existing != null)
            {
                return null;
            }

            var sharePost = new Post
            {
                UserId = userId,
                PostType = "share",
                PostTypeId = postId,
                PostContent = postContent ?? string.Empty
            };

            var createdPost = await _postRepository.AddPostAsync(sharePost);
            if (createdPost == null)
            {
                return null;
            }

            var share = new PostShare
            {
                UserId = userId,
                PostId = postId
            };
            await _postRepository.AddPostShareAsync(share);

            var createdWithUser = await _postRepository.GetPostByIdAsync(createdPost.PostId);
            if (createdWithUser == null)
            {
                return null;
            }

            var dto = MapToReadDto(createdWithUser, userId);
            dto.SharedPost = MapToReadDto(post, userId);
            return dto;
        }

        public async Task<List<PostReadDto>> GetLikedPostsAsync(long userId, int limit)
        {
            var posts = await _postRepository.GetLikedPostsAsync(userId, limit);
            return await MapPostsWithSharedAsync(posts, userId);
        }

        public async Task<List<PostReadDto>> GetCommentedPostsAsync(long userId, int limit)
        {
            var posts = await _postRepository.GetCommentedPostsAsync(userId, limit);
            return await MapPostsWithSharedAsync(posts, userId);
        }

        public async Task<bool> RecordPostViewAsync(long userId, int postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("貼文不存在");
            }

            var existing = await _postRepository.GetPostViewAsync(userId, postId);
            if (existing != null)
            {
                var updated = await _postRepository.UpdatePostViewAsync(existing);
                return updated != null;
            }

            var view = new PostView
            {
                UserId = userId,
                PostId = postId
            };
            var created = await _postRepository.AddPostViewAsync(view);
            return created != null;
        }

        public async Task<List<PostReadDto>> GetRecentViewedPostsAsync(long userId, int limit)
        {
            var posts = await _postRepository.GetRecentViewedPostsAsync(userId, limit);
            return await MapPostsWithSharedAsync(posts, userId);
        }

        public async Task<List<PostReadDto>> GetSharedPostsAsync(long userId, int limit)
        {
            var posts = await _postRepository.GetSharedPostsAsync(userId, limit);
            return await MapPostsWithSharedAsync(posts, userId);
        }

        private async Task<List<PostReadDto>> MapPostsWithSharedAsync(List<Post> posts, long? currentUserId)
        {
            var dtos = posts.Select(p => MapToReadDto(p, currentUserId)).ToList();
            var sharedSourceIds = posts
                .Where(p => p.PostType == "share" && p.PostTypeId.HasValue)
                .Select(p => p.PostTypeId!.Value)
                .Distinct()
                .ToList();

            if (sharedSourceIds.Count == 0)
            {
                return dtos;
            }

            var sharedPosts = await _postRepository.GetPostsByIdsAsync(sharedSourceIds);
            var sharedMap = sharedPosts.ToDictionary(p => p.PostId, p => MapToReadDto(p, currentUserId));

            foreach (var dto in dtos)
            {
                if (dto.PostType == "share" && dto.PostTypeId.HasValue &&
                    sharedMap.TryGetValue(dto.PostTypeId.Value, out var sharedDto))
                {
                    dto.SharedPost = sharedDto;
                }
            }

            return dtos;
        }

        private static PostReadDto MapToReadDto(Post post, long? currentUserId)
        {
            var likeCount = post.PostLikes?.Count ?? 0;
            var isLiked = currentUserId.HasValue && post.PostLikes.Any(l => l.UserId == currentUserId.Value);
            var shareCount = post.PostShares?.Count ?? 0;
            var isShared = currentUserId.HasValue && post.PostShares.Any(s => s.UserId == currentUserId.Value);
            var commentCount = post.CommentTargets?
                .SelectMany(ct => ct.Comments)
                .Count(c => c.DeletedAt == null) ?? 0;
            return new PostReadDto
            {
                PostId = post.PostId,
                UserId = post.UserId,
                UserName = post.User?.UserProfile?.DisplayName ?? "新使用者",
                Avatar = post.User?.UserProfile?.Avatar,
                PostContent = post.PostContent,
                PostType = post.PostType,
                PostTypeId = post.PostTypeId,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                IsOwner = currentUserId.HasValue && post.UserId == currentUserId.Value,
                LikeCount = likeCount,
                IsLiked = isLiked,
                ShareCount = shareCount,
                IsShared = isShared,
                IsSharedPost = post.PostType == "share",
                CommentCount = commentCount,
                SharedPost = null
            };
        }
    }
}
