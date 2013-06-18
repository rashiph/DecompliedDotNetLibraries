namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Net.Mail;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal static class SctClaimSerializer
    {
        public static Claim DeserializeClaim(XmlDictionaryReader reader, SctClaimDictionary dictionary, XmlObjectSerializer serializer)
        {
            if (reader.IsStartElement(dictionary.NullValue, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return null;
            }
            if (reader.IsStartElement(dictionary.WindowsSidClaim, dictionary.EmptyString))
            {
                string right = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] binaryForm = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Sid, new SecurityIdentifier(binaryForm, 0), right);
            }
            if (reader.IsStartElement(dictionary.DenyOnlySidClaim, dictionary.EmptyString))
            {
                string str2 = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] buffer2 = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.DenyOnlySid, new SecurityIdentifier(buffer2, 0), str2);
            }
            if (reader.IsStartElement(dictionary.X500DistinguishedNameClaim, dictionary.EmptyString))
            {
                string str3 = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] encodedDistinguishedName = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.X500DistinguishedName, new X500DistinguishedName(encodedDistinguishedName), str3);
            }
            if (reader.IsStartElement(dictionary.X509ThumbprintClaim, dictionary.EmptyString))
            {
                string str4 = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] resource = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Thumbprint, resource, str4);
            }
            if (reader.IsStartElement(dictionary.NameClaim, dictionary.EmptyString))
            {
                string str5 = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string str6 = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Name, str6, str5);
            }
            if (reader.IsStartElement(dictionary.DnsClaim, dictionary.EmptyString))
            {
                string str7 = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string str8 = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Dns, str8, str7);
            }
            if (reader.IsStartElement(dictionary.RsaClaim, dictionary.EmptyString))
            {
                string str9 = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string xmlString = reader.ReadString();
                reader.ReadEndElement();
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.FromXmlString(xmlString);
                return new Claim(ClaimTypes.Rsa, provider, str9);
            }
            if (reader.IsStartElement(dictionary.MailAddressClaim, dictionary.EmptyString))
            {
                string str11 = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string address = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Email, new MailAddress(address), str11);
            }
            if (reader.IsStartElement(dictionary.SystemClaim, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return Claim.System;
            }
            if (reader.IsStartElement(dictionary.HashClaim, dictionary.EmptyString))
            {
                string str13 = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                byte[] buffer5 = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Hash, buffer5, str13);
            }
            if (reader.IsStartElement(dictionary.SpnClaim, dictionary.EmptyString))
            {
                string str14 = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string str15 = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Spn, str15, str14);
            }
            if (reader.IsStartElement(dictionary.UpnClaim, dictionary.EmptyString))
            {
                string str16 = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string str17 = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Upn, str17, str16);
            }
            if (reader.IsStartElement(dictionary.UrlClaim, dictionary.EmptyString))
            {
                string str18 = ReadRightAttribute(reader, dictionary);
                reader.ReadStartElement();
                string uriString = reader.ReadString();
                reader.ReadEndElement();
                return new Claim(ClaimTypes.Uri, new Uri(uriString), str18);
            }
            return (Claim) serializer.ReadObject(reader);
        }

        public static ClaimSet DeserializeClaimSet(XmlDictionaryReader reader, SctClaimDictionary dictionary, XmlObjectSerializer serializer, XmlObjectSerializer claimSerializer)
        {
            if (reader.IsStartElement(dictionary.NullValue, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return null;
            }
            if (reader.IsStartElement(dictionary.X509CertificateClaimSet, dictionary.EmptyString))
            {
                reader.ReadStartElement();
                byte[] rawData = reader.ReadContentAsBase64();
                reader.ReadEndElement();
                return new X509CertificateClaimSet(new X509Certificate2(rawData), false);
            }
            if (reader.IsStartElement(dictionary.SystemClaimSet, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return ClaimSet.System;
            }
            if (reader.IsStartElement(dictionary.WindowsClaimSet, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return ClaimSet.Windows;
            }
            if (reader.IsStartElement(dictionary.AnonymousClaimSet, dictionary.EmptyString))
            {
                reader.ReadElementString();
                return ClaimSet.Anonymous;
            }
            if (!reader.IsStartElement(dictionary.ClaimSet, dictionary.EmptyString))
            {
                return (ClaimSet) serializer.ReadObject(reader);
            }
            ClaimSet issuer = null;
            List<Claim> claims = new List<Claim>();
            reader.ReadStartElement();
            if (reader.IsStartElement(dictionary.PrimaryIssuer, dictionary.EmptyString))
            {
                reader.ReadStartElement();
                issuer = DeserializeClaimSet(reader, dictionary, serializer, claimSerializer);
                reader.ReadEndElement();
            }
            while (reader.IsStartElement())
            {
                reader.ReadStartElement();
                claims.Add(DeserializeClaim(reader, dictionary, claimSerializer));
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
            if (issuer == null)
            {
                return new DefaultClaimSet(claims);
            }
            return new DefaultClaimSet(issuer, claims);
        }

        public static IList<IIdentity> DeserializeIdentities(XmlDictionaryReader reader, SctClaimDictionary dictionary, XmlObjectSerializer serializer)
        {
            List<IIdentity> list = null;
            if (reader.IsStartElement(dictionary.Identities, dictionary.EmptyString))
            {
                list = new List<IIdentity>();
                reader.ReadStartElement();
                while (reader.IsStartElement(dictionary.PrimaryIdentity, dictionary.EmptyString))
                {
                    IIdentity item = DeserializePrimaryIdentity(reader, dictionary, serializer);
                    if ((item != null) && (item != System.ServiceModel.Security.SecurityUtils.AnonymousIdentity))
                    {
                        list.Add(item);
                    }
                }
                reader.ReadEndElement();
            }
            return list;
        }

        private static IIdentity DeserializePrimaryIdentity(XmlDictionaryReader reader, SctClaimDictionary dictionary, XmlObjectSerializer serializer)
        {
            IIdentity identity = null;
            if (reader.IsStartElement(dictionary.PrimaryIdentity, dictionary.EmptyString))
            {
                reader.ReadStartElement();
                if (reader.IsStartElement(dictionary.WindowsSidIdentity, dictionary.EmptyString))
                {
                    SecurityIdentifier sid = ReadSidAttribute(reader, dictionary);
                    string attribute = reader.GetAttribute(dictionary.AuthenticationType, dictionary.EmptyString);
                    reader.ReadStartElement();
                    string name = reader.ReadContentAsString();
                    identity = new WindowsSidIdentity(sid, name, attribute ?? string.Empty);
                    reader.ReadEndElement();
                }
                else if (reader.IsStartElement(dictionary.GenericIdentity, dictionary.EmptyString))
                {
                    string str3 = reader.GetAttribute(dictionary.AuthenticationType, dictionary.EmptyString);
                    reader.ReadStartElement();
                    identity = System.ServiceModel.Security.SecurityUtils.CreateIdentity(reader.ReadContentAsString(), str3 ?? string.Empty);
                    reader.ReadEndElement();
                }
                else
                {
                    identity = (IIdentity) serializer.ReadObject(reader);
                }
                reader.ReadEndElement();
            }
            return identity;
        }

        private static string ReadRightAttribute(XmlDictionaryReader reader, SctClaimDictionary dictionary)
        {
            string attribute = reader.GetAttribute(dictionary.Right, dictionary.EmptyString);
            if (!string.IsNullOrEmpty(attribute))
            {
                return attribute;
            }
            return Rights.PossessProperty;
        }

        private static SecurityIdentifier ReadSidAttribute(XmlDictionaryReader reader, SctClaimDictionary dictionary)
        {
            return new SecurityIdentifier(Convert.FromBase64String(reader.GetAttribute(dictionary.Sid, dictionary.EmptyString)), 0);
        }

        public static void SerializeClaim(Claim claim, SctClaimDictionary dictionary, XmlDictionaryWriter writer, XmlObjectSerializer serializer)
        {
            if (claim == null)
            {
                writer.WriteElementString(dictionary.NullValue, dictionary.EmptyString, string.Empty);
            }
            else if (ClaimTypes.Sid.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.WindowsSidClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                SerializeSid((SecurityIdentifier) claim.Resource, dictionary, writer);
                writer.WriteEndElement();
            }
            else if (ClaimTypes.DenyOnlySid.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.DenyOnlySidClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                SerializeSid((SecurityIdentifier) claim.Resource, dictionary, writer);
                writer.WriteEndElement();
            }
            else if (ClaimTypes.X500DistinguishedName.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.X500DistinguishedNameClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                byte[] rawData = ((X500DistinguishedName) claim.Resource).RawData;
                writer.WriteBase64(rawData, 0, rawData.Length);
                writer.WriteEndElement();
            }
            else if (ClaimTypes.Thumbprint.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.X509ThumbprintClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                byte[] resource = (byte[]) claim.Resource;
                writer.WriteBase64(resource, 0, resource.Length);
                writer.WriteEndElement();
            }
            else if (ClaimTypes.Name.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.NameClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string) claim.Resource);
                writer.WriteEndElement();
            }
            else if (ClaimTypes.Dns.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.DnsClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string) claim.Resource);
                writer.WriteEndElement();
            }
            else if (ClaimTypes.Rsa.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.RsaClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString(((RSA) claim.Resource).ToXmlString(false));
                writer.WriteEndElement();
            }
            else if (ClaimTypes.Email.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.MailAddressClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString(((MailAddress) claim.Resource).Address);
                writer.WriteEndElement();
            }
            else if (claim == Claim.System)
            {
                writer.WriteElementString(dictionary.SystemClaim, dictionary.EmptyString, string.Empty);
            }
            else if (ClaimTypes.Hash.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.HashClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                byte[] buffer = (byte[]) claim.Resource;
                writer.WriteBase64(buffer, 0, buffer.Length);
                writer.WriteEndElement();
            }
            else if (ClaimTypes.Spn.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.SpnClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string) claim.Resource);
                writer.WriteEndElement();
            }
            else if (ClaimTypes.Upn.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.UpnClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString((string) claim.Resource);
                writer.WriteEndElement();
            }
            else if (ClaimTypes.Uri.Equals(claim.ClaimType))
            {
                writer.WriteStartElement(dictionary.UrlClaim, dictionary.EmptyString);
                WriteRightAttribute(claim, dictionary, writer);
                writer.WriteString(((Uri) claim.Resource).AbsoluteUri);
                writer.WriteEndElement();
            }
            else
            {
                serializer.WriteObject(writer, claim);
            }
        }

        public static void SerializeClaimSet(ClaimSet claimSet, SctClaimDictionary dictionary, XmlDictionaryWriter writer, XmlObjectSerializer serializer, XmlObjectSerializer claimSerializer)
        {
            if (claimSet is X509CertificateClaimSet)
            {
                X509CertificateClaimSet set = (X509CertificateClaimSet) claimSet;
                writer.WriteStartElement(dictionary.X509CertificateClaimSet, dictionary.EmptyString);
                byte[] rawData = set.X509Certificate.RawData;
                writer.WriteBase64(rawData, 0, rawData.Length);
                writer.WriteEndElement();
            }
            else if (claimSet == ClaimSet.System)
            {
                writer.WriteElementString(dictionary.SystemClaimSet, dictionary.EmptyString, string.Empty);
            }
            else if (claimSet == ClaimSet.Windows)
            {
                writer.WriteElementString(dictionary.WindowsClaimSet, dictionary.EmptyString, string.Empty);
            }
            else if (claimSet == ClaimSet.Anonymous)
            {
                writer.WriteElementString(dictionary.AnonymousClaimSet, dictionary.EmptyString, string.Empty);
            }
            else if ((claimSet is WindowsClaimSet) || (claimSet is DefaultClaimSet))
            {
                writer.WriteStartElement(dictionary.ClaimSet, dictionary.EmptyString);
                writer.WriteStartElement(dictionary.PrimaryIssuer, dictionary.EmptyString);
                if (claimSet.Issuer == claimSet)
                {
                    writer.WriteElementString(dictionary.NullValue, dictionary.EmptyString, string.Empty);
                }
                else
                {
                    SerializeClaimSet(claimSet.Issuer, dictionary, writer, serializer, claimSerializer);
                }
                writer.WriteEndElement();
                foreach (Claim claim in claimSet)
                {
                    writer.WriteStartElement(dictionary.Claim, dictionary.EmptyString);
                    SerializeClaim(claim, dictionary, writer, claimSerializer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            else
            {
                serializer.WriteObject(writer, claimSet);
            }
        }

        public static void SerializeIdentities(AuthorizationContext authContext, SctClaimDictionary dictionary, XmlDictionaryWriter writer, XmlObjectSerializer serializer)
        {
            object obj2;
            if (authContext.Properties.TryGetValue("Identities", out obj2))
            {
                IList<IIdentity> list = obj2 as IList<IIdentity>;
                if ((list != null) && (list.Count > 0))
                {
                    writer.WriteStartElement(dictionary.Identities, dictionary.EmptyString);
                    for (int i = 0; i < list.Count; i++)
                    {
                        SerializePrimaryIdentity(list[i], dictionary, writer, serializer);
                    }
                    writer.WriteEndElement();
                }
            }
        }

        private static void SerializePrimaryIdentity(IIdentity identity, SctClaimDictionary dictionary, XmlDictionaryWriter writer, XmlObjectSerializer serializer)
        {
            if ((identity != null) && (identity != System.ServiceModel.Security.SecurityUtils.AnonymousIdentity))
            {
                writer.WriteStartElement(dictionary.PrimaryIdentity, dictionary.EmptyString);
                if (identity is WindowsIdentity)
                {
                    WindowsIdentity identity2 = (WindowsIdentity) identity;
                    writer.WriteStartElement(dictionary.WindowsSidIdentity, dictionary.EmptyString);
                    WriteSidAttribute(identity2.User, dictionary, writer);
                    string authenticationType = null;
                    using (WindowsIdentity identity3 = WindowsIdentity.GetCurrent())
                    {
                        if (((identity3.User == identity2.Owner) || ((identity2.Owner != null) && identity3.Groups.Contains(identity2.Owner))) || ((identity2.Owner != System.ServiceModel.Security.SecurityUtils.AdministratorsSid) && identity3.Groups.Contains(System.ServiceModel.Security.SecurityUtils.AdministratorsSid)))
                        {
                            authenticationType = identity2.AuthenticationType;
                        }
                    }
                    if (!string.IsNullOrEmpty(authenticationType))
                    {
                        writer.WriteAttributeString(dictionary.AuthenticationType, dictionary.EmptyString, authenticationType);
                    }
                    writer.WriteString(identity2.Name);
                    writer.WriteEndElement();
                }
                else if (identity is WindowsSidIdentity)
                {
                    WindowsSidIdentity identity4 = (WindowsSidIdentity) identity;
                    writer.WriteStartElement(dictionary.WindowsSidIdentity, dictionary.EmptyString);
                    WriteSidAttribute(identity4.SecurityIdentifier, dictionary, writer);
                    if (!string.IsNullOrEmpty(identity4.AuthenticationType))
                    {
                        writer.WriteAttributeString(dictionary.AuthenticationType, dictionary.EmptyString, identity4.AuthenticationType);
                    }
                    writer.WriteString(identity4.Name);
                    writer.WriteEndElement();
                }
                else if (identity is GenericIdentity)
                {
                    GenericIdentity identity5 = (GenericIdentity) identity;
                    writer.WriteStartElement(dictionary.GenericIdentity, dictionary.EmptyString);
                    if (!string.IsNullOrEmpty(identity5.AuthenticationType))
                    {
                        writer.WriteAttributeString(dictionary.AuthenticationType, dictionary.EmptyString, identity5.AuthenticationType);
                    }
                    writer.WriteString(identity5.Name);
                    writer.WriteEndElement();
                }
                else
                {
                    serializer.WriteObject(writer, identity);
                }
                writer.WriteEndElement();
            }
        }

        private static void SerializeSid(SecurityIdentifier sid, SctClaimDictionary dictionary, XmlDictionaryWriter writer)
        {
            byte[] binaryForm = new byte[sid.BinaryLength];
            sid.GetBinaryForm(binaryForm, 0);
            writer.WriteBase64(binaryForm, 0, binaryForm.Length);
        }

        private static void WriteRightAttribute(Claim claim, SctClaimDictionary dictionary, XmlDictionaryWriter writer)
        {
            if (!Rights.PossessProperty.Equals(claim.Right))
            {
                writer.WriteAttributeString(dictionary.Right, dictionary.EmptyString, claim.Right);
            }
        }

        private static void WriteSidAttribute(SecurityIdentifier sid, SctClaimDictionary dictionary, XmlDictionaryWriter writer)
        {
            byte[] binaryForm = new byte[sid.BinaryLength];
            sid.GetBinaryForm(binaryForm, 0);
            writer.WriteAttributeString(dictionary.Sid, dictionary.EmptyString, Convert.ToBase64String(binaryForm));
        }
    }
}

