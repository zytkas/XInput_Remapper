# ControllerService

Virtual gamepad emulation via the **ViGEm Bus** (`Nefarius.ViGEm.Client`). Only **Xbox 360** is supported.

## Contents

| File | Purpose |
|------|---------|
| `IControllerService.cs` | Contract: `Connect/Disconnect/SetButton/SetAxis/Submit/...` + connection/state events. |
| `ViGemControllerService.cs` | Implementation on top of `ViGEmClient` + `IXbox360Controller`. |

## Report batching

`SetButton` / `SetAxis` do **not** call `SubmitReport()` immediately — they only mutate the local `ControllerState` and flip the `_dirty` flag via `Interlocked.Exchange`. The actual send happens in `Submit()`, which is invoked:

- from the `InputPipeline` timer (~500 Hz) while mapping is active;
- from `ControllerDebugWindow` after manual manipulations (stick, buttons, triggers);
- from `ResetState()` immediately.

This removes 6× `SubmitReport` calls per tick when working with the stick and smooths the load on the ViGEm bus.

## Requirements

- The **ViGEm Bus Driver** must be installed (https://github.com/ViGEm/ViGEmBus).
- Without the driver, `new ViGEmClient()` throws — the exception is logged and surfaced via the `ConnectionStateChanged` event.

## Extending

If DualShock 4 / DualSense support is ever needed — add a separate `ControllerType` branch with a parallel `IDualShock4Controller`; the shared `ControllerState` is already abstracted.
