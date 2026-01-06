using FrameZone_WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrameZone_WebApi.Socials.Repositories
{
    public class MessageRepository
    {
        private readonly AAContext _context;

        public MessageRepository(AAContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得指定聊天室的所有訊息（不包含已刪除）
        /// </summary>
        public List<Message> GetRoomMessages(int roomId)
        {
            return _context.Messages
                // 限制聊天室
                .Where(m => m.RoomId == roomId && m.DeletedAt == null)
                // 依時間排序（舊 → 新）
                .OrderBy(m => m.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// 新增一筆純文字訊息
        /// </summary>
        public Message AddTextMessage(int roomId, long senderUserId, string content)
        {
            var message = new Message
            {
                RoomId = roomId,
                SenderUserId = senderUserId,
                MessageContent = content,
                MessageType = "text",
                CreatedAt = DateTime.UtcNow
            };

            return AddMessage(message);
        }

        /// <summary>
        /// 新增一筆自訂類型的訊息（含商品/訂單/媒體）
        /// </summary>
        public Message AddMessage(Message message)
        {
            _context.Messages.Add(message);
            _context.SaveChanges();
            return message;
        }
    }

}
