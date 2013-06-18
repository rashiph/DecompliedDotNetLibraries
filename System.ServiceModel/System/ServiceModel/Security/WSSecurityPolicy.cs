namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    internal abstract class WSSecurityPolicy
    {
        private bool _mustSupportRefIssuerSerialName;
        private bool _mustSupportRefKeyIdentifierName;
        private bool _mustSupportRefThumbprintName;
        private bool _protectionTokenHasAsymmetricKey;
        public const string AlgorithmSuiteName = "AlgorithmSuite";
        public const string AsymmetricBindingName = "AsymmetricBinding";
        public const string Basic128Name = "Basic128";
        public const string Basic128Rsa15Name = "Basic128Rsa15";
        public const string Basic128Sha256Name = "Basic128Sha256";
        public const string Basic128Sha256Rsa15Name = "Basic128Sha256Rsa15";
        public const string Basic192Name = "Basic192";
        public const string Basic192Rsa15Name = "Basic192Rsa15";
        public const string Basic192Sha256Name = "Basic192Sha256";
        public const string Basic192Sha256Rsa15Name = "Basic192Sha256Rsa15";
        public const string Basic256Name = "Basic256";
        public const string Basic256Rsa15Name = "Basic256Rsa15";
        public const string Basic256Sha256Name = "Basic256Sha256";
        public const string Basic256Sha256Rsa15Name = "Basic256Sha256Rsa15";
        public const string BodyName = "Body";
        public const string BootstrapPolicyName = "BootstrapPolicy";
        public static XmlDocument doc = new XmlDocument();
        public const string EncryptBeforeSigningName = "EncryptBeforeSigning";
        public const string EncryptedPartsName = "EncryptedParts";
        public const string EncryptSignatureName = "EncryptSignature";
        public const string EndorsingSupportingTokensName = "EndorsingSupportingTokens";
        public const string FalseName = "false";
        public const string HeaderName = "Header";
        public const string HttpBasicAuthenticationName = "HttpBasicAuthentication";
        public const string HttpDigestAuthenticationName = "HttpDigestAuthentication";
        public const string HttpsTokenName = "HttpsToken";
        public const string IncludeTimestampName = "IncludeTimestamp";
        public const string IncludeTokenName = "IncludeToken";
        public const string InitiatorTokenName = "InitiatorToken";
        public const string IssuedTokenName = "IssuedToken";
        public const string IssuerName = "Issuer";
        public const string KerberosTokenName = "KerberosToken";
        public const string KeyValueTokenName = "KeyValueToken";
        public const string LaxName = "Lax";
        public const string LaxTsFirstName = "LaxTsFirst";
        public const string LaxTsLastName = "LaxTsLast";
        public const string LayoutName = "Layout";
        public const string MsspNamespace = "http://schemas.microsoft.com/ws/2005/07/securitypolicy";
        public const string MsspPrefix = "mssp";
        public const string MustNotSendAmendName = "MustNotSendAmend";
        public const string MustNotSendCancelName = "MustNotSendCancel";
        public const string MustNotSendRenewName = "MustNotSendRenew";
        public const string MustSupportIssuedTokensName = "MustSupportIssuedTokens";
        public const string MustSupportRefEncryptedKeyName = "MustSupportRefEncryptedKey";
        public const string MustSupportRefIssuerSerialName = "MustSupportRefIssuerSerial";
        public const string MustSupportRefKeyIdentifierName = "MustSupportRefKeyIdentifier";
        public const string MustSupportRefThumbprintName = "MustSupportRefThumbprint";
        public const string NameName = "Name";
        public const string NamespaceName = "Namespace";
        public static ContractDescription NullContract = new ContractDescription("null");
        public static ServiceEndpoint NullServiceEndpoint = new ServiceEndpoint(NullContract);
        public const string OnlySignEntireHeadersAndBodyName = "OnlySignEntireHeadersAndBody";
        public const string OptionalName = "Optional";
        public const string PolicyName = "Policy";
        public const string ProtectionTokenName = "ProtectionToken";
        public const string RecipientTokenName = "RecipientToken";
        public const string RequestSecurityTokenTemplateName = "RequestSecurityTokenTemplate";
        public const string RequireAppliesTo = "RequireAppliesTo";
        public const string RequireClientCertificateName = "RequireClientCertificate";
        public const string RequireClientEntropyName = "RequireClientEntropy";
        public const string RequireDerivedKeysName = "RequireDerivedKeys";
        public const string RequireExternalReferenceName = "RequireExternalReference";
        public const string RequireInternalReferenceName = "RequireInternalReference";
        public const string RequireIssuerSerialReferenceName = "RequireIssuerSerialReference";
        public const string RequireKeyIdentifierReferenceName = "RequireKeyIdentifierReference";
        public const string RequireServerEntropyName = "RequireServerEntropy";
        public const string RequireSignatureConfirmationName = "RequireSignatureConfirmation";
        public const string RequireThumbprintReferenceName = "RequireThumbprintReference";
        public const string RsaTokenName = "RsaToken";
        public const string SecureConversationTokenName = "SecureConversationToken";
        public const string SignedEndorsingSupportingTokensName = "SignedEndorsingSupportingTokens";
        public const string SignedPartsName = "SignedParts";
        public const string SignedSupportingTokensName = "SignedSupportingTokens";
        public const string SpnegoContextTokenName = "SpnegoContextToken";
        public const string SslContextTokenName = "SslContextToken";
        public const string StrictName = "Strict";
        public const string SymmetricBindingName = "SymmetricBinding";
        public const string TransportBindingName = "TransportBinding";
        public const string TransportTokenName = "TransportToken";
        public const string TripleDesName = "TripleDes";
        public const string TripleDesRsa15Name = "TripleDesRsa15";
        public const string TripleDesSha256Name = "TripleDesSha256";
        public const string TripleDesSha256Rsa15Name = "TripleDesSha256Rsa15";
        public const string TrueName = "true";
        public const string Trust10Name = "Trust10";
        public const string Trust13Name = "Trust13";
        public const string UsernameTokenName = "UsernameToken";
        public const string Wsp15Namespace = "http://www.w3.org/ns/ws-policy";
        public const string WspNamespace = "http://schemas.xmlsoap.org/ws/2004/09/policy";
        public const string WspPrefix = "wsp";
        public const string Wss10Name = "Wss10";
        public const string Wss11Name = "Wss11";
        public const string WssGssKerberosV5ApReqToken11Name = "WssGssKerberosV5ApReqToken11";
        public const string WsspPrefix = "sp";
        public const string WssUsernameToken10Name = "WssUsernameToken10";
        public const string WssX509V3Token10Name = "WssX509V3Token10";
        public const string X509TokenName = "X509Token";

        protected WSSecurityPolicy()
        {
        }

        public virtual bool CanImportAssertion(ICollection<XmlElement> assertions)
        {
            foreach (XmlElement element in assertions)
            {
                if ((element.NamespaceURI == this.WsspNamespaceUri) || (element.NamespaceURI == "http://schemas.microsoft.com/ws/2005/07/securitypolicy"))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ContainsEncryptionParts(PolicyConversionContext policyContext, SecurityBindingElement security)
        {
            if (policyContext.Contract == NullContract)
            {
                return true;
            }
            if ((security.EndpointSupportingTokenParameters.SignedEncrypted.Count > 0) || (security.OptionalEndpointSupportingTokenParameters.SignedEncrypted.Count > 0))
            {
                return true;
            }
            foreach (SupportingTokenParameters parameters in security.OperationSupportingTokenParameters.Values)
            {
                if (parameters.SignedEncrypted.Count > 0)
                {
                    return true;
                }
            }
            foreach (SupportingTokenParameters parameters2 in security.OptionalOperationSupportingTokenParameters.Values)
            {
                if (parameters2.SignedEncrypted.Count > 0)
                {
                    return true;
                }
            }
            BindingParameterCollection parameterCollection = new BindingParameterCollection();
            parameterCollection.Add(ChannelProtectionRequirements.CreateFromContract(policyContext.Contract, policyContext.BindingElements.Find<SecurityBindingElement>().GetIndividualProperty<ISecurityCapabilities>(), false));
            ChannelProtectionRequirements requirements = SecurityBindingElement.ComputeProtectionRequirements(security, parameterCollection, policyContext.BindingElements, true);
            requirements.MakeReadOnly();
            GetSecurityPolicyDriver(security.MessageSecurityVersion);
            foreach (OperationDescription description in policyContext.Contract.Operations)
            {
                foreach (MessageDescription description2 in description.Messages)
                {
                    MessagePartSpecification specification;
                    ScopedMessagePartSpecification incomingEncryptionParts;
                    if (description2.Direction == MessageDirection.Input)
                    {
                        incomingEncryptionParts = requirements.IncomingEncryptionParts;
                    }
                    else
                    {
                        incomingEncryptionParts = requirements.OutgoingEncryptionParts;
                    }
                    if (incomingEncryptionParts.TryGetParts(description2.Action, out specification) && !specification.IsEmpty())
                    {
                        return true;
                    }
                }
                foreach (FaultDescription description3 in description.Faults)
                {
                    MessagePartSpecification specification3;
                    if (requirements.OutgoingEncryptionParts.TryGetParts(description3.Action, out specification3) && !specification3.IsEmpty())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual bool ContainsWsspHttpsTokenAssertion(ICollection<XmlElement> assertions)
        {
            return (PolicyConversionContext.FindAssertion(assertions, "HttpsToken", this.WsspNamespaceUri, false) != null);
        }

        public virtual XmlElement CreateAlgorithmSuiteAssertion(SecurityAlgorithmSuite suite)
        {
            if (suite == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("suite");
            }
            if (suite == SecurityAlgorithmSuite.Basic256)
            {
                return this.CreateWsspAssertion("Basic256");
            }
            if (suite == SecurityAlgorithmSuite.Basic192)
            {
                return this.CreateWsspAssertion("Basic192");
            }
            if (suite == SecurityAlgorithmSuite.Basic128)
            {
                return this.CreateWsspAssertion("Basic128");
            }
            if (suite == SecurityAlgorithmSuite.TripleDes)
            {
                return this.CreateWsspAssertion("TripleDes");
            }
            if (suite == SecurityAlgorithmSuite.Basic256Rsa15)
            {
                return this.CreateWsspAssertion("Basic256Rsa15");
            }
            if (suite == SecurityAlgorithmSuite.Basic192Rsa15)
            {
                return this.CreateWsspAssertion("Basic192Rsa15");
            }
            if (suite == SecurityAlgorithmSuite.Basic128Rsa15)
            {
                return this.CreateWsspAssertion("Basic128Rsa15");
            }
            if (suite == SecurityAlgorithmSuite.TripleDesRsa15)
            {
                return this.CreateWsspAssertion("TripleDesRsa15");
            }
            if (suite == SecurityAlgorithmSuite.Basic256Sha256)
            {
                return this.CreateWsspAssertion("Basic256Sha256");
            }
            if (suite == SecurityAlgorithmSuite.Basic192Sha256)
            {
                return this.CreateWsspAssertion("Basic192Sha256");
            }
            if (suite == SecurityAlgorithmSuite.Basic128Sha256)
            {
                return this.CreateWsspAssertion("Basic128Sha256");
            }
            if (suite == SecurityAlgorithmSuite.TripleDesSha256)
            {
                return this.CreateWsspAssertion("TripleDesSha256");
            }
            if (suite == SecurityAlgorithmSuite.Basic256Sha256Rsa15)
            {
                return this.CreateWsspAssertion("Basic256Sha256Rsa15");
            }
            if (suite == SecurityAlgorithmSuite.Basic192Sha256Rsa15)
            {
                return this.CreateWsspAssertion("Basic192Sha256Rsa15");
            }
            if (suite == SecurityAlgorithmSuite.Basic128Sha256Rsa15)
            {
                return this.CreateWsspAssertion("Basic128Sha256Rsa15");
            }
            if (suite != SecurityAlgorithmSuite.TripleDesSha256Rsa15)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("suite"));
            }
            return this.CreateWsspAssertion("TripleDesSha256Rsa15");
        }

        public virtual XmlElement CreateLayoutAssertion(SecurityHeaderLayout layout)
        {
            switch (layout)
            {
                case SecurityHeaderLayout.Strict:
                    return this.CreateWsspAssertion("Strict");

                case SecurityHeaderLayout.Lax:
                    return this.CreateWsspAssertion("Lax");

                case SecurityHeaderLayout.LaxTimestampFirst:
                    return this.CreateWsspAssertion("LaxTsFirst");

                case SecurityHeaderLayout.LaxTimestampLast:
                    return this.CreateWsspAssertion("LaxTsLast");
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("layout"));
        }

        public virtual XmlElement CreateMsspAssertion(string name)
        {
            return doc.CreateElement("mssp", name, "http://schemas.microsoft.com/ws/2005/07/securitypolicy");
        }

        public virtual XmlElement CreateMsspRequireClientCertificateAssertion(bool requireClientCertificate)
        {
            if (requireClientCertificate)
            {
                return this.CreateMsspAssertion("RequireClientCertificate");
            }
            return null;
        }

        public virtual XmlElement CreateMsspSslContextTokenAssertion(MetadataExporter exporter, SslSecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateMsspAssertion("SslContextToken");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            tokenAssertion.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys), this.CreateWsspMustNotSendCancelAssertion(parameters.RequireCancellation), this.CreateMsspRequireClientCertificateAssertion(parameters.RequireClientCertificate) }));
            return tokenAssertion;
        }

        public virtual XmlElement CreateReferenceStyleAssertion(SecurityTokenReferenceStyle referenceStyle)
        {
            switch (referenceStyle)
            {
                case SecurityTokenReferenceStyle.Internal:
                    return this.CreateWsspAssertion("RequireInternalReference");

                case SecurityTokenReferenceStyle.External:
                    return this.CreateWsspAssertion("RequireExternalReference");
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("referenceStyle"));
        }

        public virtual XmlElement CreateTokenAssertion(MetadataExporter exporter, SecurityTokenParameters parameters)
        {
            return this.CreateTokenAssertion(exporter, parameters, false);
        }

        public virtual XmlElement CreateTokenAssertion(MetadataExporter exporter, SecurityTokenParameters parameters, bool isOptional)
        {
            XmlElement element;
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            if (parameters is KerberosSecurityTokenParameters)
            {
                element = this.CreateWsspKerberosTokenAssertion(exporter, (KerberosSecurityTokenParameters) parameters);
            }
            else if (parameters is X509SecurityTokenParameters)
            {
                element = this.CreateWsspX509TokenAssertion(exporter, (X509SecurityTokenParameters) parameters);
            }
            else if (parameters is UserNameSecurityTokenParameters)
            {
                element = this.CreateWsspUsernameTokenAssertion(exporter, (UserNameSecurityTokenParameters) parameters);
            }
            else if (parameters is IssuedSecurityTokenParameters)
            {
                element = this.CreateWsspIssuedTokenAssertion(exporter, (IssuedSecurityTokenParameters) parameters);
            }
            else if (parameters is SspiSecurityTokenParameters)
            {
                element = this.CreateWsspSpnegoContextTokenAssertion(exporter, (SspiSecurityTokenParameters) parameters);
            }
            else if (parameters is SslSecurityTokenParameters)
            {
                element = this.CreateMsspSslContextTokenAssertion(exporter, (SslSecurityTokenParameters) parameters);
            }
            else if (parameters is SecureConversationSecurityTokenParameters)
            {
                element = this.CreateWsspSecureConversationTokenAssertion(exporter, (SecureConversationSecurityTokenParameters) parameters);
            }
            else
            {
                if (!(parameters is RsaSecurityTokenParameters))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("parameters"));
                }
                element = this.CreateWsspRsaTokenAssertion((RsaSecurityTokenParameters) parameters);
            }
            if ((element != null) && isOptional)
            {
                element.SetAttribute("Optional", exporter.PolicyVersion.Namespace, "true");
            }
            return element;
        }

        public virtual XmlElement CreateWspPolicyWrapper(MetadataExporter exporter, params XmlElement[] nestedAssertions)
        {
            XmlElement element = doc.CreateElement("wsp", "Policy", exporter.PolicyVersion.Namespace);
            if (nestedAssertions != null)
            {
                foreach (XmlElement element2 in nestedAssertions)
                {
                    if (element2 != null)
                    {
                        element.AppendChild(element2);
                    }
                }
            }
            return element;
        }

        public virtual XmlElement CreateWsspAlgorithmSuiteAssertion(MetadataExporter exporter, SecurityAlgorithmSuite suite)
        {
            XmlElement element = this.CreateWsspAssertion("AlgorithmSuite");
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateAlgorithmSuiteAssertion(suite) }));
            return element;
        }

        public virtual XmlElement CreateWsspAssertion(string name)
        {
            return doc.CreateElement("sp", name, this.WsspNamespaceUri);
        }

        public virtual XmlElement CreateWsspAssertionMustSupportRefEncryptedKeyName()
        {
            if (this._protectionTokenHasAsymmetricKey)
            {
                return this.CreateWsspAssertion("MustSupportRefEncryptedKey");
            }
            return null;
        }

        public virtual XmlElement CreateWsspAssertionMustSupportRefIssuerSerialName()
        {
            if (this._mustSupportRefIssuerSerialName)
            {
                return this.CreateWsspAssertion("MustSupportRefIssuerSerial");
            }
            return null;
        }

        public virtual XmlElement CreateWsspAssertionMustSupportRefKeyIdentifierName()
        {
            if (this._mustSupportRefKeyIdentifierName)
            {
                return this.CreateWsspAssertion("MustSupportRefKeyIdentifier");
            }
            return null;
        }

        public virtual XmlElement CreateWsspAssertionMustSupportRefThumbprintName()
        {
            if (this._mustSupportRefThumbprintName)
            {
                return this.CreateWsspAssertion("MustSupportRefThumbprint");
            }
            return null;
        }

        public virtual XmlElement CreateWsspAsymmetricBindingAssertion(MetadataExporter exporter, PolicyConversionContext policyContext, AsymmetricSecurityBindingElement binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            XmlElement element = this.CreateWsspAssertion("AsymmetricBinding");
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspInitiatorTokenAssertion(exporter, binding.InitiatorTokenParameters), this.CreateWsspRecipientTokenAssertion(exporter, binding.RecipientTokenParameters), this.CreateWsspAlgorithmSuiteAssertion(exporter, binding.DefaultAlgorithmSuite), this.CreateWsspLayoutAssertion(exporter, binding.SecurityHeaderLayout), this.CreateWsspIncludeTimestampAssertion(binding.IncludeTimestamp), this.CreateWsspEncryptBeforeSigningAssertion(binding.MessageProtectionOrder), this.CreateWsspEncryptSignatureAssertion(policyContext, binding), this.CreateWsspAssertion("OnlySignEntireHeadersAndBody") }));
            return element;
        }

        public virtual XmlElement CreateWsspBootstrapPolicyAssertion(MetadataExporter exporter, SecurityBindingElement bootstrapSecurity)
        {
            if (bootstrapSecurity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bootstrapBinding");
            }
            WSSecurityPolicy securityPolicyDriver = GetSecurityPolicyDriver(bootstrapSecurity.MessageSecurityVersion);
            CustomBinding binding = new CustomBinding(new BindingElement[] { bootstrapSecurity });
            if (exporter.State.ContainsKey("SecureConversationBootstrapBindingElementsBelowSecurityKey"))
            {
                BindingElementCollection elements = exporter.State["SecureConversationBootstrapBindingElementsBelowSecurityKey"] as BindingElementCollection;
                if (elements != null)
                {
                    foreach (BindingElement element in elements)
                    {
                        binding.Elements.Add(element);
                    }
                }
            }
            ServiceEndpoint endpoint = new ServiceEndpoint(NullContract) {
                Binding = binding
            };
            PolicyConversionContext context = exporter.ExportPolicy(endpoint);
            ChannelProtectionRequirements requirements = new ChannelProtectionRequirements();
            requirements.IncomingEncryptionParts.AddParts(new MessagePartSpecification(true));
            requirements.OutgoingEncryptionParts.AddParts(new MessagePartSpecification(true));
            requirements.IncomingSignatureParts.AddParts(new MessagePartSpecification(true));
            requirements.OutgoingSignatureParts.AddParts(new MessagePartSpecification(true));
            ChannelProtectionRequirements property = binding.GetProperty<ChannelProtectionRequirements>(new BindingParameterCollection());
            if (property != null)
            {
                requirements.Add(property);
            }
            MessagePartSpecification parts = new MessagePartSpecification();
            parts.Union(requirements.IncomingEncryptionParts.ChannelParts);
            parts.Union(requirements.OutgoingEncryptionParts.ChannelParts);
            parts.MakeReadOnly();
            MessagePartSpecification specification2 = new MessagePartSpecification();
            specification2.Union(requirements.IncomingSignatureParts.ChannelParts);
            specification2.Union(requirements.OutgoingSignatureParts.ChannelParts);
            specification2.MakeReadOnly();
            XmlElement newChild = this.CreateWspPolicyWrapper(exporter, new XmlElement[] { securityPolicyDriver.CreateWsspSignedPartsAssertion(specification2), securityPolicyDriver.CreateWsspEncryptedPartsAssertion(parts) });
            foreach (XmlElement element3 in securityPolicyDriver.FilterWsspPolicyAssertions(context.GetBindingAssertions()))
            {
                newChild.AppendChild(element3);
            }
            XmlElement element4 = this.CreateWsspAssertion("BootstrapPolicy");
            element4.AppendChild(newChild);
            return element4;
        }

        public virtual XmlElement CreateWsspEncryptBeforeSigningAssertion(MessageProtectionOrder protectionOrder)
        {
            if (protectionOrder == MessageProtectionOrder.EncryptBeforeSign)
            {
                return this.CreateWsspAssertion("EncryptBeforeSigning");
            }
            return null;
        }

        public virtual XmlElement CreateWsspEncryptedPartsAssertion(MessagePartSpecification parts)
        {
            if (parts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parts");
            }
            if (parts.IsEmpty())
            {
                return null;
            }
            XmlElement element = this.CreateWsspAssertion("EncryptedParts");
            if (parts.IsBodyIncluded)
            {
                element.AppendChild(this.CreateWsspAssertion("Body"));
            }
            foreach (XmlQualifiedName name in parts.HeaderTypes)
            {
                element.AppendChild(this.CreateWsspHeaderAssertion(name));
            }
            return element;
        }

        public virtual XmlElement CreateWsspEncryptSignatureAssertion(PolicyConversionContext policyContext, SecurityBindingElement binding)
        {
            MessageProtectionOrder messageProtectionOrder;
            if (binding is SymmetricSecurityBindingElement)
            {
                messageProtectionOrder = ((SymmetricSecurityBindingElement) binding).MessageProtectionOrder;
            }
            else
            {
                messageProtectionOrder = ((AsymmetricSecurityBindingElement) binding).MessageProtectionOrder;
            }
            if ((messageProtectionOrder == MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature) && this.ContainsEncryptionParts(policyContext, binding))
            {
                return this.CreateWsspAssertion("EncryptSignature");
            }
            return null;
        }

        protected XmlElement CreateWsspEndorsingSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> optionalEndorsing, AddressingVersion addressingVersion)
        {
            return this.CreateWsspiSupportingTokensAssertion(exporter, endorsing, optionalEndorsing, addressingVersion, "EndorsingSupportingTokens");
        }

        public virtual XmlElement CreateWsspHeaderAssertion(XmlQualifiedName header)
        {
            XmlElement element = this.CreateWsspAssertion("Header");
            element.SetAttribute("Name", header.Name);
            element.SetAttribute("Namespace", header.Namespace);
            return element;
        }

        public abstract XmlElement CreateWsspHttpsTokenAssertion(MetadataExporter exporter, HttpsTransportBindingElement httpsBinding);
        public virtual XmlElement CreateWsspIncludeTimestampAssertion(bool includeTimestamp)
        {
            if (includeTimestamp)
            {
                return this.CreateWsspAssertion("IncludeTimestamp");
            }
            return null;
        }

        public virtual XmlElement CreateWsspInitiatorTokenAssertion(MetadataExporter exporter, SecurityTokenParameters parameters)
        {
            XmlElement element = this.CreateWsspAssertion("InitiatorToken");
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateTokenAssertion(exporter, parameters) }));
            return element;
        }

        public virtual XmlElement CreateWsspIssuedTokenAssertion(MetadataExporter exporter, IssuedSecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateWsspAssertion("IssuedToken");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            XmlElement newChild = this.CreateWsspIssuerElement(parameters.IssuerAddress, parameters.IssuerMetadataAddress);
            if (newChild != null)
            {
                tokenAssertion.AppendChild(newChild);
            }
            XmlElement element3 = this.CreateWsspAssertion("RequestSecurityTokenTemplate");
            System.ServiceModel.Security.TrustDriver trustDriver = this.TrustDriver;
            foreach (XmlElement element4 in parameters.CreateRequestParameters(trustDriver))
            {
                element3.AppendChild(doc.ImportNode(element4, true));
            }
            tokenAssertion.AppendChild(element3);
            tokenAssertion.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys), this.CreateReferenceStyleAssertion(parameters.ReferenceStyle) }));
            return tokenAssertion;
        }

        public virtual XmlElement CreateWsspIssuerElement(EndpointAddress issuerAddress, EndpointAddress issuerMetadataAddress)
        {
            MemoryStream stream;
            XmlWriter writer;
            if ((issuerAddress == null) && (issuerMetadataAddress == null))
            {
                return null;
            }
            EndpointAddress address = (issuerAddress == null) ? EndpointAddress.AnonymousAddress : issuerAddress;
            if (issuerMetadataAddress != null)
            {
                MetadataSet set = new MetadataSet {
                    MetadataSections = { new MetadataSection(null, null, new MetadataReference(issuerMetadataAddress, AddressingVersion.WSAddressing10)) }
                };
                stream = new MemoryStream();
                writer = new XmlTextWriter(stream, Encoding.UTF8);
                set.WriteTo(XmlDictionaryWriter.CreateDictionaryWriter(writer));
                writer.Flush();
                stream.Seek(0L, SeekOrigin.Begin);
                address = new EndpointAddress(address.Uri, address.Identity, address.Headers, XmlDictionaryReader.CreateDictionaryReader(XmlReader.Create(stream)), address.GetReaderAtExtensions());
            }
            stream = new MemoryStream();
            writer = new XmlTextWriter(stream, Encoding.UTF8);
            writer.WriteStartElement("Issuer", this.WsspNamespaceUri);
            address.WriteContentsTo(AddressingVersion.WSAddressing10, writer);
            writer.WriteEndElement();
            writer.Flush();
            stream.Seek(0L, SeekOrigin.Begin);
            return (XmlElement) doc.ReadNode(new XmlTextReader(stream));
        }

        protected XmlElement CreateWsspiSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> optionalEndorsing, AddressingVersion addressingVersion, string assertionName)
        {
            bool flag = false;
            if (((endorsing == null) || (endorsing.Count == 0)) && ((optionalEndorsing == null) || (optionalEndorsing.Count == 0)))
            {
                return null;
            }
            XmlElement newChild = this.CreateWspPolicyWrapper(exporter, new XmlElement[0]);
            if (endorsing != null)
            {
                foreach (SecurityTokenParameters parameters in endorsing)
                {
                    if (parameters.HasAsymmetricKey)
                    {
                        flag = true;
                    }
                    newChild.AppendChild(this.CreateTokenAssertion(exporter, parameters));
                }
            }
            if (optionalEndorsing != null)
            {
                foreach (SecurityTokenParameters parameters2 in optionalEndorsing)
                {
                    if (parameters2.HasAsymmetricKey)
                    {
                        flag = true;
                    }
                    newChild.AppendChild(this.CreateTokenAssertion(exporter, parameters2, true));
                }
            }
            if (((addressingVersion != null) && (AddressingVersion.None != addressingVersion)) && flag)
            {
                newChild.AppendChild(this.CreateWsspSignedPartsAssertion(new MessagePartSpecification(new XmlQualifiedName[] { new XmlQualifiedName("To", addressingVersion.Namespace) })));
            }
            XmlElement element = this.CreateWsspAssertion(assertionName);
            element.AppendChild(newChild);
            return element;
        }

        public virtual XmlElement CreateWsspKerberosTokenAssertion(MetadataExporter exporter, KerberosSecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateWsspAssertion("KerberosToken");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            tokenAssertion.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys), this.CreateWsspAssertion("WssGssKerberosV5ApReqToken11") }));
            return tokenAssertion;
        }

        public virtual XmlElement CreateWsspLayoutAssertion(MetadataExporter exporter, SecurityHeaderLayout layout)
        {
            XmlElement element = this.CreateWsspAssertion("Layout");
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateLayoutAssertion(layout) }));
            return element;
        }

        public virtual XmlElement CreateWsspMustNotSendCancelAssertion(bool requireCancel)
        {
            if (!requireCancel)
            {
                return this.CreateWsspAssertion("MustNotSendCancel");
            }
            return null;
        }

        public virtual XmlElement CreateWsspProtectionTokenAssertion(MetadataExporter exporter, SecurityTokenParameters parameters)
        {
            XmlElement element = this.CreateWsspAssertion("ProtectionToken");
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateTokenAssertion(exporter, parameters) }));
            this._protectionTokenHasAsymmetricKey = parameters.HasAsymmetricKey;
            return element;
        }

        public virtual XmlElement CreateWsspRecipientTokenAssertion(MetadataExporter exporter, SecurityTokenParameters parameters)
        {
            XmlElement element = this.CreateWsspAssertion("RecipientToken");
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateTokenAssertion(exporter, parameters) }));
            return element;
        }

        public virtual XmlElement CreateWsspRequireClientEntropyAssertion(SecurityKeyEntropyMode keyEntropyMode)
        {
            if ((keyEntropyMode != SecurityKeyEntropyMode.ClientEntropy) && (keyEntropyMode != SecurityKeyEntropyMode.CombinedEntropy))
            {
                return null;
            }
            return this.CreateWsspAssertion("RequireClientEntropy");
        }

        public virtual XmlElement CreateWsspRequireDerivedKeysAssertion(bool requireDerivedKeys)
        {
            if (requireDerivedKeys)
            {
                return this.CreateWsspAssertion("RequireDerivedKeys");
            }
            return null;
        }

        public virtual XmlElement CreateWsspRequireServerEntropyAssertion(SecurityKeyEntropyMode keyEntropyMode)
        {
            if ((keyEntropyMode != SecurityKeyEntropyMode.ServerEntropy) && (keyEntropyMode != SecurityKeyEntropyMode.CombinedEntropy))
            {
                return null;
            }
            return this.CreateWsspAssertion("RequireServerEntropy");
        }

        public virtual XmlElement CreateWsspRequireSignatureConformationAssertion(bool requireSignatureConfirmation)
        {
            if (requireSignatureConfirmation)
            {
                return this.CreateWsspAssertion("RequireSignatureConfirmation");
            }
            return null;
        }

        public virtual XmlElement CreateWsspRsaTokenAssertion(RsaSecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateMsspAssertion("RsaToken");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            return tokenAssertion;
        }

        public virtual XmlElement CreateWsspSecureConversationTokenAssertion(MetadataExporter exporter, SecureConversationSecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateWsspAssertion("SecureConversationToken");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            tokenAssertion.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys), this.CreateWsspMustNotSendCancelAssertion(parameters.RequireCancellation), this.CreateWsspBootstrapPolicyAssertion(exporter, parameters.BootstrapSecurityBindingElement) }));
            return tokenAssertion;
        }

        protected XmlElement CreateWsspSignedEndorsingSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing, AddressingVersion addressingVersion)
        {
            return this.CreateWsspiSupportingTokensAssertion(exporter, signedEndorsing, optionalSignedEndorsing, addressingVersion, "SignedEndorsingSupportingTokens");
        }

        public virtual XmlElement CreateWsspSignedPartsAssertion(MessagePartSpecification parts)
        {
            if (parts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parts");
            }
            if (parts.IsEmpty())
            {
                return null;
            }
            XmlElement element = this.CreateWsspAssertion("SignedParts");
            if (parts.IsBodyIncluded)
            {
                element.AppendChild(this.CreateWsspAssertion("Body"));
            }
            foreach (XmlQualifiedName name in parts.HeaderTypes)
            {
                element.AppendChild(this.CreateWsspHeaderAssertion(name));
            }
            return element;
        }

        protected XmlElement CreateWsspSignedSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted)
        {
            if ((((signed == null) || (signed.Count == 0)) && ((signedEncrypted == null) || (signedEncrypted.Count == 0))) && (((optionalSigned == null) || (optionalSigned.Count == 0)) && ((optionalSignedEncrypted == null) || (optionalSignedEncrypted.Count == 0))))
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
            if (signedEncrypted != null)
            {
                foreach (SecurityTokenParameters parameters2 in signedEncrypted)
                {
                    newChild.AppendChild(this.CreateTokenAssertion(exporter, parameters2));
                }
            }
            if (optionalSigned != null)
            {
                foreach (SecurityTokenParameters parameters3 in optionalSigned)
                {
                    newChild.AppendChild(this.CreateTokenAssertion(exporter, parameters3, true));
                }
            }
            if (optionalSignedEncrypted != null)
            {
                foreach (SecurityTokenParameters parameters4 in optionalSignedEncrypted)
                {
                    newChild.AppendChild(this.CreateTokenAssertion(exporter, parameters4, true));
                }
            }
            XmlElement element = this.CreateWsspAssertion("SignedSupportingTokens");
            element.AppendChild(newChild);
            return element;
        }

        public virtual XmlElement CreateWsspSpnegoContextTokenAssertion(MetadataExporter exporter, SspiSecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateWsspAssertion("SpnegoContextToken");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            tokenAssertion.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys), this.CreateWsspMustNotSendCancelAssertion(parameters.RequireCancellation) }));
            return tokenAssertion;
        }

        public virtual Collection<XmlElement> CreateWsspSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, Collection<SecurityTokenParameters> optionalEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing)
        {
            return this.CreateWsspSupportingTokensAssertion(exporter, signed, signedEncrypted, endorsing, signedEndorsing, optionalSigned, optionalSignedEncrypted, optionalEndorsing, optionalSignedEndorsing, null);
        }

        public virtual Collection<XmlElement> CreateWsspSupportingTokensAssertion(MetadataExporter exporter, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, Collection<SecurityTokenParameters> optionalEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing, AddressingVersion addressingVersion)
        {
            Collection<XmlElement> collection = new Collection<XmlElement>();
            XmlElement item = this.CreateWsspSignedSupportingTokensAssertion(exporter, signed, signedEncrypted, optionalSigned, optionalSignedEncrypted);
            if (item != null)
            {
                collection.Add(item);
            }
            item = this.CreateWsspEndorsingSupportingTokensAssertion(exporter, endorsing, optionalEndorsing, addressingVersion);
            if (item != null)
            {
                collection.Add(item);
            }
            item = this.CreateWsspSignedEndorsingSupportingTokensAssertion(exporter, signedEndorsing, optionalSignedEndorsing, addressingVersion);
            if (item != null)
            {
                collection.Add(item);
            }
            return collection;
        }

        public virtual XmlElement CreateWsspSymmetricBindingAssertion(MetadataExporter exporter, PolicyConversionContext policyContext, SymmetricSecurityBindingElement binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            XmlElement element = this.CreateWsspAssertion("SymmetricBinding");
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspProtectionTokenAssertion(exporter, binding.ProtectionTokenParameters), this.CreateWsspAlgorithmSuiteAssertion(exporter, binding.DefaultAlgorithmSuite), this.CreateWsspLayoutAssertion(exporter, binding.SecurityHeaderLayout), this.CreateWsspIncludeTimestampAssertion(binding.IncludeTimestamp), this.CreateWsspEncryptBeforeSigningAssertion(binding.MessageProtectionOrder), this.CreateWsspEncryptSignatureAssertion(policyContext, binding), this.CreateWsspAssertion("OnlySignEntireHeadersAndBody") }));
            return element;
        }

        public virtual XmlElement CreateWsspTransportBindingAssertion(MetadataExporter exporter, TransportSecurityBindingElement binding, XmlElement transportTokenAssertion)
        {
            XmlElement element = this.CreateWsspAssertion("TransportBinding");
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspTransportTokenAssertion(exporter, transportTokenAssertion), this.CreateWsspAlgorithmSuiteAssertion(exporter, binding.DefaultAlgorithmSuite), this.CreateWsspLayoutAssertion(exporter, binding.SecurityHeaderLayout), this.CreateWsspIncludeTimestampAssertion(binding.IncludeTimestamp) }));
            return element;
        }

        public virtual XmlElement CreateWsspTransportTokenAssertion(MetadataExporter exporter, XmlElement transportTokenAssertion)
        {
            if (transportTokenAssertion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("transportTokenAssertion");
            }
            XmlElement element = this.CreateWsspAssertion("TransportToken");
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { (XmlElement) doc.ImportNode(transportTokenAssertion, true) }));
            return element;
        }

        public abstract XmlElement CreateWsspTrustAssertion(MetadataExporter exporter, SecurityKeyEntropyMode keyEntropyMode);
        protected XmlElement CreateWsspTrustAssertion(string trustName, MetadataExporter exporter, SecurityKeyEntropyMode keyEntropyMode)
        {
            XmlElement element = this.CreateWsspAssertion(trustName);
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspAssertion("MustSupportIssuedTokens"), this.CreateWsspRequireClientEntropyAssertion(keyEntropyMode), this.CreateWsspRequireServerEntropyAssertion(keyEntropyMode) }));
            return element;
        }

        public virtual XmlElement CreateWsspUsernameTokenAssertion(MetadataExporter exporter, UserNameSecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateWsspAssertion("UsernameToken");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            tokenAssertion.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspAssertion("WssUsernameToken10") }));
            return tokenAssertion;
        }

        public virtual XmlElement CreateWsspWss10Assertion(MetadataExporter exporter)
        {
            XmlElement element = this.CreateWsspAssertion("Wss10");
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspAssertionMustSupportRefKeyIdentifierName(), this.CreateWsspAssertionMustSupportRefIssuerSerialName() }));
            return element;
        }

        public virtual XmlElement CreateWsspWss11Assertion(MetadataExporter exporter, bool requireSignatureConfirmation)
        {
            XmlElement element = this.CreateWsspAssertion("Wss11");
            element.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspAssertionMustSupportRefKeyIdentifierName(), this.CreateWsspAssertionMustSupportRefIssuerSerialName(), this.CreateWsspAssertionMustSupportRefThumbprintName(), this.CreateWsspAssertionMustSupportRefEncryptedKeyName(), this.CreateWsspRequireSignatureConformationAssertion(requireSignatureConfirmation) }));
            return element;
        }

        public virtual XmlElement CreateWsspWssAssertion(MetadataExporter exporter, SecurityBindingElement binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (binding.MessageSecurityVersion.SecurityVersion == SecurityVersion.WSSecurity10)
            {
                return this.CreateWsspWss10Assertion(exporter);
            }
            if (binding.MessageSecurityVersion.SecurityVersion != SecurityVersion.WSSecurity11)
            {
                return null;
            }
            if (binding is SymmetricSecurityBindingElement)
            {
                return this.CreateWsspWss11Assertion(exporter, ((SymmetricSecurityBindingElement) binding).RequireSignatureConfirmation);
            }
            if (binding is AsymmetricSecurityBindingElement)
            {
                return this.CreateWsspWss11Assertion(exporter, ((AsymmetricSecurityBindingElement) binding).RequireSignatureConfirmation);
            }
            return this.CreateWsspWss11Assertion(exporter, false);
        }

        public virtual XmlElement CreateWsspX509TokenAssertion(MetadataExporter exporter, X509SecurityTokenParameters parameters)
        {
            XmlElement tokenAssertion = this.CreateWsspAssertion("X509Token");
            this.SetIncludeTokenValue(tokenAssertion, parameters.InclusionMode);
            tokenAssertion.AppendChild(this.CreateWspPolicyWrapper(exporter, new XmlElement[] { this.CreateWsspRequireDerivedKeysAssertion(parameters.RequireDerivedKeys), this.CreateX509ReferenceStyleAssertion(parameters.X509ReferenceStyle), this.CreateWsspAssertion("WssX509V3Token10") }));
            return tokenAssertion;
        }

        public virtual XmlElement CreateX509ReferenceStyleAssertion(X509KeyIdentifierClauseType referenceStyle)
        {
            switch (referenceStyle)
            {
                case X509KeyIdentifierClauseType.Any:
                    this._mustSupportRefIssuerSerialName = true;
                    this._mustSupportRefKeyIdentifierName = true;
                    this._mustSupportRefThumbprintName = true;
                    return null;

                case X509KeyIdentifierClauseType.Thumbprint:
                    this._mustSupportRefThumbprintName = true;
                    return this.CreateWsspAssertion("RequireThumbprintReference");

                case X509KeyIdentifierClauseType.IssuerSerial:
                    this._mustSupportRefIssuerSerialName = true;
                    return this.CreateWsspAssertion("RequireIssuerSerialReference");

                case X509KeyIdentifierClauseType.SubjectKeyIdentifier:
                    this._mustSupportRefKeyIdentifierName = true;
                    return this.CreateWsspAssertion("RequireKeyIdentifierReference");
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("referenceStyle"));
        }

        public virtual ICollection<XmlElement> FilterWsspPolicyAssertions(ICollection<XmlElement> policyAssertions)
        {
            Collection<XmlElement> collection = new Collection<XmlElement>();
            foreach (XmlElement element in policyAssertions)
            {
                if (this.IsWsspAssertion(element))
                {
                    collection.Add(element);
                }
            }
            return collection;
        }

        public static WSSecurityPolicy GetSecurityPolicyDriver(MessageSecurityVersion version)
        {
            SecurityPolicyManager manager = new SecurityPolicyManager();
            return manager.GetSecurityPolicyDriver(version);
        }

        public abstract MessageSecurityVersion GetSupportedMessageSecurityVersion(SecurityVersion version);
        public virtual bool IsMsspAssertion(XmlElement assertion, string name)
        {
            return ((assertion.NamespaceURI == "http://schemas.microsoft.com/ws/2005/07/securitypolicy") && (assertion.LocalName == name));
        }

        public abstract bool IsSecurityVersionSupported(MessageSecurityVersion version);
        public virtual bool IsWsspAssertion(XmlElement assertion)
        {
            return (assertion.NamespaceURI == this.WsspNamespaceUri);
        }

        public virtual bool IsWsspAssertion(XmlElement assertion, string name)
        {
            return ((assertion.NamespaceURI == this.WsspNamespaceUri) && (assertion.LocalName == name));
        }

        public virtual void SetIncludeTokenValue(XmlElement tokenAssertion, SecurityTokenInclusionMode inclusionMode)
        {
            switch (inclusionMode)
            {
                case SecurityTokenInclusionMode.AlwaysToRecipient:
                    tokenAssertion.SetAttribute("IncludeToken", this.WsspNamespaceUri, this.AlwaysToRecipientUri);
                    return;

                case SecurityTokenInclusionMode.Never:
                    tokenAssertion.SetAttribute("IncludeToken", this.WsspNamespaceUri, this.NeverUri);
                    return;

                case SecurityTokenInclusionMode.Once:
                    tokenAssertion.SetAttribute("IncludeToken", this.WsspNamespaceUri, this.OnceUri);
                    return;

                case SecurityTokenInclusionMode.AlwaysToInitiator:
                    tokenAssertion.SetAttribute("IncludeToken", this.WsspNamespaceUri, this.AlwaysToInitiatorUri);
                    return;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inclusionMode"));
        }

        public virtual bool TryGetIncludeTokenValue(XmlElement assertion, out SecurityTokenInclusionMode mode)
        {
            string attribute = assertion.GetAttribute("IncludeToken", this.WsspNamespaceUri);
            if (attribute == this.AlwaysToInitiatorUri)
            {
                mode = SecurityTokenInclusionMode.AlwaysToInitiator;
                return true;
            }
            if (attribute == this.AlwaysToRecipientUri)
            {
                mode = SecurityTokenInclusionMode.AlwaysToRecipient;
                return true;
            }
            if (attribute == this.NeverUri)
            {
                mode = SecurityTokenInclusionMode.Never;
                return true;
            }
            if (attribute == this.OnceUri)
            {
                mode = SecurityTokenInclusionMode.Once;
                return true;
            }
            mode = SecurityTokenInclusionMode.Never;
            return false;
        }

        public virtual bool TryGetIssuer(XmlElement assertion, out EndpointAddress issuer, out EndpointAddress issuerMetadata)
        {
            bool flag = true;
            issuer = null;
            issuerMetadata = null;
            foreach (System.Xml.XmlNode node in assertion.ChildNodes)
            {
                if ((node is XmlElement) && this.IsWsspAssertion((XmlElement) node, "Issuer"))
                {
                    try
                    {
                        issuer = EndpointAddress.ReadFrom(XmlDictionaryReader.CreateDictionaryReader(new XmlNodeReader(node)));
                        XmlDictionaryReader readerAtMetadata = issuer.GetReaderAtMetadata();
                        if (readerAtMetadata != null)
                        {
                            while (readerAtMetadata.MoveToContent() == XmlNodeType.Element)
                            {
                                if ((readerAtMetadata.Name == "Metadata") && (readerAtMetadata.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/mex"))
                                {
                                    foreach (MetadataSection section in MetadataSet.ReadFrom(readerAtMetadata).MetadataSections)
                                    {
                                        if (section.Metadata is MetadataReference)
                                        {
                                            issuerMetadata = ((MetadataReference) section.Metadata).Address;
                                        }
                                    }
                                    return flag;
                                }
                                readerAtMetadata.Skip();
                            }
                        }
                        return flag;
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (exception is NullReferenceException)
                        {
                            throw;
                        }
                        return false;
                    }
                }
            }
            return flag;
        }

        public virtual bool TryGetNestedPolicyAlternatives(MetadataImporter importer, XmlElement assertion, out Collection<Collection<XmlElement>> alternatives)
        {
            alternatives = null;
            XmlElement element = null;
            foreach (System.Xml.XmlNode node in assertion.ChildNodes)
            {
                if (((node is XmlElement) && (node.LocalName == "Policy")) && ((node.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/09/policy") || (node.NamespaceURI == "http://www.w3.org/ns/ws-policy")))
                {
                    element = (XmlElement) node;
                    break;
                }
            }
            if (element == null)
            {
                alternatives = null;
            }
            else
            {
                IEnumerable<IEnumerable<XmlElement>> enumerable = importer.NormalizePolicy(new XmlElement[] { element });
                alternatives = new Collection<Collection<XmlElement>>();
                foreach (IEnumerable<XmlElement> enumerable2 in enumerable)
                {
                    Collection<XmlElement> collection;
                    collection = new Collection<XmlElement> {
                        collection
                    };
                    foreach (XmlElement element2 in enumerable2)
                    {
                        collection.Add(element2);
                    }
                }
            }
            return (alternatives != null);
        }

        public virtual MessagePartSpecification TryGetProtectedParts(XmlElement assertion)
        {
            MessagePartSpecification specification = new MessagePartSpecification();
            foreach (System.Xml.XmlNode node in assertion.ChildNodes)
            {
                if ((node.NodeType != XmlNodeType.Whitespace) && (node.NodeType != XmlNodeType.Comment))
                {
                    if (!(node is XmlElement))
                    {
                        return null;
                    }
                    XmlElement element = (XmlElement) node;
                    if (!this.IsWsspAssertion(element, "Body"))
                    {
                        if (!this.IsWsspAssertion(element, "Header"))
                        {
                            return null;
                        }
                        string attribute = element.GetAttribute("Name");
                        string ns = element.GetAttribute("Namespace");
                        if (ns == null)
                        {
                            return null;
                        }
                        specification.HeaderTypes.Add(new XmlQualifiedName(attribute, ns));
                    }
                    else
                    {
                        specification.IsBodyIncluded = true;
                    }
                }
            }
            return specification;
        }

        public virtual bool TryGetRequestSecurityTokenTemplate(XmlElement assertion, out Collection<XmlElement> requestParameters)
        {
            requestParameters = null;
            foreach (System.Xml.XmlNode node in assertion.ChildNodes)
            {
                if ((node is XmlElement) && this.IsWsspAssertion((XmlElement) node, "RequestSecurityTokenTemplate"))
                {
                    requestParameters = new Collection<XmlElement>();
                    foreach (System.Xml.XmlNode node2 in node.ChildNodes)
                    {
                        if (node2 is XmlElement)
                        {
                            requestParameters.Add((XmlElement) node2);
                        }
                    }
                }
            }
            return (requestParameters != null);
        }

        public static bool TryGetSecurityPolicyDriver(ICollection<XmlElement> assertions, out WSSecurityPolicy securityPolicy)
        {
            SecurityPolicyManager manager = new SecurityPolicyManager();
            return manager.TryGetSecurityPolicyDriver(assertions, out securityPolicy);
        }

        public virtual bool TryImportAlgorithmSuiteAssertion(ICollection<XmlElement> assertions, out SecurityAlgorithmSuite suite)
        {
            if (this.TryImportWsspAssertion(assertions, "Basic256"))
            {
                suite = SecurityAlgorithmSuite.Basic256;
            }
            else if (this.TryImportWsspAssertion(assertions, "Basic192"))
            {
                suite = SecurityAlgorithmSuite.Basic192;
            }
            else if (this.TryImportWsspAssertion(assertions, "Basic128"))
            {
                suite = SecurityAlgorithmSuite.Basic128;
            }
            else if (this.TryImportWsspAssertion(assertions, "TripleDes"))
            {
                suite = SecurityAlgorithmSuite.TripleDes;
            }
            else if (this.TryImportWsspAssertion(assertions, "Basic256Rsa15"))
            {
                suite = SecurityAlgorithmSuite.Basic256Rsa15;
            }
            else if (this.TryImportWsspAssertion(assertions, "Basic192Rsa15"))
            {
                suite = SecurityAlgorithmSuite.Basic192Rsa15;
            }
            else if (this.TryImportWsspAssertion(assertions, "Basic128Rsa15"))
            {
                suite = SecurityAlgorithmSuite.Basic128Rsa15;
            }
            else if (this.TryImportWsspAssertion(assertions, "TripleDesRsa15"))
            {
                suite = SecurityAlgorithmSuite.TripleDesRsa15;
            }
            else if (this.TryImportWsspAssertion(assertions, "Basic256Sha256"))
            {
                suite = SecurityAlgorithmSuite.Basic256Sha256;
            }
            else if (this.TryImportWsspAssertion(assertions, "Basic192Sha256"))
            {
                suite = SecurityAlgorithmSuite.Basic192Sha256;
            }
            else if (this.TryImportWsspAssertion(assertions, "Basic128Sha256"))
            {
                suite = SecurityAlgorithmSuite.Basic128Sha256;
            }
            else if (this.TryImportWsspAssertion(assertions, "TripleDesSha256"))
            {
                suite = SecurityAlgorithmSuite.TripleDesSha256;
            }
            else if (this.TryImportWsspAssertion(assertions, "Basic256Sha256Rsa15"))
            {
                suite = SecurityAlgorithmSuite.Basic256Sha256Rsa15;
            }
            else if (this.TryImportWsspAssertion(assertions, "Basic192Sha256Rsa15"))
            {
                suite = SecurityAlgorithmSuite.Basic192Sha256Rsa15;
            }
            else if (this.TryImportWsspAssertion(assertions, "Basic128Sha256Rsa15"))
            {
                suite = SecurityAlgorithmSuite.Basic128Sha256Rsa15;
            }
            else if (this.TryImportWsspAssertion(assertions, "TripleDesSha256Rsa15"))
            {
                suite = SecurityAlgorithmSuite.TripleDesSha256Rsa15;
            }
            else
            {
                suite = null;
            }
            return (suite != null);
        }

        public virtual bool TryImportLayoutAssertion(ICollection<XmlElement> assertions, out SecurityHeaderLayout layout)
        {
            bool flag = true;
            layout = SecurityHeaderLayout.Lax;
            if (this.TryImportWsspAssertion(assertions, "Lax"))
            {
                layout = SecurityHeaderLayout.Lax;
                return flag;
            }
            if (this.TryImportWsspAssertion(assertions, "LaxTsFirst"))
            {
                layout = SecurityHeaderLayout.LaxTimestampFirst;
                return flag;
            }
            if (this.TryImportWsspAssertion(assertions, "LaxTsLast"))
            {
                layout = SecurityHeaderLayout.LaxTimestampLast;
                return flag;
            }
            if (this.TryImportWsspAssertion(assertions, "Strict"))
            {
                layout = SecurityHeaderLayout.Strict;
                return flag;
            }
            return false;
        }

        public virtual bool TryImportMessageProtectionOrderAssertions(ICollection<XmlElement> assertions, out MessageProtectionOrder order)
        {
            if (this.TryImportWsspAssertion(assertions, "EncryptBeforeSigning"))
            {
                order = MessageProtectionOrder.EncryptBeforeSign;
            }
            else if (this.TryImportWsspAssertion(assertions, "EncryptSignature"))
            {
                order = MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature;
            }
            else
            {
                order = MessageProtectionOrder.SignBeforeEncrypt;
            }
            return true;
        }

        public virtual bool TryImportMsspAssertion(ICollection<XmlElement> assertions, string name)
        {
            foreach (XmlElement element in assertions)
            {
                if ((element.LocalName == name) && (element.NamespaceURI == "http://schemas.microsoft.com/ws/2005/07/securitypolicy"))
                {
                    assertions.Remove(element);
                    return true;
                }
            }
            return false;
        }

        public virtual bool TryImportMsspRequireClientCertificateAssertion(ICollection<XmlElement> assertions, SslSecurityTokenParameters parameters)
        {
            parameters.RequireClientCertificate = this.TryImportMsspAssertion(assertions, "RequireClientCertificate");
            return true;
        }

        public virtual bool TryImportMsspSslContextTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
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
                        SslSecurityTokenParameters parameters2 = new SslSecurityTokenParameters();
                        parameters = parameters2;
                        if ((this.TryImportWsspRequireDerivedKeysAssertion(collection2, parameters2) && this.TryImportWsspMustNotSendCancelAssertion(collection2, out flag)) && (this.TryImportMsspRequireClientCertificateAssertion(collection2, parameters2) && (collection2.Count == 0)))
                        {
                            parameters2.RequireCancellation = flag;
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

        public virtual bool TryImportReferenceStyleAssertion(ICollection<XmlElement> assertions, IssuedSecurityTokenParameters parameters)
        {
            if (this.TryImportWsspAssertion(assertions, "RequireExternalReference"))
            {
                parameters.ReferenceStyle = SecurityTokenReferenceStyle.External;
            }
            else if (this.TryImportWsspAssertion(assertions, "RequireInternalReference"))
            {
                parameters.ReferenceStyle = SecurityTokenReferenceStyle.Internal;
            }
            return true;
        }

        public virtual bool TryImportTokenAssertion(MetadataImporter importer, PolicyConversionContext policyContext, Collection<XmlElement> assertions, out SecurityTokenParameters parameters, out bool isOptional)
        {
            parameters = null;
            isOptional = false;
            if (assertions.Count >= 1)
            {
                XmlElement assertion = assertions[0];
                if (((this.TryImportWsspKerberosTokenAssertion(importer, assertion, out parameters) || this.TryImportWsspX509TokenAssertion(importer, assertion, out parameters)) || (this.TryImportWsspUsernameTokenAssertion(importer, assertion, out parameters) || this.TryImportWsspIssuedTokenAssertion(importer, policyContext, assertion, out parameters))) || ((this.TryImportWsspSpnegoContextTokenAssertion(importer, assertion, out parameters) || this.TryImportMsspSslContextTokenAssertion(importer, assertion, out parameters)) || (this.TryImportWsspSecureConversationTokenAssertion(importer, assertion, out parameters) || this.TryImportWsspRsaTokenAssertion(importer, assertion, out parameters))))
                {
                    string attribute = assertion.GetAttribute("Optional", "http://schemas.xmlsoap.org/ws/2004/09/policy");
                    if (string.IsNullOrEmpty(attribute))
                    {
                        attribute = assertion.GetAttribute("Optional", "http://www.w3.org/ns/ws-policy");
                    }
                    try
                    {
                        isOptional = XmlUtil.IsTrue(attribute);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        if (exception is NullReferenceException)
                        {
                            throw;
                        }
                        importer.Errors.Add(new MetadataConversionError(System.ServiceModel.SR.GetString("UnsupportedBooleanAttribute", new object[] { "Optional", exception.Message }), false));
                        return false;
                    }
                    assertions.RemoveAt(0);
                }
            }
            return (parameters != null);
        }

        public virtual bool TryImportWsspAlgorithmSuiteAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding)
        {
            SecurityAlgorithmSuite suite = null;
            XmlElement element;
            Collection<Collection<XmlElement>> collection;
            if (this.TryImportWsspAssertion(assertions, "AlgorithmSuite", out element) && this.TryGetNestedPolicyAlternatives(importer, element, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    if (this.TryImportAlgorithmSuiteAssertion(collection2, out suite) && (collection2.Count == 0))
                    {
                        binding.DefaultAlgorithmSuite = suite;
                        break;
                    }
                    suite = null;
                }
            }
            return (suite != null);
        }

        public virtual bool TryImportWsspAssertion(ICollection<XmlElement> assertions, string name)
        {
            return this.TryImportWsspAssertion(assertions, name, false);
        }

        public virtual bool TryImportWsspAssertion(ICollection<XmlElement> assertions, string name, out XmlElement assertion)
        {
            assertion = null;
            foreach (XmlElement element in assertions)
            {
                if ((element.LocalName == name) && (element.NamespaceURI == this.WsspNamespaceUri))
                {
                    assertion = element;
                    assertions.Remove(element);
                    return true;
                }
            }
            return false;
        }

        public virtual bool TryImportWsspAssertion(ICollection<XmlElement> assertions, string name, bool isOptional)
        {
            foreach (XmlElement element in assertions)
            {
                if ((element.LocalName == name) && (element.NamespaceURI == this.WsspNamespaceUri))
                {
                    assertions.Remove(element);
                    return true;
                }
            }
            return isOptional;
        }

        public virtual bool TryImportWsspAsymmetricBindingAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, out AsymmetricSecurityBindingElement binding, out XmlElement assertion)
        {
            Collection<Collection<XmlElement>> collection;
            binding = null;
            if (this.TryImportWsspAssertion(assertions, "AsymmetricBinding", out assertion) && this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    MessageProtectionOrder order;
                    binding = new AsymmetricSecurityBindingElement();
                    if (((this.TryImportWsspInitiatorTokenAssertion(importer, policyContext, collection2, binding) && this.TryImportWsspRecipientTokenAssertion(importer, policyContext, collection2, binding)) && (this.TryImportWsspAlgorithmSuiteAssertion(importer, collection2, binding) && this.TryImportWsspLayoutAssertion(importer, collection2, binding))) && ((this.TryImportWsspIncludeTimestampAssertion(collection2, binding) && this.TryImportMessageProtectionOrderAssertions(collection2, out order)) && (this.TryImportWsspAssertion(collection2, "OnlySignEntireHeadersAndBody", true) && (collection2.Count == 0))))
                    {
                        binding.MessageProtectionOrder = order;
                        break;
                    }
                    binding = null;
                }
            }
            return (binding != null);
        }

        public virtual bool TryImportWsspBootstrapPolicyAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecureConversationSecurityTokenParameters parameters)
        {
            XmlElement element;
            Collection<Collection<XmlElement>> collection;
            BindingElementCollection elements;
            bool flag = false;
            if (!this.TryImportWsspAssertion(assertions, "BootstrapPolicy", out element) || !this.TryGetNestedPolicyAlternatives(importer, element, out collection))
            {
                return flag;
            }
            importer.State["InSecureConversationBootstrapBindingImportMode"] = "InSecureConversationBootstrapBindingImportMode";
            try
            {
                elements = importer.ImportPolicy(NullServiceEndpoint, collection);
                if (importer.State.ContainsKey("SecureConversationBootstrapEncryptionRequirements"))
                {
                    MessagePartSpecification specification = (MessagePartSpecification) importer.State["SecureConversationBootstrapEncryptionRequirements"];
                    if (!specification.IsBodyIncluded)
                    {
                        importer.Errors.Add(new MetadataConversionError(System.ServiceModel.SR.GetString("UnsupportedSecureConversationBootstrapProtectionRequirements"), false));
                        elements = null;
                    }
                }
                if (importer.State.ContainsKey("SecureConversationBootstrapSignatureRequirements"))
                {
                    MessagePartSpecification specification2 = (MessagePartSpecification) importer.State["SecureConversationBootstrapSignatureRequirements"];
                    if (!specification2.IsBodyIncluded)
                    {
                        importer.Errors.Add(new MetadataConversionError(System.ServiceModel.SR.GetString("UnsupportedSecureConversationBootstrapProtectionRequirements"), false));
                        elements = null;
                    }
                }
            }
            finally
            {
                importer.State.Remove("InSecureConversationBootstrapBindingImportMode");
                if (importer.State.ContainsKey("SecureConversationBootstrapEncryptionRequirements"))
                {
                    importer.State.Remove("SecureConversationBootstrapEncryptionRequirements");
                }
                if (importer.State.ContainsKey("SecureConversationBootstrapSignatureRequirements"))
                {
                    importer.State.Remove("SecureConversationBootstrapSignatureRequirements");
                }
            }
            if (elements != null)
            {
                parameters.BootstrapSecurityBindingElement = elements.Find<SecurityBindingElement>();
                return true;
            }
            parameters.BootstrapSecurityBindingElement = null;
            return true;
        }

        public virtual bool TryImportWsspEncryptedPartsAssertion(ICollection<XmlElement> assertions, out MessagePartSpecification parts, out XmlElement assertion)
        {
            if (this.TryImportWsspAssertion(assertions, "EncryptedParts", out assertion))
            {
                parts = this.TryGetProtectedParts(assertion);
            }
            else
            {
                parts = null;
            }
            return (parts != null);
        }

        protected bool TryImportWsspEndorsingSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> optionalEndorsing, out XmlElement assertion)
        {
            Collection<Collection<XmlElement>> collection;
            if (endorsing == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endorsing");
            }
            if (optionalEndorsing == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalEndorsing");
            }
            bool flag = true;
            if (this.TryImportWsspAssertion(assertions, "EndorsingSupportingTokens", out assertion) && this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    MessagePartSpecification specification;
                    SecurityTokenParameters parameters;
                    bool flag2;
                    if (!this.TryImportWsspSignedPartsAssertion(collection2, out specification, out assertion) && (assertion != null))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { assertion.OuterXml })));
                    }
                    Collection<SecurityTokenParameters> collection3 = new Collection<SecurityTokenParameters>();
                    Collection<SecurityTokenParameters> collection4 = new Collection<SecurityTokenParameters>();
                    while ((collection2.Count > 0) && this.TryImportTokenAssertion(importer, policyContext, collection2, out parameters, out flag2))
                    {
                        if (flag2)
                        {
                            collection4.Add(parameters);
                        }
                        else
                        {
                            collection3.Add(parameters);
                        }
                    }
                    if (collection2.Count == 0)
                    {
                        foreach (SecurityTokenParameters parameters2 in collection3)
                        {
                            endorsing.Add(parameters2);
                        }
                        foreach (SecurityTokenParameters parameters3 in collection4)
                        {
                            optionalEndorsing.Add(parameters3);
                        }
                        return true;
                    }
                    flag = false;
                }
            }
            return flag;
        }

        public abstract bool TryImportWsspHttpsTokenAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, HttpsTransportBindingElement httpsBinding);
        public virtual bool TryImportWsspIncludeTimestampAssertion(ICollection<XmlElement> assertions, SecurityBindingElement binding)
        {
            binding.IncludeTimestamp = this.TryImportWsspAssertion(assertions, "IncludeTimestamp");
            return true;
        }

        public virtual bool TryImportWsspInitiatorTokenAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, AsymmetricSecurityBindingElement binding)
        {
            XmlElement element;
            Collection<Collection<XmlElement>> collection;
            bool flag = false;
            if (this.TryImportWsspAssertion(assertions, "InitiatorToken", out element) && this.TryGetNestedPolicyAlternatives(importer, element, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    SecurityTokenParameters parameters;
                    bool flag2;
                    if (this.TryImportTokenAssertion(importer, policyContext, collection2, out parameters, out flag2) && (collection2.Count == 0))
                    {
                        flag = true;
                        binding.InitiatorTokenParameters = parameters;
                        return flag;
                    }
                }
            }
            return flag;
        }

        public virtual bool TryImportWsspIssuedTokenAssertion(MetadataImporter importer, PolicyConversionContext policyContext, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            SecurityTokenInclusionMode mode;
            EndpointAddress address;
            EndpointAddress address2;
            Collection<XmlElement> collection2;
            parameters = null;
            if ((this.IsWsspAssertion(assertion, "IssuedToken") && this.TryGetIncludeTokenValue(assertion, out mode)) && (this.TryGetIssuer(assertion, out address, out address2) && this.TryGetRequestSecurityTokenTemplate(assertion, out collection2)))
            {
                Collection<Collection<XmlElement>> collection;
                if (this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
                {
                    foreach (Collection<XmlElement> collection3 in collection)
                    {
                        IssuedSecurityTokenParameters parameters2 = new IssuedSecurityTokenParameters();
                        parameters = parameters2;
                        if ((this.TryImportWsspRequireDerivedKeysAssertion(collection3, parameters2) && this.TryImportReferenceStyleAssertion(collection3, parameters2)) && (collection3.Count == 0))
                        {
                            parameters2.InclusionMode = mode;
                            parameters2.IssuerAddress = address;
                            parameters2.IssuerMetadataAddress = address2;
                            parameters2.SetRequestParameters(collection2, this.TrustDriver);
                            new TokenIssuerPolicyResolver(this.TrustDriver).ResolveTokenIssuerPolicy(importer, policyContext, parameters2);
                            break;
                        }
                        parameters = null;
                    }
                }
                else
                {
                    IssuedSecurityTokenParameters parameters3 = new IssuedSecurityTokenParameters();
                    parameters = parameters3;
                    parameters3.InclusionMode = mode;
                    parameters3.IssuerAddress = address;
                    parameters3.IssuerMetadataAddress = address2;
                    parameters3.SetRequestParameters(collection2, this.TrustDriver);
                    parameters3.RequireDerivedKeys = false;
                }
            }
            return (parameters != null);
        }

        public virtual bool TryImportWsspKerberosTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            SecurityTokenInclusionMode mode;
            parameters = null;
            if (this.IsWsspAssertion(assertion, "KerberosToken") && this.TryGetIncludeTokenValue(assertion, out mode))
            {
                Collection<Collection<XmlElement>> collection;
                if (this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
                {
                    foreach (Collection<XmlElement> collection2 in collection)
                    {
                        parameters = new KerberosSecurityTokenParameters();
                        if ((this.TryImportWsspRequireDerivedKeysAssertion(collection2, parameters) && this.TryImportWsspAssertion(collection2, "WssGssKerberosV5ApReqToken11", true)) && (collection2.Count == 0))
                        {
                            parameters.InclusionMode = mode;
                            break;
                        }
                        parameters = null;
                    }
                }
                else
                {
                    parameters = new KerberosSecurityTokenParameters();
                    parameters.RequireDerivedKeys = false;
                    parameters.InclusionMode = mode;
                }
            }
            return (parameters != null);
        }

        public virtual bool TryImportWsspLayoutAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding)
        {
            XmlElement element;
            bool flag = false;
            if (this.TryImportWsspAssertion(assertions, "Layout", out element))
            {
                Collection<Collection<XmlElement>> collection;
                if (this.TryGetNestedPolicyAlternatives(importer, element, out collection))
                {
                    foreach (Collection<XmlElement> collection2 in collection)
                    {
                        SecurityHeaderLayout layout;
                        if (this.TryImportLayoutAssertion(collection2, out layout) && (collection2.Count == 0))
                        {
                            binding.SecurityHeaderLayout = layout;
                            return true;
                        }
                    }
                }
                return flag;
            }
            binding.SecurityHeaderLayout = SecurityHeaderLayout.Lax;
            return true;
        }

        public virtual bool TryImportWsspMustNotSendCancelAssertion(ICollection<XmlElement> assertions, out bool requireCancellation)
        {
            requireCancellation = !this.TryImportWsspAssertion(assertions, "MustNotSendCancel");
            return true;
        }

        public virtual bool TryImportWsspProtectionTokenAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, SymmetricSecurityBindingElement binding)
        {
            XmlElement element;
            Collection<Collection<XmlElement>> collection;
            bool flag = false;
            if (this.TryImportWsspAssertion(assertions, "ProtectionToken", out element) && this.TryGetNestedPolicyAlternatives(importer, element, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    SecurityTokenParameters parameters;
                    bool flag2;
                    if (this.TryImportTokenAssertion(importer, policyContext, collection2, out parameters, out flag2) && (collection2.Count == 0))
                    {
                        flag = true;
                        binding.ProtectionTokenParameters = parameters;
                        return flag;
                    }
                }
            }
            return flag;
        }

        public virtual bool TryImportWsspRecipientTokenAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, AsymmetricSecurityBindingElement binding)
        {
            XmlElement element;
            Collection<Collection<XmlElement>> collection;
            bool flag = false;
            if (this.TryImportWsspAssertion(assertions, "RecipientToken", out element) && this.TryGetNestedPolicyAlternatives(importer, element, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    SecurityTokenParameters parameters;
                    bool flag2;
                    if (this.TryImportTokenAssertion(importer, policyContext, collection2, out parameters, out flag2) && (collection2.Count == 0))
                    {
                        flag = true;
                        binding.RecipientTokenParameters = parameters;
                        return flag;
                    }
                }
            }
            return flag;
        }

        public virtual bool TryImportWsspRequireDerivedKeysAssertion(ICollection<XmlElement> assertions, SecurityTokenParameters parameters)
        {
            parameters.RequireDerivedKeys = this.TryImportWsspAssertion(assertions, "RequireDerivedKeys");
            return true;
        }

        public virtual bool TryImportWsspRsaTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            SecurityTokenInclusionMode mode;
            Collection<Collection<XmlElement>> collection;
            parameters = null;
            if ((this.IsMsspAssertion(assertion, "RsaToken") && this.TryGetIncludeTokenValue(assertion, out mode)) && !this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                parameters = new RsaSecurityTokenParameters();
                parameters.InclusionMode = mode;
            }
            return (parameters != null);
        }

        public virtual bool TryImportWsspSecureConversationTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
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
                        SecureConversationSecurityTokenParameters parameters2 = new SecureConversationSecurityTokenParameters();
                        parameters = parameters2;
                        if ((this.TryImportWsspRequireDerivedKeysAssertion(collection2, parameters2) && this.TryImportWsspMustNotSendCancelAssertion(collection2, out flag)) && (this.TryImportWsspBootstrapPolicyAssertion(importer, collection2, parameters2) && (collection2.Count == 0)))
                        {
                            parameters2.RequireCancellation = flag;
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

        protected bool TryImportWsspSignedEndorsingSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing, out XmlElement assertion)
        {
            Collection<Collection<XmlElement>> collection;
            if (signedEndorsing == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signedEndorsing");
            }
            if (optionalSignedEndorsing == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalSignedEndorsing");
            }
            bool flag = true;
            if (this.TryImportWsspAssertion(assertions, "SignedEndorsingSupportingTokens", out assertion) && this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    MessagePartSpecification specification;
                    SecurityTokenParameters parameters;
                    bool flag2;
                    if (!this.TryImportWsspSignedPartsAssertion(collection2, out specification, out assertion) && (assertion != null))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { assertion.OuterXml })));
                    }
                    Collection<SecurityTokenParameters> collection3 = new Collection<SecurityTokenParameters>();
                    Collection<SecurityTokenParameters> collection4 = new Collection<SecurityTokenParameters>();
                    while ((collection2.Count > 0) && this.TryImportTokenAssertion(importer, policyContext, collection2, out parameters, out flag2))
                    {
                        if (flag2)
                        {
                            collection4.Add(parameters);
                        }
                        else
                        {
                            collection3.Add(parameters);
                        }
                    }
                    if (collection2.Count == 0)
                    {
                        foreach (SecurityTokenParameters parameters2 in collection3)
                        {
                            signedEndorsing.Add(parameters2);
                        }
                        foreach (SecurityTokenParameters parameters3 in collection4)
                        {
                            optionalSignedEndorsing.Add(parameters3);
                        }
                        return true;
                    }
                    flag = false;
                }
            }
            return flag;
        }

        public virtual bool TryImportWsspSignedPartsAssertion(ICollection<XmlElement> assertions, out MessagePartSpecification parts, out XmlElement assertion)
        {
            if (this.TryImportWsspAssertion(assertions, "SignedParts", out assertion))
            {
                parts = this.TryGetProtectedParts(assertion);
            }
            else
            {
                parts = null;
            }
            return (parts != null);
        }

        protected bool TryImportWsspSignedSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, out XmlElement assertion)
        {
            Collection<Collection<XmlElement>> collection;
            if (signed == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signed");
            }
            if (signedEncrypted == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("signedEncrypted");
            }
            if (optionalSigned == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalSigned");
            }
            if (optionalSignedEncrypted == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("optionalSignedEncrypted");
            }
            bool flag = true;
            if (this.TryImportWsspAssertion(assertions, "SignedSupportingTokens", out assertion) && this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    SecurityTokenParameters parameters;
                    bool flag2;
                    Collection<SecurityTokenParameters> collection3 = new Collection<SecurityTokenParameters>();
                    Collection<SecurityTokenParameters> collection4 = new Collection<SecurityTokenParameters>();
                    while ((collection2.Count > 0) && this.TryImportTokenAssertion(importer, policyContext, collection2, out parameters, out flag2))
                    {
                        if (flag2)
                        {
                            collection4.Add(parameters);
                        }
                        else
                        {
                            collection3.Add(parameters);
                        }
                    }
                    if (collection2.Count == 0)
                    {
                        foreach (SecurityTokenParameters parameters2 in collection3)
                        {
                            if (parameters2 is UserNameSecurityTokenParameters)
                            {
                                signedEncrypted.Add(parameters2);
                            }
                            else
                            {
                                signed.Add(parameters2);
                            }
                        }
                        foreach (SecurityTokenParameters parameters3 in collection4)
                        {
                            if (parameters3 is UserNameSecurityTokenParameters)
                            {
                                optionalSignedEncrypted.Add(parameters3);
                            }
                            else
                            {
                                optionalSigned.Add(parameters3);
                            }
                        }
                        return true;
                    }
                    flag = false;
                }
            }
            return flag;
        }

        public virtual bool TryImportWsspSpnegoContextTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
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
                        SspiSecurityTokenParameters parameters2 = new SspiSecurityTokenParameters();
                        parameters = parameters2;
                        if ((this.TryImportWsspRequireDerivedKeysAssertion(collection2, parameters2) && this.TryImportWsspMustNotSendCancelAssertion(collection2, out flag)) && (collection2.Count == 0))
                        {
                            parameters2.RequireCancellation = flag;
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

        public virtual bool TryImportWsspSupportingTokensAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, Collection<SecurityTokenParameters> signed, Collection<SecurityTokenParameters> signedEncrypted, Collection<SecurityTokenParameters> endorsing, Collection<SecurityTokenParameters> signedEndorsing, Collection<SecurityTokenParameters> optionalSigned, Collection<SecurityTokenParameters> optionalSignedEncrypted, Collection<SecurityTokenParameters> optionalEndorsing, Collection<SecurityTokenParameters> optionalSignedEndorsing)
        {
            XmlElement element;
            if (!this.TryImportWsspSignedSupportingTokensAssertion(importer, policyContext, assertions, signed, signedEncrypted, optionalSigned, optionalSignedEncrypted, out element) && (element != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element.OuterXml })));
            }
            if (!this.TryImportWsspEndorsingSupportingTokensAssertion(importer, policyContext, assertions, endorsing, optionalEndorsing, out element) && (element != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element.OuterXml })));
            }
            if (!this.TryImportWsspSignedEndorsingSupportingTokensAssertion(importer, policyContext, assertions, signedEndorsing, optionalSignedEndorsing, out element) && (element != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element.OuterXml })));
            }
            return true;
        }

        public virtual bool TryImportWsspSymmetricBindingAssertion(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, out SymmetricSecurityBindingElement binding, out XmlElement assertion)
        {
            Collection<Collection<XmlElement>> collection;
            binding = null;
            if (this.TryImportWsspAssertion(assertions, "SymmetricBinding", out assertion) && this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    MessageProtectionOrder order;
                    binding = new SymmetricSecurityBindingElement();
                    if (((this.TryImportWsspProtectionTokenAssertion(importer, policyContext, collection2, binding) && this.TryImportWsspAlgorithmSuiteAssertion(importer, collection2, binding)) && (this.TryImportWsspLayoutAssertion(importer, collection2, binding) && this.TryImportWsspIncludeTimestampAssertion(collection2, binding))) && ((this.TryImportMessageProtectionOrderAssertions(collection2, out order) && this.TryImportWsspAssertion(collection2, "OnlySignEntireHeadersAndBody", true)) && (collection2.Count == 0)))
                    {
                        binding.MessageProtectionOrder = order;
                        break;
                    }
                    binding = null;
                }
            }
            return (binding != null);
        }

        public virtual bool TryImportWsspTransportBindingAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, out TransportSecurityBindingElement binding, out XmlElement assertion)
        {
            Collection<Collection<XmlElement>> collection;
            binding = null;
            if (this.TryImportWsspAssertion(assertions, "TransportBinding", out assertion) && this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    XmlElement element;
                    binding = new TransportSecurityBindingElement();
                    if (((this.TryImportWsspTransportTokenAssertion(importer, collection2, out element) && this.TryImportWsspAlgorithmSuiteAssertion(importer, collection2, binding)) && (this.TryImportWsspLayoutAssertion(importer, collection2, binding) && this.TryImportWsspIncludeTimestampAssertion(collection2, binding))) && (collection2.Count == 0))
                    {
                        if (!importer.State.ContainsKey("InSecureConversationBootstrapBindingImportMode"))
                        {
                            assertions.Add(element);
                        }
                        break;
                    }
                    binding = null;
                }
            }
            return (binding != null);
        }

        public virtual bool TryImportWsspTransportTokenAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, out XmlElement transportBindingAssertion)
        {
            XmlElement element;
            Collection<Collection<XmlElement>> collection;
            transportBindingAssertion = null;
            if ((this.TryImportWsspAssertion(assertions, "TransportToken", out element) && this.TryGetNestedPolicyAlternatives(importer, element, out collection)) && ((collection.Count == 1) && (collection[0].Count == 1)))
            {
                transportBindingAssertion = collection[0][0];
            }
            return (transportBindingAssertion != null);
        }

        public abstract bool TryImportWsspTrustAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding, out XmlElement assertion);
        protected bool TryImportWsspTrustAssertion(string trustName, MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding, out XmlElement assertion)
        {
            Collection<Collection<XmlElement>> collection;
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (assertions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertions");
            }
            bool flag = true;
            if (this.TryImportWsspAssertion(assertions, trustName, out assertion) && this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                foreach (Collection<XmlElement> collection2 in collection)
                {
                    this.TryImportWsspAssertion(collection2, "MustSupportIssuedTokens");
                    bool flag2 = this.TryImportWsspAssertion(collection2, "RequireClientEntropy");
                    bool flag3 = this.TryImportWsspAssertion(collection2, "RequireServerEntropy");
                    if (trustName == "Trust13")
                    {
                        this.TryImportWsspAssertion(collection2, "RequireAppliesTo");
                    }
                    if (collection2.Count == 0)
                    {
                        if (flag2)
                        {
                            if (flag3)
                            {
                                binding.KeyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
                            }
                            else
                            {
                                binding.KeyEntropyMode = SecurityKeyEntropyMode.ClientEntropy;
                            }
                        }
                        else if (flag3)
                        {
                            binding.KeyEntropyMode = SecurityKeyEntropyMode.ServerEntropy;
                        }
                        return true;
                    }
                    flag = false;
                }
            }
            return flag;
        }

        public virtual bool TryImportWsspUsernameTokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            SecurityTokenInclusionMode mode;
            parameters = null;
            if (this.IsWsspAssertion(assertion, "UsernameToken") && this.TryGetIncludeTokenValue(assertion, out mode))
            {
                Collection<Collection<XmlElement>> collection;
                if (this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
                {
                    foreach (Collection<XmlElement> collection2 in collection)
                    {
                        if (this.TryImportWsspAssertion(collection2, "WssUsernameToken10") && (collection2.Count == 0))
                        {
                            parameters = new UserNameSecurityTokenParameters();
                            parameters.InclusionMode = mode;
                            break;
                        }
                    }
                }
                else
                {
                    parameters = new UserNameSecurityTokenParameters();
                    parameters.InclusionMode = mode;
                }
            }
            return (parameters != null);
        }

        public virtual bool TryImportWsspWssAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding, out XmlElement assertion)
        {
            Collection<Collection<XmlElement>> collection;
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (assertions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertions");
            }
            bool flag = true;
            if (this.TryImportWsspAssertion(assertions, "Wss10", out assertion))
            {
                if (this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
                {
                    foreach (Collection<XmlElement> collection2 in collection)
                    {
                        this.TryImportWsspAssertion(collection2, "MustSupportRefKeyIdentifier");
                        this.TryImportWsspAssertion(collection2, "MustSupportRefIssuerSerial");
                        if (collection2.Count == 0)
                        {
                            binding.MessageSecurityVersion = this.GetSupportedMessageSecurityVersion(SecurityVersion.WSSecurity10);
                            return true;
                        }
                        flag = false;
                    }
                }
                return flag;
            }
            if (this.TryImportWsspAssertion(assertions, "Wss11", out assertion) && this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
            {
                foreach (Collection<XmlElement> collection3 in collection)
                {
                    this.TryImportWsspAssertion(collection3, "MustSupportRefKeyIdentifier");
                    this.TryImportWsspAssertion(collection3, "MustSupportRefIssuerSerial");
                    this.TryImportWsspAssertion(collection3, "MustSupportRefThumbprint");
                    this.TryImportWsspAssertion(collection3, "MustSupportRefEncryptedKey");
                    bool flag2 = this.TryImportWsspAssertion(collection3, "RequireSignatureConfirmation");
                    if (collection3.Count == 0)
                    {
                        binding.MessageSecurityVersion = this.GetSupportedMessageSecurityVersion(SecurityVersion.WSSecurity11);
                        if (binding is SymmetricSecurityBindingElement)
                        {
                            ((SymmetricSecurityBindingElement) binding).RequireSignatureConfirmation = flag2;
                        }
                        else if (binding is AsymmetricSecurityBindingElement)
                        {
                            ((AsymmetricSecurityBindingElement) binding).RequireSignatureConfirmation = flag2;
                        }
                        return true;
                    }
                    flag = false;
                }
            }
            return flag;
        }

        public virtual bool TryImportWsspX509TokenAssertion(MetadataImporter importer, XmlElement assertion, out SecurityTokenParameters parameters)
        {
            SecurityTokenInclusionMode mode;
            parameters = null;
            if (this.IsWsspAssertion(assertion, "X509Token") && this.TryGetIncludeTokenValue(assertion, out mode))
            {
                Collection<Collection<XmlElement>> collection;
                if (this.TryGetNestedPolicyAlternatives(importer, assertion, out collection))
                {
                    foreach (Collection<XmlElement> collection2 in collection)
                    {
                        X509SecurityTokenParameters parameters2 = new X509SecurityTokenParameters();
                        parameters = parameters2;
                        if ((this.TryImportWsspRequireDerivedKeysAssertion(collection2, parameters2) && this.TryImportX509ReferenceStyleAssertion(collection2, parameters2)) && (this.TryImportWsspAssertion(collection2, "WssX509V3Token10", true) && (collection2.Count == 0)))
                        {
                            parameters.InclusionMode = mode;
                            break;
                        }
                        parameters = null;
                    }
                }
                else
                {
                    parameters = new X509SecurityTokenParameters();
                    parameters.RequireDerivedKeys = false;
                    parameters.InclusionMode = mode;
                }
            }
            return (parameters != null);
        }

        public virtual bool TryImportX509ReferenceStyleAssertion(ICollection<XmlElement> assertions, X509SecurityTokenParameters parameters)
        {
            if (this.TryImportWsspAssertion(assertions, "RequireIssuerSerialReference"))
            {
                parameters.X509ReferenceStyle = X509KeyIdentifierClauseType.IssuerSerial;
            }
            else if (this.TryImportWsspAssertion(assertions, "RequireKeyIdentifierReference"))
            {
                parameters.X509ReferenceStyle = X509KeyIdentifierClauseType.SubjectKeyIdentifier;
            }
            else if (this.TryImportWsspAssertion(assertions, "RequireThumbprintReference"))
            {
                parameters.X509ReferenceStyle = X509KeyIdentifierClauseType.Thumbprint;
            }
            return true;
        }

        public virtual string AlwaysToInitiatorUri
        {
            get
            {
                return (this.WsspNamespaceUri + "/IncludeToken/AlwaysToInitiator");
            }
        }

        public virtual string AlwaysToRecipientUri
        {
            get
            {
                return (this.WsspNamespaceUri + "/IncludeToken/AlwaysToRecipient");
            }
        }

        public virtual string NeverUri
        {
            get
            {
                return (this.WsspNamespaceUri + "/IncludeToken/Never");
            }
        }

        public virtual string OnceUri
        {
            get
            {
                return (this.WsspNamespaceUri + "/IncludeToken/Once");
            }
        }

        public abstract System.ServiceModel.Security.TrustDriver TrustDriver { get; }

        public abstract string WsspNamespaceUri { get; }

        private class SecurityPolicyManager
        {
            private List<WSSecurityPolicy> drivers = new List<WSSecurityPolicy>();

            public SecurityPolicyManager()
            {
                this.Initialize();
            }

            public WSSecurityPolicy GetSecurityPolicyDriver(MessageSecurityVersion version)
            {
                for (int i = 0; i < this.drivers.Count; i++)
                {
                    if (this.drivers[i].IsSecurityVersionSupported(version))
                    {
                        return this.drivers[i];
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public void Initialize()
            {
                this.drivers.Add(new WSSecurityPolicy11());
                this.drivers.Add(new WSSecurityPolicy12());
            }

            public bool TryGetSecurityPolicyDriver(ICollection<XmlElement> assertions, out WSSecurityPolicy securityPolicy)
            {
                securityPolicy = null;
                for (int i = 0; i < this.drivers.Count; i++)
                {
                    if (this.drivers[i].CanImportAssertion(assertions))
                    {
                        securityPolicy = this.drivers[i];
                        return true;
                    }
                }
                return false;
            }
        }

        private class TokenIssuerPolicyResolver
        {
            private static readonly Uri SelfIssuerUri = new Uri("http://schemas.xmlsoap.org/ws/2005/05/identity/issuer/self");
            private TrustDriver trustDriver;
            private const string WSIdentityNamespace = "http://schemas.xmlsoap.org/ws/2005/05/identity";

            public TokenIssuerPolicyResolver(TrustDriver driver)
            {
                this.trustDriver = driver;
            }

            private void AddCompatibleFederationEndpoints(ServiceEndpointCollection serviceEndpoints, IssuedSecurityTokenParameters parameters)
            {
                bool flag = (parameters.IssuerAddress != null) && !parameters.IssuerAddress.IsAnonymous;
                foreach (ServiceEndpoint endpoint in serviceEndpoints)
                {
                    TrustDriver trustDriver;
                    if (!this.TryGetTrustDriver(endpoint, out trustDriver))
                    {
                        trustDriver = this.trustDriver;
                    }
                    bool flag2 = false;
                    ContractDescription contract = endpoint.Contract;
                    for (int i = 0; i < contract.Operations.Count; i++)
                    {
                        OperationDescription description2 = contract.Operations[i];
                        bool flag3 = false;
                        bool flag4 = false;
                        for (int j = 0; j < description2.Messages.Count; j++)
                        {
                            MessageDescription description3 = description2.Messages[j];
                            if ((description3.Action == trustDriver.RequestSecurityTokenAction.Value) && (description3.Direction == MessageDirection.Input))
                            {
                                flag3 = true;
                            }
                            else if ((((trustDriver.StandardsManager.TrustVersion == TrustVersion.WSTrustFeb2005) && (description3.Action == trustDriver.RequestSecurityTokenResponseAction.Value)) || ((trustDriver.StandardsManager.TrustVersion == TrustVersion.WSTrust13) && (description3.Action == trustDriver.RequestSecurityTokenResponseFinalAction.Value))) && (description3.Direction == MessageDirection.Output))
                            {
                                flag4 = true;
                            }
                        }
                        if (flag3 && flag4)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    if (flag2 && (!flag || parameters.IssuerAddress.Uri.Equals(endpoint.Address.Uri)))
                    {
                        if (parameters.IssuerBinding == null)
                        {
                            parameters.IssuerAddress = endpoint.Address;
                            parameters.IssuerBinding = endpoint.Binding;
                        }
                        else
                        {
                            IssuedSecurityTokenParameters.AlternativeIssuerEndpoint item = new IssuedSecurityTokenParameters.AlternativeIssuerEndpoint {
                                IssuerAddress = endpoint.Address,
                                IssuerBinding = endpoint.Binding
                            };
                            parameters.AlternativeIssuerEndpoints.Add(item);
                        }
                    }
                }
            }

            private static string InsertEllipsisIfTooLong(string message)
            {
                if ((message != null) && (message.Length > 0x400))
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[] { message.Substring(0, (0x400 - "....".Length) / 2), "....", message.Substring(message.Length - ((0x400 - "....".Length) / 2)) });
                }
                return message;
            }

            public void ResolveTokenIssuerPolicy(MetadataImporter importer, PolicyConversionContext policyContext, IssuedSecurityTokenParameters parameters)
            {
                if (policyContext == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");
                }
                if (parameters == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
                }
                EndpointAddress address = (parameters.IssuerMetadataAddress != null) ? parameters.IssuerMetadataAddress : parameters.IssuerAddress;
                if (((address != null) && !address.IsAnonymous) && !address.Uri.Equals(SelfIssuerUri))
                {
                    int num = (int) importer.State["MaxPolicyRedirections"];
                    if (num <= 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MaximumPolicyRedirectionsExceeded")));
                    }
                    num--;
                    MetadataExchangeClient client = null;
                    if ((importer.State != null) && importer.State.ContainsKey("MetadataExchangeClientKey"))
                    {
                        client = importer.State["MetadataExchangeClientKey"] as MetadataExchangeClient;
                    }
                    if (client == null)
                    {
                        client = new MetadataExchangeClient(address);
                    }
                    ServiceEndpointCollection serviceEndpoints = null;
                    MetadataSet metadata = null;
                    Exception exception = null;
                    try
                    {
                        metadata = client.GetMetadata(address);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        if (exception2 is NullReferenceException)
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    if (metadata == null)
                    {
                        try
                        {
                            metadata = client.GetMetadata(address.Uri, MetadataExchangeClientMode.HttpGet);
                        }
                        catch (Exception exception3)
                        {
                            if (Fx.IsFatal(exception3))
                            {
                                throw;
                            }
                            if (exception3 is NullReferenceException)
                            {
                                throw;
                            }
                            if (exception == null)
                            {
                                exception = exception3;
                            }
                        }
                    }
                    if (metadata == null)
                    {
                        if (exception != null)
                        {
                            importer.Errors.Add(new MetadataConversionError(System.ServiceModel.SR.GetString("UnableToObtainIssuerMetadata", new object[] { address, exception }), false));
                        }
                    }
                    else
                    {
                        WsdlImporter importer2;
                        WsdlImporter importer3 = importer as WsdlImporter;
                        if (importer3 != null)
                        {
                            importer2 = new WsdlImporter(metadata, importer.PolicyImportExtensions, importer3.WsdlImportExtensions);
                        }
                        else
                        {
                            importer2 = new WsdlImporter(metadata, importer.PolicyImportExtensions, null);
                        }
                        if ((importer.State != null) && importer.State.ContainsKey("MetadataExchangeClientKey"))
                        {
                            importer2.State.Add("MetadataExchangeClientKey", importer.State["MetadataExchangeClientKey"]);
                        }
                        importer2.State.Add("MaxPolicyRedirections", num);
                        serviceEndpoints = importer2.ImportAllEndpoints();
                        for (int i = 0; i < importer2.Errors.Count; i++)
                        {
                            MetadataConversionError error = importer2.Errors[i];
                            importer.Errors.Add(new MetadataConversionError(System.ServiceModel.SR.GetString("ErrorImportingIssuerMetadata", new object[] { address, InsertEllipsisIfTooLong(error.Message) }), error.IsWarning));
                        }
                        if (serviceEndpoints != null)
                        {
                            this.AddCompatibleFederationEndpoints(serviceEndpoints, parameters);
                            if ((parameters.AlternativeIssuerEndpoints != null) && (parameters.AlternativeIssuerEndpoints.Count > 0))
                            {
                                importer.Errors.Add(new MetadataConversionError(System.ServiceModel.SR.GetString("MultipleIssuerEndpointsFound", new object[] { address })));
                            }
                        }
                    }
                }
            }

            private bool TryGetTrustDriver(ServiceEndpoint endpoint, out TrustDriver trustDriver)
            {
                SecurityBindingElement element = endpoint.Binding.CreateBindingElements().Find<SecurityBindingElement>();
                trustDriver = null;
                if (element != null)
                {
                    MessageSecurityVersion messageSecurityVersion = element.MessageSecurityVersion;
                    if (messageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                    {
                        trustDriver = new WSTrustFeb2005.DriverFeb2005(new SecurityStandardsManager(messageSecurityVersion, WSSecurityTokenSerializer.DefaultInstance));
                    }
                    else if (messageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                    {
                        trustDriver = new WSTrustDec2005.DriverDec2005(new SecurityStandardsManager(messageSecurityVersion, WSSecurityTokenSerializer.DefaultInstance));
                    }
                }
                return (trustDriver != null);
            }
        }
    }
}

