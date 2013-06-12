namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Printing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [System.Windows.Forms.SRDescription("DescriptionPageSetupDialog"), DefaultProperty("Document")]
    public sealed class PageSetupDialog : CommonDialog
    {
        private bool allowMargins;
        private bool allowOrientation;
        private bool allowPaper;
        private bool allowPrinter;
        private bool enableMetric;
        private Margins minMargins;
        private System.Drawing.Printing.PageSettings pageSettings;
        private PrintDocument printDocument;
        private System.Drawing.Printing.PrinterSettings printerSettings;
        private bool showHelp;
        private bool showNetwork;

        public PageSetupDialog()
        {
            this.Reset();
        }

        private int GetFlags()
        {
            int num = 0;
            num |= 0x2000;
            if (!this.allowMargins)
            {
                num |= 0x10;
            }
            if (!this.allowOrientation)
            {
                num |= 0x100;
            }
            if (!this.allowPaper)
            {
                num |= 0x200;
            }
            if (!this.allowPrinter || (this.printerSettings == null))
            {
                num |= 0x20;
            }
            if (this.showHelp)
            {
                num |= 0x800;
            }
            if (!this.showNetwork)
            {
                num |= 0x200000;
            }
            if (this.minMargins != null)
            {
                num |= 1;
            }
            if (this.pageSettings.Margins != null)
            {
                num |= 2;
            }
            return num;
        }

        public override void Reset()
        {
            this.allowMargins = true;
            this.allowOrientation = true;
            this.allowPaper = true;
            this.allowPrinter = true;
            this.MinMargins = null;
            this.pageSettings = null;
            this.printDocument = null;
            this.printerSettings = null;
            this.showHelp = false;
            this.showNetwork = true;
        }

        private void ResetMinMargins()
        {
            this.MinMargins = null;
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            System.Windows.Forms.NativeMethods.PAGESETUPDLG pagesetupdlg;
            bool flag2;
            System.Windows.Forms.IntSecurity.SafePrinting.Demand();
            System.Windows.Forms.NativeMethods.WndProc proc = new System.Windows.Forms.NativeMethods.WndProc(this.HookProc);
            if (this.pageSettings == null)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("PSDcantShowWithoutPage"));
            }
            pagesetupdlg = new System.Windows.Forms.NativeMethods.PAGESETUPDLG {
                lStructSize = Marshal.SizeOf(pagesetupdlg),
                Flags = this.GetFlags(),
                hwndOwner = hwndOwner,
                lpfnPageSetupHook = proc
            };
            PrinterUnit thousandthsOfAnInch = PrinterUnit.ThousandthsOfAnInch;
            if (this.EnableMetric)
            {
                StringBuilder lpLCData = new StringBuilder(2);
                if ((UnsafeNativeMethods.GetLocaleInfo(System.Windows.Forms.NativeMethods.LOCALE_USER_DEFAULT, 13, lpLCData, lpLCData.Capacity) > 0) && (int.Parse(lpLCData.ToString(), CultureInfo.InvariantCulture) == 0))
                {
                    thousandthsOfAnInch = PrinterUnit.HundredthsOfAMillimeter;
                }
            }
            if (this.MinMargins != null)
            {
                Margins margins = PrinterUnitConvert.Convert(this.MinMargins, PrinterUnit.Display, thousandthsOfAnInch);
                pagesetupdlg.minMarginLeft = margins.Left;
                pagesetupdlg.minMarginTop = margins.Top;
                pagesetupdlg.minMarginRight = margins.Right;
                pagesetupdlg.minMarginBottom = margins.Bottom;
            }
            if (this.pageSettings.Margins != null)
            {
                Margins margins2 = PrinterUnitConvert.Convert(this.pageSettings.Margins, PrinterUnit.Display, thousandthsOfAnInch);
                pagesetupdlg.marginLeft = margins2.Left;
                pagesetupdlg.marginTop = margins2.Top;
                pagesetupdlg.marginRight = margins2.Right;
                pagesetupdlg.marginBottom = margins2.Bottom;
            }
            pagesetupdlg.marginLeft = Math.Max(pagesetupdlg.marginLeft, pagesetupdlg.minMarginLeft);
            pagesetupdlg.marginTop = Math.Max(pagesetupdlg.marginTop, pagesetupdlg.minMarginTop);
            pagesetupdlg.marginRight = Math.Max(pagesetupdlg.marginRight, pagesetupdlg.minMarginRight);
            pagesetupdlg.marginBottom = Math.Max(pagesetupdlg.marginBottom, pagesetupdlg.minMarginBottom);
            System.Drawing.Printing.PrinterSettings settings = (this.printerSettings == null) ? this.pageSettings.PrinterSettings : this.printerSettings;
            System.Windows.Forms.IntSecurity.AllPrintingAndUnmanagedCode.Assert();
            try
            {
                pagesetupdlg.hDevMode = settings.GetHdevmode(this.pageSettings);
                pagesetupdlg.hDevNames = settings.GetHdevnames();
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            try
            {
                if (!UnsafeNativeMethods.PageSetupDlg(pagesetupdlg))
                {
                    return false;
                }
                UpdateSettings(pagesetupdlg, this.pageSettings, this.printerSettings);
                flag2 = true;
            }
            finally
            {
                UnsafeNativeMethods.GlobalFree(new HandleRef(pagesetupdlg, pagesetupdlg.hDevMode));
                UnsafeNativeMethods.GlobalFree(new HandleRef(pagesetupdlg, pagesetupdlg.hDevNames));
            }
            return flag2;
        }

        private bool ShouldSerializeMinMargins()
        {
            if (((this.minMargins.Left == 0) && (this.minMargins.Right == 0)) && (this.minMargins.Top == 0))
            {
                return (this.minMargins.Bottom != 0);
            }
            return true;
        }

        private static void UpdateSettings(System.Windows.Forms.NativeMethods.PAGESETUPDLG data, System.Drawing.Printing.PageSettings pageSettings, System.Drawing.Printing.PrinterSettings printerSettings)
        {
            System.Windows.Forms.IntSecurity.AllPrintingAndUnmanagedCode.Assert();
            try
            {
                pageSettings.SetHdevmode(data.hDevMode);
                if (printerSettings != null)
                {
                    printerSettings.SetHdevmode(data.hDevMode);
                    printerSettings.SetHdevnames(data.hDevNames);
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            Margins margins = new Margins {
                Left = data.marginLeft,
                Top = data.marginTop,
                Right = data.marginRight,
                Bottom = data.marginBottom
            };
            PrinterUnit fromUnit = ((data.Flags & 8) != 0) ? PrinterUnit.HundredthsOfAMillimeter : PrinterUnit.ThousandthsOfAnInch;
            pageSettings.Margins = PrinterUnitConvert.Convert(margins, fromUnit, PrinterUnit.Display);
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("PSDallowMarginsDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool AllowMargins
        {
            get
            {
                return this.allowMargins;
            }
            set
            {
                this.allowMargins = value;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("PSDallowOrientationDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool AllowOrientation
        {
            get
            {
                return this.allowOrientation;
            }
            set
            {
                this.allowOrientation = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true), System.Windows.Forms.SRDescription("PSDallowPaperDescr")]
        public bool AllowPaper
        {
            get
            {
                return this.allowPaper;
            }
            set
            {
                this.allowPaper = value;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("PSDallowPrinterDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool AllowPrinter
        {
            get
            {
                return this.allowPrinter;
            }
            set
            {
                this.allowPrinter = value;
            }
        }

        [System.Windows.Forms.SRDescription("PDdocumentDescr"), DefaultValue((string) null), System.Windows.Forms.SRCategory("CatData")]
        public PrintDocument Document
        {
            get
            {
                return this.printDocument;
            }
            set
            {
                this.printDocument = value;
                if (this.printDocument != null)
                {
                    this.pageSettings = this.printDocument.DefaultPageSettings;
                    this.printerSettings = this.printDocument.PrinterSettings;
                }
            }
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(false), System.Windows.Forms.SRDescription("PSDenableMetricDescr")]
        public bool EnableMetric
        {
            get
            {
                return this.enableMetric;
            }
            set
            {
                this.enableMetric = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("PSDminMarginsDescr")]
        public Margins MinMargins
        {
            get
            {
                return this.minMargins;
            }
            set
            {
                if (value == null)
                {
                    value = new Margins(0, 0, 0, 0);
                }
                this.minMargins = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), System.Windows.Forms.SRDescription("PSDpageSettingsDescr"), DefaultValue((string) null), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Printing.PageSettings PageSettings
        {
            get
            {
                return this.pageSettings;
            }
            set
            {
                this.pageSettings = value;
                this.printDocument = null;
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("PSDprinterSettingsDescr"), Browsable(false), DefaultValue((string) null)]
        public System.Drawing.Printing.PrinterSettings PrinterSettings
        {
            get
            {
                return this.printerSettings;
            }
            set
            {
                this.printerSettings = value;
                this.printDocument = null;
            }
        }

        [System.Windows.Forms.SRDescription("PSDshowHelpDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool ShowHelp
        {
            get
            {
                return this.showHelp;
            }
            set
            {
                this.showHelp = value;
            }
        }

        [System.Windows.Forms.SRDescription("PSDshowNetworkDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(true)]
        public bool ShowNetwork
        {
            get
            {
                return this.showNetwork;
            }
            set
            {
                this.showNetwork = value;
            }
        }
    }
}

