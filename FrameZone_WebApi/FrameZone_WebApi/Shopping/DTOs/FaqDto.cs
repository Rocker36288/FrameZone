namespace FrameZone_WebApi.Shopping.DTOs
{
    /// <summary>
    /// 常見問題資料傳輸物件
    /// </summary>
    public class FaqDto
    {
        /// <summary>
        /// FAQ ID
        /// </summary>
        public int FaqId { get; set; }

        /// <summary>
        /// 系統 ID
        /// </summary>
        public int SystemId { get; set; }

        /// <summary>
        /// 分類
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 問題
        /// </summary>
        public string Question { get; set; } = string.Empty;

        /// <summary>
        /// 答案
        /// </summary>
        public string Answer { get; set; } = string.Empty;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
