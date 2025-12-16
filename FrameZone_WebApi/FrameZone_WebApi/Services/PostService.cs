using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;

namespace FrameZone_WebApi.Services
{
    public class PostService
    {
        private readonly PostRepository _postRepository;
        public PostService(PostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        // ================= 取得貼文 =================
        public async Task<Post?> GetPostAsync(int postId)
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
        public async Task<Post?> EditPostAsync(int postId, PostDto dto)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);

            //貼文不存在
            if (post == null) 
            {   
                return null;
            }

            post.PostContent = dto.PostContent;
            post.PostType = dto.PostType ?? post.PostType;
            post.PostTypeId = dto.PostTypeId ?? post.PostTypeId;

            return await _postRepository.UpdatePostAsync(post);
        }

        // ================= 刪除貼文 =================
        public async Task<bool> DeletePostAsync(int postId)
        {
            var post = await _postRepository.GetPostByIdAsync(postId);

            //貼文不存在
            if (post == null)
            {
                return false;
            }

            return await _postRepository.DeletePostAsync(post);
        }       


    }
}
