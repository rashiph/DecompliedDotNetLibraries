namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ReadOnlyActiveDirectorySchemaClassCollection : ReadOnlyCollectionBase
    {
        internal ReadOnlyActiveDirectorySchemaClassCollection()
        {
        }

        internal ReadOnlyActiveDirectorySchemaClassCollection(ICollection values)
        {
            if (values != null)
            {
                base.InnerList.AddRange(values);
            }
        }

        public bool Contains(ActiveDirectorySchemaClass schemaClass)
        {
            if (schemaClass == null)
            {
                throw new ArgumentNullException("schemaClass");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySchemaClass class2 = (ActiveDirectorySchemaClass) base.InnerList[i];
                if (Utils.Compare(class2.Name, schemaClass.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ActiveDirectorySchemaClass[] classes, int index)
        {
            base.InnerList.CopyTo(classes, index);
        }

        public int IndexOf(ActiveDirectorySchemaClass schemaClass)
        {
            if (schemaClass == null)
            {
                throw new ArgumentNullException("schemaClass");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ActiveDirectorySchemaClass class2 = (ActiveDirectorySchemaClass) base.InnerList[i];
                if (Utils.Compare(class2.Name, schemaClass.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public ActiveDirectorySchemaClass this[int index]
        {
            get
            {
                return (ActiveDirectorySchemaClass) base.InnerList[index];
            }
        }
    }
}

