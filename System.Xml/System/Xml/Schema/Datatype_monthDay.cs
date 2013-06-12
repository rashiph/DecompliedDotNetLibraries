namespace System.Xml.Schema
{
    using System;

    internal class Datatype_monthDay : Datatype_dateTimeBase
    {
        internal Datatype_monthDay() : base(XsdDateTimeFlags.GMonthDay)
        {
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.GMonthDay;
            }
        }
    }
}

