namespace System.CodeDom.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class CompilerInfo
    {
        internal string _codeDomProviderTypeName;
        internal string[] _compilerExtensions;
        internal string[] _compilerLanguages;
        internal CompilerParameters _compilerParams;
        internal bool _mapped;
        internal IDictionary<string, string> _providerOptions;
        internal int configFileLineNumber;
        internal string configFileName;
        private Type type;

        private CompilerInfo()
        {
        }

        internal CompilerInfo(CompilerParameters compilerParams, string codeDomProviderTypeName)
        {
            this._codeDomProviderTypeName = codeDomProviderTypeName;
            if (compilerParams == null)
            {
                compilerParams = new CompilerParameters();
            }
            this._compilerParams = compilerParams;
        }

        internal CompilerInfo(CompilerParameters compilerParams, string codeDomProviderTypeName, string[] compilerLanguages, string[] compilerExtensions)
        {
            this._compilerLanguages = compilerLanguages;
            this._compilerExtensions = compilerExtensions;
            this._codeDomProviderTypeName = codeDomProviderTypeName;
            if (compilerParams == null)
            {
                compilerParams = new CompilerParameters();
            }
            this._compilerParams = compilerParams;
        }

        private string[] CloneCompilerExtensions()
        {
            string[] destinationArray = new string[this._compilerExtensions.Length];
            Array.Copy(this._compilerExtensions, destinationArray, this._compilerExtensions.Length);
            return destinationArray;
        }

        private string[] CloneCompilerLanguages()
        {
            string[] destinationArray = new string[this._compilerLanguages.Length];
            Array.Copy(this._compilerLanguages, destinationArray, this._compilerLanguages.Length);
            return destinationArray;
        }

        private CompilerParameters CloneCompilerParameters()
        {
            return new CompilerParameters { IncludeDebugInformation = this._compilerParams.IncludeDebugInformation, TreatWarningsAsErrors = this._compilerParams.TreatWarningsAsErrors, WarningLevel = this._compilerParams.WarningLevel, CompilerOptions = this._compilerParams.CompilerOptions };
        }

        public CompilerParameters CreateDefaultCompilerParameters()
        {
            return this.CloneCompilerParameters();
        }

        public CodeDomProvider CreateProvider()
        {
            if (this._providerOptions.Count > 0)
            {
                ConstructorInfo constructor = this.CodeDomProviderType.GetConstructor(new Type[] { typeof(IDictionary<string, string>) });
                if (constructor != null)
                {
                    return (CodeDomProvider) constructor.Invoke(new object[] { this._providerOptions });
                }
            }
            return (CodeDomProvider) Activator.CreateInstance(this.CodeDomProviderType);
        }

        public CodeDomProvider CreateProvider(IDictionary<string, string> providerOptions)
        {
            if (providerOptions == null)
            {
                throw new ArgumentNullException("providerOptions");
            }
            ConstructorInfo constructor = this.CodeDomProviderType.GetConstructor(new Type[] { typeof(IDictionary<string, string>) });
            if (constructor == null)
            {
                throw new InvalidOperationException(System.SR.GetString("Provider_does_not_support_options", new object[] { this.CodeDomProviderType.ToString() }));
            }
            return (CodeDomProvider) constructor.Invoke(new object[] { providerOptions });
        }

        public override bool Equals(object o)
        {
            CompilerInfo info = o as CompilerInfo;
            if (o == null)
            {
                return false;
            }
            return ((((this.CodeDomProviderType == info.CodeDomProviderType) && (this.CompilerParams.WarningLevel == info.CompilerParams.WarningLevel)) && (this.CompilerParams.IncludeDebugInformation == info.CompilerParams.IncludeDebugInformation)) && (this.CompilerParams.CompilerOptions == info.CompilerParams.CompilerOptions));
        }

        public string[] GetExtensions()
        {
            return this.CloneCompilerExtensions();
        }

        public override int GetHashCode()
        {
            return this._codeDomProviderTypeName.GetHashCode();
        }

        public string[] GetLanguages()
        {
            return this.CloneCompilerLanguages();
        }

        public Type CodeDomProviderType
        {
            get
            {
                if (this.type == null)
                {
                    lock (this)
                    {
                        if (this.type == null)
                        {
                            this.type = Type.GetType(this._codeDomProviderTypeName);
                            if (this.type == null)
                            {
                                if (this.configFileName == null)
                                {
                                    throw new ConfigurationErrorsException(System.SR.GetString("Unable_To_Locate_Type", new object[] { this._codeDomProviderTypeName, string.Empty, 0 }));
                                }
                                throw new ConfigurationErrorsException(System.SR.GetString("Unable_To_Locate_Type", new object[] { this._codeDomProviderTypeName }), this.configFileName, this.configFileLineNumber);
                            }
                        }
                    }
                }
                return this.type;
            }
        }

        internal CompilerParameters CompilerParams
        {
            get
            {
                return this._compilerParams;
            }
        }

        public bool IsCodeDomProviderTypeValid
        {
            get
            {
                return (Type.GetType(this._codeDomProviderTypeName) != null);
            }
        }

        internal IDictionary<string, string> ProviderOptions
        {
            get
            {
                return this._providerOptions;
            }
        }
    }
}

