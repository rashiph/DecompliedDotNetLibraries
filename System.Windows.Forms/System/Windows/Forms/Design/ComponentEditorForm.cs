namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Internal;

    [ClassInterface(ClassInterfaceType.AutoDispatch), ToolboxItem(false), ComVisible(true), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class ComponentEditorForm : Form
    {
        private int activePage;
        private Button applyButton;
        private const int BUTTON_HEIGHT = 0x17;
        private const int BUTTON_PAD = 6;
        private const int BUTTON_WIDTH = 80;
        private Button cancelButton;
        private IComponent component;
        private bool dirty;
        private bool firstActivate;
        private Button helpButton;
        private int initialActivePage;
        private Size maxSize = Size.Empty;
        private const int MIN_SELECTOR_WIDTH = 90;
        private Button okButton;
        private Panel pageHost = new Panel();
        private ComponentEditorPageSite[] pageSites;
        private System.Type[] pageTypes;
        private PageSelector selector;
        private const int SELECTOR_PADDING = 10;
        private ImageList selectorImageList;
        private const int STRIP_HEIGHT = 4;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler AutoSizeChanged
        {
            add
            {
                base.AutoSizeChanged += value;
            }
            remove
            {
                base.AutoSizeChanged -= value;
            }
        }

        public ComponentEditorForm(object component, System.Type[] pageTypes)
        {
            if (!(component is IComponent))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("ComponentEditorFormBadComponent"), "component");
            }
            this.component = (IComponent) component;
            this.pageTypes = pageTypes;
            this.dirty = false;
            this.firstActivate = true;
            this.activePage = -1;
            this.initialActivePage = 0;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MinimizeBox = false;
            base.MaximizeBox = false;
            base.ShowInTaskbar = false;
            base.Icon = null;
            base.StartPosition = FormStartPosition.CenterParent;
            this.OnNewObjects();
            this.OnConfigureUI();
        }

        internal virtual void ApplyChanges(bool lastApply)
        {
            if (this.dirty)
            {
                IComponentChangeService service = null;
                if (this.component.Site != null)
                {
                    service = (IComponentChangeService) this.component.Site.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        try
                        {
                            service.OnComponentChanging(this.component, null);
                        }
                        catch (CheckoutException exception)
                        {
                            if (exception != CheckoutException.Canceled)
                            {
                                throw exception;
                            }
                            return;
                        }
                    }
                }
                for (int i = 0; i < this.pageSites.Length; i++)
                {
                    if (this.pageSites[i].Dirty)
                    {
                        this.pageSites[i].GetPageControl().ApplyChanges();
                        this.pageSites[i].Dirty = false;
                    }
                }
                if (service != null)
                {
                    service.OnComponentChanged(this.component, null, null, null);
                }
                this.applyButton.Enabled = false;
                this.cancelButton.Text = System.Windows.Forms.SR.GetString("CloseCaption");
                this.dirty = false;
                if (!lastApply)
                {
                    for (int j = 0; j < this.pageSites.Length; j++)
                    {
                        this.pageSites[j].GetPageControl().OnApplyComplete();
                    }
                }
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (this.firstActivate)
            {
                this.firstActivate = false;
                this.selector.SelectedNode = this.selector.Nodes[this.initialActivePage];
                this.pageSites[this.initialActivePage].Active = true;
                this.activePage = this.initialActivePage;
                this.helpButton.Enabled = this.pageSites[this.activePage].GetPageControl().SupportsHelp();
            }
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            if (sender == this.okButton)
            {
                this.ApplyChanges(true);
                base.DialogResult = DialogResult.OK;
            }
            else if (sender == this.cancelButton)
            {
                base.DialogResult = DialogResult.Cancel;
            }
            else if (sender == this.applyButton)
            {
                this.ApplyChanges(false);
            }
            else if (sender == this.helpButton)
            {
                this.ShowPageHelp();
            }
        }

        private void OnConfigureUI()
        {
            Font defaultFont = Control.DefaultFont;
            if (this.component.Site != null)
            {
                IUIService service = (IUIService) this.component.Site.GetService(typeof(IUIService));
                if (service != null)
                {
                    defaultFont = (Font) service.Styles["DialogFont"];
                }
            }
            this.Font = defaultFont;
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.applyButton = new Button();
            this.helpButton = new Button();
            this.selectorImageList = new ImageList();
            this.selectorImageList.ImageSize = new Size(0x10, 0x10);
            this.selector = new PageSelector();
            this.selector.ImageList = this.selectorImageList;
            this.selector.AfterSelect += new TreeViewEventHandler(this.OnSelChangeSelector);
            Label label = new Label {
                BackColor = SystemColors.ControlDark
            };
            int width = 90;
            if (this.pageSites != null)
            {
                for (int i = 0; i < this.pageSites.Length; i++)
                {
                    ComponentEditorPage pageControl = this.pageSites[i].GetPageControl();
                    string title = pageControl.Title;
                    Graphics graphics = base.CreateGraphicsInternal();
                    int num3 = (int) graphics.MeasureString(title, this.Font).Width;
                    graphics.Dispose();
                    this.selectorImageList.Images.Add(pageControl.Icon.ToBitmap());
                    this.selector.Nodes.Add(new TreeNode(title, i, i));
                    if (num3 > width)
                    {
                        width = num3;
                    }
                }
            }
            width += 10;
            string str2 = string.Empty;
            ISite site = this.component.Site;
            if (site != null)
            {
                str2 = System.Windows.Forms.SR.GetString("ComponentEditorFormProperties", new object[] { site.Name });
            }
            else
            {
                str2 = System.Windows.Forms.SR.GetString("ComponentEditorFormPropertiesNoName");
            }
            this.Text = str2;
            Rectangle rectangle = new Rectangle(12 + width, 0x10, this.maxSize.Width, this.maxSize.Height);
            this.pageHost.Bounds = rectangle;
            label.Bounds = new Rectangle(rectangle.X, 6, rectangle.Width, 4);
            if (this.pageSites != null)
            {
                Rectangle rectangle2 = new Rectangle(0, 0, rectangle.Width, rectangle.Height);
                for (int j = 0; j < this.pageSites.Length; j++)
                {
                    this.pageSites[j].GetPageControl().GetControl().Bounds = rectangle2;
                }
            }
            int num5 = SystemInformation.FixedFrameBorderSize.Width;
            Rectangle rectangle3 = rectangle;
            Size size = new Size((rectangle3.Width + (3 * (6 + num5))) + width, ((((rectangle3.Height + 4) + 0x18) + 0x17) + (2 * num5)) + SystemInformation.CaptionHeight);
            base.Size = size;
            this.selector.Bounds = new Rectangle(6, 6, width, ((rectangle3.Height + 4) + 12) + 0x17);
            rectangle3.X = (rectangle3.Width + rectangle3.X) - 80;
            rectangle3.Y = (rectangle3.Height + rectangle3.Y) + 6;
            rectangle3.Width = 80;
            rectangle3.Height = 0x17;
            this.helpButton.Bounds = rectangle3;
            this.helpButton.Text = System.Windows.Forms.SR.GetString("HelpCaption");
            this.helpButton.Click += new EventHandler(this.OnButtonClick);
            this.helpButton.Enabled = false;
            this.helpButton.FlatStyle = FlatStyle.System;
            rectangle3.X -= 0x56;
            this.applyButton.Bounds = rectangle3;
            this.applyButton.Text = System.Windows.Forms.SR.GetString("ApplyCaption");
            this.applyButton.Click += new EventHandler(this.OnButtonClick);
            this.applyButton.Enabled = false;
            this.applyButton.FlatStyle = FlatStyle.System;
            rectangle3.X -= 0x56;
            this.cancelButton.Bounds = rectangle3;
            this.cancelButton.Text = System.Windows.Forms.SR.GetString("CancelCaption");
            this.cancelButton.Click += new EventHandler(this.OnButtonClick);
            this.cancelButton.FlatStyle = FlatStyle.System;
            base.CancelButton = this.cancelButton;
            rectangle3.X -= 0x56;
            this.okButton.Bounds = rectangle3;
            this.okButton.Text = System.Windows.Forms.SR.GetString("OKCaption");
            this.okButton.Click += new EventHandler(this.OnButtonClick);
            this.okButton.FlatStyle = FlatStyle.System;
            base.AcceptButton = this.okButton;
            base.Controls.Clear();
            base.Controls.AddRange(new Control[] { this.selector, label, this.pageHost, this.okButton, this.cancelButton, this.applyButton, this.helpButton });
            this.AutoScaleBaseSize = new Size(5, 14);
            base.ApplyAutoScaling();
        }

        protected override void OnHelpRequested(HelpEventArgs e)
        {
            base.OnHelpRequested(e);
            this.ShowPageHelp();
        }

        private void OnNewObjects()
        {
            this.pageSites = null;
            this.maxSize = new Size(0x102, 0x18 * this.pageTypes.Length);
            this.pageSites = new ComponentEditorPageSite[this.pageTypes.Length];
            for (int i = 0; i < this.pageTypes.Length; i++)
            {
                this.pageSites[i] = new ComponentEditorPageSite(this.pageHost, this.pageTypes[i], this.component, this);
                Size size = this.pageSites[i].GetPageControl().Size;
                if (size.Width > this.maxSize.Width)
                {
                    this.maxSize.Width = size.Width;
                }
                if (size.Height > this.maxSize.Height)
                {
                    this.maxSize.Height = size.Height;
                }
            }
            for (int j = 0; j < this.pageSites.Length; j++)
            {
                this.pageSites[j].GetPageControl().Size = this.maxSize;
            }
        }

        protected virtual void OnSelChangeSelector(object source, TreeViewEventArgs e)
        {
            if (!this.firstActivate)
            {
                int index = this.selector.SelectedNode.Index;
                if (index != this.activePage)
                {
                    if (this.activePage != -1)
                    {
                        if (this.pageSites[this.activePage].AutoCommit)
                        {
                            this.ApplyChanges(false);
                        }
                        this.pageSites[this.activePage].Active = false;
                    }
                    this.activePage = index;
                    this.pageSites[this.activePage].Active = true;
                    this.helpButton.Enabled = this.pageSites[this.activePage].GetPageControl().SupportsHelp();
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public override bool PreProcessMessage(ref Message msg)
        {
            return (((this.pageSites != null) && this.pageSites[this.activePage].GetPageControl().IsPageMessage(ref msg)) || base.PreProcessMessage(ref msg));
        }

        internal virtual void SetDirty()
        {
            this.dirty = true;
            this.applyButton.Enabled = true;
            this.cancelButton.Text = System.Windows.Forms.SR.GetString("CancelCaption");
        }

        public virtual DialogResult ShowForm()
        {
            return this.ShowForm(null, 0);
        }

        public virtual DialogResult ShowForm(int page)
        {
            return this.ShowForm(null, page);
        }

        public virtual DialogResult ShowForm(IWin32Window owner)
        {
            return this.ShowForm(owner, 0);
        }

        public virtual DialogResult ShowForm(IWin32Window owner, int page)
        {
            this.initialActivePage = page;
            base.ShowDialog(owner);
            return base.DialogResult;
        }

        private void ShowPageHelp()
        {
            if (this.pageSites[this.activePage].GetPageControl().SupportsHelp())
            {
                this.pageSites[this.activePage].GetPageControl().ShowHelp();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }

        private sealed class ComponentEditorPageSite : IComponentEditorPageSite
        {
            internal IComponent component;
            private ComponentEditorForm form;
            internal bool isActive;
            internal bool isDirty;
            internal ComponentEditorPage pageControl;
            internal Control parent;

            internal ComponentEditorPageSite(Control parent, System.Type pageClass, IComponent component, ComponentEditorForm form)
            {
                this.component = component;
                this.parent = parent;
                this.isActive = false;
                this.isDirty = false;
                if (form == null)
                {
                    throw new ArgumentNullException("form");
                }
                this.form = form;
                try
                {
                    this.pageControl = (ComponentEditorPage) System.Windows.Forms.SecurityUtils.SecureCreateInstance(pageClass);
                }
                catch (TargetInvocationException exception)
                {
                    throw new TargetInvocationException(System.Windows.Forms.SR.GetString("ExceptionCreatingCompEditorControl", new object[] { exception.ToString() }), exception.InnerException);
                }
                this.pageControl.SetSite(this);
                this.pageControl.SetComponent(component);
            }

            public Control GetControl()
            {
                return this.parent;
            }

            internal ComponentEditorPage GetPageControl()
            {
                return this.pageControl;
            }

            public void SetDirty()
            {
                if (this.isActive)
                {
                    this.Dirty = true;
                }
                this.form.SetDirty();
            }

            internal bool Active
            {
                set
                {
                    if (value)
                    {
                        this.pageControl.CreateControl();
                        this.pageControl.Activate();
                    }
                    else
                    {
                        this.pageControl.Deactivate();
                    }
                    this.isActive = value;
                }
            }

            internal bool AutoCommit
            {
                get
                {
                    return this.pageControl.CommitOnDeactivate;
                }
            }

            internal bool Dirty
            {
                get
                {
                    return this.isDirty;
                }
                set
                {
                    this.isDirty = value;
                }
            }
        }

        internal sealed class PageSelector : TreeView
        {
            private IntPtr hbrushDither;
            private const int PADDING_HORZ = 4;
            private const int PADDING_VERT = 3;
            private const int SIZE_ICON_X = 0x10;
            private const int SIZE_ICON_Y = 0x10;
            private const int STATE_HOT = 2;
            private const int STATE_NORMAL = 0;
            private const int STATE_SELECTED = 1;

            public PageSelector()
            {
                base.HotTracking = true;
                base.HideSelection = false;
                this.BackColor = SystemColors.Control;
                base.Indent = 0;
                base.LabelEdit = false;
                base.Scrollable = false;
                base.ShowLines = false;
                base.ShowPlusMinus = false;
                base.ShowRootLines = false;
                base.BorderStyle = BorderStyle.None;
                base.Indent = 0;
                base.FullRowSelect = true;
            }

            private void CreateDitherBrush()
            {
                short[] lpvBits = new short[] { -21846, 0x5555, -21846, 0x5555, -21846, 0x5555, -21846, 0x5555 };
                IntPtr handle = System.Windows.Forms.SafeNativeMethods.CreateBitmap(8, 8, 1, 1, lpvBits);
                if (handle != IntPtr.Zero)
                {
                    this.hbrushDither = System.Windows.Forms.SafeNativeMethods.CreatePatternBrush(new HandleRef(null, handle));
                    System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(null, handle));
                }
            }

            private void DrawTreeItem(string itemText, int imageIndex, IntPtr dc, System.Windows.Forms.NativeMethods.RECT rcIn, int state, int backColor, int textColor)
            {
                IntNativeMethods.SIZE size = new IntNativeMethods.SIZE();
                IntNativeMethods.RECT lpRect = new IntNativeMethods.RECT();
                IntNativeMethods.RECT rect = new IntNativeMethods.RECT(rcIn.left, rcIn.top, rcIn.right, rcIn.bottom);
                ImageList imageList = base.ImageList;
                IntPtr zero = IntPtr.Zero;
                if ((state & 2) != 0)
                {
                    zero = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, dc), new HandleRef(base.Parent, base.Parent.FontHandle));
                }
                if (((state & 1) != 0) && (this.hbrushDither != IntPtr.Zero))
                {
                    this.FillRectDither(dc, rcIn);
                    System.Windows.Forms.SafeNativeMethods.SetBkMode(new HandleRef(null, dc), 1);
                }
                else
                {
                    System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(null, dc), backColor);
                    IntUnsafeNativeMethods.ExtTextOut(new HandleRef(null, dc), 0, 0, 6, ref rect, null, 0, null);
                }
                IntUnsafeNativeMethods.GetTextExtentPoint32(new HandleRef(null, dc), itemText, size);
                lpRect.left = (rect.left + 0x10) + 8;
                lpRect.top = rect.top + (((rect.bottom - rect.top) - size.cy) >> 1);
                lpRect.bottom = lpRect.top + size.cy;
                lpRect.right = rect.right;
                System.Windows.Forms.SafeNativeMethods.SetTextColor(new HandleRef(null, dc), textColor);
                IntUnsafeNativeMethods.DrawText(new HandleRef(null, dc), itemText, ref lpRect, 0x8804);
                System.Windows.Forms.SafeNativeMethods.ImageList_Draw(new HandleRef(imageList, imageList.Handle), imageIndex, new HandleRef(null, dc), 4, rect.top + (((rect.bottom - rect.top) - 0x10) >> 1), 1);
                if ((state & 2) != 0)
                {
                    int clr = System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(null, dc), ColorTranslator.ToWin32(SystemColors.ControlLightLight));
                    lpRect.left = rect.left;
                    lpRect.top = rect.top;
                    lpRect.bottom = rect.top + 1;
                    lpRect.right = rect.right;
                    IntUnsafeNativeMethods.ExtTextOut(new HandleRef(null, dc), 0, 0, 2, ref lpRect, null, 0, null);
                    lpRect.bottom = rect.bottom;
                    lpRect.right = rect.left + 1;
                    IntUnsafeNativeMethods.ExtTextOut(new HandleRef(null, dc), 0, 0, 2, ref lpRect, null, 0, null);
                    System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(null, dc), ColorTranslator.ToWin32(SystemColors.ControlDark));
                    lpRect.left = rect.left;
                    lpRect.right = rect.right;
                    lpRect.top = rect.bottom - 1;
                    lpRect.bottom = rect.bottom;
                    IntUnsafeNativeMethods.ExtTextOut(new HandleRef(null, dc), 0, 0, 2, ref lpRect, null, 0, null);
                    lpRect.left = rect.right - 1;
                    lpRect.top = rect.top;
                    IntUnsafeNativeMethods.ExtTextOut(new HandleRef(null, dc), 0, 0, 2, ref lpRect, null, 0, null);
                    System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(null, dc), clr);
                }
                if (zero != IntPtr.Zero)
                {
                    System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, dc), new HandleRef(null, zero));
                }
            }

            private void FillRectDither(IntPtr dc, System.Windows.Forms.NativeMethods.RECT rc)
            {
                if (System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(null, dc), new HandleRef(this, this.hbrushDither)) != IntPtr.Zero)
                {
                    int crColor = System.Windows.Forms.SafeNativeMethods.SetTextColor(new HandleRef(null, dc), ColorTranslator.ToWin32(SystemColors.ControlLightLight));
                    int clr = System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(null, dc), ColorTranslator.ToWin32(SystemColors.Control));
                    System.Windows.Forms.SafeNativeMethods.PatBlt(new HandleRef(null, dc), rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top, 0xf00021);
                    System.Windows.Forms.SafeNativeMethods.SetTextColor(new HandleRef(null, dc), crColor);
                    System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(null, dc), clr);
                }
            }

            private void OnCustomDraw(ref Message m)
            {
                System.Windows.Forms.NativeMethods.NMTVCUSTOMDRAW lParam = (System.Windows.Forms.NativeMethods.NMTVCUSTOMDRAW) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMTVCUSTOMDRAW));
                switch (lParam.nmcd.dwDrawStage)
                {
                    case 1:
                        m.Result = (IntPtr) 0x30;
                        return;

                    case 2:
                        m.Result = (IntPtr) 4;
                        return;

                    case 0x10001:
                    {
                        TreeNode node = TreeNode.FromHandle(this, lParam.nmcd.dwItemSpec);
                        if (node != null)
                        {
                            int state = 0;
                            int uItemState = lParam.nmcd.uItemState;
                            if (((uItemState & 0x40) != 0) || ((uItemState & 0x10) != 0))
                            {
                                state |= 2;
                            }
                            if ((uItemState & 1) != 0)
                            {
                                state |= 1;
                            }
                            this.DrawTreeItem(node.Text, node.ImageIndex, lParam.nmcd.hdc, lParam.nmcd.rc, state, ColorTranslator.ToWin32(SystemColors.Control), ColorTranslator.ToWin32(SystemColors.ControlText));
                        }
                        m.Result = (IntPtr) 4;
                        return;
                    }
                }
                m.Result = IntPtr.Zero;
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                int wParam = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x111c, 0, 0);
                wParam += 6;
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x111b, wParam, 0);
                if (this.hbrushDither == IntPtr.Zero)
                {
                    this.CreateDitherBrush();
                }
            }

            protected override void OnHandleDestroyed(EventArgs e)
            {
                base.OnHandleDestroyed(e);
                if (!base.RecreatingHandle && (this.hbrushDither != IntPtr.Zero))
                {
                    System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(this, this.hbrushDither));
                    this.hbrushDither = IntPtr.Zero;
                }
            }

            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x204e)
                {
                    System.Windows.Forms.NativeMethods.NMHDR lParam = (System.Windows.Forms.NativeMethods.NMHDR) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHDR));
                    if (lParam.code == -12)
                    {
                        this.OnCustomDraw(ref m);
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
                    createParams.ExStyle |= 0x20000;
                    return createParams;
                }
            }
        }
    }
}

