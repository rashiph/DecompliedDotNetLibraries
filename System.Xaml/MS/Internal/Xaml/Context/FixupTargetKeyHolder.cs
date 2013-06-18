namespace MS.Internal.Xaml.Context
{
    using System;
    using System.Runtime.CompilerServices;

    internal class FixupTargetKeyHolder
    {
        public FixupTargetKeyHolder(object key)
        {
            this.Key = key;
        }

        public object Key { get; set; }
    }
}

