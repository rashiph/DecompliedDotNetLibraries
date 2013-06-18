namespace System.Web.Services.Configuration
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Xml;

    public sealed class WsdlHelpGeneratorElement : ConfigurationElement
    {
        private string actualPath;
        private readonly ConfigurationProperty href = new ConfigurationProperty("href", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private bool needToValidateHref;
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private string virtualPath;

        public WsdlHelpGeneratorElement()
        {
            this.properties.Add(this.href);
        }

        private static void CheckIOReadPermission(string path, string file)
        {
            if (path != null)
            {
                string fullPath = Path.GetFullPath(Path.Combine(path, file));
                new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
            }
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);
            WebContext hostingContext = base.EvaluationContext.HostingContext as WebContext;
            if ((hostingContext != null) && (this.Href.Length != 0))
            {
                string path = hostingContext.Path;
                string configurationDirectory = null;
                if (path == null)
                {
                    path = HostingEnvironment.ApplicationVirtualPath;
                    if (path == null)
                    {
                        path = "";
                    }
                    configurationDirectory = this.GetConfigurationDirectory();
                }
                else
                {
                    configurationDirectory = HostingEnvironment.MapPath(path);
                }
                if (!path.EndsWith("/", StringComparison.Ordinal))
                {
                    path = path + "/";
                }
                CheckIOReadPermission(configurationDirectory, this.Href);
                this.actualPath = configurationDirectory;
                this.virtualPath = path;
                this.needToValidateHref = true;
            }
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        private string GetConfigurationDirectory()
        {
            return HttpRuntime.MachineConfigurationDirectory;
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            WsdlHelpGeneratorElement element = (WsdlHelpGeneratorElement) parentElement;
            WebContext hostingContext = base.EvaluationContext.HostingContext as WebContext;
            if (hostingContext != null)
            {
                string path = hostingContext.Path;
                bool flag = path == null;
                this.actualPath = element.actualPath;
                if (flag)
                {
                    path = HostingEnvironment.ApplicationVirtualPath;
                }
                if ((path != null) && !path.EndsWith("/", StringComparison.Ordinal))
                {
                    path = path + "/";
                }
                if ((path == null) && (parentElement != null))
                {
                    this.virtualPath = element.virtualPath;
                }
                else if (path != null)
                {
                    this.virtualPath = path;
                }
            }
            base.Reset(parentElement);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void SetDefaults()
        {
            if (HttpContext.Current != null)
            {
                this.virtualPath = HostingEnvironment.ApplicationVirtualPath;
            }
            this.actualPath = this.GetConfigurationDirectory();
            if ((this.virtualPath != null) && !this.virtualPath.EndsWith("/", StringComparison.Ordinal))
            {
                this.virtualPath = this.virtualPath + "/";
            }
            if ((this.actualPath != null) && !this.actualPath.EndsWith(@"\", StringComparison.Ordinal))
            {
                this.actualPath = this.actualPath + @"\";
            }
            this.Href = "DefaultWsdlHelpGenerator.aspx";
            CheckIOReadPermission(this.actualPath, this.Href);
            this.needToValidateHref = true;
        }

        internal string HelpGeneratorPath
        {
            get
            {
                return Path.Combine(this.actualPath, this.Href);
            }
        }

        internal string HelpGeneratorVirtualPath
        {
            get
            {
                return (this.virtualPath + this.Href);
            }
        }

        [ConfigurationProperty("href", IsRequired=true)]
        public string Href
        {
            get
            {
                return (string) base[this.href];
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (this.needToValidateHref && (value.Length > 0))
                {
                    CheckIOReadPermission(this.actualPath, value);
                }
                base[this.href] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.properties;
            }
        }
    }
}

