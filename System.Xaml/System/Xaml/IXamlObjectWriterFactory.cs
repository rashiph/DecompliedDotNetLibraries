namespace System.Xaml
{
    public interface IXamlObjectWriterFactory
    {
        XamlObjectWriterSettings GetParentSettings();
        XamlObjectWriter GetXamlObjectWriter(XamlObjectWriterSettings settings);
    }
}

