# InputService

**Keyboard** interception via the [Soju06 Input](https://www.nuget.org/packages/Input) library (low-level WH_KEYBOARD_LL hook).

## Contents

| File | Purpose |
|------|---------|
| `IInputService.cs` | Contract: `KeyDown` / `KeyUp` / `MouseStateChanged` events, `Start/Stop/EnableDebug` methods. |
| `Soju06InputService.cs` | Implementation using `IKeyboardHook` + Windows message pump. Supports key "blocking" via `InputCaptureManager`. |
| `InputEventArgs.cs` | DTO for key/mouse events. |
| `InputMonitorService.cs` | Debug viewer: dumps the event stream into a `TextBox` (used by debug windows). |

## Data flow

- The physical keyboard fires a `WH_KEYBOARD_LL` event.
- `Soju06InputService` raises `KeyDown` / `KeyUp`, which `InputPipeline` forwards to `KeyToControllerMapper`.
- Before forwarding, `Soju06InputService` asks `InputCaptureManager.ShouldBlockKey?` — if true, the event is swallowed.
- The `F9` key is a panic-stop: it stops capture without forwarding the event further.
- `MouseStateChanged` is **not used** by our services — the mouse is handled through `InputCaptureService`. The field remains only for the contract.

## Starting the message pump

After `HookStart()` a background task is launched running `WindowsMessagePump.Pumping()` — this is mandatory for low-level hooks, otherwise the callbacks silently die.
