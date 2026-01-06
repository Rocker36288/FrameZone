using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.DTOs
{
    #region 建立自訂標籤

    /// <summary>
    /// 建立自訂標籤請求 DTO
    /// </summary>
    public class CreateCustomTagRequestDTO
    {
        /// <summary>
        /// 標籤名稱（必填）
        /// </summary>
        [Required(ErrorMessage = "標籤名稱不可為空")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "標籤名稱長度必須在 1-100 字元之間")]
        public string TagName { get; set; }

        /// <summary>
        /// 父標籤 ID（可選，用於建立子標籤）
        /// 例如：「日本」的子標籤可以是「東京」、「大阪」
        /// </summary>
        public int? ParentTagId { get; set; }

        /// <summary>
        /// 分類 ID（可選）
        /// 不提供時的處理邏輯：
        /// - 如果有 ParentTagId，繼承父標籤的分類
        /// - 否則自動放入「用戶自定義」分類
        /// </summary>
        public int? CategoryId { get; set; }
    }

    /// <summary>
    /// 建立自訂標籤回應 DTO
    /// </summary>
    public class CreateCustomTagResponseDTO
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 建立的標籤資料（包含完整的標籤樹節點資訊）
        /// </summary>
        public TagTreeNodeDTO Tag { get; set; }
    }

    #endregion

    #region 批次添加標籤

    /// <summary>
    /// 批次添加標籤請求 DTO
    /// 用於編輯模式下，為多張照片批次添加標籤
    /// </summary>
    public class BatchAddTagsRequestDTO
    {
        /// <summary>
        /// 照片 ID 列表（必填）
        /// 例如：[1, 2, 3, 4, 5]
        /// </summary>
        [Required(ErrorMessage = "照片 ID 列表不可為空")]
        [MinLength(1, ErrorMessage = "至少需要選擇一張照片")]
        public List<long> PhotoIds { get; set; } = new List<long>();

        /// <summary>
        /// 現有標籤 ID 列表（可選）
        /// 從搜尋結果或標籤列表中選擇的標籤
        /// 例如：[10, 20, 30]
        /// </summary>
        public List<int> ExistingTagIds { get; set; } = new List<int>();

        /// <summary>
        /// 新建標籤列表（可選）
        /// 用戶輸入的新標籤，系統會先建立這些標籤，再關聯到照片
        /// </summary>
        public List<NewTagItem> NewTags { get; set; } = new List<NewTagItem>();
    }

    /// <summary>
    /// 新建標籤項目
    /// </summary>
    public class NewTagItem
    {
        /// <summary>
        /// 標籤名稱（必填）
        /// </summary>
        [Required(ErrorMessage = "標籤名稱不可為空")]
        [StringLength(100, ErrorMessage = "標籤名稱不可超過 100 字元")]
        public string TagName { get; set; }

        /// <summary>
        /// 分類 ID（可選）
        /// 不提供時，系統會自動判斷或放入「用戶自定義」分類
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 父標籤 ID（可選）
        /// 用於建立階層式標籤
        /// </summary>
        public int? ParentTagId { get; set; }
    }

    /// <summary>
    /// 批次添加標籤回應 DTO
    /// </summary>
    public class BatchAddTagsResponseDTO
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 總共處理的照片數量
        /// </summary>
        public int TotalPhotos { get; set; }

        /// <summary>
        /// 成功處理的照片數量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失敗的照片數量
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// 新建立的標籤列表
        /// </summary>
        public List<TagTreeNodeDTO> CreatedTags { get; set; } = new List<TagTreeNodeDTO>();

        /// <summary>
        /// 處理結果詳細列表（可選，用於除錯或詳細報告）
        /// </summary>
        public List<BatchAddTagResultItem> Results { get; set; } = new List<BatchAddTagResultItem>();
    }

    /// <summary>
    /// 批次添加標籤結果項目
    /// </summary>
    public class BatchAddTagResultItem
    {
        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 錯誤訊息（如果失敗）
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 成功添加的標籤數量
        /// </summary>
        public int TagsAdded { get; set; }
    }

    #endregion

    #region 移除標籤

    /// <summary>
    /// 移除標籤回應 DTO
    /// </summary>
    public class RemoveTagResponseDTO
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 被移除的標籤 ID
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// 被移除的標籤名稱
        /// </summary>
        public string TagName { get; set; }
    }

    #endregion

    #region 搜尋標籤

    /// <summary>
    /// 搜尋標籤請求 DTO
    /// </summary>
    public class SearchTagsRequestDTO
    {
        /// <summary>
        /// 搜尋關鍵字（必填）
        /// 支援模糊搜尋，最少 1 個字元
        /// </summary>
        [Required(ErrorMessage = "搜尋關鍵字不可為空")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "搜尋關鍵字長度必須在 1-100 字元之間")]
        public string Keyword { get; set; }

        /// <summary>
        /// 是否包含系統標籤（預設：true）
        /// </summary>
        public bool IncludeSystemTags { get; set; } = true;

        /// <summary>
        /// 是否包含用戶自定義標籤（預設：true）
        /// </summary>
        public bool IncludeUserTags { get; set; } = true;

        /// <summary>
        /// 限制返回數量（預設：20，最大：100）
        /// </summary>
        [Range(1, 100, ErrorMessage = "返回數量必須在 1-100 之間")]
        public int Limit { get; set; } = 20;

        /// <summary>
        /// 指定分類 ID（可選）
        /// 只搜尋特定分類下的標籤
        /// </summary>
        public int? CategoryId { get; set; }
    }

    /// <summary>
    /// 搜尋標籤回應 DTO
    /// </summary>
    public class SearchTagsResponseDTO
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 標籤列表
        /// </summary>
        public List<TagItemDTO> Tags { get; set; } = new List<TagItemDTO>();

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// 標籤項目 DTO（用於搜尋結果、列表顯示）
    /// </summary>
    public class TagItemDTO
    {
        /// <summary>
        /// 標籤 ID
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// 標籤名稱
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 標籤類型
        /// 可選值：SYSTEM（系統）、USER（用戶）、CUSTOM（自定義）
        /// </summary>
        public string TagType { get; set; }

        /// <summary>
        /// 所屬分類 ID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 所屬分類名稱
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 父標籤 ID（可選）
        /// </summary>
        public int? ParentTagId { get; set; }

        /// <summary>
        /// 父標籤名稱（可選）
        /// </summary>
        public string ParentTagName { get; set; }

        /// <summary>
        /// 該標籤下的照片數量
        /// </summary>
        public int PhotoCount { get; set; }

        /// <summary>
        /// 顯示順序
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 是否為使用者建立
        /// </summary>
        public bool IsUserCreated { get; set; }
    }

    #endregion

    #region 照片標籤詳細資訊

    /// <summary>
    /// 照片標籤詳細資訊 DTO
    /// 用於顯示單張照片的所有標籤，並按來源分類
    /// </summary>
    public class PhotoTagsDetailDTO
    {
        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 所有標籤（合併後的完整列表）
        /// </summary>
        public List<PhotoTagItemDTO> AllTags { get; set; } = new List<PhotoTagItemDTO>();

        /// <summary>
        /// EXIF 自動標籤（來源：EXIF）
        /// </summary>
        public List<PhotoTagItemDTO> ExifTags { get; set; } = new List<PhotoTagItemDTO>();

        /// <summary>
        /// 地理編碼標籤（來源：GEOCODING）
        /// </summary>
        public List<PhotoTagItemDTO> GeocodingTags { get; set; } = new List<PhotoTagItemDTO>();

        /// <summary>
        /// 用戶手動標籤（來源：MANUAL）
        /// </summary>
        public List<PhotoTagItemDTO> ManualTags { get; set; } = new List<PhotoTagItemDTO>();

        /// <summary>
        /// AI 識別標籤（來源：AI）
        /// </summary>
        public List<PhotoTagItemDTO> AiTags { get; set; } = new List<PhotoTagItemDTO>();

        /// <summary>
        /// 標籤總數
        /// </summary>
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// 照片標籤項目 DTO
    /// </summary>
    public class PhotoTagItemDTO
    {
        /// <summary>
        /// 標籤 ID
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// 標籤名稱
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 標籤類型
        /// </summary>
        public string TagType { get; set; }

        /// <summary>
        /// 所屬分類名稱
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 來源 ID
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// 來源名稱（EXIF、MANUAL、AI、GEOCODING）
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        /// 信心度（AI 標籤專用，0-100）
        /// </summary>
        public decimal? Confidence { get; set; }

        /// <summary>
        /// 添加時間
        /// </summary>
        public DateTime AddedAt { get; set; }

        /// <summary>
        /// 是否可移除（MANUAL 來源的標籤可移除）
        /// </summary>
        public bool CanRemove { get; set; }
    }

    #endregion

    #region 分類列表

    /// <summary>
    /// 可用分類列表回應 DTO
    /// </summary>
    public class AvailableCategoriesResponseDTO
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 系統分類列表
        /// </summary>
        public List<CategoryItemDTO> SystemCategories { get; set; } = new List<CategoryItemDTO>();

        /// <summary>
        /// 用戶自定義分類列表
        /// </summary>
        public List<CategoryItemDTO> UserCategories { get; set; } = new List<CategoryItemDTO>();
    }

    /// <summary>
    /// 分類項目 DTO
    /// </summary>
    public class CategoryItemDTO
    {
        /// <summary>
        /// 分類 ID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 分類名稱
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 分類代碼
        /// </summary>
        public string CategoryCode { get; set; }

        /// <summary>
        /// 是否為用戶自定義
        /// </summary>
        public bool IsUserDefined { get; set; }

        /// <summary>
        /// 該分類下的標籤數量
        /// </summary>
        public int TagCount { get; set; }

        /// <summary>
        /// 顯示順序
        /// </summary>
        public int DisplayOrder { get; set; }
    }

    #endregion
}