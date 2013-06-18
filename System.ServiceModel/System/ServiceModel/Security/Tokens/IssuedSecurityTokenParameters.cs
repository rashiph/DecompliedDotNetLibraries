namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Xml;

    public class IssuedSecurityTokenParameters : SecurityTokenParameters
    {
        private Collection<XmlElement> additionalRequestParameters;
        private Collection<AlternativeIssuerEndpoint> alternativeIssuerEndpoints;
        private Collection<ClaimTypeRequirement> claimTypeRequirements;
        internal const SecurityKeyType defaultKeyType = SecurityKeyType.SymmetricKey;
        private MessageSecurityVersion defaultMessageSecurityVersion;
        private EndpointAddress issuerAddress;
        private Binding issuerBinding;
        private EndpointAddress issuerMetadataAddress;
        private int keySize;
        private SecurityKeyType keyType;
        private string tokenType;
        private const string wsidNamespace = "http://schemas.xmlsoap.org/ws/2005/05/identity";
        private static readonly string wsidPPIClaim = string.Format(CultureInfo.InvariantCulture, "{0}/claims/privatepersonalidentifier", new object[] { "http://schemas.xmlsoap.org/ws/2005/05/identity" });
        private const string wsidPrefix = "wsid";

        public IssuedSecurityTokenParameters() : this(null, null, null)
        {
        }

        protected IssuedSecurityTokenParameters(IssuedSecurityTokenParameters other) : base(other)
        {
            this.additionalRequestParameters = new Collection<XmlElement>();
            this.alternativeIssuerEndpoints = new Collection<AlternativeIssuerEndpoint>();
            this.claimTypeRequirements = new Collection<ClaimTypeRequirement>();
            this.defaultMessageSecurityVersion = other.defaultMessageSecurityVersion;
            this.issuerAddress = other.issuerAddress;
            this.keyType = other.keyType;
            this.tokenType = other.tokenType;
            this.keySize = other.keySize;
            foreach (XmlElement element in other.additionalRequestParameters)
            {
                this.additionalRequestParameters.Add((XmlElement) element.Clone());
            }
            foreach (ClaimTypeRequirement requirement in other.claimTypeRequirements)
            {
                this.claimTypeRequirements.Add(requirement);
            }
            if (other.issuerBinding != null)
            {
                this.issuerBinding = new CustomBinding(other.issuerBinding);
            }
            this.issuerMetadataAddress = other.issuerMetadataAddress;
        }

        public IssuedSecurityTokenParameters(string tokenType) : this(tokenType, null, null)
        {
        }

        public IssuedSecurityTokenParameters(string tokenType, EndpointAddress issuerAddress) : this(tokenType, issuerAddress, null)
        {
        }

        public IssuedSecurityTokenParameters(string tokenType, EndpointAddress issuerAddress, Binding issuerBinding)
        {
            this.additionalRequestParameters = new Collection<XmlElement>();
            this.alternativeIssuerEndpoints = new Collection<AlternativeIssuerEndpoint>();
            this.claimTypeRequirements = new Collection<ClaimTypeRequirement>();
            this.tokenType = tokenType;
            this.issuerAddress = issuerAddress;
            this.issuerBinding = issuerBinding;
        }

        internal void AddAlgorithmParameters(SecurityAlgorithmSuite algorithmSuite, SecurityStandardsManager standardsManager, SecurityKeyType issuedKeyType)
        {
            this.additionalRequestParameters.Insert(0, standardsManager.TrustDriver.CreateEncryptionAlgorithmElement(algorithmSuite.DefaultEncryptionAlgorithm));
            this.additionalRequestParameters.Insert(0, standardsManager.TrustDriver.CreateCanonicalizationAlgorithmElement(algorithmSuite.DefaultCanonicalizationAlgorithm));
            if (this.keyType != SecurityKeyType.BearerKey)
            {
                string defaultEncryptionAlgorithm;
                string signatureAlgorithm = (this.keyType == SecurityKeyType.SymmetricKey) ? algorithmSuite.DefaultSymmetricSignatureAlgorithm : algorithmSuite.DefaultAsymmetricSignatureAlgorithm;
                this.additionalRequestParameters.Insert(0, standardsManager.TrustDriver.CreateSignWithElement(signatureAlgorithm));
                if (issuedKeyType == SecurityKeyType.SymmetricKey)
                {
                    defaultEncryptionAlgorithm = algorithmSuite.DefaultEncryptionAlgorithm;
                }
                else
                {
                    defaultEncryptionAlgorithm = algorithmSuite.DefaultAsymmetricKeyWrapAlgorithm;
                }
                this.additionalRequestParameters.Insert(0, standardsManager.TrustDriver.CreateEncryptWithElement(defaultEncryptionAlgorithm));
                if (standardsManager.TrustVersion != TrustVersion.WSTrustFeb2005)
                {
                    this.additionalRequestParameters.Insert(0, ((WSTrustDec2005.DriverDec2005) standardsManager.TrustDriver).CreateKeyWrapAlgorithmElement(algorithmSuite.DefaultAsymmetricKeyWrapAlgorithm));
                }
            }
        }

        private bool CanPromoteToRoot(XmlElement innerElement, WSTrustDec2005.DriverDec2005 trust13Driver, bool clientSideClaimTypeRequirementsSpecified)
        {
            SecurityKeyType type;
            int num;
            string str;
            Collection<XmlElement> requiredClaims = null;
            if (trust13Driver.TryParseRequiredClaimsElement(innerElement, out requiredClaims))
            {
                return !clientSideClaimTypeRequirementsSpecified;
            }
            return ((((!trust13Driver.TryParseKeyTypeElement(innerElement, out type) && !trust13Driver.TryParseKeySizeElement(innerElement, out num)) && (!trust13Driver.TryParseTokenTypeElement(innerElement, out str) && !trust13Driver.IsSignWithElement(innerElement, out str))) && !trust13Driver.IsEncryptWithElement(innerElement, out str)) && !trust13Driver.IsKeyWrapAlgorithmElement(innerElement, out str));
        }

        protected override SecurityTokenParameters CloneCore()
        {
            return new IssuedSecurityTokenParameters(this);
        }

        private bool CollectionContainsElementsWithTrustNamespace(Collection<XmlElement> collection, string trustNamespace)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                if ((collection[i] != null) && (collection[i].NamespaceURI == trustNamespace))
                {
                    return true;
                }
            }
            return false;
        }

        internal static IssuedSecurityTokenParameters CreateInfoCardParameters(SecurityStandardsManager standardsManager, SecurityAlgorithmSuite algorithm)
        {
            IssuedSecurityTokenParameters parameters = new IssuedSecurityTokenParameters("http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1") {
                KeyType = SecurityKeyType.AsymmetricKey
            };
            parameters.ClaimTypeRequirements.Add(new ClaimTypeRequirement(wsidPPIClaim));
            parameters.IssuerAddress = null;
            parameters.AddAlgorithmParameters(algorithm, standardsManager, parameters.KeyType);
            return parameters;
        }

        protected internal override SecurityKeyIdentifierClause CreateKeyIdentifierClause(SecurityToken token, SecurityTokenReferenceStyle referenceStyle)
        {
            if (token is GenericXmlSecurityToken)
            {
                return base.CreateGenericXmlTokenKeyIdentifierClause(token, referenceStyle);
            }
            return base.CreateKeyIdentifierClause<SamlAssertionKeyIdentifierClause, SamlAssertionKeyIdentifierClause>(token, referenceStyle);
        }

        internal Collection<XmlElement> CreateRequestParameters(TrustDriver driver)
        {
            if (driver == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("driver");
            }
            Collection<XmlElement> collection = new Collection<XmlElement>();
            if (this.tokenType != null)
            {
                collection.Add(driver.CreateTokenTypeElement(this.tokenType));
            }
            collection.Add(driver.CreateKeyTypeElement(this.keyType));
            if (this.keySize != 0)
            {
                collection.Add(driver.CreateKeySizeElement(this.keySize));
            }
            if (this.claimTypeRequirements.Count > 0)
            {
                Collection<XmlElement> claimsList = new Collection<XmlElement>();
                XmlDocument document = new XmlDocument();
                foreach (ClaimTypeRequirement requirement in this.claimTypeRequirements)
                {
                    XmlElement item = document.CreateElement("wsid", "ClaimType", "http://schemas.xmlsoap.org/ws/2005/05/identity");
                    System.Xml.XmlAttribute node = document.CreateAttribute("Uri");
                    node.Value = requirement.ClaimType;
                    item.Attributes.Append(node);
                    if (requirement.IsOptional)
                    {
                        node = document.CreateAttribute("Optional");
                        node.Value = XmlConvert.ToString(requirement.IsOptional);
                        item.Attributes.Append(node);
                    }
                    claimsList.Add(item);
                }
                collection.Add(driver.CreateRequiredClaimsElement(claimsList));
            }
            if (this.additionalRequestParameters.Count > 0)
            {
                foreach (XmlElement element2 in this.NormalizeAdditionalParameters(this.additionalRequestParameters, driver, this.claimTypeRequirements.Count > 0))
                {
                    collection.Add(element2);
                }
            }
            return collection;
        }

        public Collection<XmlElement> CreateRequestParameters(MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer)
        {
            return this.CreateRequestParameters(System.ServiceModel.Security.SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer).TrustDriver);
        }

        internal bool DoAlgorithmsMatch(SecurityAlgorithmSuite algorithmSuite, SecurityStandardsManager standardsManager, out Collection<XmlElement> otherRequestParameters)
        {
            Collection<XmlElement> additionalRequestParameters;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            otherRequestParameters = new Collection<XmlElement>();
            bool flag6 = false;
            if (((standardsManager.TrustVersion == TrustVersion.WSTrust13) && (this.AdditionalRequestParameters.Count == 1)) && ((WSTrustDec2005.DriverDec2005) standardsManager.TrustDriver).IsSecondaryParametersElement(this.AdditionalRequestParameters[0]))
            {
                flag6 = true;
                additionalRequestParameters = new Collection<XmlElement>();
                foreach (XmlElement element in this.AdditionalRequestParameters[0])
                {
                    additionalRequestParameters.Add(element);
                }
            }
            else
            {
                additionalRequestParameters = this.AdditionalRequestParameters;
            }
            for (int i = 0; i < additionalRequestParameters.Count; i++)
            {
                string str;
                XmlElement element2 = additionalRequestParameters[i];
                if (standardsManager.TrustDriver.IsCanonicalizationAlgorithmElement(element2, out str))
                {
                    if (algorithmSuite.DefaultCanonicalizationAlgorithm != str)
                    {
                        return false;
                    }
                    flag4 = true;
                }
                else if (standardsManager.TrustDriver.IsSignWithElement(element2, out str))
                {
                    if (((this.keyType == SecurityKeyType.SymmetricKey) && (str != algorithmSuite.DefaultSymmetricSignatureAlgorithm)) || ((this.keyType == SecurityKeyType.AsymmetricKey) && (str != algorithmSuite.DefaultAsymmetricSignatureAlgorithm)))
                    {
                        return false;
                    }
                    flag = true;
                }
                else if (standardsManager.TrustDriver.IsEncryptWithElement(element2, out str))
                {
                    if (((this.keyType == SecurityKeyType.SymmetricKey) && (str != algorithmSuite.DefaultEncryptionAlgorithm)) || ((this.keyType == SecurityKeyType.AsymmetricKey) && (str != algorithmSuite.DefaultAsymmetricKeyWrapAlgorithm)))
                    {
                        return false;
                    }
                    flag2 = true;
                }
                else if (standardsManager.TrustDriver.IsEncryptionAlgorithmElement(element2, out str))
                {
                    if (str != algorithmSuite.DefaultEncryptionAlgorithm)
                    {
                        return false;
                    }
                    flag3 = true;
                }
                else if (standardsManager.TrustDriver.IsKeyWrapAlgorithmElement(element2, out str))
                {
                    if (str != algorithmSuite.DefaultAsymmetricKeyWrapAlgorithm)
                    {
                        return false;
                    }
                    flag5 = true;
                }
                else
                {
                    otherRequestParameters.Add(element2);
                }
            }
            if (flag6)
            {
                otherRequestParameters = this.AdditionalRequestParameters;
            }
            if (this.keyType == SecurityKeyType.BearerKey)
            {
                return true;
            }
            if (standardsManager.TrustVersion == TrustVersion.WSTrustFeb2005)
            {
                return (((flag && flag4) && flag3) && flag2);
            }
            return (((flag && flag4) && (flag3 && flag2)) && flag5);
        }

        internal static XmlElement GetClaimTypeRequirement(Collection<XmlElement> additionalRequestParameters, SecurityStandardsManager standardsManager)
        {
            foreach (XmlElement element in additionalRequestParameters)
            {
                if ((element.LocalName == ((WSTrust.Driver) standardsManager.TrustDriver).DriverDictionary.Claims.Value) && (element.NamespaceURI == ((WSTrust.Driver) standardsManager.TrustDriver).DriverDictionary.Namespace.Value))
                {
                    return element;
                }
                if ((element.LocalName == DXD.TrustDec2005Dictionary.SecondaryParameters.Value) && (element.NamespaceURI == DXD.TrustDec2005Dictionary.Namespace.Value))
                {
                    Collection<XmlElement> collection = new Collection<XmlElement>();
                    foreach (System.Xml.XmlNode node in element.ChildNodes)
                    {
                        XmlElement item = node as XmlElement;
                        if (item != null)
                        {
                            collection.Add(item);
                        }
                    }
                    XmlElement claimTypeRequirement = GetClaimTypeRequirement(collection, standardsManager);
                    if (claimTypeRequirement != null)
                    {
                        return claimTypeRequirement;
                    }
                }
            }
            return null;
        }

        protected internal override void InitializeSecurityTokenRequirement(SecurityTokenRequirement requirement)
        {
            requirement.TokenType = this.TokenType;
            requirement.RequireCryptographicToken = true;
            requirement.KeyType = this.KeyType;
            ServiceModelSecurityTokenRequirement requirement2 = requirement as ServiceModelSecurityTokenRequirement;
            if (requirement2 != null)
            {
                requirement2.DefaultMessageSecurityVersion = this.DefaultMessageSecurityVersion;
            }
            else
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.DefaultMessageSecurityVersionProperty] = this.DefaultMessageSecurityVersion;
            }
            if (this.KeySize > 0)
            {
                requirement.KeySize = this.KeySize;
            }
            requirement.Properties[ServiceModelSecurityTokenRequirement.IssuerAddressProperty] = this.IssuerAddress;
            if (this.IssuerBinding != null)
            {
                requirement.Properties[ServiceModelSecurityTokenRequirement.IssuerBindingProperty] = this.IssuerBinding;
            }
            requirement.Properties[ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty] = base.Clone();
        }

        internal static bool IsInfoCardParameters(IssuedSecurityTokenParameters parameters, SecurityStandardsManager standardsManager)
        {
            if (parameters == null)
            {
                return false;
            }
            if (parameters.TokenType != "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1")
            {
                return false;
            }
            if (parameters.KeyType != SecurityKeyType.AsymmetricKey)
            {
                return false;
            }
            if (parameters.ClaimTypeRequirements.Count == 1)
            {
                ClaimTypeRequirement requirement = parameters.ClaimTypeRequirements[0];
                if (requirement == null)
                {
                    return false;
                }
                if (requirement.ClaimType != wsidPPIClaim)
                {
                    return false;
                }
            }
            else
            {
                if ((parameters.AdditionalRequestParameters == null) || (parameters.AdditionalRequestParameters.Count <= 0))
                {
                    return false;
                }
                bool flag = false;
                XmlElement claimTypeRequirement = GetClaimTypeRequirement(parameters.AdditionalRequestParameters, standardsManager);
                if ((claimTypeRequirement != null) && (claimTypeRequirement.ChildNodes.Count == 1))
                {
                    XmlElement element2 = claimTypeRequirement.ChildNodes[0] as XmlElement;
                    if (element2 != null)
                    {
                        System.Xml.XmlNode namedItem = element2.Attributes.GetNamedItem("Uri");
                        if ((namedItem != null) && (namedItem.Value == wsidPPIClaim))
                        {
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    return false;
                }
            }
            if (parameters.IssuerAddress != null)
            {
                return false;
            }
            if ((parameters.AlternativeIssuerEndpoints != null) && (parameters.AlternativeIssuerEndpoints.Count > 0))
            {
                return false;
            }
            return true;
        }

        private Collection<XmlElement> NormalizeAdditionalParameters(Collection<XmlElement> additionalParameters, TrustDriver driver, bool clientSideClaimTypeRequirementsSpecified)
        {
            Collection<XmlElement> collection = new Collection<XmlElement>();
            foreach (XmlElement element in additionalParameters)
            {
                collection.Add(element);
            }
            if (driver.StandardsManager.TrustVersion == TrustVersion.WSTrust13)
            {
                XmlElement item = null;
                XmlElement element3 = null;
                XmlElement element4 = null;
                XmlElement element5 = null;
                for (int i = 0; i < collection.Count; i++)
                {
                    string str;
                    if (driver.IsEncryptionAlgorithmElement(collection[i], out str))
                    {
                        item = collection[i];
                    }
                    else if (driver.IsCanonicalizationAlgorithmElement(collection[i], out str))
                    {
                        element3 = collection[i];
                    }
                    else if (driver.IsKeyWrapAlgorithmElement(collection[i], out str))
                    {
                        element4 = collection[i];
                    }
                    else if (((WSTrustDec2005.DriverDec2005) driver).IsSecondaryParametersElement(collection[i]))
                    {
                        element5 = collection[i];
                    }
                }
                if (element5 != null)
                {
                    foreach (System.Xml.XmlNode node in element5.ChildNodes)
                    {
                        XmlElement element6 = node as XmlElement;
                        if (element6 != null)
                        {
                            string encryptionAlgorithm = null;
                            if (driver.IsEncryptionAlgorithmElement(element6, out encryptionAlgorithm) && (item != null))
                            {
                                collection.Remove(item);
                            }
                            else if (driver.IsCanonicalizationAlgorithmElement(element6, out encryptionAlgorithm) && (element3 != null))
                            {
                                collection.Remove(element3);
                            }
                            else if (driver.IsKeyWrapAlgorithmElement(element6, out encryptionAlgorithm) && (element4 != null))
                            {
                                collection.Remove(element4);
                            }
                        }
                    }
                }
            }
            if (((driver.StandardsManager.TrustVersion == TrustVersion.WSTrustFeb2005) && !this.CollectionContainsElementsWithTrustNamespace(additionalParameters, "http://schemas.xmlsoap.org/ws/2005/02/trust")) || ((driver.StandardsManager.TrustVersion == TrustVersion.WSTrust13) && !this.CollectionContainsElementsWithTrustNamespace(additionalParameters, "http://docs.oasis-open.org/ws-sx/ws-trust/200512")))
            {
                if (driver.StandardsManager.TrustVersion == TrustVersion.WSTrust13)
                {
                    WSTrustFeb2005.DriverFeb2005 feb = (WSTrustFeb2005.DriverFeb2005) SecurityStandardsManager.DefaultInstance.TrustDriver;
                    for (int j = 0; j < collection.Count; j++)
                    {
                        string signatureAlgorithm = string.Empty;
                        if (feb.IsSignWithElement(collection[j], out signatureAlgorithm))
                        {
                            collection[j] = driver.CreateSignWithElement(signatureAlgorithm);
                        }
                        else if (feb.IsEncryptWithElement(collection[j], out signatureAlgorithm))
                        {
                            collection[j] = driver.CreateEncryptWithElement(signatureAlgorithm);
                        }
                        else if (feb.IsEncryptionAlgorithmElement(collection[j], out signatureAlgorithm))
                        {
                            collection[j] = driver.CreateEncryptionAlgorithmElement(signatureAlgorithm);
                        }
                        else if (feb.IsCanonicalizationAlgorithmElement(collection[j], out signatureAlgorithm))
                        {
                            collection[j] = driver.CreateCanonicalizationAlgorithmElement(signatureAlgorithm);
                        }
                    }
                    return collection;
                }
                Collection<XmlElement> collection2 = null;
                WSSecurityTokenSerializer tokenSerializer = new WSSecurityTokenSerializer(SecurityVersion.WSSecurity11, TrustVersion.WSTrust13, SecureConversationVersion.WSSecureConversation13, true, null, null, null);
                SecurityStandardsManager manager2 = new SecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12, tokenSerializer);
                WSTrustDec2005.DriverDec2005 trustDriver = (WSTrustDec2005.DriverDec2005) manager2.TrustDriver;
                foreach (XmlElement element7 in collection)
                {
                    if (trustDriver.IsSecondaryParametersElement(element7))
                    {
                        collection2 = new Collection<XmlElement>();
                        foreach (System.Xml.XmlNode node2 in element7.ChildNodes)
                        {
                            XmlElement innerElement = node2 as XmlElement;
                            if ((innerElement != null) && this.CanPromoteToRoot(innerElement, trustDriver, clientSideClaimTypeRequirementsSpecified))
                            {
                                collection2.Add(innerElement);
                            }
                        }
                        collection.Remove(element7);
                        break;
                    }
                }
                if ((collection2 != null) && (collection2.Count > 0))
                {
                    XmlElement element9 = null;
                    string str4 = string.Empty;
                    XmlElement element10 = null;
                    string canonicalizationAlgorithm = string.Empty;
                    XmlElement element11 = null;
                    Collection<XmlElement> requiredClaims = null;
                    Collection<XmlElement> collection4 = new Collection<XmlElement>();
                    foreach (XmlElement element12 in collection2)
                    {
                        if ((element9 == null) && trustDriver.IsEncryptionAlgorithmElement(element12, out str4))
                        {
                            element9 = driver.CreateEncryptionAlgorithmElement(str4);
                            collection4.Add(element12);
                        }
                        else if ((element10 == null) && trustDriver.IsCanonicalizationAlgorithmElement(element12, out canonicalizationAlgorithm))
                        {
                            element10 = driver.CreateCanonicalizationAlgorithmElement(canonicalizationAlgorithm);
                            collection4.Add(element12);
                        }
                        else if ((element11 == null) && trustDriver.TryParseRequiredClaimsElement(element12, out requiredClaims))
                        {
                            element11 = driver.CreateRequiredClaimsElement(requiredClaims);
                            collection4.Add(element12);
                        }
                    }
                    for (int k = 0; k < collection4.Count; k++)
                    {
                        collection2.Remove(collection4[k]);
                    }
                    XmlElement element13 = null;
                    for (int m = 0; m < collection.Count; m++)
                    {
                        string str6;
                        if (trustDriver.IsSignWithElement(collection[m], out str6))
                        {
                            collection[m] = driver.CreateSignWithElement(str6);
                        }
                        else if (trustDriver.IsEncryptWithElement(collection[m], out str6))
                        {
                            collection[m] = driver.CreateEncryptWithElement(str6);
                        }
                        else if (trustDriver.IsEncryptionAlgorithmElement(collection[m], out str6) && (element9 != null))
                        {
                            collection[m] = element9;
                            element9 = null;
                        }
                        else if (trustDriver.IsCanonicalizationAlgorithmElement(collection[m], out str6) && (element10 != null))
                        {
                            collection[m] = element10;
                            element10 = null;
                        }
                        else if (trustDriver.IsKeyWrapAlgorithmElement(collection[m], out str6) && (element13 == null))
                        {
                            element13 = collection[m];
                        }
                        else
                        {
                            Collection<XmlElement> collection5;
                            if (trustDriver.TryParseRequiredClaimsElement(collection[m], out collection5) && (element11 != null))
                            {
                                collection[m] = element11;
                                element11 = null;
                            }
                        }
                    }
                    if (element13 != null)
                    {
                        collection.Remove(element13);
                    }
                    if (element9 != null)
                    {
                        collection.Add(element9);
                    }
                    if (element10 != null)
                    {
                        collection.Add(element10);
                    }
                    if (element11 != null)
                    {
                        collection.Add(element11);
                    }
                    if (collection2.Count <= 0)
                    {
                        return collection;
                    }
                    for (int n = 0; n < collection2.Count; n++)
                    {
                        collection.Add(collection2[n]);
                    }
                }
            }
            return collection;
        }

        internal void SetRequestParameters(Collection<XmlElement> requestParameters, TrustDriver trustDriver)
        {
            if (requestParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("requestParameters");
            }
            if (trustDriver == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("trustDriver");
            }
            Collection<XmlElement> unknownRequestParameters = new Collection<XmlElement>();
            foreach (XmlElement element in requestParameters)
            {
                int num;
                if (trustDriver.TryParseKeySizeElement(element, out num))
                {
                    this.keySize = num;
                }
                else
                {
                    SecurityKeyType type;
                    if (trustDriver.TryParseKeyTypeElement(element, out type))
                    {
                        this.KeyType = type;
                    }
                    else
                    {
                        string str;
                        if (trustDriver.TryParseTokenTypeElement(element, out str))
                        {
                            this.TokenType = str;
                        }
                        else if (trustDriver.StandardsManager.TrustVersion == TrustVersion.WSTrustFeb2005)
                        {
                            Collection<XmlElement> collection2;
                            if (trustDriver.TryParseRequiredClaimsElement(element, out collection2))
                            {
                                Collection<XmlElement> claimsList = new Collection<XmlElement>();
                                foreach (XmlElement element2 in collection2)
                                {
                                    if ((element2.LocalName == "ClaimType") && (element2.NamespaceURI == "http://schemas.xmlsoap.org/ws/2005/05/identity"))
                                    {
                                        string attribute = element2.GetAttribute("Uri", string.Empty);
                                        if (!string.IsNullOrEmpty(attribute))
                                        {
                                            ClaimTypeRequirement requirement;
                                            string str3 = element2.GetAttribute("Optional", string.Empty);
                                            if (string.IsNullOrEmpty(str3))
                                            {
                                                requirement = new ClaimTypeRequirement(attribute);
                                            }
                                            else
                                            {
                                                requirement = new ClaimTypeRequirement(attribute, XmlConvert.ToBoolean(str3));
                                            }
                                            this.claimTypeRequirements.Add(requirement);
                                        }
                                    }
                                    else
                                    {
                                        claimsList.Add(element2);
                                    }
                                }
                                if (claimsList.Count > 0)
                                {
                                    unknownRequestParameters.Add(trustDriver.CreateRequiredClaimsElement(claimsList));
                                }
                            }
                            else
                            {
                                unknownRequestParameters.Add(element);
                            }
                        }
                    }
                }
            }
            unknownRequestParameters = trustDriver.ProcessUnknownRequestParameters(unknownRequestParameters, requestParameters);
            if (unknownRequestParameters.Count > 0)
            {
                for (int i = 0; i < unknownRequestParameters.Count; i++)
                {
                    this.AdditionalRequestParameters.Add(unknownRequestParameters[i]);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(base.ToString());
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "TokenType: {0}", new object[] { (this.tokenType == null) ? "null" : this.tokenType }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "KeyType: {0}", new object[] { this.keyType.ToString() }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "KeySize: {0}", new object[] { this.keySize.ToString(CultureInfo.InvariantCulture) }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "IssuerAddress: {0}", new object[] { (this.issuerAddress == null) ? "null" : this.issuerAddress.ToString() }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "IssuerMetadataAddress: {0}", new object[] { (this.issuerMetadataAddress == null) ? "null" : this.issuerMetadataAddress.ToString() }));
            builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "DefaultMessgeSecurityVersion: {0}", new object[] { (this.defaultMessageSecurityVersion == null) ? "null" : this.defaultMessageSecurityVersion.ToString() }));
            if (this.issuerBinding == null)
            {
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "IssuerBinding: null", new object[0]));
            }
            else
            {
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "IssuerBinding:", new object[0]));
                BindingElementCollection elements = this.issuerBinding.CreateBindingElements();
                for (int i = 0; i < elements.Count; i++)
                {
                    builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "  BindingElement[{0}]:", new object[] { i.ToString(CultureInfo.InvariantCulture) }));
                    builder.AppendLine("    " + elements[i].ToString().Trim().Replace("\n", "\n    "));
                }
            }
            if (this.claimTypeRequirements.Count == 0)
            {
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "ClaimTypeRequirements: none", new object[0]));
            }
            else
            {
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "ClaimTypeRequirements:", new object[0]));
                for (int j = 0; j < this.claimTypeRequirements.Count; j++)
                {
                    builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "  {0}, optional={1}", new object[] { this.claimTypeRequirements[j].ClaimType, this.claimTypeRequirements[j].IsOptional }));
                }
            }
            return builder.ToString().Trim();
        }

        public Collection<XmlElement> AdditionalRequestParameters
        {
            get
            {
                return this.additionalRequestParameters;
            }
        }

        internal Collection<AlternativeIssuerEndpoint> AlternativeIssuerEndpoints
        {
            get
            {
                return this.alternativeIssuerEndpoints;
            }
        }

        public Collection<ClaimTypeRequirement> ClaimTypeRequirements
        {
            get
            {
                return this.claimTypeRequirements;
            }
        }

        public MessageSecurityVersion DefaultMessageSecurityVersion
        {
            get
            {
                return this.defaultMessageSecurityVersion;
            }
            set
            {
                this.defaultMessageSecurityVersion = value;
            }
        }

        protected internal override bool HasAsymmetricKey
        {
            get
            {
                return (this.KeyType == SecurityKeyType.AsymmetricKey);
            }
        }

        public EndpointAddress IssuerAddress
        {
            get
            {
                return this.issuerAddress;
            }
            set
            {
                this.issuerAddress = value;
            }
        }

        public Binding IssuerBinding
        {
            get
            {
                return this.issuerBinding;
            }
            set
            {
                this.issuerBinding = value;
            }
        }

        public EndpointAddress IssuerMetadataAddress
        {
            get
            {
                return this.issuerMetadataAddress;
            }
            set
            {
                this.issuerMetadataAddress = value;
            }
        }

        public int KeySize
        {
            get
            {
                return this.keySize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.keySize = value;
            }
        }

        public SecurityKeyType KeyType
        {
            get
            {
                return this.keyType;
            }
            set
            {
                SecurityKeyTypeHelper.Validate(value);
                this.keyType = value;
            }
        }

        protected internal override bool SupportsClientAuthentication
        {
            get
            {
                return true;
            }
        }

        protected internal override bool SupportsClientWindowsIdentity
        {
            get
            {
                return false;
            }
        }

        protected internal override bool SupportsServerAuthentication
        {
            get
            {
                return true;
            }
        }

        public string TokenType
        {
            get
            {
                return this.tokenType;
            }
            set
            {
                this.tokenType = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AlternativeIssuerEndpoint
        {
            public EndpointAddress IssuerAddress;
            public EndpointAddress IssuerMetadataAddress;
            public Binding IssuerBinding;
        }
    }
}

