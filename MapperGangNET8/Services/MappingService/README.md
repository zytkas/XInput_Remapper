# MappingService

Conversion of keyboard/mouse input into virtual gamepad state. The heart of the application.

## Contents

| File | Purpose |
|------|---------|
| `InputPipeline.cs` | Orchestrator: subscriptions to input services, ownership of the 500 Hz timer, ClipCursor, propagating config to mappers. |
| `KeyToControllerMapper.cs` | Keyboard + mouse buttons → buttons / trigger axes. Plus WASD → left stick with diagonal normalization. |
| `MouseToStickMapper.cs` | Mouse → right stick. reWASD-like chain: noise filter → sensitivity/scale → spring decay → response curve → smoothing → inversion. |

## Pipeline

- **Keyboard events** → `KeyToControllerMapper.ProcessKeyDown/Up` → `IControllerService.SetButton/SetAxis` (state only, no submit).
- **Mouse buttons** → `KeyToControllerMapper.ProcessMouseButton` → same path.
- **Mouse deltas** → `MouseToStickMapper.ProcessMouseDelta` (accumulates momentum).
- **Timer tick (~2 ms ≈ 500 Hz)** → `MouseToStickMapper.Update()` runs stick physics and calls `SetAxis`, then `IControllerService.Submit()` flushes a single `SubmitReport` if any state was dirty.

## Right-stick physics

`MouseToStickMapper`:

- **Spring mode** (default): the accumulator decays exponentially toward zero — the stick returns to center at a rate of `1 / (returnTime + 1)` per millisecond.
- **Absolute mode**: the accumulator is simply clamped to `[-32767, 32767]`, with no return.
- **Response curve** — 4-point piecewise-linear interpolation; presets `Linear` / `Precision` / `Aggressive`.
- **Smoothing** — exponential moving average on the output, `alpha = 1 - smoothing/20`.
- **Inversion** — at the final stage, before normalization.

## ClipCursor

While the pipeline is enabled, the cursor is "pinned" to a 1×1 rectangle in the center of the screen via `ClipCursor`. This prevents the OS from losing relative deltas at the screen edges. On `SetEnabled(false)` the clip is released.

## What the pipeline does NOT do

- It does not call `SubmitReport` itself — that is the job of `IControllerService.Submit()`.
- It does not connect the controller automatically — that is an explicit step via `ConnectControllerAsync()` / `SetMappingEnabledAsync()`.
