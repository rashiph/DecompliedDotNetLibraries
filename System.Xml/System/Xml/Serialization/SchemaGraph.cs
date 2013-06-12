namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Xml;
    using System.Xml.Schema;

    internal class SchemaGraph
    {
        private ArrayList empty = new ArrayList();
        private int items;
        private XmlSchemas schemas;
        private Hashtable scope;

        internal SchemaGraph(Hashtable scope, XmlSchemas schemas)
        {
            this.scope = scope;
            schemas.Compile(null, false);
            this.schemas = schemas;
            this.items = 0;
            foreach (XmlSchema schema in schemas)
            {
                this.items += schema.Items.Count;
                foreach (XmlSchemaObject obj2 in schema.Items)
                {
                    this.Depends(obj2);
                }
            }
        }

        internal void AddRef(ArrayList list, XmlSchemaObject o)
        {
            if (((((o != null) && !this.schemas.IsReference(o)) && (o.Parent is XmlSchema)) && (((XmlSchema) o.Parent).TargetNamespace != "http://www.w3.org/2001/XMLSchema")) && !list.Contains(o))
            {
                list.Add(o);
            }
        }

        internal ArrayList Depends(XmlSchemaObject item)
        {
            if (!(item.Parent is XmlSchema))
            {
                return this.empty;
            }
            if (this.scope[item] != null)
            {
                return (ArrayList) this.scope[item];
            }
            ArrayList refs = new ArrayList();
            this.Depends(item, refs);
            this.scope.Add(item, refs);
            return refs;
        }

        internal void Depends(XmlSchemaObject item, ArrayList refs)
        {
            if ((item != null) && (this.scope[item] == null))
            {
                Type c = item.GetType();
                if (typeof(XmlSchemaType).IsAssignableFrom(c))
                {
                    XmlQualifiedName empty = XmlQualifiedName.Empty;
                    XmlSchemaType o = null;
                    XmlSchemaParticle particle = null;
                    XmlSchemaObjectCollection attributes = null;
                    if (item is XmlSchemaComplexType)
                    {
                        XmlSchemaComplexType type3 = (XmlSchemaComplexType) item;
                        if (type3.ContentModel != null)
                        {
                            XmlSchemaContent content = type3.ContentModel.Content;
                            if (content is XmlSchemaComplexContentRestriction)
                            {
                                empty = ((XmlSchemaComplexContentRestriction) content).BaseTypeName;
                                attributes = ((XmlSchemaComplexContentRestriction) content).Attributes;
                            }
                            else if (content is XmlSchemaSimpleContentRestriction)
                            {
                                XmlSchemaSimpleContentRestriction restriction = (XmlSchemaSimpleContentRestriction) content;
                                if (restriction.BaseType != null)
                                {
                                    o = restriction.BaseType;
                                }
                                else
                                {
                                    empty = restriction.BaseTypeName;
                                }
                                attributes = restriction.Attributes;
                            }
                            else if (content is XmlSchemaComplexContentExtension)
                            {
                                XmlSchemaComplexContentExtension extension = (XmlSchemaComplexContentExtension) content;
                                attributes = extension.Attributes;
                                particle = extension.Particle;
                                empty = extension.BaseTypeName;
                            }
                            else if (content is XmlSchemaSimpleContentExtension)
                            {
                                XmlSchemaSimpleContentExtension extension2 = (XmlSchemaSimpleContentExtension) content;
                                attributes = extension2.Attributes;
                                empty = extension2.BaseTypeName;
                            }
                        }
                        else
                        {
                            attributes = type3.Attributes;
                            particle = type3.Particle;
                        }
                        if (particle is XmlSchemaGroupRef)
                        {
                            XmlSchemaGroupRef ref2 = (XmlSchemaGroupRef) particle;
                            particle = ((XmlSchemaGroup) this.schemas.Find(ref2.RefName, typeof(XmlSchemaGroup), false)).Particle;
                        }
                        else if (particle is XmlSchemaGroupBase)
                        {
                            particle = (XmlSchemaGroupBase) particle;
                        }
                    }
                    else if (item is XmlSchemaSimpleType)
                    {
                        XmlSchemaSimpleType type4 = (XmlSchemaSimpleType) item;
                        XmlSchemaSimpleTypeContent content2 = type4.Content;
                        if (content2 is XmlSchemaSimpleTypeRestriction)
                        {
                            o = ((XmlSchemaSimpleTypeRestriction) content2).BaseType;
                            empty = ((XmlSchemaSimpleTypeRestriction) content2).BaseTypeName;
                        }
                        else if (content2 is XmlSchemaSimpleTypeList)
                        {
                            XmlSchemaSimpleTypeList list = (XmlSchemaSimpleTypeList) content2;
                            if ((list.ItemTypeName != null) && !list.ItemTypeName.IsEmpty)
                            {
                                empty = list.ItemTypeName;
                            }
                            if (list.ItemType != null)
                            {
                                o = list.ItemType;
                            }
                        }
                        else if (content2 is XmlSchemaSimpleTypeRestriction)
                        {
                            empty = ((XmlSchemaSimpleTypeRestriction) content2).BaseTypeName;
                        }
                        else if (c == typeof(XmlSchemaSimpleTypeUnion))
                        {
                            XmlQualifiedName[] memberTypes = ((XmlSchemaSimpleTypeUnion) item).MemberTypes;
                            if (memberTypes != null)
                            {
                                for (int i = 0; i < memberTypes.Length; i++)
                                {
                                    XmlSchemaType type5 = (XmlSchemaType) this.schemas.Find(memberTypes[i], typeof(XmlSchemaType), false);
                                    this.AddRef(refs, type5);
                                }
                            }
                        }
                    }
                    if (((o == null) && !empty.IsEmpty) && (empty.Namespace != "http://www.w3.org/2001/XMLSchema"))
                    {
                        o = (XmlSchemaType) this.schemas.Find(empty, typeof(XmlSchemaType), false);
                    }
                    if (o != null)
                    {
                        this.AddRef(refs, o);
                    }
                    if (particle != null)
                    {
                        this.Depends(particle, refs);
                    }
                    if (attributes != null)
                    {
                        for (int j = 0; j < attributes.Count; j++)
                        {
                            this.Depends(attributes[j], refs);
                        }
                    }
                }
                else if (c == typeof(XmlSchemaElement))
                {
                    XmlSchemaElement element = (XmlSchemaElement) item;
                    if (!element.SubstitutionGroup.IsEmpty && (element.SubstitutionGroup.Namespace != "http://www.w3.org/2001/XMLSchema"))
                    {
                        XmlSchemaElement element2 = (XmlSchemaElement) this.schemas.Find(element.SubstitutionGroup, typeof(XmlSchemaElement), false);
                        this.AddRef(refs, element2);
                    }
                    if (!element.RefName.IsEmpty)
                    {
                        element = (XmlSchemaElement) this.schemas.Find(element.RefName, typeof(XmlSchemaElement), false);
                        this.AddRef(refs, element);
                    }
                    else if (!element.SchemaTypeName.IsEmpty)
                    {
                        XmlSchemaType type6 = (XmlSchemaType) this.schemas.Find(element.SchemaTypeName, typeof(XmlSchemaType), false);
                        this.AddRef(refs, type6);
                    }
                    else
                    {
                        this.Depends(element.SchemaType, refs);
                    }
                }
                else if (c == typeof(XmlSchemaGroup))
                {
                    this.Depends(((XmlSchemaGroup) item).Particle);
                }
                else if (c == typeof(XmlSchemaGroupRef))
                {
                    XmlSchemaGroup group = (XmlSchemaGroup) this.schemas.Find(((XmlSchemaGroupRef) item).RefName, typeof(XmlSchemaGroup), false);
                    this.AddRef(refs, group);
                }
                else if (typeof(XmlSchemaGroupBase).IsAssignableFrom(c))
                {
                    foreach (XmlSchemaObject obj2 in ((XmlSchemaGroupBase) item).Items)
                    {
                        this.Depends(obj2, refs);
                    }
                }
                else if (c == typeof(XmlSchemaAttributeGroupRef))
                {
                    XmlSchemaAttributeGroup group2 = (XmlSchemaAttributeGroup) this.schemas.Find(((XmlSchemaAttributeGroupRef) item).RefName, typeof(XmlSchemaAttributeGroup), false);
                    this.AddRef(refs, group2);
                }
                else if (c == typeof(XmlSchemaAttributeGroup))
                {
                    foreach (XmlSchemaObject obj3 in ((XmlSchemaAttributeGroup) item).Attributes)
                    {
                        this.Depends(obj3, refs);
                    }
                }
                else if (c == typeof(XmlSchemaAttribute))
                {
                    XmlSchemaAttribute attribute = (XmlSchemaAttribute) item;
                    if (!attribute.RefName.IsEmpty)
                    {
                        attribute = (XmlSchemaAttribute) this.schemas.Find(attribute.RefName, typeof(XmlSchemaAttribute), false);
                        this.AddRef(refs, attribute);
                    }
                    else if (!attribute.SchemaTypeName.IsEmpty)
                    {
                        XmlSchemaType type7 = (XmlSchemaType) this.schemas.Find(attribute.SchemaTypeName, typeof(XmlSchemaType), false);
                        this.AddRef(refs, type7);
                    }
                    else
                    {
                        this.Depends(attribute.SchemaType, refs);
                    }
                }
                if (typeof(XmlSchemaAnnotated).IsAssignableFrom(c))
                {
                    XmlAttribute[] unhandledAttributes = ((XmlSchemaAnnotated) item).UnhandledAttributes;
                    if (unhandledAttributes != null)
                    {
                        for (int k = 0; k < unhandledAttributes.Length; k++)
                        {
                            XmlAttribute attribute2 = unhandledAttributes[k];
                            if ((attribute2.LocalName == "arrayType") && (attribute2.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/"))
                            {
                                string str;
                                XmlQualifiedName name = TypeScope.ParseWsdlArrayType(attribute2.Value, out str, item);
                                XmlSchemaType type8 = (XmlSchemaType) this.schemas.Find(name, typeof(XmlSchemaType), false);
                                this.AddRef(refs, type8);
                            }
                        }
                    }
                }
            }
        }

        internal ArrayList GetItems()
        {
            return new ArrayList(this.scope.Keys);
        }
    }
}

