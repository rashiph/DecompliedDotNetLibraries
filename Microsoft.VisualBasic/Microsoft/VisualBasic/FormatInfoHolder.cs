namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Globalization;

    internal sealed class FormatInfoHolder : IFormatProvider
    {
        private NumberFormatInfo nfi;

        internal FormatInfoHolder(NumberFormatInfo nfi)
        {
            this.nfi = nfi;
        }

        private object GetFormat(Type service)
        {
            if (service != typeof(NumberFormatInfo))
            {
                throw new ArgumentException(Utils.GetResourceString("InternalError"));
            }
            return this.nfi;
        }
    }
}

