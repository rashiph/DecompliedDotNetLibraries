namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Xaml;

    public abstract class TypeConverterBase : TypeConverter
    {
        private Type baseType;
        private TypeConverterHelper helper;
        private Lazy<ConcurrentDictionary<Type, TypeConverterHelper>> helpers;
        private Type helperType;

        internal TypeConverterBase(Type baseType, Type helperType)
        {
            this.helpers = new Lazy<ConcurrentDictionary<Type, TypeConverterHelper>>();
            this.baseType = baseType;
            this.helperType = helperType;
        }

        internal TypeConverterBase(Type targetType, Type baseType, Type helperType)
        {
            this.helpers = new Lazy<ConcurrentDictionary<Type, TypeConverterHelper>>();
            this.helper = this.GetTypeConverterHelper(targetType, baseType, helperType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == TypeHelper.StringType) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == TypeHelper.StringType)
            {
                return false;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string text = value as string;
            if (text == null)
            {
                return base.ConvertFrom(context, culture, value);
            }
            TypeConverterHelper helper = this.helper;
            if (helper == null)
            {
                Type destinationType = (context.GetService(typeof(IDestinationTypeProvider)) as IDestinationTypeProvider).GetDestinationType();
                if (!this.helpers.Value.TryGetValue(destinationType, out helper))
                {
                    helper = this.GetTypeConverterHelper(destinationType, this.baseType, this.helperType);
                    if (!this.helpers.Value.TryAdd(destinationType, helper) && !this.helpers.Value.TryGetValue(destinationType, out helper))
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.TypeConverterHelperCacheAddFailed(destinationType)));
                    }
                }
            }
            return helper.UntypedConvertFromString(text, context);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }

        private TypeConverterHelper GetTypeConverterHelper(Type targetType, Type baseType, Type helperType)
        {
            Type[] genericArguments;
            if (!(baseType.BaseType == targetType))
            {
                while (!targetType.IsGenericType || !(targetType.GetGenericTypeDefinition() == baseType))
                {
                    if (targetType == TypeHelper.ObjectType)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidTypeConverterUsage));
                    }
                    targetType = targetType.BaseType;
                }
                genericArguments = targetType.GetGenericArguments();
            }
            else
            {
                genericArguments = new Type[] { TypeHelper.ObjectType };
            }
            return (TypeConverterHelper) Activator.CreateInstance(helperType.MakeGenericType(genericArguments));
        }

        internal abstract class TypeConverterHelper
        {
            protected TypeConverterHelper()
            {
            }

            public static T GetService<T>(ITypeDescriptorContext context) where T: class
            {
                T service = (T) context.GetService(typeof(T));
                if (service == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidTypeConverterUsage));
                }
                return service;
            }

            public abstract object UntypedConvertFromString(string text, ITypeDescriptorContext context);
        }

        internal abstract class TypeConverterHelper<T> : TypeConverterBase.TypeConverterHelper
        {
            protected TypeConverterHelper()
            {
            }

            public abstract T ConvertFromString(string text, ITypeDescriptorContext context);
            public sealed override object UntypedConvertFromString(string text, ITypeDescriptorContext context)
            {
                return this.ConvertFromString(text, context);
            }
        }
    }
}

