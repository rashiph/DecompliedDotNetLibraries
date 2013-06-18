namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class WSSecurityPolicy12 : WSSecurityPolicy
    {
        public const string RequireExplicitDerivedKeysName = "RequireExplicitDerivedKeys";
        public const string RequireImpliedDerivedKeysName = "RequireImpliedDerivedKeys";
        public const string SignedEncryptedSupportingTokensName = "SignedEncryptedSupportingTokens";
        public const string WsspNamespace = "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200702";

        public override XmlElement CreateMsspSslContextTokenAssertion(MetadataExporter exporter, SslSecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateMsspAssertion("SslContextToken");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            tokenAssertion.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys), this.CreateWsspMustNotSendCancelAssertion(false), this.CreateMsspRequireClientCertificateAssertion(parameters.RequireClientCertificate), this.CreateWsspMustNotSendAmendAssertion(), this.CreateWsspMustNotSendRenewAssertion() }));
            return tokenAssertion;
        }

        public override XmlElement CreateWsspHttpsTokenAssertion(MetadataExporter exporter, HttpsTransportBindingElement httpsBinding)
        {
            XmlElement element = this.CreateWsspAssertion("HttpsToken");
            if ((httpsBinding.RequireClientCertificate || (httpsBinding.AuthenticationScheme == AuthenticationSchemes.Basic)) || (httpsBinding.AuthenticationScheme == AuthenticationSchemes.Digest))
            {
                XmlElement newChild = this.CreateWspPolicyWrapper(exporter, new XmlElement[0]);
                if (httpsBinding.RequireClientCertificate)
                {
                    newChild.AppendChild(this.CreateWsspAssertion("RequireClientCertificate"));
                }
                if (httpsBinding.AuthenticationScheme == AuthenticationSchemes.Basic)
                {
                    newChild.AppendChild(this.CreateWsspAssertion("HttpBasicAuthentication"));
                }
                else if (httpsBinding.AuthenticationScheme == AuthenticationSchemes.Digest)
                {
                    newChild.AppendChild(this.CreateWsspAssertion("HttpDigestAuthentication"));
                }
                element.AppendChild(newChild);
            }
            return element;
        }

        private XmlElement CreateWsspMustNotSendAmendAssertion()
        {
            return this.CreateWsspAssertion("MustNotSendAmend");
        }

        private XmlElement CreateWsspMustNotSendRenewAssertion()
        {
            return this.CreateWsspAssertion("MustNotSendRenew");
        }

        public override XmlElement CreateWsspRsaTokenAssertion(RsaSecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateWsspAssertion("KeyValueToken");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            return tokenAssertion;
        }

        public override XmlElement CreateWsspSecureConversationTokenAssertion(MetadataExporter exporter, SecureConversationSecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateWsspAssertion("SecureConversationToken");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            tokenAssertion.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys), this.CreateWsspMustNotSendCancelAssertion(parameters.RequireCancellation), this.CreateWsspBootstrapPolicyAssertion(exporter, parameters.BootstrapSecurityBindingElement), this.CreateWsspMustNotSendAmendAssertion(), (!parameters.RequireCancellation || !parameters.CanRenewSession) ? this.CreateWsspMustNotSendRenewAssertion() : null }));
            return tokenAssertion;
        }

        private XmlElement CreateWsspSignedEncryptedSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> optionalSignedEncrypted)
        {
            if (((signedEncrypted == null) || (signedEncrypted.Count == 0)) && ((optionalSignedEncrypted == null) || (optionalSignedEncrypted.Count == 0)))
            {
                return null;
            }
            XmlElement newChild = this.CreateWspPolicyWrapper(exporter, new XmlElement[0]);
            if (signedEncrypted != null)
            {
                foreach (SecurityTokenParameters parameters in signedEncrypted)
                {
                    newChild.AppendChild(this.CreateTokenAssertion(exporter, parameters));
                }
            }
            if (optionalSignedEncrypted != null)
            {
                foreach (SecurityTokenParameters parameters2 in optionalSignedEncrypted)
                {
                    newChild.AppendChild(this.CreateTokenAssertion(exporter, parameters2, true));
                }
            }
            XmlElement element = this.CreateWsspAssertion("SignedEncryptedSupportingTokens");
            element.AppendChild(newChild);
            return element;
        }

        private XmlElement CreateWsspSignedSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> optionalSigned)
        {
            if (((signed == null) || (signed.Count == 0)) && ((optionalSigned == null) || (optionalSigned.Count == 0)))
            {
                return null;
            }
            XmlElement newChild = this.CreateWspPolicyWrapper(exporter, new XmlElement[0]);
            if (signed != null)
            {
                foreach (SecurityTokenParameters parameters in signed)
                {
                    newChild.AppendChild(this.CreateTokenAssertion(exporter, parameters));
                }
            }
            if (optionalSigned != null)
            {
                foreach (SecurityTokenParameters parameters2 in optionalSigned)
                {
                    newChild.AppendChild(this.CreateTokenAssertion(exporter, parameters2, true));
                }
            }
            XmlElement element = this.CreateWsspAssertion("SignedSupportingTokens");
            element.AppendChild(newChild);
            return element;
        }

        public override XmlElement CreateWsspSpnegoContextTokenAssertion(MetadataExporter exporter, SspiSecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateWsspAssertion("SpnegoContextToken");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            tokenAssertion.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys), this.CreateWsspMustNotSendCancelAssertion(false), this.CreateWsspMustNotSendAmendAssertion(), this.CreateWsspMustNotSendRenewAssertion() }));
            return tokenAssertion;
        }

        public override Collection<XmlElement> CreateWsspSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, Collection<SecurityTokenParameters> optionalEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing, AddressingVersion addressingVersion)
        {
            Collection<XmlElement> collection = new Collection<XmlElement>();
            XmlElement item = this.CreateWsspSignedSupportingTokensAssertion(exporter, signed, optionalSigned);
            if (item != null)
            {
                collection.Add(item);
            }
            item = this.CreateWsspSignedEncryptedSupportingTokensAssertion(exporter, signedEncrypted, optionalSignedEncrypted);
            if (item != null)
            {
                collection.Add(item);
            }
            item = base.CreateWsspEndorsingSupportingTokensAssertion(exporter, endorsing, optionalEndorsing, addressingVersion);
            if (item != null)
            {
                collection.Add(item);
            }
            item = base.CreateWsspSignedEndorsingSupportingTokensAssertion(exporter, signedEndorsing, optionalSignedEndorsing, addressingVersion);
            if (item != null)
            {
                collection.Add(item);
            }
            return collection;
        }

        public override XmlElement CreateWsspTrustAssertion(MetadataExporter exporter, SecurityKeyEntropyMode keyEntropyMode)
        {
            return base.CreateWsspTrustAssertion("Trust13", exporter, keyEntropyMode);
        }

        public override MessageSecurityVersion GetSupportedMessageSecurityVersion(SecurityVersion version)
        {
            if (version != SecurityVersion.WSSecurity10)
            {
                return MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;
            }
            return MessageSecurityVersion.WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10;
        }

        public override bool IsSecurityVersionSupported(MessageSecurityVersion version)
        {
            if ((version != MessageSecurityVersion.WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10) && (version != MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12))
            {
                return (version == MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10);
            }
            return true;
        }

        public override bool TryImportMsspSslContextTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            SecurityTokenInclusionMode mode;
            parameters = null;
            if (this.IsMsspAssertion(assertion, "SslContextToken") && this.TryGetIncludeTokenValue(assertion, out mode))
            {
                Collection<Collection<XmlElement>> collection;
                if (this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
                {
                    foreach (Collection<XmlElement> collection2 in collection)
                    {
                        bool flag;
                        bool flag2;
                        SslSecurityTokenParameters parameters2 = new SslSecurityTokenParameters();
                        parameters = parameters2;
                        if (((this.TryImportWsspRequireDerivedKeysAssertion(collection2, parameters2) && this.TryImportWsspMustNotSendCancelAssertion(collection2, out flag)) && (this.TryImportWsspMustNotSendAmendAssertion(collection2) && this.TryImportWsspMustNotSendRenewAssertion(collection2, out flag2))) && (this.TryImportMsspRequireClientCertificateAssertion(collection2, parameters2) && (collection2.Count == 0)))
                        {
                            parameters2.RequireCancellation = true;
                            parameters2.InclusionMode = mode;
                            break;
                        }
                        parameters = null;
                    }
                }
                else
                {
                    parameters = new SslSecurityTokenParameters();
                    parameters.RequireDerivedKeys = false;
                    parameters.InclusionMode = mode;
                }
            }
            return (parameters != null);
        }

        public override bool TryImportWsspHttpsTokenAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, HttpsTransportBindingElement httpsBinding)
        {
            XmlElement element;
            if (assertions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertions");
            }
            if (!this.TryImportWsspAssertion(assertions, "HttpsToken", out element))
            {
                return false;
            }
            XmlElement element2 = null;
            foreach (System.Xml.XmlNode node in element.ChildNodes)
            {
                if (((node is XmlElement) && (node.LocalName == "Policy")) && ((node.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/policy") || (node.NamespaceURI == "http://www.w3.org/ns/ws-policy")))
                {
                    element2 = (XmlElement) node;
                    break;
                }
            }
            if (element2 != null)
            {
                foreach (System.Xml.XmlNode node2 in element2.ChildNodes)
                {
                    if ((node2 is XmlElement) && (node2.NamespaceURI == this.WsspNamespaceUri))
                    {
                        if (node2.LocalName == "RequireClientCertificate")
                        {
                            httpsBinding.RequireClientCertificate = true;
                        }
                        else if (node2.LocalName == "HttpBasicAuthentication")
                        {
                            httpsBinding.AuthenticationScheme = AuthenticationSchemes.Basic;
                        }
                        else if (node2.LocalName == "HttpDigestAuthentication")
                        {
                            httpsBinding.AuthenticationScheme = AuthenticationSchemes.Digest;
                        }
                    }
                }
            }
            return true;
        }

        public virtual bool TryImportWsspMustNotSendAmendAssertion(ICollection<XmlElement> assertions)
        {
            this.TryImportWsspAssertion(assertions, "MustNotSendAmend");
            return true;
        }

        public virtual bool TryImportWsspMustNotSendRenewAssertion(ICollection<XmlElement> assertions, out bool canRenewSession)
        {
            canRenewSession = !this.TryImportWsspAssertion(assertions, "MustNotSendRenew");
            return true;
        }

        public override bool TryImportWsspRequireDerivedKeysAssertion(ICollection<XmlElement> assertions, SecurityTokenParameters parameters)
        {
            parameters.RequireDerivedKeys = this.TryImportWsspAssertion(assertions, "RequireDerivedKeys");
            if (!parameters.RequireDerivedKeys)
            {
                parameters.RequireDerivedKeys = this.TryImportWsspAssertion(assertions, "RequireExplicitDerivedKeys");
            }
            if (!parameters.RequireDerivedKeys)
            {
                XmlElement assertion = null;
                if (this.TryImportWsspAssertion(assertions, "RequireImpliedDerivedKeys", out assertion))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { assertion.OuterXml })));
                }
            }
            return true;
        }

        public override bool TryImportWsspRsaTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            SecurityTokenInclusionMode mode;
            Collection<Collection<XmlElement>> collection;
            parameters = null;
            if ((this.IsWsspAssertion(assertion, "KeyValueToken") && this.TryGetIncludeTokenValue(assertion, out mode)) && !this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                parameters = new RsaSecurityTokenParameters();
                parameters.InclusionMode = mode;
            }
            return (parameters != null);
        }

        public override bool TryImportWsspSecureConversationTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            SecurityTokenInclusionMode mode;
            parameters = null;
            if (this.IsWsspAssertion(assertion, "SecureConversationToken") && this.TryGetIncludeTokenValue(assertion, out mode))
            {
                Collection<Collection<XmlElement>> collection;
                if (this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
                {
                    foreach (Collection<XmlElement> collection2 in collection)
                    {
                        bool flag;
                        bool flag2;
                        SecureConversationSecurityTokenParameters parameters2 = new SecureConversationSecurityTokenParameters();
                        parameters = parameters2;
                        if (((this.TryImportWsspRequireDerivedKeysAssertion(collection2, parameters2) && this.TryImportWsspMustNotSendCancelAssertion(collection2, out flag)) && (this.TryImportWsspMustNotSendAmendAssertion(collection2) && this.TryImportWsspMustNotSendRenewAssertion(collection2, out flag2))) && (this.TryImportWsspBootstrapPolicyAssertion(importer, collection2, parameters2) && (collection2.Count == 0)))
                        {
                            parameters2.RequireCancellation = flag;
                            parameters2.CanRenewSession = flag2;
                            parameters2.InclusionMode = mode;
                            break;
                        }
                        parameters = null;
                    }
                }
                else
                {
                    parameters = new SecureConversationSecurityTokenParameters();
                    parameters.InclusionMode = mode;
                    parameters.RequireDerivedKeys = false;
                }
            }
            return (parameters != null);
        }

        private bool TryImportWsspSignedEncryptedSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> optionalSignedEncrypted, out XmlElement assertion)
        {
            Collection<Collection<XmlElement>> collection;
            if (signedEncrypted == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signedEncrypted");
            }
            if (optionalSignedEncrypted == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalSignedEncrypted");
            }
            bool flag = true;
            if (this.TryImportWsspAssertion(assertions, "SignedEncryptedSupportingTokens", out assertion) && this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    SecurityTokenParameters parameters;
                    bool flag2;
                    while ((collection2.Count > 0) && this.TryImportTokenAssertion(importer, policyContext, collection2, out parameters, out flag2))
                    {
                        if (flag2)
                        {
                            optionalSignedEncrypted.Add(parameters);
                        }
                        else
                        {
                            signedEncrypted.Add(parameters);
                        }
                    }
                    if (collection2.Count == 0)
                    {
                        return true;
                    }
                    flag = false;
                }
            }
            return flag;
        }

        private bool TryImportWsspSignedSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> optionalSigned, out XmlElement assertion)
        {
            Collection<Collection<XmlElement>> collection;
            if (signed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signed");
            }
            if (optionalSigned == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalSigned");
            }
            bool flag = true;
            if (this.TryImportWsspAssertion(assertions, "SignedSupportingTokens", out assertion) && this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    SecurityTokenParameters parameters;
                    bool flag2;
                    while ((collection2.Count > 0) && this.TryImportTokenAssertion(importer, policyContext, collection2, out parameters, out flag2))
                    {
                        if (flag2)
                        {
                            optionalSigned.Add(parameters);
                        }
                        else
                        {
                            signed.Add(parameters);
                        }
                    }
                    if (collection2.Count == 0)
                    {
                        return true;
                    }
                    flag = false;
                }
            }
            return flag;
        }

        public override bool TryImportWsspSpnegoContextTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            SecurityTokenInclusionMode mode;
            parameters = null;
            if (this.IsWsspAssertion(assertion, "SpnegoContextToken") && this.TryGetIncludeTokenValue(assertion, out mode))
            {
                Collection<Collection<XmlElement>> collection;
                if (this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
                {
                    foreach (Collection<XmlElement> collection2 in collection)
                    {
                        bool flag;
                        bool flag2;
                        SspiSecurityTokenParameters parameters2 = new SspiSecurityTokenParameters();
                        parameters = parameters2;
                        if (((this.TryImportWsspRequireDerivedKeysAssertion(collection2, parameters2) && this.TryImportWsspMustNotSendCancelAssertion(collection2, out flag)) && (this.TryImportWsspMustNotSendAmendAssertion(collection2) && this.TryImportWsspMustNotSendRenewAssertion(collection2, out flag2))) && (collection2.Count == 0))
                        {
                            parameters2.RequireCancellation = true;
                            parameters2.InclusionMode = mode;
                            break;
                        }
                        parameters = null;
                    }
                }
                else
                {
                    parameters = new SspiSecurityTokenParameters();
                    parameters.RequireDerivedKeys = false;
                    parameters.InclusionMode = mode;
                }
            }
            return (parameters != null);
        }

        public override bool TryImportWsspSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, Collection<SecurityTokenParameters> optionalEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing)
        {
            XmlElement element;
            if (!this.TryImportWsspSignedSupportingTokensAssertion(importer, policyContext, assertions, signed, optionalSigned, out element) && (element != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element.OuterXml })));
            }
            if (!this.TryImportWsspSignedEncryptedSupportingTokensAssertion(importer, policyContext, assertions, signedEncrypted, optionalSignedEncrypted, out element) && (element != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element.OuterXml })));
            }
            if (!base.TryImportWsspEndorsingSupportingTokensAssertion(importer, policyContext, assertions, endorsing, optionalEndorsing, out element) && (element != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element.OuterXml })));
            }
            if (!base.TryImportWsspSignedEndorsingSupportingTokensAssertion(importer, policyContext, assertions, signedEndorsing, optionalSignedEndorsing, out element) && (element != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element.OuterXml })));
            }
            return true;
        }

        public override bool TryImportWsspTrustAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding, out XmlElement assertion)
        {
            return base.TryImportWsspTrustAssertion("Trust13", importer, assertions, binding, out assertion);
        }

        public override System.ServiceModel.Security.TrustDriver TrustDriver
        {
            get
            {
                return new WSTrustDec2005.DriverDec2005(new SecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12, WSSecurityTokenSerializer.DefaultInstance));
            }
        }

        public override string WsspNamespaceUri
        {
            get
            {
                return "http://docs.oasis-open.org/ws-sx/ws-securitypolicy/200702";
            }
        }
    }
}

