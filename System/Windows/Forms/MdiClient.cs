namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DesignTimeVisible(false), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), ToolboxItem(false)]
    public sealed class MdiClient : Control
    {
        private ArrayList children = new ArrayList();

        public MdiClient()
        {
            base.SetStyle(ControlStyles.Selectable, false);
            this.BackColor = SystemColors.AppWorkspace;
            this.Dock = DockStyle.Fill;
        }

        protected override Control.ControlCollection CreateControlsInstance()
        {
            return new ControlCollection(this);
        }

        public void LayoutMdi(MdiLayout value)
        {
            if (base.Handle != IntPtr.Zero)
            {
                switch (value)
                {
                    case MdiLayout.Cascade:
                        base.SendMessage(0x227, 0, 0);
                        return;

                    case MdiLayout.TileHorizontal:
                        base.SendMessage(550, 1, 0);
                        return;

                    case MdiLayout.TileVertical:
                        base.SendMessage(550, 0, 0);
                        return;

                    case MdiLayout.ArrangeIcons:
                        base.SendMessage(0x228, 0, 0);
                        return;
                }
            }
        }

        private void OnIdle(object sender, EventArgs e)
        {
            Application.Idle -= new EventHandler(this.OnIdle);
            base.OnInvokedSetScrollPosition(sender, e);
        }

        internal override void OnInvokedSetScrollPosition(object sender, EventArgs e)
        {
            Application.Idle += new EventHandler(this.OnIdle);
        }

        protected override void OnResize(EventArgs e)
        {
            ISite site = (this.ParentInternal == null) ? null : this.ParentInternal.Site;
            if (((site != null) && site.DesignMode) && (base.Handle != IntPtr.Zero))
            {
                this.SetWindowRgn();
            }
            base.OnResize(e);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            specified &= ~BoundsSpecified.Location;
            base.ScaleControl(factor, specified);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void ScaleCore(float dx, float dy)
        {
            base.SuspendLayout();
            try
            {
                Rectangle bounds = base.Bounds;
                int x = (int) Math.Round((double) (bounds.X * dx));
                int y = (int) Math.Round((double) (bounds.Y * dy));
                int width = (int) Math.Round((double) (((bounds.X + bounds.Width) * dx) - x));
                int height = (int) Math.Round((double) (((bounds.Y + bounds.Height) * dy) - y));
                base.SetBounds(x, y, width, height, BoundsSpecified.All);
            }
            finally
            {
                base.ResumeLayout();
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            ISite site = (this.ParentInternal == null) ? null : this.ParentInternal.Site;
            if (base.IsHandleCreated && ((site == null) || !site.DesignMode))
            {
                Rectangle bounds = base.Bounds;
                base.SetBoundsCore(x, y, width, height, specified);
                Rectangle rectangle2 = base.Bounds;
                int num = bounds.Height - rectangle2.Height;
                if (num != 0)
                {
                    System.Windows.Forms.NativeMethods.WINDOWPLACEMENT placement = new System.Windows.Forms.NativeMethods.WINDOWPLACEMENT {
                        length = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.WINDOWPLACEMENT))
                    };
                    for (int i = 0; i < base.Controls.Count; i++)
                    {
                        Control control = base.Controls[i];
                        if ((control != null) && (control is Form))
                        {
                            Form wrapper = (Form) control;
                            if (wrapper.CanRecreateHandle() && (wrapper.WindowState == FormWindowState.Minimized))
                            {
                                System.Windows.Forms.UnsafeNativeMethods.GetWindowPlacement(new HandleRef(wrapper, wrapper.Handle), ref placement);
                                placement.ptMinPosition_y -= num;
                                if (placement.ptMinPosition_y == -1)
                                {
                                    if (num < 0)
                                    {
                                        placement.ptMinPosition_y = 0;
                                    }
                                    else
                                    {
                                        placement.ptMinPosition_y = -2;
                                    }
                                }
                                placement.flags = 1;
                                System.Windows.Forms.UnsafeNativeMethods.SetWindowPlacement(new HandleRef(wrapper, wrapper.Handle), ref placement);
                                placement.flags = 0;
                            }
                        }
                    }
                }
            }
            else
            {
                base.SetBoundsCore(x, y, width, height, specified);
            }
        }

        private void SetWindowRgn()
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr handle = IntPtr.Zero;
            System.Windows.Forms.NativeMethods.RECT lpRect = new System.Windows.Forms.NativeMethods.RECT();
            System.Windows.Forms.CreateParams createParams = this.CreateParams;
            System.Windows.Forms.SafeNativeMethods.AdjustWindowRectEx(ref lpRect, createParams.Style, false, createParams.ExStyle);
            Rectangle bounds = base.Bounds;
            zero = System.Windows.Forms.SafeNativeMethods.CreateRectRgn(0, 0, bounds.Width, bounds.Height);
            try
            {
                handle = System.Windows.Forms.SafeNativeMethods.CreateRectRgn(-lpRect.left, -lpRect.top, bounds.Width - lpRect.right, bounds.Height - lpRect.bottom);
                try
                {
                    if ((zero == IntPtr.Zero) || (handle == IntPtr.Zero))
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ErrorSettingWindowRegion"));
                    }
                    if (System.Windows.Forms.SafeNativeMethods.CombineRgn(new HandleRef(null, zero), new HandleRef(null, zero), new HandleRef(null, handle), 4) == 0)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ErrorSettingWindowRegion"));
                    }
                    if (System.Windows.Forms.UnsafeNativeMethods.SetWindowRgn(new HandleRef(this, base.Handle), new HandleRef(null, zero), true) == 0)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ErrorSettingWindowRegion"));
                    }
                    zero = IntPtr.Zero;
                }
                finally
                {
                    if (handle != IntPtr.Zero)
                    {
                        System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, handle));
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, zero));
                }
            }
        }

        internal override bool ShouldSerializeBackColor()
        {
            return (this.BackColor != SystemColors.AppWorkspace);
        }

        private bool ShouldSerializeLocation()
        {
            return false;
        }

        internal override bool ShouldSerializeSize()
        {
            return false;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 7:
                {
                    base.InvokeGotFocus(this.ParentInternal, EventArgs.Empty);
                    Form activeMdiChildInternal = null;
                    if (this.ParentInternal is Form)
                    {
                        activeMdiChildInternal = ((Form) this.ParentInternal).ActiveMdiChildInternal;
                    }
                    if (((activeMdiChildInternal == null) && (this.MdiChildren.Length > 0)) && this.MdiChildren[0].IsMdiChildFocusable)
                    {
                        activeMdiChildInternal = this.MdiChildren[0];
                    }
                    if ((activeMdiChildInternal != null) && activeMdiChildInternal.Visible)
                    {
                        activeMdiChildInternal.Active = true;
                    }
                    base.WmImeSetFocus();
                    this.DefWndProc(ref m);
                    base.InvokeGotFocus(this, EventArgs.Empty);
                    return;
                }
                case 8:
                    base.InvokeLostFocus(this.ParentInternal, EventArgs.Empty);
                    break;

                case 1:
                    if (((this.ParentInternal != null) && (this.ParentInternal.Site != null)) && (this.ParentInternal.Site.DesignMode && (base.Handle != IntPtr.Zero)))
                    {
                        this.SetWindowRgn();
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        [Localizable(true)]
        public override Image BackgroundImage
        {
            get
            {
                Image backgroundImage = base.BackgroundImage;
                if ((backgroundImage == null) && (this.ParentInternal != null))
                {
                    backgroundImage = this.ParentInternal.BackgroundImage;
                }
                return backgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                if (((this.BackgroundImage != null) && (this.ParentInternal != null)) && (base.BackgroundImageLayout != this.ParentInternal.BackgroundImageLayout))
                {
                    return this.ParentInternal.BackgroundImageLayout;
                }
                return base.BackgroundImageLayout;
            }
            set
            {
                base.BackgroundImageLayout = value;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "MDICLIENT";
                createParams.Style |= 0x300000;
                createParams.ExStyle |= 0x200;
                createParams.Param = new System.Windows.Forms.NativeMethods.CLIENTCREATESTRUCT(IntPtr.Zero, 1);
                ISite site = (this.ParentInternal == null) ? null : this.ParentInternal.Site;
                if ((site != null) && site.DesignMode)
                {
                    createParams.Style |= 0x8000000;
                    base.SetState(4, false);
                }
                if (((this.RightToLeft == RightToLeft.Yes) && (this.ParentInternal != null)) && this.ParentInternal.IsMirrored)
                {
                    createParams.ExStyle |= 0x500000;
                    createParams.ExStyle &= -28673;
                }
                return createParams;
            }
        }

        public Form[] MdiChildren
        {
            get
            {
                Form[] array = new Form[this.children.Count];
                this.children.CopyTo(array, 0);
                return array;
            }
        }

        [ComVisible(false)]
        public class ControlCollection : Control.ControlCollection
        {
            private MdiClient owner;

            public ControlCollection(MdiClient owner) : base(owner)
            {
                this.owner = owner;
            }

            public override void Add(Control value)
            {
                if (value != null)
                {
                    if (!(value is Form) || !((Form) value).IsMdiChild)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("MDIChildAddToNonMDIParent"), "value");
                    }
                    if (this.owner.CreateThreadId != value.CreateThreadId)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("AddDifferentThreads"), "value");
                    }
                    this.owner.children.Add((Form) value);
                    base.Add(value);
                }
            }

            public override void Remove(Control value)
            {
                this.owner.children.Remove(value);
                base.Remove(value);
            }
        }
    }
}

