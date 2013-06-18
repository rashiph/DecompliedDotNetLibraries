namespace System.Configuration.Install
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class Res
    {
        internal const string CantAddSelf = "CantAddSelf";
        internal const string CreatingEventLog = "CreatingEventLog";
        internal const string CreatingPerformanceCounter = "CreatingPerformanceCounter";
        internal const string DeletingEventLog = "DeletingEventLog";
        internal const string Desc_AssemblyInstaller_Assembly = "Desc_AssemblyInstaller_Assembly";
        internal const string Desc_AssemblyInstaller_CommandLine = "Desc_AssemblyInstaller_CommandLine";
        internal const string Desc_AssemblyInstaller_Path = "Desc_AssemblyInstaller_Path";
        internal const string Desc_AssemblyInstaller_UseNewContext = "Desc_AssemblyInstaller_UseNewContext";
        internal const string Desc_CategoryCount = "Desc_CategoryCount";
        internal const string Desc_CategoryResourceFile = "Desc_CategoryResourceFile";
        internal const string Desc_Installer_HelpText = "Desc_Installer_HelpText";
        internal const string Desc_Installer_Parent = "Desc_Installer_Parent";
        internal const string Desc_Log = "Desc_Log";
        internal const string Desc_MessageResourceFile = "Desc_MessageResourceFile";
        internal const string Desc_ParameterResourceFile = "Desc_ParameterResourceFile";
        internal const string Desc_Source = "Desc_Source";
        internal const string Desc_UninstallAction = "Desc_UninstallAction";
        internal const string IncompleteEventLog = "IncompleteEventLog";
        internal const string IncompletePerformanceCounter = "IncompletePerformanceCounter";
        internal const string InstallAbort = "InstallAbort";
        internal const string InstallActivityCommitting = "InstallActivityCommitting";
        internal const string InstallActivityInstalling = "InstallActivityInstalling";
        internal const string InstallActivityRollingBack = "InstallActivityRollingBack";
        internal const string InstallActivityUninstalling = "InstallActivityUninstalling";
        internal const string InstallAssemblyHelp = "InstallAssemblyHelp";
        internal const string InstallBadParent = "InstallBadParent";
        internal const string InstallCannotCreateInstance = "InstallCannotCreateInstance";
        internal const string InstallCommitException = "InstallCommitException";
        internal const string InstallCommitNtRun = "InstallCommitNtRun";
        internal const string InstallDictionaryCorrupted = "InstallDictionaryCorrupted";
        internal const string InstallDictionaryMissingValues = "InstallDictionaryMissingValues";
        internal const string InstallEventException = "InstallEventException";
        internal const string InstallException = "InstallException";
        internal const string InstallFileDoesntExist = "InstallFileDoesntExist";
        internal const string InstallFileDoesntExistCommandLine = "InstallFileDoesntExistCommandLine";
        internal const string InstallFileLocation = "InstallFileLocation";
        internal const string InstallFileNotFound = "InstallFileNotFound";
        internal const string InstallHelpMessageEnd = "InstallHelpMessageEnd";
        internal const string InstallHelpMessageStart = "InstallHelpMessageStart";
        internal const string InstallInfoBeginCommit = "InstallInfoBeginCommit";
        internal const string InstallInfoBeginInstall = "InstallInfoBeginInstall";
        internal const string InstallInfoBeginRollback = "InstallInfoBeginRollback";
        internal const string InstallInfoBeginUninstall = "InstallInfoBeginUninstall";
        internal const string InstallInfoCommitDone = "InstallInfoCommitDone";
        internal const string InstallInfoException = "InstallInfoException";
        internal const string InstallInfoRollbackDone = "InstallInfoRollbackDone";
        internal const string InstallInfoTransacted = "InstallInfoTransacted";
        internal const string InstallInfoTransactedDone = "InstallInfoTransactedDone";
        internal const string InstallInfoUninstallDone = "InstallInfoUninstallDone";
        internal const string InstallInitializeException = "InstallInitializeException";
        internal const string InstallInstallerNotFound = "InstallInstallerNotFound";
        internal const string InstallInstallNtRun = "InstallInstallNtRun";
        internal const string InstallLogCommitException = "InstallLogCommitException";
        internal const string InstallLogContent = "InstallLogContent";
        internal const string InstallLogError = "InstallLogError";
        internal const string InstallLogInner = "InstallLogInner";
        internal const string InstallLogNone = "InstallLogNone";
        internal const string InstallLogParameters = "InstallLogParameters";
        internal const string InstallLogRollbackException = "InstallLogRollbackException";
        internal const string InstallLogUninstallException = "InstallLogUninstallException";
        internal const string InstallNoInstallerTypes = "InstallNoInstallerTypes";
        internal const string InstallNoPublicInstallers = "InstallNoPublicInstallers";
        internal const string InstallNullParameter = "InstallNullParameter";
        internal const string InstallRecursiveParent = "InstallRecursiveParent";
        internal const string InstallRollback = "InstallRollback";
        internal const string InstallRollbackException = "InstallRollbackException";
        internal const string InstallRollbackNtRun = "InstallRollbackNtRun";
        internal const string InstallSavedStateFileCorruptedWarning = "InstallSavedStateFileCorruptedWarning";
        internal const string InstallSeverityError = "InstallSeverityError";
        internal const string InstallSeverityWarning = "InstallSeverityWarning";
        internal const string InstallUnableDeleteFile = "InstallUnableDeleteFile";
        internal const string InstallUninstallException = "InstallUninstallException";
        internal const string InstallUninstallNtRun = "InstallUninstallNtRun";
        internal const string InvalidProperty = "InvalidProperty";
        private static Res loader;
        internal const string LocalSourceNotRegisteredWarning = "LocalSourceNotRegisteredWarning";
        internal const string NewCategory = "NewCategory";
        internal const string NotAnEventLog = "NotAnEventLog";
        internal const string NotAPerformanceCounter = "NotAPerformanceCounter";
        internal const string NotCustomPerformanceCategory = "NotCustomPerformanceCategory";
        internal const string PCCategoryName = "PCCategoryName";
        internal const string PCCounterName = "PCCounterName";
        internal const string PCI_CategoryHelp = "PCI_CategoryHelp";
        internal const string PCI_Counters = "PCI_Counters";
        internal const string PCI_IsMultiInstance = "PCI_IsMultiInstance";
        internal const string PCI_UninstallAction = "PCI_UninstallAction";
        internal const string PCInstanceName = "PCInstanceName";
        internal const string PCMachineName = "PCMachineName";
        internal const string PerfInvalidCategoryName = "PerfInvalidCategoryName";
        internal const string RemovingEventLog = "RemovingEventLog";
        internal const string RemovingInstallState = "RemovingInstallState";
        internal const string RemovingPerformanceCounter = "RemovingPerformanceCounter";
        private ResourceManager resources;
        internal const string RestoringEventLog = "RestoringEventLog";
        internal const string RestoringPerformanceCounter = "RestoringPerformanceCounter";
        internal const string WinNTRequired = "WinNTRequired";
        internal const string WrappedExceptionSource = "WrappedExceptionSource";

        internal Res()
        {
            this.resources = new ResourceManager("System.Configuration.Install", base.GetType().Assembly);
        }

        private static Res GetLoader()
        {
            if (loader == null)
            {
                Res res = new Res();
                Interlocked.CompareExchange<Res>(ref loader, res, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            Res loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

