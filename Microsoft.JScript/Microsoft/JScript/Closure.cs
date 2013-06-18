namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class Closure : ScriptFunction
    {
        public object arguments;
        public object caller;
        private object declaringObject;
        private ScriptObject enclosing_scope;
        internal FunctionObject func;

        public Closure(FunctionObject func) : this(func, null)
        {
            if (func.enclosing_scope is Microsoft.JScript.StackFrame)
            {
                this.enclosing_scope = func.enclosing_scope;
            }
        }

        internal Closure(FunctionObject func, object declaringObject) : base(func.GetParent(), func.name, func.GetNumberOfFormalParameters())
        {
            this.func = func;
            base.engine = func.engine;
            base.proto = new JSPrototypeObject(((ScriptObject) func.proto).GetParent(), this);
            this.enclosing_scope = base.engine.ScriptObjectStackTop();
            this.arguments = DBNull.Value;
            this.caller = DBNull.Value;
            this.declaringObject = declaringObject;
            base.noExpando = func.noExpando;
            if (func.isExpandoMethod)
            {
                Microsoft.JScript.StackFrame frame = new Microsoft.JScript.StackFrame(new WithObject(this.enclosing_scope, declaringObject), new JSLocalField[0], new object[0], null);
                this.enclosing_scope = frame;
                frame.closureInstance = declaringObject;
            }
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object Call(object[] args, object thisob)
        {
            return this.Call(args, thisob, JSBinder.ob, null);
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal override object Call(object[] args, object thisob, Binder binder, CultureInfo culture)
        {
            if (this.func.isExpandoMethod)
            {
                ((Microsoft.JScript.StackFrame) this.enclosing_scope).thisObject = thisob;
            }
            else if ((this.declaringObject != null) && !(this.declaringObject is ClassScope))
            {
                thisob = this.declaringObject;
            }
            if (thisob == null)
            {
                thisob = ((IActivationObject) base.engine.ScriptObjectStackTop()).GetDefaultThisObject();
            }
            if ((this.enclosing_scope is ClassScope) && (this.declaringObject == null))
            {
                if (thisob is Microsoft.JScript.StackFrame)
                {
                    thisob = ((Microsoft.JScript.StackFrame) thisob).closureInstance;
                }
                if (!this.func.isStatic && !((ClassScope) this.enclosing_scope).HasInstance(thisob))
                {
                    throw new JScriptException(JSError.InvalidCall);
                }
            }
            return this.func.Call(args, thisob, this.enclosing_scope, this, binder, culture);
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        internal Delegate ConvertToDelegate(Type delegateType)
        {
            return Delegate.CreateDelegate(delegateType, this.declaringObject, this.func.name);
        }

        public override string ToString()
        {
            return this.func.ToString();
        }
    }
}

