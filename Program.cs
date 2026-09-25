using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace FinnishKeyHelper
{
    internal static class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12; // Alt
        private const int VK_OEM_1 = 0xBA; // ';' on US keyboards
        private const int VK_OEM_7 = 0xDE; // ''' on US keyboards

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;
        private const uint LLKHF_INJECTED = 0x0010;
        private const uint INJECTED_EXTRA_INFO = 0xFEEDBEEF;

        private const string RUN_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_NAME = "FinnishKeyHelper";

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookId = IntPtr.Zero;

        private static bool _suppressedSemicolon = false;
        private static bool _suppressedQuote = false;

        private static NotifyIcon _trayIcon;

        [STAThread]
        private static void Main(string[] args)
        {
            bool silent = false;

            if (args != null && args.Length > 0)
            {
                foreach (string arg in args)
                {
                    string a = arg.Trim().ToLowerInvariant();
                    if (a == "--install" || a == "-i")
                    {
                        SetRunAtStartup(true);
                        return;
                    }
                    else if (a == "--uninstall" || a == "-u")
                    {
                        SetRunAtStartup(false);
                        return;
                    }
                    else if (a == "--silent" || a == "-s")
                    {
                        silent = true;
                    }
                    else if (a == "--help" || a == "-h" || a == "/?")
                    {
                        MessageBox.Show(
                            "Finnish Key Helper\n\n" +
                            "Hotkeys:\n" +
                            "  Ctrl + Alt + ;         -> ä\n" +
                            "  Ctrl + Alt + Shift + ; -> Ä\n" +
                            "  Ctrl + Alt + '         -> ö\n" +
                            "  Ctrl + Alt + Shift + ' -> Ö\n\n" +
                            "Options:\n" +
                            "  --install    Add to Windows startup\n" +
                            "  --uninstall  Remove from Windows startup\n" +
                            "  --silent     Run without system tray icon\n",
                            "Finnish Key Helper",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            bool createdNew;
            using (Mutex mutex = new Mutex(true, "FinnishKeyHelper_SingleInstance_Mutex_98a7c2e1", out createdNew))
            {
                if (!createdNew)
                {
                    return; // Already running
                }

                _hookId = SetHook(_proc);
                if (_hookId == IntPtr.Zero)
                {
                    MessageBox.Show("Failed to install keyboard hook. Error code: " + Marshal.GetLastWin32Error(), "Finnish Key Helper Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!silent)
                {
                    SetupTrayIcon();
                }

                Application.ApplicationExit += (s, e) =>
                {
                    Unhook();
                    if (_trayIcon != null)
                    {
                        _trayIcon.Visible = false;
                        _trayIcon.Dispose();
                    }
                };

                Application.Run();
                Unhook();
            }
        }

        private static void SetupTrayIcon()
        {
            try
            {
                _trayIcon = new NotifyIcon();
                _trayIcon.Text = "Finnish Key Helper (Ctrl+Alt+; = ä, Ctrl+Alt+' = ö)";
                _trayIcon.Icon = CreateAppIcon();

                ContextMenuStrip menu = new ContextMenuStrip();

                ToolStripMenuItem titleItem = new ToolStripMenuItem("Finnish Key Helper (Active)");
                titleItem.Enabled = false;
                titleItem.Font = new Font(titleItem.Font, FontStyle.Bold);
                menu.Items.Add(titleItem);

                menu.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem info1 = new ToolStripMenuItem("Ctrl + Alt + ;  -->  ä (Ä)");
                info1.Enabled = false;
                menu.Items.Add(info1);

                ToolStripMenuItem info2 = new ToolStripMenuItem("Ctrl + Alt + '  -->  ö (Ö)");
                info2.Enabled = false;
                menu.Items.Add(info2);

                menu.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem startupItem = new ToolStripMenuItem("Start with Windows");
                startupItem.Checked = IsRunAtStartup();
                startupItem.Click += (s, e) =>
                {
                    bool newState = !startupItem.Checked;
                    SetRunAtStartup(newState);
                    startupItem.Checked = newState;
                };
                menu.Items.Add(startupItem);

                menu.Items.Add(new ToolStripSeparator());

                ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
                exitItem.Click += (s, e) =>
                {
                    Application.Exit();
                };
                menu.Items.Add(exitItem);

                _trayIcon.ContextMenuStrip = menu;
                _trayIcon.Visible = true;
            }
            catch
            {
                // Fallback to silent if tray icon creation fails
            }
        }

        private static Icon CreateAppIcon()
        {
            using (Bitmap bmp = new Bitmap(16, 16))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(0, 53, 128)); // Blue background
                    using (Font font = new Font("Arial", 8, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (SolidBrush brush = new SolidBrush(Color.White))
                    {
                        g.DrawString("FI", font, brush, 1, 3);
                    }
                }
                return Icon.FromHandle(bmp.GetHicon());
            }
        }

        public static bool IsRunAtStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_KEY, false))
                {
                    return key != null && key.GetValue(APP_NAME) != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void SetRunAtStartup(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true))
                {
                    if (key != null)
                    {
                        if (enable)
                        {
                            key.SetValue(APP_NAME, "\"" + Application.ExecutablePath + "\"");
                        }
                        else
                        {
                            key.DeleteValue(APP_NAME, false);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static void Unhook()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT info = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

                // Ignore keys injected by SendInput or our own application
                if ((info.flags & LLKHF_INJECTED) != 0 || (uint)info.dwExtraInfo == INJECTED_EXTRA_INFO)
                {
                    return CallNextHookEx(_hookId, nCode, wParam, lParam);
                }

                int msg = wParam.ToInt32();
                bool isKeyDown = (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN);
                bool isKeyUp = (msg == WM_KEYUP || msg == WM_SYSKEYUP);

                // Check for ';' or '''
                // MapVirtualKey: 2 = MAPVK_VK_TO_CHAR
                uint mappedChar = MapVirtualKey((uint)info.vkCode, 2) & 0xFFFF;
                bool isSemicolon = (info.vkCode == VK_OEM_1 || info.scanCode == 0x27 || mappedChar == ';');
                bool isQuote = (info.vkCode == VK_OEM_7 || info.scanCode == 0x28 || mappedChar == '\'');

                if (isSemicolon || isQuote)
                {
                    bool ctrl = (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;
                    bool alt = (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;

                    if (isSemicolon)
                    {
                        if (isKeyDown && ctrl && alt)
                        {
                            _suppressedSemicolon = true;
                            bool shift = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
                            char targetChar = shift ? 'Ä' : 'ä';
                            InjectUnicodeCharacter(targetChar, ctrl, alt, shift);
                            return (IntPtr)1; // Swallow original keydown
                        }
                        else if (isKeyUp && _suppressedSemicolon)
                        {
                            _suppressedSemicolon = false;
                            return (IntPtr)1; // Swallow original keyup
                        }
                    }
                    else if (isQuote)
                    {
                        if (isKeyDown && ctrl && alt)
                        {
                            _suppressedQuote = true;
                            bool shift = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
                            char targetChar = shift ? 'Ö' : 'ö';
                            InjectUnicodeCharacter(targetChar, ctrl, alt, shift);
                            return (IntPtr)1; // Swallow original keydown
                        }
                        else if (isKeyUp && _suppressedQuote)
                        {
                            _suppressedQuote = false;
                            return (IntPtr)1; // Swallow original keyup
                        }
                    }
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private static void InjectUnicodeCharacter(char c, bool releaseCtrl, bool releaseAlt, bool releaseShift)
        {
            List<INPUT> inputs = new List<INPUT>();

            // 1. Temporarily release modifier keys so apps don't interpret this as Ctrl+Alt+char shortcut
            if (releaseCtrl)
            {
                inputs.Add(CreateKeyInput(VK_CONTROL, KEYEVENTF_KEYUP));
            }
            if (releaseAlt)
            {
                inputs.Add(CreateKeyInput(VK_MENU, KEYEVENTF_KEYUP));
            }
            if (releaseShift)
            {
                inputs.Add(CreateKeyInput(VK_SHIFT, KEYEVENTF_KEYUP));
            }

            // 2. Inject Unicode character (down and up)
            inputs.Add(CreateUnicodeInput(c, false));
            inputs.Add(CreateUnicodeInput(c, true));

            // 3. Re-press modifiers if physically held so continued holding works
            if (releaseShift && (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0)
            {
                inputs.Add(CreateKeyInput(VK_SHIFT, 0));
            }
            if (releaseAlt && (GetAsyncKeyState(VK_MENU) & 0x8000) != 0)
            {
                inputs.Add(CreateKeyInput(VK_MENU, 0));
            }
            if (releaseCtrl && (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0)
            {
                inputs.Add(CreateKeyInput(VK_CONTROL, 0));
            }

            INPUT[] array = inputs.ToArray();
            SendInput((uint)array.Length, array, Marshal.SizeOf(typeof(INPUT)));
        }

        private static INPUT CreateKeyInput(ushort vk, uint flags)
        {
            INPUT input = new INPUT();
            input.type = INPUT_KEYBOARD;
            input.u.ki.wVk = vk;
            input.u.ki.wScan = 0;
            input.u.ki.dwFlags = flags;
            input.u.ki.time = 0;
            input.u.ki.dwExtraInfo = (UIntPtr)INJECTED_EXTRA_INFO;
            return input;
        }

        private static INPUT CreateUnicodeInput(char c, bool keyUp)
        {
            INPUT input = new INPUT();
            input.type = INPUT_KEYBOARD;
            input.u.ki.wVk = 0;
            input.u.ki.wScan = (ushort)c;
            input.u.ki.dwFlags = KEYEVENTF_UNICODE | (keyUp ? KEYEVENTF_KEYUP : 0);
            input.u.ki.time = 0;
            input.u.ki.dwExtraInfo = (UIntPtr)INJECTED_EXTRA_INFO;
            return input;
        }

        #region Win32 API

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, [In] INPUT[] pInputs, int cbSize);

        #endregion
    }
}
