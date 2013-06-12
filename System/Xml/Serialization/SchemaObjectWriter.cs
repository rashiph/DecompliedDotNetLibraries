namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    internal class SchemaObjectWriter
    {
        private int indentLevel = -1;
        private StringBuilder w = new StringBuilder();

        protected string GetString()
        {
            return this.w.ToString();
        }

        protected void NewLine()
        {
            this.w.Append(Environment.NewLine);
            this.WriteIndent();
        }

        internal static string ToString(NamespaceList list)
        {
            if (list == null)
            {
                return null;
            }
            switch (list.Type)
            {
                case NamespaceList.ListType.Any:
                    return "##any";

                case NamespaceList.ListType.Other:
                    return "##other";

                case NamespaceList.ListType.Set:
                {
                    ArrayList list2 = new ArrayList();
                    foreach (string str in list.Enumerate)
                    {
                        list2.Add(str);
                    }
                    list2.Sort();
                    StringBuilder builder = new StringBuilder();
                    bool flag = true;
                    foreach (string str2 in list2)
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            builder.Append(" ");
                        }
                        if (str2.Length == 0)
                        {
                            builder.Append("##local");
                        }
                        else
                        {
                            builder.Append(str2);
                        }
                    }
                    return builder.ToString();
                }
            }
            return list.ToString();
        }

        private void Write_XmlSchemaFacet(string name, XmlSchemaFacet o)
        {
            if (o != null)
            {
                this.WriteStartElement(name);
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("value", "", o.Value);
                if (o.IsFixed)
                {
                    this.WriteAttribute("fixed", "", XmlConvert.ToString(o.IsFixed));
                }
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.WriteEndElement();
            }
        }

        private void Write1_XmlSchemaAttribute(XmlSchemaAttribute o)
        {
            if (o != null)
            {
                this.WriteStartElement("attribute");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.WriteAttribute("default", "", o.DefaultValue);
                this.WriteAttribute("fixed", "", o.FixedValue);
                if ((o.Parent != null) && !(o.Parent is XmlSchema))
                {
                    if (((o.QualifiedName != null) && !o.QualifiedName.IsEmpty) && ((o.QualifiedName.Namespace != null) && (o.QualifiedName.Namespace.Length != 0)))
                    {
                        this.WriteAttribute("form", "", "qualified");
                    }
                    else
                    {
                        this.WriteAttribute("form", "", "unqualified");
                    }
                }
                this.WriteAttribute("name", "", o.Name);
                if (!o.RefName.IsEmpty)
                {
                    this.WriteAttribute("ref", "", o.RefName);
                }
                else if (!o.SchemaTypeName.IsEmpty)
                {
                    this.WriteAttribute("type", "", o.SchemaTypeName);
                }
                XmlSchemaUse v = (o.Use == XmlSchemaUse.None) ? XmlSchemaUse.Optional : o.Use;
                this.WriteAttribute("use", "", this.Write30_XmlSchemaUse(v));
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.Write9_XmlSchemaSimpleType(o.SchemaType);
                this.WriteEndElement();
            }
        }

        private string Write11_XmlSchemaDerivationMethod(XmlSchemaDerivationMethod v)
        {
            return v.ToString();
        }

        private void Write12_XmlSchemaSimpleTypeUnion(XmlSchemaSimpleTypeUnion o)
        {
            if (o != null)
            {
                this.WriteStartElement("union");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttributes(o.UnhandledAttributes, o);
                if (o.MemberTypes != null)
                {
                    ArrayList list = new ArrayList();
                    for (int i = 0; i < o.MemberTypes.Length; i++)
                    {
                        list.Add(o.MemberTypes[i]);
                    }
                    list.Sort(new QNameComparer());
                    this.w.Append(",");
                    this.w.Append("memberTypes=");
                    for (int j = 0; j < list.Count; j++)
                    {
                        this.w.Append(((XmlQualifiedName) list[j]).ToString());
                        this.w.Append(",");
                    }
                }
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.WriteSortedItems(o.BaseTypes);
                this.WriteEndElement();
            }
        }

        private void Write14_XmlSchemaSimpleTypeList(XmlSchemaSimpleTypeList o)
        {
            if (o != null)
            {
                this.WriteStartElement("list");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttributes(o.UnhandledAttributes, o);
                if (!o.ItemTypeName.IsEmpty)
                {
                    this.WriteAttribute("itemType", "", o.ItemTypeName);
                }
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.Write9_XmlSchemaSimpleType(o.ItemType);
                this.WriteEndElement();
            }
        }

        private void Write15_XmlSchemaSimpleTypeRestriction(XmlSchemaSimpleTypeRestriction o)
        {
            if (o != null)
            {
                this.WriteStartElement("restriction");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttributes(o.UnhandledAttributes, o);
                if (!o.BaseTypeName.IsEmpty)
                {
                    this.WriteAttribute("base", "", o.BaseTypeName);
                }
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.Write9_XmlSchemaSimpleType(o.BaseType);
                this.WriteFacets(o.Facets);
                this.WriteEndElement();
            }
        }

        private void Write3_XmlSchemaObject(XmlSchemaObject o)
        {
            if (o != null)
            {
                Type type = o.GetType();
                if (type == typeof(XmlSchemaComplexType))
                {
                    this.Write35_XmlSchemaComplexType((XmlSchemaComplexType) o);
                }
                else if (type == typeof(XmlSchemaSimpleType))
                {
                    this.Write9_XmlSchemaSimpleType((XmlSchemaSimpleType) o);
                }
                else if (type == typeof(XmlSchemaElement))
                {
                    this.Write46_XmlSchemaElement((XmlSchemaElement) o);
                }
                else if (type == typeof(XmlSchemaAppInfo))
                {
                    this.Write7_XmlSchemaAppInfo((XmlSchemaAppInfo) o);
                }
                else if (type == typeof(XmlSchemaDocumentation))
                {
                    this.Write6_XmlSchemaDocumentation((XmlSchemaDocumentation) o);
                }
                else if (type == typeof(XmlSchemaAnnotation))
                {
                    this.Write5_XmlSchemaAnnotation((XmlSchemaAnnotation) o);
                }
                else if (type == typeof(XmlSchemaGroup))
                {
                    this.Write57_XmlSchemaGroup((XmlSchemaGroup) o);
                }
                else if (type == typeof(XmlSchemaXPath))
                {
                    this.Write49_XmlSchemaXPath("xpath", "", (XmlSchemaXPath) o);
                }
                else if (type == typeof(XmlSchemaIdentityConstraint))
                {
                    this.Write48_XmlSchemaIdentityConstraint((XmlSchemaIdentityConstraint) o);
                }
                else if (type == typeof(XmlSchemaUnique))
                {
                    this.Write51_XmlSchemaUnique((XmlSchemaUnique) o);
                }
                else if (type == typeof(XmlSchemaKeyref))
                {
                    this.Write50_XmlSchemaKeyref((XmlSchemaKeyref) o);
                }
                else if (type == typeof(XmlSchemaKey))
                {
                    this.Write47_XmlSchemaKey((XmlSchemaKey) o);
                }
                else if (type == typeof(XmlSchemaGroupRef))
                {
                    this.Write55_XmlSchemaGroupRef((XmlSchemaGroupRef) o);
                }
                else if (type == typeof(XmlSchemaAny))
                {
                    this.Write53_XmlSchemaAny((XmlSchemaAny) o);
                }
                else if (type == typeof(XmlSchemaSequence))
                {
                    this.Write54_XmlSchemaSequence((XmlSchemaSequence) o);
                }
                else if (type == typeof(XmlSchemaChoice))
                {
                    this.Write52_XmlSchemaChoice((XmlSchemaChoice) o);
                }
                else if (type == typeof(XmlSchemaAll))
                {
                    this.Write43_XmlSchemaAll((XmlSchemaAll) o);
                }
                else if (type == typeof(XmlSchemaComplexContentRestriction))
                {
                    this.Write56_XmlSchemaComplexContentRestriction((XmlSchemaComplexContentRestriction) o);
                }
                else if (type == typeof(XmlSchemaComplexContentExtension))
                {
                    this.Write42_XmlSchemaComplexContentExtension((XmlSchemaComplexContentExtension) o);
                }
                else if (type == typeof(XmlSchemaSimpleContentRestriction))
                {
                    this.Write40_XmlSchemaSimpleContentRestriction((XmlSchemaSimpleContentRestriction) o);
                }
                else if (type == typeof(XmlSchemaSimpleContentExtension))
                {
                    this.Write38_XmlSchemaSimpleContentExtension((XmlSchemaSimpleContentExtension) o);
                }
                else if (type == typeof(XmlSchemaComplexContent))
                {
                    this.Write41_XmlSchemaComplexContent((XmlSchemaComplexContent) o);
                }
                else if (type == typeof(XmlSchemaSimpleContent))
                {
                    this.Write36_XmlSchemaSimpleContent((XmlSchemaSimpleContent) o);
                }
                else if (type == typeof(XmlSchemaAnyAttribute))
                {
                    this.Write33_XmlSchemaAnyAttribute((XmlSchemaAnyAttribute) o);
                }
                else if (type == typeof(XmlSchemaAttributeGroupRef))
                {
                    this.Write32_XmlSchemaAttributeGroupRef((XmlSchemaAttributeGroupRef) o);
                }
                else if (type == typeof(XmlSchemaAttributeGroup))
                {
                    this.Write31_XmlSchemaAttributeGroup((XmlSchemaAttributeGroup) o);
                }
                else if (type == typeof(XmlSchemaSimpleTypeRestriction))
                {
                    this.Write15_XmlSchemaSimpleTypeRestriction((XmlSchemaSimpleTypeRestriction) o);
                }
                else if (type == typeof(XmlSchemaSimpleTypeList))
                {
                    this.Write14_XmlSchemaSimpleTypeList((XmlSchemaSimpleTypeList) o);
                }
                else if (type == typeof(XmlSchemaSimpleTypeUnion))
                {
                    this.Write12_XmlSchemaSimpleTypeUnion((XmlSchemaSimpleTypeUnion) o);
                }
                else if (type == typeof(XmlSchemaAttribute))
                {
                    this.Write1_XmlSchemaAttribute((XmlSchemaAttribute) o);
                }
            }
        }

        private string Write30_XmlSchemaUse(XmlSchemaUse v)
        {
            switch (v)
            {
                case XmlSchemaUse.Optional:
                    return "optional";

                case XmlSchemaUse.Prohibited:
                    return "prohibited";

                case XmlSchemaUse.Required:
                    return "required";
            }
            return null;
        }

        private void Write31_XmlSchemaAttributeGroup(XmlSchemaAttributeGroup o)
        {
            if (o != null)
            {
                this.WriteStartElement("attributeGroup");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("name", "", o.Name);
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.WriteSortedItems(o.Attributes);
                this.Write33_XmlSchemaAnyAttribute(o.AnyAttribute);
                this.WriteEndElement();
            }
        }

        private void Write32_XmlSchemaAttributeGroupRef(XmlSchemaAttributeGroupRef o)
        {
            if (o != null)
            {
                this.WriteStartElement("attributeGroup");
                this.WriteAttribute("id", "", o.Id);
                if (!o.RefName.IsEmpty)
                {
                    this.WriteAttribute("ref", "", o.RefName);
                }
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.WriteEndElement();
            }
        }

        private void Write33_XmlSchemaAnyAttribute(XmlSchemaAnyAttribute o)
        {
            if (o != null)
            {
                this.WriteStartElement("anyAttribute");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("namespace", "", ToString(o.NamespaceList));
                XmlSchemaContentProcessing v = (o.ProcessContents == XmlSchemaContentProcessing.None) ? XmlSchemaContentProcessing.Strict : o.ProcessContents;
                this.WriteAttribute("processContents", "", this.Write34_XmlSchemaContentProcessing(v));
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.WriteEndElement();
            }
        }

        private string Write34_XmlSchemaContentProcessing(XmlSchemaContentProcessing v)
        {
            switch (v)
            {
                case XmlSchemaContentProcessing.Skip:
                    return "skip";

                case XmlSchemaContentProcessing.Lax:
                    return "lax";

                case XmlSchemaContentProcessing.Strict:
                    return "strict";
            }
            return null;
        }

        private void Write35_XmlSchemaComplexType(XmlSchemaComplexType o)
        {
            if (o != null)
            {
                this.WriteStartElement("complexType");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("name", "", o.Name);
                this.WriteAttribute("final", "", this.Write11_XmlSchemaDerivationMethod(o.FinalResolved));
                if (o.IsAbstract)
                {
                    this.WriteAttribute("abstract", "", XmlConvert.ToString(o.IsAbstract));
                }
                this.WriteAttribute("block", "", this.Write11_XmlSchemaDerivationMethod(o.BlockResolved));
                if (o.IsMixed)
                {
                    this.WriteAttribute("mixed", "", XmlConvert.ToString(o.IsMixed));
                }
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                if (o.ContentModel is XmlSchemaComplexContent)
                {
                    this.Write41_XmlSchemaComplexContent((XmlSchemaComplexContent) o.ContentModel);
                }
                else if (o.ContentModel is XmlSchemaSimpleContent)
                {
                    this.Write36_XmlSchemaSimpleContent((XmlSchemaSimpleContent) o.ContentModel);
                }
                if (o.Particle is XmlSchemaSequence)
                {
                    this.Write54_XmlSchemaSequence((XmlSchemaSequence) o.Particle);
                }
                else if (o.Particle is XmlSchemaGroupRef)
                {
                    this.Write55_XmlSchemaGroupRef((XmlSchemaGroupRef) o.Particle);
                }
                else if (o.Particle is XmlSchemaChoice)
                {
                    this.Write52_XmlSchemaChoice((XmlSchemaChoice) o.Particle);
                }
                else if (o.Particle is XmlSchemaAll)
                {
                    this.Write43_XmlSchemaAll((XmlSchemaAll) o.Particle);
                }
                this.WriteSortedItems(o.Attributes);
                this.Write33_XmlSchemaAnyAttribute(o.AnyAttribute);
                this.WriteEndElement();
            }
        }

        private void Write36_XmlSchemaSimpleContent(XmlSchemaSimpleContent o)
        {
            if (o != null)
            {
                this.WriteStartElement("simpleContent");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                if (o.Content is XmlSchemaSimpleContentRestriction)
                {
                    this.Write40_XmlSchemaSimpleContentRestriction((XmlSchemaSimpleContentRestriction) o.Content);
                }
                else if (o.Content is XmlSchemaSimpleContentExtension)
                {
                    this.Write38_XmlSchemaSimpleContentExtension((XmlSchemaSimpleContentExtension) o.Content);
                }
                this.WriteEndElement();
            }
        }

        private void Write38_XmlSchemaSimpleContentExtension(XmlSchemaSimpleContentExtension o)
        {
            if (o != null)
            {
                this.WriteStartElement("extension");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttributes(o.UnhandledAttributes, o);
                if (!o.BaseTypeName.IsEmpty)
                {
                    this.WriteAttribute("base", "", o.BaseTypeName);
                }
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.WriteSortedItems(o.Attributes);
                this.Write33_XmlSchemaAnyAttribute(o.AnyAttribute);
                this.WriteEndElement();
            }
        }

        private void Write40_XmlSchemaSimpleContentRestriction(XmlSchemaSimpleContentRestriction o)
        {
            if (o != null)
            {
                this.WriteStartElement("restriction");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttributes(o.UnhandledAttributes, o);
                if (!o.BaseTypeName.IsEmpty)
                {
                    this.WriteAttribute("base", "", o.BaseTypeName);
                }
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.Write9_XmlSchemaSimpleType(o.BaseType);
                this.WriteFacets(o.Facets);
                this.WriteSortedItems(o.Attributes);
                this.Write33_XmlSchemaAnyAttribute(o.AnyAttribute);
                this.WriteEndElement();
            }
        }

        private void Write41_XmlSchemaComplexContent(XmlSchemaComplexContent o)
        {
            if (o != null)
            {
                this.WriteStartElement("complexContent");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("mixed", "", XmlConvert.ToString(o.IsMixed));
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                if (o.Content is XmlSchemaComplexContentRestriction)
                {
                    this.Write56_XmlSchemaComplexContentRestriction((XmlSchemaComplexContentRestriction) o.Content);
                }
                else if (o.Content is XmlSchemaComplexContentExtension)
                {
                    this.Write42_XmlSchemaComplexContentExtension((XmlSchemaComplexContentExtension) o.Content);
                }
                this.WriteEndElement();
            }
        }

        private void Write42_XmlSchemaComplexContentExtension(XmlSchemaComplexContentExtension o)
        {
            if (o != null)
            {
                this.WriteStartElement("extension");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttributes(o.UnhandledAttributes, o);
                if (!o.BaseTypeName.IsEmpty)
                {
                    this.WriteAttribute("base", "", o.BaseTypeName);
                }
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                if (o.Particle is XmlSchemaSequence)
                {
                    this.Write54_XmlSchemaSequence((XmlSchemaSequence) o.Particle);
                }
                else if (o.Particle is XmlSchemaGroupRef)
                {
                    this.Write55_XmlSchemaGroupRef((XmlSchemaGroupRef) o.Particle);
                }
                else if (o.Particle is XmlSchemaChoice)
                {
                    this.Write52_XmlSchemaChoice((XmlSchemaChoice) o.Particle);
                }
                else if (o.Particle is XmlSchemaAll)
                {
                    this.Write43_XmlSchemaAll((XmlSchemaAll) o.Particle);
                }
                this.WriteSortedItems(o.Attributes);
                this.Write33_XmlSchemaAnyAttribute(o.AnyAttribute);
                this.WriteEndElement();
            }
        }

        private void Write43_XmlSchemaAll(XmlSchemaAll o)
        {
            if (o != null)
            {
                this.WriteStartElement("all");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("minOccurs", "", XmlConvert.ToString(o.MinOccurs));
                this.WriteAttribute("maxOccurs", "", (o.MaxOccurs == 79228162514264337593543950335M) ? "unbounded" : XmlConvert.ToString(o.MaxOccurs));
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.WriteSortedItems(o.Items);
                this.WriteEndElement();
            }
        }

        private void Write46_XmlSchemaElement(XmlSchemaElement o)
        {
            if (o != null)
            {
                o.GetType();
                this.WriteStartElement("element");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("minOccurs", "", XmlConvert.ToString(o.MinOccurs));
                this.WriteAttribute("maxOccurs", "", (o.MaxOccurs == 79228162514264337593543950335M) ? "unbounded" : XmlConvert.ToString(o.MaxOccurs));
                if (o.IsAbstract)
                {
                    this.WriteAttribute("abstract", "", XmlConvert.ToString(o.IsAbstract));
                }
                this.WriteAttribute("block", "", this.Write11_XmlSchemaDerivationMethod(o.BlockResolved));
                this.WriteAttribute("default", "", o.DefaultValue);
                this.WriteAttribute("final", "", this.Write11_XmlSchemaDerivationMethod(o.FinalResolved));
                this.WriteAttribute("fixed", "", o.FixedValue);
                if ((o.Parent != null) && !(o.Parent is XmlSchema))
                {
                    if (((o.QualifiedName != null) && !o.QualifiedName.IsEmpty) && ((o.QualifiedName.Namespace != null) && (o.QualifiedName.Namespace.Length != 0)))
                    {
                        this.WriteAttribute("form", "", "qualified");
                    }
                    else
                    {
                        this.WriteAttribute("form", "", "unqualified");
                    }
                }
                if ((o.Name != null) && (o.Name.Length != 0))
                {
                    this.WriteAttribute("name", "", o.Name);
                }
                if (o.IsNillable)
                {
                    this.WriteAttribute("nillable", "", XmlConvert.ToString(o.IsNillable));
                }
                if (!o.SubstitutionGroup.IsEmpty)
                {
                    this.WriteAttribute("substitutionGroup", "", o.SubstitutionGroup);
                }
                if (!o.RefName.IsEmpty)
                {
                    this.WriteAttribute("ref", "", o.RefName);
                }
                else if (!o.SchemaTypeName.IsEmpty)
                {
                    this.WriteAttribute("type", "", o.SchemaTypeName);
                }
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                if (o.SchemaType is XmlSchemaComplexType)
                {
                    this.Write35_XmlSchemaComplexType((XmlSchemaComplexType) o.SchemaType);
                }
                else if (o.SchemaType is XmlSchemaSimpleType)
                {
                    this.Write9_XmlSchemaSimpleType((XmlSchemaSimpleType) o.SchemaType);
                }
                this.WriteSortedItems(o.Constraints);
                this.WriteEndElement();
            }
        }

        private void Write47_XmlSchemaKey(XmlSchemaKey o)
        {
            if (o != null)
            {
                o.GetType();
                this.WriteStartElement("key");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("name", "", o.Name);
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.Write49_XmlSchemaXPath("selector", "", o.Selector);
                XmlSchemaObjectCollection fields = o.Fields;
                if (fields != null)
                {
                    for (int i = 0; i < fields.Count; i++)
                    {
                        this.Write49_XmlSchemaXPath("field", "", (XmlSchemaXPath) fields[i]);
                    }
                }
                this.WriteEndElement();
            }
        }

        private void Write48_XmlSchemaIdentityConstraint(XmlSchemaIdentityConstraint o)
        {
            if (o != null)
            {
                Type type = o.GetType();
                if (type == typeof(XmlSchemaUnique))
                {
                    this.Write51_XmlSchemaUnique((XmlSchemaUnique) o);
                }
                else if (type == typeof(XmlSchemaKeyref))
                {
                    this.Write50_XmlSchemaKeyref((XmlSchemaKeyref) o);
                }
                else if (type == typeof(XmlSchemaKey))
                {
                    this.Write47_XmlSchemaKey((XmlSchemaKey) o);
                }
            }
        }

        private void Write49_XmlSchemaXPath(string name, string ns, XmlSchemaXPath o)
        {
            if (o != null)
            {
                this.WriteStartElement(name);
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("xpath", "", o.XPath);
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.WriteEndElement();
            }
        }

        private void Write5_XmlSchemaAnnotation(XmlSchemaAnnotation o)
        {
            if (o != null)
            {
                this.WriteStartElement("annotation");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttributes(o.UnhandledAttributes, o);
                XmlSchemaObjectCollection items = o.Items;
                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        XmlSchemaObject obj2 = items[i];
                        if (obj2 is XmlSchemaAppInfo)
                        {
                            this.Write7_XmlSchemaAppInfo((XmlSchemaAppInfo) obj2);
                        }
                        else if (obj2 is XmlSchemaDocumentation)
                        {
                            this.Write6_XmlSchemaDocumentation((XmlSchemaDocumentation) obj2);
                        }
                    }
                }
                this.WriteEndElement();
            }
        }

        private void Write50_XmlSchemaKeyref(XmlSchemaKeyref o)
        {
            if (o != null)
            {
                o.GetType();
                this.WriteStartElement("keyref");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("name", "", o.Name);
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.WriteAttribute("refer", "", o.Refer);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.Write49_XmlSchemaXPath("selector", "", o.Selector);
                XmlSchemaObjectCollection fields = o.Fields;
                if (fields != null)
                {
                    for (int i = 0; i < fields.Count; i++)
                    {
                        this.Write49_XmlSchemaXPath("field", "", (XmlSchemaXPath) fields[i]);
                    }
                }
                this.WriteEndElement();
            }
        }

        private void Write51_XmlSchemaUnique(XmlSchemaUnique o)
        {
            if (o != null)
            {
                o.GetType();
                this.WriteStartElement("unique");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("name", "", o.Name);
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.Write49_XmlSchemaXPath("selector", "", o.Selector);
                XmlSchemaObjectCollection fields = o.Fields;
                if (fields != null)
                {
                    for (int i = 0; i < fields.Count; i++)
                    {
                        this.Write49_XmlSchemaXPath("field", "", (XmlSchemaXPath) fields[i]);
                    }
                }
                this.WriteEndElement();
            }
        }

        private void Write52_XmlSchemaChoice(XmlSchemaChoice o)
        {
            if (o != null)
            {
                o.GetType();
                this.WriteStartElement("choice");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("minOccurs", "", XmlConvert.ToString(o.MinOccurs));
                this.WriteAttribute("maxOccurs", "", (o.MaxOccurs == 79228162514264337593543950335M) ? "unbounded" : XmlConvert.ToString(o.MaxOccurs));
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.WriteSortedItems(o.Items);
                this.WriteEndElement();
            }
        }

        private void Write53_XmlSchemaAny(XmlSchemaAny o)
        {
            if (o != null)
            {
                this.WriteStartElement("any");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("minOccurs", "", XmlConvert.ToString(o.MinOccurs));
                this.WriteAttribute("maxOccurs", "", (o.MaxOccurs == 79228162514264337593543950335M) ? "unbounded" : XmlConvert.ToString(o.MaxOccurs));
                this.WriteAttribute("namespace", "", ToString(o.NamespaceList));
                XmlSchemaContentProcessing v = (o.ProcessContents == XmlSchemaContentProcessing.None) ? XmlSchemaContentProcessing.Strict : o.ProcessContents;
                this.WriteAttribute("processContents", "", this.Write34_XmlSchemaContentProcessing(v));
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.WriteEndElement();
            }
        }

        private void Write54_XmlSchemaSequence(XmlSchemaSequence o)
        {
            if (o != null)
            {
                this.WriteStartElement("sequence");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("minOccurs", "", XmlConvert.ToString(o.MinOccurs));
                this.WriteAttribute("maxOccurs", "", (o.MaxOccurs == 79228162514264337593543950335M) ? "unbounded" : XmlConvert.ToString(o.MaxOccurs));
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                XmlSchemaObjectCollection items = o.Items;
                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        XmlSchemaObject obj2 = items[i];
                        if (obj2 is XmlSchemaAny)
                        {
                            this.Write53_XmlSchemaAny((XmlSchemaAny) obj2);
                        }
                        else if (obj2 is XmlSchemaSequence)
                        {
                            this.Write54_XmlSchemaSequence((XmlSchemaSequence) obj2);
                        }
                        else if (obj2 is XmlSchemaChoice)
                        {
                            this.Write52_XmlSchemaChoice((XmlSchemaChoice) obj2);
                        }
                        else if (obj2 is XmlSchemaElement)
                        {
                            this.Write46_XmlSchemaElement((XmlSchemaElement) obj2);
                        }
                        else if (obj2 is XmlSchemaGroupRef)
                        {
                            this.Write55_XmlSchemaGroupRef((XmlSchemaGroupRef) obj2);
                        }
                    }
                }
                this.WriteEndElement();
            }
        }

        private void Write55_XmlSchemaGroupRef(XmlSchemaGroupRef o)
        {
            if (o != null)
            {
                this.WriteStartElement("group");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("minOccurs", "", XmlConvert.ToString(o.MinOccurs));
                this.WriteAttribute("maxOccurs", "", (o.MaxOccurs == 79228162514264337593543950335M) ? "unbounded" : XmlConvert.ToString(o.MaxOccurs));
                if (!o.RefName.IsEmpty)
                {
                    this.WriteAttribute("ref", "", o.RefName);
                }
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                this.WriteEndElement();
            }
        }

        private void Write56_XmlSchemaComplexContentRestriction(XmlSchemaComplexContentRestriction o)
        {
            if (o != null)
            {
                this.WriteStartElement("restriction");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttributes(o.UnhandledAttributes, o);
                if (!o.BaseTypeName.IsEmpty)
                {
                    this.WriteAttribute("base", "", o.BaseTypeName);
                }
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                if (o.Particle is XmlSchemaSequence)
                {
                    this.Write54_XmlSchemaSequence((XmlSchemaSequence) o.Particle);
                }
                else if (o.Particle is XmlSchemaGroupRef)
                {
                    this.Write55_XmlSchemaGroupRef((XmlSchemaGroupRef) o.Particle);
                }
                else if (o.Particle is XmlSchemaChoice)
                {
                    this.Write52_XmlSchemaChoice((XmlSchemaChoice) o.Particle);
                }
                else if (o.Particle is XmlSchemaAll)
                {
                    this.Write43_XmlSchemaAll((XmlSchemaAll) o.Particle);
                }
                this.WriteSortedItems(o.Attributes);
                this.Write33_XmlSchemaAnyAttribute(o.AnyAttribute);
                this.WriteEndElement();
            }
        }

        private void Write57_XmlSchemaGroup(XmlSchemaGroup o)
        {
            if (o != null)
            {
                this.WriteStartElement("group");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttribute("name", "", o.Name);
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                if (o.Particle is XmlSchemaSequence)
                {
                    this.Write54_XmlSchemaSequence((XmlSchemaSequence) o.Particle);
                }
                else if (o.Particle is XmlSchemaChoice)
                {
                    this.Write52_XmlSchemaChoice((XmlSchemaChoice) o.Particle);
                }
                else if (o.Particle is XmlSchemaAll)
                {
                    this.Write43_XmlSchemaAll((XmlSchemaAll) o.Particle);
                }
                this.WriteEndElement();
            }
        }

        private void Write6_XmlSchemaDocumentation(XmlSchemaDocumentation o)
        {
            if (o != null)
            {
                this.WriteStartElement("documentation");
                this.WriteAttribute("source", "", o.Source);
                this.WriteAttribute("lang", "http://www.w3.org/XML/1998/namespace", o.Language);
                XmlNode[] markup = o.Markup;
                if (markup != null)
                {
                    for (int i = 0; i < markup.Length; i++)
                    {
                        XmlNode node = markup[i];
                        this.WriteStartElement("node");
                        this.WriteAttribute("xml", "", node.OuterXml);
                    }
                }
                this.WriteEndElement();
            }
        }

        private void Write7_XmlSchemaAppInfo(XmlSchemaAppInfo o)
        {
            if (o != null)
            {
                this.WriteStartElement("appinfo");
                this.WriteAttribute("source", "", o.Source);
                XmlNode[] markup = o.Markup;
                if (markup != null)
                {
                    for (int i = 0; i < markup.Length; i++)
                    {
                        XmlNode node = markup[i];
                        this.WriteStartElement("node");
                        this.WriteAttribute("xml", "", node.OuterXml);
                    }
                }
                this.WriteEndElement();
            }
        }

        private void Write9_XmlSchemaSimpleType(XmlSchemaSimpleType o)
        {
            if (o != null)
            {
                this.WriteStartElement("simpleType");
                this.WriteAttribute("id", "", o.Id);
                this.WriteAttributes(o.UnhandledAttributes, o);
                this.WriteAttribute("name", "", o.Name);
                this.WriteAttribute("final", "", this.Write11_XmlSchemaDerivationMethod(o.FinalResolved));
                this.Write5_XmlSchemaAnnotation(o.Annotation);
                if (o.Content is XmlSchemaSimpleTypeUnion)
                {
                    this.Write12_XmlSchemaSimpleTypeUnion((XmlSchemaSimpleTypeUnion) o.Content);
                }
                else if (o.Content is XmlSchemaSimpleTypeRestriction)
                {
                    this.Write15_XmlSchemaSimpleTypeRestriction((XmlSchemaSimpleTypeRestriction) o.Content);
                }
                else if (o.Content is XmlSchemaSimpleTypeList)
                {
                    this.Write14_XmlSchemaSimpleTypeList((XmlSchemaSimpleTypeList) o.Content);
                }
                this.WriteEndElement();
            }
        }

        private void WriteAttribute(XmlAttribute a)
        {
            if (a.Value != null)
            {
                this.WriteAttribute(a.Name, a.NamespaceURI, a.Value);
            }
        }

        protected void WriteAttribute(string localName, string ns, string value)
        {
            if ((value != null) && (value.Length != 0))
            {
                this.w.Append(",");
                this.w.Append(ns);
                if ((ns != null) && (ns.Length != 0))
                {
                    this.w.Append(":");
                }
                this.w.Append(localName);
                this.w.Append("=");
                this.w.Append(value);
            }
        }

        protected void WriteAttribute(string localName, string ns, XmlQualifiedName value)
        {
            if (!value.IsEmpty)
            {
                this.WriteAttribute(localName, ns, value.ToString());
            }
        }

        private void WriteAttributes(XmlAttribute[] a, XmlSchemaObject o)
        {
            if (a != null)
            {
                ArrayList list = new ArrayList();
                for (int i = 0; i < a.Length; i++)
                {
                    list.Add(a[i]);
                }
                list.Sort(new XmlAttributeComparer());
                for (int j = 0; j < list.Count; j++)
                {
                    XmlAttribute attribute = (XmlAttribute) list[j];
                    this.WriteAttribute(attribute);
                }
            }
        }

        protected void WriteEndElement()
        {
            this.w.Append("]");
            this.indentLevel--;
        }

        private void WriteFacets(XmlSchemaObjectCollection facets)
        {
            if (facets != null)
            {
                ArrayList list = new ArrayList();
                for (int i = 0; i < facets.Count; i++)
                {
                    list.Add(facets[i]);
                }
                list.Sort(new XmlFacetComparer());
                for (int j = 0; j < list.Count; j++)
                {
                    XmlSchemaObject obj2 = (XmlSchemaObject) list[j];
                    if (obj2 is XmlSchemaMinExclusiveFacet)
                    {
                        this.Write_XmlSchemaFacet("minExclusive", (XmlSchemaFacet) obj2);
                    }
                    else if (obj2 is XmlSchemaMaxInclusiveFacet)
                    {
                        this.Write_XmlSchemaFacet("maxInclusive", (XmlSchemaFacet) obj2);
                    }
                    else if (obj2 is XmlSchemaMaxExclusiveFacet)
                    {
                        this.Write_XmlSchemaFacet("maxExclusive", (XmlSchemaFacet) obj2);
                    }
                    else if (obj2 is XmlSchemaMinInclusiveFacet)
                    {
                        this.Write_XmlSchemaFacet("minInclusive", (XmlSchemaFacet) obj2);
                    }
                    else if (obj2 is XmlSchemaLengthFacet)
                    {
                        this.Write_XmlSchemaFacet("length", (XmlSchemaFacet) obj2);
                    }
                    else if (obj2 is XmlSchemaEnumerationFacet)
                    {
                        this.Write_XmlSchemaFacet("enumeration", (XmlSchemaFacet) obj2);
                    }
                    else if (obj2 is XmlSchemaMinLengthFacet)
                    {
                        this.Write_XmlSchemaFacet("minLength", (XmlSchemaFacet) obj2);
                    }
                    else if (obj2 is XmlSchemaPatternFacet)
                    {
                        this.Write_XmlSchemaFacet("pattern", (XmlSchemaFacet) obj2);
                    }
                    else if (obj2 is XmlSchemaTotalDigitsFacet)
                    {
                        this.Write_XmlSchemaFacet("totalDigits", (XmlSchemaFacet) obj2);
                    }
                    else if (obj2 is XmlSchemaMaxLengthFacet)
                    {
                        this.Write_XmlSchemaFacet("maxLength", (XmlSchemaFacet) obj2);
                    }
                    else if (obj2 is XmlSchemaWhiteSpaceFacet)
                    {
                        this.Write_XmlSchemaFacet("whiteSpace", (XmlSchemaFacet) obj2);
                    }
                    else if (obj2 is XmlSchemaFractionDigitsFacet)
                    {
                        this.Write_XmlSchemaFacet("fractionDigit", (XmlSchemaFacet) obj2);
                    }
                }
            }
        }

        private void WriteIndent()
        {
            for (int i = 0; i < this.indentLevel; i++)
            {
                this.w.Append(" ");
            }
        }

        private void WriteSortedItems(XmlSchemaObjectCollection items)
        {
            if (items != null)
            {
                ArrayList list = new ArrayList();
                for (int i = 0; i < items.Count; i++)
                {
                    list.Add(items[i]);
                }
                list.Sort(new XmlSchemaObjectComparer());
                for (int j = 0; j < list.Count; j++)
                {
                    this.Write3_XmlSchemaObject((XmlSchemaObject) list[j]);
                }
            }
        }

        protected void WriteStartElement(string name)
        {
            this.NewLine();
            this.indentLevel++;
            this.w.Append("[");
            this.w.Append(name);
        }

        internal string WriteXmlSchemaObject(XmlSchemaObject o)
        {
            if (o == null)
            {
                return string.Empty;
            }
            this.Write3_XmlSchemaObject(o);
            return this.GetString();
        }
    }
}

