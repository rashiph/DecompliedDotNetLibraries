namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Xml;

    public sealed class XmlSchemaInference
    {
        internal const short HC_ST_boolean = 0;
        internal const short HC_ST_byte = 1;
        internal const short HC_ST_Count = 0x13;
        internal const short HC_ST_date = 0x10;
        internal const short HC_ST_dateTime = 14;
        internal const short HC_ST_decimal = 10;
        internal const short HC_ST_double = 12;
        internal const short HC_ST_duration = 13;
        internal const short HC_ST_float = 11;
        internal const short HC_ST_gYearMonth = 0x11;
        internal const short HC_ST_int = 5;
        internal const short HC_ST_integer = 9;
        internal const short HC_ST_long = 7;
        internal const short HC_ST_short = 3;
        internal const short HC_ST_string = 0x12;
        internal const short HC_ST_time = 15;
        internal const short HC_ST_unsignedByte = 2;
        internal const short HC_ST_unsignedInt = 6;
        internal const short HC_ST_unsignedLong = 8;
        internal const short HC_ST_unsignedShort = 4;
        private XmlNamespaceManager NamespaceManager;
        private NameTable nametable = new NameTable();
        private InferenceOption occurrence;
        private XmlSchema rootSchema;
        private ArrayList schemaList;
        private XmlSchemaSet schemaSet;
        internal static XmlQualifiedName[] SimpleTypes = new XmlQualifiedName[] { 
            ST_boolean, ST_byte, ST_unsignedByte, ST_short, ST_unsignedShort, ST_int, ST_unsignedInt, ST_long, ST_unsignedLong, ST_integer, ST_decimal, ST_float, ST_double, ST_duration, ST_dateTime, ST_time, 
            ST_date, ST_gYearMonth, ST_string
         };
        internal static XmlQualifiedName ST_anySimpleType = new XmlQualifiedName("anySimpleType", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_boolean = new XmlQualifiedName("boolean", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_byte = new XmlQualifiedName("byte", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_date = new XmlQualifiedName("date", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_dateTime = new XmlQualifiedName("dateTime", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_decimal = new XmlQualifiedName("decimal", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_double = new XmlQualifiedName("double", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_duration = new XmlQualifiedName("duration", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_float = new XmlQualifiedName("float", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_gYearMonth = new XmlQualifiedName("gYearMonth", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_int = new XmlQualifiedName("int", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_integer = new XmlQualifiedName("integer", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_long = new XmlQualifiedName("long", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_short = new XmlQualifiedName("short", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_string = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_time = new XmlQualifiedName("time", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_unsignedByte = new XmlQualifiedName("unsignedByte", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_unsignedInt = new XmlQualifiedName("unsignedInt", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_unsignedLong = new XmlQualifiedName("unsignedLong", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName ST_unsignedShort = new XmlQualifiedName("unsignedShort", "http://www.w3.org/2001/XMLSchema");
        private string TargetNamespace;
        internal const int TF_boolean = 1;
        internal const int TF_byte = 2;
        internal const int TF_date = 0x10000;
        internal const int TF_dateTime = 0x4000;
        internal const int TF_decimal = 0x400;
        internal const int TF_double = 0x1000;
        internal const int TF_duration = 0x2000;
        internal const int TF_float = 0x800;
        internal const int TF_gYearMonth = 0x20000;
        internal const int TF_int = 0x20;
        internal const int TF_integer = 0x200;
        internal const int TF_long = 0x80;
        internal const int TF_short = 8;
        internal const int TF_string = 0x40000;
        internal const int TF_time = 0x8000;
        internal const int TF_unsignedByte = 4;
        internal const int TF_unsignedInt = 0x40;
        internal const int TF_unsignedLong = 0x100;
        internal const int TF_unsignedShort = 0x10;
        private InferenceOption typeInference;
        private XmlReader xtr;

        public XmlSchemaInference()
        {
            this.NamespaceManager = new XmlNamespaceManager(this.nametable);
            this.NamespaceManager.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
            this.schemaList = new ArrayList();
        }

        private XmlSchemaAttribute AddAttribute(string localName, string prefix, string childURI, string attrValue, bool bCreatingNewType, XmlSchema parentSchema, XmlSchemaObjectCollection addLocation, XmlSchemaObjectTable compiledAttributes)
        {
            ICollection values;
            ICollection is3;
            if (childURI == "http://www.w3.org/2001/XMLSchema")
            {
                throw new XmlSchemaInferenceException("SchInf_schema", 0, 0);
            }
            XmlSchemaAttribute item = null;
            int iTypeFlags = -1;
            XmlSchemaAttribute attribute2 = null;
            XmlSchema schema = null;
            bool flag = true;
            if (compiledAttributes.Count > 0)
            {
                values = compiledAttributes.Values;
                is3 = addLocation;
            }
            else
            {
                values = addLocation;
                is3 = null;
            }
            if (childURI == "http://www.w3.org/XML/1998/namespace")
            {
                XmlSchemaAttribute attribute3 = null;
                attribute3 = this.FindAttributeRef(values, localName, childURI);
                if ((attribute3 == null) && (is3 != null))
                {
                    attribute3 = this.FindAttributeRef(is3, localName, childURI);
                }
                if (attribute3 == null)
                {
                    attribute3 = new XmlSchemaAttribute {
                        RefName = new XmlQualifiedName(localName, childURI)
                    };
                    if (bCreatingNewType && (this.Occurrence == InferenceOption.Restricted))
                    {
                        attribute3.Use = XmlSchemaUse.Required;
                    }
                    else
                    {
                        attribute3.Use = XmlSchemaUse.Optional;
                    }
                    addLocation.Add(attribute3);
                }
                attribute2 = attribute3;
            }
            else
            {
                if (childURI.Length == 0)
                {
                    schema = parentSchema;
                    flag = false;
                }
                else if ((childURI != null) && !this.schemaSet.Contains(childURI))
                {
                    schema = new XmlSchema {
                        AttributeFormDefault = XmlSchemaForm.Unqualified,
                        ElementFormDefault = XmlSchemaForm.Qualified
                    };
                    if (childURI.Length != 0)
                    {
                        schema.TargetNamespace = childURI;
                    }
                    this.schemaSet.Add(schema);
                    if ((prefix.Length != 0) && (string.Compare(prefix, "xml", StringComparison.OrdinalIgnoreCase) != 0))
                    {
                        this.NamespaceManager.AddNamespace(prefix, childURI);
                    }
                }
                else
                {
                    ArrayList list = this.schemaSet.Schemas(childURI) as ArrayList;
                    if ((list != null) && (list.Count > 0))
                    {
                        schema = list[0] as XmlSchema;
                    }
                }
                if (childURI.Length != 0)
                {
                    XmlSchemaAttribute attribute4 = null;
                    attribute4 = this.FindAttributeRef(values, localName, childURI);
                    if ((attribute4 == null) & (is3 != null))
                    {
                        attribute4 = this.FindAttributeRef(is3, localName, childURI);
                    }
                    if (attribute4 == null)
                    {
                        attribute4 = new XmlSchemaAttribute {
                            RefName = new XmlQualifiedName(localName, childURI)
                        };
                        if (bCreatingNewType && (this.Occurrence == InferenceOption.Restricted))
                        {
                            attribute4.Use = XmlSchemaUse.Required;
                        }
                        else
                        {
                            attribute4.Use = XmlSchemaUse.Optional;
                        }
                        addLocation.Add(attribute4);
                    }
                    attribute2 = attribute4;
                    item = this.FindAttribute(schema.Items, localName);
                    if (item == null)
                    {
                        item = new XmlSchemaAttribute {
                            Name = localName,
                            SchemaTypeName = this.RefineSimpleType(attrValue, ref iTypeFlags),
                            LineNumber = iTypeFlags
                        };
                        schema.Items.Add(item);
                    }
                    else
                    {
                        if (item.Parent == null)
                        {
                            iTypeFlags = item.LineNumber;
                        }
                        else
                        {
                            iTypeFlags = GetSchemaType(item.SchemaTypeName);
                            item.Parent = null;
                        }
                        item.SchemaTypeName = this.RefineSimpleType(attrValue, ref iTypeFlags);
                        item.LineNumber = iTypeFlags;
                    }
                }
                else
                {
                    item = this.FindAttribute(values, localName);
                    if ((item == null) && (is3 != null))
                    {
                        item = this.FindAttribute(is3, localName);
                    }
                    if (item == null)
                    {
                        item = new XmlSchemaAttribute {
                            Name = localName,
                            SchemaTypeName = this.RefineSimpleType(attrValue, ref iTypeFlags),
                            LineNumber = iTypeFlags
                        };
                        if (bCreatingNewType && (this.Occurrence == InferenceOption.Restricted))
                        {
                            item.Use = XmlSchemaUse.Required;
                        }
                        else
                        {
                            item.Use = XmlSchemaUse.Optional;
                        }
                        addLocation.Add(item);
                        if (schema.AttributeFormDefault != XmlSchemaForm.Unqualified)
                        {
                            item.Form = XmlSchemaForm.Unqualified;
                        }
                    }
                    else
                    {
                        if (item.Parent == null)
                        {
                            iTypeFlags = item.LineNumber;
                        }
                        else
                        {
                            iTypeFlags = GetSchemaType(item.SchemaTypeName);
                            item.Parent = null;
                        }
                        item.SchemaTypeName = this.RefineSimpleType(attrValue, ref iTypeFlags);
                        item.LineNumber = iTypeFlags;
                    }
                    attribute2 = item;
                }
            }
            string str = null;
            if (flag && (childURI != parentSchema.TargetNamespace))
            {
                for (int i = 0; i < parentSchema.Includes.Count; i++)
                {
                    XmlSchemaImport import = parentSchema.Includes[i] as XmlSchemaImport;
                    if ((import != null) && (import.Namespace == childURI))
                    {
                        flag = false;
                    }
                }
                if (!flag)
                {
                    return attribute2;
                }
                XmlSchemaImport import2 = new XmlSchemaImport {
                    Schema = schema
                };
                if (childURI.Length != 0)
                {
                    str = childURI;
                }
                import2.Namespace = str;
                parentSchema.Includes.Add(import2);
            }
            return attribute2;
        }

        private XmlSchemaElement AddElement(string localName, string prefix, string childURI, XmlSchema parentSchema, XmlSchemaObjectCollection addLocation, int positionWithinCollection)
        {
            if (childURI == "http://www.w3.org/2001/XMLSchema")
            {
                throw new XmlSchemaInferenceException("SchInf_schema", 0, 0);
            }
            XmlSchemaElement item = null;
            XmlSchemaElement element2 = item;
            XmlSchema schema = null;
            bool bCreatingNewType = true;
            if (childURI == string.Empty)
            {
                childURI = null;
            }
            if ((parentSchema != null) && (childURI == parentSchema.TargetNamespace))
            {
                item = new XmlSchemaElement {
                    Name = localName
                };
                schema = parentSchema;
                if ((schema.ElementFormDefault != XmlSchemaForm.Qualified) && (addLocation != null))
                {
                    item.Form = XmlSchemaForm.Qualified;
                }
            }
            else if (this.schemaSet.Contains(childURI))
            {
                item = this.FindGlobalElement(childURI, localName, out schema);
                if (item == null)
                {
                    ArrayList list = this.schemaSet.Schemas(childURI) as ArrayList;
                    if ((list != null) && (list.Count > 0))
                    {
                        schema = list[0] as XmlSchema;
                    }
                    item = new XmlSchemaElement {
                        Name = localName
                    };
                    schema.Items.Add(item);
                }
                else
                {
                    bCreatingNewType = false;
                }
            }
            else
            {
                schema = this.CreateXmlSchema(childURI);
                if (prefix.Length != 0)
                {
                    this.NamespaceManager.AddNamespace(prefix, childURI);
                }
                item = new XmlSchemaElement {
                    Name = localName
                };
                schema.Items.Add(item);
            }
            if (parentSchema == null)
            {
                parentSchema = schema;
                this.rootSchema = parentSchema;
            }
            if (childURI != parentSchema.TargetNamespace)
            {
                bool flag2 = true;
                for (int i = 0; i < parentSchema.Includes.Count; i++)
                {
                    XmlSchemaImport import = parentSchema.Includes[i] as XmlSchemaImport;
                    if ((import != null) && (import.Namespace == childURI))
                    {
                        flag2 = false;
                    }
                }
                if (flag2)
                {
                    XmlSchemaImport import2 = new XmlSchemaImport {
                        Schema = schema,
                        Namespace = childURI
                    };
                    parentSchema.Includes.Add(import2);
                }
            }
            element2 = item;
            if (addLocation != null)
            {
                if (childURI == parentSchema.TargetNamespace)
                {
                    if (this.Occurrence == InferenceOption.Relaxed)
                    {
                        item.MinOccurs = 0M;
                    }
                    if (positionWithinCollection == -1)
                    {
                        positionWithinCollection = addLocation.Add(item);
                    }
                    else
                    {
                        addLocation.Insert(positionWithinCollection, item);
                    }
                }
                else
                {
                    XmlSchemaElement element3 = new XmlSchemaElement {
                        RefName = new XmlQualifiedName(localName, childURI)
                    };
                    if (this.Occurrence == InferenceOption.Relaxed)
                    {
                        element3.MinOccurs = 0M;
                    }
                    if (positionWithinCollection == -1)
                    {
                        positionWithinCollection = addLocation.Add(element3);
                    }
                    else
                    {
                        addLocation.Insert(positionWithinCollection, element3);
                    }
                    element2 = element3;
                }
            }
            this.InferElement(item, bCreatingNewType, schema);
            return element2;
        }

        private XmlSchemaSimpleContentExtension CheckSimpleContentExtension(XmlSchemaComplexType ct)
        {
            XmlSchemaSimpleContent contentModel = ct.ContentModel as XmlSchemaSimpleContent;
            if (contentModel == null)
            {
                throw new XmlSchemaInferenceException("SchInf_simplecontent", 0, 0);
            }
            XmlSchemaSimpleContentExtension content = contentModel.Content as XmlSchemaSimpleContentExtension;
            if (content == null)
            {
                throw new XmlSchemaInferenceException("SchInf_extension", 0, 0);
            }
            return content;
        }

        private XmlSchemaElement CreateNewElementforChoice(XmlSchemaElement copyElement)
        {
            XmlSchemaElement element = new XmlSchemaElement {
                Annotation = copyElement.Annotation,
                Block = copyElement.Block,
                DefaultValue = copyElement.DefaultValue,
                Final = copyElement.Final,
                FixedValue = copyElement.FixedValue,
                Form = copyElement.Form,
                Id = copyElement.Id
            };
            if (copyElement.IsNillable)
            {
                element.IsNillable = copyElement.IsNillable;
            }
            element.LineNumber = copyElement.LineNumber;
            element.LinePosition = copyElement.LinePosition;
            element.Name = copyElement.Name;
            element.Namespaces = copyElement.Namespaces;
            element.RefName = copyElement.RefName;
            element.SchemaType = copyElement.SchemaType;
            element.SchemaTypeName = copyElement.SchemaTypeName;
            element.SourceUri = copyElement.SourceUri;
            element.SubstitutionGroup = copyElement.SubstitutionGroup;
            element.UnhandledAttributes = copyElement.UnhandledAttributes;
            if ((copyElement.MinOccurs != 1M) && (this.Occurrence == InferenceOption.Relaxed))
            {
                element.MinOccurs = copyElement.MinOccurs;
            }
            if (copyElement.MaxOccurs != 1M)
            {
                element.MaxOccurs = copyElement.MaxOccurs;
            }
            return element;
        }

        private XmlSchema CreateXmlSchema(string targetNS)
        {
            XmlSchema schema = new XmlSchema {
                AttributeFormDefault = XmlSchemaForm.Unqualified,
                ElementFormDefault = XmlSchemaForm.Qualified,
                TargetNamespace = targetNS
            };
            this.schemaSet.Add(schema);
            return schema;
        }

        internal static int DateTime(string s, bool bDate, bool bTime)
        {
            try
            {
                XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.RoundtripKind);
            }
            catch (FormatException)
            {
                return 0x40000;
            }
            if (bDate && bTime)
            {
                return 0x44000;
            }
            if (bDate)
            {
                return 0x50000;
            }
            if (bTime)
            {
                return 0x48000;
            }
            return 0x40000;
        }

        internal XmlSchemaAttribute FindAttribute(ICollection attributes, string attrName)
        {
            foreach (XmlSchemaObject obj2 in attributes)
            {
                XmlSchemaAttribute attribute = obj2 as XmlSchemaAttribute;
                if ((attribute != null) && (attribute.Name == attrName))
                {
                    return attribute;
                }
            }
            return null;
        }

        internal XmlSchemaAttribute FindAttributeRef(ICollection attributes, string attributeName, string nsURI)
        {
            foreach (XmlSchemaObject obj2 in attributes)
            {
                XmlSchemaAttribute attribute = obj2 as XmlSchemaAttribute;
                if (((attribute != null) && (attribute.RefName.Name == attributeName)) && (attribute.RefName.Namespace == nsURI))
                {
                    return attribute;
                }
            }
            return null;
        }

        internal XmlSchemaElement FindElement(XmlSchemaObjectCollection elements, string elementName)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                XmlSchemaElement element = elements[i] as XmlSchemaElement;
                if (((element != null) && (element.RefName != null)) && (element.Name == elementName))
                {
                    return element;
                }
            }
            return null;
        }

        internal XmlSchemaElement FindElementRef(XmlSchemaObjectCollection elements, string elementName, string nsURI)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                XmlSchemaElement element = elements[i] as XmlSchemaElement;
                if (((element != null) && (element.RefName != null)) && ((element.RefName.Name == elementName) && (element.RefName.Namespace == nsURI)))
                {
                    return element;
                }
            }
            return null;
        }

        internal XmlSchemaElement FindGlobalElement(string namespaceURI, string localName, out XmlSchema parentSchema)
        {
            ICollection is2 = this.schemaSet.Schemas(namespaceURI);
            XmlSchemaElement element = null;
            parentSchema = null;
            foreach (XmlSchema schema in is2)
            {
                element = this.FindElement(schema.Items, localName);
                if (element != null)
                {
                    parentSchema = schema;
                    return element;
                }
            }
            return null;
        }

        internal XmlSchemaElement FindMatchingElement(bool bCreatingNewType, XmlReader xtr, XmlSchemaComplexType ct, ref int lastUsedSeqItem, ref bool bParticleChanged, XmlSchema parentSchema, bool setMaxoccurs)
        {
            int num6;
            if (xtr.NamespaceURI == "http://www.w3.org/2001/XMLSchema")
            {
                throw new XmlSchemaInferenceException("SchInf_schema", 0, 0);
            }
            bool flag = lastUsedSeqItem == -1;
            XmlSchemaObjectCollection objects = new XmlSchemaObjectCollection();
            if (!(ct.Particle.GetType() == typeof(XmlSchemaSequence)))
            {
                throw new XmlSchemaInferenceException("SchInf_noseq", 0, 0);
            }
            string namespaceURI = xtr.NamespaceURI;
            if (namespaceURI.Length == 0)
            {
                namespaceURI = null;
            }
            XmlSchemaSequence sequence = (XmlSchemaSequence) ct.Particle;
            if ((sequence.Items.Count < 1) && !bCreatingNewType)
            {
                lastUsedSeqItem = 0;
                XmlSchemaElement element = this.AddElement(xtr.LocalName, xtr.Prefix, xtr.NamespaceURI, parentSchema, sequence.Items, -1);
                element.MinOccurs = 0M;
                return element;
            }
            if (sequence.Items[0].GetType() == typeof(XmlSchemaChoice))
            {
                XmlSchemaChoice choice = (XmlSchemaChoice) sequence.Items[0];
                for (int i = 0; i < choice.Items.Count; i++)
                {
                    XmlSchemaElement element2 = choice.Items[i] as XmlSchemaElement;
                    if (element2 == null)
                    {
                        throw new XmlSchemaInferenceException("SchInf_UnknownParticle", 0, 0);
                    }
                    if ((element2.Name == xtr.LocalName) && (parentSchema.TargetNamespace == namespaceURI))
                    {
                        this.InferElement(element2, false, parentSchema);
                        this.SetMinMaxOccurs(element2, setMaxoccurs);
                        return element2;
                    }
                    if ((element2.RefName.Name == xtr.LocalName) && (element2.RefName.Namespace == xtr.NamespaceURI))
                    {
                        XmlSchemaElement element3 = this.FindGlobalElement(namespaceURI, xtr.LocalName, out parentSchema);
                        this.InferElement(element3, false, parentSchema);
                        this.SetMinMaxOccurs(element2, setMaxoccurs);
                        return element3;
                    }
                }
                return this.AddElement(xtr.LocalName, xtr.Prefix, xtr.NamespaceURI, parentSchema, choice.Items, -1);
            }
            int num2 = 0;
            if (lastUsedSeqItem >= 0)
            {
                num2 = lastUsedSeqItem;
            }
            XmlSchemaParticle particle = sequence.Items[num2] as XmlSchemaParticle;
            XmlSchemaElement xse = particle as XmlSchemaElement;
            if (xse == null)
            {
                throw new XmlSchemaInferenceException("SchInf_UnknownParticle", 0, 0);
            }
            if ((xse.Name == xtr.LocalName) && (parentSchema.TargetNamespace == namespaceURI))
            {
                if (!flag)
                {
                    xse.MaxOccurs = 79228162514264337593543950335M;
                }
                lastUsedSeqItem = num2;
                this.InferElement(xse, false, parentSchema);
                this.SetMinMaxOccurs(xse, false);
                return xse;
            }
            if ((xse.RefName.Name == xtr.LocalName) && (xse.RefName.Namespace == xtr.NamespaceURI))
            {
                if (!flag)
                {
                    xse.MaxOccurs = 79228162514264337593543950335M;
                }
                lastUsedSeqItem = num2;
                XmlSchemaElement element6 = this.FindGlobalElement(namespaceURI, xtr.LocalName, out parentSchema);
                this.InferElement(element6, false, parentSchema);
                this.SetMinMaxOccurs(xse, false);
                return xse;
            }
            if (flag && (xse.MinOccurs != 0M))
            {
                objects.Add(xse);
            }
            num2++;
            while (num2 < sequence.Items.Count)
            {
                particle = sequence.Items[num2] as XmlSchemaParticle;
                xse = particle as XmlSchemaElement;
                if (xse == null)
                {
                    throw new XmlSchemaInferenceException("SchInf_UnknownParticle", 0, 0);
                }
                if ((xse.Name == xtr.LocalName) && (parentSchema.TargetNamespace == namespaceURI))
                {
                    lastUsedSeqItem = num2;
                    for (int j = 0; j < objects.Count; j++)
                    {
                        ((XmlSchemaElement) objects[j]).MinOccurs = 0M;
                    }
                    this.InferElement(xse, false, parentSchema);
                    this.SetMinMaxOccurs(xse, setMaxoccurs);
                    return xse;
                }
                if ((xse.RefName.Name == xtr.LocalName) && (xse.RefName.Namespace == xtr.NamespaceURI))
                {
                    lastUsedSeqItem = num2;
                    for (int k = 0; k < objects.Count; k++)
                    {
                        ((XmlSchemaElement) objects[k]).MinOccurs = 0M;
                    }
                    XmlSchemaElement element7 = this.FindGlobalElement(namespaceURI, xtr.LocalName, out parentSchema);
                    this.InferElement(element7, false, parentSchema);
                    this.SetMinMaxOccurs(xse, setMaxoccurs);
                    return element7;
                }
                objects.Add(xse);
                num2++;
            }
            XmlSchemaElement el = null;
            XmlSchemaElement element9 = null;
            if (parentSchema.TargetNamespace == namespaceURI)
            {
                el = this.FindElement(sequence.Items, xtr.LocalName);
                element9 = el;
            }
            else
            {
                el = this.FindElementRef(sequence.Items, xtr.LocalName, xtr.NamespaceURI);
                if (el != null)
                {
                    element9 = this.FindGlobalElement(namespaceURI, xtr.LocalName, out parentSchema);
                }
            }
            if (el != null)
            {
                XmlSchemaChoice item = new XmlSchemaChoice {
                    MaxOccurs = 79228162514264337593543950335M
                };
                this.SetMinMaxOccurs(el, setMaxoccurs);
                this.InferElement(element9, false, parentSchema);
                for (int m = 0; m < sequence.Items.Count; m++)
                {
                    item.Items.Add(this.CreateNewElementforChoice((XmlSchemaElement) sequence.Items[m]));
                }
                sequence.Items.Clear();
                sequence.Items.Add(item);
                return el;
            }
            lastUsedSeqItem = num6 = lastUsedSeqItem + 1;
            el = this.AddElement(xtr.LocalName, xtr.Prefix, xtr.NamespaceURI, parentSchema, sequence.Items, num6);
            if (!bCreatingNewType)
            {
                el.MinOccurs = 0M;
            }
            return el;
        }

        private XmlSchemaType GetEffectiveSchemaType(XmlSchemaElement elem, bool bCreatingNewType)
        {
            XmlSchemaType builtInSimpleType = null;
            if (!bCreatingNewType && (elem.ElementSchemaType != null))
            {
                return elem.ElementSchemaType;
            }
            if (elem.SchemaType != null)
            {
                return elem.SchemaType;
            }
            if (elem.SchemaTypeName != XmlQualifiedName.Empty)
            {
                builtInSimpleType = this.schemaSet.GlobalTypes[elem.SchemaTypeName] as XmlSchemaType;
                if (builtInSimpleType == null)
                {
                    builtInSimpleType = XmlSchemaType.GetBuiltInSimpleType(elem.SchemaTypeName);
                }
                if (builtInSimpleType == null)
                {
                    builtInSimpleType = XmlSchemaType.GetBuiltInComplexType(elem.SchemaTypeName);
                }
            }
            return builtInSimpleType;
        }

        private static int GetSchemaType(XmlQualifiedName qname)
        {
            if (qname == SimpleTypes[0])
            {
                return 0x40001;
            }
            if (qname == SimpleTypes[1])
            {
                return 0x41eaa;
            }
            if (qname == SimpleTypes[2])
            {
                return 0x41ffe;
            }
            if (qname == SimpleTypes[3])
            {
                return 0x41ea8;
            }
            if (qname == SimpleTypes[4])
            {
                return 0x41ff8;
            }
            if (qname == SimpleTypes[5])
            {
                return 0x41ea0;
            }
            if (qname == SimpleTypes[6])
            {
                return 0x41fe0;
            }
            if (qname == SimpleTypes[7])
            {
                return 0x41e80;
            }
            if (qname == SimpleTypes[8])
            {
                return 0x41f80;
            }
            if (qname == SimpleTypes[9])
            {
                return 0x41e00;
            }
            if (qname == SimpleTypes[10])
            {
                return 0x41c00;
            }
            if (qname == SimpleTypes[11])
            {
                return 0x41800;
            }
            if (qname == SimpleTypes[12])
            {
                return 0x41000;
            }
            if (qname == SimpleTypes[13])
            {
                return 0x42000;
            }
            if (qname == SimpleTypes[14])
            {
                return 0x44000;
            }
            if (qname == SimpleTypes[15])
            {
                return 0x48000;
            }
            if (qname == SimpleTypes[0x10])
            {
                return 0x10000;
            }
            if (qname == SimpleTypes[0x11])
            {
                return 0x20000;
            }
            if (qname == SimpleTypes[0x12])
            {
                return 0x40000;
            }
            if ((qname != null) && !qname.IsEmpty)
            {
                throw new XmlSchemaInferenceException("SchInf_schematype", 0, 0);
            }
            return -1;
        }

        internal void InferElement(XmlSchemaElement xse, bool bCreatingNewType, XmlSchema parentSchema)
        {
            bool isEmptyElement = this.xtr.IsEmptyElement;
            int lastUsedSeqItem = -1;
            Hashtable hashtable = new Hashtable();
            XmlSchemaType effectiveSchemaType = this.GetEffectiveSchemaType(xse, bCreatingNewType);
            XmlSchemaComplexType ct = effectiveSchemaType as XmlSchemaComplexType;
            if (this.xtr.MoveToFirstAttribute())
            {
                this.ProcessAttributes(ref xse, effectiveSchemaType, bCreatingNewType, parentSchema);
            }
            else if (!bCreatingNewType && (ct != null))
            {
                this.MakeExistingAttributesOptional(ct, null);
            }
            if ((ct == null) || (ct == XmlSchemaComplexType.AnyType))
            {
                ct = xse.SchemaType as XmlSchemaComplexType;
            }
            if (isEmptyElement)
            {
                if (!bCreatingNewType)
                {
                    if (ct == null)
                    {
                        if (!xse.SchemaTypeName.IsEmpty)
                        {
                            xse.LineNumber = 0x40000;
                            xse.SchemaTypeName = ST_string;
                        }
                    }
                    else if (ct.Particle != null)
                    {
                        ct.Particle.MinOccurs = 0M;
                    }
                    else if (ct.ContentModel != null)
                    {
                        XmlSchemaSimpleContentExtension extension = this.CheckSimpleContentExtension(ct);
                        extension.BaseTypeName = ST_string;
                        extension.LineNumber = 0x40000;
                    }
                }
                else
                {
                    xse.LineNumber = 0x40000;
                }
            }
            else
            {
                bool flag2 = false;
                do
                {
                    this.xtr.Read();
                    if (this.xtr.NodeType == XmlNodeType.Whitespace)
                    {
                        flag2 = true;
                    }
                    if (this.xtr.NodeType == XmlNodeType.EntityReference)
                    {
                        throw new XmlSchemaInferenceException("SchInf_entity", 0, 0);
                    }
                }
                while ((!this.xtr.EOF && (this.xtr.NodeType != XmlNodeType.EndElement)) && (((this.xtr.NodeType != XmlNodeType.CDATA) && (this.xtr.NodeType != XmlNodeType.Element)) && (this.xtr.NodeType != XmlNodeType.Text)));
                if (this.xtr.NodeType == XmlNodeType.EndElement)
                {
                    if (flag2)
                    {
                        if (ct != null)
                        {
                            if (ct.ContentModel != null)
                            {
                                XmlSchemaSimpleContentExtension extension2 = this.CheckSimpleContentExtension(ct);
                                extension2.BaseTypeName = ST_string;
                                extension2.LineNumber = 0x40000;
                            }
                            else if (bCreatingNewType)
                            {
                                XmlSchemaSimpleContent content = new XmlSchemaSimpleContent();
                                ct.ContentModel = content;
                                XmlSchemaSimpleContentExtension simpleContentExtension = new XmlSchemaSimpleContentExtension();
                                content.Content = simpleContentExtension;
                                this.MoveAttributes(ct, simpleContentExtension, bCreatingNewType);
                                simpleContentExtension.BaseTypeName = ST_string;
                                simpleContentExtension.LineNumber = 0x40000;
                            }
                            else
                            {
                                ct.IsMixed = true;
                            }
                        }
                        else
                        {
                            xse.SchemaTypeName = ST_string;
                            xse.LineNumber = 0x40000;
                        }
                    }
                    if (bCreatingNewType)
                    {
                        xse.LineNumber = 0x40000;
                    }
                    else if (ct != null)
                    {
                        if (ct.Particle != null)
                        {
                            ct.Particle.MinOccurs = 0M;
                        }
                        else if (ct.ContentModel != null)
                        {
                            XmlSchemaSimpleContentExtension extension4 = this.CheckSimpleContentExtension(ct);
                            extension4.BaseTypeName = ST_string;
                            extension4.LineNumber = 0x40000;
                        }
                    }
                    else if (!xse.SchemaTypeName.IsEmpty)
                    {
                        xse.LineNumber = 0x40000;
                        xse.SchemaTypeName = ST_string;
                    }
                }
                else
                {
                    int num2 = 0;
                    bool flag3 = false;
                    while (!this.xtr.EOF && (this.xtr.NodeType != XmlNodeType.EndElement))
                    {
                        bool flag4 = false;
                        num2++;
                        if ((this.xtr.NodeType == XmlNodeType.Text) || (this.xtr.NodeType == XmlNodeType.CDATA))
                        {
                            if (ct != null)
                            {
                                if (ct.Particle != null)
                                {
                                    ct.IsMixed = true;
                                    if (num2 == 1)
                                    {
                                        do
                                        {
                                            this.xtr.Read();
                                        }
                                        while (!this.xtr.EOF && ((((this.xtr.NodeType == XmlNodeType.CDATA) || (this.xtr.NodeType == XmlNodeType.Text)) || ((this.xtr.NodeType == XmlNodeType.Comment) || (this.xtr.NodeType == XmlNodeType.ProcessingInstruction))) || (((this.xtr.NodeType == XmlNodeType.Whitespace) || (this.xtr.NodeType == XmlNodeType.SignificantWhitespace)) || (this.xtr.NodeType == XmlNodeType.XmlDeclaration))));
                                        flag4 = true;
                                        if (this.xtr.NodeType == XmlNodeType.EndElement)
                                        {
                                            ct.Particle.MinOccurs = 0M;
                                        }
                                    }
                                }
                                else if (ct.ContentModel != null)
                                {
                                    XmlSchemaSimpleContentExtension extension5 = this.CheckSimpleContentExtension(ct);
                                    if ((this.xtr.NodeType == XmlNodeType.Text) && (num2 == 1))
                                    {
                                        int iTypeFlags = -1;
                                        if (xse.Parent == null)
                                        {
                                            iTypeFlags = extension5.LineNumber;
                                        }
                                        else
                                        {
                                            iTypeFlags = GetSchemaType(extension5.BaseTypeName);
                                            xse.Parent = null;
                                        }
                                        extension5.BaseTypeName = this.RefineSimpleType(this.xtr.Value, ref iTypeFlags);
                                        extension5.LineNumber = iTypeFlags;
                                    }
                                    else
                                    {
                                        extension5.BaseTypeName = ST_string;
                                        extension5.LineNumber = 0x40000;
                                    }
                                }
                                else
                                {
                                    XmlSchemaSimpleContent content2 = new XmlSchemaSimpleContent();
                                    ct.ContentModel = content2;
                                    XmlSchemaSimpleContentExtension extension6 = new XmlSchemaSimpleContentExtension();
                                    content2.Content = extension6;
                                    this.MoveAttributes(ct, extension6, bCreatingNewType);
                                    if (this.xtr.NodeType == XmlNodeType.Text)
                                    {
                                        int num4;
                                        if (!bCreatingNewType)
                                        {
                                            num4 = 0x40000;
                                        }
                                        else
                                        {
                                            num4 = -1;
                                        }
                                        extension6.BaseTypeName = this.RefineSimpleType(this.xtr.Value, ref num4);
                                        extension6.LineNumber = num4;
                                    }
                                    else
                                    {
                                        extension6.BaseTypeName = ST_string;
                                        extension6.LineNumber = 0x40000;
                                    }
                                }
                            }
                            else if (num2 > 1)
                            {
                                xse.SchemaTypeName = ST_string;
                                xse.LineNumber = 0x40000;
                            }
                            else
                            {
                                int lineNumber = -1;
                                if (bCreatingNewType)
                                {
                                    if (this.xtr.NodeType == XmlNodeType.Text)
                                    {
                                        xse.SchemaTypeName = this.RefineSimpleType(this.xtr.Value, ref lineNumber);
                                        xse.LineNumber = lineNumber;
                                    }
                                    else
                                    {
                                        xse.SchemaTypeName = ST_string;
                                        xse.LineNumber = 0x40000;
                                    }
                                }
                                else if (this.xtr.NodeType == XmlNodeType.Text)
                                {
                                    if (xse.Parent == null)
                                    {
                                        lineNumber = xse.LineNumber;
                                    }
                                    else
                                    {
                                        lineNumber = GetSchemaType(xse.SchemaTypeName);
                                        if ((lineNumber == -1) && (xse.LineNumber == 0x40000))
                                        {
                                            lineNumber = 0x40000;
                                        }
                                        xse.Parent = null;
                                    }
                                    xse.SchemaTypeName = this.RefineSimpleType(this.xtr.Value, ref lineNumber);
                                    xse.LineNumber = lineNumber;
                                }
                                else
                                {
                                    xse.SchemaTypeName = ST_string;
                                    xse.LineNumber = 0x40000;
                                }
                            }
                        }
                        else if (this.xtr.NodeType == XmlNodeType.Element)
                        {
                            XmlQualifiedName key = new XmlQualifiedName(this.xtr.LocalName, this.xtr.NamespaceURI);
                            bool setMaxoccurs = false;
                            if (hashtable.Contains(key))
                            {
                                setMaxoccurs = true;
                            }
                            else
                            {
                                hashtable.Add(key, null);
                            }
                            if (ct == null)
                            {
                                ct = new XmlSchemaComplexType();
                                xse.SchemaType = ct;
                                if (!xse.SchemaTypeName.IsEmpty)
                                {
                                    ct.IsMixed = true;
                                    xse.SchemaTypeName = XmlQualifiedName.Empty;
                                }
                            }
                            if (ct.ContentModel != null)
                            {
                                XmlSchemaSimpleContentExtension scExtension = this.CheckSimpleContentExtension(ct);
                                this.MoveAttributes(scExtension, ct);
                                ct.ContentModel = null;
                                ct.IsMixed = true;
                                if (ct.Particle != null)
                                {
                                    throw new XmlSchemaInferenceException("SchInf_particle", 0, 0);
                                }
                                ct.Particle = new XmlSchemaSequence();
                                flag3 = true;
                                this.AddElement(this.xtr.LocalName, this.xtr.Prefix, this.xtr.NamespaceURI, parentSchema, ((XmlSchemaSequence) ct.Particle).Items, -1);
                                lastUsedSeqItem = 0;
                                if (!bCreatingNewType)
                                {
                                    ct.Particle.MinOccurs = 0M;
                                }
                            }
                            else if (ct.Particle == null)
                            {
                                ct.Particle = new XmlSchemaSequence();
                                flag3 = true;
                                this.AddElement(this.xtr.LocalName, this.xtr.Prefix, this.xtr.NamespaceURI, parentSchema, ((XmlSchemaSequence) ct.Particle).Items, -1);
                                if (!bCreatingNewType)
                                {
                                    ((XmlSchemaSequence) ct.Particle).MinOccurs = 0M;
                                }
                                lastUsedSeqItem = 0;
                            }
                            else
                            {
                                bool bParticleChanged = false;
                                this.FindMatchingElement(bCreatingNewType || flag3, this.xtr, ct, ref lastUsedSeqItem, ref bParticleChanged, parentSchema, setMaxoccurs);
                            }
                        }
                        else if (this.xtr.NodeType == XmlNodeType.Text)
                        {
                            if (ct == null)
                            {
                                throw new XmlSchemaInferenceException("SchInf_ct", 0, 0);
                            }
                            ct.IsMixed = true;
                        }
                        do
                        {
                            if (this.xtr.NodeType == XmlNodeType.EntityReference)
                            {
                                throw new XmlSchemaInferenceException("SchInf_entity", 0, 0);
                            }
                            if (!flag4)
                            {
                                this.xtr.Read();
                            }
                            else
                            {
                                flag4 = false;
                            }
                        }
                        while ((!this.xtr.EOF && (this.xtr.NodeType != XmlNodeType.EndElement)) && (((this.xtr.NodeType != XmlNodeType.CDATA) && (this.xtr.NodeType != XmlNodeType.Element)) && (this.xtr.NodeType != XmlNodeType.Text)));
                    }
                    if (lastUsedSeqItem != -1)
                    {
                        while (++lastUsedSeqItem < ((XmlSchemaSequence) ct.Particle).Items.Count)
                        {
                            if (((XmlSchemaSequence) ct.Particle).Items[lastUsedSeqItem].GetType() != typeof(XmlSchemaElement))
                            {
                                throw new XmlSchemaInferenceException("SchInf_seq", 0, 0);
                            }
                            XmlSchemaElement element = (XmlSchemaElement) ((XmlSchemaSequence) ct.Particle).Items[lastUsedSeqItem];
                            element.MinOccurs = 0M;
                        }
                    }
                }
            }
        }

        public XmlSchemaSet InferSchema(XmlReader instanceDocument)
        {
            return this.InferSchema1(instanceDocument, new XmlSchemaSet(this.nametable));
        }

        public XmlSchemaSet InferSchema(XmlReader instanceDocument, XmlSchemaSet schemas)
        {
            if (schemas == null)
            {
                schemas = new XmlSchemaSet(this.nametable);
            }
            return this.InferSchema1(instanceDocument, schemas);
        }

        internal XmlSchemaSet InferSchema1(XmlReader instanceDocument, XmlSchemaSet schemas)
        {
            if (instanceDocument == null)
            {
                throw new ArgumentNullException("instanceDocument");
            }
            this.rootSchema = null;
            this.xtr = instanceDocument;
            schemas.Compile();
            this.schemaSet = schemas;
            while ((this.xtr.NodeType != XmlNodeType.Element) && this.xtr.Read())
            {
            }
            if (this.xtr.NodeType != XmlNodeType.Element)
            {
                throw new XmlSchemaInferenceException("SchInf_NoElement", 0, 0);
            }
            this.TargetNamespace = this.xtr.NamespaceURI;
            if (this.xtr.NamespaceURI == "http://www.w3.org/2001/XMLSchema")
            {
                throw new XmlSchemaInferenceException("SchInf_schema", 0, 0);
            }
            XmlSchemaElement xse = null;
            foreach (XmlSchemaElement element2 in schemas.GlobalElements.Values)
            {
                if ((element2.Name == this.xtr.LocalName) && (element2.QualifiedName.Namespace == this.xtr.NamespaceURI))
                {
                    this.rootSchema = element2.Parent as XmlSchema;
                    xse = element2;
                    break;
                }
            }
            if (this.rootSchema == null)
            {
                xse = this.AddElement(this.xtr.LocalName, this.xtr.Prefix, this.xtr.NamespaceURI, null, null, -1);
            }
            else
            {
                this.InferElement(xse, false, this.rootSchema);
            }
            foreach (string str in this.NamespaceManager)
            {
                if (!str.Equals("xml") && !str.Equals("xmlns"))
                {
                    string ns = this.NamespaceManager.LookupNamespace(this.nametable.Get(str));
                    if (ns.Length != 0)
                    {
                        this.rootSchema.Namespaces.Add(str, ns);
                    }
                }
            }
            schemas.Reprocess(this.rootSchema);
            schemas.Compile();
            return schemas;
        }

        internal static int InferSimpleType(string s, ref bool bNeedsRangeCheck)
        {
            int num;
            bool flag = false;
            bool flag2 = false;
            bool bDate = false;
            bool bTime = false;
            bool flag5 = false;
            if (s.Length != 0)
            {
                num = 0;
                switch (s[num])
                {
                    case 'N':
                        if (!(s == "NaN"))
                        {
                            return 0x40000;
                        }
                        return 0x41800;

                    case 'O':
                    case ',':
                    case '/':
                        goto Label_0EAB;

                    case 'P':
                        goto Label_031D;

                    case 'f':
                    case 't':
                        if (s == "true")
                        {
                            return 0x40001;
                        }
                        if (s == "false")
                        {
                            return 0x40001;
                        }
                        return 0x40000;

                    case '+':
                        flag2 = true;
                        num++;
                        if (num != s.Length)
                        {
                            switch (s[num])
                            {
                                case '.':
                                    goto Label_0107;

                                case 'P':
                                    goto Label_031D;
                            }
                            if ((s[num] < '0') || (s[num] > '9'))
                            {
                                return 0x40000;
                            }
                            goto Label_0754;
                        }
                        return 0x40000;

                    case '-':
                        flag = true;
                        num++;
                        if (num != s.Length)
                        {
                            switch (s[num])
                            {
                                case '.':
                                    goto Label_0107;

                                case 'P':
                                    goto Label_031D;
                            }
                            if ((s[num] < '0') || (s[num] > '9'))
                            {
                                return 0x40000;
                            }
                            goto Label_0754;
                        }
                        return 0x40000;

                    case '.':
                        goto Label_0107;

                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        goto Label_0754;

                    case 'I':
                        break;

                    default:
                        goto Label_0EAB;
                }
                if (s.Substring(num) == "INF")
                {
                    return 0x41800;
                }
            }
            return 0x40000;
        Label_0107:
            bNeedsRangeCheck = true;
            num++;
            if (num == s.Length)
            {
                if ((num != 1) && ((num != 2) || (!flag2 && !flag)))
                {
                    return 0x41c00;
                }
                return 0x40000;
            }
            switch (s[num])
            {
                case 'E':
                case 'e':
                    break;

                default:
                    if ((s[num] >= '0') && (s[num] <= '9'))
                    {
                        do
                        {
                            num++;
                            if (num == s.Length)
                            {
                                return 0x41c00;
                            }
                            switch (s[num])
                            {
                            }
                        }
                        while ((s[num] >= '0') && (s[num] <= '9'));
                    }
                    return 0x40000;
            }
            num++;
            if (num != s.Length)
            {
                switch (s[num])
                {
                    case '+':
                    case '-':
                        num++;
                        if (num != s.Length)
                        {
                            if ((s[num] < '0') || (s[num] > '9'))
                            {
                                return 0x40000;
                            }
                            break;
                        }
                        return 0x40000;

                    default:
                        if ((s[num] < '0') || (s[num] > '9'))
                        {
                            return 0x40000;
                        }
                        break;
                }
                do
                {
                    num++;
                    if (num == s.Length)
                    {
                        return 0x41800;
                    }
                }
                while ((s[num] >= '0') && (s[num] <= '9'));
            }
            return 0x40000;
        Label_031D:
            num++;
            if (num == s.Length)
            {
                return 0x40000;
            }
            char ch7 = s[num];
            if (ch7 == 'T')
            {
                goto Label_050E;
            }
            if ((s[num] < '0') || (s[num] > '9'))
            {
                return 0x40000;
            }
        Label_0364:
            num++;
            if (num != s.Length)
            {
                switch (s[num])
                {
                    case 'D':
                        goto Label_04DF;

                    case 'M':
                        goto Label_0451;

                    case 'Y':
                    {
                        num++;
                        if (num == s.Length)
                        {
                            bNeedsRangeCheck = true;
                            return 0x42000;
                        }
                        char ch9 = s[num];
                        if (ch9 == 'T')
                        {
                            goto Label_050E;
                        }
                        if ((s[num] >= '0') && (s[num] <= '9'))
                        {
                        Label_0404:
                            num++;
                            if (num != s.Length)
                            {
                                switch (s[num])
                                {
                                    case 'D':
                                        goto Label_04DF;

                                    case 'M':
                                        goto Label_0451;
                                }
                                if ((s[num] >= '0') && (s[num] <= '9'))
                                {
                                    goto Label_0404;
                                }
                            }
                            return 0x40000;
                        }
                        return 0x40000;
                    }
                }
                if ((s[num] >= '0') && (s[num] <= '9'))
                {
                    goto Label_0364;
                }
            }
            return 0x40000;
        Label_0451:
            num++;
            if (num == s.Length)
            {
                bNeedsRangeCheck = true;
                return 0x42000;
            }
            char ch11 = s[num];
            if (ch11 == 'T')
            {
                goto Label_050E;
            }
            if ((s[num] < '0') || (s[num] > '9'))
            {
                return 0x40000;
            }
            while (true)
            {
                num++;
                if (num == s.Length)
                {
                    return 0x40000;
                }
                char ch12 = s[num];
                if (ch12 == 'D')
                {
                    break;
                }
                if ((s[num] < '0') || (s[num] > '9'))
                {
                    return 0x40000;
                }
            }
        Label_04DF:
            num++;
            if (num == s.Length)
            {
                bNeedsRangeCheck = true;
                return 0x42000;
            }
            char ch13 = s[num];
            if (ch13 != 'T')
            {
                return 0x40000;
            }
        Label_050E:
            num++;
            if (num != s.Length)
            {
                if ((s[num] < '0') || (s[num] > '9'))
                {
                    return 0x40000;
                }
                do
                {
                    num++;
                    if (num == s.Length)
                    {
                        return 0x40000;
                    }
                    switch (s[num])
                    {
                        case '.':
                            goto Label_06BA;

                        case 'H':
                            num++;
                            if (num == s.Length)
                            {
                                bNeedsRangeCheck = true;
                                return 0x42000;
                            }
                            if ((s[num] < '0') || (s[num] > '9'))
                            {
                                return 0x40000;
                            }
                        Label_05E0:
                            num++;
                            if (num != s.Length)
                            {
                                switch (s[num])
                                {
                                    case '.':
                                        goto Label_06BA;

                                    case 'M':
                                        goto Label_0636;

                                    case 'S':
                                        goto Label_0735;
                                }
                                if ((s[num] >= '0') && (s[num] <= '9'))
                                {
                                    goto Label_05E0;
                                }
                            }
                            return 0x40000;

                        case 'M':
                            goto Label_0636;

                        case 'S':
                            goto Label_0735;
                    }
                }
                while ((s[num] >= '0') && (s[num] <= '9'));
            }
            return 0x40000;
        Label_0636:
            num++;
            if (num == s.Length)
            {
                bNeedsRangeCheck = true;
                return 0x42000;
            }
            if ((s[num] < '0') || (s[num] > '9'))
            {
                return 0x40000;
            }
        Label_066D:
            num++;
            if (num != s.Length)
            {
                switch (s[num])
                {
                    case '.':
                        goto Label_06BA;

                    case 'S':
                        goto Label_0735;
                }
                if ((s[num] >= '0') && (s[num] <= '9'))
                {
                    goto Label_066D;
                }
            }
            return 0x40000;
        Label_06BA:
            num++;
            if (num == s.Length)
            {
                bNeedsRangeCheck = true;
                return 0x42000;
            }
            if ((s[num] < '0') || (s[num] > '9'))
            {
                return 0x40000;
            }
            while (true)
            {
                num++;
                if (num == s.Length)
                {
                    return 0x40000;
                }
                char ch17 = s[num];
                if (ch17 == 'S')
                {
                    break;
                }
                if ((s[num] < '0') || (s[num] > '9'))
                {
                    return 0x40000;
                }
            }
        Label_0735:
            num++;
            if (num == s.Length)
            {
                bNeedsRangeCheck = true;
                return 0x42000;
            }
            return 0x40000;
        Label_0754:
            num++;
            if (num == s.Length)
            {
                bNeedsRangeCheck = true;
                if (flag || flag2)
                {
                    return 0x41eaa;
                }
                if (!(s == "0") && !(s == "1"))
                {
                    return 0x41ffe;
                }
                return 0x41fff;
            }
            switch (s[num])
            {
                case '.':
                    goto Label_0107;

                case 'E':
                case 'e':
                    bNeedsRangeCheck = true;
                    return 0x41800;

                default:
                    if ((s[num] >= '0') && (s[num] <= '9'))
                    {
                        num++;
                        if (num == s.Length)
                        {
                            bNeedsRangeCheck = true;
                            if (!flag && !flag2)
                            {
                                return 0x41ffe;
                            }
                            return 0x41eaa;
                        }
                        switch (s[num])
                        {
                            case 'E':
                            case 'e':
                                bNeedsRangeCheck = true;
                                return 0x41800;

                            case '.':
                                goto Label_0107;

                            case ':':
                                bTime = true;
                                goto Label_0CC1;
                        }
                        if ((s[num] >= '0') && (s[num] <= '9'))
                        {
                            num++;
                            if (num == s.Length)
                            {
                                bNeedsRangeCheck = true;
                                if (!flag && !flag2)
                                {
                                    return 0x41ffe;
                                }
                                return 0x41eaa;
                            }
                            switch (s[num])
                            {
                                case '.':
                                    goto Label_0107;

                                case 'E':
                                case 'e':
                                    bNeedsRangeCheck = true;
                                    return 0x41800;
                            }
                            if ((s[num] < '0') || (s[num] > '9'))
                            {
                                return 0x40000;
                            }
                            do
                            {
                                num++;
                                if (num == s.Length)
                                {
                                    bNeedsRangeCheck = true;
                                    if (!flag && !flag2)
                                    {
                                        return 0x41ffe;
                                    }
                                    return 0x41eaa;
                                }
                                switch (s[num])
                                {
                                    case '-':
                                        bDate = true;
                                        num++;
                                        if (num == s.Length)
                                        {
                                            return 0x40000;
                                        }
                                        if ((s[num] < '0') || (s[num] > '9'))
                                        {
                                            return 0x40000;
                                        }
                                        num++;
                                        if (num == s.Length)
                                        {
                                            return 0x40000;
                                        }
                                        if ((s[num] < '0') || (s[num] > '9'))
                                        {
                                            return 0x40000;
                                        }
                                        num++;
                                        if (num == s.Length)
                                        {
                                            bNeedsRangeCheck = true;
                                            return 0x60000;
                                        }
                                        switch (s[num])
                                        {
                                            case '+':
                                                flag5 = true;
                                                goto Label_0B0D;

                                            case '-':
                                                num++;
                                                if (num != s.Length)
                                                {
                                                    if ((s[num] < '0') || (s[num] > '9'))
                                                    {
                                                        return 0x40000;
                                                    }
                                                    num++;
                                                    if (num == s.Length)
                                                    {
                                                        return 0x40000;
                                                    }
                                                    if ((s[num] < '0') || (s[num] > '9'))
                                                    {
                                                        return 0x40000;
                                                    }
                                                    num++;
                                                    if (num == s.Length)
                                                    {
                                                        return DateTime(s, bDate, bTime);
                                                    }
                                                    switch (s[num])
                                                    {
                                                        case 'T':
                                                            bTime = true;
                                                            num++;
                                                            if (num == s.Length)
                                                            {
                                                                return 0x40000;
                                                            }
                                                            if ((s[num] < '0') || (s[num] > '9'))
                                                            {
                                                                return 0x40000;
                                                            }
                                                            num++;
                                                            if (num == s.Length)
                                                            {
                                                                return 0x40000;
                                                            }
                                                            if ((s[num] < '0') || (s[num] > '9'))
                                                            {
                                                                return 0x40000;
                                                            }
                                                            num++;
                                                            if (num == s.Length)
                                                            {
                                                                return 0x40000;
                                                            }
                                                            if (s[num] != ':')
                                                            {
                                                                return 0x40000;
                                                            }
                                                            goto Label_0CC1;

                                                        case 'Z':
                                                        case 'z':
                                                            goto Label_0AE1;

                                                        case '+':
                                                        case '-':
                                                            goto Label_0B0D;

                                                        case ':':
                                                            flag5 = true;
                                                            goto Label_0B9D;
                                                    }
                                                }
                                                return 0x40000;

                                            case 'Z':
                                            case 'z':
                                                flag5 = true;
                                                goto Label_0AE1;
                                        }
                                        goto Label_0A03;

                                    case '.':
                                        goto Label_0107;

                                    case 'E':
                                    case 'e':
                                        bNeedsRangeCheck = true;
                                        return 0x41800;
                                }
                            }
                            while ((s[num] >= '0') && (s[num] <= '9'));
                        }
                    }
                    return 0x40000;
            }
        Label_0A03:
            return 0x40000;
        Label_0AE1:
            num++;
            if (num != s.Length)
            {
                return 0x40000;
            }
            if (flag5)
            {
                bNeedsRangeCheck = true;
                return 0x60000;
            }
            return DateTime(s, bDate, bTime);
        Label_0B0D:
            num++;
            if (num == s.Length)
            {
                return 0x40000;
            }
            if ((s[num] < '0') || (s[num] > '9'))
            {
                return 0x40000;
            }
            num++;
            if (num == s.Length)
            {
                return 0x40000;
            }
            if ((s[num] < '0') || (s[num] > '9'))
            {
                return 0x40000;
            }
            num++;
            if (num == s.Length)
            {
                return 0x40000;
            }
            if (s[num] != ':')
            {
                return 0x40000;
            }
        Label_0B9D:
            num++;
            if (num == s.Length)
            {
                return 0x40000;
            }
            if ((s[num] < '0') || (s[num] > '9'))
            {
                return 0x40000;
            }
            num++;
            if (num == s.Length)
            {
                return 0x40000;
            }
            if ((s[num] < '0') || (s[num] > '9'))
            {
                return 0x40000;
            }
            num++;
            if (num != s.Length)
            {
                return 0x40000;
            }
            if (flag5)
            {
                bNeedsRangeCheck = true;
                return 0x60000;
            }
            return DateTime(s, bDate, bTime);
        Label_0CC1:
            num++;
            if (num != s.Length)
            {
                if ((s[num] < '0') || (s[num] > '9'))
                {
                    return 0x40000;
                }
                num++;
                if (num == s.Length)
                {
                    return 0x40000;
                }
                if ((s[num] < '0') || (s[num] > '9'))
                {
                    return 0x40000;
                }
                num++;
                if (num == s.Length)
                {
                    return 0x40000;
                }
                if (s[num] != ':')
                {
                    return 0x40000;
                }
                num++;
                if (num == s.Length)
                {
                    return 0x40000;
                }
                if ((s[num] < '0') || (s[num] > '9'))
                {
                    return 0x40000;
                }
                num++;
                if (num == s.Length)
                {
                    return 0x40000;
                }
                if ((s[num] < '0') || (s[num] > '9'))
                {
                    return 0x40000;
                }
                num++;
                if (num == s.Length)
                {
                    return DateTime(s, bDate, bTime);
                }
                switch (s[num])
                {
                    case '+':
                    case '-':
                        goto Label_0B0D;

                    case '.':
                        num++;
                        if (num != s.Length)
                        {
                            if ((s[num] >= '0') && (s[num] <= '9'))
                            {
                                do
                                {
                                    num++;
                                    if (num == s.Length)
                                    {
                                        return DateTime(s, bDate, bTime);
                                    }
                                    switch (s[num])
                                    {
                                        case '+':
                                        case '-':
                                            goto Label_0B0D;

                                        case 'Z':
                                        case 'z':
                                            goto Label_0AE1;
                                    }
                                }
                                while ((s[num] >= '0') && (s[num] <= '9'));
                            }
                            return 0x40000;
                        }
                        return 0x40000;

                    case 'Z':
                    case 'z':
                        goto Label_0AE1;
                }
            }
            return 0x40000;
        Label_0EAB:
            return 0x40000;
        }

        internal void MakeExistingAttributesOptional(XmlSchemaComplexType ct, XmlSchemaObjectCollection attributesInInstance)
        {
            if (ct == null)
            {
                throw new XmlSchemaInferenceException("SchInf_noct", 0, 0);
            }
            if (ct.ContentModel != null)
            {
                XmlSchemaSimpleContentExtension extension = this.CheckSimpleContentExtension(ct);
                this.SwitchUseToOptional(extension.Attributes, attributesInInstance);
            }
            else
            {
                this.SwitchUseToOptional(ct.Attributes, attributesInInstance);
            }
        }

        private void MoveAttributes(XmlSchemaSimpleContentExtension scExtension, XmlSchemaComplexType ct)
        {
            for (int i = 0; i < scExtension.Attributes.Count; i++)
            {
                ct.Attributes.Add(scExtension.Attributes[i]);
            }
        }

        private void MoveAttributes(XmlSchemaComplexType ct, XmlSchemaSimpleContentExtension simpleContentExtension, bool bCreatingNewType)
        {
            ICollection values;
            if (!bCreatingNewType && (ct.AttributeUses.Count > 0))
            {
                values = ct.AttributeUses.Values;
            }
            else
            {
                values = ct.Attributes;
            }
            foreach (XmlSchemaAttribute attribute in values)
            {
                simpleContentExtension.Attributes.Add(attribute);
            }
            ct.Attributes.Clear();
        }

        internal void ProcessAttributes(ref XmlSchemaElement xse, XmlSchemaType effectiveSchemaType, bool bCreatingNewType, XmlSchema parentSchema)
        {
            XmlSchemaObjectCollection attributesInInstance = new XmlSchemaObjectCollection();
            XmlSchemaComplexType ct = effectiveSchemaType as XmlSchemaComplexType;
            do
            {
                if (this.xtr.NamespaceURI == "http://www.w3.org/2001/XMLSchema")
                {
                    throw new XmlSchemaInferenceException("SchInf_schema", 0, 0);
                }
                if (this.xtr.NamespaceURI == "http://www.w3.org/2000/xmlns/")
                {
                    if (this.xtr.Prefix == "xmlns")
                    {
                        this.NamespaceManager.AddNamespace(this.xtr.LocalName, this.xtr.Value);
                    }
                }
                else if (this.xtr.NamespaceURI == "http://www.w3.org/2001/XMLSchema-instance")
                {
                    string localName = this.xtr.LocalName;
                    if (localName != "nil")
                    {
                        if (((localName != "type") && (localName != "schemaLocation")) && (localName != "noNamespaceSchemaLocation"))
                        {
                            throw new XmlSchemaInferenceException("Sch_NotXsiAttribute", localName);
                        }
                    }
                    else
                    {
                        xse.IsNillable = true;
                    }
                }
                else
                {
                    if ((ct == null) || (ct == XmlSchemaComplexType.AnyType))
                    {
                        ct = new XmlSchemaComplexType();
                        xse.SchemaType = ct;
                    }
                    XmlSchemaAttribute item = null;
                    if (((effectiveSchemaType != null) && (effectiveSchemaType.Datatype != null)) && !xse.SchemaTypeName.IsEmpty)
                    {
                        XmlSchemaSimpleContent content = new XmlSchemaSimpleContent();
                        ct.ContentModel = content;
                        XmlSchemaSimpleContentExtension extension = new XmlSchemaSimpleContentExtension();
                        content.Content = extension;
                        extension.BaseTypeName = xse.SchemaTypeName;
                        extension.LineNumber = xse.LineNumber;
                        xse.LineNumber = 0;
                        xse.SchemaTypeName = XmlQualifiedName.Empty;
                    }
                    if (ct.ContentModel != null)
                    {
                        XmlSchemaSimpleContentExtension extension2 = this.CheckSimpleContentExtension(ct);
                        item = this.AddAttribute(this.xtr.LocalName, this.xtr.Prefix, this.xtr.NamespaceURI, this.xtr.Value, bCreatingNewType, parentSchema, extension2.Attributes, ct.AttributeUses);
                    }
                    else
                    {
                        item = this.AddAttribute(this.xtr.LocalName, this.xtr.Prefix, this.xtr.NamespaceURI, this.xtr.Value, bCreatingNewType, parentSchema, ct.Attributes, ct.AttributeUses);
                    }
                    if (item != null)
                    {
                        attributesInInstance.Add(item);
                    }
                }
            }
            while (this.xtr.MoveToNextAttribute());
            if (!bCreatingNewType && (ct != null))
            {
                this.MakeExistingAttributesOptional(ct, attributesInInstance);
            }
        }

        internal XmlQualifiedName RefineSimpleType(string s, ref int iTypeFlags)
        {
            bool bNeedsRangeCheck = false;
            s = s.Trim();
            if ((iTypeFlags != 0x40000) && (this.typeInference != InferenceOption.Relaxed))
            {
                iTypeFlags &= InferSimpleType(s, ref bNeedsRangeCheck);
                if (iTypeFlags == 0x40000)
                {
                    return ST_string;
                }
                if (bNeedsRangeCheck)
                {
                    if ((iTypeFlags & 2) != 0)
                    {
                        try
                        {
                            XmlConvert.ToSByte(s);
                            if ((iTypeFlags & 4) != 0)
                            {
                                return ST_unsignedByte;
                            }
                            return ST_byte;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags &= -3;
                    }
                    if ((iTypeFlags & 4) != 0)
                    {
                        try
                        {
                            XmlConvert.ToByte(s);
                            return ST_unsignedByte;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags &= -5;
                    }
                    if ((iTypeFlags & 8) != 0)
                    {
                        try
                        {
                            XmlConvert.ToInt16(s);
                            if ((iTypeFlags & 0x10) != 0)
                            {
                                return ST_unsignedShort;
                            }
                            return ST_short;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags &= -9;
                    }
                    if ((iTypeFlags & 0x10) != 0)
                    {
                        try
                        {
                            XmlConvert.ToUInt16(s);
                            return ST_unsignedShort;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags &= -17;
                    }
                    if ((iTypeFlags & 0x20) != 0)
                    {
                        try
                        {
                            XmlConvert.ToInt32(s);
                            if ((iTypeFlags & 0x40) != 0)
                            {
                                return ST_unsignedInt;
                            }
                            return ST_int;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags &= -33;
                    }
                    if ((iTypeFlags & 0x40) != 0)
                    {
                        try
                        {
                            XmlConvert.ToUInt32(s);
                            return ST_unsignedInt;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags &= -65;
                    }
                    if ((iTypeFlags & 0x80) != 0)
                    {
                        try
                        {
                            XmlConvert.ToInt64(s);
                            if ((iTypeFlags & 0x100) != 0)
                            {
                                return ST_unsignedLong;
                            }
                            return ST_long;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags &= -129;
                    }
                    if ((iTypeFlags & 0x100) != 0)
                    {
                        try
                        {
                            XmlConvert.ToUInt64(s);
                            return ST_unsignedLong;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags &= -257;
                    }
                    if ((iTypeFlags & 0x1000) != 0)
                    {
                        try
                        {
                            double num = XmlConvert.ToDouble(s);
                            if ((iTypeFlags & 0x200) != 0)
                            {
                                return ST_integer;
                            }
                            if ((iTypeFlags & 0x400) != 0)
                            {
                                return ST_decimal;
                            }
                            if ((iTypeFlags & 0x800) != 0)
                            {
                                try
                                {
                                    if (string.Compare(XmlConvert.ToString(XmlConvert.ToSingle(s)), XmlConvert.ToString(num), StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        return ST_float;
                                    }
                                }
                                catch (FormatException)
                                {
                                }
                                catch (OverflowException)
                                {
                                }
                            }
                            iTypeFlags &= -2049;
                            return ST_double;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags &= -4097;
                    }
                    if ((iTypeFlags & 0x800) != 0)
                    {
                        try
                        {
                            XmlConvert.ToSingle(s);
                            if ((iTypeFlags & 0x200) != 0)
                            {
                                return ST_integer;
                            }
                            if ((iTypeFlags & 0x400) != 0)
                            {
                                return ST_decimal;
                            }
                            return ST_float;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags &= -2049;
                    }
                    if ((iTypeFlags & 0x200) != 0)
                    {
                        return ST_integer;
                    }
                    if ((iTypeFlags & 0x400) != 0)
                    {
                        return ST_decimal;
                    }
                    if (iTypeFlags == 0x60000)
                    {
                        try
                        {
                            XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.RoundtripKind);
                            return ST_gYearMonth;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags = 0x40000;
                        return ST_string;
                    }
                    if (iTypeFlags == 0x42000)
                    {
                        try
                        {
                            XmlConvert.ToTimeSpan(s);
                            return ST_duration;
                        }
                        catch (FormatException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        iTypeFlags = 0x40000;
                        return ST_string;
                    }
                    if (iTypeFlags == 0x40001)
                    {
                        return ST_boolean;
                    }
                }
                switch (iTypeFlags)
                {
                    case 0x10:
                        return ST_unsignedShort;

                    case 0x20:
                        return ST_int;

                    case 0x40:
                        return ST_unsignedInt;

                    case 1:
                        return ST_boolean;

                    case 2:
                        return ST_byte;

                    case 4:
                        return ST_unsignedByte;

                    case 8:
                        return ST_short;

                    case 0x80:
                        return ST_long;

                    case 0x100:
                        return ST_unsignedLong;

                    case 0x200:
                        return ST_integer;

                    case 0x400:
                        return ST_decimal;

                    case 0x800:
                        return ST_float;

                    case 0x1000:
                        return ST_double;

                    case 0x8000:
                        return ST_time;

                    case 0x10000:
                        return ST_date;

                    case 0x20000:
                        return ST_gYearMonth;

                    case 0x2000:
                        return ST_duration;

                    case 0x4000:
                        return ST_dateTime;

                    case 0x40000:
                        return ST_string;

                    case 0x40001:
                        return ST_boolean;

                    case 0x41000:
                        return ST_double;

                    case 0x41800:
                        return ST_float;

                    case 0x44000:
                        return ST_dateTime;

                    case 0x48000:
                        return ST_time;

                    case 0x50000:
                        return ST_date;
                }
            }
            return ST_string;
        }

        internal void SetMinMaxOccurs(XmlSchemaElement el, bool setMaxOccurs)
        {
            if (this.Occurrence == InferenceOption.Relaxed)
            {
                if (setMaxOccurs || (el.MaxOccurs > 1M))
                {
                    el.MaxOccurs = 79228162514264337593543950335M;
                }
                el.MinOccurs = 0M;
            }
            else if (el.MinOccurs > 1M)
            {
                el.MinOccurs = 1M;
            }
        }

        private void SwitchUseToOptional(XmlSchemaObjectCollection attributes, XmlSchemaObjectCollection attributesInInstance)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                XmlSchemaAttribute attribute = attributes[i] as XmlSchemaAttribute;
                if (attribute != null)
                {
                    if (attributesInInstance != null)
                    {
                        if (attribute.RefName.Name.Length == 0)
                        {
                            if (this.FindAttribute(attributesInInstance, attribute.Name) == null)
                            {
                                attribute.Use = XmlSchemaUse.Optional;
                            }
                        }
                        else if (this.FindAttributeRef(attributesInInstance, attribute.RefName.Name, attribute.RefName.Namespace) == null)
                        {
                            attribute.Use = XmlSchemaUse.Optional;
                        }
                    }
                    else
                    {
                        attribute.Use = XmlSchemaUse.Optional;
                    }
                }
            }
        }

        public InferenceOption Occurrence
        {
            get
            {
                return this.occurrence;
            }
            set
            {
                this.occurrence = value;
            }
        }

        public InferenceOption TypeInference
        {
            get
            {
                return this.typeInference;
            }
            set
            {
                this.typeInference = value;
            }
        }

        public enum InferenceOption
        {
            Restricted,
            Relaxed
        }
    }
}

