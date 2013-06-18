namespace System.Configuration.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public interface IInternalConfigClientHost
    {
        string GetExeConfigPath();
        string GetLocalUserConfigPath();
        string GetRoamingUserConfigPath();
        bool IsExeConfig(string configPath);
        bool IsLocalUserConfig(string configPath);
        bool IsRoamingUserConfig(string configPath);
    }
}

