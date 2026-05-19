# ConfigService

Loading/saving user configuration and managing profiles.

## Contents

| File | Purpose |
|------|---------|
| `IConfigService.cs` | Contract: load/save/reset/export/import + profile CRUD + switching the active profile. |
| `FileConfigService.cs` | File-based implementation using `System.Text.Json` on top of `%MyDocuments%\MapperGang\config.json`. |

## Storage location

```
%USERPROFILE%\Documents\MapperGang\config.json
```

The folder is created at service startup if missing. The config is cached in the `_cachedConfig` field after the first load — all profile operations work on the cache and persist it back with a single `SaveConfigAsync` call.

## Structure

`ConfigModel` contains:

- `AppSettings` — global application settings (theme, polling rate, selected controller type, etc.);
- `MouseSettings` / `KeyboardSettings` / `SensitivitySettings` — currently active settings;
- `Profiles` — dictionary of named profiles (each is a copy of Mouse/Keyboard/Sensitivity Settings);
- `ActiveProfile` — name of the current profile.

On `SwitchToProfileAsync(name)` the matching profile is cloned (via JSON round-trip) into the active `*Settings`.

## `Default` profile

Created automatically on first launch. If the config is found without an active profile, it is restored via `EnsureDefaultProfileExists`.

## Events

- `ConfigurationReset` — after `ResetConfigAsync()`;
- `ProfileChanged` — after `SwitchToProfileAsync(name)`.

ViewModels subscribe to them to re-read values into the UI.
