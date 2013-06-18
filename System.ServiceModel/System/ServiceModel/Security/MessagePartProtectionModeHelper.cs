namespace System.ServiceModel.Security
{
    using System;

    internal static class MessagePartProtectionModeHelper
    {
        public static MessagePartProtectionMode GetProtectionMode(bool sign, bool encrypt, bool signThenEncrypt)
        {
            if (sign)
            {
                if (!encrypt)
                {
                    return MessagePartProtectionMode.Sign;
                }
                if (signThenEncrypt)
                {
                    return MessagePartProtectionMode.SignThenEncrypt;
                }
                return MessagePartProtectionMode.EncryptThenSign;
            }
            if (encrypt)
            {
                return MessagePartProtectionMode.Encrypt;
            }
            return MessagePartProtectionMode.None;
        }
    }
}

