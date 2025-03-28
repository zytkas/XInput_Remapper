namespace MapperGangNET8.Models
{
    /// <summary>
    /// Модель для хранения привязок клавиш к действиям контроллера
    /// </summary>
    public class KeyBindingModel
    {
        public InputDeviceType InputType { get; set; }

        public int InputCode { get; set; }

        public ControllerAction Action { get; set; }

        public object Parameters { get; set; }

        public string Description => $"{InputCode} -> {Action}";
    }
}