namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), ClassInterface(ClassInterfaceType.None), Guid("5D13802C-C830-4b41-8E7A-F69D9DD6A095")]
    public class BuildSettings : IBuildSettings
    {
        private string applicationFile;
        private string applicationName;
        private bool applicationRequiresElevation;
        private string applicationUrl;
        private Microsoft.Build.Tasks.Deployment.Bootstrapper.ComponentsLocation componentsLocation;
        private string componentsUrl;
        private string culture;
        private string fallbackCulture;
        private int fallbackLCID = Util.DefaultCultureInfo.LCID;
        private bool fCopyComponents;
        private bool fValidate;
        private int lcid = Util.DefaultCultureInfo.LCID;
        private string outputPath;
        private ProductBuilderCollection productBuilders = new ProductBuilderCollection();
        private string supportUrl;

        public string ApplicationFile
        {
            get
            {
                return this.applicationFile;
            }
            set
            {
                this.applicationFile = value;
            }
        }

        public string ApplicationName
        {
            get
            {
                return this.applicationName;
            }
            set
            {
                this.applicationName = value;
            }
        }

        public bool ApplicationRequiresElevation
        {
            get
            {
                return this.applicationRequiresElevation;
            }
            set
            {
                this.applicationRequiresElevation = value;
            }
        }

        public string ApplicationUrl
        {
            get
            {
                return this.applicationUrl;
            }
            set
            {
                this.applicationUrl = value;
            }
        }

        public Microsoft.Build.Tasks.Deployment.Bootstrapper.ComponentsLocation ComponentsLocation
        {
            get
            {
                return this.componentsLocation;
            }
            set
            {
                this.componentsLocation = value;
            }
        }

        public string ComponentsUrl
        {
            get
            {
                return this.componentsUrl;
            }
            set
            {
                this.componentsUrl = value;
            }
        }

        public bool CopyComponents
        {
            get
            {
                return this.fCopyComponents;
            }
            set
            {
                this.fCopyComponents = value;
            }
        }

        internal string Culture
        {
            get
            {
                return this.culture;
            }
            set
            {
                this.culture = value;
            }
        }

        internal string FallbackCulture
        {
            get
            {
                return this.fallbackCulture;
            }
            set
            {
                this.fallbackCulture = value;
            }
        }

        public int FallbackLCID
        {
            get
            {
                return this.fallbackLCID;
            }
            set
            {
                this.fallbackLCID = value;
            }
        }

        public int LCID
        {
            get
            {
                return this.lcid;
            }
            set
            {
                this.lcid = value;
            }
        }

        public string OutputPath
        {
            get
            {
                return this.outputPath;
            }
            set
            {
                this.outputPath = value;
            }
        }

        public ProductBuilderCollection ProductBuilders
        {
            get
            {
                return this.productBuilders;
            }
        }

        public string SupportUrl
        {
            get
            {
                return this.supportUrl;
            }
            set
            {
                this.supportUrl = value;
            }
        }

        public bool Validate
        {
            get
            {
                return this.fValidate;
            }
            set
            {
                this.fValidate = value;
            }
        }
    }
}

