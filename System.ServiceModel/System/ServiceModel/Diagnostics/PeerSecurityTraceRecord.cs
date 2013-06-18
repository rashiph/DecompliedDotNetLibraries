namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.IdentityModel.Claims;
    using System.Runtime.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;

    internal class PeerSecurityTraceRecord : TraceRecord
    {
        protected ClaimSet claimSet;
        private Exception exception;
        protected string meshId;
        protected string remoteAddress;

        protected PeerSecurityTraceRecord(string meshId, string remoteAddress) : this(meshId, remoteAddress, null, null)
        {
        }

        protected PeerSecurityTraceRecord(string meshId, string remoteAddress, ClaimSet claimSet, Exception exception)
        {
            this.meshId = meshId;
            this.remoteAddress = remoteAddress;
            this.claimSet = claimSet;
            this.exception = exception;
        }

        internal static void WriteClaimSet(XmlWriter writer, ClaimSet claimSet)
        {
            writer.WriteStartElement("NeighborCredentials");
            if (claimSet != null)
            {
                foreach (Claim claim in claimSet)
                {
                    if (claim.ClaimType == ClaimTypes.Name)
                    {
                        writer.WriteElementString("Name", claim.Resource.ToString());
                    }
                    else if (claim.ClaimType == ClaimTypes.X500DistinguishedName)
                    {
                        writer.WriteElementString("X500DistinguishedName", (claim.Resource as X500DistinguishedName).Name.ToString());
                    }
                    else if (claim.ClaimType == ClaimTypes.Thumbprint)
                    {
                        writer.WriteElementString("Thumbprint", Convert.ToBase64String(claim.Resource as byte[]));
                    }
                }
            }
            writer.WriteEndElement();
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("MeshId", this.meshId);
            writer.WriteElementString("RemoteAddress", this.remoteAddress);
            WriteClaimSet(writer, this.claimSet);
            if (this.exception != null)
            {
                writer.WriteElementString("Exception", this.exception.GetType().ToString() + ":" + this.exception.Message);
            }
        }
    }
}

