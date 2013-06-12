namespace System.Reflection
{
    using System;
    using System.Globalization;

    internal class MetadataException : Exception
    {
        private int m_hr;

        internal MetadataException(int hr)
        {
            this.m_hr = hr;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "MetadataException HResult = {0:x}.", new object[] { this.m_hr });
        }
    }
}

