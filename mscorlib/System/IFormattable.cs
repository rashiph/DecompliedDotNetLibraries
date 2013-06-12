namespace System
{
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IFormattable
    {
        string ToString(string format, IFormatProvider formatProvider);
    }
}

