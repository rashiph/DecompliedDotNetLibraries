namespace System.Web.Configuration
{
    using System;
    using System.CodeDom.Compiler;
    using System.Configuration;
    using System.Web.Compilation;

    public sealed class Compiler : ConfigurationElement
    {
        private CompilerType _compilerType;
        private static readonly ConfigurationProperty _propCompilerOptions = new ConfigurationProperty("compilerOptions", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propExtension = new ConfigurationProperty("extension", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propLanguage = new ConfigurationProperty("language", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propType = new ConfigurationProperty("type", typeof(string), string.Empty, ConfigurationPropertyOptions.IsTypeStringTransformationRequired | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propWarningLevel = new ConfigurationProperty("warningLevel", typeof(int), 0, null, new IntegerValidator(0, 4), ConfigurationPropertyOptions.None);
        private const string compilerOptionsAttribName = "compilerOptions";

        static Compiler()
        {
            _properties.Add(_propLanguage);
            _properties.Add(_propExtension);
            _properties.Add(_propType);
            _properties.Add(_propWarningLevel);
            _properties.Add(_propCompilerOptions);
        }

        internal Compiler()
        {
        }

        public Compiler(string compilerOptions, string extension, string language, string type, int warningLevel) : this()
        {
            base[_propCompilerOptions] = compilerOptions;
            base[_propExtension] = extension;
            base[_propLanguage] = language;
            base[_propType] = type;
            base[_propWarningLevel] = warningLevel;
        }

        [ConfigurationProperty("compilerOptions", DefaultValue="")]
        public string CompilerOptions
        {
            get
            {
                return (string) base[_propCompilerOptions];
            }
        }

        internal CompilerType CompilerTypeInternal
        {
            get
            {
                if (this._compilerType == null)
                {
                    lock (this)
                    {
                        if (this._compilerType == null)
                        {
                            System.Type codeDomProviderType = CompilationUtil.LoadTypeWithChecks(this.Type, typeof(CodeDomProvider), null, this, "type");
                            CompilerParameters compilParams = new CompilerParameters {
                                WarningLevel = this.WarningLevel,
                                TreatWarningsAsErrors = this.WarningLevel > 0
                            };
                            string compilerOptions = this.CompilerOptions;
                            CompilationUtil.CheckCompilerOptionsAllowed(compilerOptions, true, base.ElementInformation.Properties["compilerOptions"].Source, base.ElementInformation.Properties["compilerOptions"].LineNumber);
                            compilParams.CompilerOptions = compilerOptions;
                            this._compilerType = new CompilerType(codeDomProviderType, compilParams);
                        }
                    }
                }
                return this._compilerType;
            }
        }

        [ConfigurationProperty("extension", DefaultValue="")]
        public string Extension
        {
            get
            {
                return (string) base[_propExtension];
            }
        }

        [ConfigurationProperty("language", DefaultValue="", IsRequired=true, IsKey=true)]
        public string Language
        {
            get
            {
                return (string) base[_propLanguage];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("type", IsRequired=true, DefaultValue="")]
        public string Type
        {
            get
            {
                return (string) base[_propType];
            }
        }

        [IntegerValidator(MinValue=0, MaxValue=4), ConfigurationProperty("warningLevel", DefaultValue=0)]
        public int WarningLevel
        {
            get
            {
                return (int) base[_propWarningLevel];
            }
        }
    }
}

