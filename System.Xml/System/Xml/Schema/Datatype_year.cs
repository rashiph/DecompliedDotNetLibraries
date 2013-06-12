namespace System.Xml.Schema
{
    using System;

    internal class Datatype_year : Datatype_dateTimeBase
    {
        internal Datatype_year() : base(XsdDateTimeFlags.GYear)
        {
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.GYear;
            }
        }
    }
}

