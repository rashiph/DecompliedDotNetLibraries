namespace System.Xml.Schema
{
    using System;

    internal class Datatype_dateTimeNoTimeZone : Datatype_dateTimeBase
    {
        internal Datatype_dateTimeNoTimeZone() : base(XsdDateTimeFlags.XdrDateTimeNoTz)
        {
        }
    }
}

