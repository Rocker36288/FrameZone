using FrameZone_WebApi.Models;
using Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FrameZone_WebApi.Repositories.Member
{
    /// <summary>
    /// 會員個人資料 Repository 實作
    /// 負責所有資料庫存取操作
    /// </summary>
    public class MemberProfileRepository : IMemberProfileRepository
    {
        private readonly AAContext _context;
        private IDbContextTransaction? _transaction;

        public MemberProfileRepository(AAContext context)
        {
            _context = context;
        }

        #region 查詢

        /// <summary>
        /// 根據 UserId 取得使用者完整資料（包含 UserProfile 和 UserPrivateInfo）
        /// </summary>
        public async Task<User?> GetUserWithProfileAsync(long userId)
        {
            return await _context.Users
                .Include(u => u.UserProfile)
                .Include(u => u.UserPrivateInfo)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        /// <summary>
        /// 取得或建立 UserProfile
        /// </summary>
        public async Task<UserProfile> GetOrCreateUserProfileAsync(long userId)
        {
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return profile;
        }

        /// <summary>
        /// 取得或建立 UserPrivateInfo
        /// </summary>
        public async Task<UserPrivateInfo> GetOrCreateUserPrivateInfoAsync(long userId)
        {
            var privateInfo = await _context.UserPrivateInfos
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (privateInfo == null)
            {
                privateInfo = new UserPrivateInfo
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.UserPrivateInfos.Add(privateInfo);
                await _context.SaveChangesAsync();
            }

            return privateInfo;
        }

        #endregion

        #region 更新

        /// <summary>
        /// 更新 User 基本資訊
        /// </summary>
        public Task UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.Now;
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 更新 UserProfile
        /// </summary>
        public Task UpdateUserProfileAsync(UserProfile profile)
        {
            profile.UpdatedAt = DateTime.Now;
            _context.UserProfiles.Update(profile);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 更新 UserPrivateInfo
        /// </summary>
        public Task UpdateUserPrivateInfoAsync(UserPrivateInfo privateInfo)
        {
            privateInfo.UpdatedAt = DateTime.Now;
            _context.UserPrivateInfos.Update(privateInfo);
            return Task.CompletedTask;
        }

        #endregion

        #region 儲存與交易

        /// <summary>
        /// 儲存變更
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 開始資料庫交易
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// 提交交易
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// 回滾交易
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        #endregion
    }
}