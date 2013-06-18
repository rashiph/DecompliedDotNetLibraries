namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class DirectoryAttributeModificationCollection : CollectionBase
    {
        public DirectoryAttributeModificationCollection()
        {
            Utility.CheckOSVersion();
        }

        public int Add(DirectoryAttributeModification attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
            }
            return base.List.Add(attribute);
        }

        public void AddRange(DirectoryAttributeModification[] attributes)
        {
            if (attributes == null)
            {
                throw new ArgumentNullException("attributes");
            }
            DirectoryAttributeModification[] modificationArray = attributes;
            for (int i = 0; i < modificationArray.Length; i++)
            {
                if (modificationArray[i] == null)
                {
                    throw new ArgumentException(Res.GetString("NullDirectoryAttributeCollection"));
                }
            }
            base.InnerList.AddRange(attributes);
        }

        public void AddRange(DirectoryAttributeModificationCollection attributeCollection)
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

        public bool Contains(DirectoryAttributeModification value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(DirectoryAttributeModification[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(DirectoryAttributeModification value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, DirectoryAttributeModification value)
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
            if (!(value is DirectoryAttributeModification))
            {
                throw new ArgumentException(Res.GetString("InvalidValueType", new object[] { "DirectoryAttributeModification" }), "value");
            }
        }

        public void Remove(DirectoryAttributeModification value)
        {
            base.List.Remove(value);
        }

        public DirectoryAttributeModification this[int index]
        {
            get
            {
                return (DirectoryAttributeModification) base.List[index];
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

