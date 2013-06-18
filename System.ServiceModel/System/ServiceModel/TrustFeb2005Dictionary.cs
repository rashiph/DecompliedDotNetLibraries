namespace System.ServiceModel
{
    using System;

    internal class TrustFeb2005Dictionary : TrustDictionary
    {
        public TrustFeb2005Dictionary(ServiceModelDictionary dictionary) : base(dictionary)
        {
            base.RequestSecurityTokenResponseCollection = dictionary.CreateString("RequestSecurityTokenResponseCollection", 0x3e);
            base.Namespace = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust", 0x3f);
            base.BinarySecretClauseType = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust#BinarySecret", 0x40);
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
            base.Context = dictionary.CreateString("Context", 0xd1);
            base.BinarySecret = dictionary.CreateString("BinarySecret", 210);
            base.Type = dictionary.CreateString("Type", 0x3b);
            base.SpnegoValueTypeUri = dictionary.CreateString("http://schemas.microsoft.com/net/2004/07/secext/WS-SPNego", 0xe9);
            base.TlsnegoValueTypeUri = dictionary.CreateString("http://schemas.microsoft.com/net/2004/07/secext/TLSNego", 0xea);
            base.Prefix = dictionary.CreateString("t", 0xeb);
            base.RequestSecurityTokenIssuance = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue", 0xec);
            base.RequestSecurityTokenIssuanceResponse = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue", 0xed);
            base.RequestTypeIssue = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/Issue", 0xee);
            base.SymmetricKeyBinarySecret = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/SymmetricKey", 0xef);
            base.Psha1ComputedKeyUri = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/CK/PSHA1", 240);
            base.NonceBinarySecret = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/Nonce", 0xf1);
            base.RenewTarget = dictionary.CreateString("RenewTarget", 0xf2);
            base.CloseTarget = dictionary.CreateString("CancelTarget", 0xf3);
            base.RequestedTokenClosed = dictionary.CreateString("RequestedTokenCancelled", 0xf4);
            base.RequestedAttachedReference = dictionary.CreateString("RequestedAttachedReference", 0xf5);
            base.RequestedUnattachedReference = dictionary.CreateString("RequestedUnattachedReference", 0xf6);
            base.IssuedTokensHeader = dictionary.CreateString("IssuedTokens", 0xf7);
            base.RequestTypeRenew = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/Renew", 0xf8);
            base.RequestTypeClose = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/Cancel", 0xf9);
            base.KeyType = dictionary.CreateString("KeyType", 0xdd);
            base.SymmetricKeyType = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/SymmetricKey", 0xef);
            base.PublicKeyType = dictionary.CreateString("http://schemas.xmlsoap.org/ws/2005/02/trust/PublicKey", 250);
            base.Claims = dictionary.CreateString("Claims", 0xe0);
            base.InvalidRequestFaultCode = dictionary.CreateString("InvalidRequest", 0xe1);
            base.FailedAuthenticationFaultCode = dictionary.CreateString("FailedAuthentication", 0xb6);
            base.UseKey = dictionary.CreateString("UseKey", 0xe8);
            base.SignWith = dictionary.CreateString("SignWith", 0xe3);
            base.EncryptWith = dictionary.CreateString("EncryptWith", 0xe4);
            base.EncryptionAlgorithm = dictionary.CreateString("EncryptionAlgorithm", 0xe5);
            base.CanonicalizationAlgorithm = dictionary.CreateString("CanonicalizationAlgorithm", 230);
            base.ComputedKeyAlgorithm = dictionary.CreateString("ComputedKeyAlgorithm", 0xe7);
        }
    }
}

