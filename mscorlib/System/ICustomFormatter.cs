namespace System
{
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ICustomFormatter
    {
        string Format(string format, object arg, IFormatProvider formatProvider);
    }
}

