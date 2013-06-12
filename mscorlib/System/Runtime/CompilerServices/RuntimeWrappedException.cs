namespace System.Runtime.CompilerServices
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    public sealed class RuntimeWrappedException : Exception
    {
        private object m_wrappedException;

        private RuntimeWrappedException(object thrownObject) : base(Environment.GetResourceString("RuntimeWrappedException"))
        {
            base.SetErrorCode(-2146233026);
            this.m_wrappedException = thrownObject;
        }

        internal RuntimeWrappedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.m_wrappedException = info.GetValue("WrappedException", typeof(object));
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("WrappedException", this.m_wrappedException, typeof(object));
        }

        public object WrappedException
        {
            get
            {
                return this.m_wrappedException;
            }
        }
    }
}

