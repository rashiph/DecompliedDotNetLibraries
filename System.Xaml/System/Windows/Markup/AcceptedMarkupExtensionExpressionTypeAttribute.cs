namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [Obsolete("This is not used by the XAML parser. Please look at XamlSetMarkupExtensionAttribute."), AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=true)]
    public class AcceptedMarkupExtensionExpressionTypeAttribute : Attribute
    {
        public AcceptedMarkupExtensionExpressionTypeAttribute(System.Type type)
        {
            this.Type = type;
        }

        public System.Type Type { get; set; }
    }
}

