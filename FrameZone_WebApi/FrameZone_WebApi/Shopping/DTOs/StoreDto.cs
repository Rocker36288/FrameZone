using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.Shopping.DTOs
{
    public class PickupStoreDto
    {
        public int ConvenienceStoreId { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string ConvenienceStoreCode { get; set; } = string.Empty;
        public string ConvenienceStoreName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePickupStoreDto
    {
        [Required(ErrorMessage = "收件人姓名為必填")]
        [StringLength(100, ErrorMessage = "姓名長度不可超過 100 字元")]
        public string RecipientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "電話號碼為必填")]
        [StringLength(20, ErrorMessage = "電話號碼長度不可超過 20 字元")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "電話號碼格式不正確")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "門市代碼為必填")]
        [StringLength(20, ErrorMessage = "門市代碼長度不可超過 20 字元")]
        public string ConvenienceStoreCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "門市名稱為必填")]
        [StringLength(100, ErrorMessage = "門市名稱長度不可超過 100 字元")]
        public string ConvenienceStoreName { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
    }
}
