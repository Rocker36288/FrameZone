namespace FrameZone_WebApi.Videos.DTOs
{
    public class VideoUploadDto
    {
        public IFormFile File { get; set; } = null!;
        public string Title { get; set; } = "";
    }

    public class VideoUploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int VideoId { get; set; }
        public string Guid { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public bool ReviewPassed { get; set; }

        public string ReviewReason { get; set; } = string.Empty;
    }

    public class TranscodeResult
    {
        public bool Success { get; }
        public string? M3u8Url { get; }
        public string[]? Qualities { get; }
        public string? Error { get; }
        public int? ExitCode { get; }
        public string? StackTrace { get; }
        public List<string>? MissingFiles { get; }

        private TranscodeResult(bool success, string? m3u8Url = null, string[]? qualities = null,
                                string? error = null, int? exitCode = null, string? stackTrace = null,
                                List<string>? missingFiles = null)
        {
            Success = success;
            M3u8Url = m3u8Url;
            Qualities = qualities;
            Error = error;
            ExitCode = exitCode;
            StackTrace = stackTrace;
            MissingFiles = missingFiles;
        }

        public static TranscodeResult SuccessResult(string m3u8Url, string[] qualities)
            => new TranscodeResult(true, m3u8Url, qualities);

        public static TranscodeResult Fail(string error, int? exitCode = null, string? stackTrace = null, List<string>? missingFiles = null)
            => new TranscodeResult(false, error: error, exitCode: exitCode, stackTrace: stackTrace, missingFiles: missingFiles);
    }
}
