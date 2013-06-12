namespace System
{
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IFormatProvider
    {
        object GetFormat(Type formatType);
    }
}

