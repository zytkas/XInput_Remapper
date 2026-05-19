# MapperGang

Keyboard-and-mouse to virtual-gamepad mapper for Windows. Plays controller-only games with KB/M by emulating an Xbox 360 pad: WASD drives the left stick, mouse movement drives the right stick, and keys/mouse buttons map to face buttons and triggers.

## Stack

- **.NET 8** WPF (`net8.0-windows`)
- **WPF-UI 4** — Fluent UI styling
- **CommunityToolkit.Mvvm** — `ObservableObject` / `RelayCommand`
- **Microsoft.Extensions.DependencyInjection** — DI container wired in [Infrastructure/DI/ContainerConfig.cs](Infrastructure/DI/ContainerConfig.cs)
- **Nefarius.ViGEm.Client** — virtual Xbox 360 pad over the [ViGEm Bus Driver](https://github.com/ViGEm/ViGEmBus)
- **Soju06 Input** — low-level WH_KEYBOARD_LL hook
- **MouHid.dll** (native, P/Invoke) — raw mouse capture and physical-mouse blocking

## Requirements

- Windows 10/11
- [ViGEm Bus Driver](https://github.com/ViGEm/ViGEmBus) installed
- [MouHid.dll](https://github.com/zytkas/MouHid_POC) next to the executable
- Application must be run **as administrator** (low-level hooks + driver IO)

## Project layout

```
MapperGangNET8/
├── App.xaml(.cs)              app entry point, DI bootstrap
├── Infrastructure/
│   ├── Commands/              RelayCommand
│   └── DI/                    ContainerConfig — service registration
├── Models/                    POCOs: ConfigModel, *Settings, KeyBinding, ActionEnum, ControllerModel
├── Services/
│   ├── ConfigService/         persisted config + profiles
│   ├── ControllerService/     virtual Xbox 360 pad via ViGEm
│   ├── InputService/          keyboard hook (Soju06)
│   ├── InputCaptureService/   raw mouse capture / blocking via MouHid
│   └── MappingService/        the pipeline turning KB/M into pad state
├── ViewModels/                MVVM view-models
├── Views/                     WPF views (MainWindow, Mouse/Keyboard/Settings)
├── InputDebugWindow.*         live key/mouse event monitor
└── ControllerDebugWindow.*    manual stick/button/trigger driver for the virtual pad
```

Each service folder has its own `README.md` with details.

## How it fits together

- **InputService** captures keyboard events; **InputCaptureService** captures and blocks the mouse.
- Events flow into **MappingService** (`InputPipeline`), which delegates to:
  - `KeyToControllerMapper` — keys + mouse buttons → buttons, triggers, and WASD → left stick;
  - `MouseToStickMapper` — mouse deltas → right stick (spring/absolute physics, response curves, smoothing).
- A 500 Hz timer inside `InputPipeline` ticks the stick physics and asks **ControllerService** to flush a single `SubmitReport` to ViGEm if anything changed.
- **ConfigService** owns the user config + named profiles at `%USERPROFILE%\Documents\MapperGang\config.json`. ViewModels read/write through it and react to `ProfileChanged` / `ConfigurationReset` events.

## Running

1. Install the ViGEm Bus Driver.
2. Place `MouHid.dll` next to the built executable.
3. Build and run as administrator:
   ```
   dotnet build MapperGangNET8.csproj -c Debug
   ```
4. In the app: connect the virtual pad, configure mappings/sensitivity, enable mapping. `F9` is the global panic-stop; `Mouse4` / `Mouse5` toggle between captured and free-mouse modes.

## Debugging

- **Input Debug Window** — live dump of every key/mouse event going through the hook.
- **Controller Debug Window** — manually drives the virtual pad (sticks, triggers, buttons) to verify the ViGEm connection independently of the mapping pipeline.
