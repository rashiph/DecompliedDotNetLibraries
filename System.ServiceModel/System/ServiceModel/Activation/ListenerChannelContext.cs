namespace System.ServiceModel.Activation
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    [DataContract]
    internal class ListenerChannelContext
    {
        [DataMember]
        private string appKey;
        [DataMember]
        private int listenerChannelId;
        [DataMember]
        private Guid token;

        internal ListenerChannelContext(string appKey, int listenerChannelId, Guid token)
        {
            this.appKey = appKey;
            this.listenerChannelId = listenerChannelId;
            this.token = token;
        }

        public byte[] Dehydrate()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                new DataContractSerializer(typeof(ListenerChannelContext)).WriteObject(stream, this);
                return stream.ToArray();
            }
        }

        public static ListenerChannelContext Hydrate(byte[] blob)
        {
            using (MemoryStream stream = new MemoryStream(blob))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ListenerChannelContext));
                return (ListenerChannelContext) serializer.ReadObject(stream);
            }
        }

        internal string AppKey
        {
            get
            {
                return this.appKey;
            }
        }

        internal int ListenerChannelId
        {
            get
            {
                return this.listenerChannelId;
            }
        }

        internal Guid Token
        {
            get
            {
                return this.token;
            }
        }
    }
}

