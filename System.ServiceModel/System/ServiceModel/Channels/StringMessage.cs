namespace System.ServiceModel.Channels
{
    using System;
    using System.Xml;

    internal class StringMessage : ContentOnlyMessage
    {
        private string data;

        public StringMessage(string data)
        {
            this.data = data;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            if ((this.data != null) && (this.data.Length > 0))
            {
                writer.WriteElementString("BODY", this.data);
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(this.data);
            }
        }
    }
}

