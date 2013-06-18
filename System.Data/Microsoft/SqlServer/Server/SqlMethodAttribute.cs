namespace Microsoft.SqlServer.Server
{
    using System;

    [Serializable, AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=false)]
    public sealed class SqlMethodAttribute : SqlFunctionAttribute
    {
        private bool m_fCallOnNullInputs = true;
        private bool m_fInvokeIfReceiverIsNull = false;
        private bool m_fMutator = false;

        public bool InvokeIfReceiverIsNull
        {
            get
            {
                return this.m_fInvokeIfReceiverIsNull;
            }
            set
            {
                this.m_fInvokeIfReceiverIsNull = value;
            }
        }

        public bool IsMutator
        {
            get
            {
                return this.m_fMutator;
            }
            set
            {
                this.m_fMutator = value;
            }
        }

        public bool OnNullCall
        {
            get
            {
                return this.m_fCallOnNullInputs;
            }
            set
            {
                this.m_fCallOnNullInputs = value;
            }
        }
    }
}

