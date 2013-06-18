namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using Microsoft.Internal.Performance;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Text;
    using System.Xml;

    [RequiredRuntime("v2.0")]
    public sealed class GenerateResource : TaskExtension
    {
        private ITaskItem[] additionalInputs;
        private ITaskItem[] excludedInputPaths;
        private bool executeAsTool = true;
        private ArrayList filesWritten = new ArrayList();
        private string fileTrackerPath;
        private static string generateResourceTlogFilenamePrefix = "GenerateResource";
        private static int MaximumCommandLength = 0x7d00;
        private bool minimalRebuildFromTracking = true;
        private bool neverLockTypeAssemblies;
        private static string outputFileMetadataName = "OutputResource";
        private ITaskItem[] outputResources;
        private bool publicClass;
        private ITaskItem[] references;
        private List<ITaskItem> remotedTaskItems;
        private string resgenPath;
        private static string resGenTlogFilenamePrefix = "ResGen";
        private ExecutableType resGenType = ExecutableType.ManagedIL;
        private string sdkToolsPath;
        private ITaskItem[] sources;
        private static readonly char[] SpecialChars = new char[] { ' ', '\r', '\n' };
        private ITaskItem stateFile;
        private string stronglyTypedClassName;
        private string stronglyTypedFileName;
        private string stronglyTypedLanguage;
        private string stronglyTypedManifestPrefix;
        private string stronglyTypedNamespace;
        private bool stronglyTypedResourceSuccessfullyCreated;
        private string toolArchitecture;
        private string trackerLogDirectory;
        private string trackerPath;
        private bool trackFileAccess = true;
        private bool trackingInProc;
        private Dictionary<string, Type> typeTable = new Dictionary<string, Type>(StringComparer.Ordinal);
        private ArrayList unsuccessfullyCreatedOutFiles = new ArrayList();
        private bool useSourcePath;

        private bool AnyAdditionalInputOutOfDate(ITaskItem[] readTLogs, ITaskItem[] writeTLogs)
        {
            DateTime minValue = DateTime.MinValue;
            if (readTLogs != null)
            {
                foreach (ITaskItem item in readTLogs)
                {
                    DateTime lastWriteFileUtcTime = Microsoft.Build.Shared.NativeMethodsShared.GetLastWriteFileUtcTime(item.ItemSpec);
                    if (lastWriteFileUtcTime > minValue)
                    {
                        minValue = lastWriteFileUtcTime;
                    }
                }
            }
            if (writeTLogs != null)
            {
                foreach (ITaskItem item2 in writeTLogs)
                {
                    DateTime time3 = Microsoft.Build.Shared.NativeMethodsShared.GetLastWriteFileUtcTime(item2.ItemSpec);
                    if (time3 > minValue)
                    {
                        minValue = time3;
                    }
                }
            }
            foreach (ITaskItem item3 in this.AdditionalInputs)
            {
                if (Microsoft.Build.Shared.NativeMethodsShared.GetLastWriteFileUtcTime(item3.ItemSpec) > minValue)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "GenerateResource.AdditionalInputNewerThanTLog", new object[] { item3.ItemSpec });
                    return true;
                }
            }
            return false;
        }

        private static byte[] ByteArrayFromBase64WrappedString(string text)
        {
            if (text.IndexOfAny(SpecialChars) == -1)
            {
                return Convert.FromBase64String(text);
            }
            StringBuilder builder = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (((ch != '\n') && (ch != '\r')) && (ch != ' '))
                {
                    builder.Append(text[i]);
                }
            }
            return Convert.FromBase64String(builder.ToString());
        }

        private int CalculateResourceBatchSize(List<ITaskItem> inputsToProcess, List<ITaskItem> outputsToProcess, string resourcelessCommand, int initialResourceIndex)
        {
            CommandLineBuilderExtension extension = new CommandLineBuilderExtension();
            if (!string.IsNullOrEmpty(resourcelessCommand))
            {
                extension.AppendTextUnquoted(resourcelessCommand);
            }
            int num = initialResourceIndex;
            while ((extension.Length < MaximumCommandLength) && (num < inputsToProcess.Count))
            {
                extension.AppendFileNamesIfNotNull(new ITaskItem[] { inputsToProcess[num], outputsToProcess[num] }, ",");
                num++;
            }
            if (num == inputsToProcess.Count)
            {
                return (num - initialResourceIndex);
            }
            return ((num - initialResourceIndex) - 1);
        }

        private void CompactTrackingLogs(bool taskSucceeded)
        {
            CanonicalTrackedOutputFiles outputs = new CanonicalTrackedOutputFiles(this.TLogWriteFiles);
            CanonicalTrackedInputFiles files2 = new CanonicalTrackedInputFiles(this.TLogReadFiles, this.Sources, this.ExcludedInputPaths, outputs, true, false);
            outputs.RemoveDependenciesFromEntryIfMissing(this.Sources);
            files2.RemoveDependenciesFromEntryIfMissing(this.Sources);
            if (!taskSucceeded)
            {
                foreach (ITaskItem item in this.Sources)
                {
                    string sourceKey = FileTracker.FormatRootingMarker(item);
                    outputs.AddComputedOutputForSourceRoot(sourceKey, item.GetMetadata(outputFileMetadataName));
                }
                ITaskItem[] source = files2.ComputeSourcesNeedingCompilation();
                foreach (ITaskItem item2 in this.Sources)
                {
                    string sourceRoot = FileTracker.FormatRootingMarker(item2);
                    outputs.RemoveOutputForSourceRoot(sourceRoot, item2.GetMetadata(outputFileMetadataName));
                }
                outputs.RemoveEntriesForSource(source);
                files2.RemoveEntriesForSource(source);
            }
            if (!string.IsNullOrEmpty(this.StronglyTypedFileName) && this.stronglyTypedResourceSuccessfullyCreated)
            {
                string stronglyTypedFileName = this.StronglyTypedFileName;
                foreach (ITaskItem item3 in this.Sources)
                {
                    string str4 = FileTracker.FormatRootingMarker(item3);
                    outputs.AddComputedOutputForSourceRoot(str4, stronglyTypedFileName);
                }
            }
            files2.SaveTlog();
            outputs.SaveTlog();
        }

        private bool ComputePathToResGen()
        {
            this.resgenPath = null;
            if (string.IsNullOrEmpty(this.sdkToolsPath))
            {
                this.resgenPath = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("resgen.exe", TargetDotNetFrameworkVersion.Version35);
                this.resGenType = ExecutableType.ManagedIL;
                if ((this.resgenPath == null) && this.ExecuteAsTool)
                {
                    base.Log.LogErrorWithCodeFromResources("General.PlatformSDKFileNotFound", new object[] { "resgen.exe", ToolLocationHelper.GetDotNetFrameworkSdkInstallKeyValue(TargetDotNetFrameworkVersion.Version35), ToolLocationHelper.GetDotNetFrameworkSdkRootRegistryKey(TargetDotNetFrameworkVersion.Version35) });
                }
            }
            else
            {
                this.resgenPath = SdkToolsPathUtility.GeneratePathToTool(SdkToolsPathUtility.FileInfoExists, ProcessorArchitecture.CurrentProcessArchitecture, this.SdkToolsPath, "Resgen.exe", base.Log, this.ExecuteAsTool);
                if (this.ExecuteAsTool && (this.resgenPath != null))
                {
                    this.resgenPath = Microsoft.Build.Shared.NativeMethodsShared.GetLongFilePath(this.resgenPath);
                    if (string.IsNullOrEmpty(this.ToolArchitecture))
                    {
                        if (this.resgenPath.Equals(Microsoft.Build.Shared.NativeMethodsShared.GetLongFilePath(ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("resgen.exe", TargetDotNetFrameworkVersion.Version40)), StringComparison.OrdinalIgnoreCase))
                        {
                            this.resGenType = ExecutableType.Managed32Bit;
                        }
                        else if ((this.resgenPath.Equals(Microsoft.Build.Shared.NativeMethodsShared.GetLongFilePath(ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("resgen.exe", TargetDotNetFrameworkVersion.Version35)), StringComparison.OrdinalIgnoreCase) || this.resgenPath.Equals(Microsoft.Build.Shared.NativeMethodsShared.GetLongFilePath(ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("resgen.exe", TargetDotNetFrameworkVersion.Version20)), StringComparison.OrdinalIgnoreCase)) || this.resgenPath.Equals(Microsoft.Build.Shared.NativeMethodsShared.GetLongFilePath(ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("resgen.exe", TargetDotNetFrameworkVersion.Version11)), StringComparison.OrdinalIgnoreCase))
                        {
                            this.resGenType = ExecutableType.ManagedIL;
                        }
                        else
                        {
                            this.resGenType = ExecutableType.Managed32Bit;
                        }
                    }
                    else if (!Enum.TryParse<ExecutableType>(this.ToolArchitecture, out this.resGenType))
                    {
                        base.Log.LogErrorWithCodeFromResources("General.InvalidValue", new object[] { "ToolArchitecture", "GenerateResource" });
                        return false;
                    }
                }
            }
            if ((this.resgenPath == null) && !this.ExecuteAsTool)
            {
                this.resgenPath = string.Empty;
                return true;
            }
            if (this.resgenPath != null)
            {
                this.resgenPath = Path.GetDirectoryName(this.resgenPath);
            }
            return (this.resgenPath != null);
        }

        private bool CreateOutputResourcesNames()
        {
            if (this.OutputResources == null)
            {
                this.OutputResources = new ITaskItem[this.Sources.Length];
                int index = 0;
                try
                {
                    index = 0;
                    while (index < this.Sources.Length)
                    {
                        this.OutputResources[index] = new TaskItem(Path.ChangeExtension(this.Sources[index].ItemSpec, ".resources"));
                        index++;
                    }
                }
                catch (ArgumentException exception)
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateResource.InvalidFilename", new object[] { this.Sources[index].ItemSpec, exception.Message });
                    return false;
                }
            }
            return true;
        }

        private ResGen CreateResGenTaskWithDefaultParameters()
        {
            return new ResGen { BuildEngine = base.BuildEngine, SdkToolsPath = this.resgenPath, PublicClass = this.PublicClass, References = this.References, UseSourcePath = this.UseSourcePath, TrackerLogDirectory = this.TrackerLogDirectory, TrackFileAccess = this.TrackFileAccess && !this.trackingInProc, ToolType = this.resGenType, TrackerFrameworkPath = this.TrackerFrameworkPath, TrackerSdkPath = this.TrackerSdkPath, EnvironmentVariables = this.EnvironmentVariables };
        }

        private bool DetermineWhetherSerializedObjectLoads(string data)
        {
            byte[] buffer = ByteArrayFromBase64WrappedString(data);
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                return (formatter.Deserialize(stream) != null);
            }
        }

        public override bool Execute()
        {
            bool flag = true;
            using (new CodeMarkerStartEnd(CodeMarkerEvent.perfMSBuildGenerateResourceBegin, CodeMarkerEvent.perfMSBuildGenerateResourceEnd))
            {
                List<ITaskItem> list;
                List<ITaskItem> list2;
                if ((this.Sources == null) || (this.Sources.Length == 0))
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "GenerateResource.NoSources", new object[0]);
                    this.OutputResources = null;
                    return true;
                }
                if (!this.ValidateParameters())
                {
                    this.OutputResources = null;
                    return false;
                }
                if (!this.CreateOutputResourcesNames())
                {
                    this.OutputResources = null;
                    return false;
                }
                string rootMarkerResponseFile = null;
                string str2 = null;
                this.trackingInProc = false;
                this.GetResourcesToProcess(out list, out list2);
                if (!base.Log.HasLoggedErrors && (list.Count != 0))
                {
                    if (!this.ComputePathToResGen())
                    {
                        return false;
                    }
                    bool flag2 = false;
                    try
                    {
                        if (this.ExecuteAsTool)
                        {
                            if (this.TrackFileAccess && !FileTracker.ForceOutOfProcTracking(this.resGenType))
                            {
                                try
                                {
                                    str2 = FileTracker.EnsureFileTrackerOnPath(this.TrackerFrameworkPath);
                                }
                                catch (Exception exception)
                                {
                                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                                    {
                                        throw;
                                    }
                                    flag2 = true;
                                    base.Log.LogErrorWithCodeFromResources("General.InvalidValue", new object[] { "TrackerFrameworkPath", "GenerateResource" });
                                }
                                if (!flag2)
                                {
                                    rootMarkerResponseFile = FileTracker.CreateRootingMarkerResponseFile(this.Sources);
                                    FileTracker.StartTrackingContextWithRoot(this.TrackerLogDirectory, generateResourceTlogFilenamePrefix, rootMarkerResponseFile);
                                    this.trackingInProc = true;
                                }
                            }
                            if (!flag2)
                            {
                                flag = this.GenerateResourcesUsingResGen(list, list2);
                            }
                        }
                        else
                        {
                            if (this.TrackFileAccess)
                            {
                                try
                                {
                                    str2 = FileTracker.EnsureFileTrackerOnPath(this.TrackerFrameworkPath);
                                }
                                catch (Exception exception2)
                                {
                                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception2))
                                    {
                                        throw;
                                    }
                                    flag2 = true;
                                    base.Log.LogErrorWithCodeFromResources("General.InvalidValue", new object[] { "TrackerFrameworkPath", "GenerateResource" });
                                }
                                if (!flag2)
                                {
                                    rootMarkerResponseFile = FileTracker.CreateRootingMarkerResponseFile(this.Sources);
                                    FileTracker.StartTrackingContextWithRoot(this.TrackerLogDirectory, generateResourceTlogFilenamePrefix, rootMarkerResponseFile);
                                    this.trackingInProc = true;
                                }
                            }
                            if (!flag2)
                            {
                                this.LogResgenCommandLine(list, list2);
                                bool flag3 = this.NeedSeparateAppDomain();
                                AppDomain domain = null;
                                ProcessResourceFiles files = null;
                                try
                                {
                                    if (flag3)
                                    {
                                        this.remotedTaskItems = new List<ITaskItem>();
                                        domain = AppDomain.CreateDomain("generateResourceAppDomain", null, AppDomain.CurrentDomain.SetupInformation);
                                        object obj2 = domain.CreateInstanceFromAndUnwrap(typeof(ProcessResourceFiles).Module.FullyQualifiedName, typeof(ProcessResourceFiles).FullName);
                                        Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(obj2.GetType() == typeof(ProcessResourceFiles), "Somehow got a wrong and possibly incompatible type for ProcessResourceFiles.");
                                        files = (ProcessResourceFiles) obj2;
                                        this.RecordItemsForDisconnectIfNecessary(this.references);
                                        this.RecordItemsForDisconnectIfNecessary(list);
                                        this.RecordItemsForDisconnectIfNecessary(list2);
                                    }
                                    else
                                    {
                                        files = new ProcessResourceFiles();
                                    }
                                    files.Run(base.Log, this.references, list, list2, this.UseSourcePath, this.StronglyTypedLanguage, this.stronglyTypedNamespace, this.stronglyTypedManifestPrefix, this.StronglyTypedFileName, this.StronglyTypedClassName, this.PublicClass);
                                    this.StronglyTypedClassName = files.StronglyTypedClassName;
                                    this.StronglyTypedFileName = files.StronglyTypedFilename;
                                    this.stronglyTypedResourceSuccessfullyCreated = files.StronglyTypedResourceSuccessfullyCreated;
                                    if (files.UnsuccessfullyCreatedOutFiles != null)
                                    {
                                        foreach (string str3 in files.UnsuccessfullyCreatedOutFiles)
                                        {
                                            this.unsuccessfullyCreatedOutFiles.Add(str3);
                                        }
                                    }
                                    files = null;
                                }
                                finally
                                {
                                    if (flag3 && (domain != null))
                                    {
                                        AppDomain.Unload(domain);
                                        files = null;
                                        domain = null;
                                        if (this.remotedTaskItems != null)
                                        {
                                            foreach (ITaskItem item in this.remotedTaskItems)
                                            {
                                                if (item is MarshalByRefObject)
                                                {
                                                    RemotingServices.Disconnect((MarshalByRefObject) item);
                                                }
                                            }
                                        }
                                        this.remotedTaskItems = null;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (this.TrackFileAccess)
                        {
                            try
                            {
                                if (this.trackingInProc && !flag2)
                                {
                                    FileTracker.WriteContextTLogs(this.TrackerLogDirectory, generateResourceTlogFilenamePrefix);
                                    FileTracker.EndTrackingContext();
                                }
                                this.CompactTrackingLogs(!base.Log.HasLoggedErrors && flag);
                            }
                            catch (Exception exception3)
                            {
                                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception3) && !(exception3 is UnauthorizedAccessException))
                                {
                                    throw;
                                }
                                base.Log.LogErrorFromException(exception3);
                            }
                            finally
                            {
                                if (str2 != null)
                                {
                                    Environment.SetEnvironmentVariable("PATH", str2);
                                }
                            }
                            if ((rootMarkerResponseFile != null) && File.Exists(rootMarkerResponseFile))
                            {
                                File.Delete(rootMarkerResponseFile);
                            }
                        }
                    }
                }
                this.RemoveUnsuccessfullyCreatedResourcesFromOutputResources();
                this.RecordFilesWritten();
            }
            return (!base.Log.HasLoggedErrors && flag);
        }

        private void GenerateResGenCommandLineWithoutResources(CommandLineBuilderExtension resGenCommand)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowInternalNull(resGenCommand, "resGenCommand");
            if (this.UseSourcePath)
            {
                resGenCommand.AppendSwitch("/useSourcePath");
            }
            if (this.PublicClass)
            {
                resGenCommand.AppendSwitch("/publicClass");
            }
            if (this.References != null)
            {
                foreach (ITaskItem item in this.References)
                {
                    resGenCommand.AppendSwitchIfNotNull("/r:", item);
                }
            }
        }

        private bool GenerateResourcesUsingResGen(List<ITaskItem> inputsToProcess, List<ITaskItem> outputsToProcess)
        {
            if (this.StronglyTypedLanguage != null)
            {
                return this.GenerateStronglyTypedResourceUsingResGen(inputsToProcess, outputsToProcess);
            }
            return this.TransformResourceFilesUsingResGen(inputsToProcess, outputsToProcess);
        }

        private bool GenerateStronglyTypedResourceUsingResGen(List<ITaskItem> inputsToProcess, List<ITaskItem> outputsToProcess)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow((inputsToProcess.Count == 1) && (outputsToProcess.Count == 1), "For STR, there should only be one input and one output.");
            ResGen gen = this.CreateResGenTaskWithDefaultParameters();
            gen.InputFiles = inputsToProcess.ToArray();
            gen.OutputFiles = outputsToProcess.ToArray();
            gen.StronglyTypedLanguage = this.StronglyTypedLanguage;
            gen.StronglyTypedNamespace = this.StronglyTypedNamespace;
            gen.StronglyTypedClassName = this.StronglyTypedClassName;
            gen.StronglyTypedFileName = this.StronglyTypedFileName;
            ITaskItem item = gen.OutputFiles[0];
            this.stronglyTypedResourceSuccessfullyCreated = gen.Execute();
            if (!this.stronglyTypedResourceSuccessfullyCreated && ((gen.OutputFiles == null) || (gen.OutputFiles.Length == 0)))
            {
                this.unsuccessfullyCreatedOutFiles.Add(item.ItemSpec);
            }
            this.StronglyTypedClassName = gen.StronglyTypedClassName;
            this.StronglyTypedFileName = gen.StronglyTypedFileName;
            return this.stronglyTypedResourceSuccessfullyCreated;
        }

        private string GenerateTrackerLogDirectory()
        {
            if (this.StateFile == null)
            {
                return null;
            }
            try
            {
                return Path.GetDirectoryName(this.StateFile.ItemSpec);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                {
                    throw;
                }
                return null;
            }
        }

        private void GetResourcesToProcess(out List<ITaskItem> inputsToProcess, out List<ITaskItem> outputsToProcess)
        {
            CanonicalTrackedInputFiles files2;
            inputsToProcess = new List<ITaskItem>();
            outputsToProcess = new List<ITaskItem>();
            for (int i = 0; i < this.Sources.Length; i++)
            {
                this.Sources[i].CopyMetadataTo(this.OutputResources[i]);
                this.Sources[i].SetMetadata("OutputResource", this.OutputResources[i].ItemSpec);
            }
            if (!this.MinimalRebuildFromTracking)
            {
                for (int j = 0; j < this.Sources.Length; j++)
                {
                    inputsToProcess.Add(this.Sources[j]);
                    outputsToProcess.Add(this.OutputResources[j]);
                }
                return;
            }
            ITaskItem[] readTLogs = TrackedDependencies.ExpandWildcards(this.TLogReadFiles);
            ITaskItem[] writeTLogs = TrackedDependencies.ExpandWildcards(this.TLogWriteFiles);
            if (((this.AdditionalInputs != null) && (this.AdditionalInputs.Length > 0)) && this.AnyAdditionalInputOutOfDate(readTLogs, writeTLogs))
            {
                for (int k = 0; k < this.Sources.Length; k++)
                {
                    inputsToProcess.Add(this.Sources[k]);
                    outputsToProcess.Add(this.OutputResources[k]);
                }
                return;
            }
            CanonicalTrackedOutputFiles outputs = new CanonicalTrackedOutputFiles(this, writeTLogs, false);
            foreach (ITaskItem item in this.Sources)
            {
                string sourceKey = FileTracker.FormatRootingMarker(item);
                outputs.AddComputedOutputForSourceRoot(sourceKey, item.GetMetadata(outputFileMetadataName));
            }
            if (this.StronglyTypedLanguage != null)
            {
                try
                {
                    if (this.StronglyTypedFileName == null)
                    {
                        CodeDomProvider provider = null;
                        if (ProcessResourceFiles.TryCreateCodeDomProvider(base.Log, this.StronglyTypedLanguage, out provider))
                        {
                            this.StronglyTypedFileName = ProcessResourceFiles.GenerateDefaultStronglyTypedFilename(provider, this.OutputResources[0].ItemSpec);
                        }
                    }
                    if (this.StronglyTypedFileName != null)
                    {
                        string str2 = FileTracker.FormatRootingMarker(this.Sources[0]);
                        outputs.AddComputedOutputForSourceRoot(str2, this.StronglyTypedFileName);
                        goto Label_01F8;
                    }
                    this.unsuccessfullyCreatedOutFiles.Add(this.OutputResources[0].ItemSpec);
                }
                catch (Exception exception)
                {
                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                    {
                        throw;
                    }
                    base.Log.LogErrorWithCodeFromResources("GenerateResource.CannotWriteSTRFile", new object[] { this.StronglyTypedFileName, exception.Message });
                    this.unsuccessfullyCreatedOutFiles.Add(this.OutputResources[0].ItemSpec);
                }
                return;
            }
        Label_01F8:
            files2 = new CanonicalTrackedInputFiles(this, readTLogs, this.Sources, this.ExcludedInputPaths, outputs, true, false);
            ITaskItem[] source = files2.ComputeSourcesNeedingCompilation();
            if (source.Length == 0)
            {
                if (!string.IsNullOrEmpty(this.StronglyTypedLanguage))
                {
                    this.stronglyTypedResourceSuccessfullyCreated = true;
                }
            }
            else
            {
                foreach (ITaskItem item2 in source)
                {
                    inputsToProcess.Add(item2);
                    ITaskItem2 item3 = item2 as ITaskItem2;
                    if (item3 != null)
                    {
                        outputsToProcess.Add(new TaskItem(item3.GetMetadataValueEscaped("OutputResource")));
                    }
                    else
                    {
                        outputsToProcess.Add(new TaskItem(Microsoft.Build.Shared.EscapingUtilities.Escape(item2.GetMetadata("OutputResource"))));
                    }
                }
                try
                {
                    files2.RemoveEntriesForSource(source);
                    files2.SaveTlog();
                    outputs = new CanonicalTrackedOutputFiles(this, this.TLogWriteFiles);
                    outputs.RemoveEntriesForSource(source);
                    outputs.SaveTlog();
                }
                catch (Exception exception2)
                {
                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception2) && !(exception2 is UnauthorizedAccessException))
                    {
                        throw;
                    }
                    base.Log.LogErrorFromException(exception2);
                }
            }
        }

        private void LogResgenCommandLine(List<ITaskItem> inputFiles, List<ITaskItem> outputFiles)
        {
            CommandLineBuilderExtension resGenCommand = new CommandLineBuilderExtension();
            resGenCommand.AppendFileNameIfNotNull(Path.Combine(this.resgenPath, "resgen.exe"));
            this.GenerateResGenCommandLineWithoutResources(resGenCommand);
            if (this.StronglyTypedLanguage == null)
            {
                resGenCommand.AppendSwitch("/compile");
                for (int i = 0; i < inputFiles.Count; i++)
                {
                    resGenCommand.AppendFileNamesIfNotNull(new string[] { inputFiles[i].ItemSpec, outputFiles[i].ItemSpec }, ",");
                }
            }
            else
            {
                resGenCommand.AppendFileNamesIfNotNull(inputFiles.ToArray(), " ");
                resGenCommand.AppendFileNamesIfNotNull(outputFiles.ToArray(), " ");
                resGenCommand.AppendSwitchIfNotNull("/str:", new string[] { this.StronglyTypedLanguage, this.StronglyTypedNamespace, this.StronglyTypedClassName, this.StronglyTypedFileName }, ",");
            }
            base.Log.LogCommandLine(MessageImportance.Low, resGenCommand.ToString());
        }

        private bool NeedSeparateAppDomain()
        {
            if (this.NeverLockTypeAssemblies)
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "GenerateResource.SeparateAppDomainBecauseNeverLockTypeAssembliesTrue", new object[0]);
                return true;
            }
            foreach (ITaskItem item in this.sources)
            {
                if (string.Compare(Path.GetExtension(item.ItemSpec), ".resx", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    XmlTextReader reader = null;
                    string str2 = null;
                    try
                    {
                        reader = new XmlTextReader(item.ItemSpec);
                        while (reader.Read())
                        {
                            if ((reader.NodeType == XmlNodeType.Element) && (string.Compare(reader.Name, "data", StringComparison.OrdinalIgnoreCase) == 0))
                            {
                                string attribute = reader.GetAttribute("type");
                                str2 = reader.GetAttribute("name");
                                if (attribute != null)
                                {
                                    Type type;
                                    if (!this.typeTable.TryGetValue(attribute, out type))
                                    {
                                        type = Type.GetType(attribute, false, false);
                                        this.typeTable[attribute] = type;
                                    }
                                    if (type == null)
                                    {
                                        base.Log.LogMessageFromResources(MessageImportance.Low, "GenerateResource.SeparateAppDomainBecauseOfType", new object[] { (str2 == null) ? string.Empty : str2, attribute, item.ItemSpec, reader.LineNumber });
                                        return true;
                                    }
                                }
                                else
                                {
                                    string str4 = reader.GetAttribute("mimetype");
                                    if ((str4 != null) && this.NeedSeparateAppDomainBasedOnSerializedType(reader))
                                    {
                                        base.Log.LogMessageFromResources(MessageImportance.Low, "GenerateResource.SeparateAppDomainBecauseOfMimeType", new object[] { (str2 == null) ? string.Empty : str2, str4, item.ItemSpec, reader.LineNumber });
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    catch (XmlException exception)
                    {
                        base.Log.LogMessageFromResources(MessageImportance.Low, "GenerateResource.SeparateAppDomainBecauseOfExceptionLineNumber", new object[] { item.ItemSpec, reader.LineNumber, exception.Message });
                        return true;
                    }
                    catch (SerializationException exception2)
                    {
                        base.Log.LogMessageFromResources(MessageImportance.Low, "GenerateResource.SeparateAppDomainBecauseOfErrorDeserializingLineNumber", new object[] { item.ItemSpec, (str2 == null) ? string.Empty : str2, reader.LineNumber, exception2.Message });
                        return true;
                    }
                    catch (Exception exception3)
                    {
                        if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception3))
                        {
                            throw;
                        }
                        base.Log.LogMessageFromResources(MessageImportance.Low, "GenerateResource.SeparateAppDomainBecauseOfException", new object[] { item.ItemSpec, exception3.Message });
                        if (Environment.GetEnvironmentVariable("MSBUILDDEBUG") == "1")
                        {
                            base.Log.LogErrorFromException(exception3, true, true, null);
                        }
                        return true;
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            return false;
        }

        private bool NeedSeparateAppDomainBasedOnSerializedType(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (!string.Equals(reader.Name, "value", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    string data = reader.ReadElementContentAsString();
                    return !this.DetermineWhetherSerializedObjectLoads(data);
                }
            }
            return true;
        }

        private void RecordFilesWritten()
        {
            foreach (ITaskItem item in this.OutputResources)
            {
                this.filesWritten.Add(item);
            }
            if (this.TrackFileAccess)
            {
                ITaskItem[] c = TrackedDependencies.ExpandWildcards(this.TLogReadFiles);
                ITaskItem[] itemArray2 = TrackedDependencies.ExpandWildcards(this.TLogWriteFiles);
                if ((c != null) && (c.Length > 0))
                {
                    this.filesWritten.AddRange(c);
                }
                if ((itemArray2 != null) && (itemArray2.Length > 0))
                {
                    this.filesWritten.AddRange(itemArray2);
                }
            }
            if (this.stronglyTypedResourceSuccessfullyCreated)
            {
                if (this.StronglyTypedFileName == null)
                {
                    CodeDomProvider provider = null;
                    if (ProcessResourceFiles.TryCreateCodeDomProvider(base.Log, this.StronglyTypedLanguage, out provider))
                    {
                        this.StronglyTypedFileName = ProcessResourceFiles.GenerateDefaultStronglyTypedFilename(provider, this.OutputResources[0].ItemSpec);
                    }
                }
                this.filesWritten.Add(new TaskItem(this.StronglyTypedFileName));
            }
        }

        private void RecordItemsForDisconnectIfNecessary(IEnumerable<ITaskItem> items)
        {
            if ((this.remotedTaskItems != null) && (items != null))
            {
                this.remotedTaskItems.AddRange(items);
            }
        }

        private void RemoveUnsuccessfullyCreatedResourcesFromOutputResources()
        {
            if ((this.unsuccessfullyCreatedOutFiles != null) && (this.unsuccessfullyCreatedOutFiles.Count != 0))
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow((this.OutputResources != null) && (this.OutputResources.Length != 0), "Should be at least one output resource");
                ITaskItem[] itemArray = new ITaskItem[this.OutputResources.Length - this.unsuccessfullyCreatedOutFiles.Count];
                int index = 0;
                int num2 = 0;
                for (int i = 0; i < this.Sources.Length; i++)
                {
                    if ((num2 < this.unsuccessfullyCreatedOutFiles.Count) && this.unsuccessfullyCreatedOutFiles.Contains(this.OutputResources[i].ItemSpec))
                    {
                        num2++;
                        this.Sources[i].SetMetadata("OutputResource", string.Empty);
                    }
                    else
                    {
                        itemArray[index] = this.OutputResources[i];
                        index++;
                    }
                }
                this.OutputResources = itemArray;
            }
        }

        private bool TransformResourceFilesUsingResGen(List<ITaskItem> inputsToProcess, List<ITaskItem> outputsToProcess)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(inputsToProcess.Count != 0, "There should be resource files to process");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(inputsToProcess.Count == outputsToProcess.Count, "The number of inputs and outputs should be equal");
            bool flag = true;
            if (this.resgenPath.Equals(Path.GetDirectoryName(Microsoft.Build.Shared.NativeMethodsShared.GetLongFilePath(ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("resgen.exe", TargetDotNetFrameworkVersion.Version40))), StringComparison.OrdinalIgnoreCase))
            {
                ResGen gen = this.CreateResGenTaskWithDefaultParameters();
                gen.InputFiles = inputsToProcess.ToArray();
                gen.OutputFiles = outputsToProcess.ToArray();
                ITaskItem[] outputFiles = gen.OutputFiles;
                flag = gen.Execute();
                if (!flag && ((gen.OutputFiles == null) || (gen.OutputFiles.Length == 0)))
                {
                    foreach (ITaskItem item in outputFiles)
                    {
                        this.unsuccessfullyCreatedOutFiles.Add(item.ItemSpec);
                    }
                }
                return flag;
            }
            int initialResourceIndex = 0;
            int count = 0;
            bool flag2 = false;
            CommandLineBuilderExtension resGenCommand = new CommandLineBuilderExtension();
            string resourcelessCommand = null;
            this.GenerateResGenCommandLineWithoutResources(resGenCommand);
            if (resGenCommand.Length > 0)
            {
                resourcelessCommand = resGenCommand.ToString();
            }
            while (!flag2)
            {
                count = this.CalculateResourceBatchSize(inputsToProcess, outputsToProcess, resourcelessCommand, initialResourceIndex);
                ResGen gen2 = this.CreateResGenTaskWithDefaultParameters();
                gen2.InputFiles = inputsToProcess.GetRange(initialResourceIndex, count).ToArray();
                gen2.OutputFiles = outputsToProcess.GetRange(initialResourceIndex, count).ToArray();
                ITaskItem[] itemArray2 = gen2.OutputFiles;
                bool flag3 = gen2.Execute();
                if (!flag3 && ((gen2.OutputFiles == null) || (gen2.OutputFiles.Length == 0)))
                {
                    foreach (ITaskItem item2 in itemArray2)
                    {
                        this.unsuccessfullyCreatedOutFiles.Add(item2.ItemSpec);
                    }
                }
                initialResourceIndex += count;
                flag2 = initialResourceIndex == inputsToProcess.Count;
                flag = flag && flag3;
            }
            return flag;
        }

        private bool ValidateParameters()
        {
            if ((this.OutputResources != null) && (this.OutputResources.Length != this.Sources.Length))
            {
                base.Log.LogErrorWithCodeFromResources("General.TwoVectorsMustHaveSameLength", new object[] { this.Sources.Length, this.OutputResources.Length, "Sources", "OutputResources" });
                return false;
            }
            if (this.stronglyTypedLanguage != null)
            {
                if (this.Sources.Length != 1)
                {
                    base.Log.LogErrorWithCodeFromResources("GenerateResource.STRLanguageButNotExactlyOneSourceFile", new object[0]);
                    return false;
                }
            }
            else if (((this.StronglyTypedClassName != null) || (this.StronglyTypedNamespace != null)) || ((this.StronglyTypedFileName != null) || (this.StronglyTypedManifestPrefix != null)))
            {
                base.Log.LogErrorWithCodeFromResources("GenerateResource.STRClassNamespaceOrFilenameWithoutLanguage", new object[0]);
                return false;
            }
            if (string.IsNullOrEmpty(this.TrackerLogDirectory))
            {
                this.MinimalRebuildFromTracking = false;
                this.TrackFileAccess = false;
            }
            return true;
        }

        public ITaskItem[] AdditionalInputs
        {
            get
            {
                return this.additionalInputs;
            }
            set
            {
                this.additionalInputs = value;
            }
        }

        public string[] EnvironmentVariables { get; set; }

        public ITaskItem[] ExcludedInputPaths
        {
            get
            {
                return this.excludedInputPaths;
            }
            set
            {
                this.excludedInputPaths = value;
            }
        }

        public bool ExecuteAsTool
        {
            get
            {
                return this.executeAsTool;
            }
            set
            {
                this.executeAsTool = value;
            }
        }

        [Output]
        public ITaskItem[] FilesWritten
        {
            get
            {
                return (ITaskItem[]) this.filesWritten.ToArray(typeof(ITaskItem));
            }
        }

        public bool MinimalRebuildFromTracking
        {
            get
            {
                return this.minimalRebuildFromTracking;
            }
            set
            {
                this.minimalRebuildFromTracking = value;
            }
        }

        public bool NeverLockTypeAssemblies
        {
            get
            {
                return this.neverLockTypeAssemblies;
            }
            set
            {
                this.neverLockTypeAssemblies = value;
            }
        }

        [Output]
        public ITaskItem[] OutputResources
        {
            get
            {
                return this.outputResources;
            }
            set
            {
                this.outputResources = value;
            }
        }

        public bool PublicClass
        {
            get
            {
                return this.publicClass;
            }
            set
            {
                this.publicClass = value;
            }
        }

        public ITaskItem[] References
        {
            get
            {
                return this.references;
            }
            set
            {
                this.references = value;
            }
        }

        public string SdkToolsPath
        {
            get
            {
                return this.sdkToolsPath;
            }
            set
            {
                this.sdkToolsPath = value;
            }
        }

        [Output, Required]
        public ITaskItem[] Sources
        {
            get
            {
                return this.sources;
            }
            set
            {
                this.sources = value;
            }
        }

        public ITaskItem StateFile
        {
            get
            {
                return this.stateFile;
            }
            set
            {
                this.stateFile = value;
            }
        }

        [Output]
        public string StronglyTypedClassName
        {
            get
            {
                return this.stronglyTypedClassName;
            }
            set
            {
                this.stronglyTypedClassName = value;
            }
        }

        [Output]
        public string StronglyTypedFileName
        {
            get
            {
                return this.stronglyTypedFileName;
            }
            set
            {
                this.stronglyTypedFileName = value;
            }
        }

        public string StronglyTypedLanguage
        {
            get
            {
                return this.stronglyTypedLanguage;
            }
            set
            {
                this.stronglyTypedLanguage = value;
            }
        }

        public string StronglyTypedManifestPrefix
        {
            get
            {
                return this.stronglyTypedManifestPrefix;
            }
            set
            {
                this.stronglyTypedManifestPrefix = value;
            }
        }

        public string StronglyTypedNamespace
        {
            get
            {
                return this.stronglyTypedNamespace;
            }
            set
            {
                this.stronglyTypedNamespace = value;
            }
        }

        public ITaskItem[] TLogReadFiles
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowInternalNull(this.TrackerLogDirectory, "TrackerLogDirectory is allowed to be null, but if it is null, TLogReadFiles should NEVER be used");
                return new ITaskItem[] { new TaskItem(Path.Combine(this.TrackerLogDirectory, generateResourceTlogFilenamePrefix + "*.read.*.tlog")), new TaskItem(Path.Combine(this.TrackerLogDirectory, resGenTlogFilenamePrefix + "*.read.*.tlog")) };
            }
        }

        public ITaskItem[] TLogWriteFiles
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowInternalNull(this.TrackerLogDirectory, "TrackerLogDirectory is allowed to be null, but if it is null, TLogWriteFiles should NEVER be used");
                return new ITaskItem[] { new TaskItem(Path.Combine(this.TrackerLogDirectory, generateResourceTlogFilenamePrefix + "*.write.*.tlog")), new TaskItem(Path.Combine(this.TrackerLogDirectory, resGenTlogFilenamePrefix + "*.write.*.tlog")) };
            }
        }

        public string ToolArchitecture
        {
            get
            {
                return this.toolArchitecture;
            }
            set
            {
                this.toolArchitecture = value;
            }
        }

        public string TrackerFrameworkPath
        {
            get
            {
                return this.fileTrackerPath;
            }
            set
            {
                this.fileTrackerPath = value;
            }
        }

        public string TrackerLogDirectory
        {
            get
            {
                return (this.trackerLogDirectory ?? this.GenerateTrackerLogDirectory());
            }
            set
            {
                this.trackerLogDirectory = value;
            }
        }

        public string TrackerSdkPath
        {
            get
            {
                return this.trackerPath;
            }
            set
            {
                this.trackerPath = value;
            }
        }

        public bool TrackFileAccess
        {
            get
            {
                return this.trackFileAccess;
            }
            set
            {
                this.trackFileAccess = value;
            }
        }

        public bool UseSourcePath
        {
            get
            {
                return this.useSourcePath;
            }
            set
            {
                this.useSourcePath = value;
            }
        }

        internal class ResGen : ToolTaskExtension
        {
            protected internal override void AddCommandLineCommands(CommandLineBuilderExtension commandLine)
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(!IsNullOrEmpty(this.InputFiles), "If InputFiles is empty, the task should have returned before reaching this point");
                CommandLineBuilderExtension resGenArguments = new CommandLineBuilderExtension();
                this.GenerateResGenCommands(resGenArguments, false);
                string str = this.GenerateResGenFullPath();
                if (this.TrackFileAccess)
                {
                    string rootFiles = FileTracker.FormatRootingMarker(this.InputFiles);
                    string temporaryFile = Microsoft.Build.Shared.FileUtilities.GetTemporaryFile();
                    string fileTrackerPath = FileTracker.GetFileTrackerPath(this.ToolType, this.TrackerFrameworkPath);
                    using (StreamWriter writer = new StreamWriter(temporaryFile, false, this.ResponseFileEncoding))
                    {
                        writer.Write(FileTracker.TrackerResponseFileArguments(fileTrackerPath, this.TrackerLogDirectory, rootFiles));
                    }
                    commandLine.AppendTextUnquoted(this.GetResponseFileSwitch(temporaryFile));
                    commandLine.AppendSwitch(FileTracker.TrackerCommandArguments(str ?? string.Empty, resGenArguments.ToString()));
                }
                else if (((str == null) || !str.Equals(Microsoft.Build.Shared.NativeMethodsShared.GetLongFilePath(ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("resgen.exe", TargetDotNetFrameworkVersion.Version40)), StringComparison.OrdinalIgnoreCase)) || !string.IsNullOrEmpty(this.StronglyTypedLanguage))
                {
                    commandLine.AppendTextUnquoted(resGenArguments.ToString());
                }
            }

            protected internal override void AddResponseFileCommands(CommandLineBuilderExtension commandLine)
            {
                string str = this.GenerateResGenFullPath();
                if ((!this.TrackFileAccess && (str != null)) && (str.Equals(ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("resgen.exe", TargetDotNetFrameworkVersion.Version40), StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(this.StronglyTypedLanguage)))
                {
                    CommandLineBuilderExtension resGenArguments = new CommandLineBuilderExtension();
                    this.GenerateResGenCommands(resGenArguments, true);
                    commandLine.AppendTextUnquoted(resGenArguments.ToString());
                }
            }

            public override bool Execute()
            {
                if (IsNullOrEmpty(this.InputFiles))
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResGen.NoInputFiles", new object[0]);
                    return !base.Log.HasLoggedErrors;
                }
                if (IsNullOrEmpty(this.OutputFiles))
                {
                    this.GenerateOutputFileNames();
                }
                bool flag = base.Execute();
                if (string.IsNullOrEmpty(this.StronglyTypedLanguage))
                {
                    if (!flag)
                    {
                        ITaskItem[] outputFiles = this.OutputFiles;
                        List<ITaskItem> list = new List<ITaskItem>();
                        for (int i = 0; i < outputFiles.Length; i++)
                        {
                            if (File.Exists(outputFiles[i].ItemSpec))
                            {
                                list.Add(outputFiles[i]);
                            }
                        }
                        this.OutputFiles = list.ToArray();
                    }
                }
                else
                {
                    ITaskItem item = this.OutputFiles[0];
                    if (!flag && !File.Exists(item.ItemSpec))
                    {
                        this.OutputFiles = new ITaskItem[0];
                    }
                    if (this.StronglyTypedClassName == null)
                    {
                        this.StronglyTypedClassName = Path.GetFileNameWithoutExtension(item.ItemSpec);
                    }
                    if (this.StronglyTypedFileName == null)
                    {
                        CodeDomProvider provider = null;
                        try
                        {
                            provider = CodeDomProvider.CreateProvider(this.StronglyTypedLanguage);
                        }
                        catch (ConfigurationException)
                        {
                            return false;
                        }
                        catch (SecurityException)
                        {
                            return false;
                        }
                        this.StronglyTypedFileName = ProcessResourceFiles.GenerateDefaultStronglyTypedFilename(provider, item.ItemSpec);
                    }
                }
                return (flag && !base.Log.HasLoggedErrors);
            }

            protected override string GenerateFullPathToTool()
            {
                string trackerPath = null;
                trackerPath = this.GenerateResGenFullPath();
                if (this.TrackFileAccess && (trackerPath != null))
                {
                    try
                    {
                        trackerPath = FileTracker.GetTrackerPath(this.ToolType, this.TrackerSdkPath);
                    }
                    catch (Exception exception)
                    {
                        if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                        {
                            throw;
                        }
                        base.Log.LogErrorWithCodeFromResources("General.InvalidValue", new object[] { "TrackerSdkPath", "GenerateResource" });
                        return null;
                    }
                    if (trackerPath == null)
                    {
                        base.Log.LogErrorWithCodeFromResources("ResGen.TrackerNotFound", new object[] { ToolLocationHelper.GetDotNetFrameworkSdkInstallKeyValue(TargetDotNetFrameworkVersion.Version40), ToolLocationHelper.GetDotNetFrameworkSdkRootRegistryKey(TargetDotNetFrameworkVersion.Version40) });
                    }
                }
                return trackerPath;
            }

            private void GenerateOutputFileNames()
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(!IsNullOrEmpty(this.InputFiles), "If InputFiles is empty, the task should have returned before reaching this point");
                ITaskItem[] inputFiles = this.InputFiles;
                ITaskItem[] itemArray2 = new ITaskItem[inputFiles.Length];
                for (int i = 0; i < inputFiles.Length; i++)
                {
                    ITaskItem2 item = inputFiles[i] as ITaskItem2;
                    if (item != null)
                    {
                        itemArray2[i] = new TaskItem(Path.ChangeExtension(item.EvaluatedIncludeEscaped, ".resources"));
                    }
                    else
                    {
                        itemArray2[i] = new TaskItem(Path.ChangeExtension(Microsoft.Build.Shared.EscapingUtilities.Escape(inputFiles[i].ItemSpec), ".resources"));
                    }
                }
                base.Bag["OutputFiles"] = itemArray2;
            }

            private void GenerateResGenCommands(CommandLineBuilderExtension resGenArguments, bool useForResponseFile)
            {
                resGenArguments = resGenArguments ?? new CommandLineBuilderExtension();
                if (IsNullOrEmpty(this.OutputFiles))
                {
                    this.GenerateOutputFileNames();
                }
                string switchName = "/useSourcePath" + (useForResponseFile ? "\n" : string.Empty);
                string str2 = "/publicClass" + (useForResponseFile ? "\n" : string.Empty);
                resGenArguments.AppendWhenTrue(switchName, base.Bag, "UseSourcePath");
                resGenArguments.AppendWhenTrue(str2, base.Bag, "PublicClass");
                if (this.References != null)
                {
                    foreach (ITaskItem item in this.References)
                    {
                        if (useForResponseFile && (item != null))
                        {
                            resGenArguments.AppendTextUnquoted("/r:");
                            resGenArguments.AppendTextUnquoted(item.ItemSpec);
                            resGenArguments.AppendTextUnquoted("\n");
                        }
                        else
                        {
                            resGenArguments.AppendSwitchIfNotNull("/r:", item);
                        }
                    }
                }
                if (string.IsNullOrEmpty(this.StronglyTypedLanguage))
                {
                    resGenArguments.AppendSwitch("/compile" + (useForResponseFile ? "\n" : string.Empty));
                    if ((this.InputFiles != null) && (this.InputFiles.Length > 0))
                    {
                        ITaskItem[] inputFiles = this.InputFiles;
                        ITaskItem[] outputFiles = this.OutputFiles;
                        for (int i = 0; i < inputFiles.Length; i++)
                        {
                            if (useForResponseFile)
                            {
                                if ((inputFiles[i] != null) && (outputFiles[i] != null))
                                {
                                    resGenArguments.AppendTextUnquoted(inputFiles[i].ItemSpec);
                                    resGenArguments.AppendTextUnquoted(",");
                                    resGenArguments.AppendTextUnquoted(outputFiles[i].ItemSpec);
                                    resGenArguments.AppendTextUnquoted("\n");
                                }
                            }
                            else
                            {
                                resGenArguments.AppendFileNamesIfNotNull(new ITaskItem[] { inputFiles[i], outputFiles[i] }, ",");
                            }
                        }
                    }
                }
                else
                {
                    resGenArguments.AppendFileNamesIfNotNull(this.InputFiles, " ");
                    resGenArguments.AppendFileNamesIfNotNull(this.OutputFiles, " ");
                    resGenArguments.AppendSwitchIfNotNull("/str:", new string[] { this.StronglyTypedLanguage, this.StronglyTypedNamespace, this.StronglyTypedClassName, this.StronglyTypedFileName }, ",");
                }
            }

            private string GenerateResGenFullPath()
            {
                string path = null;
                path = (string) base.Bag["ToolPathWithFile"];
                if (path == null)
                {
                    if (base.ToolPath != null)
                    {
                        path = Path.Combine(base.ToolPath, this.ToolExe);
                        if (!File.Exists(path))
                        {
                            path = null;
                        }
                    }
                    if (path == null)
                    {
                        path = Microsoft.Build.Shared.NativeMethodsShared.GetLongFilePath(SdkToolsPathUtility.GeneratePathToTool(SdkToolsPathUtility.FileInfoExists, ProcessorArchitecture.CurrentProcessArchitecture, this.SdkToolsPath, this.ToolName, base.Log, true));
                    }
                    base.Bag["ToolPathWithFile"] = path;
                }
                return path;
            }

            private static bool IsNullOrEmpty(ITaskItem[] value)
            {
                if (value != null)
                {
                    return (value.Length == 0);
                }
                return true;
            }

            protected override bool ValidateParameters()
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(!IsNullOrEmpty(this.InputFiles), "If InputFiles is empty, the task should have returned before reaching this point");
                if (!IsNullOrEmpty(this.OutputFiles) && (this.OutputFiles.Length != this.InputFiles.Length))
                {
                    base.Log.LogErrorWithCodeFromResources("General.TwoVectorsMustHaveSameLength", new object[] { this.InputFiles.Length, this.OutputFiles.Length, "InputFiles", "OutputFiles" });
                    return false;
                }
                if (!string.IsNullOrEmpty(this.StronglyTypedLanguage))
                {
                    if (this.InputFiles.Length != 1)
                    {
                        base.Log.LogErrorWithCodeFromResources("ResGen.STRLanguageButNotExactlyOneSourceFile", new object[0]);
                        return false;
                    }
                }
                else if ((!string.IsNullOrEmpty(this.StronglyTypedClassName) || !string.IsNullOrEmpty(this.StronglyTypedNamespace)) || !string.IsNullOrEmpty(this.StronglyTypedFileName))
                {
                    base.Log.LogErrorWithCodeFromResources("ResGen.STRClassNamespaceOrFilenameWithoutLanguage", new object[0]);
                    return false;
                }
                if ((string.IsNullOrEmpty(base.ToolPath) || !Directory.Exists(base.ToolPath)) && (string.IsNullOrEmpty(this.SdkToolsPath) || !Directory.Exists(this.SdkToolsPath)))
                {
                    object[] messageArgs = new object[] { this.SdkToolsPath ?? "", base.ToolPath ?? "" };
                    base.Log.LogErrorWithCodeFromResources("ResGen.SdkOrToolPathNotSpecifiedOrInvalid", messageArgs);
                    return false;
                }
                return base.ValidateParameters();
            }

            public ITaskItem[] InputFiles
            {
                get
                {
                    return (ITaskItem[]) base.Bag["InputFiles"];
                }
                set
                {
                    base.Bag["InputFiles"] = value;
                }
            }

            public ITaskItem[] OutputFiles
            {
                get
                {
                    return (ITaskItem[]) base.Bag["OutputFiles"];
                }
                set
                {
                    base.Bag["OutputFiles"] = value;
                }
            }

            public bool PublicClass
            {
                get
                {
                    return base.GetBoolParameterWithDefault("PublicClass", false);
                }
                set
                {
                    base.Bag["PublicClass"] = value;
                }
            }

            public ITaskItem[] References
            {
                get
                {
                    return (ITaskItem[]) base.Bag["References"];
                }
                set
                {
                    base.Bag["References"] = value;
                }
            }

            public string SdkToolsPath
            {
                get
                {
                    return (string) base.Bag["SdkToolsPath"];
                }
                set
                {
                    base.Bag["SdkToolsPath"] = value;
                }
            }

            public string StronglyTypedClassName
            {
                get
                {
                    return (string) base.Bag["StronglyTypedClassName"];
                }
                set
                {
                    base.Bag["StronglyTypedClassName"] = value;
                }
            }

            public string StronglyTypedFileName
            {
                get
                {
                    return (string) base.Bag["StronglyTypedFileName"];
                }
                set
                {
                    base.Bag["StronglyTypedFileName"] = value;
                }
            }

            public string StronglyTypedLanguage
            {
                get
                {
                    return (string) base.Bag["StronglyTypedLanguage"];
                }
                set
                {
                    base.Bag["StronglyTypedLanguage"] = value;
                }
            }

            public string StronglyTypedNamespace
            {
                get
                {
                    return (string) base.Bag["StronglyTypedNamespace"];
                }
                set
                {
                    base.Bag["StronglyTypedNamespace"] = value;
                }
            }

            protected override string ToolName
            {
                get
                {
                    return "ResGen.exe";
                }
            }

            public ExecutableType ToolType { get; set; }

            public string TrackerFrameworkPath { get; set; }

            public string TrackerLogDirectory { get; set; }

            public string TrackerSdkPath { get; set; }

            public bool TrackFileAccess { get; set; }

            public bool UseSourcePath
            {
                get
                {
                    return base.GetBoolParameterWithDefault("UseSourcePath", false);
                }
                set
                {
                    base.Bag["UseSourcePath"] = value;
                }
            }
        }
    }
}

