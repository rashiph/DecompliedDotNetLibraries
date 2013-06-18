namespace Microsoft.JScript
{
    using System;

    public class ErrorPrototype : JSObject
    {
        internal ErrorConstructor _constructor;
        public readonly string name;
        internal static readonly ErrorPrototype ob = new ErrorPrototype(ObjectPrototype.ob, "Error");

        internal ErrorPrototype(ScriptObject parent, string name) : base(parent)
        {
            this.name = name;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Error_toString)]
        public static string toString(object thisob)
        {
            if (!(thisob is ErrorObject))
            {
                return thisob.ToString();
            }
            string message = ((ErrorObject) thisob).Message;
            if (message.Length == 0)
            {
                return LateBinding.GetMemberValue(thisob, "name").ToString();
            }
            return (LateBinding.GetMemberValue(thisob, "name").ToString() + ": " + message);
        }

        public ErrorConstructor constructor
        {
            get
            {
                return this._constructor;
            }
        }
    }
}

