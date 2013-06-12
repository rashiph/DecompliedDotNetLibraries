namespace System.Xml.Schema
{
    using System;

    internal class Datatype_yearMonth : Datatype_dateTimeBase
    {
        internal Datatype_yearMonth() : base(XsdDateTimeFlags.GYearMonth)
        {
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.GYearMonth;
            }
        }
    }
}

