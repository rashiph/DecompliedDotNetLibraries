namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Text;

    public static class FileTracker
    {
        private static string s_applicationDataPath = FileUtilities.EnsureTrailingSlash(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToUpperInvariant());
        private static string s_FileTrackerFilename = "FileTracker.dll";
        private static string s_localApplicationDataPath = FileUtilities.EnsureTrailingSlash(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToUpperInvariant());
        private static string s_localLowApplicationDataPath = FileUtilities.EnsureTrailingSlash(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"AppData\LocalLow").ToUpperInvariant());
        private static string s_tempLongPath = FileUtilities.EnsureTrailingSlash(NativeMethodsShared.GetLongFilePath(s_tempPath).ToUpperInvariant());
        private static string s_tempPath = Path.GetDirectoryName(Path.GetTempPath());
        private static string s_tempShortPath = FileUtilities.EnsureTrailingSlash(NativeMethodsShared.GetShortFilePath(s_tempPath).ToUpperInvariant());
        private static string s_TrackerFilename = "Tracker.exe";

        public static string CreateRootingMarkerResponseFile(ITaskItem[] sources)
        {
            return CreateRootingMarkerResponseFile(FormatRootingMarker(sources));
        }

        public static string CreateRootingMarkerResponseFile(string rootMarker)
        {
            string temporaryFile = FileUtilities.GetTemporaryFile(".rsp");
            File.WriteAllText(temporaryFile, "/r \"" + rootMarker + "\"", Encoding.Unicode);
            return temporaryFile;
        }

        public static void EndTrackingContext()
        {
            NativeMethodsShared.InprocTracking.EndTrackingContext();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string EnsureFileTrackerOnPath()
        {
            return EnsureFileTrackerOnPath(null);
        }

        public static string EnsureFileTrackerOnPath(string rootPath)
        {
            string environmentVariable = Environment.GetEnvironmentVariable("PATH");
            string fileTrackerPath = GetFileTrackerPath(ExecutableType.SameAsCurrentProcess, rootPath);
            if (!string.IsNullOrEmpty(fileTrackerPath))
            {
                Environment.SetEnvironmentVariable("Path", Path.GetDirectoryName(fileTrackerPath) + ";" + environmentVariable);
            }
            return environmentVariable;
        }

        public static bool FileIsExcludedFromDependencies(string fileName)
        {
            return (((FileIsUnderPath(fileName, s_applicationDataPath) || FileIsUnderPath(fileName, s_localApplicationDataPath)) || (FileIsUnderPath(fileName, s_localLowApplicationDataPath) || FileIsUnderPath(fileName, s_tempShortPath))) || FileIsUnderPath(fileName, s_tempLongPath));
        }

        public static bool FileIsUnderPath(string fileName, string path)
        {
            path = FileUtilities.EnsureTrailingSlash(path);
            return (string.Compare(fileName, 0, path, 0, path.Length, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static string FindTrackerOnPath()
        {
            foreach (string str in Environment.GetEnvironmentVariable("PATH").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    string fullPath;
                    if (!Path.IsPathRooted(str))
                    {
                        fullPath = Path.GetFullPath(str);
                    }
                    else
                    {
                        fullPath = str;
                    }
                    fullPath = Path.Combine(fullPath, s_TrackerFilename);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
                catch (Exception exception)
                {
                    if (ExceptionHandling.NotExpectedException(exception))
                    {
                        throw;
                    }
                }
            }
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static bool ForceOutOfProcTracking(ExecutableType toolType)
        {
            return ForceOutOfProcTracking(toolType, null, null);
        }

        public static bool ForceOutOfProcTracking(ExecutableType toolType, string dllName, string cancelEventName)
        {
            bool flag = false;
            if (cancelEventName == null)
            {
                if (dllName != null)
                {
                    return true;
                }
                if (IntPtr.Size == 4)
                {
                    if ((toolType == ExecutableType.Managed64Bit) || (toolType == ExecutableType.Native64Bit))
                    {
                        return true;
                    }
                    if ((toolType == ExecutableType.ManagedIL) && (ToolLocationHelper.GetPathToDotNetFrameworkFile(s_TrackerFilename, TargetDotNetFrameworkVersion.Version40, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness64) != null))
                    {
                        flag = true;
                    }
                    return flag;
                }
                if ((IntPtr.Size != 8) || ((toolType != ExecutableType.Managed32Bit) && (toolType != ExecutableType.Native32Bit)))
                {
                    return flag;
                }
            }
            return true;
        }

        public static string FormatRootingMarker(ITaskItem source)
        {
            return FormatRootingMarker(new ITaskItem[] { source }, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string FormatRootingMarker(ITaskItem[] sources)
        {
            return FormatRootingMarker(sources, null);
        }

        public static string FormatRootingMarker(ITaskItem source, ITaskItem output)
        {
            return FormatRootingMarker(new ITaskItem[] { source }, new ITaskItem[] { output });
        }

        public static string FormatRootingMarker(ITaskItem[] sources, ITaskItem[] outputs)
        {
            ErrorUtilities.VerifyThrowArgumentNull(sources, "sources");
            ArrayList list = new ArrayList();
            StringBuilder builder = new StringBuilder();
            int length = 0;
            foreach (ITaskItem item in sources)
            {
                list.Add(FileUtilities.NormalizePath(item.ItemSpec).ToUpperInvariant());
            }
            if (outputs != null)
            {
                foreach (ITaskItem item2 in outputs)
                {
                    list.Add(FileUtilities.NormalizePath(item2.ItemSpec).ToUpperInvariant());
                }
            }
            list.Sort(StringComparer.OrdinalIgnoreCase);
            foreach (string str in list)
            {
                builder.Append(str);
                builder.Append('|');
            }
            length = builder.Length - 1;
            if (length < 0)
            {
                length = 0;
            }
            return builder.ToString(0, length);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string GetFileTrackerPath(ExecutableType toolType)
        {
            return GetFileTrackerPath(toolType, null);
        }

        public static string GetFileTrackerPath(ExecutableType toolType, string rootPath)
        {
            return GetPath(s_FileTrackerFilename, toolType, rootPath);
        }

        private static string GetPath(string filename, Microsoft.Build.Utilities.DotNetFrameworkArchitecture bitness)
        {
            ErrorUtilities.VerifyThrow(s_TrackerFilename.Equals(filename, StringComparison.OrdinalIgnoreCase) || s_FileTrackerFilename.Equals(filename, StringComparison.OrdinalIgnoreCase), "This method should only be passed s_TrackerFilename or s_FileTrackerFilename, but was passed {0} instead!", filename);
            string str = ToolLocationHelper.GetPathToDotNetFrameworkFile(filename, TargetDotNetFrameworkVersion.Version40, bitness);
            if ((string.IsNullOrEmpty(str) || !File.Exists(str)) && s_TrackerFilename.Equals(filename, StringComparison.OrdinalIgnoreCase))
            {
                str = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile(filename, TargetDotNetFrameworkVersion.Version40, bitness);
            }
            return str;
        }

        private static string GetPath(string filename, ExecutableType toolType, string rootPath)
        {
            string path = null;
            if (!string.IsNullOrEmpty(rootPath))
            {
                path = Path.Combine(rootPath, filename);
                if (!File.Exists(path))
                {
                    path = null;
                }
                return path;
            }
            switch (toolType)
            {
                case ExecutableType.Native32Bit:
                    return GetPath(filename, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness32);

                case ExecutableType.Native64Bit:
                    return GetPath(filename, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness64);

                case ExecutableType.ManagedIL:
                    path = GetPath(filename, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness64);
                    if (path == null)
                    {
                        path = GetPath(filename, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness32);
                    }
                    return path;

                case ExecutableType.Managed32Bit:
                    return GetPath(filename, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness32);

                case ExecutableType.Managed64Bit:
                    return GetPath(filename, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness64);

                case ExecutableType.SameAsCurrentProcess:
                    if (IntPtr.Size != 4)
                    {
                        return GetPath(filename, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness64);
                    }
                    return GetPath(filename, Microsoft.Build.Utilities.DotNetFrameworkArchitecture.Bitness32);
            }
            ErrorUtilities.ThrowInternalErrorUnreachable();
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string GetTrackerPath(ExecutableType toolType)
        {
            return GetTrackerPath(toolType, null);
        }

        public static string GetTrackerPath(ExecutableType toolType, string rootPath)
        {
            return GetPath(s_TrackerFilename, toolType, rootPath);
        }

        internal static void LogMessage(TaskLoggingHelper Log, MessageImportance importance, string message, params object[] messageArgs)
        {
            if (Log != null)
            {
                Log.LogMessage(importance, message, messageArgs);
            }
        }

        internal static void LogMessageFromResources(TaskLoggingHelper Log, MessageImportance importance, string messageResourceName, params object[] messageArgs)
        {
            if (Log != null)
            {
                ErrorUtilities.VerifyThrowArgumentNull(messageResourceName, "messageResourceName");
                Log.LogMessage(importance, AssemblyResources.FormatResourceString(messageResourceName, messageArgs), new object[0]);
            }
        }

        internal static void LogWarningWithCodeFromResources(TaskLoggingHelper Log, string messageResourceName, params object[] messageArgs)
        {
            if (Log != null)
            {
                Log.LogWarningWithCodeFromResources(messageResourceName, messageArgs);
            }
        }

        public static void ResumeTracking()
        {
            NativeMethodsShared.InprocTracking.ResumeTracking();
        }

        public static void SetThreadCount(int threadCount)
        {
            NativeMethodsShared.InprocTracking.SetThreadCount(threadCount);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Process StartProcess(string command, string arguments, ExecutableType toolType)
        {
            return StartProcess(command, arguments, toolType, null, null, null, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Process StartProcess(string command, string arguments, ExecutableType toolType, string rootFiles)
        {
            return StartProcess(command, arguments, toolType, null, null, rootFiles, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Process StartProcess(string command, string arguments, ExecutableType toolType, string intermediateDirectory, string rootFiles)
        {
            return StartProcess(command, arguments, toolType, null, intermediateDirectory, rootFiles, null);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Process StartProcess(string command, string arguments, ExecutableType toolType, string dllName, string intermediateDirectory, string rootFiles)
        {
            return StartProcess(command, arguments, toolType, dllName, intermediateDirectory, rootFiles, null);
        }

        public static Process StartProcess(string command, string arguments, ExecutableType toolType, string dllName, string intermediateDirectory, string rootFiles, string cancelEventName)
        {
            dllName = dllName ?? GetFileTrackerPath(toolType);
            string str = TrackerArguments(command, arguments, dllName, intermediateDirectory, rootFiles, cancelEventName);
            return Process.Start(GetTrackerPath(toolType), str);
        }

        public static void StartTrackingContext(string intermediateDirectory, string taskName)
        {
            NativeMethodsShared.InprocTracking.StartTrackingContext(intermediateDirectory, taskName);
        }

        public static void StartTrackingContextWithRoot(string intermediateDirectory, string taskName, string rootMarkerResponseFile)
        {
            NativeMethodsShared.InprocTracking.StartTrackingContextWithRoot(intermediateDirectory, taskName, rootMarkerResponseFile);
        }

        public static void StopTrackingAndCleanup()
        {
            NativeMethodsShared.InprocTracking.StopTrackingAndCleanup();
        }

        public static void SuspendTracking()
        {
            NativeMethodsShared.InprocTracking.SuspendTracking();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string TrackerArguments(string command, string arguments, string dllName, string intermediateDirectory, string rootFiles)
        {
            return TrackerArguments(command, arguments, dllName, intermediateDirectory, rootFiles, null);
        }

        public static string TrackerArguments(string command, string arguments, string dllName, string intermediateDirectory, string rootFiles, string cancelEventName)
        {
            return (TrackerResponseFileArguments(dllName, intermediateDirectory, rootFiles, cancelEventName) + TrackerCommandArguments(command, arguments));
        }

        public static string TrackerCommandArguments(string command, string arguments)
        {
            CommandLineBuilder builder = new CommandLineBuilder();
            builder.AppendSwitch(" /c");
            builder.AppendFileNameIfNotNull(command);
            return (builder.ToString() + " " + arguments);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string TrackerResponseFileArguments(string dllName, string intermediateDirectory, string rootFiles)
        {
            return TrackerResponseFileArguments(dllName, intermediateDirectory, rootFiles, null);
        }

        public static string TrackerResponseFileArguments(string dllName, string intermediateDirectory, string rootFiles, string cancelEventName)
        {
            CommandLineBuilder builder = new CommandLineBuilder();
            builder.AppendSwitchIfNotNull("/d ", dllName);
            if (!string.IsNullOrEmpty(intermediateDirectory))
            {
                intermediateDirectory = FileUtilities.NormalizePath(intermediateDirectory);
                if (FileUtilities.EndsWithSlash(intermediateDirectory))
                {
                    intermediateDirectory = Path.GetDirectoryName(intermediateDirectory);
                }
                builder.AppendSwitchIfNotNull("/i ", intermediateDirectory);
            }
            builder.AppendSwitchIfNotNull("/r ", rootFiles);
            builder.AppendSwitchIfNotNull("/b ", cancelEventName);
            return (builder.ToString() + " ");
        }

        public static void WriteAllTLogs(string intermediateDirectory, string taskName)
        {
            NativeMethodsShared.InprocTracking.WriteAllTLogs(intermediateDirectory, taskName);
        }

        public static void WriteContextTLogs(string intermediateDirectory, string taskName)
        {
            NativeMethodsShared.InprocTracking.WriteContextTLogs(intermediateDirectory, taskName);
        }
    }
}

