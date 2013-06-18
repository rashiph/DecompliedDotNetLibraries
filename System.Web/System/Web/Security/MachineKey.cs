namespace System.Web.Security
{
    using System;
    using System.Web.Configuration;
    using System.Web.Util;

    public static class MachineKey
    {
        public static byte[] Decode(string encodedData, MachineKeyProtection protectionOption)
        {
            if (encodedData == null)
            {
                throw new ArgumentNullException("encodedData");
            }
            if ((encodedData.Length % 2) != 0)
            {
                throw new ArgumentException(null, "encodedData");
            }
            byte[] buf = null;
            try
            {
                buf = MachineKeySection.HexStringToByteArray(encodedData);
            }
            catch
            {
                throw new ArgumentException(null, "encodedData");
            }
            if ((buf == null) || (buf.Length < 1))
            {
                throw new ArgumentException(null, "encodedData");
            }
            if ((protectionOption == MachineKeyProtection.All) || (protectionOption == MachineKeyProtection.Encryption))
            {
                buf = MachineKeySection.EncryptOrDecryptData(false, buf, null, 0, buf.Length, false, false, IVType.Random, !AppSettings.UseLegacyMachineKeyEncryption);
                if (buf == null)
                {
                    return null;
                }
            }
            if ((protectionOption == MachineKeyProtection.All) || (protectionOption == MachineKeyProtection.Validation))
            {
                if (buf.Length < MachineKeySection.HashSize)
                {
                    return null;
                }
                byte[] src = buf;
                buf = new byte[src.Length - MachineKeySection.HashSize];
                Buffer.BlockCopy(src, 0, buf, 0, buf.Length);
                byte[] buffer3 = MachineKeySection.HashData(buf, null, 0, buf.Length);
                if ((buffer3 == null) || (buffer3.Length != MachineKeySection.HashSize))
                {
                    return null;
                }
                for (int i = 0; i < buffer3.Length; i++)
                {
                    if (buffer3[i] != src[buf.Length + i])
                    {
                        return null;
                    }
                }
            }
            return buf;
        }

        public static string Encode(byte[] data, MachineKeyProtection protectionOption)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if ((protectionOption == MachineKeyProtection.All) || (protectionOption == MachineKeyProtection.Validation))
            {
                byte[] src = MachineKeySection.HashData(data, null, 0, data.Length);
                byte[] dst = new byte[src.Length + data.Length];
                Buffer.BlockCopy(data, 0, dst, 0, data.Length);
                Buffer.BlockCopy(src, 0, dst, data.Length, src.Length);
                data = dst;
            }
            if ((protectionOption == MachineKeyProtection.All) || (protectionOption == MachineKeyProtection.Encryption))
            {
                data = MachineKeySection.EncryptOrDecryptData(true, data, null, 0, data.Length, false, false, IVType.Random, !AppSettings.UseLegacyMachineKeyEncryption);
            }
            return MachineKeySection.ByteArrayToHexString(data, 0);
        }
    }
}

