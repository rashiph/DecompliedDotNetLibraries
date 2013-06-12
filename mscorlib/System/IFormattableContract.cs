namespace System
{
    internal abstract class IFormattableContract : IFormattable
    {
        protected IFormattableContract()
        {
        }

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }
    }
}

