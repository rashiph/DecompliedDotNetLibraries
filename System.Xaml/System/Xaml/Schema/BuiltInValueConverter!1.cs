namespace System.Xaml.Schema
{
    using System;

    internal class BuiltInValueConverter<TConverterBase> : XamlValueConverter<TConverterBase> where TConverterBase: class
    {
        private Func<TConverterBase> _factory;

        internal BuiltInValueConverter(Type converterType, Func<TConverterBase> factory) : base(converterType, null)
        {
            this._factory = factory;
        }

        protected override TConverterBase CreateInstance()
        {
            return this._factory();
        }

        internal override bool IsPublic
        {
            get
            {
                return true;
            }
        }
    }
}

