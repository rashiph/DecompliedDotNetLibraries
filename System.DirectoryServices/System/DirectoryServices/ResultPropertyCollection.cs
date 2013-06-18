namespace System.DirectoryServices
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;

    public class ResultPropertyCollection : DictionaryBase
    {
        internal ResultPropertyCollection()
        {
        }

        internal void Add(string name, ResultPropertyValueCollection value)
        {
            base.Dictionary.Add(name.ToLower(CultureInfo.InvariantCulture), value);
        }

        public bool Contains(string propertyName)
        {
            object key = propertyName.ToLower(CultureInfo.InvariantCulture);
            return base.Dictionary.Contains(key);
        }

        public void CopyTo(ResultPropertyValueCollection[] array, int index)
        {
            base.Dictionary.Values.CopyTo(array, index);
        }

        public ResultPropertyValueCollection this[string name]
        {
            get
            {
                object obj2 = name.ToLower(CultureInfo.InvariantCulture);
                if (this.Contains((string) obj2))
                {
                    return (ResultPropertyValueCollection) base.InnerHashtable[obj2];
                }
                return new ResultPropertyValueCollection(new object[0]);
            }
        }

        public ICollection PropertyNames
        {
            get
            {
                return base.Dictionary.Keys;
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

