namespace System.Data.Design
{
    using System;

    internal class ConversionHelper
    {
        private static short[] urtConversionTable = new short[] { 0x5ffd, 0x3fe1, 0x7ffd, 0x7ffd, 0x7ffd, 0x7ffd, 0x7ffd, 0x7ffd, 0x7ffd, 0x7ffd, 0x5ffd, 0x5ffd, 0x5ffd, 3, 0x7fff };
        private static short[] urtSafeConversionTable = new short[] { 0x5ffd, 0x3fe1, 0x7ffd, 0x7ffd, 0x7ffd, 0x7ffd, 0x7ffd, 0x7ffd, 0x7ffd, 0x7ffd, 0x5ffd, 0x5ffd, 0x5ffd, 3, 1 };
        private static Type[] urtTypeIndexTable = new Type[] { typeof(bool), typeof(char), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(DateTime), typeof(string) };

        private ConversionHelper()
        {
        }

        internal static bool CanConvert(Type sourceUrtType, Type destinationUrtType)
        {
            short index = -1;
            short num2 = -1;
            for (short i = 0; i < urtTypeIndexTable.Length; i = (short) (i + 1))
            {
                if (sourceUrtType == urtTypeIndexTable[i])
                {
                    index = i;
                    break;
                }
            }
            for (short j = 0; j < urtTypeIndexTable.Length; j = (short) (j + 1))
            {
                if (destinationUrtType == urtTypeIndexTable[j])
                {
                    num2 = j;
                    break;
                }
            }
            if ((index != -1) && (num2 != -1))
            {
                short num5 = urtConversionTable[index];
                short num6 = (short) (((int) 0x4000) >> num2);
                if ((num5 & num6) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal static string GetConversionMethodName(Type sourceUrtType, Type targetUrtType)
        {
            return ("To" + targetUrtType.Name);
        }
    }
}

