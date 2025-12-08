using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;

namespace FrameZone_WebApi.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(UserRepository userRepository, JwtHelper jwtHelper,IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        // ========== 登入相關 ===========

        /// <summary>
        /// 使用者登入
        /// </summary>
        /// <param name="request">登入請求相關</param>
        /// <returns>登入結果</returns>
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var user = await _userRepository.GetUserByAccountOrEmailAsync(request.AccountOrEmail);

                // 使用者不存在
                if (user == null)
                {
                    // 記錄登入失敗日誌
                    await LogLoginAttemptAsync(null, false, "帳號不存在");

                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "帳號或密碼錯誤"
                    };
                }

                // 檢查帳號是否被刪除
                if (user.IsDeleted)
                {
                    await LogLoginAttemptAsync(user.UserId, false, "帳號已刪除");
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "此帳號已被停用,請聯繫客服"
                    };
                }

                // 檢查帳號是否被鎖定
                bool isLocked = await _userRepository.IsAccountLockedAsync(user.UserId);
                if (isLocked)
                {
                    await LogLoginAttemptAsync(user.UserId, false, "帳號被鎖定");
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "帳號已被鎖定,請 30 分鐘後再試或聯繫客服"
                    };
                }

                // 檢查帳號狀態
                if (user.AccountStatus != "Active")
                {
                    await LogLoginAttemptAsync(user.UserId, false, $"帳號狀態異常:{user.AccountStatus}");
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "帳號狀態異常,請聯繫客服"
                    };
                }

                // 驗證密碼
                bool isPasswordValid = PasswordHelper.VerifyPassword(request.Password, user.Password);
                if (!isPasswordValid)
                {
                    // 密碼錯誤，紀錄失敗次數
                    await _userRepository.IncrementFailedLoginAttemptsAsync(user.UserId);
                    await LogLoginAttemptAsync(user.UserId, false, "密碼錯誤");

                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "帳號或密碼錯誤"
                    };
                }

                // 產生 JWT Token
                string token = _jwtHelper.GenerateJwtToken(
                    userId: user.UserId,
                    account: user.Account,
                    email: user.Email,
                    rememberMe: request.RememberMe
                );

                // 重設失敗次數
                await _userRepository.ResetFailedLoginAttemptsAsync(user.UserId);

                // 記錄登入成功日誌
                await LogLoginAttemptAsync(user.UserId, true, "登入成功");

                // 建立使用者會話
                await CreateUserSessionAsync(user.UserId, token, request.RememberMe);

                // 回傳登入成功結果
                return new LoginResponseDto
                {
                    Success = true,
                    Message = "登入成功",
                    Token = token,
                    UserID = user.UserId,
                    Account = user.Account,
                    Email = user.Email,
                    DisplayName = user.UserProfile?.DisplayName ?? user.Account,
                    Avatar = user.UserProfile?.Avatar
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"登入錯誤: {ex.Message}");

                // Log the exception (not implemented here)
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "登入過程中發生錯誤: " + ex.Message
                };
            }
        }

        /// <summary>
        /// 使用者註冊
        /// </summary>
        /// <param name="request">註冊請求資料</param>
        /// <returns>註冊結果</returns>
        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                // 檢查密碼是否相符
                if (request.Password != request.ConfirmPassword)
                {
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "密碼與確認密碼不符"
                    };
                }

                // 檢查密碼強度
                if (!PasswordHelper.IsPasswordStrong(request.Password))
                {
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "密碼強度不足,必須包含英文字母和數字,長度 6-50 個字元"
                    };
                }

                // 檢查帳號
                bool accountExists = await _userRepository.IsAccountExistsAsync(request.Account);
                if (accountExists)
                {
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "此帳號已被使用"
                    };
                }

                // 檢查 Email
                bool emailExists = await _userRepository.IsEmailExistsAsync(request.Email);
                if (emailExists)
                {
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "此 Email 已被註冊"
                    };
                }

                // 加密密碼
                string hashedPassword = PasswordHelper.HashPassword(request.Password);

                // 建立新使用者物件
                var newUser = new User
                {
                    Account = request.Account,
                    Email = request.Email,
                    Phone = request.Phone,
                    Password = hashedPassword,
                    AccountType = "Email",              // 註冊方式: Email
                    AccountStatus = "Active",           // 帳號狀態: 啟用
                    IsDeleted = false,                  // 未刪除
                    RegistrationSource = "Web",         // 註冊來源: 網站
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                // 新增使用者到資料庫
                bool createUserSession = await _userRepository.CreateUserAsync(newUser);

                if (!createUserSession)
                {
                    return new RegisterResponseDto
                    {
                        Success = false,
                        Message = "註冊失敗,請稍後再試"
                    };
                }

                // 建立使用者檔案
                var userProfile = new UserProfile
                {
                    UserId = newUser.UserId,
                    DisplayName = request.Account,      // 預設顯示名稱為帳號
                    Avatar = null,                      // 預設無頭像
                    CoverImage = null,                  // 預設沒有封面
                    Bio = null,                         // 預設沒有個人簡介
                    Website = null,
                    Location = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                bool createProfileSuccess = await _userRepository.CreateUserProfileAsync(userProfile);

                // 紀錄註冊日誌
                await LogUserActionAsync(
                    userId: newUser.UserId,
                    actionType: "Register",
                    actionCategory: "Account",
                    status: "Success",
                    description: "使用者註冊成功"
                );

                // 回傳註冊成功結果
                return new RegisterResponseDto
                {
                    Success = true,
                    Message = "註冊成功",
                    UserId = newUser.UserId
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"註冊錯誤: {ex.Message}");

                return new RegisterResponseDto
                {
                    Success = false,
                    Message = "系統錯誤,請稍後再試"
                };
            }
        }

        // ========== 輔助方法 ===========

        /// <summary>
        /// 紀錄登入嘗試日誌
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <param name="success">是否成功</param>
        /// <param name="description">描述</param>
        private async Task LogLoginAttemptAsync(long? userId, bool success, string description)
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
                    ActionType = "Login",
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
            catch{}

        }

        /// <summary>
        /// 紀錄使用者操作日誌
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="actionType"></param>
        /// <param name="actionCategory"></param>
        /// <param name="status"></param>
        /// <param name="description"></param>
        /// <returns></returns>
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
            catch{}
        }


        /// <summary>
        /// 建立使用者會話
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="rememberMe"></param>
        /// <returns></returns>
        private async Task CreateUserSessionAsync(long userId, string token, bool rememberMe)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                string userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";

                // 計算過期時間
                int expirationMinutes = rememberMe ? 7 : 1;

                var userSession = new UserSession
                {
                    UserId = userId,
                    UserAgent = userAgent,                                          // 活耀中  
                    IsActive = true,                                                // 最後活動時間
                    LastActivityAt = DateTime.UtcNow,                               // 過期時間
                    ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    CreatedAt = DateTime.UtcNow,
                };

                await _userRepository.CreateUserSessionAsync(userSession);

            }
            catch{}
        }


        /// <summary>
        /// 從 User-Agent 判斷裝置類型
        /// </summary>
        /// <param name="userAgent">User-Agent 字串</param>
        /// <returns>裝置類型</returns>
        private string GetDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            userAgent = userAgent.ToLower();

            // 判斷是否為行動裝置
            if (userAgent.Contains("mobile") || 
                userAgent.Contains("android") || 
                userAgent.Contains("iphone") || 
                userAgent.Contains("ipad"))
            {
                if (userAgent.Contains("android"))
                    return "android";
                if (userAgent.Contains("iphone") || userAgent.Contains("ipad"))
                    return "ios";

                return "mobile";
            }

            // 判斷是否為平板
            if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
                return "Tablet";

            // 預設為桌面裝置
            return "Desktop";

        }
    }
}
