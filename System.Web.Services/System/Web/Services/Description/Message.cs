namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Xml.Serialization;

    [XmlFormatExtensionPoint("Extensions")]
    public sealed class Message : NamedItem
    {
        private ServiceDescriptionFormatExtensionCollection extensions;
        private System.Web.Services.Description.ServiceDescription parent;
        private MessagePartCollection parts;

        public MessagePart FindPartByName(string partName)
        {
            for (int i = 0; i < this.parts.Count; i++)
            {
                MessagePart part = this.parts[i];
                if (part.Name == partName)
                {
                    return part;
                }
            }
            throw new ArgumentException(Res.GetString("MissingMessagePartForMessageFromNamespace3", new object[] { partName, base.Name, this.ServiceDescription.TargetNamespace }), "partName");
        }

        public MessagePart[] FindPartsByName(string[] partNames)
        {
            MessagePart[] partArray = new MessagePart[partNames.Length];
            for (int i = 0; i < partNames.Length; i++)
            {
                partArray[i] = this.FindPartByName(partNames[i]);
            }
            return partArray;
        }

        internal void SetParent(System.Web.Services.Description.ServiceDescription parent)
        {
            this.parent = parent;
        }

        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new ServiceDescriptionFormatExtensionCollection(this);
                }
                return this.extensions;
            }
        }

        [XmlElement("part")]
        public MessagePartCollection Parts
        {
            get
            {
                if (this.parts == null)
                {
                    this.parts = new MessagePartCollection(this);
                }
                return this.parts;
            }
        }

        public System.Web.Services.Description.ServiceDescription ServiceDescription
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parent;
            }
        }
    }
}

