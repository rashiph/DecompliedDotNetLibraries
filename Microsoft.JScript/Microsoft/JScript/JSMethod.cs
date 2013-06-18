namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("561AC104-8869-4368-902F-4E0D7DDEDDDD")]
    public abstract class JSMethod : MethodInfo
    {
        internal object obj;

        internal JSMethod(object obj)
        {
            this.obj = obj;
        }

        internal abstract object Construct(object[] args);
        public override MethodInfo GetBaseDefinition()
        {
            return this;
        }

        internal virtual string GetClassFullName()
        {
            if (!(this.obj is ClassScope))
            {
                throw new JScriptException(JSError.InternalError);
            }
            return ((ClassScope) this.obj).GetFullName();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[0];
        }

        public override object[] GetCustomAttributes(Type t, bool inherit)
        {
            return new object[0];
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return MethodImplAttributes.IL;
        }

        internal abstract MethodInfo GetMethodInfo(CompilerGlobals compilerGlobals);
        internal virtual PackageScope GetPackage()
        {
            if (!(this.obj is ClassScope))
            {
                throw new JScriptException(JSError.InternalError);
            }
            return ((ClassScope) this.obj).GetPackage();
        }

        [DebuggerStepThrough, DebuggerHidden]
        public override object Invoke(object obj, BindingFlags options, Binder binder, object[] parameters, CultureInfo culture)
        {
            return this.Invoke(obj, obj, options, binder, parameters, culture);
        }

        internal abstract object Invoke(object obj, object thisob, BindingFlags options, Binder binder, object[] parameters, CultureInfo culture);
        public sealed override bool IsDefined(Type type, bool inherit)
        {
            return false;
        }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Method;
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                return this.GetMethodInfo(null).MethodHandle;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.DeclaringType;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return null;
            }
        }
    }
}

