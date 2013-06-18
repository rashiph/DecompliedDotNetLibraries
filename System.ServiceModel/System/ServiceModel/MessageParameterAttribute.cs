namespace System.ServiceModel
{
    using System;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter, Inherited=false)]
    public sealed class MessageParameterAttribute : Attribute
    {
        private bool isNameSetExplicit;
        private string name;
        internal const string NamePropertyName = "Name";

        internal bool IsNameSetExplicit
        {
            get
            {
                return this.isNameSetExplicit;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (value == string.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("SFxNameCannotBeEmpty")));
                }
                this.name = value;
                this.isNameSetExplicit = true;
            }
        }
    }
}

