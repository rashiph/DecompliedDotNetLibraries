namespace System.Net
{
    using System;
    using System.Collections;

    internal class Comparer : IComparer
    {
        int IComparer.Compare(object ol, object or)
        {
            Cookie cookie = (Cookie) ol;
            Cookie cookie2 = (Cookie) or;
            int num = string.Compare(cookie.Name, cookie2.Name, StringComparison.OrdinalIgnoreCase);
            if (num != 0)
            {
                return num;
            }
            num = string.Compare(cookie.Domain, cookie2.Domain, StringComparison.OrdinalIgnoreCase);
            if (num != 0)
            {
                return num;
            }
            num = string.Compare(cookie.Path, cookie2.Path, StringComparison.Ordinal);
            if (num != 0)
            {
                return num;
            }
            return 0;
        }
    }
}

