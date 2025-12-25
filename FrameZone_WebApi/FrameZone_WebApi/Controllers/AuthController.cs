using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Security.Claims;
using static FrameZone_WebApi.DTOs.GoogleAuthDtos;


namespace FrameZone_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IPasswordService _passwordService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, IPasswordService passwordService, ILogger<AuthController> logger, IGoogleAuthService googleAuthService)
        {
            _authService = authService;
            _passwordService = passwordService;
            _logger = logger;
            _googleAuthService = googleAuthService;

        }

        // =========== 登入相關 API ===========

        /// <summary>
        /// 使用者登入
        /// </summary>
        /// <param name="request">登入請求資料</param>
        /// <returns>登入結果</returns>

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                // 基本驗證
                if (!ModelState.IsValid)
                {
                    // 取得第一個錯誤訊息
                    var firstError = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .FirstOrDefault()?.ErrorMessage ?? "輸入資料不正確";

                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = firstError
                    });
                }

                // 呼叫 Service 處理登入邏輯
                var response = await _authService.LoginAsync(request);

                // 根據登入結果回傳適當的 HTTP 狀態碼
                if (response.Success)
                {
                    _logger.LogWarning($"使用者 {response.Account} 登入成功");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"登入失敗: {response.Message}");
                    return Unauthorized(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登入時發生錯誤");
                return StatusCode(500, new LoginResponseDto
                {
                    Success = false,
                    Message = "系統錯誤,請稍後再試"
                });
            }
        }

        // =========== 註冊相關 API ===========

        /// <summary>
        /// 使用者註冊
        /// </summary>
        /// <param name="request">註冊請求資料</param>
        /// <returns>註冊結果</returns>

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // 取得第一個錯誤訊息
                    var firstError = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .FirstOrDefault()?.ErrorMessage ?? "輸入資料不正確";

                    return BadRequest(new RegisterResponseDto
                    {
                        Success = false,
                        Message = firstError
                    });
                }

                // 呼叫 Service 處理註冊邏輯
                var response = await _authService.RegisterAsync(request);

                // 根據結果回傳適當的 HTTP 狀態碼
                if (response.Success)
                {
                    _logger.LogInformation($"新使用者註冊成功: {request.Account}");
                    return CreatedAtAction(
                        nameof(Register),
                        new { id = response.UserId },
                        response
                    );
                }
                else
                {
                    _logger.LogWarning($"註冊失敗: {response.Message}");
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "註冊時發生錯誤");

                return StatusCode(500, new RegisterResponseDto
                {
                    Success = false,
                    Message = "系統錯誤,請稍後再試"
                });
            }
        }


        // =========== 密碼重設相關 API ===========

        /// <summary>
        /// 忘記密碼
        /// </summary>
        /// <param name="request">忘記密碼請求</param>
        /// <returns>處理結果</returns>

        // POST: api/Auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // 取得第一個錯誤訊息
                    var firstError = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .FirstOrDefault()?.ErrorMessage ?? "輸入資料不正確";

                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = firstError
                    });
                }

                // 呼叫 Service 處理忘記密碼邏輯
                var response = await _passwordService.ForgotPasswordAsync(request);

                return Ok(response);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "忘記密碼時發生錯誤");
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "系統錯誤,請稍後再試"
                });
            }
        }

        /// <summary>
        /// 重設密碼
        /// </summary>
        /// <param name="request">重設密碼請求</param>
        /// <returns>處理結果</returns>

        // POST: api/Auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .FirstOrDefault()?.ErrorMessage ?? "輸入資料不正確";

                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = firstError
                    });
                }

                // 呼叫 PasswordService 處理重設密碼邏輯
                var response = await _passwordService.ResetPasswordAsync(request);

                if (response.Success)
                {
                    _logger.LogInformation("密碼重設成功");
                    return Ok(response);
                }
                else
                {
                    _logger.LogInformation($"密碼重設失敗: {response.Message}");
                    return BadRequest(response);
                }
            }

            catch (Exception ex)
            {
                _logger.LogInformation(ex, "重設密碼時發生錯誤");
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 變更密碼 (需先登入)
        /// </summary>
        /// <param name="request">變更密碼請求</param>
        /// <returns>處理結果</returns>

        // POST: api/Auth/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .FirstOrDefault()?.ErrorMessage ?? "輸入資料不正確";

                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = firstError
                    });
                }

                // 從 JWT Token 取得使用者 ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("無法從 Token 取得使用者 ID");
                    return Unauthorized(new ApiResponseDto
                    {
                        Success = false,
                        Message = "未授權的請求"
                    });
                }

                var response = await _passwordService.ChangePasswordAsync(userId, request);

                if (response.Success)
                {
                    _logger.LogInformation($"密碼變更成功: UserId={userId}");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"密碼變更失敗: userId={userId}, Message={response.Message}");
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "變更密碼時發生錯誤");
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                });
            }

        }

        [HttpGet("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = "Token 不能為空"
                    });
                }

                var response = await _passwordService.ValidateResetTokenAsync(token);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證重設密碼 Token 時發生錯誤");
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後在試"
                });
            }
        }

        /// <summary>
        /// 測試 API 是否正常運作
        /// </summary>
        /// <returns>測試訊息</returns>

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                success = true,
                message = "測試成功",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        [HttpGet("generate-password")]
        public IActionResult GeneratePassword([FromQuery] string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return BadRequest("請提供密碼");
            }

            var hashedPassword = PasswordHelper.HashPassword(password);

            return Ok(new
            {
                原始密碼 = password,
                加密後密碼 = hashedPassword,
                說明 = "請將加密後的密碼更新到資料庫的 Password 欄位"
            });
        }

        // =========== Google 登入相關 API ===========

        /// <summary>
        /// 使用 Google 登入
        /// </summary>
        /// <param name="request">Google 登入請求</param>
        /// <returns>登入結果</returns>
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .FirstOrDefault()?.ErrorMessage ?? "輸入資料不正確";

                    return BadRequest(new GoogleLoginResponseDto
                    {
                        Success = false,
                        Message = firstError
                    });
                }

                // 呼叫 GoogleAuthService 處理登入邏輯
                var response = await _googleAuthService.GoogleLoginAsync(request);

                if (response.Success)
                {
                    _logger.LogInformation($"使用者透過 Google 登入成功: {response.Email}");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"Google 登入失敗: {response.Message}");
                    return Unauthorized(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google 登入時發生錯誤");
                return StatusCode(500, new GoogleLoginResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 綁定 Google 帳號 (需先登入)
        /// </summary>
        /// <param name="request">綁定請求</param>
        /// <returns>處理結果</returns>
        [HttpPost("link-google")]
        public async Task<IActionResult> LinkGoogleAccount([FromBody] LinkGoogleAccountRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .FirstOrDefault()?.ErrorMessage ?? "輸入資料不正確";

                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = firstError
                    });
                }

                // 從 JWT Token 取得使用者 ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new ApiResponseDto
                    {
                        Success = false,
                        Message = "未授權的請求"
                    });
                }

                var response = await _googleAuthService.LinkGoogleAccountAsync(userId, request);

                if (response.Success)
                {
                    _logger.LogInformation($"使用者成功綁定 Google 帳號: UserId={userId}");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"綁定 Google 帳號失敗: UserId={userId}, Message={response.Message}");
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "綁定 Google 帳號時發生錯誤");
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 解除 Google 帳號綁定 (需先登入)
        /// </summary>
        /// <param name="request">解除綁定請求</param>
        /// <returns>處理結果</returns>
        [HttpPost("unlink-google")]
        public async Task<IActionResult> UnlinkGoogleAccount([FromBody] UnlinkGoogleAccountRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .FirstOrDefault()?.ErrorMessage ?? "輸入資料不正確";

                    return BadRequest(new ApiResponseDto
                    {
                        Success = false,
                        Message = firstError
                    });
                }

                // 從 JWT Token 取得使用者 ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new ApiResponseDto
                    {
                        Success = false,
                        Message = "未授權的請求"
                    });
                }

                var response = await _googleAuthService.UnlinkGoogleAccountAsync(userId, request);

                if (response.Success)
                {
                    _logger.LogInformation($"使用者成功解除 Google 帳號綁定: UserId={userId}");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning($"解除 Google 帳號綁定失敗: UserId={userId}, Message={response.Message}");
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解除 Google 帳號綁定時發生錯誤");
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 檢查是否已綁定 Google 帳號 (需先登入)
        /// </summary>
        /// <returns>是否已綁定</returns>
        [HttpGet("google-linked")]
        public async Task<IActionResult> IsGoogleLinked()
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new ApiResponseDto
                    {
                        Success = false,
                        Message = "未授權的請求"
                    });
                }

                var isLinked = await _googleAuthService.IsGoogleAccountLinkedAsync(userId);

                return Ok(new
                {
                    success = true,
                    isLinked = isLinked
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查 Google 綁定狀態時發生錯誤");
                return StatusCode(500, new ApiResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                });
            }
        }
    }
}
