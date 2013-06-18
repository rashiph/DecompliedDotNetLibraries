namespace System.Activities.XamlIntegration
{
    using System;
    using System.Windows.Markup;

    public interface IValueSerializableExpression
    {
        bool CanConvertToString(IValueSerializerContext context);
        string ConvertToString(IValueSerializerContext context);
    }
}

