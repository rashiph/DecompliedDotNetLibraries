namespace Microsoft.CSharp
{
    using System;
    using System.Reflection;

    internal class CSharpTypeAttributeConverter : CSharpModifierAttributeConverter
    {
        private static CSharpTypeAttributeConverter defaultConverter;
        private static string[] names;
        private static object[] values;

        private CSharpTypeAttributeConverter()
        {
        }

        public static CSharpTypeAttributeConverter Default
        {
            get
            {
                if (defaultConverter == null)
                {
                    defaultConverter = new CSharpTypeAttributeConverter();
                }
                return defaultConverter;
            }
        }

        protected override object DefaultValue
        {
            get
            {
                return TypeAttributes.AnsiClass;
            }
        }

        protected override string[] Names
        {
            get
            {
                if (names == null)
                {
                    names = new string[] { "Public", "Internal" };
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

