namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    [ToolboxItem(false)]
    internal sealed class TabControl : Control
    {
        private bool allowDockChange = true;
        private EventHandler idleHandler;
        private bool itemsMinimized = true;
        private System.Windows.Forms.ScrollBar scrollBar;
        private Splitter splitter;
        private const int SplitterSize = 6;
        private AnchorAlignment stripAnchor;
        private System.Workflow.ComponentModel.Design.TabStrip tabStrip;

        public TabControl(DockStyle dockStyle, AnchorAlignment stripAnchor)
        {
            if ((dockStyle == DockStyle.Fill) || (dockStyle == DockStyle.None))
            {
                throw new ArgumentException(DR.GetString("InvalidDockingStyle", new object[] { "dockStyle" }));
            }
            base.SuspendLayout();
            this.stripAnchor = stripAnchor;
            this.Dock = dockStyle;
            this.allowDockChange = false;
            if ((this.Dock == DockStyle.Left) || (this.Dock == DockStyle.Right))
            {
                base.Width = SystemInformation.VerticalScrollBarWidth + 2;
                this.splitter = new Splitter();
                this.tabStrip = new System.Workflow.ComponentModel.Design.TabStrip(Orientation.Vertical, SystemInformation.VerticalScrollBarWidth);
                this.scrollBar = new VScrollBar();
                if (this.stripAnchor == AnchorAlignment.Near)
                {
                    this.tabStrip.Dock = DockStyle.Top;
                    this.splitter.Dock = DockStyle.Top;
                    this.scrollBar.Dock = DockStyle.Fill;
                }
                else
                {
                    this.tabStrip.Dock = DockStyle.Bottom;
                    this.splitter.Dock = DockStyle.Bottom;
                    this.scrollBar.Dock = DockStyle.Fill;
                }
            }
            else
            {
                base.Height = SystemInformation.HorizontalScrollBarHeight + 2;
                this.splitter = new Splitter();
                this.tabStrip = new System.Workflow.ComponentModel.Design.TabStrip(Orientation.Horizontal, SystemInformation.HorizontalScrollBarHeight);
                this.scrollBar = new HScrollBar();
                if (this.stripAnchor == AnchorAlignment.Near)
                {
                    this.tabStrip.Dock = DockStyle.Left;
                    this.splitter.Dock = DockStyle.Left;
                    this.scrollBar.Dock = DockStyle.Fill;
                }
                else
                {
                    this.tabStrip.Dock = DockStyle.Right;
                    this.splitter.Dock = DockStyle.Right;
                    this.scrollBar.Dock = DockStyle.Fill;
                }
            }
            base.Controls.AddRange(new Control[] { this.scrollBar, this.splitter, this.tabStrip });
            this.splitter.Size = new Size(6, 6);
            this.splitter.Paint += new PaintEventHandler(this.OnSplitterPaint);
            this.splitter.DoubleClick += new EventHandler(this.OnSplitterDoubleClick);
            ((ItemList<System.Workflow.ComponentModel.Design.ItemInfo>) this.TabStrip.Tabs).ListChanged += new ItemListChangeEventHandler<System.Workflow.ComponentModel.Design.ItemInfo>(this.OnTabsChanged);
            this.BackColor = SystemColors.Control;
            base.ResumeLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.idleHandler != null)
            {
                Application.Idle -= this.idleHandler;
                this.idleHandler = null;
            }
            base.Dispose(disposing);
        }

        protected override void OnDockChanged(EventArgs e)
        {
            if (!this.allowDockChange)
            {
                throw new InvalidOperationException(SR.GetString("Error_ChangingDock"));
            }
        }

        private void OnIdle(object sender, EventArgs e)
        {
            Application.Idle -= this.idleHandler;
            this.idleHandler = null;
            if ((this.splitter.Dock == DockStyle.Left) || (this.splitter.Dock == DockStyle.Right))
            {
                if (!this.itemsMinimized && (this.splitter.SplitPosition != (base.Width - this.splitter.MinExtra)))
                {
                    this.splitter.SplitPosition = base.Width - this.splitter.MinExtra;
                }
            }
            else if (!this.itemsMinimized && (this.splitter.SplitPosition != (base.Height - this.splitter.MinExtra)))
            {
                this.splitter.SplitPosition = base.Height - this.splitter.MinExtra;
            }
            if (this.itemsMinimized && (this.splitter.SplitPosition > this.splitter.MinSize))
            {
                this.splitter.SplitPosition = this.splitter.MinSize;
            }
            if (this.splitter.SplitPosition < this.splitter.MinSize)
            {
                this.splitter.SplitPosition = this.splitter.MinSize;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            bool flag = this.splitter.SplitPosition < this.splitter.MinSize;
            if ((this.splitter.Dock == DockStyle.Left) || (this.splitter.Dock == DockStyle.Right))
            {
                int num = Math.Max(this.splitter.MinSize, (base.Width - this.tabStrip.MaximumRequiredSize) - this.splitter.Width);
                if (this.splitter.MinExtra != num)
                {
                    this.splitter.MinExtra = num;
                }
                flag |= this.itemsMinimized ? (this.splitter.SplitPosition != this.splitter.MinSize) : (this.splitter.SplitPosition != (base.Width - this.splitter.MinExtra));
            }
            else
            {
                int num2 = Math.Max(this.splitter.MinSize, (base.Height - this.tabStrip.MaximumRequiredSize) - this.splitter.Height);
                if (this.splitter.MinExtra != num2)
                {
                    this.splitter.MinExtra = num2;
                }
                flag |= this.itemsMinimized ? (this.splitter.SplitPosition != this.splitter.MinSize) : (this.splitter.SplitPosition != (base.Height - this.splitter.MinExtra));
            }
            if (flag && (this.idleHandler == null))
            {
                this.idleHandler = new EventHandler(this.OnIdle);
                Application.Idle += this.idleHandler;
            }
        }

        private void OnSplitterDoubleClick(object sender, EventArgs e)
        {
            this.itemsMinimized = !this.itemsMinimized;
            if (!this.itemsMinimized)
            {
                this.splitter.SplitPosition = (((this.splitter.Dock == DockStyle.Left) || (this.splitter.Dock == DockStyle.Right)) ? base.Width : base.Height) - this.splitter.MinExtra;
            }
            else
            {
                this.splitter.SplitPosition = this.splitter.MinSize;
            }
        }

        private void OnSplitterPaint(object sender, PaintEventArgs e)
        {
            Rectangle clientRectangle = base.ClientRectangle;
            if ((this.splitter.Dock == DockStyle.Left) || (this.splitter.Dock == DockStyle.Right))
            {
                e.Graphics.DrawLine(SystemPens.ControlLightLight, 0, 0, 0, this.splitter.Height);
                e.Graphics.DrawLine(SystemPens.ControlLightLight, 0, 0, 5, 0);
                e.Graphics.DrawLine(SystemPens.ControlDark, 4, 0, 4, this.splitter.Height - 1);
                e.Graphics.DrawLine(SystemPens.ControlDark, 4, this.splitter.Height - 1, 0, this.splitter.Height - 1);
                e.Graphics.DrawLine(SystemPens.ControlText, 5, 0, 5, this.splitter.Height);
            }
            else
            {
                e.Graphics.DrawLine(SystemPens.ControlLightLight, 0, 1, this.splitter.Width, 1);
                e.Graphics.DrawLine(SystemPens.ControlLightLight, 0, 1, 0, 5);
                e.Graphics.DrawLine(SystemPens.ControlDark, 0, 4, this.splitter.Width, 4);
                e.Graphics.DrawLine(SystemPens.ControlDark, this.splitter.Width - 1, 4, this.splitter.Width - 1, 1);
                e.Graphics.DrawLine(SystemPens.ControlText, 0, 5, this.splitter.Width, 5);
            }
        }

        private void OnTabsChanged(object sender, ItemListChangeEventArgs<System.Workflow.ComponentModel.Design.ItemInfo> e)
        {
            if ((this.splitter.Dock == DockStyle.Left) || (this.splitter.Dock == DockStyle.Right))
            {
                this.splitter.MinExtra = (base.Width - this.tabStrip.MaximumRequiredSize) - this.splitter.Width;
                this.splitter.MinSize = this.tabStrip.MinimumRequiredSize;
            }
            else if ((this.splitter.Dock == DockStyle.Top) || (this.splitter.Dock == DockStyle.Bottom))
            {
                this.splitter.MinExtra = (base.Height - this.tabStrip.MaximumRequiredSize) - this.splitter.Height;
                this.splitter.MinSize = this.tabStrip.MinimumRequiredSize;
            }
        }

        public System.Windows.Forms.ScrollBar ScrollBar
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.scrollBar;
            }
        }

        public System.Workflow.ComponentModel.Design.TabStrip TabStrip
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.tabStrip;
            }
        }
    }
}

