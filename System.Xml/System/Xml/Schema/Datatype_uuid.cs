namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_uuid : Datatype_anySimpleType
    {
        private static readonly Type atomicValueType = typeof(Guid);
        private static readonly Type listValueType = typeof(Guid[]);

        internal override int Compare(object value1, object value2)
        {
            Guid guid = (Guid) value1;
            if (!guid.Equals(value2))
            {
                return -1;
            }
            return 0;
        }

        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr)
        {
            object obj2;
            try
            {
                obj2 = XmlConvert.ToGuid(s);
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
            Guid guid;
            typedValue = null;
            Exception exception = XmlConvert.TryToGuid(s, out guid);
            if (exception == null)
            {
                typedValue = guid;
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

