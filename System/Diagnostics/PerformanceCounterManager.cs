namespace System.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), Obsolete("This class has been deprecated.  Use the PerformanceCounters through the System.Diagnostics.PerformanceCounter class instead.  http://go.microsoft.com/fwlink/?linkid=14202"), Guid("82840BE1-D273-11D2-B94A-00600893B17A"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class PerformanceCounterManager : ICollectData
    {
        [Obsolete("This class has been deprecated.  Use the PerformanceCounters through the System.Diagnostics.PerformanceCounter class instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        void ICollectData.CloseData()
        {
        }

        [Obsolete("This class has been deprecated.  Use the PerformanceCounters through the System.Diagnostics.PerformanceCounter class instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        void ICollectData.CollectData(int callIdx, IntPtr valueNamePtr, IntPtr dataPtr, int totalBytes, out IntPtr res)
        {
            res = (IntPtr) (-1);
        }
    }
}

