using FrameZone_WebApi.DTOs.Member;

namespace FrameZone_WebApi.Services.Member
{
    /// <summary>
    /// 會員個人資料服務介面
    /// </summary>
    public interface IMemberProfileService
    {
        /// <summary>
        /// 取得使用者的個人資料（不含圖片）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>個人資料 Response DTO</returns>
        Task<GetProfileResponseDto> GetProfileAsync(long userId);

        /// <summary>
        /// 更新使用者的個人資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="dto">更新資料 DTO</param>
        /// <returns>更新結果 Response DTO</returns>
        Task<UpdateProfileResponseDto> UpdateProfileAsync(long userId, UpdateUserProfileDto dto);
    }
}