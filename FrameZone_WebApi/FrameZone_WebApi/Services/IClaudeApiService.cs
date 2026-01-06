using FrameZone_WebApi.DTOs.AI;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// Claude API 服務介面
    /// 提供照片內容的深度語義分析和智能分類建議
    /// </summary>
    /// <remarks>
    /// Claude API Service 是照片分類系統的「智能大腦」，它負責：
    /// 1. 整合來自 EXIF、Azure Vision、Google Places 的分散資訊
    /// 2. 理解照片的深層語義和故事脈絡
    /// 3. 為沒有 EXIF 資料的照片提供視覺內容推理
    /// 4. 生成人類可讀的照片描述和建議標籤
    /// 
    /// 這個服務扮演著「最後防線」和「語義整合者」的角色，
    /// 當其他 API 無法提供完整資訊時，Claude 可以從視覺內容本身進行推理。
    /// </remarks>
    public interface IClaudeApiService
    {
        #region 核心分析功能

        /// <summary>
        /// 分析單張照片並提供智能分類建議
        /// </summary>
        /// <param name="request">照片分析請求，包含照片資訊和已知的分析結果</param>
        /// <returns>完整的分析結果，包含建議標籤、描述、景點判斷等資訊</returns>
        /// <remarks>
        /// 這是整個服務最重要的方法，它會根據輸入資料的完整程度提供不同深度的分析：
        /// 
        /// <para><b>情境一：有完整資訊（EXIF + Azure Vision + Google Places）</b></para>
        /// Claude 會進行深度的語義整合，理解照片的完整故事。
        /// 例如：Azure Vision 說「建築物、天空、傳統裝飾」，Google Places 說「淺草寺」，
        /// Claude 會整合為「在東京淺草寺拍攝的傳統建築照片」，建議標籤：日本、東京、寺廟、建築、文化。
        /// 
        /// <para><b>情境二：缺少 EXIF 資料（只有 Azure Vision + 照片縮圖）</b></para>
        /// Claude 會依靠視覺分析結果進行推理，但不會嘗試猜測地點。
        /// 例如：照片有「壽司、餐盤、餐廳環境」，Claude 會建議標籤：美食、日式料理、餐廳，
        /// 但不會猜測「這可能是在東京拍的」，因為缺少地點證據。
        /// 
        /// <para><b>使用縮圖而非原圖</b></para>
        /// 為了降低 token 消耗和成本，這個方法會使用照片縮圖進行分析。
        /// 縮圖已經足夠 Claude 理解照片的視覺內容，不需要完整解析度的原圖。
        /// </remarks>
        Task<ClaudeAnalysisResultDto> AnalyzeSinglePhotoAsync(PhotoAnalysisContextDto request);

        #endregion

        #region 未來擴展功能（介面預留）

        /// <summary>
        /// 分析照片群組並判斷是否在同一景點拍攝
        /// </summary>
        /// <param name="request">照片群組分析請求，包含多張照片的資訊和關聯性證據</param>
        /// <returns>群組分析結果，包含景點判斷、信心分數、建議標籤等</returns>
        /// <remarks>
        /// <b>⚠️ 此功能目前未實作，介面預留供未來擴展使用</b>
        /// 
        /// 這個方法用於處理「困難景點」的識別問題，例如沖繩美麗海水族館。
        /// 單張水族館內部的照片可能無法明確判斷是哪個水族館，但如果綜合分析
        /// 同一批次的多張照片（都有水族館展示缸、都在沖繩附近、GPS 座標接近），
        /// 就能更準確地識別出具體景點。
        /// 
        /// 實作時會需要配合上層的「關聯性探索」邏輯，先找出可能有關聯的照片群組，
        /// 再使用這個方法進行批次分析。這是一個複雜的功能，會在基礎功能穩定後再開發。
        /// </remarks>
        Task<ClaudeAnalysisResultDto> AnalyzePhotoGroupAsync(PhotoGroupAnalysisContextDto request);

        #endregion

        #region 輔助功能

        /// <summary>
        /// 測試 Claude API 連線
        /// </summary>
        /// <returns>是否連線成功</returns>
        /// <remarks>
        /// 這個方法會發送一個簡單的測試請求來驗證：
        /// 1. API Key 是否有效
        /// 2. 網路連線是否正常
        /// 3. 配額是否已經用盡
        /// 
        /// 在系統啟動時或健康檢查時使用，可以提前發現問題。
        /// </remarks>
        Task<bool> TestConnectionAsync();

        #endregion
    }

    #region 請求 DTO 定義

    /// <summary>
    /// 照片分析上下文（單張照片）
    /// 包含照片的所有已知資訊，用於 Claude 進行語義分析
    /// </summary>
    public class PhotoAnalysisContextDto
    {
        /// <summary>
        /// 照片 ID（用於追蹤和記錄）
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 照片縮圖（Base64 編碼）
        /// 注意：使用縮圖而非原圖，以降低 token 消耗
        /// </summary>
        public string? ThumbnailBase64 { get; set; }

        /// <summary>
        /// 照片縮圖的 MIME 類型（例如：image/jpeg）
        /// </summary>
        public string? ThumbnailMediaType { get; set; } = "image/jpeg";

        /// <summary>
        /// EXIF 資訊（如果有）
        /// </summary>
        public ExifContextDto? Exif { get; set; }

        /// <summary>
        /// Azure Vision 分析結果（如果有）
        /// </summary>
        public AzureVisionContextDto? AzureVision { get; set; }

        /// <summary>
        /// Google Places 分析結果（如果有）
        /// </summary>
        public GooglePlacesContextDto? GooglePlaces { get; set; }

        /// <summary>
        /// 分析選項
        /// </summary>
        public AnalysisOptionsDto Options { get; set; } = new();
    }

    /// <summary>
    /// EXIF 上下文資訊（精簡版）
    /// </summary>
    public class ExifContextDto
    {
        /// <summary>
        /// 拍攝時間
        /// </summary>
        public DateTime? DateTaken { get; set; }

        /// <summary>
        /// GPS 緯度
        /// </summary>
        public decimal? Latitude { get; set; }

        /// <summary>
        /// GPS 經度
        /// </summary>
        public decimal? Longitude { get; set; }

        /// <summary>
        /// 相機品牌
        /// </summary>
        public string? CameraMake { get; set; }

        /// <summary>
        /// 相機型號
        /// </summary>
        public string? CameraModel { get; set; }
    }

    /// <summary>
    /// Azure Vision 上下文資訊（精簡版）
    /// </summary>
    public class AzureVisionContextDto
    {
        /// <summary>
        /// 辨識出的物體列表
        /// </summary>
        public List<string> Objects { get; set; } = new();

        /// <summary>
        /// 標籤列表
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// 主要顏色
        /// </summary>
        public List<string> DominantColors { get; set; } = new();

        /// <summary>
        /// 照片描述（Azure 生成的）
        /// </summary>
        public string? Caption { get; set; }

        /// <summary>
        /// 是否為成人內容
        /// </summary>
        public bool IsAdultContent { get; set; }

        /// <summary>
        /// 是否為暴力內容
        /// </summary>
        public bool IsRacyContent { get; set; }
    }

    /// <summary>
    /// Google Places 上下文資訊（精簡版）
    /// </summary>
    public class GooglePlacesContextDto
    {
        /// <summary>
        /// 地理編碼結果（國家、城市、地區）
        /// </summary>
        public GeocodingResultDto? Geocode { get; set; }

        /// <summary>
        /// 景點識別結果（如果有）
        /// </summary>
        public TouristSpotIdentificationDto? TouristSpot { get; set; }

        /// <summary>
        /// 附近的候選景點列表（用於批次分析）
        /// </summary>
        public List<CandidateSpotDto> CandidateSpots { get; set; } = new();
    }

    /// <summary>
    /// 候選景點資訊
    /// </summary>
    public class CandidateSpotDto
    {
        /// <summary>
        /// 景點名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 景點類型列表
        /// </summary>
        public List<string> Types { get; set; } = new();

        /// <summary>
        /// 評分 (0.0 - 5.0)
        /// </summary>
        public double? Rating { get; set; }

        /// <summary>
        /// 評論數
        /// </summary>
        public int? ReviewCount { get; set; }

        /// <summary>
        /// 距離（公尺）
        /// </summary>
        public double? DistanceMeters { get; set; }

        /// <summary>
        /// Place ID
        /// </summary>
        public string? PlaceId { get; set; }
    }

    /// <summary>
    /// 分析選項
    /// </summary>
    public class AnalysisOptionsDto
    {
        /// <summary>
        /// 是否需要詳細描述
        /// </summary>
        public bool IncludeDescription { get; set; } = true;

        /// <summary>
        /// 是否需要歷史/文化背景資訊
        /// </summary>
        public bool IncludeHistoricalContext { get; set; } = false;

        /// <summary>
        /// 溫度參數 (0.0 - 1.0)，控制輸出隨機性
        /// 較低的值（0.2）產生更確定性的輸出，適合分類任務
        /// </summary>
        public double Temperature { get; set; } = 0.2;

        /// <summary>
        /// 最大生成 token 數
        /// </summary>
        public int MaxTokens { get; set; } = 2048;
    }

    /// <summary>
    /// 照片群組分析上下文（未來功能）
    /// </summary>
    public class PhotoGroupAnalysisContextDto
    {
        /// <summary>
        /// 群組內的照片列表
        /// </summary>
        public List<PhotoAnalysisContextDto> Photos { get; set; } = new();

        /// <summary>
        /// 關聯性證據（例如：時間接近、空間接近、視覺相似）
        /// </summary>
        public string? RelationshipEvidence { get; set; }

        /// <summary>
        /// 候選景點列表（綜合所有照片的結果）
        /// </summary>
        public List<CandidateSpotDto> CandidateSpots { get; set; } = new();

        /// <summary>
        /// 分析選項
        /// </summary>
        public AnalysisOptionsDto Options { get; set; } = new();
    }

    #endregion
}