namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal class WSTrustFeb2005 : WSTrust
    {
        public WSTrustFeb2005(WSSecurityTokenSerializer tokenSerializer) : base(tokenSerializer)
        {
        }

        public override TrustDictionary SerializerDictionary
        {
            get
            {
                return XD.TrustFeb2005Dictionary;
            }
        }

        public class DriverFeb2005 : WSTrust.Driver
        {
            public DriverFeb2005(SecurityStandardsManager standardsManager) : base(standardsManager)
            {
            }

            public override IChannelFactory<IRequestChannel> CreateFederationProxy(EndpointAddress address, Binding binding, KeyedByTypeCollection<IEndpointBehavior> channelBehaviors)
            {
                if (channelBehaviors == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelBehaviors");
                }
                ChannelFactory<IWsTrustFeb2005SecurityTokenService> innerChannelFactory = new ChannelFactory<IWsTrustFeb2005SecurityTokenService>(binding, address);
                base.SetProtectionLevelForFederation(innerChannelFactory.Endpoint.Contract.Operations);
                innerChannelFactory.Endpoint.Behaviors.Remove<ClientCredentials>();
                for (int i = 0; i < channelBehaviors.Count; i++)
                {
                    innerChannelFactory.Endpoint.Behaviors.Add(channelBehaviors[i]);
                }
                innerChannelFactory.Endpoint.Behaviors.Add(new InteractiveInitializersRemovingBehavior());
                return new RequestChannelFactory<IWsTrustFeb2005SecurityTokenService>(innerChannelFactory);
            }

            public override Collection<XmlElement> ProcessUnknownRequestParameters(Collection<XmlElement> unknownRequestParameters, Collection<XmlElement> originalRequestParameters)
            {
                return unknownRequestParameters;
            }

            protected override void ReadReferences(XmlElement rstrXml, out SecurityKeyIdentifierClause requestedAttachedReference, out SecurityKeyIdentifierClause requestedUnattachedReference)
            {
                XmlElement childElement = null;
                requestedAttachedReference = null;
                requestedUnattachedReference = null;
                for (int i = 0; i < rstrXml.ChildNodes.Count; i++)
                {
                    XmlElement parent = rstrXml.ChildNodes[i] as XmlElement;
                    if (parent != null)
                    {
                        if ((parent.LocalName == this.DriverDictionary.RequestedSecurityToken.Value) && (parent.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            childElement = XmlHelper.GetChildElement(parent);
                        }
                        else if ((parent.LocalName == this.DriverDictionary.RequestedAttachedReference.Value) && (parent.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            requestedAttachedReference = this.StandardsManager.SecurityTokenSerializer.ReadKeyIdentifierClause(new XmlNodeReader(XmlHelper.GetChildElement(parent)));
                        }
                        else if ((parent.LocalName == this.DriverDictionary.RequestedUnattachedReference.Value) && (parent.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            requestedUnattachedReference = this.StandardsManager.SecurityTokenSerializer.ReadKeyIdentifierClause(new XmlNodeReader(XmlHelper.GetChildElement(parent)));
                        }
                    }
                }
                try
                {
                    if (childElement != null)
                    {
                        if (requestedAttachedReference == null)
                        {
                            this.StandardsManager.TryCreateKeyIdentifierClauseFromTokenXml(childElement, SecurityTokenReferenceStyle.Internal, out requestedAttachedReference);
                        }
                        if (requestedUnattachedReference == null)
                        {
                            this.StandardsManager.TryCreateKeyIdentifierClauseFromTokenXml(childElement, SecurityTokenReferenceStyle.External, out requestedUnattachedReference);
                        }
                    }
                }
                catch (XmlException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.ServiceModel.SR.GetString("TrustDriverIsUnableToCreatedNecessaryAttachedOrUnattachedReferences", new object[] { childElement.ToString() })));
                }
            }

            protected override bool ReadRequestedTokenClosed(XmlElement rstrXml)
            {
                for (int i = 0; i < rstrXml.ChildNodes.Count; i++)
                {
                    XmlElement element = rstrXml.ChildNodes[i] as XmlElement;
                    if (((element != null) && (element.LocalName == this.DriverDictionary.RequestedTokenClosed.Value)) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value))
                    {
                        return true;
                    }
                }
                return false;
            }

            protected override void ReadTargets(XmlElement rstXml, out SecurityKeyIdentifierClause renewTarget, out SecurityKeyIdentifierClause closeTarget)
            {
                renewTarget = null;
                closeTarget = null;
                for (int i = 0; i < rstXml.ChildNodes.Count; i++)
                {
                    XmlElement element = rstXml.ChildNodes[i] as XmlElement;
                    if (element != null)
                    {
                        if ((element.LocalName == this.DriverDictionary.RenewTarget.Value) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            renewTarget = this.StandardsManager.SecurityTokenSerializer.ReadKeyIdentifierClause(new XmlNodeReader(element.FirstChild));
                        }
                        else if ((element.LocalName == this.DriverDictionary.CloseTarget.Value) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value))
                        {
                            closeTarget = this.StandardsManager.SecurityTokenSerializer.ReadKeyIdentifierClause(new XmlNodeReader(element.FirstChild));
                        }
                    }
                }
            }

            protected override void WriteReferences(RequestSecurityTokenResponse rstr, XmlDictionaryWriter writer)
            {
                if (rstr.RequestedAttachedReference != null)
                {
                    writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.RequestedAttachedReference, this.DriverDictionary.Namespace);
                    this.StandardsManager.SecurityTokenSerializer.WriteKeyIdentifierClause(writer, rstr.RequestedAttachedReference);
                    writer.WriteEndElement();
                }
                if (rstr.RequestedUnattachedReference != null)
                {
                    writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.RequestedUnattachedReference, this.DriverDictionary.Namespace);
                    this.StandardsManager.SecurityTokenSerializer.WriteKeyIdentifierClause(writer, rstr.RequestedUnattachedReference);
                    writer.WriteEndElement();
                }
            }

            protected override void WriteRequestedTokenClosed(RequestSecurityTokenResponse rstr, XmlDictionaryWriter writer)
            {
                if (rstr.IsRequestedTokenClosed)
                {
                    writer.WriteElementString(this.DriverDictionary.RequestedTokenClosed, this.DriverDictionary.Namespace, string.Empty);
                }
            }

            protected override void WriteTargets(RequestSecurityToken rst, XmlDictionaryWriter writer)
            {
                if (rst.RenewTarget != null)
                {
                    writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.RenewTarget, this.DriverDictionary.Namespace);
                    this.StandardsManager.SecurityTokenSerializer.WriteKeyIdentifierClause(writer, rst.RenewTarget);
                    writer.WriteEndElement();
                }
                if (rst.CloseTarget != null)
                {
                    writer.WriteStartElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.CloseTarget, this.DriverDictionary.Namespace);
                    this.StandardsManager.SecurityTokenSerializer.WriteKeyIdentifierClause(writer, rst.CloseTarget);
                    writer.WriteEndElement();
                }
            }

            public override TrustDictionary DriverDictionary
            {
                get
                {
                    return XD.TrustFeb2005Dictionary;
                }
            }

            public override bool IsIssuedTokensSupported
            {
                get
                {
                    return true;
                }
            }

            public override bool IsSessionSupported
            {
                get
                {
                    return true;
                }
            }

            public override string IssuedTokensHeaderName
            {
                get
                {
                    return this.DriverDictionary.IssuedTokensHeader.Value;
                }
            }

            public override string IssuedTokensHeaderNamespace
            {
                get
                {
                    return this.DriverDictionary.Namespace.Value;
                }
            }

            public override XmlDictionaryString RequestSecurityTokenResponseFinalAction
            {
                get
                {
                    return XD.TrustFeb2005Dictionary.RequestSecurityTokenIssuanceResponse;
                }
            }

            public override string RequestTypeClose
            {
                get
                {
                    return this.DriverDictionary.RequestTypeClose.Value;
                }
            }

            public override string RequestTypeRenew
            {
                get
                {
                    return this.DriverDictionary.RequestTypeRenew.Value;
                }
            }

            public class InteractiveInitializersRemovingBehavior : IEndpointBehavior
            {
                public void AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
                {
                }

                public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
                {
                    if ((behavior != null) && (behavior.InteractiveChannelInitializers != null))
                    {
                        behavior.InteractiveChannelInitializers.Clear();
                    }
                }

                public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
                {
                }

                public void Validate(ServiceEndpoint serviceEndpoint)
                {
                }
            }

            [ServiceContract]
            internal interface IWsTrustFeb2005SecurityTokenService
            {
                [FaultContract(typeof(string), Action="*", ProtectionLevel=ProtectionLevel.Sign), OperationContract(IsOneWay=false, Action="http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue", ReplyAction="http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue")]
                Message RequestToken(Message message);
            }

            public class RequestChannelFactory<TokenService> : ChannelFactoryBase, IChannelFactory<IRequestChannel>, IChannelFactory, ICommunicationObject
            {
                private ChannelFactory<TokenService> innerChannelFactory;

                public RequestChannelFactory(ChannelFactory<TokenService> innerChannelFactory)
                {
                    this.innerChannelFactory = innerChannelFactory;
                }

                public IRequestChannel CreateChannel(EndpointAddress address)
                {
                    return this.innerChannelFactory.CreateChannel<IRequestChannel>(address);
                }

                public IRequestChannel CreateChannel(EndpointAddress address, Uri via)
                {
                    return this.innerChannelFactory.CreateChannel<IRequestChannel>(address, via);
                }

                public override T GetProperty<T>() where T: class
                {
                    return this.innerChannelFactory.GetProperty<T>();
                }

                protected override void OnAbort()
                {
                    this.innerChannelFactory.Abort();
                }

                protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    return this.innerChannelFactory.BeginClose(timeout, callback, state);
                }

                protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    return this.innerChannelFactory.BeginOpen(timeout, callback, state);
                }

                protected override void OnClose(TimeSpan timeout)
                {
                    this.innerChannelFactory.Close(timeout);
                }

                protected override void OnEndClose(IAsyncResult result)
                {
                    this.innerChannelFactory.EndClose(result);
                }

                protected override void OnEndOpen(IAsyncResult result)
                {
                    this.innerChannelFactory.EndOpen(result);
                }

                protected override void OnOpen(TimeSpan timeout)
                {
                    this.innerChannelFactory.Open(timeout);
                }
            }
        }
    }
}

