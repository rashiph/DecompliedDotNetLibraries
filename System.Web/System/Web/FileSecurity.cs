namespace System.Web
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Web.Util;

    internal sealed class FileSecurity
    {
        private const int DACL_INFORMATION = 7;
        private static Hashtable s_interned = new Hashtable(0, 1f, new DaclComparer());
        private static byte[] s_nullDacl = new byte[0];

        internal static byte[] GetDacl(string filename)
        {
            int lengthNeeded = 0;
            int num2 = UnsafeNativeMethods.GetFileSecurity(filename, 7, null, 0, ref lengthNeeded);
            int lastError = Marshal.GetLastWin32Error();
            if (num2 != 0)
            {
                return s_nullDacl;
            }
            if (HttpException.HResultFromLastError(lastError) != -2147024774)
            {
                return null;
            }
            byte[] securityDescriptor = new byte[lengthNeeded];
            if (UnsafeNativeMethods.GetFileSecurity(filename, 7, securityDescriptor, securityDescriptor.Length, ref lengthNeeded) == 0)
            {
                return null;
            }
            byte[] buffer2 = (byte[]) s_interned[securityDescriptor];
            if (buffer2 == null)
            {
                lock (s_interned.SyncRoot)
                {
                    buffer2 = (byte[]) s_interned[securityDescriptor];
                    if (buffer2 == null)
                    {
                        buffer2 = securityDescriptor;
                        s_interned[buffer2] = buffer2;
                    }
                }
            }
            return buffer2;
        }

        private class DaclComparer : IEqualityComparer
        {
            private int Compare(byte[] a, byte[] b)
            {
                int num = a.Length - b.Length;
                for (int i = 0; (num == 0) && (i < a.Length); i++)
                {
                    num = a[i] - b[i];
                }
                return num;
            }

            bool IEqualityComparer.Equals(object x, object y)
            {
                if ((x == null) && (y == null))
                {
                    return true;
                }
                if ((x == null) || (y == null))
                {
                    return false;
                }
                byte[] a = x as byte[];
                byte[] b = y as byte[];
                return (((a != null) && (b != null)) && (this.Compare(a, b) == 0));
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                byte[] buffer = (byte[]) obj;
                HashCodeCombiner combiner = new HashCodeCombiner();
                foreach (byte num in buffer)
                {
                    combiner.AddObject(num);
                }
                return combiner.CombinedHash32;
            }
        }
    }
}

