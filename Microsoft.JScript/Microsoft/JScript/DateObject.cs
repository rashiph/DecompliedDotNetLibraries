namespace Microsoft.JScript
{
    using System;

    public class DateObject : JSObject
    {
        internal double value;

        internal DateObject(ScriptObject parent, double value) : base(parent)
        {
            this.value = (((value != value) || (value > 9.2233720368547758E+18)) || (value < -9.2233720368547758E+18)) ? double.NaN : Math.Round(value);
            base.noExpando = false;
        }

        internal override string GetClassName()
        {
            return "Date";
        }

        internal override object GetDefaultValue(PreferredType preferred_type)
        {
            if (base.GetParent() is LenientDatePrototype)
            {
                return base.GetDefaultValue(preferred_type);
            }
            if ((preferred_type == PreferredType.String) || (preferred_type == PreferredType.Either))
            {
                if (!base.noExpando && (base.NameTable["toString"] != null))
                {
                    return base.GetDefaultValue(preferred_type);
                }
                return DatePrototype.toString(this);
            }
            if (preferred_type == PreferredType.LocaleString)
            {
                if (!base.noExpando && (base.NameTable["toLocaleString"] != null))
                {
                    return base.GetDefaultValue(preferred_type);
                }
                return DatePrototype.toLocaleString(this);
            }
            if (!base.noExpando)
            {
                object obj4 = base.NameTable["valueOf"];
                if ((obj4 == null) && (preferred_type == PreferredType.Either))
                {
                    obj4 = base.NameTable["toString"];
                }
                if (obj4 != null)
                {
                    return base.GetDefaultValue(preferred_type);
                }
            }
            return this.value;
        }
    }
}

