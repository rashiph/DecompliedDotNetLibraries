namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class PnrpRegisterTraceRecord : TraceRecord
    {
        private PnrpPeerResolver.PnrpRegistration global;
        private IEnumerable<PnrpPeerResolver.PnrpRegistration> linkEntries;
        private string meshId;
        private IEnumerable<PnrpPeerResolver.PnrpRegistration> siteEntries;

        public PnrpRegisterTraceRecord(string meshId, PnrpPeerResolver.PnrpRegistration global, IEnumerable<PnrpPeerResolver.PnrpRegistration> siteEntries, IEnumerable<PnrpPeerResolver.PnrpRegistration> linkEntries)
        {
            this.meshId = meshId;
            this.siteEntries = siteEntries;
            this.linkEntries = linkEntries;
            this.global = global;
        }

        private void WriteEntries(XmlWriter writer, IEnumerable<PnrpPeerResolver.PnrpRegistration> entries)
        {
            if (entries != null)
            {
                foreach (PnrpPeerResolver.PnrpRegistration registration in entries)
                {
                    this.WriteEntry(writer, registration);
                }
            }
        }

        private void WriteEntry(XmlWriter writer, PnrpPeerResolver.PnrpRegistration entry)
        {
            if (entry != null)
            {
                writer.WriteStartElement("Registration");
                writer.WriteAttributeString("CloudName", entry.CloudName);
                foreach (IPEndPoint point in entry.Addresses)
                {
                    writer.WriteElementString("Address", point.ToString());
                }
                writer.WriteEndElement();
            }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("MeshId", this.meshId.ToString());
            writer.WriteStartElement("Registrations");
            this.WriteEntry(writer, this.global);
            this.WriteEntries(writer, this.siteEntries);
            this.WriteEntries(writer, this.linkEntries);
            writer.WriteEndElement();
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/PnrpRegistrationTraceRecord";
            }
        }
    }
}

