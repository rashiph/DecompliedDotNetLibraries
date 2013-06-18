namespace Microsoft.JScript
{
    using System;

    public class NumberObject : JSObject
    {
        internal Type baseType;
        private bool implicitWrapper;
        internal object value;

        protected NumberObject(ScriptObject parent, object value) : base(parent)
        {
            this.baseType = Globals.TypeRefs.ToReferenceContext(value.GetType());
            this.value = value;
            base.noExpando = false;
            this.implicitWrapper = false;
        }

        internal NumberObject(ScriptObject parent, Type baseType) : base(parent)
        {
            this.baseType = baseType;
            this.value = 0.0;
            base.noExpando = false;
        }

        internal NumberObject(ScriptObject parent, object value, bool implicitWrapper) : base(parent, typeof(NumberObject))
        {
            this.baseType = Globals.TypeRefs.ToReferenceContext(value.GetType());
            this.value = value;
            base.noExpando = implicitWrapper;
            this.implicitWrapper = implicitWrapper;
        }

        internal override string GetClassName()
        {
            return "Number";
        }

        internal override object GetDefaultValue(PreferredType preferred_type)
        {
            if (base.GetParent() is LenientNumberPrototype)
            {
                return base.GetDefaultValue(preferred_type);
            }
            if (preferred_type == PreferredType.String)
            {
                if (!base.noExpando && (base.NameTable["toString"] != null))
                {
                    return base.GetDefaultValue(preferred_type);
                }
                return Microsoft.JScript.Convert.ToString(this.value);
            }
            if (preferred_type == PreferredType.LocaleString)
            {
                return base.GetDefaultValue(preferred_type);
            }
            if (!base.noExpando)
            {
                object obj3 = base.NameTable["valueOf"];
                if ((obj3 == null) && (preferred_type == PreferredType.Either))
                {
                    obj3 = base.NameTable["toString"];
                }
                if (obj3 != null)
                {
                    return base.GetDefaultValue(preferred_type);
                }
            }
            return this.value;
        }

        public Type GetType()
        {
            if (!this.implicitWrapper)
            {
                return Typeob.NumberObject;
            }
            return this.baseType;
        }
    }
}

