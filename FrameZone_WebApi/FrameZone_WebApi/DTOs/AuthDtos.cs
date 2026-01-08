using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.DTOs
{
    // =========== 登入相關 ========== 

    /// <summary>
    /// 登入請求 DTO
    /// </summary>
    public class LoginRequestDto
    {
        // 帳號或Email
        [Required(ErrorMessage = "請輸入帳號或Email")]
        [MinLength(3, ErrorMessage = "請輸入至少3個字")]
        public string AccountOrEmail { get; set; } = string.Empty;

        // 密碼
        [Required(ErrorMessage = "請輸入密碼")]
        [MinLength(6, ErrorMessage = "密碼至少需要6個字")]
        public string Password { get; set; } = string.Empty;

        // 記住我
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// 登入回應 DTO
    /// </summary>
    public class LoginResponseDto
    {
        // 操作是否成功
        public bool Success { get; set; } 

        // 回應訊息
        public string Message { get; set; } = string.Empty;

        //JWT Token
        public string? Token { get; set; }

        // 使用者ID
        public long? UserId { get; set; }

        // 帳號
        public string? Account { get; set; }

        // Email
        public string? Email { get; set; }

        // 顯示名稱
        public string? DisplayName { get; set; }

        // 頭像 URL
        public string? Avatar { get; set; }
    }

    /// <summary>
    /// 註冊請求 DTO
    /// </summary>
    public class RegisterRequestDto
    {
        // 帳號
        [Required(ErrorMessage = "請輸入帳號")]
        [MinLength(3, ErrorMessage = "帳號至少需要 3 個字")]
        [MaxLength(50, ErrorMessage = "帳號不能超過 50 個字")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "帳號只能包含英文字母、數字和底線")]
        public string Account { get; set; } = string.Empty;

        // Email
        [Required(ErrorMessage = "請輸入Email")]
        [EmailAddress(ErrorMessage = "Email格式不正確")]
        public string Email { get; set; } = string.Empty;

        // 密碼
        [Required(ErrorMessage = "請輸入密碼")]
        [MinLength(6, ErrorMessage = "密碼至少需要 6 個字")]
        [MaxLength(50, ErrorMessage = "密碼至少超過 50 個字")]
        public string Password { get; set; } = string.Empty;

        // 確認密碼
        [Required(ErrorMessage = "請輸入確認密碼")]
        [Compare("Password", ErrorMessage = "密碼與確認密碼不相符")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // 手機號碼
        [Phone(ErrorMessage = "手機號碼格式不正確")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "請輸入正確的台灣手機號碼格式（09開頭共10碼）")]
        public string? Phone { get; set; }
    }

    /// <summary>
    /// 註冊回應 DTO
    /// </summary>
    public class RegisterResponseDto
    {
        // 操作是否成功
        public bool Success { get; set; }

        // 回應訊息
        public string Message { get; set; } = string.Empty;

        // 新建立的使用者ID
        public long? UserId { get; set; }
    }

    /// <summary>
    /// 忘記密碼請求 DTO
    /// </summary>
    public class ForgotPasswordRequestDto
    {
        // Email
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// 重設密碼請求 DTO
    /// </summary>
    public class ResetPasswordRequestDto
    {
        // 重設密碼Token
        public string Token { get; set; } = string.Empty;

        // 新密碼
        public string NewPassword { get; set; } = string.Empty;
        
        // 確認新密碼
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// 變更密碼請求 DTO
    /// </summary>
    public class ChangePasswordRequestDto
    {
        // 目前密碼
        public string CurrentPassword { get; set; } = string.Empty;

        // 新密碼
        public string NewPassword { get; set; } = string.Empty;

        // 確認新密碼
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// 通用回應DTO
    /// </summary>
    public class ApiResponseDto
    {
        // 操作是否成功
        public bool Success { get; set; }

        // 回應訊息
        public string Message { get; set; } = string.Empty;
    }
}
