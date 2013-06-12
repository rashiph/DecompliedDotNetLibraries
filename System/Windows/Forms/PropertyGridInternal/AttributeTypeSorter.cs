namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Collections;
    using System.Globalization;

    internal class AttributeTypeSorter : IComparer
    {
        private static IDictionary typeIds;

        public int Compare(object obj1, object obj2)
        {
            Attribute a = obj1 as Attribute;
            Attribute attribute2 = obj2 as Attribute;
            if ((a == null) && (attribute2 == null))
            {
                return 0;
            }
            if (a == null)
            {
                return -1;
            }
            if (attribute2 == null)
            {
                return 1;
            }
            return string.Compare(GetTypeIdString(a), GetTypeIdString(attribute2), false, CultureInfo.InvariantCulture);
        }

        private static string GetTypeIdString(Attribute a)
        {
            string str;
            object typeId = a.TypeId;
            if (typeId == null)
            {
                return "";
            }
            if (typeIds == null)
            {
                typeIds = new Hashtable();
                str = null;
            }
            else
            {
                str = typeIds[typeId] as string;
            }
            if (str == null)
            {
                str = typeId.ToString();
                typeIds[typeId] = str;
            }
            return str;
        }
    }
}

