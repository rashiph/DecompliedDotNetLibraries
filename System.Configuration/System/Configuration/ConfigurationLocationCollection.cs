namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ConfigurationLocationCollection : ReadOnlyCollectionBase
    {
        internal ConfigurationLocationCollection(ICollection col)
        {
            base.InnerList.AddRange(col);
        }

        public ConfigurationLocation this[int index]
        {
            get
            {
                return (ConfigurationLocation) base.InnerList[index];
            }
        }
    }
}

