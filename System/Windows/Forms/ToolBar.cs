namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), DefaultProperty("Buttons"), ClassInterface(ClassInterfaceType.AutoDispatch), DefaultEvent("ButtonClick"), Designer("System.Windows.Forms.Design.ToolBarDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ToolBar : Control
    {
        private ToolBarAppearance appearance;
        private System.Windows.Forms.BorderStyle borderStyle;
        private int buttonCount;
        private ToolBarButton[] buttons;
        private ToolBarButtonCollection buttonsCollection;
        internal Size buttonSize = Size.Empty;
        private float currentScaleDX = 1f;
        private float currentScaleDY = 1f;
        internal const int DDARROW_WIDTH = 15;
        private int hotItem = -1;
        private System.Windows.Forms.ImageList imageList;
        private int maxWidth = -1;
        private int requestedSize;
        private ToolBarTextAlign textAlign;
        private BitVector32 toolBarState = new BitVector32(0x1f);
        private const int TOOLBARSTATE_autoSize = 0x10;
        private const int TOOLBARSTATE_divider = 4;
        private const int TOOLBARSTATE_dropDownArrows = 2;
        private const int TOOLBARSTATE_showToolTips = 8;
        private const int TOOLBARSTATE_wrappable = 1;

        [System.Windows.Forms.SRDescription("ControlOnAutoSizeChangedDescr"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRCategory("CatPropertyChanged")]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ToolBarButtonClickDescr")]
        public event ToolBarButtonClickEventHandler ButtonClick;

        [System.Windows.Forms.SRDescription("ToolBarButtonDropDownDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event ToolBarButtonClickEventHandler ButtonDropDown;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler RightToLeftChanged
        {
            add
            {
                base.RightToLeftChanged += value;
            }
            remove
            {
                base.RightToLeftChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        public ToolBar()
        {
            base.SetStyle(ControlStyles.UserPaint, false);
            base.SetStyle(ControlStyles.FixedHeight, this.AutoSize);
            base.SetStyle(ControlStyles.FixedWidth, false);
            this.TabStop = false;
            this.Dock = DockStyle.Top;
            this.buttonsCollection = new ToolBarButtonCollection(this);
        }

        private void AdjustSize(DockStyle dock)
        {
            int requestedSize = this.requestedSize;
            try
            {
                if ((dock == DockStyle.Left) || (dock == DockStyle.Right))
                {
                    base.Width = this.AutoSize ? this.PreferredWidth : requestedSize;
                }
                else
                {
                    base.Height = this.AutoSize ? this.PreferredHeight : requestedSize;
                }
            }
            finally
            {
                this.requestedSize = requestedSize;
            }
        }

        internal void BeginUpdate()
        {
            base.BeginUpdateInternal();
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

        private void DetachImageList(object sender, EventArgs e)
        {
            this.ImageList = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    bool state = base.GetState(0x1000);
                    try
                    {
                        base.SetState(0x1000, true);
                        if (this.imageList != null)
                        {
                            this.imageList.Disposed -= new EventHandler(this.DetachImageList);
                            this.imageList = null;
                        }
                        if (this.buttonsCollection != null)
                        {
                            ToolBarButton[] array = new ToolBarButton[this.buttonsCollection.Count];
                            ((ICollection) this.buttonsCollection).CopyTo(array, 0);
                            this.buttonsCollection.Clear();
                            foreach (ToolBarButton button in array)
                            {
                                button.Dispose();
                            }
                        }
                    }
                    finally
                    {
                        base.SetState(0x1000, state);
                    }
                }
            }
            base.Dispose(disposing);
        }

        internal void EndUpdate()
        {
            base.EndUpdateInternal();
        }

        private void ForceButtonWidths()
        {
            if (((this.buttons != null) && this.buttonSize.IsEmpty) && base.IsHandleCreated)
            {
                this.maxWidth = -1;
                for (int i = 0; i < this.buttonCount; i++)
                {
                    System.Windows.Forms.NativeMethods.TBBUTTONINFO lParam = new System.Windows.Forms.NativeMethods.TBBUTTONINFO {
                        cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TBBUTTONINFO)),
                        cx = this.buttons[i].Width
                    };
                    if (lParam.cx > this.maxWidth)
                    {
                        this.maxWidth = lParam.cx;
                    }
                    lParam.dwMask = 0x40;
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.TB_SETBUTTONINFO, i, ref lParam);
                }
            }
        }

        private void ImageListRecreateHandle(object sender, EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                base.RecreateHandle();
            }
        }

        private void Insert(int index, ToolBarButton button)
        {
            button.parent = this;
            if (this.buttons == null)
            {
                this.buttons = new ToolBarButton[4];
            }
            else if (this.buttons.Length == this.buttonCount)
            {
                ToolBarButton[] destinationArray = new ToolBarButton[this.buttonCount + 4];
                Array.Copy(this.buttons, 0, destinationArray, 0, this.buttonCount);
                this.buttons = destinationArray;
            }
            if (index < this.buttonCount)
            {
                Array.Copy(this.buttons, index, this.buttons, index + 1, this.buttonCount - index);
            }
            this.buttons[index] = button;
            this.buttonCount++;
        }

        private void InsertButton(int index, ToolBarButton value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((index < 0) || ((this.buttons != null) && (index > this.buttonCount)))
            {
                throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
            }
            this.Insert(index, value);
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.TBBUTTON tBBUTTON = value.GetTBBUTTON(index);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.TB_INSERTBUTTON, index, ref tBBUTTON);
            }
            this.UpdateButtons();
        }

        private int InternalAddButton(ToolBarButton button)
        {
            if (button == null)
            {
                throw new ArgumentNullException("button");
            }
            int buttonCount = this.buttonCount;
            this.Insert(buttonCount, button);
            return buttonCount;
        }

        internal void InternalSetButton(int index, ToolBarButton value, bool recreate, bool updateText)
        {
            this.buttons[index].parent = null;
            this.buttons[index].stringIndex = (IntPtr) (-1);
            this.buttons[index] = value;
            this.buttons[index].parent = this;
            if (base.IsHandleCreated)
            {
                System.Windows.Forms.NativeMethods.TBBUTTONINFO tBBUTTONINFO = value.GetTBBUTTONINFO(updateText, index);
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), System.Windows.Forms.NativeMethods.TB_SETBUTTONINFO, index, ref tBBUTTONINFO);
                if (tBBUTTONINFO.pszText != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(tBBUTTONINFO.pszText);
                }
                if (recreate)
                {
                    this.UpdateButtons();
                }
                else
                {
                    base.SendMessage(0x421, 0, 0);
                    this.ForceButtonWidths();
                    base.Invalidate();
                }
            }
        }

        protected virtual void OnButtonClick(ToolBarButtonClickEventArgs e)
        {
            if (this.onButtonClick != null)
            {
                this.onButtonClick(this, e);
            }
        }

        protected virtual void OnButtonDropDown(ToolBarButtonClickEventArgs e)
        {
            if (this.onButtonDropDown != null)
            {
                this.onButtonDropDown(this, e);
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (base.IsHandleCreated)
            {
                if (!this.buttonSize.IsEmpty)
                {
                    this.SendToolbarButtonSizeMessage();
                }
                else
                {
                    this.AdjustSize(this.Dock);
                    this.ForceButtonWidths();
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            base.SendMessage(0x41e, Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TBBUTTON)), 0);
            if (this.DropDownArrows)
            {
                base.SendMessage(0x454, 0, 1);
            }
            if (this.imageList != null)
            {
                base.SendMessage(0x430, 0, this.imageList.Handle);
            }
            this.RealizeButtons();
            this.BeginUpdate();
            try
            {
                Size size = base.Size;
                base.Size = new Size(size.Width + 1, size.Height);
                base.Size = size;
            }
            finally
            {
                this.EndUpdate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.Wrappable)
            {
                this.AdjustSize(this.Dock);
            }
        }

        private void RealizeButtons()
        {
            if (this.buttons != null)
            {
                IntPtr zero = IntPtr.Zero;
                try
                {
                    this.BeginUpdate();
                    for (int i = 0; i < this.buttonCount; i++)
                    {
                        if (this.buttons[i].Text.Length > 0)
                        {
                            string lparam = this.buttons[i].Text + '\0'.ToString();
                            this.buttons[i].stringIndex = base.SendMessage(System.Windows.Forms.NativeMethods.TB_ADDSTRING, 0, lparam);
                        }
                        else
                        {
                            this.buttons[i].stringIndex = (IntPtr) (-1);
                        }
                    }
                    int num2 = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TBBUTTON));
                    int buttonCount = this.buttonCount;
                    zero = Marshal.AllocHGlobal((int) (num2 * buttonCount));
                    for (int j = 0; j < buttonCount; j++)
                    {
                        Marshal.StructureToPtr(this.buttons[j].GetTBBUTTON(j), (IntPtr) (((long) zero) + (num2 * j)), true);
                        this.buttons[j].parent = this;
                    }
                    base.SendMessage(System.Windows.Forms.NativeMethods.TB_ADDBUTTONS, buttonCount, zero);
                    base.SendMessage(0x421, 0, 0);
                    if (!this.buttonSize.IsEmpty)
                    {
                        this.SendToolbarButtonSizeMessage();
                    }
                    else
                    {
                        this.ForceButtonWidths();
                    }
                    this.AdjustSize(this.Dock);
                }
                finally
                {
                    Marshal.FreeHGlobal(zero);
                    this.EndUpdate();
                }
            }
        }

        private void RemoveAt(int index)
        {
            this.buttons[index].parent = null;
            this.buttons[index].stringIndex = (IntPtr) (-1);
            this.buttonCount--;
            if (index < this.buttonCount)
            {
                Array.Copy(this.buttons, index + 1, this.buttons, index, this.buttonCount - index);
            }
            this.buttons[this.buttonCount] = null;
        }

        private void ResetButtonSize()
        {
            this.buttonSize = Size.Empty;
            base.RecreateHandle();
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            this.currentScaleDX = factor.Width;
            this.currentScaleDY = factor.Height;
            base.ScaleControl(factor, specified);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void ScaleCore(float dx, float dy)
        {
            this.currentScaleDX = dx;
            this.currentScaleDY = dy;
            base.ScaleCore(dx, dy);
            this.UpdateButtons();
        }

        private void SendToolbarButtonSizeMessage()
        {
            base.SendMessage(0x41f, 0, System.Windows.Forms.NativeMethods.Util.MAKELPARAM((int) (this.buttonSize.Width * this.currentScaleDX), (int) (this.buttonSize.Height * this.currentScaleDY)));
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            int num = height;
            int num2 = width;
            base.SetBoundsCore(x, y, width, height, specified);
            Rectangle bounds = base.Bounds;
            if ((this.Dock == DockStyle.Left) || (this.Dock == DockStyle.Right))
            {
                if ((specified & BoundsSpecified.Width) != BoundsSpecified.None)
                {
                    this.requestedSize = width;
                }
                if (this.AutoSize)
                {
                    width = this.PreferredWidth;
                }
                if ((width != num2) && (this.Dock == DockStyle.Right))
                {
                    int num3 = num2 - width;
                    x += num3;
                }
            }
            else
            {
                if ((specified & BoundsSpecified.Height) != BoundsSpecified.None)
                {
                    this.requestedSize = height;
                }
                if (this.AutoSize)
                {
                    height = this.PreferredHeight;
                }
                if ((height != num) && (this.Dock == DockStyle.Bottom))
                {
                    int num4 = num - height;
                    y += num4;
                }
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        internal void SetToolTip(ToolTip toolTip)
        {
            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, base.Handle), 0x424, new HandleRef(toolTip, toolTip.Handle), 0);
        }

        private bool ShouldSerializeButtonSize()
        {
            return !this.buttonSize.IsEmpty;
        }

        public override string ToString()
        {
            string str = base.ToString() + ", Buttons.Count: " + this.buttonCount.ToString(CultureInfo.CurrentCulture);
            if (this.buttonCount > 0)
            {
                str = str + ", Buttons[0]: " + this.buttons[0].ToString();
            }
            return str;
        }

        internal void UpdateButtons()
        {
            if (base.IsHandleCreated)
            {
                base.RecreateHandle();
            }
        }

        private void WmNotifyDropDown(ref Message m)
        {
            System.Windows.Forms.NativeMethods.NMTOOLBAR lParam = (System.Windows.Forms.NativeMethods.NMTOOLBAR) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMTOOLBAR));
            ToolBarButton button = this.buttons[lParam.iItem];
            if (button == null)
            {
                throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ToolBarButtonNotFound"));
            }
            this.OnButtonDropDown(new ToolBarButtonClickEventArgs(button));
            Menu dropDownMenu = button.DropDownMenu;
            if (dropDownMenu != null)
            {
                System.Windows.Forms.NativeMethods.RECT lparam = new System.Windows.Forms.NativeMethods.RECT();
                System.Windows.Forms.NativeMethods.TPMPARAMS tpm = new System.Windows.Forms.NativeMethods.TPMPARAMS();
                base.SendMessage(0x433, lParam.iItem, ref lparam);
                if (dropDownMenu.GetType().IsAssignableFrom(typeof(ContextMenu)))
                {
                    ((ContextMenu) dropDownMenu).Show(this, new Point(lparam.left, lparam.bottom));
                }
                else
                {
                    Menu mainMenu = dropDownMenu.GetMainMenu();
                    if (mainMenu != null)
                    {
                        mainMenu.ProcessInitMenuPopup(dropDownMenu.Handle);
                    }
                    System.Windows.Forms.UnsafeNativeMethods.MapWindowPoints(new HandleRef(lParam.hdr, lParam.hdr.hwndFrom), System.Windows.Forms.NativeMethods.NullHandleRef, ref lparam, 2);
                    tpm.rcExclude_left = lparam.left;
                    tpm.rcExclude_top = lparam.top;
                    tpm.rcExclude_right = lparam.right;
                    tpm.rcExclude_bottom = lparam.bottom;
                    System.Windows.Forms.SafeNativeMethods.TrackPopupMenuEx(new HandleRef(dropDownMenu, dropDownMenu.Handle), 0x40, lparam.left, lparam.bottom, new HandleRef(this, base.Handle), tpm);
                }
            }
        }

        private void WmNotifyHotItemChange(ref Message m)
        {
            System.Windows.Forms.NativeMethods.NMTBHOTITEM lParam = (System.Windows.Forms.NativeMethods.NMTBHOTITEM) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMTBHOTITEM));
            if (0x10 == (lParam.dwFlags & 0x10))
            {
                this.hotItem = lParam.idNew;
            }
            else if (0x20 == (lParam.dwFlags & 0x20))
            {
                this.hotItem = -1;
            }
            else if (1 == (lParam.dwFlags & 1))
            {
                this.hotItem = lParam.idNew;
            }
            else if (2 == (lParam.dwFlags & 2))
            {
                this.hotItem = lParam.idNew;
            }
            else if (4 == (lParam.dwFlags & 4))
            {
                this.hotItem = lParam.idNew;
            }
            else if (8 == (lParam.dwFlags & 8))
            {
                this.hotItem = lParam.idNew;
            }
            else if (0x40 == (lParam.dwFlags & 0x40))
            {
                this.hotItem = lParam.idNew;
            }
            else if (0x80 == (lParam.dwFlags & 0x80))
            {
                this.hotItem = lParam.idNew;
            }
            else if (0x100 == (lParam.dwFlags & 0x100))
            {
                this.hotItem = lParam.idNew;
            }
        }

        private void WmNotifyNeedText(ref Message m)
        {
            System.Windows.Forms.NativeMethods.TOOLTIPTEXT lParam = (System.Windows.Forms.NativeMethods.TOOLTIPTEXT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.TOOLTIPTEXT));
            int idFrom = (int) lParam.hdr.idFrom;
            ToolBarButton button = this.buttons[idFrom];
            if ((button != null) && (button.ToolTipText != null))
            {
                lParam.lpszText = button.ToolTipText;
            }
            else
            {
                lParam.lpszText = null;
            }
            lParam.hinst = IntPtr.Zero;
            if (this.RightToLeft == System.Windows.Forms.RightToLeft.Yes)
            {
                lParam.uFlags |= 4;
            }
            Marshal.StructureToPtr(lParam, m.LParam, false);
        }

        private void WmNotifyNeedTextA(ref Message m)
        {
            System.Windows.Forms.NativeMethods.TOOLTIPTEXTA lParam = (System.Windows.Forms.NativeMethods.TOOLTIPTEXTA) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.TOOLTIPTEXTA));
            int idFrom = (int) lParam.hdr.idFrom;
            ToolBarButton button = this.buttons[idFrom];
            if ((button != null) && (button.ToolTipText != null))
            {
                lParam.lpszText = button.ToolTipText;
            }
            else
            {
                lParam.lpszText = null;
            }
            lParam.hinst = IntPtr.Zero;
            if (this.RightToLeft == System.Windows.Forms.RightToLeft.Yes)
            {
                lParam.uFlags |= 4;
            }
            Marshal.StructureToPtr(lParam, m.LParam, false);
        }

        private void WmReflectCommand(ref Message m)
        {
            int index = System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam);
            ToolBarButton button = this.buttons[index];
            if (button != null)
            {
                ToolBarButtonClickEventArgs e = new ToolBarButtonClickEventArgs(button);
                this.OnButtonClick(e);
            }
            base.WndProc(ref m);
            base.ResetMouseEventArgs();
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            int msg = m.Msg;
            switch (msg)
            {
                case 0x4e:
                case 0x204e:
                {
                    System.Windows.Forms.NativeMethods.NMHDR lParam = (System.Windows.Forms.NativeMethods.NMHDR) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHDR));
                    switch (lParam.code)
                    {
                        case -521:
                        {
                            System.Windows.Forms.NativeMethods.WINDOWPLACEMENT placement = new System.Windows.Forms.NativeMethods.WINDOWPLACEMENT();
                            System.Windows.Forms.UnsafeNativeMethods.GetWindowPlacement(new HandleRef(null, lParam.hwndFrom), ref placement);
                            if (((placement.rcNormalPosition_left != 0) || (placement.rcNormalPosition_top != 0)) || (this.hotItem == -1))
                            {
                                goto Label_02BE;
                            }
                            int num = 0;
                            for (int i = 0; i <= this.hotItem; i++)
                            {
                                num += this.buttonsCollection[i].GetButtonWidth();
                            }
                            int num3 = placement.rcNormalPosition_right - placement.rcNormalPosition_left;
                            int num4 = placement.rcNormalPosition_bottom - placement.rcNormalPosition_top;
                            int x = (base.Location.X + num) + 1;
                            int y = base.Location.Y + (this.ButtonSize.Height / 2);
                            System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT(x, y);
                            System.Windows.Forms.UnsafeNativeMethods.ClientToScreen(new HandleRef(this, base.Handle), pt);
                            if (pt.y < SystemInformation.WorkingArea.Y)
                            {
                                pt.y += (this.ButtonSize.Height / 2) + 1;
                            }
                            if ((pt.y + num4) > SystemInformation.WorkingArea.Height)
                            {
                                pt.y -= ((this.ButtonSize.Height / 2) + num4) + 1;
                            }
                            if ((pt.x + num3) > SystemInformation.WorkingArea.Right)
                            {
                                pt.x -= (this.ButtonSize.Width + num3) + 2;
                            }
                            System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(null, lParam.hwndFrom), System.Windows.Forms.NativeMethods.NullHandleRef, pt.x, pt.y, 0, 0, 0x15);
                            m.Result = (IntPtr) 1;
                            return;
                        }
                        case -520:
                            this.WmNotifyNeedTextA(ref m);
                            m.Result = (IntPtr) 1;
                            return;

                        case -530:
                            if (Marshal.SystemDefaultCharSize != 2)
                            {
                                break;
                            }
                            this.WmNotifyNeedText(ref m);
                            m.Result = (IntPtr) 1;
                            return;

                        case -706:
                            m.Result = (IntPtr) 1;
                            goto Label_02BE;

                        case -713:
                            this.WmNotifyHotItemChange(ref m);
                            goto Label_02BE;

                        case -710:
                            this.WmNotifyDropDown(ref m);
                            goto Label_02BE;
                    }
                    break;
                }
                default:
                    if (msg == 0x2111)
                    {
                        this.WmReflectCommand(ref m);
                    }
                    break;
            }
        Label_02BE:
            base.WndProc(ref m);
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ToolBarAppearanceDescr"), DefaultValue(0), Localizable(true)]
        public ToolBarAppearance Appearance
        {
            get
            {
                return this.appearance;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolBarAppearance));
                }
                if (value != this.appearance)
                {
                    this.appearance = value;
                    base.RecreateHandle();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), Localizable(true), System.Windows.Forms.SRDescription("ToolBarAutoSizeDescr"), Browsable(true)]
        public override bool AutoSize
        {
            get
            {
                return this.toolBarState[0x10];
            }
            set
            {
                if (this.AutoSize != value)
                {
                    this.toolBarState[0x10] = value;
                    if ((this.Dock == DockStyle.Left) || (this.Dock == DockStyle.Right))
                    {
                        base.SetStyle(ControlStyles.FixedWidth, this.AutoSize);
                        base.SetStyle(ControlStyles.FixedHeight, false);
                    }
                    else
                    {
                        base.SetStyle(ControlStyles.FixedHeight, this.AutoSize);
                        base.SetStyle(ControlStyles.FixedWidth, false);
                    }
                    this.AdjustSize(this.Dock);
                    this.OnAutoSizeChanged(EventArgs.Empty);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0), DispId(-504), System.Windows.Forms.SRDescription("ToolBarBorderStyleDescr")]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return this.borderStyle;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.BorderStyle));
                }
                if (this.borderStyle != value)
                {
                    this.borderStyle = value;
                    base.RecreateHandle();
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), MergableProperty(false), System.Windows.Forms.SRDescription("ToolBarButtonsDescr")]
        public ToolBarButtonCollection Buttons
        {
            get
            {
                return this.buttonsCollection;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolBarButtonSizeDescr"), RefreshProperties(RefreshProperties.All), Localizable(true)]
        public Size ButtonSize
        {
            get
            {
                if (!this.buttonSize.IsEmpty)
                {
                    return this.buttonSize;
                }
                if ((base.IsHandleCreated && (this.buttons != null)) && (this.buttonCount > 0))
                {
                    int n = (int) ((long) base.SendMessage(0x43a, 0, 0));
                    if (n > 0)
                    {
                        return new Size(System.Windows.Forms.NativeMethods.Util.LOWORD(n), System.Windows.Forms.NativeMethods.Util.HIWORD(n));
                    }
                }
                if (this.TextAlign == ToolBarTextAlign.Underneath)
                {
                    return new Size(0x27, 0x24);
                }
                return new Size(0x17, 0x16);
            }
            set
            {
                if ((value.Width < 0) || (value.Height < 0))
                {
                    throw new ArgumentOutOfRangeException("ButtonSize", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "ButtonSize", value.ToString() }));
                }
                if (this.buttonSize != value)
                {
                    this.buttonSize = value;
                    this.maxWidth = -1;
                    base.RecreateHandle();
                    this.AdjustSize(this.Dock);
                }
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassName = "ToolbarWindow32";
                createParams.Style |= 12;
                if (!this.Divider)
                {
                    createParams.Style |= 0x40;
                }
                if (this.Wrappable)
                {
                    createParams.Style |= 0x200;
                }
                if (this.ShowToolTips && !base.DesignMode)
                {
                    createParams.Style |= 0x100;
                }
                createParams.ExStyle &= -513;
                createParams.Style &= -8388609;
                switch (this.borderStyle)
                {
                    case System.Windows.Forms.BorderStyle.FixedSingle:
                        createParams.Style |= 0x800000;
                        break;

                    case System.Windows.Forms.BorderStyle.Fixed3D:
                        createParams.ExStyle |= 0x200;
                        break;
                }
                switch (this.appearance)
                {
                    case ToolBarAppearance.Flat:
                        createParams.Style |= 0x800;
                        break;
                }
                switch (this.textAlign)
                {
                    case ToolBarTextAlign.Underneath:
                        return createParams;

                    case ToolBarTextAlign.Right:
                        createParams.Style |= 0x1000;
                        return createParams;
                }
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

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolBarDividerDescr"), DefaultValue(true)]
        public bool Divider
        {
            get
            {
                return this.toolBarState[4];
            }
            set
            {
                if (this.Divider != value)
                {
                    this.toolBarState[4] = value;
                    base.RecreateHandle();
                }
            }
        }

        [DefaultValue(1), Localizable(true)]
        public override DockStyle Dock
        {
            get
            {
                return base.Dock;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 5))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DockStyle));
                }
                if (this.Dock != value)
                {
                    if ((value == DockStyle.Left) || (value == DockStyle.Right))
                    {
                        base.SetStyle(ControlStyles.FixedWidth, this.AutoSize);
                        base.SetStyle(ControlStyles.FixedHeight, false);
                    }
                    else
                    {
                        base.SetStyle(ControlStyles.FixedHeight, this.AutoSize);
                        base.SetStyle(ControlStyles.FixedWidth, false);
                    }
                    this.AdjustSize(value);
                    base.Dock = value;
                }
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

        [DefaultValue(false), System.Windows.Forms.SRDescription("ToolBarDropDownArrowsDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance")]
        public bool DropDownArrows
        {
            get
            {
                return this.toolBarState[2];
            }
            set
            {
                if (this.DropDownArrows != value)
                {
                    this.toolBarState[2] = value;
                    base.RecreateHandle();
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [System.Windows.Forms.SRDescription("ToolBarImageListDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue((string) null)]
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
                    EventHandler handler = new EventHandler(this.ImageListRecreateHandle);
                    EventHandler handler2 = new EventHandler(this.DetachImageList);
                    if (this.imageList != null)
                    {
                        this.imageList.Disposed -= handler2;
                        this.imageList.RecreateHandle -= handler;
                    }
                    this.imageList = value;
                    if (value != null)
                    {
                        value.Disposed += handler2;
                        value.RecreateHandle += handler;
                    }
                    if (base.IsHandleCreated)
                    {
                        base.RecreateHandle();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolBarImageSizeDescr"), EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRCategory("CatBehavior")]
        public Size ImageSize
        {
            get
            {
                if (this.imageList != null)
                {
                    return this.imageList.ImageSize;
                }
                return new Size(0, 0);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        internal int PreferredHeight
        {
            get
            {
                int height = 0;
                if (((this.buttons == null) || (this.buttonCount == 0)) || !base.IsHandleCreated)
                {
                    height = this.ButtonSize.Height;
                }
                else
                {
                    System.Windows.Forms.NativeMethods.RECT lparam = new System.Windows.Forms.NativeMethods.RECT();
                    int index = 0;
                    while (index < this.buttons.Length)
                    {
                        if ((this.buttons[index] != null) && this.buttons[index].Visible)
                        {
                            break;
                        }
                        index++;
                    }
                    if (index == this.buttons.Length)
                    {
                        index = 0;
                    }
                    base.SendMessage(0x433, index, ref lparam);
                    height = lparam.bottom - lparam.top;
                }
                if (this.Wrappable && base.IsHandleCreated)
                {
                    height *= (int) ((long) base.SendMessage(0x428, 0, 0));
                }
                height = (height > 0) ? height : 1;
                switch (this.borderStyle)
                {
                    case System.Windows.Forms.BorderStyle.FixedSingle:
                        height += SystemInformation.BorderSize.Height;
                        break;

                    case System.Windows.Forms.BorderStyle.Fixed3D:
                        height += SystemInformation.Border3DSize.Height;
                        break;
                }
                if (this.Divider)
                {
                    height += 2;
                }
                return (height + 4);
            }
        }

        internal int PreferredWidth
        {
            get
            {
                if (this.maxWidth == -1)
                {
                    if (!base.IsHandleCreated || (this.buttons == null))
                    {
                        this.maxWidth = this.ButtonSize.Width;
                    }
                    else
                    {
                        System.Windows.Forms.NativeMethods.RECT lparam = new System.Windows.Forms.NativeMethods.RECT();
                        for (int i = 0; i < this.buttonCount; i++)
                        {
                            base.SendMessage(0x433, 0, ref lparam);
                            if ((lparam.right - lparam.left) > this.maxWidth)
                            {
                                this.maxWidth = lparam.right - lparam.left;
                            }
                        }
                    }
                }
                int maxWidth = this.maxWidth;
                if (this.borderStyle != System.Windows.Forms.BorderStyle.None)
                {
                    maxWidth += (SystemInformation.BorderSize.Height * 4) + 3;
                }
                return maxWidth;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                return base.RightToLeft;
            }
            set
            {
                base.RightToLeft = value;
            }
        }

        [DefaultValue(false), Localizable(true), System.Windows.Forms.SRDescription("ToolBarShowToolTipsDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool ShowToolTips
        {
            get
            {
                return this.toolBarState[8];
            }
            set
            {
                if (this.ShowToolTips != value)
                {
                    this.toolBarState[8] = value;
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Bindable(false), Browsable(false)]
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

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0), Localizable(true), System.Windows.Forms.SRDescription("ToolBarTextAlignDescr")]
        public ToolBarTextAlign TextAlign
        {
            get
            {
                return this.textAlign;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 1))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolBarTextAlign));
                }
                if (this.textAlign != value)
                {
                    this.textAlign = value;
                    base.RecreateHandle();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolBarWrappableDescr"), Localizable(true), DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool Wrappable
        {
            get
            {
                return this.toolBarState[1];
            }
            set
            {
                if (this.Wrappable != value)
                {
                    this.toolBarState[1] = value;
                    base.RecreateHandle();
                }
            }
        }

        public class ToolBarButtonCollection : IList, ICollection, IEnumerable
        {
            private int lastAccessedIndex = -1;
            private ToolBar owner;
            private bool suspendUpdate;

            public ToolBarButtonCollection(ToolBar owner)
            {
                this.owner = owner;
            }

            public int Add(string text)
            {
                ToolBarButton button = new ToolBarButton(text);
                return this.Add(button);
            }

            public int Add(ToolBarButton button)
            {
                int num = this.owner.InternalAddButton(button);
                if (!this.suspendUpdate)
                {
                    this.owner.UpdateButtons();
                }
                return num;
            }

            public void AddRange(ToolBarButton[] buttons)
            {
                if (buttons == null)
                {
                    throw new ArgumentNullException("buttons");
                }
                try
                {
                    this.suspendUpdate = true;
                    foreach (ToolBarButton button in buttons)
                    {
                        this.Add(button);
                    }
                }
                finally
                {
                    this.suspendUpdate = false;
                    this.owner.UpdateButtons();
                }
            }

            public void Clear()
            {
                if (this.owner.buttons != null)
                {
                    for (int i = this.owner.buttonCount; i > 0; i--)
                    {
                        if (this.owner.IsHandleCreated)
                        {
                            this.owner.SendMessage(0x416, (int) (i - 1), 0);
                        }
                        this.owner.RemoveAt(i - 1);
                    }
                    this.owner.buttons = null;
                    this.owner.buttonCount = 0;
                    if (!this.owner.Disposing)
                    {
                        this.owner.UpdateButtons();
                    }
                }
            }

            public bool Contains(ToolBarButton button)
            {
                return (this.IndexOf(button) != -1);
            }

            public virtual bool ContainsKey(string key)
            {
                return this.IsValidIndex(this.IndexOfKey(key));
            }

            public IEnumerator GetEnumerator()
            {
                return new WindowsFormsUtils.ArraySubsetEnumerator(this.owner.buttons, this.owner.buttonCount);
            }

            public int IndexOf(ToolBarButton button)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i] == button)
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

            public void Insert(int index, ToolBarButton button)
            {
                this.owner.InsertButton(index, button);
            }

            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < this.Count));
            }

            public void Remove(ToolBarButton button)
            {
                int index = this.IndexOf(button);
                if (index != -1)
                {
                    this.RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                int num = (this.owner.buttons == null) ? 0 : this.owner.buttonCount;
                if ((index < 0) || (index >= num))
                {
                    throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                }
                if (this.owner.IsHandleCreated)
                {
                    this.owner.SendMessage(0x416, index, 0);
                }
                this.owner.RemoveAt(index);
                this.owner.UpdateButtons();
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
                if (this.owner.buttonCount > 0)
                {
                    Array.Copy(this.owner.buttons, 0, dest, index, this.owner.buttonCount);
                }
            }

            int IList.Add(object button)
            {
                if (!(button is ToolBarButton))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ToolBarBadToolBarButton"), "button");
                }
                return this.Add((ToolBarButton) button);
            }

            bool IList.Contains(object button)
            {
                return ((button is ToolBarButton) && this.Contains((ToolBarButton) button));
            }

            int IList.IndexOf(object button)
            {
                if (button is ToolBarButton)
                {
                    return this.IndexOf((ToolBarButton) button);
                }
                return -1;
            }

            void IList.Insert(int index, object button)
            {
                if (!(button is ToolBarButton))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ToolBarBadToolBarButton"), "button");
                }
                this.Insert(index, (ToolBarButton) button);
            }

            void IList.Remove(object button)
            {
                if (button is ToolBarButton)
                {
                    this.Remove((ToolBarButton) button);
                }
            }

            [Browsable(false)]
            public int Count
            {
                get
                {
                    return this.owner.buttonCount;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public virtual ToolBarButton this[int index]
            {
                get
                {
                    if ((index < 0) || ((this.owner.buttons != null) && (index >= this.owner.buttonCount)))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    return this.owner.buttons[index];
                }
                set
                {
                    if ((index < 0) || ((this.owner.buttons != null) && (index >= this.owner.buttonCount)))
                    {
                        throw new ArgumentOutOfRangeException("index", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "index", index.ToString(CultureInfo.CurrentCulture) }));
                    }
                    if (value == null)
                    {
                        throw new ArgumentNullException("value");
                    }
                    this.owner.InternalSetButton(index, value, true, true);
                }
            }

            public virtual ToolBarButton this[string key]
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
                    if (!(value is ToolBarButton))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ToolBarBadToolBarButton"), "value");
                    }
                    this[index] = (ToolBarButton) value;
                }
            }
        }
    }
}

