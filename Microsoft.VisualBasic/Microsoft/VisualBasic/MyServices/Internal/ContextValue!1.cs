namespace Microsoft.VisualBasic.MyServices.Internal
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.ComponentModel;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ContextValue<T>
    {
        private readonly string m_ContextKey;

        public ContextValue()
        {
            this.m_ContextKey = Guid.NewGuid().ToString();
        }

        public T Value
        {
            [SecuritySafeCritical]
            get
            {
                object current = SkuSafeHttpContext.Current;
                if (current != null)
                {
                    return (T) NewLateBinding.LateGet(current, null, "Items", new object[] { this.m_ContextKey }, null, null, null);
                }
                return (T) CallContext.GetData(this.m_ContextKey);
            }
            [SecuritySafeCritical]
            set
            {
                object current = SkuSafeHttpContext.Current;
                if (current != null)
                {
                    NewLateBinding.LateSet(current, null, "Items", new object[] { this.m_ContextKey, value }, null, null);
                }
                else
                {
                    CallContext.SetData(this.m_ContextKey, value);
                }
            }
        }
    }
}

