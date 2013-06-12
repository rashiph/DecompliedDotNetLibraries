namespace System.Xml.Schema
{
    using System;

    internal class Datatype_month : Datatype_dateTimeBase
    {
        internal Datatype_month() : base(XsdDateTimeFlags.GMonth)
        {
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.GMonth;
            }
        }
    }
}

