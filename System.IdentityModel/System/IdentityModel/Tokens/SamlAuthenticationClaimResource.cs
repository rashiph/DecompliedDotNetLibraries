namespace System.IdentityModel.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class SamlAuthenticationClaimResource
    {
        [DataMember]
        private DateTime authenticationInstant;
        [DataMember]
        private string authenticationMethod;
        private ReadOnlyCollection<SamlAuthorityBinding> authorityBindings;
        [DataMember]
        private string dnsAddress;
        [DataMember]
        private string ipAddress;

        public SamlAuthenticationClaimResource(DateTime authenticationInstant, string authenticationMethod, string dnsAddress, string ipAddress)
        {
            if (string.IsNullOrEmpty(authenticationMethod))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authenticationMethod");
            }
            this.authenticationInstant = authenticationInstant;
            this.authenticationMethod = authenticationMethod;
            this.dnsAddress = dnsAddress;
            this.ipAddress = ipAddress;
            this.authorityBindings = new List<SamlAuthorityBinding>().AsReadOnly();
        }

        public SamlAuthenticationClaimResource(DateTime authenticationInstant, string authenticationMethod, string dnsAddress, string ipAddress, IEnumerable<SamlAuthorityBinding> authorityBindings) : this(authenticationInstant, authenticationMethod, dnsAddress, ipAddress)
        {
            if (authorityBindings == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("authorityBindings"));
            }
            List<SamlAuthorityBinding> list = new List<SamlAuthorityBinding>();
            foreach (SamlAuthorityBinding binding in authorityBindings)
            {
                if (binding != null)
                {
                    list.Add(binding);
                }
            }
            this.authorityBindings = list.AsReadOnly();
        }

        public SamlAuthenticationClaimResource(DateTime authenticationInstant, string authenticationMethod, string dnsAddress, string ipAddress, ReadOnlyCollection<SamlAuthorityBinding> authorityBindings) : this(authenticationInstant, authenticationMethod, dnsAddress, ipAddress)
        {
            if (authorityBindings == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("authorityBindings"));
            }
            this.authorityBindings = authorityBindings;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!object.ReferenceEquals(this, obj))
            {
                SamlAuthenticationClaimResource resource = obj as SamlAuthenticationClaimResource;
                if (resource == null)
                {
                    return false;
                }
                if (((this.AuthenticationInstant != resource.AuthenticationInstant) || (this.AuthenticationMethod != resource.AuthenticationMethod)) || (((this.AuthorityBindings.Count != resource.AuthorityBindings.Count) || (this.IPAddress != resource.IPAddress)) || (this.DnsAddress != resource.DnsAddress)))
                {
                    return false;
                }
                int num = 0;
                for (num = 0; num < this.AuthorityBindings.Count; num++)
                {
                    bool flag = false;
                    for (int i = 0; i < resource.AuthorityBindings.Count; i++)
                    {
                        if (((this.AuthorityBindings[num].AuthorityKind == resource.AuthorityBindings[i].AuthorityKind) && (this.AuthorityBindings[num].Binding == resource.AuthorityBindings[i].Binding)) && (this.AuthorityBindings[num].Location == resource.AuthorityBindings[i].Location))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (this.authenticationInstant.GetHashCode() ^ this.authenticationMethod.GetHashCode());
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (string.IsNullOrEmpty(this.authenticationMethod))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authenticationMethod");
            }
            if (this.authorityBindings == null)
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorityBindings");
            }
        }

        public DateTime AuthenticationInstant
        {
            get
            {
                return this.authenticationInstant;
            }
        }

        public string AuthenticationMethod
        {
            get
            {
                return this.authenticationMethod;
            }
        }

        public ReadOnlyCollection<SamlAuthorityBinding> AuthorityBindings
        {
            get
            {
                return this.authorityBindings;
            }
        }

        public string DnsAddress
        {
            get
            {
                return this.dnsAddress;
            }
        }

        public string IPAddress
        {
            get
            {
                return this.ipAddress;
            }
        }

        [DataMember]
        private List<SamlAuthorityBinding> SamlAuthorityBindings
        {
            get
            {
                List<SamlAuthorityBinding> list = new List<SamlAuthorityBinding>();
                for (int i = 0; i < this.authorityBindings.Count; i++)
                {
                    list.Add(this.authorityBindings[i]);
                }
                return list;
            }
            set
            {
                if (value != null)
                {
                    this.authorityBindings = value.AsReadOnly();
                }
            }
        }
    }
}

