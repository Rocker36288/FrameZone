using FrameZone_WebApi.DTOs;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// 密碼服務介面
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// 處理忘記密碼請求
        /// </summary>
        /// <param name="request">忘記密碼請求資料</param>
        /// <returns>處理結果</returns>
        Task<ApiResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);

        /// <summary>
        /// 驗證重設密碼 Token 是否有效
        /// </summary>
        /// <param name="token">重設密碼 Token</param>
        /// <returns>是否有效</returns>
        Task<ApiResponseDto> ValidateResetTokenAsync(string token);

        /// <summary>
        /// 重設密碼
        /// </summary>
        /// <param name="request">重設密碼請求資料</param>
        /// <returns>處理結果</returns>
        Task<ApiResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);

        /// <summary>
        /// 變更密碼
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="request">變更密碼請求資料</param>
        /// <returns></returns>
        Task<ApiResponseDto> ChangePasswordAsync(long userId, ChangePasswordRequestDto request);
    }
}
