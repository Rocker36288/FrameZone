using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.Socials.DTOs
{
    // =========== 貼文相關 ========== 

    /// <summary>
    /// 貼文 DTO，可用於建立或編輯貼文
    /// </summary>
    public class PostDto
    {
        // 貼文文字內容
        [Required(ErrorMessage = "請輸入貼文內容")]
        [MaxLength(500, ErrorMessage = "貼文內容不能超過500個字")]
        public string PostContent { get; set; } = string.Empty;

        // 貼文種類: 社團 / 活動 / 個人
        public string? PostType { get; set; }

        // 貼文種類 Id: 對應社團Id / 活動Id / 個人(null)
        public int? PostTypeId { get; set; }
    }
}
