using FrameZone_WebApi.DTOs;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// Email 服務介面
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// 發送通用 Email
        /// </summary>
        /// <param name="emailDto">Email 內容</param>
        /// <returns>是否發送成功</returns>
        Task<bool> SendEmailAsync(EmailDto emailDto);

        /// <summary>
        /// 發送驗證碼 Email
        /// </summary>
        /// <param name="verificationCode">驗證碼 Email 內容</param>
        /// <returns>是否發送成功</returns>
        Task<bool> SendVerificationCodeEmailAsync(VerificationEmailDto verificationEmailDto);

        /// <summary>
        /// 發送重設密碼 Email
        /// </summary>
        /// <param name="resetPasswordEmailDto">重設密碼 Email 內容</param>
        /// <returns>是否發送成功</returns>
        Task<bool> SendResetPasswordEmailAsync(ResetPasswordEmailDto resetPasswordEmailDto);

    }
}
