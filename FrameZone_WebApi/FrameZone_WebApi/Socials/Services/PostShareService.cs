using FrameZone_WebApi.Models;
using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;

namespace FrameZone_WebApi.Socials.Services
{
    public class PostShareService
    {
        private readonly PostRepository _postRepository;
        private readonly PostShareRepository _postShareRepository;

        public PostShareService(PostRepository postRepository, PostShareRepository postShareRepository)
        {
            _postRepository = postRepository;
            _postShareRepository = postShareRepository;
        }

        public async Task<PostReadDto?> CreateSharePostAsync(long userId, int postId, string? postContent)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("貼文不存在");
            }

            var existing = await _postShareRepository.GetPostShareAsync(userId, postId);
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
            await _postShareRepository.AddPostShareAsync(share);

            var createdWithUser = await _postRepository.GetPostByIdAsync(createdPost.PostId);
            if (createdWithUser == null)
            {
                return null;
            }

            var dto = PostReadMapper.MapToReadDto(createdWithUser, userId);
            dto.SharedPost = PostReadMapper.MapToReadDto(post, userId);
            return dto;
        }

        public async Task<List<PostReadDto>> GetSharedPostsAsync(long userId, int limit)
        {
            var posts = await _postShareRepository.GetSharedPostsAsync(userId, limit);
            return await PostReadMapper.MapPostsWithSharedAsync(_postRepository, posts, userId);
        }
    }
}
