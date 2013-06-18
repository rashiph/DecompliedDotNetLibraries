namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class SamlAuthorizationDecisionClaimResource
    {
        [DataMember]
        private SamlAccessDecision accessDecision;
        [DataMember]
        private string actionName;
        [DataMember]
        private string actionNamespace;
        [DataMember]
        private string resource;

        public SamlAuthorizationDecisionClaimResource(string resource, SamlAccessDecision accessDecision, string actionNamespace, string actionName)
        {
            if (string.IsNullOrEmpty(resource))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("resource");
            }
            if (string.IsNullOrEmpty(actionName))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("actionName");
            }
            this.resource = resource;
            this.accessDecision = accessDecision;
            this.actionNamespace = actionNamespace;
            this.actionName = actionName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            SamlAuthorizationDecisionClaimResource resource = obj as SamlAuthorizationDecisionClaimResource;
            if (resource == null)
            {
                return false;
            }
            return ((((this.ActionName == resource.ActionName) && (this.ActionNamespace == resource.ActionNamespace)) && (this.Resource == resource.Resource)) && (this.AccessDecision == resource.AccessDecision));
        }

        public override int GetHashCode()
        {
            return (this.resource.GetHashCode() ^ this.accessDecision.GetHashCode());
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (string.IsNullOrEmpty(this.resource))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("resource");
            }
            if (string.IsNullOrEmpty(this.actionName))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("actionName");
            }
        }

        public SamlAccessDecision AccessDecision
        {
            get
            {
                return this.accessDecision;
            }
        }

        public string ActionName
        {
            get
            {
                return this.actionName;
            }
        }

        public string ActionNamespace
        {
            get
            {
                return this.actionNamespace;
            }
        }

        public string Resource
        {
            get
            {
                return this.resource;
            }
        }
    }
}

