namespace Microsoft.Transactions.Wsat.Clusters
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class ClusterUtils
    {
        private static SafeHResource GetResourceFromEnumeration(SafeHCluster hCluster, SafeHClusEnum hEnum, uint index)
        {
            uint num;
            uint lpcchName = 0;
            uint num3 = SafeNativeMethods.ClusterEnum(hEnum, index, out num, null, ref lpcchName);
            switch (num3)
            {
                case 0x103:
                    return null;

                case 0:
                case 0xea:
                {
                    StringBuilder lpszName = new StringBuilder((int) (++lpcchName));
                    num3 = SafeNativeMethods.ClusterEnum(hEnum, index, out num, lpszName, ref lpcchName);
                    if (num3 == 0)
                    {
                        string str = lpszName.ToString();
                        if (DebugTrace.Verbose)
                        {
                            DebugTrace.Trace(TraceLevel.Verbose, "Opening cluster resource {0}", str);
                        }
                        SafeHResource resource = SafeNativeMethods.OpenClusterResource(hCluster, str);
                        if (resource.IsInvalid)
                        {
                            int num4 = Marshal.GetLastWin32Error();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("OpenClusterResourceFailed", new object[] { num4 })));
                        }
                        return resource;
                    }
                    break;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("ClusterEnumFailed", new object[] { num3 })));
        }

        private static string GetResourceNetworkName(SafeHResource hResource)
        {
            uint nSize = 0x40;
            StringBuilder lpBuffer = new StringBuilder((int) nSize);
            bool flag = SafeNativeMethods.GetClusterResourceNetworkName(hResource, lpBuffer, ref nSize);
            int num2 = Marshal.GetLastWin32Error();
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("GetClusterResourceNetworkNameFailed", new object[] { num2 })));
            }
            return lpBuffer.ToString();
        }

        private static string GetResourceType(SafeHResource hResource)
        {
            return IssueClusterResourceControlString(hResource, ClusterResourceControlCode.GetResourceType);
        }

        public static SafeHResource GetTransactionManagerClusterResource(string virtualServerName, string transactionManagerResourceType)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.Trace(TraceLevel.Info, "Looking for cluster resource of type {0} dependent on network name {1}", transactionManagerResourceType, virtualServerName);
            }
            SafeHCluster hCluster = SafeNativeMethods.OpenCluster(null);
            if (hCluster.IsInvalid)
            {
                int num = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("OpenClusterFailed", new object[] { num })));
            }
            using (hCluster)
            {
                SafeHClusEnum hEnum = SafeNativeMethods.ClusterOpenEnum(hCluster, ClusterEnum.Resource);
                if (hEnum.IsInvalid)
                {
                    int num2 = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("ClusterOpenEnumFailed", new object[] { num2 })));
                }
                using (hEnum)
                {
                    uint index = 0;
                    while (true)
                    {
                        SafeHResource hResource = GetResourceFromEnumeration(hCluster, hEnum, index);
                        if (hResource == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("ClusterResourceNotFound", new object[] { virtualServerName })));
                        }
                        try
                        {
                            if (IsTransactionManager(hResource, virtualServerName, transactionManagerResourceType))
                            {
                                return hResource;
                            }
                        }
                        catch
                        {
                            hResource.Dispose();
                            throw;
                        }
                        index++;
                    }
                }
            }
        }

        private static byte[] IssueClusterResourceControl(SafeHResource hResource, ClusterResourceControlCode code)
        {
            uint lpcbBytesReturned = 0;
            uint num2 = SafeNativeMethods.ClusterResourceControl(hResource, IntPtr.Zero, code, IntPtr.Zero, 0, null, 0, ref lpcbBytesReturned);
            switch (num2)
            {
                case 0:
                case 0xea:
                {
                    byte[] buffer = new byte[lpcbBytesReturned];
                    num2 = SafeNativeMethods.ClusterResourceControl(hResource, IntPtr.Zero, code, IntPtr.Zero, 0, buffer, lpcbBytesReturned, ref lpcbBytesReturned);
                    if (num2 == 0)
                    {
                        return buffer;
                    }
                    break;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("ClusterResourceControlFailed", new object[] { code, num2 })));
        }

        private static string IssueClusterResourceControlString(SafeHResource hResource, ClusterResourceControlCode code)
        {
            string str;
            byte[] bytes = IssueClusterResourceControl(hResource, code);
            try
            {
                str = Encoding.Unicode.GetString(bytes, 0, bytes.Length - 2);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("ClusterResourceControlInvalidResults", new object[] { code }), exception));
            }
            return str;
        }

        private static bool IsTransactionManager(SafeHResource hResource, string virtualServerName, string transactionManagerResourceType)
        {
            string resourceType = GetResourceType(hResource);
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Examining cluster resource of type {0}", resourceType);
            }
            if (string.Compare(resourceType, transactionManagerResourceType, StringComparison.OrdinalIgnoreCase) == 0)
            {
                string resourceNetworkName = GetResourceNetworkName(hResource);
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dependent network name is {0}", resourceNetworkName);
                }
                if (string.Compare(resourceNetworkName, virtualServerName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

