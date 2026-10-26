# Hive Screen RGB

A portable Windows app that samples your display and sends its colors to a Kreo Hive 65 keyboard. The EXE uses Windows .NET Framework 4.x; no Python, Node, REA, or Kreo installation is required to run it. Built for Windows 10/11. No administrator access is required.

## Use now, without a keyboard

1. Run HiveScreenRGB.exe.
2. Choose your display. Per key is the default: every visible key follows its own rectangular region of the screen. Six zones and whole-screen average are optional.
3. Click Start preview. Play a video or change the visible content to see the keyboard preview react.
4. Adjust brightness and smoothing. Updates target 15 times per second; actual speed depends on capture and USB latency. Vivid colors weights saturated pixels more strongly; switch it off for neutral linear-light averaging. Black stays black and white stays neutral. Brightness 75% and smoothing 55% are the starting settings.

Screen capture happens locally, only while running. No images are saved or transmitted. Minimize the window to avoid including the preview itself in the sampled display. Protected video, secure desktops, or some exclusive fullscreen games may appear black; try borderless windowed mode.

## When your keyboard is available

Connect the Hive 65 with a USB data cable, close the Kreo software, select Enable USB output (experimental), then Start RGB sync. Only VID 258A / PID 010C, usage page FF00, usage 1, and feature-report length 520 are accepted. Bluetooth and wireless dongles are not implemented.

Hardware compatibility is NOT verified. This is a source-derived driver implementation, not a confirmed working hardware release. Test red, green and blue screen content first. If colors, mapping or activation are wrong, stop and collect a USB trace of the vendor app for comparison. A successful Windows write does not prove that firmware displayed the colors. No firmware update, factory reset, macro, or persistent-profile command is sent.

Stop ends streaming; the keyboard may retain the final colors. Restore your preferred effect using Kreo or unplug/replug the keyboard. Automatic restoration of the previous lighting effect is not implemented.

## Build / tests

Run build.ps1 from PowerShell. It uses the Windows .NET Framework C# compiler and standard Windows libraries. Source is in source/HiveScreenRGB.cs.

Run `HiveScreenRGB.exe --self-test C:\path\test-results.txt` to test color averaging, RGB report encoding, brightness/smoothing, desktop capture, and UI creation. Quote paths containing spaces. The test also saves an image of the app next to the report. Self-test sends no USB reports. See TEST-RESULTS.txt for the executed checks.

## REA repository

The requested https://github.com/morluto/rea repository was cloned into work/rea in the original task workspace at commit 2edf3b23c1465b7e6609b020d0eabfef438081a7. It is an analysis toolkit, not the keyboard application. Its MCP setup was not installed or registered, and its native provider was not used. Local PE parsing and x86 disassembly were performed with pefile and Capstone. The EXE is an independent C# implementation and does not contain the vendor executable or REA runtime.

## Interface and color update

Black-and-purple interface inspired by the installed Kreo palette, with readable numeric sliders and a dark title bar where supported by Windows. Display selection opens a menu; mapping and sliders support left/right arrows. Per key maps the entire screen to the physical key rectangles in two dimensions, including wider regions for wide keys. It sends 67 independent colors through the existing RGB matrix protocol. The screen-map and keycap previews use those same colors.

Color processing downsamples to 192 x 108, averages in linear sRGB light, optionally gives saturated pixels more weight, then applies brightness and temporal smoothing. It does not artificially brighten black. This is local desktop capture, not game-specific integrations. HDR tone mapping, LED calibration and automatic letterbox detection are not implemented. Exact monitor-to-LED color matching and hardware behavior need a real keyboard test.
