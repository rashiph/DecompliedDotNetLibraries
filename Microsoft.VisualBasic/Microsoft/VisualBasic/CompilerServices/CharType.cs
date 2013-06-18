namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CharType
    {
        private CharType()
        {
        }

        public static char FromObject(object Value)
        {
            if (Value == null)
            {
                return '\0';
            }
            IConvertible convertible = Value as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Char:
                        return convertible.ToChar(null);

                    case TypeCode.String:
                        return FromString(convertible.ToString(null));
                }
            }
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Char" }));
        }

        public static char FromString(string Value)
        {
            if ((Value != null) && (Value.Length != 0))
            {
                return Value[0];
            }
            return '\0';
        }
    }
}

