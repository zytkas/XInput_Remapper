namespace MapperGang.Services.ConfigResetService
{
    public interface IConfigResetService
    {
        event EventHandler ConfigurationReset;
        void NotifyConfigurationReset();
    }
}
namespace MapperGang.Services.ConfigResetService
{
    public class ConfigResetService : IConfigResetService
    {
        public event EventHandler ConfigurationReset;

        public void NotifyConfigurationReset()
        {
            ConfigurationReset?.Invoke(this, EventArgs.Empty);
        }
    }
}