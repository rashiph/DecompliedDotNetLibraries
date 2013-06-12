namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Printing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [System.Windows.Forms.SRDescription("DescriptionPrintPreviewControl"), DefaultProperty("Document"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class PrintPreviewControl : Control
    {
        private bool antiAlias;
        private bool autoZoom = true;
        private const int border = 10;
        private int columns = 1;
        private const double DefaultZoom = 0.3;
        private PrintDocument document;
        private static readonly object EVENT_STARTPAGECHANGED = new object();
        private bool exceptionPrinting;
        private Size imageSize = Size.Empty;
        private Point lastOffset;
        private bool layoutOk;
        private PreviewPageInfo[] pageInfo;
        private bool pageInfoCalcPending;
        private Point position = new Point(0, 0);
        private int rows = 1;
        private Point screendpi = Point.Empty;
        private const int SCROLL_LINE = 5;
        private const int SCROLL_PAGE = 100;
        private int startPage;
        private Size virtualSize = new Size(1, 1);
        private double zoom = 0.3;

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("RadioButtonOnStartPageChangedDescr")]
        public event EventHandler StartPageChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_STARTPAGECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_STARTPAGECHANGED, value);
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

        public PrintPreviewControl()
        {
            this.ResetBackColor();
            this.ResetForeColor();
            base.Size = new Size(100, 100);
            base.SetStyle(ControlStyles.ResizeRedraw, false);
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.Opaque, true);
        }

        private int AdjustScroll(Message m, int pos, int maxPos, bool horizontal)
        {
            switch (System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam))
            {
                case 0:
                    if (pos <= 5)
                    {
                        pos = 0;
                        return pos;
                    }
                    pos -= 5;
                    return pos;

                case 1:
                    if (pos >= (maxPos - 5))
                    {
                        pos = maxPos;
                        return pos;
                    }
                    pos += 5;
                    return pos;

                case 2:
                    if (pos <= 100)
                    {
                        pos = 0;
                        return pos;
                    }
                    pos -= 100;
                    return pos;

                case 3:
                    if (pos >= (maxPos - 100))
                    {
                        pos = maxPos;
                        return pos;
                    }
                    pos += 100;
                    return pos;

                case 4:
                case 5:
                {
                    System.Windows.Forms.NativeMethods.SCROLLINFO si = new System.Windows.Forms.NativeMethods.SCROLLINFO {
                        cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.SCROLLINFO)),
                        fMask = 0x10
                    };
                    int fnBar = horizontal ? 0 : 1;
                    if (!System.Windows.Forms.SafeNativeMethods.GetScrollInfo(new HandleRef(this, m.HWnd), fnBar, si))
                    {
                        pos = System.Windows.Forms.NativeMethods.Util.HIWORD(m.WParam);
                        return pos;
                    }
                    pos = si.nTrackPos;
                    return pos;
                }
            }
            return pos;
        }

        private void CalculatePageInfo()
        {
            if (!this.pageInfoCalcPending)
            {
                this.pageInfoCalcPending = true;
                try
                {
                    if (this.pageInfo == null)
                    {
                        try
                        {
                            this.ComputePreview();
                        }
                        catch
                        {
                            this.exceptionPrinting = true;
                            throw;
                        }
                        finally
                        {
                            base.Invalidate();
                        }
                    }
                }
                finally
                {
                    this.pageInfoCalcPending = false;
                }
            }
        }

        private void ComputeLayout()
        {
            this.layoutOk = true;
            if (this.pageInfo.Length == 0)
            {
                base.ClientSize = base.Size;
            }
            else
            {
                Graphics wrapper = base.CreateGraphicsInternal();
                IntPtr hdc = wrapper.GetHdc();
                this.screendpi = new Point(System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(wrapper, hdc), 0x58), System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(wrapper, hdc), 90));
                wrapper.ReleaseHdcInternal(hdc);
                wrapper.Dispose();
                Size physicalSize = this.pageInfo[this.StartPage].PhysicalSize;
                Size size2 = new Size(PixelsToPhysical(new Point(base.Size), this.screendpi));
                if (this.autoZoom)
                {
                    double num = (size2.Width - (10 * (this.columns + 1))) / ((double) (this.columns * physicalSize.Width));
                    double num2 = (size2.Height - (10 * (this.rows + 1))) / ((double) (this.rows * physicalSize.Height));
                    this.zoom = Math.Min(num, num2);
                }
                this.imageSize = new Size((int) (this.zoom * physicalSize.Width), (int) (this.zoom * physicalSize.Height));
                int x = (this.imageSize.Width * this.columns) + (10 * (this.columns + 1));
                int y = (this.imageSize.Height * this.rows) + (10 * (this.rows + 1));
                this.SetVirtualSizeNoInvalidate(new Size(PhysicalToPixels(new Point(x, y), this.screendpi)));
            }
        }

        private void ComputePreview()
        {
            int startPage = this.StartPage;
            if (this.document == null)
            {
                this.pageInfo = new PreviewPageInfo[0];
            }
            else
            {
                System.Windows.Forms.IntSecurity.SafePrinting.Demand();
                PrintController printController = this.document.PrintController;
                PreviewPrintController underlyingController = new PreviewPrintController {
                    UseAntiAlias = this.UseAntiAlias
                };
                this.document.PrintController = new PrintControllerWithStatusDialog(underlyingController, System.Windows.Forms.SR.GetString("PrintControllerWithStatusDialog_DialogTitlePreview"));
                this.document.Print();
                this.pageInfo = underlyingController.GetPreviewPageInfo();
                this.document.PrintController = printController;
            }
            if (startPage != this.StartPage)
            {
                this.OnStartPageChanged(EventArgs.Empty);
            }
        }

        private void InvalidateLayout()
        {
            this.layoutOk = false;
            base.Invalidate();
        }

        public void InvalidatePreview()
        {
            this.pageInfo = null;
            this.InvalidateLayout();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            using (Brush brush = new SolidBrush(this.BackColor))
            {
                if ((this.pageInfo == null) || (this.pageInfo.Length == 0))
                {
                    pevent.Graphics.FillRectangle(brush, base.ClientRectangle);
                    if ((this.pageInfo != null) || this.exceptionPrinting)
                    {
                        StringFormat format = new StringFormat {
                            Alignment = ControlPaint.TranslateAlignment(ContentAlignment.MiddleCenter),
                            LineAlignment = ControlPaint.TranslateLineAlignment(ContentAlignment.MiddleCenter)
                        };
                        SolidBrush brush2 = new SolidBrush(this.ForeColor);
                        try
                        {
                            if (this.exceptionPrinting)
                            {
                                pevent.Graphics.DrawString(System.Windows.Forms.SR.GetString("PrintPreviewExceptionPrinting"), this.Font, brush2, base.ClientRectangle, format);
                            }
                            else
                            {
                                pevent.Graphics.DrawString(System.Windows.Forms.SR.GetString("PrintPreviewNoPages"), this.Font, brush2, base.ClientRectangle, format);
                            }
                            goto Label_04E9;
                        }
                        finally
                        {
                            brush2.Dispose();
                            format.Dispose();
                        }
                    }
                    base.BeginInvoke(new MethodInvoker(this.CalculatePageInfo));
                }
                else
                {
                    Point point2;
                    if (!this.layoutOk)
                    {
                        this.ComputeLayout();
                    }
                    Size size = new Size(PixelsToPhysical(new Point(base.Size), this.screendpi));
                    Point point = new Point(this.VirtualSize);
                    point2 = new Point(Math.Max(0, (base.Size.Width - point.X) / 2), Math.Max(0, (base.Size.Height - point.Y) / 2)) {
                        X = point2.X - this.Position.X,
                        Y = point2.Y - this.Position.Y
                    };
                    this.lastOffset = point2;
                    int num = PhysicalToPixels(10, this.screendpi.X);
                    int num2 = PhysicalToPixels(10, this.screendpi.Y);
                    Region clip = pevent.Graphics.Clip;
                    Rectangle[] rectangleArray = new Rectangle[this.rows * this.columns];
                    Point empty = Point.Empty;
                    int num3 = 0;
                    try
                    {
                        for (int j = 0; j < this.rows; j++)
                        {
                            empty.X = 0;
                            empty.Y = num3 * j;
                            for (int k = 0; k < this.columns; k++)
                            {
                                int index = (this.StartPage + k) + (j * this.columns);
                                if (index < this.pageInfo.Length)
                                {
                                    Size physicalSize = this.pageInfo[index].PhysicalSize;
                                    if (this.autoZoom)
                                    {
                                        double num7 = (size.Width - (10 * (this.columns + 1))) / ((double) (this.columns * physicalSize.Width));
                                        double num8 = (size.Height - (10 * (this.rows + 1))) / ((double) (this.rows * physicalSize.Height));
                                        this.zoom = Math.Min(num7, num8);
                                    }
                                    this.imageSize = new Size((int) (this.zoom * physicalSize.Width), (int) (this.zoom * physicalSize.Height));
                                    Point point4 = PhysicalToPixels(new Point(this.imageSize), this.screendpi);
                                    int x = (point2.X + (num * (k + 1))) + empty.X;
                                    int y = (point2.Y + (num2 * (j + 1))) + empty.Y;
                                    empty.X += point4.X;
                                    num3 = Math.Max(num3, point4.Y);
                                    rectangleArray[index - this.StartPage] = new Rectangle(x, y, point4.X, point4.Y);
                                    pevent.Graphics.ExcludeClip(rectangleArray[index - this.StartPage]);
                                }
                            }
                        }
                        pevent.Graphics.FillRectangle(brush, base.ClientRectangle);
                    }
                    finally
                    {
                        pevent.Graphics.Clip = clip;
                    }
                    for (int i = 0; i < rectangleArray.Length; i++)
                    {
                        if ((i + this.StartPage) < this.pageInfo.Length)
                        {
                            Rectangle rect = rectangleArray[i];
                            pevent.Graphics.DrawRectangle(Pens.Black, rect);
                            using (SolidBrush brush3 = new SolidBrush(this.ForeColor))
                            {
                                pevent.Graphics.FillRectangle(brush3, rect);
                            }
                            rect.Inflate(-1, -1);
                            if (this.pageInfo[i + this.StartPage].Image != null)
                            {
                                pevent.Graphics.DrawImage(this.pageInfo[i + this.StartPage].Image, rect);
                            }
                            rect.Width--;
                            rect.Height--;
                            pevent.Graphics.DrawRectangle(Pens.Black, rect);
                        }
                    }
                }
            }
        Label_04E9:
            base.OnPaint(pevent);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            this.InvalidateLayout();
            base.OnResize(eventargs);
        }

        protected virtual void OnStartPageChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_STARTPAGECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private static Point PhysicalToPixels(Point physical, Point dpi)
        {
            return new Point(PhysicalToPixels(physical.X, dpi.X), PhysicalToPixels(physical.Y, dpi.Y));
        }

        private static Size PhysicalToPixels(Size physicalSize, Point dpi)
        {
            return new Size(PhysicalToPixels(physicalSize.Width, dpi.X), PhysicalToPixels(physicalSize.Height, dpi.Y));
        }

        private static int PhysicalToPixels(int physicalSize, int dpi)
        {
            return (int) (((double) (physicalSize * dpi)) / 100.0);
        }

        private static Point PixelsToPhysical(Point pixels, Point dpi)
        {
            return new Point(PixelsToPhysical(pixels.X, dpi.X), PixelsToPhysical(pixels.Y, dpi.Y));
        }

        private static Size PixelsToPhysical(Size pixels, Point dpi)
        {
            return new Size(PixelsToPhysical(pixels.Width, dpi.X), PixelsToPhysical(pixels.Height, dpi.Y));
        }

        private static int PixelsToPhysical(int pixels, int dpi)
        {
            return (int) ((pixels * 100.0) / ((double) dpi));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ResetBackColor()
        {
            this.BackColor = SystemColors.AppWorkspace;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ResetForeColor()
        {
            this.ForeColor = Color.White;
        }

        private void SetPositionNoInvalidate(Point value)
        {
            Point position = this.position;
            this.position = value;
            this.position.X = Math.Min(this.position.X, this.virtualSize.Width - base.Width);
            this.position.Y = Math.Min(this.position.Y, this.virtualSize.Height - base.Height);
            if (this.position.X < 0)
            {
                this.position.X = 0;
            }
            if (this.position.Y < 0)
            {
                this.position.Y = 0;
            }
            Rectangle clientRectangle = base.ClientRectangle;
            System.Windows.Forms.NativeMethods.RECT rectScrollRegion = System.Windows.Forms.NativeMethods.RECT.FromXYWH(clientRectangle.X, clientRectangle.Y, clientRectangle.Width, clientRectangle.Height);
            System.Windows.Forms.SafeNativeMethods.ScrollWindow(new HandleRef(this, base.Handle), position.X - this.position.X, position.Y - this.position.Y, ref rectScrollRegion, ref rectScrollRegion);
            System.Windows.Forms.UnsafeNativeMethods.SetScrollPos(new HandleRef(this, base.Handle), 0, this.position.X, true);
            System.Windows.Forms.UnsafeNativeMethods.SetScrollPos(new HandleRef(this, base.Handle), 1, this.position.Y, true);
        }

        internal void SetVirtualSizeNoInvalidate(Size value)
        {
            this.virtualSize = value;
            this.SetPositionNoInvalidate(this.position);
            System.Windows.Forms.NativeMethods.SCROLLINFO si = new System.Windows.Forms.NativeMethods.SCROLLINFO {
                fMask = 3,
                nMin = 0,
                nMax = Math.Max(base.Height, this.virtualSize.Height) - 1,
                nPage = base.Height
            };
            System.Windows.Forms.UnsafeNativeMethods.SetScrollInfo(new HandleRef(this, base.Handle), 1, si, true);
            si.fMask = 3;
            si.nMin = 0;
            si.nMax = Math.Max(base.Width, this.virtualSize.Width) - 1;
            si.nPage = base.Width;
            System.Windows.Forms.UnsafeNativeMethods.SetScrollInfo(new HandleRef(this, base.Handle), 0, si, true);
        }

        internal override bool ShouldSerializeBackColor()
        {
            return !this.BackColor.Equals(SystemColors.AppWorkspace);
        }

        internal override bool ShouldSerializeForeColor()
        {
            return !this.ForeColor.Equals(Color.White);
        }

        private void WmHScroll(ref Message m)
        {
            if (m.LParam != IntPtr.Zero)
            {
                base.WndProc(ref m);
            }
            else
            {
                Point position = this.position;
                int x = position.X;
                int maxPos = Math.Max(base.Width, this.virtualSize.Width);
                position.X = this.AdjustScroll(m, x, maxPos, true);
                this.Position = position;
            }
        }

        private void WmKeyDown(ref Message msg)
        {
            Keys keys = ((Keys) ((int) msg.WParam)) | Control.ModifierKeys;
            Point position = this.Position;
            int x = 0;
            int num2 = 0;
            switch ((keys & Keys.KeyCode))
            {
                case Keys.PageUp:
                    if ((keys & ~Keys.KeyCode) != Keys.Control)
                    {
                        if (this.StartPage > 0)
                        {
                            this.StartPage--;
                        }
                        return;
                    }
                    x = position.X;
                    if (x <= 100)
                    {
                        x = 0;
                        break;
                    }
                    x -= 100;
                    break;

                case Keys.Next:
                    if ((keys & ~Keys.KeyCode) != Keys.Control)
                    {
                        if (this.StartPage < this.pageInfo.Length)
                        {
                            this.StartPage++;
                        }
                        return;
                    }
                    x = position.X;
                    num2 = Math.Max(base.Width, this.virtualSize.Width);
                    if (x >= (num2 - 100))
                    {
                        x = num2;
                    }
                    else
                    {
                        x += 100;
                    }
                    position.X = x;
                    this.Position = position;
                    return;

                case Keys.End:
                    if ((keys & ~Keys.KeyCode) == Keys.Control)
                    {
                        this.StartPage = this.pageInfo.Length;
                    }
                    return;

                case Keys.Home:
                    if ((keys & ~Keys.KeyCode) == Keys.Control)
                    {
                        this.StartPage = 0;
                    }
                    return;

                case Keys.Left:
                    x = position.X;
                    if (x <= 5)
                    {
                        x = 0;
                    }
                    else
                    {
                        x -= 5;
                    }
                    position.X = x;
                    this.Position = position;
                    return;

                case Keys.Up:
                    x = position.Y;
                    if (x <= 5)
                    {
                        x = 0;
                    }
                    else
                    {
                        x -= 5;
                    }
                    position.Y = x;
                    this.Position = position;
                    return;

                case Keys.Right:
                    x = position.X;
                    num2 = Math.Max(base.Width, this.virtualSize.Width);
                    if (x >= (num2 - 5))
                    {
                        x = num2;
                    }
                    else
                    {
                        x += 5;
                    }
                    position.X = x;
                    this.Position = position;
                    return;

                case Keys.Down:
                    x = position.Y;
                    num2 = Math.Max(base.Height, this.virtualSize.Height);
                    if (x >= (num2 - 5))
                    {
                        x = num2;
                    }
                    else
                    {
                        x += 5;
                    }
                    position.Y = x;
                    this.Position = position;
                    return;

                default:
                    return;
            }
            position.X = x;
            this.Position = position;
        }

        private void WmVScroll(ref Message m)
        {
            if (m.LParam != IntPtr.Zero)
            {
                base.WndProc(ref m);
            }
            else
            {
                Point position = this.Position;
                int y = position.Y;
                int maxPos = Math.Max(base.Height, this.virtualSize.Height);
                position.Y = this.AdjustScroll(m, y, maxPos, false);
                this.Position = position;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x114:
                    this.WmHScroll(ref m);
                    return;

                case 0x115:
                    this.WmVScroll(ref m);
                    return;

                case 0x100:
                    this.WmKeyDown(ref m);
                    return;
            }
            base.WndProc(ref m);
        }

        [System.Windows.Forms.SRDescription("PrintPreviewAutoZoomDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool AutoZoom
        {
            get
            {
                return this.autoZoom;
            }
            set
            {
                if (this.autoZoom != value)
                {
                    this.autoZoom = value;
                    this.InvalidateLayout();
                }
            }
        }

        [System.Windows.Forms.SRDescription("PrintPreviewColumnsDescr"), System.Windows.Forms.SRCategory("CatLayout"), DefaultValue(1)]
        public int Columns
        {
            get
            {
                return this.columns;
            }
            set
            {
                if (value < 1)
                {
                    object[] args = new object[] { "Columns", value.ToString(CultureInfo.CurrentCulture), 1.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("Columns", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.columns = value;
                this.InvalidateLayout();
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.Style |= 0x100000;
                createParams.Style |= 0x200000;
                return createParams;
            }
        }

        [System.Windows.Forms.SRDescription("PrintPreviewDocumentDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue((string) null)]
        public PrintDocument Document
        {
            get
            {
                return this.document;
            }
            set
            {
                this.document = value;
                this.InvalidatePreview();
            }
        }

        [Browsable(false), System.Windows.Forms.SRCategory("CatLayout"), System.Windows.Forms.SRDescription("ControlWithScrollbarsPositionDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Point Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.SetPositionNoInvalidate(value);
            }
        }

        [Localizable(true), AmbientValue(2), System.Windows.Forms.SRDescription("ControlRightToLeftDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public override System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                return base.RightToLeft;
            }
            set
            {
                base.RightToLeft = value;
                this.InvalidatePreview();
            }
        }

        [DefaultValue(1), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("PrintPreviewRowsDescr")]
        public int Rows
        {
            get
            {
                return this.rows;
            }
            set
            {
                if (value < 1)
                {
                    object[] args = new object[] { "Rows", value.ToString(CultureInfo.CurrentCulture), 1.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("Rows", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                this.rows = value;
                this.InvalidateLayout();
            }
        }

        [System.Windows.Forms.SRDescription("PrintPreviewStartPageDescr"), DefaultValue(0), System.Windows.Forms.SRCategory("CatBehavior")]
        public int StartPage
        {
            get
            {
                int startPage = this.startPage;
                if (this.pageInfo != null)
                {
                    startPage = Math.Min(startPage, this.pageInfo.Length - (this.rows * this.columns));
                }
                return Math.Max(startPage, 0);
            }
            set
            {
                if (value < 0)
                {
                    object[] args = new object[] { "StartPage", value.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentOutOfRangeException("StartPage", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", args));
                }
                int startPage = this.StartPage;
                this.startPage = value;
                if (startPage != this.startPage)
                {
                    this.InvalidateLayout();
                    this.OnStartPageChanged(EventArgs.Empty);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Bindable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("PrintPreviewAntiAliasDescr")]
        public bool UseAntiAlias
        {
            get
            {
                return this.antiAlias;
            }
            set
            {
                this.antiAlias = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatLayout"), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlWithScrollbarsVirtualSizeDescr"), Browsable(false)]
        private Size VirtualSize
        {
            get
            {
                return this.virtualSize;
            }
            set
            {
                this.SetVirtualSizeNoInvalidate(value);
                base.Invalidate();
            }
        }

        [DefaultValue((double) 0.3), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("PrintPreviewZoomDescr")]
        public double Zoom
        {
            get
            {
                return this.zoom;
            }
            set
            {
                if (value <= 0.0)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("PrintPreviewControlZoomNegative"));
                }
                this.autoZoom = false;
                this.zoom = value;
                this.InvalidateLayout();
            }
        }
    }
}

