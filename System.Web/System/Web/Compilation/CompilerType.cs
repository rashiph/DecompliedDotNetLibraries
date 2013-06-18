namespace System.Web.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Web;
    using System.Web.Configuration;

    public sealed class CompilerType
    {
        private Type _codeDomProviderType;
        private System.CodeDom.Compiler.CompilerParameters _compilParams;

        internal CompilerType(Type codeDomProviderType, System.CodeDom.Compiler.CompilerParameters compilParams)
        {
            this._codeDomProviderType = codeDomProviderType;
            if (compilParams == null)
            {
                this._compilParams = new System.CodeDom.Compiler.CompilerParameters();
            }
            else
            {
                this._compilParams = compilParams;
            }
        }

        internal CompilerType Clone()
        {
            return new CompilerType(this._codeDomProviderType, this.CloneCompilerParameters());
        }

        private System.CodeDom.Compiler.CompilerParameters CloneCompilerParameters()
        {
            return new System.CodeDom.Compiler.CompilerParameters { IncludeDebugInformation = this._compilParams.IncludeDebugInformation, TreatWarningsAsErrors = this._compilParams.TreatWarningsAsErrors, WarningLevel = this._compilParams.WarningLevel, CompilerOptions = this._compilParams.CompilerOptions };
        }

        internal AssemblyBuilder CreateAssemblyBuilder(CompilationSection compConfig, ICollection referencedAssemblies)
        {
            return this.CreateAssemblyBuilder(compConfig, referencedAssemblies, null, null);
        }

        internal AssemblyBuilder CreateAssemblyBuilder(CompilationSection compConfig, ICollection referencedAssemblies, string generatedFilesDir, string outputAssemblyName)
        {
            if (generatedFilesDir != null)
            {
                return new CbmCodeGeneratorBuildProviderHost(compConfig, referencedAssemblies, this, generatedFilesDir, outputAssemblyName);
            }
            return new AssemblyBuilder(compConfig, referencedAssemblies, this, outputAssemblyName);
        }

        public override bool Equals(object o)
        {
            CompilerType type = o as CompilerType;
            if (o == null)
            {
                return false;
            }
            return ((((this._codeDomProviderType == type._codeDomProviderType) && (this._compilParams.WarningLevel == type._compilParams.WarningLevel)) && (this._compilParams.IncludeDebugInformation == type._compilParams.IncludeDebugInformation)) && (this._compilParams.CompilerOptions == type._compilParams.CompilerOptions));
        }

        internal static AssemblyBuilder GetDefaultAssemblyBuilder(CompilationSection compConfig, ICollection referencedAssemblies, VirtualPath configPath, string outputAssemblyName)
        {
            return GetDefaultAssemblyBuilder(compConfig, referencedAssemblies, configPath, null, outputAssemblyName);
        }

        internal static AssemblyBuilder GetDefaultAssemblyBuilder(CompilationSection compConfig, ICollection referencedAssemblies, VirtualPath configPath, string generatedFilesDir, string outputAssemblyName)
        {
            return GetDefaultCompilerTypeWithParams(compConfig, configPath).CreateAssemblyBuilder(compConfig, referencedAssemblies, generatedFilesDir, outputAssemblyName);
        }

        private static CompilerType GetDefaultCompilerTypeWithParams(CompilationSection compConfig, VirtualPath configPath)
        {
            return CompilationUtil.GetCSharpCompilerInfo(compConfig, configPath);
        }

        public override int GetHashCode()
        {
            return this._codeDomProviderType.GetHashCode();
        }

        public Type CodeDomProviderType
        {
            get
            {
                return this._codeDomProviderType;
            }
        }

        public System.CodeDom.Compiler.CompilerParameters CompilerParameters
        {
            get
            {
                return this._compilParams;
            }
        }
    }
}

