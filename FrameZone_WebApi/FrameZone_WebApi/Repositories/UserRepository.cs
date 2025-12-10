using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Repositories
{
    public class UserRepository
    {
        private readonly AAContext _context;

        public UserRepository(AAContext context)
        {
            _context = context;
        }

        // ========== 查詢使用者相關 ==========

        /// <summary>
        /// 根據帳號或Email查詢使用者
        /// </summary>
        /// <param name="accountOrEmail">帳號或Email</param>
        /// <returns>使用者物件，找不到則回傳 null</returns>
        public async Task<User?> GetUserByAccountOrEmailAsync(string accountOrEmail)
        {
            return await _context.Users
                .Include(u => u.UserProfile)
                .Where(u => u.Account == accountOrEmail || u.Email == accountOrEmail)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根據使用者ID查詢使用者
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <returns>使用者物件，找不到則回傳 null</returns>
        public async Task<User?> GetUserByIdAsync(long userId)
        {
            return await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        /// <summary>
        /// 帳號是否已存在
        /// </summary>
        /// <param name="account">帳號</param>
        /// <returns>存在回傳 true，不存在回傳 false</returns>
        public async Task<bool> IsAccountExistsAsync(string account)
        {
            return await _context.Users.AnyAsync(u => u.Account == account);
        }

        /// <summary>
        /// Email 是否已存在
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>存在回傳 true，不存在回傳false</returns>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        // ========== 新增使用者相關 ==========

        /// <summary>
        /// 新增使用者
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <returns>新增成功回傳 true，失敗回傳 false</returns>
        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                // 設定建立時間與更新時間
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                // 將使用者加入到DbSet
                await _context.Users.AddAsync(user);

                // 儲存變更到資料庫
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception)
            {
                // Log exception (not implemented here)
                return false;
            }
        }

        /// <summary>
        /// 新增使用者檔案
        /// </summary>
        /// <param name="userProfile">使用者檔案物件</param>
        /// <returns>新增成功回傳 true，失敗回傳 false</returns>
        public async Task<bool> CreateUserProfileAsync(UserProfile userProfile)
        {
            try
            {
                // 設定建立時間與更新時間
                userProfile.CreatedAt = DateTime.UtcNow;
                userProfile.UpdatedAt = DateTime.UtcNow;

                // 將使用者檔案加入到DbSet
                await _context.UserProfiles.AddAsync(userProfile);

                // 儲存變更到資料庫
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception)
            {
                // Log exception (not implemented here)
                return false;
            }
        }

        /// <summary>
        /// 更新使用者資料
        /// </summary>
        /// <param name="user">使用者物件</param>
        /// <returns>更新成功回傳 true，失敗回傳 false</returns>
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                // 更新時間
                user.UpdatedAt = DateTime.UtcNow;

                // 更新使用者資料
                _context.Users.Update(user);

                // 儲存變更到資料庫
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception)
            {
                // Log exception (not implemented here)
                return false;
            }
        }

        /// <summary>
        /// 更新使用者檔案
        /// </summary>
        /// <param name="userProfile">使用者檔案物件</param>
        /// <returns>更新成功回傳 true，失敗會傳 false</returns>
        public async Task<bool> UpdateUserProfileAsync(UserProfile userProfile)
        {
            try
            {
                // 更新時間
                userProfile.UpdatedAt = DateTime.UtcNow;
                // 更新使用者檔案資料
                _context.UserProfiles.Update(userProfile);
                // 儲存變更到資料庫
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception)
            {
                // Log exception (not implemented here)
                return false;
            }
        }

        // ========== 安全相關 ==========

        /// <summary>
        /// 紀錄登入失敗次數
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <returns>更新成功回傳 true，失敗回傳 false</returns>
        public async Task<bool> IncrementFailedLoginAttemptsAsync(long userId)
        {
            try
            {
                var securityStatus = await _context.UserSecurityStatuses
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (securityStatus == null)
                {
                    securityStatus = new UserSecurityStatus
                    {
                        UserId = userId,
                        FailedLoginAttempts = 1,
                        LastFailedLoginAt = DateTime.UtcNow,
                        IsLocked = false,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _context.UserSecurityStatuses.AddAsync(securityStatus);
                }
                else
                {
                    securityStatus.FailedLoginAttempts += 1;
                    securityStatus.LastFailedLoginAt = DateTime.UtcNow;
                    securityStatus.UpdatedAt = DateTime.UtcNow;

                    if (securityStatus.FailedLoginAttempts >= 5)
                    {
                        securityStatus.IsLocked = true;
                        securityStatus.LockedAt = DateTime.UtcNow;
                        securityStatus.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                        securityStatus.LockedReason = "連續登入失敗超過 5 次";
                    }

                    _context.UserSecurityStatuses.Update(securityStatus);
                }
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception)
            {
                // Log exception (not implemented here)
                return false;
            }
        }

        /// <summary>
        /// 重設登入失敗次數 (登入成功時呼叫)
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <returns>更新成功回傳 true，失敗回傳 false</returns>
        public async Task<bool> ResetFailedLoginAttemptsAsync(long userId)
        {
            try
            {
                var securityStatus = await _context.UserSecurityStatuses
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (securityStatus != null)
                {
                    // 重設失敗次數
                    securityStatus.FailedLoginAttempts = 0;
                    securityStatus.LastFailedLoginAt = null;
                    securityStatus.UpdatedAt = DateTime.UtcNow;

                    _context.UserSecurityStatuses.Update(securityStatus);
                    var result = await _context.SaveChangesAsync();
                    return result > 0;
                }
                return true; // 如果沒有安全狀態，表示沒有失敗次數需要重設
            }
            catch (Exception)
            {
                // Log exception (not implemented here)
                return false;
            }
        }

        /// <summary>
        /// 檢查帳號是否被鎖定
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <returns>被鎖定回傳 true，否則回傳 false</returns>
        public async Task<bool> IsAccountLockedAsync(long userId)
        {
            var securityStatus = await _context.UserSecurityStatuses
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (securityStatus == null)
            {
                return false;
            }

            if (securityStatus.IsLocked && securityStatus.LockedUntil.HasValue)
            {
                if (DateTime.UtcNow > securityStatus.LockedUntil.Value)
                {
                    // 鎖定時間已過，解除鎖定
                    securityStatus.IsLocked = false;
                    securityStatus.LockedAt = null;
                    securityStatus.LockedUntil = null;
                    securityStatus.LockedReason = null;
                    securityStatus.FailedLoginAttempts = 0;
                    securityStatus.UpdatedAt = DateTime.UtcNow;
                    
                    _context.UserSecurityStatuses.Update(securityStatus);
                    await _context.SaveChangesAsync();

                    return false; // 帳號已解除鎖定
                }
                return true; // 還在鎖定期間

            }
            return false; // 帳號未被鎖定
        }

        // ========= 日誌相關 ==========
        /// <summary>
        /// 紀錄使用者操作日誌
        /// </summary>
        /// <param name="userLog">日誌物件</param>
        /// <returns>紀錄成功回傳 true，失敗回傳 false</returns>
        public async Task<bool> CreateUserLogAsync(UserLog userLog)
        {
            try
            {
                userLog.CreatedAt = DateTime.UtcNow;
                await _context.UserLogs.AddAsync(userLog);

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception)
            {
                // Log exception (not implemented here)
                return false;
            }
        }

        /// <summary>
        /// 建立使用者會話
        /// </summary>
        /// <param name="userSession">會話物件</param>
        /// <returns>建立成功回傳 true，失敗回傳 false</returns>
        public async Task<bool> CreateUserSessionAsync(UserSession userSession)
        {
            try
            {
                userSession.CreatedAt = DateTime.UtcNow;
                await _context.UserSessions.AddAsync(userSession);

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception)
            {
                // Log exception (not implemented here)
                return false;
            }
        }

        // ========= 驗證相關 ==========

        /// <summary>
        /// 根據 Email 查詢使用者
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>使用者物件，找不到回傳 null</returns>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// 建立驗證紀錄
        /// </summary>
        /// <param name="verification">驗證紀錄物件</param>
        /// <returns>建立成功回傳 true，失敗回傳 false</returns>
        public async Task<bool> CreateVerificationAsync(UserVerification verification)
        {
            try
            {
                // 設定建立和更新時間
                verification.CreatedAt = DateTime.UtcNow;
                verification.UpdatedAt = DateTime.UtcNow;

                // 加入到資料庫
                await _context.UserVerifications.AddAsync(verification);

                // 儲存變更
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 根據 Token 查詢驗證紀錄
        /// </summary>
        /// <param name="token">驗證 Token</param>
        /// <returns>驗證紀錄物件，找不到則回傳 null</returns>
        public async Task<UserVerification?> GetVerificationByTokenAsync(string token)
        {
            return await _context.UserVerifications
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.VerificationToken == token);
        }

        /// <summary>
        /// 更新驗證紀錄
        /// </summary>
        /// <param name="verification">驗證紀錄物件</param>
        /// <returns>更新成功回傳 true，失敗回傳 false</returns>
        public async Task<bool> UpdateVerificationAsync(UserVerification verification)
        {
            try
            {
                // 更新時間
                verification.UpdatedAt = DateTime.UtcNow;

                // 更新資料
                _context.UserVerifications.Update(verification);

                // 儲存變更
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 標記驗證紀錄為已使用
        /// </summary>
        /// <param name="verificationId">驗證ID</param>
        /// <returns>更新成功回傳 true，失敗回傳 false</returns>
        public async Task<bool> MarkVerificationAsUsedAsync(long verificationId)
        {
            try
            {
                var verification = await _context.UserVerifications
                    .FirstOrDefaultAsync(v => v.VerificationId == verificationId);

                if (verification == null)
                {
                    return false;
                }

                verification.IsUsed = true;
                verification.IsVerified = true;
                verification.VerifiedAt = DateTime.UtcNow;
                verification.UpdatedAt = DateTime.UtcNow;

                _context.UserVerifications.Update(verification);

                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
