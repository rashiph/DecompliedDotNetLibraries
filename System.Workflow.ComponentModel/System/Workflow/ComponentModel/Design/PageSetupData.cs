namespace System.Workflow.ComponentModel.Design
{
    using Microsoft.Win32;
    using System;
    using System.Drawing.Printing;
    using System.Runtime;
    using System.Windows.Forms;

    internal sealed class PageSetupData
    {
        private bool adjustToScaleFactor = true;
        private bool centerHorizontally;
        private bool centerVertically;
        internal static readonly int DefaultFooterMargin = 50;
        internal static readonly int DefaultHeaderMargin = 50;
        internal static readonly int DefaultMaxScaleFactor = 400;
        internal static readonly int DefaultMinScaleFactor = 10;
        internal static readonly int DefaultPages = 1;
        internal static readonly int DefaultScaleFactor = 100;
        private HorizontalAlignment footerAlignment = HorizontalAlignment.Center;
        private bool footerCustom;
        private int footerMargin = DefaultFooterMargin;
        private string footerTemplate = string.Empty;
        private HorizontalAlignment headerAlignment = HorizontalAlignment.Center;
        private bool headerCustom;
        private int headerMargin = DefaultHeaderMargin;
        private string headerTemplate = string.Empty;
        private bool landscape;
        private System.Drawing.Printing.Margins margins;
        private int pagesTall = DefaultPages;
        private int pagesWide = DefaultPages;
        private const string RegistryCenterHorizontally = "CenterHorizontally";
        private const string RegistryCenterVertically = "CenterVertically";
        private const string RegistryFooterAlignment = "FooterAlignment";
        private const string RegistryFooterCustom = "FooterCustom";
        private const string RegistryFooterMarging = "FooterMargin";
        private const string RegistryFooterTemplate = "FooterTemplate";
        private const string RegistryHeaderAlignment = "HeaderAlignment";
        private const string RegistryHeaderCustom = "HeaderCustom";
        private const string RegistryHeaderMarging = "HeaderMargin";
        private const string RegistryHeaderTemplate = "HeaderTemplate";
        private int scaleFactor = DefaultScaleFactor;
        private static readonly string WinOEPrintingSubKey = (DesignerHelpers.DesignerPerUserRegistryKey + @"\Printing");

        internal PageSetupData()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(WinOEPrintingSubKey);
            if (key != null)
            {
                try
                {
                    object obj2 = null;
                    obj2 = key.GetValue("HeaderAlignment");
                    if ((obj2 != null) && (obj2 is int))
                    {
                        this.headerAlignment = (HorizontalAlignment) obj2;
                    }
                    obj2 = key.GetValue("FooterAlignment");
                    if ((obj2 != null) && (obj2 is int))
                    {
                        this.footerAlignment = (HorizontalAlignment) obj2;
                    }
                    obj2 = key.GetValue("HeaderMargin");
                    if ((obj2 != null) && (obj2 is int))
                    {
                        this.headerMargin = (int) obj2;
                    }
                    obj2 = key.GetValue("FooterMargin");
                    if ((obj2 != null) && (obj2 is int))
                    {
                        this.footerMargin = (int) obj2;
                    }
                    obj2 = key.GetValue("HeaderTemplate");
                    if ((obj2 != null) && (obj2 is string))
                    {
                        this.headerTemplate = (string) obj2;
                    }
                    obj2 = key.GetValue("FooterTemplate");
                    if ((obj2 != null) && (obj2 is string))
                    {
                        this.footerTemplate = (string) obj2;
                    }
                    obj2 = key.GetValue("HeaderCustom");
                    if ((obj2 != null) && (obj2 is int))
                    {
                        this.headerCustom = Convert.ToBoolean((int) obj2);
                    }
                    obj2 = key.GetValue("FooterCustom");
                    if ((obj2 != null) && (obj2 is int))
                    {
                        this.footerCustom = Convert.ToBoolean((int) obj2);
                    }
                    obj2 = key.GetValue("CenterHorizontally");
                    if ((obj2 != null) && (obj2 is int))
                    {
                        this.centerHorizontally = Convert.ToBoolean((int) obj2);
                    }
                    obj2 = key.GetValue("CenterVertically");
                    if ((obj2 != null) && (obj2 is int))
                    {
                        this.centerVertically = Convert.ToBoolean((int) obj2);
                    }
                }
                finally
                {
                    key.Close();
                }
            }
            PrinterSettings settings = new PrinterSettings();
            this.landscape = settings.DefaultPageSettings.Landscape;
            this.margins = settings.DefaultPageSettings.Margins;
        }

        public void StorePropertiesToRegistry()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(WinOEPrintingSubKey);
            if (key != null)
            {
                try
                {
                    key.SetValue("HeaderAlignment", (int) this.headerAlignment);
                    key.SetValue("FooterAlignment", (int) this.footerAlignment);
                    key.SetValue("HeaderMargin", this.headerMargin);
                    key.SetValue("FooterMargin", this.footerMargin);
                    key.SetValue("HeaderTemplate", this.headerTemplate);
                    key.SetValue("FooterTemplate", this.footerTemplate);
                    key.SetValue("HeaderCustom", Convert.ToInt32(this.headerCustom));
                    key.SetValue("FooterCustom", Convert.ToInt32(this.footerCustom));
                    key.SetValue("CenterHorizontally", Convert.ToInt32(this.centerHorizontally));
                    key.SetValue("CenterVertically", Convert.ToInt32(this.centerVertically));
                }
                finally
                {
                    key.Close();
                }
            }
        }

        public bool AdjustToScaleFactor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.adjustToScaleFactor;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.adjustToScaleFactor = value;
            }
        }

        public bool CenterHorizontally
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.centerHorizontally;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.centerHorizontally = value;
            }
        }

        public bool CenterVertically
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.centerVertically;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.centerVertically = value;
            }
        }

        public HorizontalAlignment FooterAlignment
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.footerAlignment;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.footerAlignment = value;
            }
        }

        public bool FooterCustom
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.footerCustom;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.footerCustom = value;
            }
        }

        public int FooterMargin
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.footerMargin;
            }
            set
            {
                if (value >= 0)
                {
                    this.footerMargin = value;
                }
            }
        }

        public string FooterTemplate
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.footerTemplate;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.footerTemplate = value;
            }
        }

        public HorizontalAlignment HeaderAlignment
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.headerAlignment;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.headerAlignment = value;
            }
        }

        public bool HeaderCustom
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.headerCustom;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.headerCustom = value;
            }
        }

        public int HeaderMargin
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.headerMargin;
            }
            set
            {
                if (value >= 0)
                {
                    this.headerMargin = value;
                }
            }
        }

        public string HeaderTemplate
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.headerTemplate;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.headerTemplate = value;
            }
        }

        public bool Landscape
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.landscape;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.landscape = value;
            }
        }

        public System.Drawing.Printing.Margins Margins
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.margins;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.margins = value;
            }
        }

        public int PagesTall
        {
            get
            {
                if (this.pagesTall >= 1)
                {
                    return this.pagesTall;
                }
                return 1;
            }
            set
            {
                if (value > 0)
                {
                    this.pagesTall = value;
                }
            }
        }

        public int PagesWide
        {
            get
            {
                if (this.pagesWide >= 1)
                {
                    return this.pagesWide;
                }
                return 1;
            }
            set
            {
                if (value > 0)
                {
                    this.pagesWide = value;
                }
            }
        }

        public int ScaleFactor
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.scaleFactor;
            }
            set
            {
                if ((value >= DefaultMinScaleFactor) && (value <= DefaultMaxScaleFactor))
                {
                    this.scaleFactor = value;
                }
            }
        }
    }
}

