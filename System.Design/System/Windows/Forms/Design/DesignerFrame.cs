namespace System.Windows.Forms.Design
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class DesignerFrame : Control, IOverlayService, ISplitWindowService
    {
        private System.Windows.Forms.Design.Behavior.BehaviorService behaviorService;
        private Control designer;
        private OverlayControl designerRegion;
        private ISite designerSite;
        private Splitter splitter;

        public DesignerFrame(ISite site)
        {
            this.Text = "DesignerFrame";
            this.designerSite = site;
            this.designerRegion = new OverlayControl(site);
            base.Controls.Add(this.designerRegion);
            this.designerRegion.AutoScroll = true;
            this.designerRegion.Dock = DockStyle.Fill;
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.designer != null))
            {
                Control designer = this.designer;
                this.designer = null;
                designer.Visible = false;
                designer.Parent = null;
                SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
            }
            base.Dispose(disposing);
        }

        private void ForceDesignerRedraw(bool focus)
        {
            if ((this.designer != null) && this.designer.IsHandleCreated)
            {
                System.Design.NativeMethods.SendMessage(this.designer.Handle, 0x86, focus ? 1 : 0, 0);
                System.Design.SafeNativeMethods.RedrawWindow(this.designer.Handle, null, IntPtr.Zero, 0x400);
            }
        }

        public void Initialize(Control view)
        {
            this.designer = view;
            Form designer = this.designer as Form;
            if (designer != null)
            {
                designer.TopLevel = false;
            }
            this.designerRegion.Controls.Add(this.designer);
            this.SyncDesignerUI();
            this.designer.Visible = true;
            this.designer.Enabled = true;
            IntPtr handle = this.designer.Handle;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            this.ForceDesignerRedraw(true);
            ISelectionService service = (ISelectionService) this.designerSite.GetService(typeof(ISelectionService));
            if (service != null)
            {
                Control primarySelection = service.PrimarySelection as Control;
                if (primarySelection != null)
                {
                    System.Design.UnsafeNativeMethods.NotifyWinEvent(0x8005, new HandleRef(primarySelection, primarySelection.Handle), -4, 0);
                }
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            this.ForceDesignerRedraw(false);
        }

        private void OnSplitterMoved(object sender, SplitterEventArgs e)
        {
            IComponentChangeService service = this.designerSite.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service != null)
            {
                try
                {
                    service.OnComponentChanging(this.designerSite.Component, null);
                    service.OnComponentChanged(this.designerSite.Component, null, null, null);
                }
                catch
                {
                }
            }
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Window)
            {
                this.SyncDesignerUI();
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            return false;
        }

        private void SyncDesignerUI()
        {
            Size adornmentDimensions = DesignerUtils.GetAdornmentDimensions(AdornmentType.Maximum);
            this.designerRegion.AutoScrollMargin = adornmentDimensions;
            this.designer.Location = new Point(adornmentDimensions.Width, adornmentDimensions.Height);
            if (this.BehaviorService != null)
            {
                this.BehaviorService.SyncSelection();
            }
        }

        void IOverlayService.InsertOverlay(Control control, int index)
        {
            this.designerRegion.InsertOverlay(control, index);
        }

        void IOverlayService.InvalidateOverlays(Rectangle screenRectangle)
        {
            this.designerRegion.InvalidateOverlays(screenRectangle);
        }

        void IOverlayService.InvalidateOverlays(Region screenRegion)
        {
            this.designerRegion.InvalidateOverlays(screenRegion);
        }

        int IOverlayService.PushOverlay(Control control)
        {
            return this.designerRegion.PushOverlay(control);
        }

        void IOverlayService.RemoveOverlay(Control control)
        {
            this.designerRegion.RemoveOverlay(control);
        }

        void ISplitWindowService.AddSplitWindow(Control window)
        {
            if (this.splitter == null)
            {
                this.splitter = new Splitter();
                this.splitter.BackColor = SystemColors.Control;
                this.splitter.BorderStyle = BorderStyle.Fixed3D;
                this.splitter.Height = 7;
                this.splitter.Dock = DockStyle.Bottom;
                this.splitter.SplitterMoved += new SplitterEventHandler(this.OnSplitterMoved);
            }
            base.SuspendLayout();
            window.Dock = DockStyle.Bottom;
            int num = 80;
            if (window.Height < num)
            {
                window.Height = num;
            }
            base.Controls.Add(this.splitter);
            base.Controls.Add(window);
            base.ResumeLayout();
        }

        void ISplitWindowService.RemoveSplitWindow(Control window)
        {
            base.SuspendLayout();
            base.Controls.Remove(window);
            base.Controls.Remove(this.splitter);
            base.ResumeLayout();
        }

        protected override void WndProc(ref Message m)
        {
            int num;
            int num2;
            int msg = m.Msg;
            switch (msg)
            {
                case 0x7b:
                    System.Design.NativeMethods.SendMessage(this.designer.Handle, m.Msg, m.WParam, m.LParam);
                    return;

                case 0x100:
                {
                    num = 0;
                    num2 = 0;
                    int num3 = ((int) ((long) m.WParam)) & 0xffff;
                    switch (((Keys) num3))
                    {
                        case Keys.PageUp:
                            num = 2;
                            num2 = 0x115;
                            goto Label_00F5;

                        case Keys.Next:
                            num = 3;
                            num2 = 0x115;
                            goto Label_00F5;

                        case Keys.End:
                            num = 7;
                            num2 = 0x115;
                            goto Label_00F5;

                        case Keys.Home:
                            num = 6;
                            num2 = 0x115;
                            goto Label_00F5;

                        case Keys.Left:
                            num = 0;
                            num2 = 0x114;
                            goto Label_00F5;

                        case Keys.Up:
                            num = 0;
                            num2 = 0x115;
                            goto Label_00F5;

                        case Keys.Right:
                            num = 1;
                            num2 = 0x114;
                            goto Label_00F5;

                        case Keys.Down:
                            num = 1;
                            num2 = 0x115;
                            goto Label_00F5;
                    }
                    break;
                }
                default:
                    if ((msg == 0x20a) && !this.designerRegion.messageMouseWheelProcessed)
                    {
                        this.designerRegion.messageMouseWheelProcessed = true;
                        System.Design.NativeMethods.SendMessage(this.designerRegion.Handle, 0x20a, m.WParam, m.LParam);
                        return;
                    }
                    goto Label_0144;
            }
        Label_00F5:
            switch (num2)
            {
                case 0x115:
                case 0x114:
                    System.Design.NativeMethods.SendMessage(this.designerRegion.Handle, num2, System.Design.NativeMethods.Util.MAKELONG(num, 0), 0);
                    return;
            }
        Label_0144:
            base.WndProc(ref m);
        }

        internal Point AutoScrollPosition
        {
            get
            {
                return this.designerRegion.AutoScrollPosition;
            }
        }

        private System.Windows.Forms.Design.Behavior.BehaviorService BehaviorService
        {
            get
            {
                if (this.behaviorService == null)
                {
                    this.behaviorService = this.designerSite.GetService(typeof(System.Windows.Forms.Design.Behavior.BehaviorService)) as System.Windows.Forms.Design.Behavior.BehaviorService;
                }
                return this.behaviorService;
            }
        }

        private class OverlayControl : ScrollableControl
        {
            private System.Windows.Forms.Design.Behavior.BehaviorService behaviorService;
            internal bool messageMouseWheelProcessed;
            private ArrayList overlayList;
            private IServiceProvider provider;

            public OverlayControl(IServiceProvider provider)
            {
                this.provider = provider;
                this.overlayList = new ArrayList();
                this.AutoScroll = true;
                this.Text = "OverlayControl";
            }

            protected override AccessibleObject CreateAccessibilityInstance()
            {
                return new OverlayControlAccessibleObject(this);
            }

            public void InsertOverlay(Control control, int index)
            {
                Control control2 = (Control) this.overlayList[index];
                this.RemoveOverlay(control2);
                this.PushOverlay(control);
                this.PushOverlay(control2);
                control2.Visible = true;
            }

            public void InvalidateOverlays(Rectangle screenRectangle)
            {
                for (int i = this.overlayList.Count - 1; i >= 0; i--)
                {
                    Control control = this.overlayList[i] as Control;
                    if (control != null)
                    {
                        Rectangle rect = new Rectangle(control.PointToClient(screenRectangle.Location), screenRectangle.Size);
                        if (control.ClientRectangle.IntersectsWith(rect))
                        {
                            control.Invalidate(rect);
                        }
                    }
                }
            }

            public void InvalidateOverlays(Region screenRegion)
            {
                for (int i = this.overlayList.Count - 1; i >= 0; i--)
                {
                    Control control = this.overlayList[i] as Control;
                    if (control != null)
                    {
                        Rectangle bounds = control.Bounds;
                        bounds.Location = control.PointToScreen(control.Location);
                        using (Region region = screenRegion.Clone())
                        {
                            region.Intersect(bounds);
                            region.Translate(-bounds.X, -bounds.Y);
                            control.Invalidate(region);
                        }
                    }
                }
            }

            protected override void OnCreateControl()
            {
                base.OnCreateControl();
                if (this.overlayList != null)
                {
                    foreach (Control control in this.overlayList)
                    {
                        this.ParentOverlay(control);
                    }
                }
                if (this.BehaviorService != null)
                {
                    this.BehaviorService.SyncSelection();
                }
            }

            protected override void OnLayout(LayoutEventArgs e)
            {
                base.OnLayout(e);
                Rectangle displayRectangle = this.DisplayRectangle;
                if (this.overlayList != null)
                {
                    foreach (Control control in this.overlayList)
                    {
                        control.Bounds = displayRectangle;
                    }
                }
            }

            private void ParentOverlay(Control control)
            {
                System.Design.NativeMethods.SetParent(control.Handle, base.Handle);
                System.Design.SafeNativeMethods.SetWindowPos(control.Handle, IntPtr.Zero, 0, 0, 0, 0, 3);
            }

            public int PushOverlay(Control control)
            {
                this.overlayList.Add(control);
                if (base.IsHandleCreated)
                {
                    this.ParentOverlay(control);
                    control.Bounds = this.DisplayRectangle;
                }
                return this.overlayList.IndexOf(control);
            }

            public void RemoveOverlay(Control control)
            {
                this.overlayList.Remove(control);
                control.Visible = false;
                control.Parent = null;
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if ((m.Msg != 0x210) || (System.Design.NativeMethods.Util.LOWORD((int) ((long) m.WParam)) != 1))
                {
                    if (((m.Msg == 0x115) || (m.Msg == 0x114)) && (this.BehaviorService != null))
                    {
                        this.BehaviorService.SyncSelection();
                    }
                    else if (m.Msg == 0x20a)
                    {
                        this.messageMouseWheelProcessed = false;
                        if (this.BehaviorService != null)
                        {
                            this.BehaviorService.SyncSelection();
                        }
                    }
                }
                else if (this.overlayList != null)
                {
                    bool flag = false;
                    foreach (Control control in this.overlayList)
                    {
                        if (control.IsHandleCreated && (m.LParam == control.Handle))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        foreach (Control control2 in this.overlayList)
                        {
                            System.Design.SafeNativeMethods.SetWindowPos(control2.Handle, IntPtr.Zero, 0, 0, 0, 0, 3);
                        }
                    }
                }
            }

            private System.Windows.Forms.Design.Behavior.BehaviorService BehaviorService
            {
                get
                {
                    if (this.behaviorService == null)
                    {
                        this.behaviorService = this.provider.GetService(typeof(System.Windows.Forms.Design.Behavior.BehaviorService)) as System.Windows.Forms.Design.Behavior.BehaviorService;
                    }
                    return this.behaviorService;
                }
            }

            public class OverlayControlAccessibleObject : Control.ControlAccessibleObject
            {
                public OverlayControlAccessibleObject(DesignerFrame.OverlayControl owner) : base(owner)
                {
                }

                public override AccessibleObject HitTest(int x, int y)
                {
                    foreach (Control control in base.Owner.Controls)
                    {
                        AccessibleObject accessibilityObject = control.AccessibilityObject;
                        if (accessibilityObject.Bounds.Contains(x, y))
                        {
                            return accessibilityObject;
                        }
                    }
                    return base.HitTest(x, y);
                }
            }
        }
    }
}

