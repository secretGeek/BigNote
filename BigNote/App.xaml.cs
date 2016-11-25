
namespace BigNote
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Threading;

    public partial class App : Application
    {
        private MainWindow window;
        private WindowInteropHelper interopHelper;

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;

        public App(KeyboardHook keyboardHook)
        {
            if (keyboardHook == null) throw new ArgumentNullException("keyboardHook");
            keyboardHook.KeyCombinationPressed += KeyCombinationPressed;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            window = new MainWindow();
            interopHelper = new WindowInteropHelper(window);
            window.Show();
        }

        void KeyCombinationPressed(object sender, EventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(ActivateWindow));
        }

        private void ActivateWindow()
        {
            try
            {
                IntPtr currentForegroundWindow = GetForegroundWindow();
                uint thisWindowThreadId = GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);
                uint currentForegroundWindowThreadId = GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);
                AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);
                //SetForegroundWindow(thisWindowHandle);
                SetWindowPos(interopHelper.Handle, new IntPtr(0), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);

                AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);

                window.WindowState = WindowState.Maximized;
                window.Show();
                window.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
            }
            //…do all stuff here


            //window.Topmost = true;
            //window.Activate();

            //// force window to have focus
            //uint foreThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            //uint appThread = GetCurrentThreadId();
            ////const int SW_SHOW = 5;
            //if (foreThread != appThread)
            //{
            //    AttachThreadInput(foreThread, appThread, true);
            //    BringWindowToTop(interopHelper.Handle);
            //    //ShowWindow(form.Handle, SW_SHOW);
            //    window.Show();
            //    AttachThreadInput(foreThread, appThread, false);
            //}
            //else
            //{
            //    BringWindowToTop(interopHelper.Handle);
            //    //ShowWindow(form.Handle, SW_SHOW);
            //    window.Show();
            //}

            ////this.Show();
            ////this.Activate();
            //////…do all stuff here


            //////var result = SetFocus(interopHelper.Handle);
            ////Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(() => SetFocus(interopHelper.Handle)));
            //////System.Diagnostics.Trace.WriteLine("SetFocus : " + result);
            //window.mainTextBox.Focus();
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(IntPtr hWnd);

        /// <summary>The GetForegroundWindow function returns a handle to the foreground window.</summary>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BringWindowToTop(HandleRef hWnd);

    }
}
