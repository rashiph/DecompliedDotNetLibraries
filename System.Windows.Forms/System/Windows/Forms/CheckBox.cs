namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.ButtonInternal;
    using System.Windows.Forms.Layout;

    [DefaultEvent("CheckedChanged"), ComVisible(true), DefaultProperty("Checked"), ToolboxItem("System.Windows.Forms.Design.AutoSizeToolboxItem,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ClassInterface(ClassInterfaceType.AutoDispatch), System.Windows.Forms.SRDescription("DescriptionCheckBox"), DefaultBindingProperty("CheckState")]
    public class CheckBox : ButtonBase
    {
        private bool accObjDoDefaultAction;
        private static readonly ContentAlignment anyRight = (ContentAlignment.BottomRight | ContentAlignment.MiddleRight | ContentAlignment.TopRight);
        private System.Windows.Forms.Appearance appearance;
        private bool autoCheck;
        private ContentAlignment checkAlign = ContentAlignment.MiddleLeft;
        private System.Windows.Forms.CheckState checkState;
        private static readonly object EVENT_APPEARANCECHANGED = new object();
        private static readonly object EVENT_CHECKEDCHANGED = new object();
        private static readonly object EVENT_CHECKSTATECHANGED = new object();
        private bool threeState;

        [System.Windows.Forms.SRDescription("CheckBoxOnAppearanceChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler AppearanceChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_APPEARANCECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_APPEARANCECHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("CheckBoxOnCheckedChangedDescr")]
        public event EventHandler CheckedChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_CHECKEDCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CHECKEDCHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("CheckBoxOnCheckStateChangedDescr")]
        public event EventHandler CheckStateChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_CHECKSTATECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CHECKSTATECHANGED, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler DoubleClick
        {
            add
            {
                base.DoubleClick += value;
            }
            remove
            {
                base.DoubleClick -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event MouseEventHandler MouseDoubleClick
        {
            add
            {
                base.MouseDoubleClick += value;
            }
            remove
            {
                base.MouseDoubleClick -= value;
            }
        }

        public CheckBox()
        {
            base.SetStyle(ControlStyles.StandardDoubleClick | ControlStyles.StandardClick, false);
            base.SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
            this.autoCheck = true;
            this.TextAlign = ContentAlignment.MiddleLeft;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new CheckBoxAccessibleObject(this);
        }

        internal override ButtonBaseAdapter CreateFlatAdapter()
        {
            return new CheckBoxFlatAdapter(this);
        }

        internal override ButtonBaseAdapter CreatePopupAdapter()
        {
            return new CheckBoxPopupAdapter(this);
        }

        internal override ButtonBaseAdapter CreateStandardAdapter()
        {
            return new CheckBoxStandardAdapter(this);
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            if (this.Appearance == System.Windows.Forms.Appearance.Button)
            {
                ButtonStandardAdapter adapter = new ButtonStandardAdapter(this);
                return adapter.GetPreferredSizeCore(proposedConstraints);
            }
            if (base.FlatStyle != FlatStyle.System)
            {
                return base.GetPreferredSizeCore(proposedConstraints);
            }
            Size clientSize = TextRenderer.MeasureText(this.Text, this.Font);
            Size size2 = this.SizeFromClientSize(clientSize);
            size2.Width += 0x19;
            size2.Height += 5;
            return (size2 + base.Padding.Size);
        }

        protected virtual void OnAppearanceChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_APPEARANCECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            if (base.FlatStyle == FlatStyle.System)
            {
                base.AccessibilityNotifyClients(AccessibleEvents.SystemCaptureStart, -1);
            }
            base.AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
            base.AccessibilityNotifyClients(AccessibleEvents.NameChange, -1);
            if (base.FlatStyle == FlatStyle.System)
            {
                base.AccessibilityNotifyClients(AccessibleEvents.SystemCaptureEnd, -1);
            }
            EventHandler handler = (EventHandler) base.Events[EVENT_CHECKEDCHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCheckStateChanged(EventArgs e)
        {
            if (base.OwnerDraw)
            {
                this.Refresh();
            }
            EventHandler handler = (EventHandler) base.Events[EVENT_CHECKSTATECHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.autoCheck)
            {
                switch (this.CheckState)
                {
                    case System.Windows.Forms.CheckState.Unchecked:
                        this.CheckState = System.Windows.Forms.CheckState.Checked;
                        break;

                    case System.Windows.Forms.CheckState.Checked:
                        if (!this.threeState)
                        {
                            this.CheckState = System.Windows.Forms.CheckState.Unchecked;
                            break;
                        }
                        this.CheckState = System.Windows.Forms.CheckState.Indeterminate;
                        if (this.AccObjDoDefaultAction)
                        {
                            base.AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
                        }
                        break;

                    default:
                        this.CheckState = System.Windows.Forms.CheckState.Unchecked;
                        break;
                }
            }
            base.OnClick(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (base.IsHandleCreated)
            {
                base.SendMessage(0xf1, (int) this.checkState, 0);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            if (((mevent.Button == MouseButtons.Left) && base.MouseIsPressed) && base.MouseIsDown)
            {
                Point point = base.PointToScreen(new Point(mevent.X, mevent.Y));
                if (System.Windows.Forms.UnsafeNativeMethods.WindowFromPoint(point.X, point.Y) == base.Handle)
                {
                    base.ResetFlagsandPaint();
                    if (!base.ValidationCancelled)
                    {
                        if (base.Capture)
                        {
                            this.OnClick(mevent);
                        }
                        this.OnMouseClick(mevent);
                    }
                }
            }
            base.OnMouseUp(mevent);
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if ((!base.UseMnemonic || !Control.IsMnemonic(charCode, this.Text)) || !base.CanSelect)
            {
                return false;
            }
            if (this.FocusInternal())
            {
                base.ResetFlagsandPaint();
                if (!base.ValidationCancelled)
                {
                    this.OnClick(EventArgs.Empty);
                }
            }
            return true;
        }

        public override string ToString()
        {
            string str = base.ToString();
            int checkState = (int) this.CheckState;
            return (str + ", CheckState: " + checkState.ToString(CultureInfo.InvariantCulture));
        }

        private bool AccObjDoDefaultAction
        {
            get
            {
                return this.accObjDoDefaultAction;
            }
            set
            {
                this.accObjDoDefaultAction = value;
            }
        }

        [System.Windows.Forms.SRDescription("CheckBoxAppearanceDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0), Localizable(true)]
        public System.Windows.Forms.Appearance Appearance
        {
            get
            {
                return this.appearance;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.Appearance));
                }
                if (this.appearance != value)
                {
                    using (LayoutTransaction.CreateTransactionIf(this.AutoSize, this.ParentInternal, this, PropertyNames.Appearance))
                    {
                        this.appearance = value;
                        if (base.OwnerDraw)
                        {
                            this.Refresh();
                        }
                        else
                        {
                            base.UpdateStyles();
                        }
                        this.OnAppearanceChanged(EventArgs.Empty);
                    }
                }
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("CheckBoxAutoCheckDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool AutoCheck
        {
            get
            {
                return this.autoCheck;
            }
            set
            {
                this.autoCheck = value;
            }
        }

        [Bindable(true), System.Windows.Forms.SRDescription("CheckBoxCheckAlignDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0x10)]
        public ContentAlignment CheckAlign
        {
            get
            {
                return this.checkAlign;
            }
            set
            {
                if (!WindowsFormsUtils.EnumValidator.IsValidContentAlignment(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ContentAlignment));
                }
                if (this.checkAlign != value)
                {
                    this.checkAlign = value;
                    LayoutTransaction.DoLayoutIf(this.AutoSize, this.ParentInternal, this, PropertyNames.CheckAlign);
                    if (base.OwnerDraw)
                    {
                        base.Invalidate();
                    }
                    else
                    {
                        base.UpdateStyles();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("CheckBoxCheckedDescr"), System.Windows.Forms.SRCategory("CatAppearance"), RefreshProperties(RefreshProperties.All), SettingsBindable(true), DefaultValue(false), Bindable(true)]
        public bool Checked
        {
            get
            {
                return (this.checkState != System.Windows.Forms.CheckState.Unchecked);
            }
            set
            {
                if (value != this.Checked)
                {
                    this.CheckState = value ? System.Windows.Forms.CheckState.Checked : System.Windows.Forms.CheckState.Unchecked;
                }
            }
        }

        [Bindable(true), System.Windows.Forms.SRDescription("CheckBoxCheckStateDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0), RefreshProperties(RefreshProperties.All)]
        public System.Windows.Forms.CheckState CheckState
        {
            get
            {
                return this.checkState;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.CheckState));
                }
                if (this.checkState != value)
                {
                    bool flag = this.Checked;
                    this.checkState = value;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0xf1, (int) this.checkState, 0);
                    }
                    if (flag != this.Checked)
                    {
                        this.OnCheckedChanged(EventArgs.Empty);
                    }
                    this.OnCheckStateChanged(EventArgs.Empty);
                }
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "BUTTON";
                if (base.OwnerDraw)
                {
                    createParams.Style |= 11;
                    return createParams;
                }
                createParams.Style |= 5;
                if (this.Appearance == System.Windows.Forms.Appearance.Button)
                {
                    createParams.Style |= 0x1000;
                }
                if ((base.RtlTranslateContent(this.CheckAlign) & anyRight) != ((ContentAlignment) 0))
                {
                    createParams.Style |= 0x20;
                }
                return createParams;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(0x68, 0x18);
            }
        }

        internal override Rectangle DownChangeRectangle
        {
            get
            {
                if ((this.Appearance != System.Windows.Forms.Appearance.Button) && (base.FlatStyle != FlatStyle.System))
                {
                    return base.Adapter.CommonLayout().Layout().checkBounds;
                }
                return base.DownChangeRectangle;
            }
        }

        internal override Rectangle OverChangeRectangle
        {
            get
            {
                if (this.Appearance == System.Windows.Forms.Appearance.Button)
                {
                    return base.OverChangeRectangle;
                }
                if (base.FlatStyle == FlatStyle.Standard)
                {
                    return new Rectangle(-1, -1, 1, 1);
                }
                return base.Adapter.CommonLayout().Layout().checkBounds;
            }
        }

        [Localizable(true), DefaultValue(0x10)]
        public override ContentAlignment TextAlign
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

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("CheckBoxThreeStateDescr"), DefaultValue(false)]
        public bool ThreeState
        {
            get
            {
                return this.threeState;
            }
            set
            {
                this.threeState = value;
            }
        }

        [ComVisible(true)]
        public class CheckBoxAccessibleObject : ButtonBase.ButtonBaseAccessibleObject
        {
            public CheckBoxAccessibleObject(Control owner) : base(owner)
            {
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                CheckBox owner = base.Owner as CheckBox;
                if (owner != null)
                {
                    owner.AccObjDoDefaultAction = true;
                }
                try
                {
                    base.DoDefaultAction();
                }
                finally
                {
                    if (owner != null)
                    {
                        owner.AccObjDoDefaultAction = false;
                    }
                }
            }

            public override string DefaultAction
            {
                get
                {
                    string accessibleDefaultActionDescription = base.Owner.AccessibleDefaultActionDescription;
                    if (accessibleDefaultActionDescription != null)
                    {
                        return accessibleDefaultActionDescription;
                    }
                    if (((CheckBox) base.Owner).Checked)
                    {
                        return System.Windows.Forms.SR.GetString("AccessibleActionUncheck");
                    }
                    return System.Windows.Forms.SR.GetString("AccessibleActionCheck");
                }
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
                    return AccessibleRole.CheckButton;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    switch (((CheckBox) base.Owner).CheckState)
                    {
                        case CheckState.Checked:
                            return (AccessibleStates.Checked | base.State);

                        case CheckState.Indeterminate:
                            return (AccessibleStates.Indeterminate | base.State);
                    }
                    return base.State;
                }
            }
        }
    }
}

