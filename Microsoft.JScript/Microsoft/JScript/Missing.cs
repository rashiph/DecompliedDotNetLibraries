namespace Microsoft.JScript
{
    using System;

    public sealed class Missing
    {
        public static readonly Missing Value = new Missing();

        private Missing()
        {
        }
    }
}

