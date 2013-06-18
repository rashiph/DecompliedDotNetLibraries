namespace Microsoft.JScript
{
    using System;

    public class BooleanPrototype : BooleanObject
    {
        internal static BooleanConstructor _constructor;
        internal static readonly BooleanPrototype ob = new BooleanPrototype(ObjectPrototype.ob, typeof(BooleanPrototype));

        protected BooleanPrototype(ObjectPrototype parent, Type baseType) : base(parent, baseType)
        {
            base.noExpando = true;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Boolean_toString)]
        public static string toString(object thisob)
        {
            if (thisob is BooleanObject)
            {
                return Microsoft.JScript.Convert.ToString(((BooleanObject) thisob).value);
            }
            if (Microsoft.JScript.Convert.GetTypeCode(thisob) != TypeCode.Boolean)
            {
                throw new JScriptException(JSError.BooleanExpected);
            }
            return Microsoft.JScript.Convert.ToString(thisob);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Boolean_valueOf)]
        public static object valueOf(object thisob)
        {
            if (thisob is BooleanObject)
            {
                return ((BooleanObject) thisob).value;
            }
            if (Microsoft.JScript.Convert.GetTypeCode(thisob) != TypeCode.Boolean)
            {
                throw new JScriptException(JSError.BooleanExpected);
            }
            return thisob;
        }

        public static BooleanConstructor constructor
        {
            get
            {
                return _constructor;
            }
        }
    }
}

