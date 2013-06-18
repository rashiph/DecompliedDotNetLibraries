namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class CharArrayType
    {
        private CharArrayType()
        {
        }

        public static char[] FromObject(object Value)
        {
            if (Value == null)
            {
                return "".ToCharArray();
            }
            char[] chArray = Value as char[];
            if ((chArray != null) && (chArray.Rank == 1))
            {
                return chArray;
            }
            IConvertible convertible = Value as IConvertible;
            if ((convertible == null) || (convertible.GetTypeCode() != TypeCode.String))
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(Value), "Char()" }));
            }
            return convertible.ToString(null).ToCharArray();
        }

        public static char[] FromString(string Value)
        {
            if (Value == null)
            {
                Value = "";
            }
            return Value.ToCharArray();
        }
    }
}

