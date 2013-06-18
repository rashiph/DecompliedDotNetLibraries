namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;

    public sealed class StackFrame : ScriptObject, IActivationObject
    {
        internal ArgumentsObject caller_arguments;
        public object closureInstance;
        private JSLocalField[] fields;
        public object[] localVars;
        private FunctionScope nestedFunctionScope;
        internal object thisObject;

        internal StackFrame(ScriptObject parent, JSLocalField[] fields, object[] local_vars, object thisObject) : base(parent)
        {
            this.caller_arguments = null;
            this.fields = fields;
            this.localVars = local_vars;
            this.nestedFunctionScope = null;
            this.thisObject = thisObject;
            if (parent is Microsoft.JScript.StackFrame)
            {
                this.closureInstance = ((Microsoft.JScript.StackFrame) parent).closureInstance;
            }
            else if (parent is JSObject)
            {
                this.closureInstance = parent;
            }
            else
            {
                this.closureInstance = null;
            }
        }

        internal JSVariableField AddNewField(string name, object value, FieldAttributes attributeFlags)
        {
            this.AllocateFunctionScope();
            return this.nestedFunctionScope.AddNewField(name, value, attributeFlags);
        }

        private void AllocateFunctionScope()
        {
            if (this.nestedFunctionScope == null)
            {
                this.nestedFunctionScope = new FunctionScope(base.parent);
                if (this.fields != null)
                {
                    int index = 0;
                    int length = this.fields.Length;
                    while (index < length)
                    {
                        this.nestedFunctionScope.AddOuterScopeField(this.fields[index].Name, this.fields[index]);
                        index++;
                    }
                }
            }
        }

        public object GetDefaultThisObject()
        {
            ScriptObject parent = base.GetParent();
            IActivationObject obj3 = parent as IActivationObject;
            if (obj3 != null)
            {
                return obj3.GetDefaultThisObject();
            }
            return parent;
        }

        public FieldInfo GetField(string name, int lexLevel)
        {
            return null;
        }

        public GlobalScope GetGlobalScope()
        {
            return ((IActivationObject) base.GetParent()).GetGlobalScope();
        }

        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            this.AllocateFunctionScope();
            return this.nestedFunctionScope.GetMember(name, bindingAttr);
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            this.AllocateFunctionScope();
            return this.nestedFunctionScope.GetMembers(bindingAttr);
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object GetMemberValue(string name)
        {
            this.AllocateFunctionScope();
            return this.nestedFunctionScope.GetMemberValue(name);
        }

        [DebuggerHidden, DebuggerStepThrough]
        public object GetMemberValue(string name, int lexlevel)
        {
            if (lexlevel <= 0)
            {
                return Microsoft.JScript.Missing.Value;
            }
            if (this.nestedFunctionScope != null)
            {
                return this.nestedFunctionScope.GetMemberValue(name, lexlevel);
            }
            return ((IActivationObject) base.parent).GetMemberValue(name, lexlevel - 1);
        }

        internal override void GetPropertyEnumerator(ArrayList enums, ArrayList objects)
        {
            throw new JScriptException(JSError.InternalError);
        }

        FieldInfo IActivationObject.GetLocalField(string name)
        {
            this.AllocateFunctionScope();
            return this.nestedFunctionScope.GetLocalField(name);
        }

        public static void PushStackFrameForMethod(object thisob, JSLocalField[] fields, VsaEngine engine)
        {
            Globals globals = engine.Globals;
            IActivationObject obj2 = (IActivationObject) globals.ScopeStack.Peek();
            string name = thisob.GetType().Namespace;
            WithObject parent = null;
            if ((name != null) && (name.Length > 0))
            {
                parent = new WithObject(obj2.GetGlobalScope(), new WrappedNamespace(name, engine)) {
                    isKnownAtCompileTime = true
                };
                parent = new WithObject(parent, thisob);
            }
            else
            {
                parent = new WithObject(obj2.GetGlobalScope(), thisob);
            }
            parent.isKnownAtCompileTime = true;
            Microsoft.JScript.StackFrame item = new Microsoft.JScript.StackFrame(parent, fields, new object[fields.Length], thisob) {
                closureInstance = thisob
            };
            globals.ScopeStack.GuardedPush(item);
        }

        public static void PushStackFrameForStaticMethod(RuntimeTypeHandle thisclass, JSLocalField[] fields, VsaEngine engine)
        {
            PushStackFrameForMethod(Type.GetTypeFromHandle(thisclass), fields, engine);
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override void SetMemberValue(string name, object value)
        {
            this.AllocateFunctionScope();
            this.nestedFunctionScope.SetMemberValue(name, value, this);
        }
    }
}

