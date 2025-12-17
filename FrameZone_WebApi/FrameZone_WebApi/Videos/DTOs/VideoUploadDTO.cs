namespace FrameZone_WebApi.Videos.DTOs
{
    public class VideoUploadDto
    {
        public IFormFile File { get; set; } = null!;
        public string Title { get; set; } = "";
    }
}
