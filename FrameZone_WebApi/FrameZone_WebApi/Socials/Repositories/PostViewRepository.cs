using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Socials.Repositories
{
    public class PostViewRepository
    {
        private readonly AAContext _context;

        public PostViewRepository(AAContext context)
        {
            _context = context;
        }

        public async Task<PostView?> GetPostViewAsync(long userId, int postId)
        {
            return await _context.PostViews
                .Where(pv => pv.UserId == userId && pv.PostId == postId)
                .OrderByDescending(pv => pv.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<PostView?> AddPostViewAsync(PostView view)
        {
            view.CreatedAt = DateTime.UtcNow;
            await _context.PostViews.AddAsync(view);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? view : null;
        }

        public async Task<PostView?> UpdatePostViewAsync(PostView view)
        {
            view.CreatedAt = DateTime.UtcNow;
            _context.PostViews.Update(view);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? view : null;
        }

        public async Task<List<Post>> GetRecentViewedPostsAsync(long userId, int limit)
        {
            var recent = await _context.PostViews
                .Where(pv => pv.UserId == userId)
                .GroupBy(pv => pv.PostId)
                .Select(g => new { PostId = g.Key, LastViewedAt = g.Max(x => x.CreatedAt) })
                .OrderByDescending(x => x.LastViewedAt)
                .Take(limit)
                .ToListAsync();

            if (recent.Count == 0)
            {
                return new List<Post>();
            }

            var postIds = recent.Select(x => x.PostId).ToList();
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

            return posts
                .OrderByDescending(p => p.UpdatedAt)
                .ToList();
        }
    }
}
