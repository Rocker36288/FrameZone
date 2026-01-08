using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FrameZone_WebApi.Socials.Repositories
{
    public class FollowRepository
    {
        private readonly AAContext _context;
        public FollowRepository(AAContext context)
        {
            _context = context;
        }

        public async Task<Follow?> GetFollowAsync(long followerId, long followingId)
        {
            return await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<Follow?> AddFollowAsync(Follow follow)
        {
            await _context.Follows.AddAsync(follow);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? follow : null;
        }

        public async Task<Follow?> UpdateFollowAsync(Follow follow)
        {
            _context.Follows.Update(follow);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? follow : null;
        }

        public async Task<List<User>?> GetFollowingUsersAsync(long userId)
        {
            return await _context.Follows
                .Where(f => f.FollowerId == userId && f.DeleteAt == null)
                .Include(f => f.Following)
                    .ThenInclude(u => u.UserProfile)
                .Select(f => f.Following)
                .ToListAsync();
        }

        public async Task<List<User>?> GetFollowerUsersAsync(long userId)
        {
            return await _context.Follows
                .Where(f => f.FollowingId == userId && f.DeleteAt == null)
                .Include(f => f.Follower)
                    .ThenInclude(u => u.UserProfile)
                .Select(f => f.Follower)
                .ToListAsync();
        }
    }
}
