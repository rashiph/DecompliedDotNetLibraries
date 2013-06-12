namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;
    using System.Windows.Forms.VisualStyles;

    [DefaultEvent("AfterSelect"), Designer("System.Windows.Forms.Design.TreeViewDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultProperty("Nodes"), System.Windows.Forms.SRDescription("DescriptionTreeView"), Docking(DockingBehavior.Ask)]
    public class TreeView : Control
    {
        private static readonly string backSlash = @"\";
        private System.Windows.Forms.BorderStyle borderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        private string controlToolTipText;
        private const int DefaultTreeViewIndent = 0x13;
        private MouseButtons downButton;
        private TreeViewDrawMode drawMode;
        internal TreeNode editNode;
        private IntPtr hNodeMouseDown = IntPtr.Zero;
        private bool hoveredAlready;
        private System.Windows.Forms.ImageList.Indexer imageIndexer;
        private System.Windows.Forms.ImageList imageList;
        private int indent = -1;
        private System.Windows.Forms.ImageList internalStateImageList;
        private int itemHeight = -1;
        private Color lineColor;
        private static readonly int MaxIndent = 0x7d00;
        internal TreeNodeCollection nodes;
        internal bool nodesCollectionClear;
        internal Hashtable nodeTable = new Hashtable();
        private string pathSeparator = backSlash;
        private TreeNode prevHoveredNode;
        private bool rightToLeftLayout;
        internal TreeNode root;
        private System.Windows.Forms.ImageList.Indexer selectedImageIndexer;
        internal TreeNode selectedNode;
        private bool setOddHeight;
        private System.Windows.Forms.ImageList stateImageList;
        private TreeNode topNode;
        private IComparer treeViewNodeSorter;
        private BitVector32 treeViewState = new BitVector32(0x75);
        private const int TREEVIEWSTATE_checkBoxes = 8;
        private const int TREEVIEWSTATE_doubleclickFired = 0x800;
        private const int TREEVIEWSTATE_fullRowSelect = 0x200;
        private const int TREEVIEWSTATE_hideSelection = 1;
        private const int TREEVIEWSTATE_hotTracking = 0x100;
        private const int TREEVIEWSTATE_ignoreSelects = 0x10000;
        private const int TREEVIEWSTATE_labelEdit = 2;
        private const int TREEVIEWSTATE_lastControlValidated = 0x4000;
        private const int TREEVIEWSTATE_mouseUpFired = 0x1000;
        private const int TREEVIEWSTATE_scrollable = 4;
        private const int TREEVIEWSTATE_showLines = 0x10;
        private const int TREEVIEWSTATE_showNodeToolTips = 0x400;
        private const int TREEVIEWSTATE_showPlusMinus = 0x20;
        private const int TREEVIEWSTATE_showRootLines = 0x40;
        private const int TREEVIEWSTATE_showTreeViewContextMenu = 0x2000;
        private const int TREEVIEWSTATE_sorted = 0x80;
        private const int TREEVIEWSTATE_stopResizeWindowMsgs = 0x8000;

        [System.Windows.Forms.SRDescription("TreeViewAfterCheckDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event TreeViewEventHandler AfterCheck;

        [System.Windows.Forms.SRDescription("TreeViewAfterCollapseDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event TreeViewEventHandler AfterCollapse;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewAfterExpandDescr")]
        public event TreeViewEventHandler AfterExpand;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewAfterEditDescr")]
        public event NodeLabelEditEventHandler AfterLabelEdit;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewAfterSelectDescr")]
        public event TreeViewEventHandler AfterSelect;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRDescription("TreeViewBeforeCheckDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event TreeViewCancelEventHandler BeforeCheck;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewBeforeCollapseDescr")]
        public event TreeViewCancelEventHandler BeforeCollapse;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewBeforeExpandDescr")]
        public event TreeViewCancelEventHandler BeforeExpand;

        [System.Windows.Forms.SRDescription("TreeViewBeforeEditDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event NodeLabelEditEventHandler BeforeLabelEdit;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewBeforeSelectDescr")]
        public event TreeViewCancelEventHandler BeforeSelect;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewDrawNodeEventDescr")]
        public event DrawTreeNodeEventHandler DrawNode;

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("ListViewItemDragDescr")]
        public event ItemDragEventHandler ItemDrag;

        [System.Windows.Forms.SRDescription("TreeViewNodeMouseClickDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event TreeNodeMouseClickEventHandler NodeMouseClick;

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewNodeMouseDoubleClickDescr")]
        public event TreeNodeMouseClickEventHandler NodeMouseDoubleClick;

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("TreeViewNodeMouseHoverDescr")]
        public event TreeNodeMouseHoverEventHandler NodeMouseHover;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnRightToLeftLayoutChangedDescr")]
        public event EventHandler RightToLeftLayoutChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TextChanged
        {
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        public TreeView()
        {
            this.root = new TreeNode(this);
            this.SelectedImageIndexer.Index = 0;
            this.ImageIndexer.Index = 0;
            base.SetStyle(ControlStyles.UserPaint, false);
            base.SetStyle(ControlStyles.StandardClick, false);
            base.SetStyle(ControlStyles.UseTextForAccessibility, false);
        }

        private void AttachImageListHandlers()
        {
            if (this.imageList != null)
            {
                this.imageList.RecreateHandle += new EventHandler(this.ImageListRecreateHandle);
                this.imageList.Disposed += new EventHandler(this.DetachImageList);
                this.imageList.ChangeHandle += new EventHandler(this.ImageListChangedHandle);
            }
        }

        private void AttachStateImageListHandlers()
        {
            if (this.stateImageList != null)
            {
                this.stateImageList.RecreateHandle += new EventHandler(this.StateImageListRecreateHandle);
                this.stateImageList.Disposed += new EventHandler(this.DetachStateImageList);
                this.stateImageList.ChangeHandle += new EventHandler(this.StateImageListChangedHandle);
            }
        }

        public void BeginUpdate()
        {
            base.BeginUpdateInternal();
        }

        public void CollapseAll()
        {
            this.root.Collapse();
        }

        private void ContextMenuStripClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            ContextMenuStrip strip = sender as ContextMenuStrip;
            strip.Closing -= new ToolStripDropDownClosingEventHandler(this.ContextMenuStripClosing);
            base.SendMessage(0x110b, 8, (string) null);
        }

        protected override void CreateHandle()
        {
            if (!base.RecreatingHandle)
            {
                IntPtr userCookie = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                try
                {
                    System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                        dwICC = 2
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

        private void CustomDraw(ref Message m)
        {
            TreeNode node;
            System.Windows.Forms.NativeMethods.NMTVCUSTOMDRAW lParam = (System.Windows.Forms.NativeMethods.NMTVCUSTOMDRAW) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMTVCUSTOMDRAW));
            switch (lParam.nmcd.dwDrawStage)
            {
                case 0x10001:
                    node = this.NodeFromHandle(lParam.nmcd.dwItemSpec);
                    if (node != null)
                    {
                        int uItemState = lParam.nmcd.uItemState;
                        if (this.drawMode == TreeViewDrawMode.OwnerDrawText)
                        {
                            lParam.clrText = lParam.clrTextBk;
                            Marshal.StructureToPtr(lParam, m.LParam, false);
                            m.Result = (IntPtr) 0x12;
                            return;
                        }
                        if (this.drawMode == TreeViewDrawMode.OwnerDrawAll)
                        {
                            DrawTreeNodeEventArgs args;
                            using (Graphics graphics = Graphics.FromHdcInternal(lParam.nmcd.hdc))
                            {
                                Rectangle rowBounds = node.RowBounds;
                                System.Windows.Forms.NativeMethods.SCROLLINFO si = new System.Windows.Forms.NativeMethods.SCROLLINFO {
                                    cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.SCROLLINFO)),
                                    fMask = 4
                                };
                                if (System.Windows.Forms.UnsafeNativeMethods.GetScrollInfo(new HandleRef(this, base.Handle), 0, si))
                                {
                                    int nPos = si.nPos;
                                    if (nPos > 0)
                                    {
                                        rowBounds.X -= nPos;
                                        rowBounds.Width += nPos;
                                    }
                                }
                                args = new DrawTreeNodeEventArgs(graphics, node, rowBounds, (TreeNodeStates) uItemState);
                                this.OnDrawNode(args);
                            }
                            if (!args.DrawDefault)
                            {
                                m.Result = (IntPtr) 4;
                                return;
                            }
                        }
                        OwnerDrawPropertyBag itemRenderStyles = this.GetItemRenderStyles(node, uItemState);
                        bool flag = false;
                        Color foreColor = itemRenderStyles.ForeColor;
                        Color backColor = itemRenderStyles.BackColor;
                        if ((itemRenderStyles != null) && !foreColor.IsEmpty)
                        {
                            lParam.clrText = ColorTranslator.ToWin32(foreColor);
                            flag = true;
                        }
                        if ((itemRenderStyles != null) && !backColor.IsEmpty)
                        {
                            lParam.clrTextBk = ColorTranslator.ToWin32(backColor);
                            flag = true;
                        }
                        if (flag)
                        {
                            Marshal.StructureToPtr(lParam, m.LParam, false);
                        }
                        if ((itemRenderStyles == null) || (itemRenderStyles.Font == null))
                        {
                            break;
                        }
                        System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(lParam.nmcd, lParam.nmcd.hdc), new HandleRef(itemRenderStyles, itemRenderStyles.FontHandle));
                        m.Result = (IntPtr) 2;
                        return;
                    }
                    m.Result = (IntPtr) 4;
                    return;

                case 0x10002:
                    if (this.drawMode != TreeViewDrawMode.OwnerDrawText)
                    {
                        break;
                    }
                    node = this.NodeFromHandle(lParam.nmcd.dwItemSpec);
                    if (node != null)
                    {
                        using (Graphics graphics2 = Graphics.FromHdcInternal(lParam.nmcd.hdc))
                        {
                            Rectangle bounds = node.Bounds;
                            Size size = TextRenderer.MeasureText(node.Text, node.TreeView.Font);
                            Point location = new Point(bounds.X - 1, bounds.Y);
                            bounds = new Rectangle(location, new Size(size.Width, bounds.Height));
                            DrawTreeNodeEventArgs e = new DrawTreeNodeEventArgs(graphics2, node, bounds, (TreeNodeStates) lParam.nmcd.uItemState);
                            this.OnDrawNode(e);
                            if (e.DrawDefault)
                            {
                                TreeNodeStates state = e.State;
                                Font font = (node.NodeFont != null) ? node.NodeFont : node.TreeView.Font;
                                Color color3 = (((state & TreeNodeStates.Selected) == TreeNodeStates.Selected) && node.TreeView.Focused) ? SystemColors.HighlightText : ((node.ForeColor != Color.Empty) ? node.ForeColor : node.TreeView.ForeColor);
                                if ((state & TreeNodeStates.Selected) == TreeNodeStates.Selected)
                                {
                                    graphics2.FillRectangle(SystemBrushes.Highlight, bounds);
                                    ControlPaint.DrawFocusRectangle(graphics2, bounds, color3, SystemColors.Highlight);
                                    TextRenderer.DrawText(graphics2, e.Node.Text, font, bounds, color3, TextFormatFlags.Default);
                                }
                                else
                                {
                                    using (Brush brush = new SolidBrush(this.BackColor))
                                    {
                                        graphics2.FillRectangle(brush, bounds);
                                    }
                                    TextRenderer.DrawText(graphics2, e.Node.Text, font, bounds, color3, TextFormatFlags.Default);
                                }
                            }
                        }
                        m.Result = (IntPtr) 0x20;
                        return;
                    }
                    return;

                case 1:
                    m.Result = (IntPtr) 0x20;
                    return;
            }
            m.Result = IntPtr.Zero;
        }

        private void DetachImageList(object sender, EventArgs e)
        {
            this.ImageList = null;
        }

        private void DetachImageListHandlers()
        {
            if (this.imageList != null)
            {
                this.imageList.RecreateHandle -= new EventHandler(this.ImageListRecreateHandle);
                this.imageList.Disposed -= new EventHandler(this.DetachImageList);
                this.imageList.ChangeHandle -= new EventHandler(this.ImageListChangedHandle);
            }
        }

        private void DetachStateImageList(object sender, EventArgs e)
        {
            this.internalStateImageList = null;
            this.StateImageList = null;
        }

        private void DetachStateImageListHandlers()
        {
            if (this.stateImageList != null)
            {
                this.stateImageList.RecreateHandle -= new EventHandler(this.StateImageListRecreateHandle);
                this.stateImageList.Disposed -= new EventHandler(this.DetachStateImageList);
                this.stateImageList.ChangeHandle -= new EventHandler(this.StateImageListChangedHandle);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (TreeNode node in this.Nodes)
                {
                    node.ContextMenu = null;
                }
                lock (this)
                {
                    this.DetachImageListHandlers();
                    this.imageList = null;
                    this.DetachStateImageListHandlers();
                    this.stateImageList = null;
                }
            }
            base.Dispose(disposing);
        }

        public void EndUpdate()
        {
            base.EndUpdateInternal();
        }

        public void ExpandAll()
        {
            this.root.ExpandAll();
        }

        internal void ForceScrollbarUpdate(bool delayed)
        {
            if (!base.IsUpdating() && base.IsHandleCreated)
            {
                base.SendMessage(11, 0, 0);
                if (delayed)
                {
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.Handle), 11, (IntPtr) 1, IntPtr.Zero);
                }
                else
                {
                    base.SendMessage(11, 1, 0);
                }
            }
        }

        protected OwnerDrawPropertyBag GetItemRenderStyles(TreeNode node, int state)
        {
            OwnerDrawPropertyBag bag = new OwnerDrawPropertyBag();
            if ((node != null) && (node.propBag != null))
            {
                if ((state & 0x47) == 0)
                {
                    bag.ForeColor = node.propBag.ForeColor;
                    bag.BackColor = node.propBag.BackColor;
                }
                bag.Font = node.propBag.Font;
            }
            return bag;
        }

        public TreeNode GetNodeAt(Point pt)
        {
            return this.GetNodeAt(pt.X, pt.Y);
        }

        public TreeNode GetNodeAt(int x, int y)
        {
            System.Windows.Forms.NativeMethods.TV_HITTESTINFO lParam = new System.Windows.Forms.NativeMethods.TV_HITTESTINFO {
                pt_x = x,
                pt_y = y
            };
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1111, 0, lParam);
            if (!(handle == IntPtr.Zero))
            {
                return this.NodeFromHandle(handle);
            }
            return null;
        }

        public int GetNodeCount(bool includeSubTrees)
        {
            return this.root.GetNodeCount(includeSubTrees);
        }

        public TreeViewHitTestInfo HitTest(Point pt)
        {
            return this.HitTest(pt.X, pt.Y);
        }

        public TreeViewHitTestInfo HitTest(int x, int y)
        {
            System.Windows.Forms.NativeMethods.TV_HITTESTINFO lParam = new System.Windows.Forms.NativeMethods.TV_HITTESTINFO {
                pt_x = x,
                pt_y = y
            };
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1111, 0, lParam);
            TreeNode hitNode = (handle == IntPtr.Zero) ? null : this.NodeFromHandle(handle);
            return new TreeViewHitTestInfo(hitNode, (TreeViewHitTestLocations) lParam.flags);
        }

        private void ImageListChangedHandle(object sender, EventArgs e)
        {
            if (((sender != null) && (sender == this.imageList)) && base.IsHandleCreated)
            {
                this.BeginUpdate();
                foreach (TreeNode node in this.Nodes)
                {
                    this.UpdateImagesRecursive(node);
                }
                this.EndUpdate();
            }
        }

        private void ImageListRecreateHandle(object sender, EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                IntPtr lparam = (this.ImageList == null) ? IntPtr.Zero : this.ImageList.Handle;
                base.SendMessage(0x1109, 0, lparam);
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if ((this.editNode != null) && ((keyData & Keys.Alt) == Keys.None))
            {
                switch ((keyData & Keys.KeyCode))
                {
                    case Keys.PageUp:
                    case Keys.Next:
                    case Keys.End:
                    case Keys.Home:
                    case Keys.Enter:
                    case Keys.Escape:
                        return true;
                }
            }
            return base.IsInputKey(keyData);
        }

        internal TreeNode NodeFromHandle(IntPtr handle)
        {
            return (TreeNode) this.nodeTable[handle];
        }

        protected virtual void OnAfterCheck(TreeViewEventArgs e)
        {
            if (this.onAfterCheck != null)
            {
                this.onAfterCheck(this, e);
            }
        }

        protected internal virtual void OnAfterCollapse(TreeViewEventArgs e)
        {
            if (this.onAfterCollapse != null)
            {
                this.onAfterCollapse(this, e);
            }
        }

        protected virtual void OnAfterExpand(TreeViewEventArgs e)
        {
            if (this.onAfterExpand != null)
            {
                this.onAfterExpand(this, e);
            }
        }

        protected virtual void OnAfterLabelEdit(NodeLabelEditEventArgs e)
        {
            if (this.onAfterLabelEdit != null)
            {
                this.onAfterLabelEdit(this, e);
            }
        }

        protected virtual void OnAfterSelect(TreeViewEventArgs e)
        {
            if (this.onAfterSelect != null)
            {
                this.onAfterSelect(this, e);
            }
        }

        protected virtual void OnBeforeCheck(TreeViewCancelEventArgs e)
        {
            if (this.onBeforeCheck != null)
            {
                this.onBeforeCheck(this, e);
            }
        }

        protected internal virtual void OnBeforeCollapse(TreeViewCancelEventArgs e)
        {
            if (this.onBeforeCollapse != null)
            {
                this.onBeforeCollapse(this, e);
            }
        }

        protected virtual void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            if (this.onBeforeExpand != null)
            {
                this.onBeforeExpand(this, e);
            }
        }

        protected virtual void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
        {
            if (this.onBeforeLabelEdit != null)
            {
                this.onBeforeLabelEdit(this, e);
            }
        }

        protected virtual void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            if (this.onBeforeSelect != null)
            {
                this.onBeforeSelect(this, e);
            }
        }

        protected virtual void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            if (this.onDrawNode != null)
            {
                this.onDrawNode(this, e);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            TreeNode selectedNode = this.selectedNode;
            this.selectedNode = null;
            base.OnHandleCreated(e);
            int num = (int) ((long) base.SendMessage(0x2008, 0, 0));
            if (num < 5)
            {
                base.SendMessage(0x2007, 5, 0);
            }
            if (this.CheckBoxes)
            {
                int windowLong = (int) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.Handle), -16);
                windowLong |= 0x100;
                System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, base.Handle), -16, new HandleRef(null, (IntPtr) windowLong));
            }
            if (this.ShowNodeToolTips && !base.DesignMode)
            {
                int num3 = (int) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.Handle), -16);
                num3 |= 0x800;
                System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, base.Handle), -16, new HandleRef(null, (IntPtr) num3));
            }
            Color backColor = this.BackColor;
            if (backColor != SystemColors.Window)
            {
                base.SendMessage(0x111d, 0, ColorTranslator.ToWin32(backColor));
            }
            backColor = this.ForeColor;
            if (backColor != SystemColors.WindowText)
            {
                base.SendMessage(0x111e, 0, ColorTranslator.ToWin32(backColor));
            }
            if (this.lineColor != Color.Empty)
            {
                base.SendMessage(0x1128, 0, ColorTranslator.ToWin32(this.lineColor));
            }
            if (this.imageList != null)
            {
                base.SendMessage(0x1109, 0, this.imageList.Handle);
            }
            if ((this.stateImageList != null) && (this.stateImageList.Images.Count > 0))
            {
                Image[] images = new Image[this.stateImageList.Images.Count + 1];
                images[0] = this.stateImageList.Images[0];
                for (int i = 1; i <= this.stateImageList.Images.Count; i++)
                {
                    images[i] = this.stateImageList.Images[i - 1];
                }
                this.internalStateImageList = new System.Windows.Forms.ImageList();
                this.internalStateImageList.Images.AddRange(images);
                base.SendMessage(0x1109, 2, this.internalStateImageList.Handle);
            }
            if (this.indent != -1)
            {
                base.SendMessage(0x1107, this.indent, 0);
            }
            if (this.itemHeight != -1)
            {
                base.SendMessage(0x111b, this.ItemHeight, 0);
            }
            int cx = 0;
            try
            {
                this.treeViewState[0x8000] = true;
                cx = base.Width;
                int flags = 0x16;
                System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.NullHandleRef, base.Left, base.Top, 0x7fffffff, base.Height, flags);
                this.root.Realize(false);
                if (cx != 0)
                {
                    System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.NullHandleRef, base.Left, base.Top, cx, base.Height, flags);
                }
            }
            finally
            {
                this.treeViewState[0x8000] = false;
            }
            this.SelectedNode = selectedNode;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.selectedNode = this.SelectedNode;
            base.OnHandleDestroyed(e);
        }

        protected virtual void OnItemDrag(ItemDragEventArgs e)
        {
            if (this.onItemDrag != null)
            {
                this.onItemDrag(this, e);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled && (this.CheckBoxes && ((e.KeyData & Keys.KeyCode) == Keys.Space)))
            {
                TreeNode selectedNode = this.SelectedNode;
                if (selectedNode != null)
                {
                    if (!this.TreeViewBeforeCheck(selectedNode, TreeViewAction.ByKeyboard))
                    {
                        selectedNode.CheckedInternal = !selectedNode.CheckedInternal;
                        this.TreeViewAfterCheck(selectedNode, TreeViewAction.ByKeyboard);
                    }
                    e.Handled = true;
                }
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (!e.Handled && (e.KeyChar == ' '))
            {
                e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (!e.Handled && ((e.KeyData & Keys.KeyCode) == Keys.Space))
            {
                e.Handled = true;
            }
        }

        protected override void OnMouseHover(EventArgs e)
        {
            System.Windows.Forms.NativeMethods.TV_HITTESTINFO lParam = new System.Windows.Forms.NativeMethods.TV_HITTESTINFO();
            Point position = Cursor.Position;
            position = base.PointToClientInternal(position);
            lParam.pt_x = position.X;
            lParam.pt_y = position.Y;
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1111, 0, lParam);
            if ((handle != IntPtr.Zero) && ((lParam.flags & 70) != 0))
            {
                TreeNode node = this.NodeFromHandle(handle);
                if ((node != this.prevHoveredNode) && (node != null))
                {
                    this.OnNodeMouseHover(new TreeNodeMouseHoverEventArgs(node));
                    this.prevHoveredNode = node;
                }
            }
            if (!this.hoveredAlready)
            {
                base.OnMouseHover(e);
                this.hoveredAlready = true;
            }
            base.ResetMouseEventArgs();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.hoveredAlready = false;
            base.OnMouseLeave(e);
        }

        protected virtual void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            if (this.onNodeMouseClick != null)
            {
                this.onNodeMouseClick(this, e);
            }
        }

        protected virtual void OnNodeMouseDoubleClick(TreeNodeMouseClickEventArgs e)
        {
            if (this.onNodeMouseDoubleClick != null)
            {
                this.onNodeMouseDoubleClick(this, e);
            }
        }

        protected virtual void OnNodeMouseHover(TreeNodeMouseHoverEventArgs e)
        {
            if (this.onNodeMouseHover != null)
            {
                this.onNodeMouseHover(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnRightToLeftLayoutChanged(EventArgs e)
        {
            if (!base.GetAnyDisposingInHierarchy())
            {
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    base.RecreateHandle();
                }
                if (this.onRightToLeftLayoutChanged != null)
                {
                    this.onRightToLeftLayoutChanged(this, e);
                }
            }
        }

        private void RefreshNodes()
        {
            TreeNode[] dest = new TreeNode[this.Nodes.Count];
            this.Nodes.CopyTo(dest, 0);
            this.Nodes.Clear();
            this.Nodes.AddRange(dest);
        }

        private void ResetIndent()
        {
            this.indent = -1;
            base.RecreateHandle();
        }

        private void ResetItemHeight()
        {
            this.itemHeight = -1;
            base.RecreateHandle();
        }

        internal void SetToolTip(System.Windows.Forms.ToolTip toolTip, string toolTipText)
        {
            if (toolTip != null)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(toolTip, toolTip.Handle), 0x418, 0, SystemInformation.MaxWindowTrackSize.Width);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1118, new HandleRef(toolTip, toolTip.Handle), 0);
                this.controlToolTipText = toolTipText;
            }
        }

        private bool ShouldSerializeImageIndex()
        {
            if (this.imageList != null)
            {
                return (this.ImageIndex != 0);
            }
            return (this.ImageIndex != -1);
        }

        private bool ShouldSerializeIndent()
        {
            return (this.indent != -1);
        }

        private bool ShouldSerializeItemHeight()
        {
            return (this.itemHeight != -1);
        }

        private bool ShouldSerializeSelectedImageIndex()
        {
            if (this.imageList != null)
            {
                return (this.SelectedImageIndex != 0);
            }
            return (this.SelectedImageIndex != -1);
        }

        private void ShowContextMenu(TreeNode treeNode)
        {
            if ((treeNode.ContextMenu != null) || (treeNode.ContextMenuStrip != null))
            {
                ContextMenu contextMenu = treeNode.ContextMenu;
                ContextMenuStrip contextMenuStrip = treeNode.ContextMenuStrip;
                if (contextMenu != null)
                {
                    System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT();
                    System.Windows.Forms.UnsafeNativeMethods.GetCursorPos(pt);
                    System.Windows.Forms.UnsafeNativeMethods.SetForegroundWindow(new HandleRef(this, base.Handle));
                    contextMenu.OnPopup(EventArgs.Empty);
                    System.Windows.Forms.SafeNativeMethods.TrackPopupMenuEx(new HandleRef(contextMenu, contextMenu.Handle), 0x40, pt.x, pt.y, new HandleRef(this, base.Handle), null);
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.Handle), 0, IntPtr.Zero, IntPtr.Zero);
                }
                else if (contextMenuStrip != null)
                {
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.Handle), 0x110b, 8, treeNode.Handle);
                    contextMenuStrip.ShowInternal(this, base.PointToClient(Control.MousePosition), false);
                    contextMenuStrip.Closing += new ToolStripDropDownClosingEventHandler(this.ContextMenuStripClosing);
                }
            }
        }

        public void Sort()
        {
            this.Sorted = true;
            this.RefreshNodes();
        }

        private void StateImageListChangedHandle(object sender, EventArgs e)
        {
            if (((sender != null) && (sender == this.stateImageList)) && base.IsHandleCreated)
            {
                if ((this.stateImageList != null) && (this.stateImageList.Images.Count > 0))
                {
                    Image[] images = new Image[this.stateImageList.Images.Count + 1];
                    images[0] = this.stateImageList.Images[0];
                    for (int i = 1; i <= this.stateImageList.Images.Count; i++)
                    {
                        images[i] = this.stateImageList.Images[i - 1];
                    }
                    if (this.internalStateImageList != null)
                    {
                        this.internalStateImageList.Images.Clear();
                        this.internalStateImageList.Images.AddRange(images);
                    }
                    else
                    {
                        this.internalStateImageList = new System.Windows.Forms.ImageList();
                        this.internalStateImageList.Images.AddRange(images);
                    }
                    if (this.internalStateImageList != null)
                    {
                        base.SendMessage(0x1109, 2, this.internalStateImageList.Handle);
                    }
                }
                else
                {
                    this.UpdateCheckedState(this.root, true);
                }
            }
        }

        private void StateImageListRecreateHandle(object sender, EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                IntPtr zero = IntPtr.Zero;
                if (this.internalStateImageList != null)
                {
                    zero = this.internalStateImageList.Handle;
                }
                base.SendMessage(0x1109, 2, zero);
            }
        }

        public override string ToString()
        {
            string str = base.ToString();
            if (this.Nodes != null)
            {
                str = str + ", Nodes.Count: " + this.Nodes.Count.ToString(CultureInfo.CurrentCulture);
                if (this.Nodes.Count > 0)
                {
                    str = str + ", Nodes[0]: " + this.Nodes[0].ToString();
                }
            }
            return str;
        }

        internal void TreeViewAfterCheck(TreeNode node, TreeViewAction actionTaken)
        {
            this.OnAfterCheck(new TreeViewEventArgs(node, actionTaken));
        }

        internal bool TreeViewBeforeCheck(TreeNode node, TreeViewAction actionTaken)
        {
            TreeViewCancelEventArgs e = new TreeViewCancelEventArgs(node, false, actionTaken);
            this.OnBeforeCheck(e);
            return e.Cancel;
        }

        private unsafe void TvnBeginDrag(MouseButtons buttons, System.Windows.Forms.NativeMethods.NMTREEVIEW* nmtv)
        {
            System.Windows.Forms.NativeMethods.TV_ITEM itemNew = nmtv.itemNew;
            if (itemNew.hItem != IntPtr.Zero)
            {
                TreeNode item = this.NodeFromHandle(itemNew.hItem);
                this.OnItemDrag(new ItemDragEventArgs(buttons, item));
            }
        }

        private IntPtr TvnBeginLabelEdit(System.Windows.Forms.NativeMethods.NMTVDISPINFO nmtvdi)
        {
            if (nmtvdi.item.hItem == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            TreeNode node = this.NodeFromHandle(nmtvdi.item.hItem);
            NodeLabelEditEventArgs e = new NodeLabelEditEventArgs(node);
            this.OnBeforeLabelEdit(e);
            if (!e.CancelEdit)
            {
                this.editNode = node;
            }
            return (e.CancelEdit ? ((IntPtr) 1) : IntPtr.Zero);
        }

        private IntPtr TvnEndLabelEdit(System.Windows.Forms.NativeMethods.NMTVDISPINFO nmtvdi)
        {
            this.editNode = null;
            if (nmtvdi.item.hItem == IntPtr.Zero)
            {
                return (IntPtr) 1;
            }
            TreeNode node = this.NodeFromHandle(nmtvdi.item.hItem);
            string label = (nmtvdi.item.pszText == IntPtr.Zero) ? null : Marshal.PtrToStringAuto(nmtvdi.item.pszText);
            NodeLabelEditEventArgs e = new NodeLabelEditEventArgs(node, label);
            this.OnAfterLabelEdit(e);
            if (((label != null) && !e.CancelEdit) && (node != null))
            {
                node.text = label;
                if (this.Scrollable)
                {
                    this.ForceScrollbarUpdate(true);
                }
            }
            return (e.CancelEdit ? IntPtr.Zero : ((IntPtr) 1));
        }

        private unsafe void TvnExpanded(System.Windows.Forms.NativeMethods.NMTREEVIEW* nmtv)
        {
            System.Windows.Forms.NativeMethods.TV_ITEM itemNew = nmtv.itemNew;
            if (itemNew.hItem != IntPtr.Zero)
            {
                TreeViewEventArgs args;
                TreeNode node = this.NodeFromHandle(itemNew.hItem);
                if ((itemNew.state & 0x20) == 0)
                {
                    args = new TreeViewEventArgs(node, TreeViewAction.Collapse);
                    this.OnAfterCollapse(args);
                }
                else
                {
                    args = new TreeViewEventArgs(node, TreeViewAction.Expand);
                    this.OnAfterExpand(args);
                }
            }
        }

        private unsafe IntPtr TvnExpanding(System.Windows.Forms.NativeMethods.NMTREEVIEW* nmtv)
        {
            System.Windows.Forms.NativeMethods.TV_ITEM itemNew = nmtv.itemNew;
            if (itemNew.hItem == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            TreeViewCancelEventArgs e = null;
            if ((itemNew.state & 0x20) == 0)
            {
                e = new TreeViewCancelEventArgs(this.NodeFromHandle(itemNew.hItem), false, TreeViewAction.Expand);
                this.OnBeforeExpand(e);
            }
            else
            {
                e = new TreeViewCancelEventArgs(this.NodeFromHandle(itemNew.hItem), false, TreeViewAction.Collapse);
                this.OnBeforeCollapse(e);
            }
            return (e.Cancel ? ((IntPtr) 1) : IntPtr.Zero);
        }

        private unsafe void TvnSelected(System.Windows.Forms.NativeMethods.NMTREEVIEW* nmtv)
        {
            if (!this.nodesCollectionClear)
            {
                if (nmtv.itemNew.hItem != IntPtr.Zero)
                {
                    TreeViewAction unknown = TreeViewAction.Unknown;
                    switch (nmtv.action)
                    {
                        case 1:
                            unknown = TreeViewAction.ByMouse;
                            break;

                        case 2:
                            unknown = TreeViewAction.ByKeyboard;
                            break;
                    }
                    this.OnAfterSelect(new TreeViewEventArgs(this.NodeFromHandle(nmtv.itemNew.hItem), unknown));
                }
                System.Windows.Forms.NativeMethods.RECT lParam = new System.Windows.Forms.NativeMethods.RECT();
                (IntPtr) &lParam.left = nmtv.itemOld.hItem;
                if ((nmtv.itemOld.hItem != IntPtr.Zero) && (((int) ((long) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1104, 1, ref lParam))) != 0))
                {
                    System.Windows.Forms.SafeNativeMethods.InvalidateRect(new HandleRef(this, base.Handle), ref lParam, true);
                }
            }
        }

        private unsafe IntPtr TvnSelecting(System.Windows.Forms.NativeMethods.NMTREEVIEW* nmtv)
        {
            if (this.treeViewState[0x10000])
            {
                return (IntPtr) 1;
            }
            if (nmtv.itemNew.hItem == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            TreeNode node = this.NodeFromHandle(nmtv.itemNew.hItem);
            TreeViewAction unknown = TreeViewAction.Unknown;
            switch (nmtv.action)
            {
                case 1:
                    unknown = TreeViewAction.ByMouse;
                    break;

                case 2:
                    unknown = TreeViewAction.ByKeyboard;
                    break;
            }
            TreeViewCancelEventArgs e = new TreeViewCancelEventArgs(node, false, unknown);
            this.OnBeforeSelect(e);
            return (e.Cancel ? ((IntPtr) 1) : IntPtr.Zero);
        }

        private void UpdateCheckedState(TreeNode node, bool update)
        {
            if (update)
            {
                node.CheckedInternal = node.CheckedInternal;
                for (int i = node.Nodes.Count - 1; i >= 0; i--)
                {
                    this.UpdateCheckedState(node.Nodes[i], update);
                }
            }
            else
            {
                node.CheckedInternal = false;
                for (int j = node.Nodes.Count - 1; j >= 0; j--)
                {
                    this.UpdateCheckedState(node.Nodes[j], update);
                }
            }
        }

        private void UpdateImagesRecursive(TreeNode node)
        {
            node.UpdateImage();
            foreach (TreeNode node2 in node.Nodes)
            {
                this.UpdateImagesRecursive(node2);
            }
        }

        internal override void UpdateStylesCore()
        {
            base.UpdateStylesCore();
            if ((base.IsHandleCreated && this.CheckBoxes) && ((this.StateImageList != null) && (this.internalStateImageList != null)))
            {
                base.SendMessage(0x1109, 2, this.internalStateImageList.Handle);
            }
        }

        private void WmMouseDown(ref Message m, MouseButtons button, int clicks)
        {
            base.SendMessage(0x110b, 8, (string) null);
            this.OnMouseDown(new MouseEventArgs(button, clicks, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
            if (!base.ValidationCancelled)
            {
                this.DefWndProc(ref m);
            }
        }

        private void WmNeedText(ref Message m)
        {
            System.Windows.Forms.NativeMethods.TOOLTIPTEXT lParam = (System.Windows.Forms.NativeMethods.TOOLTIPTEXT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.TOOLTIPTEXT));
            string controlToolTipText = this.controlToolTipText;
            System.Windows.Forms.NativeMethods.TV_HITTESTINFO tv_hittestinfo = new System.Windows.Forms.NativeMethods.TV_HITTESTINFO();
            Point position = Cursor.Position;
            position = base.PointToClientInternal(position);
            tv_hittestinfo.pt_x = position.X;
            tv_hittestinfo.pt_y = position.Y;
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1111, 0, tv_hittestinfo);
            if ((handle != IntPtr.Zero) && ((tv_hittestinfo.flags & 70) != 0))
            {
                TreeNode node = this.NodeFromHandle(handle);
                if ((this.ShowNodeToolTips && (node != null)) && !string.IsNullOrEmpty(node.ToolTipText))
                {
                    controlToolTipText = node.ToolTipText;
                }
                else if ((node != null) && (node.Bounds.Right > base.Bounds.Right))
                {
                    controlToolTipText = node.Text;
                }
                else
                {
                    controlToolTipText = null;
                }
            }
            lParam.lpszText = controlToolTipText;
            lParam.hinst = IntPtr.Zero;
            if (this.RightToLeft == RightToLeft.Yes)
            {
                lParam.uFlags |= 4;
            }
            Marshal.StructureToPtr(lParam, m.LParam, false);
        }

        private unsafe void WmNotify(ref Message m)
        {
            System.Windows.Forms.NativeMethods.NMHDR* lParam = (System.Windows.Forms.NativeMethods.NMHDR*) m.LParam;
            if (lParam->code == -12)
            {
                this.CustomDraw(ref m);
            }
            else
            {
                System.Windows.Forms.NativeMethods.NMTREEVIEW* nmtv = (System.Windows.Forms.NativeMethods.NMTREEVIEW*) m.LParam;
                switch (nmtv->nmhdr.code)
                {
                    case -5:
                    case -2:
                    {
                        MouseButtons left = MouseButtons.Left;
                        System.Windows.Forms.NativeMethods.TV_HITTESTINFO tv_hittestinfo = new System.Windows.Forms.NativeMethods.TV_HITTESTINFO();
                        Point position = Cursor.Position;
                        position = base.PointToClientInternal(position);
                        tv_hittestinfo.pt_x = position.X;
                        tv_hittestinfo.pt_y = position.Y;
                        IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1111, 0, tv_hittestinfo);
                        if ((nmtv->nmhdr.code != -2) || ((tv_hittestinfo.flags & 70) != 0))
                        {
                            left = (nmtv->nmhdr.code == -2) ? MouseButtons.Left : MouseButtons.Right;
                        }
                        if ((((nmtv->nmhdr.code != -2) || ((tv_hittestinfo.flags & 70) != 0)) || this.FullRowSelect) && ((handle != IntPtr.Zero) && !base.ValidationCancelled))
                        {
                            this.OnNodeMouseClick(new TreeNodeMouseClickEventArgs(this.NodeFromHandle(handle), left, 1, position.X, position.Y));
                            this.OnClick(new MouseEventArgs(left, 1, position.X, position.Y, 0));
                            this.OnMouseClick(new MouseEventArgs(left, 1, position.X, position.Y, 0));
                        }
                        if (nmtv->nmhdr.code == -5)
                        {
                            TreeNode treeNode = this.NodeFromHandle(handle);
                            if ((treeNode != null) && ((treeNode.ContextMenu != null) || (treeNode.ContextMenuStrip != null)))
                            {
                                this.ShowContextMenu(treeNode);
                            }
                            else
                            {
                                this.treeViewState[0x2000] = true;
                                base.SendMessage(0x7b, base.Handle, System.Windows.Forms.SafeNativeMethods.GetMessagePos());
                            }
                            m.Result = (IntPtr) 1;
                        }
                        if (!this.treeViewState[0x1000] && ((nmtv->nmhdr.code != -2) || ((tv_hittestinfo.flags & 70) != 0)))
                        {
                            this.OnMouseUp(new MouseEventArgs(left, 1, position.X, position.Y, 0));
                            this.treeViewState[0x1000] = true;
                        }
                        break;
                    }
                    case -460:
                    case -411:
                        m.Result = this.TvnEndLabelEdit((System.Windows.Forms.NativeMethods.NMTVDISPINFO) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMTVDISPINFO)));
                        return;

                    case -459:
                    case -410:
                        m.Result = this.TvnBeginLabelEdit((System.Windows.Forms.NativeMethods.NMTVDISPINFO) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMTVDISPINFO)));
                        return;

                    case -458:
                    case -453:
                    case -452:
                    case -409:
                    case -404:
                    case -403:
                        break;

                    case -457:
                    case -408:
                        this.TvnBeginDrag(MouseButtons.Right, nmtv);
                        return;

                    case -456:
                    case -407:
                        this.TvnBeginDrag(MouseButtons.Left, nmtv);
                        return;

                    case -455:
                    case -406:
                        this.TvnExpanded(nmtv);
                        return;

                    case -454:
                    case -405:
                        m.Result = this.TvnExpanding(nmtv);
                        return;

                    case -451:
                    case -402:
                        this.TvnSelected(nmtv);
                        return;

                    case -450:
                    case -401:
                        m.Result = this.TvnSelecting(nmtv);
                        return;

                    default:
                        return;
                }
            }
        }

        private void WmPrint(ref Message m)
        {
            base.WndProc(ref m);
            if ((((2 & ((int) m.LParam)) != 0) && Application.RenderWithVisualStyles) && (this.BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D))
            {
                System.Windows.Forms.IntSecurity.UnmanagedCode.Assert();
                try
                {
                    using (Graphics graphics = Graphics.FromHdc(m.WParam))
                    {
                        Rectangle rect = new Rectangle(0, 0, base.Size.Width - 1, base.Size.Height - 1);
                        graphics.DrawRectangle(new Pen(VisualStyleInformation.TextControlBorder), rect);
                        rect.Inflate(-1, -1);
                        graphics.DrawRectangle(SystemPens.Window, rect);
                    }
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }

        private unsafe bool WmShowToolTip(ref Message m)
        {
            System.Windows.Forms.NativeMethods.NMHDR* lParam = (System.Windows.Forms.NativeMethods.NMHDR*) m.LParam;
            IntPtr hwndFrom = lParam->hwndFrom;
            System.Windows.Forms.NativeMethods.TV_HITTESTINFO tv_hittestinfo = new System.Windows.Forms.NativeMethods.TV_HITTESTINFO();
            Point position = Cursor.Position;
            position = base.PointToClientInternal(position);
            tv_hittestinfo.pt_x = position.X;
            tv_hittestinfo.pt_y = position.Y;
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1111, 0, tv_hittestinfo);
            if ((handle != IntPtr.Zero) && ((tv_hittestinfo.flags & 70) != 0))
            {
                TreeNode node = this.NodeFromHandle(handle);
                if ((node != null) && !this.ShowNodeToolTips)
                {
                    Rectangle bounds = node.Bounds;
                    bounds.Location = base.PointToScreen(bounds.Location);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, hwndFrom), 0x41f, 1, ref bounds);
                    System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, hwndFrom), System.Windows.Forms.NativeMethods.HWND_TOPMOST, bounds.Left, bounds.Top, 0, 0, 0x15);
                    return true;
                }
            }
            return false;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x4e:
                {
                    System.Windows.Forms.NativeMethods.NMHDR lParam = (System.Windows.Forms.NativeMethods.NMHDR) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHDR));
                    switch (lParam.code)
                    {
                        case -521:
                            if (!this.WmShowToolTip(ref m))
                            {
                                base.WndProc(ref m);
                                return;
                            }
                            m.Result = (IntPtr) 1;
                            return;

                        case -520:
                        case -530:
                            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(lParam, lParam.hwndFrom), 0x418, 0, SystemInformation.MaxWindowTrackSize.Width);
                            this.WmNeedText(ref m);
                            m.Result = (IntPtr) 1;
                            return;
                    }
                    base.WndProc(ref m);
                    return;
                }
                case 0x7b:
                {
                    if (this.treeViewState[0x2000])
                    {
                        this.treeViewState[0x2000] = false;
                        base.WndProc(ref m);
                        return;
                    }
                    TreeNode selectedNode = this.SelectedNode;
                    if ((selectedNode == null) || ((selectedNode.ContextMenu == null) && (selectedNode.ContextMenuStrip == null)))
                    {
                        base.WndProc(ref m);
                        return;
                    }
                    Point pt = new Point(selectedNode.Bounds.X, selectedNode.Bounds.Y + (selectedNode.Bounds.Height / 2));
                    if (base.ClientRectangle.Contains(pt))
                    {
                        if (selectedNode.ContextMenu != null)
                        {
                            selectedNode.ContextMenu.Show(this, pt);
                            return;
                        }
                        if (selectedNode.ContextMenuStrip == null)
                        {
                            return;
                        }
                        bool isKeyboardActivated = ((int) ((long) m.LParam)) == -1;
                        selectedNode.ContextMenuStrip.ShowInternal(this, pt, isKeyboardActivated);
                    }
                    return;
                }
                case 0x83:
                case 5:
                case 70:
                case 0x47:
                    if (this.treeViewState[0x8000])
                    {
                        this.DefWndProc(ref m);
                        return;
                    }
                    base.WndProc(ref m);
                    return;

                case 7:
                    if (!this.treeViewState[0x4000])
                    {
                        base.WndProc(ref m);
                        return;
                    }
                    this.treeViewState[0x4000] = false;
                    base.WmImeSetFocus();
                    this.DefWndProc(ref m);
                    this.OnGotFocus(EventArgs.Empty);
                    return;

                case 0x15:
                    base.SendMessage(0x1107, this.Indent, 0);
                    base.WndProc(ref m);
                    return;

                case 0x201:
                {
                    try
                    {
                        this.treeViewState[0x10000] = true;
                        this.FocusInternal();
                    }
                    finally
                    {
                        this.treeViewState[0x10000] = false;
                    }
                    this.treeViewState[0x1000] = false;
                    System.Windows.Forms.NativeMethods.TV_HITTESTINFO tv_hittestinfo = new System.Windows.Forms.NativeMethods.TV_HITTESTINFO {
                        pt_x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam),
                        pt_y = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam)
                    };
                    this.hNodeMouseDown = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1111, 0, tv_hittestinfo);
                    if ((tv_hittestinfo.flags & 0x40) != 0)
                    {
                        this.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                        if (!base.ValidationCancelled && this.CheckBoxes)
                        {
                            TreeNode node = this.NodeFromHandle(this.hNodeMouseDown);
                            if (!this.TreeViewBeforeCheck(node, TreeViewAction.ByMouse) && (node != null))
                            {
                                node.CheckedInternal = !node.CheckedInternal;
                                this.TreeViewAfterCheck(node, TreeViewAction.ByMouse);
                            }
                        }
                        m.Result = IntPtr.Zero;
                    }
                    else
                    {
                        this.WmMouseDown(ref m, MouseButtons.Left, 1);
                    }
                    this.downButton = MouseButtons.Left;
                    return;
                }
                case 0x202:
                case 0x205:
                {
                    System.Windows.Forms.NativeMethods.TV_HITTESTINFO tv_hittestinfo2 = new System.Windows.Forms.NativeMethods.TV_HITTESTINFO {
                        pt_x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam),
                        pt_y = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam)
                    };
                    IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1111, 0, tv_hittestinfo2);
                    if (handle != IntPtr.Zero)
                    {
                        if (!base.ValidationCancelled && (!this.treeViewState[0x800] & !this.treeViewState[0x1000]))
                        {
                            if (handle == this.hNodeMouseDown)
                            {
                                this.OnNodeMouseClick(new TreeNodeMouseClickEventArgs(this.NodeFromHandle(handle), this.downButton, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam)));
                            }
                            this.OnClick(new MouseEventArgs(this.downButton, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                            this.OnMouseClick(new MouseEventArgs(this.downButton, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                        }
                        if (this.treeViewState[0x800])
                        {
                            this.treeViewState[0x800] = false;
                            if (!base.ValidationCancelled)
                            {
                                this.OnNodeMouseDoubleClick(new TreeNodeMouseClickEventArgs(this.NodeFromHandle(handle), this.downButton, 2, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam)));
                                this.OnDoubleClick(new MouseEventArgs(this.downButton, 2, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                                this.OnMouseDoubleClick(new MouseEventArgs(this.downButton, 2, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                            }
                        }
                    }
                    if (!this.treeViewState[0x1000])
                    {
                        this.OnMouseUp(new MouseEventArgs(this.downButton, 1, System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam), System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam), 0));
                    }
                    this.treeViewState[0x800] = false;
                    this.treeViewState[0x1000] = false;
                    base.CaptureInternal = false;
                    this.hNodeMouseDown = IntPtr.Zero;
                    return;
                }
                case 0x203:
                    this.WmMouseDown(ref m, MouseButtons.Left, 2);
                    this.treeViewState[0x800] = true;
                    this.treeViewState[0x1000] = false;
                    base.CaptureInternal = true;
                    return;

                case 0x204:
                {
                    this.treeViewState[0x1000] = false;
                    System.Windows.Forms.NativeMethods.TV_HITTESTINFO tv_hittestinfo3 = new System.Windows.Forms.NativeMethods.TV_HITTESTINFO {
                        pt_x = System.Windows.Forms.NativeMethods.Util.SignedLOWORD(m.LParam),
                        pt_y = System.Windows.Forms.NativeMethods.Util.SignedHIWORD(m.LParam)
                    };
                    this.hNodeMouseDown = System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x1111, 0, tv_hittestinfo3);
                    this.WmMouseDown(ref m, MouseButtons.Right, 1);
                    this.downButton = MouseButtons.Right;
                    return;
                }
                case 0x206:
                    this.WmMouseDown(ref m, MouseButtons.Right, 2);
                    this.treeViewState[0x800] = true;
                    this.treeViewState[0x1000] = false;
                    base.CaptureInternal = true;
                    return;

                case 0x207:
                    this.treeViewState[0x1000] = false;
                    this.WmMouseDown(ref m, MouseButtons.Middle, 1);
                    this.downButton = MouseButtons.Middle;
                    return;

                case 0x209:
                    this.treeViewState[0x1000] = false;
                    this.WmMouseDown(ref m, MouseButtons.Middle, 2);
                    return;

                case 0x2a3:
                    this.prevHoveredNode = null;
                    base.WndProc(ref m);
                    return;

                case 0x114:
                    base.WndProc(ref m);
                    if (this.DrawMode == TreeViewDrawMode.OwnerDrawAll)
                    {
                        base.Invalidate();
                    }
                    return;

                case 0x113f:
                case 0x110d:
                    base.WndProc(ref m);
                    if (this.CheckBoxes)
                    {
                        System.Windows.Forms.NativeMethods.TV_ITEM tv_item = (System.Windows.Forms.NativeMethods.TV_ITEM) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.TV_ITEM));
                        if (!(tv_item.hItem != IntPtr.Zero))
                        {
                            return;
                        }
                        System.Windows.Forms.NativeMethods.TV_ITEM tv_item2 = new System.Windows.Forms.NativeMethods.TV_ITEM {
                            mask = 0x18,
                            hItem = tv_item.hItem,
                            stateMask = 0xf000
                        };
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(null, base.Handle), System.Windows.Forms.NativeMethods.TVM_GETITEM, 0, ref tv_item2);
                        this.NodeFromHandle(tv_item.hItem).CheckedStateInternal = (tv_item2.state >> 12) > 1;
                    }
                    return;

                case 0x204e:
                    this.WmNotify(ref m);
                    return;

                case 0x317:
                    this.WmPrint(ref m);
                    return;
            }
            base.WndProc(ref m);
        }

        public override Color BackColor
        {
            get
            {
                if (this.ShouldSerializeBackColor())
                {
                    return base.BackColor;
                }
                return SystemColors.Window;
            }
            set
            {
                base.BackColor = value;
                if (base.IsHandleCreated)
                {
                    base.SendMessage(0x111d, 0, ColorTranslator.ToWin32(this.BackColor));
                    base.SendMessage(0x1107, this.Indent, 0);
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRDescription("borderStyleDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(2), DispId(-504)]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return this.borderStyle;
            }
            set
            {
                if (this.borderStyle != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.BorderStyle));
                    }
                    this.borderStyle = value;
                    base.UpdateStyles();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(false), System.Windows.Forms.SRDescription("TreeViewCheckBoxesDescr")]
        public bool CheckBoxes
        {
            get
            {
                return this.treeViewState[8];
            }
            set
            {
                if (this.CheckBoxes != value)
                {
                    this.treeViewState[8] = value;
                    if (base.IsHandleCreated)
                    {
                        if (this.CheckBoxes)
                        {
                            base.UpdateStyles();
                        }
                        else
                        {
                            this.UpdateCheckedState(this.root, false);
                            base.RecreateHandle();
                        }
                    }
                }
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "SysTreeView32";
                if (base.IsHandleCreated)
                {
                    int windowLong = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.Handle), -16));
                    createParams.Style |= windowLong & 0x300000;
                }
                switch (this.borderStyle)
                {
                    case System.Windows.Forms.BorderStyle.FixedSingle:
                        createParams.Style |= 0x800000;
                        break;

                    case System.Windows.Forms.BorderStyle.Fixed3D:
                        createParams.ExStyle |= 0x200;
                        break;
                }
                if (!this.Scrollable)
                {
                    createParams.Style |= 0x2000;
                }
                if (!this.HideSelection)
                {
                    createParams.Style |= 0x20;
                }
                if (this.LabelEdit)
                {
                    createParams.Style |= 8;
                }
                if (this.ShowLines)
                {
                    createParams.Style |= 2;
                }
                if (this.ShowPlusMinus)
                {
                    createParams.Style |= 1;
                }
                if (this.ShowRootLines)
                {
                    createParams.Style |= 4;
                }
                if (this.HotTracking)
                {
                    createParams.Style |= 0x200;
                }
                if (this.FullRowSelect)
                {
                    createParams.Style |= 0x1000;
                }
                if (this.setOddHeight)
                {
                    createParams.Style |= 0x4000;
                }
                if ((this.ShowNodeToolTips && base.IsHandleCreated) && !base.DesignMode)
                {
                    createParams.Style |= 0x800;
                }
                if (this.CheckBoxes && base.IsHandleCreated)
                {
                    createParams.Style |= 0x100;
                }
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    if (this.RightToLeftLayout)
                    {
                        createParams.ExStyle |= 0x400000;
                        createParams.ExStyle &= -28673;
                        return createParams;
                    }
                    createParams.Style |= 0x40;
                }
                return createParams;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(0x79, 0x61);
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

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0), System.Windows.Forms.SRDescription("TreeViewDrawModeDescr")]
        public TreeViewDrawMode DrawMode
        {
            get
            {
                return this.drawMode;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(TreeViewDrawMode));
                }
                if (this.drawMode != value)
                {
                    this.drawMode = value;
                    base.Invalidate();
                    if (this.DrawMode == TreeViewDrawMode.OwnerDrawAll)
                    {
                        base.SetStyle(ControlStyles.ResizeRedraw, true);
                    }
                }
            }
        }

        public override Color ForeColor
        {
            get
            {
                if (this.ShouldSerializeForeColor())
                {
                    return base.ForeColor;
                }
                return SystemColors.WindowText;
            }
            set
            {
                base.ForeColor = value;
                if (base.IsHandleCreated)
                {
                    base.SendMessage(0x111e, 0, ColorTranslator.ToWin32(this.ForeColor));
                }
            }
        }

        [System.Windows.Forms.SRDescription("TreeViewFullRowSelectDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool FullRowSelect
        {
            get
            {
                return this.treeViewState[0x200];
            }
            set
            {
                if (this.FullRowSelect != value)
                {
                    this.treeViewState[0x200] = value;
                    if (base.IsHandleCreated)
                    {
                        base.UpdateStyles();
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewHideSelectionDescr"), DefaultValue(true)]
        public bool HideSelection
        {
            get
            {
                return this.treeViewState[1];
            }
            set
            {
                if (this.HideSelection != value)
                {
                    this.treeViewState[1] = value;
                    if (base.IsHandleCreated)
                    {
                        base.UpdateStyles();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("TreeViewHotTrackingDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool HotTracking
        {
            get
            {
                return this.treeViewState[0x100];
            }
            set
            {
                if (this.HotTracking != value)
                {
                    this.treeViewState[0x100] = value;
                    if (base.IsHandleCreated)
                    {
                        base.UpdateStyles();
                    }
                }
            }
        }

        [Localizable(true), DefaultValue(-1), System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.Repaint), TypeConverter(typeof(NoneExcludedImageIndexConverter)), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRDescription("TreeViewImageIndexDescr"), RelatedImageList("ImageList")]
        public int ImageIndex
        {
            get
            {
                if (this.imageList == null)
                {
                    return -1;
                }
                if (this.ImageIndexer.Index >= this.imageList.Images.Count)
                {
                    return Math.Max(0, this.imageList.Images.Count - 1);
                }
                return this.ImageIndexer.Index;
            }
            set
            {
                if (value == -1)
                {
                    value = 0;
                }
                if (value < 0)
                {
                    object[] args = new object[] { "ImageIndex", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("ImageIndex", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this.ImageIndexer.Index != value)
                {
                    this.ImageIndexer.Index = value;
                    if (base.IsHandleCreated)
                    {
                        base.RecreateHandle();
                    }
                }
            }
        }

        internal System.Windows.Forms.ImageList.Indexer ImageIndexer
        {
            get
            {
                if (this.imageIndexer == null)
                {
                    this.imageIndexer = new System.Windows.Forms.ImageList.Indexer();
                }
                this.imageIndexer.ImageList = this.ImageList;
                return this.imageIndexer;
            }
        }

        [System.Windows.Forms.SRDescription("TreeViewImageKeyDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), TypeConverter(typeof(ImageKeyConverter)), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), RefreshProperties(RefreshProperties.Repaint), RelatedImageList("ImageList")]
        public string ImageKey
        {
            get
            {
                return this.ImageIndexer.Key;
            }
            set
            {
                if (this.ImageIndexer.Key != value)
                {
                    this.ImageIndexer.Key = value;
                    if (string.IsNullOrEmpty(value) || value.Equals(System.Windows.Forms.SR.GetString("toStringNone")))
                    {
                        this.ImageIndex = (this.ImageList != null) ? 0 : -1;
                    }
                    if (base.IsHandleCreated)
                    {
                        base.RecreateHandle();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("TreeViewImageListDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue((string) null), RefreshProperties(RefreshProperties.Repaint)]
        public System.Windows.Forms.ImageList ImageList
        {
            get
            {
                return this.imageList;
            }
            set
            {
                if (value != this.imageList)
                {
                    this.DetachImageListHandlers();
                    this.imageList = value;
                    this.AttachImageListHandlers();
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x1109, 0, (value == null) ? IntPtr.Zero : value.Handle);
                        if ((this.StateImageList != null) && (this.StateImageList.Images.Count > 0))
                        {
                            base.SendMessage(0x1109, 2, this.internalStateImageList.Handle);
                        }
                    }
                    this.UpdateCheckedState(this.root, true);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), System.Windows.Forms.SRDescription("TreeViewIndentDescr")]
        public int Indent
        {
            get
            {
                if (this.indent != -1)
                {
                    return this.indent;
                }
                if (base.IsHandleCreated)
                {
                    return (int) ((long) base.SendMessage(0x1106, 0, 0));
                }
                return 0x13;
            }
            set
            {
                if (this.indent != value)
                {
                    if (value < 0)
                    {
                        object[] args = new object[] { "Indent", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("Indent", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                    }
                    if (value > MaxIndent)
                    {
                        throw new ArgumentOutOfRangeException("Indent", System.Windows.Forms.SR.GetString("InvalidHighBoundArgumentEx", new object[] { "Indent", value.ToString(CultureInfo.CurrentCulture), MaxIndent.ToString(CultureInfo.CurrentCulture) }));
                    }
                    this.indent = value;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x1107, value, 0);
                        this.indent = (int) ((long) base.SendMessage(0x1106, 0, 0));
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("TreeViewItemHeightDescr")]
        public int ItemHeight
        {
            get
            {
                if (this.itemHeight != -1)
                {
                    return this.itemHeight;
                }
                if (base.IsHandleCreated)
                {
                    return (int) ((long) base.SendMessage(0x111c, 0, 0));
                }
                if (this.CheckBoxes && (this.DrawMode == TreeViewDrawMode.OwnerDrawAll))
                {
                    return Math.Max(0x10, base.FontHeight + 3);
                }
                return (base.FontHeight + 3);
            }
            set
            {
                if (this.itemHeight != value)
                {
                    if (value < 1)
                    {
                        object[] args = new object[] { "ItemHeight", value.ToString(CultureInfo.CurrentCulture), 1.ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("ItemHeight", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                    }
                    if (value >= 0x7fff)
                    {
                        object[] objArray2 = new object[] { "ItemHeight", value.ToString(CultureInfo.CurrentCulture), ((short) 0x7fff).ToString(CultureInfo.CurrentCulture) };
                        throw new ArgumentOutOfRangeException("ItemHeight", System.Windows.Forms.SR.GetString("InvalidHighBoundArgument", objArray2));
                    }
                    this.itemHeight = value;
                    if (base.IsHandleCreated)
                    {
                        if ((this.itemHeight % 2) != 0)
                        {
                            this.setOddHeight = true;
                            try
                            {
                                base.RecreateHandle();
                            }
                            finally
                            {
                                this.setOddHeight = false;
                            }
                        }
                        base.SendMessage(0x111b, value, 0);
                        this.itemHeight = (int) ((long) base.SendMessage(0x111c, 0, 0));
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("TreeViewLabelEditDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool LabelEdit
        {
            get
            {
                return this.treeViewState[2];
            }
            set
            {
                if (this.LabelEdit != value)
                {
                    this.treeViewState[2] = value;
                    if (base.IsHandleCreated)
                    {
                        base.UpdateStyles();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("TreeViewLineColorDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(typeof(Color), "Black")]
        public Color LineColor
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    int num = (int) ((long) base.SendMessage(0x1129, 0, 0));
                    return ColorTranslator.FromWin32(num);
                }
                return this.lineColor;
            }
            set
            {
                if (this.lineColor != value)
                {
                    this.lineColor = value;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0x1128, 0, ColorTranslator.ToWin32(this.lineColor));
                    }
                }
            }
        }

        [Localizable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewNodesDescr"), MergableProperty(false)]
        public TreeNodeCollection Nodes
        {
            get
            {
                if (this.nodes == null)
                {
                    this.nodes = new TreeNodeCollection(this.root);
                }
                return this.nodes;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [DefaultValue(@"\"), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewPathSeparatorDescr")]
        public string PathSeparator
        {
            get
            {
                return this.pathSeparator;
            }
            set
            {
                this.pathSeparator = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), DefaultValue(false), System.Windows.Forms.SRDescription("ControlRightToLeftLayoutDescr")]
        public virtual bool RightToLeftLayout
        {
            get
            {
                return this.rightToLeftLayout;
            }
            set
            {
                if (value != this.rightToLeftLayout)
                {
                    this.rightToLeftLayout = value;
                    using (new LayoutTransaction(this, this, PropertyNames.RightToLeftLayout))
                    {
                        this.OnRightToLeftLayoutChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("TreeViewScrollableDescr")]
        public bool Scrollable
        {
            get
            {
                return this.treeViewState[4];
            }
            set
            {
                if (this.Scrollable != value)
                {
                    this.treeViewState[4] = value;
                    base.RecreateHandle();
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), TypeConverter(typeof(NoneExcludedImageIndexConverter)), DefaultValue(-1), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRDescription("TreeViewSelectedImageIndexDescr"), RelatedImageList("ImageList")]
        public int SelectedImageIndex
        {
            get
            {
                if (this.imageList == null)
                {
                    return -1;
                }
                if (this.SelectedImageIndexer.Index >= this.imageList.Images.Count)
                {
                    return Math.Max(0, this.imageList.Images.Count - 1);
                }
                return this.SelectedImageIndexer.Index;
            }
            set
            {
                if (value == -1)
                {
                    value = 0;
                }
                if (value < 0)
                {
                    object[] args = new object[] { "SelectedImageIndex", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("SelectedImageIndex", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                if (this.SelectedImageIndexer.Index != value)
                {
                    this.SelectedImageIndexer.Index = value;
                    if (base.IsHandleCreated)
                    {
                        base.RecreateHandle();
                    }
                }
            }
        }

        internal System.Windows.Forms.ImageList.Indexer SelectedImageIndexer
        {
            get
            {
                if (this.selectedImageIndexer == null)
                {
                    this.selectedImageIndexer = new System.Windows.Forms.ImageList.Indexer();
                }
                this.selectedImageIndexer.ImageList = this.ImageList;
                return this.selectedImageIndexer;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), TypeConverter(typeof(ImageKeyConverter)), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("TreeViewSelectedImageKeyDescr"), RelatedImageList("ImageList")]
        public string SelectedImageKey
        {
            get
            {
                return this.SelectedImageIndexer.Key;
            }
            set
            {
                if (this.SelectedImageIndexer.Key != value)
                {
                    this.SelectedImageIndexer.Key = value;
                    if (string.IsNullOrEmpty(value) || value.Equals(System.Windows.Forms.SR.GetString("toStringNone")))
                    {
                        this.SelectedImageIndex = (this.ImageList != null) ? 0 : -1;
                    }
                    if (base.IsHandleCreated)
                    {
                        base.RecreateHandle();
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("TreeViewSelectedNodeDescr")]
        public TreeNode SelectedNode
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    IntPtr handle = base.SendMessage(0x110a, 9, 0);
                    if (handle == IntPtr.Zero)
                    {
                        return null;
                    }
                    return this.NodeFromHandle(handle);
                }
                if ((this.selectedNode != null) && (this.selectedNode.TreeView == this))
                {
                    return this.selectedNode;
                }
                return null;
            }
            set
            {
                if (base.IsHandleCreated && ((value == null) || (value.TreeView == this)))
                {
                    IntPtr lparam = (value == null) ? IntPtr.Zero : value.Handle;
                    base.SendMessage(0x110b, 9, lparam);
                    this.selectedNode = null;
                }
                else
                {
                    this.selectedNode = value;
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("TreeViewShowLinesDescr")]
        public bool ShowLines
        {
            get
            {
                return this.treeViewState[0x10];
            }
            set
            {
                if (this.ShowLines != value)
                {
                    this.treeViewState[0x10] = value;
                    if (base.IsHandleCreated)
                    {
                        base.UpdateStyles();
                    }
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewShowShowNodeToolTipsDescr")]
        public bool ShowNodeToolTips
        {
            get
            {
                return this.treeViewState[0x400];
            }
            set
            {
                if (this.ShowNodeToolTips != value)
                {
                    this.treeViewState[0x400] = value;
                    if (this.ShowNodeToolTips)
                    {
                        base.RecreateHandle();
                    }
                }
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewShowPlusMinusDescr")]
        public bool ShowPlusMinus
        {
            get
            {
                return this.treeViewState[0x20];
            }
            set
            {
                if (this.ShowPlusMinus != value)
                {
                    this.treeViewState[0x20] = value;
                    if (base.IsHandleCreated)
                    {
                        base.UpdateStyles();
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("TreeViewShowRootLinesDescr")]
        public bool ShowRootLines
        {
            get
            {
                return this.treeViewState[0x40];
            }
            set
            {
                if (this.ShowRootLines != value)
                {
                    this.treeViewState[0x40] = value;
                    if (base.IsHandleCreated)
                    {
                        base.UpdateStyles();
                    }
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("TreeViewSortedDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool Sorted
        {
            get
            {
                return this.treeViewState[0x80];
            }
            set
            {
                if (this.Sorted != value)
                {
                    this.treeViewState[0x80] = value;
                    if ((this.Sorted && (this.TreeViewNodeSorter == null)) && (this.Nodes.Count >= 1))
                    {
                        this.RefreshNodes();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("TreeViewStateImageListDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue((string) null)]
        public System.Windows.Forms.ImageList StateImageList
        {
            get
            {
                return this.stateImageList;
            }
            set
            {
                if (value != this.stateImageList)
                {
                    this.DetachStateImageListHandlers();
                    this.stateImageList = value;
                    if ((this.stateImageList != null) && (this.stateImageList.Images.Count > 0))
                    {
                        Image[] images = new Image[this.stateImageList.Images.Count + 1];
                        images[0] = this.stateImageList.Images[0];
                        for (int i = 1; i <= this.stateImageList.Images.Count; i++)
                        {
                            images[i] = this.stateImageList.Images[i - 1];
                        }
                        this.internalStateImageList = new System.Windows.Forms.ImageList();
                        this.internalStateImageList.Images.AddRange(images);
                    }
                    this.AttachStateImageListHandlers();
                    if (base.IsHandleCreated)
                    {
                        if (((this.stateImageList != null) && (this.stateImageList.Images.Count > 0)) && (this.internalStateImageList != null))
                        {
                            base.SendMessage(0x1109, 2, this.internalStateImageList.Handle);
                        }
                        this.UpdateCheckedState(this.root, true);
                        if (((value == null) || (this.stateImageList.Images.Count == 0)) && this.CheckBoxes)
                        {
                            base.RecreateHandle();
                        }
                        else
                        {
                            this.RefreshNodes();
                        }
                    }
                }
            }
        }

        [Bindable(false), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("TreeViewTopNodeDescr")]
        public TreeNode TopNode
        {
            get
            {
                if (!base.IsHandleCreated)
                {
                    return this.topNode;
                }
                IntPtr handle = base.SendMessage(0x110a, 5, 0);
                if (!(handle == IntPtr.Zero))
                {
                    return this.NodeFromHandle(handle);
                }
                return null;
            }
            set
            {
                if (base.IsHandleCreated && ((value == null) || (value.TreeView == this)))
                {
                    IntPtr lparam = (value == null) ? IntPtr.Zero : value.Handle;
                    base.SendMessage(0x110b, 5, lparam);
                    this.topNode = null;
                }
                else
                {
                    this.topNode = value;
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("TreeViewNodeSorterDescr")]
        public IComparer TreeViewNodeSorter
        {
            get
            {
                return this.treeViewNodeSorter;
            }
            set
            {
                if (this.treeViewNodeSorter != value)
                {
                    this.treeViewNodeSorter = value;
                    if (value != null)
                    {
                        this.Sort();
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("TreeViewVisibleCountDescr")]
        public int VisibleCount
        {
            get
            {
                if (base.IsHandleCreated)
                {
                    return (int) ((long) base.SendMessage(0x1110, 0, 0));
                }
                return 0;
            }
        }
    }
}

