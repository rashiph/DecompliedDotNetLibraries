namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Printing;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal sealed class PrintPreviewLayout : WorkflowLayout
    {
        private static Margins DefaultPageMargins = new Margins(20, 20, 20, 20);
        private static Size DefaultPageSeparator = new Size(30, 30);
        private Margins headerFooterMargins;
        private ArrayList pageLayoutInfo;
        private Margins pageMargins;
        private Size pageSeparator;
        private Size pageSize;
        private DateTime previewTime;
        private WorkflowPrintDocument printDocument;
        private Size rowColumns;
        private float scaling;

        internal PrintPreviewLayout(IServiceProvider serviceProvider, WorkflowPrintDocument printDoc) : base(serviceProvider)
        {
            this.pageLayoutInfo = new ArrayList();
            this.headerFooterMargins = new Margins(0, 0, 0, 0);
            this.pageSeparator = DefaultPageSeparator;
            this.pageMargins = DefaultPageMargins;
            this.rowColumns = new Size(1, 1);
            this.scaling = 1f;
            this.pageSize = Size.Empty;
            this.previewTime = DateTime.Now;
            this.printDocument = printDoc;
        }

        private Margins GetAdjustedMargins(Graphics graphics)
        {
            Margins margins = this.printDocument.PageSetupData.Margins;
            if (this.printDocument.PageSetupData.Landscape)
            {
                int left = margins.Left;
                margins.Left = margins.Right;
                margins.Right = left;
                left = margins.Bottom;
                margins.Bottom = margins.Top;
                margins.Top = left;
            }
            Margins hardMargins = new Margins();
            using (Graphics graphics2 = this.printDocument.PrinterSettings.CreateMeasurementGraphics())
            {
                hardMargins = this.printDocument.GetHardMargins(graphics2);
            }
            return new Margins(Math.Max(margins.Left, hardMargins.Left), Math.Max(margins.Right, hardMargins.Right), Math.Max(margins.Top, hardMargins.Top), Math.Max(margins.Bottom, hardMargins.Bottom));
        }

        private Size GetPaperSize(Graphics graphics)
        {
            Size empty = Size.Empty;
            PaperSize paperSize = this.printDocument.DefaultPageSettings.PaperSize;
            this.printDocument.DefaultPageSettings.PaperSize = paperSize;
            if (this.printDocument.PageSetupData.Landscape)
            {
                empty.Width = Math.Max(paperSize.Height, 1);
                empty.Height = Math.Max(paperSize.Width, 1);
                return empty;
            }
            empty.Width = Math.Max(paperSize.Width, 1);
            empty.Height = Math.Max(paperSize.Height, 1);
            return empty;
        }

        public override bool IsCoOrdInLayout(Point logicalCoOrd)
        {
            foreach (PageLayoutData data in this.pageLayoutInfo)
            {
                if (data.ViewablePageBounds.Contains(logicalCoOrd))
                {
                    return true;
                }
            }
            return false;
        }

        public override Point MapInCoOrdToLayout(Point logicalPoint)
        {
            foreach (PageLayoutData data in this.pageLayoutInfo)
            {
                if (data.PageBounds.Contains(logicalPoint))
                {
                    Point point = new Point(logicalPoint.X - data.ViewablePageBounds.Left, logicalPoint.Y - data.ViewablePageBounds.Top);
                    logicalPoint = new Point(data.LogicalPageBounds.Left + point.X, data.LogicalPageBounds.Top + point.Y);
                    return logicalPoint;
                }
            }
            return logicalPoint;
        }

        public override Rectangle MapInRectangleToLayout(Rectangle logicalRectangle)
        {
            Rectangle empty = Rectangle.Empty;
            foreach (PageLayoutData data in this.pageLayoutInfo)
            {
                Rectangle rectangle2 = logicalRectangle;
                rectangle2.Intersect(data.ViewablePageBounds);
                if (!rectangle2.IsEmpty)
                {
                    Size size;
                    Point point = new Point(rectangle2.X - data.ViewablePageBounds.X, rectangle2.Y - data.ViewablePageBounds.Y);
                    size = new Size(data.ViewablePageBounds.Width - rectangle2.Width, data.ViewablePageBounds.Height - rectangle2.Height) {
                        Width = size.Width - point.X,
                        Height = size.Height - point.Y
                    };
                    Rectangle b = Rectangle.Empty;
                    b.X = data.LogicalPageBounds.X + point.X;
                    b.Y = data.LogicalPageBounds.Y + point.Y;
                    b.Width = data.LogicalPageBounds.Width - point.X;
                    b.Width -= size.Width;
                    b.Height = data.LogicalPageBounds.Height - point.Y;
                    b.Height -= size.Height;
                    empty = empty.IsEmpty ? b : Rectangle.Union(empty, b);
                }
            }
            return empty;
        }

        public override Point MapOutCoOrdFromLayout(Point logicalPoint)
        {
            foreach (PageLayoutData data in this.pageLayoutInfo)
            {
                if (data.LogicalPageBounds.Contains(logicalPoint))
                {
                    Point point = new Point(logicalPoint.X - data.LogicalPageBounds.Left, logicalPoint.Y - data.LogicalPageBounds.Top);
                    logicalPoint = new Point(data.ViewablePageBounds.Left + point.X, data.ViewablePageBounds.Top + point.Y);
                    return logicalPoint;
                }
            }
            return logicalPoint;
        }

        public override Rectangle MapOutRectangleFromLayout(Rectangle logicalRectangle)
        {
            Rectangle empty = Rectangle.Empty;
            foreach (PageLayoutData data in this.pageLayoutInfo)
            {
                Rectangle rectangle2 = logicalRectangle;
                rectangle2.Intersect(data.LogicalPageBounds);
                if (!rectangle2.IsEmpty)
                {
                    Size size;
                    Point point = new Point(rectangle2.X - data.LogicalPageBounds.X, rectangle2.Y - data.LogicalPageBounds.Y);
                    size = new Size(data.LogicalPageBounds.Width - rectangle2.Width, data.LogicalPageBounds.Height - rectangle2.Height) {
                        Width = size.Width - point.X,
                        Height = size.Height - point.Y
                    };
                    Rectangle b = Rectangle.Empty;
                    b.X = data.ViewablePageBounds.X + point.X;
                    b.Y = data.ViewablePageBounds.Y + point.Y;
                    b.Width = data.ViewablePageBounds.Width - point.X;
                    b.Width -= size.Width;
                    b.Height = data.ViewablePageBounds.Height - point.Y;
                    b.Height -= size.Height;
                    empty = empty.IsEmpty ? b : Rectangle.Union(empty, b);
                }
            }
            return empty;
        }

        public override void OnPaint(PaintEventArgs e, ViewPortData viewPortData)
        {
            Graphics graphics = e.Graphics;
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            Bitmap memoryBitmap = viewPortData.MemoryBitmap;
            graphics.FillRectangle(Brushes.White, new Rectangle(Point.Empty, memoryBitmap.Size));
            if (ambientTheme.WorkflowWatermarkImage != null)
            {
                GraphicsContainer container = graphics.BeginContainer();
                Matrix matrix = new Matrix();
                matrix.Scale(viewPortData.Scaling.Width, viewPortData.Scaling.Height, MatrixOrder.Prepend);
                matrix.Invert();
                Point[] pts = new Point[] { viewPortData.Translation, new Point(viewPortData.ViewPortSize) };
                matrix.TransformPoints(pts);
                Rectangle rect = new Rectangle(pts[0], new Size(pts[1]));
                matrix = new Matrix();
                matrix.Scale((viewPortData.Scaling.Width / ((float) base.parentView.Zoom)) * 100f, (viewPortData.Scaling.Height / ((float) base.parentView.Zoom)) * 100f);
                Matrix matrix2 = new Matrix();
                matrix2.Scale(((float) base.parentView.Zoom) / 100f, ((float) base.parentView.Zoom) / 100f);
                graphics.Transform = matrix2;
                foreach (PageLayoutData data in this.pageLayoutInfo)
                {
                    if (data.PageBounds.IntersectsWith(rect))
                    {
                        Rectangle empty = Rectangle.Empty;
                        empty.X = data.LogicalPageBounds.X - viewPortData.LogicalViewPort.X;
                        empty.Y = data.LogicalPageBounds.Y - viewPortData.LogicalViewPort.Y;
                        empty.Width = data.LogicalPageBounds.Width;
                        empty.Height = data.LogicalPageBounds.Height;
                        pts = new Point[] { empty.Location, new Point(empty.Size) };
                        matrix.TransformPoints(pts);
                        empty.Location = pts[0];
                        empty.Size = new Size(pts[1]);
                        ActivityDesignerPaint.DrawImage(graphics, ambientTheme.WorkflowWatermarkImage, empty, new Rectangle(Point.Empty, ambientTheme.WorkflowWatermarkImage.Size), ambientTheme.WatermarkAlignment, 0.25f, false);
                    }
                }
                graphics.EndContainer(container);
            }
        }

        public override void OnPaintWorkflow(PaintEventArgs e, ViewPortData viewPortData)
        {
            Graphics graphics = e.Graphics;
            Bitmap memoryBitmap = viewPortData.MemoryBitmap;
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            GraphicsContainer container = graphics.BeginContainer();
            Rectangle rect = new Rectangle(Point.Empty, memoryBitmap.Size);
            graphics.FillRectangle(AmbientTheme.WorkspaceBackgroundBrush, rect);
            using (Font font = new Font(ambientTheme.Font.FontFamily, ambientTheme.Font.Size / this.scaling, ambientTheme.Font.Style))
            {
                int num = 0;
                Matrix matrix = new Matrix();
                Matrix matrix2 = new Matrix();
                matrix2.Scale(viewPortData.Scaling.Width, viewPortData.Scaling.Height, MatrixOrder.Prepend);
                matrix2.Invert();
                Point[] pts = new Point[] { viewPortData.Translation, new Point(viewPortData.ViewPortSize) };
                matrix2.TransformPoints(pts);
                matrix2.Invert();
                Rectangle rectangle2 = new Rectangle(pts[0], new Size(pts[1]));
                WorkflowPrintDocument.HeaderFooterData headerFooterPrintData = new WorkflowPrintDocument.HeaderFooterData {
                    HeaderFooterMargins = this.headerFooterMargins,
                    PrintTime = this.previewTime,
                    TotalPages = this.pageLayoutInfo.Count,
                    Scaling = this.scaling,
                    Font = font
                };
                WorkflowDesignerLoader service = base.serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                headerFooterPrintData.FileName = (service != null) ? service.FileName : string.Empty;
                Matrix matrix3 = new Matrix();
                matrix3.Scale(viewPortData.Scaling.Width, viewPortData.Scaling.Height, MatrixOrder.Prepend);
                matrix3.Translate((float) -viewPortData.Translation.X, (float) -viewPortData.Translation.Y, MatrixOrder.Append);
                foreach (PageLayoutData data2 in this.pageLayoutInfo)
                {
                    num++;
                    if ((data2.PageBounds.IntersectsWith(rectangle2) && (data2.PageBounds.Width > 0)) && (data2.PageBounds.Height > 0))
                    {
                        graphics.Transform = matrix3;
                        graphics.FillRectangle(Brushes.White, data2.PageBounds);
                        ActivityDesignerPaint.DrawDropShadow(graphics, data2.PageBounds, Color.Black, 4, LightSourcePosition.Top | LightSourcePosition.Left, 0.2f, false);
                        Rectangle logicalPageBounds = data2.LogicalPageBounds;
                        logicalPageBounds.Intersect(viewPortData.LogicalViewPort);
                        if (!logicalPageBounds.IsEmpty)
                        {
                            graphics.Transform = matrix;
                            Point empty = Point.Empty;
                            empty.X = data2.ViewablePageBounds.X + Math.Abs((int) (data2.LogicalPageBounds.X - logicalPageBounds.X));
                            empty.Y = data2.ViewablePageBounds.Y + Math.Abs((int) (data2.LogicalPageBounds.Y - logicalPageBounds.Y));
                            pts = new Point[] { empty };
                            matrix2.TransformPoints(pts);
                            empty = new Point(pts[0].X - viewPortData.Translation.X, pts[0].Y - viewPortData.Translation.Y);
                            Rectangle source = Rectangle.Empty;
                            source.X = logicalPageBounds.X - viewPortData.LogicalViewPort.X;
                            source.Y = logicalPageBounds.Y - viewPortData.LogicalViewPort.Y;
                            source.Width = logicalPageBounds.Width;
                            source.Height = logicalPageBounds.Height;
                            pts = new Point[] { source.Location, new Point(source.Size) };
                            matrix2.TransformPoints(pts);
                            source.Location = pts[0];
                            source.Size = new Size(pts[1]);
                            ActivityDesignerPaint.DrawImage(graphics, memoryBitmap, new Rectangle(empty, source.Size), source, DesignerContentAlignment.Fill, 1f, WorkflowTheme.CurrentTheme.AmbientTheme.DrawGrayscale);
                        }
                        graphics.Transform = matrix3;
                        graphics.DrawRectangle(Pens.Black, data2.PageBounds);
                        graphics.DrawRectangle(ambientTheme.ForegroundPen, (int) (data2.ViewablePageBounds.Left - 3), (int) (data2.ViewablePageBounds.Top - 3), (int) (data2.ViewablePageBounds.Width + 6), (int) (data2.ViewablePageBounds.Height + 6));
                        headerFooterPrintData.PageBounds = data2.PageBounds;
                        headerFooterPrintData.PageBoundsWithoutMargin = data2.ViewablePageBounds;
                        headerFooterPrintData.CurrentPage = num;
                        if (this.printDocument.PageSetupData.HeaderTemplate.Length > 0)
                        {
                            this.printDocument.PrintHeaderFooter(graphics, true, headerFooterPrintData);
                        }
                        if (this.printDocument.PageSetupData.FooterTemplate.Length > 0)
                        {
                            this.printDocument.PrintHeaderFooter(graphics, false, headerFooterPrintData);
                        }
                    }
                }
                graphics.EndContainer(container);
            }
        }

        public override void Update(Graphics graphics, WorkflowLayout.LayoutUpdateReason reason)
        {
            if (reason != WorkflowLayout.LayoutUpdateReason.ZoomChanged)
            {
                Size size5;
                if (graphics == null)
                {
                    throw new ArgumentException("graphics");
                }
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Size paperSize = this.GetPaperSize(graphics);
                Margins adjustedMargins = this.GetAdjustedMargins(graphics);
                Size size2 = (base.parentView.RootDesigner != null) ? base.parentView.RootDesigner.Size : Size.Empty;
                if (!size2.IsEmpty)
                {
                    Size selectionSize = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize;
                    size2.Width += 3 * selectionSize.Width;
                    size2.Height += 3 * selectionSize.Height;
                }
                if (this.printDocument.PageSetupData.AdjustToScaleFactor)
                {
                    this.scaling = ((float) this.printDocument.PageSetupData.ScaleFactor) / 100f;
                }
                else
                {
                    Size size4;
                    size4 = new Size(paperSize.Width - (adjustedMargins.Left + adjustedMargins.Right), paperSize.Height - (adjustedMargins.Top + adjustedMargins.Bottom)) {
                        Width = Math.Max(size4.Width, 1),
                        Height = Math.Max(size4.Height, 1)
                    };
                    PointF tf = new PointF((this.printDocument.PageSetupData.PagesWide * size4.Width) / ((float) size2.Width), (this.printDocument.PageSetupData.PagesTall * size4.Height) / ((float) size2.Height));
                    this.scaling = Math.Min(tf.X, tf.Y);
                    this.scaling = (float) (Math.Floor((double) (this.scaling * 1000.0)) / 1000.0);
                }
                this.pageSize = paperSize;
                this.pageSize.Width = Convert.ToInt32(Math.Ceiling((double) (((float) this.pageSize.Width) / this.scaling)));
                this.pageSize.Height = Convert.ToInt32(Math.Ceiling((double) (((float) this.pageSize.Height) / this.scaling)));
                IDesignerOptionService service = base.serviceProvider.GetService(typeof(IDesignerOptionService)) as IDesignerOptionService;
                if (service != null)
                {
                    object optionValue = service.GetOptionValue("WinOEDesigner", "PageSeparator");
                    this.PageSeparator = (optionValue != null) ? ((Size) optionValue) : DefaultPageSeparator;
                }
                this.PageSeparator = new Size(Convert.ToInt32(Math.Ceiling((double) (((float) this.PageSeparator.Width) / this.scaling))), Convert.ToInt32(Math.Ceiling((double) (((float) this.PageSeparator.Height) / this.scaling))));
                this.PageMargins = adjustedMargins;
                this.PageMargins.Left = Convert.ToInt32((float) (((float) this.PageMargins.Left) / this.scaling));
                this.PageMargins.Right = Convert.ToInt32((float) (((float) this.PageMargins.Right) / this.scaling));
                this.PageMargins.Top = Convert.ToInt32((float) (((float) this.PageMargins.Top) / this.scaling));
                this.PageMargins.Bottom = Convert.ToInt32((float) (((float) this.PageMargins.Bottom) / this.scaling));
                this.headerFooterMargins.Top = Convert.ToInt32((float) (((float) this.printDocument.PageSetupData.HeaderMargin) / this.scaling));
                this.headerFooterMargins.Bottom = Convert.ToInt32((float) (((float) this.printDocument.PageSetupData.FooterMargin) / this.scaling));
                this.previewTime = DateTime.Now;
                size5 = new Size(this.pageSize.Width - (this.PageMargins.Left + this.PageMargins.Right), this.pageSize.Height - (this.PageMargins.Top + this.PageMargins.Bottom)) {
                    Width = Math.Max(size5.Width, 1),
                    Height = Math.Max(size5.Height, 1)
                };
                this.rowColumns.Width = size2.Width / size5.Width;
                this.rowColumns.Width += ((size2.Width % size5.Width) > 1) ? 1 : 0;
                this.rowColumns.Width = Math.Max(1, this.rowColumns.Width);
                this.rowColumns.Height = size2.Height / size5.Height;
                this.rowColumns.Height += ((size2.Height % size5.Height) > 1) ? 1 : 0;
                this.rowColumns.Height = Math.Max(1, this.rowColumns.Height);
                this.pageLayoutInfo.Clear();
                for (int i = 0; i < this.rowColumns.Height; i++)
                {
                    for (int j = 0; j < this.rowColumns.Width; j++)
                    {
                        Point empty = Point.Empty;
                        empty.X = (j * this.pageSize.Width) + ((j + 1) * this.PageSeparator.Width);
                        empty.Y = (i * this.pageSize.Height) + ((i + 1) * this.PageSeparator.Height);
                        Point location = Point.Empty;
                        location.X = empty.X + this.PageMargins.Left;
                        location.Y = empty.Y + this.PageMargins.Top;
                        Rectangle logicalPageBounds = new Rectangle(j * size5.Width, i * size5.Height, size5.Width, size5.Height);
                        Rectangle pageBounds = new Rectangle(empty, this.pageSize);
                        Rectangle viewablePageBounds = new Rectangle(location, size5);
                        this.pageLayoutInfo.Add(new PageLayoutData(logicalPageBounds, pageBounds, viewablePageBounds, new Point(j, i)));
                    }
                }
            }
        }

        public override Size Extent
        {
            get
            {
                Size empty = Size.Empty;
                empty.Width = (this.rowColumns.Width * this.pageSize.Width) + ((this.rowColumns.Width + 1) * this.PageSeparator.Width);
                empty.Height = (this.rowColumns.Height * this.pageSize.Height) + ((this.rowColumns.Height + 1) * this.PageSeparator.Height);
                return empty;
            }
        }

        private Margins PageMargins
        {
            get
            {
                return this.pageMargins;
            }
            set
            {
                this.pageMargins = value;
            }
        }

        private Size PageSeparator
        {
            get
            {
                return this.pageSeparator;
            }
            set
            {
                this.pageSeparator = value;
            }
        }

        public override Point RootDesignerAlignment
        {
            get
            {
                Point empty = Point.Empty;
                Size size = new Size(this.pageSize.Width - (this.PageMargins.Left + this.PageMargins.Right), this.pageSize.Height - (this.PageMargins.Top + this.PageMargins.Bottom));
                Size size2 = new Size(this.rowColumns.Width * size.Width, this.rowColumns.Height * size.Height);
                Size size3 = (base.parentView.RootDesigner != null) ? base.parentView.RootDesigner.Size : Size.Empty;
                Size selectionSize = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize;
                if (this.printDocument.PageSetupData.CenterHorizontally)
                {
                    empty.X = (size2.Width - size3.Width) / 2;
                }
                empty.X = Math.Max(empty.X, selectionSize.Width + (selectionSize.Width / 2));
                if (this.printDocument.PageSetupData.CenterVertically)
                {
                    empty.Y = (size2.Height - size3.Height) / 2;
                }
                empty.Y = Math.Max(empty.Y, selectionSize.Height + (selectionSize.Height / 2));
                return empty;
            }
        }

        public override float Scaling
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.scaling;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PageLayoutData
        {
            public Rectangle LogicalPageBounds;
            public Rectangle PageBounds;
            public Rectangle ViewablePageBounds;
            public Point Position;
            public PageLayoutData(Rectangle logicalPageBounds, Rectangle pageBounds, Rectangle viewablePageBounds, Point rowColumnPos)
            {
                this.LogicalPageBounds = logicalPageBounds;
                this.PageBounds = pageBounds;
                this.ViewablePageBounds = viewablePageBounds;
                this.Position = rowColumnPos;
            }
        }
    }
}

