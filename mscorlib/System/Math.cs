namespace System
{
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    public static class Math
    {
        private static double doubleRoundLimit = 1E+16;
        public const double E = 2.7182818284590451;
        private const int maxRoundingDigits = 15;
        public const double PI = 3.1415926535897931;
        private static double[] roundPower10Double = new double[] { 1.0, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, 10000000.0, 100000000.0, 1000000000.0, 10000000000, 100000000000, 1000000000000, 10000000000000, 100000000000000, 1E+15 };

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static decimal Abs(decimal value)
        {
            return decimal.Abs(value);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Abs(double value);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static short Abs(short value)
        {
            if (value >= 0)
            {
                return value;
            }
            return AbsHelper(value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Abs(int value)
        {
            if (value >= 0)
            {
                return value;
            }
            return AbsHelper(value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static long Abs(long value)
        {
            if (value >= 0L)
            {
                return value;
            }
            return AbsHelper(value);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), CLSCompliant(false)]
        public static sbyte Abs(sbyte value)
        {
            if (value >= 0)
            {
                return value;
            }
            return AbsHelper(value);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern float Abs(float value);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private static short AbsHelper(short value)
        {
            if (value == -32768)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_NegateTwosCompNum"));
            }
            return -value;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private static int AbsHelper(int value)
        {
            if (value == -2147483648)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_NegateTwosCompNum"));
            }
            return -value;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private static long AbsHelper(long value)
        {
            if (value == -9223372036854775808L)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_NegateTwosCompNum"));
            }
            return -value;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private static sbyte AbsHelper(sbyte value)
        {
            if (value == -128)
            {
                throw new OverflowException(Environment.GetResourceString("Overflow_NegateTwosCompNum"));
            }
            return -value;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Acos(double d);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Asin(double d);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Atan(double d);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Atan2(double y, double x);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static long BigMul(int a, int b)
        {
            return (a * b);
        }

        public static decimal Ceiling(decimal d)
        {
            return decimal.Ceiling(d);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Ceiling(double a);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Cos(double d);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Cosh(double value);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int DivRem(int a, int b, out int result)
        {
            result = a % b;
            return (a / b);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static long DivRem(long a, long b, out long result)
        {
            result = a % b;
            return (a / b);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Exp(double d);
        public static decimal Floor(decimal d)
        {
            return decimal.Floor(d);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Floor(double d);
        [SecuritySafeCritical]
        public static double IEEERemainder(double x, double y)
        {
            if (double.IsNaN(x))
            {
                return x;
            }
            if (double.IsNaN(y))
            {
                return y;
            }
            double d = x % y;
            if (double.IsNaN(d))
            {
                return double.NaN;
            }
            if ((d == 0.0) && double.IsNegative(x))
            {
                return double.NegativeZero;
            }
            double num2 = d - (Abs(y) * Sign(x));
            if (Abs(num2) == Abs(d))
            {
                double a = x / y;
                if (Abs(Round(a)) > Abs(a))
                {
                    return num2;
                }
                return d;
            }
            if (Abs(num2) < Abs(d))
            {
                return num2;
            }
            return d;
        }

        [SecuritySafeCritical]
        private static unsafe double InternalRound(double value, int digits, MidpointRounding mode)
        {
            if (Abs(value) < doubleRoundLimit)
            {
                double num = roundPower10Double[digits];
                value *= num;
                if (mode == MidpointRounding.AwayFromZero)
                {
                    double num2 = SplitFractionDouble(&value);
                    if (Abs(num2) >= 0.5)
                    {
                        value += Sign(num2);
                    }
                }
                else
                {
                    value = Round(value);
                }
                value /= num;
            }
            return value;
        }

        [SecuritySafeCritical]
        private static unsafe double InternalTruncate(double d)
        {
            SplitFractionDouble(&d);
            return d;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Log(double d);
        public static double Log(double a, double newBase)
        {
            if (double.IsNaN(a))
            {
                return a;
            }
            if (double.IsNaN(newBase))
            {
                return newBase;
            }
            if ((newBase != 1.0) && ((a == 1.0) || ((newBase != 0.0) && !double.IsPositiveInfinity(newBase))))
            {
                return (Log(a) / Log(newBase));
            }
            return double.NaN;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Log10(double d);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static byte Max(byte val1, byte val2)
        {
            if (val1 < val2)
            {
                return val2;
            }
            return val1;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static decimal Max(decimal val1, decimal val2)
        {
            return decimal.Max(val1, val2);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static double Max(double val1, double val2)
        {
            if (val1 > val2)
            {
                return val1;
            }
            if (double.IsNaN(val1))
            {
                return val1;
            }
            return val2;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static short Max(short val1, short val2)
        {
            if (val1 < val2)
            {
                return val2;
            }
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Max(int val1, int val2)
        {
            if (val1 < val2)
            {
                return val2;
            }
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static long Max(long val1, long val2)
        {
            if (val1 < val2)
            {
                return val2;
            }
            return val1;
        }

        [CLSCompliant(false), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static sbyte Max(sbyte val1, sbyte val2)
        {
            if (val1 < val2)
            {
                return val2;
            }
            return val1;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static float Max(float val1, float val2)
        {
            if (val1 > val2)
            {
                return val1;
            }
            if (float.IsNaN(val1))
            {
                return val1;
            }
            return val2;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), CLSCompliant(false), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static ushort Max(ushort val1, ushort val2)
        {
            if (val1 < val2)
            {
                return val2;
            }
            return val1;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static uint Max(uint val1, uint val2)
        {
            if (val1 < val2)
            {
                return val2;
            }
            return val1;
        }

        [CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static ulong Max(ulong val1, ulong val2)
        {
            if (val1 < val2)
            {
                return val2;
            }
            return val1;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static byte Min(byte val1, byte val2)
        {
            if (val1 > val2)
            {
                return val2;
            }
            return val1;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static decimal Min(decimal val1, decimal val2)
        {
            return decimal.Min(val1, val2);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static double Min(double val1, double val2)
        {
            if (val1 < val2)
            {
                return val1;
            }
            if (double.IsNaN(val1))
            {
                return val1;
            }
            return val2;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static short Min(short val1, short val2)
        {
            if (val1 > val2)
            {
                return val2;
            }
            return val1;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static int Min(int val1, int val2)
        {
            if (val1 > val2)
            {
                return val2;
            }
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static long Min(long val1, long val2)
        {
            if (val1 > val2)
            {
                return val2;
            }
            return val1;
        }

        [CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static sbyte Min(sbyte val1, sbyte val2)
        {
            if (val1 > val2)
            {
                return val2;
            }
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static float Min(float val1, float val2)
        {
            if (val1 < val2)
            {
                return val1;
            }
            if (float.IsNaN(val1))
            {
                return val1;
            }
            return val2;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), CLSCompliant(false)]
        public static ushort Min(ushort val1, ushort val2)
        {
            if (val1 > val2)
            {
                return val2;
            }
            return val1;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), CLSCompliant(false)]
        public static uint Min(uint val1, uint val2)
        {
            if (val1 > val2)
            {
                return val2;
            }
            return val1;
        }

        [CLSCompliant(false), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static ulong Min(ulong val1, ulong val2)
        {
            if (val1 > val2)
            {
                return val2;
            }
            return val1;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Pow(double x, double y);
        public static decimal Round(decimal d)
        {
            return decimal.Round(d, 0);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Round(double a);
        public static decimal Round(decimal d, int decimals)
        {
            return decimal.Round(d, decimals);
        }

        [SecuritySafeCritical]
        public static decimal Round(decimal d, MidpointRounding mode)
        {
            return decimal.Round(d, 0, mode);
        }

        public static double Round(double value, int digits)
        {
            if ((digits < 0) || (digits > 15))
            {
                throw new ArgumentOutOfRangeException("digits", Environment.GetResourceString("ArgumentOutOfRange_RoundingDigits"));
            }
            return InternalRound(value, digits, MidpointRounding.ToEven);
        }

        public static double Round(double value, MidpointRounding mode)
        {
            return Round(value, 0, mode);
        }

        [SecuritySafeCritical]
        public static decimal Round(decimal d, int decimals, MidpointRounding mode)
        {
            return decimal.Round(d, decimals, mode);
        }

        public static double Round(double value, int digits, MidpointRounding mode)
        {
            if ((digits < 0) || (digits > 15))
            {
                throw new ArgumentOutOfRangeException("digits", Environment.GetResourceString("ArgumentOutOfRange_RoundingDigits"));
            }
            if ((mode < MidpointRounding.ToEven) || (mode > MidpointRounding.AwayFromZero))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidEnumValue", new object[] { mode, "MidpointRounding" }), "mode");
            }
            return InternalRound(value, digits, mode);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Sign(decimal value)
        {
            if (value < 0M)
            {
                return -1;
            }
            if (value > 0M)
            {
                return 1;
            }
            return 0;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Sign(double value)
        {
            if (value < 0.0)
            {
                return -1;
            }
            if (value > 0.0)
            {
                return 1;
            }
            if (value != 0.0)
            {
                throw new ArithmeticException(Environment.GetResourceString("Arithmetic_NaN"));
            }
            return 0;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Sign(short value)
        {
            if (value < 0)
            {
                return -1;
            }
            if (value > 0)
            {
                return 1;
            }
            return 0;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Sign(int value)
        {
            if (value < 0)
            {
                return -1;
            }
            if (value > 0)
            {
                return 1;
            }
            return 0;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Sign(long value)
        {
            if (value < 0L)
            {
                return -1;
            }
            if (value > 0L)
            {
                return 1;
            }
            return 0;
        }

        [CLSCompliant(false), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Sign(sbyte value)
        {
            if (value < 0)
            {
                return -1;
            }
            if (value > 0)
            {
                return 1;
            }
            return 0;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Sign(float value)
        {
            if (value < 0f)
            {
                return -1;
            }
            if (value > 0f)
            {
                return 1;
            }
            if (value != 0f)
            {
                throw new ArithmeticException(Environment.GetResourceString("Arithmetic_NaN"));
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Sin(double a);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Sinh(double value);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe double SplitFractionDouble(double* value);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern double Sqrt(double d);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Tan(double a);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern double Tanh(double value);
        public static decimal Truncate(decimal d)
        {
            return decimal.Truncate(d);
        }

        public static double Truncate(double d)
        {
            return InternalTruncate(d);
        }
    }
}

