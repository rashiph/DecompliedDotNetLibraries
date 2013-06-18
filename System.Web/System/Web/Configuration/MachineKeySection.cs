namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;
    using System.Web.Util;

    public sealed class MachineKeySection : ConfigurationSection
    {
        private static int _AutoGenDecryptionKeySize = 0x18;
        private bool _AutogenKey;
        private static int _AutoGenValidationKeySize = 0x40;
        private string _cachedValidation;
        private MachineKeyValidation _cachedValidationEnum;
        private static string _CustomValidationName;
        private static bool _CustomValidationTypeIsKeyed;
        private byte[] _DecryptionKey;
        private static int _HashSize = 0x20;
        private static int _IVLengthDecryption = 0x40;
        private static int _IVLengthValidation = 0x40;
        private static readonly ConfigurationProperty _propCompatibilityMode = new ConfigurationProperty("compatibilityMode", typeof(MachineKeyCompatibilityMode), MachineKeyCompatibilityMode.Framework20SP1, null, null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDecryption = new ConfigurationProperty("decryption", typeof(string), "Auto", StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDecryptionKey = new ConfigurationProperty("decryptionKey", typeof(string), "AutoGenerate,IsolateApps", StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propValidation = new ConfigurationProperty("validation", typeof(string), "HMACSHA256", StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propValidationKey = new ConfigurationProperty("validationKey", typeof(string), "AutoGenerate,IsolateApps", StdValidatorsAndConverters.WhiteSpaceTrimStringConverter, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static bool _UseHMACSHA = true;
        private static bool _UsingCustomEncryption = false;
        private bool _validationIsCached;
        private byte[] _ValidationKey;
        private const string ALGO_PREFIX = "alg:";
        private bool DataInitialized;
        internal const MachineKeyValidation DefaultValidation = MachineKeyValidation.SHA1;
        internal const string DefaultValidationAlgorithm = "HMACSHA256";
        private const int HMACSHA256_HASH_SIZE = 0x20;
        private const int HMACSHA256_KEY_SIZE = 0x40;
        private const int HMACSHA384_HASH_SIZE = 0x30;
        private const int HMACSHA384_KEY_SIZE = 0x80;
        private const int HMACSHA512_HASH_SIZE = 0x40;
        private const int HMACSHA512_KEY_SIZE = 0x80;
        private const int MD5_HASH_SIZE = 0x10;
        private const int MD5_KEY_SIZE = 0x40;
        private static char[] s_acharval;
        private static byte[] s_ahexval;
        private static MachineKeyCompatibilityMode s_compatMode;
        private static MachineKeySection s_config;
        private static object s_initLock = new object();
        private static byte[] s_inner = null;
        private static SymmetricAlgorithm s_oSymAlgoDecryption;
        private static SymmetricAlgorithm s_oSymAlgoLegacy;
        private static SymmetricAlgorithm s_oSymAlgoValidation;
        private static byte[] s_outer = null;
        private static RNGCryptoServiceProvider s_randomNumberGenerator;
        private static byte[] s_validationKey;
        private const int SHA1_HASH_SIZE = 20;
        private const int SHA1_KEY_SIZE = 0x40;

        static MachineKeySection()
        {
            _properties.Add(_propValidationKey);
            _properties.Add(_propDecryptionKey);
            _properties.Add(_propValidation);
            _properties.Add(_propDecryption);
            _properties.Add(_propCompatibilityMode);
        }

        internal static unsafe string ByteArrayToHexString(byte[] buf, int iLen)
        {
            char[] chArray = s_acharval;
            if (chArray == null)
            {
                chArray = new char[0x10];
                int length = chArray.Length;
                while (--length >= 0)
                {
                    if (length < 10)
                    {
                        chArray[length] = (char) (0x30 + length);
                    }
                    else
                    {
                        chArray[length] = (char) (0x41 + (length - 10));
                    }
                }
                s_acharval = chArray;
            }
            if (buf == null)
            {
                return null;
            }
            if (iLen == 0)
            {
                iLen = buf.Length;
            }
            char[] chArray2 = new char[iLen * 2];
            fixed (char* chRef = chArray2)
            {
                fixed (char* chRef2 = chArray)
                {
                    fixed (byte* numRef = buf)
                    {
                        char* chPtr = chRef;
                        for (byte* numPtr = numRef; --iLen >= 0; numPtr++)
                        {
                            chPtr++;
                            chPtr[0] = chRef2[(numPtr[0] & 240) >> 4];
                            chPtr++;
                            chPtr[0] = chRef2[numPtr[0] & 15];
                        }
                    }
                }
            }
            return new string(chArray2);
        }

        private void CacheValidation()
        {
            this._cachedValidation = (string) base[_propValidation];
            if (this._cachedValidation == null)
            {
                this._cachedValidation = "HMACSHA256";
            }
            this._cachedValidationEnum = MachineKeyValidationConverter.ConvertToEnum(this._cachedValidation);
            this._validationIsCached = true;
        }

        private void ConfigureEncryptionObject()
        {
            using (new ApplicationImpersonationContext())
            {
                s_validationKey = this.ValidationKeyInternal;
                byte[] decryptionKeyInternal = this.DecryptionKeyInternal;
                if (_UseHMACSHA)
                {
                    SetInnerOuterKeys(s_validationKey, ref s_inner, ref s_outer);
                }
                this.DestroyKeys();
                string decryption = this.Decryption;
                if (decryption != null)
                {
                    if (!(decryption == "3DES"))
                    {
                        if (decryption == "DES")
                        {
                            goto Label_0085;
                        }
                        if (decryption == "AES")
                        {
                            goto Label_0091;
                        }
                        if (decryption == "Auto")
                        {
                            goto Label_009D;
                        }
                    }
                    else
                    {
                        s_oSymAlgoDecryption = new TripleDESCryptoServiceProvider();
                    }
                }
                goto Label_00B9;
            Label_0085:
                s_oSymAlgoDecryption = new DESCryptoServiceProvider();
                goto Label_00B9;
            Label_0091:
                s_oSymAlgoDecryption = GetAESAlgorithm();
                goto Label_00B9;
            Label_009D:
                if (decryptionKeyInternal.Length == 8)
                {
                    s_oSymAlgoDecryption = new DESCryptoServiceProvider();
                }
                else
                {
                    s_oSymAlgoDecryption = GetAESAlgorithm();
                }
            Label_00B9:
                if (s_oSymAlgoDecryption == null)
                {
                    this.InitValidationAndEncyptionSizes();
                }
                switch (this.Validation)
                {
                    case MachineKeyValidation.TripleDES:
                        if (decryptionKeyInternal.Length != 8)
                        {
                            break;
                        }
                        s_oSymAlgoValidation = new DESCryptoServiceProvider();
                        goto Label_0107;

                    case MachineKeyValidation.AES:
                        s_oSymAlgoValidation = GetAESAlgorithm();
                        goto Label_0107;

                    default:
                        goto Label_0107;
                }
                s_oSymAlgoValidation = new TripleDESCryptoServiceProvider();
            Label_0107:
                if (s_oSymAlgoValidation != null)
                {
                    this.SetKeyOnSymAlgorithm(s_oSymAlgoValidation, decryptionKeyInternal);
                    _IVLengthValidation = RoundupNumBitsToNumBytes(s_oSymAlgoValidation.KeySize);
                }
                this.SetKeyOnSymAlgorithm(s_oSymAlgoDecryption, decryptionKeyInternal);
                _IVLengthDecryption = RoundupNumBitsToNumBytes(s_oSymAlgoDecryption.KeySize);
                InitLegacyEncAlgorithm(decryptionKeyInternal);
                DestroyByteArray(decryptionKeyInternal);
            }
        }

        internal static void DestroyByteArray(byte[] buf)
        {
            if ((buf != null) && (buf.Length >= 1))
            {
                for (int i = 0; i < buf.Length; i++)
                {
                    buf[i] = 0;
                }
            }
        }

        internal void DestroyKeys()
        {
            DestroyByteArray(this._ValidationKey);
            DestroyByteArray(this._DecryptionKey);
        }

        internal static byte[] EncryptOrDecryptData(bool fEncrypt, byte[] buf, byte[] modifier, int start, int length)
        {
            return EncryptOrDecryptData(fEncrypt, buf, modifier, start, length, false, false, IVType.Random);
        }

        internal static byte[] EncryptOrDecryptData(bool fEncrypt, byte[] buf, byte[] modifier, int start, int length, bool useValidationSymAlgo)
        {
            return EncryptOrDecryptData(fEncrypt, buf, modifier, start, length, useValidationSymAlgo, false, IVType.Random);
        }

        internal static byte[] EncryptOrDecryptData(bool fEncrypt, byte[] buf, byte[] modifier, int start, int length, bool useValidationSymAlgo, bool useLegacyMode, IVType ivType)
        {
            return EncryptOrDecryptData(fEncrypt, buf, modifier, start, length, useValidationSymAlgo, useLegacyMode, ivType, !AppSettings.UseLegacyEncryption);
        }

        internal static byte[] EncryptOrDecryptData(bool fEncrypt, byte[] buf, byte[] modifier, int start, int length, bool useValidationSymAlgo, bool useLegacyMode, IVType ivType, bool signData)
        {
            byte[] buffer8;
            try
            {
                byte[] buffer4;
                EnsureConfig();
                if (!fEncrypt && signData)
                {
                    if ((start != 0) || (length != buf.Length))
                    {
                        byte[] dst = new byte[length];
                        Buffer.BlockCopy(buf, start, dst, 0, length);
                        buf = dst;
                        start = 0;
                    }
                    buf = GetUnHashedData(buf);
                    if (buf == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("Unable_to_validate_data"));
                    }
                    length = buf.Length;
                }
                if (useLegacyMode)
                {
                    useLegacyMode = _UsingCustomEncryption;
                }
                MemoryStream stream = new MemoryStream();
                ICryptoTransform transform = GetCryptoTransform(fEncrypt, useValidationSymAlgo, useLegacyMode);
                CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Write);
                bool flag = signData || ((ivType != IVType.None) && (CompatMode > MachineKeyCompatibilityMode.Framework20SP1));
                if (fEncrypt && flag)
                {
                    int ivLength = useValidationSymAlgo ? _IVLengthValidation : _IVLengthDecryption;
                    byte[] data = null;
                    switch (ivType)
                    {
                        case IVType.Random:
                            data = new byte[ivLength];
                            RandomNumberGenerator.GetBytes(data);
                            break;

                        case IVType.Hash:
                            data = GetIVHash(buf, ivLength);
                            break;
                    }
                    stream2.Write(data, 0, data.Length);
                }
                stream2.Write(buf, start, length);
                if (fEncrypt && (modifier != null))
                {
                    stream2.Write(modifier, 0, modifier.Length);
                }
                stream2.FlushFinalBlock();
                byte[] src = stream.ToArray();
                stream2.Close();
                ReturnCryptoTransform(fEncrypt, transform, useValidationSymAlgo, useLegacyMode);
                if (!fEncrypt && flag)
                {
                    int srcOffset = useValidationSymAlgo ? _IVLengthValidation : _IVLengthDecryption;
                    int count = src.Length - srcOffset;
                    if (count < 0)
                    {
                        throw new Exception();
                    }
                    buffer4 = new byte[count];
                    Buffer.BlockCopy(src, srcOffset, buffer4, 0, count);
                }
                else
                {
                    buffer4 = src;
                }
                if ((!fEncrypt && (modifier != null)) && (modifier.Length > 0))
                {
                    bool flag2 = false;
                    for (int i = 0; i < modifier.Length; i++)
                    {
                        if (buffer4[(buffer4.Length - modifier.Length) + i] != modifier[i])
                        {
                            flag2 = true;
                        }
                    }
                    if (flag2)
                    {
                        throw new HttpException(System.Web.SR.GetString("Unable_to_validate_data"));
                    }
                    byte[] buffer5 = new byte[buffer4.Length - modifier.Length];
                    Buffer.BlockCopy(buffer4, 0, buffer5, 0, buffer5.Length);
                    buffer4 = buffer5;
                }
                if (fEncrypt && signData)
                {
                    byte[] buffer6 = HashData(buffer4, null, 0, buffer4.Length);
                    byte[] buffer7 = new byte[buffer4.Length + buffer6.Length];
                    Buffer.BlockCopy(buffer4, 0, buffer7, 0, buffer4.Length);
                    Buffer.BlockCopy(buffer6, 0, buffer7, buffer4.Length, buffer6.Length);
                    buffer4 = buffer7;
                }
                buffer8 = buffer4;
            }
            catch
            {
                throw new HttpException(System.Web.SR.GetString("Unable_to_validate_data"));
            }
            return buffer8;
        }

        private static void EnsureConfig()
        {
            if (s_config == null)
            {
                lock (s_initLock)
                {
                    if (s_config == null)
                    {
                        MachineKeySection machineKey = RuntimeConfig.GetAppConfig().MachineKey;
                        machineKey.ConfigureEncryptionObject();
                        s_config = machineKey;
                        s_compatMode = machineKey.CompatibilityMode;
                    }
                }
            }
        }

        internal static SymmetricAlgorithm GetAESAlgorithm()
        {
            try
            {
                return new RijndaelManaged();
            }
            catch
            {
            }
            return new AesCryptoServiceProvider();
        }

        private static ICryptoTransform GetCryptoTransform(bool fEncrypt, bool useValidationSymAlgo, bool legacyMode)
        {
            SymmetricAlgorithm algorithm = legacyMode ? s_oSymAlgoLegacy : (useValidationSymAlgo ? s_oSymAlgoValidation : s_oSymAlgoDecryption);
            lock (algorithm)
            {
                return (fEncrypt ? algorithm.CreateEncryptor() : algorithm.CreateDecryptor());
            }
        }

        internal static byte[] GetDecodedData(byte[] buf, byte[] modifier, int start, int length, ref int dataLength)
        {
            EnsureConfig();
            if ((s_config.Validation == MachineKeyValidation.TripleDES) || (s_config.Validation == MachineKeyValidation.AES))
            {
                buf = EncryptOrDecryptData(false, buf, modifier, start, length, true);
                if ((buf == null) || (buf.Length < _HashSize))
                {
                    throw new HttpException(System.Web.SR.GetString("Unable_to_validate_data"));
                }
                length = buf.Length;
                start = 0;
            }
            if (((length < _HashSize) || (start < 0)) || (start >= length))
            {
                throw new HttpException(System.Web.SR.GetString("Unable_to_validate_data"));
            }
            byte[] buffer = HashData(buf, modifier, start, length - _HashSize);
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] != buf[((start + length) - _HashSize) + i])
                {
                    throw new HttpException(System.Web.SR.GetString("Unable_to_validate_data"));
                }
            }
            dataLength = length - _HashSize;
            return buf;
        }

        internal static byte[] GetEncodedData(byte[] buf, byte[] modifier, int start, ref int length)
        {
            byte[] buffer2;
            EnsureConfig();
            byte[] src = HashData(buf, modifier, start, length);
            if (((buf.Length - start) - length) >= src.Length)
            {
                Buffer.BlockCopy(src, 0, buf, start + length, src.Length);
                buffer2 = buf;
            }
            else
            {
                buffer2 = new byte[length + src.Length];
                Buffer.BlockCopy(buf, start, buffer2, 0, length);
                Buffer.BlockCopy(src, 0, buffer2, length, src.Length);
                start = 0;
            }
            length += src.Length;
            if ((s_config.Validation == MachineKeyValidation.TripleDES) || (s_config.Validation == MachineKeyValidation.AES))
            {
                buffer2 = EncryptOrDecryptData(true, buffer2, modifier, start, length, true);
                length = buffer2.Length;
            }
            return buffer2;
        }

        private static byte[] GetHMACSHA1Hash(byte[] buf, byte[] modifier, int start, int length)
        {
            if ((start < 0) || (start > buf.Length))
            {
                throw new ArgumentException(System.Web.SR.GetString("InvalidArgumentValue", new object[] { "start" }));
            }
            if (((length < 0) || (buf == null)) || ((start + length) > buf.Length))
            {
                throw new ArgumentException(System.Web.SR.GetString("InvalidArgumentValue", new object[] { "length" }));
            }
            byte[] hash = new byte[_HashSize];
            if (System.Web.UnsafeNativeMethods.GetHMACSHA1Hash(buf, start, length, modifier, (modifier == null) ? 0 : modifier.Length, s_inner, s_inner.Length, s_outer, s_outer.Length, hash, hash.Length) == 0)
            {
                return hash;
            }
            _UseHMACSHA = false;
            return null;
        }

        private static byte[] GetIVHash(byte[] buf, int ivLength)
        {
            int num = ivLength;
            int dstOffset = 0;
            byte[] dst = new byte[ivLength];
            byte[] data = buf;
            while (dstOffset < ivLength)
            {
                byte[] hash = new byte[_HashSize];
                Marshal.ThrowExceptionForHR(System.Web.UnsafeNativeMethods.GetSHA1Hash(data, data.Length, hash, hash.Length));
                data = hash;
                int count = Math.Min(_HashSize, num);
                Buffer.BlockCopy(data, 0, dst, dstOffset, count);
                dstOffset += count;
                num -= count;
            }
            return dst;
        }

        internal static byte[] GetUnHashedData(byte[] bufHashed)
        {
            if (!VerifyHashedData(bufHashed))
            {
                return null;
            }
            byte[] dst = new byte[bufHashed.Length - _HashSize];
            Buffer.BlockCopy(bufHashed, 0, dst, 0, dst.Length);
            return dst;
        }

        internal static string HashAndBase64EncodeString(string s)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            return Convert.ToBase64String(HashData(bytes, null, 0, bytes.Length));
        }

        internal static byte[] HashData(byte[] buf, byte[] modifier, int start, int length)
        {
            EnsureConfig();
            if (s_config.Validation == MachineKeyValidation.MD5)
            {
                return HashDataUsingNonKeyedAlgorithm(null, buf, modifier, start, length, s_validationKey);
            }
            if (_UseHMACSHA)
            {
                byte[] buffer = GetHMACSHA1Hash(buf, modifier, start, length);
                if (buffer != null)
                {
                    return buffer;
                }
            }
            if (_CustomValidationTypeIsKeyed)
            {
                return HashDataUsingKeyedAlgorithm(KeyedHashAlgorithm.Create(_CustomValidationName), buf, modifier, start, length, s_validationKey);
            }
            return HashDataUsingNonKeyedAlgorithm(HashAlgorithm.Create(_CustomValidationName), buf, modifier, start, length, s_validationKey);
        }

        private static byte[] HashDataUsingKeyedAlgorithm(KeyedHashAlgorithm hashAlgo, byte[] buf, byte[] modifier, int start, int length, byte[] validationKey)
        {
            int num = length + ((modifier != null) ? modifier.Length : 0);
            byte[] dst = new byte[num];
            Buffer.BlockCopy(buf, start, dst, 0, length);
            if (modifier != null)
            {
                Buffer.BlockCopy(modifier, 0, dst, length, modifier.Length);
            }
            hashAlgo.Key = validationKey;
            return hashAlgo.ComputeHash(dst);
        }

        private static byte[] HashDataUsingNonKeyedAlgorithm(HashAlgorithm hashAlgo, byte[] buf, byte[] modifier, int start, int length, byte[] validationKey)
        {
            int num = (length + validationKey.Length) + ((modifier != null) ? modifier.Length : 0);
            byte[] dst = new byte[num];
            Buffer.BlockCopy(buf, start, dst, 0, length);
            if (modifier != null)
            {
                Buffer.BlockCopy(modifier, 0, dst, length, modifier.Length);
            }
            Buffer.BlockCopy(validationKey, 0, dst, length, validationKey.Length);
            if (hashAlgo != null)
            {
                return hashAlgo.ComputeHash(dst);
            }
            byte[] hash = new byte[0x10];
            Marshal.ThrowExceptionForHR(System.Web.UnsafeNativeMethods.GetSHA1Hash(dst, dst.Length, hash, hash.Length));
            return hash;
        }

        internal static byte[] HexStringToByteArray(string str)
        {
            if ((str.Length & 1) == 1)
            {
                return null;
            }
            byte[] buffer = s_ahexval;
            if (buffer == null)
            {
                buffer = new byte[0x67];
                int index = buffer.Length;
                while (--index >= 0)
                {
                    if ((0x30 <= index) && (index <= 0x39))
                    {
                        buffer[index] = (byte) (index - 0x30);
                    }
                    else
                    {
                        if ((0x61 <= index) && (index <= 0x66))
                        {
                            buffer[index] = (byte) ((index - 0x61) + 10);
                            continue;
                        }
                        if ((0x41 <= index) && (index <= 70))
                        {
                            buffer[index] = (byte) ((index - 0x41) + 10);
                        }
                    }
                }
                s_ahexval = buffer;
            }
            byte[] buffer2 = new byte[str.Length / 2];
            int num2 = 0;
            int num3 = 0;
            int length = buffer2.Length;
            while (--length >= 0)
            {
                int num5;
                int num6;
                try
                {
                    num5 = buffer[str[num2++]];
                }
                catch (ArgumentNullException)
                {
                    num5 = 0;
                    return null;
                }
                catch (ArgumentException)
                {
                    num5 = 0;
                    return null;
                }
                catch (IndexOutOfRangeException)
                {
                    num5 = 0;
                    return null;
                }
                try
                {
                    num6 = buffer[str[num2++]];
                }
                catch (ArgumentNullException)
                {
                    num6 = 0;
                    return null;
                }
                catch (ArgumentException)
                {
                    num6 = 0;
                    return null;
                }
                catch (IndexOutOfRangeException)
                {
                    num6 = 0;
                    return null;
                }
                buffer2[num3++] = (byte) ((num5 << 4) + num6);
            }
            return buffer2;
        }

        private static void InitLegacyEncAlgorithm(byte[] dKey)
        {
            if (_UsingCustomEncryption)
            {
                s_oSymAlgoLegacy = GetAESAlgorithm();
                try
                {
                    s_oSymAlgoLegacy.Key = dKey;
                }
                catch
                {
                    if (dKey.Length <= 0x18)
                    {
                        throw;
                    }
                    byte[] dst = new byte[0x18];
                    Buffer.BlockCopy(dKey, 0, dst, 0, dst.Length);
                    dKey = dst;
                    s_oSymAlgoLegacy.Key = dKey;
                }
            }
        }

        private void InitValidationAndEncyptionSizes()
        {
            _CustomValidationName = this.ValidationAlgorithm;
            _CustomValidationTypeIsKeyed = true;
            switch (this.ValidationAlgorithm)
            {
                case "AES":
                case "3DES":
                    _UseHMACSHA = true;
                    _HashSize = 20;
                    _AutoGenValidationKeySize = 0x40;
                    break;

                case "SHA1":
                    _UseHMACSHA = true;
                    _HashSize = 20;
                    _AutoGenValidationKeySize = 0x40;
                    break;

                case "MD5":
                    _CustomValidationTypeIsKeyed = false;
                    _UseHMACSHA = false;
                    _HashSize = 0x10;
                    _AutoGenValidationKeySize = 0x40;
                    break;

                case "HMACSHA256":
                    _UseHMACSHA = true;
                    _HashSize = 0x20;
                    _AutoGenValidationKeySize = 0x40;
                    break;

                case "HMACSHA384":
                    _UseHMACSHA = true;
                    _HashSize = 0x30;
                    _AutoGenValidationKeySize = 0x80;
                    break;

                case "HMACSHA512":
                    _UseHMACSHA = true;
                    _HashSize = 0x40;
                    _AutoGenValidationKeySize = 0x80;
                    break;

                default:
                {
                    _UseHMACSHA = false;
                    if (!_CustomValidationName.StartsWith("alg:", StringComparison.Ordinal))
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Wrong_validation_enum"), base.ElementInformation.Properties["validation"].Source, base.ElementInformation.Properties["validation"].LineNumber);
                    }
                    _CustomValidationName = _CustomValidationName.Substring("alg:".Length);
                    HashAlgorithm algorithm = null;
                    try
                    {
                        _CustomValidationTypeIsKeyed = false;
                        algorithm = HashAlgorithm.Create(_CustomValidationName);
                    }
                    catch (Exception exception)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Wrong_validation_enum"), exception, base.ElementInformation.Properties["validation"].Source, base.ElementInformation.Properties["validation"].LineNumber);
                    }
                    if (algorithm == null)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Wrong_validation_enum"), base.ElementInformation.Properties["validation"].Source, base.ElementInformation.Properties["validation"].LineNumber);
                    }
                    _AutoGenValidationKeySize = 0;
                    _HashSize = 0;
                    _CustomValidationTypeIsKeyed = algorithm is KeyedHashAlgorithm;
                    if (!_CustomValidationTypeIsKeyed)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Wrong_validation_enum"), base.ElementInformation.Properties["validation"].Source, base.ElementInformation.Properties["validation"].LineNumber);
                    }
                    try
                    {
                        _HashSize = RoundupNumBitsToNumBytes(algorithm.HashSize);
                        if (_CustomValidationTypeIsKeyed)
                        {
                            _AutoGenValidationKeySize = ((KeyedHashAlgorithm) algorithm).Key.Length;
                        }
                        if (_AutoGenValidationKeySize < 1)
                        {
                            _AutoGenValidationKeySize = RoundupNumBitsToNumBytes(algorithm.InputBlockSize);
                        }
                        if (_AutoGenValidationKeySize < 1)
                        {
                            _AutoGenValidationKeySize = RoundupNumBitsToNumBytes(algorithm.OutputBlockSize);
                        }
                    }
                    catch
                    {
                    }
                    if ((_HashSize < 1) || (_AutoGenValidationKeySize < 1))
                    {
                        byte[] data = new byte[10];
                        byte[] buffer2 = new byte[0x200];
                        RandomNumberGenerator.GetBytes(data);
                        RandomNumberGenerator.GetBytes(buffer2);
                        _HashSize = algorithm.ComputeHash(data).Length;
                        if (_AutoGenValidationKeySize < 1)
                        {
                            if (_CustomValidationTypeIsKeyed)
                            {
                                _AutoGenValidationKeySize = ((KeyedHashAlgorithm) algorithm).Key.Length;
                            }
                            else
                            {
                                _AutoGenValidationKeySize = RoundupNumBitsToNumBytes(algorithm.InputBlockSize);
                            }
                        }
                        algorithm.Clear();
                    }
                    if (_HashSize < 1)
                    {
                        _HashSize = 0x40;
                    }
                    if (_AutoGenValidationKeySize < 1)
                    {
                        _AutoGenValidationKeySize = 0x80;
                    }
                    break;
                }
            }
            _AutoGenDecryptionKeySize = 0;
            switch (this.Decryption)
            {
                case "AES":
                    _AutoGenDecryptionKeySize = 0x18;
                    return;

                case "3DES":
                    _AutoGenDecryptionKeySize = 0x18;
                    return;

                case "Auto":
                    _AutoGenDecryptionKeySize = 0x18;
                    return;

                case "DES":
                    if ((this.ValidationAlgorithm == "AES") || (this.ValidationAlgorithm == "3DES"))
                    {
                        _AutoGenDecryptionKeySize = 0x18;
                        return;
                    }
                    _AutoGenDecryptionKeySize = 8;
                    return;
            }
            _UsingCustomEncryption = true;
            if (!this.Decryption.StartsWith("alg:", StringComparison.Ordinal))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Wrong_decryption_enum"), base.ElementInformation.Properties["decryption"].Source, base.ElementInformation.Properties["decryption"].LineNumber);
            }
            try
            {
                s_oSymAlgoDecryption = SymmetricAlgorithm.Create(this.Decryption.Substring("alg:".Length));
            }
            catch (Exception exception2)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Wrong_decryption_enum"), exception2, base.ElementInformation.Properties["decryption"].Source, base.ElementInformation.Properties["decryption"].LineNumber);
            }
            if (s_oSymAlgoDecryption == null)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Wrong_decryption_enum"), base.ElementInformation.Properties["decryption"].Source, base.ElementInformation.Properties["decryption"].LineNumber);
            }
            _AutoGenDecryptionKeySize = RoundupNumBitsToNumBytes(s_oSymAlgoDecryption.KeySize);
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            MachineKeySection section = parentElement as MachineKeySection;
            base.Reset(parentElement);
        }

        private static void ReturnCryptoTransform(bool fEncrypt, ICryptoTransform ct, bool useValidationSymAlgo, bool legacyMode)
        {
            ct.Dispose();
        }

        internal static int RoundupNumBitsToNumBytes(int numBits)
        {
            if (numBits < 0)
            {
                return 0;
            }
            return ((numBits / 8) + (((numBits & 7) != 0) ? 1 : 0));
        }

        private void RuntimeDataInitialize()
        {
            if (!this.DataInitialized)
            {
                byte[] data = null;
                bool flag = false;
                string validationKey = this.ValidationKey;
                string appDomainAppVirtualPath = HttpRuntime.AppDomainAppVirtualPath;
                this.InitValidationAndEncyptionSizes();
                if (appDomainAppVirtualPath == null)
                {
                    appDomainAppVirtualPath = Process.GetCurrentProcess().MainModule.ModuleName;
                    if (this.ValidationKey.Contains("AutoGenerate") || this.DecryptionKey.Contains("AutoGenerate"))
                    {
                        flag = true;
                        data = new byte[_AutoGenValidationKeySize + _AutoGenDecryptionKeySize];
                        RandomNumberGenerator.GetBytes(data);
                    }
                }
                bool flag2 = System.Web.Util.StringUtil.StringEndsWith(validationKey, ",IsolateApps");
                if (flag2)
                {
                    validationKey = validationKey.Substring(0, validationKey.Length - ",IsolateApps".Length);
                }
                if (validationKey == "AutoGenerate")
                {
                    this._ValidationKey = new byte[_AutoGenValidationKeySize];
                    if (flag)
                    {
                        Buffer.BlockCopy(data, 0, this._ValidationKey, 0, _AutoGenValidationKeySize);
                    }
                    else
                    {
                        Buffer.BlockCopy(HttpRuntime.s_autogenKeys, 0, this._ValidationKey, 0, _AutoGenValidationKeySize);
                    }
                }
                else
                {
                    if ((validationKey.Length < 40) || ((validationKey.Length & 1) == 1))
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Unable_to_get_cookie_authentication_validation_key", new object[] { validationKey.Length.ToString(CultureInfo.InvariantCulture) }), base.ElementInformation.Properties["validationKey"].Source, base.ElementInformation.Properties["validationKey"].LineNumber);
                    }
                    this._ValidationKey = HexStringToByteArray(validationKey);
                    if (this._ValidationKey == null)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_validation_key"), base.ElementInformation.Properties["validationKey"].Source, base.ElementInformation.Properties["validationKey"].LineNumber);
                    }
                }
                if (flag2)
                {
                    int hashCode = StringComparer.InvariantCultureIgnoreCase.GetHashCode(appDomainAppVirtualPath);
                    this._ValidationKey[0] = (byte) (hashCode & 0xff);
                    this._ValidationKey[1] = (byte) ((hashCode & 0xff00) >> 8);
                    this._ValidationKey[2] = (byte) ((hashCode & 0xff0000) >> 0x10);
                    this._ValidationKey[3] = (byte) ((hashCode & 0xff000000L) >> 0x18);
                }
                validationKey = this.DecryptionKey;
                flag2 = System.Web.Util.StringUtil.StringEndsWith(validationKey, ",IsolateApps");
                if (flag2)
                {
                    validationKey = validationKey.Substring(0, validationKey.Length - ",IsolateApps".Length);
                }
                if (validationKey == "AutoGenerate")
                {
                    this._DecryptionKey = new byte[_AutoGenDecryptionKeySize];
                    if (flag)
                    {
                        Buffer.BlockCopy(data, _AutoGenValidationKeySize, this._DecryptionKey, 0, _AutoGenDecryptionKeySize);
                    }
                    else
                    {
                        Buffer.BlockCopy(HttpRuntime.s_autogenKeys, _AutoGenValidationKeySize, this._DecryptionKey, 0, _AutoGenDecryptionKeySize);
                    }
                    this._AutogenKey = true;
                }
                else
                {
                    this._AutogenKey = false;
                    if ((validationKey.Length & 1) != 0)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_decryption_key"), base.ElementInformation.Properties["decryptionKey"].Source, base.ElementInformation.Properties["decryptionKey"].LineNumber);
                    }
                    this._DecryptionKey = HexStringToByteArray(validationKey);
                    if (this._DecryptionKey == null)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_decryption_key"), base.ElementInformation.Properties["decryptionKey"].Source, base.ElementInformation.Properties["decryptionKey"].LineNumber);
                    }
                }
                if (flag2)
                {
                    int num2 = StringComparer.InvariantCultureIgnoreCase.GetHashCode(appDomainAppVirtualPath);
                    this._DecryptionKey[0] = (byte) (num2 & 0xff);
                    this._DecryptionKey[1] = (byte) ((num2 & 0xff00) >> 8);
                    this._DecryptionKey[2] = (byte) ((num2 & 0xff0000) >> 0x10);
                    this._DecryptionKey[3] = (byte) ((num2 & 0xff000000L) >> 0x18);
                }
                this.DataInitialized = true;
            }
        }

        private static void SetInnerOuterKeys(byte[] validationKey, ref byte[] inner, ref byte[] outer)
        {
            byte[] hash = null;
            int num2;
            if (validationKey.Length > _AutoGenValidationKeySize)
            {
                hash = new byte[_HashSize];
                Marshal.ThrowExceptionForHR(System.Web.UnsafeNativeMethods.GetSHA1Hash(validationKey, validationKey.Length, hash, hash.Length));
            }
            if (inner == null)
            {
                inner = new byte[_AutoGenValidationKeySize];
            }
            if (outer == null)
            {
                outer = new byte[_AutoGenValidationKeySize];
            }
            for (num2 = 0; num2 < _AutoGenValidationKeySize; num2++)
            {
                inner[num2] = 0x36;
                outer[num2] = 0x5c;
            }
            for (num2 = 0; num2 < validationKey.Length; num2++)
            {
                inner[num2] = (byte) (inner[num2] ^ validationKey[num2]);
                outer[num2] = (byte) (outer[num2] ^ validationKey[num2]);
            }
        }

        private void SetKeyOnSymAlgorithm(SymmetricAlgorithm symAlgo, byte[] dKey)
        {
            try
            {
                if ((dKey.Length > 8) && (symAlgo is DESCryptoServiceProvider))
                {
                    byte[] dst = new byte[8];
                    Buffer.BlockCopy(dKey, 0, dst, 0, 8);
                    symAlgo.Key = dst;
                    DestroyByteArray(dst);
                }
                else
                {
                    symAlgo.Key = dKey;
                }
                symAlgo.GenerateIV();
                symAlgo.IV = new byte[symAlgo.IV.Length];
            }
            catch (Exception exception)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Bad_machine_key", new object[] { exception.Message }), base.ElementInformation.Properties["decryptionKey"].Source, base.ElementInformation.Properties["decryptionKey"].LineNumber);
            }
        }

        internal static bool VerifyHashedData(byte[] bufHashed)
        {
            EnsureConfig();
            if (bufHashed.Length <= _HashSize)
            {
                return false;
            }
            byte[] buffer = HashData(bufHashed, null, 0, bufHashed.Length - _HashSize);
            if ((buffer == null) || (buffer.Length != _HashSize))
            {
                return false;
            }
            int num = bufHashed.Length - _HashSize;
            bool flag = false;
            for (int i = 0; i < _HashSize; i++)
            {
                if (buffer[i] != bufHashed[num + i])
                {
                    flag = true;
                }
            }
            return !flag;
        }

        internal bool AutogenKey
        {
            get
            {
                this.RuntimeDataInitialize();
                return this._AutogenKey;
            }
        }

        [ConfigurationProperty("compatibilityMode", DefaultValue=0)]
        public MachineKeyCompatibilityMode CompatibilityMode
        {
            get
            {
                return (MachineKeyCompatibilityMode) base[_propCompatibilityMode];
            }
            set
            {
                base[_propCompatibilityMode] = value;
            }
        }

        internal static MachineKeyCompatibilityMode CompatMode
        {
            get
            {
                EnsureConfig();
                return s_compatMode;
            }
        }

        [ConfigurationProperty("decryption", DefaultValue="Auto"), TypeConverter(typeof(WhiteSpaceTrimStringConverter)), StringValidator(MinLength=1)]
        public string Decryption
        {
            get
            {
                string str = base[_propDecryption] as string;
                if (str == null)
                {
                    return "Auto";
                }
                if ((((str != "Auto") && (str != "AES")) && ((str != "3DES") && (str != "DES"))) && !str.StartsWith("alg:", StringComparison.Ordinal))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Wrong_decryption_enum"), base.ElementInformation.Properties["decryption"].Source, base.ElementInformation.Properties["decryption"].LineNumber);
                }
                return str;
            }
            set
            {
                if ((((value != "AES") && (value != "3DES")) && ((value != "Auto") && (value != "DES"))) && !value.StartsWith("alg:", StringComparison.Ordinal))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Wrong_decryption_enum"), base.ElementInformation.Properties["decryption"].Source, base.ElementInformation.Properties["decryption"].LineNumber);
                }
                base[_propDecryption] = value;
            }
        }

        [ConfigurationProperty("decryptionKey", DefaultValue="AutoGenerate,IsolateApps"), StringValidator(MinLength=1), TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        public string DecryptionKey
        {
            get
            {
                return (string) base[_propDecryptionKey];
            }
            set
            {
                base[_propDecryptionKey] = value;
            }
        }

        internal byte[] DecryptionKeyInternal
        {
            get
            {
                this.RuntimeDataInitialize();
                return (byte[]) this._DecryptionKey.Clone();
            }
        }

        internal static int HashSize
        {
            get
            {
                s_config.RuntimeDataInitialize();
                return _HashSize;
            }
        }

        internal static bool IsDecryptionKeyAutogenerated
        {
            get
            {
                EnsureConfig();
                return s_config.AutogenKey;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        private static RNGCryptoServiceProvider RandomNumberGenerator
        {
            get
            {
                if (s_randomNumberGenerator == null)
                {
                    s_randomNumberGenerator = new RNGCryptoServiceProvider();
                }
                return s_randomNumberGenerator;
            }
        }

        internal static bool UsingCustomEncryption
        {
            get
            {
                EnsureConfig();
                return _UsingCustomEncryption;
            }
        }

        public MachineKeyValidation Validation
        {
            get
            {
                if (!this._validationIsCached)
                {
                    this.CacheValidation();
                }
                return this._cachedValidationEnum;
            }
            set
            {
                if (!this._validationIsCached || (value != this._cachedValidationEnum))
                {
                    this._cachedValidation = MachineKeyValidationConverter.ConvertFromEnum(value);
                    this._cachedValidationEnum = value;
                    base[_propValidation] = this._cachedValidation;
                    this._validationIsCached = true;
                }
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("validation", DefaultValue="HMACSHA256"), TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        public string ValidationAlgorithm
        {
            get
            {
                if (!this._validationIsCached)
                {
                    this.CacheValidation();
                }
                return this._cachedValidation;
            }
            set
            {
                if (!this._validationIsCached || (value != this._cachedValidation))
                {
                    if (value == null)
                    {
                        value = "HMACSHA256";
                    }
                    this._cachedValidationEnum = MachineKeyValidationConverter.ConvertToEnum(value);
                    this._cachedValidation = value;
                    base[_propValidation] = value;
                    this._validationIsCached = true;
                }
            }
        }

        [ConfigurationProperty("validationKey", DefaultValue="AutoGenerate,IsolateApps"), StringValidator(MinLength=1), TypeConverter(typeof(WhiteSpaceTrimStringConverter))]
        public string ValidationKey
        {
            get
            {
                return (string) base[_propValidationKey];
            }
            set
            {
                base[_propValidationKey] = value;
            }
        }

        internal byte[] ValidationKeyInternal
        {
            get
            {
                this.RuntimeDataInitialize();
                return (byte[]) this._ValidationKey.Clone();
            }
        }

        internal static int ValidationKeySize
        {
            get
            {
                s_config.RuntimeDataInitialize();
                return _AutoGenValidationKeySize;
            }
        }
    }
}

