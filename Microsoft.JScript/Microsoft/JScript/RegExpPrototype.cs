namespace Microsoft.JScript
{
    using System;

    public class RegExpPrototype : JSObject
    {
        internal static RegExpConstructor _constructor;
        internal static readonly RegExpPrototype ob = new RegExpPrototype(ObjectPrototype.ob);

        internal RegExpPrototype(ObjectPrototype parent) : base(parent)
        {
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.RegExp_compile)]
        public static RegExpObject compile(object thisob, object source, object flags)
        {
            RegExpObject obj2 = thisob as RegExpObject;
            if (obj2 == null)
            {
                throw new JScriptException(JSError.RegExpExpected);
            }
            return obj2.compile(((source == null) || (source is Missing)) ? "" : Microsoft.JScript.Convert.ToString(source), ((flags == null) || (flags is Missing)) ? "" : Microsoft.JScript.Convert.ToString(flags));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.RegExp_exec)]
        public static object exec(object thisob, object input)
        {
            RegExpObject obj2 = thisob as RegExpObject;
            if (obj2 == null)
            {
                throw new JScriptException(JSError.RegExpExpected);
            }
            if ((input is Missing) && !obj2.regExpConst.noExpando)
            {
                input = obj2.regExpConst.input;
            }
            return obj2.exec(Microsoft.JScript.Convert.ToString(input));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.RegExp_test)]
        public static bool test(object thisob, object input)
        {
            RegExpObject obj2 = thisob as RegExpObject;
            if (obj2 == null)
            {
                throw new JScriptException(JSError.RegExpExpected);
            }
            if ((input is Missing) && !obj2.regExpConst.noExpando)
            {
                input = obj2.regExpConst.input;
            }
            return obj2.test(Microsoft.JScript.Convert.ToString(input));
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.RegExp_toString)]
        public static string toString(object thisob)
        {
            RegExpObject obj2 = thisob as RegExpObject;
            if (obj2 == null)
            {
                throw new JScriptException(JSError.RegExpExpected);
            }
            return obj2.ToString();
        }

        public static RegExpConstructor constructor
        {
            get
            {
                return _constructor;
            }
        }
    }
}

