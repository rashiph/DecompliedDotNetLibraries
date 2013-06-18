namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.ServiceModel;

    internal class TransactionProtocolConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((typeof(string) == sourceType) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((typeof(InstanceDescriptor) == destinationType) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;
            switch (str)
            {
                case "OleTransactions":
                    return TransactionProtocol.OleTransactions;

                case "WSAtomicTransactionOctober2004":
                    return TransactionProtocol.WSAtomicTransactionOctober2004;

                case "WSAtomicTransaction11":
                    return TransactionProtocol.WSAtomicTransaction11;

                case null:
                    return base.ConvertFrom(context, culture, value);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigInvalidTransactionFlowProtocolValue", new object[] { str }));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((typeof(string) == destinationType) && (value is TransactionProtocol))
            {
                TransactionProtocol protocol = (TransactionProtocol) value;
                return protocol.Name;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

