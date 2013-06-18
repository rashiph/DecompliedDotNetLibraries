namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;

    public sealed class GenericEnumConverter : ConfigurationConverterBase
    {
        private Type _enumType;

        public GenericEnumConverter(Type typeEnum)
        {
            if (typeEnum == null)
            {
                throw new ArgumentNullException("typeEnum");
            }
            this._enumType = typeEnum;
        }

        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            object obj2 = null;
            try
            {
                string str = (string) data;
                if (string.IsNullOrEmpty(str))
                {
                    throw new Exception();
                }
                if (!string.IsNullOrEmpty(str) && ((char.IsDigit(str[0]) || (str[0] == '-')) || (str[0] == '+')))
                {
                    throw new Exception();
                }
                if (str != str.Trim())
                {
                    throw new Exception();
                }
                obj2 = Enum.Parse(this._enumType, str);
            }
            catch
            {
                StringBuilder builder = new StringBuilder();
                foreach (string str2 in Enum.GetNames(this._enumType))
                {
                    if (builder.Length != 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(str2);
                }
                throw new ArgumentException(System.Configuration.SR.GetString("Invalid_enum_value", new object[] { builder.ToString() }));
            }
            return obj2;
        }

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            return value.ToString();
        }
    }
}

