namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class DotNetAtomicTransactionExternalDictionary
    {
        public XmlDictionaryString AccessDenied;
        public XmlDictionaryString ContextId;
        public XmlDictionaryString CoordinatorRegistrationFailed;
        public XmlDictionaryString Description;
        public XmlDictionaryString Disabled;
        public XmlDictionaryString Enlistment;
        public XmlDictionaryString InvalidPolicy;
        public XmlDictionaryString IsolationFlags;
        public XmlDictionaryString IsolationLevel;
        public XmlDictionaryString LocalTransactionId;
        public XmlDictionaryString Loopback;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString Prefix;
        public XmlDictionaryString Protocol;
        public XmlDictionaryString RegisterInfo;
        public XmlDictionaryString TokenId;
        public XmlDictionaryString TooManyEnlistments;

        public DotNetAtomicTransactionExternalDictionary(ServiceModelDictionary dictionary)
        {
            this.Namespace = dictionary.CreateString("http://schemas.microsoft.com/ws/2006/02/transactions", 0x41);
            this.Prefix = dictionary.CreateString("mstx", 0x199);
            this.Enlistment = dictionary.CreateString("Enlistment", 410);
            this.Protocol = dictionary.CreateString("protocol", 0x19b);
            this.LocalTransactionId = dictionary.CreateString("LocalTransactionId", 0x19c);
            this.IsolationLevel = dictionary.CreateString("IsolationLevel", 0x19d);
            this.IsolationFlags = dictionary.CreateString("IsolationFlags", 0x19e);
            this.Description = dictionary.CreateString("Description", 0x19f);
            this.Loopback = dictionary.CreateString("Loopback", 0x1a0);
            this.RegisterInfo = dictionary.CreateString("RegisterInfo", 0x1a1);
            this.ContextId = dictionary.CreateString("ContextId", 0x1a2);
            this.TokenId = dictionary.CreateString("TokenId", 0x1a3);
            this.AccessDenied = dictionary.CreateString("AccessDenied", 420);
            this.InvalidPolicy = dictionary.CreateString("InvalidPolicy", 0x1a5);
            this.CoordinatorRegistrationFailed = dictionary.CreateString("CoordinatorRegistrationFailed", 0x1a6);
            this.TooManyEnlistments = dictionary.CreateString("TooManyEnlistments", 0x1a7);
            this.Disabled = dictionary.CreateString("Disabled", 0x1a8);
        }
    }
}

