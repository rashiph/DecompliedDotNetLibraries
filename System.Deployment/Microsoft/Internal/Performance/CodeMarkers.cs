namespace Microsoft.Internal.Performance
{
    using Microsoft.Win32;
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

        private static string GetPerformanceSubKey(RegistryKey hKey, string strRegRoot)
        {
            if (hKey == null)
            {
                return null;
            }
            string str = null;
            using (RegistryKey key = hKey.OpenSubKey(strRegRoot + @"\Performance"))
            {
                if (key != null)
                {
                    str = key.GetValue("").ToString();
                }
            }
            return str;
        }

        public void InitPerformanceDll(CodeMarkerApp iApp, string strRegRoot)
        {
            this.fUseCodeMarkers = false;
            if (UseCodeMarkers(strRegRoot))
            {
                try
                {
                    NativeMethods.AddAtom("VSCodeMarkersEnabled");
                    NativeMethods.DllInitPerf((int) iApp);
                    this.fUseCodeMarkers = true;
                }
                catch (DllNotFoundException)
                {
                }
            }
        }

        public void UninitializePerformanceDLL(CodeMarkerApp iApp)
        {
            if (this.fUseCodeMarkers)
            {
                this.fUseCodeMarkers = false;
                ushort atom = NativeMethods.FindAtom("VSCodeMarkersEnabled");
                if (atom != 0)
                {
                    NativeMethods.DeleteAtom(atom);
                }
                try
                {
                    NativeMethods.DllUnInitPerf((int) iApp);
                }
                catch (DllNotFoundException)
                {
                }
            }
        }

        private static bool UseCodeMarkers(string strRegRoot)
        {
            return !string.IsNullOrEmpty(GetPerformanceSubKey(Registry.LocalMachine, strRegRoot));
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
            public static extern ushort AddAtom(string lpString);
            [DllImport("kernel32.dll")]
            public static extern ushort DeleteAtom(ushort atom);
            [DllImport("Microsoft.Internal.Performance.CodeMarkers.dll", EntryPoint="InitPerf")]
            public static extern void DllInitPerf(int iApp);
            [DllImport("Microsoft.Internal.Performance.CodeMarkers.dll", EntryPoint="PerfCodeMarker")]
            public static extern void DllPerfCodeMarker(int nTimerID, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] byte[] aUserParams, int cbParams);
            [DllImport("Microsoft.Internal.Performance.CodeMarkers.dll", EntryPoint="UnInitPerf")]
            public static extern void DllUnInitPerf(int iApp);
            [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
            public static extern ushort FindAtom(string lpString);
        }
    }
}

