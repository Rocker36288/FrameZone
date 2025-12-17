using Microsoft.CodeAnalysis.Scripting;

namespace FrameZone_WebApi.Helpers
{
    public class PasswordHelper
    {
        /// <summary>
        /// 加密密碼
        /// </summary>
        /// <param name="password"></param>
        /// <returns>加密後的密碼雜湊值</returns>
        public static string HashPassword(string password)
        {
            // 使用 BCrypt 加密密碼
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// 驗證密碼是否正確
        /// </summary>
        /// <param name="password">使用者輸入的密碼</param>
        /// <param name="hashedPassword">資料庫儲存的加密密碼</param>
        /// <returns>密碼正確回傳 true，否則回傳 false</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // 使用 BCrypt 驗證密碼
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                // 如果 hashedPassword 格式錯誤，回傳 false
                return false;
            }
        }

        /// <summary>
        /// 檢查密碼強度是否符合要求
        /// </summary>
        /// <param name="password">要求檢查的密碼</param>
        /// <returns>符合要求回傳 true，否則回傳 false</returns>
        public static bool IsPasswordStrong(string password)
        {
            // 檢查密碼長度
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            };

            if (password.Length < 6 || password.Length > 50)
            {
                return false;
            }

            // 檢查是否包含至少一個英文字母
            bool hasLetter = password.Any(char.IsLetter);

            // 檢查是否包含至少一個數字
            bool hasDigit = password.Any(char.IsDigit);

            // 必須符合兩個條件
            return hasLetter && hasDigit;
        }

        /// <summary>
        /// 產生隨機密碼
        /// </summary>
        /// <param name="length">密碼長度 (預設12個字)</param>
        /// <returns>隨機產生的密碼</returns>
        public static string GenerateRandomPassword(int length = 12)
        {
            // 定義密碼可使用的字元集
            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*";

            // 合併所有字元
            string allChars = upperCase + lowerCase + digits+ specialChars;

            // 使用亂數產生器
            var random = new Random();
            var password = new char[length];

            // 確保密碼至少包含每種類型的字元各一個
            password[0] = upperCase[random.Next(upperCase.Length)];         // 至少一個大寫
            password[1] = lowerCase[random.Next(lowerCase.Length)];         // 至少一個小寫
            password[2] = digits[random.Next(digits.Length)];               // 至少一個數字
            password[3] = specialChars[random.Next(specialChars.Length)];   // 至少一個特殊符號

            // 剩餘的字元從所有字元中隨機選取
            for (int i = 4; i < length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }

            // 打亂順序
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }
    }
}
