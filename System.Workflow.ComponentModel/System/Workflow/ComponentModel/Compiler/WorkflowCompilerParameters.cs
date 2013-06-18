namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text.RegularExpressions;

    [Serializable]
    public sealed class WorkflowCompilerParameters : CompilerParameters
    {
        private bool checkTypes;
        internal const string CheckTypesSwitch = "/checktypes";
        private string compilerOptions;
        private bool compileWithNoCode;
        private bool generateCCU;
        private string languageToUse;
        private StringCollection libraryPaths;
        private Assembly localAssembly;
        [OptionalField(VersionAdded=2)]
        private MultiTargetingInfo mtInfo;
        internal const string NoCodeSwitch = "/nocode";
        private IList<CodeCompileUnit> userCodeCCUs;

        public WorkflowCompilerParameters()
        {
            this.languageToUse = "CSharp";
        }

        public WorkflowCompilerParameters(string[] assemblyNames) : base(assemblyNames)
        {
            this.languageToUse = "CSharp";
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowCompilerParameters(WorkflowCompilerParameters parameters) : this(parameters, null)
        {
        }

        public WorkflowCompilerParameters(string[] assemblyNames, string outputName) : base(assemblyNames, outputName)
        {
            this.languageToUse = "CSharp";
        }

        internal WorkflowCompilerParameters(WorkflowCompilerParameters parameters, string[] newReferencedAssemblies) : this()
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            this.CompilerOptions = parameters.CompilerOptions;
            foreach (string str in parameters.EmbeddedResources)
            {
                base.EmbeddedResources.Add(str);
            }
            base.GenerateExecutable = parameters.GenerateExecutable;
            base.GenerateInMemory = parameters.GenerateInMemory;
            base.IncludeDebugInformation = parameters.IncludeDebugInformation;
            foreach (string str2 in parameters.LinkedResources)
            {
                base.LinkedResources.Add(str2);
            }
            base.MainClass = parameters.MainClass;
            base.OutputAssembly = parameters.OutputAssembly;
            if (newReferencedAssemblies != null)
            {
                base.ReferencedAssemblies.AddRange(newReferencedAssemblies);
            }
            else
            {
                foreach (string str3 in parameters.ReferencedAssemblies)
                {
                    base.ReferencedAssemblies.Add(str3);
                }
            }
            base.TreatWarningsAsErrors = parameters.TreatWarningsAsErrors;
            base.UserToken = parameters.UserToken;
            base.WarningLevel = parameters.WarningLevel;
            base.Win32Resource = parameters.Win32Resource;
            this.generateCCU = parameters.generateCCU;
            this.languageToUse = parameters.languageToUse;
            if (parameters.libraryPaths != null)
            {
                this.libraryPaths = new StringCollection();
                foreach (string str4 in parameters.libraryPaths)
                {
                    this.libraryPaths.Add(str4);
                }
            }
            if (parameters.userCodeCCUs != null)
            {
                this.userCodeCCUs = new List<CodeCompileUnit>(parameters.userCodeCCUs);
            }
            this.localAssembly = parameters.localAssembly;
        }

        public WorkflowCompilerParameters(string[] assemblyNames, string outputName, bool includeDebugInformation) : base(assemblyNames, outputName, includeDebugInformation)
        {
            this.languageToUse = "CSharp";
        }

        internal static string ExtractRootNamespace(WorkflowCompilerParameters parameters)
        {
            string str = string.Empty;
            if ((parameters.CompilerOptions != null) && (CompilerHelpers.GetSupportedLanguage(parameters.LanguageToUse) == SupportedLanguages.VB))
            {
                Match match = new Regex(@"\s*[/-]rootnamespace[:=]\s*(?<RootNamespace>[^\s]*)").Match(parameters.CompilerOptions);
                if (match.Success)
                {
                    str = match.Groups["RootNamespace"].Value;
                }
            }
            return str;
        }

        internal bool CheckTypes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.checkTypes;
            }
        }

        public string CompilerOptions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.compilerOptions;
            }
            set
            {
                this.compilerOptions = value;
                base.CompilerOptions = XomlCompilerHelper.ProcessCompilerOptions(value, out this.compileWithNoCode, out this.checkTypes);
            }
        }

        internal string CompilerVersion
        {
            get
            {
                if (this.mtInfo == null)
                {
                    return string.Empty;
                }
                return this.mtInfo.CompilerVersion;
            }
        }

        internal bool CompileWithNoCode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.compileWithNoCode;
            }
        }

        public bool GenerateCodeCompileUnitOnly
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.generateCCU;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.generateCCU = value;
            }
        }

        public string LanguageToUse
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.languageToUse;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                if ((string.Compare(value, SupportedLanguages.CSharp.ToString(), StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(value, SupportedLanguages.VB.ToString(), StringComparison.OrdinalIgnoreCase) != 0))
                {
                    throw new NotSupportedException(SR.GetString("Error_LanguageNeedsToBeVBCSharp", new object[] { value }));
                }
                this.languageToUse = value;
            }
        }

        public StringCollection LibraryPaths
        {
            get
            {
                if (this.libraryPaths == null)
                {
                    this.libraryPaths = new StringCollection();
                }
                return this.libraryPaths;
            }
        }

        internal Assembly LocalAssembly
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.localAssembly;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.localAssembly = value;
            }
        }

        internal MultiTargetingInfo MultiTargetingInformation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.mtInfo;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.mtInfo = value;
            }
        }

        public IList<CodeCompileUnit> UserCodeCompileUnits
        {
            get
            {
                if (this.userCodeCCUs == null)
                {
                    this.userCodeCCUs = new List<CodeCompileUnit>();
                }
                return this.userCodeCCUs;
            }
        }
    }
}

