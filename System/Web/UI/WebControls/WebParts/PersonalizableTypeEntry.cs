namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;

    internal sealed class PersonalizableTypeEntry
    {
        private IDictionary _propertyEntries;
        private PropertyInfo[] _propertyInfos;
        private Type _type;

        public PersonalizableTypeEntry(Type type)
        {
            this._type = type;
            this.InitializePersonalizableProperties();
        }

        private void InitializePersonalizableProperties()
        {
            this._propertyEntries = new HybridDictionary(false);
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            PropertyInfo[] properties = this._type.GetProperties(bindingAttr);
            Array.Sort(properties, new DeclaringTypeComparer());
            if ((properties != null) && (properties.Length != 0))
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo element = properties[i];
                    string name = element.Name;
                    PersonalizableAttribute attr = Attribute.GetCustomAttribute(element, PersonalizableAttribute.PersonalizableAttributeType, true) as PersonalizableAttribute;
                    if ((attr == null) || !attr.IsPersonalizable)
                    {
                        this._propertyEntries.Remove(name);
                    }
                    else
                    {
                        ParameterInfo[] indexParameters = element.GetIndexParameters();
                        if (((indexParameters != null) && (indexParameters.Length > 0)) || ((element.GetGetMethod() == null) || (element.GetSetMethod() == null)))
                        {
                            throw new HttpException(System.Web.SR.GetString("PersonalizableTypeEntry_InvalidProperty", new object[] { name, this._type.FullName }));
                        }
                        this._propertyEntries[name] = new PersonalizablePropertyEntry(element, attr);
                    }
                }
            }
        }

        public IDictionary PropertyEntries
        {
            get
            {
                return this._propertyEntries;
            }
        }

        public ICollection PropertyInfos
        {
            get
            {
                if (this._propertyInfos == null)
                {
                    PropertyInfo[] infoArray = new PropertyInfo[this._propertyEntries.Count];
                    int index = 0;
                    foreach (PersonalizablePropertyEntry entry in this._propertyEntries.Values)
                    {
                        infoArray[index] = entry.PropertyInfo;
                        index++;
                    }
                    this._propertyInfos = infoArray;
                }
                return this._propertyInfos;
            }
        }

        private sealed class DeclaringTypeComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                Type declaringType = ((PropertyInfo) x).DeclaringType;
                Type c = ((PropertyInfo) y).DeclaringType;
                if (declaringType == c)
                {
                    return 0;
                }
                if (declaringType.IsSubclassOf(c))
                {
                    return 1;
                }
                return -1;
            }
        }
    }
}

