using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Socials.Repositories
{
    public class PostShareRepository
    {
        private readonly AAContext _context;

        public PostShareRepository(AAContext context)
        {
            _context = context;
        }

        public async Task<PostShare?> GetPostShareAsync(long userId, int postId)
        {
            return await _context.PostShares
                .FirstOrDefaultAsync(s => s.UserId == userId && s.PostId == postId);
        }

        public async Task<PostShare?> AddPostShareAsync(PostShare share)
        {
            share.CreatedAt = DateTime.UtcNow;
            await _context.PostShares.AddAsync(share);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? share : null;
        }

        public async Task<List<Post>> GetSharedPostsAsync(long userId, int limit)
        {
            return await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(p => p.PostLikes)
                .Include(p => p.PostShares)
                .Include(p => p.CommentTargets)
                    .ThenInclude(ct => ct.Comments)
                .Where(p =>
                    p.UserId == userId &&
                    p.PostType == "share" &&
                    p.Status != "Deleted" &&
                    p.DeletedAt == null)
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<int>> GetSharedPostIdsByUserIdAsync(long userId)
        {
            return await _context.PostShares
                .Where(s => s.UserId == userId)
                .Select(s => s.PostId)
                .Distinct()
                .ToListAsync();
        }
    }
}
