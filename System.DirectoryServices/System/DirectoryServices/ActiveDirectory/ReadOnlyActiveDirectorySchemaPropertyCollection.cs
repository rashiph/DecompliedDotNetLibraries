namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ReadOnlyActiveDirectorySchemaPropertyCollection : ReadOnlyCollectionBase
    {
        internal ReadOnlyActiveDirectorySchemaPropertyCollection()
        {
        }

        internal ReadOnlyActiveDirectorySchemaPropertyCollection(ArrayList values)
        {
            if (values != null)
            {
                base.InnerList.AddRange(values);
            }
        }

        public bool Contains(ActiveDirectorySchemaProperty schemaProperty)
        {
            if (schemaProperty == null)
            {
                throw new ArgumentNullException("schemaProperty");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySchemaProperty property = (ActiveDirectorySchemaProperty) base.InnerList[i];
                if (Utils.Compare(property.Name, schemaProperty.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ActiveDirectorySchemaProperty[] properties, int index)
        {
            base.InnerList.CopyTo(properties, index);
        }

        public int IndexOf(ActiveDirectorySchemaProperty schemaProperty)
        {
            if (schemaProperty == null)
            {
                throw new ArgumentNullException("schemaProperty");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySchemaProperty property = (ActiveDirectorySchemaProperty) base.InnerList[i];
                if (Utils.Compare(property.Name, schemaProperty.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public ActiveDirectorySchemaProperty this[int index]
        {
            get
            {
                return (ActiveDirectorySchemaProperty) base.InnerList[index];
            }
        }
    }
}

