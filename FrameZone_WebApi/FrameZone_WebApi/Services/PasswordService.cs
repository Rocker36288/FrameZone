using FrameZone_WebApi.Configuration;
using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace FrameZone_WebApi.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly UserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly VerificationSettings _verificationSettings;
        private readonly ILogger<PasswordService> _logger;
        private readonly IConfiguration _configuration;

        public PasswordService(UserRepository userRepository, IEmailService emailService, IOptions<VerificationSettings> verificationSettings, ILogger<PasswordService> logger, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _verificationSettings = verificationSettings.Value;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 處理忘記密碼請求
        /// </summary>
        public async Task<ApiResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "請輸入 Email"
                    };
                }

                // 檢查使用者是否存在
                var user = await _userRepository.GetUserByEmailAsync(request.Email);

                if (user == null)
                {
                    _logger.LogWarning($"忘記密碼請求失敗: Email 不存在 - {request.Email}");

                    // 回傳成功訊息
                    return new ApiResponseDto
                    {
                        Success = true,
                        Message = "如果此 Email 存在，我們已發送重設密碼連結到您的信箱"
                    };
                }

                // 檢查帳號狀態
                if (user.IsDeleted)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "此帳號已被刪除，無法重設密碼"
                    };
                }

                // 產生加密的 Token
                var token = GenerateSecureToken();

                // 建立驗證紀錄
                var verification = new UserVerification
                {
                    UserId = user.UserId,
                    VerificationTypeId = 1,
                    VerificationCode = string.Empty,
                    VerificationToken = token,
                    SentTo = request.Email,
                    Purpose = "重設密碼",
                    RequestedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddHours(_verificationSettings.ResetTokenExpirationHours),
                    IsVerified = false,
                    IsUsed = false,
                    FailedAttempts = 0,
                    Ipaddress = null
                };

                // 儲存到資料庫
                var createResult = await _userRepository.CreateVerificationAsync(verification);

                if (!createResult)
                {
                    _logger.LogError($"建立驗證紀錄失敗: UserId={user.UserId}");

                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "系統錯誤，請稍後在試"
                    };
                }

                // 產生重設密碼
                var resetUrl = GenerateResetPasswordUrl(token);

                // 發送 Email
                var emailDto = new ResetPasswordEmailDto
                {
                    ToEmail = request.Email,
                    ToName = user.UserProfile?.DisplayName ?? user.Account,
                    ResetUrl = resetUrl,
                    ExpirationHours = _verificationSettings.ResetTokenExpirationHours,
                };

                var emailResult = await _emailService.SendResetPasswordEmailAsync(emailDto);

                if (!emailResult)
                {
                    _logger.LogError($"發送重設密碼 Email 失敗: {request.Email}");
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Email 發送失敗，請稍後在試"
                    };
                }

                _logger.LogInformation($"重設密碼 Email 已發送: {request.Email}");

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "重設密碼連結到您的信箱，請查收"
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"處理忘記密碼請求時發生錯誤: {request.Email}");
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後在試"
                };
            }
        }

        /// <summary>
        /// 驗證重設密碼 Token 是否有效
        /// </summary>
        public async Task<ApiResponseDto> ValidateResetTokenAsync(string token)
        {
            try
            {
                // Token 是否為空
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Token 不可為空"
                    };
                }

                // 從資料庫查詢驗證紀錄
                var verification = await _userRepository.GetVerificationByTokenAsync(token);

                // 是否已過期
                if (verification == null)
                {
                    _logger.LogWarning($"驗證失敗: Token 不存在 - {token}");
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "此重設密碼連結已失效或以使用"
                    };
                }

                // 是否已使用
                if (verification.IsUsed)
                {
                    _logger.LogWarning($"驗證失敗: Token 已使用 - VerificationId={verification.VerificationId}");
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "此重設密碼連接已使用"
                    };
                }

                // 是否過期
                if (verification.ExpiredAt < DateTime.UtcNow)
                {
                    _logger.LogWarning($"驗證失敗: Token 已過期 - VerificationId={verification.VerificationId}");
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "此重設密碼連結已過期"
                    };
                }

                _logger.LogInformation($"驗證成功: VerificationId={verification.VerificationId}");
                return new ApiResponseDto
                {
                    Success = true,
                    Message = "驗證成功"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"驗證 Token 時發生錯誤: {token}");
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後在試"
                };
            }
        }

        /// <summary>
        /// 重設密碼
        /// </summary>
        public async Task<ApiResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            try
            {
                // 基本驗證
                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Token 不可為空"
                    };
                }

                if (string.IsNullOrEmpty(request.NewPassword))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "請輸入新密碼"
                    };
                }

                // 檢查兩次密碼是否一致
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "兩次輸入的密碼不一致"
                    };
                }

                // 使用 PasswordHelper 檢查密碼強度
                if (!PasswordHelper.IsPasswordStrong(request.NewPassword))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "密碼強度不足: 密碼長度須為 6-50 個字，且必須包含英文字母和數字"
                    };
                }

                // 從資料庫取得驗證紀錄
                var verification = await _userRepository.GetVerificationByTokenAsync(request.Token);

                if (verification == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "無效的重設密碼連結"
                    };
                }

                // 驗證 Token
                var validationResult = await ValidateResetTokenAsync(request.Token);

                if (!validationResult.Success)
                {
                    verification.FailedAttempts += 1;
                    await _userRepository.UpdateVerificationAsync(verification);

                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "重設密碼連結已失效或已過期"
                    };
                }

                // 取得使用者資料
                var user = await _userRepository.GetUserByIdAsync(verification.UserId);

                if (user == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "找不到使用者"
                    };
                }

                // 檢查新密碼是否與舊密碼相同
                if (PasswordHelper.VerifyPassword(request.NewPassword, user.Password))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "新密碼不可與舊密碼相同"
                    };
                }

                // 使用 PasswordHelper 加密新密碼
                var hashedPassword = PasswordHelper.HashPassword(request.NewPassword);

                // 更新密碼
                user.Password = hashedPassword;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userRepository.UpdateUserAsync(user);

                if (!updateResult)
                {
                    _logger.LogError($"更新密碼失敗: UserId={user.UserId}");
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "密碼更新失敗，請稍後在試"
                    };
                }

                // 標記 Token 為已使用
                await _userRepository.MarkVerificationAsUsedAsync(verification.VerificationId);

                var userLog = new UserLog
                {
                    UserId = user.UserId,
                    Status = "Success",
                    ActionType = "ResetPassword",
                    ActionCategory = "Security",
                    ActionDescription = "用戶重設密碼成功",
                    TargetType = "User",
                    TargetId = user.UserId,
                    SystemName = "FrameZone",
                    Severity = "Info",
                    PerformedBy = "User"
                };

                await _userRepository.CreateUserLogAsync(userLog);

                _logger.LogInformation($"密碼重設成功: UserId={user.UserId}");

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "密碼重設成功，請使用新密碼登入"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重設密碼時發生錯誤");
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後在試"
                };
            }
        }

        public async Task<ApiResponseDto> ChangePasswordAsync(long userId, ChangePasswordRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "請輸入當前密碼"
                    };
                }

                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "請輸入新密碼"
                    };
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "兩次輸入的密碼不一致"
                    };
                }

                if (!PasswordHelper.IsPasswordStrong(request.NewPassword))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "密碼強度不足，密碼長度須為 6-50 個字，且必須包含英文字母和數字"
                    };
                }

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "找不到使用者"
                    };
                }

                if (!PasswordHelper.VerifyPassword(request.CurrentPassword, user.Password))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "當前密碼不正確"
                    };
                }

                if (PasswordHelper.VerifyPassword(request.NewPassword, user.Password))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "新密碼不可與舊密碼相同"
                    };
                }

                var hashedPassword = PasswordHelper.HashPassword(request.NewPassword);

                user.Password = hashedPassword;
                user.UpdatedAt = DateTime.UtcNow;

                var updateResult = await _userRepository.UpdateUserAsync(user);

                if (!updateResult)
                {
                    _logger.LogError($"更新密碼失敗: UserId={user.UserId}");
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "密碼更新失敗，請稍後在試"
                    };
                }

                var userLog = new UserLog
                {
                    UserId = user.UserId,
                    Status = "Success",
                    ActionType = "ChangePassword",
                    ActionCategory = "Security",
                    ActionDescription = "用戶變更密碼成功",
                    TargetType = "User",
                    TargetId = user.UserId,
                    SystemName = "FrameZone",
                    Severity = "Info",
                    PerformedBy = "User"
                };

                await _userRepository.CreateUserLogAsync(userLog);

                _logger.LogInformation($"密碼變更成功: UserId={user.UserId}");

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "密碼變更成功"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"變更密碼時發生錯誤：UserId={userId}");
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 產生 Token
        /// </summary>
        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            var token = Convert.ToBase64String(bytes);
            token = token.Replace("/", "_").Replace("+", "-").Replace("=", "");
            return token;
        }

        /// <summary>
        /// 產生重設密碼 URL
        /// </summary>
        private string GenerateResetPasswordUrl(string token)
        {
            var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://localhost:4200";
            frontendUrl = frontendUrl.TrimEnd('/');
            return $"{frontendUrl}/reset-password?token={token}";
        }
    }
}
