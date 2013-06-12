namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Xml;
    using System.Xml.Schema;

    internal class XmlSchemaObjectComparer : IComparer
    {
        private QNameComparer comparer = new QNameComparer();

        public int Compare(object o1, object o2)
        {
            return this.comparer.Compare(NameOf((XmlSchemaObject) o1), NameOf((XmlSchemaObject) o2));
        }

        internal static XmlQualifiedName NameOf(XmlSchemaObject o)
        {
            if (o is XmlSchemaAttribute)
            {
                return ((XmlSchemaAttribute) o).QualifiedName;
            }
            if (o is XmlSchemaAttributeGroup)
            {
                return ((XmlSchemaAttributeGroup) o).QualifiedName;
            }
            if (o is XmlSchemaComplexType)
            {
                return ((XmlSchemaComplexType) o).QualifiedName;
            }
            if (o is XmlSchemaSimpleType)
            {
                return ((XmlSchemaSimpleType) o).QualifiedName;
            }
            if (o is XmlSchemaElement)
            {
                return ((XmlSchemaElement) o).QualifiedName;
            }
            if (o is XmlSchemaGroup)
            {
                return ((XmlSchemaGroup) o).QualifiedName;
            }
            if (o is XmlSchemaGroupRef)
            {
                return ((XmlSchemaGroupRef) o).RefName;
            }
            if (o is XmlSchemaNotation)
            {
                return ((XmlSchemaNotation) o).QualifiedName;
            }
            if (o is XmlSchemaSequence)
            {
                XmlSchemaSequence sequence = (XmlSchemaSequence) o;
                if (sequence.Items.Count == 0)
                {
                    return new XmlQualifiedName(".sequence", Namespace(o));
                }
                return NameOf(sequence.Items[0]);
            }
            if (o is XmlSchemaAll)
            {
                XmlSchemaAll all = (XmlSchemaAll) o;
                if (all.Items.Count == 0)
                {
                    return new XmlQualifiedName(".all", Namespace(o));
                }
                return NameOf(all.Items);
            }
            if (o is XmlSchemaChoice)
            {
                XmlSchemaChoice choice = (XmlSchemaChoice) o;
                if (choice.Items.Count == 0)
                {
                    return new XmlQualifiedName(".choice", Namespace(o));
                }
                return NameOf(choice.Items);
            }
            if (o is XmlSchemaAny)
            {
                return new XmlQualifiedName("*", SchemaObjectWriter.ToString(((XmlSchemaAny) o).NamespaceList));
            }
            if (o is XmlSchemaIdentityConstraint)
            {
                return ((XmlSchemaIdentityConstraint) o).QualifiedName;
            }
            return new XmlQualifiedName("?", Namespace(o));
        }

        internal static XmlQualifiedName NameOf(XmlSchemaObjectCollection items)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < items.Count; i++)
            {
                list.Add(NameOf(items[i]));
            }
            list.Sort(new QNameComparer());
            return (XmlQualifiedName) list[0];
        }

        internal static string Namespace(XmlSchemaObject o)
        {
            while ((o != null) && !(o is XmlSchema))
            {
                o = o.Parent;
            }
            if (o != null)
            {
                return ((XmlSchema) o).TargetNamespace;
            }
            return "";
        }
    }
}

