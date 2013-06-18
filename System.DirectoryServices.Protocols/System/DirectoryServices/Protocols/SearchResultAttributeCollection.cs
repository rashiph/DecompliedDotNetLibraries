namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;

    public class SearchResultAttributeCollection : DictionaryBase
    {
        internal SearchResultAttributeCollection()
        {
        }

        internal void Add(string name, DirectoryAttribute value)
        {
            base.Dictionary.Add(name.ToLower(CultureInfo.InvariantCulture), value);
        }

        public bool Contains(string attributeName)
        {
            if (attributeName == null)
            {
                throw new ArgumentNullException("attributeName");
            }
            object key = attributeName.ToLower(CultureInfo.InvariantCulture);
            return base.Dictionary.Contains(key);
        }

        public void CopyTo(DirectoryAttribute[] array, int index)
        {
            base.Dictionary.Values.CopyTo(array, index);
        }

        public ICollection AttributeNames
        {
            get
            {
                return base.Dictionary.Keys;
            }
        }

        public DirectoryAttribute this[string attributeName]
        {
            get
            {
                if (attributeName == null)
                {
                    throw new ArgumentNullException("attributeName");
                }
                object obj2 = attributeName.ToLower(CultureInfo.InvariantCulture);
                return (DirectoryAttribute) base.InnerHashtable[obj2];
            }
        }

        public ICollection Values
        {
            get
            {
                return base.Dictionary.Values;
            }
        }
    }
}

