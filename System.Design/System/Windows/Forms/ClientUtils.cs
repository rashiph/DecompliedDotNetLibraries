namespace System.Windows.Forms
{
    using System;
    using System.Security;
    using System.Threading;

    internal static class ClientUtils
    {
        public static int GetBitCount(uint x)
        {
            int num = 0;
            while (x > 0)
            {
                x &= x - 1;
                num++;
            }
            return num;
        }

        public static bool IsCriticalException(Exception ex)
        {
            return (((((ex is NullReferenceException) || (ex is StackOverflowException)) || ((ex is OutOfMemoryException) || (ex is ThreadAbortException))) || ((ex is ExecutionEngineException) || (ex is IndexOutOfRangeException))) || (ex is AccessViolationException));
        }

        public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue)
        {
            return ((value >= minValue) && (value <= maxValue));
        }

        public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue, int maxNumberOfBitsOn)
        {
            return (((value >= minValue) && (value <= maxValue)) && (GetBitCount((uint) value) <= maxNumberOfBitsOn));
        }

        public static bool IsEnumValid_Masked(Enum enumValue, int value, uint mask)
        {
            return ((value & mask) == value);
        }

        public static bool IsEnumValid_NotSequential(Enum enumValue, int value, params int[] enumValues)
        {
            for (int i = 0; i < enumValues.Length; i++)
            {
                if (enumValues[i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsSecurityOrCriticalException(Exception ex)
        {
            return ((ex is SecurityException) || IsCriticalException(ex));
        }
    }
}

