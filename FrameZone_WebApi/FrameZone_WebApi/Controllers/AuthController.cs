using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;


namespace FrameZone_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
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
            // TODO: 實作忘記密碼邏輯

            return Ok(new ApiResponseDto
            {
                Success = false,
                Message = "密碼重設功能尚未實作"
            });
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
            // TODO: 實作重設密碼邏輯

            return Ok(new ApiResponseDto
            {
                Success = false,
                Message = "密碼重設功能尚未實作"
            });
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
            // TODO: 實作變更密碼邏輯

            return Ok(new ApiResponseDto
            {
                Success = false,
                Message = "變更密碼功能尚未實作"
            });
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

    }
}
