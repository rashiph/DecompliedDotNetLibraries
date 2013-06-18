namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    [DesignTimeVisible(false), ToolboxItem(false), Designer("System.Windows.Forms.Design.ToolStripItemDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("Click"), DefaultProperty("Text")]
    public abstract class ToolStripItem : Component, IDropTarget, ISupportOleDropSource, IArrangedElement, IComponent, IDisposable
    {
        private ToolStripItemAlignment alignment;
        private Rectangle bounds;
        private System.Drawing.Size cachedTextSize;
        private ToolStripItemDisplayStyle displayStyle;
        private static readonly ArrangedElementCollection EmptyChildCollection = new ArrangedElementCollection();
        internal static readonly object EventAvailableChanged = new object();
        internal static readonly object EventBackColorChanged = new object();
        internal static readonly object EventClick = new object();
        internal static readonly object EventDisplayStyleChanged = new object();
        internal static readonly object EventDoubleClick = new object();
        internal static readonly object EventDragDrop = new object();
        internal static readonly object EventDragEnter = new object();
        internal static readonly object EventDragLeave = new object();
        internal static readonly object EventDragOver = new object();
        internal static readonly object EventEnabledChanged = new object();
        internal static readonly object EventFontChanged = new object();
        internal static readonly object EventForeColorChanged = new object();
        internal static readonly object EventGiveFeedback = new object();
        internal static readonly object EventInternalEnabledChanged = new object();
        internal static readonly object EventLayout = new object();
        internal static readonly object EventLocationChanged = new object();
        internal static readonly object EventMouseDown = new object();
        internal static readonly object EventMouseEnter = new object();
        internal static readonly object EventMouseHover = new object();
        internal static readonly object EventMouseLeave = new object();
        internal static readonly object EventMouseMove = new object();
        internal static readonly object EventMouseUp = new object();
        internal static readonly object EventMouseWheel = new object();
        internal static readonly object EventMove = new object();
        internal static readonly object EventOwnerChanged = new object();
        internal static readonly object EventPaint = new object();
        internal static readonly object EventQueryAccessibilityHelp = new object();
        internal static readonly object EventQueryContinueDrag = new object();
        internal static readonly object EventResize = new object();
        internal static readonly object EventRightToLeft = new object();
        internal static readonly object EventSelectedChanged = new object();
        internal static readonly object EventText = new object();
        internal static readonly object EventVisibleChanged = new object();
        private ContentAlignment imageAlign;
        private ToolStripItemImageIndexer imageIndexer;
        private ToolStripItemImageScaling imageScaling;
        private Color imageTransparentColor;
        private long lastClickTime;
        internal static readonly TraceSwitch MouseDebugging;
        private ToolStripItemOverflow overflow;
        private ToolStrip owner;
        private ToolStrip parent;
        private ToolStripItemPlacement placement;
        private static readonly int PropAccessibility = PropertyStore.CreateKey();
        private static readonly int PropAccessibleDefaultActionDescription = PropertyStore.CreateKey();
        private static readonly int PropAccessibleDescription = PropertyStore.CreateKey();
        private static readonly int PropAccessibleHelpProvider = PropertyStore.CreateKey();
        private static readonly int PropAccessibleName = PropertyStore.CreateKey();
        private static readonly int PropAccessibleRole = PropertyStore.CreateKey();
        private static readonly int PropBackColor = PropertyStore.CreateKey();
        private static readonly int PropBackgroundImage = PropertyStore.CreateKey();
        private static readonly int PropBackgroundImageLayout = PropertyStore.CreateKey();
        private PropertyStore propertyStore;
        private static readonly int PropFont = PropertyStore.CreateKey();
        private static readonly int PropForeColor = PropertyStore.CreateKey();
        private static readonly int PropImage = PropertyStore.CreateKey();
        private static readonly int PropMergeAction = PropertyStore.CreateKey();
        private static readonly int PropMergeIndex = PropertyStore.CreateKey();
        private static readonly int PropMirroredImage = PropertyStore.CreateKey();
        private static readonly int PropName = PropertyStore.CreateKey();
        private static readonly int PropRightToLeft = PropertyStore.CreateKey();
        private static readonly int PropTag = PropertyStore.CreateKey();
        private static readonly int PropText = PropertyStore.CreateKey();
        private static readonly int PropTextDirection = PropertyStore.CreateKey();
        private BitVector32 state;
        private static readonly int stateAllowDrop = BitVector32.CreateMask();
        private static readonly int stateAutoSize = BitVector32.CreateMask(stateMouseDownAndNoDrag);
        private static readonly int stateAutoToolTip = BitVector32.CreateMask(stateDoubleClickEnabled);
        private static readonly int stateContstructing = BitVector32.CreateMask(stateSelected);
        private static readonly int stateCurrentlyAnimatingImage = BitVector32.CreateMask(stateDisposed);
        private static readonly int stateDisposed = BitVector32.CreateMask(stateContstructing);
        private static readonly int stateDisposing = BitVector32.CreateMask(stateUseAmbientMargin);
        private static readonly int stateDoubleClickEnabled = BitVector32.CreateMask(stateCurrentlyAnimatingImage);
        private static readonly int stateEnabled = BitVector32.CreateMask(stateVisible);
        private static readonly int stateInvalidMirroredImage = BitVector32.CreateMask(stateRightToLeftAutoMirrorImage);
        private static readonly int stateMouseDownAndNoDrag = BitVector32.CreateMask(stateEnabled);
        private static readonly int stateMouseDownAndUpMustBeInSameItem = BitVector32.CreateMask(stateSupportsSpaceKey);
        private static readonly int statePressed = BitVector32.CreateMask(stateAutoSize);
        private static readonly int stateRightToLeftAutoMirrorImage = BitVector32.CreateMask(stateSupportsItemClick);
        private static readonly int stateSelected = BitVector32.CreateMask(statePressed);
        private static readonly int stateSupportsDisabledHotTracking = BitVector32.CreateMask(stateMouseDownAndUpMustBeInSameItem);
        private static readonly int stateSupportsItemClick = BitVector32.CreateMask(stateSupportsRightClick);
        private static readonly int stateSupportsRightClick = BitVector32.CreateMask(stateAutoToolTip);
        private static readonly int stateSupportsSpaceKey = BitVector32.CreateMask(stateInvalidMirroredImage);
        private static readonly int stateUseAmbientMargin = BitVector32.CreateMask(stateSupportsDisabledHotTracking);
        private static readonly int stateVisible = BitVector32.CreateMask(stateAllowDrop);
        private ContentAlignment textAlign;
        private System.Windows.Forms.TextImageRelation textImageRelation;
        private ToolStripItemInternalLayout toolStripItemInternalLayout;
        private string toolTipText;

        [System.Windows.Forms.SRDescription("ToolStripItemOnAvailableChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged"), Browsable(false)]
        public event EventHandler AvailableChanged
        {
            add
            {
                base.Events.AddHandler(EventAvailableChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventAvailableChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemOnBackColorChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler BackColorChanged
        {
            add
            {
                base.Events.AddHandler(EventBackColorChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventBackColorChanged, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("ToolStripItemOnClickDescr")]
        public event EventHandler Click
        {
            add
            {
                base.Events.AddHandler(EventClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventClick, value);
            }
        }

        public event EventHandler DisplayStyleChanged
        {
            add
            {
                base.Events.AddHandler(EventDisplayStyleChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDisplayStyleChanged, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("ControlOnDoubleClickDescr")]
        public event EventHandler DoubleClick
        {
            add
            {
                base.Events.AddHandler(EventDoubleClick, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDoubleClick, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemOnDragDropDescr"), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), System.Windows.Forms.SRCategory("CatDragDrop")]
        public event DragEventHandler DragDrop
        {
            add
            {
                base.Events.AddHandler(EventDragDrop, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragDrop, value);
            }
        }

        [Browsable(false), System.Windows.Forms.SRCategory("CatDragDrop"), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ToolStripItemOnDragEnterDescr")]
        public event DragEventHandler DragEnter
        {
            add
            {
                base.Events.AddHandler(EventDragEnter, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragEnter, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatDragDrop"), System.Windows.Forms.SRDescription("ToolStripItemOnDragLeaveDescr"), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public event EventHandler DragLeave
        {
            add
            {
                base.Events.AddHandler(EventDragLeave, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragLeave, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ToolStripItemOnDragOverDescr"), System.Windows.Forms.SRCategory("CatDragDrop")]
        public event DragEventHandler DragOver
        {
            add
            {
                base.Events.AddHandler(EventDragOver, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDragOver, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemEnabledChangedDescr")]
        public event EventHandler EnabledChanged
        {
            add
            {
                base.Events.AddHandler(EventEnabledChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventEnabledChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemOnForeColorChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler ForeColorChanged
        {
            add
            {
                base.Events.AddHandler(EventForeColorChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventForeColorChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemOnGiveFeedbackDescr"), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatDragDrop"), Browsable(false)]
        public event GiveFeedbackEventHandler GiveFeedback
        {
            add
            {
                base.Events.AddHandler(EventGiveFeedback, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventGiveFeedback, value);
            }
        }

        internal event EventHandler InternalEnabledChanged
        {
            add
            {
                base.Events.AddHandler(EventInternalEnabledChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventInternalEnabledChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemOnLocationChangedDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public event EventHandler LocationChanged
        {
            add
            {
                base.Events.AddHandler(EventLocationChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLocationChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemOnMouseDownDescr"), System.Windows.Forms.SRCategory("CatMouse")]
        public event MouseEventHandler MouseDown
        {
            add
            {
                base.Events.AddHandler(EventMouseDown, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseDown, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatMouse"), System.Windows.Forms.SRDescription("ToolStripItemOnMouseEnterDescr")]
        public event EventHandler MouseEnter
        {
            add
            {
                base.Events.AddHandler(EventMouseEnter, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseEnter, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemOnMouseHoverDescr"), System.Windows.Forms.SRCategory("CatMouse")]
        public event EventHandler MouseHover
        {
            add
            {
                base.Events.AddHandler(EventMouseHover, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseHover, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemOnMouseLeaveDescr"), System.Windows.Forms.SRCategory("CatMouse")]
        public event EventHandler MouseLeave
        {
            add
            {
                base.Events.AddHandler(EventMouseLeave, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseLeave, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatMouse"), System.Windows.Forms.SRDescription("ToolStripItemOnMouseMoveDescr")]
        public event MouseEventHandler MouseMove
        {
            add
            {
                base.Events.AddHandler(EventMouseMove, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseMove, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatMouse"), System.Windows.Forms.SRDescription("ToolStripItemOnMouseUpDescr")]
        public event MouseEventHandler MouseUp
        {
            add
            {
                base.Events.AddHandler(EventMouseUp, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMouseUp, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemOwnerChangedDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler OwnerChanged
        {
            add
            {
                base.Events.AddHandler(EventOwnerChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventOwnerChanged, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripItemOnPaintDescr")]
        public event PaintEventHandler Paint
        {
            add
            {
                base.Events.AddHandler(EventPaint, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventPaint, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ToolStripItemOnQueryAccessibilityHelpDescr")]
        public event QueryAccessibilityHelpEventHandler QueryAccessibilityHelp
        {
            add
            {
                base.Events.AddHandler(EventQueryAccessibilityHelp, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventQueryAccessibilityHelp, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatDragDrop"), Browsable(false), System.Windows.Forms.SRDescription("ToolStripItemOnQueryContinueDragDescr"), EditorBrowsable(EditorBrowsableState.Advanced)]
        public event QueryContinueDragEventHandler QueryContinueDrag
        {
            add
            {
                base.Events.AddHandler(EventQueryContinueDrag, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventQueryContinueDrag, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ToolStripItemOnRightToLeftChangedDescr")]
        public event EventHandler RightToLeftChanged
        {
            add
            {
                base.Events.AddHandler(EventRightToLeft, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRightToLeft, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ToolStripItemOnTextChangedDescr")]
        public event EventHandler TextChanged
        {
            add
            {
                base.Events.AddHandler(EventText, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventText, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ToolStripItemOnVisibleChangedDescr")]
        public event EventHandler VisibleChanged
        {
            add
            {
                base.Events.AddHandler(EventVisibleChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventVisibleChanged, value);
            }
        }

        protected ToolStripItem()
        {
            this.bounds = Rectangle.Empty;
            this.overflow = ToolStripItemOverflow.AsNeeded;
            this.placement = ToolStripItemPlacement.None;
            this.imageAlign = ContentAlignment.MiddleCenter;
            this.textAlign = ContentAlignment.MiddleCenter;
            this.textImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.state = new BitVector32();
            this.imageTransparentColor = Color.Empty;
            this.imageScaling = ToolStripItemImageScaling.SizeToFit;
            this.cachedTextSize = System.Drawing.Size.Empty;
            this.displayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.state[((((((stateEnabled | stateAutoSize) | stateVisible) | stateContstructing) | stateSupportsItemClick) | stateInvalidMirroredImage) | stateMouseDownAndUpMustBeInSameItem) | stateUseAmbientMargin] = true;
            this.state[(((((((stateAllowDrop | stateMouseDownAndNoDrag) | stateSupportsRightClick) | statePressed) | stateSelected) | stateDisposed) | stateDoubleClickEnabled) | stateRightToLeftAutoMirrorImage) | stateSupportsSpaceKey] = false;
            this.SetAmbientMargin();
            this.Size = this.DefaultSize;
            this.DisplayStyle = this.DefaultDisplayStyle;
            CommonProperties.SetAutoSize(this, true);
            this.state[stateContstructing] = false;
            this.AutoToolTip = this.DefaultAutoToolTip;
        }

        protected ToolStripItem(string text, System.Drawing.Image image, EventHandler onClick) : this(text, image, onClick, null)
        {
        }

        protected ToolStripItem(string text, System.Drawing.Image image, EventHandler onClick, string name) : this()
        {
            this.Text = text;
            this.Image = image;
            if (onClick != null)
            {
                this.Click += onClick;
            }
            this.Name = name;
        }

        internal void AccessibilityNotifyClients(AccessibleEvents accEvent)
        {
            if (this.ParentInternal != null)
            {
                int index = this.ParentInternal.DisplayedItems.IndexOf(this);
                this.ParentInternal.AccessibilityNotifyClients(accEvent, index);
            }
        }

        private void Animate()
        {
            this.Animate(((!base.DesignMode && this.Visible) && this.Enabled) && (this.ParentInternal != null));
        }

        private void Animate(bool animate)
        {
            if (animate != this.state[stateCurrentlyAnimatingImage])
            {
                if (animate)
                {
                    if (this.Image != null)
                    {
                        ImageAnimator.Animate(this.Image, new EventHandler(this.OnAnimationFrameChanged));
                        this.state[stateCurrentlyAnimatingImage] = animate;
                    }
                }
                else if (this.Image != null)
                {
                    ImageAnimator.StopAnimate(this.Image, new EventHandler(this.OnAnimationFrameChanged));
                    this.state[stateCurrentlyAnimatingImage] = animate;
                }
            }
        }

        internal bool BeginDragForItemReorder()
        {
            if (((Control.ModifierKeys == Keys.Alt) && this.ParentInternal.Items.Contains(this)) && this.ParentInternal.AllowItemReorder)
            {
                ToolStripItem data = this;
                this.DoDragDrop(data, DragDropEffects.Move);
                return true;
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual AccessibleObject CreateAccessibilityInstance()
        {
            return new ToolStripItemAccessibleObject(this);
        }

        internal virtual ToolStripItemInternalLayout CreateInternalLayout()
        {
            return new ToolStripItemInternalLayout(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.state[stateDisposing] = true;
                if (this.Owner != null)
                {
                    this.StopAnimate();
                    this.Owner.Items.Remove(this);
                    this.toolStripItemInternalLayout = null;
                    this.state[stateDisposed] = true;
                }
            }
            base.Dispose(disposing);
            if (disposing)
            {
                this.Properties.SetObject(PropMirroredImage, null);
                this.Properties.SetObject(PropImage, null);
                this.state[stateDisposing] = false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), UIPermission(SecurityAction.Demand, Clipboard=UIPermissionClipboard.OwnClipboard)]
        public DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects)
        {
            int[] finalEffect = new int[1];
            System.Windows.Forms.UnsafeNativeMethods.IOleDropSource dropSource = this.DropSource;
            System.Runtime.InteropServices.ComTypes.IDataObject dataObject = null;
            dataObject = data as System.Runtime.InteropServices.ComTypes.IDataObject;
            if (dataObject == null)
            {
                DataObject obj3 = null;
                System.Windows.Forms.IDataObject obj4 = data as System.Windows.Forms.IDataObject;
                if (obj4 != null)
                {
                    obj3 = new DataObject(obj4);
                }
                else if (data is ToolStripItem)
                {
                    obj3 = new DataObject();
                    obj3.SetData(typeof(ToolStripItem).ToString(), data);
                }
                else
                {
                    obj3 = new DataObject();
                    obj3.SetData(data);
                }
                dataObject = obj3;
            }
            try
            {
                System.Windows.Forms.SafeNativeMethods.DoDragDrop(dataObject, dropSource, (int) allowedEffects, finalEffect);
            }
            catch
            {
            }
            return (DragDropEffects) finalEffect[0];
        }

        private void EnsureParentDropTargetRegistered()
        {
            if (this.ParentInternal != null)
            {
                System.Windows.Forms.IntSecurity.ClipboardRead.Demand();
                this.ParentInternal.DropTargetManager.EnsureRegistered(this);
            }
        }

        internal void FireEvent(ToolStripItemEventType met)
        {
            this.FireEvent(new EventArgs(), met);
        }

        internal void FireEvent(EventArgs e, ToolStripItemEventType met)
        {
            switch (met)
            {
                case ToolStripItemEventType.Paint:
                    this.HandlePaint(e as PaintEventArgs);
                    return;

                case ToolStripItemEventType.LocationChanged:
                    this.OnLocationChanged(e);
                    return;

                case ToolStripItemEventType.MouseMove:
                    if (this.Enabled || (this.ParentInternal == null))
                    {
                        this.FireEventInteractive(e, met);
                        return;
                    }
                    this.BeginDragForItemReorder();
                    return;

                case ToolStripItemEventType.MouseEnter:
                    this.HandleMouseEnter(e);
                    return;

                case ToolStripItemEventType.MouseLeave:
                    if (this.Enabled || (this.ParentInternal == null))
                    {
                        this.HandleMouseLeave(e);
                        return;
                    }
                    this.ParentInternal.UpdateToolTip(null);
                    return;

                case ToolStripItemEventType.MouseHover:
                    if ((this.Enabled || (this.ParentInternal == null)) || string.IsNullOrEmpty(this.ToolTipText))
                    {
                        this.FireEventInteractive(e, met);
                        return;
                    }
                    this.ParentInternal.UpdateToolTip(this);
                    return;
            }
            this.FireEventInteractive(e, met);
        }

        internal void FireEventInteractive(EventArgs e, ToolStripItemEventType met)
        {
            if (this.Enabled)
            {
                switch (met)
                {
                    case ToolStripItemEventType.MouseUp:
                        this.HandleMouseUp(e as MouseEventArgs);
                        return;

                    case ToolStripItemEventType.MouseDown:
                        this.HandleMouseDown(e as MouseEventArgs);
                        return;

                    case ToolStripItemEventType.MouseMove:
                        this.HandleMouseMove(e as MouseEventArgs);
                        return;

                    case ToolStripItemEventType.MouseEnter:
                    case ToolStripItemEventType.MouseLeave:
                        return;

                    case ToolStripItemEventType.MouseHover:
                        this.HandleMouseHover(e);
                        return;

                    case ToolStripItemEventType.Click:
                        this.HandleClick(e);
                        return;

                    case ToolStripItemEventType.DoubleClick:
                        this.HandleDoubleClick(e);
                        return;
                }
            }
        }

        public ToolStrip GetCurrentParent()
        {
            return this.Parent;
        }

        internal ToolStripDropDown GetCurrentParentDropDown()
        {
            if (this.ParentInternal != null)
            {
                return (this.ParentInternal as ToolStripDropDown);
            }
            return (this.Owner as ToolStripDropDown);
        }

        private System.Drawing.Font GetOwnerFont()
        {
            if (this.Owner != null)
            {
                return this.Owner.Font;
            }
            return null;
        }

        public virtual System.Drawing.Size GetPreferredSize(System.Drawing.Size constrainingSize)
        {
            constrainingSize = LayoutUtils.ConvertZeroToUnbounded(constrainingSize);
            return (this.InternalLayout.GetPreferredSize(constrainingSize - this.Padding.Size) + this.Padding.Size);
        }

        internal System.Drawing.Size GetTextSize()
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                return System.Drawing.Size.Empty;
            }
            if (this.cachedTextSize == System.Drawing.Size.Empty)
            {
                this.cachedTextSize = TextRenderer.MeasureText(this.Text, this.Font);
            }
            return this.cachedTextSize;
        }

        private void HandleClick(EventArgs e)
        {
            try
            {
                if (!base.DesignMode)
                {
                    this.state[statePressed] = true;
                }
                this.InvokePaint();
                if (this.SupportsItemClick && (this.Owner != null))
                {
                    this.Owner.HandleItemClick(this);
                }
                this.OnClick(e);
                if (this.SupportsItemClick && (this.Owner != null))
                {
                    this.Owner.HandleItemClicked(this);
                }
            }
            finally
            {
                this.state[statePressed] = false;
            }
            this.Invalidate();
        }

        private void HandleDoubleClick(EventArgs e)
        {
            this.OnDoubleClick(e);
        }

        private void HandleLeave()
        {
            if ((this.state[stateMouseDownAndNoDrag] || this.state[statePressed]) || this.state[stateSelected])
            {
                this.state[(stateMouseDownAndNoDrag | statePressed) | stateSelected] = false;
                this.Invalidate();
            }
        }

        private void HandleMouseDown(MouseEventArgs e)
        {
            this.state[stateMouseDownAndNoDrag] = !this.BeginDragForItemReorder();
            if (this.state[stateMouseDownAndNoDrag])
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.Push(true);
                }
                this.OnMouseDown(e);
                this.RaiseMouseEvent(EventMouseDown, e);
            }
        }

        private void HandleMouseEnter(EventArgs e)
        {
            if ((!base.DesignMode && (this.ParentInternal != null)) && (this.ParentInternal.CanHotTrack && this.ParentInternal.ShouldSelectItem()))
            {
                if (this.Enabled)
                {
                    bool menuAutoExpand = this.ParentInternal.MenuAutoExpand;
                    if ((this.ParentInternal.LastMouseDownedItem == this) && (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(1) < 0))
                    {
                        this.Push(true);
                    }
                    this.Select();
                    this.ParentInternal.MenuAutoExpand = menuAutoExpand;
                }
                else if (this.SupportsDisabledHotTracking)
                {
                    this.Select();
                }
            }
            if (this.Enabled)
            {
                this.OnMouseEnter(e);
                this.RaiseEvent(EventMouseEnter, e);
            }
        }

        private void HandleMouseHover(EventArgs e)
        {
            this.OnMouseHover(e);
            this.RaiseEvent(EventMouseHover, e);
        }

        private void HandleMouseLeave(EventArgs e)
        {
            this.HandleLeave();
            if (this.Enabled)
            {
                this.OnMouseLeave(e);
                this.RaiseEvent(EventMouseLeave, e);
            }
        }

        private void HandleMouseMove(MouseEventArgs mea)
        {
            if (((this.Enabled && this.CanSelect) && (!this.Selected && (this.ParentInternal != null))) && (this.ParentInternal.CanHotTrack && this.ParentInternal.ShouldSelectItem()))
            {
                this.Select();
            }
            this.OnMouseMove(mea);
            this.RaiseMouseEvent(EventMouseMove, mea);
        }

        private void HandleMouseUp(MouseEventArgs e)
        {
            bool flag = this.ParentInternal.LastMouseDownedItem == this;
            if (!flag && !this.MouseDownAndUpMustBeInSameItem)
            {
                flag = this.ParentInternal.ShouldSelectItem();
            }
            if (this.state[stateMouseDownAndNoDrag] || flag)
            {
                this.Push(false);
                if ((e.Button == MouseButtons.Left) || ((e.Button == MouseButtons.Right) && this.state[stateSupportsRightClick]))
                {
                    bool flag2 = false;
                    if (this.DoubleClickEnabled)
                    {
                        long ticks = DateTime.Now.Ticks;
                        long num2 = ticks - this.lastClickTime;
                        this.lastClickTime = ticks;
                        if ((num2 >= 0L) && (num2 < DoubleClickTicks))
                        {
                            flag2 = true;
                        }
                    }
                    if (flag2)
                    {
                        this.HandleDoubleClick(new EventArgs());
                        this.lastClickTime = 0L;
                    }
                    else
                    {
                        this.HandleClick(new EventArgs());
                    }
                }
                this.OnMouseUp(e);
                this.RaiseMouseEvent(EventMouseUp, e);
            }
        }

        private void HandlePaint(PaintEventArgs e)
        {
            this.Animate();
            ImageAnimator.UpdateFrames();
            this.OnPaint(e);
            this.RaisePaintEvent(EventPaint, e);
        }

        public void Invalidate()
        {
            if (this.ParentInternal != null)
            {
                this.ParentInternal.Invalidate(this.Bounds, true);
            }
        }

        public void Invalidate(Rectangle r)
        {
            Point location = this.TranslatePoint(r.Location, ToolStripPointType.ToolStripItemCoords, ToolStripPointType.ToolStripCoords);
            if (this.ParentInternal != null)
            {
                this.ParentInternal.Invalidate(new Rectangle(location, r.Size), true);
            }
        }

        internal void InvalidateImageListImage()
        {
            if (this.ImageIndexer.ActualIndex >= 0)
            {
                this.Properties.SetObject(PropImage, null);
                this.InvalidateItemLayout(PropertyNames.Image);
            }
        }

        internal void InvalidateItemLayout(string affectedProperty)
        {
            this.InvalidateItemLayout(affectedProperty, true);
        }

        internal void InvalidateItemLayout(string affectedProperty, bool invalidatePainting)
        {
            this.toolStripItemInternalLayout = null;
            if (this.Owner != null)
            {
                LayoutTransaction.DoLayout(this.Owner, this, affectedProperty);
            }
            if (invalidatePainting && (this.Owner != null))
            {
                this.Owner.Invalidate();
            }
        }

        internal void InvokePaint()
        {
            if (this.ParentInternal != null)
            {
                this.ParentInternal.InvokePaintItem(this);
            }
        }

        protected internal virtual bool IsInputChar(char charCode)
        {
            return false;
        }

        protected internal virtual bool IsInputKey(Keys keyData)
        {
            return false;
        }

        internal virtual void OnAccessibleDefaultActionDescriptionChanged(EventArgs e)
        {
        }

        internal virtual void OnAccessibleDescriptionChanged(EventArgs e)
        {
        }

        internal virtual void OnAccessibleNameChanged(EventArgs e)
        {
        }

        internal virtual void OnAccessibleRoleChanged(EventArgs e)
        {
        }

        private void OnAnimationFrameChanged(object o, EventArgs e)
        {
            ToolStrip parentInternal = this.ParentInternal;
            if ((parentInternal != null) && (!parentInternal.Disposing && !parentInternal.IsDisposed))
            {
                if (parentInternal.IsHandleCreated && parentInternal.InvokeRequired)
                {
                    parentInternal.BeginInvoke(new EventHandler(this.OnAnimationFrameChanged), new object[] { o, e });
                }
                else
                {
                    this.Invalidate();
                }
            }
        }

        protected virtual void OnAvailableChanged(EventArgs e)
        {
            this.RaiseEvent(EventAvailableChanged, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnBackColorChanged(EventArgs e)
        {
            this.Invalidate();
            this.RaiseEvent(EventBackColorChanged, e);
        }

        protected virtual void OnBoundsChanged()
        {
            LayoutTransaction.DoLayout(this.ParentInternal, this, PropertyNames.Bounds);
            this.InternalLayout.PerformLayout();
        }

        protected virtual void OnClick(EventArgs e)
        {
            this.RaiseEvent(EventClick, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDisplayStyleChanged(EventArgs e)
        {
            this.RaiseEvent(EventDisplayStyleChanged, e);
        }

        protected virtual void OnDoubleClick(EventArgs e)
        {
            this.RaiseEvent(EventDoubleClick, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDragDrop(DragEventArgs dragEvent)
        {
            this.RaiseDragEvent(EventDragDrop, dragEvent);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDragEnter(DragEventArgs dragEvent)
        {
            this.RaiseDragEvent(EventDragEnter, dragEvent);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDragLeave(EventArgs e)
        {
            this.RaiseEvent(EventDragLeave, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDragOver(DragEventArgs dragEvent)
        {
            this.RaiseDragEvent(EventDragOver, dragEvent);
        }

        protected virtual void OnEnabledChanged(EventArgs e)
        {
            this.RaiseEvent(EventEnabledChanged, e);
            this.Animate();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnFontChanged(EventArgs e)
        {
            this.cachedTextSize = System.Drawing.Size.Empty;
            if ((this.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text)
            {
                this.InvalidateItemLayout(PropertyNames.Font);
            }
            else
            {
                this.toolStripItemInternalLayout = null;
            }
            this.RaiseEvent(EventFontChanged, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnForeColorChanged(EventArgs e)
        {
            this.Invalidate();
            this.RaiseEvent(EventForeColorChanged, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnGiveFeedback(GiveFeedbackEventArgs giveFeedbackEvent)
        {
            GiveFeedbackEventHandler handler = (GiveFeedbackEventHandler) base.Events[EventGiveFeedback];
            if (handler != null)
            {
                handler(this, giveFeedbackEvent);
            }
        }

        internal virtual void OnImageScalingSizeChanged(EventArgs e)
        {
        }

        internal void OnInternalEnabledChanged(EventArgs e)
        {
            this.RaiseEvent(EventInternalEnabledChanged, e);
        }

        protected internal virtual void OnLayout(LayoutEventArgs e)
        {
        }

        protected virtual void OnLocationChanged(EventArgs e)
        {
            this.RaiseEvent(EventLocationChanged, e);
        }

        protected virtual void OnMouseDown(MouseEventArgs e)
        {
        }

        protected virtual void OnMouseEnter(EventArgs e)
        {
        }

        protected virtual void OnMouseHover(EventArgs e)
        {
            if ((this.ParentInternal != null) && !string.IsNullOrEmpty(this.ToolTipText))
            {
                this.ParentInternal.UpdateToolTip(this);
            }
        }

        protected virtual void OnMouseLeave(EventArgs e)
        {
            if (this.ParentInternal != null)
            {
                this.ParentInternal.UpdateToolTip(null);
            }
        }

        protected virtual void OnMouseMove(MouseEventArgs mea)
        {
        }

        protected virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        protected virtual void OnOwnerChanged(EventArgs e)
        {
            this.RaiseEvent(EventOwnerChanged, e);
            this.SetAmbientMargin();
            if (this.Owner != null)
            {
                bool found = false;
                int integer = this.Properties.GetInteger(PropRightToLeft, out found);
                if (!found)
                {
                    integer = 2;
                }
                if ((integer == 2) && (this.RightToLeft != this.DefaultRightToLeft))
                {
                    this.OnRightToLeftChanged(EventArgs.Empty);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual void OnOwnerFontChanged(EventArgs e)
        {
            if (this.Properties.GetObject(PropFont) == null)
            {
                this.OnFontChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal void OnOwnerTextDirectionChanged()
        {
            ToolStripTextDirection inherit = ToolStripTextDirection.Inherit;
            if (this.Properties.ContainsObject(PropTextDirection))
            {
                inherit = (ToolStripTextDirection) this.Properties.GetObject(PropTextDirection);
            }
            if (inherit == ToolStripTextDirection.Inherit)
            {
                this.InvalidateItemLayout("TextDirection");
            }
        }

        protected virtual void OnPaint(PaintEventArgs e)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentBackColorChanged(EventArgs e)
        {
            if (this.Properties.GetColor(PropBackColor).IsEmpty)
            {
                this.OnBackColorChanged(e);
            }
        }

        protected virtual void OnParentChanged(ToolStrip oldParent, ToolStrip newParent)
        {
            this.SetAmbientMargin();
            if ((oldParent != null) && (oldParent.DropTargetManager != null))
            {
                oldParent.DropTargetManager.EnsureUnRegistered(this);
            }
            if (this.AllowDrop && (newParent != null))
            {
                this.EnsureParentDropTargetRegistered();
            }
            this.Animate();
        }

        protected internal virtual void OnParentEnabledChanged(EventArgs e)
        {
            this.OnEnabledChanged(EventArgs.Empty);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnParentForeColorChanged(EventArgs e)
        {
            if (this.Properties.GetColor(PropForeColor).IsEmpty)
            {
                this.OnForeColorChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual void OnParentRightToLeftChanged(EventArgs e)
        {
            if (!this.Properties.ContainsInteger(PropRightToLeft) || (this.Properties.GetInteger(PropRightToLeft) == 2))
            {
                this.OnRightToLeftChanged(e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs queryContinueDragEvent)
        {
            this.RaiseQueryContinueDragEvent(EventQueryContinueDrag, queryContinueDragEvent);
        }

        protected virtual void OnRightToLeftChanged(EventArgs e)
        {
            this.InvalidateItemLayout(PropertyNames.RightToLeft);
            this.RaiseEvent(EventRightToLeft, e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnTextChanged(EventArgs e)
        {
            this.cachedTextSize = System.Drawing.Size.Empty;
            this.InvalidateItemLayout(PropertyNames.Text);
            this.RaiseEvent(EventText, e);
        }

        protected virtual void OnVisibleChanged(EventArgs e)
        {
            if (((this.Owner != null) && !this.Owner.IsDisposed) && !this.Owner.Disposing)
            {
                this.Owner.OnItemVisibleChanged(new ToolStripItemEventArgs(this), true);
            }
            this.RaiseEvent(EventVisibleChanged, e);
            this.Animate();
        }

        public void PerformClick()
        {
            if (this.Enabled && this.Available)
            {
                this.FireEvent(ToolStripItemEventType.Click);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected internal virtual bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            return false;
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows), UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal virtual bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData != Keys.Enter) && (!this.state[stateSupportsSpaceKey] || (keyData != Keys.Space)))
            {
                return false;
            }
            this.FireEvent(ToolStripItemEventType.Click);
            if ((this.ParentInternal != null) && !this.ParentInternal.IsDropDown)
            {
                this.ParentInternal.RestoreFocusInternal();
            }
            return true;
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows), UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal virtual bool ProcessMnemonic(char charCode)
        {
            this.FireEvent(ToolStripItemEventType.Click);
            return true;
        }

        internal void Push(bool push)
        {
            if (((this.CanSelect && this.Enabled) && !base.DesignMode) && (this.state[statePressed] != push))
            {
                this.state[statePressed] = push;
                if (this.Available)
                {
                    this.Invalidate();
                }
            }
        }

        internal void RaiseCancelEvent(object key, CancelEventArgs e)
        {
            CancelEventHandler handler = (CancelEventHandler) base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void RaiseDragEvent(object key, DragEventArgs e)
        {
            DragEventHandler handler = (DragEventHandler) base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void RaiseEvent(object key, EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void RaiseKeyEvent(object key, KeyEventArgs e)
        {
            KeyEventHandler handler = (KeyEventHandler) base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void RaiseKeyPressEvent(object key, KeyPressEventArgs e)
        {
            KeyPressEventHandler handler = (KeyPressEventHandler) base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void RaiseMouseEvent(object key, MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void RaisePaintEvent(object key, PaintEventArgs e)
        {
            PaintEventHandler handler = (PaintEventHandler) base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal void RaiseQueryContinueDragEvent(object key, QueryContinueDragEventArgs e)
        {
            QueryContinueDragEventHandler handler = (QueryContinueDragEventHandler) base.Events[key];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetBackColor()
        {
            this.BackColor = Color.Empty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetDisplayStyle()
        {
            this.DisplayStyle = this.DefaultDisplayStyle;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetFont()
        {
            this.Font = null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetForeColor()
        {
            this.ForeColor = Color.Empty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetImage()
        {
            this.Image = null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private void ResetImageTransparentColor()
        {
            this.ImageTransparentColor = Color.Empty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ResetMargin()
        {
            this.state[stateUseAmbientMargin] = true;
            this.SetAmbientMargin();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ResetPadding()
        {
            CommonProperties.ResetPadding(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetRightToLeft()
        {
            this.RightToLeft = System.Windows.Forms.RightToLeft.Inherit;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ResetTextDirection()
        {
            this.TextDirection = ToolStripTextDirection.Inherit;
        }

        private void ResetToolTipText()
        {
            this.toolTipText = null;
        }

        public void Select()
        {
            if (((this.CanSelect && ((this.Owner == null) || !this.Owner.IsCurrentlyDragging)) && ((this.ParentInternal == null) || !this.ParentInternal.IsSelectionSuspended)) && !this.Selected)
            {
                this.state[stateSelected] = true;
                if (this.ParentInternal != null)
                {
                    this.ParentInternal.NotifySelectionChange(this);
                }
                if ((this.IsOnDropDown && (this.OwnerItem != null)) && this.OwnerItem.IsOnDropDown)
                {
                    this.OwnerItem.Select();
                }
            }
        }

        internal void SetAmbientMargin()
        {
            if (this.state[stateUseAmbientMargin] && (this.Margin != this.DefaultMargin))
            {
                CommonProperties.SetMargin(this, this.DefaultMargin);
            }
        }

        protected internal virtual void SetBounds(Rectangle bounds)
        {
            Rectangle rectangle = this.bounds;
            this.bounds = bounds;
            if (!this.state[stateContstructing])
            {
                if (this.bounds != rectangle)
                {
                    this.OnBoundsChanged();
                }
                if (this.bounds.Location != rectangle.Location)
                {
                    this.OnLocationChanged(EventArgs.Empty);
                }
            }
        }

        internal void SetBounds(int x, int y, int width, int height)
        {
            this.SetBounds(new Rectangle(x, y, width, height));
        }

        internal void SetOwner(ToolStrip newOwner)
        {
            if (this.owner != newOwner)
            {
                System.Drawing.Font font = this.Font;
                this.owner = newOwner;
                if (newOwner == null)
                {
                    this.ParentInternal = null;
                }
                if (!this.state[stateDisposing] && !this.IsDisposed)
                {
                    this.OnOwnerChanged(EventArgs.Empty);
                    if (font != this.Font)
                    {
                        this.OnFontChanged(EventArgs.Empty);
                    }
                }
            }
        }

        internal void SetPlacement(ToolStripItemPlacement placement)
        {
            this.placement = placement;
        }

        protected virtual void SetVisibleCore(bool visible)
        {
            if (this.state[stateVisible] != visible)
            {
                this.state[stateVisible] = visible;
                this.Unselect();
                this.Push(false);
                this.OnAvailableChanged(EventArgs.Empty);
                this.OnVisibleChanged(EventArgs.Empty);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeBackColor()
        {
            return !this.Properties.GetColor(PropBackColor).IsEmpty;
        }

        private bool ShouldSerializeDisplayStyle()
        {
            return (this.DisplayStyle != this.DefaultDisplayStyle);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeFont()
        {
            bool flag;
            object obj2 = this.Properties.GetObject(PropFont, out flag);
            return (flag && (obj2 != null));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeForeColor()
        {
            return !this.Properties.GetColor(PropForeColor).IsEmpty;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeImage()
        {
            return ((this.Image != null) && (this.ImageIndexer.ActualIndex < 0));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeImageIndex()
        {
            return (((this.Image != null) && (this.ImageIndexer.ActualIndex >= 0)) && (this.ImageIndexer.Index != -1));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeImageKey()
        {
            if ((this.Image == null) || (this.ImageIndexer.ActualIndex < 0))
            {
                return false;
            }
            return ((this.ImageIndexer.Key != null) && (this.ImageIndexer.Key.Length != 0));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeImageTransparentColor()
        {
            return (this.ImageTransparentColor != Color.Empty);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeMargin()
        {
            return (this.Margin != this.DefaultMargin);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializePadding()
        {
            return (this.Padding != this.DefaultPadding);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeRightToLeft()
        {
            bool found = false;
            int integer = this.Properties.GetInteger(PropRightToLeft, out found);
            if (!found)
            {
                return false;
            }
            return (integer != this.DefaultRightToLeft);
        }

        private bool ShouldSerializeTextDirection()
        {
            ToolStripTextDirection inherit = ToolStripTextDirection.Inherit;
            if (this.Properties.ContainsObject(PropTextDirection))
            {
                inherit = (ToolStripTextDirection) this.Properties.GetObject(PropTextDirection);
            }
            return (inherit != ToolStripTextDirection.Inherit);
        }

        private bool ShouldSerializeToolTipText()
        {
            return !string.IsNullOrEmpty(this.toolTipText);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeVisible()
        {
            return !this.state[stateVisible];
        }

        private void StopAnimate()
        {
            this.Animate(false);
        }

        void IDropTarget.OnDragDrop(DragEventArgs dragEvent)
        {
            this.OnDragDrop(dragEvent);
        }

        void IDropTarget.OnDragEnter(DragEventArgs dragEvent)
        {
            this.OnDragEnter(dragEvent);
        }

        void IDropTarget.OnDragLeave(EventArgs e)
        {
            this.OnDragLeave(e);
        }

        void IDropTarget.OnDragOver(DragEventArgs dragEvent)
        {
            this.OnDragOver(dragEvent);
        }

        void ISupportOleDropSource.OnGiveFeedback(GiveFeedbackEventArgs giveFeedbackEventArgs)
        {
            this.OnGiveFeedback(giveFeedbackEventArgs);
        }

        void ISupportOleDropSource.OnQueryContinueDrag(QueryContinueDragEventArgs queryContinueDragEventArgs)
        {
            this.OnQueryContinueDrag(queryContinueDragEventArgs);
        }

        void IArrangedElement.PerformLayout(IArrangedElement container, string propertyName)
        {
        }

        void IArrangedElement.SetBounds(Rectangle bounds, BoundsSpecified specified)
        {
            this.SetBounds(bounds);
        }

        public override string ToString()
        {
            if ((this.Text != null) && (this.Text.Length != 0))
            {
                return this.Text;
            }
            return base.ToString();
        }

        internal Point TranslatePoint(Point fromPoint, ToolStripPointType fromPointType, ToolStripPointType toPointType)
        {
            ToolStrip parentInternal = this.ParentInternal;
            if (parentInternal == null)
            {
                parentInternal = (this.IsOnOverflow && (this.Owner != null)) ? this.Owner.OverflowButton.DropDown : this.Owner;
            }
            if (parentInternal != null)
            {
                if (fromPointType == toPointType)
                {
                    return fromPoint;
                }
                Point empty = Point.Empty;
                Point location = this.Bounds.Location;
                if (fromPointType == ToolStripPointType.ScreenCoords)
                {
                    empty = parentInternal.PointToClient(fromPoint);
                    if (toPointType == ToolStripPointType.ToolStripItemCoords)
                    {
                        empty.X += location.X;
                        empty.Y += location.Y;
                    }
                    return empty;
                }
                if (fromPointType == ToolStripPointType.ToolStripItemCoords)
                {
                    fromPoint.X += location.X;
                    fromPoint.Y += location.Y;
                }
                if (toPointType == ToolStripPointType.ScreenCoords)
                {
                    return parentInternal.PointToScreen(fromPoint);
                }
                if (toPointType == ToolStripPointType.ToolStripItemCoords)
                {
                    fromPoint.X -= location.X;
                    fromPoint.Y -= location.Y;
                    return fromPoint;
                }
            }
            return fromPoint;
        }

        internal void Unselect()
        {
            if (this.state[stateSelected])
            {
                this.state[stateSelected] = false;
                if (this.Available)
                {
                    this.Invalidate();
                    if (this.ParentInternal != null)
                    {
                        this.ParentInternal.NotifySelectionChange(this);
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRDescription("ToolStripItemAccessibilityObjectDescr"), EditorBrowsable(EditorBrowsableState.Advanced)]
        public AccessibleObject AccessibilityObject
        {
            get
            {
                AccessibleObject obj2 = (AccessibleObject) this.Properties.GetObject(PropAccessibility);
                if (obj2 == null)
                {
                    obj2 = this.CreateAccessibilityInstance();
                    this.Properties.SetObject(PropAccessibility, obj2);
                }
                return obj2;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ToolStripItemAccessibleDefaultActionDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatAccessibility")]
        public string AccessibleDefaultActionDescription
        {
            get
            {
                return (string) this.Properties.GetObject(PropAccessibleDefaultActionDescription);
            }
            set
            {
                this.Properties.SetObject(PropAccessibleDefaultActionDescription, value);
                this.OnAccessibleDefaultActionDescriptionChanged(EventArgs.Empty);
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRDescription("ToolStripItemAccessibleDescriptionDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatAccessibility")]
        public string AccessibleDescription
        {
            get
            {
                return (string) this.Properties.GetObject(PropAccessibleDescription);
            }
            set
            {
                this.Properties.SetObject(PropAccessibleDescription, value);
                this.OnAccessibleDescriptionChanged(EventArgs.Empty);
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatAccessibility"), DefaultValue((string) null), System.Windows.Forms.SRDescription("ToolStripItemAccessibleNameDescr")]
        public string AccessibleName
        {
            get
            {
                return (string) this.Properties.GetObject(PropAccessibleName);
            }
            set
            {
                this.Properties.SetObject(PropAccessibleName, value);
                this.OnAccessibleNameChanged(EventArgs.Empty);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemAccessibleRoleDescr"), System.Windows.Forms.SRCategory("CatAccessibility"), DefaultValue(-1)]
        public System.Windows.Forms.AccessibleRole AccessibleRole
        {
            get
            {
                bool flag;
                int integer = this.Properties.GetInteger(PropAccessibleRole, out flag);
                if (flag)
                {
                    return (System.Windows.Forms.AccessibleRole) integer;
                }
                return System.Windows.Forms.AccessibleRole.Default;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, -1, 0x40))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.AccessibleRole));
                }
                this.Properties.SetInteger(PropAccessibleRole, (int) value);
                this.OnAccessibleRoleChanged(EventArgs.Empty);
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ToolStripItemAlignmentDescr")]
        public ToolStripItemAlignment Alignment
        {
            get
            {
                return this.alignment;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolStripItemAlignment));
                }
                if (this.alignment != value)
                {
                    this.alignment = value;
                    if ((this.ParentInternal != null) && this.ParentInternal.IsHandleCreated)
                    {
                        this.ParentInternal.PerformLayout();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemAllowDropDescr"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatDragDrop"), DefaultValue(false)]
        public virtual bool AllowDrop
        {
            get
            {
                return this.state[stateAllowDrop];
            }
            set
            {
                if (value != this.state[stateAllowDrop])
                {
                    this.EnsureParentDropTargetRegistered();
                    this.state[stateAllowDrop] = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), DefaultValue(5)]
        public AnchorStyles Anchor
        {
            get
            {
                return CommonProperties.xGetAnchor(this);
            }
            set
            {
                if (value != this.Anchor)
                {
                    CommonProperties.xSetAnchor(this, value);
                    if (this.ParentInternal != null)
                    {
                        LayoutTransaction.DoLayout(this, this.ParentInternal, PropertyNames.Anchor);
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemAutoSizeDescr"), DefaultValue(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), RefreshProperties(RefreshProperties.All)]
        public bool AutoSize
        {
            get
            {
                return this.state[stateAutoSize];
            }
            set
            {
                if (this.state[stateAutoSize] != value)
                {
                    this.state[stateAutoSize] = value;
                    CommonProperties.SetAutoSize(this, value);
                    this.InvalidateItemLayout(PropertyNames.AutoSize);
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("ToolStripItemAutoToolTipDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool AutoToolTip
        {
            get
            {
                return this.state[stateAutoToolTip];
            }
            set
            {
                this.state[stateAutoToolTip] = value;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemAvailableDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Available
        {
            get
            {
                return this.state[stateVisible];
            }
            set
            {
                this.SetVisibleCore(value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemBackColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public virtual Color BackColor
        {
            get
            {
                Color rawBackColor = this.RawBackColor;
                if (!rawBackColor.IsEmpty)
                {
                    return rawBackColor;
                }
                Control parentInternal = this.ParentInternal;
                if (parentInternal != null)
                {
                    return parentInternal.BackColor;
                }
                return Control.DefaultBackColor;
            }
            set
            {
                Color backColor = this.BackColor;
                if (!value.IsEmpty || this.Properties.ContainsObject(PropBackColor))
                {
                    this.Properties.SetColor(PropBackColor, value);
                }
                if (!backColor.Equals(this.BackColor))
                {
                    this.OnBackColorChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue((string) null), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripItemImageDescr")]
        public virtual System.Drawing.Image BackgroundImage
        {
            get
            {
                return (this.Properties.GetObject(PropBackgroundImage) as System.Drawing.Image);
            }
            set
            {
                if (this.BackgroundImage != value)
                {
                    this.Properties.SetObject(PropBackgroundImage, value);
                    this.Invalidate();
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), System.Windows.Forms.SRDescription("ControlBackgroundImageLayoutDescr"), DefaultValue(1)]
        public virtual ImageLayout BackgroundImageLayout
        {
            get
            {
                if (!this.Properties.ContainsObject(PropBackgroundImageLayout))
                {
                    return ImageLayout.Tile;
                }
                return (ImageLayout) this.Properties.GetObject(PropBackgroundImageLayout);
            }
            set
            {
                if (this.BackgroundImageLayout != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 4))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(ImageLayout));
                    }
                    this.Properties.SetObject(PropBackgroundImageLayout, value);
                    this.Invalidate();
                }
            }
        }

        [Browsable(false)]
        public virtual Rectangle Bounds
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.bounds;
            }
        }

        internal virtual bool CanKeyboardSelect
        {
            get
            {
                return this.CanSelect;
            }
        }

        [Browsable(false)]
        public virtual bool CanSelect
        {
            get
            {
                return true;
            }
        }

        internal Rectangle ClientBounds
        {
            get
            {
                Rectangle bounds = this.bounds;
                bounds.Location = Point.Empty;
                return bounds;
            }
        }

        [Browsable(false)]
        public Rectangle ContentRectangle
        {
            get
            {
                Rectangle rectangle = LayoutUtils.InflateRect(this.InternalLayout.ContentRectangle, this.Padding);
                rectangle.Size = LayoutUtils.UnionSizes(System.Drawing.Size.Empty, rectangle.Size);
                return rectangle;
            }
        }

        protected virtual bool DefaultAutoToolTip
        {
            get
            {
                return false;
            }
        }

        protected virtual ToolStripItemDisplayStyle DefaultDisplayStyle
        {
            get
            {
                return ToolStripItemDisplayStyle.ImageAndText;
            }
        }

        protected internal virtual System.Windows.Forms.Padding DefaultMargin
        {
            get
            {
                if ((this.Owner != null) && (this.Owner is StatusStrip))
                {
                    return new System.Windows.Forms.Padding(0, 2, 0, 0);
                }
                return new System.Windows.Forms.Padding(0, 1, 0, 2);
            }
        }

        protected virtual System.Windows.Forms.Padding DefaultPadding
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return System.Windows.Forms.Padding.Empty;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        private System.Windows.Forms.RightToLeft DefaultRightToLeft
        {
            get
            {
                return System.Windows.Forms.RightToLeft.Inherit;
            }
        }

        protected virtual System.Drawing.Size DefaultSize
        {
            get
            {
                return new System.Drawing.Size(0x17, 0x17);
            }
        }

        protected internal virtual bool DismissWhenClicked
        {
            get
            {
                return true;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripItemDisplayStyleDescr")]
        public virtual ToolStripItemDisplayStyle DisplayStyle
        {
            get
            {
                return this.displayStyle;
            }
            set
            {
                if (this.displayStyle != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolStripItemDisplayStyle));
                    }
                    this.displayStyle = value;
                    if (!this.state[stateContstructing])
                    {
                        this.InvalidateItemLayout(PropertyNames.DisplayStyle);
                        this.OnDisplayStyleChanged(new EventArgs());
                    }
                }
            }
        }

        [DefaultValue(0), Browsable(false)]
        public DockStyle Dock
        {
            get
            {
                return CommonProperties.xGetDock(this);
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 5))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DockStyle));
                }
                if (value != this.Dock)
                {
                    CommonProperties.xSetDock(this, value);
                    if (this.ParentInternal != null)
                    {
                        LayoutTransaction.DoLayout(this, this.ParentInternal, PropertyNames.Dock);
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemDoubleClickedEnabledDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool DoubleClickEnabled
        {
            get
            {
                return this.state[stateDoubleClickEnabled];
            }
            set
            {
                this.state[stateDoubleClickEnabled] = value;
            }
        }

        internal static long DoubleClickTicks
        {
            get
            {
                return (long) (SystemInformation.DoubleClickTime * 0x2710);
            }
        }

        private System.Windows.Forms.DropSource DropSource
        {
            get
            {
                if (((this.ParentInternal != null) && this.ParentInternal.AllowItemReorder) && (this.ParentInternal.ItemReorderDropSource != null))
                {
                    return new System.Windows.Forms.DropSource(this.ParentInternal.ItemReorderDropSource);
                }
                return new System.Windows.Forms.DropSource(this);
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("ToolStripItemEnabledDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public virtual bool Enabled
        {
            get
            {
                bool enabled = true;
                if (this.Owner != null)
                {
                    enabled = this.Owner.Enabled;
                }
                return (this.state[stateEnabled] && enabled);
            }
            set
            {
                if (this.state[stateEnabled] != value)
                {
                    this.state[stateEnabled] = value;
                    if (!this.state[stateEnabled])
                    {
                        this.state[stateSelected | statePressed] = false;
                    }
                    this.OnEnabledChanged(EventArgs.Empty);
                    this.Invalidate();
                }
                this.OnInternalEnabledChanged(EventArgs.Empty);
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("ToolStripItemFontDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public virtual System.Drawing.Font Font
        {
            get
            {
                System.Drawing.Font font = (System.Drawing.Font) this.Properties.GetObject(PropFont);
                if (font != null)
                {
                    return font;
                }
                System.Drawing.Font ownerFont = this.GetOwnerFont();
                if (ownerFont != null)
                {
                    return ownerFont;
                }
                return ToolStripManager.DefaultFont;
            }
            set
            {
                System.Drawing.Font font = (System.Drawing.Font) this.Properties.GetObject(PropFont);
                if (font != value)
                {
                    this.Properties.SetObject(PropFont, value);
                    this.OnFontChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripItemForeColorDescr")]
        public virtual Color ForeColor
        {
            get
            {
                Color color = this.Properties.GetColor(PropForeColor);
                if (!color.IsEmpty)
                {
                    return color;
                }
                Control parentInternal = this.ParentInternal;
                if (parentInternal != null)
                {
                    return parentInternal.ForeColor;
                }
                return Control.DefaultForeColor;
            }
            set
            {
                Color foreColor = this.ForeColor;
                if (!value.IsEmpty || this.Properties.ContainsObject(PropForeColor))
                {
                    this.Properties.SetColor(PropForeColor, value);
                }
                if (!foreColor.Equals(this.ForeColor))
                {
                    this.OnForeColorChanged(EventArgs.Empty);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRCategory("CatLayout"), Browsable(false)]
        public int Height
        {
            get
            {
                return this.Bounds.Height;
            }
            set
            {
                Rectangle bounds = this.Bounds;
                this.SetBounds(bounds.X, bounds.Y, bounds.Width, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), System.Windows.Forms.SRDescription("ToolStripItemImageDescr")]
        public virtual System.Drawing.Image Image
        {
            get
            {
                System.Drawing.Image image = (System.Drawing.Image) this.Properties.GetObject(PropImage);
                if (((image == null) && (this.Owner != null)) && ((this.Owner.ImageList != null) && (this.ImageIndexer.ActualIndex >= 0)))
                {
                    if (this.ImageIndexer.ActualIndex < this.Owner.ImageList.Images.Count)
                    {
                        image = this.Owner.ImageList.Images[this.ImageIndexer.ActualIndex];
                        this.state[stateInvalidMirroredImage] = true;
                        this.Properties.SetObject(PropImage, image);
                        return image;
                    }
                    return null;
                }
                return image;
            }
            set
            {
                if (this.Image != value)
                {
                    this.StopAnimate();
                    Bitmap image = value as Bitmap;
                    if ((image != null) && (this.ImageTransparentColor != Color.Empty))
                    {
                        if ((image.RawFormat.Guid != ImageFormat.Icon.Guid) && !ImageAnimator.CanAnimate(image))
                        {
                            image.MakeTransparent(this.ImageTransparentColor);
                        }
                        value = image;
                    }
                    if (value != null)
                    {
                        this.ImageIndex = -1;
                    }
                    this.Properties.SetObject(PropImage, value);
                    this.state[stateInvalidMirroredImage] = true;
                    this.Animate();
                    this.InvalidateItemLayout(PropertyNames.Image);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripItemImageAlignDescr"), DefaultValue(0x20), Localizable(true)]
        public ContentAlignment ImageAlign
        {
            get
            {
                return this.imageAlign;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidContentAlignment(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ContentAlignment));
                }
                if (this.imageAlign != value)
                {
                    this.imageAlign = value;
                    this.InvalidateItemLayout(PropertyNames.ImageAlign);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemImageIndexDescr"), Editor("System.Windows.Forms.Design.ToolStripImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Browsable(false), Localizable(true), TypeConverter(typeof(NoneExcludedImageIndexConverter)), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatBehavior"), RelatedImageList("Owner.ImageList")]
        public int ImageIndex
        {
            get
            {
                if (((this.Owner != null) && (this.ImageIndexer.Index != -1)) && ((this.Owner.ImageList != null) && (this.ImageIndexer.Index >= this.Owner.ImageList.Images.Count)))
                {
                    return (this.Owner.ImageList.Images.Count - 1);
                }
                return this.ImageIndexer.Index;
            }
            set
            {
                if (value < -1)
                {
                    object[] args = new object[] { "ImageIndex", value.ToString(CultureInfo.CurrentCulture), -1.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("ImageIndex", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.ImageIndexer.Index = value;
                this.state[stateInvalidMirroredImage] = true;
                this.Properties.SetObject(PropImage, null);
                this.InvalidateItemLayout(PropertyNames.ImageIndex);
            }
        }

        internal ToolStripItemImageIndexer ImageIndexer
        {
            get
            {
                if (this.imageIndexer == null)
                {
                    this.imageIndexer = new ToolStripItemImageIndexer(this);
                }
                return this.imageIndexer;
            }
        }

        [Localizable(true), TypeConverter(typeof(ImageKeyConverter)), RefreshProperties(RefreshProperties.Repaint), Editor("System.Windows.Forms.Design.ToolStripImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRDescription("ToolStripItemImageKeyDescr"), Browsable(false), RelatedImageList("Owner.ImageList"), System.Windows.Forms.SRCategory("CatBehavior")]
        public string ImageKey
        {
            get
            {
                return this.ImageIndexer.Key;
            }
            set
            {
                this.ImageIndexer.Key = value;
                this.state[stateInvalidMirroredImage] = true;
                this.Properties.SetObject(PropImage, null);
                this.InvalidateItemLayout(PropertyNames.ImageKey);
            }
        }

        [DefaultValue(1), System.Windows.Forms.SRDescription("ToolStripItemImageScalingDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public ToolStripItemImageScaling ImageScaling
        {
            get
            {
                return this.imageScaling;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolStripItemImageScaling));
                }
                if (this.imageScaling != value)
                {
                    this.imageScaling = value;
                    this.InvalidateItemLayout(PropertyNames.ImageScaling);
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("ToolStripItemImageTransparentColorDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public Color ImageTransparentColor
        {
            get
            {
                return this.imageTransparentColor;
            }
            set
            {
                if (this.imageTransparentColor != value)
                {
                    this.imageTransparentColor = value;
                    Bitmap image = this.Image as Bitmap;
                    if (((image != null) && (value != Color.Empty)) && ((image.RawFormat.Guid != ImageFormat.Icon.Guid) && !ImageAnimator.CanAnimate(image)))
                    {
                        image.MakeTransparent(this.imageTransparentColor);
                    }
                    this.Invalidate();
                }
            }
        }

        internal ToolStripItemInternalLayout InternalLayout
        {
            get
            {
                if (this.toolStripItemInternalLayout == null)
                {
                    this.toolStripItemInternalLayout = this.CreateInternalLayout();
                }
                return this.toolStripItemInternalLayout;
            }
        }

        [Browsable(false)]
        public bool IsDisposed
        {
            get
            {
                return this.state[stateDisposed];
            }
        }

        internal bool IsForeColorSet
        {
            get
            {
                if (!this.Properties.GetColor(PropForeColor).IsEmpty)
                {
                    return true;
                }
                Control parentInternal = this.ParentInternal;
                return ((parentInternal != null) && parentInternal.ShouldSerializeForeColor());
            }
        }

        internal bool IsInDesignMode
        {
            get
            {
                return base.DesignMode;
            }
        }

        internal virtual bool IsMnemonicsListenerAxSourced
        {
            get
            {
                return true;
            }
        }

        [Browsable(false)]
        public bool IsOnDropDown
        {
            get
            {
                if (this.ParentInternal != null)
                {
                    return this.ParentInternal.IsDropDown;
                }
                return ((this.Owner != null) && this.Owner.IsDropDown);
            }
        }

        [Browsable(false)]
        public bool IsOnOverflow
        {
            get
            {
                return (this.Placement == ToolStripItemPlacement.Overflow);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemMarginDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public System.Windows.Forms.Padding Margin
        {
            get
            {
                return CommonProperties.GetMargin(this);
            }
            set
            {
                if (this.Margin != value)
                {
                    this.state[stateUseAmbientMargin] = false;
                    CommonProperties.SetMargin(this, value);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripMergeActionDescr"), System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(0)]
        public System.Windows.Forms.MergeAction MergeAction
        {
            get
            {
                bool flag;
                int integer = this.Properties.GetInteger(PropMergeAction, out flag);
                if (flag)
                {
                    return (System.Windows.Forms.MergeAction) integer;
                }
                return System.Windows.Forms.MergeAction.Append;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 4))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.MergeAction));
                }
                this.Properties.SetInteger(PropMergeAction, (int) value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripMergeIndexDescr"), DefaultValue(-1), System.Windows.Forms.SRCategory("CatLayout")]
        public int MergeIndex
        {
            get
            {
                bool flag;
                int integer = this.Properties.GetInteger(PropMergeIndex, out flag);
                if (flag)
                {
                    return integer;
                }
                return -1;
            }
            set
            {
                this.Properties.SetInteger(PropMergeIndex, value);
            }
        }

        internal System.Drawing.Image MirroredImage
        {
            get
            {
                if (!this.state[stateInvalidMirroredImage])
                {
                    return (this.Properties.GetObject(PropMirroredImage) as System.Drawing.Image);
                }
                System.Drawing.Image image = this.Image;
                if (image != null)
                {
                    System.Drawing.Image image2 = image.Clone() as System.Drawing.Image;
                    image2.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    this.Properties.SetObject(PropMirroredImage, image2);
                    this.state[stateInvalidMirroredImage] = false;
                    return image2;
                }
                return null;
            }
        }

        internal bool MouseDownAndUpMustBeInSameItem
        {
            get
            {
                return this.state[stateMouseDownAndUpMustBeInSameItem];
            }
            set
            {
                this.state[stateMouseDownAndUpMustBeInSameItem] = value;
            }
        }

        [Browsable(false), DefaultValue((string) null)]
        public string Name
        {
            get
            {
                return WindowsFormsUtils.GetComponentName(this, (string) this.Properties.GetObject(PropName));
            }
            set
            {
                if (!base.DesignMode)
                {
                    this.Properties.SetObject(PropName, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(2), System.Windows.Forms.SRDescription("ToolStripItemOverflowDescr")]
        public ToolStripItemOverflow Overflow
        {
            get
            {
                return this.overflow;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolStripGripStyle));
                }
                if (this.overflow != value)
                {
                    this.overflow = value;
                    if (this.Owner != null)
                    {
                        LayoutTransaction.DoLayout(this.Owner, this.Owner, "Overflow");
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ToolStrip Owner
        {
            get
            {
                return this.owner;
            }
            set
            {
                if (this.owner != value)
                {
                    if (this.owner != null)
                    {
                        this.owner.Items.Remove(this);
                    }
                    if (value != null)
                    {
                        value.Items.Add(this);
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public ToolStripItem OwnerItem
        {
            get
            {
                ToolStripDropDown parentInternal = null;
                if (this.ParentInternal != null)
                {
                    parentInternal = this.ParentInternal as ToolStripDropDown;
                }
                else if (this.Owner != null)
                {
                    parentInternal = this.Owner as ToolStripDropDown;
                }
                if (parentInternal != null)
                {
                    return parentInternal.OwnerItem;
                }
                return null;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ToolStripItemPaddingDescr")]
        public virtual System.Windows.Forms.Padding Padding
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return CommonProperties.GetPadding(this, this.DefaultPadding);
            }
            set
            {
                if (this.Padding != value)
                {
                    CommonProperties.SetPadding(this, value);
                    this.InvalidateItemLayout(PropertyNames.Padding);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        protected internal ToolStrip Parent
        {
            get
            {
                return this.ParentInternal;
            }
            set
            {
                this.ParentInternal = value;
            }
        }

        internal ToolStrip ParentInternal
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.parent;
            }
            set
            {
                if (this.parent != value)
                {
                    ToolStrip parent = this.parent;
                    this.parent = value;
                    this.OnParentChanged(parent, value);
                }
            }
        }

        [Browsable(false)]
        public ToolStripItemPlacement Placement
        {
            get
            {
                return this.placement;
            }
        }

        internal System.Drawing.Size PreferredImageSize
        {
            get
            {
                if ((this.DisplayStyle & ToolStripItemDisplayStyle.Image) != ToolStripItemDisplayStyle.Image)
                {
                    return System.Drawing.Size.Empty;
                }
                System.Drawing.Image image = (System.Drawing.Image) this.Properties.GetObject(PropImage);
                bool flag = ((this.Owner != null) && (this.Owner.ImageList != null)) && (this.ImageIndexer.ActualIndex >= 0);
                if (this.ImageScaling == ToolStripItemImageScaling.SizeToFit)
                {
                    ToolStrip owner = this.Owner;
                    if ((owner != null) && ((image != null) || flag))
                    {
                        return owner.ImageScalingSize;
                    }
                }
                if (flag)
                {
                    return this.Owner.ImageList.ImageSize;
                }
                return ((image == null) ? System.Drawing.Size.Empty : image.Size);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool Pressed
        {
            get
            {
                return (this.CanSelect && this.state[statePressed]);
            }
        }

        internal PropertyStore Properties
        {
            get
            {
                if (this.propertyStore == null)
                {
                    this.propertyStore = new PropertyStore();
                }
                return this.propertyStore;
            }
        }

        internal Color RawBackColor
        {
            get
            {
                return this.Properties.GetColor(PropBackColor);
            }
        }

        internal ToolStripRenderer Renderer
        {
            get
            {
                if (this.Owner != null)
                {
                    return this.Owner.Renderer;
                }
                if (this.ParentInternal == null)
                {
                    return null;
                }
                return this.ParentInternal.Renderer;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemRightToLeftDescr"), System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true)]
        public virtual System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                bool flag;
                int integer = this.Properties.GetInteger(PropRightToLeft, out flag);
                if (!flag)
                {
                    integer = 2;
                }
                if (integer == 2)
                {
                    if (this.Owner != null)
                    {
                        integer = (int) this.Owner.RightToLeft;
                    }
                    else if (this.ParentInternal != null)
                    {
                        integer = (int) this.ParentInternal.RightToLeft;
                    }
                    else
                    {
                        integer = (int) this.DefaultRightToLeft;
                    }
                }
                return (System.Windows.Forms.RightToLeft) integer;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("RightToLeft", (int) value, typeof(System.Windows.Forms.RightToLeft));
                }
                System.Windows.Forms.RightToLeft rightToLeft = this.RightToLeft;
                if (this.Properties.ContainsInteger(PropRightToLeft) || (value != System.Windows.Forms.RightToLeft.Inherit))
                {
                    this.Properties.SetInteger(PropRightToLeft, (int) value);
                }
                if (rightToLeft != this.RightToLeft)
                {
                    this.OnRightToLeftChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(false), System.Windows.Forms.SRDescription("ToolStripItemRightToLeftAutoMirrorImageDescr"), Localizable(true)]
        public bool RightToLeftAutoMirrorImage
        {
            get
            {
                return this.state[stateRightToLeftAutoMirrorImage];
            }
            set
            {
                if (this.state[stateRightToLeftAutoMirrorImage] != value)
                {
                    this.state[stateRightToLeftAutoMirrorImage] = value;
                    this.Invalidate();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual bool Selected
        {
            get
            {
                if (!this.CanSelect || base.DesignMode)
                {
                    return false;
                }
                return (this.state[stateSelected] || (((this.ParentInternal != null) && this.ParentInternal.IsSelectionSuspended) && (this.ParentInternal.LastMouseDownedItem == this)));
            }
        }

        protected internal virtual bool ShowKeyboardCues
        {
            get
            {
                if (!base.DesignMode)
                {
                    return ToolStripManager.ShowMenuFocusCues;
                }
                return true;
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ToolStripItemSizeDescr")]
        public virtual System.Drawing.Size Size
        {
            get
            {
                return this.Bounds.Size;
            }
            set
            {
                Rectangle bounds = this.Bounds;
                bounds.Size = value;
                this.SetBounds(bounds);
            }
        }

        internal bool SupportsDisabledHotTracking
        {
            get
            {
                return this.state[stateSupportsDisabledHotTracking];
            }
            set
            {
                this.state[stateSupportsDisabledHotTracking] = value;
            }
        }

        internal bool SupportsItemClick
        {
            get
            {
                return this.state[stateSupportsItemClick];
            }
            set
            {
                this.state[stateSupportsItemClick] = value;
            }
        }

        internal bool SupportsRightClick
        {
            get
            {
                return this.state[stateSupportsRightClick];
            }
            set
            {
                this.state[stateSupportsRightClick] = value;
            }
        }

        internal bool SupportsSpaceKey
        {
            get
            {
                return this.state[stateSupportsSpaceKey];
            }
            set
            {
                this.state[stateSupportsSpaceKey] = value;
            }
        }

        ArrangedElementCollection IArrangedElement.Children
        {
            get
            {
                return EmptyChildCollection;
            }
        }

        IArrangedElement IArrangedElement.Container
        {
            get
            {
                if (this.ParentInternal == null)
                {
                    return this.Owner;
                }
                return this.ParentInternal;
            }
        }

        Rectangle IArrangedElement.DisplayRectangle
        {
            get
            {
                return this.Bounds;
            }
        }

        bool IArrangedElement.ParticipatesInLayout
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.state[stateVisible];
            }
        }

        PropertyStore IArrangedElement.Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return this.Properties;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemTagDescr"), Localizable(false), System.Windows.Forms.SRCategory("CatData"), DefaultValue((string) null), Bindable(true), TypeConverter(typeof(StringConverter))]
        public object Tag
        {
            get
            {
                if (this.Properties.ContainsObject(PropTag))
                {
                    return this.propertyStore.GetObject(PropTag);
                }
                return null;
            }
            set
            {
                this.Properties.SetObject(PropTag, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripItemTextDescr"), System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), DefaultValue("")]
        public virtual string Text
        {
            get
            {
                if (this.Properties.ContainsObject(PropText))
                {
                    return (string) this.Properties.GetObject(PropText);
                }
                return "";
            }
            set
            {
                if (value != this.Text)
                {
                    this.Properties.SetObject(PropText, value);
                    this.OnTextChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0x20), System.Windows.Forms.SRDescription("ToolStripItemTextAlignDescr"), Localizable(true)]
        public virtual ContentAlignment TextAlign
        {
            get
            {
                return this.textAlign;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidContentAlignment(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ContentAlignment));
                }
                if (this.textAlign != value)
                {
                    this.textAlign = value;
                    this.InvalidateItemLayout(PropertyNames.TextAlign);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripTextDirectionDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public virtual ToolStripTextDirection TextDirection
        {
            get
            {
                ToolStripTextDirection inherit = ToolStripTextDirection.Inherit;
                if (this.Properties.ContainsObject(PropTextDirection))
                {
                    inherit = (ToolStripTextDirection) this.Properties.GetObject(PropTextDirection);
                }
                if (inherit != ToolStripTextDirection.Inherit)
                {
                    return inherit;
                }
                if (this.ParentInternal != null)
                {
                    return this.ParentInternal.TextDirection;
                }
                return ((this.Owner == null) ? ToolStripTextDirection.Horizontal : this.Owner.TextDirection);
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolStripTextDirection));
                }
                this.Properties.SetObject(PropTextDirection, value);
                this.InvalidateItemLayout("TextDirection");
            }
        }

        [DefaultValue(4), System.Windows.Forms.SRDescription("ToolStripItemTextImageRelationDescr"), System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true)]
        public System.Windows.Forms.TextImageRelation TextImageRelation
        {
            get
            {
                return this.textImageRelation;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidTextImageRelation(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.TextImageRelation));
                }
                if (value != this.TextImageRelation)
                {
                    this.textImageRelation = value;
                    this.InvalidateItemLayout(PropertyNames.TextImageRelation);
                }
            }
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ToolStripItemToolTipTextDescr"), Localizable(true)]
        public string ToolTipText
        {
            get
            {
                if (!this.AutoToolTip || !string.IsNullOrEmpty(this.toolTipText))
                {
                    return this.toolTipText;
                }
                string text = this.Text;
                if (WindowsFormsUtils.ContainsMnemonic(text))
                {
                    text = string.Join("", text.Split(new char[] { '&' }));
                }
                return text;
            }
            set
            {
                this.toolTipText = value;
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("ToolStripItemVisibleDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool Visible
        {
            get
            {
                return (((this.ParentInternal != null) && this.ParentInternal.Visible) && this.Available);
            }
            set
            {
                this.SetVisibleCore(value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatLayout")]
        public int Width
        {
            get
            {
                return this.Bounds.Width;
            }
            set
            {
                Rectangle bounds = this.Bounds;
                this.SetBounds(bounds.X, bounds.Y, value, bounds.Height);
            }
        }

        [ComVisible(true)]
        public class ToolStripItemAccessibleObject : AccessibleObject
        {
            private AccessibleStates additionalState;
            private ToolStripItem ownerItem;

            public ToolStripItemAccessibleObject(ToolStripItem ownerItem)
            {
                if (ownerItem == null)
                {
                    throw new ArgumentNullException("ownerItem");
                }
                this.ownerItem = ownerItem;
            }

            public void AddState(AccessibleStates state)
            {
                if (state == AccessibleStates.None)
                {
                    this.additionalState = state;
                }
                else
                {
                    this.additionalState |= state;
                }
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                if (this.Owner != null)
                {
                    this.Owner.PerformClick();
                }
            }

            public override int GetHelpTopic(out string fileName)
            {
                int num = 0;
                QueryAccessibilityHelpEventHandler handler = (QueryAccessibilityHelpEventHandler) this.Owner.Events[ToolStripItem.EventQueryAccessibilityHelp];
                if (handler == null)
                {
                    return base.GetHelpTopic(out fileName);
                }
                QueryAccessibilityHelpEventArgs e = new QueryAccessibilityHelpEventArgs();
                handler(this.Owner, e);
                fileName = e.HelpNamespace;
                if ((fileName != null) && (fileName.Length > 0))
                {
                    System.Windows.Forms.IntSecurity.DemandFileIO(FileIOPermissionAccess.PathDiscovery, fileName);
                }
                try
                {
                    num = int.Parse(e.HelpKeyword, CultureInfo.InvariantCulture);
                }
                catch
                {
                }
                return num;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navigationDirection)
            {
                ToolStripItem item = null;
                if (this.Owner != null)
                {
                    ToolStrip parentInternal = this.Owner.ParentInternal;
                    if (parentInternal == null)
                    {
                        return null;
                    }
                    RightToLeft rightToLeft = parentInternal.RightToLeft;
                    switch (navigationDirection)
                    {
                        case AccessibleNavigation.Up:
                            item = this.Owner.IsOnDropDown ? parentInternal.GetNextItem(this.Owner, ArrowDirection.Up) : parentInternal.GetNextItem(this.Owner, ArrowDirection.Left, true);
                            break;

                        case AccessibleNavigation.Down:
                            item = this.Owner.IsOnDropDown ? parentInternal.GetNextItem(this.Owner, ArrowDirection.Down) : parentInternal.GetNextItem(this.Owner, ArrowDirection.Right, true);
                            break;

                        case AccessibleNavigation.Left:
                        case AccessibleNavigation.Previous:
                            item = parentInternal.GetNextItem(this.Owner, ArrowDirection.Left, true);
                            break;

                        case AccessibleNavigation.Right:
                        case AccessibleNavigation.Next:
                            item = parentInternal.GetNextItem(this.Owner, ArrowDirection.Right, true);
                            break;

                        case AccessibleNavigation.FirstChild:
                            item = parentInternal.GetNextItem(null, ArrowDirection.Right, true);
                            break;

                        case AccessibleNavigation.LastChild:
                            item = parentInternal.GetNextItem(null, ArrowDirection.Left, true);
                            break;
                    }
                }
                if (item != null)
                {
                    return item.AccessibilityObject;
                }
                return null;
            }

            public override string ToString()
            {
                if (this.Owner != null)
                {
                    return ("ToolStripItemAccessibleObject: Owner = " + this.Owner.ToString());
                }
                return "ToolStripItemAccessibleObject: Owner = null";
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle bounds = this.Owner.Bounds;
                    if ((this.Owner.ParentInternal != null) && this.Owner.ParentInternal.Visible)
                    {
                        Point location = this.Owner.ParentInternal.PointToScreen(bounds.Location);
                        return new Rectangle(location, bounds.Size);
                    }
                    return Rectangle.Empty;
                }
            }

            public override string DefaultAction
            {
                get
                {
                    string accessibleDefaultActionDescription = this.ownerItem.AccessibleDefaultActionDescription;
                    if (accessibleDefaultActionDescription != null)
                    {
                        return accessibleDefaultActionDescription;
                    }
                    return System.Windows.Forms.SR.GetString("AccessibleActionPress");
                }
            }

            public override string Description
            {
                get
                {
                    string accessibleDescription = this.ownerItem.AccessibleDescription;
                    if (accessibleDescription != null)
                    {
                        return accessibleDescription;
                    }
                    return base.Description;
                }
            }

            public override string Help
            {
                get
                {
                    QueryAccessibilityHelpEventHandler handler = (QueryAccessibilityHelpEventHandler) this.Owner.Events[ToolStripItem.EventQueryAccessibilityHelp];
                    if (handler != null)
                    {
                        QueryAccessibilityHelpEventArgs e = new QueryAccessibilityHelpEventArgs();
                        handler(this.Owner, e);
                        return e.HelpString;
                    }
                    return base.Help;
                }
            }

            public override string KeyboardShortcut
            {
                get
                {
                    char mnemonic = WindowsFormsUtils.GetMnemonic(this.ownerItem.Text, false);
                    if (this.ownerItem.IsOnDropDown)
                    {
                        if (mnemonic != '\0')
                        {
                            return mnemonic.ToString();
                        }
                        return string.Empty;
                    }
                    if (mnemonic != '\0')
                    {
                        return ("Alt+" + mnemonic);
                    }
                    return string.Empty;
                }
            }

            public override string Name
            {
                get
                {
                    string accessibleName = this.ownerItem.AccessibleName;
                    if (accessibleName != null)
                    {
                        return accessibleName;
                    }
                    string name = base.Name;
                    if ((name != null) && (name.Length != 0))
                    {
                        return name;
                    }
                    return WindowsFormsUtils.TextWithoutMnemonics(this.ownerItem.Text);
                }
                set
                {
                    this.ownerItem.AccessibleName = value;
                }
            }

            internal ToolStripItem Owner
            {
                get
                {
                    return this.ownerItem;
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    if (this.Owner.IsOnDropDown)
                    {
                        ToolStripDropDown currentParentDropDown = this.Owner.GetCurrentParentDropDown();
                        if (currentParentDropDown.OwnerItem != null)
                        {
                            return currentParentDropDown.OwnerItem.AccessibilityObject;
                        }
                        return currentParentDropDown.AccessibilityObject;
                    }
                    if (this.Owner.Parent == null)
                    {
                        return base.Parent;
                    }
                    return this.Owner.Parent.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    AccessibleRole accessibleRole = this.ownerItem.AccessibleRole;
                    if (accessibleRole != AccessibleRole.Default)
                    {
                        return accessibleRole;
                    }
                    return AccessibleRole.PushButton;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    if (!this.ownerItem.CanSelect)
                    {
                        return (base.State | this.additionalState);
                    }
                    if (!this.ownerItem.Enabled)
                    {
                        return (AccessibleStates.Unavailable | this.additionalState);
                    }
                    AccessibleStates states = AccessibleStates.Focusable | this.additionalState;
                    if (this.ownerItem.Selected || this.ownerItem.Pressed)
                    {
                        states |= AccessibleStates.HotTracked | AccessibleStates.Focused;
                    }
                    if (this.ownerItem.Pressed)
                    {
                        states |= AccessibleStates.Pressed;
                    }
                    return states;
                }
            }
        }
    }
}

