namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class GridToolTip : Control
    {
        private Control[] controls;
        private bool dontShow;
        private Point lastMouseMove = Point.Empty;
        private int maximumToolTipLength = 0x3e8;
        private System.Windows.Forms.NativeMethods.TOOLINFO_T[] toolInfos;
        private string toolTipText;

        internal GridToolTip(Control[] controls)
        {
            this.controls = controls;
            base.SetStyle(ControlStyles.UserPaint, false);
            this.Font = controls[0].Font;
            this.toolInfos = new System.Windows.Forms.NativeMethods.TOOLINFO_T[controls.Length];
            for (int i = 0; i < controls.Length; i++)
            {
                controls[i].HandleCreated += new EventHandler(this.OnControlCreateHandle);
                controls[i].HandleDestroyed += new EventHandler(this.OnControlDestroyHandle);
                if (controls[i].IsHandleCreated)
                {
                    this.SetupToolTip(controls[i]);
                }
            }
        }

        private System.Windows.Forms.NativeMethods.TOOLINFO_T GetTOOLINFO(Control c)
        {
            int index = Array.IndexOf<Control>(this.controls, c);
            if (this.toolInfos[index] == null)
            {
                this.toolInfos[index] = new System.Windows.Forms.NativeMethods.TOOLINFO_T();
                this.toolInfos[index].cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_T));
                System.Windows.Forms.NativeMethods.TOOLINFO_T toolinfo_t1 = this.toolInfos[index];
                toolinfo_t1.uFlags |= 0x111;
            }
            this.toolInfos[index].lpszText = this.toolTipText;
            this.toolInfos[index].hwnd = c.Handle;
            this.toolInfos[index].uId = c.Handle;
            return this.toolInfos[index];
        }

        private void OnControlCreateHandle(object sender, EventArgs e)
        {
            this.SetupToolTip((Control) sender);
        }

        private void OnControlDestroyHandle(object sender, EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.TTM_DELTOOL, 0, this.GetTOOLINFO((Control) sender));
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            for (int i = 0; i < this.controls.Length; i++)
            {
                if (this.controls[i].IsHandleCreated)
                {
                    this.SetupToolTip(this.controls[i]);
                }
            }
        }

        public void Reset()
        {
            string toolTip = this.ToolTip;
            this.toolTipText = "";
            for (int i = 0; i < this.controls.Length; i++)
            {
                int num1 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.TTM_UPDATETIPTEXT, 0, this.GetTOOLINFO(this.controls[i]));
            }
            this.toolTipText = toolTip;
            base.SendMessage(0x41d, 0, 0);
        }

        private void SetupToolTip(Control c)
        {
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, 0x13);
                int num1 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.TTM_ADDTOOL, 0, this.GetTOOLINFO(c));
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x418, 0, SystemInformation.MaxWindowTrackSize.Width);
            }
        }

        protected override void WndProc(ref Message msg)
        {
            switch (msg.Msg)
            {
                case 0x18:
                    if ((((int) ((long) msg.WParam)) != 0) && this.dontShow)
                    {
                        msg.WParam = IntPtr.Zero;
                    }
                    break;

                case 0x84:
                    msg.Result = (IntPtr) (-1);
                    return;
            }
            base.WndProc(ref msg);
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams @params;
                System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                    dwICC = 8
                };
                System.Windows.Forms.SafeNativeMethods.InitCommonControlsEx(icc);
                return new System.Windows.Forms.CreateParams { Parent = IntPtr.Zero, ClassName = "tooltips_class32", Style = @params.Style | 3, ExStyle = 0, Caption = this.ToolTip };
            }
        }

        public string ToolTip
        {
            get
            {
                return this.toolTipText;
            }
            set
            {
                if (base.IsHandleCreated || !string.IsNullOrEmpty(value))
                {
                    this.Reset();
                }
                if ((value != null) && (value.Length > this.maximumToolTipLength))
                {
                    value = value.Substring(0, this.maximumToolTipLength) + "...";
                }
                this.toolTipText = value;
                if (base.IsHandleCreated)
                {
                    bool visible = base.Visible;
                    if (visible)
                    {
                        base.Visible = false;
                    }
                    if ((value == null) || (value.Length == 0))
                    {
                        this.dontShow = true;
                        value = "";
                    }
                    else
                    {
                        this.dontShow = false;
                    }
                    for (int i = 0; i < this.controls.Length; i++)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.TTM_UPDATETIPTEXT, 0, this.GetTOOLINFO(this.controls[i]));
                    }
                    if (visible && !this.dontShow)
                    {
                        base.Visible = true;
                    }
                }
            }
        }
    }
}

