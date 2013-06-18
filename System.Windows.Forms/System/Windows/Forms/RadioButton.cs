namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.ButtonInternal;
    using System.Windows.Forms.Layout;

    [Designer("System.Windows.Forms.Design.RadioButtonDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ToolboxItem("System.Windows.Forms.Design.AutoSizeToolboxItem,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultProperty("Checked"), DefaultEvent("CheckedChanged"), DefaultBindingProperty("Checked"), System.Windows.Forms.SRDescription("DescriptionRadioButton")]
    public class RadioButton : ButtonBase
    {
        private static readonly ContentAlignment anyRight = (ContentAlignment.BottomRight | ContentAlignment.MiddleRight | ContentAlignment.TopRight);
        private System.Windows.Forms.Appearance appearance;
        private bool autoCheck = true;
        private ContentAlignment checkAlign = ContentAlignment.MiddleLeft;
        private static readonly object EVENT_APPEARANCECHANGED = new object();
        private static readonly object EVENT_CHECKEDCHANGED = new object();
        private bool firstfocus = true;
        private bool isChecked;

        [System.Windows.Forms.SRDescription("RadioButtonOnAppearanceChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
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

        [System.Windows.Forms.SRDescription("RadioButtonOnCheckedChangedDescr")]
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

        public RadioButton()
        {
            base.SetStyle(ControlStyles.StandardClick, false);
            this.TextAlign = ContentAlignment.MiddleLeft;
            this.TabStop = false;
            base.SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new RadioButtonAccessibleObject(this);
        }

        internal override ButtonBaseAdapter CreateFlatAdapter()
        {
            return new RadioButtonFlatAdapter(this);
        }

        internal override ButtonBaseAdapter CreatePopupAdapter()
        {
            return new RadioButtonPopupAdapter(this);
        }

        internal override ButtonBaseAdapter CreateStandardAdapter()
        {
            return new RadioButtonStandardAdapter(this);
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            if (base.FlatStyle != FlatStyle.System)
            {
                return base.GetPreferredSizeCore(proposedConstraints);
            }
            Size clientSize = TextRenderer.MeasureText(this.Text, this.Font);
            Size size2 = this.SizeFromClientSize(clientSize);
            size2.Width += 0x18;
            size2.Height += 5;
            return size2;
        }

        private void OnAppearanceChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_APPEARANCECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            base.AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
            base.AccessibilityNotifyClients(AccessibleEvents.NameChange, -1);
            EventHandler handler = (EventHandler) base.Events[EVENT_CHECKEDCHANGED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.autoCheck)
            {
                this.Checked = true;
            }
            base.OnClick(e);
        }

        protected override void OnEnter(EventArgs e)
        {
            if (Control.MouseButtons == MouseButtons.None)
            {
                if (System.Windows.Forms.UnsafeNativeMethods.GetKeyState(9) >= 0)
                {
                    base.ResetFlagsandPaint();
                    if (!base.ValidationCancelled)
                    {
                        this.OnClick(e);
                    }
                }
                else
                {
                    this.PerformAutoUpdates(true);
                    this.TabStop = true;
                }
            }
            base.OnEnter(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (base.IsHandleCreated)
            {
                base.SendMessage(0xf1, this.isChecked ? 1 : 0, 0);
            }
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            if (((mevent.Button == MouseButtons.Left) && base.GetStyle(ControlStyles.UserPaint)) && base.MouseIsDown)
            {
                Point point = base.PointToScreen(new Point(mevent.X, mevent.Y));
                if (System.Windows.Forms.UnsafeNativeMethods.WindowFromPoint(point.X, point.Y) == base.Handle)
                {
                    base.ResetFlagsandPaint();
                    if (!base.ValidationCancelled)
                    {
                        this.OnClick(mevent);
                        this.OnMouseClick(mevent);
                    }
                }
            }
            base.OnMouseUp(mevent);
        }

        private void PerformAutoUpdates(bool tabbedInto)
        {
            if (this.autoCheck)
            {
                if (this.firstfocus)
                {
                    this.WipeTabStops(tabbedInto);
                }
                this.TabStop = this.isChecked;
                if (this.isChecked)
                {
                    Control parentInternal = this.ParentInternal;
                    if (parentInternal != null)
                    {
                        Control.ControlCollection controls = parentInternal.Controls;
                        for (int i = 0; i < controls.Count; i++)
                        {
                            Control control2 = controls[i];
                            if ((control2 != this) && (control2 is RadioButton))
                            {
                                RadioButton component = (RadioButton) control2;
                                if (component.autoCheck && component.Checked)
                                {
                                    TypeDescriptor.GetProperties(this)["Checked"].SetValue(component, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void PerformClick()
        {
            if (base.CanSelect)
            {
                base.ResetFlagsandPaint();
                if (!base.ValidationCancelled)
                {
                    this.OnClick(EventArgs.Empty);
                }
            }
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if ((!base.UseMnemonic || !Control.IsMnemonic(charCode, this.Text)) || !base.CanSelect)
            {
                return false;
            }
            if (!this.Focused)
            {
                this.FocusInternal();
            }
            else
            {
                this.PerformClick();
            }
            return true;
        }

        public override string ToString()
        {
            return (base.ToString() + ", Checked: " + this.Checked.ToString());
        }

        private void WipeTabStops(bool tabbedInto)
        {
            Control parentInternal = this.ParentInternal;
            if (parentInternal != null)
            {
                Control.ControlCollection controls = parentInternal.Controls;
                for (int i = 0; i < controls.Count; i++)
                {
                    Control control2 = controls[i];
                    if (control2 is RadioButton)
                    {
                        RadioButton button = (RadioButton) control2;
                        if (!tabbedInto)
                        {
                            button.firstfocus = false;
                        }
                        if (button.autoCheck)
                        {
                            button.TabStop = false;
                        }
                    }
                }
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), System.Windows.Forms.SRDescription("RadioButtonAppearanceDescr")]
        public System.Windows.Forms.Appearance Appearance
        {
            get
            {
                return this.appearance;
            }
            set
            {
                if (this.appearance != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.Appearance));
                    }
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

        [System.Windows.Forms.SRDescription("RadioButtonAutoCheckDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool AutoCheck
        {
            get
            {
                return this.autoCheck;
            }
            set
            {
                if (this.autoCheck != value)
                {
                    this.autoCheck = value;
                    this.PerformAutoUpdates(false);
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("RadioButtonCheckAlignDescr"), DefaultValue(0x10)]
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
                this.checkAlign = value;
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

        [DefaultValue(false), System.Windows.Forms.SRDescription("RadioButtonCheckedDescr"), SettingsBindable(true), System.Windows.Forms.SRCategory("CatAppearance"), Bindable(true)]
        public bool Checked
        {
            get
            {
                return this.isChecked;
            }
            set
            {
                if (this.isChecked != value)
                {
                    this.isChecked = value;
                    if (base.IsHandleCreated)
                    {
                        base.SendMessage(0xf1, value ? 1 : 0, 0);
                    }
                    base.Invalidate();
                    base.Update();
                    this.PerformAutoUpdates(false);
                    this.OnCheckedChanged(EventArgs.Empty);
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
                createParams.Style |= 4;
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

        [DefaultValue(0x10), Localizable(true)]
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

        [ComVisible(true)]
        public class RadioButtonAccessibleObject : ButtonBase.ButtonBaseAccessibleObject
        {
            public RadioButtonAccessibleObject(RadioButton owner) : base(owner)
            {
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                ((RadioButton) base.Owner).PerformClick();
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
                    return AccessibleRole.RadioButton;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    if (((RadioButton) base.Owner).Checked)
                    {
                        return (AccessibleStates.Checked | base.State);
                    }
                    return base.State;
                }
            }
        }
    }
}

