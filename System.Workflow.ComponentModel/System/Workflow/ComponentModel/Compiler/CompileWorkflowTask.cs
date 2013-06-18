namespace System.Workflow.ComponentModel.Compiler
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;
    using Microsoft.Win32;
    using Microsoft.Workflow.Compiler;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    public sealed class CompileWorkflowTask : Task, ITask
    {
        private string assemblyName;
        private ITaskItem[] compilationOptions;
        private bool delaySign;
        private object hostObject;
        private string imports;
        private string keyContainer;
        private string keyFile;
        private ITaskItem[] outputFiles;
        private string projectDirectory;
        private string projectExt;
        private SupportedLanguages projectType;
        private ITaskItem[] referenceFiles;
        private ITaskItem[] resourceFiles;
        private string rootNamespace;
        private ITaskItem[] sourceCodeFiles;
        private string targetFramework;
        private StringCollection temporaryFiles;
        private ITaskItem[] xomlFiles;

        public CompileWorkflowTask() : base(new ResourceManager("System.Workflow.ComponentModel.BuildTasksStrings", Assembly.GetExecutingAssembly()))
        {
            this.temporaryFiles = new StringCollection();
        }

        public override bool Execute()
        {
            CompilerOptionsBuilder builder;
            if (!this.ValidateParameters())
            {
                return false;
            }
            if (this.WorkflowMarkupFiles == null)
            {
                base.Log.LogMessageFromResources(MessageImportance.Normal, "NoXomlFiles", new object[0]);
            }
            if ((this.ReferenceFiles == null) || (this.ReferenceFiles.Length == 0))
            {
                base.Log.LogMessageFromResources(MessageImportance.Normal, "NoReferenceFiles", new object[0]);
            }
            if ((this.SourceCodeFiles == null) || (this.SourceCodeFiles.Length == 0))
            {
                base.Log.LogMessageFromResources(MessageImportance.Normal, "NoSourceCodeFiles", new object[0]);
            }
            if (((this.HostObject == null) || ((this.HostObject is IWorkflowBuildHostProperties) && ((IWorkflowBuildHostProperties) this.HostObject).SkipWorkflowCompilation)) && (string.Compare(Process.GetCurrentProcess().ProcessName, "devenv", StringComparison.OrdinalIgnoreCase) == 0))
            {
                return true;
            }
            int num = 0;
            int num2 = 0;
            WorkflowCompilerParameters parameters = new WorkflowCompilerParameters();
            IWorkflowCompilerErrorLogger service = null;
            IServiceProvider provider = null;
            if (this.HostObject is IOleServiceProvider)
            {
                provider = new ServiceProvider(this.HostObject as IOleServiceProvider);
                service = provider.GetService(typeof(IWorkflowCompilerErrorLogger)) as IWorkflowCompilerErrorLogger;
            }
            string[] strArray = GetFiles(this.SourceCodeFiles, this.ProjectDirectory);
            foreach (ITaskItem item in this.ReferenceFiles)
            {
                parameters.ReferencedAssemblies.Add(item.ItemSpec);
            }
            if (!string.IsNullOrEmpty(this.targetFramework))
            {
                parameters.MultiTargetingInformation = new MultiTargetingInfo(this.targetFramework);
            }
            if (this.ProjectType != SupportedLanguages.VB)
            {
                builder = new CompilerOptionsBuilder();
            }
            else
            {
                string compilerVersion = parameters.CompilerVersion;
                if (compilerVersion != null)
                {
                    if (!(compilerVersion == "v2.0"))
                    {
                        if (compilerVersion == "v3.5")
                        {
                            builder = new OrcasVBCompilerOptionsBuilder();
                            goto Label_01BE;
                        }
                    }
                    else
                    {
                        builder = new WhidbeyVBCompilerOptionsBuilder();
                        goto Label_01BE;
                    }
                }
                builder = new CompilerOptionsBuilder();
            }
        Label_01BE:
            parameters.CompilerOptions = this.PrepareCompilerOptions(builder);
            parameters.GenerateCodeCompileUnitOnly = true;
            parameters.LanguageToUse = this.ProjectType.ToString();
            parameters.TempFiles.KeepFiles = this.ShouldKeepTempFiles();
            parameters.OutputAssembly = this.AssemblyName;
            if (!string.IsNullOrEmpty(this.assemblyName))
            {
                string str = parameters.GenerateExecutable ? ".exe" : ".dll";
                parameters.OutputAssembly = parameters.OutputAssembly + str;
            }
            CodeDomProvider provider2 = null;
            if (this.ProjectType == SupportedLanguages.VB)
            {
                provider2 = CompilerHelpers.CreateCodeProviderInstance(typeof(VBCodeProvider), parameters.CompilerVersion);
            }
            else
            {
                provider2 = CompilerHelpers.CreateCodeProviderInstance(typeof(CSharpCodeProvider), parameters.CompilerVersion);
            }
            using (TempFileCollection files = new TempFileCollection(Environment.GetEnvironmentVariable("temp", EnvironmentVariableTarget.User), true))
            {
                string[] strArray2;
                this.outputFiles = new TaskItem[1];
                if (this.WorkflowMarkupFiles != null)
                {
                    strArray2 = new string[this.WorkflowMarkupFiles.GetLength(0) + strArray.Length];
                    int index = 0;
                    while (index < this.WorkflowMarkupFiles.GetLength(0))
                    {
                        strArray2[index] = Path.Combine(this.ProjectDirectory, this.WorkflowMarkupFiles[index].ItemSpec);
                        index++;
                    }
                    strArray.CopyTo(strArray2, index);
                }
                else
                {
                    strArray2 = new string[strArray.Length];
                    strArray.CopyTo(strArray2, 0);
                }
                WorkflowCompilerResults results = new CompilerWrapper().Compile(parameters, strArray2);
                foreach (WorkflowCompilerError error in results.Errors)
                {
                    if (error.IsWarning)
                    {
                        num2++;
                        if (service != null)
                        {
                            error.FileName = Path.Combine(this.ProjectDirectory, error.FileName);
                            service.LogError(error);
                            service.LogMessage(error.ToString() + "\n");
                        }
                        else
                        {
                            base.Log.LogWarning(error.ErrorText, new object[] { error.ErrorNumber, error.FileName, error.Line, error.Column });
                        }
                    }
                    else
                    {
                        num++;
                        if (service != null)
                        {
                            error.FileName = Path.Combine(this.ProjectDirectory, error.FileName);
                            service.LogError(error);
                            service.LogMessage(error.ToString() + "\n");
                        }
                        else
                        {
                            base.Log.LogError(error.ErrorText, new object[] { error.ErrorNumber, error.FileName, error.Line, error.Column });
                        }
                    }
                }
                if (!results.Errors.HasErrors)
                {
                    CodeCompileUnit compiledUnit = results.CompiledUnit;
                    if (compiledUnit != null)
                    {
                        WorkflowMarkupSerializationHelpers.FixStandardNamespacesAndRootNamespace(compiledUnit.Namespaces, this.RootNamespace, CompilerHelpers.GetSupportedLanguage(this.ProjectType.ToString()));
                        string path = files.AddExtension(provider2.FileExtension);
                        using (StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write), Encoding.UTF8))
                        {
                            CodeGeneratorOptions options = new CodeGeneratorOptions {
                                BracingStyle = "C"
                            };
                            provider2.GenerateCodeFromCompileUnit(compiledUnit, writer, options);
                        }
                        this.outputFiles[0] = new TaskItem(path);
                        this.temporaryFiles.Add(path);
                        base.Log.LogMessageFromResources(MessageImportance.Normal, "TempCodeFile", new object[] { path });
                    }
                }
            }
            if (((num > 0) || (num2 > 0)) && (service != null))
            {
                service.LogMessage(string.Format(CultureInfo.CurrentCulture, "\nCompile complete -- {0} errors, {1} warnings \n", new object[] { num, num2 }));
            }
            base.Log.LogMessageFromResources(MessageImportance.Normal, "XomlValidationCompleted", new object[] { num, num2 });
            return (num == 0);
        }

        private static string[] GetFiles(ITaskItem[] taskItems, string projDir)
        {
            if (taskItems == null)
            {
                return new string[0];
            }
            string[] strArray = new string[taskItems.Length];
            for (int i = 0; i < taskItems.Length; i++)
            {
                if (projDir != null)
                {
                    strArray[i] = Path.Combine(projDir, taskItems[i].ItemSpec);
                }
                else
                {
                    strArray[i] = taskItems[i].ItemSpec;
                }
            }
            return strArray;
        }

        private static bool HasManifestResourceName(ITaskItem resourceFile, out string manifestResourceName)
        {
            IEnumerator enumerator = resourceFile.MetadataNames.GetEnumerator();
            manifestResourceName = null;
            bool flag = false;
            while (!flag && enumerator.MoveNext())
            {
                string current = (string) enumerator.Current;
                if (current == "ManifestResourceName")
                {
                    flag = true;
                    manifestResourceName = resourceFile.GetMetadata(current);
                }
            }
            return flag;
        }

        private string PrepareCompilerOptions(CompilerOptionsBuilder optionsBuilder)
        {
            StringBuilder options = new StringBuilder();
            if (this.DelaySign)
            {
                options.Append(" /delaysign+");
            }
            if ((this.KeyContainer != null) && (this.KeyContainer.Trim().Length > 0))
            {
                options.AppendFormat(" /keycontainer:{0}", this.KeyContainer);
            }
            if ((this.KeyFile != null) && (this.KeyFile.Trim().Length > 0))
            {
                options.AppendFormat(" /keyfile:\"{0}\"", Path.Combine(this.ProjectDirectory, this.KeyFile));
            }
            if ((this.compilationOptions != null) && (this.compilationOptions.Length > 0))
            {
                foreach (ITaskItem item in this.compilationOptions)
                {
                    optionsBuilder.AddCustomOption(options, item);
                }
            }
            if ((this.resourceFiles != null) && (this.resourceFiles.Length > 0))
            {
                foreach (ITaskItem item2 in this.resourceFiles)
                {
                    string str;
                    if (HasManifestResourceName(item2, out str))
                    {
                        options.AppendFormat(" /resource:\"{0}\",{1}", Path.Combine(this.ProjectDirectory, item2.ItemSpec), str);
                    }
                    else
                    {
                        options.AppendFormat(" /resource:\"{0}\"", Path.Combine(this.ProjectDirectory, item2.ItemSpec));
                    }
                }
            }
            if (this.ProjectType == SupportedLanguages.VB)
            {
                if (!string.IsNullOrEmpty(this.RootNamespace))
                {
                    options.AppendFormat(" /rootnamespace:{0}", this.RootNamespace);
                }
                options.AppendFormat(" /imports:{0}", this.Imports.Replace(';', ','));
            }
            if (char.IsWhiteSpace(options[0]))
            {
                options.Remove(0, 0);
            }
            return options.ToString();
        }

        private bool ShouldKeepTempFiles()
        {
            bool flag = false;
            if (this.ProjectType == SupportedLanguages.VB)
            {
                return true;
            }
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(Helpers.ProductRootRegKey);
                if (key != null)
                {
                    flag = Convert.ToInt32(key.GetValue("KeepTempFiles"), CultureInfo.InvariantCulture) != 0;
                }
            }
            catch
            {
            }
            return flag;
        }

        private bool ValidateParameters()
        {
            if ((this.ProjectDirectory == null) || (this.ProjectDirectory.Trim().Length == 0))
            {
                base.Log.LogErrorFromResources("NoProjectType", new object[0]);
                return false;
            }
            if ((this.ProjectExtension == null) || (this.ProjectExtension.Trim().Length == 0))
            {
                base.Log.LogErrorFromResources("NoProjectType", new object[0]);
                return false;
            }
            if ((string.Compare(this.ProjectExtension, ".csproj", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(this.ProjectExtension, ".vbproj", StringComparison.OrdinalIgnoreCase) != 0))
            {
                base.Log.LogErrorFromResources("UnsupportedProjectType", new object[0]);
                return false;
            }
            return true;
        }

        public string AssemblyName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.assemblyName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.assemblyName = value;
            }
        }

        public ITaskItem[] CompilationOptions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.compilationOptions;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.compilationOptions = value;
            }
        }

        public bool DelaySign
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.delaySign;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.delaySign = value;
            }
        }

        public object HostObject
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.hostObject;
            }
        }

        public string Imports
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.imports;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.imports = value;
            }
        }

        [Output]
        public string KeepTemporaryFiles
        {
            get
            {
                return this.ShouldKeepTempFiles().ToString();
            }
        }

        public string KeyContainer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.keyContainer;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.keyContainer = value;
            }
        }

        public string KeyFile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.keyFile;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.keyFile = value;
            }
        }

        ITaskHost ITask.HostObject
        {
            get
            {
                return (ITaskHost) this.hostObject;
            }
            set
            {
                this.hostObject = value;
            }
        }

        [Output]
        public ITaskItem[] OutputFiles
        {
            get
            {
                if (this.outputFiles == null)
                {
                    if (this.ProjectType == SupportedLanguages.VB)
                    {
                        this.outputFiles = new ITaskItem[0];
                    }
                    else
                    {
                        ArrayList list = new ArrayList();
                        if (this.WorkflowMarkupFiles != null)
                        {
                            list.AddRange(this.WorkflowMarkupFiles);
                        }
                        this.outputFiles = list.ToArray(typeof(ITaskItem)) as ITaskItem[];
                    }
                }
                return this.outputFiles;
            }
        }

        public string ProjectDirectory
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.projectDirectory;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.projectDirectory = value;
            }
        }

        public string ProjectExtension
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.projectExt;
            }
            set
            {
                this.projectExt = value;
                if (string.Compare(this.projectExt, ".csproj", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.ProjectType = SupportedLanguages.CSharp;
                }
                else if (string.Compare(this.projectExt, ".vbproj", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.ProjectType = SupportedLanguages.VB;
                }
            }
        }

        private SupportedLanguages ProjectType
        {
            get
            {
                return this.projectType;
            }
            set
            {
                this.projectType = value;
            }
        }

        public ITaskItem[] ReferenceFiles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.referenceFiles;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.referenceFiles = value;
            }
        }

        public ITaskItem[] ResourceFiles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.resourceFiles;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.resourceFiles = value;
            }
        }

        public string RootNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rootNamespace;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.rootNamespace = value;
            }
        }

        public ITaskItem[] SourceCodeFiles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.sourceCodeFiles;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.sourceCodeFiles = value;
            }
        }

        public string TargetFramework
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetFramework;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.targetFramework = value;
            }
        }

        [Output]
        public string[] TemporaryFiles
        {
            get
            {
                string[] array = new string[this.temporaryFiles.Count];
                this.temporaryFiles.CopyTo(array, 0);
                return array;
            }
        }

        public ITaskItem[] WorkflowMarkupFiles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.xomlFiles;
            }
            set
            {
                if (value != null)
                {
                    ArrayList list = new ArrayList();
                    foreach (ITaskItem item in value)
                    {
                        if (item != null)
                        {
                            string itemSpec = item.ItemSpec;
                            if ((itemSpec != null) && itemSpec.EndsWith(".xoml", StringComparison.OrdinalIgnoreCase))
                            {
                                list.Add(item);
                            }
                        }
                    }
                    if (list.Count > 0)
                    {
                        this.xomlFiles = list.ToArray(typeof(ITaskItem)) as ITaskItem[];
                    }
                }
                else
                {
                    this.xomlFiles = value;
                }
            }
        }

        private class CompilerOptionsBuilder
        {
            public void AddCustomOption(StringBuilder options, ITaskItem option)
            {
                string str;
                string str2;
                string str3;
                this.GetOptionInfo(option, out str, out str2, out str3);
                if (!string.IsNullOrWhiteSpace(str))
                {
                    if (string.IsNullOrEmpty(str2))
                    {
                        options.AppendFormat(" /{0}", str);
                    }
                    else if (string.IsNullOrEmpty(str3))
                    {
                        options.AppendFormat(" /{0}{1}", str, str2);
                    }
                    else
                    {
                        options.AppendFormat(" /{0}{1}{2}", str, str3, str2);
                    }
                }
            }

            protected virtual void GetOptionInfo(ITaskItem option, out string optionName, out string optionValue, out string optionDelimiter)
            {
                optionName = option.ItemSpec;
                optionValue = option.GetMetadata("value");
                optionDelimiter = option.GetMetadata("delimiter");
            }
        }

        private class OrcasVBCompilerOptionsBuilder : CompileWorkflowTask.VBCompilerOptionsBuilder
        {
            private static HashSet<string> validWarnings = new HashSet<string>(StringComparer.Ordinal) { 
                "40000", "40003", "40004", "40005", "40007", "40008", "40009", "40010", "40011", "40012", "40014", "40018", "40019", "40020", "40021", "40022", 
                "40023", "40024", "40025", "40026", "40027", "40028", "40029", "40030", "40031", "40032", "40033", "40034", "40035", "40038", "40039", "40040", 
                "40041", "40042", "40043", "40046", "40047", "40048", "40049", "40050", "40051", "40052", "40053", "40054", "40055", "40056", "40057", "41000", 
                "41001", "41002", "41003", "41004", "41005", "41006", "41007", "41008", "41998", "41999", "42000", "42001", "42002", "42004", "42014", "42015", 
                "42016", "42017", "42018", "42019", "42020", "42021", "42022", "42024", "42025", "42026", "42028", "42029", "42030", "42031", "42032", "42033", 
                "42034", "42035", "42036", "42099", "42101", "42102", "42104", "42105", "42106", "42107", "42108", "42109", "42110", "42111", "42200", "42203", 
                "42204", "42205", "42206", "42207", "42300", "42301", "42302", "42303", "42304", "42305", "42306", "42307", "42308", "42309", "42310", "42311", 
                "42312", "42313", "42314", "42315", "42316", "42317", "42318", "42319", "42320", "42321", "42322", "42324", "42326", "42327", "42328"
             };

            protected override bool IsValidWarning(string warning)
            {
                return validWarnings.Contains(warning);
            }
        }

        private abstract class VBCompilerOptionsBuilder : CompileWorkflowTask.CompilerOptionsBuilder
        {
            private const string SuppressWarningOption = "nowarn";

            protected VBCompilerOptionsBuilder()
            {
            }

            protected sealed override void GetOptionInfo(ITaskItem option, out string optionName, out string optionValue, out string optionDelimiter)
            {
                base.GetOptionInfo(option, out optionName, out optionValue, out optionDelimiter);
                if ((string.Compare(optionName, "nowarn", StringComparison.OrdinalIgnoreCase) == 0) && !string.IsNullOrWhiteSpace(optionValue))
                {
                    string[] strArray = optionValue.Split(new char[] { ',' });
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        string warning = strArray[i].Trim();
                        if (this.IsValidWarning(warning))
                        {
                            if (builder.Length == 0)
                            {
                                builder.Append(warning);
                            }
                            else
                            {
                                builder.AppendFormat(",{0}", warning);
                            }
                        }
                    }
                    optionValue = builder.ToString();
                    if (string.IsNullOrWhiteSpace(optionValue))
                    {
                        optionName = string.Empty;
                    }
                }
            }

            protected abstract bool IsValidWarning(string warning);
        }

        private class WhidbeyVBCompilerOptionsBuilder : CompileWorkflowTask.VBCompilerOptionsBuilder
        {
            private static HashSet<string> validWarnings = new HashSet<string>(StringComparer.Ordinal) { 
                "40000", "40003", "40004", "40005", "40007", "40008", "40009", "40010", "40011", "40012", "40014", "40018", "40019", "40020", "40021", "40022", 
                "40023", "40024", "40025", "40026", "40027", "40028", "40029", "40030", "40031", "40032", "40033", "40034", "40035", "40038", "40039", "40040", 
                "40041", "40042", "40043", "40046", "40047", "40048", "40049", "40050", "40051", "40052", "40053", "40054", "40055", "40056", "40057", "41000", 
                "41001", "41002", "41003", "41004", "41005", "41006", "41998", "41999", "42000", "42001", "42002", "42003", "42004", "42014", "42015", "42016", 
                "42017", "42018", "42019", "42020", "42021", "42022", "42024", "42025", "42026", "42028", "42029", "42030", "42031", "42032", "42033", "42034", 
                "42035", "42036", "42101", "42102", "42104", "42105", "42106", "42107", "42108", "42109", "42200", "42203", "42204", "42205", "42206", "42300", 
                "42301", "42302", "42303", "42304", "42305", "42306", "42307", "42308", "42309", "42310", "42311", "42312", "42313", "42314", "42315", "42316", 
                "42317", "42318", "42319", "42320", "42321"
             };

            protected override bool IsValidWarning(string warning)
            {
                return validWarnings.Contains(warning);
            }
        }
    }
}

