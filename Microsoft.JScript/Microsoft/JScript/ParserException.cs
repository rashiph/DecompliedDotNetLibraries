namespace Microsoft.JScript
{
    using System;

    [Serializable]
    public class ParserException : Exception
    {
        internal ParserException() : base(JScriptException.Localize("Parser Exception", CultureInfo.CurrentUICulture))
        {
        }
    }
}

