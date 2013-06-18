using System;
using System.Collections.Generic;
using System.Globalization;

internal class DigestComparer : IComparer<byte[]>, IEqualityComparer<byte[]>
{
    internal static string GetMD5DigestString(byte[] md5Digest)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}", new object[] { md5Digest[0].ToString("X2", CultureInfo.InvariantCulture), md5Digest[1].ToString("X2", CultureInfo.InvariantCulture), md5Digest[2].ToString("X2", CultureInfo.InvariantCulture), md5Digest[3].ToString("X2", CultureInfo.InvariantCulture), md5Digest[4].ToString("X2", CultureInfo.InvariantCulture), md5Digest[5].ToString("X2", CultureInfo.InvariantCulture), md5Digest[6].ToString("X2", CultureInfo.InvariantCulture), md5Digest[7].ToString("X2", CultureInfo.InvariantCulture), md5Digest[8].ToString("X2", CultureInfo.InvariantCulture), md5Digest[9].ToString("X2", CultureInfo.InvariantCulture), md5Digest[10].ToString("X2", CultureInfo.InvariantCulture), md5Digest[11].ToString("X2", CultureInfo.InvariantCulture), md5Digest[12].ToString("X2", CultureInfo.InvariantCulture), md5Digest[13].ToString("X2", CultureInfo.InvariantCulture), md5Digest[14].ToString("X2", CultureInfo.InvariantCulture), md5Digest[15].ToString("X2", CultureInfo.InvariantCulture) });
    }

    int IComparer<byte[]>.Compare(byte[] digest1, byte[] digest2)
    {
        for (int i = 0; i < 0x10; i++)
        {
            if (digest1[i] != digest2[i])
            {
                if (digest1[i] >= digest2[i])
                {
                    return 1;
                }
                return -1;
            }
        }
        return 0;
    }

    bool IEqualityComparer<byte[]>.Equals(byte[] digest1, byte[] digest2)
    {
        for (int i = 0; i < 0x10; i++)
        {
            if (digest1[i] != digest2[i])
            {
                return false;
            }
        }
        return true;
    }

    int IEqualityComparer<byte[]>.GetHashCode(byte[] checksumBytes)
    {
        return GetMD5DigestString(checksumBytes).GetHashCode();
    }
}

