namespace System.Xml.Xsl
{
    using System;

    public abstract class XsltMessageEncounteredEventArgs : EventArgs
    {
        protected XsltMessageEncounteredEventArgs()
        {
        }

        public abstract string Message { get; }
    }
}

