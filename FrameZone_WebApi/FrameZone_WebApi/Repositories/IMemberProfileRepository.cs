using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Repositories.Member
{
    /// <summary>
    /// 會員個人資料 Repository 介面
    /// 負責所有資料庫存取操作
    /// </summary>
    public interface IMemberProfileRepository
    {
        /// <summary>
        /// 根據 UserId 取得使用者完整資料（包含 UserProfile 和 UserPrivateInfo）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>使用者實體</returns>
        Task<User?> GetUserWithProfileAsync(long userId);

        /// <summary>
        /// 取得或建立 UserProfile
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>UserProfile 實體</returns>
        Task<UserProfile> GetOrCreateUserProfileAsync(long userId);

        /// <summary>
        /// 取得或建立 UserPrivateInfo
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>UserPrivateInfo 實體</returns>
        Task<UserPrivateInfo> GetOrCreateUserPrivateInfoAsync(long userId);

        /// <summary>
        /// 更新 User 基本資訊
        /// </summary>
        /// <param name="user">使用者實體</param>
        Task UpdateUserAsync(User user);

        /// <summary>
        /// 更新 UserProfile
        /// </summary>
        /// <param name="profile">UserProfile 實體</param>
        Task UpdateUserProfileAsync(UserProfile profile);

        /// <summary>
        /// 更新 UserPrivateInfo
        /// </summary>
        /// <param name="privateInfo">UserPrivateInfo 實體</param>
        Task UpdateUserPrivateInfoAsync(UserPrivateInfo privateInfo);

        /// <summary>
        /// 儲存變更（SaveChanges）
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// 開始資料庫交易
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// 提交交易
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// 回滾交易
        /// </summary>
        Task RollbackTransactionAsync();
    }
}