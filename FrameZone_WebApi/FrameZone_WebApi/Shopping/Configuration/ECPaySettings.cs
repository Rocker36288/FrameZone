using Microsoft.OpenApi;

namespace FrameZone_WebApi.Shopping.Configuration
{
    public class ECPaySettings
    {
        //用來放綠界測試平台商家資訊
        public string MerchantID { get; set; } = string.Empty;
        public string HashKey { get; set; } = string.Empty;
        public string HashIV { get; set; } = string.Empty;

        //以下新增
        public string ApiUrl { get; set; } = string.Empty;
        public string PayResultPath { get; set; } = string.Empty;
        public string ServerReturnURL { get; set; } = string.Empty;
    }
}
