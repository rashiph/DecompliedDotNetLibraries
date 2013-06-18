namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class MdiControlStrip : MenuStrip
    {
        private ToolStripMenuItem close;
        private MenuStrip mergedMenu;
        private ToolStripMenuItem minimize;
        private ToolStripMenuItem restore;
        private ToolStripMenuItem system;
        private IWin32Window target;

        public MdiControlStrip(IWin32Window target)
        {
            IntPtr systemMenu = System.Windows.Forms.UnsafeNativeMethods.GetSystemMenu(new HandleRef(this, Control.GetSafeHandle(target)), false);
            this.target = target;
            this.minimize = new ControlBoxMenuItem(systemMenu, 0xf020, target);
            this.close = new ControlBoxMenuItem(systemMenu, 0xf060, target);
            this.restore = new ControlBoxMenuItem(systemMenu, 0xf120, target);
            this.system = new SystemMenuItem();
            Control control = target as Control;
            if (control != null)
            {
                control.HandleCreated += new EventHandler(this.OnTargetWindowHandleRecreated);
                control.Disposed += new EventHandler(this.OnTargetWindowDisposed);
            }
            this.Items.AddRange(new ToolStripItem[] { this.minimize, this.restore, this.close, this.system });
            base.SuspendLayout();
            foreach (ToolStripItem item in this.Items)
            {
                item.DisplayStyle = ToolStripItemDisplayStyle.Image;
                item.MergeIndex = 0;
                item.MergeAction = MergeAction.Insert;
                item.Overflow = ToolStripItemOverflow.Never;
                item.Alignment = ToolStripItemAlignment.Right;
                item.Padding = Padding.Empty;
            }
            this.system.Image = this.GetTargetWindowIcon();
            this.system.Alignment = ToolStripItemAlignment.Left;
            this.system.DropDownOpening += new EventHandler(this.OnSystemMenuDropDownOpening);
            this.system.ImageScaling = ToolStripItemImageScaling.None;
            this.system.DoubleClickEnabled = true;
            this.system.DoubleClick += new EventHandler(this.OnSystemMenuDoubleClick);
            this.system.Padding = Padding.Empty;
            this.system.ShortcutKeys = Keys.Alt | Keys.OemMinus;
            base.ResumeLayout(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.UnhookTarget();
                this.target = null;
            }
            base.Dispose(disposing);
        }

        private Image GetTargetWindowIcon()
        {
            Image image = null;
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, Control.GetSafeHandle(this.target)), 0x7f, 0, 0);
            System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
            try
            {
                Icon original = (handle != IntPtr.Zero) ? Icon.FromHandle(handle) : Form.DefaultIcon;
                Icon icon2 = new Icon(original, SystemInformation.SmallIconSize);
                image = icon2.ToBitmap();
                icon2.Dispose();
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return image;
        }

        protected internal override void OnItemAdded(ToolStripItemEventArgs e)
        {
            base.OnItemAdded(e);
        }

        private void OnSystemMenuDoubleClick(object sender, EventArgs e)
        {
            this.Close.PerformClick();
        }

        private void OnSystemMenuDropDownOpening(object sender, EventArgs e)
        {
            if (!this.system.HasDropDownItems && (this.target != null))
            {
                this.system.DropDown = ToolStripDropDownMenu.FromHMenu(System.Windows.Forms.UnsafeNativeMethods.GetSystemMenu(new HandleRef(this, Control.GetSafeHandle(this.target)), false), this.target);
            }
            else if (this.MergedMenu == null)
            {
                this.system.DropDown.Dispose();
            }
        }

        private void OnTargetWindowDisposed(object sender, EventArgs e)
        {
            this.UnhookTarget();
            this.target = null;
        }

        private void OnTargetWindowHandleRecreated(object sender, EventArgs e)
        {
            this.system.SetNativeTargetWindow(this.target);
            this.minimize.SetNativeTargetWindow(this.target);
            this.close.SetNativeTargetWindow(this.target);
            this.restore.SetNativeTargetWindow(this.target);
            IntPtr systemMenu = System.Windows.Forms.UnsafeNativeMethods.GetSystemMenu(new HandleRef(this, Control.GetSafeHandle(this.target)), false);
            this.system.SetNativeTargetMenu(systemMenu);
            this.minimize.SetNativeTargetMenu(systemMenu);
            this.close.SetNativeTargetMenu(systemMenu);
            this.restore.SetNativeTargetMenu(systemMenu);
            if (this.system.HasDropDownItems)
            {
                this.system.DropDown.Items.Clear();
                this.system.DropDown.Dispose();
            }
            this.system.Image = this.GetTargetWindowIcon();
        }

        private void UnhookTarget()
        {
            if (this.target != null)
            {
                Control target = this.target as Control;
                if (target != null)
                {
                    target.HandleCreated -= new EventHandler(this.OnTargetWindowHandleRecreated);
                    target.Disposed -= new EventHandler(this.OnTargetWindowDisposed);
                }
                this.target = null;
            }
        }

        public ToolStripMenuItem Close
        {
            get
            {
                return this.close;
            }
        }

        internal MenuStrip MergedMenu
        {
            get
            {
                return this.mergedMenu;
            }
            set
            {
                this.mergedMenu = value;
            }
        }

        internal class ControlBoxMenuItem : ToolStripMenuItem
        {
            internal ControlBoxMenuItem(IntPtr hMenu, int nativeMenuCommandId, IWin32Window targetWindow) : base(hMenu, nativeMenuCommandId, targetWindow)
            {
            }

            internal override bool CanKeyboardSelect
            {
                get
                {
                    return false;
                }
            }
        }

        internal class SystemMenuItem : ToolStripMenuItem
        {
            protected override void OnOwnerChanged(EventArgs e)
            {
                if (this.HasDropDownItems && base.DropDown.Visible)
                {
                    base.HideDropDown();
                }
                base.OnOwnerChanged(e);
            }

            protected internal override bool ProcessCmdKey(ref Message m, Keys keyData)
            {
                if (base.Visible && (base.ShortcutKeys == keyData))
                {
                    base.ShowDropDown();
                    base.DropDown.SelectNextToolStripItem(null, true);
                    return true;
                }
                return base.ProcessCmdKey(ref m, keyData);
            }
        }
    }
}

