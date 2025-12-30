using FrameZone_WebApi.DTOs.Member;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories.Member;

namespace FrameZone_WebApi.Services.Member
{
    /// <summary>
    /// 會員安全服務實作
    /// 處理密碼變更、登入裝置管理、帳號鎖定狀態等安全相關功能
    /// </summary>
    public class MemberSecurityService : IMemberSecurityService
    {
        private readonly IMemberSecurityRepository _repository;
        private readonly ILogger<MemberSecurityService> _logger;

        public MemberSecurityService(
            IMemberSecurityRepository repository,
            ILogger<MemberSecurityService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // ============================================================================
        // 變更密碼
        // ============================================================================

        /// <summary>
        /// 變更密碼
        /// </summary>
        public async Task<ChangePasswordResponseDto> ChangePasswordAsync(long userId, ChangePasswordDto dto)
        {
            try
            {
                // 1. 取得使用者
                var user = await _repository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ChangePasswordResponseDto
                    {
                        Success = false,
                        Message = "找不到使用者"
                    };
                }

                // 2. 驗證目前密碼
                bool isCurrentPasswordValid = PasswordHelper.VerifyPassword(dto.CurrentPassword, user.Password);
                if (!isCurrentPasswordValid)
                {
                    _logger.LogWarning($"使用者 {userId} 變更密碼失敗：目前密碼錯誤");
                    return new ChangePasswordResponseDto
                    {
                        Success = false,
                        Message = "目前密碼錯誤"
                    };
                }

                // 3. 驗證新密碼強度
                var passwordValidation = ValidatePasswordStrength(dto.NewPassword);
                if (!passwordValidation.isValid)
                {
                    return new ChangePasswordResponseDto
                    {
                        Success = false,
                        Message = passwordValidation.errorMessage
                    };
                }

                // 4. 檢查新密碼是否與目前密碼相同
                bool isSameAsOldPassword = PasswordHelper.VerifyPassword(dto.NewPassword, user.Password);
                if (isSameAsOldPassword)
                {
                    return new ChangePasswordResponseDto
                    {
                        Success = false,
                        Message = "新密碼不能與目前密碼相同"
                    };
                }

                // 5. 更新密碼
                user.Password = PasswordHelper.HashPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.Now;

                await _repository.UpdateUserAsync(user);
                await _repository.SaveChangesAsync();

                // 6. 記錄操作日誌
                await LogPasswordChangeAsync(userId, true);

                _logger.LogInformation($"使用者 {userId} 成功變更密碼");

                return new ChangePasswordResponseDto
                {
                    Success = true,
                    Message = "密碼變更成功"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"變更密碼時發生錯誤: UserId={userId}");
                return new ChangePasswordResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                };
            }
        }

        // ============================================================================
        // 登入裝置管理
        // ============================================================================

        /// <summary>
        /// 取得使用者所有登入裝置（Session）
        /// </summary>
        public async Task<GetUserSessionsResponseDto> GetUserSessionsAsync(long userId, long? currentSessionId = null)
        {
            try
            {
                var sessions = await _repository.GetActiveSessionsAsync(userId);

                // 取得每個 Session 的最後登入 IP（從 UserLog）
                var sessionDtos = new List<UserSessionDto>();

                foreach (var session in sessions)
                {
                    // 從 UserLog 取得此 Session 的最後 IP
                    var lastLog = await _repository.GetLogAfterSessionCreatedAsync(userId, session.CreatedAt);

                    var sessionDto = new UserSessionDto
                    {
                        SessionId = session.SessionId,
                        UserId = session.UserId,
                        UserAgent = session.UserAgent,
                        IsActive = session.IsActive,
                        LastActivityAt = session.LastActivityAt,
                        ExpiresAt = session.ExpiresAt,
                        CreatedAt = session.CreatedAt,
                        IsCurrentSession = currentSessionId.HasValue && session.SessionId == currentSessionId.Value,
                        IpAddress = lastLog?.Ipaddress
                    };

                    // 解析 User Agent
                    ParseUserAgent(sessionDto);

                    sessionDtos.Add(sessionDto);
                }

                return new GetUserSessionsResponseDto
                {
                    Success = true,
                    Message = "取得成功",
                    Data = sessionDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得登入裝置時發生錯誤: UserId={userId}");
                return new GetUserSessionsResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 登出特定裝置（Session）
        /// </summary>
        public async Task<LogoutSessionResponseDto> LogoutSessionAsync(long userId, long sessionId)
        {
            try
            {
                var session = await _repository.GetSessionByIdAsync(sessionId, userId);

                if (session == null)
                {
                    return new LogoutSessionResponseDto
                    {
                        Success = false,
                        Message = "找不到此裝置"
                    };
                }

                // 設定為非活躍狀態
                session.IsActive = false;
                await _repository.UpdateSessionAsync(session);
                await _repository.SaveChangesAsync();

                // 記錄操作日誌
                await LogSessionLogoutAsync(userId, sessionId);

                _logger.LogInformation($"使用者 {userId} 成功登出裝置 {sessionId}");

                return new LogoutSessionResponseDto
                {
                    Success = true,
                    Message = "已登出此裝置"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"登出裝置時發生錯誤: UserId={userId}, SessionId={sessionId}");
                return new LogoutSessionResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 登出所有其他裝置（保留目前裝置）
        /// </summary>
        public async Task<LogoutSessionResponseDto> LogoutOtherSessionsAsync(long userId, long currentSessionId)
        {
            try
            {
                var otherSessions = await _repository.GetOtherActiveSessionsAsync(userId, currentSessionId);

                if (!otherSessions.Any())
                {
                    return new LogoutSessionResponseDto
                    {
                        Success = true,
                        Message = "沒有其他裝置需要登出"
                    };
                }

                // 設定所有其他 Session 為非活躍狀態
                foreach (var session in otherSessions)
                {
                    session.IsActive = false;
                }

                await _repository.UpdateSessionsAsync(otherSessions);
                await _repository.SaveChangesAsync();

                // 記錄操作日誌
                await LogBulkSessionLogoutAsync(userId, otherSessions.Count);

                _logger.LogInformation($"使用者 {userId} 成功登出 {otherSessions.Count} 個其他裝置");

                return new LogoutSessionResponseDto
                {
                    Success = true,
                    Message = $"已登出 {otherSessions.Count} 個其他裝置"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"登出其他裝置時發生錯誤: UserId={userId}");
                return new LogoutSessionResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                };
            }
        }

        // ============================================================================
        // 帳號鎖定狀態
        // ============================================================================

        /// <summary>
        /// 取得帳號鎖定狀態
        /// </summary>
        public async Task<GetAccountLockStatusResponseDto> GetAccountLockStatusAsync(long userId)
        {
            try
            {
                var securityStatus = await _repository.GetSecurityStatusAsync(userId);

                if (securityStatus == null)
                {
                    // 如果沒有安全狀態記錄，建立一個預設的
                    securityStatus = new UserSecurityStatus
                    {
                        UserId = userId,
                        FailedLoginAttempts = 0,
                        IsLocked = false,
                        UpdatedAt = DateTime.Now
                    };
                    await _repository.CreateSecurityStatusAsync(securityStatus);
                    await _repository.SaveChangesAsync();
                }

                // 計算剩餘鎖定時間
                int? remainingMinutes = null;
                if (securityStatus.IsLocked && securityStatus.LockedUntil.HasValue)
                {
                    var remaining = securityStatus.LockedUntil.Value - DateTime.Now;
                    if (remaining.TotalMinutes > 0)
                    {
                        remainingMinutes = (int)Math.Ceiling(remaining.TotalMinutes);
                    }
                    else
                    {
                        // 鎖定時間已過，自動解鎖
                        securityStatus.IsLocked = false;
                        securityStatus.UnlockedAt = DateTime.Now;
                        await _repository.UpdateSecurityStatusAsync(securityStatus);
                        await _repository.SaveChangesAsync();
                    }
                }

                var lockStatus = new AccountLockStatusDto
                {
                    UserId = securityStatus.UserId,
                    IsLocked = securityStatus.IsLocked,
                    FailedLoginAttempts = securityStatus.FailedLoginAttempts,
                    LastFailedLoginAt = securityStatus.LastFailedLoginAt,
                    LockedAt = securityStatus.LockedAt,
                    LockedUntil = securityStatus.LockedUntil,
                    LockedReason = securityStatus.LockedReason,
                    LockedBy = securityStatus.LockedBy,
                    UnlockedAt = securityStatus.UnlockedAt,
                    RemainingLockMinutes = remainingMinutes
                };

                return new GetAccountLockStatusResponseDto
                {
                    Success = true,
                    Message = "取得成功",
                    Data = lockStatus
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得帳號鎖定狀態時發生錯誤: UserId={userId}");
                return new GetAccountLockStatusResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                };
            }
        }

        // ============================================================================
        // 安全性概覽
        // ============================================================================

        /// <summary>
        /// 取得安全性概覽
        /// </summary>
        public async Task<GetSecurityOverviewResponseDto> GetSecurityOverviewAsync(long userId)
        {
            try
            {
                // 1. 取得鎖定狀態
                var lockStatusResponse = await GetAccountLockStatusAsync(userId);

                // 2. 取得活躍 Session 數量
                var activeSessions = await _repository.GetActiveSessionsAsync(userId);
                var activeSessionCount = activeSessions.Count;

                // 3. 取得最後一次密碼變更時間（從 UserLog）
                var lastPasswordChange = await _repository.GetLastPasswordChangeLogAsync(userId);

                // 4. 取得最後一次登入資訊
                var lastLogin = await _repository.GetLastLoginLogAsync(userId);

                // 5. 判斷是否建議變更密碼（超過 90 天）
                bool shouldChangePassword = false;
                if (lastPasswordChange != null)
                {
                    var daysSincePasswordChange = (DateTime.Now - lastPasswordChange.CreatedAt).TotalDays;
                    shouldChangePassword = daysSincePasswordChange > 90;
                }
                else
                {
                    // 如果從未變更過密碼，建議變更
                    shouldChangePassword = true;
                }

                var overview = new SecurityOverviewDto
                {
                    LockStatus = lockStatusResponse.Data,
                    ActiveSessionCount = activeSessionCount,
                    LastPasswordChangeAt = lastPasswordChange?.CreatedAt,
                    LastLoginAt = lastLogin?.CreatedAt,
                    LastLoginIp = lastLogin?.Ipaddress,
                    ShouldChangePassword = shouldChangePassword
                };

                return new GetSecurityOverviewResponseDto
                {
                    Success = true,
                    Message = "取得成功",
                    Data = overview
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得安全性概覽時發生錯誤: UserId={userId}");
                return new GetSecurityOverviewResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                };
            }
        }

        // ============================================================================
        // 私有輔助方法
        // ============================================================================

        /// <summary>
        /// 驗證密碼強度
        /// </summary>
        private (bool isValid, string errorMessage) ValidatePasswordStrength(string password)
        {
            if (password.Length < SecurityConstants.PASSWORD_MIN_LENGTH)
            {
                return (false, $"密碼長度至少需要 {SecurityConstants.PASSWORD_MIN_LENGTH} 個字元");
            }

            if (password.Length > SecurityConstants.PASSWORD_MAX_LENGTH)
            {
                return (false, $"密碼長度不能超過 {SecurityConstants.PASSWORD_MAX_LENGTH} 個字元");
            }

            if (SecurityConstants.PASSWORD_REQUIRE_UPPERCASE && !password.Any(char.IsUpper))
            {
                return (false, "密碼必須包含至少一個大寫字母");
            }

            if (SecurityConstants.PASSWORD_REQUIRE_LOWERCASE && !password.Any(char.IsLower))
            {
                return (false, "密碼必須包含至少一個小寫字母");
            }

            if (SecurityConstants.PASSWORD_REQUIRE_DIGIT && !password.Any(char.IsDigit))
            {
                return (false, "密碼必須包含至少一個數字");
            }

            if (SecurityConstants.PASSWORD_REQUIRE_SPECIAL_CHAR && !password.Any(c => !char.IsLetterOrDigit(c)))
            {
                return (false, "密碼必須包含至少一個特殊字元");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 解析 User Agent 字串
        /// </summary>
        private void ParseUserAgent(UserSessionDto session)
        {
            if (string.IsNullOrEmpty(session.UserAgent))
            {
                return;
            }

            var ua = session.UserAgent.ToLower();

            // 判斷裝置類型
            if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
            {
                session.DeviceType = "Mobile";
            }
            else if (ua.Contains("tablet") || ua.Contains("ipad"))
            {
                session.DeviceType = "Tablet";
            }
            else
            {
                session.DeviceType = "Desktop";
            }

            // 判斷瀏覽器
            if (ua.Contains("edg"))
            {
                session.BrowserName = "Edge";
            }
            else if (ua.Contains("chrome"))
            {
                session.BrowserName = "Chrome";
            }
            else if (ua.Contains("safari") && !ua.Contains("chrome"))
            {
                session.BrowserName = "Safari";
            }
            else if (ua.Contains("firefox"))
            {
                session.BrowserName = "Firefox";
            }
            else if (ua.Contains("opera") || ua.Contains("opr"))
            {
                session.BrowserName = "Opera";
            }

            // 判斷作業系統
            if (ua.Contains("windows"))
            {
                session.OperatingSystem = "Windows";
            }
            else if (ua.Contains("mac"))
            {
                session.OperatingSystem = "macOS";
            }
            else if (ua.Contains("linux"))
            {
                session.OperatingSystem = "Linux";
            }
            else if (ua.Contains("android"))
            {
                session.OperatingSystem = "Android";
            }
            else if (ua.Contains("iphone") || ua.Contains("ipad"))
            {
                session.OperatingSystem = "iOS";
            }
        }

        /// <summary>
        /// 記錄密碼變更日誌
        /// </summary>
        private async Task LogPasswordChangeAsync(long userId, bool success)
        {
            try
            {
                var log = new UserLog
                {
                    UserId = userId,
                    Status = success ? "Success" : "Failure",
                    ActionType = "PasswordChange",
                    ActionCategory = "Security",
                    ActionDescription = success ? "使用者變更密碼成功" : "使用者變更密碼失敗",
                    SystemName = "Member",
                    Severity = "Info",
                    PerformedBy = "User",
                    CreatedAt = DateTime.Now
                };

                await _repository.CreateLogAsync(log);
                await _repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄密碼變更日誌失敗");
            }
        }

        /// <summary>
        /// 記錄登出裝置日誌
        /// </summary>
        private async Task LogSessionLogoutAsync(long userId, long sessionId)
        {
            try
            {
                var log = new UserLog
                {
                    UserId = userId,
                    Status = "Success",
                    ActionType = "SessionLogout",
                    ActionCategory = "Security",
                    ActionDescription = $"使用者登出裝置 (SessionId: {sessionId})",
                    TargetType = "UserSession",
                    TargetId = sessionId,
                    SystemName = "Member",
                    Severity = "Info",
                    PerformedBy = "User",
                    CreatedAt = DateTime.Now
                };

                await _repository.CreateLogAsync(log);
                await _repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄登出裝置日誌失敗");
            }
        }

        /// <summary>
        /// 記錄批次登出裝置日誌
        /// </summary>
        private async Task LogBulkSessionLogoutAsync(long userId, int logoutCount)
        {
            try
            {
                var log = new UserLog
                {
                    UserId = userId,
                    Status = "Success",
                    ActionType = "BulkSessionLogout",
                    ActionCategory = "Security",
                    ActionDescription = $"使用者登出 {logoutCount} 個其他裝置",
                    SystemName = "Member",
                    Severity = "Info",
                    PerformedBy = "User",
                    CreatedAt = DateTime.Now
                };

                await _repository.CreateLogAsync(log);
                await _repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄批次登出裝置日誌失敗");
            }
        }
    }
}