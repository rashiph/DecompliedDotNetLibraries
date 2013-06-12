namespace System.Runtime.Remoting.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum SoapOption
    {
        AlwaysIncludeTypes = 1,
        EmbedAll = 4,
        None = 0,
        Option1 = 8,
        Option2 = 0x10,
        XsdString = 2
    }
}

