using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.Socials.DTOs
{
    public class SharePostDto
    {
        [MaxLength(500, ErrorMessage = "貼文內容不能超過500個字")]
        public string? PostContent { get; set; }
    }
}
