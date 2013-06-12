namespace System.Collections.Specialized
{
    using System;
    using System.Collections;

    public class CollectionsUtil
    {
        public static Hashtable CreateCaseInsensitiveHashtable()
        {
            return new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        }

        public static Hashtable CreateCaseInsensitiveHashtable(IDictionary d)
        {
            return new Hashtable(d, StringComparer.CurrentCultureIgnoreCase);
        }

        public static Hashtable CreateCaseInsensitiveHashtable(int capacity)
        {
            return new Hashtable(capacity, StringComparer.CurrentCultureIgnoreCase);
        }

        public static SortedList CreateCaseInsensitiveSortedList()
        {
            return new SortedList(CaseInsensitiveComparer.Default);
        }
    }
}

