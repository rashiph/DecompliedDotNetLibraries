namespace Microsoft.JScript
{
    using System;

    public class MathObject : JSObject
    {
        public const double E = 2.7182818284590451;
        private static readonly Random internalRandom = new Random();
        public const double LN10 = 2.3025850929940459;
        public const double LN2 = 0.69314718055994529;
        public const double LOG10E = 0.43429448190325182;
        public const double LOG2E = 1.4426950408889634;
        internal static MathObject ob = null;
        public const double PI = 3.1415926535897931;
        public const double SQRT1_2 = 0.70710678118654757;
        public const double SQRT2 = 1.4142135623730951;

        internal MathObject(ScriptObject parent) : base(parent)
        {
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_abs)]
        public static double abs(double d)
        {
            if (d < 0.0)
            {
                return -d;
            }
            if ((d <= 0.0) && (d == d))
            {
                return 0.0;
            }
            return d;
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_acos)]
        public static double acos(double x)
        {
            return Math.Acos(x);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_asin)]
        public static double asin(double x)
        {
            return Math.Asin(x);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_atan)]
        public static double atan(double x)
        {
            return Math.Atan(x);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_atan2)]
        public static double atan2(double dy, double dx)
        {
            return Math.Atan2(dy, dx);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_ceil)]
        public static double ceil(double x)
        {
            return Math.Ceiling(x);
        }

        private static double Compare(double x, double y)
        {
            if ((x != 0.0) || (y != 0.0))
            {
                if (x == y)
                {
                    return 0.0;
                }
                return (x - y);
            }
            double num = 1.0 / x;
            double num2 = 1.0 / y;
            if (num < 0.0)
            {
                return ((num2 < 0.0) ? ((double) 0) : ((double) (-1)));
            }
            if (num2 < 0.0)
            {
                return 1.0;
            }
            return 0.0;
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_cos)]
        public static double cos(double x)
        {
            return Math.Cos(x);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_exp)]
        public static double exp(double x)
        {
            return Math.Exp(x);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_floor)]
        public static double floor(double x)
        {
            return Math.Floor(x);
        }

        internal override string GetClassName()
        {
            return "Math";
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_log)]
        public static double log(double x)
        {
            return Math.Log(x);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Math_max)]
        public static double max(object x, object y, params object[] args)
        {
            if (x is Missing)
            {
                return double.NegativeInfinity;
            }
            double num = Microsoft.JScript.Convert.ToNumber(x);
            if (y is Missing)
            {
                return num;
            }
            double num2 = Microsoft.JScript.Convert.ToNumber(y);
            double num3 = Compare(num, num2);
            if (num3 != num3)
            {
                return num3;
            }
            double lhMax = num;
            if (num3 < 0.0)
            {
                lhMax = num2;
            }
            if (args.Length == 0)
            {
                return lhMax;
            }
            return maxv(lhMax, args, 0);
        }

        private static double maxv(double lhMax, object[] args, int start)
        {
            if (args.Length == start)
            {
                return lhMax;
            }
            double y = Microsoft.JScript.Convert.ToNumber(args[start]);
            double num2 = Compare(lhMax, y);
            if (num2 != num2)
            {
                return num2;
            }
            if (num2 > 0.0)
            {
                y = lhMax;
            }
            return maxv(y, args, start + 1);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs, JSBuiltin.Math_min)]
        public static double min(object x, object y, params object[] args)
        {
            if (x is Missing)
            {
                return double.PositiveInfinity;
            }
            double num = Microsoft.JScript.Convert.ToNumber(x);
            if (y is Missing)
            {
                return num;
            }
            double num2 = Microsoft.JScript.Convert.ToNumber(y);
            double num3 = Compare(num, num2);
            if (num3 != num3)
            {
                return num3;
            }
            double lhMin = num;
            if (num3 > 0.0)
            {
                lhMin = num2;
            }
            if (args.Length == 0)
            {
                return lhMin;
            }
            return minv(lhMin, args, 0);
        }

        private static double minv(double lhMin, object[] args, int start)
        {
            if (args.Length == start)
            {
                return lhMin;
            }
            double y = Microsoft.JScript.Convert.ToNumber(args[start]);
            double num2 = Compare(lhMin, y);
            if (num2 != num2)
            {
                return num2;
            }
            if (num2 < 0.0)
            {
                y = lhMin;
            }
            return minv(y, args, start + 1);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_pow)]
        public static double pow(double dx, double dy)
        {
            if (dy == 0.0)
            {
                return 1.0;
            }
            if (((dx == 1.0) || (dx == -1.0)) && ((dy == double.PositiveInfinity) || (dy == double.NegativeInfinity)))
            {
                return double.NaN;
            }
            if (double.IsNaN(dy))
            {
                return double.NaN;
            }
            if (((dx == double.NegativeInfinity) && (dy < 0.0)) && (Math.IEEERemainder(-dy + 1.0, 2.0) == 0.0))
            {
                return 0.0;
            }
            try
            {
                return Math.Pow(dx, dy);
            }
            catch
            {
                if (((dx == dx) && (dy == dy)) && ((dx == 0.0) && (dy < 0.0)))
                {
                    if ((((long) dy) == dy) && ((((long) -dy) % 2L) > 0L))
                    {
                        double num = 1.0 / dx;
                        if (num < 0.0)
                        {
                            return double.NegativeInfinity;
                        }
                    }
                    return double.PositiveInfinity;
                }
                return double.NaN;
            }
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_random)]
        public static double random()
        {
            return internalRandom.NextDouble();
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_round)]
        public static double round(double d)
        {
            if (d == 0.0)
            {
                return d;
            }
            return Math.Floor((double) (d + 0.5));
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_sin)]
        public static double sin(double x)
        {
            return Math.Sin(x);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_sqrt)]
        public static double sqrt(double x)
        {
            return Math.Sqrt(x);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Math_tan)]
        public static double tan(double x)
        {
            return Math.Tan(x);
        }
    }
}

