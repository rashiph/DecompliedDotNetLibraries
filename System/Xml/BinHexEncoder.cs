namespace System.Xml
{
    using System;

    internal static class BinHexEncoder
    {
        private const int CharsChunkSize = 0x80;
        private const string s_hexDigits = "0123456789ABCDEF";

        internal static string Encode(byte[] inArray, int offsetIn, int count)
        {
            if (inArray == null)
            {
                throw new ArgumentNullException("inArray");
            }
            if (0 > offsetIn)
            {
                throw new ArgumentOutOfRangeException("offsetIn");
            }
            if (0 > count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (count > (inArray.Length - offsetIn))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            char[] chArray = new char[2 * count];
            return new string(chArray, 0, Encode(inArray, offsetIn, count, chArray));
        }

        private static int Encode(byte[] inArray, int offsetIn, int count, char[] outArray)
        {
            int num = 0;
            int num2 = 0;
            int length = outArray.Length;
            for (int i = 0; i < count; i++)
            {
                byte num3 = inArray[offsetIn++];
                outArray[num++] = "0123456789ABCDEF"[num3 >> 4];
                if (num == length)
                {
                    break;
                }
                outArray[num++] = "0123456789ABCDEF"[num3 & 15];
                if (num == length)
                {
                    break;
                }
            }
            return (num - num2);
        }

        internal static void Encode(byte[] buffer, int index, int count, XmlWriter writer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (count > (buffer.Length - index))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            char[] outArray = new char[((count * 2) < 0x80) ? (count * 2) : 0x80];
            int num = index + count;
            while (index < num)
            {
                int num2 = (count < 0x40) ? count : 0x40;
                int num3 = Encode(buffer, index, num2, outArray);
                writer.WriteRaw(outArray, 0, num3);
                index += num2;
                count -= num2;
            }
        }
    }
}

