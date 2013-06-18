namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities;
    using System.Windows.Markup;
    using System.Xaml;

    public sealed class ActivityWithResultValueSerializer : ValueSerializer
    {
        private static ActivityWithResultValueSerializer valueSerializer;

        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            if (AttachablePropertyServices.GetAttachedPropertyCount(value) > 0)
            {
                return false;
            }
            return (((value != null) && (value is IValueSerializableExpression)) && ((IValueSerializableExpression) value).CanConvertToString(context));
        }

        internal static bool CanConvertToStringWrapper(object value, IValueSerializerContext context)
        {
            if (valueSerializer == null)
            {
                valueSerializer = new ActivityWithResultValueSerializer();
            }
            return valueSerializer.CanConvertToString(value, context);
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            IValueSerializableExpression expression = value as IValueSerializableExpression;
            if (expression == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotSerializeExpression(value.GetType())));
            }
            return expression.ConvertToString(context);
        }

        internal static string ConvertToStringWrapper(object value, IValueSerializerContext context)
        {
            if (valueSerializer == null)
            {
                valueSerializer = new ActivityWithResultValueSerializer();
            }
            return valueSerializer.ConvertToString(value, context);
        }
    }
}

