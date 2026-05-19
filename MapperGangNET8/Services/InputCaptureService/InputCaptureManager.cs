using Input;
using MapperGangNET8.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MapperGangNET8.Services.InputCaptureService
{
    /// <summary>
    /// Manages keyboard / mouse-button blocking and low-level mouse-delta capture via the
    /// native MouHid.dll driver. Switches between two modes:
    ///   driver mode    — mouse is blocked, we read raw packets and fire delta/button events;
    ///   system mode    — driver released, we listen for Mouse4/Mouse5 to re-engage.
    /// </summary>
    public class InputCaptureManager : IDisposable
    {
        #region MouHid P/Invoke
        [DllImport("MouHid.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr MouHid_Create();

        [DllImport("MouHid.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool MouHid_Connect(IntPtr handle);

        [DllImport("MouHid.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool MouHid_SetBlocking(IntPtr handle, bool block);

        [DllImport("MouHid.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool MouHid_ReadMouseData(IntPtr handle,
            [Out] MouseDataPacket[] data, out int packetCount, int maxPackets);

        [DllImport("MouHid.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern void MouHid_Destroy(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const int VK_XBUTTON1 = 0x05;
        private const int VK_XBUTTON2 = 0x06;

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseDataPacket
        {
            public int DeltaX;
            public int DeltaY;
            public ushort ButtonFlags;
        }

        private const ushort MOUSE_BUTTON_5_DOWN = 0x0100;
        private const ushort MOUSE_BUTTON_4_DOWN = 0x0040;
        private const ushort MOUSE_LEFT_DOWN = 0x0001;
        private const ushort MOUSE_LEFT_UP = 0x0002;
        private const ushort MOUSE_RIGHT_DOWN = 0x0004;
        private const ushort MOUSE_RIGHT_UP = 0x0008;
        private const ushort MOUSE_MIDDLE_DOWN = 0x0010;
        private const ushort MOUSE_MIDDLE_UP = 0x0020;
        #endregion

        /// <summary>
        /// Raised on every captured mouse delta packet (driver mode only).
        /// </summary>
        public event Action<int, int> MouseDeltaAction;

        /// <summary>
        /// Raised on every captured mouse-button transition (driver mode only).
        /// </summary>
        public event Action<InputButtons, bool> MouseButtonAction;

        private readonly HashSet<int> _blockedKeys = new HashSet<int>();
        private readonly HashSet<int> _blockedMouseButtons = new HashSet<int>();

        private IntPtr _mouHidHandle = IntPtr.Zero;
        private bool _mouHidEnabled = false;
        private bool _disposed = false;
        private CancellationTokenSource _readCts;
        private Task _readTask;

        public bool ShouldBlockKey(int keyCode) => _blockedKeys.Contains(keyCode);
        public bool ShouldBlockMouseButton(int buttonCode) => _blockedMouseButtons.Contains(buttonCode);

        /// <summary>
        /// Replace the set of blocked keyboard keys / mouse buttons from the bindings.
        /// </summary>
        public void UpdateBlockedKeys(IEnumerable<KeyBindingModel> keyBindings)
        {
            _blockedKeys.Clear();
            _blockedMouseButtons.Clear();

            foreach (var binding in keyBindings)
            {
                if (binding.InputType == InputDeviceType.Keyboard)
                    _blockedKeys.Add(binding.InputCode);
                else if (binding.InputType == InputDeviceType.Mouse)
                    _blockedMouseButtons.Add(binding.InputCode);
            }
        }

        /// <summary>
        /// Augment the blocked mouse-button set from mouse-button mappings.
        /// </summary>
        public void UpdateBlockedMouseButtons(IEnumerable<MouseButtonMappingModel> mouseButtonMappings)
        {
            foreach (var mapping in mouseButtonMappings)
            {
                if (!string.IsNullOrEmpty(mapping.MouseButton))
                {
                    var buttonCode = InputKeyMap.GetMouseButtonCode(mapping.MouseButton);
                    if (buttonCode != 0)
                        _blockedMouseButtons.Add(buttonCode);
                }
            }
        }

        public InputCaptureManager()
        {
            InitializeMouHid();
        }

        private void InitializeMouHid()
        {
            _mouHidHandle = MouHid_Create();
            if (_mouHidHandle != IntPtr.Zero)
                MouHid_Connect(_mouHidHandle);
        }

        /// <summary>
        /// Enable or disable raw input capture for mouse deltas.
        /// </summary>
        public void EnableMouInput(bool enable)
        {
            if (enable && !_mouHidEnabled) StartMouHidCapture();
            else if (!enable && _mouHidEnabled) StopMouHidCapture();
        }

        private void StartMouHidCapture()
        {
            if (_mouHidHandle == IntPtr.Zero) return;

            try
            {
                if (MouHid_SetBlocking(_mouHidHandle, true))
                {
                    _mouHidEnabled = true;
                    _readCts = new CancellationTokenSource();
                    _readTask = Task.Run(() => ReadMouseData(_readCts.Token));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MOUHID] Failed to start: {ex.Message}");
            }
        }

        private void StopMouHidCapture()
        {
            if (_mouHidHandle == IntPtr.Zero) return;

            try
            {
                MouHid_SetBlocking(_mouHidHandle, false);

                _mouHidEnabled = false;
                _readCts?.Cancel();
                _readTask?.Wait(1000);
                _readCts?.Dispose();
                _readCts = null;
                _readTask = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MOUHID] Failed to stop: {ex.Message}");
            }
        }

        private void ReadMouseData(CancellationToken token)
        {
            var buffer = new MouseDataPacket[64];
            bool isLogicBlocked = true;

            while (!token.IsCancellationRequested && _mouHidEnabled)
            {
                if (isLogicBlocked)
                {
                    if (MouHid_ReadMouseData(_mouHidHandle, buffer, out int count, buffer.Length))
                    {
                        if (count == 0)
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        for (int i = 0; i < count; i++)
                        {
                            var packet = buffer[i];

                            bool isUnlockButton =
                                (packet.ButtonFlags & MOUSE_BUTTON_5_DOWN) != 0 ||
                                (packet.ButtonFlags & MOUSE_BUTTON_4_DOWN) != 0;

                            if (isUnlockButton)
                            {
                                MouHid_SetBlocking(_mouHidHandle, false);
                                isLogicBlocked = false;

                                while ((GetAsyncKeyState(VK_XBUTTON2) & 0x8000) != 0 ||
                                       (GetAsyncKeyState(VK_XBUTTON1) & 0x8000) != 0)
                                {
                                    Thread.Sleep(10);
                                }

                                break;
                            }

                            if (packet.DeltaX != 0 || packet.DeltaY != 0)
                                MouseDeltaAction?.Invoke(packet.DeltaX, packet.DeltaY);

                            if ((packet.ButtonFlags & MOUSE_LEFT_DOWN) != 0) MouseButtonAction?.Invoke(InputButtons.LeftMouseDown, true);
                            if ((packet.ButtonFlags & MOUSE_LEFT_UP) != 0) MouseButtonAction?.Invoke(InputButtons.LeftMouseDown, false);
                            if ((packet.ButtonFlags & MOUSE_RIGHT_DOWN) != 0) MouseButtonAction?.Invoke(InputButtons.RightMouseDown, true);
                            if ((packet.ButtonFlags & MOUSE_RIGHT_UP) != 0) MouseButtonAction?.Invoke(InputButtons.RightMouseDown, false);
                            if ((packet.ButtonFlags & MOUSE_MIDDLE_DOWN) != 0) MouseButtonAction?.Invoke(InputButtons.WheelMoveDown, true);
                            if ((packet.ButtonFlags & MOUSE_MIDDLE_UP) != 0) MouseButtonAction?.Invoke(InputButtons.WheelMoveDown, false);
                        }
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    bool mouse5Down = (GetAsyncKeyState(VK_XBUTTON2) & 0x8000) != 0;
                    bool mouse4Down = (GetAsyncKeyState(VK_XBUTTON1) & 0x8000) != 0;

                    if (mouse5Down || mouse4Down)
                    {
                        MouHid_SetBlocking(_mouHidHandle, true);
                        isLogicBlocked = true;

                        while ((GetAsyncKeyState(VK_XBUTTON2) & 0x8000) != 0 ||
                               (GetAsyncKeyState(VK_XBUTTON1) & 0x8000) != 0)
                        {
                            Thread.Sleep(10);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
        }

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
}
