using System;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;
using MapperGangNET8.Services.InputService;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Main pipeline for processing input and sending to controller - Step 7 implementation
    /// </summary>
    public class InputPipeline : IDisposable
    {
        private readonly IInputService _inputService;
        private readonly IControllerService _controllerService;
        private readonly KeyToControllerMapper _keyMapper;
        private readonly MouseToStickMapper _mouseMapper;
        
        private bool _isEnabled;
        private bool _disposed;

        public InputPipeline(
            IInputService inputService,
            IControllerService controllerService,
            KeyToControllerMapper keyMapper,
            MouseToStickMapper mouseMapper)
        {
            _inputService = inputService;
            _controllerService = controllerService;
            _keyMapper = keyMapper;
            _mouseMapper = mouseMapper;

            // Subscribe to input events
            _inputService.KeyDown += OnKeyDown;
            _inputService.KeyUp += OnKeyUp;
            _inputService.MouseStateChanged += OnMouseStateChanged;
        }

        /// <summary>
        /// Enable or disable the input pipeline
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (_isEnabled == enabled) return;

            _isEnabled = enabled;

            if (enabled)
            {
                // Start input capture
                _inputService.Start();
                // Enable input blocking (reWASD-like behavior)
                _inputService.SetInputBlocking(true);
            }
            else
            {
                // Disable input blocking first (restore normal input)
                _inputService.SetInputBlocking(false);
                // Stop input capture
                _inputService.Stop();
                // Reset controller state when disabled
                _controllerService.ResetState();
            }
        }

        /// <summary>
        /// Update mapper configurations
        /// </summary>
        public void UpdateConfiguration(ConfigModel config)
        {
            _keyMapper.UpdateConfiguration(config);
            _mouseMapper.UpdateConfiguration(config);
        }

        /// <summary>
        /// Handle key down events
        /// </summary>
        private void OnKeyDown(object sender, InputKeyEventArgs e)
        {
            if (!_isEnabled) return;

            // Process key through mapper
            _keyMapper.ProcessKeyDown(e.KeyCode);
        }

        /// <summary>
        /// Handle key up events
        /// </summary>
        private void OnKeyUp(object sender, InputKeyEventArgs e)
        {
            if (!_isEnabled) return;

            // Process key through mapper
            _keyMapper.ProcessKeyUp(e.KeyCode);
        }

        /// <summary>
        /// Handle mouse state changes
        /// </summary>
        private void OnMouseStateChanged(object sender, InputMouseEventArgs e)
        {
            if (!_isEnabled) return;

            // Process mouse through mapper
            _mouseMapper.ProcessMouseInput(e.X, e.Y, e.Button);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Unsubscribe from events
            _inputService.KeyDown -= OnKeyDown;
            _inputService.KeyUp -= OnKeyUp;
            _inputService.MouseStateChanged -= OnMouseStateChanged;

            // Stop input service
            if (_isEnabled)
            {
                _inputService.Stop();
            }

            _disposed = true;
        }
    }
}