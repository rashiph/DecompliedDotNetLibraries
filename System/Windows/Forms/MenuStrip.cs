namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), System.Windows.Forms.SRDescription("DescriptionMenuStrip"), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class MenuStrip : ToolStrip
    {
        private static readonly object EventMenuActivate = new object();
        private static readonly object EventMenuDeactivate = new object();
        private ToolStripMenuItem mdiWindowListItem;

        [System.Windows.Forms.SRDescription("MenuStripMenuActivateDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler MenuActivate
        {
            add
            {
                base.Events.AddHandler(EventMenuActivate, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMenuActivate, value);
            }
        }

        [System.Windows.Forms.SRDescription("MenuStripMenuDeactivateDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler MenuDeactivate
        {
            add
            {
                base.Events.AddHandler(EventMenuDeactivate, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventMenuDeactivate, value);
            }
        }

        public MenuStrip()
        {
            this.CanOverflow = false;
            this.GripStyle = ToolStripGripStyle.Hidden;
            this.Stretch = true;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new MenuStripAccessibleObject(this);
        }

        protected internal override ToolStripItem CreateDefaultItem(string text, Image image, EventHandler onClick)
        {
            if (text == "-")
            {
                return new ToolStripSeparator();
            }
            return new ToolStripMenuItem(text, image, onClick);
        }

        protected virtual void OnMenuActivate(EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                base.AccessibilityNotifyClients(AccessibleEvents.SystemMenuStart, -1);
            }
            EventHandler handler = (EventHandler) base.Events[EventMenuActivate];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnMenuDeactivate(EventArgs e)
        {
            if (base.IsHandleCreated)
            {
                base.AccessibilityNotifyClients(AccessibleEvents.SystemMenuEnd, -1);
            }
            EventHandler handler = (EventHandler) base.Events[EventMenuDeactivate];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal bool OnMenuKey()
        {
            if (this.Focused || base.ContainsFocus)
            {
                return false;
            }
            ToolStripManager.ModalMenuFilter.SetActiveToolStrip(this, true);
            if (this.DisplayedItems.Count > 0)
            {
                if (this.DisplayedItems[0] is MdiControlStrip.SystemMenuItem)
                {
                    base.SelectNextToolStripItem(this.DisplayedItems[0], true);
                }
                else
                {
                    base.SelectNextToolStripItem(null, this.RightToLeft == RightToLeft.No);
                }
            }
            return true;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            if ((!ToolStripManager.ModalMenuFilter.InMenuMode || (keyData != Keys.Space)) || (!this.Focused && base.ContainsFocus))
            {
                return base.ProcessCmdKey(ref m, keyData);
            }
            base.NotifySelectionChange(null);
            ToolStripManager.ModalMenuFilter.ExitMenuMode();
            System.Windows.Forms.UnsafeNativeMethods.PostMessage(WindowsFormsUtils.GetRootHWnd(this), 0x112, 0xf100, 0x20);
            return true;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == 0x21) && (base.ActiveDropDowns.Count == 0))
            {
                Point point = base.PointToClient(WindowsFormsUtils.LastCursorPoint);
                ToolStripItem itemAt = base.GetItemAt(point);
                if ((itemAt != null) && !(itemAt is ToolStripControlHost))
                {
                    this.KeyboardActive = true;
                }
            }
            base.WndProc(ref m);
        }

        [System.Windows.Forms.SRDescription("ToolStripCanOverflowDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatLayout"), Browsable(false)]
        public bool CanOverflow
        {
            get
            {
                return base.CanOverflow;
            }
            set
            {
                base.CanOverflow = value;
            }
        }

        protected override Padding DefaultGripMargin
        {
            get
            {
                return new Padding(2, 2, 0, 2);
            }
        }

        protected override Padding DefaultPadding
        {
            get
            {
                if (this.GripStyle == ToolStripGripStyle.Visible)
                {
                    return new Padding(3, 2, 0, 2);
                }
                return new Padding(6, 2, 0, 2);
            }
        }

        protected override bool DefaultShowItemToolTips
        {
            get
            {
                return false;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(200, 0x18);
            }
        }

        [DefaultValue(0)]
        public ToolStripGripStyle GripStyle
        {
            get
            {
                return base.GripStyle;
            }
            set
            {
                base.GripStyle = value;
            }
        }

        internal override bool KeyboardActive
        {
            get
            {
                return base.KeyboardActive;
            }
            set
            {
                if (base.KeyboardActive != value)
                {
                    base.KeyboardActive = value;
                    if (value)
                    {
                        this.OnMenuActivate(EventArgs.Empty);
                    }
                    else
                    {
                        this.OnMenuDeactivate(EventArgs.Empty);
                    }
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), MergableProperty(false), TypeConverter(typeof(MdiWindowListItemConverter)), System.Windows.Forms.SRDescription("MenuStripMdiWindowListItem"), DefaultValue((string) null)]
        public ToolStripMenuItem MdiWindowListItem
        {
            get
            {
                return this.mdiWindowListItem;
            }
            set
            {
                this.mdiWindowListItem = value;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripShowItemToolTipsDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool ShowItemToolTips
        {
            get
            {
                return base.ShowItemToolTips;
            }
            set
            {
                base.ShowItemToolTips = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(true), System.Windows.Forms.SRDescription("ToolStripStretchDescr")]
        public bool Stretch
        {
            get
            {
                return base.Stretch;
            }
            set
            {
                base.Stretch = value;
            }
        }

        [ComVisible(true)]
        internal class MenuStripAccessibleObject : ToolStrip.ToolStripAccessibleObject
        {
            public MenuStripAccessibleObject(MenuStrip owner) : base(owner)
            {
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
                    return AccessibleRole.MenuBar;
                }
            }
        }
    }
}

