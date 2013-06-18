namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    public class SecurityBindingElementImporter : IPolicyImportExtension
    {
        internal const string ContractProtectionLevelKey = "ContractProtectionLevelKey";
        internal const string InSecureConversationBootstrapBindingImportMode = "InSecureConversationBootstrapBindingImportMode";
        private int maxPolicyRedirections = 10;
        internal const string MaxPolicyRedirectionsKey = "MaxPolicyRedirections";
        internal const string SecureConversationBootstrapEncryptionRequirements = "SecureConversationBootstrapEncryptionRequirements";
        internal const string SecureConversationBootstrapSignatureRequirements = "SecureConversationBootstrapSignatureRequirements";

        private void AddParts(ref MessagePartSpecification parts1, MessagePartSpecification parts2)
        {
            if (parts1 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parts1"));
            }
            if (parts2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parts2"));
            }
            if (!parts2.IsEmpty())
            {
                if (parts1.IsReadOnly)
                {
                    MessagePartSpecification specification = new MessagePartSpecification();
                    specification.Union(parts1);
                    specification.Union(parts2);
                    parts1 = specification;
                }
                else
                {
                    parts1.Union(parts2);
                }
            }
        }

        private static ProtectionLevel GetProtectionLevel(bool signed, bool encrypted, string action)
        {
            if (encrypted)
            {
                if (!signed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidChannelBindingException(System.ServiceModel.SR.GetString("PolicyRequiresConfidentialityWithoutIntegrity", new object[] { action })));
                }
                return ProtectionLevel.EncryptAndSign;
            }
            if (signed)
            {
                return ProtectionLevel.Sign;
            }
            return ProtectionLevel.None;
        }

        private static bool HasSupportingTokens(SecurityBindingElement binding)
        {
            if (((binding.EndpointSupportingTokenParameters.Endorsing.Count > 0) || (binding.EndpointSupportingTokenParameters.SignedEndorsing.Count > 0)) || ((binding.EndpointSupportingTokenParameters.SignedEncrypted.Count > 0) || (binding.EndpointSupportingTokenParameters.Signed.Count > 0)))
            {
                return true;
            }
            foreach (SupportingTokenParameters parameters in binding.OperationSupportingTokenParameters.Values)
            {
                if (((parameters.Endorsing.Count > 0) || (parameters.SignedEndorsing.Count > 0)) || ((parameters.SignedEncrypted.Count > 0) || (parameters.Signed.Count > 0)))
                {
                    return true;
                }
            }
            return false;
        }

        private void ImportEndpointScopeMessageBindingAssertions(MetadataImporter importer, PolicyConversionContext policyContext, SecurityBindingElement binding)
        {
            XmlElement assertion = null;
            WSSecurityPolicy policy;
            this.ImportSupportingTokenAssertions(importer, policyContext, policyContext.GetBindingAssertions(), binding.EndpointSupportingTokenParameters, binding.OptionalEndpointSupportingTokenParameters);
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out policy))
            {
                if (!policy.TryImportWsspWssAssertion(importer, policyContext.GetBindingAssertions(), binding, out assertion) && (assertion != null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { assertion.OuterXml })));
                }
                if (!policy.TryImportWsspTrustAssertion(importer, policyContext.GetBindingAssertions(), binding, out assertion) && (assertion != null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { assertion.OuterXml })));
                }
            }
            if (assertion == null)
            {
                binding.DoNotEmitTrust = true;
            }
        }

        private void ImportMessageScopeProtectionPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            MessagePartSpecification specification;
            MessagePartSpecification specification2;
            bool flag;
            ContractProtectionLevel level = null;
            bool hasProtectionRequirements = false;
            bool hasUniformProtectionLevel = true;
            ProtectionLevel none = ProtectionLevel.None;
            string key = string.Format("{0}:{1}:{2}", "ContractProtectionLevelKey", policyContext.Contract.Name, policyContext.Contract.Namespace);
            if (importer.State.ContainsKey(key))
            {
                flag = true;
                level = (ContractProtectionLevel) importer.State[key];
            }
            else
            {
                flag = false;
            }
            ICollection<XmlElement> bindingAssertions = policyContext.GetBindingAssertions();
            this.ImportProtectionAssertions(bindingAssertions, out specification, out specification2);
            if (importer.State.ContainsKey("InSecureConversationBootstrapBindingImportMode"))
            {
                if (specification2 != null)
                {
                    importer.State["SecureConversationBootstrapEncryptionRequirements"] = specification2;
                }
                if (specification != null)
                {
                    importer.State["SecureConversationBootstrapSignatureRequirements"] = specification;
                }
            }
            foreach (OperationDescription description in policyContext.Contract.Operations)
            {
                MessagePartSpecification specification3;
                MessagePartSpecification specification4;
                MessagePartSpecification specification5;
                MessagePartSpecification specification6;
                ICollection<XmlElement> operationBindingAssertions = policyContext.GetOperationBindingAssertions(description);
                this.ImportProtectionAssertions(operationBindingAssertions, out specification3, out specification4);
                this.AddParts(ref specification3, specification);
                this.AddParts(ref specification4, specification2);
                bool flag4 = false;
                bool flag5 = true;
                ProtectionLevel level3 = ProtectionLevel.None;
                foreach (MessageDescription description2 in description.Messages)
                {
                    ICollection<XmlElement> messageBindingAssertions = policyContext.GetMessageBindingAssertions(description2);
                    this.ImportProtectionAssertions(messageBindingAssertions, out specification5, out specification6);
                    this.AddParts(ref specification5, specification3);
                    this.AddParts(ref specification6, specification4);
                    ProtectionLevel newProtectionLevel = GetProtectionLevel(specification5.IsBodyIncluded, specification6.IsBodyIncluded, description2.Action);
                    if (OperationFormatter.IsValidReturnValue(description2.Body.ReturnValue))
                    {
                        this.ValidateExistingOrSetNewProtectionLevel(description2.Body.ReturnValue, description2, description, policyContext.Contract, newProtectionLevel);
                    }
                    foreach (MessagePartDescription description3 in description2.Body.Parts)
                    {
                        this.ValidateExistingOrSetNewProtectionLevel(description3, description2, description, policyContext.Contract, newProtectionLevel);
                    }
                    if (!OperationFormatter.IsValidReturnValue(description2.Body.ReturnValue) || (description2.Body.Parts.Count == 0))
                    {
                        this.ValidateExistingOrSetNewProtectionLevel(null, description2, description, policyContext.Contract, newProtectionLevel);
                    }
                    if (flag4)
                    {
                        if (level3 != newProtectionLevel)
                        {
                            flag5 = false;
                        }
                    }
                    else
                    {
                        level3 = newProtectionLevel;
                        flag4 = true;
                    }
                    if (hasProtectionRequirements)
                    {
                        if (none != newProtectionLevel)
                        {
                            hasUniformProtectionLevel = false;
                        }
                    }
                    else
                    {
                        none = newProtectionLevel;
                        hasProtectionRequirements = true;
                    }
                    foreach (MessageHeaderDescription description4 in description2.Headers)
                    {
                        bool signed = specification5.IsHeaderIncluded(description4.Name, description4.Namespace);
                        bool encrypted = specification6.IsHeaderIncluded(description4.Name, description4.Namespace);
                        newProtectionLevel = GetProtectionLevel(signed, encrypted, description2.Action);
                        this.ValidateExistingOrSetNewProtectionLevel(description4, description2, description, policyContext.Contract, newProtectionLevel);
                        if (flag4)
                        {
                            if (level3 != newProtectionLevel)
                            {
                                flag5 = false;
                            }
                        }
                        else
                        {
                            level3 = newProtectionLevel;
                            flag4 = true;
                        }
                        if (hasProtectionRequirements)
                        {
                            if (none != newProtectionLevel)
                            {
                                hasUniformProtectionLevel = false;
                            }
                        }
                        else
                        {
                            none = newProtectionLevel;
                            hasProtectionRequirements = true;
                        }
                    }
                }
                if (flag4 && flag5)
                {
                    this.ResetProtectionLevelForMessages(description);
                    description.ProtectionLevel = level3;
                }
                foreach (FaultDescription description5 in description.Faults)
                {
                    ICollection<XmlElement> faultBindingAssertions = policyContext.GetFaultBindingAssertions(description5);
                    this.ImportProtectionAssertions(faultBindingAssertions, out specification5, out specification6);
                    this.AddParts(ref specification5, specification3);
                    this.AddParts(ref specification6, specification4);
                    ProtectionLevel level5 = GetProtectionLevel(specification5.IsBodyIncluded, specification6.IsBodyIncluded, description5.Action);
                    if (description5.HasProtectionLevel)
                    {
                        if (description5.ProtectionLevel != level5)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("CannotImportProtectionLevelForContract", new object[] { policyContext.Contract.Name, policyContext.Contract.Namespace })));
                        }
                    }
                    else
                    {
                        description5.ProtectionLevel = level5;
                    }
                    if (hasProtectionRequirements)
                    {
                        if (none != level5)
                        {
                            hasUniformProtectionLevel = false;
                        }
                    }
                    else
                    {
                        none = level5;
                        hasProtectionRequirements = true;
                    }
                }
            }
            if (flag)
            {
                if (((hasProtectionRequirements != level.HasProtectionRequirements) || (hasUniformProtectionLevel != level.HasUniformProtectionLevel)) || (none != level.UniformProtectionLevel))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("CannotImportProtectionLevelForContract", new object[] { policyContext.Contract.Name, policyContext.Contract.Namespace })));
                }
            }
            else
            {
                if ((hasProtectionRequirements && hasUniformProtectionLevel) && (none == ProtectionLevel.EncryptAndSign))
                {
                    foreach (OperationDescription description6 in policyContext.Contract.Operations)
                    {
                        this.ResetProtectionLevelForMessages(description6);
                        foreach (FaultDescription description7 in description6.Faults)
                        {
                            description7.ResetProtectionLevel();
                        }
                        description6.ResetProtectionLevel();
                    }
                }
                importer.State[key] = new ContractProtectionLevel(hasProtectionRequirements, hasUniformProtectionLevel, none);
            }
        }

        private void ImportOperationScopeSupportingTokensPolicy(MetadataImporter importer, PolicyConversionContext policyContext, SecurityBindingElement binding)
        {
            foreach (OperationDescription description in policyContext.Contract.Operations)
            {
                string action = null;
                foreach (MessageDescription description2 in description.Messages)
                {
                    if (description2.Direction == MessageDirection.Input)
                    {
                        action = description2.Action;
                        break;
                    }
                }
                SupportingTokenParameters requirements = new SupportingTokenParameters();
                SupportingTokenParameters optionalRequirements = new SupportingTokenParameters();
                ICollection<XmlElement> operationBindingAssertions = policyContext.GetOperationBindingAssertions(description);
                this.ImportSupportingTokenAssertions(importer, policyContext, operationBindingAssertions, requirements, optionalRequirements);
                if (((requirements.Endorsing.Count > 0) || (requirements.Signed.Count > 0)) || ((requirements.SignedEncrypted.Count > 0) || (requirements.SignedEndorsing.Count > 0)))
                {
                    if (action == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotImportSupportingTokensForOperationWithoutRequestAction")));
                    }
                    binding.OperationSupportingTokenParameters[action] = requirements;
                }
                if (((optionalRequirements.Endorsing.Count > 0) || (optionalRequirements.Signed.Count > 0)) || ((optionalRequirements.SignedEncrypted.Count > 0) || (optionalRequirements.SignedEndorsing.Count > 0)))
                {
                    if (action == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotImportSupportingTokensForOperationWithoutRequestAction")));
                    }
                    binding.OptionalOperationSupportingTokenParameters[action] = optionalRequirements;
                }
            }
        }

        private void ImportProtectionAssertions(ICollection<XmlElement> assertions, out MessagePartSpecification signedParts, out MessagePartSpecification encryptedParts)
        {
            WSSecurityPolicy policy;
            signedParts = null;
            encryptedParts = null;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(assertions, out policy))
            {
                XmlElement element;
                if (!policy.TryImportWsspEncryptedPartsAssertion(assertions, out encryptedParts, out element) && (element != null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element.OuterXml })));
                }
                if (!policy.TryImportWsspSignedPartsAssertion(assertions, out signedParts, out element) && (element != null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element.OuterXml })));
                }
            }
            if (encryptedParts == null)
            {
                encryptedParts = MessagePartSpecification.NoParts;
            }
            if (signedParts == null)
            {
                signedParts = MessagePartSpecification.NoParts;
            }
        }

        private void ImportSupportingTokenAssertions(MetadataImporter importer, PolicyConversionContext policyContext, ICollection<XmlElement> assertions, SupportingTokenParameters requirements, SupportingTokenParameters optionalRequirements)
        {
            WSSecurityPolicy policy;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(assertions, out policy))
            {
                policy.TryImportWsspSupportingTokensAssertion(importer, policyContext, assertions, requirements.Signed, requirements.SignedEncrypted, requirements.Endorsing, requirements.SignedEndorsing, optionalRequirements.Signed, optionalRequirements.SignedEncrypted, optionalRequirements.Endorsing, optionalRequirements.SignedEndorsing);
            }
        }

        private void ResetProtectionLevelForMessages(OperationDescription operation)
        {
            foreach (MessageDescription description in operation.Messages)
            {
                if (OperationFormatter.IsValidReturnValue(description.Body.ReturnValue))
                {
                    description.Body.ReturnValue.ResetProtectionLevel();
                }
                foreach (MessagePartDescription description2 in description.Body.Parts)
                {
                    description2.ResetProtectionLevel();
                }
                foreach (MessageHeaderDescription description3 in description.Headers)
                {
                    description3.ResetProtectionLevel();
                }
                description.ResetProtectionLevel();
            }
        }

        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            WSSecurityPolicy policy;
            if (importer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("importer");
            }
            if (policyContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");
            }
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out policy))
            {
                if ((importer.State != null) && !importer.State.ContainsKey("MaxPolicyRedirections"))
                {
                    importer.State.Add("MaxPolicyRedirections", this.MaxPolicyRedirections);
                }
                SecurityBindingElement sbe = null;
                bool flag = this.TryImportSymmetricSecurityBindingElement(importer, policyContext, out sbe);
                if (!flag)
                {
                    flag = this.TryImportTransportSecurityBindingElement(importer, policyContext, out sbe);
                }
                if (!flag)
                {
                    this.TryImportAsymmetricSecurityBindingElement(importer, policyContext, out sbe);
                }
                if (sbe != null)
                {
                    SecurityElement element2 = new SecurityElement();
                    element2.InitializeFrom(sbe, false);
                    if (element2.HasImportFailed)
                    {
                        importer.Errors.Add(new MetadataConversionError(System.ServiceModel.SR.GetString("SecurityBindingElementCannotBeExpressedInConfig"), true));
                    }
                }
            }
        }

        private bool TryImportAsymmetricSecurityBindingElement(MetadataImporter importer, PolicyConversionContext policyContext, out SecurityBindingElement sbe)
        {
            AsymmetricSecurityBindingElement binding = null;
            WSSecurityPolicy policy;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out policy))
            {
                XmlElement element2;
                if (policy.TryImportWsspAsymmetricBindingAssertion(importer, policyContext, policyContext.GetBindingAssertions(), out binding, out element2))
                {
                    this.ImportEndpointScopeMessageBindingAssertions(importer, policyContext, binding);
                    this.ImportOperationScopeSupportingTokensPolicy(importer, policyContext, binding);
                    this.ImportMessageScopeProtectionPolicy(importer, policyContext);
                    policyContext.BindingElements.Add(binding);
                }
                else if (element2 != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element2.OuterXml })));
                }
            }
            sbe = binding;
            return (binding != null);
        }

        private bool TryImportSymmetricSecurityBindingElement(MetadataImporter importer, PolicyConversionContext policyContext, out SecurityBindingElement sbe)
        {
            SymmetricSecurityBindingElement binding = null;
            WSSecurityPolicy policy;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out policy))
            {
                XmlElement element2;
                if (policy.TryImportWsspSymmetricBindingAssertion(importer, policyContext, policyContext.GetBindingAssertions(), out binding, out element2))
                {
                    this.ImportEndpointScopeMessageBindingAssertions(importer, policyContext, binding);
                    this.ImportOperationScopeSupportingTokensPolicy(importer, policyContext, binding);
                    this.ImportMessageScopeProtectionPolicy(importer, policyContext);
                    policyContext.BindingElements.Add(binding);
                }
                else if (element2 != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element2.OuterXml })));
                }
            }
            sbe = binding;
            return (binding != null);
        }

        private bool TryImportTransportSecurityBindingElement(MetadataImporter importer, PolicyConversionContext policyContext, out SecurityBindingElement sbe)
        {
            TransportSecurityBindingElement binding = null;
            WSSecurityPolicy policy;
            sbe = null;
            if (WSSecurityPolicy.TryGetSecurityPolicyDriver(policyContext.GetBindingAssertions(), out policy))
            {
                XmlElement element2;
                if (policy.TryImportWsspTransportBindingAssertion(importer, policyContext.GetBindingAssertions(), out binding, out element2))
                {
                    this.ImportEndpointScopeMessageBindingAssertions(importer, policyContext, binding);
                    this.ImportOperationScopeSupportingTokensPolicy(importer, policyContext, binding);
                    if (importer.State.ContainsKey("InSecureConversationBootstrapBindingImportMode"))
                    {
                        this.ImportMessageScopeProtectionPolicy(importer, policyContext);
                    }
                    if (HasSupportingTokens(binding) || binding.IncludeTimestamp)
                    {
                        sbe = binding;
                        policyContext.BindingElements.Add(binding);
                    }
                }
                else if (element2 != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnsupportedSecurityPolicyAssertion", new object[] { element2.OuterXml })));
                }
            }
            return (binding != null);
        }

        private void ValidateExistingOrSetNewProtectionLevel(MessagePartDescription part, MessageDescription message, OperationDescription operation, ContractDescription contract, ProtectionLevel newProtectionLevel)
        {
            ProtectionLevel protectionLevel;
            if ((part != null) && part.HasProtectionLevel)
            {
                protectionLevel = part.ProtectionLevel;
            }
            else if (message.HasProtectionLevel)
            {
                protectionLevel = message.ProtectionLevel;
            }
            else if (operation.HasProtectionLevel)
            {
                protectionLevel = operation.ProtectionLevel;
            }
            else
            {
                if (part != null)
                {
                    part.ProtectionLevel = newProtectionLevel;
                }
                else
                {
                    message.ProtectionLevel = newProtectionLevel;
                }
                protectionLevel = newProtectionLevel;
            }
            if (protectionLevel != newProtectionLevel)
            {
                if ((part != null) && !part.HasProtectionLevel)
                {
                    part.ProtectionLevel = newProtectionLevel;
                }
                else
                {
                    if ((part != null) || message.HasProtectionLevel)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("CannotImportProtectionLevelForContract", new object[] { contract.Name, contract.Namespace })));
                    }
                    message.ProtectionLevel = newProtectionLevel;
                }
            }
        }

        public int MaxPolicyRedirections
        {
            get
            {
                return this.maxPolicyRedirections;
            }
        }

        private class ContractProtectionLevel
        {
            private bool hasProtectionRequirements;
            private bool hasUniformProtectionLevel;
            private ProtectionLevel uniformProtectionLevel;

            public ContractProtectionLevel(bool hasProtectionRequirements, bool hasUniformProtectionLevel, ProtectionLevel uniformProtectionLevel)
            {
                this.hasProtectionRequirements = hasProtectionRequirements;
                this.hasUniformProtectionLevel = hasUniformProtectionLevel;
                this.uniformProtectionLevel = uniformProtectionLevel;
            }

            public bool HasProtectionRequirements
            {
                get
                {
                    return this.hasProtectionRequirements;
                }
            }

            public bool HasUniformProtectionLevel
            {
                get
                {
                    return this.hasUniformProtectionLevel;
                }
            }

            public ProtectionLevel UniformProtectionLevel
            {
                get
                {
                    return this.uniformProtectionLevel;
                }
            }
        }
    }
}

