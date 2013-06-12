namespace System.Reflection
{
    using System;

    [Serializable]
    internal struct CustomAttributeCtorParameter
    {
        private System.Reflection.CustomAttributeEncodedArgument m_encodedArgument;
        private CustomAttributeType m_type;

        public CustomAttributeCtorParameter(CustomAttributeType type)
        {
            this.m_type = type;
            this.m_encodedArgument = new System.Reflection.CustomAttributeEncodedArgument();
        }

        public System.Reflection.CustomAttributeEncodedArgument CustomAttributeEncodedArgument
        {
            get
            {
                return this.m_encodedArgument;
            }
        }
    }
}

