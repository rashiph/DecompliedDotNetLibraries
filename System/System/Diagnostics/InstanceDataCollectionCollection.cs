namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;

    public class InstanceDataCollectionCollection : DictionaryBase
    {
        internal void Add(string counterName, InstanceDataCollection value)
        {
            object key = counterName.ToLower(CultureInfo.InvariantCulture);
            base.Dictionary.Add(key, value);
        }

        public bool Contains(string counterName)
        {
            if (counterName == null)
            {
                throw new ArgumentNullException("counterName");
            }
            object key = counterName.ToLower(CultureInfo.InvariantCulture);
            return base.Dictionary.Contains(key);
        }

        public void CopyTo(InstanceDataCollection[] counters, int index)
        {
            base.Dictionary.Values.CopyTo(counters, index);
        }

        public InstanceDataCollection this[string counterName]
        {
            get
            {
                if (counterName == null)
                {
                    throw new ArgumentNullException("counterName");
                }
                object obj2 = counterName.ToLower(CultureInfo.InvariantCulture);
                return (InstanceDataCollection) base.Dictionary[obj2];
            }
        }

        public ICollection Keys
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

