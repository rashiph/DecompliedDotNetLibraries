namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Windows.Forms.Layout;

    public class ToolStripControlHost : ToolStripItem
    {
        private System.Windows.Forms.Control control;
        private ContentAlignment controlAlign;
        internal static readonly object EventEnter = new object();
        internal static readonly object EventGotFocus = new object();
        internal static readonly object EventKeyDown = new object();
        internal static readonly object EventKeyPress = new object();
        internal static readonly object EventKeyUp = new object();
        internal static readonly object EventLeave = new object();
        internal static readonly object EventLostFocus = new object();
        internal static readonly object EventValidated = new object();
        internal static readonly object EventValidating = new object();
        private bool inSetVisibleCore;
        private int suspendSyncSizeCount;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler DisplayStyleChanged
        {
            add
            {
                base.Events.AddHandler(ToolStripItem.EventDisplayStyleChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(ToolStripItem.EventDisplayStyleChanged, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatFocus"), System.Windows.Forms.SRDescription("ControlOnEnterDescr")]
        public event EventHandler Enter
        {
            add
            {
                base.Events.AddHandler(EventEnter, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventEnter, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("ToolStripItemOnGotFocusDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatFocus")]
        public event EventHandler GotFocus
        {
            add
            {
                base.Events.AddHandler(EventGotFocus, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventGotFocus, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatKey"), System.Windows.Forms.SRDescription("ControlOnKeyDownDescr")]
        public event KeyEventHandler KeyDown
        {
            add
            {
                base.Events.AddHandler(EventKeyDown, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventKeyDown, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnKeyPressDescr"), System.Windows.Forms.SRCategory("CatKey")]
        public event KeyPressEventHandler KeyPress
        {
            add
            {
                base.Events.AddHandler(EventKeyPress, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventKeyPress, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnKeyUpDescr"), System.Windows.Forms.SRCategory("CatKey")]
        public event KeyEventHandler KeyUp
        {
            add
            {
                base.Events.AddHandler(EventKeyUp, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventKeyUp, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatFocus"), System.Windows.Forms.SRDescription("ControlOnLeaveDescr")]
        public event EventHandler Leave
        {
            add
            {
                base.Events.AddHandler(EventLeave, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLeave, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatFocus"), System.Windows.Forms.SRDescription("ToolStripItemOnLostFocusDescr"), Browsable(false)]
        public event EventHandler LostFocus
        {
            add
            {
                base.Events.AddHandler(EventLostFocus, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLostFocus, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnValidatedDescr"), System.Windows.Forms.SRCategory("CatFocus")]
        public event EventHandler Validated
        {
            add
            {
                base.Events.AddHandler(EventValidated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventValidated, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnValidatingDescr"), System.Windows.Forms.SRCategory("CatFocus")]
        public event CancelEventHandler Validating
        {
            add
            {
                base.Events.AddHandler(EventValidating, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventValidating, value);
            }
        }

        public ToolStripControlHost(System.Windows.Forms.Control c)
        {
            this.controlAlign = ContentAlignment.MiddleCenter;
            if (c == null)
            {
                throw new ArgumentNullException("c", "ControlCannotBeNull");
            }
            this.control = c;
            this.SyncControlParent();
            c.Visible = true;
            this.SetBounds(c.Bounds);
            Rectangle bounds = this.Bounds;
            CommonProperties.UpdateSpecifiedBounds(c, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            this.OnSubscribeControlEvents(c);
        }

        public ToolStripControlHost(System.Windows.Forms.Control c, string name) : this(c)
        {
            base.Name = name;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return this.Control.AccessibilityObject;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && (this.Control != null))
            {
                this.OnUnsubscribeControlEvents(this.Control);
                this.Control.Dispose();
                this.control = null;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void Focus()
        {
            this.Control.Focus();
        }

        private static WindowsFormsUtils.ReadOnlyControlCollection GetControlCollection(ToolStrip toolStrip)
        {
            return ((toolStrip != null) ? ((WindowsFormsUtils.ReadOnlyControlCollection) toolStrip.Controls) : null);
        }

        public override System.Drawing.Size GetPreferredSize(System.Drawing.Size constrainingSize)
        {
            if (this.control != null)
            {
                return (this.Control.GetPreferredSize(constrainingSize - this.Padding.Size) + this.Padding.Size);
            }
            return base.GetPreferredSize(constrainingSize);
        }

        private void HandleBackColorChanged(object sender, EventArgs e)
        {
            this.OnBackColorChanged(e);
        }

        private void HandleClick(object sender, EventArgs e)
        {
            this.OnClick(e);
        }

        private void HandleControlVisibleChanged(object sender, EventArgs e)
        {
            bool participatesInLayout = ((IArrangedElement) this.Control).ParticipatesInLayout;
            if (this.ParticipatesInLayout != participatesInLayout)
            {
                base.Visible = this.Control.Visible;
            }
        }

        private void HandleDoubleClick(object sender, EventArgs e)
        {
            this.OnDoubleClick(e);
        }

        private void HandleDragDrop(object sender, DragEventArgs e)
        {
            this.OnDragDrop(e);
        }

        private void HandleDragEnter(object sender, DragEventArgs e)
        {
            this.OnDragEnter(e);
        }

        private void HandleDragLeave(object sender, EventArgs e)
        {
            this.OnDragLeave(e);
        }

        private void HandleDragOver(object sender, DragEventArgs e)
        {
            this.OnDragOver(e);
        }

        private void HandleEnabledChanged(object sender, EventArgs e)
        {
            this.OnEnabledChanged(e);
        }

        private void HandleEnter(object sender, EventArgs e)
        {
            this.OnEnter(e);
        }

        private void HandleForeColorChanged(object sender, EventArgs e)
        {
            this.OnForeColorChanged(e);
        }

        private void HandleGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            this.OnGiveFeedback(e);
        }

        private void HandleGotFocus(object sender, EventArgs e)
        {
            this.OnGotFocus(e);
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            this.OnKeyDown(e);
        }

        private void HandleKeyPress(object sender, KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        private void HandleLeave(object sender, EventArgs e)
        {
            this.OnLeave(e);
        }

        private void HandleLocationChanged(object sender, EventArgs e)
        {
            this.OnLocationChanged(e);
        }

        private void HandleLostFocus(object sender, EventArgs e)
        {
            this.OnLostFocus(e);
        }

        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            this.OnMouseDown(e);
            base.RaiseMouseEvent(ToolStripItem.EventMouseDown, e);
        }

        private void HandleMouseEnter(object sender, EventArgs e)
        {
            this.OnMouseEnter(e);
            base.RaiseEvent(ToolStripItem.EventMouseEnter, e);
        }

        private void HandleMouseHover(object sender, EventArgs e)
        {
            this.OnMouseHover(e);
            base.RaiseEvent(ToolStripItem.EventMouseHover, e);
        }

        private void HandleMouseLeave(object sender, EventArgs e)
        {
            this.OnMouseLeave(e);
            base.RaiseEvent(ToolStripItem.EventMouseLeave, e);
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            this.OnMouseMove(e);
            base.RaiseMouseEvent(ToolStripItem.EventMouseMove, e);
        }

        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            this.OnMouseUp(e);
            base.RaiseMouseEvent(ToolStripItem.EventMouseUp, e);
        }

        private void HandlePaint(object sender, PaintEventArgs e)
        {
            this.OnPaint(e);
            base.RaisePaintEvent(ToolStripItem.EventPaint, e);
        }

        private void HandleQueryAccessibilityHelp(object sender, QueryAccessibilityHelpEventArgs e)
        {
            QueryAccessibilityHelpEventHandler handler = (QueryAccessibilityHelpEventHandler) base.Events[ToolStripItem.EventQueryAccessibilityHelp];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void HandleQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            this.OnQueryContinueDrag(e);
        }

        private void HandleResize(object sender, EventArgs e)
        {
            if (this.suspendSyncSizeCount == 0)
            {
                this.OnHostedControlResize(e);
            }
        }

        private void HandleRightToLeftChanged(object sender, EventArgs e)
        {
            this.OnRightToLeftChanged(e);
        }

        private void HandleTextChanged(object sender, EventArgs e)
        {
            this.OnTextChanged(e);
        }

        private void HandleValidated(object sender, EventArgs e)
        {
            this.OnValidated(e);
        }

        private void HandleValidating(object sender, CancelEventArgs e)
        {
            this.OnValidating(e);
        }

        internal override void OnAccessibleDefaultActionDescriptionChanged(EventArgs e)
        {
            this.Control.AccessibleDefaultActionDescription = base.AccessibleDefaultActionDescription;
        }

        internal override void OnAccessibleDescriptionChanged(EventArgs e)
        {
            this.Control.AccessibleDescription = base.AccessibleDescription;
        }

        internal override void OnAccessibleNameChanged(EventArgs e)
        {
            this.Control.AccessibleName = base.AccessibleName;
        }

        internal override void OnAccessibleRoleChanged(EventArgs e)
        {
            this.Control.AccessibleRole = base.AccessibleRole;
        }

        protected override void OnBoundsChanged()
        {
            if (this.control != null)
            {
                this.SuspendSizeSync();
                IArrangedElement control = this.control;
                if (control != null)
                {
                    Rectangle bounds = LayoutUtils.Align(LayoutUtils.DeflateRect(this.Bounds, this.Padding).Size, this.Bounds, this.ControlAlign);
                    control.SetBounds(bounds, BoundsSpecified.None);
                    if (bounds != this.control.Bounds)
                    {
                        bounds = LayoutUtils.Align(this.control.Size, this.Bounds, this.ControlAlign);
                        control.SetBounds(bounds, BoundsSpecified.None);
                    }
                    this.ResumeSizeSync();
                }
            }
        }

        protected virtual void OnEnter(EventArgs e)
        {
            base.RaiseEvent(EventEnter, e);
        }

        protected virtual void OnGotFocus(EventArgs e)
        {
            base.RaiseEvent(EventGotFocus, e);
        }

        protected virtual void OnHostedControlResize(EventArgs e)
        {
            this.Size = this.Control.Size;
        }

        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            base.RaiseKeyEvent(EventKeyDown, e);
        }

        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            base.RaiseKeyPressEvent(EventKeyPress, e);
        }

        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            base.RaiseKeyEvent(EventKeyUp, e);
        }

        protected internal override void OnLayout(LayoutEventArgs e)
        {
        }

        protected virtual void OnLeave(EventArgs e)
        {
            base.RaiseEvent(EventLeave, e);
        }

        protected virtual void OnLostFocus(EventArgs e)
        {
            base.RaiseEvent(EventLostFocus, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
        }

        protected override void OnParentChanged(ToolStrip oldParent, ToolStrip newParent)
        {
            if (((oldParent != null) && (base.Owner == null)) && ((newParent == null) && (this.Control != null)))
            {
                WindowsFormsUtils.ReadOnlyControlCollection controlCollection = GetControlCollection(this.Control.ParentInternal as ToolStrip);
                if (controlCollection != null)
                {
                    controlCollection.RemoveInternal(this.Control);
                }
            }
            else
            {
                this.SyncControlParent();
            }
            base.OnParentChanged(oldParent, newParent);
        }

        protected virtual void OnSubscribeControlEvents(System.Windows.Forms.Control control)
        {
            if (control != null)
            {
                control.Click += new EventHandler(this.HandleClick);
                control.BackColorChanged += new EventHandler(this.HandleBackColorChanged);
                control.DoubleClick += new EventHandler(this.HandleDoubleClick);
                control.DragDrop += new DragEventHandler(this.HandleDragDrop);
                control.DragEnter += new DragEventHandler(this.HandleDragEnter);
                control.DragLeave += new EventHandler(this.HandleDragLeave);
                control.DragOver += new DragEventHandler(this.HandleDragOver);
                control.Enter += new EventHandler(this.HandleEnter);
                control.EnabledChanged += new EventHandler(this.HandleEnabledChanged);
                control.ForeColorChanged += new EventHandler(this.HandleForeColorChanged);
                control.GiveFeedback += new GiveFeedbackEventHandler(this.HandleGiveFeedback);
                control.GotFocus += new EventHandler(this.HandleGotFocus);
                control.Leave += new EventHandler(this.HandleLeave);
                control.LocationChanged += new EventHandler(this.HandleLocationChanged);
                control.LostFocus += new EventHandler(this.HandleLostFocus);
                control.KeyDown += new KeyEventHandler(this.HandleKeyDown);
                control.KeyPress += new KeyPressEventHandler(this.HandleKeyPress);
                control.KeyUp += new KeyEventHandler(this.HandleKeyUp);
                control.MouseDown += new MouseEventHandler(this.HandleMouseDown);
                control.MouseEnter += new EventHandler(this.HandleMouseEnter);
                control.MouseHover += new EventHandler(this.HandleMouseHover);
                control.MouseLeave += new EventHandler(this.HandleMouseLeave);
                control.MouseMove += new MouseEventHandler(this.HandleMouseMove);
                control.MouseUp += new MouseEventHandler(this.HandleMouseUp);
                control.Paint += new PaintEventHandler(this.HandlePaint);
                control.QueryAccessibilityHelp += new QueryAccessibilityHelpEventHandler(this.HandleQueryAccessibilityHelp);
                control.QueryContinueDrag += new QueryContinueDragEventHandler(this.HandleQueryContinueDrag);
                control.Resize += new EventHandler(this.HandleResize);
                control.RightToLeftChanged += new EventHandler(this.HandleRightToLeftChanged);
                control.TextChanged += new EventHandler(this.HandleTextChanged);
                control.VisibleChanged += new EventHandler(this.HandleControlVisibleChanged);
                control.Validating += new CancelEventHandler(this.HandleValidating);
                control.Validated += new EventHandler(this.HandleValidated);
            }
        }

        protected virtual void OnUnsubscribeControlEvents(System.Windows.Forms.Control control)
        {
            if (control != null)
            {
                control.Click -= new EventHandler(this.HandleClick);
                control.BackColorChanged -= new EventHandler(this.HandleBackColorChanged);
                control.DoubleClick -= new EventHandler(this.HandleDoubleClick);
                control.DragDrop -= new DragEventHandler(this.HandleDragDrop);
                control.DragEnter -= new DragEventHandler(this.HandleDragEnter);
                control.DragLeave -= new EventHandler(this.HandleDragLeave);
                control.DragOver -= new DragEventHandler(this.HandleDragOver);
                control.Enter -= new EventHandler(this.HandleEnter);
                control.EnabledChanged -= new EventHandler(this.HandleEnabledChanged);
                control.ForeColorChanged -= new EventHandler(this.HandleForeColorChanged);
                control.GiveFeedback -= new GiveFeedbackEventHandler(this.HandleGiveFeedback);
                control.GotFocus -= new EventHandler(this.HandleGotFocus);
                control.Leave -= new EventHandler(this.HandleLeave);
                control.LocationChanged -= new EventHandler(this.HandleLocationChanged);
                control.LostFocus -= new EventHandler(this.HandleLostFocus);
                control.KeyDown -= new KeyEventHandler(this.HandleKeyDown);
                control.KeyPress -= new KeyPressEventHandler(this.HandleKeyPress);
                control.KeyUp -= new KeyEventHandler(this.HandleKeyUp);
                control.MouseDown -= new MouseEventHandler(this.HandleMouseDown);
                control.MouseEnter -= new EventHandler(this.HandleMouseEnter);
                control.MouseHover -= new EventHandler(this.HandleMouseHover);
                control.MouseLeave -= new EventHandler(this.HandleMouseLeave);
                control.MouseMove -= new MouseEventHandler(this.HandleMouseMove);
                control.MouseUp -= new MouseEventHandler(this.HandleMouseUp);
                control.Paint -= new PaintEventHandler(this.HandlePaint);
                control.QueryAccessibilityHelp -= new QueryAccessibilityHelpEventHandler(this.HandleQueryAccessibilityHelp);
                control.QueryContinueDrag -= new QueryContinueDragEventHandler(this.HandleQueryContinueDrag);
                control.Resize -= new EventHandler(this.HandleResize);
                control.RightToLeftChanged -= new EventHandler(this.HandleRightToLeftChanged);
                control.TextChanged -= new EventHandler(this.HandleTextChanged);
                control.VisibleChanged -= new EventHandler(this.HandleControlVisibleChanged);
                control.Validating -= new CancelEventHandler(this.HandleValidating);
                control.Validated -= new EventHandler(this.HandleValidated);
            }
        }

        protected virtual void OnValidated(EventArgs e)
        {
            base.RaiseEvent(EventValidated, e);
        }

        protected virtual void OnValidating(CancelEventArgs e)
        {
            base.RaiseCancelEvent(EventValidating, e);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected internal override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            return false;
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessDialogKey(Keys keyData)
        {
            return false;
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows), UIPermission(SecurityAction.InheritanceDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if (this.control != null)
            {
                return this.control.ProcessMnemonic(charCode);
            }
            return base.ProcessMnemonic(charCode);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ResetBackColor()
        {
            this.Control.ResetBackColor();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ResetForeColor()
        {
            this.Control.ResetForeColor();
        }

        private void ResumeSizeSync()
        {
            this.suspendSyncSizeCount--;
        }

        protected override void SetVisibleCore(bool visible)
        {
            if (!this.inSetVisibleCore)
            {
                this.inSetVisibleCore = true;
                this.Control.SuspendLayout();
                try
                {
                    this.Control.Visible = visible;
                }
                finally
                {
                    this.Control.ResumeLayout(false);
                    base.SetVisibleCore(visible);
                    this.inSetVisibleCore = false;
                }
            }
        }

        internal override bool ShouldSerializeBackColor()
        {
            if (this.control != null)
            {
                return this.control.ShouldSerializeBackColor();
            }
            return base.ShouldSerializeBackColor();
        }

        internal override bool ShouldSerializeFont()
        {
            if (this.control != null)
            {
                return this.control.ShouldSerializeFont();
            }
            return base.ShouldSerializeFont();
        }

        internal override bool ShouldSerializeForeColor()
        {
            if (this.control != null)
            {
                return this.control.ShouldSerializeForeColor();
            }
            return base.ShouldSerializeForeColor();
        }

        internal override bool ShouldSerializeRightToLeft()
        {
            if (this.control != null)
            {
                return this.control.ShouldSerializeRightToLeft();
            }
            return base.ShouldSerializeRightToLeft();
        }

        private void SuspendSizeSync()
        {
            this.suspendSyncSizeCount++;
        }

        private void SyncControlParent()
        {
            WindowsFormsUtils.ReadOnlyControlCollection controlCollection = GetControlCollection(base.ParentInternal);
            if (controlCollection != null)
            {
                controlCollection.AddInternal(this.Control);
            }
        }

        public override Color BackColor
        {
            get
            {
                return this.Control.BackColor;
            }
            set
            {
                this.Control.BackColor = value;
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), System.Windows.Forms.SRDescription("ToolStripItemImageDescr")]
        public override System.Drawing.Image BackgroundImage
        {
            get
            {
                return this.Control.BackgroundImage;
            }
            set
            {
                this.Control.BackgroundImage = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(1), System.Windows.Forms.SRDescription("ControlBackgroundImageLayoutDescr"), Localizable(true)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return this.Control.BackgroundImageLayout;
            }
            set
            {
                this.Control.BackgroundImageLayout = value;
            }
        }

        public override bool CanSelect
        {
            get
            {
                if (this.control == null)
                {
                    return false;
                }
                if (!base.DesignMode)
                {
                    return this.Control.CanSelect;
                }
                return true;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatFocus"), System.Windows.Forms.SRDescription("ControlCausesValidationDescr")]
        public bool CausesValidation
        {
            get
            {
                return this.Control.CausesValidation;
            }
            set
            {
                this.Control.CausesValidation = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Windows.Forms.Control Control
        {
            get
            {
                return this.control;
            }
        }

        [Browsable(false), DefaultValue(0x20)]
        public ContentAlignment ControlAlign
        {
            get
            {
                return this.controlAlign;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidContentAlignment(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ContentAlignment));
                }
                if (this.controlAlign != value)
                {
                    this.controlAlign = value;
                    this.OnBoundsChanged();
                }
            }
        }

        protected override System.Drawing.Size DefaultSize
        {
            get
            {
                if (this.Control != null)
                {
                    return this.Control.Size;
                }
                return base.DefaultSize;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public ToolStripItemDisplayStyle DisplayStyle
        {
            get
            {
                return base.DisplayStyle;
            }
            set
            {
                base.DisplayStyle = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DefaultValue(false), Browsable(false)]
        public bool DoubleClickEnabled
        {
            get
            {
                return base.DoubleClickEnabled;
            }
            set
            {
                base.DoubleClickEnabled = value;
            }
        }

        public override bool Enabled
        {
            get
            {
                return this.Control.Enabled;
            }
            set
            {
                this.Control.Enabled = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(false)]
        public virtual bool Focused
        {
            get
            {
                return this.Control.Focused;
            }
        }

        public override System.Drawing.Font Font
        {
            get
            {
                return this.Control.Font;
            }
            set
            {
                this.Control.Font = value;
            }
        }

        public override Color ForeColor
        {
            get
            {
                return this.Control.ForeColor;
            }
            set
            {
                this.Control.ForeColor = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override System.Drawing.Image Image
        {
            get
            {
                return base.Image;
            }
            set
            {
                base.Image = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public ContentAlignment ImageAlign
        {
            get
            {
                return base.ImageAlign;
            }
            set
            {
                base.ImageAlign = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ToolStripItemImageScaling ImageScaling
        {
            get
            {
                return base.ImageScaling;
            }
            set
            {
                base.ImageScaling = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Color ImageTransparentColor
        {
            get
            {
                return base.ImageTransparentColor;
            }
            set
            {
                base.ImageTransparentColor = value;
            }
        }

        public override System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                if (this.control != null)
                {
                    return this.control.RightToLeft;
                }
                return base.RightToLeft;
            }
            set
            {
                if (this.control != null)
                {
                    this.control.RightToLeft = value;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RightToLeftAutoMirrorImage
        {
            get
            {
                return base.RightToLeftAutoMirrorImage;
            }
            set
            {
                base.RightToLeftAutoMirrorImage = value;
            }
        }

        public override bool Selected
        {
            get
            {
                return ((this.Control != null) && this.Control.Focused);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                base.Site = value;
                if (value != null)
                {
                    this.Control.Site = new StubSite(this.Control, this);
                }
                else
                {
                    this.Control.Site = null;
                }
            }
        }

        public override System.Drawing.Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                Rectangle empty = Rectangle.Empty;
                if (this.control != null)
                {
                    empty = this.control.Bounds;
                    empty.Size = value;
                    CommonProperties.UpdateSpecifiedBounds(this.control, empty.X, empty.Y, empty.Width, empty.Height);
                }
                base.Size = value;
                if (this.control != null)
                {
                    Rectangle bounds = this.control.Bounds;
                    if (bounds != empty)
                    {
                        CommonProperties.UpdateSpecifiedBounds(this.control, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    }
                }
            }
        }

        [DefaultValue("")]
        public override string Text
        {
            get
            {
                return this.Control.Text;
            }
            set
            {
                this.Control.Text = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public ContentAlignment TextAlign
        {
            get
            {
                return base.TextAlign;
            }
            set
            {
                base.TextAlign = value;
            }
        }

        [DefaultValue(1), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override ToolStripTextDirection TextDirection
        {
            get
            {
                return base.TextDirection;
            }
            set
            {
                base.TextDirection = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public System.Windows.Forms.TextImageRelation TextImageRelation
        {
            get
            {
                return base.TextImageRelation;
            }
            set
            {
                base.TextImageRelation = value;
            }
        }

        private class StubSite : ISite, IServiceProvider
        {
            private IComponent comp;
            private IComponent owner;

            public StubSite(Component control, Component host)
            {
                this.comp = control;
                this.owner = host;
            }

            object IServiceProvider.GetService(System.Type service)
            {
                if (service == null)
                {
                    throw new ArgumentNullException("service");
                }
                if (this.owner.Site != null)
                {
                    return this.owner.Site.GetService(service);
                }
                return null;
            }

            IComponent ISite.Component
            {
                get
                {
                    return this.comp;
                }
            }

            IContainer ISite.Container
            {
                get
                {
                    return this.owner.Site.Container;
                }
            }

            bool ISite.DesignMode
            {
                get
                {
                    return this.owner.Site.DesignMode;
                }
            }

            string ISite.Name
            {
                get
                {
                    return this.owner.Site.Name;
                }
                set
                {
                    this.owner.Site.Name = value;
                }
            }
        }
    }
}

