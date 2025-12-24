using System;
using System.Collections.Generic;

namespace FrameZone_WebApi.DTOs
{
    #region 分類相關 DTO

    /// <summary>
    /// 自動標籤資訊（包含分類資訊）
    /// </summary>
    public class AutoTagInfo
    {
        /// <summary>
        /// 標籤名稱
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// 標籤類型
        /// </summary>
        public string TagType { get; set; } = "SYSTEM";

        /// <summary>
        /// 所屬分類代碼
        /// </summary>
        public string CategoryCode { get; set; }

        /// <summary>
        /// 父標籤名稱（用於建立層級關係）
        /// </summary>
        public string ParentTagName { get; set; }
    }

    /// <summary>
    /// 分類樹節點 DTO（用於前端 Sidebar 顯示）
    /// </summary>
    public class CategoryTreeNodeDTO
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
        /// 分類類型 ID
        /// </summary>
        public int CategoryTypeId { get; set; }

        /// <summary>
        /// 父分類 ID
        /// </summary>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// 照片數量
        /// </summary>
        public int PhotoCount { get; set; }

        /// <summary>
        /// 是否預設展開
        /// </summary>
        public bool IsDefaultExpanded { get; set; }

        /// <summary>
        /// 是否即將推出（Coming Soon）
        /// </summary>
        public bool IsComingSoon { get; set; }

        /// <summary>
        /// UI 顯示類型 (tree/flat)
        /// </summary>
        public string UiType { get; set; }

        /// <summary>
        /// 顯示順序
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 子分類列表
        /// </summary>
        public List<CategoryTreeNodeDTO> Children { get; set; } = new List<CategoryTreeNodeDTO>();
    }

    /// <summary>
    /// 標籤樹節點 DTO（用於顯示標籤階層）
    /// </summary>
    public class TagTreeNodeDTO
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
        /// 所屬分類 ID
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 父標籤 ID
        /// </summary>
        public int? ParentTagId { get; set; }

        /// <summary>
        /// 照片數量
        /// </summary>
        public int PhotoCount { get; set; }

        /// <summary>
        /// 顯示順序
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 子標籤列表
        /// </summary>
        public List<TagTreeNodeDTO> Children { get; set; } = new List<TagTreeNodeDTO>();
    }

    /// <summary>
    /// 完整的分類樹回應 DTO
    /// </summary>
    public class CategoryTreeResponseDTO
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 分類樹列表
        /// </summary>
        public List<CategoryTreeNodeDTO> Categories { get; set; } = new List<CategoryTreeNodeDTO>();

        /// <summary>
        /// 總照片數量
        /// </summary>
        public int TotalPhotoCount { get; set; }
    }

    /// <summary>
    /// 分類與其標籤 DTO (用於 Sidebar 顯示)
    /// </summary>
    public class CategoryWithTagsDTO
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
        /// 圖示名稱 (前端用於顯示 icon)
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 顯示順序
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 是否預設展開
        /// </summary>
        public bool IsDefaultExpanded { get; set; }

        /// <summary>
        /// UI 顯示類型 (hierarchical/flat)
        /// </summary>
        public string UiType { get; set; }

        /// <summary>
        /// 是否即將推出
        /// </summary>
        public bool IsComingSoon { get; set; }

        /// <summary>
        /// 該分類下的所有標籤（已建立階層結構）
        /// </summary>
        public List<TagTreeNodeDTO> Tags { get; set; } = new List<TagTreeNodeDTO>();
    }

    /// <summary>
    /// 標籤階層回應 DTO
    /// </summary>
    public class TagHierarchyResponseDTO
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
        /// 分類與標籤列表
        /// </summary>
        public List<CategoryWithTagsDTO> Categories { get; set; } = new List<CategoryWithTagsDTO>();

        /// <summary>
        /// 總照片數量
        /// </summary>
        public int TotalPhotoCount { get; set; }
    }

    #endregion

}