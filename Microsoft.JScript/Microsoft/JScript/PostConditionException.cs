namespace Microsoft.JScript
{
    using System;

    internal class PostConditionException : AssertException
    {
        internal PostConditionException(string message) : base(message)
        {
        }
    }
}

