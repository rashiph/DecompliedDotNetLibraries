namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Designer("System.Windows.Forms.Design.NotifyIconDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ToolboxItemFilter("System.Windows.Forms"), System.Windows.Forms.SRDescription("DescriptionNotifyIcon"), DefaultProperty("Text"), DefaultEvent("MouseDoubleClick")]
    public sealed class NotifyIcon : Component
    {
        private bool added;
        private ToolTipIcon balloonTipIcon;
        private string balloonTipText;
        private string balloonTipTitle;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private bool doubleClick;
        private static readonly object EVENT_BALLOONTIPCLICKED = new object();
        private static readonly object EVENT_BALLOONTIPCLOSED = new object();
        private static readonly object EVENT_BALLOONTIPSHOWN = new object();
        private static readonly object EVENT_CLICK = new object();
        private static readonly object EVENT_DOUBLECLICK = new object();
        private static readonly object EVENT_MOUSECLICK = new object();
        private static readonly object EVENT_MOUSEDOUBLECLICK = new object();
        private static readonly object EVENT_MOUSEDOWN = new object();
        private static readonly object EVENT_MOUSEMOVE = new object();
        private static readonly object EVENT_MOUSEUP = new object();
        private System.Drawing.Icon icon;
        private int id;
        private static int nextId = 0;
        private object syncObj;
        private string text;
        private object userData;
        private bool visible;
        private NotifyIconNativeWindow window;
        private static int WM_TASKBARCREATED = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage("TaskbarCreated");
        private const int WM_TRAYMOUSEMESSAGE = 0x800;

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("NotifyIconOnBalloonTipClickedDescr")]
        public event EventHandler BalloonTipClicked
        {
            add
            {
                base.Events.AddHandler(EVENT_BALLOONTIPCLICKED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_BALLOONTIPCLICKED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("NotifyIconOnBalloonTipClosedDescr")]
        public event EventHandler BalloonTipClosed
        {
            add
            {
                base.Events.AddHandler(EVENT_BALLOONTIPCLOSED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_BALLOONTIPCLOSED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("NotifyIconOnBalloonTipShownDescr")]
        public event EventHandler BalloonTipShown
        {
            add
            {
                base.Events.AddHandler(EVENT_BALLOONTIPSHOWN, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_BALLOONTIPSHOWN, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnClickDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event EventHandler Click
        {
            add
            {
                base.Events.AddHandler(EVENT_CLICK, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_CLICK, value);
            }
        }

        [System.Windows.Forms.SRDescription("ControlOnDoubleClickDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event EventHandler DoubleClick
        {
            add
            {
                base.Events.AddHandler(EVENT_DOUBLECLICK, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DOUBLECLICK, value);
            }
        }

        [System.Windows.Forms.SRDescription("NotifyIconMouseClickDescr"), System.Windows.Forms.SRCategory("CatAction")]
        public event MouseEventHandler MouseClick
        {
            add
            {
                base.Events.AddHandler(EVENT_MOUSECLICK, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MOUSECLICK, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatAction"), System.Windows.Forms.SRDescription("NotifyIconMouseDoubleClickDescr")]
        public event MouseEventHandler MouseDoubleClick
        {
            add
            {
                base.Events.AddHandler(EVENT_MOUSEDOUBLECLICK, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MOUSEDOUBLECLICK, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatMouse"), System.Windows.Forms.SRDescription("ControlOnMouseDownDescr")]
        public event MouseEventHandler MouseDown
        {
            add
            {
                base.Events.AddHandler(EVENT_MOUSEDOWN, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MOUSEDOWN, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatMouse"), System.Windows.Forms.SRDescription("ControlOnMouseMoveDescr")]
        public event MouseEventHandler MouseMove
        {
            add
            {
                base.Events.AddHandler(EVENT_MOUSEMOVE, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MOUSEMOVE, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatMouse"), System.Windows.Forms.SRDescription("ControlOnMouseUpDescr")]
        public event MouseEventHandler MouseUp
        {
            add
            {
                base.Events.AddHandler(EVENT_MOUSEUP, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_MOUSEUP, value);
            }
        }

        public NotifyIcon()
        {
            this.syncObj = new object();
            this.text = "";
            this.balloonTipText = "";
            this.balloonTipTitle = "";
            this.id = ++nextId;
            this.window = new NotifyIconNativeWindow(this);
            this.UpdateIcon(this.visible);
        }

        public NotifyIcon(IContainer container) : this()
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            container.Add(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.window != null)
                {
                    this.icon = null;
                    this.Text = string.Empty;
                    this.UpdateIcon(false);
                    this.window.DestroyHandle();
                    this.window = null;
                    this.contextMenu = null;
                    this.contextMenuStrip = null;
                }
            }
            else if ((this.window != null) && (this.window.Handle != IntPtr.Zero))
            {
                System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this.window, this.window.Handle), 0x10, 0, 0);
                this.window.ReleaseHandle();
            }
            base.Dispose(disposing);
        }

        private void OnBalloonTipClicked()
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_BALLOONTIPCLICKED];
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnBalloonTipClosed()
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_BALLOONTIPCLOSED];
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnBalloonTipShown()
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_BALLOONTIPSHOWN];
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnClick(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_CLICK];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnDoubleClick(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EVENT_DOUBLECLICK];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnMouseClick(MouseEventArgs mea)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[EVENT_MOUSECLICK];
            if (handler != null)
            {
                handler(this, mea);
            }
        }

        private void OnMouseDoubleClick(MouseEventArgs mea)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[EVENT_MOUSEDOUBLECLICK];
            if (handler != null)
            {
                handler(this, mea);
            }
        }

        private void OnMouseDown(MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[EVENT_MOUSEDOWN];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnMouseMove(MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[EVENT_MOUSEMOVE];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnMouseUp(MouseEventArgs e)
        {
            MouseEventHandler handler = (MouseEventHandler) base.Events[EVENT_MOUSEUP];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void ShowBalloonTip(int timeout)
        {
            this.ShowBalloonTip(timeout, this.balloonTipTitle, this.balloonTipText, this.balloonTipIcon);
        }

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
        {
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException("timeout", System.Windows.Forms.SR.GetString("InvalidArgument", new object[] { "timeout", timeout.ToString(CultureInfo.CurrentCulture) }));
            }
            if (string.IsNullOrEmpty(tipText))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("NotifyIconEmptyOrNullTipText"));
            }
            if (!System.Windows.Forms.ClientUtils.IsEnumValid(tipIcon, (int) tipIcon, 0, 3))
            {
                throw new InvalidEnumArgumentException("tipIcon", (int) tipIcon, typeof(ToolTipIcon));
            }
            if (this.added && !base.DesignMode)
            {
                System.Windows.Forms.IntSecurity.UnrestrictedWindows.Demand();
                System.Windows.Forms.NativeMethods.NOTIFYICONDATA pnid = new System.Windows.Forms.NativeMethods.NOTIFYICONDATA();
                if (this.window.Handle == IntPtr.Zero)
                {
                    this.window.CreateHandle(new CreateParams());
                }
                pnid.hWnd = this.window.Handle;
                pnid.uID = this.id;
                pnid.uFlags = 0x10;
                pnid.uTimeoutOrVersion = timeout;
                pnid.szInfoTitle = tipTitle;
                pnid.szInfo = tipText;
                switch (tipIcon)
                {
                    case ToolTipIcon.None:
                        pnid.dwInfoFlags = 0;
                        break;

                    case ToolTipIcon.Info:
                        pnid.dwInfoFlags = 1;
                        break;

                    case ToolTipIcon.Warning:
                        pnid.dwInfoFlags = 2;
                        break;

                    case ToolTipIcon.Error:
                        pnid.dwInfoFlags = 3;
                        break;
                }
                System.Windows.Forms.UnsafeNativeMethods.Shell_NotifyIcon(1, pnid);
            }
        }

        private void ShowContextMenu()
        {
            if ((this.contextMenu != null) || (this.contextMenuStrip != null))
            {
                System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT();
                System.Windows.Forms.UnsafeNativeMethods.GetCursorPos(pt);
                System.Windows.Forms.UnsafeNativeMethods.SetForegroundWindow(new HandleRef(this.window, this.window.Handle));
                if (this.contextMenu != null)
                {
                    this.contextMenu.OnPopup(EventArgs.Empty);
                    System.Windows.Forms.SafeNativeMethods.TrackPopupMenuEx(new HandleRef(this.contextMenu, this.contextMenu.Handle), 0x48, pt.x, pt.y, new HandleRef(this.window, this.window.Handle), null);
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this.window, this.window.Handle), 0, IntPtr.Zero, IntPtr.Zero);
                }
                else if (this.contextMenuStrip != null)
                {
                    this.contextMenuStrip.ShowInTaskbar(pt.x, pt.y);
                }
            }
        }

        private void UpdateIcon(bool showIconInTray)
        {
            lock (this.syncObj)
            {
                if (!base.DesignMode)
                {
                    System.Windows.Forms.IntSecurity.UnrestrictedWindows.Demand();
                    this.window.LockReference(showIconInTray);
                    System.Windows.Forms.NativeMethods.NOTIFYICONDATA pnid = new System.Windows.Forms.NativeMethods.NOTIFYICONDATA {
                        uCallbackMessage = 0x800,
                        uFlags = 1
                    };
                    if (showIconInTray && (this.window.Handle == IntPtr.Zero))
                    {
                        this.window.CreateHandle(new CreateParams());
                    }
                    pnid.hWnd = this.window.Handle;
                    pnid.uID = this.id;
                    pnid.hIcon = IntPtr.Zero;
                    pnid.szTip = null;
                    if (this.icon != null)
                    {
                        pnid.uFlags |= 2;
                        pnid.hIcon = this.icon.Handle;
                    }
                    pnid.uFlags |= 4;
                    pnid.szTip = this.text;
                    if (showIconInTray && (this.icon != null))
                    {
                        if (!this.added)
                        {
                            System.Windows.Forms.UnsafeNativeMethods.Shell_NotifyIcon(0, pnid);
                            this.added = true;
                        }
                        else
                        {
                            System.Windows.Forms.UnsafeNativeMethods.Shell_NotifyIcon(1, pnid);
                        }
                    }
                    else if (this.added)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.Shell_NotifyIcon(2, pnid);
                        this.added = false;
                    }
                }
            }
        }

        private void WmDrawItemMenuItem(ref Message m)
        {
            System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.DRAWITEMSTRUCT));
            MenuItem menuItemFromItemData = MenuItem.GetMenuItemFromItemData(lParam.itemData);
            if (menuItemFromItemData != null)
            {
                menuItemFromItemData.WmDrawItem(ref m);
            }
        }

        private void WmInitMenuPopup(ref Message m)
        {
            if ((this.contextMenu == null) || !this.contextMenu.ProcessInitMenuPopup(m.WParam))
            {
                this.window.DefWndProc(ref m);
            }
        }

        private void WmMeasureMenuItem(ref Message m)
        {
            System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT lParam = (System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT) m.GetLParam(typeof(System.Windows.Forms.NativeMethods.MEASUREITEMSTRUCT));
            MenuItem menuItemFromItemData = MenuItem.GetMenuItemFromItemData(lParam.itemData);
            if (menuItemFromItemData != null)
            {
                menuItemFromItemData.WmMeasureItem(ref m);
            }
        }

        private void WmMouseDown(ref Message m, MouseButtons button, int clicks)
        {
            if (clicks == 2)
            {
                this.OnDoubleClick(new MouseEventArgs(button, 2, 0, 0, 0));
                this.OnMouseDoubleClick(new MouseEventArgs(button, 2, 0, 0, 0));
                this.doubleClick = true;
            }
            this.OnMouseDown(new MouseEventArgs(button, clicks, 0, 0, 0));
        }

        private void WmMouseMove(ref Message m)
        {
            this.OnMouseMove(new MouseEventArgs(Control.MouseButtons, 0, 0, 0, 0));
        }

        private void WmMouseUp(ref Message m, MouseButtons button)
        {
            this.OnMouseUp(new MouseEventArgs(button, 0, 0, 0, 0));
            if (!this.doubleClick)
            {
                this.OnClick(new MouseEventArgs(button, 0, 0, 0, 0));
                this.OnMouseClick(new MouseEventArgs(button, 0, 0, 0, 0));
            }
            this.doubleClick = false;
        }

        private void WmTaskbarCreated(ref Message m)
        {
            this.added = false;
            this.UpdateIcon(this.visible);
        }

        private void WndProc(ref Message msg)
        {
            int num = msg.Msg;
            if (num <= 0x2c)
            {
                switch (num)
                {
                    case 0x2b:
                        if (msg.WParam == IntPtr.Zero)
                        {
                            this.WmDrawItemMenuItem(ref msg);
                        }
                        return;

                    case 0x2c:
                        if (msg.WParam == IntPtr.Zero)
                        {
                            this.WmMeasureMenuItem(ref msg);
                        }
                        return;

                    case 2:
                        this.UpdateIcon(false);
                        return;
                }
            }
            else
            {
                switch (num)
                {
                    case 0x111:
                        if (IntPtr.Zero == msg.LParam)
                        {
                            if (Command.DispatchID(((int) msg.WParam) & 0xffff))
                            {
                                return;
                            }
                            return;
                        }
                        this.window.DefWndProc(ref msg);
                        return;

                    case 0x117:
                        this.WmInitMenuPopup(ref msg);
                        return;

                    default:
                        if (num != 0x800)
                        {
                            break;
                        }
                        switch (((int) msg.LParam))
                        {
                            case 0x200:
                                this.WmMouseMove(ref msg);
                                return;

                            case 0x201:
                                this.WmMouseDown(ref msg, MouseButtons.Left, 1);
                                return;

                            case 0x202:
                                this.WmMouseUp(ref msg, MouseButtons.Left);
                                return;

                            case 0x203:
                                this.WmMouseDown(ref msg, MouseButtons.Left, 2);
                                return;

                            case 0x204:
                                this.WmMouseDown(ref msg, MouseButtons.Right, 1);
                                return;

                            case 0x205:
                                if ((this.contextMenu != null) || (this.contextMenuStrip != null))
                                {
                                    this.ShowContextMenu();
                                }
                                this.WmMouseUp(ref msg, MouseButtons.Right);
                                return;

                            case 0x206:
                                this.WmMouseDown(ref msg, MouseButtons.Right, 2);
                                return;

                            case 0x207:
                                this.WmMouseDown(ref msg, MouseButtons.Middle, 1);
                                return;

                            case 520:
                                this.WmMouseUp(ref msg, MouseButtons.Middle);
                                return;

                            case 0x209:
                                this.WmMouseDown(ref msg, MouseButtons.Middle, 2);
                                return;

                            case 0x402:
                                this.OnBalloonTipShown();
                                return;

                            case 0x403:
                                this.OnBalloonTipClosed();
                                return;

                            case 0x404:
                                this.OnBalloonTipClosed();
                                return;

                            case 0x405:
                                this.OnBalloonTipClicked();
                                return;
                        }
                        return;
                }
            }
            if (msg.Msg == WM_TASKBARCREATED)
            {
                this.WmTaskbarCreated(ref msg);
            }
            this.window.DefWndProc(ref msg);
        }

        [DefaultValue(0), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("NotifyIconBalloonTipIconDescr")]
        public ToolTipIcon BalloonTipIcon
        {
            get
            {
                return this.balloonTipIcon;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolTipIcon));
                }
                if (value != this.balloonTipIcon)
                {
                    this.balloonTipIcon = value;
                }
            }
        }

        [Localizable(true), DefaultValue(""), System.Windows.Forms.SRDescription("NotifyIconBalloonTipTextDescr"), Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRCategory("CatAppearance")]
        public string BalloonTipText
        {
            get
            {
                return this.balloonTipText;
            }
            set
            {
                if (value != this.balloonTipText)
                {
                    this.balloonTipText = value;
                }
            }
        }

        [System.Windows.Forms.SRDescription("NotifyIconBalloonTipTitleDescr"), Localizable(true), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue("")]
        public string BalloonTipTitle
        {
            get
            {
                return this.balloonTipTitle;
            }
            set
            {
                if (value != this.balloonTipTitle)
                {
                    this.balloonTipTitle = value;
                }
            }
        }

        [System.Windows.Forms.SRDescription("NotifyIconMenuDescr"), System.Windows.Forms.SRCategory("CatBehavior"), Browsable(false), DefaultValue((string) null)]
        public System.Windows.Forms.ContextMenu ContextMenu
        {
            get
            {
                return this.contextMenu;
            }
            set
            {
                this.contextMenu = value;
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("NotifyIconMenuDescr")]
        public System.Windows.Forms.ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return this.contextMenuStrip;
            }
            set
            {
                this.contextMenuStrip = value;
            }
        }

        [DefaultValue((string) null), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("NotifyIconIconDescr"), Localizable(true)]
        public System.Drawing.Icon Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                if (this.icon != value)
                {
                    this.icon = value;
                    this.UpdateIcon(this.visible);
                }
            }
        }

        [Bindable(true), DefaultValue((string) null), Localizable(false), TypeConverter(typeof(StringConverter)), System.Windows.Forms.SRDescription("ControlTagDescr"), System.Windows.Forms.SRCategory("CatData")]
        public object Tag
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.userData = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), Localizable(true), DefaultValue(""), System.Windows.Forms.SRDescription("NotifyIconTextDescr"), Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if ((value != null) && !value.Equals(this.text))
                {
                    if ((value != null) && (value.Length > 0x3f))
                    {
                        throw new ArgumentOutOfRangeException("Text", value, System.Windows.Forms.SR.GetString("TrayIcon_TextTooLong"));
                    }
                    this.text = value;
                    if (this.added)
                    {
                        this.UpdateIcon(true);
                    }
                }
            }
        }

        [Localizable(true), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("NotifyIconVisDescr"), DefaultValue(false)]
        public bool Visible
        {
            get
            {
                return this.visible;
            }
            set
            {
                if (this.visible != value)
                {
                    this.UpdateIcon(value);
                    this.visible = value;
                }
            }
        }

        private class NotifyIconNativeWindow : NativeWindow
        {
            internal NotifyIcon reference;
            private GCHandle rootRef;

            internal NotifyIconNativeWindow(NotifyIcon component)
            {
                this.reference = component;
            }

            ~NotifyIconNativeWindow()
            {
                if (base.Handle != IntPtr.Zero)
                {
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.Handle), 0x10, 0, 0);
                }
            }

            public void LockReference(bool locked)
            {
                if (locked)
                {
                    if (!this.rootRef.IsAllocated)
                    {
                        this.rootRef = GCHandle.Alloc(this.reference, GCHandleType.Normal);
                    }
                }
                else if (this.rootRef.IsAllocated)
                {
                    this.rootRef.Free();
                }
            }

            protected override void OnThreadException(Exception e)
            {
                Application.OnThreadException(e);
            }

            protected override void WndProc(ref Message m)
            {
                this.reference.WndProc(ref m);
            }
        }
    }
}

