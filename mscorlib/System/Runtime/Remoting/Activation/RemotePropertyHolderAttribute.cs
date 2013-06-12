namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Contexts;
    using System.Security;

    internal class RemotePropertyHolderAttribute : IContextAttribute
    {
        private IList _cp;

        internal RemotePropertyHolderAttribute(IList cp)
        {
            this._cp = cp;
        }

        [SecurityCritical, ComVisible(true)]
        public virtual void GetPropertiesForNewContext(IConstructionCallMessage ctorMsg)
        {
            for (int i = 0; i < this._cp.Count; i++)
            {
                ctorMsg.ContextProperties.Add(this._cp[i]);
            }
        }

        [ComVisible(true), SecurityCritical]
        public virtual bool IsContextOK(Context ctx, IConstructionCallMessage msg)
        {
            return false;
        }
    }
}

