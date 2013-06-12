namespace Microsoft.CSharp
{
    using System;
    using System.CodeDom;

    internal class CSharpMemberAttributeConverter : CSharpModifierAttributeConverter
    {
        private static CSharpMemberAttributeConverter defaultConverter;
        private static string[] names;
        private static object[] values;

        private CSharpMemberAttributeConverter()
        {
        }

        public static CSharpMemberAttributeConverter Default
        {
            get
            {
                if (defaultConverter == null)
                {
                    defaultConverter = new CSharpMemberAttributeConverter();
                }
                return defaultConverter;
            }
        }

        protected override object DefaultValue
        {
            get
            {
                return MemberAttributes.Private;
            }
        }

        protected override string[] Names
        {
            get
            {
                if (names == null)
                {
                    names = new string[] { "Public", "Protected", "Protected Internal", "Internal", "Private" };
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
                    values = new object[] { MemberAttributes.Public, MemberAttributes.Family, MemberAttributes.FamilyOrAssembly, MemberAttributes.Assembly, MemberAttributes.Private };
                }
                return values;
            }
        }
    }
}

