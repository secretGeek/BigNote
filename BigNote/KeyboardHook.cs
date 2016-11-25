namespace BigNote
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows.Input;

    public class KeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private LowLevelKeyboardProc keyboardProc;
        private IntPtr hookId = IntPtr.Zero;

        public Key SelectedKey { get; set; }

        public KeyboardHook()
        {
            keyboardProc = HookCallback;
            hookId = SetHook(keyboardProc);
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(hookId);
        }

        public event EventHandler KeyCombinationPressed;

        public void OnKeyCombinationPressed(EventArgs e)
        {
            EventHandler handler = KeyCombinationPressed;
            if (handler != null) handler(null, e);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                                        GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr) WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var keyPressed = KeyInterop.KeyFromVirtualKey(vkCode);
                Trace.WriteLine(keyPressed);
                if (keyPressed == SelectedKey && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    Trace.WriteLine("Triggering Keyboard Hook");
                    OnKeyCombinationPressed(new EventArgs());
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
                                                      LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                                                    IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

}