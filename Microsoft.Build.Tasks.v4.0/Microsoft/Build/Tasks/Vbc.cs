namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Tasks.Hosting;
    using Microsoft.Build.Tasks.InteropUtilities;
    using Microsoft.Build.Utilities;
    using Microsoft.Internal.Performance;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class Vbc : ManagedCompiler
    {
        private bool isDoneOutputtingErrorMessage;
        private int numberOfLinesInErrorMessage;
        private bool useHostCompilerIfAvailable;
        private Queue<VBError> vbErrorLines = new Queue<VBError>();

        private void AddReferencesToCommandLine(CommandLineBuilderExtension commandLine)
        {
            if ((base.References != null) && (base.References.Length != 0))
            {
                List<ITaskItem> list = new List<ITaskItem>(base.References.Length);
                List<ITaskItem> list2 = new List<ITaskItem>(base.References.Length);
                foreach (ITaskItem item in base.References)
                {
                    if (MetadataConversionUtilities.TryConvertItemMetadataToBool(item, "EmbedInteropTypes"))
                    {
                        list2.Add(item);
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
                if (list2.Count > 0)
                {
                    commandLine.AppendSwitchIfNotNull("/link:", list2.ToArray(), ",");
                }
                if (list.Count > 0)
                {
                    commandLine.AppendSwitchIfNotNull("/reference:", list.ToArray(), ",");
                }
            }
        }

        protected internal override void AddResponseFileCommands(CommandLineBuilderExtension commandLine)
        {
            commandLine.AppendSwitchIfNotNull("/baseaddress:", this.GetBaseAddressInHex());
            commandLine.AppendSwitchIfNotNull("/libpath:", base.AdditionalLibPaths, ",");
            commandLine.AppendSwitchIfNotNull("/imports:", this.Imports, ",");
            commandLine.AppendPlusOrMinusSwitch("/doc", base.Bag, "GenerateDocumentation");
            commandLine.AppendSwitchIfNotNull("/optioncompare:", this.OptionCompare);
            commandLine.AppendPlusOrMinusSwitch("/optionexplicit", base.Bag, "OptionExplicit");
            object obj2 = base.Bag["OptionStrict"];
            if ((obj2 != null) ? ((bool) obj2) : false)
            {
                commandLine.AppendSwitch("/optionstrict+");
            }
            else
            {
                commandLine.AppendSwitch("/optionstrict:custom");
            }
            commandLine.AppendSwitchIfNotNull("/optionstrict:", this.OptionStrictType);
            commandLine.AppendWhenTrue("/nowarn", base.Bag, "NoWarnings");
            commandLine.AppendSwitchWithSplitting("/nowarn:", this.DisabledWarnings, ",", new char[] { ';', ',' });
            commandLine.AppendPlusOrMinusSwitch("/optioninfer", base.Bag, "OptionInfer");
            commandLine.AppendWhenTrue("/nostdlib", base.Bag, "NoStandardLib");
            commandLine.AppendWhenTrue("/novbruntimeref", base.Bag, "NoVBRuntimeReference");
            commandLine.AppendSwitchIfNotNull("/errorreport:", this.ErrorReport);
            commandLine.AppendSwitchIfNotNull("/platform:", this.Platform);
            commandLine.AppendPlusOrMinusSwitch("/removeintchecks", base.Bag, "RemoveIntegerChecks");
            commandLine.AppendSwitchIfNotNull("/rootnamespace:", this.RootNamespace);
            commandLine.AppendSwitchIfNotNull("/sdkpath:", this.SdkPath);
            commandLine.AppendSwitchIfNotNull("/langversion:", this.LangVersion);
            commandLine.AppendSwitchIfNotNull("/moduleassemblyname:", this.ModuleAssemblyName);
            commandLine.AppendWhenTrue("/netcf", base.Bag, "TargetCompactFramework");
            if (this.VBRuntime != null)
            {
                string vBRuntime = this.VBRuntime;
                if (string.Compare(vBRuntime, "EMBED", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    commandLine.AppendSwitch("/vbruntime*");
                }
                else if (string.Compare(vBRuntime, "NONE", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    commandLine.AppendSwitch("/vbruntime-");
                }
                else if (string.Compare(vBRuntime, "DEFAULT", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    commandLine.AppendSwitch("/vbruntime+");
                }
                else
                {
                    commandLine.AppendSwitchIfNotNull("/vbruntime:", vBRuntime);
                }
            }
            if ((this.Verbosity != null) && ((string.Compare(this.Verbosity, "quiet", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(this.Verbosity, "verbose", StringComparison.OrdinalIgnoreCase) == 0)))
            {
                commandLine.AppendSwitchIfNotNull("/", this.Verbosity);
            }
            commandLine.AppendSwitchIfNotNull("/doc:", this.DocumentationFile);
            commandLine.AppendSwitchUnquotedIfNotNull("/define:", GetDefineConstantsSwitch(base.DefineConstants));
            this.AddReferencesToCommandLine(commandLine);
            commandLine.AppendSwitchIfNotNull("/win32resource:", base.Win32Resource);
            if (string.Compare("Sub Main", base.MainEntryPoint, StringComparison.OrdinalIgnoreCase) != 0)
            {
                commandLine.AppendSwitchIfNotNull("/main:", base.MainEntryPoint);
            }
            base.AddResponseFileCommands(commandLine);
            commandLine.AppendSwitchWithSplitting("/warnaserror+:", this.WarningsAsErrors, ",", new char[] { ';', ',' });
            commandLine.AppendSwitchWithSplitting("/warnaserror-:", this.WarningsNotAsErrors, ",", new char[] { ';', ',' });
            if (base.ResponseFiles != null)
            {
                foreach (ITaskItem item in base.ResponseFiles)
                {
                    commandLine.AppendSwitchIfNotNull("@", item.ItemSpec);
                }
            }
        }

        protected override bool CallHostObjectToExecute()
        {
            bool flag;
            IVbcHostObject hostObject = base.HostObject as IVbcHostObject;
            try
            {
                CodeMarkers.Instance.CodeMarker(CodeMarkerEvent.perfMSBuildHostCompileBegin);
                flag = hostObject.Compile();
            }
            finally
            {
                CodeMarkers.Instance.CodeMarker(CodeMarkerEvent.perfMSBuildHostCompileEnd);
            }
            return flag;
        }

        protected override string GenerateFullPathToTool()
        {
            string pathToDotNetFrameworkFile = ToolLocationHelper.GetPathToDotNetFrameworkFile(this.ToolName, TargetDotNetFrameworkVersion.Version40);
            if (pathToDotNetFrameworkFile == null)
            {
                base.Log.LogErrorWithCodeFromResources("General.FrameworksFileNotFound", new object[] { this.ToolName, ToolLocationHelper.GetDotNetFrameworkVersionFolderPrefix(TargetDotNetFrameworkVersion.Version40) });
            }
            return pathToDotNetFrameworkFile;
        }

        internal string GetBaseAddressInHex()
        {
            string baseAddress = this.BaseAddress;
            if (baseAddress != null)
            {
                if (baseAddress.Length > 2)
                {
                    string strA = baseAddress.Substring(0, 2);
                    if ((string.Compare(strA, "0x", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(strA, "&h", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        return baseAddress.Substring(2);
                    }
                }
                try
                {
                    return uint.Parse(baseAddress, CultureInfo.InvariantCulture).ToString("X", CultureInfo.InvariantCulture);
                }
                catch (FormatException exception)
                {
                    Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgument(false, exception, "Vbc.ParameterHasInvalidValue", "BaseAddress", baseAddress);
                }
            }
            return null;
        }

        internal static string GetDefineConstantsSwitch(string originalDefineConstants)
        {
            if ((originalDefineConstants == null) || (originalDefineConstants.Length == 0))
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(originalDefineConstants);
            builder.Replace("\\\"", "\\\\\"");
            builder.Replace("\"", "\\\"");
            builder.Insert(0, '"');
            builder.Append('"');
            return builder.ToString();
        }

        private bool InitializeHostCompiler(IVbcHostObject vbcHostObject)
        {
            base.HostCompilerSupportsAllParameters = this.UseHostCompilerIfAvailable;
            string parameterName = "Unknown";
            try
            {
                parameterName = "BeginInitialization";
                vbcHostObject.BeginInitialization();
                parameterName = "AdditionalLibPaths";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetAdditionalLibPaths(base.AdditionalLibPaths));
                parameterName = "AddModules";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetAddModules(base.AddModules));
                parameterName = "BaseAddress";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetBaseAddress(base.TargetType, this.GetBaseAddressInHex()));
                parameterName = "CodePage";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetCodePage(base.CodePage));
                parameterName = "DebugType";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetDebugType(base.EmitDebugInformation, base.DebugType));
                parameterName = "DefineConstants";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetDefineConstants(base.DefineConstants));
                parameterName = "DelaySign";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetDelaySign(base.DelaySign));
                parameterName = "DocumentationFile";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetDocumentationFile(this.DocumentationFile));
                parameterName = "FileAlignment";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetFileAlignment(base.FileAlignment));
                parameterName = "GenerateDocumentation";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetGenerateDocumentation(this.GenerateDocumentation));
                parameterName = "Imports";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetImports(this.Imports));
                parameterName = "KeyContainer";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetKeyContainer(base.KeyContainer));
                parameterName = "KeyFile";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetKeyFile(base.KeyFile));
                parameterName = "LinkResources";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetLinkResources(base.LinkResources));
                parameterName = "MainEntryPoint";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetMainEntryPoint(base.MainEntryPoint));
                parameterName = "NoConfig";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetNoConfig(base.NoConfig));
                parameterName = "NoStandardLib";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetNoStandardLib(this.NoStandardLib));
                parameterName = "NoWarnings";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetNoWarnings(this.NoWarnings));
                parameterName = "Optimize";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetOptimize(base.Optimize));
                parameterName = "OptionCompare";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetOptionCompare(this.OptionCompare));
                parameterName = "OptionExplicit";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetOptionExplicit(this.OptionExplicit));
                parameterName = "OptionStrict";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetOptionStrict(this.OptionStrict));
                parameterName = "OptionStrictType";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetOptionStrictType(this.OptionStrictType));
                parameterName = "OutputAssembly";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetOutputAssembly(base.OutputAssembly.ItemSpec));
                parameterName = "Platform";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetPlatform(this.Platform));
                parameterName = "References";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetReferences(base.References));
                parameterName = "RemoveIntegerChecks";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetRemoveIntegerChecks(this.RemoveIntegerChecks));
                parameterName = "Resources";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetResources(base.Resources));
                parameterName = "ResponseFiles";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetResponseFiles(base.ResponseFiles));
                parameterName = "RootNamespace";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetRootNamespace(this.RootNamespace));
                parameterName = "SdkPath";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetSdkPath(this.SdkPath));
                parameterName = "Sources";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetSources(base.Sources));
                parameterName = "TargetCompactFramework";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetTargetCompactFramework(this.TargetCompactFramework));
                parameterName = "TargetType";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetTargetType(base.TargetType));
                parameterName = "TreatWarningsAsErrors";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetTreatWarningsAsErrors(base.TreatWarningsAsErrors));
                parameterName = "WarningsAsErrors";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetWarningsAsErrors(this.WarningsAsErrors));
                parameterName = "WarningsNotAsErrors";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetWarningsNotAsErrors(this.WarningsNotAsErrors));
                parameterName = "DisabledWarnings";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetDisabledWarnings(this.DisabledWarnings));
                parameterName = "Win32Icon";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetWin32Icon(base.Win32Icon));
                parameterName = "Win32Resource";
                base.CheckHostObjectSupport(parameterName, vbcHostObject.SetWin32Resource(base.Win32Resource));
                if (vbcHostObject is IVbcHostObject2)
                {
                    IVbcHostObject2 obj2 = (IVbcHostObject2) vbcHostObject;
                    parameterName = "ModuleAssemblyName";
                    base.CheckHostObjectSupport(parameterName, obj2.SetModuleAssemblyName(this.ModuleAssemblyName));
                    parameterName = "OptionInfer";
                    base.CheckHostObjectSupport(parameterName, obj2.SetOptionInfer(this.OptionInfer));
                    parameterName = "Win32Manifest";
                    base.CheckHostObjectSupport(parameterName, obj2.SetWin32Manifest(base.GetWin32ManifestSwitch(base.NoWin32Manifest, base.Win32Manifest)));
                    base.CheckHostObjectSupport("OptionInfer", obj2.SetOptionInfer(this.OptionInfer));
                }
                else
                {
                    if (!string.IsNullOrEmpty(this.ModuleAssemblyName))
                    {
                        base.CheckHostObjectSupport("ModuleAssemblyName", false);
                    }
                    if (base.Bag.ContainsKey("OptionInfer"))
                    {
                        base.CheckHostObjectSupport("OptionInfer", false);
                    }
                    if (!string.IsNullOrEmpty(base.Win32Manifest))
                    {
                        base.CheckHostObjectSupport("Win32Manifest", false);
                    }
                }
                if (vbcHostObject is IVbcHostObject3)
                {
                    IVbcHostObject3 obj3 = (IVbcHostObject3) vbcHostObject;
                    parameterName = "LangVersion";
                    base.CheckHostObjectSupport(parameterName, obj3.SetLanguageVersion(this.LangVersion));
                }
                else if (!string.IsNullOrEmpty(this.LangVersion) && !base.UsedCommandLineTool)
                {
                    base.CheckHostObjectSupport("LangVersion", false);
                }
                if (vbcHostObject is IVbcHostObject4)
                {
                    IVbcHostObject4 obj4 = (IVbcHostObject4) vbcHostObject;
                    parameterName = "VBRuntime";
                    base.CheckHostObjectSupport(parameterName, obj4.SetVBRuntime(this.VBRuntime));
                }
                if (this.NoVBRuntimeReference)
                {
                    base.CheckHostObjectSupport("NoVBRuntimeReference", false);
                }
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception))
                {
                    throw;
                }
                if (base.HostCompilerSupportsAllParameters)
                {
                    base.Log.LogErrorWithCodeFromResources("General.CouldNotSetHostObjectParameter", new object[] { parameterName, exception.Message });
                }
                return false;
            }
            finally
            {
                vbcHostObject.EndInitialization();
            }
            return true;
        }

        protected override HostObjectInitializationStatus InitializeHostObject()
        {
            if (base.HostObject != null)
            {
                using (RCWForCurrentContext<IVbcHostObject> context = new RCWForCurrentContext<IVbcHostObject>(base.HostObject as IVbcHostObject))
                {
                    IVbcHostObject rCW = context.RCW;
                    if (rCW != null)
                    {
                        bool flag = this.InitializeHostCompiler(rCW);
                        if (rCW.IsDesignTime())
                        {
                            return (flag ? HostObjectInitializationStatus.NoActionReturnSuccess : HostObjectInitializationStatus.NoActionReturnFailure);
                        }
                        if (!base.HostCompilerSupportsAllParameters || this.UseAlternateCommandLineToolToExecute())
                        {
                            if (!base.CheckAllReferencesExistOnDisk())
                            {
                                return HostObjectInitializationStatus.NoActionReturnFailure;
                            }
                            base.UsedCommandLineTool = true;
                            return HostObjectInitializationStatus.UseAlternateToolToExecute;
                        }
                        if (flag)
                        {
                            return (rCW.IsUpToDate() ? HostObjectInitializationStatus.NoActionReturnSuccess : HostObjectInitializationStatus.UseHostObjectToExecute);
                        }
                        return HostObjectInitializationStatus.NoActionReturnFailure;
                    }
                    base.Log.LogErrorWithCodeFromResources("General.IncorrectHostObject", new object[] { "Vbc", "IVbcHostObject" });
                }
            }
            base.UsedCommandLineTool = true;
            return HostObjectInitializationStatus.UseAlternateToolToExecute;
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            if (!base.UsedCommandLineTool)
            {
                base.LogEventsFromTextOutput(singleLine, messageImportance);
            }
            else if (((this.vbErrorLines.Count == 0) && (singleLine.IndexOf("warning", StringComparison.OrdinalIgnoreCase) == -1)) && (singleLine.IndexOf("error", StringComparison.OrdinalIgnoreCase) == -1))
            {
                base.LogEventsFromTextOutput(singleLine, messageImportance);
            }
            else
            {
                this.ParseVBErrorOrWarning(singleLine, messageImportance);
            }
        }

        internal void ParseVBErrorOrWarning(string singleLine, MessageImportance messageImportance)
        {
            if (this.vbErrorLines.Count > 0)
            {
                if (!this.isDoneOutputtingErrorMessage && (singleLine.Length == 0))
                {
                    this.isDoneOutputtingErrorMessage = true;
                    this.numberOfLinesInErrorMessage = this.vbErrorLines.Count;
                }
                this.vbErrorLines.Enqueue(new VBError(singleLine, messageImportance));
                if (this.isDoneOutputtingErrorMessage && (this.vbErrorLines.Count == (this.numberOfLinesInErrorMessage + 3)))
                {
                    VBError error = this.vbErrorLines.Dequeue();
                    string message = error.Message;
                    int num = singleLine.IndexOf('~') + 1;
                    int index = message.IndexOf(')');
                    if ((num < 0) || (index < 0))
                    {
                        base.Log.LogMessageFromText(message, error.MessageImportance);
                        foreach (VBError error2 in this.vbErrorLines)
                        {
                            base.LogEventsFromTextOutput(error2.Message, error2.MessageImportance);
                        }
                        this.vbErrorLines.Clear();
                    }
                    else
                    {
                        string lineOfText = null;
                        lineOfText = string.Concat(new object[] { message.Substring(0, index), ",", num, message.Substring(index) });
                        base.Log.LogMessageFromText(lineOfText, error.MessageImportance);
                        foreach (VBError error3 in this.vbErrorLines)
                        {
                            base.LogEventsFromTextOutput(error3.Message, error3.MessageImportance);
                        }
                        this.vbErrorLines.Clear();
                    }
                }
            }
            else
            {
                Microsoft.Build.Utilities.CanonicalError.Parts parts = Microsoft.Build.Utilities.CanonicalError.Parse(singleLine);
                if (parts == null)
                {
                    base.LogEventsFromTextOutput(singleLine, messageImportance);
                }
                else if (((parts.category == Microsoft.Build.Utilities.CanonicalError.Parts.Category.Error) || (parts.category == Microsoft.Build.Utilities.CanonicalError.Parts.Category.Warning)) && (parts.column == 0))
                {
                    if (parts.line != 0)
                    {
                        this.vbErrorLines.Enqueue(new VBError(singleLine, messageImportance));
                        this.isDoneOutputtingErrorMessage = false;
                        this.numberOfLinesInErrorMessage = 0;
                    }
                    else
                    {
                        base.LogEventsFromTextOutput(singleLine, messageImportance);
                    }
                }
            }
        }

        protected override bool ValidateParameters()
        {
            if (!base.ValidateParameters())
            {
                return false;
            }
            if (((this.Verbosity != null) && (string.Compare(this.Verbosity, "normal", StringComparison.OrdinalIgnoreCase) != 0)) && ((string.Compare(this.Verbosity, "quiet", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(this.Verbosity, "verbose", StringComparison.OrdinalIgnoreCase) != 0)))
            {
                base.Log.LogErrorWithCodeFromResources("Vbc.EnumParameterHasInvalidValue", new object[] { "Verbosity", this.Verbosity, "Quiet, Normal, Verbose" });
                return false;
            }
            return true;
        }

        public string BaseAddress
        {
            get
            {
                return (string) base.Bag["BaseAddress"];
            }
            set
            {
                base.Bag["BaseAddress"] = value;
            }
        }

        public string DisabledWarnings
        {
            get
            {
                return (string) base.Bag["DisabledWarnings"];
            }
            set
            {
                base.Bag["DisabledWarnings"] = value;
            }
        }

        public string DocumentationFile
        {
            get
            {
                return (string) base.Bag["DocumentationFile"];
            }
            set
            {
                base.Bag["DocumentationFile"] = value;
            }
        }

        public string ErrorReport
        {
            get
            {
                return (string) base.Bag["ErrorReport"];
            }
            set
            {
                base.Bag["ErrorReport"] = value;
            }
        }

        public bool GenerateDocumentation
        {
            get
            {
                return base.GetBoolParameterWithDefault("GenerateDocumentation", false);
            }
            set
            {
                base.Bag["GenerateDocumentation"] = value;
            }
        }

        public ITaskItem[] Imports
        {
            get
            {
                return (ITaskItem[]) base.Bag["Imports"];
            }
            set
            {
                base.Bag["Imports"] = value;
            }
        }

        public string LangVersion
        {
            get
            {
                return (string) base.Bag["LangVersion"];
            }
            set
            {
                base.Bag["LangVersion"] = value;
            }
        }

        public string ModuleAssemblyName
        {
            get
            {
                return (string) base.Bag["ModuleAssemblyName"];
            }
            set
            {
                base.Bag["ModuleAssemblyName"] = value;
            }
        }

        public bool NoStandardLib
        {
            get
            {
                return base.GetBoolParameterWithDefault("NoStandardLib", false);
            }
            set
            {
                base.Bag["NoStandardLib"] = value;
            }
        }

        public bool NoVBRuntimeReference
        {
            get
            {
                return base.GetBoolParameterWithDefault("NoVBRuntimeReference", false);
            }
            set
            {
                base.Bag["NoVBRuntimeReference"] = value;
            }
        }

        public bool NoWarnings
        {
            get
            {
                return base.GetBoolParameterWithDefault("NoWarnings", false);
            }
            set
            {
                base.Bag["NoWarnings"] = value;
            }
        }

        public string OptionCompare
        {
            get
            {
                return (string) base.Bag["OptionCompare"];
            }
            set
            {
                base.Bag["OptionCompare"] = value;
            }
        }

        public bool OptionExplicit
        {
            get
            {
                return base.GetBoolParameterWithDefault("OptionExplicit", true);
            }
            set
            {
                base.Bag["OptionExplicit"] = value;
            }
        }

        public bool OptionInfer
        {
            get
            {
                return base.GetBoolParameterWithDefault("OptionInfer", false);
            }
            set
            {
                base.Bag["OptionInfer"] = value;
            }
        }

        public bool OptionStrict
        {
            get
            {
                return base.GetBoolParameterWithDefault("OptionStrict", false);
            }
            set
            {
                base.Bag["OptionStrict"] = value;
            }
        }

        public string OptionStrictType
        {
            get
            {
                return (string) base.Bag["OptionStrictType"];
            }
            set
            {
                base.Bag["OptionStrictType"] = value;
            }
        }

        public string Platform
        {
            get
            {
                return (string) base.Bag["Platform"];
            }
            set
            {
                base.Bag["Platform"] = value;
            }
        }

        public bool RemoveIntegerChecks
        {
            get
            {
                return base.GetBoolParameterWithDefault("RemoveIntegerChecks", false);
            }
            set
            {
                base.Bag["RemoveIntegerChecks"] = value;
            }
        }

        public string RootNamespace
        {
            get
            {
                return (string) base.Bag["RootNamespace"];
            }
            set
            {
                base.Bag["RootNamespace"] = value;
            }
        }

        public string SdkPath
        {
            get
            {
                return (string) base.Bag["SdkPath"];
            }
            set
            {
                base.Bag["SdkPath"] = value;
            }
        }

        public bool TargetCompactFramework
        {
            get
            {
                return base.GetBoolParameterWithDefault("TargetCompactFramework", false);
            }
            set
            {
                base.Bag["TargetCompactFramework"] = value;
            }
        }

        protected override string ToolName
        {
            get
            {
                return "Vbc.exe";
            }
        }

        public bool UseHostCompilerIfAvailable
        {
            get
            {
                return this.useHostCompilerIfAvailable;
            }
            set
            {
                this.useHostCompilerIfAvailable = value;
            }
        }

        public string VBRuntime
        {
            get
            {
                return (string) base.Bag["VBRuntime"];
            }
            set
            {
                base.Bag["VBRuntime"] = value;
            }
        }

        public string Verbosity
        {
            get
            {
                return (string) base.Bag["Verbosity"];
            }
            set
            {
                base.Bag["Verbosity"] = value;
            }
        }

        public string WarningsAsErrors
        {
            get
            {
                return (string) base.Bag["WarningsAsErrors"];
            }
            set
            {
                base.Bag["WarningsAsErrors"] = value;
            }
        }

        public string WarningsNotAsErrors
        {
            get
            {
                return (string) base.Bag["WarningsNotAsErrors"];
            }
            set
            {
                base.Bag["WarningsNotAsErrors"] = value;
            }
        }

        private class VBError
        {
            public VBError(string message, Microsoft.Build.Framework.MessageImportance importance)
            {
                this.Message = message;
                this.MessageImportance = importance;
            }

            public string Message { get; set; }

            public Microsoft.Build.Framework.MessageImportance MessageImportance { get; set; }
        }
    }
}

