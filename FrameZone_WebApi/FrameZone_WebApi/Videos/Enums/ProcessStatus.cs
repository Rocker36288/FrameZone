using System.Text.Json.Serialization;

namespace FrameZone_WebApi.Videos.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))] // 讓 JSON 直接用字串
    public enum ProcessStatus
    {
        UPLOADING,          // 上傳中
        UPLOADED,           // 已上傳
        PRE_PROCESSING,     // 前處理中
        TRANSCODING,        // 轉碼中
        AI_AUDITING,        // AI 審核中
        READY,              // 準備完成
        PUBLISHED,
        FAILED_TRANSCODE,   // 轉碼失敗
        FAILED_AUDIT        // 審核失敗
    }
}
