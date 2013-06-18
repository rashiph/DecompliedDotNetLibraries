namespace System.Drawing.Printing
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Internal;
    using System.Drawing.Text;
    using System.Runtime.InteropServices;
    using System.Security;

    public class PreviewPrintController : PrintController
    {
        private bool antiAlias;
        private DeviceContext dc;
        private Graphics graphics;
        private IList list = new ArrayList();

        private void CheckSecurity()
        {
            IntSecurity.SafePrinting.Demand();
        }

        public PreviewPageInfo[] GetPreviewPageInfo()
        {
            this.CheckSecurity();
            PreviewPageInfo[] array = new PreviewPageInfo[this.list.Count];
            this.list.CopyTo(array, 0);
            return array;
        }

        public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
        {
            this.CheckSecurity();
            this.graphics.Dispose();
            this.graphics = null;
            base.OnEndPage(document, e);
        }

        public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
        {
            this.CheckSecurity();
            this.dc.Dispose();
            this.dc = null;
            base.OnEndPrint(document, e);
        }

        public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
        {
            this.CheckSecurity();
            base.OnStartPage(document, e);
            try
            {
                IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                e.PageSettings.CopyToHdevmode((IntPtr) base.modeHandle);
                Size size = e.PageBounds.Size;
                Size size2 = PrinterUnitConvert.Convert(size, PrinterUnit.Display, PrinterUnit.HundredthsOfAMillimeter);
                Metafile image = new Metafile(this.dc.Hdc, new Rectangle(0, 0, size2.Width, size2.Height), MetafileFrameUnit.GdiCompatible, EmfType.EmfPlusOnly);
                PreviewPageInfo info = new PreviewPageInfo(image, size);
                this.list.Add(info);
                PrintPreviewGraphics graphics = new PrintPreviewGraphics(document, e);
                this.graphics = Graphics.FromImage(image);
                if ((this.graphics != null) && document.OriginAtMargins)
                {
                    int deviceCaps = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(this.dc, this.dc.Hdc), 0x58);
                    int num2 = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(this.dc, this.dc.Hdc), 90);
                    int num3 = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(this.dc, this.dc.Hdc), 0x70);
                    int num4 = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(this.dc, this.dc.Hdc), 0x71);
                    float num5 = (num3 * 100) / deviceCaps;
                    float num6 = (num4 * 100) / num2;
                    this.graphics.TranslateTransform(-num5, -num6);
                    this.graphics.TranslateTransform((float) document.DefaultPageSettings.Margins.Left, (float) document.DefaultPageSettings.Margins.Top);
                }
                this.graphics.PrintingHelper = graphics;
                if (this.antiAlias)
                {
                    this.graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                    this.graphics.SmoothingMode = SmoothingMode.AntiAlias;
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return this.graphics;
        }

        public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
        {
            this.CheckSecurity();
            base.OnStartPrint(document, e);
            try
            {
                if (!document.PrinterSettings.IsValid)
                {
                    throw new InvalidPrinterException(document.PrinterSettings);
                }
                IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                this.dc = document.PrinterSettings.CreateInformationContext((IntPtr) base.modeHandle);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }

        public override bool IsPreview
        {
            get
            {
                return true;
            }
        }

        public virtual bool UseAntiAlias
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
    }
}

