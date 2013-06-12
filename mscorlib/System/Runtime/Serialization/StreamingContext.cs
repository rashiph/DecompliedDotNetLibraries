namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct StreamingContext
    {
        internal object m_additionalContext;
        internal StreamingContextStates m_state;
        public StreamingContext(StreamingContextStates state) : this(state, null)
        {
        }

        public StreamingContext(StreamingContextStates state, object additional)
        {
            this.m_state = state;
            this.m_additionalContext = additional;
        }

        public object Context
        {
            get
            {
                return this.m_additionalContext;
            }
        }
        public override bool Equals(object obj)
        {
            return ((obj is StreamingContext) && ((((StreamingContext) obj).m_additionalContext == this.m_additionalContext) && (((StreamingContext) obj).m_state == this.m_state)));
        }

        public override int GetHashCode()
        {
            return (int) this.m_state;
        }

        public StreamingContextStates State
        {
            get
            {
                return this.m_state;
            }
        }
    }
}

