namespace Microsoft.JScript
{
    using System;

    public sealed class LenientDatePrototype : DatePrototype
    {
        public object constructor;
        public object getDate;
        public object getDay;
        public object getFullYear;
        public object getHours;
        public object getMilliseconds;
        public object getMinutes;
        public object getMonth;
        public object getSeconds;
        public object getTime;
        public object getTimezoneOffset;
        public object getUTCDate;
        public object getUTCDay;
        public object getUTCFullYear;
        public object getUTCHours;
        public object getUTCMilliseconds;
        public object getUTCMinutes;
        public object getUTCMonth;
        public object getUTCSeconds;
        public object getVarDate;
        [NotRecommended("getYear")]
        public object getYear;
        public object setDate;
        public object setFullYear;
        public object setHours;
        public object setMilliseconds;
        public object setMinutes;
        public object setMonth;
        public object setSeconds;
        public object setTime;
        public object setUTCDate;
        public object setUTCFullYear;
        public object setUTCHours;
        public object setUTCMilliseconds;
        public object setUTCMinutes;
        public object setUTCMonth;
        public object setUTCSeconds;
        [NotRecommended("setYear")]
        public object setYear;
        public object toDateString;
        [NotRecommended("toGMTString")]
        public object toGMTString;
        public object toLocaleDateString;
        public object toLocaleString;
        public object toLocaleTimeString;
        public object toString;
        public object toTimeString;
        public object toUTCString;
        public object valueOf;

        internal LenientDatePrototype(LenientFunctionPrototype funcprot, LenientObjectPrototype parent) : base(parent)
        {
            base.noExpando = false;
            Type type = typeof(DatePrototype);
            this.getTime = new BuiltinFunction("getTime", this, type.GetMethod("getTime"), funcprot);
            this.getYear = new BuiltinFunction("getYear", this, type.GetMethod("getYear"), funcprot);
            this.getFullYear = new BuiltinFunction("getFullYear", this, type.GetMethod("getFullYear"), funcprot);
            this.getUTCFullYear = new BuiltinFunction("getUTCFullYear", this, type.GetMethod("getUTCFullYear"), funcprot);
            this.getMonth = new BuiltinFunction("getMonth", this, type.GetMethod("getMonth"), funcprot);
            this.getUTCMonth = new BuiltinFunction("getUTCMonth", this, type.GetMethod("getUTCMonth"), funcprot);
            this.getDate = new BuiltinFunction("getDate", this, type.GetMethod("getDate"), funcprot);
            this.getUTCDate = new BuiltinFunction("getUTCDate", this, type.GetMethod("getUTCDate"), funcprot);
            this.getDay = new BuiltinFunction("getDay", this, type.GetMethod("getDay"), funcprot);
            this.getUTCDay = new BuiltinFunction("getUTCDay", this, type.GetMethod("getUTCDay"), funcprot);
            this.getHours = new BuiltinFunction("getHours", this, type.GetMethod("getHours"), funcprot);
            this.getUTCHours = new BuiltinFunction("getUTCHours", this, type.GetMethod("getUTCHours"), funcprot);
            this.getMinutes = new BuiltinFunction("getMinutes", this, type.GetMethod("getMinutes"), funcprot);
            this.getUTCMinutes = new BuiltinFunction("getUTCMinutes", this, type.GetMethod("getUTCMinutes"), funcprot);
            this.getSeconds = new BuiltinFunction("getSeconds", this, type.GetMethod("getSeconds"), funcprot);
            this.getUTCSeconds = new BuiltinFunction("getUTCSeconds", this, type.GetMethod("getUTCSeconds"), funcprot);
            this.getMilliseconds = new BuiltinFunction("getMilliseconds", this, type.GetMethod("getMilliseconds"), funcprot);
            this.getUTCMilliseconds = new BuiltinFunction("getUTCMilliseconds", this, type.GetMethod("getUTCMilliseconds"), funcprot);
            this.getVarDate = new BuiltinFunction("getVarDate", this, type.GetMethod("getVarDate"), funcprot);
            this.getTimezoneOffset = new BuiltinFunction("getTimezoneOffset", this, type.GetMethod("getTimezoneOffset"), funcprot);
            this.setTime = new BuiltinFunction("setTime", this, type.GetMethod("setTime"), funcprot);
            this.setMilliseconds = new BuiltinFunction("setMilliseconds", this, type.GetMethod("setMilliseconds"), funcprot);
            this.setUTCMilliseconds = new BuiltinFunction("setUTCMilliseconds", this, type.GetMethod("setUTCMilliseconds"), funcprot);
            this.setSeconds = new BuiltinFunction("setSeconds", this, type.GetMethod("setSeconds"), funcprot);
            this.setUTCSeconds = new BuiltinFunction("setUTCSeconds", this, type.GetMethod("setUTCSeconds"), funcprot);
            this.setMinutes = new BuiltinFunction("setMinutes", this, type.GetMethod("setMinutes"), funcprot);
            this.setUTCMinutes = new BuiltinFunction("setUTCMinutes", this, type.GetMethod("setUTCMinutes"), funcprot);
            this.setHours = new BuiltinFunction("setHours", this, type.GetMethod("setHours"), funcprot);
            this.setUTCHours = new BuiltinFunction("setUTCHours", this, type.GetMethod("setUTCHours"), funcprot);
            this.setDate = new BuiltinFunction("setDate", this, type.GetMethod("setDate"), funcprot);
            this.setUTCDate = new BuiltinFunction("setUTCDate", this, type.GetMethod("setUTCDate"), funcprot);
            this.setMonth = new BuiltinFunction("setMonth", this, type.GetMethod("setMonth"), funcprot);
            this.setUTCMonth = new BuiltinFunction("setUTCMonth", this, type.GetMethod("setUTCMonth"), funcprot);
            this.setFullYear = new BuiltinFunction("setFullYear", this, type.GetMethod("setFullYear"), funcprot);
            this.setUTCFullYear = new BuiltinFunction("setUTCFullYear", this, type.GetMethod("setUTCFullYear"), funcprot);
            this.setYear = new BuiltinFunction("setYear", this, type.GetMethod("setYear"), funcprot);
            this.toDateString = new BuiltinFunction("toDateString", this, type.GetMethod("toDateString"), funcprot);
            this.toLocaleDateString = new BuiltinFunction("toLocaleDateString", this, type.GetMethod("toLocaleDateString"), funcprot);
            this.toLocaleString = new BuiltinFunction("toLocaleString", this, type.GetMethod("toLocaleString"), funcprot);
            this.toLocaleTimeString = new BuiltinFunction("toLocaleTimeString", this, type.GetMethod("toLocaleTimeString"), funcprot);
            this.toGMTString = new BuiltinFunction("toUTCString", this, type.GetMethod("toUTCString"), funcprot);
            this.toString = new BuiltinFunction("toString", this, type.GetMethod("toString"), funcprot);
            this.toTimeString = new BuiltinFunction("toTimeString", this, type.GetMethod("toTimeString"), funcprot);
            this.toUTCString = new BuiltinFunction("toUTCString", this, type.GetMethod("toUTCString"), funcprot);
            this.valueOf = new BuiltinFunction("valueOf", this, type.GetMethod("valueOf"), funcprot);
        }
    }
}

