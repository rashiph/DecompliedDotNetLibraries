namespace System.Reflection
{
    using System;

    [Serializable]
    internal struct CustomAttributeNamedParameter
    {
        private string m_argumentName;
        private CustomAttributeEncodedArgument m_encodedArgument;
        private CustomAttributeEncoding m_fieldOrProperty;
        private CustomAttributeEncoding m_padding;
        private CustomAttributeType m_type;

        public CustomAttributeNamedParameter(string argumentName, CustomAttributeEncoding fieldOrProperty, CustomAttributeType type)
        {
            if (argumentName == null)
            {
                throw new ArgumentNullException("argumentName");
            }
            this.m_argumentName = argumentName;
            this.m_fieldOrProperty = fieldOrProperty;
            this.m_padding = fieldOrProperty;
            this.m_type = type;
            this.m_encodedArgument = new CustomAttributeEncodedArgument();
        }

        public CustomAttributeEncodedArgument EncodedArgument
        {
            get
            {
                return this.m_encodedArgument;
            }
        }
    }
}

