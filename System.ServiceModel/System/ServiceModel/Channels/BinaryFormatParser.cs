namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal static class BinaryFormatParser
    {
        public static int GetSessionKey(int value)
        {
            return (value / 2);
        }

        public static int GetStaticKey(int value)
        {
            return (value / 2);
        }

        public static bool IsSessionKey(int value)
        {
            return ((value & 1) != 0);
        }

        public static bool MatchAttributeNode(byte[] buffer, int offset, int size)
        {
            if (size < 1)
            {
                return false;
            }
            System.Xml.XmlBinaryNodeType type = (System.Xml.XmlBinaryNodeType) buffer[offset];
            return ((type >= System.Xml.XmlBinaryNodeType.MinAttribute) && (type <= System.Xml.XmlBinaryNodeType.DictionaryAttribute));
        }

        public static int MatchBytes(byte[] buffer, int offset, int size, byte[] buffer2)
        {
            if (size < buffer2.Length)
            {
                return 0;
            }
            int index = offset;
            int num2 = 0;
            while (num2 < buffer2.Length)
            {
                if (buffer2[num2] != buffer[index])
                {
                    return 0;
                }
                num2++;
                index++;
            }
            return buffer2.Length;
        }

        public static int MatchInt32(byte[] buffer, int offset, int size)
        {
            if ((size > 0) && ((buffer[offset] & 0x80) == 0))
            {
                return 1;
            }
            if ((size > 1) && ((buffer[offset + 1] & 0x80) == 0))
            {
                return 2;
            }
            if ((size > 2) && ((buffer[offset + 2] & 0x80) == 0))
            {
                return 3;
            }
            if ((size > 3) && ((buffer[offset + 3] & 0x80) == 0))
            {
                return 4;
            }
            return 0;
        }

        public static int MatchKey(byte[] buffer, int offset, int size)
        {
            return MatchInt32(buffer, offset, size);
        }

        public static int MatchUniqueID(byte[] buffer, int offset, int size)
        {
            if (size < 0x10)
            {
                return 0;
            }
            return 0x10;
        }

        public static int ParseInt32(byte[] buffer, int offset, int size)
        {
            switch (size)
            {
                case 1:
                    return buffer[offset];

                case 2:
                    return ((buffer[offset] & 0x7f) + (buffer[offset + 1] << 7));

                case 3:
                    return (((buffer[offset] & 0x7f) + ((buffer[offset + 1] & 0x7f) << 7)) + (buffer[offset + 2] << 14));

                case 4:
                    return ((((buffer[offset] & 0x7f) + ((buffer[offset + 1] & 0x7f) << 7)) + ((buffer[offset + 2] & 0x7f) << 14)) + (buffer[offset + 3] << 0x15));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("size", size, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 1, 4 })));
        }

        public static int ParseKey(byte[] buffer, int offset, int size)
        {
            return ParseInt32(buffer, offset, size);
        }

        public static UniqueId ParseUniqueID(byte[] buffer, int offset, int size)
        {
            return new UniqueId(buffer, offset);
        }
    }
}

