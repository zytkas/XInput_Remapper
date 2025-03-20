namespace MapperGang.Models
{
    /// <summary>
    /// Перечисление доступных действий контроллера
    /// </summary>
    public enum ControllerAction
    {
        // Кнопки Xbox-контроллера
        A_Button,
        B_Button,
        X_Button,
        Y_Button,

        // Бамперы
        LeftBumper,
        RightBumper,

        // Триггеры
        LeftTrigger,
        RightTrigger,

        // Стики и их нажатия
        LeftStickX,
        LeftStickY,
        LeftStickPress,
        RightStickX,
        RightStickY,
        RightStickPress,

        // D-Pad
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight,

        // Дополнительные кнопки
        Start,
        Back,
        Guide
    }

    /// <summary>
    /// Перечисление типов устройств ввода
    /// </summary>
    public enum InputDeviceType
    {
        Keyboard,
        Mouse,
        GamepadPassthrough
    }
}