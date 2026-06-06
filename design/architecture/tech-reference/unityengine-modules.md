---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# UnityEngine modules (Core / IMGUI / TextRendering) @ game-bound (RimWorld's bundled Unity)

## Canonical source
- Official docs: https://docs.unity3d.com/ScriptReference/ (Unity Scripting API)
- Last verified: 2026-06-06
- Note: the exact Unity version is whatever RimWorld 1.6 ships; the assemblies are RimWorld's bundled build, not a NuGet/UPM package. Decompile the referenced DLLs in `$(RimWorldManagedDir)` to confirm the precise surface.

## Reference nature (game-bound, not a package)
- Three modules are referenced by `HintPath` from `$(RimWorldManagedDir)`, all `Private=False`:
  - `UnityEngine.CoreModule.dll`
  - `UnityEngine.IMGUIModule.dll`
  - `UnityEngine.TextRenderingModule.dll`
- **No package version**: these are the Unity runtime DLLs shipped inside RimWorld. They are loaded by the game; the mod compiles against them but never copies them to output.
- This API surface is broadly documented by Unity, but the *specific version* RimWorld bundles is not, so version-sensitive behavior must be checked against the actual DLLs.

## API surface used in project
- `UnityEngine.CoreModule`: core value types and primitives used throughout the UI and rule layer — `Rect`, `Vector2`, `Color`, `Mathf`, `Time`. These back RimWorld's `Verse`/`UI` types and the dialog layout math.
- `UnityEngine.IMGUIModule`: the immediate-mode GUI used for all mod windows. `GUI`, `GUILayout`, `GUIStyle`, `Event` underpin RimWorld's `Verse.Widgets`/`Verse.GUI` helpers that the `Windows/*` dialogs and `CustomWidgets/UiHelpers` call into. RimWorld's UI is IMGUI, so every dialog frame runs in `OnGUI`-style immediate mode.
- `UnityEngine.TextRenderingModule`: font/text glyph rendering (`Font`, `TextGenerator`, `GUIText`-era types) that backs label and tooltip drawing in the dialogs.

The mod rarely calls Unity types directly; it consumes them transitively through RimWorld's `Verse.Widgets` / `Verse.GUI` and through `LordKuper.Common.UI` widgets. The three references exist because those RimWorld/Common UI types expose Unity types (`Rect`, `Color`, `GUIStyle`) in their public signatures, so the compiler needs the modules on the reference path.

## Version-specific notes
- IMGUI is the only supported UI path in RimWorld; do not introduce UI Toolkit / UGUI assumptions.
- Immediate-mode means no retained widget tree: dialogs redraw every frame inside RimWorld's window pump; state lives in the `Window` subclass fields, not in Unity objects.

## Deprecations and breaking changes from prior version
- Unity module split: older monolithic `UnityEngine.dll` is replaced by per-module assemblies (Core/IMGUI/TextRendering). This project already references the split modules; do not add a reference to a monolithic `UnityEngine.dll`.

## Project conventions
- Reference only the three modules actually needed (Core, IMGUI, TextRendering); do not add further Unity modules without a build-time justification.
- All UI is drawn via RimWorld `Verse.Widgets`/`Verse.GUI` and `LordKuper.Common.UI.Widgets`; reach for raw `GUI`/`GUILayout` only when no helper exists.
- Layout uses `Rect`-based manual positioning (RimWorld idiom), not `GUILayout` auto-flow, for consistency with the rest of the mod.

## Known issues and workarounds
- Bundled Unity version is undocumented for RimWorld → confirm any version-sensitive API against the actual DLLs in `$(RimWorldManagedDir)`.
- IMGUI calls are only valid during the GUI phase → never invoke `GUI`/`Widgets` from background or save/load code paths.
