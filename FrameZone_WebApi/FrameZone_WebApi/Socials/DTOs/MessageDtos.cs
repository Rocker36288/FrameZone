using System;
using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.Socials.DTOs
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public long SenderUserId { get; set; }
        public string MessageContent { get; set; }
        public string MessageType { get; set; }
        public int? StickerId { get; set; }
        public string MediaUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public long? ProductId { get; set; }
        public int? OrderId { get; set; }
        public string LinkTitle { get; set; }
        public string LinkDescription { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SendMessageDto
    {
        [Required(ErrorMessage = "請輸入訊息內容")]
        [MaxLength(1000, ErrorMessage = "訊息內容不能超過1000個字")]
        public string MessageContent { get; set; } = string.Empty;
    }

    public class SendShopMessageDto
    {
        /// <summary>
        /// 支援 text, image, sticker, product, order, link, video
        /// </summary>
        [Required(ErrorMessage = "請填寫訊息類型")]
        [MaxLength(20, ErrorMessage = "訊息類型過長")]
        public string MessageType { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "訊息內容不能超過1000個字")]
        public string MessageContent { get; set; } = string.Empty;

        public long? ProductId { get; set; }

        public int? OrderId { get; set; }

        public int? StickerId { get; set; }

        [MaxLength(500, ErrorMessage = "媒體網址過長")]
        public string MediaUrl { get; set; }

        [MaxLength(500, ErrorMessage = "縮圖網址過長")]
        public string ThumbnailUrl { get; set; }

        [MaxLength(200, ErrorMessage = "連結標題過長")]
        public string LinkTitle { get; set; }

        [MaxLength(1000, ErrorMessage = "連結描述過長")]
        public string LinkDescription { get; set; }
    }
}
