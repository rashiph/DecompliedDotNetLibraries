namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.Layout;

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class ContainerControl : ScrollableControl, IContainerControl
    {
        private Control activeControl;
        private SizeF autoScaleDimensions = SizeF.Empty;
        private System.Windows.Forms.AutoScaleMode autoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        private System.Windows.Forms.AutoValidate autoValidate = System.Windows.Forms.AutoValidate.Inherit;
        private SizeF currentAutoScaleDimensions = SizeF.Empty;
        private Control focusedControl;
        private const string fontMeasureString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly int PropAxContainer = PropertyStore.CreateKey();
        private BitVector32 state = new BitVector32();
        private static readonly int stateParentChanged = BitVector32.CreateMask(stateScalingChild);
        private static readonly int stateProcessingMnemonic = BitVector32.CreateMask(stateValidating);
        private static readonly int stateScalingChild = BitVector32.CreateMask(stateProcessingMnemonic);
        private static readonly int stateScalingNeededOnLayout = BitVector32.CreateMask();
        private static readonly int stateValidating = BitVector32.CreateMask(stateScalingNeededOnLayout);
        private Control unvalidatedControl;

        [Browsable(false), System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ContainerControlOnAutoValidateChangedDescr"), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler AutoValidateChanged;

        public ContainerControl()
        {
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, false);
            base.SetState2(0x800, true);
        }

        internal bool ActivateControlInternal(Control control)
        {
            return this.ActivateControlInternal(control, true);
        }

        internal bool ActivateControlInternal(Control control, bool originator)
        {
            bool flag = true;
            bool flag2 = false;
            ContainerControl containerControlInternal = null;
            Control parentInternal = this.ParentInternal;
            if (parentInternal != null)
            {
                containerControlInternal = parentInternal.GetContainerControlInternal() as ContainerControl;
                if (containerControlInternal != null)
                {
                    flag2 = containerControlInternal.ActiveControl != this;
                }
            }
            if ((control != this.activeControl) || flag2)
            {
                if (flag2 && !containerControlInternal.ActivateControlInternal(this, false))
                {
                    return false;
                }
                flag = this.AssignActiveControlInternal((control == this) ? null : control);
            }
            if (originator)
            {
                this.ScrollActiveControlIntoView();
            }
            return flag;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void AdjustFormScrollbars(bool displayScrollbars)
        {
            base.AdjustFormScrollbars(displayScrollbars);
            if (!base.GetScrollState(8))
            {
                this.ScrollActiveControlIntoView();
            }
        }

        internal virtual void AfterControlRemoved(Control control, Control oldParent)
        {
            ContainerControl containerControlInternal;
            if ((control == this.activeControl) || control.Contains(this.activeControl))
            {
                bool flag;
                System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                try
                {
                    flag = base.SelectNextControl(control, true, true, true, true);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if (flag)
                {
                    this.FocusActiveControlInternal();
                }
                else
                {
                    this.SetActiveControlInternal(null);
                }
            }
            else if ((this.activeControl == null) && (this.ParentInternal != null))
            {
                containerControlInternal = this.ParentInternal.GetContainerControlInternal() as ContainerControl;
                if ((containerControlInternal != null) && (containerControlInternal.ActiveControl == this))
                {
                    Form form = base.FindFormInternal();
                    if (form != null)
                    {
                        System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                        try
                        {
                            form.SelectNextControl(this, true, true, true, true);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                }
            }
            containerControlInternal = this;
            while (containerControlInternal != null)
            {
                Control parentInternal = containerControlInternal.ParentInternal;
                if (parentInternal == null)
                {
                    break;
                }
                containerControlInternal = parentInternal.GetContainerControlInternal() as ContainerControl;
                if (((containerControlInternal != null) && (containerControlInternal.unvalidatedControl != null)) && ((containerControlInternal.unvalidatedControl == control) || control.Contains(containerControlInternal.unvalidatedControl)))
                {
                    containerControlInternal.unvalidatedControl = oldParent;
                }
            }
            if ((control == this.unvalidatedControl) || control.Contains(this.unvalidatedControl))
            {
                this.unvalidatedControl = null;
            }
        }

        private bool AssignActiveControlInternal(Control value)
        {
            if (this.activeControl != value)
            {
                try
                {
                    if (value != null)
                    {
                        value.BecomingActiveControl = true;
                    }
                    this.activeControl = value;
                    this.UpdateFocusedControl();
                }
                finally
                {
                    if (value != null)
                    {
                        value.BecomingActiveControl = false;
                    }
                }
                if (this.activeControl == value)
                {
                    Form form = base.FindFormInternal();
                    if (form != null)
                    {
                        form.UpdateDefaultButton();
                    }
                }
            }
            else
            {
                this.focusedControl = this.activeControl;
            }
            return (this.activeControl == value);
        }

        private void AxContainerFormCreated()
        {
            ((AxHost.AxContainer) base.Properties.GetObject(PropAxContainer)).FormCreated();
        }

        internal override bool CanProcessMnemonic()
        {
            return (this.state[stateProcessingMnemonic] || base.CanProcessMnemonic());
        }

        internal AxHost.AxContainer CreateAxContainer()
        {
            object obj2 = base.Properties.GetObject(PropAxContainer);
            if (obj2 == null)
            {
                obj2 = new AxHost.AxContainer(this);
                base.Properties.SetObject(PropAxContainer, obj2);
            }
            return (AxHost.AxContainer) obj2;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.activeControl = null;
            }
            base.Dispose(disposing);
            this.focusedControl = null;
            this.unvalidatedControl = null;
        }

        private void EnableRequiredScaling(Control start, bool enable)
        {
            start.RequiredScalingEnabled = enable;
            foreach (Control control in start.Controls)
            {
                this.EnableRequiredScaling(control, enable);
            }
        }

        private void EnsureUnvalidatedControl(Control candidate)
        {
            if (((!this.state[stateValidating] && (this.unvalidatedControl == null)) && (candidate != null)) && candidate.ShouldAutoValidate)
            {
                this.unvalidatedControl = candidate;
                while (this.unvalidatedControl is ContainerControl)
                {
                    ContainerControl unvalidatedControl = this.unvalidatedControl as ContainerControl;
                    if ((unvalidatedControl.unvalidatedControl != null) && unvalidatedControl.unvalidatedControl.ShouldAutoValidate)
                    {
                        this.unvalidatedControl = unvalidatedControl.unvalidatedControl;
                    }
                    else
                    {
                        if ((unvalidatedControl.activeControl == null) || !unvalidatedControl.activeControl.ShouldAutoValidate)
                        {
                            break;
                        }
                        this.unvalidatedControl = unvalidatedControl.activeControl;
                    }
                }
            }
        }

        private void EnterValidation(Control enterControl)
        {
            if ((this.unvalidatedControl != null) && enterControl.CausesValidation)
            {
                System.Windows.Forms.AutoValidate autoValidateForControl = Control.GetAutoValidateForControl(this.unvalidatedControl);
                if (autoValidateForControl != System.Windows.Forms.AutoValidate.Disable)
                {
                    Control ancestorControl = enterControl;
                    while ((ancestorControl != null) && !ancestorControl.IsDescendant(this.unvalidatedControl))
                    {
                        ancestorControl = ancestorControl.ParentInternal;
                    }
                    bool preventFocusChangeOnError = autoValidateForControl == System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
                    this.ValidateThroughAncestor(ancestorControl, preventFocusChangeOnError);
                }
            }
        }

        private ScrollableControl FindScrollableParent(Control ctl)
        {
            Control parentInternal = ctl.ParentInternal;
            while ((parentInternal != null) && !(parentInternal is ScrollableControl))
            {
                parentInternal = parentInternal.ParentInternal;
            }
            if (parentInternal != null)
            {
                return (ScrollableControl) parentInternal;
            }
            return null;
        }

        internal void FocusActiveControlInternal()
        {
            if ((this.activeControl != null) && this.activeControl.Visible)
            {
                IntPtr focus = System.Windows.Forms.UnsafeNativeMethods.GetFocus();
                if ((focus == IntPtr.Zero) || (Control.FromChildHandleInternal(focus) != this.activeControl))
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(this.activeControl, this.activeControl.Handle));
                }
            }
            else
            {
                ContainerControl wrapper = this;
                while ((wrapper != null) && !wrapper.Visible)
                {
                    Control parentInternal = wrapper.ParentInternal;
                    if (parentInternal == null)
                    {
                        break;
                    }
                    wrapper = parentInternal.GetContainerControlInternal() as ContainerControl;
                }
                if ((wrapper != null) && wrapper.Visible)
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(wrapper, wrapper.Handle));
                }
            }
        }

        private SizeF GetFontAutoScaleDimensions()
        {
            SizeF empty = SizeF.Empty;
            IntPtr handle = System.Windows.Forms.UnsafeNativeMethods.CreateCompatibleDC(System.Windows.Forms.NativeMethods.NullHandleRef);
            if (handle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            HandleRef hDC = new HandleRef(this, handle);
            try
            {
                HandleRef hObject = new HandleRef(this, base.FontHandle);
                HandleRef ref4 = new HandleRef(this, System.Windows.Forms.SafeNativeMethods.SelectObject(hDC, hObject));
                try
                {
                    System.Windows.Forms.NativeMethods.TEXTMETRIC lptm = new System.Windows.Forms.NativeMethods.TEXTMETRIC();
                    System.Windows.Forms.SafeNativeMethods.GetTextMetrics(hDC, ref lptm);
                    empty.Height = lptm.tmHeight;
                    if ((lptm.tmPitchAndFamily & 1) != 0)
                    {
                        IntNativeMethods.SIZE size = new IntNativeMethods.SIZE();
                        IntUnsafeNativeMethods.GetTextExtentPoint32(hDC, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", size);
                        empty.Width = (int) Math.Round((double) (((float) size.cx) / ((float) "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Length)));
                        return empty;
                    }
                    empty.Width = lptm.tmAveCharWidth;
                    return empty;
                }
                finally
                {
                    System.Windows.Forms.SafeNativeMethods.SelectObject(hDC, ref4);
                }
            }
            finally
            {
                System.Windows.Forms.UnsafeNativeMethods.DeleteCompatibleDC(hDC);
            }
            return empty;
        }

        internal override Size GetPreferredSizeCore(Size proposedSize)
        {
            Size size2 = this.SizeFromClientSize(Size.Empty) + base.Padding.Size;
            return (this.LayoutEngine.GetPreferredSize(this, proposedSize - size2) + size2);
        }

        internal bool HasFocusableChild()
        {
            Control ctl = null;
            do
            {
                ctl = base.GetNextControl(ctl, true);
            }
            while ((((ctl == null) || !ctl.CanSelect) || !ctl.TabStop) && (ctl != null));
            return (ctl != null);
        }

        private void LayoutScalingNeeded()
        {
            this.EnableRequiredScaling(this, true);
            this.state[stateScalingNeededOnLayout] = true;
            if (!base.IsLayoutSuspended)
            {
                LayoutTransaction.DoLayout(this, this, PropertyNames.Bounds);
            }
        }

        internal virtual void OnAutoScaleModeChanged()
        {
        }

        protected virtual void OnAutoValidateChanged(EventArgs e)
        {
            if (this.autoValidateChanged != null)
            {
                this.autoValidateChanged(this, e);
            }
        }

        internal override void OnChildLayoutResuming(Control child, bool performLayout)
        {
            base.OnChildLayoutResuming(child, performLayout);
            if (((!this.state[stateScalingChild] && !performLayout) && ((this.AutoScaleMode != System.Windows.Forms.AutoScaleMode.None) && (this.AutoScaleMode != System.Windows.Forms.AutoScaleMode.Inherit))) && this.state[stateScalingNeededOnLayout])
            {
                this.state[stateScalingChild] = true;
                try
                {
                    child.Scale(this.AutoScaleFactor, SizeF.Empty, this);
                }
                finally
                {
                    this.state[stateScalingChild] = false;
                }
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (base.Properties.GetObject(PropAxContainer) != null)
            {
                this.AxContainerFormCreated();
            }
            this.OnBindingContextChanged(EventArgs.Empty);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnFontChanged(EventArgs e)
        {
            if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Font)
            {
                this.currentAutoScaleDimensions = SizeF.Empty;
                this.SuspendAllLayout(this);
                try
                {
                    this.PerformAutoScale(!base.RequiredScalingEnabled, true);
                }
                finally
                {
                    this.ResumeAllLayout(this, false);
                }
            }
            base.OnFontChanged(e);
        }

        internal override void OnFrameWindowActivate(bool fActivate)
        {
            if (fActivate)
            {
                System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                try
                {
                    if (this.ActiveControl == null)
                    {
                        base.SelectNextControl(null, true, true, true, false);
                    }
                    this.InnerMostActiveContainerControl.FocusActiveControlInternal();
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            this.PerformNeededAutoScaleOnLayout();
            base.OnLayout(e);
        }

        internal override void OnLayoutResuming(bool performLayout)
        {
            this.PerformNeededAutoScaleOnLayout();
            base.OnLayoutResuming(performLayout);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            this.state[stateParentChanged] = !base.RequiredScalingEnabled;
            base.OnParentChanged(e);
        }

        public void PerformAutoScale()
        {
            this.PerformAutoScale(true, true);
        }

        private void PerformAutoScale(bool includedBounds, bool excludedBounds)
        {
            bool flag = false;
            try
            {
                if ((this.AutoScaleMode != System.Windows.Forms.AutoScaleMode.None) && (this.AutoScaleMode != System.Windows.Forms.AutoScaleMode.Inherit))
                {
                    this.SuspendAllLayout(this);
                    flag = true;
                    SizeF empty = SizeF.Empty;
                    SizeF excludedFactor = SizeF.Empty;
                    if (includedBounds)
                    {
                        empty = this.AutoScaleFactor;
                    }
                    if (excludedBounds)
                    {
                        excludedFactor = this.AutoScaleFactor;
                    }
                    this.Scale(empty, excludedFactor, this);
                    this.autoScaleDimensions = this.CurrentAutoScaleDimensions;
                }
            }
            finally
            {
                if (includedBounds)
                {
                    this.state[stateScalingNeededOnLayout] = false;
                    this.EnableRequiredScaling(this, false);
                }
                this.state[stateParentChanged] = false;
                if (flag)
                {
                    this.ResumeAllLayout(this, false);
                }
            }
        }

        private void PerformNeededAutoScaleOnLayout()
        {
            if (this.state[stateScalingNeededOnLayout])
            {
                this.PerformAutoScale(this.state[stateScalingNeededOnLayout], false);
            }
        }

        private bool ProcessArrowKey(bool forward)
        {
            Control parentInternal = this;
            if (this.activeControl != null)
            {
                parentInternal = this.activeControl.ParentInternal;
            }
            return parentInternal.SelectNextControl(this.activeControl, forward, false, false, true);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return (base.ProcessCmdKey(ref msg, keyData) || ((this.ParentInternal == null) && ToolStripManager.ProcessCmdKey(ref msg, keyData)));
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogChar(char charCode)
        {
            return ((((base.GetContainerControlInternal() is ContainerControl) && (charCode != ' ')) && this.ProcessMnemonic(charCode)) || base.ProcessDialogChar(charCode));
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData & (Keys.Alt | Keys.Control)) == Keys.None)
            {
                Keys keys = keyData & Keys.KeyCode;
                switch (keys)
                {
                    case Keys.Left:
                    case Keys.Up:
                    case Keys.Right:
                    case Keys.Down:
                        if (!this.ProcessArrowKey((keys == Keys.Right) || (keys == Keys.Down)))
                        {
                            break;
                        }
                        return true;

                    case Keys.Tab:
                        if (this.ProcessTabKey((keyData & Keys.Shift) == Keys.None))
                        {
                            return true;
                        }
                        break;
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if (!this.CanProcessMnemonic())
            {
                return false;
            }
            if (base.Controls.Count == 0)
            {
                return false;
            }
            Control activeControl = this.ActiveControl;
            this.state[stateProcessingMnemonic] = true;
            bool flag = false;
            try
            {
                bool flag2 = false;
                Control ctl = activeControl;
                do
                {
                    ctl = base.GetNextControl(ctl, true);
                    if (ctl != null)
                    {
                        if (ctl.ProcessMnemonic(charCode))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (flag2)
                        {
                            return flag;
                        }
                        flag2 = true;
                    }
                }
                while (ctl != activeControl);
            }
            finally
            {
                this.state[stateProcessingMnemonic] = false;
            }
            return flag;
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected virtual bool ProcessTabKey(bool forward)
        {
            return base.SelectNextControl(this.activeControl, forward, true, true, false);
        }

        internal void ResetActiveAndFocusedControlsRecursive()
        {
            if (this.activeControl is ContainerControl)
            {
                ((ContainerControl) this.activeControl).ResetActiveAndFocusedControlsRecursive();
            }
            this.activeControl = null;
            this.focusedControl = null;
        }

        private void ResetValidationFlag()
        {
            Control.ControlCollection controls = base.Controls;
            int count = controls.Count;
            for (int i = 0; i < count; i++)
            {
                controls[i].ValidationCancelled = false;
            }
        }

        internal void ResumeAllLayout(Control start, bool performLayout)
        {
            Control.ControlCollection controls = start.Controls;
            for (int i = 0; i < controls.Count; i++)
            {
                this.ResumeAllLayout(controls[i], performLayout);
            }
            start.ResumeLayout(performLayout);
        }

        internal override void Scale(SizeF includedFactor, SizeF excludedFactor, Control requestingControl)
        {
            if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.Inherit)
            {
                base.Scale(includedFactor, excludedFactor, requestingControl);
            }
            else
            {
                SizeF autoScaleFactor = excludedFactor;
                SizeF ef2 = includedFactor;
                if (!autoScaleFactor.IsEmpty)
                {
                    autoScaleFactor = this.AutoScaleFactor;
                }
                if (this.AutoScaleMode == System.Windows.Forms.AutoScaleMode.None)
                {
                    ef2 = this.AutoScaleFactor;
                }
                using (new LayoutTransaction(this, this, PropertyNames.Bounds, false))
                {
                    SizeF empty = autoScaleFactor;
                    if (!excludedFactor.IsEmpty && (this.ParentInternal != null))
                    {
                        empty = SizeF.Empty;
                        bool flag = (requestingControl != this) || this.state[stateParentChanged];
                        if (!flag)
                        {
                            bool designMode = false;
                            bool flag3 = false;
                            ISite site = this.Site;
                            ISite site2 = this.ParentInternal.Site;
                            if (site != null)
                            {
                                designMode = site.DesignMode;
                            }
                            if (site2 != null)
                            {
                                flag3 = site2.DesignMode;
                            }
                            if (designMode && !flag3)
                            {
                                flag = true;
                            }
                        }
                        if (flag)
                        {
                            empty = excludedFactor;
                        }
                    }
                    base.ScaleControl(includedFactor, empty, requestingControl);
                    base.ScaleChildControls(ef2, autoScaleFactor, requestingControl);
                }
            }
        }

        private void ScrollActiveControlIntoView()
        {
            Control activeControl = this.activeControl;
            if (activeControl != null)
            {
                for (ScrollableControl control2 = this.FindScrollableParent(activeControl); control2 != null; control2 = this.FindScrollableParent(control2))
                {
                    control2.ScrollControlIntoView(this.activeControl);
                    activeControl = control2;
                }
            }
        }

        protected override void Select(bool directed, bool forward)
        {
            bool flag = true;
            if (this.ParentInternal != null)
            {
                IContainerControl containerControlInternal = this.ParentInternal.GetContainerControlInternal();
                if (containerControlInternal != null)
                {
                    containerControlInternal.ActiveControl = this;
                    flag = containerControlInternal.ActiveControl == this;
                }
            }
            if (directed && flag)
            {
                base.SelectNextControl(null, forward, true, true, false);
            }
        }

        private void SetActiveControl(Control ctl)
        {
            this.SetActiveControlInternal(ctl);
        }

        internal void SetActiveControlInternal(Control value)
        {
            if ((this.activeControl != value) || ((value != null) && !value.Focused))
            {
                bool flag;
                if ((value != null) && !base.Contains(value))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("CannotActivateControl"));
                }
                ContainerControl containerControlInternal = this;
                if ((value != null) && (value.ParentInternal != null))
                {
                    containerControlInternal = value.ParentInternal.GetContainerControlInternal() as ContainerControl;
                }
                if (containerControlInternal != null)
                {
                    flag = containerControlInternal.ActivateControlInternal(value, false);
                }
                else
                {
                    flag = this.AssignActiveControlInternal(value);
                }
                if ((containerControlInternal != null) && flag)
                {
                    ContainerControl control2 = this;
                    while ((control2.ParentInternal != null) && (control2.ParentInternal.GetContainerControlInternal() is ContainerControl))
                    {
                        control2 = control2.ParentInternal.GetContainerControlInternal() as ContainerControl;
                    }
                    if (control2.ContainsFocus && (((value == null) || !(value is UserControl)) || ((value is UserControl) && !((UserControl) value).HasFocusableChild())))
                    {
                        containerControlInternal.FocusActiveControlInternal();
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeAutoValidate()
        {
            return (this.autoValidate != System.Windows.Forms.AutoValidate.Inherit);
        }

        internal void SuspendAllLayout(Control start)
        {
            start.SuspendLayout();
            CommonProperties.xClearPreferredSizeCache(start);
            Control.ControlCollection controls = start.Controls;
            for (int i = 0; i < controls.Count; i++)
            {
                this.SuspendAllLayout(controls[i]);
            }
        }

        bool IContainerControl.ActivateControl(Control control)
        {
            System.Windows.Forms.IntSecurity.ModifyFocus.Demand();
            return this.ActivateControlInternal(control, true);
        }

        protected virtual void UpdateDefaultButton()
        {
        }

        internal void UpdateFocusedControl()
        {
            this.EnsureUnvalidatedControl(this.focusedControl);
            Control focusedControl = this.focusedControl;
            while (this.activeControl != focusedControl)
            {
                if ((focusedControl == null) || focusedControl.IsDescendant(this.activeControl))
                {
                    Control activeControl = this.activeControl;
                    while (true)
                    {
                        Control parentInternal = activeControl.ParentInternal;
                        if ((parentInternal == this) || (parentInternal == focusedControl))
                        {
                            break;
                        }
                        activeControl = activeControl.ParentInternal;
                    }
                    Control control4 = this.focusedControl = focusedControl;
                    this.EnterValidation(activeControl);
                    if (this.focusedControl != control4)
                    {
                        focusedControl = this.focusedControl;
                    }
                    else
                    {
                        focusedControl = activeControl;
                        if (NativeWindow.WndProcShouldBeDebuggable)
                        {
                            focusedControl.NotifyEnter();
                        }
                        else
                        {
                            try
                            {
                                focusedControl.NotifyEnter();
                            }
                            catch (Exception exception)
                            {
                                Application.OnThreadException(exception);
                            }
                        }
                    }
                    continue;
                }
                ContainerControl innerMostFocusedContainerControl = this.InnerMostFocusedContainerControl;
                Control control6 = null;
                if (innerMostFocusedContainerControl.focusedControl != null)
                {
                    focusedControl = innerMostFocusedContainerControl.focusedControl;
                    control6 = innerMostFocusedContainerControl;
                    if (innerMostFocusedContainerControl != this)
                    {
                        innerMostFocusedContainerControl.focusedControl = null;
                        if ((innerMostFocusedContainerControl.ParentInternal == null) || !(innerMostFocusedContainerControl.ParentInternal is MdiClient))
                        {
                            innerMostFocusedContainerControl.activeControl = null;
                        }
                    }
                }
                else
                {
                    focusedControl = innerMostFocusedContainerControl;
                    if (innerMostFocusedContainerControl.ParentInternal != null)
                    {
                        ContainerControl containerControlInternal = innerMostFocusedContainerControl.ParentInternal.GetContainerControlInternal() as ContainerControl;
                        control6 = containerControlInternal;
                        if ((containerControlInternal != null) && (containerControlInternal != this))
                        {
                            containerControlInternal.focusedControl = null;
                            containerControlInternal.activeControl = null;
                        }
                    }
                }
                do
                {
                    Control control8 = focusedControl;
                    if (focusedControl != null)
                    {
                        focusedControl = focusedControl.ParentInternal;
                    }
                    if (focusedControl == this)
                    {
                        focusedControl = null;
                    }
                    if (control8 != null)
                    {
                        if (NativeWindow.WndProcShouldBeDebuggable)
                        {
                            control8.NotifyLeave();
                        }
                        else
                        {
                            try
                            {
                                control8.NotifyLeave();
                            }
                            catch (Exception exception2)
                            {
                                Application.OnThreadException(exception2);
                            }
                        }
                    }
                }
                while (((focusedControl != null) && (focusedControl != control6)) && !focusedControl.IsDescendant(this.activeControl));
            }
            this.focusedControl = this.activeControl;
            if (this.activeControl != null)
            {
                this.EnterValidation(this.activeControl);
            }
        }

        public bool Validate()
        {
            return this.Validate(false);
        }

        public bool Validate(bool checkAutoValidate)
        {
            bool flag;
            return this.ValidateInternal(checkAutoValidate, out flag);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool ValidateChildren()
        {
            return this.ValidateChildren(ValidationConstraints.Selectable);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual bool ValidateChildren(ValidationConstraints validationConstraints)
        {
            if ((validationConstraints < ValidationConstraints.None) || (validationConstraints > (ValidationConstraints.ImmediateChildren | ValidationConstraints.TabStop | ValidationConstraints.Visible | ValidationConstraints.Enabled | ValidationConstraints.Selectable)))
            {
                throw new InvalidEnumArgumentException("validationConstraints", (int) validationConstraints, typeof(ValidationConstraints));
            }
            return !base.PerformContainerValidation(validationConstraints);
        }

        internal bool ValidateInternal(bool checkAutoValidate, out bool validatedControlAllowsFocusChange)
        {
            validatedControlAllowsFocusChange = false;
            if ((this.AutoValidate != System.Windows.Forms.AutoValidate.EnablePreventFocusChange) && ((this.activeControl == null) || !this.activeControl.CausesValidation))
            {
                return true;
            }
            if (this.unvalidatedControl == null)
            {
                if ((this.focusedControl is ContainerControl) && this.focusedControl.CausesValidation)
                {
                    ContainerControl focusedControl = (ContainerControl) this.focusedControl;
                    if (!focusedControl.ValidateInternal(checkAutoValidate, out validatedControlAllowsFocusChange))
                    {
                        return false;
                    }
                }
                else
                {
                    this.unvalidatedControl = this.focusedControl;
                }
            }
            bool preventFocusChangeOnError = true;
            Control control2 = (this.unvalidatedControl != null) ? this.unvalidatedControl : this.focusedControl;
            if (control2 != null)
            {
                System.Windows.Forms.AutoValidate autoValidateForControl = Control.GetAutoValidateForControl(control2);
                if (checkAutoValidate && (autoValidateForControl == System.Windows.Forms.AutoValidate.Disable))
                {
                    return true;
                }
                preventFocusChangeOnError = autoValidateForControl == System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
                validatedControlAllowsFocusChange = autoValidateForControl == System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            }
            return this.ValidateThroughAncestor(null, preventFocusChangeOnError);
        }

        private bool ValidateThroughAncestor(Control ancestorControl, bool preventFocusChangeOnError)
        {
            if (ancestorControl == null)
            {
                ancestorControl = this;
            }
            if (this.state[stateValidating])
            {
                return false;
            }
            if (this.unvalidatedControl == null)
            {
                this.unvalidatedControl = this.focusedControl;
            }
            if (this.unvalidatedControl == null)
            {
                return true;
            }
            if (!ancestorControl.IsDescendant(this.unvalidatedControl))
            {
                return false;
            }
            this.state[stateValidating] = true;
            bool flag = false;
            Control activeControl = this.activeControl;
            Control unvalidatedControl = this.unvalidatedControl;
            if (activeControl != null)
            {
                activeControl.ValidationCancelled = false;
                if (activeControl is ContainerControl)
                {
                    (activeControl as ContainerControl).ResetValidationFlag();
                }
            }
            try
            {
                while ((unvalidatedControl != null) && (unvalidatedControl != ancestorControl))
                {
                    try
                    {
                        flag = unvalidatedControl.PerformControlValidation(false);
                    }
                    catch
                    {
                        flag = true;
                        throw;
                    }
                    if (flag)
                    {
                        break;
                    }
                    unvalidatedControl = unvalidatedControl.ParentInternal;
                }
                if (flag && preventFocusChangeOnError)
                {
                    if (((this.unvalidatedControl == null) && (unvalidatedControl != null)) && ancestorControl.IsDescendant(unvalidatedControl))
                    {
                        this.unvalidatedControl = unvalidatedControl;
                    }
                    if ((activeControl == this.activeControl) && (activeControl != null))
                    {
                        CancelEventArgs ev = new CancelEventArgs {
                            Cancel = true
                        };
                        activeControl.NotifyValidationResult(unvalidatedControl, ev);
                        if (activeControl is ContainerControl)
                        {
                            ContainerControl control4 = activeControl as ContainerControl;
                            if (control4.focusedControl != null)
                            {
                                control4.focusedControl.ValidationCancelled = true;
                            }
                            control4.ResetActiveAndFocusedControlsRecursive();
                        }
                    }
                    this.SetActiveControlInternal(this.unvalidatedControl);
                }
            }
            finally
            {
                this.unvalidatedControl = null;
                this.state[stateValidating] = false;
            }
            return !flag;
        }

        private void WmSetFocus(ref Message m)
        {
            if (!base.HostedInWin32DialogManager)
            {
                if (this.ActiveControl != null)
                {
                    base.WmImeSetFocus();
                    if (!this.ActiveControl.Visible)
                    {
                        this.OnGotFocus(EventArgs.Empty);
                    }
                    this.FocusActiveControlInternal();
                }
                else
                {
                    if (this.ParentInternal != null)
                    {
                        IContainerControl containerControlInternal = this.ParentInternal.GetContainerControlInternal();
                        if (containerControlInternal != null)
                        {
                            bool flag = false;
                            ContainerControl control2 = containerControlInternal as ContainerControl;
                            if (control2 != null)
                            {
                                flag = control2.ActivateControlInternal(this);
                            }
                            else
                            {
                                System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
                                try
                                {
                                    flag = containerControlInternal.ActivateControl(this);
                                }
                                finally
                                {
                                    CodeAccessPermission.RevertAssert();
                                }
                            }
                            if (!flag)
                            {
                                return;
                            }
                        }
                    }
                    base.WndProc(ref m);
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 7)
            {
                this.WmSetFocus(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), System.Windows.Forms.SRDescription("ContainerControlActiveControlDescr")]
        public Control ActiveControl
        {
            get
            {
                return this.activeControl;
            }
            set
            {
                this.SetActiveControl(value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Localizable(true), System.Windows.Forms.SRCategory("CatLayout"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SizeF AutoScaleDimensions
        {
            get
            {
                return this.autoScaleDimensions;
            }
            set
            {
                if ((value.Width < 0f) || (value.Height < 0f))
                {
                    throw new ArgumentOutOfRangeException(System.Windows.Forms.SR.GetString("ContainerControlInvalidAutoScaleDimensions"), "value");
                }
                this.autoScaleDimensions = value;
                if (!this.autoScaleDimensions.IsEmpty)
                {
                    this.LayoutScalingNeeded();
                }
            }
        }

        protected SizeF AutoScaleFactor
        {
            get
            {
                SizeF currentAutoScaleDimensions = this.CurrentAutoScaleDimensions;
                SizeF autoScaleDimensions = this.AutoScaleDimensions;
                if (autoScaleDimensions.IsEmpty)
                {
                    return new SizeF(1f, 1f);
                }
                return new SizeF(currentAutoScaleDimensions.Width / autoScaleDimensions.Width, currentAutoScaleDimensions.Height / autoScaleDimensions.Height);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ContainerControlAutoScaleModeDescr"), EditorBrowsable(EditorBrowsableState.Advanced)]
        public System.Windows.Forms.AutoScaleMode AutoScaleMode
        {
            get
            {
                return this.autoScaleMode;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.AutoScaleMode));
                }
                bool flag = false;
                if (value != this.autoScaleMode)
                {
                    if (this.autoScaleMode != System.Windows.Forms.AutoScaleMode.Inherit)
                    {
                        this.autoScaleDimensions = SizeF.Empty;
                    }
                    this.currentAutoScaleDimensions = SizeF.Empty;
                    this.autoScaleMode = value;
                    flag = true;
                }
                this.OnAutoScaleModeChanged();
                if (flag)
                {
                    this.LayoutScalingNeeded();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ContainerControlAutoValidate"), Browsable(false), System.Windows.Forms.SRCategory("CatBehavior"), AmbientValue(-1), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual System.Windows.Forms.AutoValidate AutoValidate
        {
            get
            {
                if (this.autoValidate == System.Windows.Forms.AutoValidate.Inherit)
                {
                    return Control.GetAutoValidateForControl(this);
                }
                return this.autoValidate;
            }
            set
            {
                switch (value)
                {
                    case System.Windows.Forms.AutoValidate.Inherit:
                    case System.Windows.Forms.AutoValidate.Disable:
                    case System.Windows.Forms.AutoValidate.EnablePreventFocusChange:
                    case System.Windows.Forms.AutoValidate.EnableAllowFocusChange:
                        if (this.autoValidate != value)
                        {
                            this.autoValidate = value;
                            this.OnAutoValidateChanged(EventArgs.Empty);
                        }
                        return;
                }
                throw new InvalidEnumArgumentException("AutoValidate", (int) value, typeof(System.Windows.Forms.AutoValidate));
            }
        }

        [System.Windows.Forms.SRDescription("ContainerControlBindingContextDescr"), Browsable(false)]
        public override System.Windows.Forms.BindingContext BindingContext
        {
            get
            {
                System.Windows.Forms.BindingContext bindingContext = base.BindingContext;
                if (bindingContext == null)
                {
                    bindingContext = new System.Windows.Forms.BindingContext();
                    this.BindingContext = bindingContext;
                }
                return bindingContext;
            }
            set
            {
                base.BindingContext = value;
            }
        }

        protected override bool CanEnableIme
        {
            get
            {
                return false;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x10000;
                return createParams;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRCategory("CatLayout"), Browsable(false)]
        public SizeF CurrentAutoScaleDimensions
        {
            get
            {
                if (this.currentAutoScaleDimensions.IsEmpty)
                {
                    switch (this.AutoScaleMode)
                    {
                        case System.Windows.Forms.AutoScaleMode.Font:
                            this.currentAutoScaleDimensions = this.GetFontAutoScaleDimensions();
                            goto Label_005C;

                        case System.Windows.Forms.AutoScaleMode.Dpi:
                            this.currentAutoScaleDimensions = (SizeF) WindowsGraphicsCacheManager.MeasurementGraphics.DeviceContext.Dpi;
                            goto Label_005C;
                    }
                    this.currentAutoScaleDimensions = this.AutoScaleDimensions;
                }
            Label_005C:
                return this.currentAutoScaleDimensions;
            }
        }

        internal ContainerControl InnerMostActiveContainerControl
        {
            get
            {
                ContainerControl activeControl = this;
                while (activeControl.ActiveControl is ContainerControl)
                {
                    activeControl = (ContainerControl) activeControl.ActiveControl;
                }
                return activeControl;
            }
        }

        internal ContainerControl InnerMostFocusedContainerControl
        {
            get
            {
                ContainerControl focusedControl = this;
                while (focusedControl.focusedControl is ContainerControl)
                {
                    focusedControl = (ContainerControl) focusedControl.focusedControl;
                }
                return focusedControl;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ContainerControlParentFormDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatAppearance")]
        public Form ParentForm
        {
            get
            {
                System.Windows.Forms.IntSecurity.GetParent.Demand();
                return this.ParentFormInternal;
            }
        }

        internal Form ParentFormInternal
        {
            get
            {
                if (this.ParentInternal != null)
                {
                    return this.ParentInternal.FindFormInternal();
                }
                if (this is Form)
                {
                    return null;
                }
                return base.FindFormInternal();
            }
        }
    }
}

