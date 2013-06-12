namespace System.Xml.Schema
{
    using System;

    internal class Datatype_time : Datatype_dateTimeBase
    {
        internal Datatype_time() : base(XsdDateTimeFlags.Time)
        {
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.Time;
            }
        }
    }
}

