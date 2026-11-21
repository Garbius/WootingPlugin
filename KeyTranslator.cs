using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Sandbox;
using VRage.Input;
using VRage.Utils;

namespace WootingPlugin
{
    /// <summary>
    /// Translates the game's key codes to the physical keys the Wooting SDK reads.
    ///
    /// The game identifies keys by their Windows virtual key code, which depends on the
    /// keyboard layout: on Dvorak the key at the QWERTY "E" position is OemPeriod. The
    /// keyboard reports analog values per physical key, as USB HID usage codes. The SDK's
    /// own virtual key mode assumes a US layout, so the translation is done here with the
    /// layout the game window uses.
    /// </summary>
    internal static class KeyTranslator
    {
        // How often to check whether the keyboard layout was switched, in milliseconds
        private const int LayoutCheckInterval = 500;

        private const uint MAPVK_VK_TO_VSC = 0;

        // Cache entries: 0 is not translated yet, NoKey is a key without a physical position
        private const ushort NoKey = 0xFFFF;
        private static readonly ushort[] cache = new ushort[256];

        private static IntPtr layout;
        private static bool layoutKnown;
        private static int lastLayoutCheck;
        private static bool unavailable;

        /// <summary>
        /// Returns the HID usage code of the physical key that produces the given game key
        /// on the current keyboard layout, or 0 if there is none.
        /// </summary>
        public static ushort GetHidCode(MyKeys key)
        {
            if (unavailable)
                return 0;

            byte virtualKey = (byte)key;

            try
            {
                int now = Environment.TickCount;
                if (!layoutKnown || unchecked(now - lastLayoutCheck) > LayoutCheckInterval)
                {
                    lastLayoutCheck = now;
                    UpdateLayout();
                }

                ushort code = cache[virtualKey];
                if (code == 0)
                {
                    IntPtr currentLayout = layout;
                    code = Translate(virtualKey, vk => MapVirtualKeyEx(vk, MAPVK_VK_TO_VSC, currentLayout));
                    if (code == 0)
                        code = NoKey;
                    cache[virtualKey] = code;
                }

                return code == NoKey ? (ushort)0 : code;
            }
            catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException)
            {
                // Without the Windows keyboard functions there is no way to tell which
                // physical key a game key is, so the controls stay digital
                unavailable = true;
                MyLogExtensions.Error(MySandboxGame.Log, $"WootingPlugin: Cannot translate key codes, analog input stays disabled: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Translates a virtual key code to a HID usage code. The scan code lookup is
        /// passed in, so this can be checked without Windows.
        /// </summary>
        internal static ushort Translate(byte virtualKey, Func<uint, uint> toScanCode)
        {
            if (virtualKey == 0)
                return 0;

            if (fixedKeys.TryGetValue(virtualKey, out byte fixedCode))
                return fixedCode;

            // Only plain scan codes are expected here, keys with an extended scan code are fixed keys
            uint scanCode = toScanCode(virtualKey);
            if (scanCode == 0 || scanCode > 0xFF)
                return 0;

            return scanCodes.TryGetValue((byte)scanCode, out byte code) ? code : (ushort)0;
        }

        private static void UpdateLayout()
        {
            // The layout is a property of the thread that owns the window receiving the
            // input. The game window is in front whenever analog values are read.
            IntPtr current = IntPtr.Zero;
            IntPtr window = GetForegroundWindow();
            if (window != IntPtr.Zero)
            {
                uint thread = GetWindowThreadProcessId(window, out uint windowProcess);
                if (thread != 0 && windowProcess == GetCurrentProcessId())
                    current = GetKeyboardLayout(thread);
            }

            if (current == IntPtr.Zero)
            {
                // Another program is in front, keep what is known
                if (layoutKnown)
                    return;

                current = GetKeyboardLayout(0);
            }

            if (layoutKnown && current == layout)
                return;

            layout = current;
            layoutKnown = true;
            Array.Clear(cache, 0, cache.Length);
            MyLogExtensions.Info(MySandboxGame.Log, $"WootingPlugin: Using keyboard layout {unchecked((uint)current.ToInt64()):X8}");
        }

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentProcessId();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        // Virtual key code to HID usage code, for keys that are at the same position on
        // every layout. Windows only gives the numpad twin of the navigation keys for
        // them, or cannot tell left from right.
        private static readonly Dictionary<byte, byte> fixedKeys = new Dictionary<byte, byte>
        {
            { 0x10, 0xE1 }, // Shift (left)
            { 0x11, 0xE0 }, // Control (left)
            { 0x12, 0xE2 }, // Alt (left)
            { 0x13, 0x48 }, // Pause
            { 0x21, 0x4B }, // Page Up
            { 0x22, 0x4E }, // Page Down
            { 0x23, 0x4D }, // End
            { 0x24, 0x4A }, // Home
            { 0x25, 0x50 }, // Left
            { 0x26, 0x52 }, // Up
            { 0x27, 0x4F }, // Right
            { 0x28, 0x51 }, // Down
            { 0x2C, 0x46 }, // Print Screen
            { 0x2D, 0x49 }, // Insert
            { 0x2E, 0x4C }, // Delete
            { 0x5B, 0xE3 }, // Left Windows
            { 0x5C, 0xE7 }, // Right Windows
            { 0x5D, 0x65 }, // Apps
            { 0x60, 0x62 }, // Numpad 0
            { 0x61, 0x59 }, // Numpad 1
            { 0x62, 0x5A }, // Numpad 2
            { 0x63, 0x5B }, // Numpad 3
            { 0x64, 0x5C }, // Numpad 4
            { 0x65, 0x5D }, // Numpad 5
            { 0x66, 0x5E }, // Numpad 6
            { 0x67, 0x5F }, // Numpad 7
            { 0x68, 0x60 }, // Numpad 8
            { 0x69, 0x61 }, // Numpad 9
            { 0x6A, 0x55 }, // Numpad Multiply
            { 0x6B, 0x57 }, // Numpad Add
            { 0x6D, 0x56 }, // Numpad Subtract
            { 0x6E, 0x63 }, // Numpad Decimal
            { 0x6F, 0x54 }, // Numpad Divide
            { 0x90, 0x53 }, // Num Lock
            { 0xA0, 0xE1 }, // Left Shift
            { 0xA1, 0xE5 }, // Right Shift
            { 0xA2, 0xE0 }, // Left Control
            { 0xA3, 0xE4 }, // Right Control
            { 0xA4, 0xE2 }, // Left Alt
            { 0xA5, 0xE6 }, // Right Alt
        };

        // Scan code (set 1, without the extended ones) to HID usage code
        private static readonly Dictionary<byte, byte> scanCodes = new Dictionary<byte, byte>
        {
            { 0x01, 0x29 }, // Escape
            { 0x02, 0x1E }, // Digit1
            { 0x03, 0x1F }, // Digit2
            { 0x04, 0x20 }, // Digit3
            { 0x05, 0x21 }, // Digit4
            { 0x06, 0x22 }, // Digit5
            { 0x07, 0x23 }, // Digit6
            { 0x08, 0x24 }, // Digit7
            { 0x09, 0x25 }, // Digit8
            { 0x0A, 0x26 }, // Digit9
            { 0x0B, 0x27 }, // Digit0
            { 0x0C, 0x2D }, // Minus
            { 0x0D, 0x2E }, // Equal
            { 0x0E, 0x2A }, // Backspace
            { 0x0F, 0x2B }, // Tab
            { 0x10, 0x14 }, // Q
            { 0x11, 0x1A }, // W
            { 0x12, 0x08 }, // E
            { 0x13, 0x15 }, // R
            { 0x14, 0x17 }, // T
            { 0x15, 0x1C }, // Y
            { 0x16, 0x18 }, // U
            { 0x17, 0x0C }, // I
            { 0x18, 0x12 }, // O
            { 0x19, 0x13 }, // P
            { 0x1A, 0x2F }, // Bracket Left
            { 0x1B, 0x30 }, // Bracket Right
            { 0x1C, 0x28 }, // Enter
            { 0x1D, 0xE0 }, // Control Left
            { 0x1E, 0x04 }, // A
            { 0x1F, 0x16 }, // S
            { 0x20, 0x07 }, // D
            { 0x21, 0x09 }, // F
            { 0x22, 0x0A }, // G
            { 0x23, 0x0B }, // H
            { 0x24, 0x0D }, // J
            { 0x25, 0x0E }, // K
            { 0x26, 0x0F }, // L
            { 0x27, 0x33 }, // Semicolon
            { 0x28, 0x34 }, // Quote
            { 0x29, 0x35 }, // Backquote
            { 0x2A, 0xE1 }, // Shift Left
            { 0x2B, 0x31 }, // Backslash
            { 0x2C, 0x1D }, // Z
            { 0x2D, 0x1B }, // X
            { 0x2E, 0x06 }, // C
            { 0x2F, 0x19 }, // V
            { 0x30, 0x05 }, // B
            { 0x31, 0x11 }, // N
            { 0x32, 0x10 }, // M
            { 0x33, 0x36 }, // Comma
            { 0x34, 0x37 }, // Period
            { 0x35, 0x38 }, // Slash
            { 0x36, 0xE5 }, // Shift Right
            { 0x37, 0x55 }, // Numpad Multiply
            { 0x38, 0xE2 }, // Alt Left
            { 0x39, 0x2C }, // Space
            { 0x3A, 0x39 }, // Caps Lock
            { 0x3B, 0x3A }, // F1
            { 0x3C, 0x3B }, // F2
            { 0x3D, 0x3C }, // F3
            { 0x3E, 0x3D }, // F4
            { 0x3F, 0x3E }, // F5
            { 0x40, 0x3F }, // F6
            { 0x41, 0x40 }, // F7
            { 0x42, 0x41 }, // F8
            { 0x43, 0x42 }, // F9
            { 0x44, 0x43 }, // F10
            { 0x45, 0x53 }, // Num Lock
            { 0x46, 0x47 }, // Scroll Lock
            { 0x47, 0x5F }, // Numpad7
            { 0x48, 0x60 }, // Numpad8
            { 0x49, 0x61 }, // Numpad9
            { 0x4A, 0x56 }, // Numpad Subtract
            { 0x4B, 0x5C }, // Numpad4
            { 0x4C, 0x5D }, // Numpad5
            { 0x4D, 0x5E }, // Numpad6
            { 0x4E, 0x57 }, // Numpad Add
            { 0x4F, 0x59 }, // Numpad1
            { 0x50, 0x5A }, // Numpad2
            { 0x51, 0x5B }, // Numpad3
            { 0x52, 0x62 }, // Numpad0
            { 0x53, 0x63 }, // Numpad Decimal
            { 0x56, 0x64 }, // Intl Backslash
            { 0x57, 0x44 }, // F11
            { 0x58, 0x45 }, // F12
            { 0x59, 0x67 }, // Numpad Equal
            { 0x64, 0x68 }, // F13
            { 0x65, 0x69 }, // F14
            { 0x66, 0x6A }, // F15
            { 0x67, 0x6B }, // F16
            { 0x68, 0x6C }, // F17
            { 0x69, 0x6D }, // F18
            { 0x6A, 0x6E }, // F19
            { 0x6B, 0x6F }, // F20
            { 0x6C, 0x70 }, // F21
            { 0x6D, 0x71 }, // F22
            { 0x6E, 0x72 }, // F23
            { 0x70, 0x88 }, // Kana Mode
            { 0x71, 0x91 }, // Lang2
            { 0x72, 0x90 }, // Lang1
            { 0x73, 0x87 }, // Intl Ro
            { 0x76, 0x73 }, // F24
            { 0x77, 0x93 }, // Lang4
            { 0x78, 0x92 }, // Lang3
            { 0x79, 0x8A }, // Convert
            { 0x7B, 0x8B }, // Non Convert
            { 0x7D, 0x89 }, // Intl Yen
            { 0x7E, 0x85 }, // Numpad Comma
        };
    }
}
