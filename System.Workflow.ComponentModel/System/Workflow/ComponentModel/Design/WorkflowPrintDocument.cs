namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Printing;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Text;
    using System.Windows.Forms;
    using System.Workflow.Interop;

    [ToolboxItem(false)]
    internal sealed class WorkflowPrintDocument : PrintDocument
    {
        private Point currentPrintablePage = Point.Empty;
        private const int MaxHeaderFooterLines = 5;
        private System.Workflow.ComponentModel.Design.PageSetupData pageSetupData = new System.Workflow.ComponentModel.Design.PageSetupData();
        private System.Workflow.ComponentModel.Design.PrintPreviewLayout previewLayout;
        private DateTime printTime;
        private float scaling;
        private Point totalPrintablePages = Point.Empty;
        private Point workflowAlignment = Point.Empty;
        private WorkflowView workflowView;

        public WorkflowPrintDocument(WorkflowView workflowView)
        {
            this.workflowView = workflowView;
            this.previewLayout = new System.Workflow.ComponentModel.Design.PrintPreviewLayout(this.workflowView, this);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this.previewLayout != null))
                {
                    this.previewLayout.Dispose();
                    this.previewLayout = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal Margins GetHardMargins(Graphics graphics)
        {
            IntPtr hdc = graphics.GetHdc();
            Point point = new Point(Math.Max(System.Workflow.Interop.NativeMethods.GetDeviceCaps(hdc, 0x58), 1), Math.Max(System.Workflow.Interop.NativeMethods.GetDeviceCaps(hdc, 90), 1));
            int num = (int) ((System.Workflow.Interop.NativeMethods.GetDeviceCaps(hdc, 8) * 100f) / ((float) point.X));
            int num2 = (int) ((System.Workflow.Interop.NativeMethods.GetDeviceCaps(hdc, 10) * 100f) / ((float) point.Y));
            int num3 = (int) ((System.Workflow.Interop.NativeMethods.GetDeviceCaps(hdc, 110) * 100f) / ((float) point.X));
            int num4 = (int) ((System.Workflow.Interop.NativeMethods.GetDeviceCaps(hdc, 0x6f) * 100f) / ((float) point.Y));
            int left = (int) ((System.Workflow.Interop.NativeMethods.GetDeviceCaps(hdc, 0x70) * 100f) / ((float) point.X));
            int top = (int) ((System.Workflow.Interop.NativeMethods.GetDeviceCaps(hdc, 0x71) * 100f) / ((float) point.Y));
            int right = (num3 - num) - left;
            int bottom = (num4 - num2) - top;
            graphics.ReleaseHdc(hdc);
            return new Margins(left, right, top, bottom);
        }

        private bool MoveNextPage()
        {
            this.currentPrintablePage.X++;
            if (this.currentPrintablePage.X < this.totalPrintablePages.X)
            {
                return true;
            }
            this.currentPrintablePage.X = 0;
            this.currentPrintablePage.Y++;
            return (this.currentPrintablePage.Y < this.totalPrintablePages.Y);
        }

        protected override void OnBeginPrint(PrintEventArgs printArgs)
        {
            base.OnBeginPrint(printArgs);
            this.currentPrintablePage = Point.Empty;
            bool flag = (base.PrinterSettings.IsValid && (PrinterSettings.InstalledPrinters.Count > 0)) && new ArrayList(PrinterSettings.InstalledPrinters).Contains(base.PrinterSettings.PrinterName);
            if (!flag)
            {
                DesignerHelpers.ShowError(this.workflowView, DR.GetString("SelectedPrinterIsInvalidErrorMessage", new object[0]));
            }
            printArgs.Cancel = !flag || (this.workflowView.RootDesigner == null);
        }

        protected override void OnPrintPage(PrintPageEventArgs printPageArg)
        {
            base.OnPrintPage(printPageArg);
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            Graphics graphics = printPageArg.Graphics;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            if (this.currentPrintablePage.IsEmpty)
            {
                this.PrepareToPrint(printPageArg);
            }
            Margins hardMargins = this.GetHardMargins(graphics);
            Margins margins2 = new Margins(Math.Max(printPageArg.PageSettings.Margins.Left, hardMargins.Left), Math.Max(printPageArg.PageSettings.Margins.Right, hardMargins.Right), Math.Max(printPageArg.PageSettings.Margins.Top, hardMargins.Top), Math.Max(printPageArg.PageSettings.Margins.Bottom, hardMargins.Bottom));
            Size size = new Size(printPageArg.PageBounds.Size.Width - (margins2.Left + margins2.Right), printPageArg.PageBounds.Size.Height - (margins2.Top + margins2.Bottom));
            Rectangle rect = new Rectangle(margins2.Left, margins2.Top, size.Width, size.Height);
            Region region = new Region(rect);
            try
            {
                graphics.TranslateTransform((float) -hardMargins.Left, (float) -hardMargins.Top);
                graphics.FillRectangle(ambientTheme.BackgroundBrush, rect);
                graphics.DrawRectangle(ambientTheme.ForegroundPen, rect);
                if (ambientTheme.WorkflowWatermarkImage != null)
                {
                    ActivityDesignerPaint.DrawImage(graphics, ambientTheme.WorkflowWatermarkImage, rect, new Rectangle(Point.Empty, ambientTheme.WorkflowWatermarkImage.Size), ambientTheme.WatermarkAlignment, 0.25f, false);
                }
                Matrix transform = graphics.Transform;
                Region clip = graphics.Clip;
                graphics.Clip = region;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                Point point = new Point((this.currentPrintablePage.X * size.Width) - this.workflowAlignment.X, (this.currentPrintablePage.Y * size.Height) - this.workflowAlignment.Y);
                graphics.TranslateTransform((float) (rect.Left - point.X), (float) (rect.Top - point.Y));
                graphics.ScaleTransform(this.scaling, this.scaling);
                Size empty = Size.Empty;
                empty.Width = Convert.ToInt32(Math.Ceiling((double) (((float) size.Width) / this.scaling)));
                empty.Height = Convert.ToInt32(Math.Ceiling((double) (((float) size.Height) / this.scaling)));
                Point point2 = Point.Empty;
                point2.X = Convert.ToInt32(Math.Ceiling((double) (((float) this.workflowAlignment.X) / this.scaling)));
                point2.Y = Convert.ToInt32(Math.Ceiling((double) (((float) this.workflowAlignment.Y) / this.scaling)));
                Rectangle viewPort = new Rectangle((this.currentPrintablePage.X * empty.Width) - point2.X, (this.currentPrintablePage.Y * empty.Height) - point2.Y, empty.Width, empty.Height);
                using (PaintEventArgs args = new PaintEventArgs(graphics, this.workflowView.RootDesigner.Bounds))
                {
                    ((IWorkflowDesignerMessageSink) this.workflowView.RootDesigner).OnPaint(args, viewPort);
                }
                graphics.Clip = clip;
                graphics.Transform = transform;
                HeaderFooterData headerFooterPrintData = new HeaderFooterData {
                    Font = ambientTheme.Font,
                    PageBounds = printPageArg.PageBounds,
                    PageBoundsWithoutMargin = rect,
                    HeaderFooterMargins = new Margins(0, 0, this.pageSetupData.HeaderMargin, this.pageSetupData.FooterMargin),
                    PrintTime = this.printTime,
                    CurrentPage = (this.currentPrintablePage.X + (this.currentPrintablePage.Y * this.totalPrintablePages.X)) + 1,
                    TotalPages = this.totalPrintablePages.X * this.totalPrintablePages.Y,
                    Scaling = this.scaling
                };
                WorkflowDesignerLoader service = ((IServiceProvider) this.workflowView).GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                headerFooterPrintData.FileName = (service != null) ? service.FileName : string.Empty;
                if (this.pageSetupData.HeaderTemplate.Length > 0)
                {
                    this.PrintHeaderFooter(graphics, true, headerFooterPrintData);
                }
                if (this.pageSetupData.FooterTemplate.Length > 0)
                {
                    this.PrintHeaderFooter(graphics, false, headerFooterPrintData);
                }
                printPageArg.HasMorePages = this.MoveNextPage();
            }
            catch (Exception exception)
            {
                DesignerHelpers.ShowError(this.workflowView, DR.GetString("SelectedPrinterIsInvalidErrorMessage", new object[0]) + "\n" + exception.Message);
                printPageArg.Cancel = true;
                printPageArg.HasMorePages = false;
            }
            finally
            {
                region.Dispose();
            }
            if (!printPageArg.HasMorePages)
            {
                this.workflowView.PerformLayout();
            }
        }

        private void PrepareToPrint(PrintPageEventArgs printPageArg)
        {
            Size size4;
            Graphics graphics = printPageArg.Graphics;
            Size selectionSize = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize;
            ((IWorkflowDesignerMessageSink) this.workflowView.RootDesigner).OnLayoutSize(graphics);
            ((IWorkflowDesignerMessageSink) this.workflowView.RootDesigner).OnLayoutPosition(graphics);
            this.workflowView.RootDesigner.Location = Point.Empty;
            Size size = this.workflowView.RootDesigner.Size;
            size.Width += 3 * selectionSize.Width;
            size.Height += 3 * selectionSize.Height;
            Size size3 = printPageArg.PageBounds.Size;
            Margins hardMargins = this.GetHardMargins(graphics);
            Margins margins2 = new Margins(Math.Max(printPageArg.PageSettings.Margins.Left, hardMargins.Left), Math.Max(printPageArg.PageSettings.Margins.Right, hardMargins.Right), Math.Max(printPageArg.PageSettings.Margins.Top, hardMargins.Top), Math.Max(printPageArg.PageSettings.Margins.Bottom, hardMargins.Bottom));
            size4 = new Size(size3.Width - (margins2.Left + margins2.Right), size3.Height - (margins2.Top + margins2.Bottom)) {
                Width = Math.Max(size4.Width, 1),
                Height = Math.Max(size4.Height, 1)
            };
            if (this.pageSetupData.AdjustToScaleFactor)
            {
                this.scaling = ((float) this.pageSetupData.ScaleFactor) / 100f;
            }
            else
            {
                float num = (this.pageSetupData.PagesWide * size4.Width) / ((float) size.Width);
                float num2 = (this.pageSetupData.PagesTall * size4.Height) / ((float) size.Height);
                this.scaling = Math.Min(num, num2);
                this.scaling = (float) (Math.Floor((double) (this.scaling * 1000.0)) / 1000.0);
            }
            this.totalPrintablePages.X = Convert.ToInt32(Math.Ceiling((double) ((this.scaling * size.Width) / ((float) size4.Width))));
            this.totalPrintablePages.X = Math.Max(this.totalPrintablePages.X, 1);
            this.totalPrintablePages.Y = Convert.ToInt32(Math.Ceiling((double) ((this.scaling * size.Height) / ((float) size4.Height))));
            this.totalPrintablePages.Y = Math.Max(this.totalPrintablePages.Y, 1);
            this.workflowAlignment = Point.Empty;
            if (this.pageSetupData.CenterHorizontally)
            {
                this.workflowAlignment.X = (int) (((((this.totalPrintablePages.X * size4.Width) / this.scaling) - size.Width) / 2f) * this.scaling);
            }
            if (this.pageSetupData.CenterVertically)
            {
                this.workflowAlignment.Y = (int) (((((this.totalPrintablePages.Y * size4.Height) / this.scaling) - size.Height) / 2f) * this.scaling);
            }
            this.workflowAlignment.X = Math.Max(this.workflowAlignment.X, selectionSize.Width + (selectionSize.Width / 2));
            this.workflowAlignment.Y = Math.Max(this.workflowAlignment.Y, selectionSize.Height + (selectionSize.Height / 2));
            this.printTime = DateTime.Now;
        }

        internal void PrintHeaderFooter(Graphics graphics, bool drawHeader, HeaderFooterData headerFooterPrintData)
        {
            string text = drawHeader ? this.pageSetupData.HeaderTemplate : this.pageSetupData.FooterTemplate;
            string[] strArray = text.Replace("{#}", headerFooterPrintData.CurrentPage.ToString(CultureInfo.CurrentCulture)).Replace("{##}", headerFooterPrintData.TotalPages.ToString(CultureInfo.CurrentCulture)).Replace("{Date}", headerFooterPrintData.PrintTime.ToShortDateString()).Replace("{Time}", headerFooterPrintData.PrintTime.ToShortTimeString()).Replace("{FullFileName}", headerFooterPrintData.FileName).Replace("{FileName}", Path.GetFileName(headerFooterPrintData.FileName)).Replace("{User}", SystemInformation.UserName).Split(new char[] { '\n', '\r' }, 6, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Math.Min(strArray.Length, 5); i++)
            {
                builder.Append(strArray[i]);
                builder.Append("\r\n");
            }
            text = builder.ToString();
            Rectangle empty = Rectangle.Empty;
            SizeF ef = graphics.MeasureString(text, headerFooterPrintData.Font);
            int width = Convert.ToInt32(Math.Ceiling((double) ef.Width));
            empty.Size = new Size(width, Convert.ToInt32(Math.Ceiling((double) ef.Height)));
            empty.Width = Math.Min(headerFooterPrintData.PageBoundsWithoutMargin.Width, empty.Width);
            HorizontalAlignment alignment = drawHeader ? this.pageSetupData.HeaderAlignment : this.pageSetupData.FooterAlignment;
            StringFormat format = new StringFormat {
                Trimming = StringTrimming.EllipsisCharacter
            };
            switch (alignment)
            {
                case HorizontalAlignment.Left:
                    empty.X = headerFooterPrintData.PageBoundsWithoutMargin.Left;
                    format.Alignment = StringAlignment.Near;
                    break;

                case HorizontalAlignment.Right:
                    empty.X = headerFooterPrintData.PageBoundsWithoutMargin.Left + (headerFooterPrintData.PageBoundsWithoutMargin.Width - empty.Width);
                    format.Alignment = StringAlignment.Far;
                    break;

                case HorizontalAlignment.Center:
                    empty.X = headerFooterPrintData.PageBoundsWithoutMargin.Left + ((headerFooterPrintData.PageBoundsWithoutMargin.Width - empty.Width) / 2);
                    format.Alignment = StringAlignment.Center;
                    break;
            }
            if (drawHeader)
            {
                empty.Y = headerFooterPrintData.PageBounds.Top + headerFooterPrintData.HeaderFooterMargins.Top;
                format.LineAlignment = StringAlignment.Near;
            }
            else
            {
                empty.Y = (headerFooterPrintData.PageBounds.Bottom - headerFooterPrintData.HeaderFooterMargins.Bottom) - empty.Size.Height;
                format.LineAlignment = StringAlignment.Far;
            }
            graphics.DrawString(text, headerFooterPrintData.Font, WorkflowTheme.CurrentTheme.AmbientTheme.ForegroundBrush, empty, format);
        }

        internal System.Workflow.ComponentModel.Design.PageSetupData PageSetupData
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.pageSetupData;
            }
        }

        internal System.Workflow.ComponentModel.Design.PrintPreviewLayout PrintPreviewLayout
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.previewLayout;
            }
        }

        internal sealed class HeaderFooterData
        {
            internal int CurrentPage;
            internal string FileName;
            internal System.Drawing.Font Font;
            internal Margins HeaderFooterMargins;
            internal Rectangle PageBounds;
            internal Rectangle PageBoundsWithoutMargin;
            internal DateTime PrintTime;
            internal float Scaling;
            internal int TotalPages;
        }
    }
}

