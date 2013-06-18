namespace System.Xaml
{
    using System;

    public abstract class XamlDeferringLoader
    {
        protected XamlDeferringLoader()
        {
        }

        public abstract object Load(XamlReader xamlReader, IServiceProvider serviceProvider);
        public abstract XamlReader Save(object value, IServiceProvider serviceProvider);
    }
}

