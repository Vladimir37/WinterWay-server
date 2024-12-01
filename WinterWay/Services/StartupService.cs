namespace WinterWay.Services
{
    public class StartupService : IHostedService
    {
        private readonly BackgroundImageService _backgroundImageService;

        public StartupService(BackgroundImageService backgroundImageService)
        {
            _backgroundImageService = backgroundImageService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _backgroundImageService.GetBackgroundData();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}