namespace System.Web
{
    using System;

    internal abstract class ErrorFormatterGenerator
    {
        protected ErrorFormatterGenerator()
        {
        }

        internal abstract ErrorFormatter GetErrorFormatter(Exception e);
    }
}

