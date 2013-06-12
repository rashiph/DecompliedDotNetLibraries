namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Activation;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, SecurityCritical, ComVisible(true), AttributeUsage(AttributeTargets.Class), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public class ContextAttribute : Attribute, IContextAttribute, IContextProperty
    {
        protected string AttributeName;

        public ContextAttribute(string name)
        {
            this.AttributeName = name;
        }

        [SecuritySafeCritical]
        public override bool Equals(object o)
        {
            IContextProperty property = o as IContextProperty;
            return ((property != null) && this.AttributeName.Equals(property.Name));
        }

        [SecurityCritical]
        public virtual void Freeze(Context newContext)
        {
        }

        [SecuritySafeCritical]
        public override int GetHashCode()
        {
            return this.AttributeName.GetHashCode();
        }

        [SecurityCritical]
        public virtual void GetPropertiesForNewContext(IConstructionCallMessage ctorMsg)
        {
            if (ctorMsg == null)
            {
                throw new ArgumentNullException("ctorMsg");
            }
            ctorMsg.ContextProperties.Add(this);
        }

        [SecurityCritical]
        public virtual bool IsContextOK(Context ctx, IConstructionCallMessage ctorMsg)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }
            if (ctorMsg == null)
            {
                throw new ArgumentNullException("ctorMsg");
            }
            if (!ctorMsg.ActivationType.IsContextful)
            {
                return true;
            }
            object property = ctx.GetProperty(this.AttributeName);
            return ((property != null) && this.Equals(property));
        }

        [SecurityCritical]
        public virtual bool IsNewContextOK(Context newCtx)
        {
            return true;
        }

        public virtual string Name
        {
            [SecurityCritical]
            get
            {
                return this.AttributeName;
            }
        }
    }
}

