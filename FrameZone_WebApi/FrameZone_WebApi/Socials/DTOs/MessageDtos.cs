using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.Socials.DTOs
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public long SenderUserId { get; set; }
        public string MessageContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SendMessageDto
    {
        [Required(ErrorMessage = "請輸入訊息內容")]
        [MaxLength(1000, ErrorMessage = "訊息內容不能超過1000個字")]
        public string MessageContent { get; set; } = string.Empty;
    }
}
