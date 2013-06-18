namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class SamlNameIdentifierClaimResource
    {
        [DataMember]
        private string format;
        [DataMember]
        private string name;
        [DataMember]
        private string nameQualifier;

        public SamlNameIdentifierClaimResource(string name, string nameQualifier, string format)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("name");
            }
            this.name = name;
            this.nameQualifier = nameQualifier;
            this.format = format;
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
            SamlNameIdentifierClaimResource resource = obj as SamlNameIdentifierClaimResource;
            if (resource == null)
            {
                return false;
            }
            return (((this.nameQualifier == resource.nameQualifier) && (this.format == resource.format)) && (this.name == resource.name));
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (string.IsNullOrEmpty(this.name))
            {
                throw System.IdentityModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("name");
            }
        }

        public string Format
        {
            get
            {
                return this.format;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string NameQualifier
        {
            get
            {
                return this.nameQualifier;
            }
        }
    }
}

