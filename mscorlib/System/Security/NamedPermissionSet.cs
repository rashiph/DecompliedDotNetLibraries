namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public sealed class NamedPermissionSet : PermissionSet
    {
        private string m_description;
        [OptionalField(VersionAdded=2)]
        internal string m_descrResource;
        private string m_name;
        private static object s_InternalSyncObject;

        internal NamedPermissionSet()
        {
        }

        public NamedPermissionSet(NamedPermissionSet permSet) : base(permSet)
        {
            this.m_name = permSet.m_name;
            this.m_description = permSet.Description;
        }

        internal NamedPermissionSet(SecurityElement permissionSetXml) : base(PermissionState.None)
        {
            this.FromXml(permissionSetXml);
        }

        public NamedPermissionSet(string name)
        {
            CheckName(name);
            this.m_name = name;
        }

        public NamedPermissionSet(string name, PermissionState state) : base(state)
        {
            CheckName(name);
            this.m_name = name;
        }

        public NamedPermissionSet(string name, PermissionSet permSet) : base(permSet)
        {
            CheckName(name);
            this.m_name = name;
        }

        private static void CheckName(string name)
        {
            if ((name == null) || name.Equals(""))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NPMSInvalidName"));
            }
        }

        public override PermissionSet Copy()
        {
            return new NamedPermissionSet(this);
        }

        public NamedPermissionSet Copy(string name)
        {
            return new NamedPermissionSet(this) { Name = name };
        }

        [ComVisible(false)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override void FromXml(SecurityElement et)
        {
            this.FromXml(et, false, false);
        }

        internal override void FromXml(SecurityElement et, bool allowInternalOnly, bool ignoreTypeLoadFailures)
        {
            if (et == null)
            {
                throw new ArgumentNullException("et");
            }
            string str = et.Attribute("Name");
            this.m_name = (str == null) ? null : str;
            str = et.Attribute("Description");
            this.m_description = (str == null) ? "" : str;
            this.m_descrResource = null;
            base.FromXml(et, allowInternalOnly, ignoreTypeLoadFailures);
        }

        internal void FromXmlNameOnly(SecurityElement et)
        {
            string str = et.Attribute("Name");
            this.m_name = (str == null) ? null : str;
        }

        [ComVisible(false)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SecuritySafeCritical]
        public override SecurityElement ToXml()
        {
            SecurityElement element = base.ToXml("System.Security.NamedPermissionSet");
            if ((this.m_name != null) && !this.m_name.Equals(""))
            {
                element.AddAttribute("Name", SecurityElement.Escape(this.m_name));
            }
            if ((this.Description != null) && !this.Description.Equals(""))
            {
                element.AddAttribute("Description", SecurityElement.Escape(this.Description));
            }
            return element;
        }

        public string Description
        {
            get
            {
                if (this.m_descrResource != null)
                {
                    this.m_description = Environment.GetResourceString(this.m_descrResource);
                    this.m_descrResource = null;
                }
                return this.m_description;
            }
            set
            {
                this.m_description = value;
                this.m_descrResource = null;
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        public string Name
        {
            get
            {
                return this.m_name;
            }
            set
            {
                CheckName(value);
                this.m_name = value;
            }
        }
    }
}

