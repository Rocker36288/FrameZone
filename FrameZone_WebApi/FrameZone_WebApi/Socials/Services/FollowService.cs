using FrameZone_WebApi.Models;
using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;

namespace FrameZone_WebApi.Socials.Services
{
    public class FollowService
    {
        private readonly FollowRepository _followRepository;

        public FollowService(FollowRepository followRepository)
        {
            _followRepository = followRepository;
        }

        public async Task<FollowResponseDto?> AddFriendAsync(long followerId, long followingId)
        {
            if (followerId == followingId)
            {
                throw new InvalidOperationException("不能新增自己為好友");
            }

            var existing = await _followRepository.GetFollowAsync(followerId, followingId);
            Follow? saved;

            if (existing != null)
            {
                if (existing.DeleteAt == null)
                {
                    throw new InvalidOperationException("已是好友");
                }

                existing.DeleteAt = null;
                existing.CreatedAt = DateTime.UtcNow;
                saved = await _followRepository.UpdateFollowAsync(existing);
            }
            else
            {
                var follow = new Follow
                {
                    FollowerId = followerId,
                    FollowingId = followingId,
                    CreatedAt = DateTime.UtcNow,
                    DeleteAt = null
                };
                saved = await _followRepository.AddFollowAsync(follow);
            }

            if (saved == null)
            {
                return null;
            }

            return new FollowResponseDto
            {
                FollowerId = saved.FollowerId,
                FollowingId = saved.FollowingId,
                CreatedAt = saved.CreatedAt
            };
        }

        public async Task<List<FollowUserDto>> GetFollowingAsync(long userId)
        {
            var users = await _followRepository.GetFollowingUsersAsync(userId) ?? new List<User>();
            return users.Select(u => new FollowUserDto
            {
                UserId = u.UserId,
                DisplayName = u.UserProfile?.DisplayName ?? u.Account ?? "使用者",
                Avatar = u.UserProfile?.Avatar
            }).ToList();
        }

        public async Task<List<FollowUserDto>> GetFollowersAsync(long userId)
        {
            var users = await _followRepository.GetFollowerUsersAsync(userId) ?? new List<User>();
            return users.Select(u => new FollowUserDto
            {
                UserId = u.UserId,
                DisplayName = u.UserProfile?.DisplayName ?? u.Account ?? "使用者",
                Avatar = u.UserProfile?.Avatar
            }).ToList();
        }
    }
}
