using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FrameZone_WebApi.DTOs
{
    /// <summary>
    /// 反向地理編碼請求 DTO
    /// </summary>
    public class ReverseGeocodeRequestDTO
    {
        /// <summary>
        /// GPS 緯度
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// GPS 經度
        /// </summary>
        public decimal Longitude { get; set; }

        public string Language { get; set; } = "zh-TW";
    }

    /// <summary>
    /// 反向地理編碼回應 DTO
    /// </summary>
    public class  ReverseGeocodeResponseDTO
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 地址資訊
        /// </summary>
        public AddressInfoDTO AddressInfo { get; set; }
    }

    public class AddressInfoDTO
    {
        /// <summary>
        /// 完全格式化地址
        /// </summary>
        public string FormattedAddress { get; set; }

        /// <summary>
        /// 國家
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 區域
        /// </summary>
        public string District { get; set; }

        /// <summary>
        /// 地點名稱
        /// </summary>
        public string PlaceName { get; set; }
        
        /// <summary>
        /// 完整詳細地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// GPS 緯度
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// GPS 經度
        /// </summary>
        public decimal Longitude { get; set; }
    }


    #region Google Geocoding API 原始回應結構

    /// <summary>
    /// Google Geocoding API 回應
    /// </summary>
    public class GoogleGeocodingResponse
    {
        /// <summary>
        /// 結果陣列
        /// </summary>
        public List<GoogleGeocodingResult> Results { get; set; }

        /// <summary>
        /// 狀態馬
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Error_Message { get; set; }
    }

    /// <summary>
    /// Google Geocoding 結果
    /// </summary>
    public class GoogleGeocodingResult
    {
        /// <summary>
        /// 地址組成元件
        /// </summary>
        public List<AddressComponent> Address_Components { get; set; }

        /// <summary>
        /// 完整格式化地址
        /// </summary>
        public string Formatted_Address { get; set; }

        /// <summary>
        /// 地點 ID
        /// </summary>
        public string Place_id { get; set; }

        /// <summary>
        /// 幾何資訊
        /// </summary>
        public Geometry Geometry { get; set; }

        /// <summary>
        /// 地點類型
        /// </summary>
        public List<string> Types { get; set; }
    }

    /// <summary>
    /// 地址組成元件
    /// </summary>
    public class AddressComponent
    {
        /// <summary>
        /// 長名稱
        /// </summary>
        public string Long_Name { get; set; }

        /// <summary>
        /// 短名稱
        /// </summary>
        public string Short_Name { get; set; }

        /// <summary>
        /// 類型
        /// </summary>
        public List<string> Types { get; set; }
    }

    /// <summary>
    /// 幾何資訊
    /// </summary>
    public class Geometry
    {
        /// <summary>
        /// 位置
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// 位置類型
        /// </summary>
        public string Location_Type { get; set; }

        /// <summary>
        /// 視野範圍
        /// </summary>
        public Viewport Viewport { get; set; }
    }

    public class Location
    {
        public decimal Lat { get; set; }

        public decimal Lng { get; set; }
    }

    /// <summary>
    /// 視野範圍
    /// </summary>
    public class Viewport
    {
        /// <summary>
        /// 東北角
        /// </summary>
        public Location Northeast { get; set; }

        /// <summary>
        /// 西南角
        /// </summary>
        public Location Southwest { get; set; }
    }

    #endregion
}
