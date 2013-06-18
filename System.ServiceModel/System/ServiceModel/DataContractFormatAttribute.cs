namespace System.ServiceModel
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
    public sealed class DataContractFormatAttribute : Attribute
    {
        private OperationFormatStyle style;

        public OperationFormatStyle Style
        {
            get
            {
                return this.style;
            }
            set
            {
                XmlSerializerFormatAttribute.ValidateOperationFormatStyle(this.style);
                this.style = value;
            }
        }
    }
}

