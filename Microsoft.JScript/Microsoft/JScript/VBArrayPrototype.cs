namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;

    public class VBArrayPrototype : JSObject
    {
        internal static VBArrayConstructor _constructor;
        internal static readonly VBArrayPrototype ob = new VBArrayPrototype(FunctionPrototype.ob, ObjectPrototype.ob);

        internal VBArrayPrototype(FunctionPrototype funcprot, ObjectPrototype parent) : base(parent)
        {
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.VBArray_dimensions)]
        public static int dimensions(object thisob)
        {
            if (!(thisob is VBArrayObject))
            {
                throw new JScriptException(JSError.VBArrayExpected);
            }
            return ((VBArrayObject) thisob).dimensions();
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.VBArray_getItem)]
        public static object getItem(object thisob, params object[] args)
        {
            if (!(thisob is VBArrayObject))
            {
                throw new JScriptException(JSError.VBArrayExpected);
            }
            return ((VBArrayObject) thisob).getItem(args);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.VBArray_lbound)]
        public static int lbound(object thisob, object dimension)
        {
            if (!(thisob is VBArrayObject))
            {
                throw new JScriptException(JSError.VBArrayExpected);
            }
            return ((VBArrayObject) thisob).lbound(dimension);
        }

        [JSFunction(JSFunctionAttributeEnum.HasEngine | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.VBArray_toArray)]
        public static ArrayObject toArray(object thisob, VsaEngine engine)
        {
            if (!(thisob is VBArrayObject))
            {
                throw new JScriptException(JSError.VBArrayExpected);
            }
            return ((VBArrayObject) thisob).toArray(engine);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.VBArray_ubound)]
        public static int ubound(object thisob, object dimension)
        {
            if (!(thisob is VBArrayObject))
            {
                throw new JScriptException(JSError.VBArrayExpected);
            }
            return ((VBArrayObject) thisob).ubound(dimension);
        }

        public static VBArrayConstructor constructor
        {
            get
            {
                return _constructor;
            }
        }
    }
}

