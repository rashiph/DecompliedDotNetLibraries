namespace System.ServiceModel.Security
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    internal static class InfoCardHelper
    {
        private const string IsManagedElementName = "IsManaged";
        private static Uri selfIssuerUri;
        private const string WSIdentityNamespace = "http://schemas.microsoft.com/ws/2005/05/identity";

        private static SecurityTokenProvider CreateTokenProviderForNextLeg(SecurityTokenRequirement tokenRequirement, EndpointAddress target, EndpointAddress issuerAddress, Uri relyingPartyIssuer, ClientCredentialsSecurityTokenManager clientCredentialsTokenManager, InfoCardChannelParameter infocardChannelParameter)
        {
            if (((null == relyingPartyIssuer) && (null == issuerAddress)) || (issuerAddress.Uri == relyingPartyIssuer))
            {
                return new InternalInfoCardTokenProvider(infocardChannelParameter);
            }
            IssuedSecurityTokenProvider provider = (IssuedSecurityTokenProvider) clientCredentialsTokenManager.CreateSecurityTokenProvider(tokenRequirement, true);
            provider.IssuerChannelBehaviors.Remove<SecurityCredentialsManager>();
            provider.IssuerChannelBehaviors.Add(new InternalClientCredentials(clientCredentialsTokenManager.ClientCredentials, target, relyingPartyIssuer, infocardChannelParameter));
            return provider;
        }

        private static void FindInfoCardIssuerBinding(SecurityBindingElement secBindingElement, ThrowOnMultipleAssignment<IssuedSecurityTokenParameters> issuedSecurityTokenParameters)
        {
            if (secBindingElement != null)
            {
                SecurityTokenParametersEnumerable enumerable = new SecurityTokenParametersEnumerable(secBindingElement);
                foreach (SecurityTokenParameters parameters in enumerable)
                {
                    IssuedSecurityTokenParameters parameters2 = parameters as IssuedSecurityTokenParameters;
                    if ((parameters2 != null) && (((parameters2.IssuerBinding == null) || (parameters2.IssuerAddress == null)) || ((parameters2.IssuerAddress.IsAnonymous || SelfIssuerUri.Equals(parameters2.IssuerAddress)) || IsReferralToManagedIssuer(parameters2.IssuerBinding))))
                    {
                        if (issuedSecurityTokenParameters != null)
                        {
                            issuedSecurityTokenParameters.Value = parameters2;
                        }
                    }
                    else if (parameters is SecureConversationSecurityTokenParameters)
                    {
                        IssuedSecurityTokenParameters parameters3 = TryGetNextStsIssuedTokenParameters(((SecureConversationSecurityTokenParameters) parameters).BootstrapSecurityBindingElement);
                        if ((parameters3 != null) && (issuedSecurityTokenParameters != null))
                        {
                            issuedSecurityTokenParameters.Value = parameters3;
                        }
                    }
                    else if (((parameters2 != null) && (parameters2.IssuerBinding != null)) && ((TryGetNextStsIssuedTokenParameters(parameters2.IssuerBinding.CreateBindingElements().Find<SecurityBindingElement>()) != null) && (issuedSecurityTokenParameters != null)))
                    {
                        issuedSecurityTokenParameters.Value = parameters2;
                    }
                }
            }
        }

        public static MessageSecurityVersion GetBindingSecurityVersionOrDefault(Binding binding)
        {
            if (binding != null)
            {
                SecurityBindingElement element = binding.CreateBindingElements().Find<SecurityBindingElement>();
                if (element != null)
                {
                    return element.MessageSecurityVersion;
                }
            }
            return MessageSecurityVersion.Default;
        }

        private static PolicyElement[] GetPolicyChain(EndpointAddress target, Binding outerBinding, IssuedSecurityTokenParameters parameters, Uri firstPrivacyNoticeLink, int firstPrivacyNoticeVersion, SecurityTokenManager clientCredentialsTokenManager)
        {
            EndpointAddress issuerAddress = target;
            IssuedSecurityTokenParameters parameters2 = parameters;
            List<PolicyElement> list = new List<PolicyElement>();
            Uri privacyNoticeLink = firstPrivacyNoticeLink;
            int privacyNoticeVersion = firstPrivacyNoticeVersion;
            bool isManagedIssuer = false;
            while (parameters2 != null)
            {
                MessageSecurityVersion messageSecurityVersion = null;
                if (parameters2.IssuerBinding == null)
                {
                    messageSecurityVersion = GetBindingSecurityVersionOrDefault(outerBinding);
                }
                else
                {
                    messageSecurityVersion = GetBindingSecurityVersionOrDefault(parameters2.IssuerBinding);
                }
                list.Add(new PolicyElement(issuerAddress, parameters2.IssuerAddress, parameters2.CreateRequestParameters(messageSecurityVersion, clientCredentialsTokenManager.CreateSecurityTokenSerializer(messageSecurityVersion.SecurityTokenVersion)), privacyNoticeLink, privacyNoticeVersion, isManagedIssuer, parameters2.IssuerBinding));
                isManagedIssuer = IsReferralToManagedIssuer(parameters2.IssuerBinding);
                GetPrivacyNoticeLinkFromIssuerBinding(parameters2.IssuerBinding, out privacyNoticeLink, out privacyNoticeVersion);
                issuerAddress = parameters2.IssuerAddress;
                outerBinding = parameters2.IssuerBinding;
                parameters2 = TryGetNextStsIssuedTokenParameters(parameters2.IssuerBinding);
            }
            if (isManagedIssuer)
            {
                list.Add(new PolicyElement(issuerAddress, null, null, privacyNoticeLink, privacyNoticeVersion, isManagedIssuer, null));
            }
            return list.ToArray();
        }

        private static void GetPrivacyNoticeLinkFromIssuerBinding(Binding issuerBinding, out Uri privacyNotice, out int privacyNoticeVersion)
        {
            privacyNotice = null;
            privacyNoticeVersion = 0;
            if (issuerBinding != null)
            {
                PrivacyNoticeBindingElement element = issuerBinding.CreateBindingElements().Find<PrivacyNoticeBindingElement>();
                if (element != null)
                {
                    privacyNotice = element.Url;
                    privacyNoticeVersion = element.Version;
                }
            }
        }

        public static bool IsInfocardRequired(Binding binding, ClientCredentials clientCreds, SecurityTokenManager clientCredentialsTokenManager, EndpointAddress target, out CardSpacePolicyElement[] infocardChain, out Uri relyingPartyIssuer)
        {
            infocardChain = null;
            bool flag = false;
            relyingPartyIssuer = null;
            if (!clientCreds.SupportInteractive || ((null != clientCreds.IssuedToken.LocalIssuerAddress) && (clientCreds.IssuedToken.LocalIssuerBinding != null)))
            {
                return false;
            }
            IssuedSecurityTokenParameters parameters = TryGetNextStsIssuedTokenParameters(binding);
            if (parameters != null)
            {
                Uri uri;
                int num;
                GetPrivacyNoticeLinkFromIssuerBinding(binding, out uri, out num);
                PolicyElement[] chain = GetPolicyChain(target, binding, parameters, uri, num, clientCredentialsTokenManager);
                relyingPartyIssuer = null;
                if (chain != null)
                {
                    flag = RequiresInfoCard(chain, out relyingPartyIssuer);
                }
                if (!flag)
                {
                    return flag;
                }
                infocardChain = new CardSpacePolicyElement[chain.Length];
                for (int i = 0; i < chain.Length; i++)
                {
                    infocardChain[i] = chain[i].ToCardSpacePolicyElement();
                }
            }
            return flag;
        }

        private static bool IsReferralToManagedIssuer(Binding issuerBinding)
        {
            bool flag = false;
            if ((issuerBinding != null) && (issuerBinding.CreateBindingElements().Find<UseManagedPresentationBindingElement>() != null))
            {
                flag = true;
            }
            return flag;
        }

        private static bool RequiresInfoCard(PolicyElement[] chain, out Uri relyingPartyIssuer)
        {
            relyingPartyIssuer = null;
            if (chain.Length == 0)
            {
                return false;
            }
            int index = chain.Length - 1;
            int num2 = -1;
            bool flag = false;
            if (1 == chain.Length)
            {
                if (((null == chain[index].Issuer) || chain[index].Issuer.IsAnonymous) || (SelfIssuerUri.Equals(chain[index].Issuer.Uri) || ((null != chain[index].Issuer) && (chain[index].Binding == null))))
                {
                    num2 = index;
                    flag = true;
                }
                else if (!chain[index].IsManagedIssuer)
                {
                    flag = false;
                }
            }
            else
            {
                if (chain[index].IsManagedIssuer)
                {
                    num2 = index - 1;
                    flag = true;
                }
                else if (((null == chain[index].Issuer) || chain[index].Issuer.IsAnonymous) || (SelfIssuerUri.Equals(chain[index].Issuer.Uri) || ((null != chain[index].Issuer) && (chain[index].Binding == null))))
                {
                    num2 = index;
                    flag = true;
                }
                else
                {
                    flag = false;
                }
                for (int i = 0; i < index; i++)
                {
                    if ((chain[i].IsManagedIssuer || SelfIssuerUri.Equals(chain[i].Issuer.Uri)) || ((null == chain[i].Issuer) || chain[i].Issuer.IsAnonymous))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InfoCardInvalidChain")));
                    }
                }
            }
            if (flag)
            {
                relyingPartyIssuer = (null == chain[num2].Issuer) ? null : chain[num2].Issuer.Uri;
            }
            return flag;
        }

        public static bool TryCreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement, ClientCredentialsSecurityTokenManager clientCredentialsTokenManager, out SecurityTokenProvider provider)
        {
            if (tokenRequirement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
            }
            if (clientCredentialsTokenManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("clientCredentialsTokenManager");
            }
            provider = null;
            if ((clientCredentialsTokenManager.ClientCredentials.SupportInteractive && ((null == clientCredentialsTokenManager.ClientCredentials.IssuedToken.LocalIssuerAddress) || (clientCredentialsTokenManager.ClientCredentials.IssuedToken.LocalIssuerBinding == null))) && clientCredentialsTokenManager.IsIssuedSecurityTokenRequirement(tokenRequirement))
            {
                ChannelParameterCollection parameters;
                Uri uri;
                int num;
                InfoCardChannelParameter infocardChannelParameter = null;
                if (tokenRequirement.TryGetProperty<ChannelParameterCollection>(ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty, out parameters))
                {
                    foreach (object obj2 in parameters)
                    {
                        if (obj2 is InfoCardChannelParameter)
                        {
                            infocardChannelParameter = (InfoCardChannelParameter) obj2;
                            break;
                        }
                    }
                }
                if ((infocardChannelParameter == null) || !infocardChannelParameter.RequiresInfoCard)
                {
                    return false;
                }
                EndpointAddress property = tokenRequirement.GetProperty<EndpointAddress>(ServiceModelSecurityTokenRequirement.TargetAddressProperty);
                IssuedSecurityTokenParameters parameters2 = tokenRequirement.GetProperty<IssuedSecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);
                if (!tokenRequirement.TryGetProperty<Uri>(ServiceModelSecurityTokenRequirement.PrivacyNoticeUriProperty, out uri))
                {
                    uri = null;
                }
                if (!tokenRequirement.TryGetProperty<int>(ServiceModelSecurityTokenRequirement.PrivacyNoticeVersionProperty, out num))
                {
                    num = 0;
                }
                provider = CreateTokenProviderForNextLeg(tokenRequirement, property, parameters2.IssuerAddress, infocardChannelParameter.RelyingPartyIssuer, clientCredentialsTokenManager, infocardChannelParameter);
            }
            return (provider != null);
        }

        private static IssuedSecurityTokenParameters TryGetNextStsIssuedTokenParameters(Binding currentStsBinding)
        {
            if (currentStsBinding == null)
            {
                return null;
            }
            return TryGetNextStsIssuedTokenParameters(currentStsBinding.CreateBindingElements().Find<SecurityBindingElement>());
        }

        private static IssuedSecurityTokenParameters TryGetNextStsIssuedTokenParameters(SecurityBindingElement securityBindingEle)
        {
            if (securityBindingEle == null)
            {
                return null;
            }
            ThrowOnMultipleAssignment<IssuedSecurityTokenParameters> issuedSecurityTokenParameters = new ThrowOnMultipleAssignment<IssuedSecurityTokenParameters>(System.ServiceModel.SR.GetString("TooManyIssuedSecurityTokenParameters"));
            FindInfoCardIssuerBinding(securityBindingEle, issuedSecurityTokenParameters);
            return issuedSecurityTokenParameters.Value;
        }

        private static Uri SelfIssuerUri
        {
            get
            {
                if (selfIssuerUri == null)
                {
                    selfIssuerUri = new Uri("http://schemas.microsoft.com/ws/2005/05/identity/issuer/self");
                }
                return selfIssuerUri;
            }
        }

        private class InternalClientCredentials : ClientCredentials
        {
            private ClientCredentials m_clientCredentials;
            private System.ServiceModel.Security.InfoCardChannelParameter m_infocardChannelParameter;
            private Uri m_relyingPartyIssuer;

            private InternalClientCredentials(InfoCardHelper.InternalClientCredentials other) : base(other)
            {
                this.m_relyingPartyIssuer = other.m_relyingPartyIssuer;
                this.m_clientCredentials = other.m_clientCredentials;
                this.m_infocardChannelParameter = other.InfoCardChannelParameter;
            }

            public InternalClientCredentials(ClientCredentials infocardCredentials, EndpointAddress target, Uri relyingPartyIssuer, System.ServiceModel.Security.InfoCardChannelParameter infocardChannelParameter) : base(infocardCredentials)
            {
                this.m_relyingPartyIssuer = relyingPartyIssuer;
                this.m_clientCredentials = infocardCredentials;
                this.m_infocardChannelParameter = infocardChannelParameter;
            }

            public override void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
            {
            }

            protected override ClientCredentials CloneCore()
            {
                return new InfoCardHelper.InternalClientCredentials(this);
            }

            public override SecurityTokenManager CreateSecurityTokenManager()
            {
                return new InternalClientCredentialsSecurityTokenManager(this, this.m_infocardChannelParameter);
            }

            public System.ServiceModel.Security.InfoCardChannelParameter InfoCardChannelParameter
            {
                get
                {
                    return this.m_infocardChannelParameter;
                }
            }

            private class InternalClientCredentialsSecurityTokenManager : ClientCredentialsSecurityTokenManager
            {
                private InfoCardChannelParameter m_infocardChannelParameter;
                private Uri m_relyingPartyIssuer;

                public InternalClientCredentialsSecurityTokenManager(InfoCardHelper.InternalClientCredentials internalClientCredentials, InfoCardChannelParameter infocardChannelParameter) : base(internalClientCredentials)
                {
                    this.m_relyingPartyIssuer = internalClientCredentials.m_relyingPartyIssuer;
                    this.m_infocardChannelParameter = infocardChannelParameter;
                }

                public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
                {
                    if (base.IsIssuedSecurityTokenRequirement(tokenRequirement))
                    {
                        EndpointAddress property = tokenRequirement.GetProperty<EndpointAddress>(ServiceModelSecurityTokenRequirement.TargetAddressProperty);
                        IssuedSecurityTokenParameters parameters = tokenRequirement.GetProperty<IssuedSecurityTokenParameters>(ServiceModelSecurityTokenRequirement.IssuedSecurityTokenParametersProperty);
                        return InfoCardHelper.CreateTokenProviderForNextLeg(tokenRequirement, property, parameters.IssuerAddress, this.m_relyingPartyIssuer, this, this.m_infocardChannelParameter);
                    }
                    return base.CreateSecurityTokenProvider(tokenRequirement);
                }
            }
        }

        private class InternalInfoCardTokenProvider : SecurityTokenProvider, IDisposable
        {
            private InfoCardChannelParameter m_infocardChannelParameter;

            public InternalInfoCardTokenProvider(InfoCardChannelParameter infocardChannelParameter)
            {
                this.m_infocardChannelParameter = infocardChannelParameter;
            }

            public void Dispose()
            {
            }

            protected override SecurityToken GetTokenCore(TimeSpan timeout)
            {
                if ((this.m_infocardChannelParameter == null) || (this.m_infocardChannelParameter.Token == null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoTokenInChannelParameters")));
                }
                if (this.m_infocardChannelParameter.Token.ValidTo < DateTime.UtcNow)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ExpiredSecurityTokenException(System.ServiceModel.SR.GetString("ExpiredTokenInChannelParameters")));
                }
                return this.m_infocardChannelParameter.Token;
            }
        }

        private class PolicyElement
        {
            private System.ServiceModel.Channels.Binding m_binding;
            private bool m_isManagedIssuer;
            private EndpointAddress m_issuer;
            private Collection<XmlElement> m_parameters;
            private Uri m_policyNoticeLink;
            private int m_policyNoticeVersion;
            private EndpointAddress m_target;

            public PolicyElement(EndpointAddress target, EndpointAddress issuer, Collection<XmlElement> parameters, Uri privacyNoticeLink, int privacyNoticeVersion, bool isManagedIssuer, System.ServiceModel.Channels.Binding binding)
            {
                this.m_target = target;
                this.m_issuer = issuer;
                this.m_parameters = parameters;
                this.m_policyNoticeLink = privacyNoticeLink;
                this.m_policyNoticeVersion = privacyNoticeVersion;
                this.m_isManagedIssuer = isManagedIssuer;
                this.m_binding = binding;
            }

            private XmlElement EndPointAddressToXmlElement(EndpointAddress epr)
            {
                XmlElement element;
                if (null == epr)
                {
                    return null;
                }
                using (MemoryStream stream = new MemoryStream())
                {
                    using (XmlWriter writer = new XmlTextWriter(stream, Encoding.UTF8))
                    {
                        epr.WriteTo(AddressingVersion.WSAddressing10, writer);
                        writer.Flush();
                        stream.Flush();
                        stream.Seek(0L, SeekOrigin.Begin);
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            XmlDocument document = new XmlDocument();
                            element = (XmlElement) document.ReadNode(reader);
                        }
                    }
                }
                return element;
            }

            public CardSpacePolicyElement ToCardSpacePolicyElement()
            {
                return new CardSpacePolicyElement(this.EndPointAddressToXmlElement(this.m_target), this.EndPointAddressToXmlElement(this.m_issuer), this.m_parameters, this.m_policyNoticeLink, this.m_policyNoticeVersion, this.m_isManagedIssuer);
            }

            public System.ServiceModel.Channels.Binding Binding
            {
                get
                {
                    return this.m_binding;
                }
            }

            public bool IsManagedIssuer
            {
                get
                {
                    return this.m_isManagedIssuer;
                }
            }

            public EndpointAddress Issuer
            {
                get
                {
                    return this.m_issuer;
                }
            }
        }

        private class SecurityTokenParametersEnumerable : IEnumerable<SecurityTokenParameters>, IEnumerable
        {
            private SecurityBindingElement sbe;

            public SecurityTokenParametersEnumerable(SecurityBindingElement sbe)
            {
                if (sbe == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sbe");
                }
                this.sbe = sbe;
            }

            public IEnumerator<SecurityTokenParameters> GetEnumerator()
            {
                foreach (SecurityTokenParameters iteratorVariable0 in this.sbe.EndpointSupportingTokenParameters.Endorsing)
                {
                    if (iteratorVariable0 != null)
                    {
                        yield return iteratorVariable0;
                    }
                }
                foreach (SecurityTokenParameters iteratorVariable1 in this.sbe.EndpointSupportingTokenParameters.SignedEndorsing)
                {
                    if (iteratorVariable1 == null)
                    {
                        continue;
                    }
                    yield return iteratorVariable1;
                }
                foreach (SupportingTokenParameters iteratorVariable2 in this.sbe.OperationSupportingTokenParameters.Values)
                {
                    if (iteratorVariable2 != null)
                    {
                        foreach (SecurityTokenParameters iteratorVariable3 in iteratorVariable2.Endorsing)
                        {
                            if (iteratorVariable3 == null)
                            {
                                continue;
                            }
                            yield return iteratorVariable3;
                        }
                        foreach (SecurityTokenParameters iteratorVariable4 in iteratorVariable2.SignedEndorsing)
                        {
                            if (iteratorVariable4 == null)
                            {
                                continue;
                            }
                            yield return iteratorVariable4;
                        }
                    }
                }
                if (this.sbe is SymmetricSecurityBindingElement)
                {
                    SymmetricSecurityBindingElement sbe = (SymmetricSecurityBindingElement) this.sbe;
                    if (sbe.ProtectionTokenParameters != null)
                    {
                        yield return sbe.ProtectionTokenParameters;
                    }
                }
                else if (this.sbe is AsymmetricSecurityBindingElement)
                {
                    AsymmetricSecurityBindingElement iteratorVariable6 = (AsymmetricSecurityBindingElement) this.sbe;
                    if (iteratorVariable6.RecipientTokenParameters != null)
                    {
                        yield return iteratorVariable6.RecipientTokenParameters;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }

        }

        private class ThrowOnMultipleAssignment<T>
        {
            private string m_errorString;
            private T m_value;

            public ThrowOnMultipleAssignment(string errorString)
            {
                this.m_errorString = errorString;
            }

            public T Value
            {
                get
                {
                    return this.m_value;
                }
                set
                {
                    if ((this.m_value != null) && (value != null))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(this.m_errorString);
                    }
                    if (this.m_value == null)
                    {
                        this.m_value = value;
                    }
                }
            }
        }
    }
}

