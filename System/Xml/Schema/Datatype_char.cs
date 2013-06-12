namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_char : Datatype_anySimpleType
    {
        private static readonly Type atomicValueType = typeof(char);
        private static readonly Type listValueType = typeof(char[]);

        internal override int Compare(object value1, object value2)
        {
            char ch = (char) value1;
            return ch.CompareTo(value2);
        }

        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr)
        {
            object obj2;
            try
            {
                obj2 = XmlConvert.ToChar(s);
            }
            catch (XmlSchemaException exception)
            {
                throw exception;
            }
            catch (Exception exception2)
            {
                throw new XmlSchemaException(Res.GetString("Sch_InvalidValue", new object[] { s }), exception2);
            }
            return obj2;
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            char ch;
            typedValue = null;
            Exception exception = XmlConvert.TryToChar(s, out ch);
            if (exception == null)
            {
                typedValue = ch;
                return null;
            }
            return exception;
        }

        internal override Type ListValueType
        {
            get
            {
                return listValueType;
            }
        }

        internal override RestrictionFlags ValidRestrictionFlags
        {
            get
            {
                return 0;
            }
        }

        public override Type ValueType
        {
            get
            {
                return atomicValueType;
            }
        }
    }
}

