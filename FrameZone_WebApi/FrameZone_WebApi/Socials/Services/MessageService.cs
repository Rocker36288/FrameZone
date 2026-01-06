using System;
using System.Collections.Generic;
using System.Linq;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;

namespace FrameZone_WebApi.Socials.Services
{
    public class MessageService
    {
        private readonly MessageRepository _messageRepo;
        private static readonly HashSet<string> _allowedMessageTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "text",
            "image",
            "sticker",
            "product",
            "order",
            "link",
            "video"
        };

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
                .Select(ToDto)
                .ToList();
        }

        /// <summary>
        /// 發送文字訊息
        /// </summary>
        public MessageDto SendTextMessage(int roomId, long senderUserId, string content)
        {
            var msg = _messageRepo.AddTextMessage(roomId, senderUserId, content);

            return ToDto(msg);
        }

        /// <summary>
        /// 發送商城/多媒體訊息（商品、訂單、連結、貼圖、圖片、影片）
        /// </summary>
        public MessageDto SendShopMessage(int roomId, long senderUserId, SendShopMessageDto dto)
        {
            var messageType = dto.MessageType?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(messageType))
                throw new ArgumentException("請填寫訊息類型", nameof(dto.MessageType));

            if (!_allowedMessageTypes.Contains(messageType))
                throw new ArgumentException("不支援的訊息類型", nameof(dto.MessageType));

            ValidateShopMessage(messageType, dto);

            var message = new Message
            {
                RoomId = roomId,
                SenderUserId = senderUserId,
                MessageContent = dto.MessageContent ?? string.Empty,
                MessageType = messageType,
                StickerId = dto.StickerId,
                MediaUrl = dto.MediaUrl,
                ThumbnailUrl = dto.ThumbnailUrl,
                ProductId = dto.ProductId,
                OrderId = dto.OrderId,
                LinkTitle = dto.LinkTitle,
                LinkDescription = dto.LinkDescription,
                CreatedAt = DateTime.UtcNow
            };

            var saved = _messageRepo.AddMessage(message);
            return ToDto(saved);
        }

        private static MessageDto ToDto(Message m)
        {
            return new MessageDto
            {
                MessageId = m.MessageId,
                SenderUserId = m.SenderUserId,
                MessageContent = m.MessageContent,
                MessageType = m.MessageType,
                StickerId = m.StickerId,
                MediaUrl = m.MediaUrl,
                ThumbnailUrl = m.ThumbnailUrl,
                ProductId = m.ProductId,
                OrderId = m.OrderId,
                LinkTitle = m.LinkTitle,
                LinkDescription = m.LinkDescription,
                CreatedAt = m.CreatedAt
            };
        }

        private static void ValidateShopMessage(string messageType, SendShopMessageDto dto)
        {
            switch (messageType)
            {
                case "product":
                    if (!dto.ProductId.HasValue)
                        throw new ArgumentException("商品訊息需要 ProductId", nameof(dto.ProductId));
                    break;
                case "order":
                    if (!dto.OrderId.HasValue)
                        throw new ArgumentException("訂單訊息需要 OrderId", nameof(dto.OrderId));
                    break;
                case "image":
                case "video":
                    if (string.IsNullOrWhiteSpace(dto.MediaUrl))
                        throw new ArgumentException("媒體訊息需要 MediaUrl", nameof(dto.MediaUrl));
                    break;
                case "sticker":
                    if (!dto.StickerId.HasValue)
                        throw new ArgumentException("貼圖訊息需要 StickerId", nameof(dto.StickerId));
                    break;
                case "link":
                    if (string.IsNullOrWhiteSpace(dto.LinkTitle) && string.IsNullOrWhiteSpace(dto.MessageContent))
                        throw new ArgumentException("連結訊息至少需要標題或內容", nameof(dto.LinkTitle));
                    break;
                default:
                    // text 無額外欄位
                    break;
            }
        }
    }

}
