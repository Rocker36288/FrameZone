using FrameZone_WebApi.Models;
using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;

namespace FrameZone_WebApi.Socials.Services
{
    public class PostViewService
    {
        private readonly PostRepository _postRepository;
        private readonly PostViewRepository _postViewRepository;

        public PostViewService(PostRepository postRepository, PostViewRepository postViewRepository)
        {
            _postRepository = postRepository;
            _postViewRepository = postViewRepository;
        }

        public async Task<bool> RecordPostViewAsync(long userId, int postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("貼文不存在");
            }

            var existing = await _postViewRepository.GetPostViewAsync(userId, postId);
            if (existing != null)
            {
                var updated = await _postViewRepository.UpdatePostViewAsync(existing);
                return updated != null;
            }

            var view = new PostView
            {
                UserId = userId,
                PostId = postId
            };
            var created = await _postViewRepository.AddPostViewAsync(view);
            return created != null;
        }

        public async Task<List<PostReadDto>> GetRecentViewedPostsAsync(long userId, int limit)
        {
            var posts = await _postViewRepository.GetRecentViewedPostsAsync(userId, limit);
            return await PostReadMapper.MapPostsWithSharedAsync(_postRepository, posts, userId);
        }
    }
}
