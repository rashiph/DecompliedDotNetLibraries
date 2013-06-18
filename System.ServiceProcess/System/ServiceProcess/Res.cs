namespace System.ServiceProcess
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class Res
    {
        internal const string ArgsCantBeNull = "ArgsCantBeNull";
        internal const string BadMachineName = "BadMachineName";
        internal const string ButtonOK = "ButtonOK";
        internal const string CallbackHandler = "CallbackHandler";
        internal const string CannotChangeName = "CannotChangeName";
        internal const string CannotChangeProperties = "CannotChangeProperties";
        internal const string CannotStart = "CannotStart";
        internal const string CantControlOnWin9x = "CantControlOnWin9x";
        internal const string CantInstallOnWin9x = "CantInstallOnWin9x";
        internal const string CantRunOnWin9x = "CantRunOnWin9x";
        internal const string CantRunOnWin9xTitle = "CantRunOnWin9xTitle";
        internal const string CantStartFromCommandLine = "CantStartFromCommandLine";
        internal const string CantStartFromCommandLineTitle = "CantStartFromCommandLineTitle";
        internal const string CommandFailed = "CommandFailed";
        internal const string CommandSuccessful = "CommandSuccessful";
        internal const string ContinueFailed = "ContinueFailed";
        internal const string ContinueSuccessful = "ContinueSuccessful";
        internal const string ControlService = "ControlService";
        internal const string DisplayNameTooLong = "DisplayNameTooLong";
        internal const string ErrorNumber = "ErrorNumber";
        internal const string FailedToUnloadAppDomain = "FailedToUnloadAppDomain";
        internal const string FileName = "FileName";
        internal const string HelpText = "HelpText";
        internal const string InstallError = "InstallError";
        internal const string InstallFailed = "InstallFailed";
        internal const string InstallingService = "InstallingService";
        internal const string InstallOK = "InstallOK";
        internal const string InstallService = "InstallService";
        internal const string InstallSuccessful = "InstallSuccessful";
        internal const string InvalidParameter = "InvalidParameter";
        internal const string Label_MissmatchedPasswords = "Label_MissmatchedPasswords";
        internal const string Label_SetServiceLogin = "Label_SetServiceLogin";
        private static Res loader;
        internal const string NoDisplayName = "NoDisplayName";
        internal const string NoGivenName = "NoGivenName";
        internal const string NoInstaller = "NoInstaller";
        internal const string NoMachineName = "NoMachineName";
        internal const string NoService = "NoService";
        internal const string NoServices = "NoServices";
        internal const string NotAService = "NotAService";
        internal const string NotInPendingState = "NotInPendingState";
        internal const string OpenSC = "OpenSC";
        internal const string OpenService = "OpenService";
        internal const string PauseFailed = "PauseFailed";
        internal const string PauseService = "PauseService";
        internal const string PauseSuccessful = "PauseSuccessful";
        internal const string PowerEventFailed = "PowerEventFailed";
        internal const string PowerEventOK = "PowerEventOK";
        private ResourceManager resources;
        internal const string ResumeService = "ResumeService";
        internal const string RTL = "RTL";
        internal const string SBAutoLog = "SBAutoLog";
        internal const string SBServiceDescription = "SBServiceDescription";
        internal const string SBServiceName = "SBServiceName";
        internal const string ServiceControllerDesc = "ServiceControllerDesc";
        internal const string ServiceDependency = "ServiceDependency";
        internal const string ServiceInstallerDelayedAutoStart = "ServiceInstallerDelayedAutoStart";
        internal const string ServiceInstallerDescription = "ServiceInstallerDescription";
        internal const string ServiceInstallerDisplayName = "ServiceInstallerDisplayName";
        internal const string ServiceInstallerServiceName = "ServiceInstallerServiceName";
        internal const string ServiceInstallerServicesDependedOn = "ServiceInstallerServicesDependedOn";
        internal const string ServiceInstallerStartType = "ServiceInstallerStartType";
        internal const string ServiceName = "ServiceName";
        internal const string ServiceNameTooLongForNt4 = "ServiceNameTooLongForNt4";
        internal const string ServiceProcessInstallerAccount = "ServiceProcessInstallerAccount";
        internal const string ServiceRemoved = "ServiceRemoved";
        internal const string ServiceRemoving = "ServiceRemoving";
        internal const string ServiceStartedIncorrectly = "ServiceStartedIncorrectly";
        internal const string ServiceStartType = "ServiceStartType";
        internal const string ServiceUsage = "ServiceUsage";
        internal const string SessionChangeFailed = "SessionChangeFailed";
        internal const string ShutdownFailed = "ShutdownFailed";
        internal const string ShutdownOK = "ShutdownOK";
        internal const string SPCanPauseAndContinue = "SPCanPauseAndContinue";
        internal const string SPCanShutdown = "SPCanShutdown";
        internal const string SPCanStop = "SPCanStop";
        internal const string SPDependentServices = "SPDependentServices";
        internal const string SPDisplayName = "SPDisplayName";
        internal const string SPMachineName = "SPMachineName";
        internal const string SPServiceName = "SPServiceName";
        internal const string SPServicesDependedOn = "SPServicesDependedOn";
        internal const string SPServiceType = "SPServiceType";
        internal const string SPStatus = "SPStatus";
        internal const string StartFailed = "StartFailed";
        internal const string StartingService = "StartingService";
        internal const string StartService = "StartService";
        internal const string StartSuccessful = "StartSuccessful";
        internal const string StopFailed = "StopFailed";
        internal const string StopService = "StopService";
        internal const string StopSuccessful = "StopSuccessful";
        internal const string Timeout = "Timeout";
        internal const string TryToStop = "TryToStop";
        internal const string UnattendedCannotPrompt = "UnattendedCannotPrompt";
        internal const string UninstallFailed = "UninstallFailed";
        internal const string UninstallSuccessful = "UninstallSuccessful";
        internal const string UserCanceledInstall = "UserCanceledInstall";
        internal const string UserName = "UserName";
        internal const string UserPassword = "UserPassword";

        internal Res()
        {
            this.resources = new ResourceManager("System.ServiceProcess", base.GetType().Assembly);
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

