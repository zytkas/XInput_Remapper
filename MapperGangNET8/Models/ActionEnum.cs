using System.Collections.Generic;

namespace MapperGangNET8.Models
{
    public enum ControllerAction
    {
        AButton,
        BButton,
        XButton,
        YButton,
        LeftBumper,
        RightBumper,
        LeftTrigger,
        RightTrigger,
        LeftStickX,
        LeftStickY,
        LeftStickPress,
        RightStickX,
        RightStickY,
        RightStickPress,
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight,
        Start,
        Back,
        Guide
    }

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
        /// Словарь клавиш клавиатуры с их виртуальными кодами
        /// </summary>
        public static readonly Dictionary<string, int> KeyboardKeys = new Dictionary<string, int>
        {
            { "Space", 32 },
            { "Left Ctrl", 17 },
            { "Left Shift", 16 },
            { "Left Alt", 18 },
            { "Tab", 9 },
            { "Enter", 13 },
            { "Esc", 27 },
            // Буквы
            { "Q", 81 }, { "W", 87 }, { "E", 69 }, { "R", 82 }, { "T", 84 }, { "Y", 89 }, { "U", 85 }, { "I", 73 }, { "O", 79 }, { "P", 80 },
            { "A", 65 }, { "S", 83 }, { "D", 68 }, { "F", 70 }, { "G", 71 }, { "H", 72 }, { "J", 74 }, { "K", 75 }, { "L", 76 },
            { "Z", 90 }, { "X", 88 }, { "C", 67 }, { "V", 86 }, { "B", 66 }, { "N", 78 }, { "M", 77 },
            // Цифры
            { "1", 49 }, { "2", 50 }, { "3", 51 }, { "4", 52 }, { "5", 53 }, { "6", 54 }, { "7", 55 }, { "8", 56 }, { "9", 57 }, { "0", 48 },
            // Функциональные клавиши
            { "F1", 112 }, { "F2", 113 }, { "F3", 114 }, { "F4", 115 }, { "F5", 116 }, { "F6", 117 },
            { "F7", 118 }, { "F8", 119 }, { "F9", 120 }, { "F10", 121 }, { "F11", 122 }, { "F12", 123 }
        };

        /// <summary>
        /// Словарь кнопок мыши с их кодами
        /// </summary>
        public static readonly Dictionary<string, int> MouseButtons = new Dictionary<string, int>
        {
            { "Left Button", 1 },
            { "Right Button", 2 },
            { "Middle Button", 4 },
            { "Side Button 1", 8 },
            { "Side Button 2", 16 },
            { "Mouse Wheel Up", 120 },
            { "Mouse Wheel Down", -120 },
            { "Extra Button 1", 32 },
            { "Extra Button 2", 64 }
        };

        /// <summary>
        /// Словарь действий контроллера с их отображаемыми названиями
        /// </summary>
        public static readonly Dictionary<string, ControllerAction> ControllerActions = new Dictionary<string, ControllerAction>
        {
            { "A Button", ControllerAction.AButton },
            { "B Button", ControllerAction.BButton },
            { "X Button", ControllerAction.XButton },
            { "Y Button", ControllerAction.YButton },
            { "Left Bumper", ControllerAction.LeftBumper },
            { "Right Bumper", ControllerAction.RightBumper },
            { "Left Trigger", ControllerAction.LeftTrigger },
            { "Right Trigger", ControllerAction.RightTrigger },
            { "Left Stick Press", ControllerAction.LeftStickPress },
            { "Right Stick Press", ControllerAction.RightStickPress },
            { "D-Pad Up", ControllerAction.DPadUp },
            { "D-Pad Down", ControllerAction.DPadDown },
            { "D-Pad Left", ControllerAction.DPadLeft },
            { "D-Pad Right", ControllerAction.DPadRight },
            { "Start Button", ControllerAction.Start },
            { "Back Button", ControllerAction.Back },
            { "Guide Button", ControllerAction.Guide }
        };

        /// <summary>
        /// Обратный словарь для получения отображаемого названия по enum
        /// </summary>
        public static readonly Dictionary<ControllerAction, string> ControllerActionNames = new Dictionary<ControllerAction, string>
        {
            { ControllerAction.AButton, "A Button" },
            { ControllerAction.BButton, "B Button" },
            { ControllerAction.XButton, "X Button" },
            { ControllerAction.YButton, "Y Button" },
            { ControllerAction.LeftBumper, "Left Bumper" },
            { ControllerAction.RightBumper, "Right Bumper" },
            { ControllerAction.LeftTrigger, "Left Trigger" },
            { ControllerAction.RightTrigger, "Right Trigger" },
            { ControllerAction.LeftStickPress, "Left Stick Press" },
            { ControllerAction.RightStickPress, "Right Stick Press" },
            { ControllerAction.DPadUp, "D-Pad Up" },
            { ControllerAction.DPadDown, "D-Pad Down" },
            { ControllerAction.DPadLeft, "D-Pad Left" },
            { ControllerAction.DPadRight, "D-Pad Right" },
            { ControllerAction.Start, "Start Button" },
            { ControllerAction.Back, "Back Button" },
            { ControllerAction.Guide, "Guide Button" }
        };

        /// <summary>
        /// Получить список всех доступных клавиш клавиатуры
        /// </summary>
        public static List<string> GetAvailableKeyboardKeys()
        {
            return new List<string>(KeyboardKeys.Keys);
        }

        /// <summary>
        /// Получить список всех доступных кнопок мыши
        /// </summary>
        public static List<string> GetAvailableMouseButtons()
        {
            return new List<string>(MouseButtons.Keys);
        }

        /// <summary>
        /// Получить список всех доступных действий контроллера
        /// </summary>
        public static List<string> GetAvailableControllerActions()
        {
            return new List<string>(ControllerActions.Keys);
        }

        /// <summary>
        /// Получить виртуальный код клавиши по её названию
        /// </summary>
        /// <param name="keyName">Название клавиши</param>
        /// <returns>Виртуальный код или 0 если клавиша не найдена</returns>
        public static int GetKeyCode(string keyName)
        {
            return KeyboardKeys.TryGetValue(keyName, out int code) ? code : 0;
        }

        /// <summary>
        /// Получить код кнопки мыши по её названию
        /// </summary>
        /// <param name="buttonName">Название кнопки</param>
        /// <returns>Код кнопки или 0 если не найдена</returns>
        public static int GetMouseButtonCode(string buttonName)
        {
            return MouseButtons.TryGetValue(buttonName, out int code) ? code : 0;
        }

        /// <summary>
        /// Получить enum действия контроллера по отображаемому названию
        /// </summary>
        /// <param name="actionName">Отображаемое название действия</param>
        /// <returns>Enum действия или null если не найдено</returns>
        public static ControllerAction? GetControllerAction(string actionName)
        {
            return ControllerActions.TryGetValue(actionName, out ControllerAction action) ? action : null;
        }

        /// <summary>
        /// Получить отображаемое название по enum действия контроллера
        /// </summary>
        /// <param name="action">Enum действия</param>
        /// <returns>Отображаемое название или пустую строку если не найдено</returns>
        public static string GetControllerActionName(ControllerAction action)
        {
            return ControllerActionNames.TryGetValue(action, out string name) ? name : "";
        }
    }
}