namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_fixed : Datatype_decimal
    {
        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr)
        {
            Exception exception;
            try
            {
                Numeric10FacetsChecker facetsChecker = this.FacetsChecker as Numeric10FacetsChecker;
                decimal num = XmlConvert.ToDecimal(s);
                exception = facetsChecker.CheckTotalAndFractionDigits(num, 0x12, 4, true, true);
                if (exception == null)
                {
                    return num;
                }
            }
            catch (XmlSchemaException exception2)
            {
                throw exception2;
            }
            catch (Exception exception3)
            {
                throw new XmlSchemaException(Res.GetString("Sch_InvalidValue", new object[] { s }), exception3);
            }
            throw exception;
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            decimal num;
            typedValue = null;
            Exception exception = XmlConvert.TryToDecimal(s, out num);
            if (exception == null)
            {
                exception = (this.FacetsChecker as Numeric10FacetsChecker).CheckTotalAndFractionDigits(num, 0x12, 4, true, true);
                if (exception == null)
                {
                    typedValue = num;
                    return null;
                }
            }
            return exception;
        }
    }
}

