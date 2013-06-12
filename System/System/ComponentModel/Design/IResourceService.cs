namespace System.ComponentModel.Design
{
    using System.Globalization;
    using System.Resources;

    public interface IResourceService
    {
        IResourceReader GetResourceReader(CultureInfo info);
        IResourceWriter GetResourceWriter(CultureInfo info);
    }
}

