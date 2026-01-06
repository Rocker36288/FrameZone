using FrameZone_WebApi.Configuration;
using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using System.Text.Json;
using static FrameZone_WebApi.DTOs.GoogleAuthDtos;

namespace FrameZone_WebApi.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly GoogleAuthSettings _googleSettings;
        private readonly IConfiguration _configuration;
        private readonly string _googleClientId;
        private readonly string _googleClientSecret;
        private readonly UserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GoogleAuthService> _logger;
        private readonly HttpClient _httpClient;

        public GoogleAuthService(
            IOptions<GoogleAuthSettings> googleSettings,
            IConfiguration configuration,
            UserRepository userRepository,
            JwtHelper jwtHelper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GoogleAuthService> logger,
            HttpClient httpClient)
        {
            _googleSettings = googleSettings.Value;
            _configuration = configuration;
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _httpClient = httpClient;

            // 從 appsettings 讀取 Google OAuth credentials（建議放在 GoogleAuth:ClientId / GoogleAuth:ClientSecret）
            // 保留對舊 Key 的相容（GoogleClientId / GoogleClientSecret），避免尚未完全移除舊流程時造成中斷
            _googleClientId = !string.IsNullOrWhiteSpace(_googleSettings.ClientId)
                ? _googleSettings.ClientId
                : _configuration["GoogleClientId"];

            _googleClientSecret = !string.IsNullOrWhiteSpace(_googleSettings.ClientSecret)
                ? _googleSettings.ClientSecret
                : _configuration["GoogleClientSecret"];
            if (string.IsNullOrEmpty(_googleClientId))
            {
                _logger.LogWarning("⚠️ Google Client ID 未設定，Google 登入功能將無法使用");
            }
            else
            {
                _logger.LogInformation("✅ Google Client ID 已從 appsettings 載入，長度: {Length}", _googleClientId.Length);
            }

            if (string.IsNullOrEmpty(_googleClientSecret))
            {
                _logger.LogWarning("⚠️ Google Client Secret 未設定");
            }
            else
            {
                _logger.LogInformation("✅ Google Client Secret 已從 appsettings 載入，長度: {Length}", _googleClientSecret.Length);
            }
        }

        public async Task<GoogleLoginResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
        {
            try
            {
                // 檢查 Google 登入功能是否啟用
                if (!_googleSettings.Enabled)
                {
                    return new GoogleLoginResponseDto
                    {
                        Success = false,
                        Message = "Google 登入功能未啟用。"
                    };
                }

                // 驗證 Google Token 並取得使用者資訊
                var googleUserInfo = await ValidateGoogleTokenAsync(request.IdToken);
                if (googleUserInfo == null)
                {
                    await LogGoogleLoginAttemptAsync(null, false, "Google Token 驗證失敗");
                    return new GoogleLoginResponseDto
                    {
                        Success = false,
                        Message = "Google 登入驗證失敗，請重試"
                    };
                }

                // 檢查是否已有此 Google 帳號的綁定紀錄
                var thirdPartyAuth = await _userRepository.GetThirdPartyAuthByProviderIdAsync("Google", googleUserInfo.Sub);

                User user;
                bool isNewUser = false;

                if (thirdPartyAuth != null)
                {
                    // 已綁定 - 取得使用者資訊
                    user = await _userRepository.GetUserByIdAsync(thirdPartyAuth.UserId);
                    if (user == null)
                    {
                        return new GoogleLoginResponseDto
                        {
                            Success = false,
                            Message = "使用者帳號異常，請聯繫客服"
                        };
                    }

                    await _userRepository.UpdateThirdPartyAuthLastUsedAsync(thirdPartyAuth.AuthId);
                }
                else
                {
                    var existingUser = await _userRepository.GetUserByEmailAsync(googleUserInfo.Email);

                    if (existingUser != null)
                    {
                        // Email 已存在 - 自動綁定到現有帳號
                        user = existingUser;

                        // 建立第三方認證綁定
                        await CreateThirdPartyAuthAsync(user.UserId, googleUserInfo, request.IdToken);

                        _logger.LogInformation($"Google 帳號已自動綁定到現有使用者: UserId={user.UserId}, Email={user.Email}");
                    }
                    else
                    {
                        // Email 不存在 - 建立新使用者
                        user = await CreateNewGoogleUserAsync(googleUserInfo);
                        isNewUser = true;

                        // 建立第三方認證綁定
                        await CreateThirdPartyAuthAsync(user.UserId, googleUserInfo, request.IdToken);

                        _logger.LogInformation($"新使用者透過 Google 註冊: UserId={user.UserId}, Email={user.Email}");
                    }
                }

                // 檢查帳號狀態
                if (user.IsDeleted)
                {
                    await LogGoogleLoginAttemptAsync(user.UserId, false, "帳號已被刪除");
                    return new GoogleLoginResponseDto
                    {
                        Success = false,
                        Message = "此帳號已被停用，請聯繫客服"
                    };
                }

                if (user.AccountStatus != "Active")
                {
                    await LogGoogleLoginAttemptAsync(user.UserId, false, $"帳號狀態異常: {user.AccountStatus}");
                    return new GoogleLoginResponseDto
                    {
                        Success = false,
                        Message = "此帳號狀態異常，請聯繫客服"
                    };
                }

                // 產生 JWT Token
                string token = _jwtHelper.GenerateJwtToken(
                    userId: user.UserId,
                    account: user.Account,
                    email: user.Email,
                    rememberMe: request.RememberMe
                );

                // 記錄登入成功日誌
                await LogGoogleLoginAttemptAsync(user.UserId, true, "Google 登入成功");

                // 建立使用者會話
                await CreateUserSessionAsync(user.UserId, token, request.RememberMe);

                return new GoogleLoginResponseDto
                {
                    Success = true,
                    Message = isNewUser ? "歡迎加入 FrameZone！" : "登入成功",
                    Token = token,
                    UserId = user.UserId,
                    Account = user.Account,
                    Email = user.Email,
                    DisplayName = user.UserProfile?.DisplayName ?? user.Account,
                    Avatar = user.UserProfile?.Avatar ?? googleUserInfo.Picture,
                    IsNewUser = isNewUser
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google 登入時發生錯誤");
                return new GoogleLoginResponseDto
                {
                    Success = false,
                    Message = "Google 登入失敗，請稍後再試"
                };
            }
        }

        public async Task<GoogleUserInfoDto> ValidateGoogleTokenAsync(string idToken)
        {
            try
            {
                _logger.LogInformation("====== 開始驗證 Google ID Token ======");
                _logger.LogInformation("Client ID: {ClientId}", _googleSettings.ClientId);

                // 使用 Google.Apis.Auth 驗證 ID Token
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string> { _googleClientId }
                };

                GoogleJsonWebSignature.Payload payload;

                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                }
                catch (InvalidJwtException ex)
                {
                    _logger.LogWarning("Google ID Token 驗證失敗: {Error}", ex.Message);
                    return null;
                }

                _logger.LogInformation("✓ Token 驗證成功");
                _logger.LogInformation("使用者 Email: {Email}", payload.Email);
                _logger.LogInformation("使用者名稱: {Name}", payload.Name);
                _logger.LogInformation("Email 已驗證: {EmailVerified}", payload.EmailVerified);
                _logger.LogInformation("===================================");

                // 檢查 Email 是否已驗證
                if (!payload.EmailVerified)
                {
                    _logger.LogWarning("Google 帳號的 Email 未經驗證");
                    return null;
                }

                // 轉換為我們的 DTO
                return new GoogleUserInfoDto
                {
                    Sub = payload.Subject,
                    Email = payload.Email,
                    EmailVerified = payload.EmailVerified,
                    Name = payload.Name ?? string.Empty,
                    GivenName = payload.GivenName ?? string.Empty,
                    FamilyName = payload.FamilyName ?? string.Empty,
                    Picture = payload.Picture ?? string.Empty,
                    Locale = payload.Locale ?? "zh-TW"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證 Google Token 時發生錯誤");
                return null;
            }
        }

        private async Task<GoogleUserInfoDto?> GetGoogleUserInfoAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var response = await _httpClient.GetAsync(_googleSettings.UserInfoUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("取得 Google 使用者資訊失敗: {StatusCode}", response.StatusCode);
                    return null;
                }

                var userInfoJson = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<GoogleUserInfoDto>(userInfoJson, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得 Google 使用者資訊時發生錯誤");
                return null;
            }
        }

        /// <summary>
        /// 建立新的 Google 使用者
        /// </summary>
        private async Task<User> CreateNewGoogleUserAsync(GoogleUserInfoDto googleUserInfo)
        {
            // 產生唯一的帳號名稱
            var accountPrefix = googleUserInfo.Email.Split('@')[0];
            var account = await GenerateUniqueAccountAsync(accountPrefix);

            var newUser = new User
            {
                Account = account,
                Email = googleUserInfo.Email,
                Password = PasswordHelper.HashPassword(Guid.NewGuid().ToString()), // 隨機密碼
                AccountType = "Google",
                AccountStatus = "Active",
                RegistrationSource = "Google",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 建立使用者
            await _userRepository.CreateUserAsync(newUser);

            // 建立使用者檔案
            var userProfile = new UserProfile
            {
                UserId = newUser.UserId,
                DisplayName = googleUserInfo.Name,
                Avatar = googleUserInfo.Picture,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateUserProfileAsync(userProfile);

            // 紀錄註冊日誌
            await LogUserActionAsync(
                userId: newUser.UserId,
                actionType: "Register",
                actionCategory: "Account",
                status: "Success",
                description: "使用者透過 Google 註冊成功"
            );

            return newUser;
        }

        /// <summary>
        /// 建立第三方認證綁定紀錄
        /// </summary>
        private async Task CreateThirdPartyAuthAsync(long userId, GoogleUserInfoDto googleUserInfo, string accessToken)
        {
            var thirdPartyAuth = new UserThirdPartyAuth
            {
                UserId = userId,
                Provider = "Google",
                ProviderId = googleUserInfo.Sub,
                ProviderEmail = googleUserInfo.Email,
                ProviderName = googleUserInfo.Name,
                ProviderAvatar = googleUserInfo.Picture,
                AccessToken = accessToken,
                IsActive = true,
                IsPrimary = true,
                LastUsedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateThirdPartyAuthAsync(thirdPartyAuth);
        }

        /// <summary>
        /// 產生唯一的帳號名稱
        /// </summary>
        private async Task<string> GenerateUniqueAccountAsync(string prefix)
        {
            // 清理前綴
            prefix = new string(prefix.Where(c => char.IsLetterOrDigit(c)).ToArray());
            if (string.IsNullOrEmpty(prefix))
            {
                prefix = "user";
            }

            var account = prefix;
            var counter = 1;

            while (await _userRepository.IsAccountExistsAsync(account))
            {
                account = $"{prefix}{counter}";
                counter++;
            }

            return account;
        }

        /// <summary>
        /// 綁定 Google 帳號
        /// </summary>
        public async Task<ApiResponseDto> LinkGoogleAccountAsync(long userId, LinkGoogleAccountRequestDto request)
        {
            try
            {
                // 驗證 Google Token
                var googleUserInfo = await ValidateGoogleTokenAsync(request.IdToken);
                if (googleUserInfo == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "Google Token 驗證失敗"
                    };
                }

                // 檢查此 Google 帳號是否已被其他使用者綁定
                var existingAuth = await _userRepository.GetThirdPartyAuthByProviderIdAsync("Google", googleUserInfo.Sub);
                if (existingAuth != null && existingAuth.UserId != userId)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "此 Google 帳號已被其他使用者綁定"
                    };
                }

                if (existingAuth != null && existingAuth.UserId == userId)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "您已經綁定過此 Google 帳號"
                    };
                }

                await CreateThirdPartyAuthAsync(userId, googleUserInfo, request.IdToken);

                await LogUserActionAsync(
                    userId: userId,
                    actionType: "LinkAccount",
                    actionCategory: "Security",
                    status: "Success",
                    description: "成功綁定 Google 帳號"
                );

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Google 帳號綁定成功"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "綁定 Google 帳號時發生錯誤");
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "綁定失敗，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 解除 Google 帳號綁定
        /// </summary>
        public async Task<ApiResponseDto> UnlinkGoogleAccountAsync(long userId, UnlinkGoogleAccountRequestDto request)
        {
            try
            {
                // 取得使用者資訊
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "使用者不存在"
                    };
                }

                // 驗證當前密碼
                if (!PasswordHelper.VerifyPassword(request.CurrentPassword, user.Password))
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "密碼驗證失敗"
                    };
                }

                // 檢查是否有綁定 Google 帳號
                var thirdPartyAuth = await _userRepository.GetThirdPartyAuthByUserIdAndProviderAsync(userId, "Google");
                if (thirdPartyAuth == null)
                {
                    return new ApiResponseDto
                    {
                        Success = false,
                        Message = "您尚未綁定 Google 帳號"
                    };
                }

                // 刪除綁定
                await _userRepository.DeleteThirdPartyAuthAsync(thirdPartyAuth.AuthId);

                await LogUserActionAsync(
                    userId: userId,
                    actionType: "UnlinkAccount",
                    actionCategory: "Security",
                    status: "Success",
                    description: "成功解除 Google 帳號綁定"
                );

                return new ApiResponseDto
                {
                    Success = true,
                    Message = "Google 帳號解除綁定成功"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解除 Google 帳號綁定時發生錯誤");
                return new ApiResponseDto
                {
                    Success = false,
                    Message = "解除綁定失敗，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 檢查是否已綁定 Google 帳號
        /// </summary>
        public async Task<bool> IsGoogleAccountLinkedAsync(long userId)
        {
            var thirdPartyAuth = await _userRepository.GetThirdPartyAuthByUserIdAndProviderAsync(userId, "Google");
            return thirdPartyAuth != null && thirdPartyAuth.IsActive;
        }

        // ========== 輔助方法 ===========

        /// <summary>
        /// 記錄 Google 登入嘗試日誌
        /// </summary>
        private async Task LogGoogleLoginAttemptAsync(long? userId, bool success, string description)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                string ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                string userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";

                var userLog = new UserLog
                {
                    UserId = userId,
                    Status = success ? "Success" : "Failure",
                    ActionType = "GoogleLogin",
                    ActionCategory = "Security",
                    ActionDescription = description,
                    TargetType = "User",
                    TargetId = userId,
                    Ipaddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceType = GetDeviceType(userAgent),
                    SystemName = "FrameZone",
                    Severity = success ? "Info" : "Warning",
                    ErrorMessage = success ? null : description,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateUserLogAsync(userLog);
            }
            catch { }
        }

        /// <summary>
        /// 記錄使用者操作日誌
        /// </summary>
        private async Task LogUserActionAsync(long userId, string actionType, string actionCategory, string status, string description)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                string ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                string userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";

                var userLog = new UserLog
                {
                    UserId = userId,
                    Status = status,
                    ActionType = actionType,
                    ActionCategory = actionCategory,
                    ActionDescription = description,
                    Ipaddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceType = GetDeviceType(userAgent),
                    SystemName = "FrameZone",
                    Severity = "Info",
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateUserLogAsync(userLog);
            }
            catch { }
        }

        /// <summary>
        /// 建立使用者會話
        /// </summary>
        private async Task CreateUserSessionAsync(long userId, string token, bool rememberMe)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                string userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";

                int expirationMinutes = rememberMe ? 7 * 24 * 60 : 24 * 60;

                var userSession = new UserSession
                {
                    UserId = userId,
                    UserAgent = userAgent,
                    IsActive = true,
                    LastActivityAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateUserSessionAsync(userSession);
            }
            catch { }
        }

        /// <summary>
        /// 從 User-Agent 判斷裝置類型
        /// </summary>
        private string GetDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone") || userAgent.Contains("ipad"))
            {
                if (userAgent.Contains("android"))
                    return "Android";
                if (userAgent.Contains("iphone") || userAgent.Contains("ipad"))
                    return "iOS";
                return "Mobile";
            }

            if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
                return "Tablet";

            return "Desktop";
        }
    }
}
