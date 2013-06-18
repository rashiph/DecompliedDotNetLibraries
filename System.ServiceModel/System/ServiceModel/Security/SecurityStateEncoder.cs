namespace System.ServiceModel.Security
{
    using System;

    public abstract class SecurityStateEncoder
    {
        protected SecurityStateEncoder()
        {
        }

        protected internal abstract byte[] DecodeSecurityState(byte[] data);
        protected internal abstract byte[] EncodeSecurityState(byte[] data);
    }
}

