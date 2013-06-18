namespace System.ServiceModel
{
    using System;

    internal static class MsmqEncryptionAlgorithmHelper
    {
        public static bool IsDefined(MsmqEncryptionAlgorithm algorithm)
        {
            if (algorithm != MsmqEncryptionAlgorithm.RC4Stream)
            {
                return (algorithm == MsmqEncryptionAlgorithm.Aes);
            }
            return true;
        }

        public static int ToInt32(MsmqEncryptionAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case MsmqEncryptionAlgorithm.RC4Stream:
                    return 0x6801;

                case MsmqEncryptionAlgorithm.Aes:
                    return 0x6611;
            }
            return -1;
        }
    }
}

