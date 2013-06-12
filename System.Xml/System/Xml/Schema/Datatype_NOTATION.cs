namespace System.Xml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_NOTATION : Datatype_anySimpleType
    {
        private static readonly Type atomicValueType = typeof(XmlQualifiedName);
        private static readonly Type listValueType = typeof(XmlQualifiedName[]);

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType)
        {
            return XmlMiscConverter.Create(schemaType);
        }

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = null;
            if ((s == null) || (s.Length == 0))
            {
                return new XmlSchemaException("Sch_EmptyAttributeValue", string.Empty);
            }
            Exception exception = DatatypeImplementation.qnameFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception == null)
            {
                XmlQualifiedName name = null;
                try
                {
                    string str;
                    name = XmlQualifiedName.Parse(s, nsmgr, out str);
                }
                catch (ArgumentException exception2)
                {
                    return exception2;
                }
                catch (XmlException exception3)
                {
                    return exception3;
                }
                exception = DatatypeImplementation.qnameFacetsChecker.CheckValueFacets(name, this);
                if (exception == null)
                {
                    typedValue = name;
                    return null;
                }
            }
            return exception;
        }

        internal override void VerifySchemaValid(XmlSchemaObjectTable notations, XmlSchemaObject caller)
        {
            for (Datatype_NOTATION e_notation = this; e_notation != null; e_notation = (Datatype_NOTATION) e_notation.Base)
            {
                if ((e_notation.Restriction != null) && ((e_notation.Restriction.Flags & RestrictionFlags.Enumeration) != 0))
                {
                    for (int i = 0; i < e_notation.Restriction.Enumeration.Count; i++)
                    {
                        XmlQualifiedName name = (XmlQualifiedName) e_notation.Restriction.Enumeration[i];
                        if (!notations.Contains(name))
                        {
                            throw new XmlSchemaException("Sch_NotationRequired", caller);
                        }
                    }
                    return;
                }
            }
            throw new XmlSchemaException("Sch_NotationRequired", caller);
        }

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet
        {
            get
            {
                return XmlSchemaWhiteSpace.Collapse;
            }
        }

        internal override System.Xml.Schema.FacetsChecker FacetsChecker
        {
            get
            {
                return DatatypeImplementation.qnameFacetsChecker;
            }
        }

        internal override Type ListValueType
        {
            get
            {
                return listValueType;
            }
        }

        public override XmlTokenizedType TokenizedType
        {
            get
            {
                return XmlTokenizedType.NOTATION;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.Notation;
            }
        }

        internal override RestrictionFlags ValidRestrictionFlags
        {
            get
            {
                return (RestrictionFlags.WhiteSpace | RestrictionFlags.Enumeration | RestrictionFlags.Pattern | RestrictionFlags.MaxLength | RestrictionFlags.MinLength | RestrictionFlags.Length);
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

