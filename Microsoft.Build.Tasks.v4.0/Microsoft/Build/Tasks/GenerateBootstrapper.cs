namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Tasks.Deployment.Bootstrapper;
    using System;
    using System.Collections;

    public sealed class GenerateBootstrapper : TaskExtension
    {
        private string applicationFile;
        private string applicationName;
        private bool applicationRequiresElevation;
        private string applicationUrl;
        private string[] bootstrapperComponentFiles;
        private ITaskItem[] bootstrapperItems;
        private string bootstrapperKeyFile;
        private string componentsLocation;
        private string componentsUrl;
        private bool copyComponents = true;
        private string culture = Util.DefaultCultureInfo.Name;
        private string fallbackCulture = Util.DefaultCultureInfo.Name;
        private string outputPath = Environment.CurrentDirectory;
        private string path = Util.DefaultPath;
        private string supportUrl;
        private bool validate = true;

        private Microsoft.Build.Tasks.Deployment.Bootstrapper.ComponentsLocation ConvertStringToComponentsLocation(string parameterValue)
        {
            if ((parameterValue == null) || (parameterValue.Length == 0))
            {
                return Microsoft.Build.Tasks.Deployment.Bootstrapper.ComponentsLocation.HomeSite;
            }
            try
            {
                return (Microsoft.Build.Tasks.Deployment.Bootstrapper.ComponentsLocation) Enum.Parse(typeof(Microsoft.Build.Tasks.Deployment.Bootstrapper.ComponentsLocation), parameterValue, false);
            }
            catch (FormatException)
            {
                base.Log.LogWarningWithCodeFromResources("GenerateBootstrapper.InvalidComponentsLocation", new object[] { parameterValue });
                return Microsoft.Build.Tasks.Deployment.Bootstrapper.ComponentsLocation.HomeSite;
            }
        }

        public override bool Execute()
        {
            BootstrapperBuilder builder = new BootstrapperBuilder {
                Validate = this.Validate,
                Path = this.Path
            };
            ProductCollection products = builder.Products;
            BuildSettings settings = new BuildSettings {
                ApplicationFile = this.ApplicationFile,
                ApplicationName = this.ApplicationName,
                ApplicationRequiresElevation = this.ApplicationRequiresElevation,
                ApplicationUrl = this.ApplicationUrl,
                ComponentsLocation = this.ConvertStringToComponentsLocation(this.ComponentsLocation),
                ComponentsUrl = this.ComponentsUrl,
                CopyComponents = this.CopyComponents,
                Culture = this.culture,
                FallbackCulture = this.fallbackCulture,
                OutputPath = this.OutputPath,
                SupportUrl = this.SupportUrl
            };
            if (this.BootstrapperItems != null)
            {
                Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                foreach (ITaskItem item in this.BootstrapperItems)
                {
                    string metadata = item.GetMetadata("Install");
                    if (string.IsNullOrEmpty(metadata) || ConversionUtilities.ConvertStringToBool(metadata))
                    {
                        if (!hashtable.Contains(item.ItemSpec))
                        {
                            hashtable.Add(item.ItemSpec, item);
                        }
                        else
                        {
                            base.Log.LogWarningWithCodeFromResources("GenerateBootstrapper.DuplicateItems", new object[] { item.ItemSpec });
                        }
                    }
                }
                foreach (Product product in products)
                {
                    if (hashtable.Contains(product.ProductCode))
                    {
                        settings.ProductBuilders.Add(product.ProductBuilder);
                        hashtable.Remove(product.ProductCode);
                    }
                }
                foreach (ITaskItem item2 in hashtable.Values)
                {
                    base.Log.LogWarningWithCodeFromResources("GenerateBootstrapper.ProductNotFound", new object[] { item2.ItemSpec, builder.Path });
                }
            }
            BuildResults results = builder.Build(settings);
            BuildMessage[] messages = results.Messages;
            if (messages != null)
            {
                foreach (BuildMessage message in messages)
                {
                    if (message.Severity == BuildMessageSeverity.Error)
                    {
                        base.Log.LogError(null, message.HelpCode, message.HelpKeyword, null, 0, 0, 0, 0, message.Message, new object[0]);
                    }
                    else if (message.Severity == BuildMessageSeverity.Warning)
                    {
                        base.Log.LogWarning(null, message.HelpCode, message.HelpKeyword, null, 0, 0, 0, 0, message.Message, new object[0]);
                    }
                }
            }
            this.BootstrapperKeyFile = results.KeyFile;
            this.BootstrapperComponentFiles = results.ComponentFiles;
            return results.Succeeded;
        }

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

        [Output]
        public string[] BootstrapperComponentFiles
        {
            get
            {
                return this.bootstrapperComponentFiles;
            }
            set
            {
                this.bootstrapperComponentFiles = value;
            }
        }

        public ITaskItem[] BootstrapperItems
        {
            get
            {
                return this.bootstrapperItems;
            }
            set
            {
                this.bootstrapperItems = value;
            }
        }

        [Output]
        public string BootstrapperKeyFile
        {
            get
            {
                return this.bootstrapperKeyFile;
            }
            set
            {
                this.bootstrapperKeyFile = value;
            }
        }

        public string ComponentsLocation
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
                return this.copyComponents;
            }
            set
            {
                this.copyComponents = value;
            }
        }

        public string Culture
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

        public string FallbackCulture
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

        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
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
                return this.validate;
            }
            set
            {
                this.validate = value;
            }
        }
    }
}

