using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FrameZone_WebApi.Helpers
{
    /// <summary>
    /// JWT Token 輔助類別
    /// </summary>
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 產生 JWT Token
        /// </summary>
        /// <param name="userId">使用者ID</param>
        /// <param name="account">帳號</param>
        /// <param name="email">Email</param>
        /// <param name="rememberMe">是否記住我</param>
        /// <returns>JWT Token字串</returns>
        public string GenerateJwtToken(long userId, string account, string email, bool rememberMe)
        {
            // 從 appsettings.json 讀取 JWT 設定
            var secretKey = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey 未設定");
            var issuer = _configuration["Jwt:Issuer"] ?? "FrameZone";
            var audience = _configuration["Jwt:Audience"] ?? "FrameZone_Users";

            // 建立安全密鑰
            // 把自動密鑰轉成 byte[] 陣列
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // 建立簽章密鑰
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // 建立 Claims (宣告)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),          // Subject: 使用者ID
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  // JWT ID: 唯一識別碼
                new Claim("account", account),                                      // 帳號
                new Claim("email", email),                                          // 密碼
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),            // 使用者識別碼
                new Claim(ClaimTypes.Name, account)                                 // 使用者名稱
            };

            // 設定 Token 過期時間
            var expirationMinutes = rememberMe
                ? int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7") * 24 * 60
                : int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "1440");

            var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

            // 建立 JWT Token
            var token = new JwtSecurityToken(
                issuer: issuer,                 // 發行者
                audience: audience,             // 接收者
                claims: claims,                 // 使用者資訊
                expires: expiration,            // 過期時間
                signingCredentials: credentials // 簽章憑證
            );

            // Token 物件轉成字串並回傳
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        /// <summary>
        /// 驗證 JWT Token 是否有效
        /// </summary>
        /// <param name="token">要驗證的 Token</param>
        /// <returns>驗證結果的 Principal</returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                // 讀取設定
                var secretKey = _configuration["Jwt:SecretKey"]
                    ?? throw new InvalidOperationException("JWT SecretKey 未設定");
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];

                // 建立驗證參數
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,              // 驗證發行者
                    ValidateAudience = true,            // 驗證接收者
                    ValidateLifetime = true,            // 驗證過期時間
                    ValidateIssuerSigningKey = true,    // 驗證簽章金鑰
                    ValidIssuer = issuer,               // 有效的發行者
                    ValidAudience = audience,           // 有效的接收者
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)
                    ),
                    ClockSkew = TimeSpan.Zero           // 時間偏移量設置0 (預設為5分鐘，時間到Token就過期         )
                };

                // 驗證 Token
                var tokenHander = new JwtSecurityTokenHandler();
                var principal = tokenHander.ValidateToken(
                    token,
                    tokenValidationParameters,
                    out SecurityToken validatedToken
                );

                // 確認 Token 使用正確的加密演算法
                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }

                return null;
            }
            catch
            {
                // Token 驗證失敗
                return null;
            }
        }
        
        /// <summary>
        /// Token 中取得使用者ID
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <returns>使用者ID，如果取得失敗則回傳 null</returns>
        public long? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
            {
                return null;
            }

            // Claims 中取得使用者ID
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (long.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }
    }
}
