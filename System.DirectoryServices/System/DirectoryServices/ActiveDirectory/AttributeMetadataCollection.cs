namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class AttributeMetadataCollection : ReadOnlyCollectionBase
    {
        internal AttributeMetadataCollection()
        {
        }

        internal int Add(AttributeMetadata metadata)
        {
            return base.InnerList.Add(metadata);
        }

        public bool Contains(AttributeMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                AttributeMetadata metadata2 = (AttributeMetadata) base.InnerList[i];
                if (Utils.Compare(metadata2.Name, metadata.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(AttributeMetadata[] metadata, int index)
        {
            base.InnerList.CopyTo(metadata, index);
        }

        public int IndexOf(AttributeMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                AttributeMetadata metadata2 = (AttributeMetadata) base.InnerList[i];
                if (Utils.Compare(metadata2.Name, metadata.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public AttributeMetadata this[int index]
        {
            get
            {
                return (AttributeMetadata) base.InnerList[index];
            }
        }
    }
}

