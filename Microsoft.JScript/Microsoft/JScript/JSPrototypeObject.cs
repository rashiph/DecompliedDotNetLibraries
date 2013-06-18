namespace Microsoft.JScript
{
    using System;

    public class JSPrototypeObject : JSObject
    {
        public object constructor;

        internal JSPrototypeObject(ScriptObject parent, ScriptFunction constructor) : base(parent, typeof(JSPrototypeObject))
        {
            this.constructor = constructor;
            base.noExpando = false;
        }
    }
}

