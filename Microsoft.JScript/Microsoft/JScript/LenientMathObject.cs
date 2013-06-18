namespace Microsoft.JScript
{
    using System;

    public sealed class LenientMathObject : MathObject
    {
        public object abs;
        public object acos;
        public object asin;
        public object atan;
        public object atan2;
        public object ceil;
        public object cos;
        public const double E = 2.7182818284590451;
        public object exp;
        public object floor;
        public const double LN10 = 2.3025850929940459;
        public const double LN2 = 0.69314718055994529;
        public object log;
        public const double LOG10E = 0.43429448190325182;
        public const double LOG2E = 1.4426950408889634;
        public object max;
        public object min;
        public const double PI = 3.1415926535897931;
        public object pow;
        public object random;
        public object round;
        public object sin;
        public object sqrt;
        public const double SQRT1_2 = 0.70710678118654757;
        public const double SQRT2 = 1.4142135623730951;
        public object tan;

        internal LenientMathObject(ScriptObject parent, FunctionPrototype funcprot) : base(parent)
        {
            base.noExpando = false;
            Type type = typeof(MathObject);
            this.abs = new BuiltinFunction("abs", this, type.GetMethod("abs"), funcprot);
            this.acos = new BuiltinFunction("acos", this, type.GetMethod("acos"), funcprot);
            this.asin = new BuiltinFunction("asin", this, type.GetMethod("asin"), funcprot);
            this.atan = new BuiltinFunction("atan", this, type.GetMethod("atan"), funcprot);
            this.atan2 = new BuiltinFunction("atan2", this, type.GetMethod("atan2"), funcprot);
            this.ceil = new BuiltinFunction("ceil", this, type.GetMethod("ceil"), funcprot);
            this.cos = new BuiltinFunction("cos", this, type.GetMethod("cos"), funcprot);
            this.exp = new BuiltinFunction("exp", this, type.GetMethod("exp"), funcprot);
            this.floor = new BuiltinFunction("floor", this, type.GetMethod("floor"), funcprot);
            this.log = new BuiltinFunction("log", this, type.GetMethod("log"), funcprot);
            this.max = new BuiltinFunction("max", this, type.GetMethod("max"), funcprot);
            this.min = new BuiltinFunction("min", this, type.GetMethod("min"), funcprot);
            this.pow = new BuiltinFunction("pow", this, type.GetMethod("pow"), funcprot);
            this.random = new BuiltinFunction("random", this, type.GetMethod("random"), funcprot);
            this.round = new BuiltinFunction("round", this, type.GetMethod("round"), funcprot);
            this.sin = new BuiltinFunction("sin", this, type.GetMethod("sin"), funcprot);
            this.sqrt = new BuiltinFunction("sqrt", this, type.GetMethod("sqrt"), funcprot);
            this.tan = new BuiltinFunction("tan", this, type.GetMethod("tan"), funcprot);
        }
    }
}

