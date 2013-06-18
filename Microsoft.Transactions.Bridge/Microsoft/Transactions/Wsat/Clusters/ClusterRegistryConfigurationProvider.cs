namespace Microsoft.Transactions.Wsat.Clusters
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.ServiceModel.Diagnostics;
    using System.Text;

    internal class ClusterRegistryConfigurationProvider : ConfigurationProvider
    {
        private SafeHKey hKey;

        public ClusterRegistryConfigurationProvider(SafeHResource hResource)
        {
            this.hKey = Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.GetClusterResourceKey(hResource, RegistryRights.ExecuteKey);
            if (this.hKey.IsInvalid)
            {
                int num = Marshal.GetLastWin32Error();
                this.hKey.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("GetClusterResourceKeyFailed", new object[] { num })));
            }
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Opened cluster resource key");
            }
        }

        private ClusterRegistryConfigurationProvider(SafeHKey rootKey, string subKey)
        {
            if ((rootKey != null) && !rootKey.IsInvalid)
            {
                int num = Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.ClusterRegOpenKey(rootKey, subKey, RegistryRights.ExecuteKey, out this.hKey);
                if (num != 0L)
                {
                    Utility.CloseInvalidOutSafeHandle(this.hKey);
                    if (num != 2L)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("ClusterRegOpenKeyFailed", new object[] { num })));
                    }
                }
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "ClusterRegOpenKey for {0} returned {1}", subKey, num);
                }
            }
        }

        public override void Dispose()
        {
            if ((this.hKey != null) && !this.hKey.IsInvalid)
            {
                this.hKey.Dispose();
            }
        }

        private string GetStringFromMultiSz(string value, byte[] buffer, ref int index)
        {
            string str;
            int num = index;
            while ((index < (buffer.Length - 1)) && (BitConverter.ToChar(buffer, index) != '\0'))
            {
                index += 2;
            }
            if (num == index)
            {
                return null;
            }
            index += 2;
            try
            {
                str = Encoding.Unicode.GetString(buffer, num, (index - num) - 2);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("ClusterRegQueryValueInvalidResults", new object[] { value }), exception));
            }
            return str;
        }

        public override ConfigurationProvider OpenKey(string key)
        {
            return new ClusterRegistryConfigurationProvider(this.hKey, key);
        }

        private byte[] QueryValue(string value, RegistryValueKind valueType)
        {
            if ((this.hKey != null) && !this.hKey.IsInvalid)
            {
                RegistryValueKind kind;
                uint lpcbData = 0;
                int num2 = Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.ClusterRegQueryValue(this.hKey, value, out kind, null, ref lpcbData);
                switch (num2)
                {
                    case 0L:
                    case 0xeaL:
                    {
                        if (valueType != kind)
                        {
                            return null;
                        }
                        byte[] lpbData = new byte[lpcbData];
                        num2 = Microsoft.Transactions.Wsat.Clusters.SafeNativeMethods.ClusterRegQueryValue(this.hKey, value, out kind, lpbData, ref lpcbData);
                        if (num2 == 0L)
                        {
                            if (valueType != kind)
                            {
                                return null;
                            }
                            return lpbData;
                        }
                        break;
                    }
                }
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "ClusterRegQueryValue for {0} returned {1}", value, num2);
                }
                if (num2 != 2L)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("ClusterRegQueryValueFailed", new object[] { num2 })));
                }
            }
            return null;
        }

        public override int ReadInteger(string value, int defaultValue)
        {
            byte[] buffer = this.QueryValue(value, RegistryValueKind.DWord);
            if (buffer == null)
            {
                return defaultValue;
            }
            return (int) BitConverter.ToUInt32(buffer, 0);
        }

        public override string[] ReadMultiString(string value, string[] defaultValue)
        {
            string str;
            byte[] buffer = this.QueryValue(value, RegistryValueKind.MultiString);
            if (buffer == null)
            {
                return defaultValue;
            }
            List<string> list = new List<string>(5);
            int index = 0;
            while ((str = this.GetStringFromMultiSz(value, buffer, ref index)) != null)
            {
                list.Add(str);
            }
            return list.ToArray();
        }

        public override string ReadString(string value, string defaultValue)
        {
            string str;
            byte[] bytes = this.QueryValue(value, RegistryValueKind.String);
            if (bytes == null)
            {
                return defaultValue;
            }
            try
            {
                str = Encoding.Unicode.GetString(bytes, 0, bytes.Length - 2);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("ClusterRegQueryValueInvalidResults", new object[] { value }), exception));
            }
            return str;
        }
    }
}

