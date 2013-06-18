namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.IdentityModel.Claims;
    using System.Xml;

    internal class PeerSignatureFailureTraceRecord : PeerSecurityTraceRecord
    {
        private Uri via;

        public PeerSignatureFailureTraceRecord(string meshId, Uri via, ClaimSet claimSet, Exception exception) : base(meshId, null, claimSet, exception)
        {
            this.via = via;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("Via", this.via.ToString());
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/PeerSignatureFailureTraceRecord";
            }
        }
    }
}

