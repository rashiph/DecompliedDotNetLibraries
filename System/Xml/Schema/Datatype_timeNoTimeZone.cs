namespace System.Xml.Schema
{
    using System;

    internal class Datatype_timeNoTimeZone : Datatype_dateTimeBase
    {
        internal Datatype_timeNoTimeZone() : base(XsdDateTimeFlags.XdrTimeNoTz)
        {
        }
    }
}

