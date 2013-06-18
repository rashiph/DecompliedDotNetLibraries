namespace Microsoft.JScript
{
    using System;

    public class StringObject : JSObject
    {
        private bool implicitWrapper;
        internal string value;

        protected StringObject(ScriptObject prototype, string value) : base(prototype)
        {
            this.value = value;
            base.noExpando = false;
            this.implicitWrapper = false;
        }

        internal StringObject(ScriptObject prototype, string value, bool implicitWrapper) : base(prototype, typeof(StringObject))
        {
            this.value = value;
            base.noExpando = implicitWrapper;
            this.implicitWrapper = implicitWrapper;
        }

        public override bool Equals(object ob)
        {
            if (ob is StringObject)
            {
                ob = ((StringObject) ob).value;
            }
            return this.value.Equals(ob);
        }

        internal override string GetClassName()
        {
            return "String";
        }

        internal override object GetDefaultValue(PreferredType preferred_type)
        {
            if (base.GetParent() is LenientStringPrototype)
            {
                return base.GetDefaultValue(preferred_type);
            }
            if (preferred_type == PreferredType.String)
            {
                if (!base.noExpando && (base.NameTable["toString"] != null))
                {
                    return base.GetDefaultValue(preferred_type);
                }
                return this.value;
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

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public Type GetType()
        {
            if (!this.implicitWrapper)
            {
                return Typeob.StringObject;
            }
            return Typeob.String;
        }

        internal override object GetValueAtIndex(uint index)
        {
            if (this.implicitWrapper && (index < this.value.Length))
            {
                return this.value[(int) index];
            }
            return base.GetValueAtIndex(index);
        }

        public int length
        {
            get
            {
                return this.value.Length;
            }
        }
    }
}

