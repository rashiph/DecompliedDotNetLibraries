namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, Synchronization=true)]
    public class AttributeCollection : ICollection, IEnumerable
    {
        private Attribute[] _attributes;
        private static Hashtable _defaultAttributes;
        private AttributeEntry[] _foundAttributeTypes;
        private int _index;
        public static readonly AttributeCollection Empty = new AttributeCollection(null);
        private const int FOUND_TYPES_LIMIT = 5;
        private static object internalSyncObject = new object();

        protected AttributeCollection()
        {
        }

        public AttributeCollection(params Attribute[] attributes)
        {
            if (attributes == null)
            {
                attributes = new Attribute[0];
            }
            this._attributes = attributes;
            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] == null)
                {
                    throw new ArgumentNullException("attributes");
                }
            }
        }

        public bool Contains(Attribute attribute)
        {
            Attribute attribute2 = this[attribute.GetType()];
            return ((attribute2 != null) && attribute2.Equals(attribute));
        }

        public bool Contains(Attribute[] attributes)
        {
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (!this.Contains(attributes[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void CopyTo(Array array, int index)
        {
            Array.Copy(this.Attributes, 0, array, index, this.Attributes.Length);
        }

        public static AttributeCollection FromExisting(AttributeCollection existing, params Attribute[] newAttributes)
        {
            if (existing == null)
            {
                throw new ArgumentNullException("existing");
            }
            if (newAttributes == null)
            {
                newAttributes = new Attribute[0];
            }
            Attribute[] array = new Attribute[existing.Count + newAttributes.Length];
            int count = existing.Count;
            existing.CopyTo(array, 0);
            for (int i = 0; i < newAttributes.Length; i++)
            {
                if (newAttributes[i] == null)
                {
                    throw new ArgumentNullException("newAttributes");
                }
                bool flag = false;
                for (int j = 0; j < existing.Count; j++)
                {
                    if (array[j].TypeId.Equals(newAttributes[i].TypeId))
                    {
                        flag = true;
                        array[j] = newAttributes[i];
                        break;
                    }
                }
                if (!flag)
                {
                    array[count++] = newAttributes[i];
                }
            }
            Attribute[] destinationArray = null;
            if (count < array.Length)
            {
                destinationArray = new Attribute[count];
                Array.Copy(array, 0, destinationArray, 0, count);
            }
            else
            {
                destinationArray = array;
            }
            return new AttributeCollection(destinationArray);
        }

        protected Attribute GetDefaultAttribute(Type attributeType)
        {
            lock (internalSyncObject)
            {
                if (_defaultAttributes == null)
                {
                    _defaultAttributes = new Hashtable();
                }
                if (_defaultAttributes.ContainsKey(attributeType))
                {
                    return (Attribute) _defaultAttributes[attributeType];
                }
                Attribute attribute = null;
                Type reflectionType = TypeDescriptor.GetReflectionType(attributeType);
                FieldInfo field = reflectionType.GetField("Default", BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static);
                if ((field != null) && field.IsStatic)
                {
                    attribute = (Attribute) field.GetValue(null);
                }
                else
                {
                    ConstructorInfo constructor = reflectionType.UnderlyingSystemType.GetConstructor(new Type[0]);
                    if (constructor != null)
                    {
                        attribute = (Attribute) constructor.Invoke(new object[0]);
                        if (!attribute.IsDefaultAttribute())
                        {
                            attribute = null;
                        }
                    }
                }
                _defaultAttributes[attributeType] = attribute;
                return attribute;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this.Attributes.GetEnumerator();
        }

        public bool Matches(Attribute attribute)
        {
            for (int i = 0; i < this.Attributes.Length; i++)
            {
                if (this.Attributes[i].Match(attribute))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Matches(Attribute[] attributes)
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                if (!this.Matches(attributes[i]))
                {
                    return false;
                }
            }
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        protected virtual Attribute[] Attributes
        {
            get
            {
                return this._attributes;
            }
        }

        public int Count
        {
            get
            {
                return this.Attributes.Length;
            }
        }

        public virtual Attribute this[int index]
        {
            get
            {
                return this.Attributes[index];
            }
        }

        public virtual Attribute this[Type attributeType]
        {
            get
            {
                lock (internalSyncObject)
                {
                    int num;
                    if (this._foundAttributeTypes == null)
                    {
                        this._foundAttributeTypes = new AttributeEntry[5];
                    }
                    for (num = 0; num < 5; num++)
                    {
                        if (this._foundAttributeTypes[num].type == attributeType)
                        {
                            int index = this._foundAttributeTypes[num].index;
                            if (index != -1)
                            {
                                return this.Attributes[index];
                            }
                            return this.GetDefaultAttribute(attributeType);
                        }
                        if (this._foundAttributeTypes[num].type == null)
                        {
                            break;
                        }
                    }
                    num = this._index++;
                    if (this._index >= 5)
                    {
                        this._index = 0;
                    }
                    this._foundAttributeTypes[num].type = attributeType;
                    int length = this.Attributes.Length;
                    for (int i = 0; i < length; i++)
                    {
                        Attribute attribute = this.Attributes[i];
                        if (attribute.GetType() == attributeType)
                        {
                            this._foundAttributeTypes[num].index = i;
                            return attribute;
                        }
                    }
                    for (int j = 0; j < length; j++)
                    {
                        Attribute attribute2 = this.Attributes[j];
                        Type type = attribute2.GetType();
                        if (attributeType.IsAssignableFrom(type))
                        {
                            this._foundAttributeTypes[num].index = j;
                            return attribute2;
                        }
                    }
                    this._foundAttributeTypes[num].index = -1;
                    return this.GetDefaultAttribute(attributeType);
                }
            }
        }

        int ICollection.Count
        {
            get
            {
                return this.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AttributeEntry
        {
            public Type type;
            public int index;
        }
    }
}

