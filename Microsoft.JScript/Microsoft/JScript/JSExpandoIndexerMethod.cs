namespace Microsoft.JScript
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    internal sealed class JSExpandoIndexerMethod : JSMethod
    {
        private ClassScope classScope;
        private ParameterInfo[] GetterParams;
        private bool isGetter;
        private ParameterInfo[] SetterParams;
        private MethodInfo token;

        internal JSExpandoIndexerMethod(ClassScope classScope, bool isGetter) : base(null)
        {
            this.isGetter = isGetter;
            this.classScope = classScope;
            this.GetterParams = new ParameterInfo[] { new ParameterDeclaration(Typeob.String, "field") };
            this.SetterParams = new ParameterInfo[] { new ParameterDeclaration(Typeob.String, "field"), new ParameterDeclaration(Typeob.Object, "value") };
        }

        internal override object Construct(object[] args)
        {
            throw new JScriptException(JSError.InvalidCall);
        }

        internal override MethodInfo GetMethodInfo(CompilerGlobals compilerGlobals)
        {
            if (this.isGetter)
            {
                if (this.token == null)
                {
                    this.token = this.classScope.owner.GetExpandoIndexerGetter();
                }
            }
            else if (this.token == null)
            {
                this.token = this.classScope.owner.GetExpandoIndexerSetter();
            }
            return this.token;
        }

        public override ParameterInfo[] GetParameters()
        {
            if (this.isGetter)
            {
                return this.GetterParams;
            }
            return this.SetterParams;
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object Invoke(object obj, object thisob, BindingFlags options, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new JScriptException(JSError.InvalidCall);
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
                return this.classScope.GetTypeBuilderOrEnumBuilder();
            }
        }

        public override string Name
        {
            get
            {
                if (this.isGetter)
                {
                    return "get_Item";
                }
                return "set_Item";
            }
        }

        public override Type ReturnType
        {
            get
            {
                if (this.isGetter)
                {
                    return Typeob.Object;
                }
                return Typeob.Void;
            }
        }
    }
}

