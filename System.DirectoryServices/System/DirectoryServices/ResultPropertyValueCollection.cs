namespace System.DirectoryServices
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ResultPropertyValueCollection : ReadOnlyCollectionBase
    {
        internal ResultPropertyValueCollection(object[] values)
        {
            if (values == null)
            {
                values = new object[0];
            }
            base.InnerList.AddRange(values);
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
                object obj2 = base.InnerList[index];
                if (obj2 is Exception)
                {
                    throw ((Exception) obj2);
                }
                return obj2;
            }
        }
    }
}

