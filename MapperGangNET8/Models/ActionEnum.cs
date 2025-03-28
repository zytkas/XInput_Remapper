namespace MapperGang.Models
{
    public enum ControllerAction
    {

        A_Button,
        B_Button,
        X_Button,
        Y_Button,


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
        GamepadPassthrough
    }
}