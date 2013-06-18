namespace System.Drawing.Printing
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Internal;
    using System.Runtime.InteropServices;
    using System.Security;

    public class StandardPrintController : PrintController
    {
        private DeviceContext dc;
        private Graphics graphics;

        private void CheckSecurity(PrintDocument document)
        {
            if (document.PrinterSettings.PrintDialogDisplayed)
            {
                System.Drawing.IntSecurity.SafePrinting.Demand();
            }
            else if (document.PrinterSettings.IsDefaultPrinter)
            {
                System.Drawing.IntSecurity.DefaultPrinting.Demand();
            }
            else
            {
                System.Drawing.IntSecurity.AllPrinting.Demand();
            }
        }

        public override void OnEndPage(PrintDocument document, PrintPageEventArgs e)
        {
            this.CheckSecurity(document);
            System.Drawing.IntSecurity.UnmanagedCode.Assert();
            try
            {
                if (SafeNativeMethods.EndPage(new HandleRef(this.dc, this.dc.Hdc)) <= 0)
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
                this.graphics.Dispose();
                this.graphics = null;
            }
            base.OnEndPage(document, e);
        }

        public override void OnEndPrint(PrintDocument document, PrintEventArgs e)
        {
            this.CheckSecurity(document);
            System.Drawing.IntSecurity.UnmanagedCode.Assert();
            try
            {
                if (this.dc != null)
                {
                    try
                    {
                        int num = e.Cancel ? SafeNativeMethods.AbortDoc(new HandleRef(this.dc, this.dc.Hdc)) : SafeNativeMethods.EndDoc(new HandleRef(this.dc, this.dc.Hdc));
                        if (num <= 0)
                        {
                            throw new Win32Exception();
                        }
                    }
                    finally
                    {
                        this.dc.Dispose();
                        this.dc = null;
                    }
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            base.OnEndPrint(document, e);
        }

        public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
        {
            this.CheckSecurity(document);
            base.OnStartPage(document, e);
            try
            {
                System.Drawing.IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                e.PageSettings.CopyToHdevmode((IntPtr) base.modeHandle);
                IntPtr handle = SafeNativeMethods.GlobalLock(new HandleRef(this, (IntPtr) base.modeHandle));
                try
                {
                    SafeNativeMethods.ResetDC(new HandleRef(this.dc, this.dc.Hdc), new HandleRef(null, handle));
                }
                finally
                {
                    SafeNativeMethods.GlobalUnlock(new HandleRef(this, (IntPtr) base.modeHandle));
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            this.graphics = Graphics.FromHdcInternal(this.dc.Hdc);
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
            if (SafeNativeMethods.StartPage(new HandleRef(this.dc, this.dc.Hdc)) <= 0)
            {
                throw new Win32Exception();
            }
            return this.graphics;
        }

        public override void OnStartPrint(PrintDocument document, PrintEventArgs e)
        {
            this.CheckSecurity(document);
            base.OnStartPrint(document, e);
            if (!document.PrinterSettings.IsValid)
            {
                throw new InvalidPrinterException(document.PrinterSettings);
            }
            this.dc = document.PrinterSettings.CreateDeviceContext((IntPtr) base.modeHandle);
            SafeNativeMethods.DOCINFO lpDocInfo = new SafeNativeMethods.DOCINFO {
                lpszDocName = document.DocumentName
            };
            if (document.PrinterSettings.PrintToFile)
            {
                lpDocInfo.lpszOutput = document.PrinterSettings.OutputPort;
            }
            else
            {
                lpDocInfo.lpszOutput = null;
            }
            lpDocInfo.lpszDatatype = null;
            lpDocInfo.fwType = 0;
            if (SafeNativeMethods.StartDoc(new HandleRef(this.dc, this.dc.Hdc), lpDocInfo) <= 0)
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 0x4c7)
                {
                    throw new Win32Exception(error);
                }
                e.Cancel = true;
            }
        }
    }
}

