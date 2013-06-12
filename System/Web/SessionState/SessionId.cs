namespace System.Web.SessionState
{
    using System;
    using System.Security.Cryptography;

    internal static class SessionId
    {
        internal const int ENCODING_BITS_PER_CHAR = 5;
        internal const int ID_LENGTH_BITS = 120;
        internal const int ID_LENGTH_BYTES = 15;
        internal const int ID_LENGTH_CHARS = 0x18;
        internal const int NUM_CHARS_IN_ENCODING = 0x20;
        private static char[] s_encoding = new char[] { 
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5'
         };
        private static bool[] s_legalchars = new bool[0x80];

        static SessionId()
        {
            for (int i = s_encoding.Length - 1; i >= 0; i--)
            {
                char index = s_encoding[i];
                s_legalchars[index] = true;
            }
        }

        internal static string Create(ref RandomNumberGenerator randgen)
        {
            if (randgen == null)
            {
                randgen = new RNGCryptoServiceProvider();
            }
            byte[] data = new byte[15];
            randgen.GetBytes(data);
            return Encode(data);
        }

        private static string Encode(byte[] buffer)
        {
            char[] chArray = new char[0x18];
            int num2 = 0;
            for (int i = 0; i < 15; i += 5)
            {
                int num4 = ((buffer[i] | (buffer[i + 1] << 8)) | (buffer[i + 2] << 0x10)) | (buffer[i + 3] << 0x18);
                int index = num4 & 0x1f;
                chArray[num2++] = s_encoding[index];
                index = (num4 >> 5) & 0x1f;
                chArray[num2++] = s_encoding[index];
                index = (num4 >> 10) & 0x1f;
                chArray[num2++] = s_encoding[index];
                index = (num4 >> 15) & 0x1f;
                chArray[num2++] = s_encoding[index];
                index = (num4 >> 20) & 0x1f;
                chArray[num2++] = s_encoding[index];
                index = (num4 >> 0x19) & 0x1f;
                chArray[num2++] = s_encoding[index];
                num4 = ((num4 >> 30) & 3) | (buffer[i + 4] << 2);
                index = num4 & 0x1f;
                chArray[num2++] = s_encoding[index];
                index = (num4 >> 5) & 0x1f;
                chArray[num2++] = s_encoding[index];
            }
            return new string(chArray);
        }

        internal static bool IsLegit(string s)
        {
            if ((s == null) || (s.Length != 0x18))
            {
                return false;
            }
            try
            {
                int num = 0x18;
                while (--num >= 0)
                {
                    char index = s[num];
                    if (!s_legalchars[index])
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }
    }
}

