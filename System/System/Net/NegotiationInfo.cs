namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NegotiationInfo
    {
        internal IntPtr PackageInfo;
        internal uint NegotiationState;
        internal static readonly int Size;
        internal static readonly int NegotiationStateOffest;
        static NegotiationInfo()
        {
            Size = Marshal.SizeOf(typeof(NegotiationInfo));
            NegotiationStateOffest = (int) Marshal.OffsetOf(typeof(NegotiationInfo), "NegotiationState");
        }
    }
}

