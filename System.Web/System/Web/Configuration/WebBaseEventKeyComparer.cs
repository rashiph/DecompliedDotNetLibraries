namespace System.Web.Configuration
{
    using System;
    using System.Collections;

    internal class WebBaseEventKeyComparer : IEqualityComparer
    {
        public int Compare(object x, object y)
        {
            CustomWebEventKey key = (CustomWebEventKey) x;
            CustomWebEventKey key2 = (CustomWebEventKey) y;
            int num = key._eventCode;
            int num2 = key2._eventCode;
            if (num == num2)
            {
                Type type = key._type;
                Type o = key2._type;
                if (type.Equals(o))
                {
                    return 0;
                }
                return Comparer.Default.Compare(type.ToString(), o.ToString());
            }
            if (num > num2)
            {
                return 1;
            }
            return -1;
        }

        public bool Equals(object x, object y)
        {
            CustomWebEventKey key = (CustomWebEventKey) x;
            CustomWebEventKey key2 = (CustomWebEventKey) y;
            return ((key._eventCode == key2._eventCode) && key._type.Equals(key2._type));
        }

        public int GetHashCode(object obj)
        {
            return (((CustomWebEventKey) obj)._eventCode ^ ((CustomWebEventKey) obj)._type.GetHashCode());
        }
    }
}

