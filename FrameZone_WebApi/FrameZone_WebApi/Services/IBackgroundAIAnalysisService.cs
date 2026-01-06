using System.Threading.Tasks;

namespace FrameZone_WebApi.Services
{
    public interface IBackgroundAIAnalysisService
    {
        Task ProcessAIAnalysisAsync(long photoId, long userId);
    }
}
