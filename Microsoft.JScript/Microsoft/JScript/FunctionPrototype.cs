namespace Microsoft.JScript
{
    using System;

    public class FunctionPrototype : ScriptFunction
    {
        internal static FunctionConstructor _constructor;
        internal static readonly FunctionPrototype ob = new FunctionPrototype(ObjectPrototype.CommonInstance());

        internal FunctionPrototype(ScriptObject parent) : base(parent)
        {
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Function_apply)]
        public static object apply(object thisob, object thisarg, object argArray)
        {
            if (!(thisob is ScriptFunction))
            {
                throw new JScriptException(JSError.FunctionExpected);
            }
            if (thisarg is Missing)
            {
                thisarg = ((IActivationObject) ((ScriptFunction) thisob).engine.ScriptObjectStackTop()).GetDefaultThisObject();
            }
            if (argArray is Missing)
            {
                return ((ScriptFunction) thisob).Call(new object[0], thisarg);
            }
            if (argArray is ArgumentsObject)
            {
                return ((ScriptFunction) thisob).Call(((ArgumentsObject) argArray).ToArray(), thisarg);
            }
            if (!(argArray is ArrayObject))
            {
                throw new JScriptException(JSError.InvalidCall);
            }
            return ((ScriptFunction) thisob).Call(((ArrayObject) argArray).ToArray(), thisarg);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Function_call)]
        public static object call(object thisob, object thisarg, params object[] args)
        {
            if (!(thisob is ScriptFunction))
            {
                throw new JScriptException(JSError.FunctionExpected);
            }
            if (thisarg is Missing)
            {
                thisarg = ((IActivationObject) ((ScriptFunction) thisob).engine.ScriptObjectStackTop()).GetDefaultThisObject();
            }
            return ((ScriptFunction) thisob).Call(args, thisarg);
        }

        internal override object Call(object[] args, object thisob)
        {
            return null;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Function_toString)]
        public static string toString(object thisob)
        {
            if (!(thisob is ScriptFunction))
            {
                throw new JScriptException(JSError.FunctionExpected);
            }
            return thisob.ToString();
        }

        public static FunctionConstructor constructor
        {
            get
            {
                return _constructor;
            }
        }
    }
}

