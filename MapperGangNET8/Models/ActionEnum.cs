using System.Collections.Generic;
using Input;

namespace MapperGangNET8.Models
{

    public enum InputDeviceType
    {
        Keyboard,
        Mouse,
        Gamepad
    }

    /// <summary>
    /// Централизованная система управления клавишами и кодами
    /// </summary>
    public static class InputKeyMap
    {
        /// <summary>
        /// Словарь клавиш клавиатуры с их InputKeys enum значениями
        /// </summary>
        public static readonly Dictionary<string, InputKeys> KeyboardKeys = new Dictionary<string, InputKeys>
        {
            { "Space", InputKeys.Space },
            { "Left Ctrl", InputKeys.LeftControl },
            { "Left Shift", InputKeys.LeftShift },
            { "Left Alt", InputKeys.LeftAlt },
            { "Right Ctrl", InputKeys.RightControl },
            { "Right Shift", InputKeys.RightShift },
            { "Right Alt", InputKeys.RightAlt },
            { "Tab", InputKeys.Tab },
            { "Enter", InputKeys.Enter },
            { "Esc", InputKeys.Escape },
            { "Backspace", InputKeys.Backspace },
            // Буквы
            { "Q", InputKeys.Q }, { "W", InputKeys.W }, { "E", InputKeys.E }, { "R", InputKeys.R }, { "T", InputKeys.T }, { "Y", InputKeys.Y }, { "U", InputKeys.U }, { "I", InputKeys.I }, { "O", InputKeys.O }, { "P", InputKeys.P },
            { "A", InputKeys.A }, { "S", InputKeys.S }, { "D", InputKeys.D }, { "F", InputKeys.F }, { "G", InputKeys.G }, { "H", InputKeys.H }, { "J", InputKeys.J }, { "K", InputKeys.K }, { "L", InputKeys.L },
            { "Z", InputKeys.Z }, { "X", InputKeys.X }, { "C", InputKeys.C }, { "V", InputKeys.V }, { "B", InputKeys.B }, { "N", InputKeys.N }, { "M", InputKeys.M },
            // Цифры
            { "1", InputKeys.D1 }, { "2", InputKeys.D2 }, { "3", InputKeys.D3 }, { "4", InputKeys.D4 }, { "5", InputKeys.D5 }, { "6", InputKeys.D6 }, { "7", InputKeys.D7 }, { "8", InputKeys.D8 }, { "9", InputKeys.D9 }, { "0", InputKeys.D0 },
            // Функциональные клавиши
            { "F1", InputKeys.F1 }, { "F2", InputKeys.F2 }, { "F3", InputKeys.F3 }, { "F4", InputKeys.F4 }, { "F5", InputKeys.F5 }, { "F6", InputKeys.F6 },
            { "F7", InputKeys.F7 }, { "F8", InputKeys.F8 }, { "F9", InputKeys.F9 }, { "F10", InputKeys.F10 }, { "F11", InputKeys.F11 }, { "F12", InputKeys.F12 },
            // Стрелки
            { "Up", InputKeys.Up }, { "Down", InputKeys.Down }, { "Left", InputKeys.Left }, { "Right", InputKeys.Right },
            // Дополнительные клавиши
            { "Insert", InputKeys.Insert }, { "Delete", InputKeys.Delete }, { "Home", InputKeys.Home }, { "End", InputKeys.End },
            { "Page Up", InputKeys.PageUp }, { "Page Down", InputKeys.PageDown }, { "Caps Lock", InputKeys.CapsLock }
        };

        /// <summary>
        /// Словарь кнопок мыши с их InputButtons enum значениями
        /// </summary>
        public static readonly Dictionary<string, InputButtons> MouseButtons = new Dictionary<string, InputButtons>
        {
            { "Left Button", InputButtons.LeftMouseDown },
            { "Right Button", InputButtons.RightMouseDown },
            { "Mouse Wheel Up", InputButtons.WheelUp },
            { "Mouse Wheel Down", InputButtons.WheelDown },
            { "Mouse Wheel Move Up", InputButtons.WheelMoveUp },
            { "Mouse Wheel Move Down", InputButtons.WheelMoveDown },
            { "Left Double Click", InputButtons.LeftDoubleClick }
        };

        /// <summary>
        /// Словарь действий контроллера с их отображаемыми названиями
        /// </summary>
        public static readonly Dictionary<string, ControllerButton> ControllerActions = new Dictionary<string, ControllerButton>
        {
            { "A Button", ControllerButton.A },
            { "B Button", ControllerButton.B },
            { "X Button", ControllerButton.X },
            { "Y Button", ControllerButton.Y },
            { "Left Bumper", ControllerButton.LeftShoulder },
            { "Right Bumper", ControllerButton.RightShoulder },
            { "Left Trigger", ControllerButton.LeftTrigger },
            { "Right Trigger", ControllerButton.RightTrigger },
            { "Left Stick Press", ControllerButton.LeftThumb },
            { "Right Stick Press", ControllerButton.RightThumb },
            { "D-Pad Up", ControllerButton.DPadUp },
            { "D-Pad Down", ControllerButton.DPadDown },
            { "D-Pad Left", ControllerButton.DPadLeft },
            { "D-Pad Right", ControllerButton.DPadRight },
            { "Start Button", ControllerButton.Start },
            { "Back Button", ControllerButton.Back },
            { "Guide Button", ControllerButton.Guide }
        };

        /// <summary>
        /// Обратный словарь для получения отображаемого названия по enum
        /// </summary>
        public static readonly Dictionary<ControllerButton, string> ControllerActionNames = new Dictionary<ControllerButton, string>
        {
            { ControllerButton.A, "A Button" },
            { ControllerButton.B, "B Button" },
            { ControllerButton.X, "X Button" },
            { ControllerButton.Y, "Y Button" },
            { ControllerButton.LeftShoulder, "Left Bumper" },
            { ControllerButton.RightShoulder, "Right Bumper" },
            { ControllerButton.LeftTrigger, "Left Trigger" },
            { ControllerButton.RightTrigger, "Right Trigger" },
            { ControllerButton.LeftThumb, "Left Stick Press" },
            { ControllerButton.RightThumb, "Right Stick Press" },
            { ControllerButton.DPadUp, "D-Pad Up" },
            { ControllerButton.DPadDown, "D-Pad Down" },
            { ControllerButton.DPadLeft, "D-Pad Left" },
            { ControllerButton.DPadRight, "D-Pad Right" },
            { ControllerButton.Start, "Start Button" },
            { ControllerButton.Back, "Back Button" },
            { ControllerButton.Guide, "Guide Button" }
        };

        /// <summary>
        /// Получить список всех доступных клавиш клавиатуры
        /// </summary>
        public static List<string> GetAvailableKeyboardKeys()
        {
            return new List<string>(KeyboardKeys.Keys);
        }

        /// <summary>
        /// Получить название клавиши по InputKeys enum
        /// </summary>
        /// <param name="inputKey">InputKeys enum</param>
        /// <returns>Название клавиши или пустую строку если не найдено</returns>
        public static string GetKeyboardKeyName(InputKeys inputKey)
        {
            foreach (var kvp in KeyboardKeys)
            {
                if (kvp.Value == inputKey)
                {
                    return kvp.Key;
                }
            }
            return "";
        }

        /// <summary>
        /// Получить список всех доступных кнопок мыши
        /// </summary>
        public static List<string> GetAvailableMouseButtons()
        {
            return new List<string>(MouseButtons.Keys);
        }

        /// <summary>
        /// Получить название кнопки мыши по InputButtons enum
        /// </summary>
        /// <param name="inputButton">InputButtons enum</param>
        /// <returns>Название кнопки или пустую строку если не найдено</returns>
        public static string GetMouseButtonName(InputButtons inputButton)
        {
            foreach (var kvp in MouseButtons)
            {
                if (kvp.Value == inputButton)
                {
                    return kvp.Key;
                }
            }
            return "";
        }

        /// <summary>
        /// Получить список всех доступных действий контроллера
        /// </summary>
        public static List<string> GetAvailableControllerActions()
        {
            return new List<string>(ControllerActions.Keys);
        }

        /// <summary>
        /// Получить InputKeys enum по названию клавиши
        /// </summary>
        /// <param name="keyName">Название клавиши</param>
        /// <returns>InputKeys enum или None если клавиша не найдена</returns>
        public static InputKeys GetInputKey(string keyName)
        {
            return KeyboardKeys.TryGetValue(keyName, out InputKeys key) ? key : InputKeys.None;
        }

        /// <summary>
        /// Получить soju06 код клавиши по её названию
        /// </summary>
        /// <param name="keyName">Название клавиши</param>
        /// <returns>Soju06 код (InputKeys enum значение) или 0 если клавиша не найдена</returns>
        public static int GetKeyCode(string keyName)
        {
            var inputKey = GetInputKey(keyName);
            return inputKey != InputKeys.None ? (int)inputKey : 0;
        }

        /// <summary>
        /// Получить InputButtons enum по названию кнопки мыши
        /// </summary>
        /// <param name="buttonName">Название кнопки</param>
        /// <returns>InputButtons enum или None если кнопка не найдена</returns>
        public static InputButtons GetInputMouseButton(string buttonName)
        {
            return MouseButtons.TryGetValue(buttonName, out InputButtons button) ? button : InputButtons.None;
        }

        /// <summary>
        /// Получить soju06 код кнопки мыши по её названию
        /// </summary>
        /// <param name="buttonName">Название кнопки</param>
        /// <returns>Soju06 код (InputButtons enum значение) или 0 если не найдена</returns>
        public static int GetMouseButtonCode(string buttonName)
        {
            var inputButton = GetInputMouseButton(buttonName);
            return inputButton != InputButtons.None ? (int)inputButton : 0;
        }

        /// <summary>
        /// Получить enum действия контроллера по отображаемому названию
        /// </summary>
        /// <param name="actionName">Отображаемое название действия</param>
        /// <returns>Enum действия или null если не найдено</returns>
        public static ControllerButton? GetControllerAction(string actionName)
        {
            return ControllerActions.TryGetValue(actionName, out ControllerButton action) ? action : null;
        }

        /// <summary>
        /// Получить отображаемое название по enum действия контроллера
        /// </summary>
        /// <param name="action">Enum действия</param>
        /// <returns>Отображаемое название или пустую строку если не найдено</returns>
        public static string GetControllerActionName(ControllerButton action)
        {
            return ControllerActionNames.TryGetValue(action, out string name) ? name : "";
        }
    }
}