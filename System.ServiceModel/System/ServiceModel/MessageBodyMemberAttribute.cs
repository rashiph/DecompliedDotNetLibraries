namespace System.ServiceModel
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited=false)]
    public class MessageBodyMemberAttribute : MessageContractMemberAttribute
    {
        private int order = -1;
        internal const string OrderPropertyName = "Order";

        public int Order
        {
            get
            {
                return this.order;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.order = value;
            }
        }
    }
}

