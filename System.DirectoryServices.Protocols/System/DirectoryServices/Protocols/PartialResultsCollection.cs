namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class PartialResultsCollection : ReadOnlyCollectionBase
    {
        internal PartialResultsCollection()
        {
        }

        internal int Add(object value)
        {
            return base.InnerList.Add(value);
        }

        public bool Contains(object value)
        {
            return base.InnerList.Contains(value);
        }

        public void CopyTo(object[] values, int index)
        {
            base.InnerList.CopyTo(values, index);
        }

        public int IndexOf(object value)
        {
            return base.InnerList.IndexOf(value);
        }

        public object this[int index]
        {
            get
            {
                return base.InnerList[index];
            }
        }
    }
}

