namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_dayTimeDuration : Datatype_duration
    {
        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = null;
            if ((s == null) || (s.Length == 0))
            {
                return new XmlSchemaException("Sch_EmptyAttributeValue", string.Empty);
            }
            Exception exception = DatatypeImplementation.durationFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception == null)
            {
                XsdDuration duration;
                exception = XsdDuration.TryParse(s, XsdDuration.DurationType.DayTimeDuration, out duration);
                if (exception == null)
                {
                    TimeSpan span;
                    exception = duration.TryToTimeSpan(XsdDuration.DurationType.DayTimeDuration, out span);
                    if (exception == null)
                    {
                        exception = DatatypeImplementation.durationFacetsChecker.CheckValueFacets(span, this);
                        if (exception == null)
                        {
                            typedValue = span;
                            return null;
                        }
                    }
                }
            }
            return exception;
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.DayTimeDuration;
            }
        }
    }
}

