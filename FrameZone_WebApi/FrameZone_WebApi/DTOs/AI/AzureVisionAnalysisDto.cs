using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrameZone_WebApi.DTOs.AI
{
    /// <summary>
    /// Azure Computer Vision API 4.0 分析結果
    /// </summary>
    public class AzureVisionAnalysisDto
    {
        // ==================== Service 層包裝屬性 ====================

        /// <summary>
        /// 是否成功執行分析
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Azure API 原始回應（JSON 格式）
        /// 保留原始回應以便除錯和未來擴充
        /// </summary>
        public string? RawResponse { get; set; }

        /// <summary>
        /// HTTP 狀態碼
        /// 用於記錄 API 呼叫的 HTTP 狀態
        /// </summary>
        public int? HttpStatusCode { get; set; }

        // ==================== Azure API 基本資訊 ====================

        /// <summary>
        /// API 提供者識別
        /// </summary>
        public string Provider { get; set; } = "AzureVision";

        /// <summary>
        /// 模型版本
        /// </summary>
        public string? ModelVersion { get; set; }

        /// <summary>
        /// 分析時間
        /// </summary>
        public DateTimeOffset AnalyzedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// API 請求 ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 處理時間（毫秒）
        /// </summary>
        public int ProcessingTimeMs { get; set; }

        /// <summary>
        /// 錯誤代碼（如果有）
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 錯誤訊息（如果有）
        /// </summary>
        public string? ErrorMessage { get; set; }

        // ==================== 照片大小追蹤資訊 ====================

        /// <summary>
        /// 是否使用縮圖進行分析
        /// 當原圖超過 Azure 的 4MB 限制時，系統會自動切換使用縮圖
        /// </summary>
        public bool UsedThumbnail { get; set; }

        /// <summary>
        /// 原始照片大小（MB）
        /// 記錄使用者上傳的原始照片大小
        /// </summary>
        public double? OriginalImageSizeMB { get; set; }

        /// <summary>
        /// 實際用於分析的照片大小（MB）
        /// 如果使用縮圖，這個值會小於 OriginalImageSizeMB
        /// </summary>
        public double? AnalyzedImageSizeMB { get; set; }

        // ==================== Azure Vision 分析結果 ====================

        /// <summary>
        /// 偵測到的物件列表
        /// </summary>
        public List<DetectedObject> Objects { get; set; } = new();

        /// <summary>
        /// 圖像標籤列表
        /// </summary>
        public List<ImageTag> Tags { get; set; } = new();

        /// <summary>
        /// 圖像描述
        /// </summary>
        public ImageDescription? Description { get; set; }

        /// <summary>
        /// 圖像類別
        /// </summary>
        public List<ImageCategory> Categories { get; set; } = new();

        /// <summary>
        /// 成人內容分析
        /// </summary>
        public AdultContent? Adult { get; set; }

        /// <summary>
        /// 色彩分析
        /// </summary>
        public ColorAnalysis? Color { get; set; }

        /// <summary>
        /// 圖像類型（ClipArt, LineDrawing 等）
        /// </summary>
        public ImageType? ImageType { get; set; }
    }

    /// <summary>
    /// 偵測到的物件
    /// </summary>
    public class DetectedObject
    {
        /// <summary>
        /// 物件名稱（英文）
        /// </summary>
        [JsonPropertyName("object")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 信心分數 (0.0 - 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// 物件邊界框（左上角為原點 (0,0)，單位：像素）
        /// </summary>
        public BoundingBox? Rectangle { get; set; }

        /// <summary>
        /// 父類別名稱（來源：Azure Vision parent 欄位）
        /// </summary>
        public JsonElement? Parent { get; set; }
    }

    /// <summary>
    /// 圖像標籤
    /// </summary>
    public class ImageTag
    {
        /// <summary>
        /// 標籤名稱（英文）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 信心分數 (0.0 - 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// 標籤提示（來源：Azure Vision hint 欄位，可能為 null）
        /// </summary>
        public string? Hint { get; set; }
    }

    /// <summary>
    /// 物件邊界框
    /// 座標系統：左上角為原點 (0,0)，單位為像素 (px)
    /// </summary>
    public class BoundingBox
    {
        /// <summary>
        /// X 座標（左上角，像素）
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y 座標（左上角，像素）
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// 寬度（像素）
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 高度（像素）
        /// </summary>
        public int Height { get; set; }
    }

    /// <summary>
    /// 圖像描述
    /// </summary>
    public class ImageDescription
    {
        /// <summary>
        /// 圖像標籤列表
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// 圖像描述文字列表
        /// </summary>
        public List<Caption> Captions { get; set; } = new();
    }

    /// <summary>
    /// 圖像描述文字
    /// </summary>
    public class Caption
    {
        /// <summary>
        /// 描述文字（英文）
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// 信心分數 (0.0 - 1.0)
        /// </summary>
        public double Confidence { get; set; }
    }

    /// <summary>
    /// 圖像類別
    /// </summary>
    public class ImageCategory
    {
        /// <summary>
        /// 類別名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 信心分數 (0.0 - 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// 類別詳細資訊
        /// </summary>
        public CategoryDetail? Detail { get; set; }
    }

    /// <summary>
    /// 類別詳細資訊
    /// </summary>
    public class CategoryDetail
    {
        /// <summary>
        /// 名人列表
        /// </summary>
        public List<Celebrity> Celebrities { get; set; } = new();

        /// <summary>
        /// 地標列表
        /// </summary>
        public List<Landmark> Landmarks { get; set; } = new();
    }

    /// <summary>
    /// 名人
    /// </summary>
    public class Celebrity
    {
        public string Name { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public BoundingBox? FaceRectangle { get; set; }
    }

    /// <summary>
    /// 地標
    /// </summary>
    public class Landmark
    {
        public string Name { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    /// <summary>
    /// 成人內容分析
    /// </summary>
    public class AdultContent
    {
        /// <summary>
        /// 是否為成人內容
        /// </summary>
        public bool IsAdultContent { get; set; }

        /// <summary>
        /// 是否為色情內容
        /// </summary>
        public bool IsRacyContent { get; set; }

        /// <summary>
        /// 是否為血腥內容
        /// </summary>
        public bool IsGoryContent { get; set; }

        /// <summary>
        /// 成人內容分數 (0.0 - 1.0)
        /// </summary>
        public double AdultScore { get; set; }

        /// <summary>
        /// 色情內容分數 (0.0 - 1.0)
        /// </summary>
        public double RacyScore { get; set; }

        /// <summary>
        /// 血腥內容分數 (0.0 - 1.0)
        /// </summary>
        public double GoreScore { get; set; }
    }

    /// <summary>
    /// 色彩分析
    /// </summary>
    public class ColorAnalysis
    {
        /// <summary>
        /// 主色調
        /// </summary>
        public string DominantColorForeground { get; set; } = string.Empty;

        /// <summary>
        /// 背景主色調
        /// </summary>
        public string DominantColorBackground { get; set; } = string.Empty;

        /// <summary>
        /// 主色調列表
        /// </summary>
        public List<string> DominantColors { get; set; } = new();

        /// <summary>
        /// 強調色
        /// </summary>
        public string AccentColor { get; set; } = string.Empty;

        /// <summary>
        /// 是否為黑白圖片
        /// </summary>
        public bool IsBWImg { get; set; }
    }

    /// <summary>
    /// 圖像類型
    /// </summary>
    public class ImageType
    {
        /// <summary>
        /// ClipArt 類型分數 (0: 非 ClipArt, 1: 模糊, 2: 一般, 3: 良好)
        /// </summary>
        public int ClipArtType { get; set; }

        /// <summary>
        /// 線條畫分數 (0: 非線條畫, 1: 線條畫)
        /// </summary>
        public int LineDrawingType { get; set; }
    }
}