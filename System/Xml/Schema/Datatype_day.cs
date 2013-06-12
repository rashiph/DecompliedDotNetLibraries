namespace System.Xml.Schema
{
    using System;

    internal class Datatype_day : Datatype_dateTimeBase
    {
        internal Datatype_day() : base(XsdDateTimeFlags.GDay)
        {
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.GDay;
            }
        }
    }
}

