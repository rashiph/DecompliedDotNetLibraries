namespace Microsoft.Internal.Performance
{
    using System;
    using System.Runtime.InteropServices;

    internal sealed class CodeMarkers
    {
        private const string AtomName = "VSCodeMarkersEnabled";
        private const string DllName = "Microsoft.Internal.Performance.CodeMarkers.dll";
        private bool fUseCodeMarkers = (NativeMethods.FindAtom("VSCodeMarkersEnabled") != 0);
        public static readonly CodeMarkers Instance = new CodeMarkers();

        private CodeMarkers()
        {
        }

        public void CodeMarker(CodeMarkerEvent nTimerID)
        {
            if (this.fUseCodeMarkers)
            {
                try
                {
                    NativeMethods.DllPerfCodeMarker((int) nTimerID, null, 0);
                }
                catch (DllNotFoundException)
                {
                    this.fUseCodeMarkers = false;
                }
            }
        }

        public void CodeMarkerEx(CodeMarkerEvent nTimerID, byte[] aBuff)
        {
            if (aBuff == null)
            {
                throw new ArgumentNullException("aBuff");
            }
            if (this.fUseCodeMarkers)
            {
                try
                {
                    NativeMethods.DllPerfCodeMarker((int) nTimerID, aBuff, aBuff.Length);
                }
                catch (DllNotFoundException)
                {
                    this.fUseCodeMarkers = false;
                }
            }
        }

        private static class NativeMethods
        {
            [DllImport("Microsoft.Internal.Performance.CodeMarkers.dll", EntryPoint="PerfCodeMarker")]
            public static extern void DllPerfCodeMarker(int nTimerID, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] aUserParams, int cbParams);
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
            public static extern ushort FindAtom(string lpString);
        }
    }
}

