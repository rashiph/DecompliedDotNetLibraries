namespace System.Xaml
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Xaml.Schema;

    internal class EventConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string methodName = value as string;
            if (methodName != null)
            {
                object rootObject = null;
                Type delegateType = null;
                GetRootObjectAndDelegateType(context, out rootObject, out delegateType);
                if ((rootObject != null) && (delegateType != null))
                {
                    return SafeReflectionInvoker.CreateDelegate(delegateType, rootObject, methodName);
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        internal static void GetRootObjectAndDelegateType(ITypeDescriptorContext context, out object rootObject, out Type delegateType)
        {
            rootObject = null;
            delegateType = null;
            if (context != null)
            {
                IRootObjectProvider service = context.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
                if (service != null)
                {
                    rootObject = service.RootObject;
                    IDestinationTypeProvider provider2 = context.GetService(typeof(IDestinationTypeProvider)) as IDestinationTypeProvider;
                    if (provider2 != null)
                    {
                        delegateType = provider2.GetDestinationType();
                    }
                }
            }
        }
    }
}

