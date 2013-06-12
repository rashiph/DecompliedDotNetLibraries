namespace Microsoft.VisualBasic
{
    using System;
    using System.Reflection;

    internal class VBTypeAttributeConverter : VBModifierAttributeConverter
    {
        private static VBTypeAttributeConverter defaultConverter;
        private static string[] names;
        private static object[] values;

        private VBTypeAttributeConverter()
        {
        }

        public static VBTypeAttributeConverter Default
        {
            get
            {
                if (defaultConverter == null)
                {
                    defaultConverter = new VBTypeAttributeConverter();
                }
                return defaultConverter;
            }
        }

        protected override object DefaultValue
        {
            get
            {
                return TypeAttributes.Public;
            }
        }

        protected override string[] Names
        {
            get
            {
                if (names == null)
                {
                    names = new string[] { "Public", "Friend" };
                }
                return names;
            }
        }

        protected override object[] Values
        {
            get
            {
                if (values == null)
                {
                    values = new object[] { TypeAttributes.Public, TypeAttributes.AnsiClass };
                }
                return values;
            }
        }
    }
}

