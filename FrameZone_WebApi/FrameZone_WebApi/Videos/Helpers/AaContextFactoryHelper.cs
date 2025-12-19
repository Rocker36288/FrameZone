using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Videos.Helpers
{
        public interface IAAContextFactory
        {
            Task<AAContext> CreateAsync();
        }

        public class AaContextFactoryHelper : IAAContextFactory
        {
            private readonly IServiceProvider _provider;

            public AaContextFactoryHelper(IServiceProvider provider)
            {
                _provider = provider;
            }

            public Task<AAContext> CreateAsync()
            {
                // 每次從 Scoped 服務提供者生成新的 AAContext
                var scope = _provider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AAContext>();
                return Task.FromResult(context);
            }
        }
    
}
