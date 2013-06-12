namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class ComponentResourceManager : ResourceManager
    {
        private CultureInfo _neutralResourcesCulture;
        private Hashtable _resourceSets;

        public ComponentResourceManager()
        {
        }

        public ComponentResourceManager(Type t) : base(t)
        {
        }

        public void ApplyResources(object value, string objectName)
        {
            this.ApplyResources(value, objectName, null);
        }

        public virtual void ApplyResources(object value, string objectName, CultureInfo culture)
        {
            SortedList<string, object> list;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (objectName == null)
            {
                throw new ArgumentNullException("objectName");
            }
            if (culture == null)
            {
                culture = CultureInfo.CurrentUICulture;
            }
            if (this._resourceSets == null)
            {
                ResourceSet set;
                this._resourceSets = new Hashtable();
                list = this.FillResources(culture, out set);
                this._resourceSets[culture] = list;
            }
            else
            {
                list = (SortedList<string, object>) this._resourceSets[culture];
                if ((list == null) || (list.Comparer.Equals(StringComparer.OrdinalIgnoreCase) != this.IgnoreCase))
                {
                    ResourceSet set2;
                    list = this.FillResources(culture, out set2);
                    this._resourceSets[culture] = list;
                }
            }
            BindingFlags bindingAttr = BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance;
            if (this.IgnoreCase)
            {
                bindingAttr |= BindingFlags.IgnoreCase;
            }
            bool flag = false;
            if (value is IComponent)
            {
                ISite site = ((IComponent) value).Site;
                if ((site != null) && site.DesignMode)
                {
                    flag = true;
                }
            }
            foreach (KeyValuePair<string, object> pair in list)
            {
                int num;
                string key = pair.Key;
                if (key == null)
                {
                    continue;
                }
                if (this.IgnoreCase)
                {
                    if (string.Compare(key, 0, objectName, 0, objectName.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        goto Label_012B;
                    }
                    continue;
                }
                if (string.CompareOrdinal(key, 0, objectName, 0, objectName.Length) != 0)
                {
                    continue;
                }
            Label_012B:
                num = objectName.Length;
                if ((key.Length > num) && (key[num] == '.'))
                {
                    string name = key.Substring(num + 1);
                    if (flag)
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(value).Find(name, this.IgnoreCase);
                        if (((descriptor != null) && !descriptor.IsReadOnly) && ((pair.Value == null) || descriptor.PropertyType.IsInstanceOfType(pair.Value)))
                        {
                            descriptor.SetValue(value, pair.Value);
                        }
                    }
                    else
                    {
                        PropertyInfo property = null;
                        try
                        {
                            property = value.GetType().GetProperty(name, bindingAttr);
                        }
                        catch (AmbiguousMatchException)
                        {
                            Type baseType = value.GetType();
                            do
                            {
                                property = baseType.GetProperty(name, bindingAttr | BindingFlags.DeclaredOnly);
                                baseType = baseType.BaseType;
                            }
                            while (((property == null) && (baseType != null)) && (baseType != typeof(object)));
                        }
                        if (((property != null) && property.CanWrite) && ((pair.Value == null) || property.PropertyType.IsInstanceOfType(pair.Value)))
                        {
                            property.SetValue(value, pair.Value, null);
                        }
                    }
                }
            }
        }

        private SortedList<string, object> FillResources(CultureInfo culture, out ResourceSet resourceSet)
        {
            SortedList<string, object> list;
            ResourceSet set = null;
            if (!culture.Equals(CultureInfo.InvariantCulture) && !culture.Equals(this.NeutralResourcesCulture))
            {
                list = this.FillResources(culture.Parent, out set);
            }
            else if (this.IgnoreCase)
            {
                list = new SortedList<string, object>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                list = new SortedList<string, object>(StringComparer.Ordinal);
            }
            resourceSet = this.GetResourceSet(culture, true, true);
            if ((resourceSet != null) && !object.ReferenceEquals(resourceSet, set))
            {
                foreach (DictionaryEntry entry in resourceSet)
                {
                    list[(string) entry.Key] = entry.Value;
                }
            }
            return list;
        }

        private CultureInfo NeutralResourcesCulture
        {
            get
            {
                if ((this._neutralResourcesCulture == null) && (base.MainAssembly != null))
                {
                    this._neutralResourcesCulture = ResourceManager.GetNeutralResourcesLanguage(base.MainAssembly);
                }
                return this._neutralResourcesCulture;
            }
        }
    }
}

