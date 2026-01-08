using FrameZone_WebApi.Models;
using FrameZone_WebApi.Shopping.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Shopping.Services
{
    public interface IShoppingMemberService
    {
        Task<ShoppingMemberProfileResponse> GetProfileAsync(long userId);
        Task<ShoppingMemberProfileResponse> UpdateProfileAsync(long userId, ShoppingMemberProfileDto dto);
    }

    public class ShoppingMemberService : IShoppingMemberService
    {
        private readonly AAContext _context;

        public ShoppingMemberService(AAContext context)
        {
            _context = context;
        }

        public async Task<ShoppingMemberProfileResponse> GetProfileAsync(long userId)
        {
            var user = await _context.Users
                .Include(u => u.UserProfile)
                .Include(u => u.UserPrivateInfo)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return new ShoppingMemberProfileResponse
                {
                    Success = false,
                    Message = "找不到使用者"
                };
            }

            return new ShoppingMemberProfileResponse
            {
                Success = true,
                Data = new ShoppingMemberProfileDto
                {
                    UserId = user.UserId,
                    Account = user.Account,
                    Email = user.Email,
                    Phone = user.Phone,
                    DisplayName = user.UserProfile?.DisplayName,
                    Avatar = user.UserProfile?.Avatar,
                    RealName = user.UserPrivateInfo?.RealName,
                    Gender = user.UserPrivateInfo?.Gender,
                    BirthDate = user.UserPrivateInfo?.BirthDate
                }
            };
        }

        public async Task<ShoppingMemberProfileResponse> UpdateProfileAsync(long userId, ShoppingMemberProfileDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserProfile)
                .Include(u => u.UserPrivateInfo)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return new ShoppingMemberProfileResponse
                {
                    Success = false,
                    Message = "找不到使用者"
                };
            }

            // 更新 User 基本欄位
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.UpdatedAt = DateTime.Now;

            // 更新 UserProfile
            if (user.UserProfile == null)
            {
                user.UserProfile = new UserProfile { UserId = userId, CreatedAt = DateTime.Now };
                _context.UserProfiles.Add(user.UserProfile);
            }
            user.UserProfile.DisplayName = dto.DisplayName;
            user.UserProfile.Avatar = dto.Avatar;
            user.UserProfile.UpdatedAt = DateTime.Now;

            // 更新 UserPrivateInfo
            if (user.UserPrivateInfo == null)
            {
                user.UserPrivateInfo = new UserPrivateInfo { UserId = userId, CreatedAt = DateTime.Now };
                _context.UserPrivateInfos.Add(user.UserPrivateInfo);
            }
            user.UserPrivateInfo.RealName = dto.RealName;
            user.UserPrivateInfo.Gender = dto.Gender;
            user.UserPrivateInfo.BirthDate = dto.BirthDate;
            user.UserPrivateInfo.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new ShoppingMemberProfileResponse
            {
                Success = true,
                Message = "基本資料更新成功",
                Data = dto
            };
        }
    }
}
