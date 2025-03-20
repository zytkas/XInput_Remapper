namespace MapperGang.Models
{
    /// <summary>
    /// Модель для хранения привязок клавиш к действиям контроллера
    /// </summary>
    public class KeyBindingModel
    {
        /// <summary>
        /// Тип устройства ввода
        /// </summary>
        public InputDeviceType InputType { get; set; }

        /// <summary>
        /// Код клавиши/кнопки устройства ввода
        /// </summary>
        public int InputCode { get; set; }

        /// <summary>
        /// Действие контроллера, на которое привязан ввод
        /// </summary>
        public ControllerAction Action { get; set; }

        /// <summary>
        /// Дополнительные параметры привязки (например, чувствительность)
        /// </summary>
        public object Parameters { get; set; }

        /// <summary>
        /// Описание привязки (например, "Левая кнопка мыши -> A")
        /// </summary>
        public string Description => $"{InputCode} -> {Action}";
    }
}