namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.ButtonInternal;
    using System.Windows.Forms.Layout;

    [System.Windows.Forms.SRDescription("DescriptionButton"), ClassInterface(ClassInterfaceType.AutoDispatch), Designer("System.Windows.Forms.Design.ButtonBaseDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ComVisible(true)]
    public class Button : ButtonBase, IButtonControl
    {
        private System.Windows.Forms.DialogResult dialogResult;
        private Size systemSize = new Size(-2147483648, -2147483648);

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
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

        public Button()
        {
            base.SetStyle(ControlStyles.StandardDoubleClick | ControlStyles.StandardClick, false);
        }

        internal override ButtonBaseAdapter CreateFlatAdapter()
        {
            return new ButtonFlatAdapter(this);
        }

        internal override ButtonBaseAdapter CreatePopupAdapter()
        {
            return new ButtonPopupAdapter(this);
        }

        internal override ButtonBaseAdapter CreateStandardAdapter()
        {
            return new ButtonStandardAdapter(this);
        }

        internal override Size GetPreferredSizeCore(Size proposedConstraints)
        {
            if (base.FlatStyle != FlatStyle.System)
            {
                Size preferredSizeCore = base.GetPreferredSizeCore(proposedConstraints);
                if (this.AutoSizeMode != System.Windows.Forms.AutoSizeMode.GrowAndShrink)
                {
                    return LayoutUtils.UnionSizes(preferredSizeCore, base.Size);
                }
                return preferredSizeCore;
            }
            if (this.systemSize.Width == -2147483648)
            {
                Size clientSize = TextRenderer.MeasureText(this.Text, this.Font);
                clientSize = this.SizeFromClientSize(clientSize);
                clientSize.Width += 14;
                clientSize.Height += 9;
                this.systemSize = clientSize;
            }
            Size a = this.systemSize + base.Padding.Size;
            if (this.AutoSizeMode != System.Windows.Forms.AutoSizeMode.GrowAndShrink)
            {
                return LayoutUtils.UnionSizes(a, base.Size);
            }
            return a;
        }

        public virtual void NotifyDefault(bool value)
        {
            if (base.IsDefault != value)
            {
                base.IsDefault = value;
            }
        }

        protected override void OnClick(EventArgs e)
        {
            Form form = base.FindFormInternal();
            if (form != null)
            {
                form.DialogResult = this.dialogResult;
            }
            base.AccessibilityNotifyClients(AccessibleEvents.StateChange, -1);
            base.AccessibilityNotifyClients(AccessibleEvents.NameChange, -1);
            base.OnClick(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            this.systemSize = new Size(-2147483648, -2147483648);
            base.OnFontChanged(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            if ((mevent.Button == MouseButtons.Left) && base.MouseIsPressed)
            {
                bool mouseIsDown = base.MouseIsDown;
                if (base.GetStyle(ControlStyles.UserPaint))
                {
                    base.ResetFlagsandPaint();
                }
                if (mouseIsDown)
                {
                    Point point = base.PointToScreen(new Point(mevent.X, mevent.Y));
                    if ((System.Windows.Forms.UnsafeNativeMethods.WindowFromPoint(point.X, point.Y) == base.Handle) && !base.ValidationCancelled)
                    {
                        if (base.GetStyle(ControlStyles.UserPaint))
                        {
                            this.OnClick(mevent);
                        }
                        this.OnMouseClick(mevent);
                    }
                }
            }
            base.OnMouseUp(mevent);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            this.systemSize = new Size(-2147483648, -2147483648);
            base.OnTextChanged(e);
        }

        public void PerformClick()
        {
            if (base.CanSelect)
            {
                bool flag;
                bool flag2 = base.ValidateActiveControl(out flag);
                if (!base.ValidationCancelled && (flag2 || flag))
                {
                    base.ResetFlagsandPaint();
                    this.OnClick(EventArgs.Empty);
                }
            }
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if ((base.UseMnemonic && this.CanProcessMnemonic()) && Control.IsMnemonic(charCode, this.Text))
            {
                this.PerformClick();
                return true;
            }
            return base.ProcessMnemonic(charCode);
        }

        public override string ToString()
        {
            return (base.ToString() + ", Text: " + this.Text);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 20:
                    this.DefWndProc(ref m);
                    return;

                case 0x2111:
                    if ((System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam) == 0) && !base.ValidationCancelled)
                    {
                        this.OnClick(EventArgs.Empty);
                        return;
                    }
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), Browsable(true), System.Windows.Forms.SRDescription("ControlAutoSizeModeDescr"), Localizable(true), DefaultValue(1)]
        public System.Windows.Forms.AutoSizeMode AutoSizeMode
        {
            get
            {
                return base.GetAutoSizeMode();
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.AutoSizeMode));
                }
                if (base.GetAutoSizeMode() != value)
                {
                    base.SetAutoSizeMode(value);
                    if (this.ParentInternal != null)
                    {
                        if (this.ParentInternal.LayoutEngine == DefaultLayout.Instance)
                        {
                            this.ParentInternal.LayoutEngine.InitLayout(this, BoundsSpecified.Size);
                        }
                        LayoutTransaction.DoLayout(this.ParentInternal, this, PropertyNames.AutoSize);
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
                createParams.ClassName = "BUTTON";
                if (base.GetStyle(ControlStyles.UserPaint))
                {
                    createParams.Style |= 11;
                    return createParams;
                }
                createParams.Style = createParams.Style;
                if (base.IsDefault)
                {
                    createParams.Style |= 1;
                }
                return createParams;
            }
        }

        [System.Windows.Forms.SRDescription("ButtonDialogResultDescr"), DefaultValue(0), System.Windows.Forms.SRCategory("CatBehavior")]
        public virtual System.Windows.Forms.DialogResult DialogResult
        {
            get
            {
                return this.dialogResult;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 7))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.DialogResult));
                }
                this.dialogResult = value;
            }
        }
    }
}

