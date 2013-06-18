namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.VisualStyles;

    [Designer("System.Windows.Forms.Design.StatusBarDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultEvent("PanelClick"), DefaultProperty("Text")]
    public class StatusBar : Control
    {
        private static readonly object EVENT_PANELCLICK = new object();
        private static readonly object EVENT_SBDRAWITEM = new object();
        private Point lastClick = new Point(0, 0);
        private bool layoutDirty;
        private System.Windows.Forms.ToolTip mainToolTip;
        private IList panels = new ArrayList();
        private StatusBarPanelCollection panelsCollection;
        private int panelsRealized;
        private static System.Windows.Forms.VisualStyles.VisualStyleRenderer renderer = null;
        private bool showPanels;
        private const int SIMPLE_INDEX = 0xff;
        private string simpleText;
        private bool sizeGrip = true;
        private int sizeGripWidth;
        private ControlToolTip tooltips;
        private bool toolTipSet;

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackColorChanged
        {
            add
            {
                base.BackColorChanged += value;
            }
            remove
            {
                base.BackColorChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackgroundImageChanged
        {
            add
            {
                base.BackgroundImageChanged += value;
            }
            remove
            {
                base.BackgroundImageChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                base.BackgroundImageLayoutChanged += value;
            }
            remove
            {
                base.BackgroundImageLayoutChanged -= value;
            }
        }

        [System.Windows.Forms.SRDescription("StatusBarDrawItem"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event StatusBarDrawItemEventHandler DrawItem
        {
            add
            {
                base.Events.AddHandler(EVENT_SBDRAWITEM, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SBDRAWITEM, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler ForeColorChanged
        {
            add
            {
                base.ForeColorChanged += value;
            }
            remove
            {
                base.ForeColorChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler ImeModeChanged
        {
            add
            {
                base.ImeModeChanged += value;
            }
            remove
            {
                base.ImeModeChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event PaintEventHandler Paint
        {
            add
            {
                base.Paint += value;
            }
            remove
            {
                base.Paint -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatMouse"), System.Windows.Forms.SRDescription("StatusBarOnPanelClickDescr")]
        public event StatusBarPanelClickEventHandler PanelClick
        {
            add
            {
                base.Events.AddHandler(EVENT_PANELCLICK, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_PANELCLICK, value);
            }
        }

        public StatusBar()
        {
            base.SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint, false);
            this.Dock = DockStyle.Bottom;
            this.TabStop = false;
        }

        private void ApplyPanelWidths()
        {
            if (base.IsHandleCreated)
            {
                StatusBarPanel panel = null;
                int count = this.panels.Count;
                if (count == 0)
                {
                    Size size = base.Size;
                    int[] lParam = new int[] { size.Width };
                    if (this.sizeGrip)
                    {
                        lParam[0] -= this.SizeGripWidth;
                    }
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x404, 1, lParam);
                    base.SendMessage(0x40f, 0, IntPtr.Zero);
                }
                else
                {
                    int[] numArray2 = new int[count];
                    int num2 = 0;
                    for (int i = 0; i < count; i++)
                    {
                        panel = (StatusBarPanel) this.panels[i];
                        numArray2[i] = num2 + panel.Width;
                        panel.Right = numArray2[i];
                    }
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x404, count, numArray2);
                    for (int j = 0; j < count; j++)
                    {
                        panel = (StatusBarPanel) this.panels[j];
                        this.UpdateTooltip(panel);
                    }
                    this.layoutDirty = false;
                }
            }
        }

        internal bool ArePanelsRealized()
        {
            return (this.showPanels && base.IsHandleCreated);
        }

        protected override void CreateHandle()
        {
            if (!base.RecreatingHandle)
            {
                IntPtr userCookie = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                try
                {
                    System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                        dwICC = 4
                    };
                    System.Windows.Forms.SafeNativeMethods.InitCommonControlsEx(icc);
                }
                finally
                {
                    System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Deactivate(userCookie);
                }
            }
            base.CreateHandle();
        }

        internal void DirtyLayout()
        {
            this.layoutDirty = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.panelsCollection != null))
            {
                StatusBarPanel[] array = new StatusBarPanel[this.panelsCollection.Count];
                ((ICollection) this.panelsCollection).CopyTo(array, 0);
                this.panelsCollection.Clear();
                foreach (StatusBarPanel panel in array)
                {
                    panel.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void ForcePanelUpdate()
        {
            if (this.ArePanelsRealized())
            {
                this.layoutDirty = true;
                this.SetPanelContentsWidths(true);
                base.PerformLayout();
                this.RealizePanels();
            }
        }

        private void LayoutPanels()
        {
            StatusBarPanel panel = null;
            int num = 0;
            int index = 0;
            StatusBarPanel[] panelArray = new StatusBarPanel[this.panels.Count];
            bool flag = false;
            for (int i = 0; i < panelArray.Length; i++)
            {
                panel = (StatusBarPanel) this.panels[i];
                if (panel.AutoSize == StatusBarPanelAutoSize.Spring)
                {
                    panelArray[index] = panel;
                    index++;
                }
                else
                {
                    num += panel.Width;
                }
            }
            if (index > 0)
            {
                Rectangle bounds = base.Bounds;
                int num4 = index;
                int num5 = bounds.Width - num;
                if (this.sizeGrip)
                {
                    num5 -= this.SizeGripWidth;
                }
                int num6 = -2147483648;
                while (num4 > 0)
                {
                    int num7 = num5 / num4;
                    if (num5 == num6)
                    {
                        break;
                    }
                    num6 = num5;
                    for (int j = 0; j < index; j++)
                    {
                        panel = panelArray[j];
                        if (panel != null)
                        {
                            if (num7 < panel.MinWidth)
                            {
                                if (panel.Width != panel.MinWidth)
                                {
                                    flag = true;
                                }
                                panel.Width = panel.MinWidth;
                                panelArray[j] = null;
                                num4--;
                                num5 -= panel.MinWidth;
                            }
                            else
                            {
                                if (panel.Width != num7)
                                {
                                    flag = true;
                                }
                                panel.Width = num7;
                            }
                        }
                    }
                }
            }
            if (flag || this.layoutDirty)
            {
                this.ApplyPanelWidths();
            }
        }

        protected virtual void OnDrawItem(StatusBarDrawItemEventArgs sbdievent)
        {
            StatusBarDrawItemEventHandler handler = (StatusBarDrawItemEventHandler) base.Events[EVENT_SBDRAWITEM];
            if (handler != null)
            {
                handler(this, sbdievent);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!base.DesignMode)
            {
                this.tooltips = new ControlToolTip(this);
            }
            if (!this.showPanels)
            {
                base.SendMessage(0x409, 1, 0);
                this.SetSimpleText(this.simpleText);
            }
            else
            {
                this.ForcePanelUpdate();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            if (this.tooltips != null)
            {
                this.tooltips.Dispose();
                this.tooltips = null;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (this.showPanels)
            {
                this.LayoutPanels();
                if (base.IsHandleCreated && (this.panelsRealized != this.panels.Count))
                {
                    this.RealizePanels();
                }
            }
            base.OnLayout(levent);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.lastClick.X = e.X;
            this.lastClick.Y = e.Y;
            base.OnMouseDown(e);
        }

        protected virtual void OnPanelClick(StatusBarPanelClickEventArgs e)
        {
            StatusBarPanelClickEventHandler handler = (StatusBarPanelClickEventHandler) base.Events[EVENT_PANELCLICK];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.Invalidate();
            base.OnResize(e);
        }

        internal void RealizePanels()
        {
            StatusBarPanel panel = null;
            int count = this.panels.Count;
            int panelsRealized = this.panelsRealized;
            this.panelsRealized = 0;
            if (count == 0)
            {
                base.SendMessage(System.Windows.Forms.NativeMethods.SB_SETTEXT, 0, "");
            }
            int num3 = 0;
            while (num3 < count)
            {
                panel = (StatusBarPanel) this.panels[num3];
                try
                {
                    panel.Realize();
                    this.panelsRealized++;
                }
                catch
                {
                }
                num3++;
            }
            while (num3 < panelsRealized)
            {
                base.SendMessage(System.Windows.Forms.NativeMethods.SB_SETTEXT, 0, (string) null);
                num3++;
            }
        }

        internal void RemoveAllPanelsWithoutUpdate()
        {
            int count = this.panels.Count;
            for (int i = 0; i < count; i++)
            {
                StatusBarPanel panel = (StatusBarPanel) this.panels[i];
                panel.ParentInternal = null;
            }
            this.panels.Clear();
            if (this.showPanels)
            {
                this.ApplyPanelWidths();
                this.ForcePanelUpdate();
            }
        }

        internal void SetPanelContentsWidths(bool newPanels)
        {
            int count = this.panels.Count;
            bool flag = false;
            for (int i = 0; i < count; i++)
            {
                StatusBarPanel panel = (StatusBarPanel) this.panels[i];
                if (panel.AutoSize == StatusBarPanelAutoSize.Contents)
                {
                    int contentsWidth = panel.GetContentsWidth(newPanels);
                    if (panel.Width != contentsWidth)
                    {
                        panel.Width = contentsWidth;
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                this.DirtyLayout();
                base.PerformLayout();
            }
        }

        private void SetSimpleText(string simpleText)
        {
            if (!this.showPanels && base.IsHandleCreated)
            {
                int wparam = 0x1ff;
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    wparam |= 0x400;
                }
                base.SendMessage(System.Windows.Forms.NativeMethods.SB_SETTEXT, wparam, simpleText);
            }
        }

        internal void SetToolTip(System.Windows.Forms.ToolTip t)
        {
            this.mainToolTip = t;
            this.toolTipSet = true;
        }

        public override string ToString()
        {
            string str = base.ToString();
            if (this.Panels != null)
            {
                str = str + ", Panels.Count: " + this.Panels.Count.ToString(CultureInfo.CurrentCulture);
                if (this.Panels.Count > 0)
                {
                    str = str + ", Panels[0]: " + this.Panels[0].ToString();
                }
            }
            return str;
        }

        private void UpdatePanelIndex()
        {
            int count = this.panels.Count;
            for (int i = 0; i < count; i++)
            {
                ((StatusBarPanel) this.panels[i]).Index = i;
            }
        }

        internal void UpdateTooltip(StatusBarPanel panel)
        {
            if (this.tooltips == null)
            {
                if (!base.IsHandleCreated || base.DesignMode)
                {
                    return;
                }
                this.tooltips = new ControlToolTip(this);
            }
            if ((panel.Parent == this) && (panel.ToolTipText.Length > 0))
            {
                int width = SystemInformation.Border3DSize.Width;
                ControlToolTip.Tool tool = this.tooltips.GetTool(panel);
                if (tool == null)
                {
                    tool = new ControlToolTip.Tool();
                }
                tool.text = panel.ToolTipText;
                tool.rect = new Rectangle((panel.Right - panel.Width) + width, 0, panel.Width - width, base.Height);
                this.tooltips.SetTool(panel, tool);
            }
            else
            {
                this.tooltips.SetTool(panel, null);
            }
        }

        private void WmDrawItem(ref Message m)
        {
            System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT));
            int count = this.panels.Count;
            if (lParam.itemID >= 0)
            {
                int itemID = lParam.itemID;
            }
            StatusBarPanel panel = (StatusBarPanel) this.panels[lParam.itemID];
            Graphics g = Graphics.FromHdcInternal(lParam.hDC);
            Rectangle r = Rectangle.FromLTRB(lParam.rcItem.left, lParam.rcItem.top, lParam.rcItem.right, lParam.rcItem.bottom);
            this.OnDrawItem(new StatusBarDrawItemEventArgs(g, this.Font, r, lParam.itemID, DrawItemState.None, panel, this.ForeColor, this.BackColor));
            g.Dispose();
        }

        private void WmNCHitTest(ref Message m)
        {
            int num = System.Windows.Forms.NativeMethods.Util.LOWORD(m.LParam);
            Rectangle bounds = base.Bounds;
            bool flag = true;
            if (num > ((bounds.X + bounds.Width) - this.SizeGripWidth))
            {
                Control parentInternal = this.ParentInternal;
                if ((parentInternal != null) && (parentInternal is Form))
                {
                    FormBorderStyle formBorderStyle = ((Form) parentInternal).FormBorderStyle;
                    if ((formBorderStyle != FormBorderStyle.Sizable) && (formBorderStyle != FormBorderStyle.SizableToolWindow))
                    {
                        flag = false;
                    }
                    if (!((Form) parentInternal).TopLevel || (this.Dock != DockStyle.Bottom))
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        Control.ControlCollection controls = parentInternal.Controls;
                        int count = controls.Count;
                        for (int i = 0; i < count; i++)
                        {
                            Control control2 = controls[i];
                            if (((control2 != this) && (control2.Dock == DockStyle.Bottom)) && (control2.Top > base.Top))
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    flag = false;
                }
            }
            if (flag)
            {
                base.WndProc(ref m);
            }
            else
            {
                m.Result = (IntPtr) 1;
            }
        }

        private void WmNotifyNMClick(System.Windows.Forms.NativeMethods.NMHDR note)
        {
            if (this.showPanels)
            {
                int count = this.panels.Count;
                int num2 = 0;
                int num3 = -1;
                for (int i = 0; i < count; i++)
                {
                    StatusBarPanel panel = (StatusBarPanel) this.panels[i];
                    num2 += panel.Width;
                    if (this.lastClick.X < num2)
                    {
                        num3 = i;
                        break;
                    }
                }
                if (num3 != -1)
                {
                    MouseButtons left = MouseButtons.Left;
                    int clicks = 0;
                    switch (note.code)
                    {
                        case -6:
                            left = MouseButtons.Right;
                            clicks = 2;
                            break;

                        case -5:
                            left = MouseButtons.Right;
                            clicks = 1;
                            break;

                        case -3:
                            left = MouseButtons.Left;
                            clicks = 2;
                            break;

                        case -2:
                            left = MouseButtons.Left;
                            clicks = 1;
                            break;
                    }
                    Point lastClick = this.lastClick;
                    StatusBarPanel statusBarPanel = (StatusBarPanel) this.panels[num3];
                    StatusBarPanelClickEventArgs e = new StatusBarPanelClickEventArgs(statusBarPanel, left, clicks, lastClick.X, lastClick.Y);
                    this.OnPanelClick(e);
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x202b:
                    this.WmDrawItem(ref m);
                    return;

                case 0x204e:
                case 0x4e:
                {
                    System.Windows.Forms.NativeMethods.NMHDR lParam = (System.Windows.Forms.NativeMethods.NMHDR) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHDR));
                    switch (lParam.code)
                    {
                        case -6:
                        case -5:
                        case -3:
                        case -2:
                            this.WmNotifyNMClick(lParam);
                            return;
                    }
                    break;
                }
                case 0x84:
                    this.WmNCHitTest(ref m);
                    return;

                default:
                    base.WndProc(ref m);
                    return;
            }
            base.WndProc(ref m);
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Color BackColor
        {
            get
            {
                return SystemColors.Control;
            }
            set
            {
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
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
                createParams.ClassName = "msctls_statusbar32";
                if (this.sizeGrip)
                {
                    createParams.Style |= 0x100;
                }
                else
                {
                    createParams.Style &= -257;
                }
                createParams.Style |= 12;
                return createParams;
            }
        }

        protected override System.Windows.Forms.ImeMode DefaultImeMode
        {
            get
            {
                return System.Windows.Forms.ImeMode.Disable;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, 0x16);
            }
        }

        [DefaultValue(2), Localizable(true)]
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override bool DoubleBuffered
        {
            get
            {
                return base.DoubleBuffered;
            }
            set
            {
                base.DoubleBuffered = value;
            }
        }

        [Localizable(true)]
        public override System.Drawing.Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                this.SetPanelContentsWidths(false);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public System.Windows.Forms.ImeMode ImeMode
        {
            get
            {
                return base.ImeMode;
            }
            set
            {
                base.ImeMode = value;
            }
        }

        internal System.Windows.Forms.ToolTip MainToolTip
        {
            get
            {
                return this.mainToolTip;
            }
        }

        [MergableProperty(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRDescription("StatusBarPanelsDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public StatusBarPanelCollection Panels
        {
            get
            {
                if (this.panelsCollection == null)
                {
                    this.panelsCollection = new StatusBarPanelCollection(this);
                }
                return this.panelsCollection;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("StatusBarShowPanelsDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool ShowPanels
        {
            get
            {
                return this.showPanels;
            }
            set
            {
                if (this.showPanels != value)
                {
                    this.showPanels = value;
                    this.layoutDirty = true;
                    if (base.IsHandleCreated)
                    {
                        int wparam = !this.showPanels ? 1 : 0;
                        base.SendMessage(0x409, wparam, 0);
                        if (this.showPanels)
                        {
                            base.PerformLayout();
                            this.RealizePanels();
                        }
                        else if (this.tooltips != null)
                        {
                            for (int i = 0; i < this.panels.Count; i++)
                            {
                                this.tooltips.SetTool(this.panels[i], null);
                            }
                        }
                        this.SetSimpleText(this.simpleText);
                    }
                }
            }
        }

        private int SizeGripWidth
        {
            get
            {
                if (this.sizeGripWidth == 0)
                {
                    if (Application.RenderWithVisualStyles && (VisualStyleRenderer != null))
                    {
                        System.Windows.Forms.VisualStyles.VisualStyleRenderer visualStyleRenderer = VisualStyleRenderer;
                        VisualStyleElement normal = VisualStyleElement.Status.GripperPane.Normal;
                        visualStyleRenderer.SetParameters(normal);
                        Size partSize = visualStyleRenderer.GetPartSize(Graphics.FromHwndInternal(base.Handle), ThemeSizeType.True);
                        this.sizeGripWidth = partSize.Width;
                        normal = VisualStyleElement.Status.Gripper.Normal;
                        visualStyleRenderer.SetParameters(normal);
                        partSize = visualStyleRenderer.GetPartSize(Graphics.FromHwndInternal(base.Handle), ThemeSizeType.True);
                        this.sizeGripWidth += partSize.Width;
                        this.sizeGripWidth = Math.Max(this.sizeGripWidth, 0x10);
                    }
                    else
                    {
                        this.sizeGripWidth = 0x10;
                    }
                }
                return this.sizeGripWidth;
            }
        }

        [System.Windows.Forms.SRDescription("StatusBarSizingGripDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(true)]
        public bool SizingGrip
        {
            get
            {
                return this.sizeGrip;
            }
            set
            {
                if (value != this.sizeGrip)
                {
                    this.sizeGrip = value;
                    base.RecreateHandle();
                }
            }
        }

        [DefaultValue(false)]
        public bool TabStop
        {
            get
            {
                return base.TabStop;
            }
            set
            {
                base.TabStop = value;
            }
        }

        [Localizable(true)]
        public override string Text
        {
            get
            {
                if (this.simpleText == null)
                {
                    return "";
                }
                return this.simpleText;
            }
            set
            {
                this.SetSimpleText(value);
                if (this.simpleText != value)
                {
                    this.simpleText = value;
                    this.OnTextChanged(EventArgs.Empty);
                }
            }
        }

        internal bool ToolTipSet
        {
            get
            {
                return this.toolTipSet;
            }
        }

        private static System.Windows.Forms.VisualStyles.VisualStyleRenderer VisualStyleRenderer
        {
            get
            {
                if (System.Windows.Forms.VisualStyles.VisualStyleRenderer.IsSupported)
                {
                    if (renderer == null)
                    {
                        renderer = new System.Windows.Forms.VisualStyles.VisualStyleRenderer(VisualStyleElement.ToolBar.Button.Normal);
                    }
                }
                else
                {
                    renderer = null;
                }
                return renderer;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        private class ControlToolTip
        {
            private int nextId;
            private Control parent;
            private Hashtable tools = new Hashtable();
            private ToolTipNativeWindow window;

            public ControlToolTip(Control parent)
            {
                this.window = new ToolTipNativeWindow(this);
                this.parent = parent;
            }

            private void AddTool(Tool tool)
            {
                if (((tool != null) && (tool.text != null)) && (tool.text.Length > 0))
                {
                    int num;
                    StatusBar parent = (StatusBar) this.parent;
                    if (parent.ToolTipSet)
                    {
                        num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(parent.MainToolTip, parent.MainToolTip.Handle), System.Windows.Forms.NativeMethods.TTM_ADDTOOL, 0, this.GetTOOLINFO(tool));
                    }
                    else
                    {
                        num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_ADDTOOL, 0, this.GetTOOLINFO(tool));
                    }
                    if (num == 0)
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("StatusBarAddFailed"));
                    }
                }
            }

            private void AssignId(Tool tool)
            {
                tool.id = (IntPtr) this.nextId;
                this.nextId++;
            }

            protected void CreateHandle()
            {
                if (!this.IsHandleCreated)
                {
                    this.window.CreateHandle(this.CreateParams);
                    System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, 0x13);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x418, 0, SystemInformation.MaxWindowTrackSize.Width);
                }
            }

            protected void DestroyHandle()
            {
                if (this.IsHandleCreated)
                {
                    this.window.DestroyHandle();
                    this.tools.Clear();
                }
            }

            public void Dispose()
            {
                this.DestroyHandle();
            }

            ~ControlToolTip()
            {
                this.DestroyHandle();
            }

            private System.Windows.Forms.NativeMethods.TOOLINFO_T GetMinTOOLINFO(Tool tool)
            {
                System.Windows.Forms.NativeMethods.TOOLINFO_T toolinfo_t = new System.Windows.Forms.NativeMethods.TOOLINFO_T {
                    cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_T)),
                    hwnd = this.parent.Handle
                };
                if (((int) tool.id) < 0)
                {
                    this.AssignId(tool);
                }
                StatusBar parent = (StatusBar) this.parent;
                if ((parent != null) && parent.ToolTipSet)
                {
                    toolinfo_t.uId = this.parent.Handle;
                    return toolinfo_t;
                }
                toolinfo_t.uId = tool.id;
                return toolinfo_t;
            }

            public Tool GetTool(object key)
            {
                return (Tool) this.tools[key];
            }

            private System.Windows.Forms.NativeMethods.TOOLINFO_T GetTOOLINFO(Tool tool)
            {
                System.Windows.Forms.NativeMethods.TOOLINFO_T minTOOLINFO = this.GetMinTOOLINFO(tool);
                minTOOLINFO.cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_T));
                minTOOLINFO.uFlags |= 0x110;
                Control parent = this.parent;
                if ((parent != null) && (parent.RightToLeft == RightToLeft.Yes))
                {
                    minTOOLINFO.uFlags |= 4;
                }
                minTOOLINFO.lpszText = tool.text;
                minTOOLINFO.rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(tool.rect.X, tool.rect.Y, tool.rect.Width, tool.rect.Height);
                return minTOOLINFO;
            }

            private void RemoveTool(Tool tool)
            {
                if (((tool != null) && (tool.text != null)) && ((tool.text.Length > 0) && (((int) tool.id) >= 0)))
                {
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_DELTOOL, 0, this.GetMinTOOLINFO(tool));
                }
            }

            public void SetTool(object key, Tool tool)
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                Tool tool2 = null;
                if (this.tools.ContainsKey(key))
                {
                    tool2 = (Tool) this.tools[key];
                }
                if (tool2 != null)
                {
                    flag = true;
                }
                if (tool != null)
                {
                    flag2 = true;
                }
                if (((tool != null) && (tool2 != null)) && (tool.id == tool2.id))
                {
                    flag3 = true;
                }
                if (flag3)
                {
                    this.UpdateTool(tool);
                }
                else
                {
                    if (flag)
                    {
                        this.RemoveTool(tool2);
                    }
                    if (flag2)
                    {
                        this.AddTool(tool);
                    }
                }
                if (tool != null)
                {
                    this.tools[key] = tool;
                }
                else
                {
                    this.tools.Remove(key);
                }
            }

            private void UpdateTool(Tool tool)
            {
                if (((tool != null) && (tool.text != null)) && ((tool.text.Length > 0) && (((int) tool.id) >= 0)))
                {
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_SETTOOLINFO, 0, this.GetTOOLINFO(tool));
                }
            }

            protected void WndProc(ref Message msg)
            {
                if (msg.Msg != 7)
                {
                    this.window.DefWndProc(ref msg);
                }
            }

            protected System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    System.Windows.Forms.CreateParams @params;
                    System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                        dwICC = 8
                    };
                    System.Windows.Forms.SafeNativeMethods.InitCommonControlsEx(icc);
                    return new System.Windows.Forms.CreateParams { Parent = IntPtr.Zero, ClassName = "tooltips_class32", Style = @params.Style | 1, ExStyle = 0, Caption = null };
                }
            }

            public IntPtr Handle
            {
                get
                {
                    if (this.window.Handle == IntPtr.Zero)
                    {
                        this.CreateHandle();
                    }
                    return this.window.Handle;
                }
            }

            private bool IsHandleCreated
            {
                get
                {
                    return (this.window.Handle != IntPtr.Zero);
                }
            }

            public class Tool
            {
                internal IntPtr id = new IntPtr(-1);
                public Rectangle rect = Rectangle.Empty;
                public string text;
            }

            private class ToolTipNativeWindow : NativeWindow
            {
                private StatusBar.ControlToolTip control;

                internal ToolTipNativeWindow(StatusBar.ControlToolTip control)
                {
                    this.control = control;
                }

                protected override void WndProc(ref Message m)
                {
                    if (this.control != null)
                    {
                        this.control.WndProc(ref m);
                    }
                }
            }
        }

        [ListBindable(false)]
        public class StatusBarPanelCollection : IList, ICollection, IEnumerable
        {
            private int lastAccessedIndex = -1;
            private StatusBar owner;

            public StatusBarPanelCollection(StatusBar owner)
            {
                this.owner = owner;
            }

            public virtual StatusBarPanel Add(string text)
            {
                StatusBarPanel panel = new StatusBarPanel {
                    Text = text
                };
                this.Add(panel);
                return panel;
            }

            public virtual int Add(StatusBarPanel value)
            {
                int count = this.owner.panels.Count;
                this.Insert(count, value);
                return count;
            }

            public virtual void AddRange(StatusBarPanel[] panels)
            {
                if (panels == null)
                {
                    throw new ArgumentNullException("panels");
                }
                foreach (StatusBarPanel panel in panels)
                {
                    this.Add(panel);
                }
            }

            public virtual void Clear()
            {
                this.owner.RemoveAllPanelsWithoutUpdate();
                this.owner.PerformLayout();
            }

            public bool Contains(StatusBarPanel panel)
            {
                return (this.IndexOf(panel) != -1);
            }

            public virtual bool ContainsKey(string key)
            {
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            public IEnumerator GetEnumerator()
            {
                if (this.owner.panels != null)
                {
                    return this.owner.panels.GetEnumerator();
                }
                return new StatusBarPanel[0].GetEnumerator();
            }

            public int IndexOf(StatusBarPanel panel)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i] == panel)
                    {
                        return i;
                    }
                }
                return -1;
            }

            public virtual int IndexOfKey(string key)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    if (this.IsValidIndex(this.lastAccessedIndex) && WindowsFormsUtils.SafeCompareStrings(this[this.lastAccessedIndex].Name, key, true))
                    {
                        return this.lastAccessedIndex;
                    }
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, true))
                        {
                            this.lastAccessedIndex = i;
                            return i;
                        }
                    }
                    this.lastAccessedIndex = -1;
                }
                return -1;
            }

            public virtual void Insert(int index, StatusBarPanel value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.owner.layoutDirty = true;
                if ((value.Parent != this.owner) && (value.Parent != null))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ObjectHasParent"), "value");
                }
                int count = this.owner.panels.Count;
                if ((index < 0) || (index > count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                value.ParentInternal = this.owner;
                switch (value.AutoSize)
                {
                    case StatusBarPanelAutoSize.Contents:
                        value.Width = value.GetContentsWidth(true);
                        break;
                }
                this.owner.panels.Insert(index, value);
                this.owner.UpdatePanelIndex();
                this.owner.ForcePanelUpdate();
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            public virtual void Remove(StatusBarPanel value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("StatusBarPanel");
                }
                if (value.Parent == this.owner)
                {
                    this.RemoveAt(value.Index);
                }
            }

            public virtual void RemoveAt(int index)
            {
                int count = this.Count;
                if ((index < 0) || (index >= count))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                StatusBarPanel panel = (StatusBarPanel) this.owner.panels[index];
                this.owner.panels.RemoveAt(index);
                panel.ParentInternal = null;
                this.owner.UpdateTooltip(panel);
                this.owner.UpdatePanelIndex();
                this.owner.ForcePanelUpdate();
            }

            public virtual void RemoveByKey(string key)
            {
                int index = this.IndexOfKey(key);
                if (this.IsValidIndex(index))
                {
                    this.RemoveAt(index);
                }
            }

            void ICollection.CopyTo(Array dest, int index)
            {
                this.owner.panels.CopyTo(dest, index);
            }

            int IList.Add(object value)
            {
                if (!(value is StatusBarPanel))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("StatusBarBadStatusBarPanel"), "value");
                }
                return this.Add((StatusBarPanel) value);
            }

            bool IList.Contains(object panel)
            {
                return ((panel is StatusBarPanel) && this.Contains((StatusBarPanel) panel));
            }

            int IList.IndexOf(object panel)
            {
                if (panel is StatusBarPanel)
                {
                    return this.IndexOf((StatusBarPanel) panel);
                }
                return -1;
            }

            void IList.Insert(int index, object value)
            {
                if (!(value is StatusBarPanel))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("StatusBarBadStatusBarPanel"), "value");
                }
                this.Insert(index, (StatusBarPanel) value);
            }

            void IList.Remove(object value)
            {
                if (value is StatusBarPanel)
                {
                    this.Remove((StatusBarPanel) value);
                }
            }

            [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
            public int Count
            {
                get
                {
                    return this.owner.panels.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public virtual StatusBarPanel this[int index]
            {
                get
                {
                    return (StatusBarPanel) this.owner.panels[index];
                }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("StatusBarPanel");
                    }
                    this.owner.layoutDirty = true;
                    if (value.Parent != null)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ObjectHasParent"), "value");
                    }
                    int count = this.owner.panels.Count;
                    if ((index < 0) || (index >= count))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    StatusBarPanel panel = (StatusBarPanel) this.owner.panels[index];
                    panel.ParentInternal = null;
                    value.ParentInternal = this.owner;
                    if (value.AutoSize == StatusBarPanelAutoSize.Contents)
                    {
                        value.Width = value.GetContentsWidth(true);
                    }
                    this.owner.panels[index] = value;
                    value.Index = index;
                    if (this.owner.ArePanelsRealized())
                    {
                        this.owner.PerformLayout();
                        value.Realize();
                    }
                }
            }

            public virtual StatusBarPanel this[string key]
            {
                get
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        int index = this.IndexOfKey(key);
                        if (this.IsValidIndex(index))
                        {
                            return this[index];
                        }
                    }
                    return null;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    if (!(value is StatusBarPanel))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("StatusBarBadStatusBarPanel"), "value");
                    }
                    this[index] = (StatusBarPanel) value;
                }
            }
        }
    }
}

