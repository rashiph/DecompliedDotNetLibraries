namespace Microsoft.JScript
{
    using System;

    internal class PreConditionException : AssertException
    {
        internal PreConditionException(string message) : base(message)
        {
        }
    }
}

