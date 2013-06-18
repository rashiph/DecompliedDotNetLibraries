namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.ServiceModel;

    internal static class CryptoHelper
    {
        private static byte[] emptyBuffer;
        private static readonly RandomNumberGenerator random = new RNGCryptoServiceProvider();

        internal static HashAlgorithm CreateHashAlgorithm(string digestMethod)
        {
            object algorithmFromConfig = System.IdentityModel.CryptoHelper.GetAlgorithmFromConfig(digestMethod);
            if (algorithmFromConfig != null)
            {
                HashAlgorithm algorithm = algorithmFromConfig as HashAlgorithm;
                if (algorithm == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("CustomCryptoAlgorithmIsNotValidHashAlgorithm", new object[] { digestMethod })));
                }
                return algorithm;
            }
            switch (digestMethod)
            {
                case "http://www.w3.org/2000/09/xmldsig#sha1":
                    if (System.ServiceModel.Security.SecurityUtils.RequiresFipsCompliance)
                    {
                        return new SHA1CryptoServiceProvider();
                    }
                    return new SHA1Managed();

                case "http://www.w3.org/2001/04/xmlenc#sha256":
                    if (System.ServiceModel.Security.SecurityUtils.RequiresFipsCompliance)
                    {
                        return new SHA256CryptoServiceProvider();
                    }
                    return new SHA256Managed();
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { digestMethod })));
        }

        internal static HashAlgorithm CreateHashForAsymmetricSignature(string signatureMethod)
        {
            object algorithmFromConfig = System.IdentityModel.CryptoHelper.GetAlgorithmFromConfig(signatureMethod);
            if (algorithmFromConfig != null)
            {
                HashAlgorithm algorithm;
                SignatureDescription description = algorithmFromConfig as SignatureDescription;
                if (description != null)
                {
                    algorithm = description.CreateDigest();
                    if (algorithm != null)
                    {
                        return algorithm;
                    }
                }
                algorithm = algorithmFromConfig as HashAlgorithm;
                if (algorithm == null)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("CustomCryptoAlgorithmIsNotValidAsymmetricSignature", new object[] { signatureMethod })));
                }
                return algorithm;
            }
            switch (signatureMethod)
            {
                case "http://www.w3.org/2000/09/xmldsig#rsa-sha1":
                case "http://www.w3.org/2000/09/xmldsig#dsa-sha1":
                    if (System.ServiceModel.Security.SecurityUtils.RequiresFipsCompliance)
                    {
                        return new SHA1CryptoServiceProvider();
                    }
                    return new SHA1Managed();

                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                    if (System.ServiceModel.Security.SecurityUtils.RequiresFipsCompliance)
                    {
                        return new SHA256CryptoServiceProvider();
                    }
                    return new SHA256Managed();
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { signatureMethod })));
        }

        internal static byte[] ExtractIVAndDecrypt(SymmetricAlgorithm algorithm, byte[] cipherText, int offset, int count)
        {
            byte[] buffer2;
            if (cipherText == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("cipherText");
            }
            if ((count < 0) || (count > cipherText.Length))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, cipherText.Length })));
            }
            if ((offset < 0) || (offset > (cipherText.Length - count)))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, cipherText.Length - count })));
            }
            int num = algorithm.BlockSize / 8;
            byte[] dst = new byte[num];
            Buffer.BlockCopy(cipherText, offset, dst, 0, dst.Length);
            algorithm.Padding = PaddingMode.ISO10126;
            algorithm.Mode = CipherMode.CBC;
            try
            {
                using (ICryptoTransform transform = algorithm.CreateDecryptor(algorithm.Key, dst))
                {
                    buffer2 = transform.TransformFinalBlock(cipherText, offset + dst.Length, count - dst.Length);
                }
            }
            catch (CryptographicException exception)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("DecryptionFailed"), exception));
            }
            return buffer2;
        }

        internal static void FillRandomBytes(byte[] buffer)
        {
            random.GetBytes(buffer);
        }

        internal static byte[] GenerateIVAndEncrypt(SymmetricAlgorithm algorithm, byte[] plainText, int offset, int count)
        {
            byte[] buffer;
            byte[] buffer2;
            GenerateIVAndEncrypt(algorithm, new ArraySegment<byte>(plainText, offset, count), out buffer, out buffer2);
            byte[] dst = System.ServiceModel.DiagnosticUtility.Utility.AllocateByteArray(buffer.Length + buffer2.Length);
            Buffer.BlockCopy(buffer, 0, dst, 0, buffer.Length);
            Buffer.BlockCopy(buffer2, 0, dst, buffer.Length, buffer2.Length);
            return dst;
        }

        internal static void GenerateIVAndEncrypt(SymmetricAlgorithm algorithm, ArraySegment<byte> plainText, out byte[] iv, out byte[] cipherText)
        {
            int num = algorithm.BlockSize / 8;
            iv = new byte[num];
            FillRandomBytes(iv);
            algorithm.Padding = PaddingMode.PKCS7;
            algorithm.Mode = CipherMode.CBC;
            using (ICryptoTransform transform = algorithm.CreateEncryptor(algorithm.Key, iv))
            {
                cipherText = transform.TransformFinalBlock(plainText.Array, plainText.Offset, plainText.Count);
            }
        }

        private static CryptoAlgorithmType GetAlgorithmType(string algorithm)
        {
            object algorithmFromConfig = null;
            try
            {
                algorithmFromConfig = System.IdentityModel.CryptoHelper.GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
                algorithmFromConfig = null;
            }
            if (algorithmFromConfig != null)
            {
                SymmetricAlgorithm algorithm2 = algorithmFromConfig as SymmetricAlgorithm;
                KeyedHashAlgorithm algorithm3 = algorithmFromConfig as KeyedHashAlgorithm;
                if ((algorithm2 != null) || (algorithm3 != null))
                {
                    return CryptoAlgorithmType.Symmetric;
                }
                AsymmetricAlgorithm algorithm4 = algorithmFromConfig as AsymmetricAlgorithm;
                SignatureDescription description = algorithmFromConfig as SignatureDescription;
                if ((algorithm4 == null) && (description == null))
                {
                    return CryptoAlgorithmType.Unknown;
                }
                return CryptoAlgorithmType.Asymmetric;
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2000/09/xmldsig#dsa-sha1":
                case "http://www.w3.org/2000/09/xmldsig#rsa-sha1":
                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                case "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p":
                case "http://www.w3.org/2001/04/xmlenc#rsa-1_5":
                    return CryptoAlgorithmType.Asymmetric;

                case "http://www.w3.org/2000/09/xmldsig#hmac-sha1":
                case "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256":
                case "http://www.w3.org/2001/04/xmlenc#aes128-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes192-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes256-cbc":
                case "http://www.w3.org/2001/04/xmlenc#tripledes-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes128":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes192":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes256":
                case "http://www.w3.org/2001/04/xmlenc#kw-tripledes":
                case "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1":
                case "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1":
                    return CryptoAlgorithmType.Symmetric;
            }
            return CryptoAlgorithmType.Unknown;
        }

        internal static bool IsEqual(byte[] a, byte[] b)
        {
            if (((a == null) || (b == null)) || (a.Length != b.Length))
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsSymmetricAlgorithm(string algorithm)
        {
            return (GetAlgorithmType(algorithm) == CryptoAlgorithmType.Symmetric);
        }

        internal static bool IsSymmetricSupportedAlgorithm(string algorithm, int keySize)
        {
            bool flag = false;
            object algorithmFromConfig = null;
            try
            {
                algorithmFromConfig = System.IdentityModel.CryptoHelper.GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
                algorithmFromConfig = null;
            }
            if (algorithmFromConfig != null)
            {
                SymmetricAlgorithm algorithm2 = algorithmFromConfig as SymmetricAlgorithm;
                KeyedHashAlgorithm algorithm3 = algorithmFromConfig as KeyedHashAlgorithm;
                if ((algorithm2 != null) || (algorithm3 != null))
                {
                    flag = true;
                }
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2000/09/xmldsig#dsa-sha1":
                case "http://www.w3.org/2000/09/xmldsig#rsa-sha1":
                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                case "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p":
                case "http://www.w3.org/2001/04/xmlenc#rsa-1_5":
                    return false;

                case "http://www.w3.org/2000/09/xmldsig#hmac-sha1":
                case "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256":
                case "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1":
                case "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1":
                    return true;

                case "http://www.w3.org/2001/04/xmlenc#aes128-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes128":
                    return (keySize == 0x80);

                case "http://www.w3.org/2001/04/xmlenc#aes192-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes192":
                    return (keySize == 0xc0);

                case "http://www.w3.org/2001/04/xmlenc#aes256-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes256":
                    return (keySize == 0x100);

                case "http://www.w3.org/2001/04/xmlenc#tripledes-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-tripledes":
                    return ((keySize == 0x80) || (keySize == 0xc0));
            }
            return flag;
        }

        internal static HashAlgorithm NewSha1HashAlgorithm()
        {
            return CreateHashAlgorithm("http://www.w3.org/2000/09/xmldsig#sha1");
        }

        internal static HashAlgorithm NewSha256HashAlgorithm()
        {
            return CreateHashAlgorithm("http://www.w3.org/2001/04/xmlenc#sha256");
        }

        internal static void ValidateBufferBounds(Array buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            }
            if ((count < 0) || (count > buffer.Length))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, buffer.Length })));
            }
            if ((offset < 0) || (offset > (buffer.Length - count)))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, buffer.Length - count })));
            }
        }

        internal static void ValidateSymmetricKeyLength(int keyLength, SecurityAlgorithmSuite algorithmSuite)
        {
            if (!algorithmSuite.IsSymmetricKeyLengthSupported(keyLength))
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ArgumentOutOfRangeException("algorithmSuite", System.ServiceModel.SR.GetString("UnsupportedKeyLength", new object[] { keyLength, algorithmSuite.ToString() })));
            }
            if ((keyLength % 8) != 0)
            {
                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new ArgumentOutOfRangeException("algorithmSuite", System.ServiceModel.SR.GetString("KeyLengthMustBeMultipleOfEight", new object[] { keyLength })));
            }
        }

        internal static byte[] EmptyBuffer
        {
            get
            {
                if (emptyBuffer == null)
                {
                    emptyBuffer = new byte[0];
                }
                return emptyBuffer;
            }
        }

        private enum CryptoAlgorithmType
        {
            Unknown,
            Symmetric,
            Asymmetric
        }
    }
}

