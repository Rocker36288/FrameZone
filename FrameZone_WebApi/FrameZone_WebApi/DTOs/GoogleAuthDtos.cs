using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.DTOs
{
    public class GoogleAuthDtos
    {
        // =========== Google 登入相關 DTO ===========

        /// <summary>
        /// Google 登入請求 DTO
        /// </summary>
        public class GoogleLoginRequestDto
        {
            /// <summary>
            /// Google ID Token (由前端 Google Sign-In 取得)
            /// </summary>
            [Required(ErrorMessage = "Google Token 不能為空")]
            public string IdToken { get; set; } = string.Empty;

            /// <summary>
            /// 是否記住我
            /// </summary>
            public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Google 登入回應 DTO
        /// </summary>
        public class GoogleLoginResponseDto
        {
            /// <summary>
            /// 操作是否成功
            /// </summary>
            public bool Success { get; set; }

            /// <summary>
            /// 回應訊息
            /// </summary>
            public string Message { get; set; } = string.Empty;

            /// <summary>
            /// JWT Token
            /// </summary>
            public string? Token { get; set; }

            /// <summary>
            /// 使用者ID
            /// </summary>
            public long? UserId { get; set; }

            /// <summary>
            /// 帳號
            /// </summary>
            public string? Account { get; set; }

            /// <summary>
            /// Email
            /// </summary>
            public string? Email { get; set; }

            /// <summary>
            /// 顯示名稱
            /// </summary>
            public string? DisplayName { get; set; }

            /// <summary>
            /// 頭像 URL
            /// </summary>
            public string? Avatar { get; set; }

            /// <summary>
            /// 是否為新註冊使用者
            /// </summary>
            public bool IsNewUser { get; set; }
        }

        /// <summary>
        /// Google 使用者資訊 DTO
        /// </summary>
        public class GoogleUserInfoDto
        {
            /// <summary>
            /// Google 使用者唯一識別碼
            /// </summary>
            public string Sub { get; set; } = string.Empty;

            /// <summary>
            /// Email
            /// </summary>
            public string Email { get; set; } = string.Empty;

            /// <summary>
            /// Email 是否已驗證
            /// </summary>
            public bool EmailVerified { get; set; }

            /// <summary>
            /// 使用者全名
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// 名字
            /// </summary>
            public string GivenName { get; set; } = string.Empty;

            /// <summary>
            /// 姓氏
            /// </summary>
            public string FamilyName { get; set; } = string.Empty;

            /// <summary>
            /// 頭像 URL
            /// </summary>
            public string Picture { get; set; } = string.Empty;

            /// <summary>
            /// 地區設定
            /// </summary>
            public string Locale { get; set; } = string.Empty;
        }

        /// <summary>
        /// Google Token 驗證回應 DTO
        /// </summary>
        public class GoogleTokenValidationDto
        {
            /// <summary>
            /// Audience (應該等於 ClientId)
            /// </summary>
            public string Aud { get; set; } = string.Empty;

            /// <summary>
            /// Google 使用者 ID
            /// </summary>
            public string Sub { get; set; } = string.Empty;

            /// <summary>
            /// Email
            /// </summary>
            public string Email { get; set; } = string.Empty;

            /// <summary>
            /// Email 是否已驗證
            /// </summary>
            public string Email_Verified { get; set; } = string.Empty;

            /// <summary>
            /// Token 過期時間 (Unix timestamp)
            /// </summary>
            public string Exp { get; set; } = string.Empty;

            /// <summary>
            /// Token 發行時間 (Unix timestamp)
            /// </summary>
            public string Iat { get; set; } = string.Empty;
        }

        /// <summary>
        /// 帳號綁定請求 DTO
        /// </summary>
        public class LinkGoogleAccountRequestDto
        {
            /// <summary>
            /// Google ID Token
            /// </summary>
            [Required(ErrorMessage = "Google Token 不能為空")]
            public string IdToken { get; set; } = string.Empty;
        }

        /// <summary>
        /// 帳號解除綁定請求 DTO
        /// </summary>
        public class UnlinkGoogleAccountRequestDto
        {
            /// <summary>
            /// 當前密碼 (安全驗證)
            /// </summary>
            [Required(ErrorMessage = "請輸入當前密碼")]
            public string CurrentPassword { get; set; } = string.Empty;
        }

    }
}
