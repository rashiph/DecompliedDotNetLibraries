namespace System.Web.DataAccess
{
    using System;
    using System.Security.Principal;
    using System.Text;
    using System.Web;

    internal static class DataConnectionHelper
    {
        internal static string GetCurrentName()
        {
            string str = "NETWORK SERVICE";
            string str2 = "NT AUTHORITY";
            IntPtr zero = IntPtr.Zero;
            try
            {
                if ((UnsafeNativeMethods.ConvertStringSidToSid("S-1-5-20", out zero) != 0) && (zero != IntPtr.Zero))
                {
                    int capacity = 0x100;
                    int num2 = 0x100;
                    int eUse = 0;
                    StringBuilder szName = new StringBuilder(capacity);
                    StringBuilder szDomain = new StringBuilder(num2);
                    if (UnsafeNativeMethods.LookupAccountSid(null, zero, szName, ref capacity, szDomain, ref num2, ref eUse) != 0)
                    {
                        str = szName.ToString();
                        str2 = szDomain.ToString();
                    }
                }
                WindowsIdentity current = WindowsIdentity.GetCurrent();
                if ((current != null) && (current.Name != null))
                {
                    if (string.Compare(current.Name, str2 + @"\" + str, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return str;
                    }
                    return current.Name;
                }
            }
            catch
            {
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    UnsafeNativeMethods.LocalFree(zero);
                }
            }
            return string.Empty;
        }
    }
}

