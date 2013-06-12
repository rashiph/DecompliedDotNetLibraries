namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    internal class PerformanceMonitor
    {
        private string machineName;
        private RegistryKey perfDataKey;

        internal PerformanceMonitor(string machineName)
        {
            this.machineName = machineName;
            this.Init();
        }

        internal void Close()
        {
            if (this.perfDataKey != null)
            {
                this.perfDataKey.Close();
            }
            this.perfDataKey = null;
        }

        internal byte[] GetData(string item)
        {
            int num = 0x11;
            int millisecondsTimeout = 0;
            int error = 0;
            new RegistryPermission(PermissionState.Unrestricted).Assert();
            while (num > 0)
            {
                try
                {
                    return (byte[]) this.perfDataKey.GetValue(item);
                }
                catch (IOException exception)
                {
                    error = Marshal.GetHRForException(exception);
                    switch (error)
                    {
                        case 6:
                        case 0x6ba:
                        case 0x6be:
                            this.Init();
                            break;

                        case 0x15:
                        case 0xa7:
                        case 170:
                        case 0x102:
                            break;

                        default:
                            throw SharedUtils.CreateSafeWin32Exception(error);
                    }
                    num--;
                    if (millisecondsTimeout == 0)
                    {
                        millisecondsTimeout = 10;
                    }
                    else
                    {
                        Thread.Sleep(millisecondsTimeout);
                        millisecondsTimeout *= 2;
                    }
                    continue;
                }
                catch (InvalidCastException exception2)
                {
                    throw new InvalidOperationException(SR.GetString("CounterDataCorrupt", new object[] { this.perfDataKey.ToString() }), exception2);
                }
            }
            throw SharedUtils.CreateSafeWin32Exception(error);
        }

        private void Init()
        {
            try
            {
                if ((this.machineName != ".") && (string.Compare(this.machineName, PerformanceCounterLib.ComputerName, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                    this.perfDataKey = RegistryKey.OpenRemoteBaseKey(RegistryHive.PerformanceData, this.machineName);
                }
                else
                {
                    this.perfDataKey = Registry.PerformanceData;
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new Win32Exception(5);
            }
            catch (IOException exception)
            {
                throw new Win32Exception(Marshal.GetHRForException(exception));
            }
        }
    }
}

