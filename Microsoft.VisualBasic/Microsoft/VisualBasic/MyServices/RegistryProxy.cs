namespace Microsoft.VisualBasic.MyServices
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Security.Permissions;

    [EditorBrowsable(EditorBrowsableState.Never), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class RegistryProxy
    {
        internal RegistryProxy()
        {
        }

        public object GetValue(string keyName, string valueName, object defaultValue)
        {
            return Registry.GetValue(keyName, valueName, defaultValue);
        }

        public void SetValue(string keyName, string valueName, object value)
        {
            Registry.SetValue(keyName, valueName, value);
        }

        public void SetValue(string keyName, string valueName, object value, RegistryValueKind valueKind)
        {
            Registry.SetValue(keyName, valueName, value, valueKind);
        }

        public RegistryKey ClassesRoot
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return Registry.ClassesRoot;
            }
        }

        public RegistryKey CurrentConfig
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return Registry.CurrentConfig;
            }
        }

        public RegistryKey CurrentUser
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return Registry.CurrentUser;
            }
        }

        [Obsolete("The DynData registry key works only on Win9x, which is not supported by this version of the .NET Framework.  Use the PerformanceData registry key instead.  This property will be removed from a future version of the framework.")]
        public RegistryKey DynData
        {
            get
            {
                return null;
            }
        }

        public RegistryKey LocalMachine
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return Registry.LocalMachine;
            }
        }

        public RegistryKey PerformanceData
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return Registry.PerformanceData;
            }
        }

        public RegistryKey Users
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return Registry.Users;
            }
        }
    }
}

