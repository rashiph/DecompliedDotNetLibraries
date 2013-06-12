namespace System.Web.Configuration
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;

    [Serializable]
    public class AdapterDictionary : OrderedDictionary
    {
        public string this[string key]
        {
            get
            {
                return (string) base[key];
            }
            set
            {
                base[key] = value;
            }
        }
    }
}

