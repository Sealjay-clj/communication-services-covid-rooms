using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Calling
{
    public abstract class LobbyTimerService : IHostedService
    {
        private readonly LobbyTimerService lobbyTimerService;
        private Timer _timer;
        private LobbyStateService _lobbyStateService;

        public LobbyTimerService(LobbyTimerService lobbyTimerService, LobbyStateService _lobbyStateService)
        {
            this.lobbyTimerService = lobbyTimerService;
        }

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            // timer calls lobby heartbeat every five seconds
            _timer = new Timer(
                CallStateService,
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5)
            );

            return Task.CompletedTask;
        }

        /// Call the Stop async method if required from within the app.
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
        private void CallStateService(object state)
        {
            _lobbyStateService.Heartbeat();
        }
    }
}