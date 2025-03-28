using System;
using System.Threading.Tasks;
using System.Timers;
using MapperGang.Models;
using MapperGang.Services.ConfigService;
using Timer = System.Timers.Timer;

namespace MapperGang.Services.AutoSaveService
{
    /// <summary>
    /// Сервис для автоматического сохранения настроек
    /// </summary>
    public class AutoSaveService : IDisposable
    {
        private readonly IConfigService _configService;
        private readonly Timer _timer;
        private ConfigModel _pendingConfig;
        private bool _isConfigChanged;
        private bool _isDisposed;

        /// <summary>
        /// Конструктор AutoSaveService
        /// </summary>
        /// <param name="configService">Сервис настроек</param>
        /// <param name="interval">Интервал автосохранения в миллисекундах</param>
        public AutoSaveService(IConfigService configService, int interval = 5000)
        {
            _configService = configService;

            // Создаем таймер
            _timer = new Timer(interval);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        /// <summary>
        /// Обработчик события таймера
        /// </summary>
        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_isConfigChanged && _pendingConfig != null)
            {
                await SaveConfigAsync();
            }
        }

        public void SetPendingConfig(ConfigModel config)
        {
            _pendingConfig = config;
            _isConfigChanged = true;
        }

        private async Task SaveConfigAsync()
        {
            _isConfigChanged = false;

            if (_pendingConfig != null)
            {
                await _configService.SaveConfigAsync(_pendingConfig);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _timer.Stop();
                _timer.Elapsed -= OnTimerElapsed;
                _timer.Dispose();

                if (_isConfigChanged && _pendingConfig != null)
                {
                    _configService.SaveConfigAsync(_pendingConfig).Wait();
                }
            }

            _isDisposed = true;
        }
    }
}