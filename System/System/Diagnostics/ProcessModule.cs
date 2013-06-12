namespace System.Diagnostics
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;

    [Designer("System.Diagnostics.Design.ProcessModuleDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class ProcessModule : Component
    {
        private System.Diagnostics.FileVersionInfo fileVersionInfo;
        internal ModuleInfo moduleInfo;

        internal ProcessModule(ModuleInfo moduleInfo)
        {
            this.moduleInfo = moduleInfo;
            GC.SuppressFinalize(this);
        }

        internal void EnsureNtProcessInfo()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", new object[] { base.ToString(), this.ModuleName });
        }

        [MonitoringDescription("ProcModBaseAddress")]
        public IntPtr BaseAddress
        {
            get
            {
                return this.moduleInfo.baseOfDll;
            }
        }

        [MonitoringDescription("ProcModEntryPointAddress")]
        public IntPtr EntryPointAddress
        {
            get
            {
                this.EnsureNtProcessInfo();
                return this.moduleInfo.entryPoint;
            }
        }

        [MonitoringDescription("ProcModFileName")]
        public string FileName
        {
            get
            {
                return this.moduleInfo.fileName;
            }
        }

        [Browsable(false)]
        public System.Diagnostics.FileVersionInfo FileVersionInfo
        {
            get
            {
                if (this.fileVersionInfo == null)
                {
                    this.fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(this.FileName);
                }
                return this.fileVersionInfo;
            }
        }

        [MonitoringDescription("ProcModModuleMemorySize")]
        public int ModuleMemorySize
        {
            get
            {
                return this.moduleInfo.sizeOfImage;
            }
        }

        [MonitoringDescription("ProcModModuleName")]
        public string ModuleName
        {
            get
            {
                return this.moduleInfo.baseName;
            }
        }
    }
}

