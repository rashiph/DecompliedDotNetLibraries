namespace System.Net
{
    using System;
    using System.Collections;
    using System.Reflection;

    [Serializable]
    internal class PathList
    {
        private SortedList m_list = SortedList.Synchronized(new SortedList(PathListComparer.StaticInstance));

        public int GetCookiesCount()
        {
            int num = 0;
            foreach (CookieCollection cookies in this.m_list.Values)
            {
                num += cookies.Count;
            }
            return num;
        }

        public IEnumerator GetEnumerator()
        {
            return this.m_list.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.m_list.Count;
            }
        }

        public object this[string s]
        {
            get
            {
                return this.m_list[s];
            }
            set
            {
                this.m_list[s] = value;
            }
        }

        public ICollection Values
        {
            get
            {
                return this.m_list.Values;
            }
        }

        [Serializable]
        private class PathListComparer : IComparer
        {
            internal static readonly PathList.PathListComparer StaticInstance = new PathList.PathListComparer();

            int IComparer.Compare(object ol, object or)
            {
                string str = CookieParser.CheckQuoted((string) ol);
                string str2 = CookieParser.CheckQuoted((string) or);
                int length = str.Length;
                int num2 = str2.Length;
                int num3 = Math.Min(length, num2);
                for (int i = 0; i < num3; i++)
                {
                    if (str[i] != str2[i])
                    {
                        return (str[i] - str2[i]);
                    }
                }
                return (num2 - length);
            }
        }
    }
}

