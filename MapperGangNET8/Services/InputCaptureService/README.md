# InputCaptureService

Low-level mouse capture and blocking via the native **`MouHid.dll`** driver (P/Invoke).

## Contents

| File | Purpose |
|------|---------|
| `InputCaptureManager.cs` | Driver manager: connection, blocking, packet read loop, `MouseDeltaAction` / `MouseButtonAction` events. |

## How it works

`MouHid.dll` is a third-party kernel component that can:
- intercept mouse events **before** the OS sees them;
- fully "mute" the physical mouse for the system when `SetBlocking(true)` is set;
- expose raw deltas and button states via `ReadMouseData`.

Inside `InputCaptureManager` a single read loop runs in two modes:

- **driver mode (block)** — packets are read and events are emitted; pressing Mouse4/Mouse5 switches to system mode.
- **system mode (unblock)** — the loop waits for Mouse4/Mouse5; pressing one switches back to driver mode.

In driver mode we filter `MOUSE_BUTTON_4_DOWN` (0x0040) and `MOUSE_BUTTON_5_DOWN` (0x0100) as "unblock hotkeys" — this gives the player a quick escape from capture.

## Integration

`InputPipeline` subscribes to both `Action` events and calls:
- `UpdateBlockedKeys(...)` / `UpdateBlockedMouseButtons(...)` — after a config change;
- `EnableMouInput(true|false)` — when enabling/disabling mapping.

## Requirements

- `MouHid.dll` must sit next to the executable.
- The application must be run **as administrator** — otherwise the Mouse4/Mouse5 system-side handling does not work.
