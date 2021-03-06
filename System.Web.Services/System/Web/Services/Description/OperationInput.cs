﻿namespace System.Web.Services.Description
{
    using System.Web.Services.Configuration;
    using System.Xml.Serialization;

    [XmlFormatExtensionPoint("Extensions")]
    public sealed class OperationInput : OperationMessage
    {
        private ServiceDescriptionFormatExtensionCollection extensions;

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
    }
}

