namespace Microsoft.Transactions.Wsat.Clusters
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Text;

    internal class SafeNativeMethods
    {
        public const string ClusApi = "clusapi.dll";
        public const uint ERROR_FILE_NOT_FOUND = 2;
        public const uint ERROR_MORE_DATA = 0xea;
        public const uint ERROR_NO_MORE_ITEMS = 0x103;
        public const uint ERROR_SUCCESS = 0;

        [DllImport("clusapi.dll", CharSet=CharSet.Unicode)]
        public static extern uint ClusterEnum([In] SafeHClusEnum hEnum, [In] uint dwIndex, out uint lpdwType, [Out] StringBuilder lpszName, [In, Out] ref uint lpcchName);
        [DllImport("clusapi.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern SafeHClusEnum ClusterOpenEnum([In] SafeHCluster hCluster, [In] Microsoft.Transactions.Wsat.Clusters.ClusterEnum dwType);
        [DllImport("clusapi.dll", CharSet=CharSet.Unicode)]
        public static extern int ClusterRegOpenKey([In] SafeHKey hKey, [In, MarshalAs(UnmanagedType.LPWStr)] string lpszSubKey, [In] RegistryRights samDesired, out SafeHKey phkResult);
        [DllImport("clusapi.dll", CharSet=CharSet.Unicode)]
        public static extern int ClusterRegQueryValue([In] SafeHKey hKey, [In, MarshalAs(UnmanagedType.LPWStr)] string lpszValueName, out RegistryValueKind lpdwValueType, [In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] lpbData, [In, Out] ref uint lpcbData);
        [DllImport("clusapi.dll", CharSet=CharSet.Unicode)]
        public static extern uint ClusterResourceControl([In] SafeHResource hResource, [In] IntPtr hHostNode, [In] ClusterResourceControlCode dwControlCode, [In] IntPtr lpInBuffer, [In] uint cbInBufferSize, [In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] buffer, [In] uint cbOutBufferSize, [In, Out] ref uint lpcbBytesReturned);
        [DllImport("clusapi.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern SafeHKey GetClusterResourceKey([In] SafeHResource hResource, [In] RegistryRights samDesired);
        [DllImport("clusapi.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern bool GetClusterResourceNetworkName([In] SafeHResource hResource, [Out] StringBuilder lpBuffer, [In, Out] ref uint nSize);
        [DllImport("clusapi.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern SafeHCluster OpenCluster([In, MarshalAs(UnmanagedType.LPWStr)] string lpszClusterName);
        [DllImport("clusapi.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern SafeHResource OpenClusterResource([In] SafeHCluster hCluster, [In, MarshalAs(UnmanagedType.LPWStr)] string lpszResourceName);
    }
}

