namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    internal class ContextAddressHeader : AddressHeader
    {
        private ContextDictionary context;

        public ContextAddressHeader(IDictionary<string, string> context)
        {
            this.context = new ContextDictionary(context);
        }

        protected override void OnWriteAddressHeaderContents(XmlDictionaryWriter writer)
        {
            ContextMessageHeader.WriteHeaderContents(writer, this.context);
        }

        public override string Name
        {
            get
            {
                return "Context";
            }
        }

        public override string Namespace
        {
            get
            {
                return "http://schemas.microsoft.com/ws/2006/05/context";
            }
        }
    }
}

