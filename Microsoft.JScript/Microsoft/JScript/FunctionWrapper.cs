namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;

    public sealed class FunctionWrapper : ScriptFunction
    {
        private MemberInfo[] members;
        private object obj;

        internal FunctionWrapper(string name, object obj, MemberInfo[] members) : base(FunctionPrototype.ob, name, 0)
        {
            this.obj = obj;
            this.members = members;
            foreach (MemberInfo info in members)
            {
                if (info is MethodInfo)
                {
                    base.ilength = ((MethodInfo) info).GetParameters().Length;
                    return;
                }
            }
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal override object Call(object[] args, object thisob)
        {
            return this.Call(args, thisob, null, null);
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal override object Call(object[] args, object thisob, Binder binder, CultureInfo culture)
        {
            MethodInfo info = this.members[0] as MethodInfo;
            if (((thisob is GlobalScope) || (thisob == null)) || ((info != null) && ((info.Attributes & MethodAttributes.Static) != MethodAttributes.PrivateScope)))
            {
                thisob = this.obj;
            }
            else if (!this.obj.GetType().IsInstanceOfType(thisob) && !(this.obj is ClassScope))
            {
                if (this.members.Length == 1)
                {
                    JSWrappedMethod method = this.members[0] as JSWrappedMethod;
                    if ((method != null) && (method.DeclaringType == Typeob.Object))
                    {
                        return LateBinding.CallOneOfTheMembers(new MemberInfo[] { method.method }, args, false, thisob, binder, culture, null, base.engine);
                    }
                }
                throw new JScriptException(JSError.TypeMismatch);
            }
            return LateBinding.CallOneOfTheMembers(this.members, args, false, thisob, binder, culture, null, base.engine);
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        internal Delegate ConvertToDelegate(Type delegateType)
        {
            return Delegate.CreateDelegate(delegateType, this.obj, base.name);
        }

        public override string ToString()
        {
            Type declaringType = this.members[0].DeclaringType;
            MethodInfo info = (declaringType == null) ? null : declaringType.GetMethod(base.name + " source");
            if (info != null)
            {
                return (string) info.Invoke(null, null);
            }
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (MemberInfo info2 in this.members)
            {
                if ((info2 is MethodInfo) || ((info2 is PropertyInfo) && (JSProperty.GetGetMethod((PropertyInfo) info2, false) != null)))
                {
                    if (!flag)
                    {
                        builder.Append("\n");
                    }
                    else
                    {
                        flag = false;
                    }
                    builder.Append(info2.ToString());
                }
            }
            if (builder.Length > 0)
            {
                return builder.ToString();
            }
            return ("function " + base.name + "() {\n    [native code]\n}");
        }
    }
}

