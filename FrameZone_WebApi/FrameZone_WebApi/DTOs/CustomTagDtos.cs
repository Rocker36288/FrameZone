using System;

namespace FrameZone_WebApi.DTOs
{
    /// <summary>
    /// 建立自訂標籤請求 DTO
    /// </summary>
    public class CreateCustomTagRequestDTO
    {
        /// <summary>
        /// 標籤名稱
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 父標籤 ID（可選，用於建立子標籤）
        /// </summary>
        public int? ParentTagId { get; set; }
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
        /// 建立的標籤資料
        /// </summary>
        public TagTreeNodeDTO Tag { get; set; }
    }
}