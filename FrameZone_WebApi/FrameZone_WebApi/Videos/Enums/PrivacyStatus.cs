using System.Text.Json.Serialization;

namespace FrameZone_WebApi.Videos.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))] // JSON 用字串
    public enum PrivacyStatus
    {
        PUBLIC,     // 公開
        UNLISTED,   // 非公開 (僅網址)
        PRIVATE,    // 私人 (僅自己)
        DRAFT       // 草稿 (僅創作者工作頁)
    }

}
