namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security;
    using System.Security.Cryptography;

    internal static class SymmetricKeyWrap
    {
        private static readonly byte[] s_rgbAES_KW_IV = new byte[] { 0xa6, 0xa6, 0xa6, 0xa6, 0xa6, 0xa6, 0xa6, 0xa6 };
        private static readonly byte[] s_rgbTripleDES_KW_IV = new byte[] { 0x4a, 0xdd, 0xa2, 0x2c, 0x79, 0xe8, 0x21, 5 };

        internal static byte[] AESKeyWrapDecrypt(byte[] rgbKey, byte[] rgbEncryptedWrappedKeyData)
        {
            int num = (rgbEncryptedWrappedKeyData.Length >> 3) - 1;
            if (((rgbEncryptedWrappedKeyData.Length % 8) != 0) || (num <= 0))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_KW_BadKeySize"));
            }
            byte[] dst = new byte[num << 3];
            ICryptoTransform transform = new RijndaelManaged { Key = rgbKey, Mode = CipherMode.ECB, Padding = PaddingMode.None }.CreateDecryptor();
            if (num == 1)
            {
                byte[] src = transform.TransformFinalBlock(rgbEncryptedWrappedKeyData, 0, rgbEncryptedWrappedKeyData.Length);
                for (int k = 0; k < 8; k++)
                {
                    if (src[k] != s_rgbAES_KW_IV[k])
                    {
                        throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_BadWrappedKeySize"));
                    }
                }
                Buffer.BlockCopy(src, 8, dst, 0, 8);
                return dst;
            }
            long num3 = 0L;
            Buffer.BlockCopy(rgbEncryptedWrappedKeyData, 8, dst, 0, dst.Length);
            byte[] buffer3 = new byte[8];
            byte[] buffer4 = new byte[0x10];
            Buffer.BlockCopy(rgbEncryptedWrappedKeyData, 0, buffer3, 0, 8);
            for (int i = 5; i >= 0; i--)
            {
                for (int m = num; m >= 1; m--)
                {
                    num3 = m + (i * num);
                    for (int n = 0; n < 8; n++)
                    {
                        byte num7 = (byte) ((num3 >> (8 * (7 - n))) & 0xffL);
                        buffer3[n] = (byte) (buffer3[n] ^ num7);
                    }
                    Buffer.BlockCopy(buffer3, 0, buffer4, 0, 8);
                    Buffer.BlockCopy(dst, 8 * (m - 1), buffer4, 8, 8);
                    byte[] buffer5 = transform.TransformFinalBlock(buffer4, 0, 0x10);
                    Buffer.BlockCopy(buffer5, 8, dst, 8 * (m - 1), 8);
                    Buffer.BlockCopy(buffer5, 0, buffer3, 0, 8);
                }
            }
            for (int j = 0; j < 8; j++)
            {
                if (buffer3[j] != s_rgbAES_KW_IV[j])
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_BadWrappedKeySize"));
                }
            }
            return dst;
        }

        internal static byte[] AESKeyWrapEncrypt(byte[] rgbKey, byte[] rgbWrappedKeyData)
        {
            int num = rgbWrappedKeyData.Length >> 3;
            if (((rgbWrappedKeyData.Length % 8) != 0) || (num <= 0))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_KW_BadKeySize"));
            }
            ICryptoTransform transform = new RijndaelManaged { Key = rgbKey, Mode = CipherMode.ECB, Padding = PaddingMode.None }.CreateEncryptor();
            if (num == 1)
            {
                byte[] buffer = new byte[s_rgbAES_KW_IV.Length + rgbWrappedKeyData.Length];
                Buffer.BlockCopy(s_rgbAES_KW_IV, 0, buffer, 0, s_rgbAES_KW_IV.Length);
                Buffer.BlockCopy(rgbWrappedKeyData, 0, buffer, s_rgbAES_KW_IV.Length, rgbWrappedKeyData.Length);
                return transform.TransformFinalBlock(buffer, 0, buffer.Length);
            }
            long num2 = 0L;
            byte[] dst = new byte[(num + 1) << 3];
            Buffer.BlockCopy(rgbWrappedKeyData, 0, dst, 8, rgbWrappedKeyData.Length);
            byte[] buffer3 = new byte[8];
            byte[] buffer4 = new byte[0x10];
            Buffer.BlockCopy(s_rgbAES_KW_IV, 0, buffer3, 0, 8);
            for (int i = 0; i <= 5; i++)
            {
                for (int j = 1; j <= num; j++)
                {
                    num2 = j + (i * num);
                    Buffer.BlockCopy(buffer3, 0, buffer4, 0, 8);
                    Buffer.BlockCopy(dst, 8 * j, buffer4, 8, 8);
                    byte[] src = transform.TransformFinalBlock(buffer4, 0, 0x10);
                    for (int k = 0; k < 8; k++)
                    {
                        byte num6 = (byte) ((num2 >> (8 * (7 - k))) & 0xffL);
                        buffer3[k] = (byte) (num6 ^ src[k]);
                    }
                    Buffer.BlockCopy(src, 8, dst, 8 * j, 8);
                }
            }
            Buffer.BlockCopy(buffer3, 0, dst, 0, 8);
            return dst;
        }

        internal static byte[] TripleDESKeyWrapDecrypt(byte[] rgbKey, byte[] rgbEncryptedWrappedKeyData)
        {
            if (((rgbEncryptedWrappedKeyData.Length != 0x20) && (rgbEncryptedWrappedKeyData.Length != 40)) && (rgbEncryptedWrappedKeyData.Length != 0x30))
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_KW_BadKeySize"));
            }
            TripleDESCryptoServiceProvider provider = new TripleDESCryptoServiceProvider {
                Padding = PaddingMode.None
            };
            byte[] array = provider.CreateDecryptor(rgbKey, s_rgbTripleDES_KW_IV).TransformFinalBlock(rgbEncryptedWrappedKeyData, 0, rgbEncryptedWrappedKeyData.Length);
            Array.Reverse(array);
            byte[] dst = new byte[8];
            Buffer.BlockCopy(array, 0, dst, 0, 8);
            byte[] buffer3 = new byte[array.Length - dst.Length];
            Buffer.BlockCopy(array, 8, buffer3, 0, buffer3.Length);
            byte[] src = provider.CreateDecryptor(rgbKey, dst).TransformFinalBlock(buffer3, 0, buffer3.Length);
            byte[] buffer5 = new byte[src.Length - 8];
            Buffer.BlockCopy(src, 0, buffer5, 0, buffer5.Length);
            byte[] buffer6 = new SHA1CryptoServiceProvider().ComputeHash(buffer5);
            int length = buffer5.Length;
            for (int i = 0; length < src.Length; i++)
            {
                if (src[length] != buffer6[i])
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_BadWrappedKeySize"));
                }
                length++;
            }
            return buffer5;
        }

        internal static byte[] TripleDESKeyWrapEncrypt(byte[] rgbKey, byte[] rgbWrappedKeyData)
        {
            byte[] src = new SHA1CryptoServiceProvider().ComputeHash(rgbWrappedKeyData);
            RNGCryptoServiceProvider provider2 = new RNGCryptoServiceProvider();
            byte[] data = new byte[8];
            provider2.GetBytes(data);
            byte[] dst = new byte[rgbWrappedKeyData.Length + 8];
            TripleDESCryptoServiceProvider provider3 = new TripleDESCryptoServiceProvider {
                Padding = PaddingMode.None
            };
            ICryptoTransform transform = provider3.CreateEncryptor(rgbKey, data);
            Buffer.BlockCopy(rgbWrappedKeyData, 0, dst, 0, rgbWrappedKeyData.Length);
            Buffer.BlockCopy(src, 0, dst, rgbWrappedKeyData.Length, 8);
            byte[] buffer4 = transform.TransformFinalBlock(dst, 0, dst.Length);
            byte[] buffer5 = new byte[data.Length + buffer4.Length];
            Buffer.BlockCopy(data, 0, buffer5, 0, data.Length);
            Buffer.BlockCopy(buffer4, 0, buffer5, data.Length, buffer4.Length);
            Array.Reverse(buffer5);
            return provider3.CreateEncryptor(rgbKey, s_rgbTripleDES_KW_IV).TransformFinalBlock(buffer5, 0, buffer5.Length);
        }
    }
}

