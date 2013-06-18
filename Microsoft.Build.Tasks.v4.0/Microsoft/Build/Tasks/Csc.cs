namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Shared.LanguageParser;
    using Microsoft.Build.Tasks.Hosting;
    using Microsoft.Build.Tasks.InteropUtilities;
    using Microsoft.Build.Utilities;
    using Microsoft.Internal.Performance;
    using System;
    using System.Globalization;
    using System.Text;

    public class Csc : ManagedCompiler
    {
        private bool useHostCompilerIfAvailable;

        private void AddReferencesToCommandLine(CommandLineBuilderExtension commandLine)
        {
            if ((base.References != null) && (base.References.Length != 0))
            {
                foreach (ITaskItem item in base.References)
                {
                    string metadata = item.GetMetadata("Aliases");
                    string switchName = "/reference:";
                    if (MetadataConversionUtilities.TryConvertItemMetadataToBool(item, "EmbedInteropTypes"))
                    {
                        switchName = "/link:";
                    }
                    if ((metadata == null) || (metadata.Length == 0))
                    {
                        commandLine.AppendSwitchIfNotNull(switchName, item.ItemSpec);
                    }
                    else
                    {
                        foreach (string str3 in metadata.Split(new char[] { ',' }))
                        {
                            string str4 = str3.Trim();
                            if (str3.Length != 0)
                            {
                                if (str4.IndexOfAny(new char[] { ',', ' ', ';', '"' }) != -1)
                                {
                                    Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgument(false, "Csc.AssemblyAliasContainsIllegalCharacters", item.ItemSpec, str4);
                                }
                                if (string.Compare("global", str4, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    commandLine.AppendSwitchIfNotNull(switchName, item.ItemSpec);
                                }
                                else
                                {
                                    commandLine.AppendSwitchAliased(switchName, str4, item.ItemSpec);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected internal override void AddResponseFileCommands(CommandLineBuilderExtension commandLine)
        {
            commandLine.AppendSwitchIfNotNull("/lib:", base.AdditionalLibPaths, ",");
            commandLine.AppendPlusOrMinusSwitch("/unsafe", base.Bag, "AllowUnsafeBlocks");
            commandLine.AppendPlusOrMinusSwitch("/checked", base.Bag, "CheckForOverflowUnderflow");
            commandLine.AppendSwitchWithSplitting("/nowarn:", this.DisabledWarnings, ",", new char[] { ';', ',' });
            commandLine.AppendWhenTrue("/fullpaths", base.Bag, "GenerateFullPaths");
            commandLine.AppendSwitchIfNotNull("/langversion:", this.LangVersion);
            commandLine.AppendSwitchIfNotNull("/moduleassemblyname:", this.ModuleAssemblyName);
            commandLine.AppendSwitchIfNotNull("/pdb:", this.PdbFile);
            commandLine.AppendPlusOrMinusSwitch("/nostdlib", base.Bag, "NoStandardLib");
            commandLine.AppendSwitchIfNotNull("/platform:", this.Platform);
            commandLine.AppendSwitchIfNotNull("/errorreport:", this.ErrorReport);
            commandLine.AppendSwitchWithInteger("/warn:", base.Bag, "WarningLevel");
            commandLine.AppendSwitchIfNotNull("/doc:", this.DocumentationFile);
            commandLine.AppendSwitchIfNotNull("/baseaddress:", this.BaseAddress);
            commandLine.AppendSwitchUnquotedIfNotNull("/define:", this.GetDefineConstantsSwitch(base.DefineConstants));
            commandLine.AppendSwitchIfNotNull("/win32res:", base.Win32Resource);
            commandLine.AppendSwitchIfNotNull("/main:", base.MainEntryPoint);
            commandLine.AppendSwitchIfNotNull("/appconfig:", this.ApplicationConfiguration);
            this.AddReferencesToCommandLine(commandLine);
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
            ICscHostObject hostObject = base.HostObject as ICscHostObject;
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

        internal string GetDefineConstantsSwitch(string originalDefineConstants)
        {
            if (originalDefineConstants != null)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string str in originalDefineConstants.Split(new char[] { ',', ';', ' ' }))
                {
                    if (IsLegalIdentifier(str))
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(";");
                        }
                        builder.Append(str);
                    }
                    else if (str.Length > 0)
                    {
                        base.Log.LogWarningWithCodeFromResources("Csc.InvalidParameterWarning", new object[] { "/define:", str });
                    }
                }
                if (builder.Length > 0)
                {
                    return builder.ToString();
                }
            }
            return null;
        }

        private bool InitializeHostCompiler(ICscHostObject cscHostObject)
        {
            bool flag;
            base.HostCompilerSupportsAllParameters = this.UseHostCompilerIfAvailable;
            string parameterName = "Unknown";
            try
            {
                parameterName = "LinkResources";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetLinkResources(base.LinkResources));
                parameterName = "References";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetReferences(base.References));
                parameterName = "Resources";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetResources(base.Resources));
                parameterName = "Sources";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetSources(base.Sources));
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
            try
            {
                parameterName = "BeginInitialization";
                cscHostObject.BeginInitialization();
                parameterName = "AdditionalLibPaths";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetAdditionalLibPaths(base.AdditionalLibPaths));
                parameterName = "AddModules";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetAddModules(base.AddModules));
                parameterName = "AllowUnsafeBlocks";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetAllowUnsafeBlocks(this.AllowUnsafeBlocks));
                parameterName = "BaseAddress";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetBaseAddress(this.BaseAddress));
                parameterName = "CheckForOverflowUnderflow";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetCheckForOverflowUnderflow(this.CheckForOverflowUnderflow));
                parameterName = "CodePage";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetCodePage(base.CodePage));
                parameterName = "EmitDebugInformation";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetEmitDebugInformation(base.EmitDebugInformation));
                parameterName = "DebugType";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetDebugType(base.DebugType));
                parameterName = "DefineConstants";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetDefineConstants(this.GetDefineConstantsSwitch(base.DefineConstants)));
                parameterName = "DelaySign";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetDelaySign(base.Bag["DelaySign"] != null, base.DelaySign));
                parameterName = "DisabledWarnings";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetDisabledWarnings(this.DisabledWarnings));
                parameterName = "DocumentationFile";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetDocumentationFile(this.DocumentationFile));
                parameterName = "ErrorReport";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetErrorReport(this.ErrorReport));
                parameterName = "FileAlignment";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetFileAlignment(base.FileAlignment));
                parameterName = "GenerateFullPaths";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetGenerateFullPaths(this.GenerateFullPaths));
                parameterName = "KeyContainer";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetKeyContainer(base.KeyContainer));
                parameterName = "KeyFile";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetKeyFile(base.KeyFile));
                parameterName = "LangVersion";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetLangVersion(this.LangVersion));
                parameterName = "MainEntryPoint";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetMainEntryPoint(base.TargetType, base.MainEntryPoint));
                parameterName = "ModuleAssemblyName";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetModuleAssemblyName(this.ModuleAssemblyName));
                parameterName = "NoConfig";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetNoConfig(base.NoConfig));
                parameterName = "NoStandardLib";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetNoStandardLib(this.NoStandardLib));
                parameterName = "Optimize";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetOptimize(base.Optimize));
                parameterName = "OutputAssembly";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetOutputAssembly(base.OutputAssembly.ItemSpec));
                parameterName = "PdbFile";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetPdbFile(this.PdbFile));
                parameterName = "Platform";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetPlatform(this.Platform));
                parameterName = "ResponseFiles";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetResponseFiles(base.ResponseFiles));
                parameterName = "TargetType";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetTargetType(base.TargetType));
                parameterName = "TreatWarningsAsErrors";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetTreatWarningsAsErrors(base.TreatWarningsAsErrors));
                parameterName = "WarningLevel";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetWarningLevel(this.WarningLevel));
                parameterName = "WarningsAsErrors";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetWarningsAsErrors(this.WarningsAsErrors));
                parameterName = "WarningsNotAsErrors";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetWarningsNotAsErrors(this.WarningsNotAsErrors));
                parameterName = "Win32Icon";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetWin32Icon(base.Win32Icon));
                if (cscHostObject is ICscHostObject2)
                {
                    ICscHostObject2 obj2 = (ICscHostObject2) cscHostObject;
                    parameterName = "Win32Manifest";
                    base.CheckHostObjectSupport(parameterName, obj2.SetWin32Manifest(base.GetWin32ManifestSwitch(base.NoWin32Manifest, base.Win32Manifest)));
                }
                else if (!string.IsNullOrEmpty(base.Win32Manifest))
                {
                    base.CheckHostObjectSupport("Win32Manifest", false);
                }
                parameterName = "Win32Resource";
                base.CheckHostObjectSupport(parameterName, cscHostObject.SetWin32Resource(base.Win32Resource));
                if (cscHostObject is ICscHostObject3)
                {
                    ICscHostObject3 obj3 = (ICscHostObject3) cscHostObject;
                    parameterName = "ApplicationConfiguration";
                    base.CheckHostObjectSupport(parameterName, obj3.SetApplicationConfiguration(this.ApplicationConfiguration));
                    return flag;
                }
                if (!string.IsNullOrEmpty(this.ApplicationConfiguration))
                {
                    base.CheckHostObjectSupport("ApplicationConfiguration", false);
                }
                return flag;
            }
            catch (Exception exception2)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception2))
                {
                    throw;
                }
                if (base.HostCompilerSupportsAllParameters)
                {
                    base.Log.LogErrorWithCodeFromResources("General.CouldNotSetHostObjectParameter", new object[] { parameterName, exception2.Message });
                }
                return false;
            }
            finally
            {
                int num;
                string str2;
                flag = cscHostObject.EndInitialization(out str2, out num);
                if (base.HostCompilerSupportsAllParameters)
                {
                    if (!flag)
                    {
                        base.Log.LogError(null, "CS" + num.ToString("D4", CultureInfo.InvariantCulture), null, null, 0, 0, 0, 0, str2, new object[0]);
                    }
                    else if ((str2 != null) && (str2.Length > 0))
                    {
                        base.Log.LogWarning(null, "CS" + num.ToString("D4", CultureInfo.InvariantCulture), null, null, 0, 0, 0, 0, str2, new object[0]);
                    }
                }
            }
            return flag;
        }

        protected override HostObjectInitializationStatus InitializeHostObject()
        {
            if (base.HostObject != null)
            {
                using (RCWForCurrentContext<ICscHostObject> context = new RCWForCurrentContext<ICscHostObject>(base.HostObject as ICscHostObject))
                {
                    ICscHostObject rCW = context.RCW;
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
                    base.Log.LogErrorWithCodeFromResources("General.IncorrectHostObject", new object[] { "Csc", "ICscHostObject" });
                }
            }
            base.UsedCommandLineTool = true;
            return HostObjectInitializationStatus.UseAlternateToolToExecute;
        }

        private static bool IsLegalIdentifier(string identifier)
        {
            if (identifier.Length == 0)
            {
                return false;
            }
            if (!TokenChar.IsLetter(identifier[0]) && (identifier[0] != '_'))
            {
                return false;
            }
            for (int i = 1; i < identifier.Length; i++)
            {
                char c = identifier[i];
                if (((!TokenChar.IsLetter(c) && !TokenChar.IsDecimalDigit(c)) && (!TokenChar.IsConnecting(c) && !TokenChar.IsCombining(c))) && !TokenChar.IsFormatting(c))
                {
                    return false;
                }
            }
            return true;
        }

        public bool AllowUnsafeBlocks
        {
            get
            {
                return base.GetBoolParameterWithDefault("AllowUnsafeBlocks", false);
            }
            set
            {
                base.Bag["AllowUnsafeBlocks"] = value;
            }
        }

        public string ApplicationConfiguration
        {
            get
            {
                return (string) base.Bag["ApplicationConfiguration"];
            }
            set
            {
                base.Bag["ApplicationConfiguration"] = value;
            }
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

        public bool CheckForOverflowUnderflow
        {
            get
            {
                return base.GetBoolParameterWithDefault("CheckForOverflowUnderflow", false);
            }
            set
            {
                base.Bag["CheckForOverflowUnderflow"] = value;
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

        public bool GenerateFullPaths
        {
            get
            {
                return base.GetBoolParameterWithDefault("GenerateFullPaths", false);
            }
            set
            {
                base.Bag["GenerateFullPaths"] = value;
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

        public string PdbFile
        {
            get
            {
                return (string) base.Bag["PdbFile"];
            }
            set
            {
                base.Bag["PdbFile"] = value;
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

        protected override string ToolName
        {
            get
            {
                return "Csc.exe";
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

        public int WarningLevel
        {
            get
            {
                return base.GetIntParameterWithDefault("WarningLevel", 4);
            }
            set
            {
                base.Bag["WarningLevel"] = value;
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
    }
}

