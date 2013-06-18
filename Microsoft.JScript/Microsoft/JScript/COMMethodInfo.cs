namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("C7B9C313-2FD4-4384-8571-7ABC08BD17E5")]
    public class COMMethodInfo : JSMethod, MemberInfoInitializer
    {
        protected COMMemberInfo _comObject;
        protected string _name;
        protected static readonly ParameterInfo[] EmptyParams = new ParameterInfo[0];

        public COMMethodInfo() : base(null)
        {
            this._comObject = null;
            this._name = null;
        }

        internal override object Construct(object[] args)
        {
            return this._comObject.Call(BindingFlags.CreateInstance, null, (args != null) ? args : new object[0], null);
        }

        public override MethodInfo GetBaseDefinition()
        {
            return this;
        }

        public COMMemberInfo GetCOMMemberInfo()
        {
            return this._comObject;
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return MethodImplAttributes.IL;
        }

        internal override MethodInfo GetMethodInfo(CompilerGlobals compilerGlobals)
        {
            return null;
        }

        public override ParameterInfo[] GetParameters()
        {
            return EmptyParams;
        }

        public virtual void Initialize(string name, COMMemberInfo dispatch)
        {
            this._name = name;
            this._comObject = dispatch;
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return this._comObject.Call(invokeAttr, binder, (parameters != null) ? parameters : new object[0], culture);
        }

        internal override object Invoke(object obj, object thisob, BindingFlags options, Binder binder, object[] parameters, CultureInfo culture)
        {
            return this.Invoke(thisob, options, binder, parameters, culture);
        }

        public override string ToString()
        {
            return "";
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return MethodAttributes.Public;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return null;
            }
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
                throw new JScriptException(JSError.InternalError);
            }
        }

        public override string Name
        {
            get
            {
                return this._name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return null;
            }
        }

        public override Type ReturnType
        {
            get
            {
                return null;
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

