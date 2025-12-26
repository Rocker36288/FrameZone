using System;
using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Videos.Services
{
    public class ChannelEnsureHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ChannelEnsureHostedService> _logger;

        public ChannelEnsureHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<ChannelEnsureHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Channel ensure check started.");

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AAContext>();

            // 1️⃣ 取得所有 User
            var users = await context.Users
                .AsNoTracking()
                .Select(u => new { u.UserId,DisplayName =u.UserProfile.DisplayName })
                .ToListAsync(stoppingToken);

            // 2️⃣ 已存在的 ChannelId
            var existingChannelIds = await context.Channels
                .AsNoTracking()
                .Select(c => c.ChannelId)
                .ToHashSetAsync(stoppingToken);

            // 3️⃣ 找出缺 Channel 的 User
            var missingUsers = users
                .Where(u => !existingChannelIds.Contains(u.UserId))
                .ToList();

            if (!missingUsers.Any())
            {
                _logger.LogInformation("All users already have channels.");
                return;
            }

            // 4️⃣ 補建 Channel
            foreach (var user in missingUsers)
            {
                context.Channels.Add(new Channel
                {
                    ChannelId = user.UserId,
                    UserId = user.UserId,
                    ChannelName = $"{user.DisplayName}'s Channel",
                    Description = $"歡迎來到 {user.DisplayName} 的頻道！",
                    CreatedAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Channel ensure completed. Created {Count} channels.", missingUsers.Count);
        }
    }
}
