namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Workflow.Interop;

    internal sealed class WorkflowToolTip : IDisposable
    {
        private NativeToolTip infoTip;
        private string infoTipText = string.Empty;
        private string infoTipTitle = string.Empty;
        private NativeToolTip inplaceTip;
        private Rectangle inplaceTipRectangle;
        private string inplaceTipText = string.Empty;
        private Control parentControl;

        internal WorkflowToolTip(Control parentControl)
        {
            this.parentControl = parentControl;
            this.infoTip = new NativeToolTip(this.parentControl.Handle);
            this.infoTip.SetDelay(3, 0x3e8);
            this.infoTip.SetDelay(1, 0x3e8);
            this.infoTip.SetDelay(2, 0xf4240);
            using (Graphics graphics = this.parentControl.CreateGraphics())
            {
                int tipWidth = Convert.ToInt32((double) (Math.Ceiling((double) graphics.MeasureString(SR.GetString("ToolTipString"), this.parentControl.Font).Width) / 3.0)) * 30;
                this.infoTip.SetMaxTipWidth(tipWidth);
            }
            this.inplaceTip = new NativeToolTip(this.parentControl.Handle);
            this.inplaceTip.SetDelay(3, 50);
            this.inplaceTip.SetDelay(1, 50);
            this.inplaceTip.SetDelay(2, 0xf4240);
            this.parentControl.Layout += new LayoutEventHandler(this.OnParentLayoutChanged);
        }

        private void OnParentLayoutChanged(object sender, LayoutEventArgs e)
        {
            this.infoTip.UpdateToolTipRectangle(this.parentControl.ClientRectangle);
            this.inplaceTip.UpdateToolTipRectangle(this.parentControl.ClientRectangle);
        }

        public void RelayParentNotify(ref Message msg)
        {
            if (((msg.Msg == 0x4e) && (msg.LParam != IntPtr.Zero)) && !this.inplaceTipRectangle.IsEmpty)
            {
                System.Workflow.Interop.NativeMethods.NMHDR nmhdr = Marshal.PtrToStructure(msg.LParam, typeof(System.Workflow.Interop.NativeMethods.NMHDR)) as System.Workflow.Interop.NativeMethods.NMHDR;
                if (((nmhdr != null) && (nmhdr.hwndFrom == this.inplaceTip.Handle)) && (nmhdr.code == System.Workflow.Interop.NativeMethods.TTN_SHOW))
                {
                    Point point = this.parentControl.PointToScreen(new Point(this.inplaceTipRectangle.Left, this.inplaceTipRectangle.Top));
                    System.Workflow.Interop.NativeMethods.SetWindowPos(this.inplaceTip.Handle, IntPtr.Zero, point.X, point.Y, 0, 0, 0x15);
                    msg.Result = new IntPtr(1);
                }
            }
        }

        public void SetText(string text, Rectangle rectangle)
        {
            if (string.IsNullOrEmpty(text))
            {
                this.inplaceTip.Pop();
                this.inplaceTip.Activate(false);
            }
            else
            {
                this.infoTip.Activate(false);
                this.inplaceTip.Activate(true);
            }
            bool flag = this.inplaceTipText != text;
            if (flag | (this.inplaceTipRectangle != rectangle))
            {
                if (System.Workflow.Interop.NativeMethods.IsWindowVisible(this.inplaceTip.Handle))
                {
                    this.inplaceTip.Pop();
                }
                this.inplaceTipText = text;
                this.inplaceTip.UpdateToolTipText(this.inplaceTipText);
                this.inplaceTipRectangle = rectangle;
            }
        }

        public void SetText(string title, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                this.infoTip.Pop();
                this.infoTip.Activate(false);
            }
            else
            {
                this.inplaceTip.Activate(false);
                this.infoTip.Activate(true);
            }
            bool flag = this.infoTipTitle != title;
            if (flag | (this.infoTipText != text))
            {
                if (System.Workflow.Interop.NativeMethods.IsWindowVisible(this.infoTip.Handle))
                {
                    this.infoTip.Pop();
                }
                this.infoTipTitle = title;
                this.infoTip.UpdateTitle(this.infoTipTitle);
                this.infoTipText = text;
                this.infoTip.UpdateToolTipText(this.infoTipText);
            }
        }

        void IDisposable.Dispose()
        {
            if (this.parentControl != null)
            {
                if (this.infoTip != null)
                {
                    ((IDisposable) this.infoTip).Dispose();
                    this.infoTip = null;
                }
                if (this.inplaceTip != null)
                {
                    ((IDisposable) this.inplaceTip).Dispose();
                    this.inplaceTip = null;
                }
                this.parentControl.Layout -= new LayoutEventHandler(this.OnParentLayoutChanged);
                this.parentControl = null;
            }
        }

        private sealed class NativeToolTip : NativeWindow, IDisposable
        {
            private bool activate = true;
            private IntPtr parentHandle;
            private const string ToolTipClass = "tooltips_class32";

            internal NativeToolTip(IntPtr parentHandle)
            {
                this.parentHandle = parentHandle;
                CreateParams cp = new CreateParams {
                    ClassName = "tooltips_class32",
                    Style = -2147483645,
                    ExStyle = 8,
                    Parent = this.parentHandle
                };
                this.CreateHandle(cp);
                if (IntPtr.Zero == base.Handle)
                {
                    throw new NullReferenceException(SR.GetString("Error_CreatingToolTip"));
                }
                System.Workflow.Interop.NativeMethods.TOOLINFO toolInfo = this.GetToolInfo();
                toolInfo.flags = 0x110;
                toolInfo.hwnd = this.parentHandle;
                this.AddTool(toolInfo);
                this.Activate(false);
            }

            public void Activate(bool activateToolTip)
            {
                if (this.activate != activateToolTip)
                {
                    this.activate = activateToolTip;
                    IntPtr wParam = this.activate ? new IntPtr(1) : new IntPtr(0);
                    System.Workflow.Interop.NativeMethods.SendMessage(base.Handle, System.Workflow.Interop.NativeMethods.TTM_ACTIVATE, wParam, IntPtr.Zero);
                }
            }

            private bool AddTool(System.Workflow.Interop.NativeMethods.TOOLINFO toolInfo)
            {
                return (System.Workflow.Interop.NativeMethods.SendMessage(base.Handle, System.Workflow.Interop.NativeMethods.TTM_ADDTOOL, IntPtr.Zero, ref toolInfo) != IntPtr.Zero);
            }

            private void DelTool(System.Workflow.Interop.NativeMethods.TOOLINFO toolInfo)
            {
                System.Workflow.Interop.NativeMethods.SendMessage(base.Handle, System.Workflow.Interop.NativeMethods.TTM_DELTOOL, IntPtr.Zero, ref toolInfo);
            }

            private System.Workflow.Interop.NativeMethods.TOOLINFO GetToolInfo()
            {
                System.Workflow.Interop.NativeMethods.TOOLINFO toolinfo;
                toolinfo = new System.Workflow.Interop.NativeMethods.TOOLINFO {
                    size = Marshal.SizeOf(toolinfo),
                    flags = 0,
                    hwnd = IntPtr.Zero,
                    id = IntPtr.Zero
                };
                toolinfo.rect.left = toolinfo.rect.right = toolinfo.rect.top = toolinfo.rect.bottom = 0;
                toolinfo.hinst = IntPtr.Zero;
                toolinfo.text = new IntPtr(-1);
                toolinfo.lParam = IntPtr.Zero;
                return toolinfo;
            }

            public void Pop()
            {
                System.Workflow.Interop.NativeMethods.SendMessage(base.Handle, System.Workflow.Interop.NativeMethods.TTM_POP, IntPtr.Zero, IntPtr.Zero);
            }

            public void SetDelay(int time, int delay)
            {
                System.Workflow.Interop.NativeMethods.SendMessage(base.Handle, System.Workflow.Interop.NativeMethods.TTM_SETDELAYTIME, new IntPtr(time), new IntPtr(delay));
            }

            public void SetMaxTipWidth(int tipWidth)
            {
                System.Workflow.Interop.NativeMethods.SendMessage(base.Handle, System.Workflow.Interop.NativeMethods.TTM_SETMAXTIPWIDTH, IntPtr.Zero, new IntPtr(tipWidth));
            }

            void IDisposable.Dispose()
            {
                if (this.parentHandle != IntPtr.Zero)
                {
                    System.Workflow.Interop.NativeMethods.TOOLINFO toolInfo = this.GetToolInfo();
                    toolInfo.hwnd = this.parentHandle;
                    this.DelTool(toolInfo);
                    this.DestroyHandle();
                    this.parentHandle = IntPtr.Zero;
                }
            }

            public void UpdateTitle(string title)
            {
                System.Workflow.Interop.NativeMethods.SendMessage(base.Handle, System.Workflow.Interop.NativeMethods.TTM_SETTITLE, new IntPtr(0), Marshal.StringToBSTR(title));
            }

            public void UpdateToolTipRectangle(Rectangle rectangle)
            {
                System.Workflow.Interop.NativeMethods.TOOLINFO toolInfo = this.GetToolInfo();
                toolInfo.hwnd = this.parentHandle;
                toolInfo.rect.left = rectangle.Left;
                toolInfo.rect.top = rectangle.Top;
                toolInfo.rect.right = rectangle.Right;
                toolInfo.rect.bottom = rectangle.Bottom;
                System.Workflow.Interop.NativeMethods.SendMessage(base.Handle, System.Workflow.Interop.NativeMethods.TTM_NEWTOOLRECT, IntPtr.Zero, ref toolInfo);
            }

            public void UpdateToolTipText(string toolTipText)
            {
                System.Workflow.Interop.NativeMethods.TOOLINFO toolInfo = this.GetToolInfo();
                toolInfo.hwnd = this.parentHandle;
                toolInfo.text = Marshal.StringToBSTR(toolTipText);
                System.Workflow.Interop.NativeMethods.SendMessage(base.Handle, System.Workflow.Interop.NativeMethods.TTM_UPDATETIPTEXT, IntPtr.Zero, ref toolInfo);
            }
        }
    }
}

