using FrameZone_WebApi.Models;
using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;

namespace FrameZone_WebApi.Socials.Services
{
    public class PostLikeService
    {
        private readonly PostRepository _postRepository;
        private readonly PostLikeRepository _postLikeRepository;

        public PostLikeService(PostRepository postRepository, PostLikeRepository postLikeRepository)
        {
            _postRepository = postRepository;
            _postLikeRepository = postLikeRepository;
        }

        public async Task<bool> AddLikeAsync(long userId, int postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("貼文不存在");
            }

            var existing = await _postLikeRepository.GetPostLikeAsync(userId, postId);
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

            var saved = await _postLikeRepository.AddPostLikeAsync(like);
            return saved != null;
        }

        public async Task<bool> RemoveLikeAsync(long userId, int postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("貼文不存在");
            }

            var existing = await _postLikeRepository.GetPostLikeAsync(userId, postId);
            if (existing == null)
            {
                return false;
            }

            return await _postLikeRepository.RemovePostLikeAsync(existing);
        }

        public async Task<List<PostReadDto>> GetLikedPostsAsync(long userId, int limit)
        {
            var posts = await _postLikeRepository.GetLikedPostsAsync(userId, limit);
            return await PostReadMapper.MapPostsWithSharedAsync(_postRepository, posts, userId);
        }
    }
}
