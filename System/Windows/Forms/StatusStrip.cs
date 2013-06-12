namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [ComVisible(true), System.Windows.Forms.SRDescription("DescriptionStatusStrip"), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class StatusStrip : ToolStrip
    {
        private const AnchorStyles AllAnchor = (AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top);
        private const int gripWidth = 12;
        private const AnchorStyles HorizontalAnchor = (AnchorStyles.Right | AnchorStyles.Left);
        private Orientation lastOrientation;
        private RightToLeftLayoutGrip rtlLayoutGrip;
        private BitVector32 state = new BitVector32();
        private static readonly int stateCalledSpringTableLayout = BitVector32.CreateMask(stateSizingGrip);
        private static readonly int stateSizingGrip = BitVector32.CreateMask();
        private const AnchorStyles VerticalAnchor = (AnchorStyles.Bottom | AnchorStyles.Top);

        [Browsable(false)]
        public event EventHandler PaddingChanged
        {
            add
            {
                base.PaddingChanged += value;
            }
            remove
            {
                base.PaddingChanged -= value;
            }
        }

        public StatusStrip()
        {
            base.SuspendLayout();
            this.CanOverflow = false;
            this.LayoutStyle = ToolStripLayoutStyle.Table;
            base.RenderMode = ToolStripRenderMode.System;
            this.GripStyle = ToolStripGripStyle.Hidden;
            base.SetStyle(ControlStyles.ResizeRedraw, true);
            this.Stretch = true;
            this.state[stateSizingGrip] = true;
            base.ResumeLayout(true);
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new StatusStripAccessibleObject(this);
        }

        protected internal override ToolStripItem CreateDefaultItem(string text, Image image, EventHandler onClick)
        {
            return new ToolStripStatusLabel(text, image, onClick);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.rtlLayoutGrip != null))
            {
                this.rtlLayoutGrip.Dispose();
                this.rtlLayoutGrip = null;
            }
            base.Dispose(disposing);
        }

        private void EnsureRightToLeftGrip()
        {
            if (this.SizingGrip && (this.RightToLeft == RightToLeft.Yes))
            {
                this.RTLGrip.Bounds = this.SizeGripBounds;
                if (!base.Controls.Contains(this.RTLGrip))
                {
                    WindowsFormsUtils.ReadOnlyControlCollection controls = base.Controls as WindowsFormsUtils.ReadOnlyControlCollection;
                    if (controls != null)
                    {
                        controls.AddInternal(this.RTLGrip);
                    }
                }
            }
            else if ((this.rtlLayoutGrip != null) && base.Controls.Contains(this.rtlLayoutGrip))
            {
                WindowsFormsUtils.ReadOnlyControlCollection controls2 = base.Controls as WindowsFormsUtils.ReadOnlyControlCollection;
                if (controls2 != null)
                {
                    controls2.RemoveInternal(this.rtlLayoutGrip);
                }
                this.rtlLayoutGrip.Dispose();
                this.rtlLayoutGrip = null;
            }
        }

        internal override Size GetPreferredSizeCore(Size proposedSize)
        {
            if (this.LayoutStyle != ToolStripLayoutStyle.Table)
            {
                return base.GetPreferredSizeCore(proposedSize);
            }
            if (proposedSize.Width == 1)
            {
                proposedSize.Width = 0x7fffffff;
            }
            if (proposedSize.Height == 1)
            {
                proposedSize.Height = 0x7fffffff;
            }
            if (base.Orientation == Orientation.Horizontal)
            {
                return (ToolStrip.GetPreferredSizeHorizontal(this, proposedSize) + this.Padding.Size);
            }
            return (ToolStrip.GetPreferredSizeVertical(this, proposedSize) + this.Padding.Size);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.state[stateCalledSpringTableLayout] = false;
            bool flag = false;
            ToolStripItem affectedComponent = levent.AffectedComponent as ToolStripItem;
            int count = this.DisplayedItems.Count;
            if (affectedComponent != null)
            {
                flag = this.DisplayedItems.Contains(affectedComponent);
            }
            if (this.LayoutStyle == ToolStripLayoutStyle.Table)
            {
                this.OnSpringTableLayoutCore();
            }
            base.OnLayout(levent);
            if (((count != this.DisplayedItems.Count) || ((affectedComponent != null) && (flag != this.DisplayedItems.Contains(affectedComponent)))) && (this.LayoutStyle == ToolStripLayoutStyle.Table))
            {
                this.OnSpringTableLayoutCore();
                base.OnLayout(levent);
            }
            this.EnsureRightToLeftGrip();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (this.ShowSizingGrip)
            {
                base.Renderer.DrawStatusStripSizingGrip(new ToolStripRenderEventArgs(e.Graphics, this));
            }
        }

        protected virtual void OnSpringTableLayoutCore()
        {
            if (this.LayoutStyle == ToolStripLayoutStyle.Table)
            {
                this.state[stateCalledSpringTableLayout] = true;
                base.SuspendLayout();
                if (this.lastOrientation != base.Orientation)
                {
                    System.Windows.Forms.TableLayoutSettings tableLayoutSettings = this.TableLayoutSettings;
                    tableLayoutSettings.RowCount = 0;
                    tableLayoutSettings.ColumnCount = 0;
                    tableLayoutSettings.ColumnStyles.Clear();
                    tableLayoutSettings.RowStyles.Clear();
                }
                this.lastOrientation = base.Orientation;
                if (base.Orientation == Orientation.Horizontal)
                {
                    this.TableLayoutSettings.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
                    int count = this.TableLayoutSettings.ColumnStyles.Count;
                    for (int i = 0; i < this.DisplayedItems.Count; i++)
                    {
                        if (i >= count)
                        {
                            this.TableLayoutSettings.ColumnStyles.Add(new ColumnStyle());
                        }
                        ToolStripStatusLabel label = this.DisplayedItems[i] as ToolStripStatusLabel;
                        bool flag = (label != null) && label.Spring;
                        this.DisplayedItems[i].Anchor = flag ? (AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top) : (AnchorStyles.Bottom | AnchorStyles.Top);
                        ColumnStyle style = this.TableLayoutSettings.ColumnStyles[i];
                        style.Width = 100f;
                        style.SizeType = flag ? SizeType.Percent : SizeType.AutoSize;
                    }
                    if ((this.TableLayoutSettings.RowStyles.Count > 1) || (this.TableLayoutSettings.RowStyles.Count == 0))
                    {
                        this.TableLayoutSettings.RowStyles.Clear();
                        this.TableLayoutSettings.RowStyles.Add(new RowStyle());
                    }
                    this.TableLayoutSettings.RowCount = 1;
                    this.TableLayoutSettings.RowStyles[0].SizeType = SizeType.Absolute;
                    this.TableLayoutSettings.RowStyles[0].Height = Math.Max(0, this.DisplayRectangle.Height);
                    this.TableLayoutSettings.ColumnCount = this.DisplayedItems.Count + 1;
                    for (int j = this.DisplayedItems.Count; j < this.TableLayoutSettings.ColumnStyles.Count; j++)
                    {
                        this.TableLayoutSettings.ColumnStyles[j].SizeType = SizeType.AutoSize;
                    }
                }
                else
                {
                    this.TableLayoutSettings.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
                    int num4 = this.TableLayoutSettings.RowStyles.Count;
                    for (int k = 0; k < this.DisplayedItems.Count; k++)
                    {
                        if (k >= num4)
                        {
                            this.TableLayoutSettings.RowStyles.Add(new RowStyle());
                        }
                        ToolStripStatusLabel label2 = this.DisplayedItems[k] as ToolStripStatusLabel;
                        bool flag2 = (label2 != null) && label2.Spring;
                        this.DisplayedItems[k].Anchor = flag2 ? (AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top) : (AnchorStyles.Right | AnchorStyles.Left);
                        RowStyle style2 = this.TableLayoutSettings.RowStyles[k];
                        style2.Height = 100f;
                        style2.SizeType = flag2 ? SizeType.Percent : SizeType.AutoSize;
                    }
                    this.TableLayoutSettings.ColumnCount = 1;
                    if ((this.TableLayoutSettings.ColumnStyles.Count > 1) || (this.TableLayoutSettings.ColumnStyles.Count == 0))
                    {
                        this.TableLayoutSettings.ColumnStyles.Clear();
                        this.TableLayoutSettings.ColumnStyles.Add(new ColumnStyle());
                    }
                    this.TableLayoutSettings.ColumnCount = 1;
                    this.TableLayoutSettings.ColumnStyles[0].SizeType = SizeType.Absolute;
                    this.TableLayoutSettings.ColumnStyles[0].Width = Math.Max(0, this.DisplayRectangle.Width);
                    this.TableLayoutSettings.RowCount = this.DisplayedItems.Count + 1;
                    for (int m = this.DisplayedItems.Count; m < this.TableLayoutSettings.RowStyles.Count; m++)
                    {
                        this.TableLayoutSettings.RowStyles[m].SizeType = SizeType.AutoSize;
                    }
                }
                base.ResumeLayout(false);
            }
        }

        internal override void ResetRenderMode()
        {
            base.RenderMode = ToolStripRenderMode.System;
        }

        protected override void SetDisplayedItems()
        {
            if (this.state[stateCalledSpringTableLayout])
            {
                if (base.Orientation == Orientation.Horizontal)
                {
                    RightToLeft rightToLeft = this.RightToLeft;
                }
                Point location = this.DisplayRectangle.Location;
                location.X += base.ClientSize.Width + 1;
                location.Y += base.ClientSize.Height + 1;
                bool flag = false;
                Rectangle empty = Rectangle.Empty;
                ToolStripItem item = null;
                for (int i = 0; i < this.Items.Count; i++)
                {
                    ToolStripItem item2 = this.Items[i];
                    if (flag || ((IArrangedElement) item2).ParticipatesInLayout)
                    {
                        if (flag || (this.SizingGrip && item2.Bounds.IntersectsWith(this.SizeGripBounds)))
                        {
                            base.SetItemLocation(item2, location);
                            item2.SetPlacement(ToolStripItemPlacement.None);
                        }
                    }
                    else if ((item != null) && empty.IntersectsWith(item2.Bounds))
                    {
                        base.SetItemLocation(item2, location);
                        item2.SetPlacement(ToolStripItemPlacement.None);
                    }
                    else if (item2.Bounds.Width == 1)
                    {
                        ToolStripStatusLabel label = item2 as ToolStripStatusLabel;
                        if ((label != null) && label.Spring)
                        {
                            base.SetItemLocation(item2, location);
                            item2.SetPlacement(ToolStripItemPlacement.None);
                        }
                    }
                    if (item2.Bounds.Location != location)
                    {
                        item = item2;
                        empty = item.Bounds;
                    }
                    else if (((IArrangedElement) item2).ParticipatesInLayout)
                    {
                        flag = true;
                    }
                }
            }
            base.SetDisplayedItems();
        }

        internal override bool ShouldSerializeRenderMode()
        {
            return ((base.RenderMode != ToolStripRenderMode.System) && (base.RenderMode != ToolStripRenderMode.Custom));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == 0x84) && this.SizingGrip)
            {
                Rectangle sizeGripBounds = this.SizeGripBounds;
                int x = System.Windows.Forms.NativeMethods.Util.LOWORD(m.LParam);
                int y = System.Windows.Forms.NativeMethods.Util.HIWORD(m.LParam);
                if (sizeGripBounds.Contains(base.PointToClient(new Point(x, y))))
                {
                    HandleRef rootHWnd = WindowsFormsUtils.GetRootHWnd(this);
                    if ((rootHWnd.Handle != IntPtr.Zero) && !System.Windows.Forms.UnsafeNativeMethods.IsZoomed(rootHWnd))
                    {
                        System.Windows.Forms.NativeMethods.POINT point;
                        System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                        System.Windows.Forms.UnsafeNativeMethods.GetClientRect(rootHWnd, ref rect);
                        if (this.RightToLeft == RightToLeft.Yes)
                        {
                            point = new System.Windows.Forms.NativeMethods.POINT(this.SizeGripBounds.Left, this.SizeGripBounds.Bottom);
                        }
                        else
                        {
                            point = new System.Windows.Forms.NativeMethods.POINT(this.SizeGripBounds.Right, this.SizeGripBounds.Bottom);
                        }
                        System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(new HandleRef(this, base.Handle), rootHWnd, point, 1);
                        int num3 = Math.Abs((int) (rect.bottom - point.y));
                        int num4 = Math.Abs((int) (rect.right - point.x));
                        if ((this.RightToLeft != RightToLeft.Yes) && ((num4 + num3) < 2))
                        {
                            m.Result = (IntPtr) 0x11;
                            return;
                        }
                    }
                }
            }
            base.WndProc(ref m);
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("ToolStripCanOverflowDescr"), System.Windows.Forms.SRCategory("CatLayout"), Browsable(false)]
        public bool CanOverflow
        {
            get
            {
                return base.CanOverflow;
            }
            set
            {
                base.CanOverflow = value;
            }
        }

        protected override DockStyle DefaultDock
        {
            get
            {
                return DockStyle.Bottom;
            }
        }

        protected override System.Windows.Forms.Padding DefaultPadding
        {
            get
            {
                if (base.Orientation != Orientation.Horizontal)
                {
                    return new System.Windows.Forms.Padding(1, 3, 1, this.DefaultSize.Height);
                }
                if (this.RightToLeft == RightToLeft.No)
                {
                    return new System.Windows.Forms.Padding(1, 0, 14, 0);
                }
                return new System.Windows.Forms.Padding(14, 0, 1, 0);
            }
        }

        protected override bool DefaultShowItemToolTips
        {
            get
            {
                return false;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(200, 0x16);
            }
        }

        [DefaultValue(2)]
        public override DockStyle Dock
        {
            get
            {
                return base.Dock;
            }
            set
            {
                base.Dock = value;
            }
        }

        [DefaultValue(0)]
        public ToolStripGripStyle GripStyle
        {
            get
            {
                return base.GripStyle;
            }
            set
            {
                base.GripStyle = value;
            }
        }

        [DefaultValue(4)]
        public ToolStripLayoutStyle LayoutStyle
        {
            get
            {
                return base.LayoutStyle;
            }
            set
            {
                base.LayoutStyle = value;
            }
        }

        [Browsable(false)]
        public System.Windows.Forms.Padding Padding
        {
            get
            {
                return base.Padding;
            }
            set
            {
                base.Padding = value;
            }
        }

        private Control RTLGrip
        {
            get
            {
                if (this.rtlLayoutGrip == null)
                {
                    this.rtlLayoutGrip = new RightToLeftLayoutGrip();
                }
                return this.rtlLayoutGrip;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripShowItemToolTipsDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool ShowItemToolTips
        {
            get
            {
                return base.ShowItemToolTips;
            }
            set
            {
                base.ShowItemToolTips = value;
            }
        }

        private bool ShowSizingGrip
        {
            get
            {
                if (this.SizingGrip && base.IsHandleCreated)
                {
                    if (base.DesignMode)
                    {
                        return true;
                    }
                    HandleRef rootHWnd = WindowsFormsUtils.GetRootHWnd(this);
                    if (rootHWnd.Handle != IntPtr.Zero)
                    {
                        return !System.Windows.Forms.UnsafeNativeMethods.IsZoomed(rootHWnd);
                    }
                }
                return false;
            }
        }

        [Browsable(false)]
        public Rectangle SizeGripBounds
        {
            get
            {
                if (!this.SizingGrip)
                {
                    return Rectangle.Empty;
                }
                Size size = base.Size;
                int height = Math.Min(this.DefaultSize.Height, size.Height);
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    return new Rectangle(0, size.Height - height, 12, height);
                }
                return new Rectangle(size.Width - 12, size.Height - height, 12, height);
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("StatusStripSizingGripDescr")]
        public bool SizingGrip
        {
            get
            {
                return this.state[stateSizingGrip];
            }
            set
            {
                if (value != this.state[stateSizingGrip])
                {
                    this.state[stateSizingGrip] = value;
                    this.EnsureRightToLeftGrip();
                    base.Invalidate(true);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ToolStripStretchDescr"), DefaultValue(true)]
        public bool Stretch
        {
            get
            {
                return base.Stretch;
            }
            set
            {
                base.Stretch = value;
            }
        }

        private System.Windows.Forms.TableLayoutSettings TableLayoutSettings
        {
            get
            {
                return (base.LayoutSettings as System.Windows.Forms.TableLayoutSettings);
            }
        }

        private class RightToLeftLayoutGrip : Control
        {
            public RightToLeftLayoutGrip()
            {
                base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.BackColor = Color.Transparent;
            }

            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x84)
                {
                    int x = System.Windows.Forms.NativeMethods.Util.LOWORD(m.LParam);
                    int y = System.Windows.Forms.NativeMethods.Util.HIWORD(m.LParam);
                    if (base.ClientRectangle.Contains(base.PointToClient(new Point(x, y))))
                    {
                        m.Result = (IntPtr) 0x10;
                        return;
                    }
                }
                base.WndProc(ref m);
            }

            protected override System.Windows.Forms.CreateParams CreateParams
            {
                [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    System.Windows.Forms.CreateParams createParams = base.CreateParams;
                    createParams.ExStyle |= 0x400000;
                    return createParams;
                }
            }
        }

        [ComVisible(true)]
        internal class StatusStripAccessibleObject : ToolStrip.ToolStripAccessibleObject
        {
            public StatusStripAccessibleObject(StatusStrip owner) : base(owner)
            {
            }

            public override AccessibleRole Role
            {
                get
                {
                    AccessibleRole accessibleRole = base.Owner.AccessibleRole;
                    if (accessibleRole != AccessibleRole.Default)
                    {
                        return accessibleRole;
                    }
                    return AccessibleRole.StatusBar;
                }
            }
        }
    }
}

