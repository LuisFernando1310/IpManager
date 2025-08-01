using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IpManager.Domain.Service;

namespace IpManagerJob
{
    public class BackgroundJob : IHostedService, IDisposable
    {
        private readonly IConfiguration _config;
        private readonly ILogger<BackgroundJob> _log;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly int _timer;
        private Timer _timerObj;

        public BackgroundJob(IConfiguration config, IServiceScopeFactory scopeFactory, ILogger<BackgroundJob> log)
        {
            _scopeFactory = scopeFactory;
            _log = log;
            _config = config;
            _timer = Convert.ToInt32(_config["JobTimer"]);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogWarning("Iniciando Job para update de IPs");

            // Configura o timer para executar o job a cada _timer milissegundos
            _timerObj = new Timer(ExecuteJob, null, TimeSpan.Zero, TimeSpan.FromMinutes(_timer));

            return Task.CompletedTask;
        }

        private void ExecuteJob(object state)
        {
            _log.LogInformation("Executando Job para update de IPs em: {time}", DateTimeOffset.Now);

            using (var scope = _scopeFactory.CreateScope())
            {
                var ipService = scope.ServiceProvider.GetRequiredService<IIpService>();
                ipService.UpdateIps();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogWarning("Parando Job para update de IPs");

            // Para o timer
            _timerObj?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timerObj?.Dispose();
        }
    }
}
