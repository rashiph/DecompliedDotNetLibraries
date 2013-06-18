namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [MarkupExtensionReturnType(typeof(object)), TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class NullExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return null;
        }
    }
}

