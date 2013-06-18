namespace Microsoft.VisualBasic.Activities.XamlIntegration
{
    using Microsoft.VisualBasic.Activities;
    using System;
    using System.Windows.Markup;

    public sealed class VisualBasicSettingsValueSerializer : ValueSerializer
    {
        internal const string ImplementationVisualBasicSettingsValue = "Assembly references and imported namespaces for internal implementation";
        internal const string VisualBasicSettingsValue = "Assembly references and imported namespaces serialized as XML namespaces";

        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            VisualBasicSettings settings = value as VisualBasicSettings;
            if (settings != null)
            {
                settings.GenerateXamlReferences(context);
            }
            return true;
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            VisualBasicSettings settings = value as VisualBasicSettings;
            if ((settings != null) && settings.SuppressXamlSerialization)
            {
                return "Assembly references and imported namespaces for internal implementation";
            }
            return "Assembly references and imported namespaces serialized as XML namespaces";
        }
    }
}

