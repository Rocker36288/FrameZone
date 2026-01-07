namespace FrameZone_WebApi.Shopping.DTOs
{
    /// <summary>
    /// 收件地址資料傳輸物件
    /// </summary>
    public class ReceivingAddressDto
    {
        /// <summary>
        /// 地址 ID
        /// </summary>
        public int AddressId { get; set; }

        /// <summary>
        /// 收件人姓名
        /// </summary>
        public string RecipientName { get; set; } = string.Empty;

        /// <summary>
        /// 電話號碼
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// 完整地址
        /// </summary>
        public string FullAddress { get; set; } = string.Empty;

        /// <summary>
        /// 是否為預設地址
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
