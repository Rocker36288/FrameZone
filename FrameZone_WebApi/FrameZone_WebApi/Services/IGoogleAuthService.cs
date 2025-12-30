using FrameZone_WebApi.DTOs;
using static FrameZone_WebApi.DTOs.GoogleAuthDtos;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// Google 第三方登入服務介面
    /// </summary>
    public interface IGoogleAuthService
    {
        /// <summary>
        /// 使用 Google 登入
        /// </summary>
        /// <param name="request">Google 登入請求</param>
        /// <returns>登入結果</returns>
        Task<GoogleLoginResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request);

        /// <summary>
        /// 驗證 Google ID Token
        /// </summary>
        /// <param name="idToken">Google ID Token</param>
        /// <returns>Google 使用者資訊</returns>
        Task<GoogleUserInfoDto?> ValidateGoogleTokenAsync(string idToken);

        /// <summary>
        /// 綁定 Google 帳號到現有使用者
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="request">綁定請求</param>
        /// <returns>處理結果</returns>
        Task<ApiResponseDto> LinkGoogleAccountAsync(long userId, LinkGoogleAccountRequestDto request);

        /// <summary>
        /// 解除 Google 帳號綁定
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="request">解除綁定請求</param>
        /// <returns>處理結果</returns>
        Task<ApiResponseDto> UnlinkGoogleAccountAsync(long userId, UnlinkGoogleAccountRequestDto request);

        /// <summary>
        /// 檢查使用者是否已綁定 Google 帳號
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>是否已綁定</returns>
        Task<bool> IsGoogleAccountLinkedAsync(long userId);
    }
}