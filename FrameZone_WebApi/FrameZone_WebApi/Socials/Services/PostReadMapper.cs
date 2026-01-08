using FrameZone_WebApi.Models;
using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;
using System.Linq;

namespace FrameZone_WebApi.Socials.Services
{
    public static class PostReadMapper
    {
        public static async Task<List<PostReadDto>> MapPostsWithSharedAsync(
            PostRepository postRepository,
            List<Post> posts,
            long? currentUserId)
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

            var sharedPosts = await postRepository.GetPostsByIdsAsync(sharedSourceIds);
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

        public static PostReadDto MapToReadDto(Post post, long? currentUserId)
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
