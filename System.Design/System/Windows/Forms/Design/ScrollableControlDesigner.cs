namespace System.Windows.Forms.Design
{
    using System;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    public class ScrollableControlDesigner : ParentControlDesigner
    {
        private SelectionManager selManager;

        protected override bool GetHitTest(Point pt)
        {
            if (base.GetHitTest(pt))
            {
                return true;
            }
            ScrollableControl control = (ScrollableControl) this.Control;
            if (control.IsHandleCreated && control.AutoScroll)
            {
                switch (((int) System.Design.NativeMethods.SendMessage(control.Handle, 0x84, IntPtr.Zero, (IntPtr) System.Design.NativeMethods.Util.MAKELPARAM(pt.X, pt.Y))))
                {
                    case 7:
                    case 6:
                        return true;
                }
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case 0x114:
                case 0x115:
                    if (this.selManager == null)
                    {
                        this.selManager = this.GetService(typeof(SelectionManager)) as SelectionManager;
                    }
                    if (this.selManager != null)
                    {
                        this.selManager.Refresh();
                    }
                    this.Control.Invalidate();
                    this.Control.Update();
                    return;
            }
        }
    }
}

