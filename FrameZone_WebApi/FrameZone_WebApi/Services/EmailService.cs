using FrameZone_WebApi.Configuration;
using FrameZone_WebApi.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;


namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// Email 服務實作
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(EmailDto emailDto)
        {
            try
            {
                // 建立 MimeMessage 物件
                var message = new MimeMessage();

                // 設定發件者
                message.From.Add(new MailboxAddress(
                    _emailSettings.SenderName,
                    _emailSettings.SenderEmail
                ));

                // 設定收件者
                message.To.Add(new MailboxAddress(
                    emailDto.ToName ?? emailDto.ToEmail,
                    emailDto.ToEmail
                ));

                // 設定主旨
                message.Subject = emailDto.Subject;

                // 建立 Email 內容
                var bodyBuilder = new BodyBuilder();

                if (emailDto.IsHtml)
                {
                    // HTML 內容
                    bodyBuilder.HtmlBody = emailDto.Body;
                }
                else
                {
                    // 純文字內容
                    bodyBuilder.TextBody = emailDto.Body;
                }

                // 使用 SMTPClient 發送 Email
                using var client = new SmtpClient();

                // 連線到 SMTP 伺服器
                await client.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    SecureSocketOptions.StartTls // 使用 TLS 加密
                );

                // 驗證登入
                await client.AuthenticateAsync(
                    _emailSettings.Username,
                    _emailSettings.Password
                );

                // 發送 Email
                await client.SendAsync(message);

                // 中斷連線
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email 發送成功至 {ToEmail}", emailDto.ToEmail);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送 Email 失敗: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 發送驗證碼 Email
        /// </summary>
        public async Task<bool> SendVerificationCodeEmailAsync(VerificationEmailDto verificationEmailDto)
        {
            // 建立 HTML 格式的 Email 內容
            var htmlBody = CreateVerificationCodeEmailBody(
                verificationEmailDto.ToName ?? "使用者",
                verificationEmailDto.VerificationCode,
                verificationEmailDto.ExpirationMinutes,
                verificationEmailDto.Purpose
            );

            // 組合成通用 EmailDto
            var emailDto = new EmailDto
            {
                ToEmail = verificationEmailDto.ToEmail,
                ToName = verificationEmailDto.ToName,
                Subject = $"FrameZone - {verificationEmailDto.Purpose}驗證碼",
                Body = htmlBody,
                IsHtml = true
            };

            return await SendEmailAsync(emailDto);
        }

        /// <summary>
        /// 發送重設密碼 Email
        /// </summary>
        public async Task<bool> SendResetPasswordEmailAsync(ResetPasswordEmailDto resetPasswordEmailDto)
        {
            var htmlBody = CreateResetPasswordEmailBody(
                resetPasswordEmailDto.ToName ?? "使用者",
                resetPasswordEmailDto.ResetUrl,
                resetPasswordEmailDto.ExpirationHours
            );

            var emailDto = new EmailDto
            {
                ToEmail = resetPasswordEmailDto.ToEmail,
                ToName = resetPasswordEmailDto.ToName,
                Subject = "FrameZone - 重設密碼",
                Body = htmlBody,
                IsHtml = true
            };

            return await SendEmailAsync(emailDto);
        }

        private string CreateVerificationCodeEmailBody(
            string userName,
            string verificationCode,
            int expirationMinutes,
            string purpose)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        color: #333;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                    }}
                    .header {{
                        background-color: #206bc4;
                        color: white;
                        padding: 20px;
                        text-align: center;
                    }}
                    .content {{
                        background-color: #f8f9fa;
                        padding: 30px;
                        border-radius: 5px;
                        margin-top: 20px;
                    }}
                    .code-box {{
                        background-color: white;
                        border: 2px dashed #206bc4;
                        padding: 20px;
                        text-align: center;
                        font-size: 32px;
                        font-weight: bold;
                        letter-spacing: 5px;
                        margin: 20px 0;
                        color: #206bc4;
                    }}
                    .footer {{
                        text-align: center;
                        margin-top: 20px;
                        color: #666;
                        font-size: 14px;
                    }}
                    .warning {{
                        color: #d63939;
                        font-weight: bold;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>FrameZone</h1>
                    </div>
                    <div class='content'>
                        <h2>您好，{userName}</h2>
                        <p>您正在進行<strong>{purpose}</strong>，請使用以下驗證碼：</p>
            
                        <div class='code-box'>
                            {verificationCode}
                        </div>
            
                        <p>此驗證碼將在 <span class='warning'>{expirationMinutes} 分鐘</span>後失效。</p>
            
                        <p>如果這不是您的操作，請忽略此郵件。</p>
                    </div>
                    <div class='footer'>
                        <p>© 2024 FrameZone. All rights reserved.</p>
                        <p>這是系統自動發送的郵件，請勿直接回覆。</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string CreateResetPasswordEmailBody(
            string userName,
            string resetUrl,
            int expirationHours)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        line-height: 1.6;
                        color: #333;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                    }}
                    .header {{
                        background-color: #206bc4;
                        color: white;
                        padding: 20px;
                        text-align: center;
                    }}
                    .content {{
                        background-color: #f8f9fa;
                        padding: 30px;
                        border-radius: 5px;
                        margin-top: 20px;
                    }}
                    .button {{
                        display: inline-block;
                        background-color: #206bc4;
                        color: white;
                        padding: 15px 30px;
                        text-decoration: none;
                        border-radius: 5px;
                        margin: 20px 0;
                        font-weight: bold;
                    }}
                    .button:hover {{
                        background-color: #1c5ba8;
                    }}
                    .footer {{
                        text-align: center;
                        margin-top: 20px;
                        color: #666;
                        font-size: 14px;
                    }}
                    .warning {{
                        color: #d63939;
                        font-weight: bold;
                    }}
                    .note {{
                        background-color: #fff3cd;
                        border-left: 4px solid #ffc107;
                        padding: 15px;
                        margin: 20px 0;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>FrameZone</h1>
                    </div>
                    <div class='content'>
                        <h2>您好，{userName}</h2>
                        <p>我們收到了您的密碼重設請求。請點擊下方按鈕來重設您的密碼：</p>
            
                        <div style='text-align: center;'>
                            <a href='{resetUrl}' class='button'>重設密碼</a>
                        </div>
            
                        <div class='note'>
                            <strong>⚠️ 重要提醒：</strong>
                            <ul>
                                <li>此連結將在 <span class='warning'>{expirationHours} 小時</span>後失效</li>
                                <li>為了您的帳號安全，請勿將此連結分享給他人</li>
                                <li>如果按鈕無法點擊，請複製以下連結到瀏覽器：<br>
                                    <code style='word-break: break-all;'>{resetUrl}</code>
                                </li>
                            </ul>
                        </div>
            
                        <p>如果您沒有提出密碼重設請求，請忽略此郵件，您的密碼不會被變更。</p>
                    </div>
                    <div class='footer'>
                        <p>© 2024 FrameZone. All rights reserved.</p>
                        <p>這是系統自動發送的郵件，請勿直接回覆。</p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}
