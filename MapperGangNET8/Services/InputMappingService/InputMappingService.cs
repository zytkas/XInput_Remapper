using System;
using System.Threading.Tasks;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ConfigService;
using MapperGangNET8.Services.ControllerService;
using MapperGangNET8.Services.MappingService;

namespace MapperGangNET8.Services.InputMappingService
{
    /// <summary>
    /// Simplified service that orchestrates the input mapping pipeline - Step 7 implementation
    /// </summary>
    public class InputMappingService : IDisposable
    {
        private readonly IControllerService _controllerService;
        private readonly IConfigService _configService;
        private readonly InputPipeline _inputPipeline;

        private ConfigModel _currentConfig;
        private bool _disposed;
        private bool _isControllerConnected;
        private bool _isMappingEnabled;

        /// <summary>
        /// Constructor
        /// </summary>
        public InputMappingService(
            IControllerService controllerService,
            IConfigService configService,
            InputPipeline inputPipeline)
        {
            _controllerService = controllerService;
            _configService = configService;
            _inputPipeline = inputPipeline;

            // Load config asynchronously
            _ = LoadConfigAsync();
        }

        /// <summary>
        /// Connect controller (once per app session)
        /// </summary>
        public async Task<bool> ConnectControllerAsync()
        {
            if (_isControllerConnected) return true;
            
            var success = await ConnectControllerInternalAsync();
            _isControllerConnected = success;
            return success;
        }
        
        /// <summary>
        /// Disconnect controller (on app close)
        /// </summary>
        public async Task DisconnectControllerAsync()
        {
            if (!_isControllerConnected) return;
            
            // Stop mapping first if active
            if (_isMappingEnabled)
            {
                await SetMappingEnabledAsync(false);
            }
            
            await DisconnectControllerInternalAsync();
            _isControllerConnected = false;
        }
        
        /// <summary>
        /// Enable or disable input mapping (controller stays connected)
        /// </summary>
        public async Task SetMappingEnabledAsync(bool enabled)
        {
            if (_isMappingEnabled == enabled) return;
            
            if (enabled)
            {
                // Ensure controller is connected first
                if (!_isControllerConnected)
                {
                    var connected = await ConnectControllerAsync();
                    if (!connected) return; // Failed to connect
                }
                
                // Enable input pipeline (this will block inputs and start mapping)
                _inputPipeline.SetEnabled(true);
                _isMappingEnabled = true;
            }
            else
            {
                // Disable input pipeline (this will unblock inputs and stop mapping)
                _inputPipeline.SetEnabled(false);
                _isMappingEnabled = false;
                // Note: Controller stays connected!
            }
        }

        /// <summary>
        /// Load configuration settings
        /// </summary>
        private async Task LoadConfigAsync()
        {
            _currentConfig = await _configService.LoadConfigAsync();
            
            // Update pipeline with new configuration
            _inputPipeline.UpdateConfiguration(_currentConfig);
        }

        /// <summary>
        /// Internal method to connect the virtual controller
        /// </summary>
        private async Task<bool> ConnectControllerInternalAsync()
        {
            try
            {
                // Get controller type from settings
                ControllerType controllerType = _currentConfig?.ControllerSettings?.SelectedControllerType == "DualShock 4 Controller"
                    ? ControllerType.DualShock4
                    : ControllerType.Xbox360;

                // Get controller number from settings
                int controllerNumber = 1;
                if (_currentConfig?.ControllerSettings?.ControllerNumber != null)
                {
                    string numberStr = _currentConfig.ControllerSettings.ControllerNumber.Replace("Controller ", "");
                    if (int.TryParse(numberStr, out int number))
                    {
                        controllerNumber = number;
                    }
                }

                // Connect controller
                return await _controllerService.ConnectAsync(controllerType, controllerNumber);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error connecting controller: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Internal method to disconnect the virtual controller
        /// </summary>
        private async Task DisconnectControllerInternalAsync()
        {
            try
            {
                await _controllerService.DisconnectAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disconnecting controller: {ex.Message}");
            }
        }

        /// <summary>
        /// Refresh configuration from file
        /// </summary>
        public async Task RefreshConfigurationAsync()
        {
            await LoadConfigAsync();
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Dispose pipeline (handles input service cleanup)
            _inputPipeline?.Dispose();
            
            // Disconnect controller properly
            DisconnectControllerAsync().Wait();

            _disposed = true;
        }
    }
}