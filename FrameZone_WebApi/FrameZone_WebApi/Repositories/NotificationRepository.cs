using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories.Interfaces;
using Google;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Repositories
{
    /// <summary>
    /// 通知 Repository 實作
    /// </summary>
    public class NotificationRepository : INotificationRepository
    {
        private readonly AAContext _context;

        public NotificationRepository(AAContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得使用者未讀通知數量
        /// </summary>
        public async Task<UnreadCountDto> GetUnreadCountAsync(long userId)
        {
            // 總未讀數量
            var totalCount = await _context.BellNotificationRecipients
                .Where(r => r.UserId == userId && !r.IsRead && !r.IsDeleted)
                .CountAsync();

            // 各系統未讀數量
            var systemCounts = await _context.BellNotificationRecipients
                .Where(r => r.UserId == userId && !r.IsRead && !r.IsDeleted)
                .Join(_context.BellNotifications,
                    r => r.NotificationId,
                    n => n.NotificationId,
                    (r, n) => new { r, n })
                .Join(_context.UserSystemModules,
                    rn => rn.n.SystemId,
                    s => s.SystemId,
                    (rn, s) => new { rn.r, rn.n, s })
                .GroupBy(x => x.s.SystemCode)
                .Select(g => new
                {
                    SystemCode = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.SystemCode, x => x.Count);

            return new UnreadCountDto
            {
                TotalCount = totalCount,
                SystemCounts = systemCounts
            };
        }

        /// <summary>
        /// 取得使用者通知清單（分頁）
        /// </summary>
        public async Task<NotificationPagedResultDto> GetNotificationsAsync(long userId, NotificationQueryDto query)
        {
            // 基礎查詢
            var baseQuery = _context.BellNotificationRecipients
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .Join(_context.BellNotifications,
                    r => r.NotificationId,
                    n => n.NotificationId,
                    (r, n) => new { Recipient = r, Notification = n })
                .Join(_context.UserSystemModules,
                    rn => rn.Notification.SystemId,
                    s => s.SystemId,
                    (rn, s) => new { rn.Recipient, rn.Notification, System = s })
                .Join(_context.Categories,
                    rns => rns.Notification.CategoryId,
                    c => c.CategoryId,
                    (rns, c) => new { rns.Recipient, rns.Notification, rns.System, Category = c })
                .Join(_context.Priorities,
                    rnsc => rnsc.Notification.PriorityId,
                    p => p.PriorityId,
                    (rnsc, p) => new { rnsc.Recipient, rnsc.Notification, rnsc.System, rnsc.Category, Priority = p });

            // 系統篩選
            if (!string.IsNullOrEmpty(query.SystemCode))
            {
                baseQuery = baseQuery.Where(x => x.System.SystemCode == query.SystemCode);
            }

            // 未讀篩選
            if (query.IsUnreadOnly.HasValue)
            {
                baseQuery = baseQuery.Where(x => x.Recipient.IsRead == !query.IsUnreadOnly.Value);
            }

            // 總筆數
            var totalCount = await baseQuery.CountAsync();

            // 分頁查詢
            var items = await baseQuery
                .OrderByDescending(x => x.Notification.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new NotificationDto
                {
                    RecipientId = x.Recipient.RecipientId,
                    NotificationId = x.Notification.NotificationId,
                    UserId = x.Recipient.UserId,
                    SystemCode = x.System.SystemCode,
                    SystemName = x.System.SystemName,
                    CategoryCode = x.Category.CategoryCode,
                    CategoryName = x.Category.CategoryName,
                    CategoryIcon = GetCategoryIcon(x.Category.CategoryCode),
                    PriorityCode = x.Priority.PriorityCode,
                    PriorityLevel = x.Priority.PriorityLevel,
                    NotificationTitle = x.Notification.NotificationTitle,
                    NotificationContent = x.Notification.NotificationContent,
                    RelatedObjectType = x.Notification.RelatedObjectType,
                    RelatedObjectId = x.Notification.RelatedObjectId,
                    IsRead = x.Recipient.IsRead,
                    ReadAt = x.Recipient.ReadAt,
                    CreatedAt = x.Notification.CreatedAt,
                    ExpiresAt = x.Notification.ExpiresAt
                })
                .ToListAsync();

            // 計算分頁資訊
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return new NotificationPagedResultDto
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = query.Page,
                PageSize = query.PageSize,
                TotalPages = totalPages,
                HasPrevious = query.Page > 1,
                HasNext = query.Page < totalPages
            };
        }

        /// <summary>
        /// 取得單筆通知詳細資訊
        /// </summary>
        public async Task<NotificationDto?> GetNotificationByIdAsync(long userId, long recipientId)
        {
            return await _context.BellNotificationRecipients
                .Where(r => r.RecipientId == recipientId && r.UserId == userId && !r.IsDeleted)
                .Join(_context.BellNotifications,
                    r => r.NotificationId,
                    n => n.NotificationId,
                    (r, n) => new { Recipient = r, Notification = n })
                .Join(_context.UserSystemModules,
                    rn => rn.Notification.SystemId,
                    s => s.SystemId,
                    (rn, s) => new { rn.Recipient, rn.Notification, System = s })
                .Join(_context.Categories,
                    rns => rns.Notification.CategoryId,
                    c => c.CategoryId,
                    (rns, c) => new { rns.Recipient, rns.Notification, rns.System, Category = c })
                .Join(_context.Priorities,
                    rnsc => rnsc.Notification.PriorityId,
                    p => p.PriorityId,
                    (rnsc, p) => new { rnsc.Recipient, rnsc.Notification, rnsc.System, rnsc.Category, Priority = p })
                .Select(x => new NotificationDto
                {
                    RecipientId = x.Recipient.RecipientId,
                    NotificationId = x.Notification.NotificationId,
                    UserId = x.Recipient.UserId,
                    SystemCode = x.System.SystemCode,
                    SystemName = x.System.SystemName,
                    CategoryCode = x.Category.CategoryCode,
                    CategoryName = x.Category.CategoryName,
                    CategoryIcon = GetCategoryIcon(x.Category.CategoryCode),
                    PriorityCode = x.Priority.PriorityCode,
                    PriorityLevel = x.Priority.PriorityLevel,
                    NotificationTitle = x.Notification.NotificationTitle,
                    NotificationContent = x.Notification.NotificationContent,
                    RelatedObjectType = x.Notification.RelatedObjectType,
                    RelatedObjectId = x.Notification.RelatedObjectId,
                    IsRead = x.Recipient.IsRead,
                    ReadAt = x.Recipient.ReadAt,
                    CreatedAt = x.Notification.CreatedAt,
                    ExpiresAt = x.Notification.ExpiresAt
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 標記通知為已讀
        /// </summary>
        public async Task<int> MarkAsReadAsync(long userId, List<long> recipientIds)
        {
            var recipients = await _context.BellNotificationRecipients
                .Where(r => recipientIds.Contains(r.RecipientId) && r.UserId == userId && !r.IsDeleted)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var recipient in recipients)
            {
                if (!recipient.IsRead)
                {
                    recipient.IsRead = true;
                    recipient.ReadAt = now;
                    recipient.UpdatedAt = now;
                }
            }

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 標記所有通知為已讀
        /// </summary>
        public async Task<int> MarkAllAsReadAsync(long userId, string? systemCode = null)
        {
            var query = _context.BellNotificationRecipients
                .Where(r => r.UserId == userId && !r.IsRead && !r.IsDeleted);

            // 如果指定系統代碼，需要 JOIN 過濾
            if (!string.IsNullOrEmpty(systemCode))
            {
                query = query
                    .Join(_context.BellNotifications,
                        r => r.NotificationId,
                        n => n.NotificationId,
                        (r, n) => new { Recipient = r, Notification = n })
                    .Join(_context.UserSystemModules,
                        rn => rn.Notification.SystemId,
                        s => s.SystemId,
                        (rn, s) => new { rn.Recipient, rn.Notification, System = s })
                    .Where(x => x.System.SystemCode == systemCode)
                    .Select(x => x.Recipient);
            }

            var recipients = await query.ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var recipient in recipients)
            {
                recipient.IsRead = true;
                recipient.ReadAt = now;
                recipient.UpdatedAt = now;
            }

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 刪除通知（軟刪除）
        /// </summary>
        public async Task<int> DeleteNotificationsAsync(long userId, List<long> recipientIds)
        {
            var recipients = await _context.BellNotificationRecipients
                .Where(r => recipientIds.Contains(r.RecipientId) && r.UserId == userId && !r.IsDeleted)
                .ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var recipient in recipients)
            {
                recipient.IsDeleted = true;
                recipient.DeletedAt = now;
                recipient.UpdatedAt = now;
            }

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 清空所有通知（軟刪除）
        /// </summary>
        public async Task<int> ClearAllNotificationsAsync(long userId, string? systemCode = null)
        {
            var query = _context.BellNotificationRecipients
                .Where(r => r.UserId == userId && !r.IsDeleted);

            // 如果指定系統代碼，需要 JOIN 過濾
            if (!string.IsNullOrEmpty(systemCode))
            {
                query = query
                    .Join(_context.BellNotifications,
                        r => r.NotificationId,
                        n => n.NotificationId,
                        (r, n) => new { Recipient = r, Notification = n })
                    .Join(_context.UserSystemModules,
                        rn => rn.Notification.SystemId,
                        s => s.SystemId,
                        (rn, s) => new { rn.Recipient, rn.Notification, System = s })
                    .Where(x => x.System.SystemCode == systemCode)
                    .Select(x => x.Recipient);
            }

            var recipients = await query.ToListAsync();

            var now = DateTime.UtcNow;
            foreach (var recipient in recipients)
            {
                recipient.IsDeleted = true;
                recipient.DeletedAt = now;
                recipient.UpdatedAt = now;
            }

            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 取得系統模組清單（含未讀數）
        /// </summary>
        public async Task<List<SystemModuleDto>> GetSystemModulesAsync(long userId)
        {
            var systems = await _context.UserSystemModules
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    s.SystemCode,
                    s.SystemName,
                    UnreadCount = _context.BellNotificationRecipients
                        .Where(r => r.UserId == userId && !r.IsRead && !r.IsDeleted)
                        .Join(_context.BellNotifications,
                            r => r.NotificationId,
                            n => n.NotificationId,
                            (r, n) => new { r, n })
                        .Count(x => x.n.SystemId == s.SystemId)
                })
                .ToListAsync();

            return systems.Select(s => new SystemModuleDto
            {
                SystemCode = s.SystemCode,
                SystemName = s.SystemName,
                UnreadCount = s.UnreadCount
            }).ToList();
        }

        /// <summary>
        /// 檢查通知是否屬於該使用者
        /// </summary>
        public async Task<bool> IsNotificationOwnedByUserAsync(long userId, long recipientId)
        {
            return await _context.BellNotificationRecipients
                .AnyAsync(r => r.RecipientId == recipientId && r.UserId == userId);
        }

        /// <summary>
        /// 取得使用者通知偏好設定
        /// </summary>
        public async Task<NotificationPreferenceCheckDto?> GetNotificationPreferenceAsync(long userId, string categoryCode)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryCode == categoryCode);

            if (category == null)
                return null;

            var preference = await _context.BellNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && p.CategoryId == category.CategoryId);

            if (preference == null)
            {
                // 如果沒有設定，預設全部啟用
                return new NotificationPreferenceCheckDto
                {
                    UserId = userId,
                    CategoryCode = categoryCode,
                    ShouldSendBellNotification = true,
                    ShouldSendEmail = false,
                    ShouldSendSms = false,
                    ShouldSendPush = true
                };
            }

            return new NotificationPreferenceCheckDto
            {
                UserId = userId,
                CategoryCode = categoryCode,
                ShouldSendBellNotification = preference.IsEnabled,
                ShouldSendEmail = false,
                ShouldSendSms = false,
                ShouldSendPush = preference.IsEnabled
            };
        }

        /// <summary>
        /// 建立新通知（給特定使用者）
        /// </summary>
        public async Task<long> CreateNotificationAsync(
            long userId,
            string systemCode,
            string categoryCode,
            string priorityCode,
            string title,
            string content,
            string? relatedObjectType = null,
            long? relatedObjectId = null)
        {
            // 取得系統、類別、優先級 ID
            var system = await _context.UserSystemModules.FirstOrDefaultAsync(s => s.SystemCode == systemCode);
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryCode == categoryCode);
            var priority = await _context.Priorities.FirstOrDefaultAsync(p => p.PriorityCode == priorityCode);

            if (system == null || category == null || priority == null)
                throw new Exception("系統、類別或優先級不存在");

            // 檢查使用者偏好
            var preference = await GetNotificationPreferenceAsync(userId, categoryCode);
            if (preference == null || !preference.ShouldSendBellNotification)
                return 0; // 使用者不希望接收此類通知

            // 建立通知
            var notification = new BellNotification
            {
                SystemId = system.SystemId,
                CategoryId = category.CategoryId,
                PriorityId = priority.PriorityId,
                NotificationTitle = title,
                NotificationContent = content,
                RelatedObjectType = relatedObjectType,
                RelatedObjectId = relatedObjectId,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(NotificationConstant.Expiration.DEFAULT_DAYS)
            };

            _context.BellNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // 建立接收者記錄
            var recipient = new BellNotificationRecipient
            {
                NotificationId = notification.NotificationId,
                UserId = userId,
                IsRead = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.BellNotificationRecipients.Add(recipient);
            await _context.SaveChangesAsync();

            return notification.NotificationId;
        }

        /// <summary>
        /// 批次建立通知（給多個使用者）
        /// </summary>
        public async Task<int> CreateBatchNotificationsAsync(
            List<long> userIds,
            string systemCode,
            string categoryCode,
            string priorityCode,
            string title,
            string content,
            string? relatedObjectType = null,
            long? relatedObjectId = null)
        {
            // 取得系統、類別、優先級 ID
            var system = await _context.UserSystemModules.FirstOrDefaultAsync(s => s.SystemCode == systemCode);
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryCode == categoryCode);
            var priority = await _context.Priorities.FirstOrDefaultAsync(p => p.PriorityCode == priorityCode);

            if (system == null || category == null || priority == null)
                throw new Exception("系統、類別或優先級不存在");

            // 建立通知
            var notification = new BellNotification
            {
                SystemId = system.SystemId,
                CategoryId = category.CategoryId,
                PriorityId = priority.PriorityId,
                NotificationTitle = title,
                NotificationContent = content,
                RelatedObjectType = relatedObjectType,
                RelatedObjectId = relatedObjectId,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(NotificationConstant.Expiration.DEFAULT_DAYS)
            };

            _context.BellNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // 批次建立接收者記錄
            var recipients = new List<BellNotificationRecipient>();
            foreach (var userId in userIds)
            {
                // 檢查使用者偏好
                var preference = await GetNotificationPreferenceAsync(userId, categoryCode);
                if (preference != null && preference.ShouldSendBellNotification)
                {
                    recipients.Add(new BellNotificationRecipient
                    {
                        NotificationId = notification.NotificationId,
                        UserId = userId,
                        IsRead = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.BellNotificationRecipients.AddRange(recipients);
            await _context.SaveChangesAsync();

            return recipients.Count;
        }

        /// <summary>
        /// 刪除過期通知
        /// </summary>
        public async Task<int> DeleteExpiredNotificationsAsync()
        {
            var now = DateTime.UtcNow;
            var expiredNotifications = await _context.BellNotifications
                .Where(n => n.ExpiresAt.HasValue && n.ExpiresAt.Value < now)
                .ToListAsync();

            _context.BellNotifications.RemoveRange(expiredNotifications);
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 取得類別圖示
        /// </summary>
        private static string GetCategoryIcon(string categoryCode)
        {
            return NotificationConstant.CategoryIcons.TryGetValue(categoryCode, out var icon)
                ? icon
                : "🔔";
        }
    }
}