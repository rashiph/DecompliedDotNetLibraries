namespace System.IdentityModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;

    internal static class CryptoHelper
    {
        private static Dictionary<string, Func<object>> algorithmDelegateDictionary = new Dictionary<string, Func<object>>();
        private static object AlgorithmDictionaryLock = new object();
        private static byte[] emptyBuffer;
        private static System.Security.Cryptography.RandomNumberGenerator random;
        private static System.Security.Cryptography.Rijndael rijndael;
        private static System.Security.Cryptography.TripleDES tripleDES;

        internal static byte[] ComputeHash(byte[] buffer)
        {
            using (HashAlgorithm algorithm = NewSha1HashAlgorithm())
            {
                return algorithm.ComputeHash(buffer);
            }
        }

        internal static ICryptoTransform CreateDecryptor(byte[] key, byte[] iv, string algorithm)
        {
            object algorithmFromConfig = GetAlgorithmFromConfig(algorithm);
            if (algorithmFromConfig != null)
            {
                SymmetricAlgorithm algorithm2 = algorithmFromConfig as SymmetricAlgorithm;
                if (algorithm2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm", new object[] { algorithm })));
                }
                return algorithm2.CreateDecryptor(key, iv);
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2001/04/xmlenc#tripledes-cbc":
                    return TripleDES.CreateDecryptor(key, iv);

                case "http://www.w3.org/2001/04/xmlenc#aes128-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes192-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes256-cbc":
                    return Rijndael.CreateDecryptor(key, iv);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnsupportedEncryptionAlgorithm", new object[] { algorithm })));
        }

        internal static ICryptoTransform CreateEncryptor(byte[] key, byte[] iv, string algorithm)
        {
            object algorithmFromConfig = GetAlgorithmFromConfig(algorithm);
            if (algorithmFromConfig != null)
            {
                SymmetricAlgorithm algorithm2 = algorithmFromConfig as SymmetricAlgorithm;
                if (algorithm2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm", new object[] { algorithm })));
                }
                return algorithm2.CreateEncryptor(key, iv);
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2001/04/xmlenc#tripledes-cbc":
                    return TripleDES.CreateEncryptor(key, iv);

                case "http://www.w3.org/2001/04/xmlenc#aes128-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes192-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes256-cbc":
                    return Rijndael.CreateEncryptor(key, iv);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnsupportedEncryptionAlgorithm", new object[] { algorithm })));
        }

        internal static HashAlgorithm CreateHashAlgorithm(string algorithm)
        {
            object algorithmFromConfig = GetAlgorithmFromConfig(algorithm);
            if (algorithmFromConfig != null)
            {
                HashAlgorithm algorithm2 = algorithmFromConfig as HashAlgorithm;
                if (algorithm2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("CustomCryptoAlgorithmIsNotValidHashAlgorithm", new object[] { algorithm })));
                }
                return algorithm2;
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2000/09/xmldsig#sha1":
                    if (System.IdentityModel.SecurityUtils.RequiresFipsCompliance)
                    {
                        return new SHA1CryptoServiceProvider();
                    }
                    return new SHA1Managed();

                case "http://www.w3.org/2001/04/xmlenc#sha256":
                    if (System.IdentityModel.SecurityUtils.RequiresFipsCompliance)
                    {
                        return new SHA256CryptoServiceProvider();
                    }
                    return new SHA256Managed();
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { algorithm })));
        }

        internal static KeyedHashAlgorithm CreateKeyedHashAlgorithm(byte[] key, string algorithm)
        {
            object algorithmFromConfig = GetAlgorithmFromConfig(algorithm);
            if (algorithmFromConfig != null)
            {
                KeyedHashAlgorithm algorithm2 = algorithmFromConfig as KeyedHashAlgorithm;
                if (algorithm2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("CustomCryptoAlgorithmIsNotValidKeyedHashAlgorithm", new object[] { algorithm })));
                }
                algorithm2.Key = key;
                return algorithm2;
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2000/09/xmldsig#hmac-sha1":
                    return new HMACSHA1(key, !System.IdentityModel.SecurityUtils.RequiresFipsCompliance);

                case "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256":
                    if (System.IdentityModel.SecurityUtils.RequiresFipsCompliance)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("CryptoAlgorithmIsNotFipsCompliant", new object[] { algorithm })));
                    }
                    return new HMACSHA256(key);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { algorithm })));
        }

        internal static byte[] GenerateDerivedKey(byte[] key, string algorithm, byte[] label, byte[] nonce, int derivedKeySize, int position)
        {
            if ((algorithm != "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1") && (algorithm != "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1"))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnsupportedKeyDerivationAlgorithm", new object[] { algorithm })));
            }
            return new Psha1DerivedKeyGenerator(key).GenerateDerivedKey(label, nonce, derivedKeySize, position);
        }

        internal static object GetAlgorithmFromConfig(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("algorithm"));
            }
            object obj2 = null;
            object defaultAlgorithm = null;
            Func<object> func = null;
            if (!algorithmDelegateDictionary.TryGetValue(algorithm, out func))
            {
                lock (AlgorithmDictionaryLock)
                {
                    if (algorithmDelegateDictionary.ContainsKey(algorithm))
                    {
                        goto Label_0111;
                    }
                    try
                    {
                        obj2 = CryptoConfig.CreateFromName(algorithm);
                    }
                    catch (TargetInvocationException)
                    {
                        algorithmDelegateDictionary[algorithm] = null;
                    }
                    if (obj2 == null)
                    {
                        algorithmDelegateDictionary[algorithm] = null;
                        goto Label_0111;
                    }
                    defaultAlgorithm = GetDefaultAlgorithm(algorithm);
                    if ((!System.IdentityModel.SecurityUtils.RequiresFipsCompliance && (obj2 is SHA1CryptoServiceProvider)) || ((defaultAlgorithm != null) && (defaultAlgorithm.GetType() == obj2.GetType())))
                    {
                        algorithmDelegateDictionary[algorithm] = null;
                        goto Label_0111;
                    }
                    LambdaExpression expression2 = Expression.Lambda<Func<object>>(Expression.New(obj2.GetType()), new ParameterExpression[0]);
                    func = expression2.Compile() as Func<object>;
                    if (func != null)
                    {
                        algorithmDelegateDictionary[algorithm] = func;
                    }
                    return obj2;
                }
            }
            if (func != null)
            {
                return func();
            }
        Label_0111:
            return null;
        }

        private static object GetDefaultAlgorithm(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("algorithm"));
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2000/09/xmldsig#sha1":
                    if (!System.IdentityModel.SecurityUtils.RequiresFipsCompliance)
                    {
                        return new SHA1Managed();
                    }
                    return new SHA1CryptoServiceProvider();

                case "http://www.w3.org/2001/10/xml-exc-c14n#":
                    return new XmlDsigExcC14NTransform();

                case "http://www.w3.org/2001/04/xmlenc#sha256":
                    if (!System.IdentityModel.SecurityUtils.RequiresFipsCompliance)
                    {
                        return new SHA256Managed();
                    }
                    return new SHA256CryptoServiceProvider();

                case "http://www.w3.org/2001/04/xmlenc#sha512":
                    if (!System.IdentityModel.SecurityUtils.RequiresFipsCompliance)
                    {
                        return new SHA512Managed();
                    }
                    return new SHA512CryptoServiceProvider();

                case "http://www.w3.org/2001/04/xmlenc#aes128-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes192-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes256-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes128":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes192":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes256":
                    if (!System.IdentityModel.SecurityUtils.RequiresFipsCompliance)
                    {
                        return new RijndaelManaged();
                    }
                    return new RijndaelCryptoServiceProvider();

                case "http://www.w3.org/2001/04/xmlenc#tripledes-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-tripledes":
                    return new TripleDESCryptoServiceProvider();

                case "http://www.w3.org/2000/09/xmldsig#hmac-sha1":
                {
                    byte[] data = new byte[0x40];
                    new RNGCryptoServiceProvider().GetBytes(data);
                    return new HMACSHA1(data, !System.IdentityModel.SecurityUtils.RequiresFipsCompliance);
                }
                case "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256":
                    if (System.IdentityModel.SecurityUtils.RequiresFipsCompliance)
                    {
                        return null;
                    }
                    return new HMACSHA256();

                case "http://www.w3.org/2001/10/xml-exc-c14n#WithComments":
                    return new XmlDsigExcC14NWithCommentsTransform();

                case "http://www.w3.org/2001/04/xmlenc#ripemd160":
                    if (System.IdentityModel.SecurityUtils.RequiresFipsCompliance)
                    {
                        return null;
                    }
                    return new RIPEMD160Managed();

                case "http://www.w3.org/2001/04/xmlenc#des-cbc":
                    return new DESCryptoServiceProvider();
            }
            return null;
        }

        internal static int GetIVSize(string algorithm)
        {
            object algorithmFromConfig = GetAlgorithmFromConfig(algorithm);
            if (algorithmFromConfig != null)
            {
                SymmetricAlgorithm algorithm2 = algorithmFromConfig as SymmetricAlgorithm;
                if (algorithm2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm", new object[] { algorithm })));
                }
                return algorithm2.BlockSize;
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2001/04/xmlenc#tripledes-cbc":
                    return TripleDES.BlockSize;

                case "http://www.w3.org/2001/04/xmlenc#aes128-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes192-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes256-cbc":
                    return Rijndael.BlockSize;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnsupportedEncryptionAlgorithm", new object[] { algorithm })));
        }

        internal static SymmetricAlgorithm GetSymmetricAlgorithm(byte[] key, string algorithm)
        {
            SymmetricAlgorithm algorithm2;
            object algorithmFromConfig = GetAlgorithmFromConfig(algorithm);
            if (algorithmFromConfig != null)
            {
                algorithm2 = algorithmFromConfig as SymmetricAlgorithm;
                if (algorithm2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm", new object[] { algorithm })));
                }
                if (key != null)
                {
                    algorithm2.Key = key;
                }
                return algorithm2;
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2001/04/xmlenc#tripledes-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-tripledes":
                    algorithm2 = new TripleDESCryptoServiceProvider();
                    break;

                case "http://www.w3.org/2001/04/xmlenc#aes128-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes192-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes256-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes128":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes192":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes256":
                    algorithm2 = System.IdentityModel.SecurityUtils.RequiresFipsCompliance ? ((SymmetricAlgorithm) new RijndaelCryptoServiceProvider()) : ((SymmetricAlgorithm) new RijndaelManaged());
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnsupportedEncryptionAlgorithm", new object[] { algorithm })));
            }
            if (key != null)
            {
                algorithm2.Key = key;
            }
            return algorithm2;
        }

        internal static bool IsAsymmetricAlgorithm(string algorithm)
        {
            object algorithmFromConfig = null;
            string str;
            try
            {
                algorithmFromConfig = GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
                algorithmFromConfig = null;
            }
            if (algorithmFromConfig != null)
            {
                AsymmetricAlgorithm algorithm2 = algorithmFromConfig as AsymmetricAlgorithm;
                SignatureDescription description = algorithmFromConfig as SignatureDescription;
                if ((algorithm2 == null) && (description == null))
                {
                    return false;
                }
                return true;
            }
            if (((str = algorithm) == null) || ((!(str == "http://www.w3.org/2000/09/xmldsig#dsa-sha1") && !(str == "http://www.w3.org/2000/09/xmldsig#rsa-sha1")) && ((!(str == "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256") && !(str == "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p")) && !(str == "http://www.w3.org/2001/04/xmlenc#rsa-1_5"))))
            {
                return false;
            }
            return true;
        }

        internal static bool IsEqual(byte[] a, byte[] b)
        {
            if (!object.ReferenceEquals(a, b))
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
            }
            return true;
        }

        internal static bool IsSymmetricAlgorithm(string algorithm)
        {
            object algorithmFromConfig = null;
            try
            {
                algorithmFromConfig = GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
                algorithmFromConfig = null;
            }
            if (algorithmFromConfig != null)
            {
                SymmetricAlgorithm algorithm2 = algorithmFromConfig as SymmetricAlgorithm;
                KeyedHashAlgorithm algorithm3 = algorithmFromConfig as KeyedHashAlgorithm;
                if ((algorithm2 == null) && (algorithm3 == null))
                {
                    return false;
                }
                return true;
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
                case "http://www.w3.org/2001/04/xmlenc#aes128-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes192-cbc":
                case "http://www.w3.org/2001/04/xmlenc#des-cbc":
                case "http://www.w3.org/2001/04/xmlenc#aes256-cbc":
                case "http://www.w3.org/2001/04/xmlenc#tripledes-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes128":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes192":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes256":
                case "http://www.w3.org/2001/04/xmlenc#kw-tripledes":
                case "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1":
                case "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1":
                    return true;
            }
            return false;
        }

        internal static bool IsSymmetricSupportedAlgorithm(string algorithm, int keySize)
        {
            bool flag = false;
            object algorithmFromConfig = null;
            try
            {
                algorithmFromConfig = GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
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
                    if (keySize < 0x80)
                    {
                        return false;
                    }
                    return (keySize <= 0x100);

                case "http://www.w3.org/2001/04/xmlenc#aes192-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes192":
                    if (keySize < 0xc0)
                    {
                        return false;
                    }
                    return (keySize <= 0x100);

                case "http://www.w3.org/2001/04/xmlenc#aes256-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-aes256":
                    return (keySize == 0x100);

                case "http://www.w3.org/2001/04/xmlenc#tripledes-cbc":
                case "http://www.w3.org/2001/04/xmlenc#kw-tripledes":
                    return ((keySize == 0x80) || (keySize == 0xc0));
            }
            return flag;
        }

        internal static KeyedHashAlgorithm NewHmacSha1KeyedHashAlgorithm(byte[] key)
        {
            return CreateKeyedHashAlgorithm(key, "http://www.w3.org/2000/09/xmldsig#hmac-sha1");
        }

        internal static KeyedHashAlgorithm NewHmacSha256KeyedHashAlgorithm(byte[] key)
        {
            return CreateKeyedHashAlgorithm(key, "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256");
        }

        internal static System.Security.Cryptography.Rijndael NewRijndaelSymmetricAlgorithm()
        {
            System.Security.Cryptography.Rijndael symmetricAlgorithm = GetSymmetricAlgorithm(null, "http://www.w3.org/2001/04/xmlenc#aes128-cbc") as System.Security.Cryptography.Rijndael;
            if (symmetricAlgorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("CustomCryptoAlgorithmIsNotValidSymmetricAlgorithm", new object[] { "http://www.w3.org/2001/04/xmlenc#aes128-cbc" })));
            }
            return symmetricAlgorithm;
        }

        internal static HashAlgorithm NewSha1HashAlgorithm()
        {
            return CreateHashAlgorithm("http://www.w3.org/2000/09/xmldsig#sha1");
        }

        internal static HashAlgorithm NewSha256HashAlgorithm()
        {
            return CreateHashAlgorithm("http://www.w3.org/2001/04/xmlenc#sha256");
        }

        internal static byte[] UnwrapKey(byte[] wrappingKey, byte[] wrappedKey, string algorithm)
        {
            SymmetricAlgorithm algorithm2;
            object algorithmFromConfig = GetAlgorithmFromConfig(algorithm);
            if (algorithmFromConfig != null)
            {
                algorithm2 = algorithmFromConfig as SymmetricAlgorithm;
                if (algorithm2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("InvalidCustomKeyWrapAlgorithm", new object[] { algorithm })));
                }
                using (algorithm2)
                {
                    algorithm2.Key = wrappingKey;
                    return EncryptedXml.DecryptKey(wrappedKey, algorithm2);
                }
            }
            string str = algorithm;
            if (str != null)
            {
                if (!(str == "http://www.w3.org/2001/04/xmlenc#kw-tripledes"))
                {
                    if (((str == "http://www.w3.org/2001/04/xmlenc#kw-aes128") || (str == "http://www.w3.org/2001/04/xmlenc#kw-aes192")) || (str == "http://www.w3.org/2001/04/xmlenc#kw-aes256"))
                    {
                        algorithm2 = System.IdentityModel.SecurityUtils.RequiresFipsCompliance ? ((SymmetricAlgorithm) new RijndaelCryptoServiceProvider()) : ((SymmetricAlgorithm) new RijndaelManaged());
                        goto Label_00E4;
                    }
                }
                else
                {
                    algorithm2 = new TripleDESCryptoServiceProvider();
                    goto Label_00E4;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnsupportedKeyWrapAlgorithm", new object[] { algorithm })));
        Label_00E4:
            using (algorithm2)
            {
                algorithm2.Key = wrappingKey;
                return EncryptedXml.DecryptKey(wrappedKey, algorithm2);
            }
        }

        internal static void ValidateBufferBounds(Array buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));
            }
            if ((count < 0) || (count > buffer.Length))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", System.IdentityModel.SR.GetString("ValueMustBeInRange", new object[] { 0, buffer.Length })));
            }
            if ((offset < 0) || (offset > (buffer.Length - count)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", System.IdentityModel.SR.GetString("ValueMustBeInRange", new object[] { 0, buffer.Length - count })));
            }
        }

        internal static byte[] WrapKey(byte[] wrappingKey, byte[] keyToBeWrapped, string algorithm)
        {
            SymmetricAlgorithm algorithm2;
            object algorithmFromConfig = GetAlgorithmFromConfig(algorithm);
            if (algorithmFromConfig != null)
            {
                algorithm2 = algorithmFromConfig as SymmetricAlgorithm;
                if (algorithm2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("InvalidCustomKeyWrapAlgorithm", new object[] { algorithm })));
                }
                using (algorithm2)
                {
                    algorithm2.Key = wrappingKey;
                    return EncryptedXml.EncryptKey(keyToBeWrapped, algorithm2);
                }
            }
            string str = algorithm;
            if (str != null)
            {
                if (!(str == "http://www.w3.org/2001/04/xmlenc#kw-tripledes"))
                {
                    if (((str == "http://www.w3.org/2001/04/xmlenc#kw-aes128") || (str == "http://www.w3.org/2001/04/xmlenc#kw-aes192")) || (str == "http://www.w3.org/2001/04/xmlenc#kw-aes256"))
                    {
                        algorithm2 = System.IdentityModel.SecurityUtils.RequiresFipsCompliance ? ((SymmetricAlgorithm) new RijndaelCryptoServiceProvider()) : ((SymmetricAlgorithm) new RijndaelManaged());
                        goto Label_00E4;
                    }
                }
                else
                {
                    algorithm2 = new TripleDESCryptoServiceProvider();
                    goto Label_00E4;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(System.IdentityModel.SR.GetString("UnsupportedKeyWrapAlgorithm", new object[] { algorithm })));
        Label_00E4:
            using (algorithm2)
            {
                algorithm2.Key = wrappingKey;
                return EncryptedXml.EncryptKey(keyToBeWrapped, algorithm2);
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

        internal static System.Security.Cryptography.RandomNumberGenerator RandomNumberGenerator
        {
            get
            {
                if (random == null)
                {
                    random = new RNGCryptoServiceProvider();
                }
                return random;
            }
        }

        internal static System.Security.Cryptography.Rijndael Rijndael
        {
            get
            {
                if (CryptoHelper.rijndael == null)
                {
                    System.Security.Cryptography.Rijndael rijndael = System.IdentityModel.SecurityUtils.RequiresFipsCompliance ? ((System.Security.Cryptography.Rijndael) new RijndaelCryptoServiceProvider()) : ((System.Security.Cryptography.Rijndael) new RijndaelManaged());
                    rijndael.Padding = PaddingMode.ISO10126;
                    CryptoHelper.rijndael = rijndael;
                }
                return CryptoHelper.rijndael;
            }
        }

        internal static System.Security.Cryptography.TripleDES TripleDES
        {
            get
            {
                if (tripleDES == null)
                {
                    TripleDESCryptoServiceProvider provider = new TripleDESCryptoServiceProvider {
                        Padding = PaddingMode.ISO10126
                    };
                    tripleDES = provider;
                }
                return tripleDES;
            }
        }
    }
}

