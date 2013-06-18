namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;

    public sealed class ArgumentsObject : JSObject
    {
        private object[] arguments;
        public object callee;
        public object caller;
        private string[] formal_names;
        public object length;
        private ScriptObject scope;

        internal ArgumentsObject(ScriptObject parent, object[] arguments, FunctionObject function, Closure callee, ScriptObject scope, ArgumentsObject caller) : base(parent)
        {
            this.arguments = arguments;
            this.formal_names = function.formal_parameters;
            this.scope = scope;
            this.callee = callee;
            this.caller = caller;
            this.length = arguments.Length;
            base.noExpando = false;
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object GetMemberValue(string name)
        {
            long num = ArrayObject.Array_index_for(name);
            if (num < 0L)
            {
                return base.GetMemberValue(name);
            }
            return this.GetValueAtIndex((uint) num);
        }

        internal override object GetValueAtIndex(uint index)
        {
            if (index < this.arguments.Length)
            {
                return this.arguments[index];
            }
            return base.GetValueAtIndex(index);
        }

        internal override void SetValueAtIndex(uint index, object value)
        {
            if (index < this.arguments.Length)
            {
                this.arguments[index] = value;
            }
            else
            {
                base.SetValueAtIndex(index, value);
            }
        }

        internal object[] ToArray()
        {
            return this.arguments;
        }
    }
}

