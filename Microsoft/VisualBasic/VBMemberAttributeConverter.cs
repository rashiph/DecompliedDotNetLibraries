namespace Microsoft.VisualBasic
{
    using System;
    using System.CodeDom;

    internal class VBMemberAttributeConverter : VBModifierAttributeConverter
    {
        private static VBMemberAttributeConverter defaultConverter;
        private static string[] names;
        private static object[] values;

        private VBMemberAttributeConverter()
        {
        }

        public static VBMemberAttributeConverter Default
        {
            get
            {
                if (defaultConverter == null)
                {
                    defaultConverter = new VBMemberAttributeConverter();
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
                    names = new string[] { "Public", "Protected", "Protected Friend", "Friend", "Private" };
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

