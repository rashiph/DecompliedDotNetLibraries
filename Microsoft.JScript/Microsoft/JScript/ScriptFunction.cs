namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    public abstract class ScriptFunction : JSObject
    {
        protected int ilength;
        internal string name;
        internal object proto;

        internal ScriptFunction(ScriptObject parent) : base(parent)
        {
            this.ilength = 0;
            this.name = "Function.prototype";
            this.proto = Microsoft.JScript.Missing.Value;
        }

        protected ScriptFunction(ScriptObject parent, string name) : base(parent, typeof(ScriptFunction))
        {
            this.ilength = 0;
            this.name = name;
            this.proto = new JSPrototypeObject(parent.GetParent(), this);
        }

        internal ScriptFunction(ScriptObject parent, string name, int length) : base(parent)
        {
            this.ilength = length;
            this.name = name;
            this.proto = new JSPrototypeObject(parent.GetParent(), this);
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal abstract object Call(object[] args, object thisob);
        internal virtual object Call(object[] args, object thisob, Binder binder, CultureInfo culture)
        {
            return this.Call(args, thisob);
        }

        internal virtual object Call(object[] args, object thisob, ScriptObject enclosing_scope, Closure calleeClosure, Binder binder, CultureInfo culture)
        {
            return this.Call(args, thisob);
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal virtual object Construct(object[] args)
        {
            JSObject thisob = new JSObject(null, false);
            thisob.SetParent(this.GetPrototypeForConstructedObject());
            object obj3 = this.Call(args, thisob);
            if (!(obj3 is ScriptObject) && (!(this is BuiltinFunction) || !((BuiltinFunction) this).method.Name.Equals("CreateInstance")))
            {
                return thisob;
            }
            return obj3;
        }

        [DebuggerStepThrough, JSFunction(JSFunctionAttributeEnum.HasVarArgs), DebuggerHidden]
        public object CreateInstance(params object[] args)
        {
            return this.Construct(args);
        }

        internal override string GetClassName()
        {
            return "Function";
        }

        internal virtual int GetNumberOfFormalParameters()
        {
            return this.ilength;
        }

        protected ScriptObject GetPrototypeForConstructedObject()
        {
            object proto = this.proto;
            if (proto is JSObject)
            {
                return (JSObject) proto;
            }
            if (proto is ClassScope)
            {
                return (ClassScope) proto;
            }
            return (ObjectPrototype) base.GetParent().GetParent();
        }

        internal virtual bool HasInstance(object ob)
        {
            if (ob is JSObject)
            {
                object proto = this.proto;
                if (!(proto is ScriptObject))
                {
                    throw new JScriptException(JSError.InvalidPrototype);
                }
                ScriptObject parent = ((JSObject) ob).GetParent();
                ScriptObject obj4 = (ScriptObject) proto;
                while (parent != null)
                {
                    if (parent == obj4)
                    {
                        return true;
                    }
                    if (parent is WithObject)
                    {
                        object obj5 = ((WithObject) parent).contained_object;
                        if ((obj5 == obj4) && (obj5 is ClassScope))
                        {
                            return true;
                        }
                    }
                    parent = parent.GetParent();
                }
            }
            return false;
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs | JSFunctionAttributeEnum.HasThisObject), DebuggerStepThrough, DebuggerHidden]
        public object Invoke(object thisob, params object[] args)
        {
            return this.Call(args, thisob);
        }

        [DebuggerHidden, DebuggerStepThrough]
        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            if (target != this)
            {
                throw new TargetException();
            }
            string str = "this";
            if (name.Equals("[DISPID=0]"))
            {
                name = string.Empty;
                if (namedParameters != null)
                {
                    str = "[DISPID=-613]";
                }
            }
            if ((name == null) || (name == string.Empty))
            {
                if ((invokeAttr & BindingFlags.CreateInstance) != BindingFlags.Default)
                {
                    if ((invokeAttr & (BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.GetField | BindingFlags.InvokeMethod)) != BindingFlags.Default)
                    {
                        throw new ArgumentException();
                    }
                    return this.Construct(args);
                }
                if ((invokeAttr & BindingFlags.InvokeMethod) != BindingFlags.Default)
                {
                    object thisob = null;
                    if (namedParameters != null)
                    {
                        int index = Array.IndexOf<string>(namedParameters, str);
                        if (index == 0)
                        {
                            thisob = args[0];
                            int n = args.Length - 1;
                            object[] objArray = new object[n];
                            ArrayObject.Copy(args, 1, objArray, 0, n);
                            args = objArray;
                        }
                        if ((index != 0) || (namedParameters.Length != 1))
                        {
                            throw new ArgumentException();
                        }
                    }
                    if ((args.Length > 0) || ((invokeAttr & (BindingFlags.GetProperty | BindingFlags.GetField)) == BindingFlags.Default))
                    {
                        return this.Call(args, thisob, binder, culture);
                    }
                }
            }
            return base.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        public override string ToString()
        {
            return ("function " + this.name + "() {\n    [native code]\n}");
        }

        public virtual int length
        {
            get
            {
                return this.ilength;
            }
            set
            {
            }
        }

        public object prototype
        {
            get
            {
                return this.proto;
            }
            set
            {
                if (!base.noExpando)
                {
                    this.proto = value;
                }
            }
        }
    }
}

