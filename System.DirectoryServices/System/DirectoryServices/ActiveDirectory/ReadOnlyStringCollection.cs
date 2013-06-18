namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ReadOnlyStringCollection : ReadOnlyCollectionBase
    {
        internal ReadOnlyStringCollection()
        {
        }

        internal ReadOnlyStringCollection(ArrayList values)
        {
            if (values == null)
            {
                values = new ArrayList();
            }
            base.InnerList.AddRange(values);
        }

        internal void Add(string value)
        {
            base.InnerList.Add(value);
        }

        public bool Contains(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                string str = (string) base.InnerList[i];
                if (Utils.Compare(str, value) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(string[] values, int index)
        {
            base.InnerList.CopyTo(values, index);
        }

        public int IndexOf(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                string str = (string) base.InnerList[i];
                if (Utils.Compare(str, value) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public string this[int index]
        {
            get
            {
                object obj2 = base.InnerList[index];
                if (obj2 is Exception)
                {
                    throw ((Exception) obj2);
                }
                return (string) obj2;
            }
        }
    }
}

