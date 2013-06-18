namespace Microsoft.VisualBasic.Devices
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [DebuggerTypeProxy(typeof(ComputerInfo.ComputerInfoDebugView)), HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class ComputerInfo
    {
        private InternalMemoryStatus m_InternalMemoryStatus = null;
        [SecurityCritical]
        private ManagementBaseObject m_OSManagementObject = null;

        [CLSCompliant(false)]
        public ulong AvailablePhysicalMemory
        {
            [SecuritySafeCritical]
            get
            {
                return this.MemoryStatus.AvailablePhysicalMemory;
            }
        }

        [CLSCompliant(false)]
        public ulong AvailableVirtualMemory
        {
            [SecuritySafeCritical]
            get
            {
                return this.MemoryStatus.AvailableVirtualMemory;
            }
        }

        public CultureInfo InstalledUICulture
        {
            get
            {
                return CultureInfo.InstalledUICulture;
            }
        }

        private InternalMemoryStatus MemoryStatus
        {
            get
            {
                if (this.m_InternalMemoryStatus == null)
                {
                    this.m_InternalMemoryStatus = new InternalMemoryStatus();
                }
                return this.m_InternalMemoryStatus;
            }
        }

        public string OSFullName
        {
            [SecuritySafeCritical]
            get
            {
                try
                {
                    string str2 = "Name";
                    char ch = '|';
                    string str3 = Conversions.ToString(this.OSManagementBaseObject.Properties[str2].Value);
                    if (str3.Contains(Conversions.ToString(ch)))
                    {
                        return str3.Substring(0, str3.IndexOf(ch));
                    }
                    return str3;
                }
                catch (COMException)
                {
                    return this.OSPlatform;
                }
            }
        }

        private ManagementBaseObject OSManagementBaseObject
        {
            [SecurityCritical]
            get
            {
                string queryOrClassName = "Win32_OperatingSystem";
                if (this.m_OSManagementObject == null)
                {
                    SelectQuery query = new SelectQuery(queryOrClassName);
                    ManagementObjectCollection objects = new ManagementObjectSearcher(query).Get();
                    if (objects.Count <= 0)
                    {
                        throw ExceptionUtils.GetInvalidOperationException("DiagnosticInfo_FullOSName", new string[0]);
                    }
                    ManagementObjectCollection.ManagementObjectEnumerator enumerator = objects.GetEnumerator();
                    enumerator.MoveNext();
                    this.m_OSManagementObject = enumerator.Current;
                }
                return this.m_OSManagementObject;
            }
        }

        public string OSPlatform
        {
            get
            {
                return Environment.OSVersion.Platform.ToString();
            }
        }

        public string OSVersion
        {
            get
            {
                return Environment.OSVersion.Version.ToString();
            }
        }

        [CLSCompliant(false)]
        public ulong TotalPhysicalMemory
        {
            [SecuritySafeCritical]
            get
            {
                return this.MemoryStatus.TotalPhysicalMemory;
            }
        }

        [CLSCompliant(false)]
        public ulong TotalVirtualMemory
        {
            [SecuritySafeCritical]
            get
            {
                return this.MemoryStatus.TotalVirtualMemory;
            }
        }

        internal sealed class ComputerInfoDebugView
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private ComputerInfo m_InstanceBeingWatched;

            public ComputerInfoDebugView(ComputerInfo RealClass)
            {
                this.m_InstanceBeingWatched = RealClass;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public ulong AvailablePhysicalMemory
            {
                get
                {
                    return this.m_InstanceBeingWatched.AvailablePhysicalMemory;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public ulong AvailableVirtualMemory
            {
                get
                {
                    return this.m_InstanceBeingWatched.AvailableVirtualMemory;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public CultureInfo InstalledUICulture
            {
                get
                {
                    return this.m_InstanceBeingWatched.InstalledUICulture;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public string OSPlatform
            {
                get
                {
                    return this.m_InstanceBeingWatched.OSPlatform;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public string OSVersion
            {
                get
                {
                    return this.m_InstanceBeingWatched.OSVersion;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public ulong TotalPhysicalMemory
            {
                get
                {
                    return this.m_InstanceBeingWatched.TotalPhysicalMemory;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public ulong TotalVirtualMemory
            {
                get
                {
                    return this.m_InstanceBeingWatched.TotalVirtualMemory;
                }
            }
        }

        private class InternalMemoryStatus
        {
            private bool m_IsOldOS = (Environment.OSVersion.Version.Major < 5);
            private Microsoft.VisualBasic.CompilerServices.NativeMethods.MEMORYSTATUS m_MemoryStatus;
            private Microsoft.VisualBasic.CompilerServices.NativeMethods.MEMORYSTATUSEX m_MemoryStatusEx;

            internal InternalMemoryStatus()
            {
            }

            [SecurityCritical]
            private void Refresh()
            {
                if (this.m_IsOldOS)
                {
                    this.m_MemoryStatus = new Microsoft.VisualBasic.CompilerServices.NativeMethods.MEMORYSTATUS();
                    Microsoft.VisualBasic.CompilerServices.NativeMethods.GlobalMemoryStatus(ref this.m_MemoryStatus);
                }
                else
                {
                    this.m_MemoryStatusEx = new Microsoft.VisualBasic.CompilerServices.NativeMethods.MEMORYSTATUSEX();
                    this.m_MemoryStatusEx.Init();
                    if (!Microsoft.VisualBasic.CompilerServices.NativeMethods.GlobalMemoryStatusEx(ref this.m_MemoryStatusEx))
                    {
                        throw ExceptionUtils.GetWin32Exception("DiagnosticInfo_Memory", new string[0]);
                    }
                }
            }

            internal ulong AvailablePhysicalMemory
            {
                [SecurityCritical]
                get
                {
                    this.Refresh();
                    if (this.m_IsOldOS)
                    {
                        return (ulong) this.m_MemoryStatus.dwAvailPhys;
                    }
                    return this.m_MemoryStatusEx.ullAvailPhys;
                }
            }

            internal ulong AvailableVirtualMemory
            {
                [SecurityCritical]
                get
                {
                    this.Refresh();
                    if (this.m_IsOldOS)
                    {
                        return (ulong) this.m_MemoryStatus.dwAvailVirtual;
                    }
                    return this.m_MemoryStatusEx.ullAvailVirtual;
                }
            }

            internal ulong TotalPhysicalMemory
            {
                [SecurityCritical]
                get
                {
                    this.Refresh();
                    if (this.m_IsOldOS)
                    {
                        return (ulong) this.m_MemoryStatus.dwTotalPhys;
                    }
                    return this.m_MemoryStatusEx.ullTotalPhys;
                }
            }

            internal ulong TotalVirtualMemory
            {
                [SecurityCritical]
                get
                {
                    this.Refresh();
                    if (this.m_IsOldOS)
                    {
                        return (ulong) this.m_MemoryStatus.dwTotalVirtual;
                    }
                    return this.m_MemoryStatusEx.ullTotalVirtual;
                }
            }
        }
    }
}

