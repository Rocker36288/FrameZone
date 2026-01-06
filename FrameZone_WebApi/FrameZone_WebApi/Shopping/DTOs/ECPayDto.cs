namespace FrameZone_WebApi.Shopping.DTOs
{
    //給前端的隱藏表單
    public class ECPayOrderParamsDto
    {
        // 使用綠界 API 需要的 Header 相關資訊(<form></form>)
        public string HttpMethod { get; set; } = "POST";
        public string ContentType { get; set; } = "application/x-www-form-urlencoded";
        public string ServiceURL { get; set; } = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";

        // 基本參數模板(<input />)
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object> {
            {"MerchantID", ""},
            {"MerchantTradeNo", ""},
            {"MerchantTradeDate", ""},
            {"PaymentType", "aio"},
            {"TotalAmount", 0},
            {"TradeDesc", ""},
            {"ItemName", ""},
            {"ReturnURL", ""},
            {"ChoosePayment", ""},
            //{"CheckMacValue", ""}, 計算檢查碼後再新增
            {"EncryptType", 1},
            // 其他 optional 參數會等收到前端的 request 後新增
        };

        // Debug 檢查用
        //public string OrderParamsString { get; set; } = "";
    }


}
