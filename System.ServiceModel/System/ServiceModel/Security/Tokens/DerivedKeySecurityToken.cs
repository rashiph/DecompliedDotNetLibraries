namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Xml;

    internal sealed class DerivedKeySecurityToken : SecurityToken
    {
        public const int DefaultDerivedKeyLength = 0x20;
        private static readonly byte[] DefaultLabel = new byte[] { 
            0x57, 0x53, 0x2d, 0x53, 0x65, 0x63, 0x75, 0x72, 0x65, 0x43, 0x6f, 110, 0x76, 0x65, 0x72, 0x73, 
            0x61, 0x74, 0x69, 0x6f, 110, 0x57, 0x53, 0x2d, 0x53, 0x65, 0x63, 0x75, 0x72, 0x65, 0x43, 0x6f, 
            110, 0x76, 0x65, 0x72, 0x73, 0x61, 0x74, 0x69, 0x6f, 110
         };
        public const int DefaultNonceLength = 0x10;
        private int generation;
        private string id;
        private byte[] key;
        private string keyDerivationAlgorithm;
        private string label;
        private int length;
        private byte[] nonce;
        private int offset;
        private ReadOnlyCollection<SecurityKey> securityKeys;
        private SecurityToken tokenToDerive;
        private SecurityKeyIdentifierClause tokenToDeriveIdentifier;

        public DerivedKeySecurityToken(SecurityToken tokenToDerive, SecurityKeyIdentifierClause tokenToDeriveIdentifier, int length) : this(tokenToDerive, tokenToDeriveIdentifier, length, System.ServiceModel.Security.SecurityUtils.GenerateId())
        {
        }

        internal DerivedKeySecurityToken(SecurityToken tokenToDerive, SecurityKeyIdentifierClause tokenToDeriveIdentifier, int length, string id)
        {
            this.length = -1;
            this.offset = -1;
            this.generation = -1;
            if (((length != 0x10) && (length != 0x18)) && (length != 0x20))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("Psha1KeyLengthInvalid", new object[] { length * 8 })));
            }
            byte[] data = new byte[0x10];
            new RNGCryptoServiceProvider().GetBytes(data);
            this.Initialize(id, -1, 0, length, null, data, tokenToDerive, tokenToDeriveIdentifier, "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1");
        }

        internal DerivedKeySecurityToken(int generation, int offset, int length, string label, int minNonceLength, SecurityToken tokenToDerive, SecurityKeyIdentifierClause tokenToDeriveIdentifier, string derivationAlgorithm, string id)
        {
            this.length = -1;
            this.offset = -1;
            this.generation = -1;
            byte[] data = new byte[minNonceLength];
            new RNGCryptoServiceProvider().GetBytes(data);
            this.Initialize(id, generation, offset, length, label, data, tokenToDerive, tokenToDeriveIdentifier, derivationAlgorithm);
        }

        internal DerivedKeySecurityToken(int generation, int offset, int length, string label, byte[] nonce, SecurityToken tokenToDerive, SecurityKeyIdentifierClause tokenToDeriveIdentifier, string derivationAlgorithm, string id)
        {
            this.length = -1;
            this.offset = -1;
            this.generation = -1;
            this.Initialize(id, generation, offset, length, label, nonce, tokenToDerive, tokenToDeriveIdentifier, derivationAlgorithm, false);
        }

        internal static void EnsureAcceptableOffset(int offset, int generation, int length, int maxOffset)
        {
            if (offset != -1)
            {
                if (offset > maxOffset)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("DerivedKeyTokenOffsetTooHigh", new object[] { offset, maxOffset })));
                }
            }
            else
            {
                int num = generation * length;
                if (((num < generation) && (num < length)) || (num > maxOffset))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("DerivedKeyTokenGenerationAndLengthTooHigh", new object[] { generation, length, maxOffset })));
                }
            }
        }

        public byte[] GetKeyBytes()
        {
            return System.ServiceModel.Security.SecurityUtils.CloneBuffer(this.key);
        }

        public byte[] GetNonce()
        {
            return System.ServiceModel.Security.SecurityUtils.CloneBuffer(this.nonce);
        }

        private void Initialize(string id, int generation, int offset, int length, string label, byte[] nonce, SecurityToken tokenToDerive, SecurityKeyIdentifierClause tokenToDeriveIdentifier, string derivationAlgorithm)
        {
            this.Initialize(id, generation, offset, length, label, nonce, tokenToDerive, tokenToDeriveIdentifier, derivationAlgorithm, true);
        }

        private void Initialize(string id, int generation, int offset, int length, string label, byte[] nonce, SecurityToken tokenToDerive, SecurityKeyIdentifierClause tokenToDeriveIdentifier, string derivationAlgorithm, bool initializeDerivedKey)
        {
            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
            }
            if (tokenToDerive == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenToDerive");
            }
            if (tokenToDeriveIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokentoDeriveIdentifier");
            }
            if (!System.ServiceModel.Security.SecurityUtils.IsSupportedAlgorithm(derivationAlgorithm, tokenToDerive))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("DerivedKeyCannotDeriveFromSecret")));
            }
            if (nonce == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("nonce");
            }
            if (length == -1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("length"));
            }
            if ((offset == -1) && (generation == -1))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("DerivedKeyPosAndGenNotSpecified"));
            }
            if ((offset >= 0) && (generation >= 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("DerivedKeyPosAndGenBothSpecified"));
            }
            this.id = id;
            this.label = label;
            this.nonce = nonce;
            this.length = length;
            this.offset = offset;
            this.generation = generation;
            this.tokenToDerive = tokenToDerive;
            this.tokenToDeriveIdentifier = tokenToDeriveIdentifier;
            this.keyDerivationAlgorithm = derivationAlgorithm;
            if (initializeDerivedKey)
            {
                this.InitializeDerivedKey(this.length);
            }
        }

        internal void InitializeDerivedKey(int maxKeyLength)
        {
            if (this.key == null)
            {
                if (this.length > maxKeyLength)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("DerivedKeyLengthTooLong", new object[] { this.length, maxKeyLength }));
                }
                this.key = System.ServiceModel.Security.SecurityUtils.GenerateDerivedKey(this.tokenToDerive, this.keyDerivationAlgorithm, (this.label != null) ? Encoding.UTF8.GetBytes(this.label) : DefaultLabel, this.nonce, this.length * 8, (this.offset >= 0) ? this.offset : (this.generation * this.length));
                if ((this.key == null) || (this.key.Length == 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("DerivedKeyCannotDeriveFromSecret"));
                }
                this.securityKeys = new List<SecurityKey>(1) { new InMemorySymmetricSecurityKey(this.key, false) }.AsReadOnly();
            }
        }

        internal void InitializeDerivedKey(ReadOnlyCollection<SecurityKey> securityKeys)
        {
            this.key = ((SymmetricSecurityKey) securityKeys[0]).GetSymmetricKey();
            this.securityKeys = securityKeys;
        }

        public override string ToString()
        {
            StringWriter w = new StringWriter(CultureInfo.InvariantCulture);
            w.WriteLine("DerivedKeySecurityToken:");
            w.WriteLine("   Generation: {0}", this.Generation);
            w.WriteLine("   Offset: {0}", this.Offset);
            w.WriteLine("   Length: {0}", this.Length);
            w.WriteLine("   Label: {0}", this.Label);
            w.WriteLine("   Nonce: {0}", Convert.ToBase64String(this.Nonce));
            w.WriteLine("   TokenToDeriveFrom:");
            using (XmlTextWriter writer2 = new XmlTextWriter(w))
            {
                writer2.Formatting = Formatting.Indented;
                SecurityStandardsManager.DefaultInstance.SecurityTokenSerializer.WriteKeyIdentifierClause(XmlDictionaryWriter.CreateDictionaryWriter(writer2), this.TokenToDeriveIdentifier);
            }
            return w.ToString();
        }

        internal bool TryGetSecurityKeys(out ReadOnlyCollection<SecurityKey> keys)
        {
            keys = this.securityKeys;
            return (keys != null);
        }

        public int Generation
        {
            get
            {
                return this.generation;
            }
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public string KeyDerivationAlgorithm
        {
            get
            {
                return this.keyDerivationAlgorithm;
            }
        }

        public string Label
        {
            get
            {
                return this.label;
            }
        }

        public int Length
        {
            get
            {
                return this.length;
            }
        }

        internal byte[] Nonce
        {
            get
            {
                return this.nonce;
            }
        }

        public int Offset
        {
            get
            {
                return this.offset;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                if (this.securityKeys == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("DerivedKeyNotInitialized")));
                }
                return this.securityKeys;
            }
        }

        internal SecurityToken TokenToDerive
        {
            get
            {
                return this.tokenToDerive;
            }
        }

        internal SecurityKeyIdentifierClause TokenToDeriveIdentifier
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
                return this.tokenToDerive.ValidFrom;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                return this.tokenToDerive.ValidTo;
            }
        }
    }
}

