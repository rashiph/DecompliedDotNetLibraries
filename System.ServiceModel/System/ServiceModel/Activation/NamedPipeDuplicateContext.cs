namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class NamedPipeDuplicateContext : DuplicateContext
    {
        [DataMember]
        private IntPtr handle;

        public NamedPipeDuplicateContext(IntPtr handle, Uri via, byte[] readData) : base(via, readData)
        {
            this.handle = handle;
        }

        public IntPtr Handle
        {
            get
            {
                return this.handle;
            }
        }
    }
}

