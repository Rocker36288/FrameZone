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
        public async Task<List<Post>> GetPostsAsync()
        {
            return await _postRepository.GetPostsAsync();
        }

        // ================= 取得貼文 =================
        public async Task<Post?> GetPostByIdAsync(int postId)
        {
            return await _postRepository.GetPostByIdAsync(postId);
        }

        // ================= 新增貼文 =================
        public async Task<Post?> CreatePostAsync(PostDto dto, long userId)
        {
            var post = new Post
            {
                UserId = userId,
                PostType = dto.PostType ?? "default",
                PostTypeId = dto.PostTypeId,
                PostContent = dto.PostContent,                
            };
            return await _postRepository.AddPostAsync(post);
        }

        // ================= 編輯貼文 =================
        public async Task<Post> EditPostAsync(long userId, int postId, PostDto dto)
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

            return updatedPost;
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


    }
}
