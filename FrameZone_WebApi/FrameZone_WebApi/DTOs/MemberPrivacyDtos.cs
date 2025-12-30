namespace FrameZone.API.DTOs.Member
{
    /// <summary>
    /// 隱私設定項目 DTO
    /// </summary>
    public class PrivacySettingDto
    {
        public long PrivacyId { get; set; }
        public long UserId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新單一隱私設定 DTO
    /// </summary>
    public class UpdatePrivacySettingDto
    {
        public string FieldName { get; set; } = string.Empty;
        public string Visibility { get; set; } = string.Empty;
    }

    /// <summary>
    /// 批次更新隱私設定 DTO
    /// </summary>
    public class BatchUpdatePrivacySettingsDto
    {
        public List<UpdatePrivacySettingDto> Settings { get; set; } = new List<UpdatePrivacySettingDto>();
    }

    /// <summary>
    /// 隱私設定回應 DTO
    /// </summary>
    public class PrivacySettingsResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<PrivacySettingDto> Data { get; set; } = new List<PrivacySettingDto>();
    }
}