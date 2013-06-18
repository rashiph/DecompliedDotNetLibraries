namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;

    internal sealed class DerivedKeySecurityTokenStub : SecurityToken
    {
        private string derivationAlgorithm;
        private int generation;
        private string id;
        private string label;
        private int length;
        private byte[] nonce;
        private int offset;
        private SecurityKeyIdentifierClause tokenToDeriveIdentifier;

        public DerivedKeySecurityTokenStub(int generation, int offset, int length, string label, byte[] nonce, SecurityKeyIdentifierClause tokenToDeriveIdentifier, string derivationAlgorithm, string id)
        {
            this.id = id;
            this.generation = generation;
            this.offset = offset;
            this.length = length;
            this.label = label;
            this.nonce = nonce;
            this.tokenToDeriveIdentifier = tokenToDeriveIdentifier;
            this.derivationAlgorithm = derivationAlgorithm;
        }

        public DerivedKeySecurityToken CreateToken(SecurityToken tokenToDerive, int maxKeyLength)
        {
            DerivedKeySecurityToken token = new DerivedKeySecurityToken(this.generation, this.offset, this.length, this.label, this.nonce, tokenToDerive, this.tokenToDeriveIdentifier, this.derivationAlgorithm, this.Id);
            token.InitializeDerivedKey(maxKeyLength);
            return token;
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                return null;
            }
        }

        public SecurityKeyIdentifierClause TokenToDeriveIdentifier
        {
            get
            {
                return this.tokenToDeriveIdentifier;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
        }
    }
}

