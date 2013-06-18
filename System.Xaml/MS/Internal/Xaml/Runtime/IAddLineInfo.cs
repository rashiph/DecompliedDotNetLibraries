namespace MS.Internal.Xaml.Runtime
{
    using System.Xaml;

    internal interface IAddLineInfo
    {
        XamlException WithLineInfo(XamlException ex);
    }
}

