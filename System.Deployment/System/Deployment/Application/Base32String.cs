namespace System.Deployment.Application
{
    using System;
    using System.Text;

    internal class Base32String
    {
        protected static char[] charList = new char[] { 
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'G', 
            'H', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'T', 'V', 'W', 'X', 'Y', 'Z'
         };

        public static string FromBytes(byte[] bytes)
        {
            int length = bytes.Length;
            if (length <= 0)
            {
                return null;
            }
            int num2 = length << 3;
            int num3 = (num2 / 5) << 3;
            if ((num2 % 5) != 0)
            {
                num3 += 8;
            }
            int capacity = num3 >> 3;
            StringBuilder builder = new StringBuilder(capacity);
            int index = 0;
            int num6 = 0;
            int num7 = 0;
            for (index = 0; index < length; index++)
            {
                num7 = (num7 << 8) | bytes[index];
                num6 += 8;
                while (num6 >= 5)
                {
                    num6 -= 5;
                    builder.Append(charList[(num7 >> num6) & 0x1f]);
                }
            }
            if (num6 > 0)
            {
                builder.Append(charList[(num7 << (5 - num6)) & 0x1f]);
            }
            return builder.ToString();
        }
    }
}

