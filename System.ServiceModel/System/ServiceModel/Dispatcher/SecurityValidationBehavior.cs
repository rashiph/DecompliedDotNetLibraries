namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    internal class SecurityValidationBehavior : IEndpointBehavior, IServiceBehavior
    {
        private static SecurityValidationBehavior instance;

        internal void AfterBuildTimeValidation(System.ServiceModel.Description.ServiceDescription description)
        {
            S4UImpersonationRule.Validate(description);
        }

        private static SecurityBindingElement GetSecurityBinding(Binding binding, ContractDescription contract)
        {
            SecurityBindingElement element = null;
            BindingElementCollection elements = binding.CreateBindingElements();
            for (int i = 0; i < elements.Count; i++)
            {
                BindingElement element2 = elements[i];
                if (element2 is SecurityBindingElement)
                {
                    if (element != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MoreThanOneSecurityBindingElementInTheBinding", new object[] { binding.Name, binding.Namespace, contract.Name, contract.Namespace })));
                    }
                    element = (SecurityBindingElement) element2;
                }
            }
            return element;
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection parameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
            SecurityBindingElement element;
            if (serviceEndpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint");
            }
            Binding binding = new ValidationBinding(serviceEndpoint.Binding);
            this.ValidateBinding(binding, serviceEndpoint.Contract, out element);
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            for (int i = 0; i < description.Endpoints.Count; i++)
            {
                SecurityBindingElement element;
                ServiceEndpoint endpoint = description.Endpoints[i];
                Binding binding = new ValidationBinding(endpoint.Binding);
                this.ValidateBinding(binding, endpoint.Contract, out element);
                if (element != null)
                {
                    SecurityTokenParameterInclusionModeRule.Validate(element, binding, endpoint.Contract, description.Behaviors);
                }
            }
            WindowsIdentitySupportRule.Validate(description);
            UsernameImpersonationRule.Validate(description);
            MissingClientCertificateRule.Validate(description);
        }

        private void ValidateBinding(Binding binding, ContractDescription contract, out SecurityBindingElement securityBindingElement)
        {
            securityBindingElement = GetSecurityBinding(binding, contract);
            if (securityBindingElement != null)
            {
                this.ValidateSecurityBinding(securityBindingElement, binding, contract);
            }
            else
            {
                this.ValidateNoSecurityBinding(binding, contract);
            }
        }

        private void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
        {
            ContractProtectionRequirementsRule.ValidateNoSecurityBinding(binding, contract);
            CookieAndSessionProtectionRequirementsRule.ValidateNoSecurityBinding(binding, contract);
            SoapOverSecureTransportRequirementsRule.ValidateNoSecurityBinding(binding, contract);
            SecurityVersionSupportForEncryptedKeyBindingRule.ValidateNoSecurityBinding(binding, contract);
            SecurityVersionSupportForThumbprintKeyIdentifierClauseRule.ValidateNoSecurityBinding(binding, contract);
            SecurityBindingSupportForOneWayOnlyRule.ValidateNoSecurityBinding(binding, contract);
            IssuedKeySizeCompatibilityWithAlgorithmSuiteRule.ValidateNoSecurityBinding(binding, contract);
            MessageSecurityAndManualAddressingRule.ValidateNoSecurityBinding(binding, contract);
            UnknownHeaderProtectionRequirementsRule.ValidateNoSecurityBinding(binding, contract);
            BearerKeyTypeIssuanceRequirementRule.ValidateNoSecurityBinding(binding, contract);
        }

        private void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
        {
            ContractProtectionRequirementsRule.ValidateSecurityBinding(sbe, binding, contract);
            CookieAndSessionProtectionRequirementsRule.ValidateSecurityBinding(sbe, binding, contract);
            SoapOverSecureTransportRequirementsRule.ValidateSecurityBinding(sbe, binding, contract);
            SecurityVersionSupportForEncryptedKeyBindingRule.ValidateSecurityBinding(sbe, binding, contract);
            SecurityVersionSupportForThumbprintKeyIdentifierClauseRule.ValidateSecurityBinding(sbe, binding, contract);
            SecurityBindingSupportForOneWayOnlyRule.ValidateSecurityBinding(sbe, binding, contract);
            IssuedKeySizeCompatibilityWithAlgorithmSuiteRule.ValidateSecurityBinding(sbe, binding, contract);
            MessageSecurityAndManualAddressingRule.ValidateSecurityBinding(sbe, binding, contract);
            NoStreamingWithSecurityRule.ValidateSecurityBinding(sbe, binding, contract);
            UnknownHeaderProtectionRequirementsRule.ValidateSecurityBinding(sbe, binding, contract);
            BearerKeyTypeIssuanceRequirementRule.ValidateSecurityBinding(sbe, binding, contract);
        }

        public static SecurityValidationBehavior Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SecurityValidationBehavior();
                }
                return instance;
            }
        }

        private static class BearerKeyTypeIssuanceRequirementRule
        {
            private static bool IsBearerKeyType(SecurityTokenParameters tokenParameters)
            {
                return ((tokenParameters is IssuedSecurityTokenParameters) && (((IssuedSecurityTokenParameters) tokenParameters).KeyType == SecurityKeyType.BearerKey));
            }

            public static void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }

            public static void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                foreach (SecurityTokenParameters parameters in new System.ServiceModel.Security.SecurityTokenParametersEnumerable(sbe, true))
                {
                    if (parameters is IssuedSecurityTokenParameters)
                    {
                        IssuedSecurityTokenParameters parameters2 = parameters as IssuedSecurityTokenParameters;
                        if (parameters2.KeyType == SecurityKeyType.BearerKey)
                        {
                            if ((sbe is SymmetricSecurityBindingElement) && IsBearerKeyType(((SymmetricSecurityBindingElement) sbe).ProtectionTokenParameters))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidBearerKeyUsage", new object[] { binding.Name, binding.Namespace })));
                            }
                            if ((sbe is AsymmetricSecurityBindingElement) && (IsBearerKeyType(((AsymmetricSecurityBindingElement) sbe).InitiatorTokenParameters) || IsBearerKeyType(((AsymmetricSecurityBindingElement) sbe).RecipientTokenParameters)))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidBearerKeyUsage", new object[] { binding.Name, binding.Namespace })));
                            }
                            foreach (SecurityTokenParameters parameters3 in sbe.EndpointSupportingTokenParameters.Endorsing)
                            {
                                if (IsBearerKeyType(parameters3))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidBearerKeyUsage", new object[] { binding.Name, binding.Namespace })));
                                }
                            }
                            foreach (SecurityTokenParameters parameters4 in sbe.EndpointSupportingTokenParameters.SignedEndorsing)
                            {
                                if (IsBearerKeyType(parameters4))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidBearerKeyUsage", new object[] { binding.Name, binding.Namespace })));
                                }
                            }
                        }
                        if (parameters2.IssuerBinding != null)
                        {
                            SecurityBindingElement securityBinding = SecurityValidationBehavior.GetSecurityBinding(parameters2.IssuerBinding, contract);
                            if (securityBinding != null)
                            {
                                ValidateSecurityBinding(securityBinding, parameters2.IssuerBinding, contract);
                            }
                        }
                    }
                    else if (parameters is SecureConversationSecurityTokenParameters)
                    {
                        SecureConversationSecurityTokenParameters parameters5 = parameters as SecureConversationSecurityTokenParameters;
                        ValidateSecurityBinding(parameters5.BootstrapSecurityBindingElement, binding, contract);
                    }
                }
            }
        }

        private static class ContractProtectionRequirementsRule
        {
            internal static void GetRequiredProtectionLevels(ContractDescription contract, ProtectionLevel defaultRequestProtectionLevel, ProtectionLevel defaultResponseProtectionLevel, out ProtectionLevel request, out ProtectionLevel response)
            {
                ChannelProtectionRequirements requirements = ChannelProtectionRequirements.CreateFromContract(contract, defaultRequestProtectionLevel, defaultResponseProtectionLevel, false);
                if (requirements.IncomingSignatureParts.IsEmpty())
                {
                    request = ProtectionLevel.None;
                }
                else if (requirements.IncomingEncryptionParts.IsEmpty())
                {
                    request = ProtectionLevel.Sign;
                }
                else
                {
                    request = ProtectionLevel.EncryptAndSign;
                }
                if (requirements.OutgoingSignatureParts.IsEmpty())
                {
                    response = ProtectionLevel.None;
                }
                else if (requirements.OutgoingEncryptionParts.IsEmpty())
                {
                    response = ProtectionLevel.Sign;
                }
                else
                {
                    response = ProtectionLevel.EncryptAndSign;
                }
            }

            private static void ValidateBindingProtectionCapability(Binding binding, ContractDescription contract, ProtectionLevel request, ProtectionLevel response)
            {
                bool flag = request == ProtectionLevel.None;
                bool flag2 = response == ProtectionLevel.None;
                if (!flag || !flag2)
                {
                    ISecurityCapabilities property = binding.GetProperty<ISecurityCapabilities>(new BindingParameterCollection());
                    if (property != null)
                    {
                        if (!flag)
                        {
                            flag = ProtectionLevelHelper.IsStrongerOrEqual(property.SupportedRequestProtectionLevel, request);
                        }
                        if (!flag2)
                        {
                            flag2 = ProtectionLevelHelper.IsStrongerOrEqual(property.SupportedResponseProtectionLevel, response);
                        }
                    }
                }
                if (!flag)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AtLeastOneContractOperationRequestRequiresProtectionLevelNotSupportedByBinding", new object[] { contract.Name, contract.Namespace, binding.Name, binding.Namespace })));
                }
                if (!flag2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("AtLeastOneContractOperationResponseRequiresProtectionLevelNotSupportedByBinding", new object[] { contract.Name, contract.Namespace, binding.Name, binding.Namespace })));
                }
            }

            private static void ValidateContract(Binding binding, ContractDescription contract, ProtectionLevel defaultRequestProtectionLevel, ProtectionLevel defaultResponseProtectionLevel)
            {
                ProtectionLevel level;
                ProtectionLevel level2;
                GetRequiredProtectionLevels(contract, defaultRequestProtectionLevel, defaultResponseProtectionLevel, out level, out level2);
                ValidateBindingProtectionCapability(binding, contract, level, level2);
            }

            public static void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
                ValidateContract(binding, contract, ProtectionLevel.None, ProtectionLevel.None);
            }

            public static void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                if ((sbe is SymmetricSecurityBindingElement) || (sbe is AsymmetricSecurityBindingElement))
                {
                    ValidateContract(binding, contract, sbe.GetIndividualProperty<ISecurityCapabilities>().SupportedRequestProtectionLevel, sbe.GetIndividualProperty<ISecurityCapabilities>().SupportedResponseProtectionLevel);
                }
                else
                {
                    ValidateContract(binding, contract, ProtectionLevel.None, ProtectionLevel.None);
                }
            }
        }

        private static class CookieAndSessionProtectionRequirementsRule
        {
            public static void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }

            public static void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                if (!(sbe is TransportSecurityBindingElement))
                {
                    foreach (SecurityTokenParameters parameters in new System.ServiceModel.Security.SecurityTokenParametersEnumerable(sbe, true))
                    {
                        SecureConversationSecurityTokenParameters parameters2 = parameters as SecureConversationSecurityTokenParameters;
                        if (parameters2 != null)
                        {
                            ISecurityCapabilities individualProperty = parameters2.BootstrapSecurityBindingElement.GetIndividualProperty<ISecurityCapabilities>();
                            if (((individualProperty == null) || (individualProperty.SupportedRequestProtectionLevel != ProtectionLevel.EncryptAndSign)) || (individualProperty.SupportedResponseProtectionLevel != ProtectionLevel.EncryptAndSign))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BindingDoesNotSupportProtectionForRst", new object[] { binding.Name, binding.Namespace, contract.Name, contract.Namespace })));
                            }
                        }
                    }
                }
            }
        }

        private static class IssuedKeySizeCompatibilityWithAlgorithmSuiteRule
        {
            public static void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }

            public static void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                SecurityAlgorithmSuite defaultAlgorithmSuite = sbe.DefaultAlgorithmSuite;
                foreach (SecurityTokenParameters parameters in new System.ServiceModel.Security.SecurityTokenParametersEnumerable(sbe, true))
                {
                    if (parameters is IssuedSecurityTokenParameters)
                    {
                        IssuedSecurityTokenParameters parameters2 = parameters as IssuedSecurityTokenParameters;
                        if (parameters2.KeySize != 0)
                        {
                            bool flag = true;
                            if ((parameters2.KeyType == SecurityKeyType.SymmetricKey) && !sbe.DefaultAlgorithmSuite.IsSymmetricKeyLengthSupported(parameters2.KeySize))
                            {
                                flag = false;
                            }
                            else if ((parameters2.KeyType == SecurityKeyType.AsymmetricKey) && !sbe.DefaultAlgorithmSuite.IsAsymmetricKeyLengthSupported(parameters2.KeySize))
                            {
                                flag = false;
                            }
                            if (!flag)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuedKeySizeNotCompatibleWithAlgorithmSuite", new object[] { binding.Name, binding.Namespace, sbe.DefaultAlgorithmSuite, parameters2.KeySize })));
                            }
                        }
                    }
                    else if (parameters is SecureConversationSecurityTokenParameters)
                    {
                        SecureConversationSecurityTokenParameters parameters3 = parameters as SecureConversationSecurityTokenParameters;
                        ValidateSecurityBinding(parameters3.BootstrapSecurityBindingElement, binding, contract);
                    }
                }
            }
        }

        private static class MessageSecurityAndManualAddressingRule
        {
            public static void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }

            public static void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                TransportBindingElement element = binding.CreateBindingElements().Find<TransportBindingElement>();
                if ((element != null) && element.ManualAddressing)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MessageSecurityDoesNotWorkWithManualAddressing", new object[] { binding.Name, binding.Namespace })));
                }
            }
        }

        private static class MissingClientCertificateRule
        {
            public static void Validate(System.ServiceModel.Description.ServiceDescription description)
            {
                if (description.Behaviors.Contains(typeof(ServiceCredentials)))
                {
                    ValidateCore(description, description.Behaviors.Find<ServiceCredentials>());
                }
            }

            private static void ValidateCore(System.ServiceModel.Description.ServiceDescription description, ServiceCredentials credentials)
            {
                for (int i = 0; i < description.Endpoints.Count; i++)
                {
                    ServiceEndpoint endpoint = description.Endpoints[i];
                    BindingElementCollection elements = endpoint.Binding.CreateBindingElements();
                    SecurityBindingElement sbe = elements.Find<SecurityBindingElement>();
                    CompositeDuplexBindingElement element2 = elements.Find<CompositeDuplexBindingElement>();
                    if (((sbe != null) && (element2 != null)) && (SecurityBindingElement.IsMutualCertificateDuplexBinding(sbe) && (credentials.ClientCertificate.Certificate == null)))
                    {
                        ProtectionLevel level;
                        ProtectionLevel level2;
                        SecurityValidationBehavior.ContractProtectionRequirementsRule.GetRequiredProtectionLevels(endpoint.Contract, sbe.GetIndividualProperty<ISecurityCapabilities>().SupportedRequestProtectionLevel, sbe.GetIndividualProperty<ISecurityCapabilities>().SupportedResponseProtectionLevel, out level, out level2);
                        if (level2 == ProtectionLevel.EncryptAndSign)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoClientCertificate", new object[] { endpoint.Binding.Name, endpoint.Binding.Namespace })));
                        }
                    }
                }
            }
        }

        private static class NoStreamingWithSecurityRule
        {
            private static TransferMode GetTransferMode(Binding binding)
            {
                TransferMode buffered = TransferMode.Buffered;
                TransportBindingElement element = binding.CreateBindingElements().Find<TransportBindingElement>();
                if (element is ConnectionOrientedTransportBindingElement)
                {
                    return ((ConnectionOrientedTransportBindingElement) element).TransferMode;
                }
                if (element is HttpTransportBindingElement)
                {
                    buffered = ((HttpTransportBindingElement) element).TransferMode;
                }
                return buffered;
            }

            public static void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                if (((sbe is SymmetricSecurityBindingElement) || (sbe is AsymmetricSecurityBindingElement)) && (GetTransferMode(binding) != TransferMode.Buffered))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoStreamingWithSecurity", new object[] { binding.Name, binding.Namespace })));
                }
            }
        }

        private static class S4UImpersonationRule
        {
            private const int WindowsServerMajorNumber = 5;
            private const int WindowsServerMinorNumber = 2;

            private static bool IsS4URequiredForImpersonation(SecurityBindingElement sbe)
            {
                foreach (SecurityTokenParameters parameters in new System.ServiceModel.Security.SecurityTokenParametersEnumerable(sbe, true))
                {
                    if (parameters is SecureConversationSecurityTokenParameters)
                    {
                        SecureConversationSecurityTokenParameters parameters2 = (SecureConversationSecurityTokenParameters) parameters;
                        if (!parameters2.RequireCancellation)
                        {
                            return true;
                        }
                        if (parameters2.BootstrapSecurityBindingElement != null)
                        {
                            return IsS4URequiredForImpersonation(parameters2.BootstrapSecurityBindingElement);
                        }
                    }
                    if ((parameters is SspiSecurityTokenParameters) && !((SspiSecurityTokenParameters) parameters).RequireCancellation)
                    {
                        return true;
                    }
                    if (parameters is X509SecurityTokenParameters)
                    {
                        return true;
                    }
                }
                return false;
            }

            public static void Validate(System.ServiceModel.Description.ServiceDescription description)
            {
                ServiceAuthorizationBehavior behavior = description.Behaviors.Find<ServiceAuthorizationBehavior>();
                bool flag = (behavior != null) ? behavior.ImpersonateCallerForAllOperations : false;
                for (int i = 0; i < description.Endpoints.Count; i++)
                {
                    ServiceEndpoint endpoint = description.Endpoints[i];
                    if (!endpoint.InternalIsSystemEndpoint(description))
                    {
                        bool flag2 = flag;
                        if (!flag2)
                        {
                            flag2 = SecurityValidationBehavior.ValidatorUtils.EndpointRequiresImpersonation(endpoint);
                        }
                        if (flag2)
                        {
                            foreach (BindingElement element in endpoint.Binding.CreateBindingElements())
                            {
                                SecurityBindingElement sbe = element as SecurityBindingElement;
                                if (sbe != null)
                                {
                                    if (IsS4URequiredForImpersonation(sbe))
                                    {
                                        Version version = Environment.OSVersion.Version;
                                        if ((version.Major < 5) || ((version.Major == 5) && (version.Minor < 2)))
                                        {
                                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotPerformS4UImpersonationOnPlatform", new object[] { endpoint.Binding.Name, endpoint.Binding.Namespace, endpoint.Contract.Name, endpoint.Contract.Namespace })));
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static class SecurityBindingSupportForOneWayOnlyRule
        {
            public static void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }

            public static void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                if ((sbe is AsymmetricSecurityBindingElement) && ((AsymmetricSecurityBindingElement) sbe).IsCertificateSignatureBinding)
                {
                    for (int i = 0; i < contract.Operations.Count; i++)
                    {
                        OperationDescription description = contract.Operations[i];
                        if (!description.IsOneWay)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityBindingSupportsOneWayOnly", new object[] { binding.Name, binding.Namespace, contract.Name, contract.Namespace })));
                        }
                    }
                }
            }
        }

        private static class SecurityTokenParameterInclusionModeRule
        {
            private static void EnforceInclusionMode(Binding binding, SecurityTokenParameters stp, params SecurityTokenInclusionMode[] allowedInclusionModes)
            {
                bool flag = false;
                for (int i = 0; i < allowedInclusionModes.Length; i++)
                {
                    if (stp.InclusionMode == allowedInclusionModes[i])
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityTokenParametersHasIncompatibleInclusionMode", new object[] { binding.Name, binding.Namespace, stp.GetType(), stp.InclusionMode, allowedInclusionModes[0] })));
                }
            }

            public static void Validate(SecurityBindingElement sbe, Binding binding, ContractDescription contract, KeyedByTypeCollection<IServiceBehavior> behaviors)
            {
                if (behaviors != null)
                {
                    ServiceCredentials credentials = behaviors.Find<ServiceCredentials>();
                    if ((credentials != null) && (credentials.GetType() != typeof(ServiceCredentials)))
                    {
                        return;
                    }
                }
                SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
                AsymmetricSecurityBindingElement element2 = sbe as AsymmetricSecurityBindingElement;
                foreach (SecurityTokenParameters parameters in new System.ServiceModel.Security.SecurityTokenParametersEnumerable(sbe, true))
                {
                    if (parameters is RsaSecurityTokenParameters)
                    {
                        EnforceInclusionMode(binding, parameters, new SecurityTokenInclusionMode[] { SecurityTokenInclusionMode.Never });
                    }
                    else
                    {
                        if (parameters is SecureConversationSecurityTokenParameters)
                        {
                            Validate(((SecureConversationSecurityTokenParameters) parameters).BootstrapSecurityBindingElement, binding, contract, behaviors);
                        }
                        if (element != null)
                        {
                            if ((element.ProtectionTokenParameters == parameters) && parameters.HasAsymmetricKey)
                            {
                                EnforceInclusionMode(binding, parameters, new SecurityTokenInclusionMode[] { SecurityTokenInclusionMode.Never });
                            }
                            else
                            {
                                SecurityTokenInclusionMode[] allowedInclusionModes = new SecurityTokenInclusionMode[2];
                                allowedInclusionModes[1] = SecurityTokenInclusionMode.Once;
                                EnforceInclusionMode(binding, parameters, allowedInclusionModes);
                            }
                        }
                        else if (element2 != null)
                        {
                            if ((element2.InitiatorTokenParameters == parameters) && parameters.HasAsymmetricKey)
                            {
                                SecurityTokenInclusionMode[] modeArray4 = new SecurityTokenInclusionMode[3];
                                modeArray4[1] = SecurityTokenInclusionMode.AlwaysToInitiator;
                                modeArray4[2] = SecurityTokenInclusionMode.Once;
                                EnforceInclusionMode(binding, parameters, modeArray4);
                            }
                            else
                            {
                                SecurityTokenInclusionMode[] modeArray5 = new SecurityTokenInclusionMode[2];
                                modeArray5[1] = SecurityTokenInclusionMode.Once;
                                EnforceInclusionMode(binding, parameters, modeArray5);
                            }
                        }
                        else
                        {
                            SecurityTokenInclusionMode[] modeArray6 = new SecurityTokenInclusionMode[2];
                            modeArray6[1] = SecurityTokenInclusionMode.Once;
                            EnforceInclusionMode(binding, parameters, modeArray6);
                        }
                    }
                }
            }
        }

        private static class SecurityVersionSupportForEncryptedKeyBindingRule
        {
            public static void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }

            public static void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                SymmetricSecurityBindingElement element = sbe as SymmetricSecurityBindingElement;
                if (((sbe.MessageSecurityVersion.SecurityVersion == SecurityVersion.WSSecurity10) && (element != null)) && ((element.ProtectionTokenParameters != null) && element.ProtectionTokenParameters.HasAsymmetricKey))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityVersionDoesNotSupportEncryptedKeyBinding", new object[] { binding.Name, binding.Namespace, contract.Name, contract.Namespace, SecurityVersion.WSSecurity11 })));
                }
            }
        }

        private static class SecurityVersionSupportForThumbprintKeyIdentifierClauseRule
        {
            public static void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }

            public static void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                if (sbe.MessageSecurityVersion.SecurityVersion == SecurityVersion.WSSecurity10)
                {
                    foreach (SecurityTokenParameters parameters in new System.ServiceModel.Security.SecurityTokenParametersEnumerable(sbe))
                    {
                        X509SecurityTokenParameters parameters2 = parameters as X509SecurityTokenParameters;
                        if ((parameters2 != null) && (parameters2.X509ReferenceStyle == X509KeyIdentifierClauseType.Thumbprint))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityVersionDoesNotSupportThumbprintX509KeyIdentifierClause", new object[] { binding.Name, binding.Namespace, contract.Name, contract.Namespace, SecurityVersion.WSSecurity11 })));
                        }
                    }
                }
            }
        }

        private static class SoapOverSecureTransportRequirementsRule
        {
            public static void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
            }

            public static void ValidateSecurityBinding(SecurityBindingElement securityBindingElement, Binding binding, ContractDescription contract)
            {
                if ((securityBindingElement is TransportSecurityBindingElement) && !securityBindingElement.AllowInsecureTransport)
                {
                    IEnumerable<BindingElement> enumerable = binding.CreateBindingElements();
                    Collection<BindingElement> bindingElementsInTopDownChannelStackOrder = new Collection<BindingElement>();
                    bool flag = false;
                    foreach (BindingElement element in enumerable)
                    {
                        if (element is SecurityBindingElement)
                        {
                            flag = true;
                        }
                        else if (flag)
                        {
                            bindingElementsInTopDownChannelStackOrder.Add(element);
                        }
                    }
                    bool flag2 = false;
                    if (bindingElementsInTopDownChannelStackOrder.Count != 0)
                    {
                        ISecurityCapabilities innerProperty = new BindingContext(new CustomBinding(bindingElementsInTopDownChannelStackOrder), new BindingParameterCollection()).GetInnerProperty<ISecurityCapabilities>();
                        if (((innerProperty != null) && innerProperty.SupportsServerAuthentication) && ((innerProperty.SupportedRequestProtectionLevel == ProtectionLevel.EncryptAndSign) && (innerProperty.SupportedResponseProtectionLevel == ProtectionLevel.EncryptAndSign)))
                        {
                            flag2 = true;
                        }
                    }
                    if (!flag2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TransportDoesNotProtectMessage", new object[] { binding.Name, binding.Namespace, contract.Name, contract.Namespace })));
                    }
                }
            }
        }

        private static class UnknownHeaderProtectionRequirementsRule
        {
            private static void ValidateContract(Binding binding, ContractDescription contract, ProtectionLevel defaultRequestProtectionLevel, ProtectionLevel defaultResponseProtectionLevel)
            {
                ProtectionLevel protectionLevel;
                ProtectionLevel level2;
                if (contract == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contract"));
                }
                if (contract.HasProtectionLevel)
                {
                    protectionLevel = contract.ProtectionLevel;
                    level2 = contract.ProtectionLevel;
                }
                else
                {
                    protectionLevel = defaultRequestProtectionLevel;
                    level2 = defaultResponseProtectionLevel;
                }
                foreach (OperationDescription description in contract.Operations)
                {
                    ProtectionLevel level3;
                    ProtectionLevel level4;
                    if (description.HasProtectionLevel)
                    {
                        level3 = description.ProtectionLevel;
                        level4 = description.ProtectionLevel;
                    }
                    else
                    {
                        level3 = protectionLevel;
                        level4 = level2;
                    }
                    foreach (MessageDescription description2 in description.Messages)
                    {
                        ProtectionLevel level5;
                        if (description2.HasProtectionLevel)
                        {
                            level5 = description2.ProtectionLevel;
                        }
                        else if (description2.Direction == MessageDirection.Input)
                        {
                            level5 = level3;
                        }
                        else
                        {
                            level5 = level4;
                        }
                        foreach (MessageHeaderDescription description3 in description2.Headers)
                        {
                            ProtectionLevel level6;
                            if (description3.HasProtectionLevel)
                            {
                                level6 = description3.ProtectionLevel;
                            }
                            else
                            {
                                level6 = level5;
                            }
                            if (description3.IsUnknownHeaderCollection && (level6 != ProtectionLevel.None))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnknownHeaderCannotProtected", new object[] { contract.Name, contract.Namespace, description3.Name, description3.Namespace })));
                            }
                        }
                    }
                }
            }

            public static void ValidateNoSecurityBinding(Binding binding, ContractDescription contract)
            {
                ValidateContract(binding, contract, ProtectionLevel.None, ProtectionLevel.None);
            }

            public static void ValidateSecurityBinding(SecurityBindingElement sbe, Binding binding, ContractDescription contract)
            {
                if ((sbe is SymmetricSecurityBindingElement) || (sbe is AsymmetricSecurityBindingElement))
                {
                    ValidateContract(binding, contract, sbe.GetIndividualProperty<ISecurityCapabilities>().SupportedRequestProtectionLevel, sbe.GetIndividualProperty<ISecurityCapabilities>().SupportedResponseProtectionLevel);
                }
                else
                {
                    ValidateContract(binding, contract, ProtectionLevel.None, ProtectionLevel.None);
                }
            }
        }

        private static class UsernameImpersonationRule
        {
            public static void Validate(System.ServiceModel.Description.ServiceDescription description)
            {
                ServiceCredentials credentials = description.Behaviors.Find<ServiceCredentials>();
                if (credentials != null)
                {
                    ValidateCore(description, credentials);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ValidateCore(System.ServiceModel.Description.ServiceDescription description, ServiceCredentials credentials)
            {
                if (credentials.UserNameAuthentication.UserNamePasswordValidationMode != UserNamePasswordValidationMode.Windows)
                {
                    ServiceAuthorizationBehavior behavior = description.Behaviors.Find<ServiceAuthorizationBehavior>();
                    bool flag = (behavior != null) ? behavior.ImpersonateCallerForAllOperations : false;
                    for (int i = 0; i < description.Endpoints.Count; i++)
                    {
                        ServiceEndpoint endpoint = description.Endpoints[i];
                        if (!endpoint.InternalIsSystemEndpoint(description) && SecurityValidationBehavior.ValidatorUtils.IsStandardBinding(endpoint.Binding))
                        {
                            bool flag2 = flag;
                            if (!flag2)
                            {
                                flag2 = SecurityValidationBehavior.ValidatorUtils.EndpointRequiresImpersonation(endpoint);
                            }
                            if (flag2)
                            {
                                foreach (BindingElement element in endpoint.Binding.CreateBindingElements())
                                {
                                    SecurityBindingElement sbe = element as SecurityBindingElement;
                                    if (sbe != null)
                                    {
                                        ValidateSecurityBindingElement(sbe, endpoint);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private static void ValidateSecurityBindingElement(SecurityBindingElement sbe, ServiceEndpoint endpoint)
            {
                if (sbe == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sbe");
                }
                if (endpoint == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
                }
                foreach (SecurityTokenParameters parameters in new System.ServiceModel.Security.SecurityTokenParametersEnumerable(sbe, true))
                {
                    if (parameters is UserNameSecurityTokenParameters)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotPerformImpersonationOnUsernameToken", new object[] { endpoint.Binding.Name, endpoint.Binding.Namespace, endpoint.Contract.Name, endpoint.Contract.Namespace })));
                    }
                    if (parameters is SecureConversationSecurityTokenParameters)
                    {
                        ValidateSecurityBindingElement(((SecureConversationSecurityTokenParameters) parameters).BootstrapSecurityBindingElement, endpoint);
                    }
                }
            }
        }

        private class ValidationBinding : Binding
        {
            private Binding binding;
            private BindingElementCollection elements;

            public ValidationBinding(Binding binding) : base(binding.Name, binding.Namespace)
            {
                this.binding = binding;
            }

            public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingParameterCollection parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(params object[] parameters) where TChannel: class, IChannel
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingParameterCollection parameters) where TChannel: class, IChannel
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, params object[] parameters) where TChannel: class, IChannel
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, BindingParameterCollection parameters) where TChannel: class, IChannel
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, params object[] parameters) where TChannel: class, IChannel
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, BindingParameterCollection parameters) where TChannel: class, IChannel
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, ListenUriMode listenUriMode, params object[] parameters) where TChannel: class, IChannel
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, ListenUriMode listenUriMode, BindingParameterCollection parameters) where TChannel: class, IChannel
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override bool CanBuildChannelFactory<TChannel>(BindingParameterCollection parameters)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override bool CanBuildChannelListener<TChannel>(BindingParameterCollection parameters) where TChannel: class, IChannel
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public override BindingElementCollection CreateBindingElements()
            {
                if (this.elements == null)
                {
                    this.elements = this.binding.CreateBindingElements();
                }
                return this.elements;
            }

            public override string Scheme
            {
                get
                {
                    return this.binding.Scheme;
                }
            }
        }

        private static class ValidatorUtils
        {
            public static bool EndpointRequiresImpersonation(ServiceEndpoint endpoint)
            {
                if (endpoint == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
                }
                for (int i = 0; i < endpoint.Contract.Operations.Count; i++)
                {
                    OperationDescription description = endpoint.Contract.Operations[i];
                    OperationBehaviorAttribute attribute = description.Behaviors.Find<OperationBehaviorAttribute>();
                    if ((attribute != null) && (attribute.Impersonation == ImpersonationOption.Required))
                    {
                        return true;
                    }
                }
                return false;
            }

            public static bool IsStandardBinding(Binding binding)
            {
                return (((((binding is BasicHttpBinding) || (binding is NetTcpBinding)) || ((binding is NetMsmqBinding) || (binding is NetNamedPipeBinding))) || (((binding is NetPeerTcpBinding) || (binding is WSDualHttpBinding)) || (binding is WSFederationHttpBinding))) || (binding is WSHttpBinding));
            }
        }

        private static class WindowsIdentitySupportRule
        {
            public static void Validate(System.ServiceModel.Description.ServiceDescription description)
            {
                bool impersonateCallerForAllOperations = false;
                ServiceAuthorizationBehavior behavior = description.Behaviors.Find<ServiceAuthorizationBehavior>();
                if (behavior != null)
                {
                    impersonateCallerForAllOperations = behavior.ImpersonateCallerForAllOperations;
                }
                else
                {
                    impersonateCallerForAllOperations = false;
                }
                for (int i = 0; i < description.Endpoints.Count; i++)
                {
                    ServiceEndpoint endpoint = description.Endpoints[i];
                    if (!endpoint.InternalIsSystemEndpoint(description))
                    {
                        for (int j = 0; j < endpoint.Contract.Operations.Count; j++)
                        {
                            OperationDescription operation = endpoint.Contract.Operations[j];
                            OperationBehaviorAttribute attribute = operation.Behaviors.Find<OperationBehaviorAttribute>();
                            if ((impersonateCallerForAllOperations && !operation.IsServerInitiated()) && ((attribute == null) || (attribute.Impersonation == ImpersonationOption.NotAllowed)))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("OperationDoesNotAllowImpersonation", new object[] { operation.Name, endpoint.Contract.Name, endpoint.Contract.Namespace })));
                            }
                            if (impersonateCallerForAllOperations || ((attribute != null) && (attribute.Impersonation == ImpersonationOption.Required)))
                            {
                                ValidateWindowsIdentityCapability(endpoint.Binding, endpoint.Contract, operation);
                            }
                        }
                    }
                }
            }

            private static void ValidateWindowsIdentityCapability(Binding binding, ContractDescription contract, OperationDescription operation)
            {
                bool flag = false;
                ISecurityCapabilities property = binding.GetProperty<ISecurityCapabilities>(new BindingParameterCollection());
                if ((property != null) && property.SupportsClientWindowsIdentity)
                {
                    flag = true;
                }
                if (!flag)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BindingDoesNotSupportWindowsIdenityForImpersonation", new object[] { operation.Name, binding.Name, binding.Namespace, contract.Name, contract.Namespace })));
                }
            }
        }
    }
}

