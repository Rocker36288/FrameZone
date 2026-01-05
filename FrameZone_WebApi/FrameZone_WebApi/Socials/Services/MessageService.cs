using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;

namespace FrameZone_WebApi.Socials.Services
{
    public class MessageService
    {
        private readonly MessageRepository _messageRepo;

        public MessageService(MessageRepository messageRepo)
        {
            _messageRepo = messageRepo;
        }

        /// <summary>
        /// 取得聊天室訊息（轉成 DTO）
        /// </summary>
        public List<MessageDto> GetMessages(int roomId)
        {
            return _messageRepo.GetRoomMessages(roomId)
                .Select(m => new MessageDto
                {
                    MessageId = m.MessageId,
                    SenderUserId = m.SenderUserId,
                    MessageContent = m.MessageContent,
                    CreatedAt = m.CreatedAt
                })
                .ToList();
        }

        /// <summary>
        /// 發送文字訊息
        /// </summary>
        public MessageDto SendTextMessage(int roomId, long senderUserId, string content)
        {
            var msg = _messageRepo.AddTextMessage(roomId, senderUserId, content);

            return new MessageDto
            {
                MessageId = msg.MessageId,
                SenderUserId = msg.SenderUserId,
                MessageContent = msg.MessageContent,
                CreatedAt = msg.CreatedAt
            };
        }
    }

}
