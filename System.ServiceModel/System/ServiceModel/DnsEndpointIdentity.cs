namespace System.ServiceModel
{
    using System;
    using System.IdentityModel.Claims;
    using System.Xml;

    public class DnsEndpointIdentity : EndpointIdentity
    {
        public DnsEndpointIdentity(Claim identity)
        {
            if (identity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");
            }
            if (!identity.ClaimType.Equals(ClaimTypes.Dns))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("UnrecognizedClaimTypeForIdentity", new object[] { identity.ClaimType, ClaimTypes.Dns }));
            }
            base.Initialize(identity);
        }

        public DnsEndpointIdentity(string dnsName)
        {
            if (dnsName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dnsName");
            }
            base.Initialize(Claim.CreateDnsClaim(dnsName));
        }

        internal override void WriteContentsTo(XmlDictionaryWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteElementString(XD.AddressingDictionary.Dns, XD.AddressingDictionary.IdentityExtensionNamespace, (string) base.IdentityClaim.Resource);
        }
    }
}

