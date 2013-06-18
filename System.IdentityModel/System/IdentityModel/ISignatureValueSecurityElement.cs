namespace System.IdentityModel
{
    using System;

    internal interface ISignatureValueSecurityElement : ISecurityElement
    {
        byte[] GetSignatureValue();
    }
}

