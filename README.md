# Finnish Key Helper (ä, ö, å Key Remapper)

**For Finnish users who prefer to program on a non-Finnish keyboard layout — but still need to type `ä`, `ö`, and `å`.**

Many Finns switch to the US (or another international) keyboard layout for programming, because the brackets `[ ]`, braces `{ }`, backslash `\`, and other symbols programmers use constantly sit in far more convenient positions than on the Finnish/Swedish `fi` layout. The catch: switching layout makes the Finnish `ä`, `ö`, and `å` keys disappear.

This tool solves that problem. It leaves your non-Finnish layout completely intact and simply **adds the missing Finnish letters back** as easy hotkeys — so you can keep coding on the layout you prefer and still write fluent Finnish without ever switching layouts.

A lightweight background daemon for Windows that maps custom key combinations to the Finnish characters `ä`, `ö`, and `å`. Compatible with remote desktop and streaming tools like **Parsec**, **RDP**, and **Moonlight**.

---

## ⌨️ Key Combinations

| Combination | Output | Description |
|---|---|---|
| `Ctrl + Alt + ;` | `ä` | Lowercase a with umlaut (default) |
| `Ctrl + Alt + Shift + ;` | `Ä` | Uppercase A with umlaut (default) |
| `Ctrl + Alt + '` | `ö` | Lowercase o with umlaut (default) |
| `Ctrl + Alt + Shift + '` | `Ö` | Uppercase O with umlaut (default) |
| `Ctrl + Alt + L` | `å` | Lowercase a with ring |
| `Ctrl + Alt + Shift + L` | `Å` | Uppercase A with ring |

*Note: On Windows, `AltGr` is processed as `Ctrl + Alt`, so `AltGr + ;`, `AltGr + '`, and `AltGr + L` also work identically.*
*Tip: Right-click the system tray icon to swap `;` and `'` (`ö` at `;` and `ä` at `'`) if you prefer physical Finnish keyboard positions.*

---

## 🌐 Remote Desktop & Parsec Compatibility

- Uses hardware **Alt+Numpad scan codes** for text injection, ensuring keystrokes stream cleanly through **Parsec** whether `FinnishKeyHelper` is running on the **client (source)** computer, the **host (target)** computer, or **both**.
- Reliably releases modifier states so Chromium/Electron apps (VS Code, Discord, Slack) and text editors never mistake input for hotkey chords.

---

## ⚠️ Important Note on "Windows Services"

A traditional Windows Service (managed via `services.msc`) runs in **Session 0**. Since Windows Vista, **Session 0 Isolation** prevents services from interacting with the user's desktop, keyboard hooks (`SetWindowsHookEx`), and active window inputs (`SendInput`) for security reasons.

To run a keyboard utility as a background "service" on Windows, the industry-standard architecture (used by AutoHotkey, PowerToys, WinCompose, etc.) is a **User-Session Background Daemon**:
- It runs with zero UI or in the System Tray.
- Consumes ~5 MB RAM and 0% CPU.
- Automatically launches when you log in via Windows Startup or Task Scheduler.

---

## 🚀 Quick Start

### 1. Build the Executable
Double-click **`build.bat`**.
It uses Windows' built-in C# compiler (`csc.exe`). No Visual Studio or .NET SDK installation is required.

### 2. Run / Install

- **Option A: Tray Application with Startup Toggle**
  - Simply double-click `FinnishKeyHelper.exe`.
  - A small blue `FI` icon appears in your System Tray.
  - Right-click the icon to toggle **"Start with Windows"** or click **"Exit"**.

- **Option B: One-Click Startup Installer**
  - Double-click `install-startup.bat`.
  - It registers the application to start with Windows and runs it immediately.

- **Option C: Windows Scheduled Task (Service-Grade)**
  - Right-click `install-scheduled-task.ps1` and choose *Run with PowerShell*.
  - This registers it as an elevated background task running on logon with `--silent`.

### 3. Uninstall / Stop
- Run **`uninstall.bat`**. It will stop any running instances and remove the app from Windows Startup and Scheduled Tasks.

---

## 🛠️ AutoHotkey Alternative

If you already have [AutoHotkey](https://www.autohotkey.com/) installed, you can simply run **`FinnishKeys.ahk`**.
