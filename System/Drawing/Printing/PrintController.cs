namespace System.Drawing.Printing
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security;

    public abstract class PrintController
    {
        internal SafeDeviceModeHandle modeHandle;

        protected PrintController()
        {
            IntSecurity.SafePrinting.Demand();
        }

        private PrintPageEventArgs CreatePrintPageEvent(PageSettings pageSettings)
        {
            IntSecurity.AllPrintingAndUnmanagedCode.Assert();
            Rectangle bounds = pageSettings.GetBounds((IntPtr) this.modeHandle);
            return new PrintPageEventArgs(null, new Rectangle(pageSettings.Margins.Left, pageSettings.Margins.Top, bounds.Width - (pageSettings.Margins.Left + pageSettings.Margins.Right), bounds.Height - (pageSettings.Margins.Top + pageSettings.Margins.Bottom)), bounds, pageSettings);
        }

        public virtual void OnEndPage(PrintDocument document, PrintPageEventArgs e)
        {
        }

        public virtual void OnEndPrint(PrintDocument document, PrintEventArgs e)
        {
            IntSecurity.UnmanagedCode.Assert();
            if (this.modeHandle != null)
            {
                this.modeHandle.Close();
            }
        }

        public virtual Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
        {
            return null;
        }

        public virtual void OnStartPrint(PrintDocument document, PrintEventArgs e)
        {
            IntSecurity.AllPrintingAndUnmanagedCode.Assert();
            this.modeHandle = (SafeDeviceModeHandle) document.PrinterSettings.GetHdevmode(document.DefaultPageSettings);
        }

        internal void Print(PrintDocument document)
        {
            PrintAction printToPreview;
            IntSecurity.SafePrinting.Demand();
            if (this.IsPreview)
            {
                printToPreview = PrintAction.PrintToPreview;
            }
            else
            {
                printToPreview = document.PrinterSettings.PrintToFile ? PrintAction.PrintToFile : PrintAction.PrintToPrinter;
            }
            PrintEventArgs e = new PrintEventArgs(printToPreview);
            document._OnBeginPrint(e);
            if (e.Cancel)
            {
                document._OnEndPrint(e);
            }
            else
            {
                this.OnStartPrint(document, e);
                if (e.Cancel)
                {
                    document._OnEndPrint(e);
                    this.OnEndPrint(document, e);
                }
                else
                {
                    bool flag = true;
                    try
                    {
                        flag = this.PrintLoop(document);
                    }
                    finally
                    {
                        try
                        {
                            try
                            {
                                document._OnEndPrint(e);
                                e.Cancel = flag | e.Cancel;
                            }
                            finally
                            {
                                this.OnEndPrint(document, e);
                            }
                        }
                        finally
                        {
                            if (!IntSecurity.HasPermission(IntSecurity.AllPrinting))
                            {
                                IntSecurity.AllPrinting.Assert();
                                document.PrinterSettings.PrintDialogDisplayed = false;
                            }
                        }
                    }
                }
            }
        }

        private bool PrintLoop(PrintDocument document)
        {
            PrintPageEventArgs args2;
            QueryPageSettingsEventArgs e = new QueryPageSettingsEventArgs((PageSettings) document.DefaultPageSettings.Clone());
            do
            {
                document._OnQueryPageSettings(e);
                if (e.Cancel)
                {
                    return true;
                }
                args2 = this.CreatePrintPageEvent(e.PageSettings);
                Graphics graphics = this.OnStartPage(document, args2);
                args2.SetGraphics(graphics);
                try
                {
                    document._OnPrintPage(args2);
                    this.OnEndPage(document, args2);
                }
                finally
                {
                    args2.Dispose();
                }
                if (args2.Cancel)
                {
                    return true;
                }
            }
            while (args2.HasMorePages);
            return false;
        }

        public virtual bool IsPreview
        {
            get
            {
                return false;
            }
        }

        [SecurityCritical]
        internal sealed class SafeDeviceModeHandle : SafeHandle
        {
            private SafeDeviceModeHandle() : base(IntPtr.Zero, true)
            {
            }

            internal SafeDeviceModeHandle(IntPtr handle) : base(IntPtr.Zero, true)
            {
                base.SetHandle(handle);
            }

            public static explicit operator PrintController.SafeDeviceModeHandle(IntPtr handle)
            {
                return new PrintController.SafeDeviceModeHandle(handle);
            }

            public static implicit operator IntPtr(PrintController.SafeDeviceModeHandle handle)
            {
                if (handle != null)
                {
                    return handle.handle;
                }
                return IntPtr.Zero;
            }

            [SecurityCritical]
            protected override bool ReleaseHandle()
            {
                if (!this.IsInvalid)
                {
                    SafeNativeMethods.GlobalFree(new HandleRef(this, base.handle));
                }
                base.handle = IntPtr.Zero;
                return true;
            }

            public override bool IsInvalid
            {
                get
                {
                    return (base.handle == IntPtr.Zero);
                }
            }
        }
    }
}

