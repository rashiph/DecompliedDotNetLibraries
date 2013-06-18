namespace Microsoft.JScript
{
    using System;

    public class DateConstructor : ScriptFunction
    {
        internal static readonly DateConstructor ob = new DateConstructor();
        private DatePrototype originalPrototype;

        internal DateConstructor() : base(FunctionPrototype.ob, "Date", 7)
        {
            this.originalPrototype = DatePrototype.ob;
            DatePrototype._constructor = this;
            base.proto = DatePrototype.ob;
        }

        internal DateConstructor(LenientFunctionPrototype parent, LenientDatePrototype prototypeProp) : base(parent, "Date", 7)
        {
            this.originalPrototype = prototypeProp;
            prototypeProp.constructor = this;
            base.proto = prototypeProp;
            base.noExpando = false;
        }

        internal override object Call(object[] args, object thisob)
        {
            return this.Invoke();
        }

        internal DateObject Construct(DateTime dt)
        {
            return new DateObject(this.originalPrototype, (((double) dt.ToUniversalTime().Ticks) / 10000.0) - 62135596800000);
        }

        internal override object Construct(object[] args)
        {
            return this.CreateInstance(args);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs)]
        public DateObject CreateInstance(params object[] args)
        {
            if (args.Length == 0)
            {
                return new DateObject(this.originalPrototype, (((double) DateTime.Now.ToUniversalTime().Ticks) / 10000.0) - 62135596800000);
            }
            if (args.Length == 1)
            {
                object ob = args[0];
                IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(ob);
                if (Microsoft.JScript.Convert.GetTypeCode(ob, iConvertible) == TypeCode.DateTime)
                {
                    return new DateObject(this.originalPrototype, (((double) iConvertible.ToDateTime(null).ToUniversalTime().Ticks) / 10000.0) - 62135596800000);
                }
                object obj3 = Microsoft.JScript.Convert.ToPrimitive(ob, PreferredType.Either, ref iConvertible);
                if (Microsoft.JScript.Convert.GetTypeCode(obj3, iConvertible) == TypeCode.String)
                {
                    return new DateObject(this.originalPrototype, parse(iConvertible.ToString(null)));
                }
                double num = Microsoft.JScript.Convert.ToNumber(obj3, iConvertible);
                if ((-8.64E+15 <= num) && (num <= 8.64E+15))
                {
                    return new DateObject(this.originalPrototype, num);
                }
                return new DateObject(this.originalPrototype, double.NaN);
            }
            double val = Microsoft.JScript.Convert.ToNumber(args[0]);
            double month = Microsoft.JScript.Convert.ToNumber(args[1]);
            double date = (args.Length > 2) ? Microsoft.JScript.Convert.ToNumber(args[2]) : 1.0;
            double hour = (args.Length > 3) ? Microsoft.JScript.Convert.ToNumber(args[3]) : 0.0;
            double min = (args.Length > 4) ? Microsoft.JScript.Convert.ToNumber(args[4]) : 0.0;
            double sec = (args.Length > 5) ? Microsoft.JScript.Convert.ToNumber(args[5]) : 0.0;
            double ms = (args.Length > 6) ? Microsoft.JScript.Convert.ToNumber(args[6]) : 0.0;
            int num9 = (int) Runtime.DoubleToInt64(val);
            if ((!double.IsNaN(val) && (0 <= num9)) && (num9 <= 0x63))
            {
                val = num9 + 0x76c;
            }
            double day = DatePrototype.MakeDay(val, month, date);
            double time = DatePrototype.MakeTime(hour, min, sec, ms);
            return new DateObject(this.originalPrototype, DatePrototype.TimeClip(DatePrototype.UTC(DatePrototype.MakeDate(day, time))));
        }

        public string Invoke()
        {
            return DatePrototype.DateToString((((double) DateTime.Now.ToUniversalTime().Ticks) / 10000.0) - 62135596800000);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Date_parse)]
        public static double parse(string str)
        {
            return DatePrototype.ParseDate(str);
        }

        [JSFunction(JSFunctionAttributeEnum.None, JSBuiltin.Date_UTC)]
        public static double UTC(object year, object month, object date, object hours, object minutes, object seconds, object ms)
        {
            if (year is Missing)
            {
                return ((((double) DateTime.Now.ToUniversalTime().Ticks) / 10000.0) - 62135596800000);
            }
            double val = Microsoft.JScript.Convert.ToNumber(year);
            double num2 = (month is Missing) ? 0.0 : Microsoft.JScript.Convert.ToNumber(month);
            double num3 = (date is Missing) ? 1.0 : Microsoft.JScript.Convert.ToNumber(date);
            double hour = (hours is Missing) ? 0.0 : Microsoft.JScript.Convert.ToNumber(hours);
            double min = (minutes is Missing) ? 0.0 : Microsoft.JScript.Convert.ToNumber(minutes);
            double sec = (seconds is Missing) ? 0.0 : Microsoft.JScript.Convert.ToNumber(seconds);
            double num7 = (ms is Missing) ? 0.0 : Microsoft.JScript.Convert.ToNumber(ms);
            int num8 = (int) Runtime.DoubleToInt64(val);
            if ((!double.IsNaN(val) && (0 <= num8)) && (num8 <= 0x63))
            {
                val = num8 + 0x76c;
            }
            double day = DatePrototype.MakeDay(val, num2, num3);
            double time = DatePrototype.MakeTime(hour, min, sec, num7);
            return DatePrototype.TimeClip(DatePrototype.MakeDate(day, time));
        }
    }
}

