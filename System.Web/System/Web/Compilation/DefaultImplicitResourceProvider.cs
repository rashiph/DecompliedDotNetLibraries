namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Resources;

    internal class DefaultImplicitResourceProvider : IImplicitResourceProvider
    {
        private bool _attemptedGetPageResources;
        private IDictionary _implicitResources;
        private IResourceProvider _resourceProvider;

        internal DefaultImplicitResourceProvider(IResourceProvider resourceProvider)
        {
            this._resourceProvider = resourceProvider;
        }

        private static string ConstructFullKey(ImplicitResourceKey entry)
        {
            string str = entry.KeyPrefix + "." + entry.Property;
            if (entry.Filter.Length > 0)
            {
                str = entry.Filter + ":" + str;
            }
            return str;
        }

        internal void EnsureGetPageResources()
        {
            if (!this._attemptedGetPageResources)
            {
                this._attemptedGetPageResources = true;
                IResourceReader resourceReader = this._resourceProvider.ResourceReader;
                if (resourceReader != null)
                {
                    this._implicitResources = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    foreach (DictionaryEntry entry in resourceReader)
                    {
                        ImplicitResourceKey key = ParseFullKey((string) entry.Key);
                        if (key != null)
                        {
                            ArrayList list = (ArrayList) this._implicitResources[key.KeyPrefix];
                            if (list == null)
                            {
                                list = new ArrayList();
                                this._implicitResources[key.KeyPrefix] = list;
                            }
                            list.Add(key);
                        }
                    }
                }
            }
        }

        public virtual ICollection GetImplicitResourceKeys(string keyPrefix)
        {
            this.EnsureGetPageResources();
            if (this._implicitResources == null)
            {
                return null;
            }
            return (ICollection) this._implicitResources[keyPrefix];
        }

        public virtual object GetObject(ImplicitResourceKey entry, CultureInfo culture)
        {
            string resourceKey = ConstructFullKey(entry);
            return this._resourceProvider.GetObject(resourceKey, culture);
        }

        private static ImplicitResourceKey ParseFullKey(string key)
        {
            string str = string.Empty;
            if (key.IndexOf(':') > 0)
            {
                string[] strArray = key.Split(new char[] { ':' });
                if (strArray.Length > 2)
                {
                    return null;
                }
                str = strArray[0];
                key = strArray[1];
            }
            int index = key.IndexOf('.');
            if (index <= 0)
            {
                return null;
            }
            string str2 = key.Substring(0, index);
            string str3 = key.Substring(index + 1);
            return new ImplicitResourceKey { Filter = str, KeyPrefix = str2, Property = str3 };
        }
    }
}

