using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using FrameZone_WebApi.Helpers;

namespace FrameZone_WebApi.DTOs.Member
{
    #region 取得個人資料

    /// <summary>
    /// 取得個人資料回應 DTO
    /// </summary>
    public class GetProfileResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 個人資料
        /// </summary>
        public UserProfileDto? Data { get; set; }
    }

    /// <summary>
    /// 個人資料 DTO（回傳用）
    /// Avatar 和 CoverImage 存儲 Azure Blob Storage 的 URL
    /// </summary>
    public class UserProfileDto
    {
        #region User 基本資訊

        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 電話號碼
        /// </summary>
        public string? Phone { get; set; }

        #endregion

        #region UserProfile 公開資訊

        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 大頭貼 URL（Azure Blob Storage）
        /// 例如：https://yourstore.blob.core.windows.net/avatars/avatar_123_20251226.jpg
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// 封面圖片 URL（Azure Blob Storage）
        /// 例如：https://yourstore.blob.core.windows.net/covers/cover_123_20251226.jpg
        /// </summary>
        public string? CoverImage { get; set; }

        /// <summary>
        /// 個人簡介
        /// </summary>
        public string? Bio { get; set; }

        /// <summary>
        /// 個人網站
        /// </summary>
        public string? Website { get; set; }

        /// <summary>
        /// 所在地
        /// </summary>
        public string? Location { get; set; }

        #endregion

        #region UserPrivateInfo 私密資訊

        /// <summary>
        /// 真實姓名
        /// </summary>
        public string? RealName { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateOnly? BirthDate { get; set; }

        /// <summary>
        /// 國家
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// 郵遞區號
        /// </summary>
        public string? PostalCode { get; set; }

        /// <summary>
        /// 完整地址
        /// </summary>
        public string? FullAddress { get; set; }

        #endregion

        #region 輔助方法

        /// <summary>
        /// 從資料庫實體轉換為 DTO
        /// </summary>
        public static UserProfileDto FromEntity(
            Models.User user,
            Models.UserProfile? profile,
            Models.UserPrivateInfo? privateInfo)
        {
            return new UserProfileDto
            {
                // User 基本資訊
                UserId = user.UserId,
                Email = user.Email,
                Phone = user.Phone,

                // UserProfile 公開資訊（直接使用資料庫的 URL）
                DisplayName = profile?.DisplayName,
                Avatar = profile?.Avatar,
                CoverImage = profile?.CoverImage,
                Bio = profile?.Bio,
                Website = profile?.Website,
                Location = profile?.Location,

                // UserPrivateInfo 私密資訊
                RealName = privateInfo?.RealName,
                Gender = privateInfo?.Gender,
                BirthDate = privateInfo?.BirthDate,
                Country = privateInfo?.Country,
                City = privateInfo?.City,
                PostalCode = privateInfo?.PostalCode,
                FullAddress = privateInfo?.FullAddress
            };
        }

        #endregion
    }

    #endregion

    #region 更新個人資料

    /// <summary>
    /// 更新個人資料回應 DTO
    /// </summary>
    public class UpdateProfileResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 更新後的個人資料（包含 SAS URL 的圖片連結）
        /// </summary>
        public UserProfileDto? Data { get; set; }

        /// <summary>
        /// 驗證錯誤列表（如果有）
        /// </summary>
        public List<string>? Errors { get; set; }
    }

    /// <summary>
    /// 更新個人資料 DTO
    /// 所有驗證規則使用 MemberConstants 常數
    /// Data Annotations 驗證由 ASP.NET 自動執行
    /// 業務邏輯驗證在 Service 層執行
    /// </summary>
    public class UpdateUserProfileDto
    {
        #region User 基本資訊

        /// <summary>
        /// 電話號碼
        /// </summary>
        [MaxLength(MemberConstants.PHONE_MAX_LENGTH,
            ErrorMessage = "電話號碼長度不能超過 {1} 個字元")]
        [RegularExpression(MemberConstants.PHONE_REGEX,
            ErrorMessage = "電話號碼格式不正確")]
        public string? Phone { get; set; }

        #endregion

        #region UserProfile 公開資訊

        /// <summary>
        /// 顯示名稱
        /// </summary>
        [MaxLength(MemberConstants.DISPLAY_NAME_MAX_LENGTH,
            ErrorMessage = "顯示名稱長度不能超過 {1} 個字元")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 個人簡介
        /// </summary>
        [MaxLength(MemberConstants.BIO_MAX_LENGTH,
            ErrorMessage = "個人簡介長度不能超過 {1} 個字元")]
        public string? Bio { get; set; }

        /// <summary>
        /// 個人網站
        /// </summary>
        [MaxLength(MemberConstants.WEBSITE_MAX_LENGTH,
            ErrorMessage = "網站 URL 長度不能超過 {1} 個字元")]
        [RegularExpression(MemberConstants.URL_REGEX,
            ErrorMessage = "網站 URL 格式不正確，必須以 http:// 或 https:// 開頭")]
        public string? Website { get; set; }

        /// <summary>
        /// 所在地
        /// </summary>
        [MaxLength(MemberConstants.LOCATION_MAX_LENGTH,
            ErrorMessage = "所在地長度不能超過 {1} 個字元")]
        public string? Location { get; set; }

        #endregion

        #region UserPrivateInfo 私密資訊

        /// <summary>
        /// 真實姓名
        /// </summary>
        [MaxLength(MemberConstants.REAL_NAME_MAX_LENGTH,
            ErrorMessage = "真實姓名長度不能超過 {1} 個字元")]
        public string? RealName { get; set; }

        /// <summary>
        /// 性別
        /// </summary>
        [MaxLength(20)]
        public string? Gender { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateOnly? BirthDate { get; set; }

        /// <summary>
        /// 國家
        /// </summary>
        [MaxLength(MemberConstants.COUNTRY_MAX_LENGTH,
            ErrorMessage = "國家名稱長度不能超過 {1} 個字元")]
        public string? Country { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [MaxLength(MemberConstants.CITY_MAX_LENGTH,
            ErrorMessage = "城市名稱長度不能超過 {1} 個字元")]
        public string? City { get; set; }

        /// <summary>
        /// 郵遞區號
        /// </summary>
        [MaxLength(MemberConstants.POSTAL_CODE_MAX_LENGTH,
            ErrorMessage = "郵遞區號長度不能超過 {1} 個字元")]
        public string? PostalCode { get; set; }

        /// <summary>
        /// 完整地址
        /// </summary>
        [MaxLength(MemberConstants.FULL_ADDRESS_MAX_LENGTH,
            ErrorMessage = "完整地址長度不能超過 {1} 個字元")]
        public string? FullAddress { get; set; }

        #endregion

        #region 圖片檔案上傳

        /// <summary>
        /// 大頭貼檔案（上傳用）
        /// 檔案將上傳到 Azure Blob Storage，URL 存入資料庫
        /// </summary>
        public IFormFile? AvatarFile { get; set; }

        /// <summary>
        /// 封面圖片檔案（上傳用）
        /// 檔案將上傳到 Azure Blob Storage，URL 存入資料庫
        /// </summary>
        public IFormFile? CoverImageFile { get; set; }

        #endregion

        #region 圖片移除標記

        /// <summary>
        /// 是否移除大頭貼
        /// </summary>
        public bool RemoveAvatar { get; set; } = false;

        /// <summary>
        /// 是否移除封面圖片
        /// </summary>
        public bool RemoveCoverImage { get; set; } = false;

        #endregion
    }

    #endregion
}