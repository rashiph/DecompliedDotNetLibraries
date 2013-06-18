namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Security.Principal;
    using System.Web;

    [Editor("System.Web.UI.Design.WebControls.RoleGroupCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public sealed class RoleGroupCollection : CollectionBase
    {
        public void Add(RoleGroup group)
        {
            base.List.Add(group);
        }

        public bool Contains(RoleGroup group)
        {
            return base.List.Contains(group);
        }

        public void CopyTo(RoleGroup[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public RoleGroup GetMatchingRoleGroup(IPrincipal user)
        {
            int matchingRoleGroupInternal = this.GetMatchingRoleGroupInternal(user);
            if (matchingRoleGroupInternal != -1)
            {
                return this[matchingRoleGroupInternal];
            }
            return null;
        }

        internal int GetMatchingRoleGroupInternal(IPrincipal user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            int num = 0;
            foreach (RoleGroup group in this)
            {
                if (group.ContainsUser(user))
                {
                    return num;
                }
                num++;
            }
            return -1;
        }

        public int IndexOf(RoleGroup group)
        {
            return base.List.IndexOf(group);
        }

        public void Insert(int index, RoleGroup group)
        {
            base.List.Insert(index, group);
        }

        protected override void OnValidate(object value)
        {
            base.OnValidate(value);
            if (!(value is RoleGroup))
            {
                throw new ArgumentException(System.Web.SR.GetString("RoleGroupCollection_InvalidType"), "value");
            }
        }

        public void Remove(RoleGroup group)
        {
            int index = this.IndexOf(group);
            if (index >= 0)
            {
                base.List.RemoveAt(index);
            }
        }

        public RoleGroup this[int index]
        {
            get
            {
                return (RoleGroup) base.List[index];
            }
        }
    }
}

