namespace Microsoft.JScript
{
    using System;

    internal class AssertException : Exception
    {
        internal AssertException(string message) : base(message)
        {
        }
    }
}

