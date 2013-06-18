namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    internal class DataGridToolTip : MarshalByRefObject
    {
        private DataGrid dataGrid;
        private NativeWindow tipWindow;

        public DataGridToolTip(DataGrid dataGrid)
        {
            this.dataGrid = dataGrid;
        }

        public void AddToolTip(string toolTipString, IntPtr toolTipId, Rectangle iconBounds)
        {
            System.Windows.Forms.NativeMethods.TOOLINFO_T toolinfo_t;
            if (toolTipString == null)
            {
                throw new ArgumentNullException("toolTipString");
            }
            if (iconBounds.IsEmpty)
            {
                throw new ArgumentNullException("iconBounds", System.Windows.Forms.SR.GetString("DataGridToolTipEmptyIcon"));
            }
            toolinfo_t = new System.Windows.Forms.NativeMethods.TOOLINFO_T {
                cbSize = Marshal.SizeOf(toolinfo_t),
                hwnd = this.dataGrid.Handle,
                uId = toolTipId,
                lpszText = toolTipString,
                rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(iconBounds.X, iconBounds.Y, iconBounds.Width, iconBounds.Height),
                uFlags = 0x10
            };
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.tipWindow, this.tipWindow.Handle), System.Windows.Forms.NativeMethods.TTM_ADDTOOL, 0, toolinfo_t);
        }

        public void CreateToolTipHandle()
        {
            if ((this.tipWindow == null) || (this.tipWindow.Handle == IntPtr.Zero))
            {
                System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX initcommoncontrolsex;
                initcommoncontrolsex = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                    dwICC = 8,
                    dwSize = Marshal.SizeOf(initcommoncontrolsex)
                };
                System.Windows.Forms.SafeNativeMethods.InitCommonControlsEx(initcommoncontrolsex);
                CreateParams cp = new CreateParams {
                    Parent = this.dataGrid.Handle,
                    ClassName = "tooltips_class32",
                    Style = 1
                };
                this.tipWindow = new NativeWindow();
                this.tipWindow.CreateHandle(cp);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.tipWindow, this.tipWindow.Handle), 0x418, 0, SystemInformation.MaxWindowTrackSize.Width);
                System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this.tipWindow, this.tipWindow.Handle), System.Windows.Forms.NativeMethods.HWND_NOTOPMOST, 0, 0, 0, 0, 0x13);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.tipWindow, this.tipWindow.Handle), 0x403, 3, 0);
            }
        }

        public void Destroy()
        {
            this.tipWindow.DestroyHandle();
            this.tipWindow = null;
        }

        public void RemoveToolTip(IntPtr toolTipId)
        {
            System.Windows.Forms.NativeMethods.TOOLINFO_T toolinfo_t;
            toolinfo_t = new System.Windows.Forms.NativeMethods.TOOLINFO_T {
                cbSize = Marshal.SizeOf(toolinfo_t),
                hwnd = this.dataGrid.Handle,
                uId = toolTipId
            };
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.tipWindow, this.tipWindow.Handle), System.Windows.Forms.NativeMethods.TTM_DELTOOL, 0, toolinfo_t);
        }
    }
}

