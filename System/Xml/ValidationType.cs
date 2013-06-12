namespace System.Xml
{
    using System;

    public enum ValidationType
    {
        [Obsolete("Validation type should be specified as DTD or Schema.")]
        Auto = 1,
        DTD = 2,
        None = 0,
        Schema = 4,
        [Obsolete("XDR Validation through XmlValidatingReader is obsoleted")]
        XDR = 3
    }
}

