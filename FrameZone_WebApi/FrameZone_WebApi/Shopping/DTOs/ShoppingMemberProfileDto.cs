namespace FrameZone_WebApi.Shopping.DTOs
{
    public class ShoppingMemberProfileDto
    {
        public long UserId { get; set; }
        public string Account { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? DisplayName { get; set; }
        public string? Avatar { get; set; }
        public string? RealName { get; set; }
        public string? Gender { get; set; }
        public DateOnly? BirthDate { get; set; }
    }

    public class ShoppingMemberProfileResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ShoppingMemberProfileDto? Data { get; set; }
    }
}
