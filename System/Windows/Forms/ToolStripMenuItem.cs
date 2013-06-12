namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms.Design;
    using System.Windows.Forms.Layout;

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ContextMenuStrip | ToolStripItemDesignerAvailability.MenuStrip), DesignerSerializer("System.Windows.Forms.Design.ToolStripMenuItemCodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ToolStripMenuItem : ToolStripDropDownItem
    {
        private Size cachedShortcutSize;
        private string cachedShortcutText;
        [ThreadStatic]
        private static Image checkedImage;
        private bool checkOnClick;
        private static readonly object EventCheckedChanged = new object();
        private static readonly object EventCheckStateChanged = new object();
        [ThreadStatic]
        private static Image indeterminateCheckedImage;
        private ToolStrip lastOwner;
        private static System.Windows.Forms.MenuTimer menuTimer = new System.Windows.Forms.MenuTimer();
        private int nativeMenuCommandID;
        private IntPtr nativeMenuHandle;
        private byte openMouseId;
        private static readonly int PropCheckState = PropertyStore.CreateKey();
        private static readonly int PropMdiForm = PropertyStore.CreateKey();
        private static readonly int PropShortcutKeys = PropertyStore.CreateKey();
        private string shortcutKeyDisplayString;
        private bool showShortcutKeys;
        private IntPtr targetWindowHandle;

        [System.Windows.Forms.SRDescription("CheckBoxOnCheckedChangedDescr")]
        public event EventHandler CheckedChanged
        {
            add
            {
                base.Events.AddHandler(EventCheckedChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCheckedChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("CheckBoxOnCheckStateChangedDescr")]
        public event EventHandler CheckStateChanged
        {
            add
            {
                base.Events.AddHandler(EventCheckStateChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCheckStateChanged, value);
            }
        }

        public ToolStripMenuItem()
        {
            this.showShortcutKeys = true;
            this.nativeMenuCommandID = -1;
            this.targetWindowHandle = IntPtr.Zero;
            this.nativeMenuHandle = IntPtr.Zero;
            this.cachedShortcutSize = Size.Empty;
            this.Initialize();
        }

        public ToolStripMenuItem(Image image) : base(null, image, (EventHandler) null)
        {
            this.showShortcutKeys = true;
            this.nativeMenuCommandID = -1;
            this.targetWindowHandle = IntPtr.Zero;
            this.nativeMenuHandle = IntPtr.Zero;
            this.cachedShortcutSize = Size.Empty;
            this.Initialize();
        }

        public ToolStripMenuItem(string text) : base(text, null, (EventHandler) null)
        {
            this.showShortcutKeys = true;
            this.nativeMenuCommandID = -1;
            this.targetWindowHandle = IntPtr.Zero;
            this.nativeMenuHandle = IntPtr.Zero;
            this.cachedShortcutSize = Size.Empty;
            this.Initialize();
        }

        internal ToolStripMenuItem(Form mdiForm)
        {
            this.showShortcutKeys = true;
            this.nativeMenuCommandID = -1;
            this.targetWindowHandle = IntPtr.Zero;
            this.nativeMenuHandle = IntPtr.Zero;
            this.cachedShortcutSize = Size.Empty;
            this.Initialize();
            base.Properties.SetObject(PropMdiForm, mdiForm);
        }

        public ToolStripMenuItem(string text, Image image) : base(text, image, (EventHandler) null)
        {
            this.showShortcutKeys = true;
            this.nativeMenuCommandID = -1;
            this.targetWindowHandle = IntPtr.Zero;
            this.nativeMenuHandle = IntPtr.Zero;
            this.cachedShortcutSize = Size.Empty;
            this.Initialize();
        }

        internal ToolStripMenuItem(IntPtr hMenu, int nativeMenuCommandId, IWin32Window targetWindow)
        {
            this.showShortcutKeys = true;
            this.nativeMenuCommandID = -1;
            this.targetWindowHandle = IntPtr.Zero;
            this.nativeMenuHandle = IntPtr.Zero;
            this.cachedShortcutSize = Size.Empty;
            this.Initialize();
            this.Overflow = ToolStripItemOverflow.Never;
            this.nativeMenuCommandID = nativeMenuCommandId;
            this.targetWindowHandle = Control.GetSafeHandle(targetWindow);
            this.nativeMenuHandle = hMenu;
            this.Image = this.GetNativeMenuItemImage();
            base.ImageScaling = ToolStripItemImageScaling.None;
            string nativeMenuItemTextAndShortcut = this.GetNativeMenuItemTextAndShortcut();
            if (nativeMenuItemTextAndShortcut != null)
            {
                string[] strArray = nativeMenuItemTextAndShortcut.Split(new char[] { '\t' });
                if (strArray.Length >= 1)
                {
                    this.Text = strArray[0];
                }
                if (strArray.Length >= 2)
                {
                    this.ShowShortcutKeys = true;
                    this.ShortcutKeyDisplayString = strArray[1];
                }
            }
        }

        public ToolStripMenuItem(string text, Image image, EventHandler onClick) : base(text, image, onClick)
        {
            this.showShortcutKeys = true;
            this.nativeMenuCommandID = -1;
            this.targetWindowHandle = IntPtr.Zero;
            this.nativeMenuHandle = IntPtr.Zero;
            this.cachedShortcutSize = Size.Empty;
            this.Initialize();
        }

        public ToolStripMenuItem(string text, Image image, params ToolStripItem[] dropDownItems) : base(text, image, dropDownItems)
        {
            this.showShortcutKeys = true;
            this.nativeMenuCommandID = -1;
            this.targetWindowHandle = IntPtr.Zero;
            this.nativeMenuHandle = IntPtr.Zero;
            this.cachedShortcutSize = Size.Empty;
            this.Initialize();
        }

        public ToolStripMenuItem(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name)
        {
            this.showShortcutKeys = true;
            this.nativeMenuCommandID = -1;
            this.targetWindowHandle = IntPtr.Zero;
            this.nativeMenuHandle = IntPtr.Zero;
            this.cachedShortcutSize = Size.Empty;
            this.Initialize();
        }

        public ToolStripMenuItem(string text, Image image, EventHandler onClick, Keys shortcutKeys) : base(text, image, onClick)
        {
            this.showShortcutKeys = true;
            this.nativeMenuCommandID = -1;
            this.targetWindowHandle = IntPtr.Zero;
            this.nativeMenuHandle = IntPtr.Zero;
            this.cachedShortcutSize = Size.Empty;
            this.Initialize();
            this.ShortcutKeys = shortcutKeys;
        }

        internal override void AutoHide(ToolStripItem otherItemBeingSelected)
        {
            if (base.IsOnDropDown)
            {
                MenuTimer.Transition(this, otherItemBeingSelected as ToolStripMenuItem);
            }
            else
            {
                base.AutoHide(otherItemBeingSelected);
            }
        }

        private void ClearShortcutCache()
        {
            this.cachedShortcutSize = Size.Empty;
            this.cachedShortcutText = null;
        }

        internal ToolStripMenuItem Clone()
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Events.AddHandlers(base.Events);
            item.AccessibleName = base.AccessibleName;
            item.AccessibleRole = base.AccessibleRole;
            item.Alignment = base.Alignment;
            item.AllowDrop = this.AllowDrop;
            item.Anchor = base.Anchor;
            item.AutoSize = base.AutoSize;
            item.AutoToolTip = base.AutoToolTip;
            item.BackColor = this.BackColor;
            item.BackgroundImage = this.BackgroundImage;
            item.BackgroundImageLayout = this.BackgroundImageLayout;
            item.Checked = this.Checked;
            item.CheckOnClick = this.CheckOnClick;
            item.CheckState = this.CheckState;
            item.DisplayStyle = this.DisplayStyle;
            item.Dock = base.Dock;
            item.DoubleClickEnabled = base.DoubleClickEnabled;
            item.Enabled = this.Enabled;
            item.Font = this.Font;
            item.ForeColor = this.ForeColor;
            item.Image = this.Image;
            item.ImageAlign = base.ImageAlign;
            item.ImageScaling = base.ImageScaling;
            item.ImageTransparentColor = base.ImageTransparentColor;
            item.Margin = base.Margin;
            item.MergeAction = base.MergeAction;
            item.MergeIndex = base.MergeIndex;
            item.Name = base.Name;
            item.Overflow = this.Overflow;
            item.Padding = this.Padding;
            item.RightToLeft = this.RightToLeft;
            item.ShortcutKeys = this.ShortcutKeys;
            item.ShowShortcutKeys = this.ShowShortcutKeys;
            item.Tag = base.Tag;
            item.Text = this.Text;
            item.TextAlign = this.TextAlign;
            item.TextDirection = this.TextDirection;
            item.TextImageRelation = base.TextImageRelation;
            item.ToolTipText = base.ToolTipText;
            item.Visible = this.ParticipatesInLayout;
            if (!base.AutoSize)
            {
                item.Size = this.Size;
            }
            return item;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new ToolStripMenuItemAccessibleObject(this);
        }

        protected override ToolStripDropDown CreateDefaultDropDown()
        {
            return new ToolStripDropDownMenu(this, true);
        }

        internal override ToolStripItemInternalLayout CreateInternalLayout()
        {
            return new ToolStripMenuItemInternalLayout(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.lastOwner != null))
            {
                Keys shortcutKeys = this.ShortcutKeys;
                if ((shortcutKeys != Keys.None) && this.lastOwner.Shortcuts.ContainsKey(shortcutKeys))
                {
                    this.lastOwner.Shortcuts.Remove(shortcutKeys);
                }
                this.lastOwner = null;
                if (this.MdiForm != null)
                {
                    base.Properties.SetObject(PropMdiForm, null);
                }
            }
            base.Dispose(disposing);
        }

        private bool GetNativeMenuItemEnabled()
        {
            if ((this.nativeMenuCommandID == -1) || (this.nativeMenuHandle == IntPtr.Zero))
            {
                return false;
            }
            System.Windows.Forms.NativeMethods.MENUITEMINFO_T_RW lpmii = new System.Windows.Forms.NativeMethods.MENUITEMINFO_T_RW {
                cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.MENUITEMINFO_T_RW)),
                fMask = 1,
                fType = 1,
                wID = this.nativeMenuCommandID
            };
            System.Windows.Forms.UnsafeNativeMethods.GetMenuItemInfo(new HandleRef(this, this.nativeMenuHandle), this.nativeMenuCommandID, false, lpmii);
            return ((lpmii.fState & 3) == 0);
        }

        private Image GetNativeMenuItemImage()
        {
            if ((this.nativeMenuCommandID == -1) || (this.nativeMenuHandle == IntPtr.Zero))
            {
                return null;
            }
            System.Windows.Forms.NativeMethods.MENUITEMINFO_T_RW lpmii = new System.Windows.Forms.NativeMethods.MENUITEMINFO_T_RW {
                fMask = 0x80,
                fType = 0x80,
                wID = this.nativeMenuCommandID
            };
            System.Windows.Forms.UnsafeNativeMethods.GetMenuItemInfo(new HandleRef(this, this.nativeMenuHandle), this.nativeMenuCommandID, false, lpmii);
            if ((lpmii.hbmpItem != IntPtr.Zero) && (lpmii.hbmpItem.ToInt32() > 11))
            {
                return Image.FromHbitmap(lpmii.hbmpItem);
            }
            int num = -1;
            switch (lpmii.hbmpItem.ToInt32())
            {
                case 2:
                case 9:
                    num = 3;
                    break;

                case 3:
                case 7:
                case 11:
                    num = 1;
                    break;

                case 5:
                case 6:
                case 8:
                    num = 0;
                    break;

                case 10:
                    num = 2;
                    break;
            }
            if (num <= -1)
            {
                return null;
            }
            Bitmap image = new Bitmap(0x10, 0x10);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                ControlPaint.DrawCaptionButton(graphics, new Rectangle(Point.Empty, image.Size), (CaptionButton) num, ButtonState.Flat);
                graphics.DrawRectangle(SystemPens.Control, 0, 0, image.Width - 1, image.Height - 1);
            }
            image.MakeTransparent(SystemColors.Control);
            return image;
        }

        private string GetNativeMenuItemTextAndShortcut()
        {
            if ((this.nativeMenuCommandID == -1) || (this.nativeMenuHandle == IntPtr.Zero))
            {
                return null;
            }
            string str = null;
            System.Windows.Forms.NativeMethods.MENUITEMINFO_T_RW lpmii = new System.Windows.Forms.NativeMethods.MENUITEMINFO_T_RW {
                fMask = 0x40,
                fType = 0x40,
                wID = this.nativeMenuCommandID,
                dwTypeData = IntPtr.Zero
            };
            System.Windows.Forms.UnsafeNativeMethods.GetMenuItemInfo(new HandleRef(this, this.nativeMenuHandle), this.nativeMenuCommandID, false, lpmii);
            if (lpmii.cch > 0)
            {
                lpmii.cch++;
                lpmii.wID = this.nativeMenuCommandID;
                IntPtr ptr = Marshal.AllocCoTaskMem(lpmii.cch * Marshal.SystemDefaultCharSize);
                lpmii.dwTypeData = ptr;
                try
                {
                    System.Windows.Forms.UnsafeNativeMethods.GetMenuItemInfo(new HandleRef(this, this.nativeMenuHandle), this.nativeMenuCommandID, false, lpmii);
                    if (lpmii.dwTypeData != IntPtr.Zero)
                    {
                        str = Marshal.PtrToStringAuto(lpmii.dwTypeData, lpmii.cch);
                    }
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(ptr);
                    }
                }
            }
            return str;
        }

        private string GetShortcutText()
        {
            if (this.cachedShortcutText == null)
            {
                this.cachedShortcutText = ShortcutToText(this.ShortcutKeys, this.ShortcutKeyDisplayString);
            }
            return this.cachedShortcutText;
        }

        internal Size GetShortcutTextSize()
        {
            if (!this.ShowShortcutKeys)
            {
                return Size.Empty;
            }
            string shortcutText = this.GetShortcutText();
            if (string.IsNullOrEmpty(shortcutText))
            {
                return Size.Empty;
            }
            if (this.cachedShortcutSize == Size.Empty)
            {
                this.cachedShortcutSize = TextRenderer.MeasureText(shortcutText, this.Font);
            }
            return this.cachedShortcutSize;
        }

        internal void HandleAutoExpansion()
        {
            if ((this.Enabled && (base.ParentInternal != null)) && (base.ParentInternal.MenuAutoExpand && this.HasDropDownItems))
            {
                base.ShowDropDown();
                base.DropDown.SelectNextToolStripItem(null, true);
            }
        }

        private void Initialize()
        {
            this.Overflow = ToolStripItemOverflow.Never;
            base.MouseDownAndUpMustBeInSameItem = false;
            base.SupportsDisabledHotTracking = true;
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventCheckedChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCheckStateChanged(EventArgs e)
        {
            base.AccessibilityNotifyClients(AccessibleEvents.StateChange);
            EventHandler handler = (EventHandler) base.Events[EventCheckStateChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.checkOnClick)
            {
                this.Checked = !this.Checked;
            }
            base.OnClick(e);
            if (this.nativeMenuCommandID != -1)
            {
                if ((this.nativeMenuCommandID & 0xf000) != 0)
                {
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, this.targetWindowHandle), 0x112, this.nativeMenuCommandID, 0);
                }
                else
                {
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, this.targetWindowHandle), 0x111, this.nativeMenuCommandID, 0);
                }
                base.Invalidate();
            }
        }

        protected override void OnDropDownHide(EventArgs e)
        {
            MenuTimer.Cancel(this);
            base.OnDropDownHide(e);
        }

        protected override void OnDropDownShow(EventArgs e)
        {
            MenuTimer.Cancel(this);
            if (base.ParentInternal != null)
            {
                base.ParentInternal.MenuAutoExpand = true;
            }
            base.OnDropDownShow(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            this.ClearShortcutCache();
            base.OnFontChanged(e);
        }

        internal void OnMenuAutoExpand()
        {
            base.ShowDropDown();
        }

        private void OnMouseButtonStateChange(MouseEventArgs e, bool isMouseDown)
        {
            bool flag = true;
            if (base.IsOnDropDown)
            {
                ToolStripDropDown currentParentDropDown = base.GetCurrentParentDropDown();
                base.SupportsRightClick = currentParentDropDown.GetFirstDropDown() is ContextMenuStrip;
            }
            else
            {
                flag = !base.DropDown.Visible;
                base.SupportsRightClick = false;
            }
            if ((e.Button == MouseButtons.Left) || ((e.Button == MouseButtons.Right) && base.SupportsRightClick))
            {
                if (isMouseDown && flag)
                {
                    this.openMouseId = (base.ParentInternal == null) ? ((byte) 0) : base.ParentInternal.GetMouseId();
                    base.ShowDropDown(true);
                }
                else if (!isMouseDown && !flag)
                {
                    byte num = (base.ParentInternal == null) ? ((byte) 0) : base.ParentInternal.GetMouseId();
                    int openMouseId = this.openMouseId;
                    if (num != openMouseId)
                    {
                        this.openMouseId = 0;
                        ToolStripManager.ModalMenuFilter.CloseActiveDropDown(base.DropDown, ToolStripDropDownCloseReason.AppClicked);
                        base.Select();
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            MenuTimer.Cancel(this);
            this.OnMouseButtonStateChange(e, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (((base.ParentInternal != null) && base.ParentInternal.MenuAutoExpand) && this.Selected)
            {
                MenuTimer.Cancel(this);
                MenuTimer.Start(this);
            }
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            MenuTimer.Cancel(this);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.OnMouseButtonStateChange(e, false);
            base.OnMouseUp(e);
        }

        protected override void OnOwnerChanged(EventArgs e)
        {
            Keys shortcutKeys = this.ShortcutKeys;
            if (shortcutKeys != Keys.None)
            {
                if (this.lastOwner != null)
                {
                    this.lastOwner.Shortcuts.Remove(shortcutKeys);
                }
                if (base.Owner != null)
                {
                    if (base.Owner.Shortcuts.Contains(shortcutKeys))
                    {
                        base.Owner.Shortcuts[shortcutKeys] = this;
                    }
                    else
                    {
                        base.Owner.Shortcuts.Add(shortcutKeys, this);
                    }
                    this.lastOwner = base.Owner;
                }
            }
            base.OnOwnerChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (base.Owner != null)
            {
                ToolStripRenderer renderer = base.Renderer;
                Graphics g = e.Graphics;
                renderer.DrawMenuItemBackground(new ToolStripItemRenderEventArgs(g, this));
                Color menuText = SystemColors.MenuText;
                if (base.IsForeColorSet)
                {
                    menuText = this.ForeColor;
                }
                else if (!this.IsTopLevel || ToolStripManager.VisualStylesEnabled)
                {
                    if (this.Selected || this.Pressed)
                    {
                        menuText = SystemColors.HighlightText;
                    }
                    else
                    {
                        menuText = SystemColors.MenuText;
                    }
                }
                bool flag = this.RightToLeft == RightToLeft.Yes;
                ToolStripMenuItemInternalLayout internalLayout = base.InternalLayout as ToolStripMenuItemInternalLayout;
                if ((internalLayout != null) && internalLayout.UseMenuLayout)
                {
                    if ((this.CheckState != System.Windows.Forms.CheckState.Unchecked) && internalLayout.PaintCheck)
                    {
                        Rectangle checkRectangle = internalLayout.CheckRectangle;
                        if (!internalLayout.ShowCheckMargin)
                        {
                            checkRectangle = internalLayout.ImageRectangle;
                        }
                        if (checkRectangle.Width != 0)
                        {
                            renderer.DrawItemCheck(new ToolStripItemImageRenderEventArgs(g, this, this.CheckedImage, checkRectangle));
                        }
                    }
                    if ((this.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text)
                    {
                        renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(g, this, this.Text, base.InternalLayout.TextRectangle, menuText, this.Font, flag ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft));
                        bool showShortcutKeys = this.ShowShortcutKeys;
                        if (!base.DesignMode)
                        {
                            showShortcutKeys = showShortcutKeys && !this.HasDropDownItems;
                        }
                        if (showShortcutKeys)
                        {
                            renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(g, this, this.GetShortcutText(), base.InternalLayout.TextRectangle, menuText, this.Font, flag ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleRight));
                        }
                    }
                    if (this.HasDropDownItems)
                    {
                        ArrowDirection arrowDirection = flag ? ArrowDirection.Left : ArrowDirection.Right;
                        Color arrowColor = (this.Selected || this.Pressed) ? SystemColors.HighlightText : SystemColors.MenuText;
                        arrowColor = this.Enabled ? arrowColor : SystemColors.ControlDark;
                        renderer.DrawArrow(new ToolStripArrowRenderEventArgs(g, this, internalLayout.ArrowRectangle, arrowColor, arrowDirection));
                    }
                    if ((internalLayout.PaintImage && ((this.DisplayStyle & ToolStripItemDisplayStyle.Image) == ToolStripItemDisplayStyle.Image)) && (this.Image != null))
                    {
                        renderer.DrawItemImage(new ToolStripItemImageRenderEventArgs(g, this, base.InternalLayout.ImageRectangle));
                    }
                }
                else
                {
                    if ((this.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text)
                    {
                        renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(g, this, this.Text, base.InternalLayout.TextRectangle, menuText, this.Font, base.InternalLayout.TextFormat));
                    }
                    if (((this.DisplayStyle & ToolStripItemDisplayStyle.Image) == ToolStripItemDisplayStyle.Image) && (this.Image != null))
                    {
                        renderer.DrawItemImage(new ToolStripItemImageRenderEventArgs(g, this, base.InternalLayout.ImageRectangle));
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected internal override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            if ((this.Enabled && (this.ShortcutKeys == keyData)) && !this.HasDropDownItems)
            {
                base.FireEvent(ToolStripItemEventType.Click);
                return true;
            }
            return base.ProcessCmdKey(ref m, keyData);
        }

        [UIPermission(SecurityAction.LinkDemand, Window=UIPermissionWindow.AllWindows)]
        protected internal override bool ProcessMnemonic(char charCode)
        {
            if (this.HasDropDownItems)
            {
                base.Select();
                base.ShowDropDown();
                base.DropDown.SelectNextToolStripItem(null, true);
                return true;
            }
            return base.ProcessMnemonic(charCode);
        }

        protected internal override void SetBounds(Rectangle rect)
        {
            ToolStripMenuItemInternalLayout internalLayout = base.InternalLayout as ToolStripMenuItemInternalLayout;
            if ((internalLayout != null) && internalLayout.UseMenuLayout)
            {
                ToolStripDropDownMenu owner = base.Owner as ToolStripDropDownMenu;
                if (owner != null)
                {
                    rect.X -= owner.Padding.Left;
                    rect.X = Math.Max(rect.X, 0);
                }
            }
            base.SetBounds(rect);
        }

        internal void SetNativeTargetMenu(IntPtr hMenu)
        {
            this.nativeMenuHandle = hMenu;
        }

        internal void SetNativeTargetWindow(IWin32Window window)
        {
            this.targetWindowHandle = Control.GetSafeHandle(window);
        }

        internal static string ShortcutToText(Keys shortcutKeys, string shortcutKeyDisplayString)
        {
            if (!string.IsNullOrEmpty(shortcutKeyDisplayString))
            {
                return shortcutKeyDisplayString;
            }
            if (shortcutKeys == Keys.None)
            {
                return string.Empty;
            }
            return TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString(shortcutKeys);
        }

        [Bindable(true), System.Windows.Forms.SRCategory("CatAppearance"), RefreshProperties(RefreshProperties.All), System.Windows.Forms.SRDescription("CheckBoxCheckedDescr"), DefaultValue(false)]
        public bool Checked
        {
            get
            {
                return (this.CheckState != System.Windows.Forms.CheckState.Unchecked);
            }
            set
            {
                if (value != this.Checked)
                {
                    this.CheckState = value ? System.Windows.Forms.CheckState.Checked : System.Windows.Forms.CheckState.Unchecked;
                    base.InvokePaint();
                }
            }
        }

        internal Image CheckedImage
        {
            get
            {
                System.Windows.Forms.CheckState checkState = this.CheckState;
                if (checkState == System.Windows.Forms.CheckState.Indeterminate)
                {
                    if (indeterminateCheckedImage == null)
                    {
                        Bitmap bitmap = new Bitmap(typeof(ToolStripMenuItem), "IndeterminateChecked.bmp");
                        if (bitmap != null)
                        {
                            bitmap.MakeTransparent(bitmap.GetPixel(1, 1));
                            indeterminateCheckedImage = bitmap;
                        }
                    }
                    return indeterminateCheckedImage;
                }
                if (checkState != System.Windows.Forms.CheckState.Checked)
                {
                    return null;
                }
                if (checkedImage == null)
                {
                    Bitmap bitmap2 = new Bitmap(typeof(ToolStripMenuItem), "Checked.bmp");
                    if (bitmap2 != null)
                    {
                        bitmap2.MakeTransparent(bitmap2.GetPixel(1, 1));
                        checkedImage = bitmap2;
                    }
                }
                return checkedImage;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ToolStripButtonCheckOnClickDescr")]
        public bool CheckOnClick
        {
            get
            {
                return this.checkOnClick;
            }
            set
            {
                this.checkOnClick = value;
            }
        }

        [Bindable(true), System.Windows.Forms.SRDescription("CheckBoxCheckStateDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0), RefreshProperties(RefreshProperties.All)]
        public System.Windows.Forms.CheckState CheckState
        {
            get
            {
                bool found = false;
                object integer = base.Properties.GetInteger(PropCheckState, out found);
                if (!found)
                {
                    return System.Windows.Forms.CheckState.Unchecked;
                }
                return (System.Windows.Forms.CheckState) integer;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.CheckState));
                }
                if (value != this.CheckState)
                {
                    base.Properties.SetInteger(PropCheckState, (int) value);
                    this.OnCheckedChanged(EventArgs.Empty);
                    this.OnCheckStateChanged(EventArgs.Empty);
                }
            }
        }

        protected internal override Padding DefaultMargin
        {
            get
            {
                return Padding.Empty;
            }
        }

        protected override Padding DefaultPadding
        {
            get
            {
                if (base.IsOnDropDown)
                {
                    return new Padding(0, 1, 0, 1);
                }
                return new Padding(4, 0, 4, 0);
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(0x20, 0x13);
            }
        }

        public override bool Enabled
        {
            get
            {
                if (this.nativeMenuCommandID == -1)
                {
                    return base.Enabled;
                }
                return (((base.Enabled && (this.nativeMenuHandle != IntPtr.Zero)) && (this.targetWindowHandle != IntPtr.Zero)) && this.GetNativeMenuItemEnabled());
            }
            set
            {
                base.Enabled = value;
            }
        }

        [Browsable(false)]
        public bool IsMdiWindowListEntry
        {
            get
            {
                return (this.MdiForm != null);
            }
        }

        internal bool IsTopLevel
        {
            get
            {
                return !(base.ParentInternal is ToolStripDropDown);
            }
        }

        internal Form MdiForm
        {
            get
            {
                if (base.Properties.ContainsObject(PropMdiForm))
                {
                    return (base.Properties.GetObject(PropMdiForm) as Form);
                }
                return null;
            }
        }

        internal static System.Windows.Forms.MenuTimer MenuTimer
        {
            get
            {
                return menuTimer;
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRDescription("ToolStripItemOverflowDescr"), System.Windows.Forms.SRCategory("CatLayout")]
        public ToolStripItemOverflow Overflow
        {
            get
            {
                return base.Overflow;
            }
            set
            {
                base.Overflow = value;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripMenuItemShortcutKeyDisplayStringDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue((string) null), Localizable(true)]
        public string ShortcutKeyDisplayString
        {
            get
            {
                return this.shortcutKeyDisplayString;
            }
            set
            {
                if (this.shortcutKeyDisplayString != value)
                {
                    this.shortcutKeyDisplayString = value;
                    this.ClearShortcutCache();
                    if (this.ShowShortcutKeys)
                    {
                        ToolStripDropDown parentInternal = base.ParentInternal as ToolStripDropDown;
                        if (parentInternal != null)
                        {
                            LayoutTransaction.DoLayout(parentInternal, this, "ShortcutKeyDisplayString");
                            parentInternal.AdjustSize();
                        }
                    }
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("MenuItemShortCutDescr"), DefaultValue(0)]
        public Keys ShortcutKeys
        {
            get
            {
                bool found = false;
                object integer = base.Properties.GetInteger(PropShortcutKeys, out found);
                if (!found)
                {
                    return Keys.None;
                }
                return (Keys) integer;
            }
            set
            {
                if ((value != Keys.None) && !ToolStripManager.IsValidShortcut(value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(Keys));
                }
                Keys shortcutKeys = this.ShortcutKeys;
                if (shortcutKeys != value)
                {
                    this.ClearShortcutCache();
                    ToolStrip owner = base.Owner;
                    if (owner != null)
                    {
                        if (shortcutKeys != Keys.None)
                        {
                            owner.Shortcuts.Remove(shortcutKeys);
                        }
                        if (owner.Shortcuts.Contains(value))
                        {
                            owner.Shortcuts[value] = this;
                        }
                        else
                        {
                            owner.Shortcuts.Add(value, this);
                        }
                    }
                    base.Properties.SetInteger(PropShortcutKeys, (int) value);
                    if (this.ShowShortcutKeys && base.IsOnDropDown)
                    {
                        ToolStripDropDownMenu currentParentDropDown = base.GetCurrentParentDropDown() as ToolStripDropDownMenu;
                        if (currentParentDropDown != null)
                        {
                            LayoutTransaction.DoLayout(base.ParentInternal, this, "ShortcutKeys");
                            currentParentDropDown.AdjustSize();
                        }
                    }
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("MenuItemShowShortCutDescr"), DefaultValue(true)]
        public bool ShowShortcutKeys
        {
            get
            {
                return this.showShortcutKeys;
            }
            set
            {
                if (value != this.showShortcutKeys)
                {
                    this.ClearShortcutCache();
                    this.showShortcutKeys = value;
                    ToolStripDropDown parentInternal = base.ParentInternal as ToolStripDropDown;
                    if (parentInternal != null)
                    {
                        LayoutTransaction.DoLayout(parentInternal, this, "ShortcutKeys");
                        parentInternal.AdjustSize();
                    }
                }
            }
        }

        [ComVisible(true)]
        internal class ToolStripMenuItemAccessibleObject : ToolStripDropDownItemAccessibleObject
        {
            private ToolStripMenuItem ownerItem;

            public ToolStripMenuItemAccessibleObject(ToolStripMenuItem ownerItem) : base(ownerItem)
            {
                this.ownerItem = ownerItem;
            }

            public override AccessibleStates State
            {
                get
                {
                    if (!this.ownerItem.Enabled)
                    {
                        return base.State;
                    }
                    AccessibleStates state = base.State;
                    if ((state & AccessibleStates.Pressed) == AccessibleStates.Pressed)
                    {
                        state &= ~AccessibleStates.Pressed;
                    }
                    if (this.ownerItem.Checked)
                    {
                        state |= AccessibleStates.Checked;
                    }
                    return state;
                }
            }
        }
    }
}

