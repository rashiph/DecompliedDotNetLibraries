namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.Serialization;

    [KnownType(typeof(NamedPipeDuplicateContext)), KnownType(typeof(TcpDuplicateContext)), DataContract]
    internal class DuplicateContext
    {
        [DataMember]
        private byte[] readData;
        [DataMember]
        private Uri via;

        protected DuplicateContext(Uri via, byte[] readData)
        {
            this.via = via;
            this.readData = readData;
        }

        public byte[] ReadData
        {
            get
            {
                return this.readData;
            }
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }
        }
    }
}

