namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [System.Windows.Forms.SRDescription("DescriptionToolTip"), ProvideProperty("ToolTip", typeof(Control)), DefaultEvent("Popup"), ToolboxItemFilter("System.Windows.Forms")]
    public class ToolTip : Component, IExtenderProvider
    {
        private bool active;
        private bool auto;
        private const int AUTOPOP_RATIO = 10;
        private Color backColor;
        private bool cancelled;
        private Hashtable created;
        private const int DEFAULT_DELAY = 500;
        private int[] delayTimes;
        private Color foreColor;
        private bool isBalloon;
        private bool isDisposing;
        private int originalPopupDelay;
        private bool ownerDraw;
        private Hashtable owners;
        private const int RESHOW_RATIO = 5;
        private bool showAlways;
        private bool stripAmpersands;
        private ToolTipTimer timer;
        private Hashtable tools;
        private System.Windows.Forms.ToolTipIcon toolTipIcon;
        private string toolTipTitle;
        private Control topLevelControl;
        private bool trackPosition;
        private bool useAnimation;
        private bool useFading;
        private object userData;
        private ToolTipNativeWindow window;
        private const int XBALLOONOFFSET = 10;
        private const int YBALLOONOFFSET = 8;

        [System.Windows.Forms.SRDescription("ToolTipDrawEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event DrawToolTipEventHandler Draw;

        [System.Windows.Forms.SRDescription("ToolTipPopupEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event PopupEventHandler Popup;

        public ToolTip()
        {
            this.tools = new Hashtable();
            this.delayTimes = new int[4];
            this.auto = true;
            this.active = true;
            this.backColor = SystemColors.Info;
            this.foreColor = SystemColors.InfoText;
            this.toolTipTitle = string.Empty;
            this.owners = new Hashtable();
            this.useAnimation = true;
            this.useFading = true;
            this.created = new Hashtable();
            this.window = new ToolTipNativeWindow(this);
            this.auto = true;
            this.delayTimes[0] = 500;
            this.AdjustBaseFromAuto();
        }

        public ToolTip(IContainer cont) : this()
        {
            if (cont == null)
            {
                throw new ArgumentNullException("cont");
            }
            cont.Add(this);
        }

        private void AdjustBaseFromAuto()
        {
            this.delayTimes[1] = this.delayTimes[0] / 5;
            this.delayTimes[2] = this.delayTimes[0] * 10;
            this.delayTimes[3] = this.delayTimes[0];
        }

        private void BaseFormDeactivate(object sender, EventArgs e)
        {
            this.HideAllToolTips();
        }

        public bool CanExtend(object target)
        {
            return ((target is Control) && !(target is ToolTip));
        }

        private void CheckCompositeControls(Control associatedControl)
        {
            if (associatedControl is UpDownBase)
            {
                ((UpDownBase) associatedControl).SetToolTip(this, this.GetToolTip(associatedControl));
            }
        }

        private void CheckNativeToolTip(Control associatedControl)
        {
            if (this.GetHandleCreated())
            {
                TreeView view = associatedControl as TreeView;
                if ((view != null) && view.ShowNodeToolTips)
                {
                    view.SetToolTip(this, this.GetToolTip(associatedControl));
                }
                if (associatedControl is ToolBar)
                {
                    ((ToolBar) associatedControl).SetToolTip(this);
                }
                TabControl control = associatedControl as TabControl;
                if ((control != null) && control.ShowToolTips)
                {
                    control.SetToolTip(this, this.GetToolTip(associatedControl));
                }
                if (associatedControl is ListView)
                {
                    ((ListView) associatedControl).SetToolTip(this, this.GetToolTip(associatedControl));
                }
                if (associatedControl is StatusBar)
                {
                    ((StatusBar) associatedControl).SetToolTip(this);
                }
                if (associatedControl is Label)
                {
                    ((Label) associatedControl).SetToolTip(this);
                }
            }
        }

        private void ClearTopLevelControlEvents()
        {
            if (this.topLevelControl != null)
            {
                this.topLevelControl.ParentChanged -= new EventHandler(this.OnTopLevelPropertyChanged);
                this.topLevelControl.HandleCreated -= new EventHandler(this.TopLevelCreated);
                this.topLevelControl.HandleDestroyed -= new EventHandler(this.TopLevelDestroyed);
            }
        }

        private void CreateAllRegions()
        {
            Control[] array = new Control[this.tools.Keys.Count];
            this.tools.Keys.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] is DataGridView)
                {
                    return;
                }
                this.CreateRegion(array[i]);
            }
        }

        private void CreateHandle()
        {
            if (!this.GetHandleCreated())
            {
                IntPtr userCookie = System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Activate();
                try
                {
                    System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc = new System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX {
                        dwICC = 8
                    };
                    System.Windows.Forms.SafeNativeMethods.InitCommonControlsEx(icc);
                    System.Windows.Forms.CreateParams createParams = this.CreateParams;
                    if (this.GetHandleCreated())
                    {
                        return;
                    }
                    this.window.CreateHandle(createParams);
                }
                finally
                {
                    System.Windows.Forms.UnsafeNativeMethods.ThemingScope.Deactivate(userCookie);
                }
                if (this.ownerDraw)
                {
                    int windowLong = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, this.Handle), -16));
                    windowLong &= -8388609;
                    System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, this.Handle), -16, new HandleRef(null, (IntPtr) windowLong));
                }
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x418, 0, SystemInformation.MaxWindowTrackSize.Width);
                if (this.auto)
                {
                    this.SetDelayTime(0, this.delayTimes[0]);
                    this.delayTimes[2] = this.GetDelayTime(2);
                    this.delayTimes[3] = this.GetDelayTime(3);
                    this.delayTimes[1] = this.GetDelayTime(1);
                }
                else
                {
                    for (int i = 1; i < this.delayTimes.Length; i++)
                    {
                        if (this.delayTimes[i] >= 1)
                        {
                            this.SetDelayTime(i, this.delayTimes[i]);
                        }
                    }
                }
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x401, this.active ? 1 : 0, 0);
                if (this.BackColor != SystemColors.Info)
                {
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x413, ColorTranslator.ToWin32(this.BackColor), 0);
                }
                if (this.ForeColor != SystemColors.InfoText)
                {
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x414, ColorTranslator.ToWin32(this.ForeColor), 0);
                }
                if ((this.toolTipIcon > System.Windows.Forms.ToolTipIcon.None) || !string.IsNullOrEmpty(this.toolTipTitle))
                {
                    string lParam = !string.IsNullOrEmpty(this.toolTipTitle) ? this.toolTipTitle : " ";
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_SETTITLE, (int) this.toolTipIcon, lParam);
                }
            }
        }

        private void CreateRegion(Control ctl)
        {
            string toolTip = this.GetToolTip(ctl);
            bool flag = (toolTip != null) && (toolTip.Length > 0);
            bool flag2 = (ctl.IsHandleCreated && (this.TopLevelControl != null)) && this.TopLevelControl.IsHandleCreated;
            if ((!this.created.ContainsKey(ctl) && flag) && (flag2 && !base.DesignMode))
            {
                this.SetToolInfo(ctl, toolTip);
                this.created[ctl] = ctl;
            }
            if (ctl.IsHandleCreated && (this.topLevelControl == null))
            {
                ctl.MouseMove -= new MouseEventHandler(this.MouseMove);
                ctl.MouseMove += new MouseEventHandler(this.MouseMove);
            }
        }

        private void DestoyAllRegions()
        {
            Control[] array = new Control[this.tools.Keys.Count];
            this.tools.Keys.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] is DataGridView)
                {
                    return;
                }
                this.DestroyRegion(array[i]);
            }
        }

        internal void DestroyHandle()
        {
            if (this.GetHandleCreated())
            {
                this.window.DestroyHandle();
            }
        }

        private void DestroyRegion(Control ctl)
        {
            bool flag = ((ctl.IsHandleCreated && (this.topLevelControl != null)) && this.topLevelControl.IsHandleCreated) && !this.isDisposing;
            Form topLevelControl = this.topLevelControl as Form;
            if ((topLevelControl == null) || ((topLevelControl != null) && !topLevelControl.Modal))
            {
                flag = flag && this.GetHandleCreated();
            }
            if ((this.created.ContainsKey(ctl) && flag) && !base.DesignMode)
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_DELTOOL, 0, this.GetMinTOOLINFO(ctl));
                this.created.Remove(ctl);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.isDisposing = true;
                try
                {
                    this.ClearTopLevelControlEvents();
                    this.StopTimer();
                    this.DestroyHandle();
                    this.RemoveAll();
                    this.window = null;
                    Form topLevelControl = this.TopLevelControl as Form;
                    if (topLevelControl != null)
                    {
                        topLevelControl.Deactivate -= new EventHandler(this.BaseFormDeactivate);
                    }
                }
                finally
                {
                    this.isDisposing = false;
                }
            }
            base.Dispose(disposing);
        }

        ~ToolTip()
        {
            this.DestroyHandle();
        }

        private int GetDelayTime(int type)
        {
            if (this.GetHandleCreated())
            {
                return (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x415, type, 0);
            }
            return this.delayTimes[type];
        }

        internal bool GetHandleCreated()
        {
            if (this.window == null)
            {
                return false;
            }
            return (this.window.Handle != IntPtr.Zero);
        }

        private System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP GetMinTOOLINFO(Control ctl)
        {
            System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP toolinfo_tooltip;
            return new System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP { cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP)), hwnd = ctl.Handle, uFlags = toolinfo_tooltip.uFlags | 1, uId = ctl.Handle };
        }

        private System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP GetTOOLINFO(Control ctl, string caption, out bool allocatedString)
        {
            allocatedString = false;
            System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP minTOOLINFO = this.GetMinTOOLINFO(ctl);
            minTOOLINFO.cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP));
            minTOOLINFO.uFlags |= 0x110;
            Control topLevelControl = this.TopLevelControl;
            if (((topLevelControl != null) && (topLevelControl.RightToLeft == RightToLeft.Yes)) && !ctl.IsMirrored)
            {
                minTOOLINFO.uFlags |= 4;
            }
            if ((ctl is TreeView) || (ctl is ListView))
            {
                TreeView view = ctl as TreeView;
                if ((view != null) && view.ShowNodeToolTips)
                {
                    minTOOLINFO.lpszText = System.Windows.Forms.NativeMethods.InvalidIntPtr;
                    return minTOOLINFO;
                }
                ListView view2 = ctl as ListView;
                if ((view2 != null) && view2.ShowItemToolTips)
                {
                    minTOOLINFO.lpszText = System.Windows.Forms.NativeMethods.InvalidIntPtr;
                    return minTOOLINFO;
                }
                minTOOLINFO.lpszText = Marshal.StringToHGlobalAuto(caption);
                allocatedString = true;
                return minTOOLINFO;
            }
            minTOOLINFO.lpszText = Marshal.StringToHGlobalAuto(caption);
            allocatedString = true;
            return minTOOLINFO;
        }

        [System.Windows.Forms.SRDescription("ToolTipToolTipDescr"), Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Localizable(true), DefaultValue("")]
        public string GetToolTip(Control control)
        {
            if (control == null)
            {
                return string.Empty;
            }
            TipInfo info = (TipInfo) this.tools[control];
            if ((info != null) && (info.Caption != null))
            {
                return info.Caption;
            }
            return "";
        }

        private IntPtr GetWindowFromPoint(Point screenCoords, ref bool success)
        {
            Control topLevelControl = this.TopLevelControl;
            if ((topLevelControl != null) && topLevelControl.IsActiveX)
            {
                IntPtr ptr = System.Windows.Forms.UnsafeNativeMethods.WindowFromPoint(screenCoords.X, screenCoords.Y);
                if (ptr != IntPtr.Zero)
                {
                    Control key = Control.FromHandleInternal(ptr);
                    if (((key != null) && (this.tools != null)) && this.tools.ContainsKey(key))
                    {
                        return ptr;
                    }
                }
                return IntPtr.Zero;
            }
            IntPtr zero = IntPtr.Zero;
            if (topLevelControl != null)
            {
                zero = topLevelControl.Handle;
            }
            IntPtr handle = IntPtr.Zero;
            bool flag = false;
            while (!flag)
            {
                Point point = screenCoords;
                if (topLevelControl != null)
                {
                    point = topLevelControl.PointToClientInternal(screenCoords);
                }
                IntPtr ptr4 = System.Windows.Forms.UnsafeNativeMethods.ChildWindowFromPointEx(new HandleRef(null, zero), point.X, point.Y, 1);
                if (ptr4 == zero)
                {
                    handle = ptr4;
                    flag = true;
                }
                else
                {
                    if (ptr4 == IntPtr.Zero)
                    {
                        flag = true;
                        continue;
                    }
                    topLevelControl = Control.FromHandleInternal(ptr4);
                    if (topLevelControl == null)
                    {
                        topLevelControl = Control.FromChildHandleInternal(ptr4);
                        if (topLevelControl != null)
                        {
                            handle = topLevelControl.Handle;
                        }
                        flag = true;
                        continue;
                    }
                    zero = topLevelControl.Handle;
                }
            }
            if (handle != IntPtr.Zero)
            {
                Control control3 = Control.FromHandleInternal(handle);
                if (control3 == null)
                {
                    return handle;
                }
                Control parentInternal = control3;
                while ((parentInternal != null) && parentInternal.Visible)
                {
                    parentInternal = parentInternal.ParentInternal;
                }
                if (parentInternal != null)
                {
                    handle = IntPtr.Zero;
                }
                success = true;
            }
            return handle;
        }

        private System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP GetWinTOOLINFO(IntPtr hWnd)
        {
            System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP toolinfo_tooltip;
            toolinfo_tooltip = new System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP {
                cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP)),
                hwnd = hWnd,
                uFlags = toolinfo_tooltip.uFlags | 0x111
            };
            Control topLevelControl = this.TopLevelControl;
            if (((topLevelControl != null) && (topLevelControl.RightToLeft == RightToLeft.Yes)) && ((((int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, hWnd), -16))) & 0x400000) != 0x400000))
            {
                toolinfo_tooltip.uFlags |= 4;
            }
            toolinfo_tooltip.uId = toolinfo_tooltip.hwnd;
            return toolinfo_tooltip;
        }

        private void HandleCreated(object sender, EventArgs eventargs)
        {
            this.ClearTopLevelControlEvents();
            this.topLevelControl = null;
            this.CreateRegion((Control) sender);
            this.CheckNativeToolTip((Control) sender);
            this.CheckCompositeControls((Control) sender);
        }

        private void HandleDestroyed(object sender, EventArgs eventargs)
        {
            this.DestroyRegion((Control) sender);
        }

        public void Hide(IWin32Window win)
        {
            if (win == null)
            {
                throw new ArgumentNullException("win");
            }
            if (this.HasAllWindowsPermission && (this.window != null))
            {
                if (this.GetHandleCreated())
                {
                    IntPtr safeHandle = Control.GetSafeHandle(win);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x411, 0, this.GetWinTOOLINFO(safeHandle));
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_DELTOOL, 0, this.GetWinTOOLINFO(safeHandle));
                }
                this.StopTimer();
                Control key = win as Control;
                if (key == null)
                {
                    this.owners.Remove(win.Handle);
                }
                else
                {
                    if (this.tools.ContainsKey(key))
                    {
                        this.SetToolInfo(key, this.GetToolTip(key));
                    }
                    else
                    {
                        this.owners.Remove(win.Handle);
                    }
                    Form form = key.FindFormInternal();
                    if (form != null)
                    {
                        form.Deactivate -= new EventHandler(this.BaseFormDeactivate);
                    }
                }
                this.ClearTopLevelControlEvents();
                this.topLevelControl = null;
            }
        }

        private void HideAllToolTips()
        {
            Control[] array = new Control[this.owners.Values.Count];
            this.owners.Values.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                this.Hide(array[i]);
            }
        }

        private bool IsWindowActive(IWin32Window window)
        {
            Control key = window as Control;
            if ((key.ShowParams & 15) != 4)
            {
                IntPtr activeWindow = System.Windows.Forms.UnsafeNativeMethods.GetActiveWindow();
                IntPtr ancestor = System.Windows.Forms.UnsafeNativeMethods.GetAncestor(new HandleRef(window, window.Handle), 2);
                if (activeWindow != ancestor)
                {
                    TipInfo info = (TipInfo) this.tools[key];
                    if ((info != null) && ((info.TipType & TipInfo.Type.SemiAbsolute) != TipInfo.Type.None))
                    {
                        this.tools.Remove(key);
                        this.DestroyRegion(key);
                    }
                    return false;
                }
            }
            return true;
        }

        private void MouseMove(object sender, MouseEventArgs me)
        {
            Control key = (Control) sender;
            if ((!this.created.ContainsKey(key) && key.IsHandleCreated) && (this.TopLevelControl != null))
            {
                this.CreateRegion(key);
            }
            if (this.created.ContainsKey(key))
            {
                key.MouseMove -= new MouseEventHandler(this.MouseMove);
            }
        }

        private void OnDraw(DrawToolTipEventArgs e)
        {
            if (this.onDraw != null)
            {
                this.onDraw(this, e);
            }
        }

        private void OnPopup(PopupEventArgs e)
        {
            if (this.onPopup != null)
            {
                this.onPopup(this, e);
            }
        }

        private void OnTopLevelPropertyChanged(object s, EventArgs e)
        {
            this.ClearTopLevelControlEvents();
            this.topLevelControl = null;
            this.topLevelControl = this.TopLevelControl;
        }

        private void RecreateHandle()
        {
            if (!base.DesignMode)
            {
                if (this.GetHandleCreated())
                {
                    this.DestroyHandle();
                }
                this.created.Clear();
                this.CreateHandle();
                this.CreateAllRegions();
            }
        }

        public void RemoveAll()
        {
            Control[] array = new Control[this.tools.Keys.Count];
            this.tools.Keys.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].IsHandleCreated)
                {
                    this.DestroyRegion(array[i]);
                }
                array[i].HandleCreated -= new EventHandler(this.HandleCreated);
                array[i].HandleDestroyed -= new EventHandler(this.HandleDestroyed);
            }
            this.created.Clear();
            this.tools.Clear();
            this.ClearTopLevelControlEvents();
            this.topLevelControl = null;
        }

        private void Reposition(Point tipPosition, Size tipSize)
        {
            Point point = tipPosition;
            Screen screen = Screen.FromPoint(point);
            if ((point.X + tipSize.Width) > screen.WorkingArea.Right)
            {
                point.X = screen.WorkingArea.Right - tipSize.Width;
            }
            if ((point.Y + tipSize.Height) > screen.WorkingArea.Bottom)
            {
                point.Y = screen.WorkingArea.Bottom - tipSize.Height;
            }
            System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.HWND_TOPMOST, point.X, point.Y, tipSize.Width, tipSize.Height, 0x211);
        }

        private void SetDelayTime(int type, int time)
        {
            if (type == 0)
            {
                this.auto = true;
            }
            else
            {
                this.auto = false;
            }
            this.delayTimes[type] = time;
            if (this.GetHandleCreated() && (time >= 0))
            {
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x403, type, time);
                if (this.auto)
                {
                    this.delayTimes[2] = this.GetDelayTime(2);
                    this.delayTimes[3] = this.GetDelayTime(3);
                    this.delayTimes[1] = this.GetDelayTime(1);
                }
            }
            else if (this.auto)
            {
                this.AdjustBaseFromAuto();
            }
        }

        private void SetTool(IWin32Window win, string text, TipInfo.Type type, Point position)
        {
            Control key = win as Control;
            if ((key != null) && this.tools.ContainsKey(key))
            {
                bool flag = false;
                System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP lParam = new System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP();
                try
                {
                    lParam.cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP));
                    lParam.hwnd = key.Handle;
                    lParam.uId = key.Handle;
                    if (((int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_GETTOOLINFO, 0, lParam)) != 0)
                    {
                        lParam.uFlags |= 0x20;
                        if ((type == TipInfo.Type.Absolute) || (type == TipInfo.Type.SemiAbsolute))
                        {
                            lParam.uFlags |= 0x80;
                        }
                        lParam.lpszText = Marshal.StringToHGlobalAuto(text);
                        flag = true;
                    }
                    TipInfo info = (TipInfo) this.tools[key];
                    if (info == null)
                    {
                        info = new TipInfo(text, type);
                    }
                    else
                    {
                        info.TipType |= type;
                        info.Caption = text;
                    }
                    info.Position = position;
                    this.tools[key] = info;
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_SETTOOLINFO, 0, lParam);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x411, 1, lParam);
                }
                finally
                {
                    if (flag && (IntPtr.Zero != lParam.lpszText))
                    {
                        Marshal.FreeHGlobal(lParam.lpszText);
                    }
                }
            }
            else
            {
                this.Hide(win);
                TipInfo info2 = (TipInfo) this.tools[key];
                if (info2 == null)
                {
                    info2 = new TipInfo(text, type);
                }
                else
                {
                    info2.TipType |= type;
                    info2.Caption = text;
                }
                info2.Position = position;
                this.tools[key] = info2;
                IntPtr safeHandle = Control.GetSafeHandle(win);
                this.owners[safeHandle] = win;
                System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP winTOOLINFO = this.GetWinTOOLINFO(safeHandle);
                winTOOLINFO.uFlags |= 0x20;
                if ((type == TipInfo.Type.Absolute) || (type == TipInfo.Type.SemiAbsolute))
                {
                    winTOOLINFO.uFlags |= 0x80;
                }
                try
                {
                    winTOOLINFO.lpszText = Marshal.StringToHGlobalAuto(text);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_ADDTOOL, 0, winTOOLINFO);
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x411, 1, winTOOLINFO);
                }
                finally
                {
                    if (IntPtr.Zero != winTOOLINFO.lpszText)
                    {
                        Marshal.FreeHGlobal(winTOOLINFO.lpszText);
                    }
                }
            }
            if (key != null)
            {
                Form form = key.FindFormInternal();
                if (form != null)
                {
                    form.Deactivate += new EventHandler(this.BaseFormDeactivate);
                }
            }
        }

        private void SetToolInfo(Control ctl, string caption)
        {
            bool flag;
            System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP lParam = this.GetTOOLINFO(ctl, caption, out flag);
            try
            {
                int num = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_ADDTOOL, 0, lParam);
                if ((ctl is TreeView) || (ctl is ListView))
                {
                    TreeView view = ctl as TreeView;
                    if ((view != null) && view.ShowNodeToolTips)
                    {
                        return;
                    }
                    ListView view2 = ctl as ListView;
                    if ((view2 != null) && view2.ShowItemToolTips)
                    {
                        return;
                    }
                }
                if (num == 0)
                {
                    throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ToolTipAddFailed"));
                }
            }
            finally
            {
                if (flag && (IntPtr.Zero != lParam.lpszText))
                {
                    Marshal.FreeHGlobal(lParam.lpszText);
                }
            }
        }

        public void SetToolTip(Control control, string caption)
        {
            TipInfo info = new TipInfo(caption, TipInfo.Type.Auto);
            this.SetToolTipInternal(control, info);
        }

        private void SetToolTipInternal(Control control, TipInfo info)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            bool flag = false;
            bool flag2 = false;
            if (this.tools.ContainsKey(control))
            {
                flag = true;
            }
            if ((info == null) || string.IsNullOrEmpty(info.Caption))
            {
                flag2 = true;
            }
            if (flag && flag2)
            {
                this.tools.Remove(control);
            }
            else if (!flag2)
            {
                this.tools[control] = info;
            }
            if (!flag2 && !flag)
            {
                control.HandleCreated += new EventHandler(this.HandleCreated);
                control.HandleDestroyed += new EventHandler(this.HandleDestroyed);
                if (control.IsHandleCreated)
                {
                    this.HandleCreated(control, EventArgs.Empty);
                }
            }
            else
            {
                bool flag3 = (control.IsHandleCreated && (this.TopLevelControl != null)) && this.TopLevelControl.IsHandleCreated;
                if ((flag && !flag2) && (flag3 && !base.DesignMode))
                {
                    bool flag4;
                    System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP lParam = this.GetTOOLINFO(control, info.Caption, out flag4);
                    try
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_SETTOOLINFO, 0, lParam);
                    }
                    finally
                    {
                        if (flag4 && (IntPtr.Zero != lParam.lpszText))
                        {
                            Marshal.FreeHGlobal(lParam.lpszText);
                        }
                    }
                    this.CheckNativeToolTip(control);
                    this.CheckCompositeControls(control);
                }
                else if ((flag2 && flag) && !base.DesignMode)
                {
                    control.HandleCreated -= new EventHandler(this.HandleCreated);
                    control.HandleDestroyed -= new EventHandler(this.HandleDestroyed);
                    if (control.IsHandleCreated)
                    {
                        this.HandleDestroyed(control, EventArgs.Empty);
                    }
                    this.created.Remove(control);
                }
            }
        }

        private void SetTrackPosition(int pointX, int pointY)
        {
            try
            {
                this.trackPosition = true;
                System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x412, 0, System.Windows.Forms.NativeMethods.Util.MAKELONG(pointX, pointY));
            }
            finally
            {
                this.trackPosition = false;
            }
        }

        private bool ShouldSerializeAutomaticDelay()
        {
            return (this.auto && (this.AutomaticDelay != 500));
        }

        private bool ShouldSerializeAutoPopDelay()
        {
            return !this.auto;
        }

        private bool ShouldSerializeInitialDelay()
        {
            return !this.auto;
        }

        private bool ShouldSerializeReshowDelay()
        {
            return !this.auto;
        }

        public void Show(string text, IWin32Window window)
        {
            if (this.HasAllWindowsPermission && this.IsWindowActive(window))
            {
                this.ShowTooltip(text, window, 0);
            }
        }

        public void Show(string text, IWin32Window window, Point point)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            if (this.HasAllWindowsPermission && this.IsWindowActive(window))
            {
                System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(window, Control.GetSafeHandle(window)), ref rect);
                int pointX = rect.left + point.X;
                int pointY = rect.top + point.Y;
                this.SetTrackPosition(pointX, pointY);
                this.SetTool(window, text, TipInfo.Type.Absolute, new Point(pointX, pointY));
            }
        }

        public void Show(string text, IWin32Window window, int duration)
        {
            if (duration < 0)
            {
                object[] args = new object[] { "duration", duration.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("duration", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
            }
            if (this.HasAllWindowsPermission && this.IsWindowActive(window))
            {
                this.ShowTooltip(text, window, duration);
            }
        }

        public void Show(string text, IWin32Window window, Point point, int duration)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            if (duration < 0)
            {
                object[] args = new object[] { "duration", duration.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("duration", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
            }
            if (this.HasAllWindowsPermission && this.IsWindowActive(window))
            {
                System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(window, Control.GetSafeHandle(window)), ref rect);
                int pointX = rect.left + point.X;
                int pointY = rect.top + point.Y;
                this.SetTrackPosition(pointX, pointY);
                this.SetTool(window, text, TipInfo.Type.Absolute, new Point(pointX, pointY));
                this.StartTimer(window, duration);
            }
        }

        public void Show(string text, IWin32Window window, int x, int y)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            if (this.HasAllWindowsPermission && this.IsWindowActive(window))
            {
                System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(window, Control.GetSafeHandle(window)), ref rect);
                int pointX = rect.left + x;
                int pointY = rect.top + y;
                this.SetTrackPosition(pointX, pointY);
                this.SetTool(window, text, TipInfo.Type.Absolute, new Point(pointX, pointY));
            }
        }

        public void Show(string text, IWin32Window window, int x, int y, int duration)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            if (duration < 0)
            {
                object[] args = new object[] { "duration", duration.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentOutOfRangeException("duration", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
            }
            if (this.HasAllWindowsPermission && this.IsWindowActive(window))
            {
                System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(window, Control.GetSafeHandle(window)), ref rect);
                int pointX = rect.left + x;
                int pointY = rect.top + y;
                this.SetTrackPosition(pointX, pointY);
                this.SetTool(window, text, TipInfo.Type.Absolute, new Point(pointX, pointY));
                this.StartTimer(window, duration);
            }
        }

        private void ShowTooltip(string text, IWin32Window win, int duration)
        {
            if (win == null)
            {
                throw new ArgumentNullException("win");
            }
            Control wrapper = win as Control;
            if (wrapper != null)
            {
                System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(wrapper, wrapper.Handle), ref rect);
                Cursor currentInternal = Cursor.CurrentInternal;
                Point position = Cursor.Position;
                Point p = position;
                Screen screen = Screen.FromPoint(position);
                if (((position.X < rect.left) || (position.X > rect.right)) || ((position.Y < rect.top) || (position.Y > rect.bottom)))
                {
                    System.Windows.Forms.NativeMethods.RECT rect2 = new System.Windows.Forms.NativeMethods.RECT {
                        left = (rect.left < screen.WorkingArea.Left) ? screen.WorkingArea.Left : rect.left,
                        top = (rect.top < screen.WorkingArea.Top) ? screen.WorkingArea.Top : rect.top,
                        right = (rect.right > screen.WorkingArea.Right) ? screen.WorkingArea.Right : rect.right,
                        bottom = (rect.bottom > screen.WorkingArea.Bottom) ? screen.WorkingArea.Bottom : rect.bottom
                    };
                    p.X = rect2.left + ((rect2.right - rect2.left) / 2);
                    p.Y = rect2.top + ((rect2.bottom - rect2.top) / 2);
                    wrapper.PointToClientInternal(p);
                    this.SetTrackPosition(p.X, p.Y);
                    this.SetTool(win, text, TipInfo.Type.SemiAbsolute, p);
                    if (duration > 0)
                    {
                        this.StartTimer(this.window, duration);
                    }
                }
                else
                {
                    TipInfo info = (TipInfo) this.tools[wrapper];
                    if (info == null)
                    {
                        info = new TipInfo(text, TipInfo.Type.SemiAbsolute);
                    }
                    else
                    {
                        info.TipType |= TipInfo.Type.SemiAbsolute;
                        info.Caption = text;
                    }
                    info.Position = p;
                    if (duration > 0)
                    {
                        if (this.originalPopupDelay == 0)
                        {
                            this.originalPopupDelay = this.AutoPopDelay;
                        }
                        this.AutoPopDelay = duration;
                    }
                    this.SetToolTipInternal(wrapper, info);
                }
            }
        }

        private void StartTimer(IWin32Window owner, int interval)
        {
            if (this.timer == null)
            {
                this.timer = new ToolTipTimer(owner);
                this.timer.Tick += new EventHandler(this.TimerHandler);
            }
            this.timer.Interval = interval;
            this.timer.Start();
        }

        protected void StopTimer()
        {
            ToolTipTimer timer = this.timer;
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                this.timer = null;
            }
        }

        private void TimerHandler(object source, EventArgs args)
        {
            this.Hide(((ToolTipTimer) source).Host);
        }

        private void TopLevelCreated(object sender, EventArgs eventargs)
        {
            this.CreateHandle();
            this.CreateAllRegions();
        }

        private void TopLevelDestroyed(object sender, EventArgs eventargs)
        {
            this.DestoyAllRegions();
            this.DestroyHandle();
        }

        public override string ToString()
        {
            string str = base.ToString();
            return (str + " InitialDelay: " + this.InitialDelay.ToString(CultureInfo.CurrentCulture) + ", ShowAlways: " + this.ShowAlways.ToString(CultureInfo.CurrentCulture));
        }

        private void WmMouseActivate(ref Message msg)
        {
            System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP lParam = new System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP {
                cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP))
            };
            if (((int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_GETCURRENTTOOL, 0, lParam)) != 0)
            {
                IWin32Window wrapper = (IWin32Window) this.owners[lParam.hwnd];
                if (wrapper == null)
                {
                    wrapper = Control.FromHandleInternal(lParam.hwnd);
                }
                if (wrapper != null)
                {
                    System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
                    System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(wrapper, Control.GetSafeHandle(wrapper)), ref rect);
                    Point position = Cursor.Position;
                    if (((position.X >= rect.left) && (position.X <= rect.right)) && ((position.Y >= rect.top) && (position.Y <= rect.bottom)))
                    {
                        msg.Result = (IntPtr) 3;
                    }
                }
            }
        }

        private void WmMove()
        {
            System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
            System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, this.Handle), ref rect);
            System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP lParam = new System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP {
                cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP))
            };
            if (((int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_GETCURRENTTOOL, 0, lParam)) != 0)
            {
                IWin32Window window = (IWin32Window) this.owners[lParam.hwnd];
                if (window == null)
                {
                    window = Control.FromHandleInternal(lParam.hwnd);
                }
                if (window != null)
                {
                    TipInfo info = (TipInfo) this.tools[window];
                    if ((window != null) && (info != null))
                    {
                        TreeView view = window as TreeView;
                        if (((view == null) || !view.ShowNodeToolTips) && (info.Position != Point.Empty))
                        {
                            this.Reposition(info.Position, rect.Size);
                        }
                    }
                }
            }
        }

        private void WmPop()
        {
            System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP lParam = new System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP {
                cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP))
            };
            if (((int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_GETCURRENTTOOL, 0, lParam)) != 0)
            {
                IWin32Window window = (IWin32Window) this.owners[lParam.hwnd];
                if (window == null)
                {
                    window = Control.FromHandleInternal(lParam.hwnd);
                }
                if (window != null)
                {
                    Control key = window as Control;
                    TipInfo info = (TipInfo) this.tools[window];
                    if (info != null)
                    {
                        if (((info.TipType & TipInfo.Type.Auto) != TipInfo.Type.None) || ((info.TipType & TipInfo.Type.SemiAbsolute) != TipInfo.Type.None))
                        {
                            Screen screen = Screen.FromPoint(Cursor.Position);
                            System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x418, 0, screen.WorkingArea.Width);
                        }
                        if ((info.TipType & TipInfo.Type.Auto) == TipInfo.Type.None)
                        {
                            this.tools.Remove(key);
                            this.owners.Remove(window.Handle);
                            key.HandleCreated -= new EventHandler(this.HandleCreated);
                            key.HandleDestroyed -= new EventHandler(this.HandleDestroyed);
                            this.created.Remove(key);
                            if (this.originalPopupDelay != 0)
                            {
                                this.AutoPopDelay = this.originalPopupDelay;
                                this.originalPopupDelay = 0;
                            }
                        }
                        else
                        {
                            info.TipType = TipInfo.Type.Auto;
                            info.Position = Point.Empty;
                            this.tools[key] = info;
                        }
                    }
                }
            }
        }

        private void WmShow()
        {
            System.Windows.Forms.NativeMethods.RECT rect = new System.Windows.Forms.NativeMethods.RECT();
            System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, this.Handle), ref rect);
            System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP lParam = new System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP {
                cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP))
            };
            if (((int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_GETCURRENTTOOL, 0, lParam)) != 0)
            {
                IWin32Window associatedWindow = (IWin32Window) this.owners[lParam.hwnd];
                if (associatedWindow == null)
                {
                    associatedWindow = Control.FromHandleInternal(lParam.hwnd);
                }
                if (associatedWindow != null)
                {
                    Control associatedControl = associatedWindow as Control;
                    Size size = rect.Size;
                    PopupEventArgs e = new PopupEventArgs(associatedWindow, associatedControl, this.IsBalloon, size);
                    this.OnPopup(e);
                    DataGridView view = associatedControl as DataGridView;
                    if ((view != null) && view.CancelToolTipPopup(this))
                    {
                        e.Cancel = true;
                    }
                    System.Windows.Forms.UnsafeNativeMethods.GetWindowRect(new HandleRef(this, this.Handle), ref rect);
                    size = (e.ToolTipSize == size) ? rect.Size : e.ToolTipSize;
                    if (this.IsBalloon)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x41f, 1, ref rect);
                        if (rect.Size.Height > size.Height)
                        {
                            size.Height = rect.Size.Height;
                        }
                    }
                    if (size != rect.Size)
                    {
                        Screen screen = Screen.FromPoint(Cursor.Position);
                        int num2 = this.IsBalloon ? Math.Min(size.Width - 20, screen.WorkingArea.Width) : Math.Min(size.Width, screen.WorkingArea.Width);
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x418, 0, num2);
                    }
                    if (e.Cancel)
                    {
                        this.cancelled = true;
                        System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, 0x210);
                    }
                    else
                    {
                        this.cancelled = false;
                        System.Windows.Forms.SafeNativeMethods.SetWindowPos(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.HWND_TOPMOST, rect.left, rect.top, size.Width, size.Height, 0x210);
                    }
                }
            }
        }

        private void WmWindowFromPoint(ref Message msg)
        {
            System.Windows.Forms.NativeMethods.POINT lParam = (System.Windows.Forms.NativeMethods.POINT) msg.GetLParam(typeof(System.Windows.Forms.NativeMethods.POINT));
            Point screenCoords = new Point(lParam.x, lParam.y);
            bool success = false;
            msg.Result = this.GetWindowFromPoint(screenCoords, ref success);
        }

        private bool WmWindowPosChanged()
        {
            if (this.cancelled)
            {
                System.Windows.Forms.SafeNativeMethods.ShowWindow(new HandleRef(this, this.Handle), 0);
                return true;
            }
            return false;
        }

        private unsafe void WmWindowPosChanging(ref Message m)
        {
            if (!this.cancelled)
            {
                System.Windows.Forms.NativeMethods.WINDOWPOS* lParam = (System.Windows.Forms.NativeMethods.WINDOWPOS*) m.LParam;
                Cursor currentInternal = Cursor.CurrentInternal;
                Point position = Cursor.Position;
                System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP toolinfo_tooltip = new System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP {
                    cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP))
                };
                if (((int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_GETCURRENTTOOL, 0, toolinfo_tooltip)) != 0)
                {
                    IWin32Window window = (IWin32Window) this.owners[toolinfo_tooltip.hwnd];
                    if (window == null)
                    {
                        window = Control.FromHandleInternal(toolinfo_tooltip.hwnd);
                    }
                    if ((window == null) || !this.IsWindowActive(window))
                    {
                        return;
                    }
                    TipInfo info = null;
                    if (window != null)
                    {
                        info = (TipInfo) this.tools[window];
                        if (info == null)
                        {
                            return;
                        }
                        TreeView view = window as TreeView;
                        if ((view != null) && view.ShowNodeToolTips)
                        {
                            return;
                        }
                    }
                    if (this.IsBalloon)
                    {
                        lParam->cx += 20;
                        return;
                    }
                    if ((info.TipType & TipInfo.Type.Auto) != TipInfo.Type.None)
                    {
                        this.window.DefWndProc(ref m);
                        return;
                    }
                    if (((info.TipType & TipInfo.Type.SemiAbsolute) != TipInfo.Type.None) && (info.Position == Point.Empty))
                    {
                        Screen screen = Screen.FromPoint(position);
                        if (currentInternal != null)
                        {
                            lParam->x = position.X;
                            try
                            {
                                System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
                                lParam->y = position.Y;
                                if ((((lParam->y + lParam->cy) + currentInternal.Size.Height) - currentInternal.HotSpot.Y) > screen.WorkingArea.Bottom)
                                {
                                    lParam->y = position.Y - lParam->cy;
                                }
                                else
                                {
                                    lParam->y = (position.Y + currentInternal.Size.Height) - currentInternal.HotSpot.Y;
                                }
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                        if ((lParam->x + lParam->cx) > screen.WorkingArea.Right)
                        {
                            lParam->x = screen.WorkingArea.Right - lParam->cx;
                        }
                    }
                    else if (((info.TipType & TipInfo.Type.SemiAbsolute) != TipInfo.Type.None) && (info.Position != Point.Empty))
                    {
                        Screen screen2 = Screen.FromPoint(info.Position);
                        lParam->x = info.Position.X;
                        if ((lParam->x + lParam->cx) > screen2.WorkingArea.Right)
                        {
                            lParam->x = screen2.WorkingArea.Right - lParam->cx;
                        }
                        lParam->y = info.Position.Y;
                        if ((lParam->y + lParam->cy) > screen2.WorkingArea.Bottom)
                        {
                            lParam->y = screen2.WorkingArea.Bottom - lParam->cy;
                        }
                    }
                }
                m.Result = IntPtr.Zero;
            }
        }

        private void WndProc(ref Message msg)
        {
            int num2 = msg.Msg;
            if (num2 <= 0x21)
            {
                switch (num2)
                {
                    case 3:
                        this.WmMove();
                        return;

                    case 15:
                        goto Label_00FF;

                    case 0x21:
                        this.WmMouseActivate(ref msg);
                        return;
                }
                goto Label_027D;
            }
            if (num2 <= 0x318)
            {
                switch (num2)
                {
                    case 70:
                        this.WmWindowPosChanging(ref msg);
                        return;

                    case 0x47:
                        if (!this.WmWindowPosChanged())
                        {
                            this.window.DefWndProc(ref msg);
                        }
                        return;

                    case 0x318:
                        goto Label_00FF;
                }
                goto Label_027D;
            }
            if (num2 == 0x410)
            {
                this.WmWindowFromPoint(ref msg);
                return;
            }
            if (num2 != 0x204e)
            {
                goto Label_027D;
            }
            System.Windows.Forms.NativeMethods.NMHDR lParam = (System.Windows.Forms.NativeMethods.NMHDR) msg.GetLParam(typeof(System.Windows.Forms.NativeMethods.NMHDR));
            if ((lParam.code == -521) && !this.trackPosition)
            {
                this.WmShow();
                return;
            }
            if (lParam.code == -522)
            {
                this.WmPop();
                this.window.DefWndProc(ref msg);
            }
            return;
        Label_00FF:
            if ((this.ownerDraw && !this.isBalloon) && !this.trackPosition)
            {
                System.Windows.Forms.NativeMethods.PAINTSTRUCT lpPaint = new System.Windows.Forms.NativeMethods.PAINTSTRUCT();
                Graphics graphics = Graphics.FromHdcInternal(System.Windows.Forms.UnsafeNativeMethods.BeginPaint(new HandleRef(this, this.Handle), ref lpPaint));
                Rectangle bounds = new Rectangle(lpPaint.rcPaint_left, lpPaint.rcPaint_top, lpPaint.rcPaint_right - lpPaint.rcPaint_left, lpPaint.rcPaint_bottom - lpPaint.rcPaint_top);
                if (bounds == Rectangle.Empty)
                {
                    return;
                }
                System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP toolinfo_tooltip = new System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP {
                    cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP))
                };
                if (((int) ((long) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_GETCURRENTTOOL, 0, toolinfo_tooltip))) != 0)
                {
                    Font defaultFont;
                    IWin32Window associatedWindow = (IWin32Window) this.owners[toolinfo_tooltip.hwnd];
                    Control associatedControl = Control.FromHandleInternal(toolinfo_tooltip.hwnd);
                    if (associatedWindow == null)
                    {
                        associatedWindow = associatedControl;
                    }
                    System.Windows.Forms.IntSecurity.ObjectFromWin32Handle.Assert();
                    try
                    {
                        defaultFont = Font.FromHfont(System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x31, 0, 0));
                    }
                    catch (ArgumentException)
                    {
                        defaultFont = Control.DefaultFont;
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    try
                    {
                        this.OnDraw(new DrawToolTipEventArgs(graphics, associatedWindow, associatedControl, bounds, this.GetToolTip(associatedControl), this.BackColor, this.ForeColor, defaultFont));
                        return;
                    }
                    finally
                    {
                        graphics.Dispose();
                        System.Windows.Forms.UnsafeNativeMethods.EndPaint(new HandleRef(this, this.Handle), ref lpPaint);
                    }
                }
            }
        Label_027D:
            this.window.DefWndProc(ref msg);
        }

        [System.Windows.Forms.SRDescription("ToolTipActiveDescr"), DefaultValue(true)]
        public bool Active
        {
            get
            {
                return this.active;
            }
            set
            {
                if (this.active != value)
                {
                    this.active = value;
                    if (!base.DesignMode && this.GetHandleCreated())
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x401, value ? 1 : 0, 0);
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolTipAutomaticDelayDescr"), RefreshProperties(RefreshProperties.All), DefaultValue(500)]
        public int AutomaticDelay
        {
            get
            {
                return this.delayTimes[0];
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "AutomaticDelay", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("AutomaticDelay", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.SetDelayTime(0, value);
            }
        }

        [RefreshProperties(RefreshProperties.All), System.Windows.Forms.SRDescription("ToolTipAutoPopDelayDescr")]
        public int AutoPopDelay
        {
            get
            {
                return this.delayTimes[2];
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "AutoPopDelay", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("AutoPopDelay", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.SetDelayTime(2, value);
            }
        }

        [System.Windows.Forms.SRDescription("ToolTipBackColorDescr"), DefaultValue(typeof(Color), "Info")]
        public Color BackColor
        {
            get
            {
                return this.backColor;
            }
            set
            {
                this.backColor = value;
                if (this.GetHandleCreated())
                {
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x413, ColorTranslator.ToWin32(this.backColor), 0);
                }
            }
        }

        protected virtual System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams @params = new System.Windows.Forms.CreateParams();
                if ((this.TopLevelControl != null) && !this.TopLevelControl.IsDisposed)
                {
                    @params.Parent = this.TopLevelControl.Handle;
                }
                @params.ClassName = "tooltips_class32";
                if (this.showAlways)
                {
                    @params.Style = 1;
                }
                if (this.isBalloon)
                {
                    @params.Style |= 0x40;
                }
                if (!this.stripAmpersands)
                {
                    @params.Style |= 2;
                }
                if (!this.useAnimation)
                {
                    @params.Style |= 0x10;
                }
                if (!this.useFading)
                {
                    @params.Style |= 0x20;
                }
                @params.ExStyle = 0;
                @params.Caption = null;
                return @params;
            }
        }

        [System.Windows.Forms.SRDescription("ToolTipForeColorDescr"), DefaultValue(typeof(Color), "InfoText")]
        public Color ForeColor
        {
            get
            {
                return this.foreColor;
            }
            set
            {
                if (value.IsEmpty)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ToolTipEmptyColor", new object[] { "ForeColor" }));
                }
                this.foreColor = value;
                if (this.GetHandleCreated())
                {
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x414, ColorTranslator.ToWin32(this.foreColor), 0);
                }
            }
        }

        internal IntPtr Handle
        {
            get
            {
                if (!this.GetHandleCreated())
                {
                    this.CreateHandle();
                }
                return this.window.Handle;
            }
        }

        private bool HasAllWindowsPermission
        {
            get
            {
                try
                {
                    System.Windows.Forms.IntSecurity.AllWindows.Demand();
                    return true;
                }
                catch (SecurityException)
                {
                }
                return false;
            }
        }

        [RefreshProperties(RefreshProperties.All), System.Windows.Forms.SRDescription("ToolTipInitialDelayDescr")]
        public int InitialDelay
        {
            get
            {
                return this.delayTimes[3];
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "InitialDelay", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("InitialDelay", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.SetDelayTime(3, value);
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("ToolTipIsBalloonDescr")]
        public bool IsBalloon
        {
            get
            {
                return this.isBalloon;
            }
            set
            {
                if (this.isBalloon != value)
                {
                    this.isBalloon = value;
                    if (this.GetHandleCreated())
                    {
                        this.RecreateHandle();
                    }
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ToolTipOwnerDrawDescr")]
        public bool OwnerDraw
        {
            get
            {
                return this.ownerDraw;
            }
            [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
            set
            {
                this.ownerDraw = value;
            }
        }

        [System.Windows.Forms.SRDescription("ToolTipReshowDelayDescr"), RefreshProperties(RefreshProperties.All)]
        public int ReshowDelay
        {
            get
            {
                return this.delayTimes[1];
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "ReshowDelay", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("ReshowDelay", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.SetDelayTime(1, value);
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("ToolTipShowAlwaysDescr")]
        public bool ShowAlways
        {
            get
            {
                return this.showAlways;
            }
            set
            {
                if (this.showAlways != value)
                {
                    this.showAlways = value;
                    if (this.GetHandleCreated())
                    {
                        this.RecreateHandle();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolTipStripAmpersandsDescr"), DefaultValue(false), Browsable(true)]
        public bool StripAmpersands
        {
            get
            {
                return this.stripAmpersands;
            }
            set
            {
                if (this.stripAmpersands != value)
                {
                    this.stripAmpersands = value;
                    if (this.GetHandleCreated())
                    {
                        this.RecreateHandle();
                    }
                }
            }
        }

        [DefaultValue((string) null), Bindable(true), TypeConverter(typeof(StringConverter)), Localizable(false), System.Windows.Forms.SRDescription("ControlTagDescr"), System.Windows.Forms.SRCategory("CatData")]
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

        [System.Windows.Forms.SRDescription("ToolTipToolTipIconDescr"), DefaultValue(0)]
        public System.Windows.Forms.ToolTipIcon ToolTipIcon
        {
            get
            {
                return this.toolTipIcon;
            }
            set
            {
                if (this.toolTipIcon != value)
                {
                    if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 3))
                    {
                        throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.ToolTipIcon));
                    }
                    this.toolTipIcon = value;
                    if ((this.toolTipIcon > System.Windows.Forms.ToolTipIcon.None) && this.GetHandleCreated())
                    {
                        string lParam = !string.IsNullOrEmpty(this.toolTipTitle) ? this.toolTipTitle : " ";
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_SETTITLE, (int) this.toolTipIcon, lParam);
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x41d, 0, 0);
                    }
                }
            }
        }

        [DefaultValue(""), System.Windows.Forms.SRDescription("ToolTipTitleDescr")]
        public string ToolTipTitle
        {
            get
            {
                return this.toolTipTitle;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (this.toolTipTitle != value)
                {
                    this.toolTipTitle = value;
                    if (this.GetHandleCreated())
                    {
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), System.Windows.Forms.NativeMethods.TTM_SETTITLE, (int) this.toolTipIcon, this.toolTipTitle);
                        System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), 0x41d, 0, 0);
                    }
                }
            }
        }

        private Control TopLevelControl
        {
            get
            {
                Control sender = null;
                if (this.topLevelControl != null)
                {
                    return this.topLevelControl;
                }
                Control[] array = new Control[this.tools.Keys.Count];
                this.tools.Keys.CopyTo(array, 0);
                if ((array != null) && (array.Length > 0))
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        Control parentInternal = array[i];
                        sender = parentInternal.TopLevelControlInternal;
                        if (sender != null)
                        {
                            break;
                        }
                        if (parentInternal.IsActiveX)
                        {
                            sender = parentInternal;
                            break;
                        }
                        if (((sender == null) && (parentInternal != null)) && (parentInternal.ParentInternal != null))
                        {
                            while (parentInternal.ParentInternal != null)
                            {
                                parentInternal = parentInternal.ParentInternal;
                            }
                            sender = parentInternal;
                            if (sender != null)
                            {
                                break;
                            }
                        }
                    }
                }
                this.topLevelControl = sender;
                if (sender != null)
                {
                    sender.HandleCreated += new EventHandler(this.TopLevelCreated);
                    sender.HandleDestroyed += new EventHandler(this.TopLevelDestroyed);
                    if (sender.IsHandleCreated)
                    {
                        this.TopLevelCreated(sender, EventArgs.Empty);
                    }
                    sender.ParentChanged += new EventHandler(this.OnTopLevelPropertyChanged);
                }
                return sender;
            }
        }

        [System.Windows.Forms.SRDescription("ToolTipUseAnimationDescr"), Browsable(true), DefaultValue(true)]
        public bool UseAnimation
        {
            get
            {
                return this.useAnimation;
            }
            set
            {
                if (this.useAnimation != value)
                {
                    this.useAnimation = value;
                    if (this.GetHandleCreated())
                    {
                        this.RecreateHandle();
                    }
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolTipUseFadingDescr"), DefaultValue(true), Browsable(true)]
        public bool UseFading
        {
            get
            {
                return this.useFading;
            }
            set
            {
                if (this.useFading != value)
                {
                    this.useFading = value;
                    if (this.GetHandleCreated())
                    {
                        this.RecreateHandle();
                    }
                }
            }
        }

        private class TipInfo
        {
            private string caption;
            private string designerText;
            public Point Position = Point.Empty;
            public Type TipType = Type.Auto;

            public TipInfo(string caption, Type type)
            {
                this.caption = caption;
                this.TipType = type;
                if (type == Type.Auto)
                {
                    this.designerText = caption;
                }
            }

            public string Caption
            {
                get
                {
                    if ((this.TipType & (Type.SemiAbsolute | Type.Absolute)) == Type.None)
                    {
                        return this.designerText;
                    }
                    return this.caption;
                }
                set
                {
                    this.caption = value;
                }
            }

            [Flags]
            public enum Type
            {
                Absolute = 2,
                Auto = 1,
                None = 0,
                SemiAbsolute = 4
            }
        }

        private class ToolTipNativeWindow : NativeWindow
        {
            private ToolTip control;

            internal ToolTipNativeWindow(ToolTip control)
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

        private class ToolTipTimer : Timer
        {
            private IWin32Window host;

            public ToolTipTimer(IWin32Window owner)
            {
                this.host = owner;
            }

            public IWin32Window Host
            {
                get
                {
                    return this.host;
                }
            }
        }
    }
}

