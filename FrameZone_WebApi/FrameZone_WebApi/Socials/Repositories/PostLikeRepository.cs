using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Socials.Repositories
{
    public class PostLikeRepository
    {
        private readonly AAContext _context;

        public PostLikeRepository(AAContext context)
        {
            _context = context;
        }

        public async Task<PostLike?> GetPostLikeAsync(long userId, int postId)
        {
            return await _context.PostLikes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
        }

        public async Task<PostLike?> AddPostLikeAsync(PostLike like)
        {
            await _context.PostLikes.AddAsync(like);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? like : null;
        }

        public async Task<bool> RemovePostLikeAsync(PostLike like)
        {
            _context.PostLikes.Remove(like);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Post>> GetLikedPostsAsync(long userId, int limit)
        {
            var liked = await _context.PostLikes
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(limit)
                .Select(l => new { l.PostId, l.CreatedAt })
                .ToListAsync();

            if (liked.Count == 0)
            {
                return new List<Post>();
            }

            var postIds = liked.Select(x => x.PostId).ToList();
            var posts = await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(p => p.PostLikes)
                .Include(p => p.PostShares)
                .Include(p => p.CommentTargets)
                    .ThenInclude(ct => ct.Comments)
                .Where(p =>
                    postIds.Contains(p.PostId) &&
                    p.Status != "Deleted" &&
                    p.DeletedAt == null)
                .ToListAsync();

            var orderMap = liked
                .Select((item, index) => new { item.PostId, index })
                .ToDictionary(x => x.PostId, x => x.index);

            return posts
                .OrderBy(p => orderMap.ContainsKey(p.PostId) ? orderMap[p.PostId] : int.MaxValue)
                .ToList();
        }
    }
}
