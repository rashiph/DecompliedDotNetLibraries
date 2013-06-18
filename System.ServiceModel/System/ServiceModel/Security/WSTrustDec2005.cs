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
    using System.Xml;

    internal class WSTrustDec2005 : WSTrustFeb2005
    {
        public WSTrustDec2005(WSSecurityTokenSerializer tokenSerializer) : base(tokenSerializer)
        {
        }

        public override TrustDictionary SerializerDictionary
        {
            get
            {
                return DXD.TrustDec2005Dictionary;
            }
        }

        public class DriverDec2005 : WSTrustFeb2005.DriverFeb2005
        {
            public DriverDec2005(SecurityStandardsManager standardsManager) : base(standardsManager)
            {
            }

            public override IChannelFactory<IRequestChannel> CreateFederationProxy(EndpointAddress address, Binding binding, KeyedByTypeCollection<IEndpointBehavior> channelBehaviors)
            {
                if (channelBehaviors == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelBehaviors");
                }
                ChannelFactory<IWsTrustDec2005SecurityTokenService> innerChannelFactory = new ChannelFactory<IWsTrustDec2005SecurityTokenService>(binding, address);
                base.SetProtectionLevelForFederation(innerChannelFactory.Endpoint.Contract.Operations);
                innerChannelFactory.Endpoint.Behaviors.Remove<ClientCredentials>();
                for (int i = 0; i < channelBehaviors.Count; i++)
                {
                    innerChannelFactory.Endpoint.Behaviors.Add(channelBehaviors[i]);
                }
                innerChannelFactory.Endpoint.Behaviors.Add(new WSTrustFeb2005.DriverFeb2005.InteractiveInitializersRemovingBehavior());
                return new WSTrustFeb2005.DriverFeb2005.RequestChannelFactory<IWsTrustDec2005SecurityTokenService>(innerChannelFactory);
            }

            public override XmlElement CreateKeyTypeElement(SecurityKeyType keyType)
            {
                if (keyType == SecurityKeyType.BearerKey)
                {
                    XmlDocument document = new XmlDocument();
                    XmlElement element = document.CreateElement(this.DriverDictionary.Prefix.Value, this.DriverDictionary.KeyType.Value, this.DriverDictionary.Namespace.Value);
                    element.AppendChild(document.CreateTextNode(DXD.TrustDec2005Dictionary.BearerKeyType.Value));
                    return element;
                }
                return base.CreateKeyTypeElement(keyType);
            }

            public virtual XmlElement CreateKeyWrapAlgorithmElement(string keyWrapAlgorithm)
            {
                if (keyWrapAlgorithm == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyWrapAlgorithm");
                }
                XmlDocument document = new XmlDocument();
                XmlElement element = document.CreateElement(DXD.TrustDec2005Dictionary.Prefix.Value, DXD.TrustDec2005Dictionary.KeyWrapAlgorithm.Value, DXD.TrustDec2005Dictionary.Namespace.Value);
                element.AppendChild(document.CreateTextNode(keyWrapAlgorithm));
                return element;
            }

            public override XmlElement CreateRequiredClaimsElement(IEnumerable<XmlElement> claimsList)
            {
                XmlElement element = base.CreateRequiredClaimsElement(claimsList);
                System.Xml.XmlAttribute node = element.OwnerDocument.CreateAttribute(DXD.TrustDec2005Dictionary.Dialect.Value);
                node.Value = DXD.TrustDec2005Dictionary.DialectType.Value;
                element.Attributes.Append(node);
                return element;
            }

            internal override bool IsKeyWrapAlgorithmElement(XmlElement element, out string keyWrapAlgorithm)
            {
                return WSTrust.CheckElement(element, DXD.TrustDec2005Dictionary.KeyWrapAlgorithm.Value, DXD.TrustDec2005Dictionary.Namespace.Value, out keyWrapAlgorithm);
            }

            internal virtual bool IsSecondaryParametersElement(XmlElement element)
            {
                return ((element.LocalName == DXD.TrustDec2005Dictionary.SecondaryParameters.Value) && (element.NamespaceURI == DXD.TrustDec2005Dictionary.Namespace.Value));
            }

            public override Collection<XmlElement> ProcessUnknownRequestParameters(Collection<XmlElement> unknownRequestParameters, Collection<XmlElement> originalRequestParameters)
            {
                if (originalRequestParameters == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("originalRequestParameters");
                }
                if (((originalRequestParameters.Count <= 0) || (originalRequestParameters[0] == null)) || (originalRequestParameters[0].OwnerDocument == null))
                {
                    return originalRequestParameters;
                }
                XmlElement element = originalRequestParameters[0].OwnerDocument.CreateElement(DXD.TrustDec2005Dictionary.Prefix.Value, DXD.TrustDec2005Dictionary.SecondaryParameters.Value, DXD.TrustDec2005Dictionary.Namespace.Value);
                for (int i = 0; i < originalRequestParameters.Count; i++)
                {
                    element.AppendChild(originalRequestParameters[i]);
                }
                return new Collection<XmlElement> { element };
            }

            public override bool TryParseKeyTypeElement(XmlElement element, out SecurityKeyType keyType)
            {
                if (element == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
                }
                if (((element.LocalName == this.DriverDictionary.KeyType.Value) && (element.NamespaceURI == this.DriverDictionary.Namespace.Value)) && (element.InnerText == DXD.TrustDec2005Dictionary.BearerKeyType.Value))
                {
                    keyType = SecurityKeyType.BearerKey;
                    return true;
                }
                return base.TryParseKeyTypeElement(element, out keyType);
            }

            public override TrustDictionary DriverDictionary
            {
                get
                {
                    return DXD.TrustDec2005Dictionary;
                }
            }

            public override XmlDictionaryString RequestSecurityTokenResponseFinalAction
            {
                get
                {
                    return DXD.TrustDec2005Dictionary.RequestSecurityTokenCollectionIssuanceFinalResponse;
                }
            }

            [ServiceContract]
            internal interface IWsTrustDec2005SecurityTokenService
            {
                [OperationContract(IsOneWay=false, Action="http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Issue", ReplyAction="http://docs.oasis-open.org/ws-sx/ws-trust/200512/RSTRC/IssueFinal"), FaultContract(typeof(string), Action="*", ProtectionLevel=ProtectionLevel.Sign)]
                Message RequestToken(Message message);
            }
        }
    }
}

