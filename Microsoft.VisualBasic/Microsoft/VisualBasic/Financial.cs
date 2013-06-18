namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StandardModule]
    public sealed class Financial
    {
        private const double cnL_IT_EPSILON = 1E-07;
        private const double cnL_IT_STEP = 1E-05;

        public static double DDB(double Cost, double Salvage, double Life, double Period, double Factor = 2.0)
        {
            if (((Factor <= 0.0) || (Salvage < 0.0)) || ((Period <= 0.0) || (Period > Life)))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Factor" }));
            }
            if (Cost > 0.0)
            {
                double num4;
                double num5;
                if (Life < 2.0)
                {
                    return (Cost - Salvage);
                }
                if ((Life == 2.0) && (Period > 1.0))
                {
                    return 0.0;
                }
                if ((Life == 2.0) && (Period <= 1.0))
                {
                    return (Cost - Salvage);
                }
                if (Period <= 1.0)
                {
                    num4 = (Cost * Factor) / Life;
                    num5 = Cost - Salvage;
                    if (num4 > num5)
                    {
                        return num5;
                    }
                    return num4;
                }
                num5 = (Life - Factor) / Life;
                double y = Period - 1.0;
                num4 = ((Factor * Cost) / Life) * Math.Pow(num5, y);
                double num6 = Cost * (1.0 - Math.Pow(num5, Period));
                double num2 = (num6 - Cost) + Salvage;
                if (num2 > 0.0)
                {
                    num4 -= num2;
                }
                if (num4 >= 0.0)
                {
                    return num4;
                }
            }
            return 0.0;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double FV(double Rate, double NPer, double Pmt, double PV = 0.0, DueDate Due = 0)
        {
            return FV_Internal(Rate, NPer, Pmt, PV, Due);
        }

        private static double FV_Internal(double Rate, double NPer, double Pmt, double PV = 0.0, DueDate Due = 0)
        {
            double num;
            if (Rate == 0.0)
            {
                return (-PV - (Pmt * NPer));
            }
            if (Due != DueDate.EndOfPeriod)
            {
                num = 1.0 + Rate;
            }
            else
            {
                num = 1.0;
            }
            double x = 1.0 + Rate;
            double num2 = Math.Pow(x, NPer);
            return ((-PV * num2) - (((Pmt / Rate) * num) * (num2 - 1.0)));
        }

        public static double IPmt(double Rate, double Per, double NPer, double PV, double FV = 0.0, DueDate Due = 0)
        {
            double num;
            if (Due != DueDate.EndOfPeriod)
            {
                num = 2.0;
            }
            else
            {
                num = 1.0;
            }
            if ((Per <= 0.0) || (Per >= (NPer + 1.0)))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Per" }));
            }
            if ((Due != DueDate.EndOfPeriod) && (Per == 1.0))
            {
                return 0.0;
            }
            double pmt = PMT_Internal(Rate, NPer, PV, FV, Due);
            if (Due != DueDate.EndOfPeriod)
            {
                PV += pmt;
            }
            return (FV_Internal(Rate, Per - num, pmt, PV, DueDate.EndOfPeriod) * Rate);
        }

        public static double IRR(ref double[] ValueArray, double Guess = 0.1)
        {
            double num5;
            double num6;
            int num10;
            int upperBound;
            try
            {
                upperBound = ValueArray.GetUpperBound(0);
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "ValueArray" }));
            }
            int num9 = upperBound + 1;
            if (Guess <= -1.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Guess" }));
            }
            if (num9 <= 1)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "ValueArray" }));
            }
            if (ValueArray[0] > 0.0)
            {
                num6 = ValueArray[0];
            }
            else
            {
                num6 = -ValueArray[0];
            }
            int num12 = upperBound;
            for (num10 = 0; num10 <= num12; num10++)
            {
                if (ValueArray[num10] > num6)
                {
                    num6 = ValueArray[num10];
                }
                else if (-ValueArray[num10] > num6)
                {
                    num6 = -ValueArray[num10];
                }
            }
            double num3 = (num6 * 1E-07) * 0.01;
            double guess = Guess;
            double num = OptPV2(ref ValueArray, guess);
            if (num > 0.0)
            {
                num5 = guess + 1E-05;
            }
            else
            {
                num5 = guess - 1E-05;
            }
            if (num5 <= -1.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Rate" }));
            }
            double num2 = OptPV2(ref ValueArray, num5);
            num10 = 0;
            while (true)
            {
                double num7;
                if (num2 == num)
                {
                    if (num5 > guess)
                    {
                        guess -= 1E-05;
                    }
                    else
                    {
                        guess += 1E-05;
                    }
                    num = OptPV2(ref ValueArray, guess);
                    if (num2 == num)
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
                    }
                }
                guess = num5 - (((num5 - guess) * num2) / (num2 - num));
                if (guess <= -1.0)
                {
                    guess = (num5 - 1.0) * 0.5;
                }
                num = OptPV2(ref ValueArray, guess);
                if (guess > num5)
                {
                    num6 = guess - num5;
                }
                else
                {
                    num6 = num5 - guess;
                }
                if (num > 0.0)
                {
                    num7 = num;
                }
                else
                {
                    num7 = -num;
                }
                if ((num7 < num3) && (num6 < 1E-07))
                {
                    return guess;
                }
                num6 = num;
                num = num2;
                num2 = num6;
                num6 = guess;
                guess = num5;
                num5 = num6;
                num10++;
                if (num10 > 0x27)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
                }
            }
        }

        private static double LDoNPV(double Rate, ref double[] ValueArray, int iWNType)
        {
            bool flag2 = iWNType < 0;
            bool flag = iWNType > 0;
            double num = 1.0;
            double num2 = 0.0;
            int num6 = 0;
            int num8 = ValueArray.GetUpperBound(0);
            for (int i = num6; i <= num8; i++)
            {
                double num3 = ValueArray[i];
                num += num * Rate;
                if ((!flag2 || (num3 <= 0.0)) && (!flag || (num3 >= 0.0)))
                {
                    num2 += num3 / num;
                }
            }
            return num2;
        }

        private static double LEvalRate(double Rate, double NPer, double Pmt, double PV, double dFv, DueDate Due)
        {
            double num2;
            if (Rate == 0.0)
            {
                return ((PV + (Pmt * NPer)) + dFv);
            }
            double x = Rate + 1.0;
            double num = Math.Pow(x, NPer);
            if (Due != DueDate.EndOfPeriod)
            {
                num2 = 1.0 + Rate;
            }
            else
            {
                num2 = 1.0;
            }
            return (((PV * num) + (((Pmt * num2) * (num - 1.0)) / Rate)) + dFv);
        }

        public static double MIRR(ref double[] ValueArray, double FinanceRate, double ReinvestRate)
        {
            if (ValueArray.Rank != 1)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_RankEQOne1", new string[] { "ValueArray" }));
            }
            int num7 = 0;
            int num6 = (ValueArray.GetUpperBound(0) - num7) + 1;
            if (FinanceRate == -1.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "FinanceRate" }));
            }
            if (ReinvestRate == -1.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "ReinvestRate" }));
            }
            if (num6 <= 1)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "ValueArray" }));
            }
            double num = LDoNPV(FinanceRate, ref ValueArray, -1);
            if (num == 0.0)
            {
                throw new DivideByZeroException(Utils.GetResourceString("Financial_CalcDivByZero"));
            }
            double num2 = LDoNPV(ReinvestRate, ref ValueArray, 1);
            double x = ReinvestRate + 1.0;
            double y = num6;
            double num4 = (-num2 * Math.Pow(x, y)) / (num * (FinanceRate + 1.0));
            if (num4 < 0.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
            }
            x = 1.0 / (num6 - 1.0);
            return (Math.Pow(num4, x) - 1.0);
        }

        public static double NPer(double Rate, double Pmt, double PV, double FV = 0.0, DueDate Due = 0)
        {
            double num;
            if (Rate <= -1.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Rate" }));
            }
            if (Rate == 0.0)
            {
                if (Pmt == 0.0)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Pmt" }));
                }
                return (-(PV + FV) / Pmt);
            }
            if (Due != DueDate.EndOfPeriod)
            {
                num = (Pmt * (1.0 + Rate)) / Rate;
            }
            else
            {
                num = Pmt / Rate;
            }
            double d = -FV + num;
            double num4 = PV + num;
            if ((d < 0.0) && (num4 < 0.0))
            {
                d = -1.0 * d;
                num4 = -1.0 * num4;
            }
            else if ((d <= 0.0) || (num4 <= 0.0))
            {
                throw new ArgumentException(Utils.GetResourceString("Financial_CannotCalculateNPer"));
            }
            double num2 = Rate + 1.0;
            return ((Math.Log(d) - Math.Log(num4)) / Math.Log(num2));
        }

        public static double NPV(double Rate, ref double[] ValueArray)
        {
            if (ValueArray == null)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "ValueArray" }));
            }
            if (ValueArray.Rank != 1)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_RankEQOne1", new string[] { "ValueArray" }));
            }
            int num2 = 0;
            int num = (ValueArray.GetUpperBound(0) - num2) + 1;
            if (Rate == -1.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Rate" }));
            }
            if (num < 1)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "ValueArray" }));
            }
            return LDoNPV(Rate, ref ValueArray, 0);
        }

        private static double OptPV2(ref double[] ValueArray, double Guess = 0.1)
        {
            int index = 0;
            int upperBound = ValueArray.GetUpperBound(0);
            double num2 = 0.0;
            double num = 1.0 + Guess;
            while ((index <= upperBound) && (ValueArray[index] == 0.0))
            {
                index++;
            }
            int num7 = index;
            for (int i = upperBound; i >= num7; i += -1)
            {
                num2 /= num;
                num2 += ValueArray[i];
            }
            return num2;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double Pmt(double Rate, double NPer, double PV, double FV = 0.0, DueDate Due = 0)
        {
            return PMT_Internal(Rate, NPer, PV, FV, Due);
        }

        private static double PMT_Internal(double Rate, double NPer, double PV, double FV = 0.0, DueDate Due = 0)
        {
            double num;
            if (NPer == 0.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "NPer" }));
            }
            if (Rate == 0.0)
            {
                return ((-FV - PV) / NPer);
            }
            if (Due != DueDate.EndOfPeriod)
            {
                num = 1.0 + Rate;
            }
            else
            {
                num = 1.0;
            }
            double x = Rate + 1.0;
            double num2 = Math.Pow(x, NPer);
            return (((-FV - (PV * num2)) / (num * (num2 - 1.0))) * Rate);
        }

        public static double PPmt(double Rate, double Per, double NPer, double PV, double FV = 0.0, DueDate Due = 0)
        {
            if ((Per <= 0.0) || (Per >= (NPer + 1.0)))
            {
                throw new ArgumentException(Utils.GetResourceString("PPMT_PerGT0AndLTNPer", new string[] { "Per" }));
            }
            double num2 = PMT_Internal(Rate, NPer, PV, FV, Due);
            double num = IPmt(Rate, Per, NPer, PV, FV, Due);
            return (num2 - num);
        }

        public static double PV(double Rate, double NPer, double Pmt, double FV = 0.0, DueDate Due = 0)
        {
            double num;
            if (Rate == 0.0)
            {
                return (-FV - (Pmt * NPer));
            }
            if (Due != DueDate.EndOfPeriod)
            {
                num = 1.0 + Rate;
            }
            else
            {
                num = 1.0;
            }
            double x = 1.0 + Rate;
            double num2 = Math.Pow(x, NPer);
            return (-(FV + ((Pmt * num) * ((num2 - 1.0) / Rate))) / num2);
        }

        public static double Rate(double NPer, double Pmt, double PV, double FV = 0.0, DueDate Due = 0, double Guess = 0.1)
        {
            double num2;
            if (NPer <= 0.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Rate_NPerMustBeGTZero"));
            }
            double rate = Guess;
            double num4 = LEvalRate(rate, NPer, Pmt, PV, FV, Due);
            if (num4 > 0.0)
            {
                num2 = rate / 2.0;
            }
            else
            {
                num2 = rate * 2.0;
            }
            double num5 = LEvalRate(num2, NPer, Pmt, PV, FV, Due);
            int num6 = 0;
            while (true)
            {
                if (num5 == num4)
                {
                    if (num2 > rate)
                    {
                        rate -= 1E-05;
                    }
                    else
                    {
                        rate -= -1E-05;
                    }
                    num4 = LEvalRate(rate, NPer, Pmt, PV, FV, Due);
                    if (num5 == num4)
                    {
                        throw new ArgumentException(Utils.GetResourceString("Financial_CalcDivByZero"));
                    }
                }
                rate = num2 - (((num2 - rate) * num5) / (num5 - num4));
                num4 = LEvalRate(rate, NPer, Pmt, PV, FV, Due);
                if (Math.Abs(num4) < 1E-07)
                {
                    return rate;
                }
                double num3 = num4;
                num4 = num5;
                num5 = num3;
                num3 = rate;
                rate = num2;
                num2 = num3;
                num6++;
                if (num6 > 0x27)
                {
                    throw new ArgumentException(Utils.GetResourceString("Financial_CannotCalculateRate"));
                }
            }
        }

        public static double SLN(double Cost, double Salvage, double Life)
        {
            if (Life == 0.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Financial_LifeNEZero"));
            }
            return ((Cost - Salvage) / Life);
        }

        public static double SYD(double Cost, double Salvage, double Life, double Period)
        {
            if (Salvage < 0.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Financial_ArgGEZero1", new string[] { "Salvage" }));
            }
            if (Period > Life)
            {
                throw new ArgumentException(Utils.GetResourceString("Financial_PeriodLELife"));
            }
            if (Period <= 0.0)
            {
                throw new ArgumentException(Utils.GetResourceString("Financial_ArgGTZero1", new string[] { "Period" }));
            }
            double num = (Cost - Salvage) / (Life * (Life + 1.0));
            return ((num * ((Life + 1.0) - Period)) * 2.0);
        }
    }
}

