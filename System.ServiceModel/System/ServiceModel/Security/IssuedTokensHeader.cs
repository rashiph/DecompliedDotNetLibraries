namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal sealed class IssuedTokensHeader : MessageHeader
    {
        private string actor;
        private bool isRefParam;
        private bool mustUnderstand;
        private bool relay;
        private SecurityStandardsManager standardsManager;
        private ReadOnlyCollection<RequestSecurityTokenResponse> tokenIssuances;

        public IssuedTokensHeader(IEnumerable<RequestSecurityTokenResponse> tokenIssuances, SecurityStandardsManager standardsManager)
        {
            if (tokenIssuances == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenIssuances");
            }
            int num = 0;
            Collection<RequestSecurityTokenResponse> coll = new Collection<RequestSecurityTokenResponse>();
            foreach (RequestSecurityTokenResponse response in tokenIssuances)
            {
                if (response == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "tokenIssuances[{0}]", new object[] { num }));
                }
                coll.Add(response);
                num++;
            }
            this.Initialize(coll, standardsManager);
        }

        public IssuedTokensHeader(RequestSecurityTokenResponse tokenIssuance, SecurityStandardsManager standardsManager)
        {
            if (tokenIssuance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenIssuance");
            }
            Collection<RequestSecurityTokenResponse> coll = new Collection<RequestSecurityTokenResponse> {
                tokenIssuance
            };
            this.Initialize(coll, standardsManager);
        }

        public IssuedTokensHeader(RequestSecurityTokenResponse tokenIssuance, MessageSecurityVersion version, SecurityTokenSerializer tokenSerializer) : this(tokenIssuance, new SecurityStandardsManager(version, tokenSerializer))
        {
        }

        public IssuedTokensHeader(XmlReader xmlReader, MessageVersion version, SecurityStandardsManager standardsManager)
        {
            if (xmlReader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlReader");
            }
            if (standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
            MessageHeader.GetHeaderAttributes(reader, version, out this.actor, out this.mustUnderstand, out this.relay, out this.isRefParam);
            reader.ReadStartElement(this.Name, this.Namespace);
            Collection<RequestSecurityTokenResponse> list = new Collection<RequestSecurityTokenResponse>();
            if (this.standardsManager.TrustDriver.IsAtRequestSecurityTokenResponseCollection(reader))
            {
                foreach (RequestSecurityTokenResponse response in this.standardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(reader).RstrCollection)
                {
                    list.Add(response);
                }
            }
            else
            {
                RequestSecurityTokenResponse item = this.standardsManager.TrustDriver.CreateRequestSecurityTokenResponse(reader);
                list.Add(item);
            }
            this.tokenIssuances = new ReadOnlyCollection<RequestSecurityTokenResponse>(list);
            reader.ReadEndElement();
        }

        internal static Collection<RequestSecurityTokenResponse> ExtractIssuances(Message message, SecurityStandardsManager standardsManager, string[] actors, XmlQualifiedName expectedAppliesToQName)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (standardsManager == null)
            {
                standardsManager = SecurityStandardsManager.DefaultInstance;
            }
            if (actors == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("actors", message);
            }
            Collection<RequestSecurityTokenResponse> collection = new Collection<RequestSecurityTokenResponse>();
            for (int i = 0; i < message.Headers.Count; i++)
            {
                if (!(message.Headers[i].Name == standardsManager.TrustDriver.IssuedTokensHeaderName) || !(message.Headers[i].Namespace == standardsManager.TrustDriver.IssuedTokensHeaderNamespace))
                {
                    continue;
                }
                bool flag = false;
                for (int j = 0; j < actors.Length; j++)
                {
                    if (actors[j] == message.Headers[i].Actor)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    IssuedTokensHeader header = new IssuedTokensHeader(message.Headers.GetReaderAtHeader(i), message.Version, standardsManager);
                    for (int k = 0; k < header.TokenIssuances.Count; k++)
                    {
                        bool flag2;
                        if (expectedAppliesToQName != null)
                        {
                            string str;
                            string str2;
                            header.TokenIssuances[k].GetAppliesToQName(out str, out str2);
                            if ((str == expectedAppliesToQName.Name) && (str2 == expectedAppliesToQName.Namespace))
                            {
                                flag2 = true;
                            }
                            else
                            {
                                flag2 = false;
                            }
                        }
                        else
                        {
                            flag2 = true;
                        }
                        if (flag2)
                        {
                            collection.Add(header.TokenIssuances[k]);
                        }
                    }
                }
            }
            return collection;
        }

        internal static Collection<RequestSecurityTokenResponse> ExtractIssuances(Message message, MessageSecurityVersion version, WSSecurityTokenSerializer tokenSerializer, string[] actors, XmlQualifiedName expectedAppliesToQName)
        {
            return ExtractIssuances(message, new SecurityStandardsManager(version, tokenSerializer), actors, expectedAppliesToQName);
        }

        private void Initialize(Collection<RequestSecurityTokenResponse> coll, SecurityStandardsManager standardsManager)
        {
            if (standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            this.tokenIssuances = new ReadOnlyCollection<RequestSecurityTokenResponse>(coll);
            this.actor = base.Actor;
            this.mustUnderstand = base.MustUnderstand;
            this.relay = base.Relay;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (this.tokenIssuances.Count == 1)
            {
                this.standardsManager.TrustDriver.WriteRequestSecurityTokenResponse(this.tokenIssuances[0], writer);
            }
            else
            {
                new RequestSecurityTokenResponseCollection(this.tokenIssuances, this.standardsManager).WriteTo(writer);
            }
        }

        public override string Actor
        {
            get
            {
                return this.actor;
            }
        }

        public override bool IsReferenceParameter
        {
            get
            {
                return this.isRefParam;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return this.mustUnderstand;
            }
        }

        public override string Name
        {
            get
            {
                return this.standardsManager.TrustDriver.IssuedTokensHeaderName;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.standardsManager.TrustDriver.IssuedTokensHeaderNamespace;
            }
        }

        public override bool Relay
        {
            get
            {
                return this.relay;
            }
        }

        public ReadOnlyCollection<RequestSecurityTokenResponse> TokenIssuances
        {
            get
            {
                return this.tokenIssuances;
            }
        }
    }
}

