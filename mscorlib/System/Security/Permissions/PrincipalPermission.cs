namespace System.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Policy;
    using System.Security.Principal;
    using System.Security.Util;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public sealed class PrincipalPermission : IPermission, IUnrestrictedPermission, ISecurityEncodable, IBuiltInPermission
    {
        private IDRole[] m_array;

        public PrincipalPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.m_array = new IDRole[] { new IDRole() };
                this.m_array[0].m_authenticated = true;
                this.m_array[0].m_id = null;
                this.m_array[0].m_role = null;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.m_array = new IDRole[] { new IDRole() };
                this.m_array[0].m_authenticated = false;
                this.m_array[0].m_id = "";
                this.m_array[0].m_role = "";
            }
        }

        private PrincipalPermission(IDRole[] array)
        {
            this.m_array = array;
        }

        public PrincipalPermission(string name, string role)
        {
            this.m_array = new IDRole[] { new IDRole() };
            this.m_array[0].m_authenticated = true;
            this.m_array[0].m_id = name;
            this.m_array[0].m_role = role;
        }

        public PrincipalPermission(string name, string role, bool isAuthenticated)
        {
            this.m_array = new IDRole[] { new IDRole() };
            this.m_array[0].m_authenticated = isAuthenticated;
            this.m_array[0].m_id = name;
            this.m_array[0].m_role = role;
        }

        public IPermission Copy()
        {
            return new PrincipalPermission(this.m_array);
        }

        [SecuritySafeCritical]
        public void Demand()
        {
            IPrincipal currentPrincipal = null;
            new SecurityPermission(SecurityPermissionFlag.ControlPrincipal).Assert();
            currentPrincipal = Thread.CurrentPrincipal;
            if (currentPrincipal == null)
            {
                this.ThrowSecurityException();
            }
            if (this.m_array != null)
            {
                int length = this.m_array.Length;
                bool flag = false;
                for (int i = 0; i < length; i++)
                {
                    if (this.m_array[i].m_authenticated)
                    {
                        IIdentity identity = currentPrincipal.Identity;
                        if (!identity.IsAuthenticated || ((this.m_array[i].m_id != null) && (string.Compare(identity.Name, this.m_array[i].m_id, StringComparison.OrdinalIgnoreCase) != 0)))
                        {
                            continue;
                        }
                        if (this.m_array[i].m_role == null)
                        {
                            flag = true;
                        }
                        else
                        {
                            WindowsPrincipal principal2 = currentPrincipal as WindowsPrincipal;
                            if ((principal2 != null) && (this.m_array[i].Sid != null))
                            {
                                flag = principal2.IsInRole(this.m_array[i].Sid);
                            }
                            else
                            {
                                flag = currentPrincipal.IsInRole(this.m_array[i].m_role);
                            }
                        }
                        if (!flag)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                    break;
                }
                if (!flag)
                {
                    this.ThrowSecurityException();
                }
            }
        }

        [ComVisible(false)]
        public override bool Equals(object obj)
        {
            IPermission target = obj as IPermission;
            if ((obj != null) && (target == null))
            {
                return false;
            }
            if (!this.IsSubsetOf(target))
            {
                return false;
            }
            if ((target != null) && !target.IsSubsetOf(this))
            {
                return false;
            }
            return true;
        }

        public void FromXml(SecurityElement elem)
        {
            CodeAccessPermission.ValidateElement(elem, this);
            if ((elem.InternalChildren != null) && (elem.InternalChildren.Count != 0))
            {
                int count = elem.InternalChildren.Count;
                int num2 = 0;
                this.m_array = new IDRole[count];
                IEnumerator enumerator = elem.Children.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    IDRole role = new IDRole();
                    role.FromXml((SecurityElement) enumerator.Current);
                    this.m_array[num2++] = role;
                }
            }
            else
            {
                this.m_array = new IDRole[0];
            }
        }

        [ComVisible(false)]
        public override int GetHashCode()
        {
            int num = 0;
            for (int i = 0; i < this.m_array.Length; i++)
            {
                num += this.m_array[i].GetHashCode();
            }
            return num;
        }

        internal static int GetTokenIndex()
        {
            return 8;
        }

        public IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            if (!this.VerifyType(target))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (this.IsUnrestricted())
            {
                return target.Copy();
            }
            PrincipalPermission permission = (PrincipalPermission) target;
            if (permission.IsUnrestricted())
            {
                return this.Copy();
            }
            List<IDRole> list = null;
            for (int i = 0; i < this.m_array.Length; i++)
            {
                for (int j = 0; j < permission.m_array.Length; j++)
                {
                    if (permission.m_array[j].m_authenticated == this.m_array[i].m_authenticated)
                    {
                        if (((permission.m_array[j].m_id == null) || (this.m_array[i].m_id == null)) || this.m_array[i].m_id.Equals(permission.m_array[j].m_id))
                        {
                            if (list == null)
                            {
                                list = new List<IDRole>();
                            }
                            IDRole item = new IDRole {
                                m_id = (permission.m_array[j].m_id == null) ? this.m_array[i].m_id : permission.m_array[j].m_id
                            };
                            if (((permission.m_array[j].m_role == null) || (this.m_array[i].m_role == null)) || this.m_array[i].m_role.Equals(permission.m_array[j].m_role))
                            {
                                item.m_role = (permission.m_array[j].m_role == null) ? this.m_array[i].m_role : permission.m_array[j].m_role;
                            }
                            else
                            {
                                item.m_role = "";
                            }
                            item.m_authenticated = permission.m_array[j].m_authenticated;
                            list.Add(item);
                        }
                        else if (((permission.m_array[j].m_role == null) || (this.m_array[i].m_role == null)) || this.m_array[i].m_role.Equals(permission.m_array[j].m_role))
                        {
                            if (list == null)
                            {
                                list = new List<IDRole>();
                            }
                            IDRole role2 = new IDRole {
                                m_id = "",
                                m_role = (permission.m_array[j].m_role == null) ? this.m_array[i].m_role : permission.m_array[j].m_role,
                                m_authenticated = permission.m_array[j].m_authenticated
                            };
                            list.Add(role2);
                        }
                    }
                }
            }
            if (list == null)
            {
                return null;
            }
            IDRole[] array = new IDRole[list.Count];
            IEnumerator enumerator = list.GetEnumerator();
            int num3 = 0;
            while (enumerator.MoveNext())
            {
                array[num3++] = (IDRole) enumerator.Current;
            }
            return new PrincipalPermission(array);
        }

        private bool IsEmpty()
        {
            for (int i = 0; i < this.m_array.Length; i++)
            {
                if (((this.m_array[i].m_id == null) || !this.m_array[i].m_id.Equals("")) || (((this.m_array[i].m_role == null) || !this.m_array[i].m_role.Equals("")) || this.m_array[i].m_authenticated))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsSubsetOf(IPermission target)
        {
            bool flag2;
            if (target == null)
            {
                return this.IsEmpty();
            }
            try
            {
                PrincipalPermission permission = (PrincipalPermission) target;
                if (permission.IsUnrestricted())
                {
                    return true;
                }
                if (this.IsUnrestricted())
                {
                    return false;
                }
                for (int i = 0; i < this.m_array.Length; i++)
                {
                    bool flag = false;
                    for (int j = 0; j < permission.m_array.Length; j++)
                    {
                        if (((permission.m_array[j].m_authenticated == this.m_array[i].m_authenticated) && ((permission.m_array[j].m_id == null) || ((this.m_array[i].m_id != null) && this.m_array[i].m_id.Equals(permission.m_array[j].m_id)))) && ((permission.m_array[j].m_role == null) || ((this.m_array[i].m_role != null) && this.m_array[i].m_role.Equals(permission.m_array[j].m_role))))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        return false;
                    }
                }
                flag2 = true;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            return flag2;
        }

        public bool IsUnrestricted()
        {
            for (int i = 0; i < this.m_array.Length; i++)
            {
                if (((this.m_array[i].m_id != null) || (this.m_array[i].m_role != null)) || !this.m_array[i].m_authenticated)
                {
                    return false;
                }
            }
            return true;
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        [SecurityCritical]
        private void ThrowSecurityException()
        {
            AssemblyName assemblyName = null;
            Evidence evidence = null;
            PermissionSet.s_fullTrust.Assert();
            try
            {
                Assembly callingAssembly = Assembly.GetCallingAssembly();
                assemblyName = callingAssembly.GetName();
                if (callingAssembly != Assembly.GetExecutingAssembly())
                {
                    evidence = callingAssembly.Evidence;
                }
            }
            catch
            {
            }
            PermissionSet.RevertAssert();
            throw new SecurityException(Environment.GetResourceString("Security_PrincipalPermission"), assemblyName, null, null, null, SecurityAction.Demand, this, this, evidence);
        }

        public override string ToString()
        {
            return this.ToXml().ToString();
        }

        public SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("IPermission");
            XMLUtil.AddClassAttribute(element, base.GetType(), "System.Security.Permissions.PrincipalPermission");
            element.AddAttribute("version", "1");
            int length = this.m_array.Length;
            for (int i = 0; i < length; i++)
            {
                element.AddChild(this.m_array[i].ToXml());
            }
            return element;
        }

        public IPermission Union(IPermission other)
        {
            if (other == null)
            {
                return this.Copy();
            }
            if (!this.VerifyType(other))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            PrincipalPermission permission = (PrincipalPermission) other;
            if (this.IsUnrestricted() || permission.IsUnrestricted())
            {
                return new PrincipalPermission(PermissionState.Unrestricted);
            }
            int num = this.m_array.Length + permission.m_array.Length;
            IDRole[] array = new IDRole[num];
            int index = 0;
            while (index < this.m_array.Length)
            {
                array[index] = this.m_array[index];
                index++;
            }
            for (int i = 0; i < permission.m_array.Length; i++)
            {
                array[index + i] = permission.m_array[i];
            }
            return new PrincipalPermission(array);
        }

        private bool VerifyType(IPermission perm)
        {
            return ((perm != null) && !(perm.GetType() != base.GetType()));
        }
    }
}

