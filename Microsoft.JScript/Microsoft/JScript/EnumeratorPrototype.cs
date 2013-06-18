namespace Microsoft.JScript
{
    using System;

    public class EnumeratorPrototype : JSObject
    {
        internal static EnumeratorConstructor _constructor;
        internal static readonly EnumeratorPrototype ob = new EnumeratorPrototype(ObjectPrototype.ob);

        internal EnumeratorPrototype(ObjectPrototype parent) : base(parent)
        {
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Enumerator_atEnd)]
        public static bool atEnd(object thisob)
        {
            if (!(thisob is EnumeratorObject))
            {
                throw new JScriptException(JSError.EnumeratorExpected);
            }
            return ((EnumeratorObject) thisob).atEnd();
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Enumerator_item)]
        public static object item(object thisob)
        {
            if (!(thisob is EnumeratorObject))
            {
                throw new JScriptException(JSError.EnumeratorExpected);
            }
            return ((EnumeratorObject) thisob).item();
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Enumerator_moveFirst)]
        public static void moveFirst(object thisob)
        {
            if (!(thisob is EnumeratorObject))
            {
                throw new JScriptException(JSError.EnumeratorExpected);
            }
            ((EnumeratorObject) thisob).moveFirst();
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Enumerator_moveNext)]
        public static void moveNext(object thisob)
        {
            if (!(thisob is EnumeratorObject))
            {
                throw new JScriptException(JSError.EnumeratorExpected);
            }
            ((EnumeratorObject) thisob).moveNext();
        }

        public static EnumeratorConstructor constructor
        {
            get
            {
                return _constructor;
            }
        }
    }
}

