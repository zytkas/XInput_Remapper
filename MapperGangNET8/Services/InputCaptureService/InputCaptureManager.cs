using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using MapperGangNET8.Models;
using Linearstar.Windows.RawInput;

namespace MapperGangNET8.Services.InputCaptureService
{
    /// <summary>
    /// Manages keyboard and mouse button blocking, plus raw mouse input capture
    /// </summary>
    public class InputCaptureManager : IDisposable
    {
        #region MouHid P/Invoke
        [DllImport("MouHidClient.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr MouHid_Create();

        [DllImport("MouHidClient.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool MouHid_Connect(IntPtr handle);

        [DllImport("MouHidClient.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool MouHid_SetBlocking(IntPtr handle, bool block);

        [DllImport("MouHidClient.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool MouHid_ReadMouseData(IntPtr handle,
            [Out] MouseDataPacket[] data, out int packetCount, int maxPackets);

        [DllImport("MouHidClient.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void MouHid_Destroy(IntPtr handle);

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseDataPacket
        {
            public int DeltaX;
            public int DeltaY;
            public ushort ButtonFlags;
        }

        private const ushort MOUSE_BUTTON_5_DOWN = 0x0100;
        private const ushort MOUSE_BUTTON_5_UP = 0x0200;
        const ushort MOUSE_LEFT_DOWN = 0x0001;
        const ushort MOUSE_LEFT_UP = 0x0002;
        const ushort MOUSE_RIGHT_DOWN = 0x0004;
        const ushort MOUSE_RIGHT_UP = 0x0008;
        const ushort MOUSE_MIDDLE_DOWN = 0x0010;
        const ushort MOUSE_MIDDLE_UP = 0x0020;
        const int INPUT_LEFT_DOWN = 1;
        const int INPUT_LEFT_UP = 2;
        const int INPUT_RIGHT_DOWN = 3;
        const int INPUT_RIGHT_UP = 4;
        #endregion

        private readonly HashSet<int> _blockedKeys = new HashSet<int>();
        private readonly HashSet<int> _blockedMouseButtons = new HashSet<int>();


        private IntPtr _mouHidHandle = IntPtr.Zero;
        private bool _mouHidEnabled = false;
        private bool _disposed = false;
        private CancellationTokenSource _readCts;
        private Task _readTask;

        /// <summary>
        /// Event fired when mouse delta is captured from Raw Input
        /// </summary>
        public event EventHandler<MouseDeltaEventArgs> MouseDeltaCaptured;

        /// <summary>
        /// Event fired when mouse button is pressed/released
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseButtonEvent;

        public bool ShouldBlockKey(int keyCode) => _blockedKeys.Contains(keyCode);
        public bool ShouldBlockMouseButton(int buttonCode) => _blockedMouseButtons.Contains(buttonCode);

        /// <summary>
        /// Update blocked keys and mouse buttons based on current bindings
        /// </summary>
        public void UpdateBlockedKeys(IEnumerable<KeyBindingModel> keyBindings)
        {
            _blockedKeys.Clear();
            _blockedMouseButtons.Clear();

            foreach (var binding in keyBindings)
            {
                if (binding.InputType == InputDeviceType.Keyboard)
                {
                    _blockedKeys.Add(binding.InputCode);
                    System.Diagnostics.Debug.WriteLine($"[INPUT CAPTURE] Added blocked key: {binding.InputCode}");
                }
                else if (binding.InputType == InputDeviceType.Mouse)
                {
                    _blockedMouseButtons.Add(binding.InputCode);
                    System.Diagnostics.Debug.WriteLine($"[INPUT CAPTURE] Added blocked mouse button: {binding.InputCode}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[INPUT CAPTURE] Blocked keys: {_blockedKeys.Count}, Blocked mouse buttons: {_blockedMouseButtons.Count}");
        }

        /// <summary>
        /// Update blocked mouse buttons from mouse button mappings
        /// </summary>
        public void UpdateBlockedMouseButtons(IEnumerable<MouseButtonMappingModel> mouseButtonMappings)
        {
            foreach (var mapping in mouseButtonMappings)
            {
                if (!string.IsNullOrEmpty(mapping.MouseButton))
                {
                    var buttonCode = InputKeyMap.GetMouseButtonCode(mapping.MouseButton);
                    if (buttonCode != 0)
                    {
                        _blockedMouseButtons.Add(buttonCode);
                        System.Diagnostics.Debug.WriteLine($"[INPUT CAPTURE] Added blocked mouse button from mapping: {mapping.MouseButton} -> {buttonCode}");
                    }
                }
            }
        }


        public InputCaptureManager()
        {
            // Инициализируем MouHid драйвер при создании
            InitializeMouHid();
        }

        private void InitializeMouHid()
        {
            _mouHidHandle = MouHid_Create();
            if (_mouHidHandle != IntPtr.Zero)
            {
                if (MouHid_Connect(_mouHidHandle))
                {
                    System.Diagnostics.Debug.WriteLine("[MOUHID] Driver connected successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[MOUHID] Failed to connect to driver");
                }
            }
        }



        /// <summary>
        /// Enable or disable Raw Input capture for mouse deltas
        /// </summary>
        public void EnableMouInput(bool enable)
        {
            System.Diagnostics.Debug.WriteLine($"[MOUHID] EnableRawInput({enable}) - Current:={_mouHidEnabled}");

            if (enable && !_mouHidEnabled)
            {
                StartMouHidCapture();
            }
            else if (!enable && _mouHidEnabled)
            {
                StopMouHidCapture();
            }
        }

        /// <summary>
        /// Start Raw Input for clean mouse delta capture
        /// </summary>
        private void StartMouHidCapture()
        {
            if (_mouHidHandle == IntPtr.Zero) return;

            try
            {
                System.Diagnostics.Debug.WriteLine("[MOUHID] Starting mouse capture with blocking...");

                // Включаем блокировку мыши
                if (MouHid_SetBlocking(_mouHidHandle, true))
                {
                    _mouHidEnabled = true;

                    // Запускаем чтение данных
                    _readCts = new CancellationTokenSource();
                    _readTask = Task.Run(() => ReadMouseData(_readCts.Token));

                    System.Diagnostics.Debug.WriteLine("[MOUHID] ✅ Mouse capture started, mouse blocked");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[MOUHID] ❌ Failed to enable blocking");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MOUHID] ❌ Failed to start: {ex.Message}");
            }
        }
        /// <summary>
        /// Stop Raw Input capture
        /// </summary>
        private void StopMouHidCapture()
        {
            if (_mouHidHandle == IntPtr.Zero) return;

            try
            {
                System.Diagnostics.Debug.WriteLine("[MOUHID] Stopping mouse capture...");

                // Отключаем блокировку мыши
                MouHid_SetBlocking(_mouHidHandle, false);

                _mouHidEnabled = false;

                // Останавливаем чтение
                _readCts?.Cancel();
                _readTask?.Wait(1000);
                _readCts?.Dispose();
                _readCts = null;

                System.Diagnostics.Debug.WriteLine("[MOUHID] ✅ Mouse capture stopped, mouse unblocked");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MOUHID] ❌ Failed to stop: {ex.Message}");
            }
        }

        private void ReadMouseData(CancellationToken token)
        {
            var buffer = new MouseDataPacket[64];
            bool mouse5Pressed = false;

            while (!token.IsCancellationRequested && _mouHidEnabled)
            {
                if (MouHid_ReadMouseData(_mouHidHandle, buffer, out int count, buffer.Length))
                {
                    for (int i = 0; i < count; i++)
                    {
                        var packet = buffer[i];

                        // Проверяем Mouse4 для временного отключения
                        if ((packet.ButtonFlags & MOUSE_BUTTON_5_DOWN) != 0)
                        {
                            if (!mouse5Pressed)
                            {
                                mouse5Pressed = true;
                                MouHid_SetBlocking(_mouHidHandle, false);
                            }
                        }
                        else if ((packet.ButtonFlags & MOUSE_BUTTON_5_UP) != 0)
                        {
                            if (mouse5Pressed)
                            {
                                mouse5Pressed = false;
                                System.Diagnostics.Debug.WriteLine("[MOUHID] Mouse4 released - blocking again");
                                MouHid_SetBlocking(_mouHidHandle, true);
                            }
                        }
                        if (!mouse5Pressed)
                        {
                            if ((packet.ButtonFlags & MOUSE_LEFT_DOWN) != 0)
                            {
                                MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(INPUT_LEFT_DOWN, true));
                            }
                            if ((packet.ButtonFlags & MOUSE_LEFT_UP) != 0)
                            {
                                MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(INPUT_LEFT_UP, false));
                            }

                            if ((packet.ButtonFlags & MOUSE_RIGHT_DOWN) != 0)
                            {
                                MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(INPUT_RIGHT_DOWN, true));
                            }
                            if ((packet.ButtonFlags & MOUSE_RIGHT_UP) != 0)
                            {
                                MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(INPUT_RIGHT_UP, false));
                            }

                            if ((packet.ButtonFlags & MOUSE_MIDDLE_DOWN) != 0)
                            {
                                MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(3, true));
                            }
                            if ((packet.ButtonFlags & MOUSE_MIDDLE_UP) != 0)
                            {
                                MouseButtonEvent?.Invoke(this, new MouseButtonEventArgs(3, false));
                            }

                            // Передаем дельты движения
                            if (packet.DeltaX != 0 || packet.DeltaY != 0)
                            {
                                MouseDeltaCaptured?.Invoke(this,
                                    new MouseDeltaEventArgs(packet.DeltaX, packet.DeltaY));
                            }
                        }
                    // Передаем дельты если не нажата Mouse4
                    if (!mouse5Pressed && (packet.DeltaX != 0 || packet.DeltaY != 0))
                        {
                            MouseDeltaCaptured?.Invoke(this,
                                new MouseDeltaEventArgs(packet.DeltaX, packet.DeltaY));

                            System.Diagnostics.Debug.WriteLine($"[MOUHID] Delta: X={packet.DeltaX}, Y={packet.DeltaY}");
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            StopMouHidCapture();

            if (_mouHidHandle != IntPtr.Zero)
            {
                MouHid_Destroy(_mouHidHandle);
                _mouHidHandle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Event args for mouse delta capture
    /// </summary>
    public class MouseDeltaEventArgs : EventArgs
    {
        public int DeltaX { get; }
        public int DeltaY { get; }

        public MouseDeltaEventArgs(int deltaX, int deltaY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
            }
    }

    /// <summary>
    /// Event args for mouse button events
    /// </summary>
    public class MouseButtonEventArgs : EventArgs
    {
        public int Button { get; }
        public bool IsPressed { get; }

        public MouseButtonEventArgs(int button, bool isPressed)
        {
            Button = button;
            IsPressed = isPressed;
        }
    }

    /// <summary>
    /// Event args for raw input
    /// </summary>
    internal class RawInputEventArgs : EventArgs
    {
        public RawInputData Data { get; }

        public RawInputEventArgs(RawInputData data)
        {
            Data = data;
        }
    }
}