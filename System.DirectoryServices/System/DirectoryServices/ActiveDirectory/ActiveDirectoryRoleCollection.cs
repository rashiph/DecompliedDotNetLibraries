namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;

    public class ActiveDirectoryRoleCollection : ReadOnlyCollectionBase
    {
        internal ActiveDirectoryRoleCollection()
        {
        }

        internal ActiveDirectoryRoleCollection(ArrayList values)
        {
            if (values != null)
            {
                base.InnerList.AddRange(values);
            }
        }

        public bool Contains(ActiveDirectoryRole role)
        {
            if ((role < ActiveDirectoryRole.SchemaRole) || (role > ActiveDirectoryRole.InfrastructureRole))
            {
                throw new InvalidEnumArgumentException("role", (int) role, typeof(ActiveDirectoryRole));
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                int num2 = (int) base.InnerList[i];
                if (num2 == role)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ActiveDirectoryRole[] roles, int index)
        {
            base.InnerList.CopyTo(roles, index);
        }

        public int IndexOf(ActiveDirectoryRole role)
        {
            if ((role < ActiveDirectoryRole.SchemaRole) || (role > ActiveDirectoryRole.InfrastructureRole))
            {
                throw new InvalidEnumArgumentException("role", (int) role, typeof(ActiveDirectoryRole));
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                int num2 = (int) base.InnerList[i];
                if (num2 == role)
                {
                    return i;
                }
            }
            return -1;
        }

        public ActiveDirectoryRole this[int index]
        {
            get
            {
                return (ActiveDirectoryRole) base.InnerList[index];
            }
        }
    }
}

