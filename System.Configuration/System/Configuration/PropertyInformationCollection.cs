namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class PropertyInformationCollection : NameObjectCollectionBase
    {
        private ConfigurationElement ThisElement;

        internal PropertyInformationCollection(ConfigurationElement thisElement) : base(StringComparer.Ordinal)
        {
            this.ThisElement = thisElement;
            foreach (ConfigurationProperty property in this.ThisElement.Properties)
            {
                if (property.Name != this.ThisElement.ElementTagName)
                {
                    base.BaseAdd(property.Name, new PropertyInformation(thisElement, property.Name));
                }
            }
            base.IsReadOnly = true;
        }

        public void CopyTo(PropertyInformation[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Length < (this.Count + index))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            foreach (PropertyInformation information in this)
            {
                array[index++] = information;
            }
        }

        public override IEnumerator GetEnumerator()
        {
            int count = this.Count;
            int iteratorVariable1 = 0;
            while (true)
            {
                if (iteratorVariable1 >= count)
                {
                    yield break;
                }
                yield return this[iteratorVariable1];
                iteratorVariable1++;
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public PropertyInformation this[string propertyName]
        {
            get
            {
                PropertyInformation information = (PropertyInformation) base.BaseGet(propertyName);
                if (information == null)
                {
                    PropertyInformation information2 = (PropertyInformation) base.BaseGet(ConfigurationProperty.DefaultCollectionPropertyName);
                    if ((information2 != null) && (information2.ProvidedName == propertyName))
                    {
                        information = information2;
                    }
                }
                return information;
            }
        }

        internal PropertyInformation this[int index]
        {
            get
            {
                return (PropertyInformation) base.BaseGet(base.BaseGetKey(index));
            }
        }

    }
}

