namespace System.IdentityModel
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class Asn1IntegerConverter
    {
        private static readonly char[] digitMap = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private static List<byte[]> powersOfTwo = new List<byte[]>(new byte[][] { new byte[] { 1 } });

        private static void AddSecondDecimalToFirst(List<byte> first, byte[] second)
        {
            byte item = 0;
            for (int i = 0; (i < second.Length) || (i < first.Count); i++)
            {
                byte num3;
                if (i >= first.Count)
                {
                    first.Add(0);
                }
                if (i < second.Length)
                {
                    num3 = (first[i] + second[i]) + item;
                }
                else
                {
                    num3 = first[i] + item;
                }
                first[i] = (byte) (num3 % 10);
                item = (byte) (num3 / 10);
            }
            if (item > 0)
            {
                first.Add(item);
            }
        }

        public static string Asn1IntegerToDecimalString(byte[] asn1)
        {
            byte num2;
            if (asn1 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("asn1");
            }
            if (asn1.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("asn1", System.IdentityModel.SR.GetString("LengthOfArrayToConvertMustGreaterThanZero")));
            }
            List<byte> first = new List<byte>((asn1.Length * 8) / 3);
            int n = 0;
            for (int i = 0; i < (asn1.Length - 1); i++)
            {
                num2 = asn1[i];
                for (int k = 0; k < 8; k++)
                {
                    if ((num2 & 1) == 1)
                    {
                        AddSecondDecimalToFirst(first, TwoToThePowerOf(n));
                    }
                    n++;
                    num2 = (byte) (num2 >> 1);
                }
            }
            num2 = asn1[asn1.Length - 1];
            for (int j = 0; j < 7; j++)
            {
                if ((num2 & 1) == 1)
                {
                    AddSecondDecimalToFirst(first, TwoToThePowerOf(n));
                }
                n++;
                num2 = (byte) (num2 >> 1);
            }
            StringBuilder builder = new StringBuilder(first.Count + 1);
            List<byte> list2 = null;
            if (num2 == 0)
            {
                list2 = first;
            }
            else
            {
                List<byte> list3 = new List<byte>(TwoToThePowerOf(n));
                SubtractSecondDecimalFromFirst(list3, first);
                list2 = list3;
                builder.Append('-');
            }
            int num6 = list2.Count - 1;
            while (num6 >= 0)
            {
                if (list2[num6] != 0)
                {
                    break;
                }
                num6--;
            }
            if ((num6 >= 0) || (asn1.Length <= 0))
            {
                while (num6 >= 0)
                {
                    builder.Append(digitMap[list2[num6--]]);
                }
            }
            else
            {
                builder.Append(digitMap[0]);
            }
            return builder.ToString();
        }

        private static void SubtractSecondDecimalFromFirst(List<byte> first, List<byte> second)
        {
            byte num = 0;
            for (int i = 0; i < second.Count; i++)
            {
                int num3 = (first[i] - second[i]) - num;
                if (num3 < 0)
                {
                    num = 1;
                    first[i] = (byte) (num3 + 10);
                }
                else
                {
                    num = 0;
                    first[i] = (byte) num3;
                }
            }
            if (num > 0)
            {
                for (int j = second.Count; j < first.Count; j++)
                {
                    int num5 = first[j] - num;
                    if (num5 < 0)
                    {
                        num = 1;
                        first[j] = (byte) (num5 + 10);
                    }
                    else
                    {
                        num = 0;
                        first[j] = (byte) num5;
                        return;
                    }
                }
            }
        }

        private static byte[] TwoToThePowerOf(int n)
        {
            lock (powersOfTwo)
            {
                if (n >= powersOfTwo.Count)
                {
                    for (int i = powersOfTwo.Count; i <= n; i++)
                    {
                        List<byte> list = new List<byte>(powersOfTwo[i - 1]);
                        byte item = 0;
                        for (int j = 0; j < list.Count; j++)
                        {
                            byte num4 = (list[j] << 1) + item;
                            list[j] = (byte) (num4 % 10);
                            item = (byte) (num4 / 10);
                        }
                        if (item > 0)
                        {
                            list.Add(item);
                            item = 0;
                        }
                        powersOfTwo.Add(list.ToArray());
                    }
                }
                return powersOfTwo[n];
            }
        }
    }
}

