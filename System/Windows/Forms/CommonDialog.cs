namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ToolboxItemFilter("System.Windows.Forms")]
    public abstract class CommonDialog : Component
    {
        private const int CDM_SETDEFAULTFOCUS = 0x451;
        private IntPtr defaultControlHwnd;
        private IntPtr defOwnerWndProc;
        private static readonly object EventHelpRequest = new object();
        private static int helpMsg;
        private IntPtr hookedWndProc;
        private object userData;

        [System.Windows.Forms.SRDescription("CommonDialogHelpRequested")]
        public event EventHandler HelpRequest
        {
            add
            {
                base.Events.AddHandler(EventHelpRequest, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventHelpRequest, value);
            }
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected virtual IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            if (msg == 0x110)
            {
                MoveToScreenCenter(hWnd);
                this.defaultControlHwnd = wparam;
                System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(null, wparam));
            }
            else if (msg == 7)
            {
                System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(null, hWnd), 0x451, 0, 0);
            }
            else if (msg == 0x451)
            {
                System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(this, this.defaultControlHwnd));
            }
            return IntPtr.Zero;
        }

        internal static void MoveToScreenCenter(IntPtr hWnd)
        {
            System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
            System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(null, hWnd), ref rect);
            Rectangle workingArea = Screen.GetWorkingArea(Control.MousePosition);
            int x = workingArea.X + (((workingArea.Width - rect.right) + rect.left) / 2);
            int y = workingArea.Y + (((workingArea.Height - rect.bottom) + rect.top) / 3);
            System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(null, hWnd), System.Windows.Forms.NativeMethods.NullHandleRef, x, y, 0, 0, 0x15);
        }

        protected virtual void OnHelpRequest(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventHelpRequest];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected virtual IntPtr OwnerWndProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            if (msg != helpMsg)
            {
                return System.Windows.Forms.UnsafeNativeMethods.CallWindowProc(this.defOwnerWndProc, hWnd, msg, wparam, lparam);
            }
            if (NativeWindow.WndProcShouldBeDebuggable)
            {
                this.OnHelpRequest(EventArgs.Empty);
            }
            else
            {
                try
                {
                    this.OnHelpRequest(EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    Application.OnThreadException(exception);
                }
            }
            return IntPtr.Zero;
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public abstract void Reset();
        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected abstract bool RunDialog(IntPtr hwndOwner);
        public DialogResult ShowDialog()
        {
            return this.ShowDialog(null);
        }

        public DialogResult ShowDialog(IWin32Window owner)
        {
            System.Windows.Forms.IntSecurity.SafeSubWindows.Demand();
            if (!SystemInformation.UserInteractive)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("CantShowModalOnNonInteractive"));
            }
            NativeWindow window = null;
            IntPtr zero = IntPtr.Zero;
            DialogResult cancel = DialogResult.Cancel;
            try
            {
                if (owner != null)
                {
                    zero = Control.GetSafeHandle(owner);
                }
                if (zero == IntPtr.Zero)
                {
                    zero = System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow();
                }
                if (zero == IntPtr.Zero)
                {
                    window = new NativeWindow();
                    window.CreateHandle(new CreateParams());
                    zero = window.Handle;
                }
                if (helpMsg == 0)
                {
                    helpMsg = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage("commdlg_help");
                }
                System.Windows.Forms.NativeMethods.WndProc d = new System.Windows.Forms.NativeMethods.WndProc(this.OwnerWndProc);
                this.hookedWndProc = Marshal.GetFunctionPointerForDelegate(d);
                IntPtr userCookie = IntPtr.Zero;
                try
                {
                    this.defOwnerWndProc = System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, zero), -4, d);
                    if (Application.UseVisualStyles)
                    {
                        userCookie = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                    }
                    Application.BeginModalMessageLoop();
                    try
                    {
                        cancel = this.RunDialog(zero) ? DialogResult.OK : DialogResult.Cancel;
                    }
                    finally
                    {
                        Application.EndModalMessageLoop();
                    }
                    return cancel;
                }
                finally
                {
                    IntPtr windowLong = System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, zero), -4);
                    if ((IntPtr.Zero != this.defOwnerWndProc) || (windowLong != this.hookedWndProc))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, zero), -4, new HandleRef(this, this.defOwnerWndProc));
                    }
                    System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Deactivate(userCookie);
                    this.defOwnerWndProc = IntPtr.Zero;
                    this.hookedWndProc = IntPtr.Zero;
                    GC.KeepAlive(d);
                }
            }
            finally
            {
                if (window != null)
                {
                    window.DestroyHandle();
                }
            }
            return cancel;
        }

        [TypeConverter(typeof(StringConverter)), System.Windows.Forms.SRCategory("CatData"), Localizable(false), Bindable(true), System.Windows.Forms.SRDescription("ControlTagDescr"), DefaultValue((string) null)]
        public object Tag
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.userData = value;
            }
        }
    }
}

