using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.Socials.DTOs
{
    public class PostDto
    {
        [Required(ErrorMessage = "請輸入貼文內容")]
        [MaxLength(500, ErrorMessage = "貼文內容不能超過500個字")]
        public string PostContent { get; set; } = string.Empty;

        public string? PostType { get; set; }

        public int? PostTypeId { get; set; }
    }
}
