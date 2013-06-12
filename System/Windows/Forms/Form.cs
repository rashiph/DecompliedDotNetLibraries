namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Windows.Forms.Layout;
    using System.Windows.Forms.VisualStyles;

    [DefaultEvent("Load"), InitializationEvent("Load"), ToolboxItem(false), DesignTimeVisible(false), Designer("System.Windows.Forms.Design.FormDocumentDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(IRootDesigner)), DesignerCategory("Form"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), ToolboxItemFilter("System.Windows.Forms.Control.TopLevel")]
    public class Form : ContainerControl
    {
        private System.Drawing.Size autoScaleBaseSize = System.Drawing.Size.Empty;
        private System.Windows.Forms.CloseReason closeReason;
        private System.Windows.Forms.MdiClient ctlClient;
        private static System.Drawing.Icon defaultIcon = null;
        private static System.Drawing.Icon defaultRestrictedIcon = null;
        private System.Windows.Forms.DialogResult dialogResult;
        private static readonly object EVENT_ACTIVATED = new object();
        private static readonly object EVENT_CLOSED = new object();
        private static readonly object EVENT_CLOSING = new object();
        private static readonly object EVENT_DEACTIVATE = new object();
        private static readonly object EVENT_FORMCLOSED = new object();
        private static readonly object EVENT_FORMCLOSING = new object();
        private static readonly object EVENT_HELPBUTTONCLICKED = new object();
        private static readonly object EVENT_INPUTLANGCHANGE = new object();
        private static readonly object EVENT_INPUTLANGCHANGEREQUEST = new object();
        private static readonly object EVENT_LOAD = new object();
        private static readonly object EVENT_MAXIMIZEDBOUNDSCHANGED = new object();
        private static readonly object EVENT_MAXIMUMSIZECHANGED = new object();
        private static readonly object EVENT_MDI_CHILD_ACTIVATE = new object();
        private static readonly object EVENT_MENUCOMPLETE = new object();
        private static readonly object EVENT_MENUSTART = new object();
        private static readonly object EVENT_MINIMUMSIZECHANGED = new object();
        private static readonly object EVENT_RESIZEBEGIN = new object();
        private static readonly object EVENT_RESIZEEND = new object();
        private static readonly object EVENT_RIGHTTOLEFTLAYOUTCHANGED = new object();
        private static readonly object EVENT_SHOWN = new object();
        private static Padding FormPadding = new Padding(9);
        private BitVector32 formState = new BitVector32(0x21338);
        private static readonly BitVector32.Section FormStateAllowTransparency = BitVector32.CreateSection(1);
        private static readonly BitVector32.Section FormStateAutoScaling = BitVector32.CreateSection(1, FormStateShowWindowOnCreate);
        private static readonly BitVector32.Section FormStateBorderStyle = BitVector32.CreateSection(6, FormStateAllowTransparency);
        private static readonly BitVector32.Section FormStateControlBox = BitVector32.CreateSection(1, FormStateTaskBar);
        private BitVector32 formStateEx = new BitVector32();
        private static readonly BitVector32.Section FormStateExAutoSize = BitVector32.CreateSection(1, FormStateExCalledCreateControl);
        private static readonly BitVector32.Section FormStateExCalledClosing = BitVector32.CreateSection(1);
        private static readonly BitVector32.Section FormStateExCalledCreateControl = BitVector32.CreateSection(1, FormStateExCalledMakeVisible);
        private static readonly BitVector32.Section FormStateExCalledMakeVisible = BitVector32.CreateSection(1, FormStateExCalledOnLoad);
        private static readonly BitVector32.Section FormStateExCalledOnLoad = BitVector32.CreateSection(1, FormStateExUseMdiChildProc);
        private static readonly BitVector32.Section FormStateExInModalSizingLoop = BitVector32.CreateSection(1, FormStateExInScale);
        private static readonly BitVector32.Section FormStateExInScale = BitVector32.CreateSection(1, FormStateExMnemonicProcessed);
        private static readonly BitVector32.Section FormStateExInUpdateMdiControlStrip = BitVector32.CreateSection(1, FormStateExAutoSize);
        private static readonly BitVector32.Section FormStateExMnemonicProcessed = BitVector32.CreateSection(1, FormStateExShowIcon);
        private static readonly BitVector32.Section FormStateExSettingAutoScale = BitVector32.CreateSection(1, FormStateExInModalSizingLoop);
        private static readonly BitVector32.Section FormStateExShowIcon = BitVector32.CreateSection(1, FormStateExInUpdateMdiControlStrip);
        private static readonly BitVector32.Section FormStateExUpdateMenuHandlesDeferred = BitVector32.CreateSection(1, FormStateExUpdateMenuHandlesSuspendCount);
        private static readonly BitVector32.Section FormStateExUpdateMenuHandlesSuspendCount = BitVector32.CreateSection(8, FormStateExCalledClosing);
        private static readonly BitVector32.Section FormStateExUseMdiChildProc = BitVector32.CreateSection(1, FormStateExUpdateMenuHandlesDeferred);
        private static readonly BitVector32.Section FormStateExWindowBoundsHeightIsClientSize = BitVector32.CreateSection(1, FormStateExWindowBoundsWidthIsClientSize);
        private static readonly BitVector32.Section FormStateExWindowBoundsWidthIsClientSize = BitVector32.CreateSection(1, FormStateExSettingAutoScale);
        private static readonly BitVector32.Section FormStateExWindowClosing = BitVector32.CreateSection(1, FormStateExWindowBoundsHeightIsClientSize);
        private static readonly BitVector32.Section FormStateHelpButton = BitVector32.CreateSection(1, FormStateMinimizeBox);
        private static readonly BitVector32.Section FormStateIconSet = BitVector32.CreateSection(1, FormStateIsActive);
        private static readonly BitVector32.Section FormStateIsActive = BitVector32.CreateSection(1, FormStateIsTextEmpty);
        private static readonly BitVector32.Section FormStateIsRestrictedWindow = BitVector32.CreateSection(1, FormStateSizeGripStyle);
        private static readonly BitVector32.Section FormStateIsRestrictedWindowChecked = BitVector32.CreateSection(1, FormStateIsRestrictedWindow);
        private static readonly BitVector32.Section FormStateIsTextEmpty = BitVector32.CreateSection(1, FormStateIsWindowActivated);
        private static readonly BitVector32.Section FormStateIsWindowActivated = BitVector32.CreateSection(1, FormStateIsRestrictedWindowChecked);
        private static readonly BitVector32.Section FormStateKeyPreview = BitVector32.CreateSection(1, FormStateControlBox);
        private static readonly BitVector32.Section FormStateLayered = BitVector32.CreateSection(1, FormStateKeyPreview);
        private static readonly BitVector32.Section FormStateMaximizeBox = BitVector32.CreateSection(1, FormStateLayered);
        private static readonly BitVector32.Section FormStateMdiChildMax = BitVector32.CreateSection(1, FormStateSWCalled);
        private static readonly BitVector32.Section FormStateMinimizeBox = BitVector32.CreateSection(1, FormStateMaximizeBox);
        private static readonly BitVector32.Section FormStateRenderSizeGrip = BitVector32.CreateSection(1, FormStateMdiChildMax);
        private static readonly BitVector32.Section FormStateSetClientSize = BitVector32.CreateSection(1, FormStateAutoScaling);
        private static readonly BitVector32.Section FormStateShowWindowOnCreate = BitVector32.CreateSection(1, FormStateWindowState);
        private static readonly BitVector32.Section FormStateSizeGripStyle = BitVector32.CreateSection(2, FormStateRenderSizeGrip);
        private static readonly BitVector32.Section FormStateStartPos = BitVector32.CreateSection(4, FormStateHelpButton);
        private static readonly BitVector32.Section FormStateSWCalled = BitVector32.CreateSection(1, FormStateTopMost);
        private static readonly BitVector32.Section FormStateTaskBar = BitVector32.CreateSection(1, FormStateBorderStyle);
        private static readonly BitVector32.Section FormStateTopMost = BitVector32.CreateSection(1, FormStateSetClientSize);
        private static readonly BitVector32.Section FormStateWindowState = BitVector32.CreateSection(2, FormStateStartPos);
        private System.Drawing.Icon icon;
        private static object internalSyncObject = new object();
        private System.Drawing.Size minAutoSize = System.Drawing.Size.Empty;
        private NativeWindow ownerWindow;
        private static readonly int PropAcceptButton = PropertyStore.CreateKey();
        private static readonly int PropActiveMdiChild = PropertyStore.CreateKey();
        private static readonly int PropCancelButton = PropertyStore.CreateKey();
        private static readonly int PropCurMenu = PropertyStore.CreateKey();
        private static readonly int PropDefaultButton = PropertyStore.CreateKey();
        private static readonly int PropDialogOwner = PropertyStore.CreateKey();
        private static readonly int PropDummyMenu = PropertyStore.CreateKey();
        private static readonly int PropFormerlyActiveMdiChild = PropertyStore.CreateKey();
        private static readonly int PropFormMdiParent = PropertyStore.CreateKey();
        private static readonly int PropMainMenu = PropertyStore.CreateKey();
        private static readonly int PropMainMenuStrip = PropertyStore.CreateKey();
        private static readonly int PropMaximizedBounds = PropertyStore.CreateKey();
        private static readonly int PropMaxTrackSizeHeight = PropertyStore.CreateKey();
        private static readonly int PropMaxTrackSizeWidth = PropertyStore.CreateKey();
        private static readonly int PropMdiChildFocusable = PropertyStore.CreateKey();
        private static readonly int PropMdiControlStrip = PropertyStore.CreateKey();
        private static readonly int PropMdiWindowListStrip = PropertyStore.CreateKey();
        private static readonly int PropMergedMenu = PropertyStore.CreateKey();
        private static readonly int PropMinTrackSizeHeight = PropertyStore.CreateKey();
        private static readonly int PropMinTrackSizeWidth = PropertyStore.CreateKey();
        private static readonly int PropOpacity = PropertyStore.CreateKey();
        private static readonly int PropOwnedForms = PropertyStore.CreateKey();
        private static readonly int PropOwnedFormsCount = PropertyStore.CreateKey();
        private static readonly int PropOwner = PropertyStore.CreateKey();
        private static readonly int PropSecurityTip = PropertyStore.CreateKey();
        private static readonly int PropTransparencyKey = PropertyStore.CreateKey();
        private Rectangle restoreBounds = new Rectangle(-1, -1, -1, -1);
        private Rectangle restoredWindowBounds = new Rectangle(-1, -1, -1, -1);
        private BoundsSpecified restoredWindowBoundsSpecified;
        private bool rightToLeftLayout;
        private string securitySite;
        private string securityZone;
        private VisualStyleRenderer sizeGripRenderer;
        private const int SizeGripSize = 0x10;
        private System.Drawing.Icon smallIcon;
        private string userWindowText;

        [System.Windows.Forms.SRDescription("FormOnActivateDescr"), System.Windows.Forms.SRCategory("CatFocus")]
        public event EventHandler Activated
        {
            add
            {
                base.Events.AddHandler(EVENT_ACTIVATED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_ACTIVATED, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnAutoSizeChangedDescr"), Browsable(true), System.Windows.Forms.SRCategory("CatPropertyChanged"), EditorBrowsable(EditorBrowsableState.Always)]
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

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        public event EventHandler AutoValidateChanged
        {
            add
            {
                base.AutoValidateChanged += value;
            }
            remove
            {
                base.AutoValidateChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("FormOnClosedDescr"), Browsable(false)]
        public event EventHandler Closed
        {
            add
            {
                base.Events.AddHandler(EVENT_CLOSED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CLOSED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), System.Windows.Forms.SRDescription("FormOnClosingDescr")]
        public event CancelEventHandler Closing
        {
            add
            {
                base.Events.AddHandler(EVENT_CLOSING, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CLOSING, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatFocus"), System.Windows.Forms.SRDescription("FormOnDeactivateDescr")]
        public event EventHandler Deactivate
        {
            add
            {
                base.Events.AddHandler(EVENT_DEACTIVATE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DEACTIVATE, value);
            }
        }

        [System.Windows.Forms.SRDescription("FormOnFormClosedDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event FormClosedEventHandler FormClosed
        {
            add
            {
                base.Events.AddHandler(EVENT_FORMCLOSED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_FORMCLOSED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("FormOnFormClosingDescr")]
        public event FormClosingEventHandler FormClosing
        {
            add
            {
                base.Events.AddHandler(EVENT_FORMCLOSING, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_FORMCLOSING, value);
            }
        }

        [System.Windows.Forms.SRDescription("FormHelpButtonClickedDescr"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRCategory("CatBehavior")]
        public event CancelEventHandler HelpButtonClicked
        {
            add
            {
                base.Events.AddHandler(EVENT_HELPBUTTONCLICKED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_HELPBUTTONCLICKED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("FormOnInputLangChangeDescr")]
        public event InputLanguageChangedEventHandler InputLanguageChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_INPUTLANGCHANGE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_INPUTLANGCHANGE, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("FormOnInputLangChangeRequestDescr")]
        public event InputLanguageChangingEventHandler InputLanguageChanging
        {
            add
            {
                base.Events.AddHandler(EVENT_INPUTLANGCHANGEREQUEST, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_INPUTLANGCHANGEREQUEST, value);
            }
        }

        [System.Windows.Forms.SRDescription("FormOnLoadDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler Load
        {
            add
            {
                base.Events.AddHandler(EVENT_LOAD, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_LOAD, value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler MarginChanged
        {
            add
            {
                base.MarginChanged += value;
            }
            remove
            {
                base.MarginChanged -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("FormOnMaximizedBoundsChangedDescr")]
        public event EventHandler MaximizedBoundsChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_MAXIMIZEDBOUNDSCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MAXIMIZEDBOUNDSCHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("FormOnMaximumSizeChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler MaximumSizeChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_MAXIMUMSIZECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MAXIMUMSIZECHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("FormOnMDIChildActivateDescr")]
        public event EventHandler MdiChildActivate
        {
            add
            {
                base.Events.AddHandler(EVENT_MDI_CHILD_ACTIVATE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MDI_CHILD_ACTIVATE, value);
            }
        }

        [System.Windows.Forms.SRDescription("FormOnMenuCompleteDescr"), Browsable(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler MenuComplete
        {
            add
            {
                base.Events.AddHandler(EVENT_MENUCOMPLETE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MENUCOMPLETE, value);
            }
        }

        [Browsable(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("FormOnMenuStartDescr")]
        public event EventHandler MenuStart
        {
            add
            {
                base.Events.AddHandler(EVENT_MENUSTART, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MENUSTART, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("FormOnMinimumSizeChangedDescr")]
        public event EventHandler MinimumSizeChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_MINIMUMSIZECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MINIMUMSIZECHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("FormOnResizeBeginDescr")]
        public event EventHandler ResizeBegin
        {
            add
            {
                base.Events.AddHandler(EVENT_RESIZEBEGIN, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_RESIZEBEGIN, value);
            }
        }

        [System.Windows.Forms.SRDescription("FormOnResizeEndDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event EventHandler ResizeEnd
        {
            add
            {
                base.Events.AddHandler(EVENT_RESIZEEND, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_RESIZEEND, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnRightToLeftLayoutChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler RightToLeftLayoutChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_RIGHTTOLEFTLAYOUTCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_RIGHTTOLEFTLAYOUTCHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("FormOnShownDescr")]
        public event EventHandler Shown
        {
            add
            {
                base.Events.AddHandler(EVENT_SHOWN, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SHOWN, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TabIndexChanged
        {
            add
            {
                base.TabIndexChanged += value;
            }
            remove
            {
                base.TabIndexChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TabStopChanged
        {
            add
            {
                base.TabStopChanged += value;
            }
            remove
            {
                base.TabStopChanged -= value;
            }
        }

        public Form()
        {
            bool isRestrictedWindow = this.IsRestrictedWindow;
            this.formStateEx[FormStateExShowIcon] = 1;
            base.SetState(2, false);
            base.SetState(0x80000, true);
        }

        public void Activate()
        {
            System.Windows.Forms.IntSecurity.ModifyFocus.Demand();
            if (base.Visible && base.IsHandleCreated)
            {
                if (this.IsMdiChild)
                {
                    this.MdiParentInternal.MdiClient.SendMessage(0x222, base.Handle, 0);
                }
                else
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetForegroundWindow(new HandleRef(this, base.Handle));
                }
            }
        }

        protected void ActivateMdiChild(Form form)
        {
            System.Windows.Forms.IntSecurity.ModifyFocus.Demand();
            this.ActivateMdiChildInternal(form);
        }

        private void ActivateMdiChildInternal(Form form)
        {
            if ((this.FormerlyActiveMdiChild != null) && !this.FormerlyActiveMdiChild.IsClosing)
            {
                this.FormerlyActiveMdiChild.UpdateWindowIcon(true);
                this.FormerlyActiveMdiChild = null;
            }
            Form activeMdiChildInternal = this.ActiveMdiChildInternal;
            if (activeMdiChildInternal != form)
            {
                if (activeMdiChildInternal != null)
                {
                    activeMdiChildInternal.Active = false;
                }
                activeMdiChildInternal = form;
                this.ActiveMdiChildInternal = form;
                if (activeMdiChildInternal != null)
                {
                    activeMdiChildInternal.IsMdiChildFocusable = true;
                    activeMdiChildInternal.Active = true;
                }
                else if (this.Active)
                {
                    base.ActivateControlInternal(this);
                }
                this.OnMdiChildActivate(EventArgs.Empty);
            }
        }

        public void AddOwnedForm(Form ownedForm)
        {
            if (ownedForm != null)
            {
                if (ownedForm.OwnerInternal != this)
                {
                    ownedForm.Owner = this;
                }
                else
                {
                    Form[] formArray = (Form[]) base.Properties.GetObject(PropOwnedForms);
                    int integer = base.Properties.GetInteger(PropOwnedFormsCount);
                    for (int i = 0; i < integer; i++)
                    {
                        if (formArray[i] == ownedForm)
                        {
                            return;
                        }
                    }
                    if (formArray == null)
                    {
                        formArray = new Form[4];
                        base.Properties.SetObject(PropOwnedForms, formArray);
                    }
                    else if (formArray.Length == integer)
                    {
                        Form[] destinationArray = new Form[integer * 2];
                        Array.Copy(formArray, 0, destinationArray, 0, integer);
                        formArray = destinationArray;
                        base.Properties.SetObject(PropOwnedForms, formArray);
                    }
                    formArray[integer] = ownedForm;
                    base.Properties.SetInteger(PropOwnedFormsCount, integer + 1);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void AdjustFormScrollbars(bool displayScrollbars)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                base.AdjustFormScrollbars(displayScrollbars);
            }
        }

        private float AdjustScale(float scale)
        {
            if (scale < 0.92f)
            {
                return (scale + 0.08f);
            }
            if (scale < 1f)
            {
                return 1f;
            }
            if (scale > 1.01f)
            {
                return (scale + 0.08f);
            }
            return scale;
        }

        private void AdjustSystemMenu()
        {
            if (base.IsHandleCreated)
            {
                IntPtr systemMenu = System.Windows.Forms.UnsafeNativeMethods.GetSystemMenu(new HandleRef(this, base.Handle), false);
                this.AdjustSystemMenu(systemMenu);
                systemMenu = IntPtr.Zero;
            }
        }

        private void AdjustSystemMenu(IntPtr hmenu)
        {
            this.UpdateWindowState();
            FormWindowState windowState = this.WindowState;
            System.Windows.Forms.FormBorderStyle formBorderStyle = this.FormBorderStyle;
            bool flag = (formBorderStyle == System.Windows.Forms.FormBorderStyle.SizableToolWindow) || (formBorderStyle == System.Windows.Forms.FormBorderStyle.Sizable);
            bool flag2 = this.MinimizeBox && (windowState != FormWindowState.Minimized);
            bool flag3 = this.MaximizeBox && (windowState != FormWindowState.Maximized);
            bool controlBox = this.ControlBox;
            bool flag5 = windowState != FormWindowState.Normal;
            bool flag6 = (flag && (windowState != FormWindowState.Minimized)) && (windowState != FormWindowState.Maximized);
            if (!flag2)
            {
                System.Windows.Forms.UnsafeNativeMethods.EnableMenuItem(new HandleRef(this, hmenu), 0xf020, 1);
            }
            else
            {
                System.Windows.Forms.UnsafeNativeMethods.EnableMenuItem(new HandleRef(this, hmenu), 0xf020, 0);
            }
            if (!flag3)
            {
                System.Windows.Forms.UnsafeNativeMethods.EnableMenuItem(new HandleRef(this, hmenu), 0xf030, 1);
            }
            else
            {
                System.Windows.Forms.UnsafeNativeMethods.EnableMenuItem(new HandleRef(this, hmenu), 0xf030, 0);
            }
            if (!controlBox)
            {
                System.Windows.Forms.UnsafeNativeMethods.EnableMenuItem(new HandleRef(this, hmenu), 0xf060, 1);
            }
            else
            {
                System.Windows.Forms.UnsafeNativeMethods.EnableMenuItem(new HandleRef(this, hmenu), 0xf060, 0);
            }
            if (!flag5)
            {
                System.Windows.Forms.UnsafeNativeMethods.EnableMenuItem(new HandleRef(this, hmenu), 0xf120, 1);
            }
            else
            {
                System.Windows.Forms.UnsafeNativeMethods.EnableMenuItem(new HandleRef(this, hmenu), 0xf120, 0);
            }
            if (!flag6)
            {
                System.Windows.Forms.UnsafeNativeMethods.EnableMenuItem(new HandleRef(this, hmenu), 0xf000, 1);
            }
            else
            {
                System.Windows.Forms.UnsafeNativeMethods.EnableMenuItem(new HandleRef(this, hmenu), 0xf000, 0);
            }
        }

        internal override void AfterControlRemoved(Control control, Control oldParent)
        {
            base.AfterControlRemoved(control, oldParent);
            if (control == this.AcceptButton)
            {
                this.AcceptButton = null;
            }
            if (control == this.CancelButton)
            {
                this.CancelButton = null;
            }
            if (control == this.ctlClient)
            {
                this.ctlClient = null;
                this.UpdateMenuHandles();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("This method has been deprecated. Use the ApplyAutoScaling method instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        protected void ApplyAutoScaling()
        {
            if (!this.autoScaleBaseSize.IsEmpty)
            {
                System.Drawing.Size autoScaleBaseSize = this.AutoScaleBaseSize;
                SizeF autoScaleSize = GetAutoScaleSize(this.Font);
                System.Drawing.Size size2 = new System.Drawing.Size((int) Math.Round((double) autoScaleSize.Width), (int) Math.Round((double) autoScaleSize.Height));
                if (!autoScaleBaseSize.Equals(size2))
                {
                    float dy = this.AdjustScale(((float) size2.Height) / ((float) autoScaleBaseSize.Height));
                    float dx = this.AdjustScale(((float) size2.Width) / ((float) autoScaleBaseSize.Width));
                    base.Scale(dx, dy);
                    this.AutoScaleBaseSize = size2;
                }
            }
        }

        internal override Rectangle ApplyBoundsConstraints(int suggestedX, int suggestedY, int proposedWidth, int proposedHeight)
        {
            Rectangle bounds = base.ApplyBoundsConstraints(suggestedX, suggestedY, proposedWidth, proposedHeight);
            if (this.IsRestrictedWindow)
            {
                Screen[] allScreens = Screen.AllScreens;
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                for (int i = 0; i < allScreens.Length; i++)
                {
                    Rectangle workingArea = allScreens[i].WorkingArea;
                    if (workingArea.Contains(suggestedX, suggestedY))
                    {
                        flag = true;
                    }
                    if (workingArea.Contains(suggestedX + proposedWidth, suggestedY))
                    {
                        flag2 = true;
                    }
                    if (workingArea.Contains(suggestedX, suggestedY + proposedHeight))
                    {
                        flag3 = true;
                    }
                    if (workingArea.Contains(suggestedX + proposedWidth, suggestedY + proposedHeight))
                    {
                        flag4 = true;
                    }
                }
                if ((flag && flag2) && (flag3 && flag4))
                {
                    return bounds;
                }
                if (this.formStateEx[FormStateExInScale] == 1)
                {
                    return WindowsFormsUtils.ConstrainToScreenWorkingAreaBounds(bounds);
                }
                bounds.X = base.Left;
                bounds.Y = base.Top;
                bounds.Width = base.Width;
                bounds.Height = base.Height;
            }
            return bounds;
        }

        private void ApplyClientSize()
        {
            if ((this.formState[FormStateWindowState] == 0) && base.IsHandleCreated)
            {
                System.Drawing.Size clientSize = this.ClientSize;
                bool hScroll = base.HScroll;
                bool vScroll = base.VScroll;
                bool flag3 = false;
                if (this.formState[FormStateSetClientSize] != 0)
                {
                    flag3 = true;
                    this.formState[FormStateSetClientSize] = 0;
                }
                if (flag3)
                {
                    if (hScroll)
                    {
                        clientSize.Height += SystemInformation.HorizontalScrollBarHeight;
                    }
                    if (vScroll)
                    {
                        clientSize.Width += SystemInformation.VerticalScrollBarWidth;
                    }
                }
                IntPtr handle = base.Handle;
                System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                System.Windows.Forms.SafeNativeMethods.GetClientRect(new HandleRef(this, handle), ref rect);
                Rectangle rectangle = Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
                Rectangle bounds = base.Bounds;
                if (clientSize.Width != rectangle.Width)
                {
                    System.Drawing.Size size2 = this.ComputeWindowSize(clientSize);
                    if (vScroll)
                    {
                        size2.Width += SystemInformation.VerticalScrollBarWidth;
                    }
                    if (hScroll)
                    {
                        size2.Height += SystemInformation.HorizontalScrollBarHeight;
                    }
                    bounds.Width = size2.Width;
                    bounds.Height = size2.Height;
                    base.Bounds = bounds;
                    System.Windows.Forms.SafeNativeMethods.GetClientRect(new HandleRef(this, handle), ref rect);
                    rectangle = Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
                }
                if (clientSize.Height != rectangle.Height)
                {
                    int num = clientSize.Height - rectangle.Height;
                    bounds.Height += num;
                    base.Bounds = bounds;
                }
                base.UpdateBounds();
            }
        }

        internal override void AssignParent(Control value)
        {
            Form form = (Form) base.Properties.GetObject(PropFormMdiParent);
            if ((form != null) && (form.MdiClient != value))
            {
                base.Properties.SetObject(PropFormMdiParent, null);
            }
            base.AssignParent(value);
        }

        private void CallShownEvent()
        {
            this.OnShown(EventArgs.Empty);
        }

        internal override bool CanProcessMnemonic()
        {
            return ((!this.IsMdiChild || (((this.formStateEx[FormStateExMnemonicProcessed] != 1) && (this == this.MdiParentInternal.ActiveMdiChildInternal)) && (this.WindowState != FormWindowState.Minimized))) && base.CanProcessMnemonic());
        }

        internal bool CanRecreateHandle()
        {
            return (!this.IsMdiChild || (base.GetState(2) && base.IsHandleCreated));
        }

        internal override bool CanSelectCore()
        {
            return ((base.GetStyle(ControlStyles.Selectable) && base.Enabled) && base.Visible);
        }

        protected void CenterToParent()
        {
            if (this.TopLevel)
            {
                Point point = new Point();
                System.Drawing.Size size = this.Size;
                IntPtr zero = IntPtr.Zero;
                zero = System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.Handle), -8);
                if (zero != IntPtr.Zero)
                {
                    Rectangle workingArea = Screen.FromHandleInternal(zero).WorkingArea;
                    System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                    System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(null, zero), ref rect);
                    point.X = ((rect.left + rect.right) - size.Width) / 2;
                    if (point.X < workingArea.X)
                    {
                        point.X = workingArea.X;
                    }
                    else if ((point.X + size.Width) > (workingArea.X + workingArea.Width))
                    {
                        point.X = (workingArea.X + workingArea.Width) - size.Width;
                    }
                    point.Y = ((rect.top + rect.bottom) - size.Height) / 2;
                    if (point.Y < workingArea.Y)
                    {
                        point.Y = workingArea.Y;
                    }
                    else if ((point.Y + size.Height) > (workingArea.Y + workingArea.Height))
                    {
                        point.Y = (workingArea.Y + workingArea.Height) - size.Height;
                    }
                    this.Location = point;
                }
                else
                {
                    this.CenterToScreen();
                }
            }
        }

        protected void CenterToScreen()
        {
            Point point = new Point();
            Screen screen = null;
            if (this.OwnerInternal != null)
            {
                screen = Screen.FromControl(this.OwnerInternal);
            }
            else
            {
                IntPtr zero = IntPtr.Zero;
                if (this.TopLevel)
                {
                    zero = System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.Handle), -8);
                }
                if (zero != IntPtr.Zero)
                {
                    screen = Screen.FromHandleInternal(zero);
                }
                else
                {
                    screen = Screen.FromPoint(Control.MousePosition);
                }
            }
            Rectangle workingArea = screen.WorkingArea;
            point.X = Math.Max(workingArea.X, workingArea.X + ((workingArea.Width - base.Width) / 2));
            point.Y = Math.Max(workingArea.Y, workingArea.Y + ((workingArea.Height - base.Height) / 2));
            this.Location = point;
        }

        internal bool CheckCloseDialog(bool closingOnly)
        {
            if ((this.dialogResult == System.Windows.Forms.DialogResult.None) && base.Visible)
            {
                return false;
            }
            try
            {
                FormClosingEventArgs e = new FormClosingEventArgs(this.closeReason, false);
                if (!this.CalledClosing)
                {
                    this.OnClosing(e);
                    this.OnFormClosing(e);
                    if (e.Cancel)
                    {
                        this.dialogResult = System.Windows.Forms.DialogResult.None;
                    }
                    else
                    {
                        this.CalledClosing = true;
                    }
                }
                if (!closingOnly && (this.dialogResult != System.Windows.Forms.DialogResult.None))
                {
                    FormClosedEventArgs args2 = new FormClosedEventArgs(this.closeReason);
                    this.OnClosed(args2);
                    this.OnFormClosed(args2);
                    this.CalledClosing = false;
                }
            }
            catch (Exception exception)
            {
                this.dialogResult = System.Windows.Forms.DialogResult.None;
                if (NativeWindow.WndProcShouldBeDebuggable)
                {
                    throw;
                }
                Application.OnThreadException(exception);
            }
            if (this.dialogResult == System.Windows.Forms.DialogResult.None)
            {
                return !base.Visible;
            }
            return true;
        }

        public void Close()
        {
            if (base.GetState(0x40000))
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ClosingWhileCreatingHandle", new object[] { "Close" }));
            }
            if (base.IsHandleCreated)
            {
                this.closeReason = System.Windows.Forms.CloseReason.UserClosing;
                base.SendMessage(0x10, 0, 0);
            }
            else
            {
                base.Dispose();
            }
        }

        private System.Drawing.Size ComputeWindowSize(System.Drawing.Size clientSize)
        {
            System.Windows.Forms.CreateParams createParams = this.CreateParams;
            return this.ComputeWindowSize(clientSize, createParams.Style, createParams.ExStyle);
        }

        private System.Drawing.Size ComputeWindowSize(System.Drawing.Size clientSize, int style, int exStyle)
        {
            System.Windows.Forms.NativeMethods.RECT lpRect = new System.Windows.Forms.NativeMethods.RECT(0, 0, clientSize.Width, clientSize.Height);
            System.Windows.Forms.SafeNativeMethods.AdjustWindowRectEx(ref lpRect, style, this.HasMenu, exStyle);
            return new System.Drawing.Size(lpRect.right - lpRect.left, lpRect.bottom - lpRect.top);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override Control.ControlCollection CreateControlsInstance()
        {
            return new ControlCollection(this);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void CreateHandle()
        {
            Form form = (Form) base.Properties.GetObject(PropFormMdiParent);
            if (form != null)
            {
                form.SuspendUpdateMenuHandles();
            }
            try
            {
                if (this.IsMdiChild && this.MdiParentInternal.IsHandleCreated)
                {
                    System.Windows.Forms.MdiClient mdiClient = this.MdiParentInternal.MdiClient;
                    if ((mdiClient != null) && !mdiClient.IsHandleCreated)
                    {
                        mdiClient.CreateControl();
                    }
                }
                if (this.IsMdiChild && (this.formState[FormStateWindowState] == 2))
                {
                    this.formState[FormStateWindowState] = 0;
                    this.formState[FormStateMdiChildMax] = 1;
                    base.CreateHandle();
                    this.formState[FormStateWindowState] = 2;
                    this.formState[FormStateMdiChildMax] = 0;
                }
                else
                {
                    base.CreateHandle();
                }
                this.UpdateHandleWithOwner();
                this.UpdateWindowIcon(false);
                this.AdjustSystemMenu();
                if (this.formState[FormStateStartPos] != 3)
                {
                    this.ApplyClientSize();
                }
                if (this.formState[FormStateShowWindowOnCreate] == 1)
                {
                    base.Visible = true;
                }
                if (((this.Menu != null) || !this.TopLevel) || this.IsMdiContainer)
                {
                    this.UpdateMenuHandles();
                }
                if ((!this.ShowInTaskbar && (this.OwnerInternal == null)) && this.TopLevel)
                {
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, base.Handle), -8, this.TaskbarOwner);
                    System.Drawing.Icon icon = this.Icon;
                    if ((icon != null) && (this.TaskbarOwner.Handle != IntPtr.Zero))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(this.TaskbarOwner, 0x80, 1, icon.Handle);
                    }
                }
                if (this.formState[FormStateTopMost] != 0)
                {
                    this.TopMost = true;
                }
            }
            finally
            {
                if (form != null)
                {
                    form.ResumeUpdateMenuHandles();
                }
                base.UpdateStyles();
            }
        }

        private void DeactivateMdiChild()
        {
            Form activeMdiChildInternal = this.ActiveMdiChildInternal;
            if (activeMdiChildInternal != null)
            {
                Form mdiParentInternal = activeMdiChildInternal.MdiParentInternal;
                activeMdiChildInternal.Active = false;
                activeMdiChildInternal.IsMdiChildFocusable = false;
                if (!activeMdiChildInternal.IsClosing)
                {
                    this.FormerlyActiveMdiChild = activeMdiChildInternal;
                }
                bool flag = true;
                foreach (Form form3 in mdiParentInternal.MdiChildren)
                {
                    if ((form3 != this) && form3.Visible)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    mdiParentInternal.ActivateMdiChildInternal(null);
                }
                this.ActiveMdiChildInternal = null;
                this.UpdateMenuHandles();
                this.UpdateToolStrip();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void DefWndProc(ref Message m)
        {
            if (((this.ctlClient != null) && this.ctlClient.IsHandleCreated) && (this.ctlClient.ParentInternal == this))
            {
                m.Result = System.Windows.Forms.UnsafeNativeMethods.DefFrameProc(m.HWnd, this.ctlClient.Handle, m.Msg, m.WParam, m.LParam);
            }
            else if (this.formStateEx[FormStateExUseMdiChildProc] != 0)
            {
                m.Result = System.Windows.Forms.UnsafeNativeMethods.DefMDIChildProc(m.HWnd, m.Msg, m.WParam, m.LParam);
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.CalledOnLoad = false;
                this.CalledMakeVisible = false;
                this.CalledCreateControl = false;
                if (base.Properties.ContainsObject(PropAcceptButton))
                {
                    base.Properties.SetObject(PropAcceptButton, null);
                }
                if (base.Properties.ContainsObject(PropCancelButton))
                {
                    base.Properties.SetObject(PropCancelButton, null);
                }
                if (base.Properties.ContainsObject(PropDefaultButton))
                {
                    base.Properties.SetObject(PropDefaultButton, null);
                }
                if (base.Properties.ContainsObject(PropActiveMdiChild))
                {
                    base.Properties.SetObject(PropActiveMdiChild, null);
                }
                if (this.MdiWindowListStrip != null)
                {
                    this.MdiWindowListStrip.Dispose();
                    this.MdiWindowListStrip = null;
                }
                if (this.MdiControlStrip != null)
                {
                    this.MdiControlStrip.Dispose();
                    this.MdiControlStrip = null;
                }
                if (this.MainMenuStrip != null)
                {
                    this.MainMenuStrip = null;
                }
                Form form = (Form) base.Properties.GetObject(PropOwner);
                if (form != null)
                {
                    form.RemoveOwnedForm(this);
                    base.Properties.SetObject(PropOwner, null);
                }
                Form[] formArray = (Form[]) base.Properties.GetObject(PropOwnedForms);
                for (int i = base.Properties.GetInteger(PropOwnedFormsCount) - 1; i >= 0; i--)
                {
                    if (formArray[i] != null)
                    {
                        formArray[i].Dispose();
                    }
                }
                if (this.smallIcon != null)
                {
                    this.smallIcon.Dispose();
                    this.smallIcon = null;
                }
                this.ResetSecurityTip(false);
                base.Dispose(disposing);
                this.ctlClient = null;
                MainMenu menu = this.Menu;
                if ((menu != null) && (menu.ownerForm == this))
                {
                    menu.Dispose();
                    base.Properties.SetObject(PropMainMenu, null);
                }
                if (base.Properties.GetObject(PropCurMenu) != null)
                {
                    base.Properties.SetObject(PropCurMenu, null);
                }
                this.MenuChanged(0, null);
                MainMenu menu2 = (MainMenu) base.Properties.GetObject(PropDummyMenu);
                if (menu2 != null)
                {
                    menu2.Dispose();
                    base.Properties.SetObject(PropDummyMenu, null);
                }
                MainMenu menu3 = (MainMenu) base.Properties.GetObject(PropMergedMenu);
                if (menu3 != null)
                {
                    if ((menu3.ownerForm == this) || (menu3.form == null))
                    {
                        menu3.Dispose();
                    }
                    base.Properties.SetObject(PropMergedMenu, null);
                }
            }
            else
            {
                base.Dispose(disposing);
            }
        }

        private void EnsureSecurityInformation()
        {
            if ((this.securityZone == null) || (this.securitySite == null))
            {
                ArrayList list;
                ArrayList list2;
                SecurityManager.GetZoneAndOrigin(out list, out list2);
                this.ResolveZoneAndSiteNames(list2, ref this.securityZone, ref this.securitySite);
            }
        }

        private void FillInCreateParamsBorderIcons(System.Windows.Forms.CreateParams cp)
        {
            if (this.FormBorderStyle != System.Windows.Forms.FormBorderStyle.None)
            {
                if ((this.Text != null) && (this.Text.Length != 0))
                {
                    cp.Style |= 0xc00000;
                }
                if (this.ControlBox || this.IsRestrictedWindow)
                {
                    cp.Style |= 0xc80000;
                }
                else
                {
                    cp.Style &= -524289;
                }
                if (this.MaximizeBox || this.IsRestrictedWindow)
                {
                    cp.Style |= 0x10000;
                }
                else
                {
                    cp.Style &= -65537;
                }
                if (this.MinimizeBox || this.IsRestrictedWindow)
                {
                    cp.Style |= 0x20000;
                }
                else
                {
                    cp.Style &= -131073;
                }
                if ((this.HelpButton && !this.MaximizeBox) && (!this.MinimizeBox && this.ControlBox))
                {
                    cp.ExStyle |= 0x400;
                }
                else
                {
                    cp.ExStyle &= -1025;
                }
            }
        }

        private void FillInCreateParamsBorderStyles(System.Windows.Forms.CreateParams cp)
        {
            switch (((System.Windows.Forms.FormBorderStyle) this.formState[FormStateBorderStyle]))
            {
                case System.Windows.Forms.FormBorderStyle.None:
                    if (!this.IsRestrictedWindow)
                    {
                        return;
                    }
                    break;

                case System.Windows.Forms.FormBorderStyle.FixedSingle:
                    break;

                case System.Windows.Forms.FormBorderStyle.Fixed3D:
                    cp.Style |= 0x800000;
                    cp.ExStyle |= 0x200;
                    return;

                case System.Windows.Forms.FormBorderStyle.FixedDialog:
                    cp.Style |= 0x800000;
                    cp.ExStyle |= 1;
                    return;

                case System.Windows.Forms.FormBorderStyle.Sizable:
                    cp.Style |= 0x840000;
                    return;

                case System.Windows.Forms.FormBorderStyle.FixedToolWindow:
                    cp.Style |= 0x800000;
                    cp.ExStyle |= 0x80;
                    return;

                case System.Windows.Forms.FormBorderStyle.SizableToolWindow:
                    cp.Style |= 0x840000;
                    cp.ExStyle |= 0x80;
                    return;

                default:
                    return;
            }
            cp.Style |= 0x800000;
        }

        private void FillInCreateParamsStartPosition(System.Windows.Forms.CreateParams cp)
        {
            if (this.formState[FormStateSetClientSize] != 0)
            {
                int style = cp.Style & -553648129;
                System.Drawing.Size size = this.ComputeWindowSize(this.ClientSize, style, cp.ExStyle);
                if (this.IsRestrictedWindow)
                {
                    size = this.ApplyBoundsConstraints(cp.X, cp.Y, size.Width, size.Height).Size;
                }
                cp.Width = size.Width;
                cp.Height = size.Height;
            }
            switch (((FormStartPosition) this.formState[FormStateStartPos]))
            {
                case FormStartPosition.CenterScreen:
                {
                    if (!this.IsMdiChild)
                    {
                        Screen screen = null;
                        IWin32Window window = (IWin32Window) base.Properties.GetObject(PropDialogOwner);
                        if ((this.OwnerInternal != null) || (window != null))
                        {
                            IntPtr hwnd = (window != null) ? Control.GetSafeHandle(window) : this.OwnerInternal.Handle;
                            screen = Screen.FromHandleInternal(hwnd);
                        }
                        else
                        {
                            screen = Screen.FromPoint(Control.MousePosition);
                        }
                        Rectangle workingArea = screen.WorkingArea;
                        if (this.WindowState != FormWindowState.Maximized)
                        {
                            cp.X = Math.Max(workingArea.X, workingArea.X + ((workingArea.Width - cp.Width) / 2));
                            cp.Y = Math.Max(workingArea.Y, workingArea.Y + ((workingArea.Height - cp.Height) / 2));
                        }
                        return;
                    }
                    Rectangle clientRectangle = this.MdiParentInternal.MdiClient.ClientRectangle;
                    cp.X = Math.Max(clientRectangle.X, clientRectangle.X + ((clientRectangle.Width - cp.Width) / 2));
                    cp.Y = Math.Max(clientRectangle.Y, clientRectangle.Y + ((clientRectangle.Height - cp.Height) / 2));
                    return;
                }
                case FormStartPosition.WindowsDefaultLocation:
                case FormStartPosition.CenterParent:
                    break;

                case FormStartPosition.WindowsDefaultBounds:
                    cp.Width = -2147483648;
                    cp.Height = -2147483648;
                    break;

                default:
                    return;
            }
            if (!this.IsMdiChild || (this.Dock == DockStyle.None))
            {
                cp.X = -2147483648;
                cp.Y = -2147483648;
            }
        }

        private void FillInCreateParamsWindowState(System.Windows.Forms.CreateParams cp)
        {
            switch (((FormWindowState) this.formState[FormStateWindowState]))
            {
                case FormWindowState.Minimized:
                    cp.Style |= 0x20000000;
                    return;

                case FormWindowState.Maximized:
                    cp.Style |= 0x1000000;
                    return;
            }
        }

        private static System.Type FindClosestStockType(System.Type type)
        {
            System.Type[] typeArray = new System.Type[] { typeof(MenuStrip) };
            foreach (System.Type type2 in typeArray)
            {
                if (type2.IsAssignableFrom(type))
                {
                    return type2;
                }
            }
            return null;
        }

        internal override bool FocusInternal()
        {
            if (this.IsMdiChild)
            {
                this.MdiParentInternal.MdiClient.SendMessage(0x222, base.Handle, 0);
                return this.Focused;
            }
            return base.FocusInternal();
        }

        [Obsolete("This method has been deprecated. Use the AutoScaleDimensions property instead.  http://go.microsoft.com/fwlink/?linkid=14202"), EditorBrowsable(EditorBrowsableState.Never)]
        public static SizeF GetAutoScaleSize(Font font)
        {
            float height = font.Height;
            float width = 9f;
            try
            {
                using (Graphics graphics = Graphics.FromHwndInternal(IntPtr.Zero))
                {
                    string text = "The quick brown fox jumped over the lazy dog.";
                    double num3 = 44.549996948242189;
                    width = (float) (((double) graphics.MeasureString(text, font).Width) / num3);
                }
            }
            catch
            {
            }
            return new SizeF(width, height);
        }

        internal override System.Drawing.Size GetPreferredSizeCore(System.Drawing.Size proposedSize)
        {
            return base.GetPreferredSizeCore(proposedSize);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified)
        {
            if (this.WindowState != FormWindowState.Normal)
            {
                bounds = this.RestoreBounds;
            }
            return base.GetScaledBounds(bounds, factor, specified);
        }

        private void InvalidateMergedMenu()
        {
            if (base.Properties.ContainsObject(PropMergedMenu))
            {
                MainMenu menu = base.Properties.GetObject(PropMergedMenu) as MainMenu;
                if ((menu != null) && (menu.ownerForm == this))
                {
                    menu.Dispose();
                }
                base.Properties.SetObject(PropMergedMenu, null);
            }
            Form parentFormInternal = base.ParentFormInternal;
            if (parentFormInternal != null)
            {
                parentFormInternal.MenuChanged(0, parentFormInternal.Menu);
            }
        }

        public void LayoutMdi(MdiLayout value)
        {
            if (this.ctlClient != null)
            {
                this.ctlClient.LayoutMdi(value);
            }
        }

        internal void MenuChanged(int change, System.Windows.Forms.Menu menu)
        {
            Form parentFormInternal = base.ParentFormInternal;
            if ((parentFormInternal != null) && (this == parentFormInternal.ActiveMdiChildInternal))
            {
                parentFormInternal.MenuChanged(change, menu);
            }
            else
            {
                switch (change)
                {
                    case 0:
                    case 3:
                        if ((this.ctlClient != null) && this.ctlClient.IsHandleCreated)
                        {
                            if (base.IsHandleCreated)
                            {
                                this.UpdateMenuHandles(null, false);
                            }
                            Control.ControlCollection controls = this.ctlClient.Controls;
                            int count = controls.Count;
                            while (count-- > 0)
                            {
                                Control control = controls[count];
                                if ((control is Form) && control.Properties.ContainsObject(PropMergedMenu))
                                {
                                    MainMenu menu2 = control.Properties.GetObject(PropMergedMenu) as MainMenu;
                                    if ((menu2 != null) && (menu2.ownerForm == control))
                                    {
                                        menu2.Dispose();
                                    }
                                    control.Properties.SetObject(PropMergedMenu, null);
                                }
                            }
                            this.UpdateMenuHandles();
                            return;
                        }
                        if ((menu != this.Menu) || (change != 0))
                        {
                            break;
                        }
                        this.UpdateMenuHandles();
                        return;

                    case 1:
                        if ((menu != this.Menu) && ((this.ActiveMdiChildInternal == null) || (menu != this.ActiveMdiChildInternal.Menu)))
                        {
                            break;
                        }
                        this.UpdateMenuHandles();
                        return;

                    case 2:
                        if ((this.ctlClient != null) && this.ctlClient.IsHandleCreated)
                        {
                            this.UpdateMenuHandles();
                        }
                        break;

                    default:
                        return;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnActivated(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_ACTIVATED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal override void OnAutoScaleModeChanged()
        {
            base.OnAutoScaleModeChanged();
            if (this.formStateEx[FormStateExSettingAutoScale] != 1)
            {
                this.AutoScale = false;
            }
        }

        protected override void OnBackgroundImageChanged(EventArgs e)
        {
            base.OnBackgroundImageChanged(e);
            if (this.IsMdiContainer)
            {
                this.MdiClient.BackgroundImage = this.BackgroundImage;
                this.MdiClient.Invalidate();
            }
        }

        protected override void OnBackgroundImageLayoutChanged(EventArgs e)
        {
            base.OnBackgroundImageLayoutChanged(e);
            if (this.IsMdiContainer)
            {
                this.MdiClient.BackgroundImageLayout = this.BackgroundImageLayout;
                this.MdiClient.Invalidate();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnClosed(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_CLOSED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnClosing(CancelEventArgs e)
        {
            CancelEventHandler handler = (CancelEventHandler) base.Events[EVENT_CLOSING];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnCreateControl()
        {
            this.CalledCreateControl = true;
            base.OnCreateControl();
            if (this.CalledMakeVisible && !this.CalledOnLoad)
            {
                this.CalledOnLoad = true;
                this.OnLoad(EventArgs.Empty);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDeactivate(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_DEACTIVATE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            if ((!base.DesignMode && base.Enabled) && this.Active)
            {
                if (base.ActiveControl == null)
                {
                    base.SelectNextControlInternal(this, true, true, true, true);
                }
                else
                {
                    base.FocusActiveControlInternal();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            if (this.IsMdiChild)
            {
                base.UpdateFocusedControl();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnFontChanged(EventArgs e)
        {
            if (base.DesignMode)
            {
                this.UpdateAutoScaleBaseSize();
            }
            base.OnFontChanged(e);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnFormClosed(FormClosedEventArgs e)
        {
            Application.OpenFormsInternalRemove(this);
            FormClosedEventHandler handler = (FormClosedEventHandler) base.Events[EVENT_FORMCLOSED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnFormClosing(FormClosingEventArgs e)
        {
            FormClosingEventHandler handler = (FormClosingEventHandler) base.Events[EVENT_FORMCLOSING];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnHandleCreated(EventArgs e)
        {
            this.formStateEx[FormStateExUseMdiChildProc] = (this.IsMdiChild && base.Visible) ? 1 : 0;
            base.OnHandleCreated(e);
            this.UpdateLayered();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            this.formStateEx[FormStateExUseMdiChildProc] = 0;
            Application.OpenFormsInternalRemove(this);
            this.ResetSecurityTip(true);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnHelpButtonClicked(CancelEventArgs e)
        {
            CancelEventHandler handler = (CancelEventHandler) base.Events[EVENT_HELPBUTTONCLICKED];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnInputLanguageChanged(InputLanguageChangedEventArgs e)
        {
            InputLanguageChangedEventHandler handler = (InputLanguageChangedEventHandler) base.Events[EVENT_INPUTLANGCHANGE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnInputLanguageChanging(InputLanguageChangingEventArgs e)
        {
            InputLanguageChangingEventHandler handler = (InputLanguageChangingEventHandler) base.Events[EVENT_INPUTLANGCHANGEREQUEST];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (this.AutoSize)
            {
                System.Drawing.Size preferredSize = base.PreferredSize;
                this.minAutoSize = preferredSize;
                System.Drawing.Size size2 = (this.AutoSizeMode == System.Windows.Forms.AutoSizeMode.GrowAndShrink) ? preferredSize : LayoutUtils.UnionSizes(preferredSize, this.Size);
                IArrangedElement element = this;
                if (element != null)
                {
                    element.SetBounds(new Rectangle(base.Left, base.Top, size2.Width, size2.Height), BoundsSpecified.None);
                }
            }
            base.OnLayout(levent);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnLoad(EventArgs e)
        {
            Application.OpenFormsInternalAdd(this);
            if (Application.UseWaitCursor)
            {
                base.UseWaitCursor = true;
            }
            if ((this.formState[FormStateAutoScaling] == 1) && !base.DesignMode)
            {
                this.formState[FormStateAutoScaling] = 0;
                this.ApplyAutoScaling();
            }
            if (base.GetState(0x20))
            {
                switch (((FormStartPosition) this.formState[FormStateStartPos]))
                {
                    case FormStartPosition.CenterParent:
                        this.CenterToParent();
                        break;

                    case FormStartPosition.CenterScreen:
                        this.CenterToScreen();
                        break;
                }
            }
            EventHandler handler = (EventHandler) base.Events[EVENT_LOAD];
            if (handler != null)
            {
                string text = this.Text;
                handler(this, e);
                foreach (Control control in base.Controls)
                {
                    control.Invalidate();
                }
            }
            if (base.IsHandleCreated)
            {
                base.BeginInvoke(new MethodInvoker(this.CallShownEvent));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMaximizedBoundsChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_MAXIMIZEDBOUNDSCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMaximumSizeChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_MAXIMUMSIZECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMdiChildActivate(EventArgs e)
        {
            this.UpdateMenuHandles();
            this.UpdateToolStrip();
            EventHandler handler = (EventHandler) base.Events[EVENT_MDI_CHILD_ACTIVATE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMenuComplete(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_MENUCOMPLETE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMenuStart(EventArgs e)
        {
            SecurityToolTip tip = (SecurityToolTip) base.Properties.GetObject(PropSecurityTip);
            if (tip != null)
            {
                tip.Pop(true);
            }
            EventHandler handler = (EventHandler) base.Events[EVENT_MENUSTART];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnMinimumSizeChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_MINIMUMSIZECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.formState[FormStateRenderSizeGrip] != 0)
            {
                System.Drawing.Size clientSize = this.ClientSize;
                if (Application.RenderWithVisualStyles)
                {
                    if (this.sizeGripRenderer == null)
                    {
                        this.sizeGripRenderer = new VisualStyleRenderer(VisualStyleElement.Status.Gripper.Normal);
                    }
                    this.sizeGripRenderer.DrawBackground(e.Graphics, new Rectangle(clientSize.Width - 0x10, clientSize.Height - 0x10, 0x10, 0x10));
                }
                else
                {
                    ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor, clientSize.Width - 0x10, clientSize.Height - 0x10, 0x10, 0x10);
                }
            }
            if (this.IsMdiContainer)
            {
                e.Graphics.FillRectangle(SystemBrushes.AppWorkspace, base.ClientRectangle);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.formState[FormStateRenderSizeGrip] != 0)
            {
                base.Invalidate();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnResizeBegin(EventArgs e)
        {
            if (this.CanRaiseEvents)
            {
                EventHandler handler = (EventHandler) base.Events[EVENT_RESIZEBEGIN];
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnResizeEnd(EventArgs e)
        {
            if (this.CanRaiseEvents)
            {
                EventHandler handler = (EventHandler) base.Events[EVENT_RESIZEEND];
                if (handler != null)
                {
                    handler(this, e);
                }
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
                EventHandler handler = base.Events[EVENT_RIGHTTOLEFTLAYOUTCHANGED] as EventHandler;
                if (handler != null)
                {
                    handler(this, e);
                }
                if (this.RightToLeft == RightToLeft.Yes)
                {
                    foreach (Control control in base.Controls)
                    {
                        control.RecreateHandleCore();
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnShown(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_SHOWN];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnStyleChanged(EventArgs e)
        {
            base.OnStyleChanged(e);
            this.AdjustSystemMenu();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            int num = (this.Text.Length == 0) ? 1 : 0;
            if (!this.ControlBox && (this.formState[FormStateIsTextEmpty] != num))
            {
                base.RecreateHandle();
            }
            this.formState[FormStateIsTextEmpty] = num;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnVisibleChanged(EventArgs e)
        {
            this.UpdateRenderSizeGrip();
            Form mdiParentInternal = this.MdiParentInternal;
            if (mdiParentInternal != null)
            {
                mdiParentInternal.UpdateMdiWindowListStrip();
            }
            base.OnVisibleChanged(e);
            bool flag = false;
            if (((base.IsHandleCreated && base.Visible) && ((this.AcceptButton != null) && System.Windows.Forms.UnsafeNativeMethods.SystemParametersInfo(0x5f, 0, ref flag, 0))) && flag)
            {
                Control acceptButton = this.AcceptButton as Control;
                System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT(acceptButton.Left + (acceptButton.Width / 2), acceptButton.Top + (acceptButton.Height / 2));
                System.Windows.Forms.UnsafeNativeMethods.ClientToScreen(new HandleRef(this, base.Handle), pt);
                if (!acceptButton.IsWindowObscured)
                {
                    System.Windows.Forms.IntSecurity.AdjustCursorPosition.Assert();
                    try
                    {
                        Cursor.Position = new Point(pt.x, pt.y);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
        }

        internal void PerformOnInputLanguageChanged(InputLanguageChangedEventArgs iplevent)
        {
            this.OnInputLanguageChanged(iplevent);
        }

        internal void PerformOnInputLanguageChanging(InputLanguageChangingEventArgs iplcevent)
        {
            this.OnInputLanguageChanging(iplcevent);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (base.ProcessCmdKey(ref msg, keyData))
            {
                return true;
            }
            MainMenu menu = (MainMenu) base.Properties.GetObject(PropCurMenu);
            if ((menu != null) && menu.ProcessCmdKey(ref msg, keyData))
            {
                return true;
            }
            bool flag = false;
            System.Windows.Forms.NativeMethods.MSG msg2 = new System.Windows.Forms.NativeMethods.MSG {
                message = msg.Msg,
                wParam = msg.WParam,
                lParam = msg.LParam,
                hwnd = msg.HWnd
            };
            if (((this.ctlClient != null) && (this.ctlClient.Handle != IntPtr.Zero)) && System.Windows.Forms.UnsafeNativeMethods.TranslateMDISysAccel(this.ctlClient.Handle, ref msg2))
            {
                flag = true;
            }
            msg.Msg = msg2.message;
            msg.WParam = msg2.wParam;
            msg.LParam = msg2.lParam;
            msg.HWnd = msg2.hwnd;
            return flag;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogChar(char charCode)
        {
            if (this.IsMdiChild && (charCode != ' '))
            {
                if (this.ProcessMnemonic(charCode))
                {
                    return true;
                }
                this.formStateEx[FormStateExMnemonicProcessed] = 1;
                try
                {
                    return base.ProcessDialogChar(charCode);
                }
                finally
                {
                    this.formStateEx[FormStateExMnemonicProcessed] = 0;
                }
            }
            return base.ProcessDialogChar(charCode);
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData & (Keys.Alt | Keys.Control)) == Keys.None)
            {
                IButtonControl control;
                switch ((keyData & Keys.KeyCode))
                {
                    case Keys.Enter:
                        control = (IButtonControl) base.Properties.GetObject(PropDefaultButton);
                        if (control != null)
                        {
                            if (control is Control)
                            {
                                control.PerformClick();
                            }
                            return true;
                        }
                        break;

                    case Keys.Escape:
                        control = (IButtonControl) base.Properties.GetObject(PropCancelButton);
                        if (control != null)
                        {
                            control.PerformClick();
                            return true;
                        }
                        break;
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessKeyPreview(ref Message m)
        {
            return (((this.formState[FormStateKeyPreview] != 0) && this.ProcessKeyEventArgs(ref m)) || base.ProcessKeyPreview(ref m));
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if (base.ProcessMnemonic(charCode))
            {
                return true;
            }
            if (this.IsMdiContainer && (base.Controls.Count > 1))
            {
                for (int i = 0; i < base.Controls.Count; i++)
                {
                    Control control = base.Controls[i];
                    if (!(control is System.Windows.Forms.MdiClient) && control.ProcessMnemonic(charCode))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected override bool ProcessTabKey(bool forward)
        {
            return (base.SelectNextControl(base.ActiveControl, forward, true, true, true) || ((this.IsMdiChild || (base.ParentFormInternal == null)) && base.SelectNextControl(null, forward, true, true, false)));
        }

        internal void RaiseFormClosedOnAppExit()
        {
            if (!this.Modal)
            {
                int integer = base.Properties.GetInteger(PropOwnedFormsCount);
                if (integer > 0)
                {
                    Form[] ownedForms = this.OwnedForms;
                    FormClosedEventArgs e = new FormClosedEventArgs(System.Windows.Forms.CloseReason.FormOwnerClosing);
                    for (int i = integer - 1; i >= 0; i--)
                    {
                        if ((ownedForms[i] != null) && !Application.OpenFormsInternal.Contains(ownedForms[i]))
                        {
                            ownedForms[i].OnFormClosed(e);
                        }
                    }
                }
            }
            this.OnFormClosed(new FormClosedEventArgs(System.Windows.Forms.CloseReason.ApplicationExitCall));
        }

        internal bool RaiseFormClosingOnAppExit()
        {
            FormClosingEventArgs e = new FormClosingEventArgs(System.Windows.Forms.CloseReason.ApplicationExitCall, false);
            if (!this.Modal)
            {
                int integer = base.Properties.GetInteger(PropOwnedFormsCount);
                if (integer > 0)
                {
                    Form[] ownedForms = this.OwnedForms;
                    FormClosingEventArgs args2 = new FormClosingEventArgs(System.Windows.Forms.CloseReason.FormOwnerClosing, false);
                    for (int i = integer - 1; i >= 0; i--)
                    {
                        if ((ownedForms[i] != null) && !Application.OpenFormsInternal.Contains(ownedForms[i]))
                        {
                            ownedForms[i].OnFormClosing(args2);
                            if (args2.Cancel)
                            {
                                e.Cancel = true;
                                break;
                            }
                        }
                    }
                }
            }
            this.OnFormClosing(e);
            return e.Cancel;
        }

        internal override void RecreateHandleCore()
        {
            System.Windows.Forms.NativeMethods.WINDOWPLACEMENT placement = new System.Windows.Forms.NativeMethods.WINDOWPLACEMENT();
            FormStartPosition manual = FormStartPosition.Manual;
            if (!this.IsMdiChild && ((this.WindowState == FormWindowState.Minimized) || (this.WindowState == FormWindowState.Maximized)))
            {
                placement.length = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.WINDOWPLACEMENT));
                System.Windows.Forms.UnsafeNativeMethods.GetWindowPlacement(new HandleRef(this, base.Handle), ref placement);
            }
            if (this.StartPosition != FormStartPosition.Manual)
            {
                manual = this.StartPosition;
                this.StartPosition = FormStartPosition.Manual;
            }
            EnumThreadWindowsCallback callback = null;
            System.Windows.Forms.SafeNativeMethods.EnumThreadWindowsCallback callback2 = null;
            if (base.IsHandleCreated)
            {
                callback = new EnumThreadWindowsCallback();
                if (callback != null)
                {
                    callback2 = new System.Windows.Forms.SafeNativeMethods.EnumThreadWindowsCallback(callback.Callback);
                    System.Windows.Forms.UnsafeNativeMethods.EnumThreadWindows(System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId(), new System.Windows.Forms.NativeMethods.EnumThreadWindowsCallback(callback2.Invoke), new HandleRef(this, base.Handle));
                    callback.ResetOwners();
                }
            }
            base.RecreateHandleCore();
            if (callback != null)
            {
                callback.SetOwners(new HandleRef(this, base.Handle));
            }
            if (manual != FormStartPosition.Manual)
            {
                this.StartPosition = manual;
            }
            if (placement.length > 0)
            {
                System.Windows.Forms.UnsafeNativeMethods.SetWindowPlacement(new HandleRef(this, base.Handle), ref placement);
            }
            if (callback2 != null)
            {
                GC.KeepAlive(callback2);
            }
        }

        public void RemoveOwnedForm(Form ownedForm)
        {
            if (ownedForm != null)
            {
                if (ownedForm.OwnerInternal != null)
                {
                    ownedForm.Owner = null;
                }
                else
                {
                    Form[] sourceArray = (Form[]) base.Properties.GetObject(PropOwnedForms);
                    int integer = base.Properties.GetInteger(PropOwnedFormsCount);
                    if (sourceArray != null)
                    {
                        for (int i = 0; i < integer; i++)
                        {
                            if (ownedForm.Equals(sourceArray[i]))
                            {
                                sourceArray[i] = null;
                                if ((i + 1) < integer)
                                {
                                    Array.Copy(sourceArray, i + 1, sourceArray, i, (integer - i) - 1);
                                    sourceArray[integer - 1] = null;
                                }
                                integer--;
                            }
                        }
                        base.Properties.SetInteger(PropOwnedFormsCount, integer);
                    }
                }
            }
        }

        private void ResetIcon()
        {
            this.icon = null;
            if (this.smallIcon != null)
            {
                this.smallIcon.Dispose();
                this.smallIcon = null;
            }
            this.formState[FormStateIconSet] = 0;
            this.UpdateWindowIcon(true);
        }

        private void ResetSecurityTip(bool modalOnly)
        {
            SecurityToolTip tip = (SecurityToolTip) base.Properties.GetObject(PropSecurityTip);
            if ((tip != null) && ((modalOnly && tip.Modal) || !modalOnly))
            {
                tip.Dispose();
                tip = null;
                base.Properties.SetObject(PropSecurityTip, null);
            }
        }

        private void ResetTransparencyKey()
        {
            this.TransparencyKey = Color.Empty;
        }

        private void ResolveZoneAndSiteNames(ArrayList sites, ref string securityZone, ref string securitySite)
        {
            securityZone = System.Windows.Forms.SR.GetString("SecurityRestrictedWindowTextUnknownZone");
            securitySite = System.Windows.Forms.SR.GetString("SecurityRestrictedWindowTextUnknownSite");
            try
            {
                if ((sites != null) && (sites.Count != 0))
                {
                    ArrayList list = new ArrayList();
                    foreach (object obj2 in sites)
                    {
                        if (obj2 == null)
                        {
                            return;
                        }
                        string url = obj2.ToString();
                        if (url.Length == 0)
                        {
                            return;
                        }
                        Zone zone = Zone.CreateFromUrl(url);
                        if (!zone.SecurityZone.Equals(SecurityZone.MyComputer))
                        {
                            string item = zone.SecurityZone.ToString();
                            if (!list.Contains(item))
                            {
                                list.Add(item);
                            }
                        }
                    }
                    if (list.Count == 0)
                    {
                        securityZone = SecurityZone.MyComputer.ToString();
                    }
                    else if (list.Count == 1)
                    {
                        securityZone = list[0].ToString();
                    }
                    else
                    {
                        securityZone = System.Windows.Forms.SR.GetString("SecurityRestrictedWindowTextMixedZone");
                    }
                    ArrayList list2 = new ArrayList();
                    new FileIOPermission(PermissionState.None) { AllFiles = FileIOPermissionAccess.PathDiscovery }.Assert();
                    try
                    {
                        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (assembly.GlobalAssemblyCache)
                            {
                                list2.Add(assembly.CodeBase.ToUpper(CultureInfo.InvariantCulture));
                            }
                        }
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    ArrayList list3 = new ArrayList();
                    foreach (object obj3 in sites)
                    {
                        Uri uri = new Uri(obj3.ToString());
                        if (!list2.Contains(uri.AbsoluteUri.ToUpper(CultureInfo.InvariantCulture)))
                        {
                            string host = uri.Host;
                            if ((host.Length > 0) && !list3.Contains(host))
                            {
                                list3.Add(host);
                            }
                        }
                    }
                    if (list3.Count == 0)
                    {
                        new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                        try
                        {
                            securitySite = Environment.MachineName;
                            return;
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                    if (list3.Count == 1)
                    {
                        securitySite = list3[0].ToString();
                    }
                    else
                    {
                        securitySite = System.Windows.Forms.SR.GetString("SecurityRestrictedWindowTextMultipleSites");
                    }
                }
            }
            catch
            {
            }
        }

        private void RestoreWindowBoundsIfNecessary()
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                System.Drawing.Size size = this.restoredWindowBounds.Size;
                if ((this.restoredWindowBoundsSpecified & BoundsSpecified.Size) != BoundsSpecified.None)
                {
                    size = base.SizeFromClientSize(size.Width, size.Height);
                }
                base.SetBounds(this.restoredWindowBounds.X, this.restoredWindowBounds.Y, (this.formStateEx[FormStateExWindowBoundsWidthIsClientSize] == 1) ? size.Width : this.restoredWindowBounds.Width, (this.formStateEx[FormStateExWindowBoundsHeightIsClientSize] == 1) ? size.Height : this.restoredWindowBounds.Height, this.restoredWindowBoundsSpecified);
                this.restoredWindowBoundsSpecified = BoundsSpecified.None;
                this.restoredWindowBounds = new Rectangle(-1, -1, -1, -1);
                this.formStateEx[FormStateExWindowBoundsHeightIsClientSize] = 0;
                this.formStateEx[FormStateExWindowBoundsWidthIsClientSize] = 0;
            }
        }

        private void RestrictedProcessNcActivate()
        {
            if (!base.IsDisposed && !base.Disposing)
            {
                SecurityToolTip tip = (SecurityToolTip) base.Properties.GetObject(PropSecurityTip);
                if (tip == null)
                {
                    if (base.IsHandleCreated && (System.Windows.Forms.UnsafeNativeMethods.GetForegroundWindow() == base.Handle))
                    {
                        tip = new SecurityToolTip(this);
                        base.Properties.SetObject(PropSecurityTip, tip);
                    }
                }
                else if (!base.IsHandleCreated || (System.Windows.Forms.UnsafeNativeMethods.GetForegroundWindow() != base.Handle))
                {
                    tip.Pop(false);
                }
                else
                {
                    tip.Show();
                }
            }
        }

        private string RestrictedWindowText(string original)
        {
            this.EnsureSecurityInformation();
            return string.Format(CultureInfo.CurrentCulture, Application.SafeTopLevelCaptionFormat, new object[] { original, this.securityZone, this.securitySite });
        }

        private void ResumeLayoutFromMinimize()
        {
            if (this.formState[FormStateWindowState] == 1)
            {
                base.ResumeLayout();
            }
        }

        private void ResumeUpdateMenuHandles()
        {
            int num = this.formStateEx[FormStateExUpdateMenuHandlesSuspendCount];
            if (num <= 0)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("TooManyResumeUpdateMenuHandles"));
            }
            this.formStateEx[FormStateExUpdateMenuHandlesSuspendCount] = --num;
            if ((num == 0) && (this.formStateEx[FormStateExUpdateMenuHandlesDeferred] != 0))
            {
                this.UpdateMenuHandles();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            this.formStateEx[FormStateExInScale] = 1;
            try
            {
                if (this.MdiParentInternal != null)
                {
                    specified &= ~BoundsSpecified.Location;
                }
                base.ScaleControl(factor, specified);
            }
            finally
            {
                this.formStateEx[FormStateExInScale] = 0;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void ScaleCore(float x, float y)
        {
            base.SuspendLayout();
            try
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    System.Drawing.Size clientSize = this.ClientSize;
                    System.Drawing.Size minimumSize = this.MinimumSize;
                    System.Drawing.Size maximumSize = this.MaximumSize;
                    if (!this.MinimumSize.IsEmpty)
                    {
                        this.MinimumSize = base.ScaleSize(minimumSize, x, y);
                    }
                    if (!this.MaximumSize.IsEmpty)
                    {
                        this.MaximumSize = base.ScaleSize(maximumSize, x, y);
                    }
                    this.ClientSize = base.ScaleSize(clientSize, x, y);
                }
                base.ScaleDockPadding(x, y);
                foreach (Control control in base.Controls)
                {
                    if (control != null)
                    {
                        control.Scale(x, y);
                    }
                }
            }
            finally
            {
                base.ResumeLayout();
            }
        }

        protected override void Select(bool directed, bool forward)
        {
            System.Windows.Forms.IntSecurity.ModifyFocus.Demand();
            this.SelectInternal(directed, forward);
        }

        private void SelectInternal(bool directed, bool forward)
        {
            System.Windows.Forms.IntSecurity.ModifyFocus.Assert();
            if (directed)
            {
                base.SelectNextControl(null, forward, true, true, false);
            }
            if (this.TopLevel)
            {
                System.Windows.Forms.UnsafeNativeMethods.SetActiveWindow(new HandleRef(this, base.Handle));
            }
            else if (this.IsMdiChild)
            {
                System.Windows.Forms.UnsafeNativeMethods.SetActiveWindow(new HandleRef(this.MdiParentInternal, this.MdiParentInternal.Handle));
                this.MdiParentInternal.MdiClient.SendMessage(0x222, base.Handle, 0);
            }
            else
            {
                Form parentFormInternal = base.ParentFormInternal;
                if (parentFormInternal != null)
                {
                    parentFormInternal.ActiveControl = this;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (this.WindowState != FormWindowState.Normal)
            {
                if ((x != -1) || (y != -1))
                {
                    this.restoredWindowBoundsSpecified |= specified & BoundsSpecified.Location;
                }
                this.restoredWindowBoundsSpecified |= specified & BoundsSpecified.Size;
                if ((specified & BoundsSpecified.X) != BoundsSpecified.None)
                {
                    this.restoredWindowBounds.X = x;
                }
                if ((specified & BoundsSpecified.Y) != BoundsSpecified.None)
                {
                    this.restoredWindowBounds.Y = y;
                }
                if ((specified & BoundsSpecified.Width) != BoundsSpecified.None)
                {
                    this.restoredWindowBounds.Width = width;
                    this.formStateEx[FormStateExWindowBoundsWidthIsClientSize] = 0;
                }
                if ((specified & BoundsSpecified.Height) != BoundsSpecified.None)
                {
                    this.restoredWindowBounds.Height = height;
                    this.formStateEx[FormStateExWindowBoundsHeightIsClientSize] = 0;
                }
            }
            if ((specified & BoundsSpecified.X) != BoundsSpecified.None)
            {
                this.restoreBounds.X = x;
            }
            if ((specified & BoundsSpecified.Y) != BoundsSpecified.None)
            {
                this.restoreBounds.Y = y;
            }
            if (((specified & BoundsSpecified.Width) != BoundsSpecified.None) || (this.restoreBounds.Width == -1))
            {
                this.restoreBounds.Width = width;
            }
            if (((specified & BoundsSpecified.Height) != BoundsSpecified.None) || (this.restoreBounds.Height == -1))
            {
                this.restoreBounds.Height = height;
            }
            if ((this.WindowState == FormWindowState.Normal) && ((base.Height != height) || (base.Width != width)))
            {
                System.Drawing.Size maxWindowTrackSize = SystemInformation.MaxWindowTrackSize;
                if (height > maxWindowTrackSize.Height)
                {
                    height = maxWindowTrackSize.Height;
                }
                if (width > maxWindowTrackSize.Width)
                {
                    width = maxWindowTrackSize.Width;
                }
            }
            System.Windows.Forms.FormBorderStyle formBorderStyle = this.FormBorderStyle;
            if (((formBorderStyle != System.Windows.Forms.FormBorderStyle.None) && (formBorderStyle != System.Windows.Forms.FormBorderStyle.FixedToolWindow)) && ((formBorderStyle != System.Windows.Forms.FormBorderStyle.SizableToolWindow) && (this.ParentInternal == null)))
            {
                System.Drawing.Size minWindowTrackSize = SystemInformation.MinWindowTrackSize;
                if (height < minWindowTrackSize.Height)
                {
                    height = minWindowTrackSize.Height;
                }
                if (width < minWindowTrackSize.Width)
                {
                    width = minWindowTrackSize.Width;
                }
            }
            if (this.IsRestrictedWindow)
            {
                Rectangle rectangle = this.ApplyBoundsConstraints(x, y, width, height);
                if (rectangle != new Rectangle(x, y, width, height))
                {
                    base.SetBoundsCore(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, BoundsSpecified.All);
                    return;
                }
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void SetClientSizeCore(int x, int y)
        {
            bool hScroll = base.HScroll;
            bool vScroll = base.VScroll;
            base.SetClientSizeCore(x, y);
            if (base.IsHandleCreated)
            {
                if ((base.VScroll != vScroll) && base.VScroll)
                {
                    x += SystemInformation.VerticalScrollBarWidth;
                }
                if ((base.HScroll != hScroll) && base.HScroll)
                {
                    y += SystemInformation.HorizontalScrollBarHeight;
                }
                if ((x != this.ClientSize.Width) || (y != this.ClientSize.Height))
                {
                    base.SetClientSizeCore(x, y);
                }
            }
            this.formState[FormStateSetClientSize] = 1;
        }

        private void SetDefaultButton(IButtonControl button)
        {
            IButtonControl control = (IButtonControl) base.Properties.GetObject(PropDefaultButton);
            if (control != button)
            {
                if (control != null)
                {
                    control.NotifyDefault(false);
                }
                base.Properties.SetObject(PropDefaultButton, button);
                if (button != null)
                {
                    button.NotifyDefault(true);
                }
            }
        }

        public void SetDesktopBounds(int x, int y, int width, int height)
        {
            Rectangle workingArea = SystemInformation.WorkingArea;
            base.SetBounds(x + workingArea.X, y + workingArea.Y, width, height, BoundsSpecified.All);
        }

        public void SetDesktopLocation(int x, int y)
        {
            Rectangle workingArea = SystemInformation.WorkingArea;
            this.Location = new Point(workingArea.X + x, workingArea.Y + y);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void SetVisibleCore(bool value)
        {
            if ((this.GetVisibleCore() != value) || (this.dialogResult != System.Windows.Forms.DialogResult.OK))
            {
                if ((this.GetVisibleCore() == value) && (!value || this.CalledMakeVisible))
                {
                    base.SetVisibleCore(value);
                }
                else
                {
                    if (value)
                    {
                        this.CalledMakeVisible = true;
                        if (this.CalledCreateControl)
                        {
                            if (this.CalledOnLoad)
                            {
                                if (!Application.OpenFormsInternal.Contains(this))
                                {
                                    Application.OpenFormsInternalAdd(this);
                                }
                            }
                            else
                            {
                                this.CalledOnLoad = true;
                                this.OnLoad(EventArgs.Empty);
                                if (this.dialogResult != System.Windows.Forms.DialogResult.None)
                                {
                                    value = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        this.ResetSecurityTip(true);
                    }
                    if (!this.IsMdiChild)
                    {
                        base.SetVisibleCore(value);
                        if (this.formState[FormStateSWCalled] == 0)
                        {
                            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x18, value ? 1 : 0, 0);
                        }
                    }
                    else
                    {
                        if (base.IsHandleCreated)
                        {
                            this.DestroyHandle();
                        }
                        if (!value)
                        {
                            this.InvalidateMergedMenu();
                            base.SetState(2, false);
                        }
                        else
                        {
                            base.SetState(2, true);
                            this.MdiParentInternal.MdiClient.PerformLayout();
                            if ((this.ParentInternal != null) && this.ParentInternal.Visible)
                            {
                                base.SuspendLayout();
                                try
                                {
                                    System.Windows.Forms.SafeNativeMethods.ShowWindow(new HandleRef(this, base.Handle), 5);
                                    base.CreateControl();
                                    if (this.WindowState == FormWindowState.Maximized)
                                    {
                                        this.MdiParentInternal.UpdateWindowIcon(true);
                                    }
                                }
                                finally
                                {
                                    base.ResumeLayout();
                                }
                            }
                        }
                        this.OnVisibleChanged(EventArgs.Empty);
                    }
                    if ((value && !this.IsMdiChild) && ((this.WindowState == FormWindowState.Maximized) || this.TopMost))
                    {
                        if (base.ActiveControl == null)
                        {
                            base.SelectNextControlInternal(null, true, true, true, false);
                        }
                        base.FocusActiveControlInternal();
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual bool ShouldSerializeAutoScaleBaseSize()
        {
            return (this.formState[FormStateAutoScaling] != 0);
        }

        private bool ShouldSerializeClientSize()
        {
            return true;
        }

        private bool ShouldSerializeIcon()
        {
            return (this.formState[FormStateIconSet] == 1);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private bool ShouldSerializeLocation()
        {
            if (base.Left == 0)
            {
                return (base.Top != 0);
            }
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal override bool ShouldSerializeSize()
        {
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal bool ShouldSerializeTransparencyKey()
        {
            return !this.TransparencyKey.Equals(Color.Empty);
        }

        public void Show(IWin32Window owner)
        {
            if (owner == this)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("OwnsSelfOrOwner", new object[] { "Show" }));
            }
            if (base.Visible)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ShowDialogOnVisible", new object[] { "Show" }));
            }
            if (!base.Enabled)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ShowDialogOnDisabled", new object[] { "Show" }));
            }
            if (!this.TopLevel)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ShowDialogOnNonTopLevel", new object[] { "Show" }));
            }
            if (!SystemInformation.UserInteractive)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("CantShowModalOnNonInteractive"));
            }
            if (((owner != null) && ((((int) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(owner, Control.GetSafeHandle(owner)), -20)) & 8) == 0)) && (owner is Control))
            {
                owner = ((Control) owner).TopLevelControlInternal;
            }
            IntPtr activeWindow = System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow();
            IntPtr handle = (owner == null) ? activeWindow : Control.GetSafeHandle(owner);
            base.Properties.SetObject(PropDialogOwner, owner);
            Form ownerInternal = this.OwnerInternal;
            if ((owner is Form) && (owner != ownerInternal))
            {
                this.Owner = (Form) owner;
            }
            if ((handle != IntPtr.Zero) && (handle != base.Handle))
            {
                if (System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(owner, handle), -8) == base.Handle)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("OwnsSelfOrOwner", new object[] { "show" }), "owner");
                }
                System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.Handle), -8);
                System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, base.Handle), -8, new HandleRef(owner, handle));
            }
            base.Visible = true;
        }

        public System.Windows.Forms.DialogResult ShowDialog()
        {
            return this.ShowDialog(null);
        }

        public System.Windows.Forms.DialogResult ShowDialog(IWin32Window owner)
        {
            if (owner == this)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("OwnsSelfOrOwner", new object[] { "showDialog" }), "owner");
            }
            if (base.Visible)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ShowDialogOnVisible", new object[] { "showDialog" }));
            }
            if (!base.Enabled)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ShowDialogOnDisabled", new object[] { "showDialog" }));
            }
            if (!this.TopLevel)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ShowDialogOnNonTopLevel", new object[] { "showDialog" }));
            }
            if (this.Modal)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ShowDialogOnModal", new object[] { "showDialog" }));
            }
            if (!SystemInformation.UserInteractive)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("CantShowModalOnNonInteractive"));
            }
            if (((owner != null) && ((((int) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(owner, Control.GetSafeHandle(owner)), -20)) & 8) == 0)) && (owner is Control))
            {
                owner = ((Control) owner).TopLevelControlInternal;
            }
            this.CalledOnLoad = false;
            this.CalledMakeVisible = false;
            this.CloseReason = System.Windows.Forms.CloseReason.None;
            IntPtr capture = System.Windows.Forms.UnsafeNativeMethods.GetCapture();
            if (capture != IntPtr.Zero)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(null, capture), 0x1f, IntPtr.Zero, IntPtr.Zero);
                System.Windows.Forms.SafeNativeMethods.ReleaseCapture();
            }
            IntPtr activeWindow = System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow();
            IntPtr handle = (owner == null) ? activeWindow : Control.GetSafeHandle(owner);
            base.Properties.SetObject(PropDialogOwner, owner);
            Form ownerInternal = this.OwnerInternal;
            if ((owner is Form) && (owner != ownerInternal))
            {
                this.Owner = (Form) owner;
            }
            try
            {
                base.SetState(0x20, true);
                this.dialogResult = System.Windows.Forms.DialogResult.None;
                base.CreateControl();
                if ((handle != IntPtr.Zero) && (handle != base.Handle))
                {
                    if (System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(owner, handle), -8) == base.Handle)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("OwnsSelfOrOwner", new object[] { "showDialog" }), "owner");
                    }
                    System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.Handle), -8);
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, base.Handle), -8, new HandleRef(owner, handle));
                }
                try
                {
                    if (this.dialogResult == System.Windows.Forms.DialogResult.None)
                    {
                        Application.RunDialog(this);
                    }
                }
                finally
                {
                    if (!System.Windows.Forms.UnsafeNativeMethods.IsWindow(new HandleRef(null, activeWindow)))
                    {
                        activeWindow = handle;
                    }
                    if (System.Windows.Forms.UnsafeNativeMethods.IsWindow(new HandleRef(null, activeWindow)) && System.Windows.Forms.SafeNativeMethods.IsWindowVisible(new HandleRef(null, activeWindow)))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetActiveWindow(new HandleRef(null, activeWindow));
                    }
                    else if (System.Windows.Forms.UnsafeNativeMethods.IsWindow(new HandleRef(null, handle)) && System.Windows.Forms.SafeNativeMethods.IsWindowVisible(new HandleRef(null, handle)))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetActiveWindow(new HandleRef(null, handle));
                    }
                    this.SetVisibleCore(false);
                    if (base.IsHandleCreated)
                    {
                        if ((this.OwnerInternal != null) && this.OwnerInternal.IsMdiContainer)
                        {
                            this.OwnerInternal.Invalidate(true);
                            this.OwnerInternal.Update();
                        }
                        this.DestroyHandle();
                    }
                    base.SetState(0x20, false);
                }
            }
            finally
            {
                this.Owner = ownerInternal;
                base.Properties.SetObject(PropDialogOwner, null);
            }
            return this.DialogResult;
        }

        private void SuspendLayoutForMinimize()
        {
            if (this.formState[FormStateWindowState] != 1)
            {
                base.SuspendLayout();
            }
        }

        private void SuspendUpdateMenuHandles()
        {
            int num = this.formStateEx[FormStateExUpdateMenuHandlesSuspendCount];
            this.formStateEx[FormStateExUpdateMenuHandlesSuspendCount] = ++num;
        }

        public override string ToString()
        {
            return (base.ToString() + ", Text: " + this.Text);
        }

        private void UpdateAutoScaleBaseSize()
        {
            this.autoScaleBaseSize = System.Drawing.Size.Empty;
        }

        protected override void UpdateDefaultButton()
        {
            ContainerControl activeControl = this;
            while (activeControl.ActiveControl is ContainerControl)
            {
                activeControl = activeControl.ActiveControl as ContainerControl;
                if (activeControl is Form)
                {
                    activeControl = this;
                    break;
                }
            }
            if (activeControl.ActiveControl is IButtonControl)
            {
                this.SetDefaultButton((IButtonControl) activeControl.ActiveControl);
            }
            else
            {
                this.SetDefaultButton(this.AcceptButton);
            }
        }

        internal void UpdateFormStyles()
        {
            System.Drawing.Size clientSize = this.ClientSize;
            base.UpdateStyles();
            if (!this.ClientSize.Equals(clientSize))
            {
                this.ClientSize = clientSize;
            }
        }

        private void UpdateHandleWithOwner()
        {
            if (base.IsHandleCreated && this.TopLevel)
            {
                HandleRef nullHandleRef = System.Windows.Forms.NativeMethods.NullHandleRef;
                Form wrapper = (Form) base.Properties.GetObject(PropOwner);
                if (wrapper != null)
                {
                    nullHandleRef = new HandleRef(wrapper, wrapper.Handle);
                }
                else if (!this.ShowInTaskbar)
                {
                    nullHandleRef = this.TaskbarOwner;
                }
                System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, base.Handle), -8, nullHandleRef);
            }
        }

        private void UpdateLayered()
        {
            if (((this.formState[FormStateLayered] != 0) && base.IsHandleCreated) && (this.TopLevel && OSFeature.Feature.IsPresent(OSFeature.LayeredWindows)))
            {
                bool flag;
                Color transparencyKey = this.TransparencyKey;
                if (transparencyKey.IsEmpty)
                {
                    flag = System.Windows.Forms.UnsafeNativeMethods.SetLayeredWindowAttributes(new HandleRef(this, base.Handle), 0, this.OpacityAsByte, 2);
                }
                else if (this.OpacityAsByte == 0xff)
                {
                    flag = System.Windows.Forms.UnsafeNativeMethods.SetLayeredWindowAttributes(new HandleRef(this, base.Handle), ColorTranslator.ToWin32(transparencyKey), 0, 1);
                }
                else
                {
                    flag = System.Windows.Forms.UnsafeNativeMethods.SetLayeredWindowAttributes(new HandleRef(this, base.Handle), ColorTranslator.ToWin32(transparencyKey), this.OpacityAsByte, 3);
                }
                if (!flag)
                {
                    throw new Win32Exception();
                }
            }
        }

        private void UpdateMdiControlStrip(bool maximized)
        {
            if (this.formStateEx[FormStateExInUpdateMdiControlStrip] == 0)
            {
                this.formStateEx[FormStateExInUpdateMdiControlStrip] = 1;
                try
                {
                    System.Windows.Forms.MdiControlStrip mdiControlStrip = this.MdiControlStrip;
                    if (this.MdiControlStrip != null)
                    {
                        if (mdiControlStrip.MergedMenu != null)
                        {
                            ToolStripManager.RevertMergeInternal(mdiControlStrip.MergedMenu, mdiControlStrip, true);
                        }
                        mdiControlStrip.MergedMenu = null;
                        mdiControlStrip.Dispose();
                        this.MdiControlStrip = null;
                    }
                    if ((((this.ActiveMdiChildInternal != null) && maximized) && (this.ActiveMdiChildInternal.ControlBox && (this.Menu == null))) && (System.Windows.Forms.UnsafeNativeMethods.GetMenu(new HandleRef(this, base.Handle)) == IntPtr.Zero))
                    {
                        MenuStrip mainMenuStrip = ToolStripManager.GetMainMenuStrip(this);
                        if (mainMenuStrip != null)
                        {
                            this.MdiControlStrip = new System.Windows.Forms.MdiControlStrip(this.ActiveMdiChildInternal);
                            ToolStripManager.Merge(this.MdiControlStrip, mainMenuStrip);
                            this.MdiControlStrip.MergedMenu = mainMenuStrip;
                        }
                    }
                }
                finally
                {
                    this.formStateEx[FormStateExInUpdateMdiControlStrip] = 0;
                }
            }
        }

        internal void UpdateMdiWindowListStrip()
        {
            if (this.IsMdiContainer)
            {
                if ((this.MdiWindowListStrip != null) && (this.MdiWindowListStrip.MergedMenu != null))
                {
                    ToolStripManager.RevertMergeInternal(this.MdiWindowListStrip.MergedMenu, this.MdiWindowListStrip, true);
                }
                MenuStrip mainMenuStrip = ToolStripManager.GetMainMenuStrip(this);
                if ((mainMenuStrip != null) && (mainMenuStrip.MdiWindowListItem != null))
                {
                    if (this.MdiWindowListStrip == null)
                    {
                        this.MdiWindowListStrip = new System.Windows.Forms.MdiWindowListStrip();
                    }
                    int count = mainMenuStrip.MdiWindowListItem.DropDownItems.Count;
                    bool includeSeparator = (count > 0) && !(mainMenuStrip.MdiWindowListItem.DropDownItems[count - 1] is ToolStripSeparator);
                    this.MdiWindowListStrip.PopulateItems(this, mainMenuStrip.MdiWindowListItem, includeSeparator);
                    ToolStripManager.Merge(this.MdiWindowListStrip, mainMenuStrip);
                    this.MdiWindowListStrip.MergedMenu = mainMenuStrip;
                }
            }
        }

        private void UpdateMenuHandles()
        {
            if (base.Properties.GetObject(PropCurMenu) != null)
            {
                base.Properties.SetObject(PropCurMenu, null);
            }
            if (base.IsHandleCreated)
            {
                if (!this.TopLevel)
                {
                    this.UpdateMenuHandles(null, true);
                }
                else
                {
                    Form activeMdiChildInternal = this.ActiveMdiChildInternal;
                    if (activeMdiChildInternal != null)
                    {
                        this.UpdateMenuHandles(activeMdiChildInternal.MergedMenuPrivate, true);
                    }
                    else
                    {
                        this.UpdateMenuHandles(this.Menu, true);
                    }
                }
            }
        }

        private void UpdateMenuHandles(MainMenu menu, bool forceRedraw)
        {
            int num = this.formStateEx[FormStateExUpdateMenuHandlesSuspendCount];
            if ((num > 0) && (menu != null))
            {
                this.formStateEx[FormStateExUpdateMenuHandlesDeferred] = 1;
            }
            else
            {
                MainMenu menu2 = menu;
                if (menu2 != null)
                {
                    menu2.form = this;
                }
                if ((menu2 != null) || base.Properties.ContainsObject(PropCurMenu))
                {
                    base.Properties.SetObject(PropCurMenu, menu2);
                }
                if ((this.ctlClient == null) || !this.ctlClient.IsHandleCreated)
                {
                    if (menu != null)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetMenu(new HandleRef(this, base.Handle), new HandleRef(menu, menu.Handle));
                    }
                    else
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetMenu(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.NullHandleRef);
                    }
                }
                else
                {
                    MenuStrip mainMenuStrip = this.MainMenuStrip;
                    if ((mainMenuStrip == null) || (menu != null))
                    {
                        MainMenu menu3 = (MainMenu) base.Properties.GetObject(PropDummyMenu);
                        if (menu3 == null)
                        {
                            menu3 = new MainMenu {
                                ownerForm = this
                            };
                            base.Properties.SetObject(PropDummyMenu, menu3);
                        }
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.ctlClient, this.ctlClient.Handle), 560, menu3.Handle, IntPtr.Zero);
                        if (menu != null)
                        {
                            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.ctlClient, this.ctlClient.Handle), 560, menu.Handle, IntPtr.Zero);
                        }
                    }
                    if (((menu == null) && (mainMenuStrip != null)) && (System.Windows.Forms.UnsafeNativeMethods.GetMenu(new HandleRef(this, base.Handle)) != IntPtr.Zero))
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetMenu(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.NullHandleRef);
                        Form activeMdiChildInternal = this.ActiveMdiChildInternal;
                        if ((activeMdiChildInternal != null) && (activeMdiChildInternal.WindowState == FormWindowState.Maximized))
                        {
                            activeMdiChildInternal.RecreateHandle();
                        }
                        CommonProperties.xClearPreferredSizeCache(this);
                    }
                }
                if (forceRedraw)
                {
                    System.Windows.Forms.SafeNativeMethods.DrawMenuBar(new HandleRef(this, base.Handle));
                }
                this.formStateEx[FormStateExUpdateMenuHandlesDeferred] = 0;
            }
        }

        private void UpdateRenderSizeGrip()
        {
            int num = this.formState[FormStateRenderSizeGrip];
            switch (this.FormBorderStyle)
            {
                case System.Windows.Forms.FormBorderStyle.None:
                case System.Windows.Forms.FormBorderStyle.FixedSingle:
                case System.Windows.Forms.FormBorderStyle.Fixed3D:
                case System.Windows.Forms.FormBorderStyle.FixedDialog:
                case System.Windows.Forms.FormBorderStyle.FixedToolWindow:
                    this.formState[FormStateRenderSizeGrip] = 0;
                    break;

                case System.Windows.Forms.FormBorderStyle.Sizable:
                case System.Windows.Forms.FormBorderStyle.SizableToolWindow:
                    switch (this.SizeGripStyle)
                    {
                        case System.Windows.Forms.SizeGripStyle.Auto:
                            if (base.GetState(0x20))
                            {
                                this.formState[FormStateRenderSizeGrip] = 1;
                            }
                            else
                            {
                                this.formState[FormStateRenderSizeGrip] = 0;
                            }
                            goto Label_00C1;

                        case System.Windows.Forms.SizeGripStyle.Show:
                            this.formState[FormStateRenderSizeGrip] = 1;
                            goto Label_00C1;

                        case System.Windows.Forms.SizeGripStyle.Hide:
                            this.formState[FormStateRenderSizeGrip] = 0;
                            goto Label_00C1;
                    }
                    break;
            }
        Label_00C1:
            if (this.formState[FormStateRenderSizeGrip] != num)
            {
                base.Invalidate();
            }
        }

        private void UpdateToolStrip()
        {
            ToolStrip mainMenuStrip = this.MainMenuStrip;
            ArrayList list = ToolStripManager.FindMergeableToolStrips(this.ActiveMdiChildInternal);
            if (mainMenuStrip != null)
            {
                ToolStripManager.RevertMerge(mainMenuStrip);
            }
            this.UpdateMdiWindowListStrip();
            if (this.ActiveMdiChildInternal != null)
            {
                foreach (ToolStrip strip2 in list)
                {
                    System.Type type = FindClosestStockType(strip2.GetType());
                    if (mainMenuStrip != null)
                    {
                        System.Type type2 = FindClosestStockType(mainMenuStrip.GetType());
                        if (((type2 != null) && (type != null)) && ((type == type2) && mainMenuStrip.GetType().IsAssignableFrom(strip2.GetType())))
                        {
                            ToolStripManager.Merge(strip2, mainMenuStrip);
                            break;
                        }
                    }
                }
            }
            Form activeMdiChildInternal = this.ActiveMdiChildInternal;
            this.UpdateMdiControlStrip((activeMdiChildInternal != null) && activeMdiChildInternal.IsMaximized);
        }

        private void UpdateWindowIcon(bool redrawFrame)
        {
            if (base.IsHandleCreated)
            {
                System.Drawing.Icon icon;
                if ((((this.FormBorderStyle == System.Windows.Forms.FormBorderStyle.FixedDialog) && (this.formState[FormStateIconSet] == 0)) && !this.IsRestrictedWindow) || !this.ShowIcon)
                {
                    icon = null;
                }
                else
                {
                    icon = this.Icon;
                }
                if (icon != null)
                {
                    if (this.smallIcon == null)
                    {
                        try
                        {
                            this.smallIcon = new System.Drawing.Icon(icon, SystemInformation.SmallIconSize);
                        }
                        catch
                        {
                        }
                    }
                    if (this.smallIcon != null)
                    {
                        base.SendMessage(0x80, 0, this.smallIcon.Handle);
                    }
                    base.SendMessage(0x80, 1, icon.Handle);
                }
                else
                {
                    base.SendMessage(0x80, 0, 0);
                    base.SendMessage(0x80, 1, 0);
                }
                if (redrawFrame)
                {
                    System.Windows.Forms.SafeNativeMethods.RedrawWindow(new HandleRef(this, base.Handle), (System.Windows.Forms.NativeMethods.COMRECT) null, System.Windows.Forms.NativeMethods.NullHandleRef, 0x401);
                }
            }
        }

        private void UpdateWindowState()
        {
            if (base.IsHandleCreated)
            {
                FormWindowState windowState = this.WindowState;
                System.Windows.Forms.NativeMethods.WINDOWPLACEMENT placement = new System.Windows.Forms.NativeMethods.WINDOWPLACEMENT {
                    length = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.WINDOWPLACEMENT))
                };
                System.Windows.Forms.UnsafeNativeMethods.GetWindowPlacement(new HandleRef(this, base.Handle), ref placement);
                switch (placement.showCmd)
                {
                    case 1:
                    case 4:
                    case 5:
                    case 8:
                    case 9:
                        if (this.formState[FormStateWindowState] != 0)
                        {
                            this.formState[FormStateWindowState] = 0;
                        }
                        break;

                    case 2:
                    case 6:
                    case 7:
                        if (this.formState[FormStateMdiChildMax] == 0)
                        {
                            this.formState[FormStateWindowState] = 1;
                        }
                        break;

                    case 3:
                        if (this.formState[FormStateMdiChildMax] == 0)
                        {
                            this.formState[FormStateWindowState] = 2;
                        }
                        break;
                }
                if ((windowState == FormWindowState.Normal) && (this.WindowState != FormWindowState.Normal))
                {
                    if (this.WindowState == FormWindowState.Minimized)
                    {
                        this.SuspendLayoutForMinimize();
                    }
                    this.restoredWindowBounds.Size = this.ClientSize;
                    this.formStateEx[FormStateExWindowBoundsWidthIsClientSize] = 1;
                    this.formStateEx[FormStateExWindowBoundsHeightIsClientSize] = 1;
                    this.restoredWindowBoundsSpecified = BoundsSpecified.Size;
                    this.restoredWindowBounds.Location = this.Location;
                    this.restoredWindowBoundsSpecified |= BoundsSpecified.Location;
                    this.restoreBounds.Size = this.Size;
                    this.restoreBounds.Location = this.Location;
                }
                if ((windowState == FormWindowState.Minimized) && (this.WindowState != FormWindowState.Minimized))
                {
                    this.ResumeLayoutFromMinimize();
                }
                switch (this.WindowState)
                {
                    case FormWindowState.Normal:
                        base.SetState(0x10000, false);
                        break;

                    case FormWindowState.Minimized:
                    case FormWindowState.Maximized:
                        base.SetState(0x10000, true);
                        break;
                }
                if (windowState != this.WindowState)
                {
                    this.AdjustSystemMenu();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        public override bool ValidateChildren()
        {
            return base.ValidateChildren();
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public override bool ValidateChildren(ValidationConstraints validationConstraints)
        {
            return base.ValidateChildren(validationConstraints);
        }

        private void WmActivate(ref Message m)
        {
            Application.FormActivated(this.Modal, true);
            this.Active = System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam) != 0;
            Application.FormActivated(this.Modal, this.Active);
        }

        private void WmClose(ref Message m)
        {
            FormClosingEventArgs e = new FormClosingEventArgs(this.CloseReason, false);
            if (m.Msg == 0x16)
            {
                e.Cancel = m.WParam == IntPtr.Zero;
            }
            else
            {
                if (this.Modal)
                {
                    if (this.dialogResult == System.Windows.Forms.DialogResult.None)
                    {
                        this.dialogResult = System.Windows.Forms.DialogResult.Cancel;
                    }
                    this.CalledClosing = false;
                    e.Cancel = !this.CheckCloseDialog(true);
                }
                else
                {
                    e.Cancel = !base.Validate(true);
                    if (this.IsMdiContainer)
                    {
                        FormClosingEventArgs args2 = new FormClosingEventArgs(System.Windows.Forms.CloseReason.MdiFormClosing, e.Cancel);
                        foreach (Form form in this.MdiChildren)
                        {
                            if (form.IsHandleCreated)
                            {
                                form.OnClosing(args2);
                                form.OnFormClosing(args2);
                                if (args2.Cancel)
                                {
                                    e.Cancel = true;
                                    break;
                                }
                            }
                        }
                    }
                    Form[] ownedForms = this.OwnedForms;
                    for (int i = base.Properties.GetInteger(PropOwnedFormsCount) - 1; i >= 0; i--)
                    {
                        FormClosingEventArgs args3 = new FormClosingEventArgs(System.Windows.Forms.CloseReason.FormOwnerClosing, e.Cancel);
                        if (ownedForms[i] != null)
                        {
                            ownedForms[i].OnFormClosing(args3);
                            if (args3.Cancel)
                            {
                                e.Cancel = true;
                                break;
                            }
                        }
                    }
                    this.OnClosing(e);
                    this.OnFormClosing(e);
                }
                if (m.Msg == 0x11)
                {
                    m.Result = e.Cancel ? IntPtr.Zero : ((IntPtr) 1);
                }
                if (this.Modal)
                {
                    return;
                }
            }
            if ((m.Msg != 0x11) && !e.Cancel)
            {
                FormClosedEventArgs args4;
                this.IsClosing = true;
                if (this.IsMdiContainer)
                {
                    args4 = new FormClosedEventArgs(System.Windows.Forms.CloseReason.MdiFormClosing);
                    foreach (Form form2 in this.MdiChildren)
                    {
                        if (form2.IsHandleCreated)
                        {
                            form2.OnClosed(args4);
                            form2.OnFormClosed(args4);
                        }
                    }
                }
                Form[] formArray2 = this.OwnedForms;
                for (int j = base.Properties.GetInteger(PropOwnedFormsCount) - 1; j >= 0; j--)
                {
                    args4 = new FormClosedEventArgs(System.Windows.Forms.CloseReason.FormOwnerClosing);
                    if (formArray2[j] != null)
                    {
                        formArray2[j].OnClosed(args4);
                        formArray2[j].OnFormClosed(args4);
                    }
                }
                args4 = new FormClosedEventArgs(this.CloseReason);
                this.OnClosed(args4);
                this.OnFormClosed(args4);
                base.Dispose();
            }
        }

        private void WmCreate(ref Message m)
        {
            base.WndProc(ref m);
            System.Windows.Forms.NativeMethods.STARTUPINFO_I startupinfo_i = new System.Windows.Forms.NativeMethods.STARTUPINFO_I();
            System.Windows.Forms.UnsafeNativeMethods.GetStartupInfo(startupinfo_i);
            if (this.TopLevel && ((startupinfo_i.dwFlags & 1) != 0))
            {
                switch (startupinfo_i.wShowWindow)
                {
                    case 3:
                        this.WindowState = FormWindowState.Maximized;
                        break;

                    case 6:
                        this.WindowState = FormWindowState.Minimized;
                        break;
                }
            }
        }

        private void WmEnterMenuLoop(ref Message m)
        {
            this.OnMenuStart(EventArgs.Empty);
            base.WndProc(ref m);
        }

        private void WmEnterSizeMove(ref Message m)
        {
            this.formStateEx[FormStateExInModalSizingLoop] = 1;
            this.OnResizeBegin(EventArgs.Empty);
        }

        private void WmEraseBkgnd(ref Message m)
        {
            this.UpdateWindowState();
            base.WndProc(ref m);
        }

        private void WmExitMenuLoop(ref Message m)
        {
            this.OnMenuComplete(EventArgs.Empty);
            base.WndProc(ref m);
        }

        private void WmExitSizeMove(ref Message m)
        {
            this.formStateEx[FormStateExInModalSizingLoop] = 0;
            this.OnResizeEnd(EventArgs.Empty);
        }

        private void WmGetMinMaxInfo(ref Message m)
        {
            System.Drawing.Size minTrack = (this.AutoSize && (this.formStateEx[FormStateExInModalSizingLoop] == 1)) ? LayoutUtils.UnionSizes(this.minAutoSize, this.MinimumSize) : this.MinimumSize;
            System.Drawing.Size maximumSize = this.MaximumSize;
            Rectangle maximizedBounds = this.MaximizedBounds;
            if ((!minTrack.IsEmpty || !maximumSize.IsEmpty) || (!maximizedBounds.IsEmpty || this.IsRestrictedWindow))
            {
                this.WmGetMinMaxInfoHelper(ref m, minTrack, maximumSize, maximizedBounds);
            }
            if (this.IsMdiChild)
            {
                base.WndProc(ref m);
            }
        }

        private void WmGetMinMaxInfoHelper(ref Message m, System.Drawing.Size minTrack, System.Drawing.Size maxTrack, Rectangle maximizedBounds)
        {
            System.Windows.Forms.NativeMethods.MINMAXINFO lParam = (System.Windows.Forms.NativeMethods.MINMAXINFO) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.MINMAXINFO));
            if (!minTrack.IsEmpty)
            {
                lParam.ptMinTrackSize.x = minTrack.Width;
                lParam.ptMinTrackSize.y = minTrack.Height;
                if (maxTrack.IsEmpty)
                {
                    System.Drawing.Size size = SystemInformation.VirtualScreen.Size;
                    if (minTrack.Height > size.Height)
                    {
                        lParam.ptMaxTrackSize.y = 0x7fffffff;
                    }
                    if (minTrack.Width > size.Width)
                    {
                        lParam.ptMaxTrackSize.x = 0x7fffffff;
                    }
                }
            }
            if (!maxTrack.IsEmpty)
            {
                System.Drawing.Size minWindowTrackSize = SystemInformation.MinWindowTrackSize;
                lParam.ptMaxTrackSize.x = Math.Max(maxTrack.Width, minWindowTrackSize.Width);
                lParam.ptMaxTrackSize.y = Math.Max(maxTrack.Height, minWindowTrackSize.Height);
            }
            if (!maximizedBounds.IsEmpty && !this.IsRestrictedWindow)
            {
                lParam.ptMaxPosition.x = maximizedBounds.X;
                lParam.ptMaxPosition.y = maximizedBounds.Y;
                lParam.ptMaxSize.x = maximizedBounds.Width;
                lParam.ptMaxSize.y = maximizedBounds.Height;
            }
            if (this.IsRestrictedWindow)
            {
                lParam.ptMinTrackSize.x = Math.Max(lParam.ptMinTrackSize.x, 100);
                lParam.ptMinTrackSize.y = Math.Max(lParam.ptMinTrackSize.y, SystemInformation.CaptionButtonSize.Height * 3);
            }
            Marshal.StructureToPtr(lParam, m.LParam, false);
            m.Result = IntPtr.Zero;
        }

        private void WmInitMenuPopup(ref Message m)
        {
            MainMenu menu = (MainMenu) base.Properties.GetObject(PropCurMenu);
            if ((menu == null) || !menu.ProcessInitMenuPopup(m.WParam))
            {
                base.WndProc(ref m);
            }
        }

        private void WmMdiActivate(ref Message m)
        {
            base.WndProc(ref m);
            Form form = (Form) base.Properties.GetObject(PropFormMdiParent);
            if (form != null)
            {
                if (base.Handle == m.WParam)
                {
                    form.DeactivateMdiChild();
                }
                else if (base.Handle == m.LParam)
                {
                    form.ActivateMdiChildInternal(this);
                }
            }
        }

        private void WmMenuChar(ref Message m)
        {
            MainMenu menu = (MainMenu) base.Properties.GetObject(PropCurMenu);
            if (menu == null)
            {
                Form wrapper = (Form) base.Properties.GetObject(PropFormMdiParent);
                if ((wrapper != null) && (wrapper.Menu != null))
                {
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(wrapper, wrapper.Handle), 0x112, new IntPtr(0xf100), m.WParam);
                    m.Result = (IntPtr) System.Windows.Forms.NativeMethods.Util.MAKELONG(0, 1);
                    return;
                }
            }
            if (menu != null)
            {
                menu.WmMenuChar(ref m);
                if (m.Result != IntPtr.Zero)
                {
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void WmNcButtonDown(ref Message m)
        {
            if (this.IsMdiChild)
            {
                Form form = (Form) base.Properties.GetObject(PropFormMdiParent);
                if (((form.ActiveMdiChildInternal == this) && (base.ActiveControl != null)) && !base.ActiveControl.ContainsFocus)
                {
                    base.InnerMostActiveContainerControl.FocusActiveControlInternal();
                }
            }
            base.WndProc(ref m);
        }

        private void WmNCDestroy(ref Message m)
        {
            MainMenu menu = this.Menu;
            MainMenu menu2 = (MainMenu) base.Properties.GetObject(PropDummyMenu);
            MainMenu menu3 = (MainMenu) base.Properties.GetObject(PropCurMenu);
            MainMenu menu4 = (MainMenu) base.Properties.GetObject(PropMergedMenu);
            if (menu != null)
            {
                menu.ClearHandles();
            }
            if (menu3 != null)
            {
                menu3.ClearHandles();
            }
            if (menu4 != null)
            {
                menu4.ClearHandles();
            }
            if (menu2 != null)
            {
                menu2.ClearHandles();
            }
            base.WndProc(ref m);
            if (this.ownerWindow != null)
            {
                this.ownerWindow.DestroyHandle();
                this.ownerWindow = null;
            }
            if (this.Modal && (this.dialogResult == System.Windows.Forms.DialogResult.None))
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }
        }

        private void WmNCHitTest(ref Message m)
        {
            if (this.formState[FormStateRenderSizeGrip] != 0)
            {
                int x = System.Windows.Forms.NativeMethods.Util.LOWORD(m.LParam);
                int y = System.Windows.Forms.NativeMethods.Util.HIWORD(m.LParam);
                System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT(x, y);
                System.Windows.Forms.UnsafeNativeMethods.ScreenToClient(new HandleRef(this, base.Handle), pt);
                System.Drawing.Size clientSize = this.ClientSize;
                if (((pt.x >= (clientSize.Width - 0x10)) && (pt.y >= (clientSize.Height - 0x10))) && (clientSize.Height >= 0x10))
                {
                    m.Result = base.IsMirrored ? ((IntPtr) 0x10) : ((IntPtr) 0x11);
                    return;
                }
            }
            base.WndProc(ref m);
            if (this.AutoSizeMode == System.Windows.Forms.AutoSizeMode.GrowAndShrink)
            {
                int result = (int) ((long) m.Result);
                if ((result >= 10) && (result <= 0x11))
                {
                    m.Result = (IntPtr) 0x12;
                }
            }
        }

        private void WmShowWindow(ref Message m)
        {
            this.formState[FormStateSWCalled] = 1;
            base.WndProc(ref m);
        }

        private void WmSize(ref Message m)
        {
            if (this.ctlClient == null)
            {
                base.WndProc(ref m);
                if (((this.MdiControlStrip == null) && (this.MdiParentInternal != null)) && (this.MdiParentInternal.ActiveMdiChildInternal == this))
                {
                    int num = m.WParam.ToInt32();
                    this.MdiParentInternal.UpdateMdiControlStrip(num == 2);
                }
            }
        }

        private void WmSysCommand(ref Message m)
        {
            bool flag = true;
            switch ((System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam) & 0xfff0))
            {
                case 0xf060:
                    this.CloseReason = System.Windows.Forms.CloseReason.UserClosing;
                    if (this.IsMdiChild && !this.ControlBox)
                    {
                        flag = false;
                    }
                    break;

                case 0xf100:
                    if (this.IsMdiChild && !this.ControlBox)
                    {
                        flag = false;
                    }
                    break;

                case 0xf180:
                {
                    CancelEventArgs e = new CancelEventArgs(false);
                    this.OnHelpButtonClicked(e);
                    if (e.Cancel)
                    {
                        flag = false;
                    }
                    break;
                }
                case 0xf000:
                case 0xf010:
                    this.formStateEx[FormStateExInModalSizingLoop] = 1;
                    break;
            }
            if (Command.DispatchID(System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam)))
            {
                flag = false;
            }
            if (flag)
            {
                base.WndProc(ref m);
            }
        }

        private void WmUnInitMenuPopup(ref Message m)
        {
            if (this.Menu != null)
            {
                this.Menu.OnCollapse(EventArgs.Empty);
            }
        }

        private void WmWindowPosChanged(ref Message m)
        {
            this.UpdateWindowState();
            base.WndProc(ref m);
            this.RestoreWindowBoundsIfNecessary();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x10:
                    if (this.CloseReason == System.Windows.Forms.CloseReason.None)
                    {
                        this.CloseReason = System.Windows.Forms.CloseReason.TaskManagerClosing;
                    }
                    this.WmClose(ref m);
                    return;

                case 0x11:
                case 0x16:
                    this.CloseReason = System.Windows.Forms.CloseReason.WindowsShutDown;
                    this.WmClose(ref m);
                    return;

                case 20:
                    this.WmEraseBkgnd(ref m);
                    return;

                case 0x18:
                    this.WmShowWindow(ref m);
                    return;

                case 0x24:
                    this.WmGetMinMaxInfo(ref m);
                    return;

                case 5:
                    this.WmSize(ref m);
                    return;

                case 6:
                    this.WmActivate(ref m);
                    return;

                case 1:
                    this.WmCreate(ref m);
                    return;

                case 130:
                    this.WmNCDestroy(ref m);
                    return;

                case 0x84:
                    this.WmNCHitTest(ref m);
                    return;

                case 0x86:
                    if (this.IsRestrictedWindow)
                    {
                        base.BeginInvoke(new MethodInvoker(this.RestrictedProcessNcActivate));
                    }
                    base.WndProc(ref m);
                    return;

                case 0x47:
                    this.WmWindowPosChanged(ref m);
                    return;

                case 0xa1:
                case 0xa4:
                case 0xa7:
                case 0xab:
                    this.WmNcButtonDown(ref m);
                    return;

                case 0x112:
                    this.WmSysCommand(ref m);
                    return;

                case 0x117:
                    this.WmInitMenuPopup(ref m);
                    return;

                case 0x120:
                    this.WmMenuChar(ref m);
                    return;

                case 0x125:
                    this.WmUnInitMenuPopup(ref m);
                    return;

                case 0x211:
                    this.WmEnterMenuLoop(ref m);
                    return;

                case 530:
                    this.WmExitMenuLoop(ref m);
                    return;

                case 0x215:
                    base.WndProc(ref m);
                    if (base.CaptureInternal && (Control.MouseButtons == MouseButtons.None))
                    {
                        base.CaptureInternal = false;
                    }
                    return;

                case 0x222:
                    this.WmMdiActivate(ref m);
                    return;

                case 0x231:
                    this.WmEnterSizeMove(ref m);
                    this.DefWndProc(ref m);
                    return;

                case 0x232:
                    this.WmExitSizeMove(ref m);
                    this.DefWndProc(ref m);
                    return;
            }
            base.WndProc(ref m);
        }

        [DefaultValue((string) null), System.Windows.Forms.SRDescription("FormAcceptButtonDescr")]
        public IButtonControl AcceptButton
        {
            get
            {
                return (IButtonControl) base.Properties.GetObject(PropAcceptButton);
            }
            set
            {
                if (this.AcceptButton != value)
                {
                    base.Properties.SetObject(PropAcceptButton, value);
                    this.UpdateDefaultButton();
                }
            }
        }

        internal bool Active
        {
            get
            {
                Form parentFormInternal = base.ParentFormInternal;
                if (parentFormInternal == null)
                {
                    return (this.formState[FormStateIsActive] != 0);
                }
                return ((parentFormInternal.ActiveControl == this) && parentFormInternal.Active);
            }
            set
            {
                if (((this.formState[FormStateIsActive] != 0) != value) && (!value || this.CanRecreateHandle()))
                {
                    this.formState[FormStateIsActive] = value ? 1 : 0;
                    if (value)
                    {
                        this.formState[FormStateIsWindowActivated] = 1;
                        if (this.IsRestrictedWindow)
                        {
                            this.WindowText = this.userWindowText;
                        }
                        if (!base.ValidationCancelled)
                        {
                            if (base.ActiveControl == null)
                            {
                                base.SelectNextControlInternal(null, true, true, true, false);
                            }
                            base.InnerMostActiveContainerControl.FocusActiveControlInternal();
                        }
                        this.OnActivated(EventArgs.Empty);
                    }
                    else
                    {
                        this.formState[FormStateIsWindowActivated] = 0;
                        if (this.IsRestrictedWindow)
                        {
                            this.Text = this.userWindowText;
                        }
                        this.OnDeactivate(EventArgs.Empty);
                    }
                }
            }
        }

        public static Form ActiveForm
        {
            get
            {
                System.Windows.Forms.IntSecurity.GetParent.Demand();
                Control control = Control.FromHandleInternal(System.Windows.Forms.UnsafeNativeMethods.GetForegroundWindow());
                if ((control != null) && (control is Form))
                {
                    return (Form) control;
                }
                return null;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("FormActiveMDIChildDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Form ActiveMdiChild
        {
            get
            {
                Form activeMdiChildInternal = this.ActiveMdiChildInternal;
                if (((activeMdiChildInternal == null) && (this.ctlClient != null)) && this.ctlClient.IsHandleCreated)
                {
                    activeMdiChildInternal = Control.FromHandleInternal(this.ctlClient.SendMessage(0x229, 0, 0)) as Form;
                }
                if (((activeMdiChildInternal != null) && activeMdiChildInternal.Visible) && activeMdiChildInternal.Enabled)
                {
                    return activeMdiChildInternal;
                }
                return null;
            }
        }

        internal Form ActiveMdiChildInternal
        {
            get
            {
                return (Form) base.Properties.GetObject(PropActiveMdiChild);
            }
            set
            {
                base.Properties.SetObject(PropActiveMdiChild, value);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlAllowTransparencyDescr")]
        public bool AllowTransparency
        {
            get
            {
                return (this.formState[FormStateAllowTransparency] != 0);
            }
            set
            {
                if ((value != (this.formState[FormStateAllowTransparency] != 0)) && OSFeature.Feature.IsPresent(OSFeature.LayeredWindows))
                {
                    this.formState[FormStateAllowTransparency] = value ? 1 : 0;
                    this.formState[FormStateLayered] = this.formState[FormStateAllowTransparency];
                    base.UpdateStyles();
                    if (!value)
                    {
                        if (base.Properties.ContainsObject(PropOpacity))
                        {
                            base.Properties.SetObject(PropOpacity, 1f);
                        }
                        if (base.Properties.ContainsObject(PropTransparencyKey))
                        {
                            base.Properties.SetObject(PropTransparencyKey, Color.Empty);
                        }
                        this.UpdateLayered();
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), Obsolete("This property has been deprecated. Use the AutoScaleMode property instead.  http://go.microsoft.com/fwlink/?linkid=14202"), System.Windows.Forms.SRDescription("FormAutoScaleDescr"), EditorBrowsable(EditorBrowsableState.Never), System.Windows.Forms.SRCategory("CatLayout")]
        public bool AutoScale
        {
            get
            {
                return (this.formState[FormStateAutoScaling] != 0);
            }
            set
            {
                this.formStateEx[FormStateExSettingAutoScale] = 1;
                try
                {
                    if (value)
                    {
                        this.formState[FormStateAutoScaling] = 1;
                        base.AutoScaleMode = AutoScaleMode.None;
                    }
                    else
                    {
                        this.formState[FormStateAutoScaling] = 0;
                    }
                }
                finally
                {
                    this.formStateEx[FormStateExSettingAutoScale] = 0;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Localizable(true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public virtual System.Drawing.Size AutoScaleBaseSize
        {
            get
            {
                if (this.autoScaleBaseSize.IsEmpty)
                {
                    SizeF autoScaleSize = GetAutoScaleSize(this.Font);
                    return new System.Drawing.Size((int) Math.Round((double) autoScaleSize.Width), (int) Math.Round((double) autoScaleSize.Height));
                }
                return this.autoScaleBaseSize;
            }
            set
            {
                this.autoScaleBaseSize = value;
            }
        }

        [Localizable(true)]
        public override bool AutoScroll
        {
            get
            {
                return base.AutoScroll;
            }
            set
            {
                if (value)
                {
                    this.IsMdiContainer = false;
                }
                base.AutoScroll = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public override bool AutoSize
        {
            get
            {
                return (this.formStateEx[FormStateExAutoSize] != 0);
            }
            set
            {
                if (value != this.AutoSize)
                {
                    this.formStateEx[FormStateExAutoSize] = value ? 1 : 0;
                    if (!this.AutoSize)
                    {
                        this.minAutoSize = System.Drawing.Size.Empty;
                        this.Size = CommonProperties.GetSpecifiedBounds(this).Size;
                    }
                    LayoutTransaction.DoLayout(this, this, PropertyNames.AutoSize);
                    this.OnAutoSizeChanged(EventArgs.Empty);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), Localizable(true), Browsable(true), DefaultValue(1), System.Windows.Forms.SRDescription("ControlAutoSizeModeDescr")]
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
                    Control elementToLayout = (base.DesignMode || (this.ParentInternal == null)) ? this : this.ParentInternal;
                    if (elementToLayout != null)
                    {
                        if (elementToLayout.LayoutEngine == DefaultLayout.Instance)
                        {
                            elementToLayout.LayoutEngine.InitLayout(this, BoundsSpecified.Size);
                        }
                        LayoutTransaction.DoLayout(elementToLayout, this, PropertyNames.AutoSize);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        public override System.Windows.Forms.AutoValidate AutoValidate
        {
            get
            {
                return base.AutoValidate;
            }
            set
            {
                base.AutoValidate = value;
            }
        }

        public override Color BackColor
        {
            get
            {
                Color rawBackColor = base.RawBackColor;
                if (!rawBackColor.IsEmpty)
                {
                    return rawBackColor;
                }
                return Control.DefaultBackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        private bool CalledClosing
        {
            get
            {
                return (this.formStateEx[FormStateExCalledClosing] != 0);
            }
            set
            {
                this.formStateEx[FormStateExCalledClosing] = value ? 1 : 0;
            }
        }

        private bool CalledCreateControl
        {
            get
            {
                return (this.formStateEx[FormStateExCalledCreateControl] != 0);
            }
            set
            {
                this.formStateEx[FormStateExCalledCreateControl] = value ? 1 : 0;
            }
        }

        private bool CalledMakeVisible
        {
            get
            {
                return (this.formStateEx[FormStateExCalledMakeVisible] != 0);
            }
            set
            {
                this.formStateEx[FormStateExCalledMakeVisible] = value ? 1 : 0;
            }
        }

        private bool CalledOnLoad
        {
            get
            {
                return (this.formStateEx[FormStateExCalledOnLoad] != 0);
            }
            set
            {
                this.formStateEx[FormStateExCalledOnLoad] = value ? 1 : 0;
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRDescription("FormCancelButtonDescr")]
        public IButtonControl CancelButton
        {
            get
            {
                return (IButtonControl) base.Properties.GetObject(PropCancelButton);
            }
            set
            {
                base.Properties.SetObject(PropCancelButton, value);
                if ((value != null) && (value.DialogResult == System.Windows.Forms.DialogResult.None))
                {
                    value.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), Localizable(true)]
        public System.Drawing.Size ClientSize
        {
            get
            {
                return base.ClientSize;
            }
            set
            {
                base.ClientSize = value;
            }
        }

        internal System.Windows.Forms.CloseReason CloseReason
        {
            get
            {
                return this.closeReason;
            }
            set
            {
                this.closeReason = value;
            }
        }

        [System.Windows.Forms.SRDescription("FormControlBoxDescr"), System.Windows.Forms.SRCategory("CatWindowStyle"), DefaultValue(true)]
        public bool ControlBox
        {
            get
            {
                return (this.formState[FormStateControlBox] != 0);
            }
            set
            {
                if (!this.IsRestrictedWindow)
                {
                    if (value)
                    {
                        this.formState[FormStateControlBox] = 1;
                    }
                    else
                    {
                        this.formState[FormStateControlBox] = 0;
                    }
                    this.UpdateFormStyles();
                }
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                if (base.IsHandleCreated && ((base.WindowStyle & 0x8000000) != 0))
                {
                    createParams.Style |= 0x8000000;
                }
                else if (this.TopLevel)
                {
                    createParams.Style &= -134217729;
                }
                if (this.TopLevel && (this.formState[FormStateLayered] != 0))
                {
                    createParams.ExStyle |= 0x80000;
                }
                IWin32Window window = (IWin32Window) base.Properties.GetObject(PropDialogOwner);
                if (window != null)
                {
                    createParams.Parent = Control.GetSafeHandle(window);
                }
                this.FillInCreateParamsBorderStyles(createParams);
                this.FillInCreateParamsWindowState(createParams);
                this.FillInCreateParamsBorderIcons(createParams);
                if (this.formState[FormStateTaskBar] != 0)
                {
                    createParams.ExStyle |= 0x40000;
                }
                System.Windows.Forms.FormBorderStyle formBorderStyle = this.FormBorderStyle;
                if (!this.ShowIcon && (((formBorderStyle == System.Windows.Forms.FormBorderStyle.Sizable) || (formBorderStyle == System.Windows.Forms.FormBorderStyle.Fixed3D)) || (formBorderStyle == System.Windows.Forms.FormBorderStyle.FixedSingle)))
                {
                    createParams.ExStyle |= 1;
                }
                if (this.IsMdiChild)
                {
                    if (base.Visible && ((this.WindowState == FormWindowState.Maximized) || (this.WindowState == FormWindowState.Normal)))
                    {
                        Form form = (Form) base.Properties.GetObject(PropFormMdiParent);
                        Form activeMdiChildInternal = form.ActiveMdiChildInternal;
                        if ((activeMdiChildInternal != null) && (activeMdiChildInternal.WindowState == FormWindowState.Maximized))
                        {
                            createParams.Style |= 0x1000000;
                            this.formState[FormStateWindowState] = 2;
                            base.SetState(0x10000, true);
                        }
                    }
                    if (this.formState[FormStateMdiChildMax] != 0)
                    {
                        createParams.Style |= 0x1000000;
                    }
                    createParams.ExStyle |= 0x40;
                }
                if (this.TopLevel || this.IsMdiChild)
                {
                    this.FillInCreateParamsStartPosition(createParams);
                    if ((createParams.Style & 0x10000000) != 0)
                    {
                        this.formState[FormStateShowWindowOnCreate] = 1;
                        createParams.Style &= -268435457;
                    }
                    else
                    {
                        this.formState[FormStateShowWindowOnCreate] = 0;
                    }
                }
                if (this.IsRestrictedWindow)
                {
                    createParams.Caption = this.RestrictedWindowText(createParams.Caption);
                }
                if ((this.RightToLeft == RightToLeft.Yes) && this.RightToLeftLayout)
                {
                    createParams.ExStyle |= 0x500000;
                    createParams.ExStyle &= -28673;
                }
                return createParams;
            }
        }

        internal static System.Drawing.Icon DefaultIcon
        {
            get
            {
                if (defaultIcon == null)
                {
                    lock (internalSyncObject)
                    {
                        if (defaultIcon == null)
                        {
                            defaultIcon = new System.Drawing.Icon(typeof(Form), "wfc.ico");
                        }
                    }
                }
                return defaultIcon;
            }
        }

        protected override ImeMode DefaultImeMode
        {
            get
            {
                return ImeMode.NoControl;
            }
        }

        private static System.Drawing.Icon DefaultRestrictedIcon
        {
            get
            {
                if (defaultRestrictedIcon == null)
                {
                    lock (internalSyncObject)
                    {
                        if (defaultRestrictedIcon == null)
                        {
                            defaultRestrictedIcon = new System.Drawing.Icon(typeof(Form), "wfsecurity.ico");
                        }
                    }
                }
                return defaultRestrictedIcon;
            }
        }

        protected override System.Drawing.Size DefaultSize
        {
            get
            {
                return new System.Drawing.Size(300, 300);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("FormDesktopBoundsDescr")]
        public Rectangle DesktopBounds
        {
            get
            {
                Rectangle workingArea = SystemInformation.WorkingArea;
                Rectangle bounds = base.Bounds;
                bounds.X -= workingArea.X;
                bounds.Y -= workingArea.Y;
                return bounds;
            }
            set
            {
                this.SetDesktopBounds(value.X, value.Y, value.Width, value.Height);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRDescription("FormDesktopLocationDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public Point DesktopLocation
        {
            get
            {
                Rectangle workingArea = SystemInformation.WorkingArea;
                Point location = this.Location;
                location.X -= workingArea.X;
                location.Y -= workingArea.Y;
                return location;
            }
            set
            {
                this.SetDesktopLocation(value.X, value.Y);
            }
        }

        [System.Windows.Forms.SRDescription("FormDialogResultDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Windows.Forms.DialogResult DialogResult
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

        [System.Windows.Forms.SRDescription("FormBorderStyleDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(4), DispId(-504)]
        public System.Windows.Forms.FormBorderStyle FormBorderStyle
        {
            get
            {
                return (System.Windows.Forms.FormBorderStyle) this.formState[FormStateBorderStyle];
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 6))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.FormBorderStyle));
                }
                if (this.IsRestrictedWindow)
                {
                    switch (value)
                    {
                        case System.Windows.Forms.FormBorderStyle.None:
                            value = System.Windows.Forms.FormBorderStyle.FixedSingle;
                            goto Label_0068;

                        case System.Windows.Forms.FormBorderStyle.FixedSingle:
                        case System.Windows.Forms.FormBorderStyle.Fixed3D:
                        case System.Windows.Forms.FormBorderStyle.FixedDialog:
                        case System.Windows.Forms.FormBorderStyle.Sizable:
                            goto Label_0068;

                        case System.Windows.Forms.FormBorderStyle.FixedToolWindow:
                            value = System.Windows.Forms.FormBorderStyle.FixedSingle;
                            goto Label_0068;

                        case System.Windows.Forms.FormBorderStyle.SizableToolWindow:
                            value = System.Windows.Forms.FormBorderStyle.Sizable;
                            goto Label_0068;
                    }
                    value = System.Windows.Forms.FormBorderStyle.Sizable;
                }
            Label_0068:
                this.formState[FormStateBorderStyle] = (int) value;
                if ((this.formState[FormStateSetClientSize] == 1) && !base.IsHandleCreated)
                {
                    this.ClientSize = this.ClientSize;
                }
                Rectangle restoredWindowBounds = this.restoredWindowBounds;
                BoundsSpecified restoredWindowBoundsSpecified = this.restoredWindowBoundsSpecified;
                int num = this.formStateEx[FormStateExWindowBoundsWidthIsClientSize];
                int num2 = this.formStateEx[FormStateExWindowBoundsHeightIsClientSize];
                this.UpdateFormStyles();
                if ((this.formState[FormStateIconSet] == 0) && !this.IsRestrictedWindow)
                {
                    this.UpdateWindowIcon(false);
                }
                if (this.WindowState != FormWindowState.Normal)
                {
                    this.restoredWindowBounds = restoredWindowBounds;
                    this.restoredWindowBoundsSpecified = restoredWindowBoundsSpecified;
                    this.formStateEx[FormStateExWindowBoundsWidthIsClientSize] = num;
                    this.formStateEx[FormStateExWindowBoundsHeightIsClientSize] = num2;
                }
            }
        }

        private Form FormerlyActiveMdiChild
        {
            get
            {
                return (Form) base.Properties.GetObject(PropFormerlyActiveMdiChild);
            }
            set
            {
                base.Properties.SetObject(PropFormerlyActiveMdiChild, value);
            }
        }

        internal override bool HasMenu
        {
            get
            {
                bool flag = false;
                System.Windows.Forms.Menu menu = this.Menu;
                if ((this.TopLevel && (menu != null)) && (menu.ItemCount > 0))
                {
                    flag = true;
                }
                return flag;
            }
        }

        [System.Windows.Forms.SRDescription("FormHelpButtonDescr"), System.Windows.Forms.SRCategory("CatWindowStyle"), DefaultValue(false)]
        public bool HelpButton
        {
            get
            {
                return (this.formState[FormStateHelpButton] != 0);
            }
            set
            {
                if (value)
                {
                    this.formState[FormStateHelpButton] = 1;
                }
                else
                {
                    this.formState[FormStateHelpButton] = 0;
                }
                this.UpdateFormStyles();
            }
        }

        [System.Windows.Forms.SRCategory("CatWindowStyle"), System.Windows.Forms.SRDescription("FormIconDescr"), AmbientValue((string) null), Localizable(true)]
        public System.Drawing.Icon Icon
        {
            get
            {
                if (this.formState[FormStateIconSet] != 0)
                {
                    return this.icon;
                }
                if (this.IsRestrictedWindow)
                {
                    return DefaultRestrictedIcon;
                }
                return DefaultIcon;
            }
            set
            {
                if ((this.icon != value) && !this.IsRestrictedWindow)
                {
                    if (value == defaultIcon)
                    {
                        value = null;
                    }
                    this.formState[FormStateIconSet] = (value == null) ? 0 : 1;
                    this.icon = value;
                    if (this.smallIcon != null)
                    {
                        this.smallIcon.Dispose();
                        this.smallIcon = null;
                    }
                    this.UpdateWindowIcon(true);
                }
            }
        }

        private bool IsClosing
        {
            get
            {
                return (this.formStateEx[FormStateExWindowClosing] == 1);
            }
            set
            {
                this.formStateEx[FormStateExWindowClosing] = value ? 1 : 0;
            }
        }

        private bool IsMaximized
        {
            get
            {
                return ((this.WindowState == FormWindowState.Maximized) || (this.IsMdiChild && (this.formState[FormStateMdiChildMax] == 1)));
            }
        }

        [System.Windows.Forms.SRCategory("CatWindowStyle"), System.Windows.Forms.SRDescription("FormIsMDIChildDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsMdiChild
        {
            get
            {
                return (base.Properties.GetObject(PropFormMdiParent) != null);
            }
        }

        internal bool IsMdiChildFocusable
        {
            get
            {
                return (base.Properties.ContainsObject(PropMdiChildFocusable) && ((bool) base.Properties.GetObject(PropMdiChildFocusable)));
            }
            set
            {
                if (value != this.IsMdiChildFocusable)
                {
                    base.Properties.SetObject(PropMdiChildFocusable, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatWindowStyle"), System.Windows.Forms.SRDescription("FormIsMDIContainerDescr"), DefaultValue(false)]
        public bool IsMdiContainer
        {
            get
            {
                return (this.ctlClient != null);
            }
            set
            {
                if (value != this.IsMdiContainer)
                {
                    if (value)
                    {
                        this.AllowTransparency = false;
                        base.Controls.Add(new System.Windows.Forms.MdiClient());
                    }
                    else
                    {
                        this.ActiveMdiChildInternal = null;
                        this.ctlClient.Dispose();
                    }
                    base.Invalidate();
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool IsRestrictedWindow
        {
            get
            {
                if (this.formState[FormStateIsRestrictedWindowChecked] == 0)
                {
                    this.formState[FormStateIsRestrictedWindow] = 0;
                    try
                    {
                        System.Windows.Forms.IntSecurity.WindowAdornmentModification.Demand();
                    }
                    catch (SecurityException)
                    {
                        this.formState[FormStateIsRestrictedWindow] = 1;
                    }
                    catch
                    {
                        this.formState[FormStateIsRestrictedWindow] = 1;
                        this.formState[FormStateIsRestrictedWindowChecked] = 1;
                        throw;
                    }
                    this.formState[FormStateIsRestrictedWindowChecked] = 1;
                }
                return (this.formState[FormStateIsRestrictedWindow] != 0);
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("FormKeyPreviewDescr")]
        public bool KeyPreview
        {
            get
            {
                return (this.formState[FormStateKeyPreview] != 0);
            }
            set
            {
                if (value)
                {
                    this.formState[FormStateKeyPreview] = 1;
                }
                else
                {
                    this.formState[FormStateKeyPreview] = 0;
                }
            }
        }

        [SettingsBindable(true)]
        public Point Location
        {
            get
            {
                return base.Location;
            }
            set
            {
                base.Location = value;
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRCategory("CatWindowStyle"), System.Windows.Forms.SRDescription("FormMenuStripDescr"), TypeConverter(typeof(ReferenceConverter))]
        public MenuStrip MainMenuStrip
        {
            get
            {
                return (MenuStrip) base.Properties.GetObject(PropMainMenuStrip);
            }
            set
            {
                base.Properties.SetObject(PropMainMenuStrip, value);
                if (base.IsHandleCreated && (this.Menu == null))
                {
                    this.UpdateMenuHandles();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public Padding Margin
        {
            get
            {
                return base.Margin;
            }
            set
            {
                base.Margin = value;
            }
        }

        [System.Windows.Forms.SRDescription("FormMaximizeBoxDescr"), System.Windows.Forms.SRCategory("CatWindowStyle"), DefaultValue(true)]
        public bool MaximizeBox
        {
            get
            {
                return (this.formState[FormStateMaximizeBox] != 0);
            }
            set
            {
                if (value)
                {
                    this.formState[FormStateMaximizeBox] = 1;
                }
                else
                {
                    this.formState[FormStateMaximizeBox] = 0;
                }
                this.UpdateFormStyles();
            }
        }

        protected Rectangle MaximizedBounds
        {
            get
            {
                return base.Properties.GetRectangle(PropMaximizedBounds);
            }
            set
            {
                if (!value.Equals(this.MaximizedBounds))
                {
                    base.Properties.SetRectangle(PropMaximizedBounds, value);
                    this.OnMaximizedBoundsChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(typeof(System.Drawing.Size), "0, 0"), Localizable(true), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("FormMaximumSizeDescr")]
        public override System.Drawing.Size MaximumSize
        {
            get
            {
                if (base.Properties.ContainsInteger(PropMaxTrackSizeWidth))
                {
                    return new System.Drawing.Size(base.Properties.GetInteger(PropMaxTrackSizeWidth), base.Properties.GetInteger(PropMaxTrackSizeHeight));
                }
                return System.Drawing.Size.Empty;
            }
            set
            {
                if (!value.Equals(this.MaximumSize))
                {
                    if ((value.Width < 0) || (value.Height < 0))
                    {
                        throw new ArgumentOutOfRangeException("MaximumSize");
                    }
                    base.Properties.SetInteger(PropMaxTrackSizeWidth, value.Width);
                    base.Properties.SetInteger(PropMaxTrackSizeHeight, value.Height);
                    if (!this.MinimumSize.IsEmpty && !value.IsEmpty)
                    {
                        if (base.Properties.GetInteger(PropMinTrackSizeWidth) > value.Width)
                        {
                            base.Properties.SetInteger(PropMinTrackSizeWidth, value.Width);
                        }
                        if (base.Properties.GetInteger(PropMinTrackSizeHeight) > value.Height)
                        {
                            base.Properties.SetInteger(PropMinTrackSizeHeight, value.Height);
                        }
                    }
                    System.Drawing.Size size = this.Size;
                    if (!value.IsEmpty && ((size.Width > value.Width) || (size.Height > value.Height)))
                    {
                        int width = Math.Min(size.Width, value.Width);
                        this.Size = new System.Drawing.Size(width, Math.Min(size.Height, value.Height));
                    }
                    this.OnMaximumSizeChanged(EventArgs.Empty);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("FormMDIChildrenDescr"), System.Windows.Forms.SRCategory("CatWindowStyle"), Browsable(false)]
        public Form[] MdiChildren
        {
            get
            {
                if (this.ctlClient != null)
                {
                    return this.ctlClient.MdiChildren;
                }
                return new Form[0];
            }
        }

        internal System.Windows.Forms.MdiClient MdiClient
        {
            get
            {
                return this.ctlClient;
            }
        }

        private System.Windows.Forms.MdiControlStrip MdiControlStrip
        {
            get
            {
                return (base.Properties.GetObject(PropMdiControlStrip) as System.Windows.Forms.MdiControlStrip);
            }
            set
            {
                base.Properties.SetObject(PropMdiControlStrip, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatWindowStyle"), Browsable(false), System.Windows.Forms.SRDescription("FormMDIParentDescr")]
        public Form MdiParent
        {
            get
            {
                System.Windows.Forms.IntSecurity.GetParent.Demand();
                return this.MdiParentInternal;
            }
            set
            {
                this.MdiParentInternal = value;
            }
        }

        private Form MdiParentInternal
        {
            get
            {
                return (Form) base.Properties.GetObject(PropFormMdiParent);
            }
            set
            {
                Form form = (Form) base.Properties.GetObject(PropFormMdiParent);
                if ((value != form) || ((value == null) && (this.ParentInternal != null)))
                {
                    if ((value != null) && (base.CreateThreadId != value.CreateThreadId))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("AddDifferentThreads"), "value");
                    }
                    bool state = base.GetState(2);
                    base.Visible = false;
                    try
                    {
                        if (value == null)
                        {
                            this.ParentInternal = null;
                            base.SetTopLevel(true);
                        }
                        else
                        {
                            if (this.IsMdiContainer)
                            {
                                throw new ArgumentException(System.Windows.Forms.SR.GetString("FormMDIParentAndChild"), "value");
                            }
                            if (!value.IsMdiContainer)
                            {
                                throw new ArgumentException(System.Windows.Forms.SR.GetString("MDIParentNotContainer"), "value");
                            }
                            this.Dock = DockStyle.None;
                            base.Properties.SetObject(PropFormMdiParent, value);
                            base.SetState(0x80000, false);
                            this.ParentInternal = value.MdiClient;
                            if ((this.ParentInternal.IsHandleCreated && this.IsMdiChild) && base.IsHandleCreated)
                            {
                                this.DestroyHandle();
                            }
                        }
                        this.InvalidateMergedMenu();
                        this.UpdateMenuHandles();
                    }
                    finally
                    {
                        base.UpdateStyles();
                        base.Visible = state;
                    }
                }
            }
        }

        private System.Windows.Forms.MdiWindowListStrip MdiWindowListStrip
        {
            get
            {
                return (base.Properties.GetObject(PropMdiWindowListStrip) as System.Windows.Forms.MdiWindowListStrip);
            }
            set
            {
                base.Properties.SetObject(PropMdiWindowListStrip, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatWindowStyle"), Browsable(false), DefaultValue((string) null), System.Windows.Forms.SRDescription("FormMenuDescr"), TypeConverter(typeof(ReferenceConverter))]
        public MainMenu Menu
        {
            get
            {
                return (MainMenu) base.Properties.GetObject(PropMainMenu);
            }
            set
            {
                MainMenu menu = this.Menu;
                if (menu != value)
                {
                    if (menu != null)
                    {
                        menu.form = null;
                    }
                    base.Properties.SetObject(PropMainMenu, value);
                    if (value != null)
                    {
                        if (value.form != null)
                        {
                            value.form.Menu = null;
                        }
                        value.form = this;
                    }
                    if ((this.formState[FormStateSetClientSize] == 1) && !base.IsHandleCreated)
                    {
                        this.ClientSize = this.ClientSize;
                    }
                    this.MenuChanged(0, value);
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatWindowStyle"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced), System.Windows.Forms.SRDescription("FormMergedMenuDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MainMenu MergedMenu
        {
            [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
            get
            {
                return this.MergedMenuPrivate;
            }
        }

        private MainMenu MergedMenuPrivate
        {
            get
            {
                Form form = (Form) base.Properties.GetObject(PropFormMdiParent);
                if (form == null)
                {
                    return null;
                }
                MainMenu menu = (MainMenu) base.Properties.GetObject(PropMergedMenu);
                if (menu == null)
                {
                    MainMenu menuSrc = form.Menu;
                    MainMenu menu3 = this.Menu;
                    if (menu3 == null)
                    {
                        return menuSrc;
                    }
                    if (menuSrc == null)
                    {
                        return menu3;
                    }
                    menu = new MainMenu {
                        ownerForm = this
                    };
                    menu.MergeMenu(menuSrc);
                    menu.MergeMenu(menu3);
                    base.Properties.SetObject(PropMergedMenu, menu);
                }
                return menu;
            }
        }

        [System.Windows.Forms.SRDescription("FormMinimizeBoxDescr"), System.Windows.Forms.SRCategory("CatWindowStyle"), DefaultValue(true)]
        public bool MinimizeBox
        {
            get
            {
                return (this.formState[FormStateMinimizeBox] != 0);
            }
            set
            {
                if (value)
                {
                    this.formState[FormStateMinimizeBox] = 1;
                }
                else
                {
                    this.formState[FormStateMinimizeBox] = 0;
                }
                this.UpdateFormStyles();
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("FormMinimumSizeDescr"), RefreshProperties(RefreshProperties.Repaint), Localizable(true)]
        public override System.Drawing.Size MinimumSize
        {
            get
            {
                if (base.Properties.ContainsInteger(PropMinTrackSizeWidth))
                {
                    return new System.Drawing.Size(base.Properties.GetInteger(PropMinTrackSizeWidth), base.Properties.GetInteger(PropMinTrackSizeHeight));
                }
                return this.DefaultMinimumSize;
            }
            set
            {
                if (!value.Equals(this.MinimumSize))
                {
                    if ((value.Width < 0) || (value.Height < 0))
                    {
                        throw new ArgumentOutOfRangeException("MinimumSize");
                    }
                    Rectangle bounds = base.Bounds;
                    bounds.Size = value;
                    value = WindowsFormsUtils.ConstrainToScreenWorkingAreaBounds(bounds).Size;
                    base.Properties.SetInteger(PropMinTrackSizeWidth, value.Width);
                    base.Properties.SetInteger(PropMinTrackSizeHeight, value.Height);
                    if (!this.MaximumSize.IsEmpty && !value.IsEmpty)
                    {
                        if (base.Properties.GetInteger(PropMaxTrackSizeWidth) < value.Width)
                        {
                            base.Properties.SetInteger(PropMaxTrackSizeWidth, value.Width);
                        }
                        if (base.Properties.GetInteger(PropMaxTrackSizeHeight) < value.Height)
                        {
                            base.Properties.SetInteger(PropMaxTrackSizeHeight, value.Height);
                        }
                    }
                    System.Drawing.Size size = this.Size;
                    if ((size.Width < value.Width) || (size.Height < value.Height))
                    {
                        int width = Math.Max(size.Width, value.Width);
                        this.Size = new System.Drawing.Size(width, Math.Max(size.Height, value.Height));
                    }
                    if (base.IsHandleCreated)
                    {
                        System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.NullHandleRef, this.Location.X, this.Location.Y, this.Size.Width, this.Size.Height, 4);
                    }
                    this.OnMinimumSizeChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(false), System.Windows.Forms.SRCategory("CatWindowStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("FormModalDescr")]
        public bool Modal
        {
            get
            {
                return base.GetState(0x20);
            }
        }

        [System.Windows.Forms.SRDescription("FormOpacityDescr"), DefaultValue((double) 1.0), System.Windows.Forms.SRCategory("CatWindowStyle"), TypeConverter(typeof(OpacityConverter))]
        public double Opacity
        {
            get
            {
                object obj2 = base.Properties.GetObject(PropOpacity);
                if (obj2 != null)
                {
                    return Convert.ToDouble(obj2, CultureInfo.InvariantCulture);
                }
                return 1.0;
            }
            set
            {
                if (this.IsRestrictedWindow)
                {
                    value = Math.Max(value, 0.5);
                }
                if (value > 1.0)
                {
                    value = 1.0;
                }
                else if (value < 0.0)
                {
                    value = 0.0;
                }
                base.Properties.SetObject(PropOpacity, value);
                bool flag = this.formState[FormStateLayered] != 0;
                if ((this.OpacityAsByte < 0xff) && OSFeature.Feature.IsPresent(OSFeature.LayeredWindows))
                {
                    this.AllowTransparency = true;
                    if (this.formState[FormStateLayered] != 1)
                    {
                        this.formState[FormStateLayered] = 1;
                        if (!flag)
                        {
                            base.UpdateStyles();
                        }
                    }
                }
                else
                {
                    this.formState[FormStateLayered] = (this.TransparencyKey != Color.Empty) ? 1 : 0;
                    if (flag != (this.formState[FormStateLayered] != 0))
                    {
                        int windowLong = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, base.Handle), -20));
                        System.Windows.Forms.CreateParams createParams = this.CreateParams;
                        if (windowLong != createParams.ExStyle)
                        {
                            System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, base.Handle), -20, new HandleRef(null, (IntPtr) createParams.ExStyle));
                        }
                    }
                }
                this.UpdateLayered();
            }
        }

        private byte OpacityAsByte
        {
            get
            {
                return (byte) (this.Opacity * 255.0);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRDescription("FormOwnedFormsDescr"), System.Windows.Forms.SRCategory("CatWindowStyle")]
        public Form[] OwnedForms
        {
            get
            {
                Form[] sourceArray = (Form[]) base.Properties.GetObject(PropOwnedForms);
                int integer = base.Properties.GetInteger(PropOwnedFormsCount);
                Form[] destinationArray = new Form[integer];
                if (integer > 0)
                {
                    Array.Copy(sourceArray, 0, destinationArray, 0, integer);
                }
                return destinationArray;
            }
        }

        [System.Windows.Forms.SRCategory("CatWindowStyle"), Browsable(false), System.Windows.Forms.SRDescription("FormOwnerDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Form Owner
        {
            get
            {
                System.Windows.Forms.IntSecurity.GetParent.Demand();
                return this.OwnerInternal;
            }
            set
            {
                Form ownerInternal = this.OwnerInternal;
                if (ownerInternal != value)
                {
                    if ((value != null) && !this.TopLevel)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("NonTopLevelCantHaveOwner"), "value");
                    }
                    Control.CheckParentingCycle(this, value);
                    Control.CheckParentingCycle(value, this);
                    base.Properties.SetObject(PropOwner, null);
                    if (ownerInternal != null)
                    {
                        ownerInternal.RemoveOwnedForm(this);
                    }
                    base.Properties.SetObject(PropOwner, value);
                    if (value != null)
                    {
                        value.AddOwnedForm(this);
                    }
                    this.UpdateHandleWithOwner();
                }
            }
        }

        internal Form OwnerInternal
        {
            get
            {
                return (Form) base.Properties.GetObject(PropOwner);
            }
        }

        internal override Control ParentInternal
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return base.ParentInternal;
            }
            set
            {
                if (value != null)
                {
                    this.Owner = null;
                }
                base.ParentInternal = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Rectangle RestoreBounds
        {
            get
            {
                if (((this.restoreBounds.Width == -1) && (this.restoreBounds.Height == -1)) && ((this.restoreBounds.X == -1) && (this.restoreBounds.Y == -1)))
                {
                    return base.Bounds;
                }
                return this.restoreBounds;
            }
        }

        [System.Windows.Forms.SRDescription("ControlRightToLeftLayoutDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(false)]
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

        [System.Windows.Forms.SRCategory("CatWindowStyle"), DefaultValue(true), System.Windows.Forms.SRDescription("FormShowIconDescr")]
        public bool ShowIcon
        {
            get
            {
                return (this.formStateEx[FormStateExShowIcon] != 0);
            }
            set
            {
                if (value)
                {
                    this.formStateEx[FormStateExShowIcon] = 1;
                }
                else
                {
                    if (this.IsRestrictedWindow)
                    {
                        return;
                    }
                    this.formStateEx[FormStateExShowIcon] = 0;
                    base.UpdateStyles();
                }
                this.UpdateWindowIcon(true);
            }
        }

        [System.Windows.Forms.SRCategory("CatWindowStyle"), DefaultValue(true), System.Windows.Forms.SRDescription("FormShowInTaskbarDescr")]
        public bool ShowInTaskbar
        {
            get
            {
                return (this.formState[FormStateTaskBar] != 0);
            }
            set
            {
                if (!this.IsRestrictedWindow && (this.ShowInTaskbar != value))
                {
                    if (value)
                    {
                        this.formState[FormStateTaskBar] = 1;
                    }
                    else
                    {
                        this.formState[FormStateTaskBar] = 0;
                    }
                    if (base.IsHandleCreated)
                    {
                        base.RecreateHandle();
                    }
                }
            }
        }

        internal override int ShowParams
        {
            get
            {
                switch (this.WindowState)
                {
                    case FormWindowState.Minimized:
                        return 2;

                    case FormWindowState.Maximized:
                        return 3;
                }
                if (this.ShowWithoutActivation)
                {
                    return 4;
                }
                return 5;
            }
        }

        [Browsable(false)]
        protected virtual bool ShowWithoutActivation
        {
            get
            {
                return false;
            }
        }

        [Localizable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatWindowStyle"), System.Windows.Forms.SRDescription("FormSizeGripStyleDescr"), DefaultValue(0)]
        public System.Windows.Forms.SizeGripStyle SizeGripStyle
        {
            get
            {
                return (System.Windows.Forms.SizeGripStyle) this.formState[FormStateSizeGripStyle];
            }
            set
            {
                if (this.SizeGripStyle != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.SizeGripStyle));
                    }
                    this.formState[FormStateSizeGripStyle] = (int) value;
                    this.UpdateRenderSizeGrip();
                }
            }
        }

        [System.Windows.Forms.SRDescription("FormStartPositionDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(2)]
        public FormStartPosition StartPosition
        {
            get
            {
                return (FormStartPosition) this.formState[FormStateStartPos];
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 4))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(FormStartPosition));
                }
                this.formState[FormStateStartPos] = (int) value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TabIndex
        {
            get
            {
                return base.TabIndex;
            }
            set
            {
                base.TabIndex = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DispId(-516), System.Windows.Forms.SRDescription("ControlTabStopDescr")]
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

        private HandleRef TaskbarOwner
        {
            get
            {
                if (this.ownerWindow == null)
                {
                    this.ownerWindow = new NativeWindow();
                }
                if (this.ownerWindow.Handle == IntPtr.Zero)
                {
                    System.Windows.Forms.CreateParams cp = new System.Windows.Forms.CreateParams {
                        ExStyle = 0x80
                    };
                    this.ownerWindow.CreateHandle(cp);
                }
                return new HandleRef(this.ownerWindow, this.ownerWindow.Handle);
            }
        }

        [SettingsBindable(true)]
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

        [EditorBrowsable(EditorBrowsableState.Advanced), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool TopLevel
        {
            get
            {
                return base.GetTopLevel();
            }
            set
            {
                if ((!value && this.IsMdiContainer) && !base.DesignMode)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("MDIContainerMustBeTopLevel"), "value");
                }
                base.SetTopLevel(value);
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatWindowStyle"), System.Windows.Forms.SRDescription("FormTopMostDescr")]
        public bool TopMost
        {
            get
            {
                return (this.formState[FormStateTopMost] != 0);
            }
            set
            {
                if (!this.IsRestrictedWindow)
                {
                    if (base.IsHandleCreated && this.TopLevel)
                    {
                        HandleRef hWndInsertAfter = value ? System.Windows.Forms.NativeMethods.HWND_TOPMOST : System.Windows.Forms.NativeMethods.HWND_NOTOPMOST;
                        System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, base.Handle), hWndInsertAfter, 0, 0, 0, 0, 3);
                    }
                    if (value)
                    {
                        this.formState[FormStateTopMost] = 1;
                    }
                    else
                    {
                        this.formState[FormStateTopMost] = 0;
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatWindowStyle"), System.Windows.Forms.SRDescription("FormTransparencyKeyDescr")]
        public Color TransparencyKey
        {
            get
            {
                object obj2 = base.Properties.GetObject(PropTransparencyKey);
                if (obj2 != null)
                {
                    return (Color) obj2;
                }
                return Color.Empty;
            }
            set
            {
                base.Properties.SetObject(PropTransparencyKey, value);
                if (!this.IsMdiContainer)
                {
                    bool flag = this.formState[FormStateLayered] == 1;
                    if (value != Color.Empty)
                    {
                        System.Windows.Forms.IntSecurity.TransparentWindows.Demand();
                        this.AllowTransparency = true;
                        this.formState[FormStateLayered] = 1;
                    }
                    else
                    {
                        this.formState[FormStateLayered] = (this.OpacityAsByte < 0xff) ? 1 : 0;
                    }
                    if (flag != (this.formState[FormStateLayered] != 0))
                    {
                        base.UpdateStyles();
                    }
                    this.UpdateLayered();
                }
            }
        }

        [System.Windows.Forms.SRDescription("FormWindowStateDescr"), System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(0)]
        public FormWindowState WindowState
        {
            get
            {
                return (FormWindowState) this.formState[FormStateWindowState];
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(FormWindowState));
                }
                if ((!this.TopLevel || !this.IsRestrictedWindow) || (value == FormWindowState.Normal))
                {
                    switch (value)
                    {
                        case FormWindowState.Normal:
                            base.SetState(0x10000, false);
                            break;

                        case FormWindowState.Minimized:
                        case FormWindowState.Maximized:
                            base.SetState(0x10000, true);
                            break;
                    }
                    if (base.IsHandleCreated && base.Visible)
                    {
                        IntPtr handle = base.Handle;
                        switch (value)
                        {
                            case FormWindowState.Normal:
                                System.Windows.Forms.SafeNativeMethods.ShowWindow(new HandleRef(this, handle), 1);
                                break;

                            case FormWindowState.Minimized:
                                System.Windows.Forms.SafeNativeMethods.ShowWindow(new HandleRef(this, handle), 6);
                                break;

                            case FormWindowState.Maximized:
                                System.Windows.Forms.SafeNativeMethods.ShowWindow(new HandleRef(this, handle), 3);
                                break;
                        }
                    }
                    this.formState[FormStateWindowState] = (int) value;
                }
            }
        }

        internal override string WindowText
        {
            get
            {
                if (!this.IsRestrictedWindow || (this.formState[FormStateIsWindowActivated] != 1))
                {
                    return base.WindowText;
                }
                if (this.userWindowText == null)
                {
                    return "";
                }
                return this.userWindowText;
            }
            set
            {
                string windowText = this.WindowText;
                this.userWindowText = value;
                if (this.IsRestrictedWindow && (this.formState[FormStateIsWindowActivated] == 1))
                {
                    if (value == null)
                    {
                        value = "";
                    }
                    base.WindowText = this.RestrictedWindowText(value);
                }
                else
                {
                    base.WindowText = value;
                }
                if (((windowText == null) || (windowText.Length == 0)) || ((value == null) || (value.Length == 0)))
                {
                    this.UpdateFormStyles();
                }
            }
        }

        [ComVisible(false)]
        public class ControlCollection : Control.ControlCollection
        {
            private Form owner;

            public ControlCollection(Form owner) : base(owner)
            {
                this.owner = owner;
            }

            public override void Add(Control value)
            {
                if ((value is MdiClient) && (this.owner.ctlClient == null))
                {
                    if (!this.owner.TopLevel && !this.owner.DesignMode)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("MDIContainerMustBeTopLevel"), "value");
                    }
                    this.owner.AutoScroll = false;
                    if (this.owner.IsMdiChild)
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("FormMDIParentAndChild"), "value");
                    }
                    this.owner.ctlClient = (MdiClient) value;
                }
                if ((value is Form) && (((Form) value).MdiParentInternal != null))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("FormMDIParentCannotAdd"), "value");
                }
                base.Add(value);
                if (this.owner.ctlClient != null)
                {
                    this.owner.ctlClient.SendToBack();
                }
            }

            public override void Remove(Control value)
            {
                if (value == this.owner.ctlClient)
                {
                    this.owner.ctlClient = null;
                }
                base.Remove(value);
            }
        }

        private class EnumThreadWindowsCallback
        {
            private List<HandleRef> ownedWindows;

            internal EnumThreadWindowsCallback()
            {
            }

            internal bool Callback(IntPtr hWnd, IntPtr lParam)
            {
                HandleRef ref2 = new HandleRef(null, hWnd);
                if (System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(ref2, -8) == lParam)
                {
                    if (this.ownedWindows == null)
                    {
                        this.ownedWindows = new List<HandleRef>();
                    }
                    this.ownedWindows.Add(ref2);
                }
                return true;
            }

            internal void ResetOwners()
            {
                if (this.ownedWindows != null)
                {
                    foreach (HandleRef ref2 in this.ownedWindows)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(ref2, -8, System.Windows.Forms.NativeMethods.NullHandleRef);
                    }
                }
            }

            internal void SetOwners(HandleRef hRefOwner)
            {
                if (this.ownedWindows != null)
                {
                    foreach (HandleRef ref2 in this.ownedWindows)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(ref2, -8, hRefOwner);
                    }
                }
            }
        }

        private class SecurityToolTip : IDisposable
        {
            private bool first = true;
            private Form owner;
            private string toolTipText;
            private ToolTipNativeWindow window;

            internal SecurityToolTip(Form owner)
            {
                this.owner = owner;
                this.SetupText();
                this.window = new ToolTipNativeWindow(this);
                this.SetupToolTip();
                owner.LocationChanged += new EventHandler(this.FormLocationChanged);
                owner.HandleCreated += new EventHandler(this.FormHandleCreated);
            }

            public void Dispose()
            {
                if (this.owner != null)
                {
                    this.owner.LocationChanged -= new EventHandler(this.FormLocationChanged);
                }
                if (this.window.Handle != IntPtr.Zero)
                {
                    this.window.DestroyHandle();
                    this.window = null;
                }
            }

            private void FormHandleCreated(object sender, EventArgs e)
            {
                this.RecreateHandle();
            }

            private void FormLocationChanged(object sender, EventArgs e)
            {
                if ((this.window != null) && this.first)
                {
                    Size captionButtonSize = SystemInformation.CaptionButtonSize;
                    if (this.owner.WindowState == FormWindowState.Minimized)
                    {
                        this.Pop(true);
                    }
                    else
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.window, this.window.Handle), 0x412, 0, System.Windows.Forms.NativeMethods.Util.MAKELONG(this.owner.Left + (captionButtonSize.Width / 2), this.owner.Top + SystemInformation.CaptionHeight));
                    }
                }
                else
                {
                    this.Pop(true);
                }
            }

            private System.Windows.Forms.NativeMethods.TOOLINFO_T GetTOOLINFO()
            {
                System.Windows.Forms.NativeMethods.TOOLINFO_T toolinfo_t;
                toolinfo_t = new System.Windows.Forms.NativeMethods.TOOLINFO_T {
                    cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_T)),
                    uFlags = toolinfo_t.uFlags | 0x10,
                    lpszText = this.toolTipText
                };
                if (this.owner.RightToLeft == RightToLeft.Yes)
                {
                    toolinfo_t.uFlags |= 4;
                }
                if (!this.first)
                {
                    toolinfo_t.uFlags |= 0x100;
                    toolinfo_t.hwnd = this.owner.Handle;
                    Size captionButtonSize = SystemInformation.CaptionButtonSize;
                    Rectangle r = new Rectangle(this.owner.Left, this.owner.Top, captionButtonSize.Width, SystemInformation.CaptionHeight);
                    r = this.owner.RectangleToClient(r);
                    r.Width -= r.X;
                    r.Y++;
                    toolinfo_t.rect = System.Windows.Forms.NativeMethods.RECT.FromXYWH(r.X, r.Y, r.Width, r.Height);
                    toolinfo_t.uId = IntPtr.Zero;
                    return toolinfo_t;
                }
                toolinfo_t.uFlags |= 0x21;
                toolinfo_t.hwnd = IntPtr.Zero;
                toolinfo_t.uId = this.owner.Handle;
                return toolinfo_t;
            }

            internal void Pop(bool noLongerFirst)
            {
                if (noLongerFirst)
                {
                    this.first = false;
                }
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.window, this.window.Handle), 0x411, 0, this.GetTOOLINFO());
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.window, this.window.Handle), System.Windows.Forms.NativeMethods.TTM_DELTOOL, 0, this.GetTOOLINFO());
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.window, this.window.Handle), System.Windows.Forms.NativeMethods.TTM_ADDTOOL, 0, this.GetTOOLINFO());
            }

            private void RecreateHandle()
            {
                if (this.window != null)
                {
                    if (this.window.Handle != IntPtr.Zero)
                    {
                        this.window.DestroyHandle();
                    }
                    this.SetupToolTip();
                }
            }

            private void SetupText()
            {
                this.owner.EnsureSecurityInformation();
                string str = System.Windows.Forms.SR.GetString("SecurityToolTipMainText");
                string str2 = System.Windows.Forms.SR.GetString("SecurityToolTipSourceInformation", new object[] { this.owner.securitySite });
                this.toolTipText = System.Windows.Forms.SR.GetString("SecurityToolTipTextFormat", new object[] { str, str2 });
            }

            private void SetupToolTip()
            {
                this.window.CreateHandle(this.CreateParams);
                System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this.window, this.window.Handle), System.Windows.Forms.NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, 0x13);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.window, this.window.Handle), 0x418, 0, this.owner.Width);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.window, this.window.Handle), System.Windows.Forms.NativeMethods.TTM_SETTITLE, 2, System.Windows.Forms.SR.GetString("SecurityToolTipCaption"));
                int num1 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.window, this.window.Handle), System.Windows.Forms.NativeMethods.TTM_ADDTOOL, 0, this.GetTOOLINFO());
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.window, this.window.Handle), 0x401, 1, 0);
                this.Show();
            }

            internal void Show()
            {
                if (this.first)
                {
                    Size captionButtonSize = SystemInformation.CaptionButtonSize;
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.window, this.window.Handle), 0x412, 0, System.Windows.Forms.NativeMethods.Util.MAKELONG(this.owner.Left + (captionButtonSize.Width / 2), this.owner.Top + SystemInformation.CaptionHeight));
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.window, this.window.Handle), 0x411, 1, this.GetTOOLINFO());
                }
            }

            private void WndProc(ref Message msg)
            {
                if (this.first && (((msg.Msg == 0x201) || (msg.Msg == 0x204)) || ((msg.Msg == 0x207) || (msg.Msg == 0x20b))))
                {
                    this.Pop(true);
                }
                this.window.DefWndProc(ref msg);
            }

            private System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    System.Windows.Forms.CreateParams @params;
                    System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                        dwICC = 8
                    };
                    System.Windows.Forms.SafeNativeMethods.InitCommonControlsEx(icc);
                    return new System.Windows.Forms.CreateParams { Parent = this.owner.Handle, ClassName = "tooltips_class32", Style = @params.Style | 0x41, ExStyle = 0, Caption = null };
                }
            }

            internal bool Modal
            {
                get
                {
                    return this.first;
                }
            }

            private sealed class ToolTipNativeWindow : NativeWindow
            {
                private Form.SecurityToolTip control;

                internal ToolTipNativeWindow(Form.SecurityToolTip control)
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
    }
}

