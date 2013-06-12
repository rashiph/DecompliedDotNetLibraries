namespace System
{
    using System.Runtime.InteropServices;

    [Guid("27FFF232-A7A8-40dd-8D4A-734AD59FCD41"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
    public interface IAppDomainSetup
    {
        string ApplicationBase { get; set; }
        string ApplicationName { get; set; }
        string CachePath { get; set; }
        string ConfigurationFile { get; set; }
        string DynamicBase { get; set; }
        string LicenseFile { get; set; }
        string PrivateBinPath { get; set; }
        string PrivateBinPathProbe { get; set; }
        string ShadowCopyDirectories { get; set; }
        string ShadowCopyFiles { get; set; }
    }
}

