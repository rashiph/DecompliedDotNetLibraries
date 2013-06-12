namespace System.StubHelpers
{
    using System;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class AnsiCharMarshaler
    {
        [ForceTokenStabilization]
        internal static char ConvertToManaged(byte nativeChar)
        {
            byte[] bytes = new byte[] { nativeChar };
            return Encoding.Default.GetString(bytes)[0];
        }

        [ForceTokenStabilization, SecurityCritical]
        internal static byte ConvertToNative(char managedChar, bool fBestFit, bool fThrowOnUnmappableChar)
        {
            int num;
            return DoAnsiConversion(managedChar.ToString(), fBestFit, fThrowOnUnmappableChar, out num)[0];
        }

        [SecurityCritical, ForceTokenStabilization]
        internal static byte[] DoAnsiConversion(string str, bool fBestFit, bool fThrowOnUnmappableChar, out int cbLength)
        {
            return str.ConvertToAnsi(Marshal.SystemMaxDBCSCharSize, fBestFit, fThrowOnUnmappableChar, out cbLength);
        }
    }
}

