namespace System.Data.OleDb
{
    using System;
    using System.Data.ProviderBase;

    internal sealed class StringMemHandle : DbBuffer
    {
        internal StringMemHandle(string value) : base((value != null) ? (2 + (2 * value.Length)) : 0)
        {
            if (value != null)
            {
                base.WriteCharArray(0, value.ToCharArray(), 0, value.Length);
            }
        }
    }
}

