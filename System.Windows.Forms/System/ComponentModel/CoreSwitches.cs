namespace System.ComponentModel
{
    using System;
    using System.Diagnostics;

    internal static class CoreSwitches
    {
        private static BooleanSwitch perfTrack;

        public static BooleanSwitch PerfTrack
        {
            get
            {
                if (perfTrack == null)
                {
                    perfTrack = new BooleanSwitch("PERFTRACK", "Debug performance critical sections.");
                }
                return perfTrack;
            }
        }
    }
}

