namespace MS.Internal.Xaml.Context
{
    using System;
    using System.Xaml;

    internal class XamlObjectWriterFactory : IXamlObjectWriterFactory
    {
        private XamlObjectWriterSettings _parentSettings;
        private XamlSavedContext _savedContext;

        public XamlObjectWriterFactory(ObjectWriterContext context)
        {
            this._savedContext = context.GetSavedContext(SavedContextType.Template);
            this._parentSettings = context.ServiceProvider_GetSettings();
        }

        public XamlObjectWriterSettings GetParentSettings()
        {
            return new XamlObjectWriterSettings(this._parentSettings);
        }

        public XamlObjectWriter GetXamlObjectWriter(XamlObjectWriterSettings settings)
        {
            return new XamlObjectWriter(this._savedContext, settings);
        }
    }
}

