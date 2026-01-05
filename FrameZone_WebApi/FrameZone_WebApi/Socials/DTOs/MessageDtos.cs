namespace FrameZone_WebApi.Socials.DTOs
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public long SenderUserId { get; set; }
        public string MessageContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
