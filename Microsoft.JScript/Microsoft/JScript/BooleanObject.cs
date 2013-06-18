namespace Microsoft.JScript
{
    using System;

    public class BooleanObject : JSObject
    {
        private bool implicitWrapper;
        internal bool value;

        protected BooleanObject(ScriptObject prototype, Type subType) : base(prototype, subType)
        {
            this.value = this.value;
            base.noExpando = false;
            this.implicitWrapper = false;
        }

        internal BooleanObject(ScriptObject prototype, bool value, bool implicitWrapper) : base(prototype, typeof(BooleanObject))
        {
            this.value = value;
            base.noExpando = implicitWrapper;
            this.implicitWrapper = implicitWrapper;
        }

        internal override string GetClassName()
        {
            return "Boolean";
        }

        internal override object GetDefaultValue(PreferredType preferred_type)
        {
            if (base.GetParent() is LenientBooleanPrototype)
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
                return Typeob.BooleanObject;
            }
            return Typeob.Boolean;
        }
    }
}

