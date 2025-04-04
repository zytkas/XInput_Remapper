using MapperGangNET8.Models;
using MapperGangNET8.Services.ConfigService;
using MapperGangNET8.Services.ProfileService;

public class ProfileService : IProfileService
{
    private readonly IConfigService _configService;
    private ConfigModel _currentConfig;
    private string _activeProfileName;

    public ProfileService(IConfigService configService)
    {
        _configService = configService;
        _ = Initialize();
    }

    private async Task Initialize()
    {
        _currentConfig = await _configService.LoadConfigAsync();
        _activeProfileName = _currentConfig.ActiveProfile;

        if (!_currentConfig.Profiles.ContainsKey(_activeProfileName))
        {
            ProfileModel defaultProfile = new ProfileModel
            {
                Name = "Default",
                Description = "Профиль по умолчанию"
            };

            _currentConfig.Profiles["Default"] = defaultProfile;
            _currentConfig.ActiveProfile = "Default";
            _activeProfileName = "Default";

            await _configService.SaveConfigAsync(_currentConfig);
        }
    }

    public async Task<List<string>> GetProfilesAsync()
    {
        await EnsureInitialized();
        return _currentConfig.Profiles.Keys.ToList();
    }

    public async Task<ProfileModel> GetProfileAsync(string name)
    {
        await EnsureInitialized();
        return _currentConfig.Profiles.TryGetValue(name, out ProfileModel profile) ? profile : null;
    }

    public async Task<ProfileModel> CreateProfileAsync(string name, string description = "")
    {
        await EnsureInitialized();

        if (_currentConfig.Profiles.ContainsKey(name))
        {
            return _currentConfig.Profiles[name];
        }

        ProfileModel newProfile = new ProfileModel
        {
            Name = name,
            Description = description
        };

        _currentConfig.Profiles[name] = newProfile;

        await _configService.SaveConfigAsync(_currentConfig);

        return newProfile;
    }

    public async Task UpdateProfileAsync(string name, ProfileModel profile)
    {
        await EnsureInitialized();

        if (!_currentConfig.Profiles.ContainsKey(name))
        {
            return;
        }
        _currentConfig.Profiles[name] = profile;

        await _configService.SaveConfigAsync(_currentConfig);
    }

    public async Task DeleteProfileAsync(string name)
    {
        await EnsureInitialized();

        if (!_currentConfig.Profiles.ContainsKey(name))
        {
            return;
        }

        if (_activeProfileName == name)
        {
            return;
        }
        _currentConfig.Profiles.Remove(name);

        await _configService.SaveConfigAsync(_currentConfig);
    }

    public async Task SwitchToProfileAsync(string name)
    {
        await EnsureInitialized();

        if (!_currentConfig.Profiles.ContainsKey(name))
        {
            return;
        }
        _currentConfig.ActiveProfile = name;
        _activeProfileName = name;

        await _configService.SaveConfigAsync(_currentConfig);
    }

    public async Task<ProfileModel> GetActiveProfileAsync()
    {
        await EnsureInitialized();
        return _currentConfig.Profiles[_activeProfileName];
    }

    public async Task<string> GetActiveProfileNameAsync()
    {
        await EnsureInitialized();
        return _activeProfileName;
    }

    /// <summary>
    /// Убедиться, что сервис инициализирован
    /// </summary>
    private async Task EnsureInitialized()
    {
        if (_currentConfig == null)
        {
            await Initialize();
        }
    }
}