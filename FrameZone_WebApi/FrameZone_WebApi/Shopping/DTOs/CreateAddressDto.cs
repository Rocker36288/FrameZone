using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.Shopping.DTOs
{
    /// <summary>
    /// 建立新收件地址的資料傳輸物件
    /// </summary>
    public class CreateAddressDto
    {
        /// <summary>
        /// 收件人姓名（必填）
        /// </summary>
        [Required(ErrorMessage = "收件人姓名為必填欄位")]
        [StringLength(100, ErrorMessage = "收件人姓名不可超過 100 個字元")]
        public string RecipientName { get; set; } = string.Empty;

        /// <summary>
        /// 電話號碼（必填）
        /// </summary>
        [Required(ErrorMessage = "電話號碼為必填欄位")]
        [Phone(ErrorMessage = "電話號碼格式不正確")]
        [StringLength(20, ErrorMessage = "電話號碼不可超過 20 個字元")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// 完整地址（必填）
        /// </summary>
        [Required(ErrorMessage = "完整地址為必填欄位")]
        [StringLength(500, ErrorMessage = "地址不可超過 500 個字元")]
        public string FullAddress { get; set; } = string.Empty;

        /// <summary>
        /// 是否設為預設地址（選填，預設 false）
        /// </summary>
        public bool IsDefault { get; set; } = false;
    }
}
