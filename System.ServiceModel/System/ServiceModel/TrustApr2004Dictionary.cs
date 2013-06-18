namespace System.ServiceModel
{
    using System;

    internal class TrustApr2004Dictionary : TrustDictionary
    {
        public TrustApr2004Dictionary(ServiceModelDictionary dictionary) : base(dictionary)
        {
            base.CombinedHashLabel = dictionary.CreateString("AUTH-HASH", 0xc2);
            base.RequestSecurityTokenResponse = dictionary.CreateString("RequestSecurityTokenResponse", 0xc3);
            base.TokenType = dictionary.CreateString("TokenType", 0xbb);
            base.KeySize = dictionary.CreateString("KeySize", 0xc4);
            base.RequestedTokenReference = dictionary.CreateString("RequestedTokenReference", 0xc5);
            base.AppliesTo = dictionary.CreateString("AppliesTo", 0xc6);
            base.Authenticator = dictionary.CreateString("Authenticator", 0xc7);
            base.CombinedHash = dictionary.CreateString("CombinedHash", 200);
            base.BinaryExchange = dictionary.CreateString("BinaryExchange", 0xc9);
            base.Lifetime = dictionary.CreateString("Lifetime", 0xca);
            base.RequestedSecurityToken = dictionary.CreateString("RequestedSecurityToken", 0xcb);
            base.Entropy = dictionary.CreateString("Entropy", 0xcc);
            base.RequestedProofToken = dictionary.CreateString("RequestedProofToken", 0xcd);
            base.ComputedKey = dictionary.CreateString("ComputedKey", 0xce);
            base.RequestSecurityToken = dictionary.CreateString("RequestSecurityToken", 0xcf);
            base.RequestType = dictionary.CreateString("RequestType", 0xd0);
            base.RequestSecurityTokenResponseCollection = dictionary.CreateString("RequestSecurityTokenResponseCollection", 0x3e);
            base.Context = dictionary.CreateString("Context", 0xd1);
            base.BinarySecret = dictionary.CreateString("BinarySecret", 210);
            base.Type = dictionary.CreateString("Type", 0x3b);
            base.SpnegoValueTypeUri = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/spnego", 0xd3);
            base.TlsnegoValueTypeUri = dictionary.CreateString(" http://schemas.xmlsoap.org/ws/2005/02/trust/tlsnego", 0xd4);
            base.Prefix = dictionary.CreateString("wst", 0xd5);
            base.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/trust", 0xd6);
            base.RequestSecurityTokenIssuance = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/Issue", 0xd7);
            base.RequestSecurityTokenIssuanceResponse = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/Issue", 0xd8);
            base.RequestTypeIssue = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/security/trust/Issue", 0xd9);
            base.Psha1ComputedKeyUri = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/security/trust/CK/PSHA1", 0xda);
            base.SymmetricKeyBinarySecret = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/security/trust/SymmetricKey", 0xdb);
            base.NonceBinarySecret = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/security/trust/Nonce", 220);
            base.KeyType = dictionary.CreateString("KeyType", 0xdd);
            base.SymmetricKeyType = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/trust/SymmetricKey", 0xde);
            base.PublicKeyType = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2004/04/trust/PublicKey", 0xdf);
            base.Claims = dictionary.CreateString("Claims", 0xe0);
            base.InvalidRequestFaultCode = dictionary.CreateString("InvalidRequest", 0xe1);
            base.FailedAuthenticationFaultCode = dictionary.CreateString("FailedAuthentication", 0xb6);
            base.RequestFailedFaultCode = dictionary.CreateString("RequestFailed", 0xe2);
            base.SignWith = dictionary.CreateString("SignWith", 0xe3);
            base.EncryptWith = dictionary.CreateString("EncryptWith", 0xe4);
            base.EncryptionAlgorithm = dictionary.CreateString("EncryptionAlgorithm", 0xe5);
            base.CanonicalizationAlgorithm = dictionary.CreateString("CanonicalizationAlgorithm", 230);
            base.ComputedKeyAlgorithm = dictionary.CreateString("ComputedKeyAlgorithm", 0xe7);
            base.UseKey = dictionary.CreateString("UseKey", 0xe8);
        }
    }
}

