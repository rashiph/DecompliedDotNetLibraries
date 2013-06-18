namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class DirectoryAttributeCollection : CollectionBase
    {
        public DirectoryAttributeCollection()
        {
            Utility.CheckOSVersion();
        }

        public int Add(DirectoryAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
            }
            return base.List.Add(attribute);
        }

        public void AddRange(DirectoryAttribute[] attributes)
        {
            if (attributes == null)
            {
                throw new ArgumentNullException("attributes");
            }
            DirectoryAttribute[] attributeArray = attributes;
            for (int i = 0; i < attributeArray.Length; i++)
            {
                if (attributeArray[i] == null)
                {
                    throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
                }
            }
            base.InnerList.AddRange(attributes);
        }

        public void AddRange(DirectoryAttributeCollection attributeCollection)
        {
            if (attributeCollection == null)
            {
                throw new ArgumentNullException("attributeCollection");
            }
            int count = attributeCollection.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(attributeCollection[i]);
            }
        }

        public bool Contains(DirectoryAttribute value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(DirectoryAttribute[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(DirectoryAttribute value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, DirectoryAttribute value)
        {
            if (value == null)
            {
                throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
            }
            base.List.Insert(index, value);
        }

        protected override void OnValidate(object value)
        {
            if (value == null)
            {
                throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
            }
            if (!(value is DirectoryAttribute))
            {
                throw new ArgumentException(Res.GetString("InvalidValueType", new object[] { "DirectoryAttribute" }), "value");
            }
        }

        public void Remove(DirectoryAttribute value)
        {
            base.List.Remove(value);
        }

        public DirectoryAttribute this[int index]
        {
            get
            {
                return (DirectoryAttribute) base.List[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
                }
                base.List[index] = value;
            }
        }
    }
}

