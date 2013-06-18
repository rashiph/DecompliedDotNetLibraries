namespace System.ServiceModel.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Channels;
    using System.Xml.Linq;

    internal class CorrelationExtension
    {
        public CorrelationExtension(XName scopeName)
        {
            this.ScopeName = scopeName;
        }

        public InstanceKey GenerateKey(IDictionary<string, string> keyData)
        {
            return new CorrelationKey(keyData, this.ScopeName.ToString(), null);
        }

        public XName ScopeName { get; private set; }
    }
}

