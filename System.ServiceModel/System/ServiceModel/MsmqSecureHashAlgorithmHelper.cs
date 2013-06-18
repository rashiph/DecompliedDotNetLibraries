namespace System.ServiceModel
{
    using System;

    internal static class MsmqSecureHashAlgorithmHelper
    {
        public static bool IsDefined(MsmqSecureHashAlgorithm algorithm)
        {
            if (((algorithm != MsmqSecureHashAlgorithm.MD5) && (algorithm != MsmqSecureHashAlgorithm.Sha1)) && (algorithm != MsmqSecureHashAlgorithm.Sha256))
            {
                return (algorithm == MsmqSecureHashAlgorithm.Sha512);
            }
            return true;
        }

        public static int ToInt32(MsmqSecureHashAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case MsmqSecureHashAlgorithm.MD5:
                    return 0x8003;

                case MsmqSecureHashAlgorithm.Sha1:
                    return 0x8004;

                case MsmqSecureHashAlgorithm.Sha256:
                    return 0x800c;

                case MsmqSecureHashAlgorithm.Sha512:
                    return 0x800e;
            }
            return -1;
        }
    }
}

