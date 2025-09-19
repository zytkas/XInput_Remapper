using System;
using System.Linq;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Maps mouse movement to controller right stick using zxmapper approach
    /// </summary>
    public class MouseToStickMapper
    {
        private readonly IControllerService _controllerService;
        private double currentStickX = 0;
        private double currentStickY = 0;


        private const double SMOOTHING_RATE = 0.2; 
        private double sensX = 12;
        private double sensY = 12
            ;
        private int capFactor = 50;    

        // Время последнего движения для автосброса
        private DateTime _lastMouseMoveTime = DateTime.Now;
        private const int MOUSE_RESET_DELAY_MS = 15; // Быстрый сброс как в zxmapper

        public MouseToStickMapper(IControllerService controllerService)
        {
            _controllerService = controllerService;

        }

        /// <summary>
        /// Process raw mouse delta (zxmapper style)
        /// </summary>
        public void ProcessMouseDelta(int deltaX, int deltaY)
        {
            // Целевая позиция на основе дельты
            double targetX = (deltaX * sensX) / capFactor;
            double targetY = -(deltaY * sensY) / capFactor;

            // Ограничиваем
            targetX = Math.Clamp(targetX, -1.0, 1.0);
            targetY = Math.Clamp(targetY, -1.0, 1.0);

            // Плавная интерполяция к целевой позиции
            currentStickX = Lerp(currentStickX, targetX, SMOOTHING_RATE);
            currentStickY = Lerp(currentStickY, targetY, SMOOTHING_RATE);

            // Мертвая зона для малых значений
            if (Math.Abs(currentStickX) < 0.01) currentStickX = 0;
            if (Math.Abs(currentStickY) < 0.01) currentStickY = 0;

            // Применяем S-кривую для плавности
            double finalX = ApplySCurve(currentStickX);
            double finalY = ApplySCurve(currentStickY);

            _controllerService.SetAxis(ControllerAxis.RightThumbX, finalX);
            _controllerService.SetAxis(ControllerAxis.RightThumbY, finalY);
        }
        // Линейная интерполяция
        private double Lerp(double current, double target, double rate)
        {
            return current + (target - current) * rate;
        }

        // S-образная кривая для плавности
        private double ApplySCurve(double input)
        {
            // Сигмоидная функция для плавного ускорения/замедления
            double abs = Math.Abs(input);
            double curved = abs * abs * (3.0 - 2.0 * abs); // Кубическая интерполяция
            return Math.Sign(input) * curved;
        }




        /// <summary>
        /// Check for mouse inactivity and reset (как в zxmapper)
        /// </summary>
        public void UpdateStickDecay()
        {
            double millisecondsSinceMove = (DateTime.Now - _lastMouseMoveTime).TotalMilliseconds;

            if (millisecondsSinceMove > 10) // Быстрый, но плавный возврат
            {
                // Плавно возвращаем к центру
                currentStickX = Lerp(currentStickX, 0, 0.2);
                currentStickY = Lerp(currentStickY, 0, 0.2);

                if (Math.Abs(currentStickX) < 0.01) currentStickX = 0;
                if (Math.Abs(currentStickY) < 0.01) currentStickY = 0;

                _controllerService.SetAxis(ControllerAxis.RightThumbX, currentStickX);
                _controllerService.SetAxis(ControllerAxis.RightThumbY, currentStickY);
            }
        }

        /// <summary>
        /// Reset stick position to center
        /// </summary>
        public void Reset()
        {
            // Сброс позиции
            _controllerService.SetAxis(ControllerAxis.RightThumbX, 0);
            _controllerService.SetAxis(ControllerAxis.RightThumbY, 0);
            _lastMouseMoveTime = DateTime.Now;

            System.Diagnostics.Debug.WriteLine("[MOUSE-ZX] Reset to center");
        }

        /// <summary>
        /// Update configuration
        /// </summary>
        public void UpdateConfiguration(ConfigModel config)
        {
            if (config?.SensitivitySettings != null)
            {
                // Можно загрузить настройки из конфига
                // Пока используем захардкоженные значения из zxmapper

                // sensX = config.SensitivitySettings.MouseXAxisSensitivity / 100.0;
                // sensY = config.SensitivitySettings.MouseYAxisSensitivity / 100.0;
            }
        }

        /// <summary>
        /// Set sensitivity values (для тестирования)
        /// </summary>
        public void SetSensitivity(double x, double y)
        {
            sensX = x;
            sensY = y;
            System.Diagnostics.Debug.WriteLine($"[MOUSE-ZX] Sensitivity updated - X:{sensX} Y:{sensY}");
        }

        /// <summary>
        /// Set cap factor (maximum value before scaling)
        /// </summary>
        public void SetCapFactor(int cap)
        {
            capFactor = cap;
            System.Diagnostics.Debug.WriteLine($"[MOUSE-ZX] Cap factor: {capFactor}");
        }

        /// <summary>
        /// Configure stick decay behavior (для совместимости)
        /// </summary>
        public void SetStickDecaySettings(bool enabled, double decayRate)
        {
            // В zxmapper используется CheckForMouseInactivity с фиксированной задержкой
            System.Diagnostics.Debug.WriteLine($"[MOUSE-ZX] Using zxmapper-style decay with {MOUSE_RESET_DELAY_MS}ms delay");
        }
    }
}