namespace System.Web
{
    using System;

    internal class UseLastUnhandledErrorFormatter : UnhandledErrorFormatter
    {
        internal UseLastUnhandledErrorFormatter(Exception e) : base(e)
        {
        }

        internal override void PrepareFormatter()
        {
            base.PrepareFormatter();
            base._initialException = this.Exception;
        }
    }
}

