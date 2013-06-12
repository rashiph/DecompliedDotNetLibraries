namespace System.Drawing
{
    using System;
    using System.Drawing.Internal;
    using System.Drawing.Printing;
    using System.Runtime.InteropServices;

    internal class PrintPreviewGraphics
    {
        private PrintDocument printDocument;
        private PrintPageEventArgs printPageEventArgs;

        public PrintPreviewGraphics(PrintDocument document, PrintPageEventArgs e)
        {
            this.printPageEventArgs = e;
            this.printDocument = document;
        }

        public RectangleF VisibleClipBounds
        {
            get
            {
                RectangleF visibleClipBounds;
                IntPtr hdevmodeInternal = this.printPageEventArgs.PageSettings.PrinterSettings.GetHdevmodeInternal();
                using (DeviceContext context = this.printPageEventArgs.PageSettings.PrinterSettings.CreateDeviceContext(hdevmodeInternal))
                {
                    using (Graphics graphics = Graphics.FromHdcInternal(context.Hdc))
                    {
                        if (this.printDocument.OriginAtMargins)
                        {
                            int deviceCaps = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(context, context.Hdc), 0x58);
                            int num2 = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(context, context.Hdc), 90);
                            int num3 = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(context, context.Hdc), 0x70);
                            int num4 = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(context, context.Hdc), 0x71);
                            float num5 = (num3 * 100) / deviceCaps;
                            float num6 = (num4 * 100) / num2;
                            graphics.TranslateTransform(-num5, -num6);
                            graphics.TranslateTransform((float) this.printDocument.DefaultPageSettings.Margins.Left, (float) this.printDocument.DefaultPageSettings.Margins.Top);
                        }
                        visibleClipBounds = graphics.VisibleClipBounds;
                    }
                }
                return visibleClipBounds;
            }
        }
    }
}

