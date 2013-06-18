namespace System.IdentityModel.Selectors
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Xml;

    public abstract class SecurityTokenSerializer
    {
        protected SecurityTokenSerializer()
        {
        }

        public bool CanReadKeyIdentifier(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return this.CanReadKeyIdentifierCore(reader);
        }

        public bool CanReadKeyIdentifierClause(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return this.CanReadKeyIdentifierClauseCore(reader);
        }

        protected abstract bool CanReadKeyIdentifierClauseCore(XmlReader reader);
        protected abstract bool CanReadKeyIdentifierCore(XmlReader reader);
        public bool CanReadToken(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return this.CanReadTokenCore(reader);
        }

        protected abstract bool CanReadTokenCore(XmlReader reader);
        public bool CanWriteKeyIdentifier(SecurityKeyIdentifier keyIdentifier)
        {
            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
            }
            return this.CanWriteKeyIdentifierCore(keyIdentifier);
        }

        public bool CanWriteKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }
            return this.CanWriteKeyIdentifierClauseCore(keyIdentifierClause);
        }

        protected abstract bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause);
        protected abstract bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier);
        public bool CanWriteToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            return this.CanWriteTokenCore(token);
        }

        protected abstract bool CanWriteTokenCore(SecurityToken token);
        public SecurityKeyIdentifier ReadKeyIdentifier(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return this.ReadKeyIdentifierCore(reader);
        }

        public SecurityKeyIdentifierClause ReadKeyIdentifierClause(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return this.ReadKeyIdentifierClauseCore(reader);
        }

        protected abstract SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader);
        protected abstract SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader);
        public SecurityToken ReadToken(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return this.ReadTokenCore(reader, tokenResolver);
        }

        protected abstract SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver);
        public void WriteKeyIdentifier(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
            }
            this.WriteKeyIdentifierCore(writer, keyIdentifier);
        }

        public void WriteKeyIdentifierClause(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }
            this.WriteKeyIdentifierClauseCore(writer, keyIdentifierClause);
        }

        protected abstract void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause);
        protected abstract void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier);
        public void WriteToken(XmlWriter writer, SecurityToken token)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            this.WriteTokenCore(writer, token);
        }

        protected abstract void WriteTokenCore(XmlWriter writer, SecurityToken token);
    }
}

