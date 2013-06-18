namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;

    public class AdamRoleCollection : ReadOnlyCollectionBase
    {
        internal AdamRoleCollection()
        {
        }

        internal AdamRoleCollection(ArrayList values)
        {
            if (values != null)
            {
                base.InnerList.AddRange(values);
            }
        }

        public bool Contains(AdamRole role)
        {
            if ((role < AdamRole.SchemaRole) || (role > AdamRole.NamingRole))
            {
                throw new InvalidEnumArgumentException("role", (int) role, typeof(AdamRole));
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

        public void CopyTo(AdamRole[] roles, int index)
        {
            base.InnerList.CopyTo(roles, index);
        }

        public int IndexOf(AdamRole role)
        {
            if ((role < AdamRole.SchemaRole) || (role > AdamRole.NamingRole))
            {
                throw new InvalidEnumArgumentException("role", (int) role, typeof(AdamRole));
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

        public AdamRole this[int index]
        {
            get
            {
                return (AdamRole) base.InnerList[index];
            }
        }
    }
}

