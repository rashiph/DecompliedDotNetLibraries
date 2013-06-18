namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    internal class NegotiationInfoClass
    {
        internal string AuthenticationPackage;
        internal const string Kerberos = "Kerberos";
        internal const string NTLM = "NTLM";

        internal NegotiationInfoClass(SafeHandle safeHandle, int negotiationState)
        {
            if (!safeHandle.IsInvalid)
            {
                IntPtr handle = safeHandle.DangerousGetHandle();
                if ((negotiationState == 0) || (negotiationState == 1))
                {
                    IntPtr ptr = Marshal.ReadIntPtr(handle, SecurityPackageInfo.NameOffest);
                    string strA = null;
                    if (ptr != IntPtr.Zero)
                    {
                        strA = Marshal.PtrToStringUni(ptr);
                    }
                    if (string.Compare(strA, "Kerberos", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.AuthenticationPackage = "Kerberos";
                    }
                    else if (string.Compare(strA, "NTLM", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.AuthenticationPackage = "NTLM";
                    }
                    else
                    {
                        this.AuthenticationPackage = strA;
                    }
                }
            }
        }
    }
}

