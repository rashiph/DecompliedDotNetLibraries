namespace Microsoft.JScript
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class NoContextException : ApplicationException
    {
        public NoContextException() : base(JScriptException.Localize("No Source Context available", CultureInfo.CurrentUICulture))
        {
        }

        public NoContextException(string m) : base(m)
        {
        }

        protected NoContextException(SerializationInfo s, StreamingContext c) : base(s, c)
        {
        }

        public NoContextException(string m, Exception e) : base(m, e)
        {
        }
    }
}

