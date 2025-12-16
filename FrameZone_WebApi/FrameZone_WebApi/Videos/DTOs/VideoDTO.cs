namespace FrameZone_WebApi.Videos.DTOs
{
    //用於傳輸資料給前端的影片卡片資料
    public class VideoCardDto
    {
        // ── 影片識別 ─────────────────────

        // ⚠️ 內部識別（可留、但前端不該用來導頁）
        public int VideoId { get; set; }

        // ✅ 對外識別（GUID 檔名 / 播放位置）
        public string VideoUri { get; set; } = "";

        // ── 影片資訊 ─────────────────────
        public string Title { get; set; } = "";
        public string Thumbnail { get; set; } = "";
        public int Duration { get; set; }
        public int Views { get; set; }
        public DateTime PublishDate { get; set; }
        public string? Description { get; set; }

        // ── 頻道資訊 ─────────────────────
        public string ChannelName { get; set; } = "";
        public string Avatar { get; set; } = "";
    }
}
