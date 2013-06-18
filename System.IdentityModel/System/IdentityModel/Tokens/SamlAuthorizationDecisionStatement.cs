namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.Xml;

    public class SamlAuthorizationDecisionStatement : SamlSubjectStatement
    {
        private SamlAccessDecision accessDecision;
        private readonly ImmutableCollection<SamlAction> actions;
        private SamlEvidence evidence;
        private bool isReadOnly;
        private string resource;

        public SamlAuthorizationDecisionStatement()
        {
            this.actions = new ImmutableCollection<SamlAction>();
        }

        public SamlAuthorizationDecisionStatement(SamlSubject samlSubject, string resource, SamlAccessDecision accessDecision, IEnumerable<SamlAction> samlActions) : this(samlSubject, resource, accessDecision, samlActions, null)
        {
        }

        public SamlAuthorizationDecisionStatement(SamlSubject samlSubject, string resource, SamlAccessDecision accessDecision, IEnumerable<SamlAction> samlActions, SamlEvidence samlEvidence) : base(samlSubject)
        {
            this.actions = new ImmutableCollection<SamlAction>();
            if (samlActions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlActions"));
            }
            foreach (SamlAction action in samlActions)
            {
                if (action == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLEntityCannotBeNullOrEmpty", new object[] { XD.SamlDictionary.Action.Value }));
                }
                this.actions.Add(action);
            }
            this.evidence = samlEvidence;
            this.accessDecision = accessDecision;
            this.resource = resource;
            this.CheckObjectValidity();
        }

        protected override void AddClaimsToList(IList<Claim> claims)
        {
            if (claims == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("claims"));
            }
            for (int i = 0; i < this.actions.Count; i++)
            {
                claims.Add(new Claim(ClaimTypes.AuthorizationDecision, new SamlAuthorizationDecisionClaimResource(this.resource, this.accessDecision, this.actions[i].Namespace, this.actions[i].Action), Rights.PossessProperty));
            }
        }

        private void CheckObjectValidity()
        {
            if (base.SamlSubject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLSubjectStatementRequiresSubject")));
            }
            if (string.IsNullOrEmpty(this.resource))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorizationDecisionResourceRequired")));
            }
            if (this.actions.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorizationDecisionShouldHaveOneAction")));
            }
        }

        public override void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                if (this.evidence != null)
                {
                    this.evidence.MakeReadOnly();
                }
                foreach (SamlAction action in this.actions)
                {
                    action.MakeReadOnly();
                }
                this.actions.MakeReadOnly();
                this.isReadOnly = true;
            }
        }

        public override void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            this.resource = reader.GetAttribute(samlDictionary.Resource, null);
            if (string.IsNullOrEmpty(this.resource))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorizationDecisionStatementMissingResourceAttributeOnRead")));
            }
            string attribute = reader.GetAttribute(samlDictionary.Decision, null);
            if (string.IsNullOrEmpty(attribute))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorizationDecisionStatementMissingDecisionAttributeOnRead")));
            }
            if (attribute.Equals(SamlAccessDecision.Deny.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                this.accessDecision = SamlAccessDecision.Deny;
            }
            else if (attribute.Equals(SamlAccessDecision.Permit.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                this.accessDecision = SamlAccessDecision.Permit;
            }
            else
            {
                this.accessDecision = SamlAccessDecision.Indeterminate;
            }
            reader.MoveToContent();
            reader.Read();
            if (!reader.IsStartElement(samlDictionary.Subject, samlDictionary.Namespace))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorizationDecisionStatementMissingSubjectOnRead")));
            }
            SamlSubject subject = new SamlSubject();
            subject.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
            base.SamlSubject = subject;
            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(samlDictionary.Action, samlDictionary.Namespace))
                {
                    SamlAction item = new SamlAction();
                    item.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                    this.actions.Add(item);
                }
                else
                {
                    if (!reader.IsStartElement(samlDictionary.Evidence, samlDictionary.Namespace))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLBadSchema", new object[] { samlDictionary.AuthorizationDecisionStatement })));
                    }
                    if (this.evidence != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorizationDecisionHasMoreThanOneEvidence")));
                    }
                    this.evidence = new SamlEvidence();
                    this.evidence.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                }
            }
            if (this.actions.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.IdentityModel.SR.GetString("SAMLAuthorizationDecisionShouldHaveOneActionOnRead")));
            }
            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            this.CheckObjectValidity();
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            }
            if (samlSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));
            }
            SamlDictionary samlDictionary = samlSerializer.DictionaryManager.SamlDictionary;
            writer.WriteStartElement(samlDictionary.PreferredPrefix.Value, samlDictionary.AuthorizationDecisionStatement, samlDictionary.Namespace);
            writer.WriteStartAttribute(samlDictionary.Decision, null);
            writer.WriteString(this.accessDecision.ToString());
            writer.WriteEndAttribute();
            writer.WriteStartAttribute(samlDictionary.Resource, null);
            writer.WriteString(this.resource);
            writer.WriteEndAttribute();
            base.SamlSubject.WriteXml(writer, samlSerializer, keyInfoSerializer);
            foreach (SamlAction action in this.actions)
            {
                action.WriteXml(writer, samlSerializer, keyInfoSerializer);
            }
            if (this.evidence != null)
            {
                this.evidence.WriteXml(writer, samlSerializer, keyInfoSerializer);
            }
            writer.WriteEndElement();
        }

        public SamlAccessDecision AccessDecision
        {
            get
            {
                return this.accessDecision;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.accessDecision = value;
            }
        }

        public static string ClaimType
        {
            get
            {
                return ClaimTypes.AuthorizationDecision;
            }
        }

        public SamlEvidence Evidence
        {
            get
            {
                return this.evidence;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                this.evidence = value;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public string Resource
        {
            get
            {
                return this.resource;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
                }
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("SAMLAuthorizationDecisionResourceRequired"));
                }
                this.resource = value;
            }
        }

        public IList<SamlAction> SamlActions
        {
            get
            {
                return this.actions;
            }
        }
    }
}

