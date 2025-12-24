using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Middlewares
{
    public class QueryStringTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<QueryStringTokenMiddleware> _logger;
        public QueryStringTokenMiddleware(RequestDelegate next, ILogger<QueryStringTokenMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 只處理特定路徑 (縮圖 API)
            if (context.Request.Path.StartsWithSegments("/api/photos") &&
                context.Request.Path.Value.Contains("/thumbnail"))
            {
                // 檢查 URL 參數中是否有 token
                var token = context.Request.Query["token"].FirstOrDefault();

                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        // 驗證 Token 並設定到 header
                        context.Request.Headers["Authorization"] = $"Bearer {token}";

                        _logger.LogDebug("從 URL 參數取得 Token 並設定到 Header");
                    } 
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "URL Token 驗證失敗");
                    }
                }
            }

            await _next(context);
        }

    }
    /// <summary>
    /// Middleware 擴展方法
    /// </summary>
    public static class QueryStringTokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseQueryStringToken(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<QueryStringTokenMiddleware>();
        }
    }
}
