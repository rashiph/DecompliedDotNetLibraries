namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true), SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public class ContextProperty
    {
        internal string _name;
        internal object _property;

        internal ContextProperty(string name, object prop)
        {
            this._name = name;
            this._property = prop;
        }

        public virtual string Name
        {
            get
            {
                return this._name;
            }
        }

        public virtual object Property
        {
            get
            {
                return this._property;
            }
        }
    }
}

