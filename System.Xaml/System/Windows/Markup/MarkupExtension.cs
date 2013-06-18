namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class MarkupExtension
    {
        protected MarkupExtension()
        {
        }

        public abstract object ProvideValue(IServiceProvider serviceProvider);
    }
}

