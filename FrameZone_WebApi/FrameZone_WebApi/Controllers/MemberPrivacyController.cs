using FrameZone.API.DTOs.Member;
using FrameZone.API.Services.Interfaces;
using FrameZone_WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone.API.Controllers
{
    /// <summary>
    /// 會員隱私設定 API
    /// </summary>
    [ApiController]
    [Route("api/member/privacy")]
    [Authorize]
    public class MemberPrivacyController : ControllerBase
    {
        private readonly IMemberPrivacyService _privacyService;
        private readonly ILogger<MemberPrivacyController> _logger;

        public MemberPrivacyController(
            IMemberPrivacyService privacyService,
            ILogger<MemberPrivacyController> logger)
        {
            _privacyService = privacyService;
            _logger = logger;
        }

        /// <summary>
        /// 取得隱私設定
        /// </summary>
        /// <returns>隱私設定列表</returns>
        [HttpGet("settings")]
        [ProducesResponseType(typeof(PrivacySettingsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPrivacySettings()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "無效的身份驗證" });
                }

                var result = await _privacyService.GetPrivacySettingsAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得隱私設定時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "取得隱私設定時發生錯誤"
                });
            }
        }

        /// <summary>
        /// 批次更新隱私設定
        /// </summary>
        /// <param name="dto">批次更新設定 DTO</param>
        /// <returns>更新結果</returns>
        [HttpPut("settings")]
        [ProducesResponseType(typeof(PrivacySettingsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePrivacySettings([FromBody] BatchUpdatePrivacySettingsDto dto)
        {
            try
            {
                // 驗證模型
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "資料驗證失敗",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { success = false, message = "無效的身份驗證" });
                }

                var result = await _privacyService.BatchUpdatePrivacySettingsAsync(userId, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新隱私設定時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "更新隱私設定時發生錯誤"
                });
            }
        }
    }
}