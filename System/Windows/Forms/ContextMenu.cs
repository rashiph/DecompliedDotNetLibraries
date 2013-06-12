namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DefaultEvent("Popup")]
    public class ContextMenu : Menu
    {
        private System.Windows.Forms.RightToLeft rightToLeft;
        internal Control sourceControl;

        [System.Windows.Forms.SRDescription("ContextMenuCollapseDescr")]
        public event EventHandler Collapse;

        [System.Windows.Forms.SRDescription("MenuItemOnInitDescr")]
        public event EventHandler Popup;

        public ContextMenu() : base(null)
        {
            this.rightToLeft = System.Windows.Forms.RightToLeft.Inherit;
        }

        public ContextMenu(MenuItem[] menuItems) : base(menuItems)
        {
            this.rightToLeft = System.Windows.Forms.RightToLeft.Inherit;
        }

        protected internal virtual void OnCollapse(EventArgs e)
        {
            if (this.onCollapse != null)
            {
                this.onCollapse(this, e);
            }
        }

        protected internal virtual void OnPopup(EventArgs e)
        {
            if (this.onPopup != null)
            {
                this.onPopup(this, e);
            }
        }

        [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected internal virtual bool ProcessCmdKey(ref Message msg, Keys keyData, Control control)
        {
            this.sourceControl = control;
            return this.ProcessCmdKey(ref msg, keyData);
        }

        private void ResetRightToLeft()
        {
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
        }

        internal virtual bool ShouldSerializeRightToLeft()
        {
            if (System.Windows.Forms.RightToLeft.Inherit == this.rightToLeft)
            {
                return false;
            }
            return true;
        }

        public void Show(Control control, Point pos)
        {
            this.Show(control, pos, 0x42);
        }

        private void Show(Control control, Point pos, int flags)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (!control.IsHandleCreated || !control.Visible)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("ContextMenuInvalidParent"), "control");
            }
            this.sourceControl = control;
            this.OnPopup(EventArgs.Empty);
            pos = control.PointToScreen(pos);
            System.Windows.Forms.SafeNativeMethods.TrackPopupMenuEx(new HandleRef(this, base.Handle), flags, pos.X, pos.Y, new HandleRef(control, control.Handle), null);
        }

        public void Show(Control control, Point pos, LeftRightAlignment alignment)
        {
            if (alignment == LeftRightAlignment.Left)
            {
                this.Show(control, pos, 0x4a);
            }
            else
            {
                this.Show(control, pos, 0x42);
            }
        }

        internal override bool RenderIsRightToLeft
        {
            get
            {
                return (this.rightToLeft == System.Windows.Forms.RightToLeft.Yes);
            }
        }

        [System.Windows.Forms.SRDescription("MenuRightToLeftDescr"), DefaultValue(0), Localizable(true)]
        public virtual System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                if (System.Windows.Forms.RightToLeft.Inherit != this.rightToLeft)
                {
                    return this.rightToLeft;
                }
                if (this.sourceControl != null)
                {
                    return this.sourceControl.RightToLeft;
                }
                return System.Windows.Forms.RightToLeft.No;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("RightToLeft", (int) value, typeof(System.Windows.Forms.RightToLeft));
                }
                if (this.RightToLeft != value)
                {
                    this.rightToLeft = value;
                    base.UpdateRtl(value == System.Windows.Forms.RightToLeft.Yes);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ContextMenuSourceControlDescr")]
        public Control SourceControl
        {
            [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
            get
            {
                return this.sourceControl;
            }
        }
    }
}

