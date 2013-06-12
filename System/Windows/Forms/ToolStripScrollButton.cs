namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Security.Permissions;

    internal class ToolStripScrollButton : ToolStripControlHost
    {
        private static readonly int AUTOSCROLL_PAUSE = SystemInformation.DoubleClickTime;
        private const int AUTOSCROLL_UPDATE = 50;
        [ThreadStatic]
        private static Bitmap downScrollImage;
        private Timer mouseDownTimer;
        private bool up;
        [ThreadStatic]
        private static Bitmap upScrollImage;

        public ToolStripScrollButton(bool up) : base(CreateControlInstance(up))
        {
            this.up = true;
            this.up = up;
        }

        private static Control CreateControlInstance(bool up)
        {
            return new StickyLabel { ImageAlign = ContentAlignment.MiddleCenter, Image = up ? UpImage : DownImage };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.mouseDownTimer != null))
            {
                this.mouseDownTimer.Enabled = false;
                this.mouseDownTimer.Dispose();
                this.mouseDownTimer = null;
            }
            base.Dispose(disposing);
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            Size empty = Size.Empty;
            empty.Height = (this.Label.Image != null) ? (this.Label.Image.Height + 4) : 0;
            empty.Width = (base.ParentInternal != null) ? (base.ParentInternal.Width - 2) : empty.Width;
            return empty;
        }

        private void OnAutoScrollAccellerate(object sender, EventArgs e)
        {
            this.Scroll();
        }

        private void OnInitialAutoScrollMouseDown(object sender, EventArgs e)
        {
            this.MouseDownTimer.Tick -= new EventHandler(this.OnInitialAutoScrollMouseDown);
            this.Scroll();
            this.MouseDownTimer.Interval = 50;
            this.MouseDownTimer.Tick += new EventHandler(this.OnAutoScrollAccellerate);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.UnsubscribeAll();
            base.OnMouseDown(e);
            this.Scroll();
            this.MouseDownTimer.Interval = AUTOSCROLL_PAUSE;
            this.MouseDownTimer.Tick += new EventHandler(this.OnInitialAutoScrollMouseDown);
            this.MouseDownTimer.Enabled = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.UnsubscribeAll();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.UnsubscribeAll();
            base.OnMouseUp(e);
        }

        private void Scroll()
        {
            ToolStripDropDownMenu parentInternal = base.ParentInternal as ToolStripDropDownMenu;
            if ((parentInternal != null) && this.Label.Enabled)
            {
                parentInternal.ScrollInternal(this.up);
            }
        }

        private void UnsubscribeAll()
        {
            this.MouseDownTimer.Enabled = false;
            this.MouseDownTimer.Tick -= new EventHandler(this.OnInitialAutoScrollMouseDown);
            this.MouseDownTimer.Tick -= new EventHandler(this.OnAutoScrollAccellerate);
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
                return Padding.Empty;
            }
        }

        private static Image DownImage
        {
            get
            {
                if (downScrollImage == null)
                {
                    downScrollImage = new Bitmap(typeof(ToolStripScrollButton), "ScrollButtonDown.bmp");
                    downScrollImage.MakeTransparent(Color.White);
                }
                return downScrollImage;
            }
        }

        internal StickyLabel Label
        {
            get
            {
                return (base.Control as StickyLabel);
            }
        }

        private Timer MouseDownTimer
        {
            get
            {
                if (this.mouseDownTimer == null)
                {
                    this.mouseDownTimer = new Timer();
                }
                return this.mouseDownTimer;
            }
        }

        private static Image UpImage
        {
            get
            {
                if (upScrollImage == null)
                {
                    upScrollImage = new Bitmap(typeof(ToolStripScrollButton), "ScrollButtonUp.bmp");
                    upScrollImage.MakeTransparent(Color.White);
                }
                return upScrollImage;
            }
        }

        internal class StickyLabel : Label
        {
            private bool freezeLocationChange;

            protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
            {
                if (((specified & BoundsSpecified.Location) == BoundsSpecified.None) || !this.FreezeLocationChange)
                {
                    base.SetBoundsCore(x, y, width, height, specified);
                }
            }

            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            protected override void WndProc(ref Message m)
            {
                if ((m.Msg >= 0x100) && (m.Msg <= 0x108))
                {
                    this.DefWndProc(ref m);
                }
                else
                {
                    base.WndProc(ref m);
                }
            }

            public bool FreezeLocationChange
            {
                get
                {
                    return this.freezeLocationChange;
                }
            }
        }
    }
}

