namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities;
    using System.Windows.Markup;

    public class ArgumentValueSerializer : ValueSerializer
    {
        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            Argument argument = value as Argument;
            if (argument == null)
            {
                return false;
            }
            return argument.CanConvertToString(context);
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            Argument argument = value as Argument;
            if (argument == null)
            {
                throw FxTrace.Exception.Argument("value", System.Activities.SR.CannotSerializeExpression(value.GetType()));
            }
            return argument.ConvertToString(context);
        }
    }
}

