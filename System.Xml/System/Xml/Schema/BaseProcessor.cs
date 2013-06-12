namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class BaseProcessor
    {
        private XmlSchemaCompilationSettings compilationSettings;
        private int errorCount;
        private ValidationEventHandler eventHandler;
        private XmlNameTable nameTable;
        private string NsXml;
        private System.Xml.Schema.SchemaNames schemaNames;

        public BaseProcessor(XmlNameTable nameTable, System.Xml.Schema.SchemaNames schemaNames, ValidationEventHandler eventHandler) : this(nameTable, schemaNames, eventHandler, new XmlSchemaCompilationSettings())
        {
        }

        public BaseProcessor(XmlNameTable nameTable, System.Xml.Schema.SchemaNames schemaNames, ValidationEventHandler eventHandler, XmlSchemaCompilationSettings compilationSettings)
        {
            this.nameTable = nameTable;
            this.schemaNames = schemaNames;
            this.eventHandler = eventHandler;
            this.compilationSettings = compilationSettings;
            this.NsXml = nameTable.Add("http://www.w3.org/XML/1998/namespace");
        }

        protected void AddToTable(XmlSchemaObjectTable table, XmlQualifiedName qname, XmlSchemaObject item)
        {
            if (qname.Name.Length != 0)
            {
                XmlSchemaObject existingObject = table[qname];
                if (existingObject != null)
                {
                    if (existingObject != item)
                    {
                        string code = "Sch_DupGlobalElement";
                        if (item is XmlSchemaAttributeGroup)
                        {
                            if (Ref.Equal(this.nameTable.Add(qname.Namespace), this.NsXml))
                            {
                                XmlSchemaObject obj3 = Preprocessor.GetBuildInSchema().AttributeGroups[qname];
                                if (existingObject == obj3)
                                {
                                    table.Insert(qname, item);
                                    return;
                                }
                                if (item == obj3)
                                {
                                    return;
                                }
                            }
                            else if (this.IsValidAttributeGroupRedefine(existingObject, item, table))
                            {
                                return;
                            }
                            code = "Sch_DupAttributeGroup";
                        }
                        else if (item is XmlSchemaAttribute)
                        {
                            if (Ref.Equal(this.nameTable.Add(qname.Namespace), this.NsXml))
                            {
                                XmlSchemaObject obj4 = Preprocessor.GetBuildInSchema().Attributes[qname];
                                if (existingObject == obj4)
                                {
                                    table.Insert(qname, item);
                                    return;
                                }
                                if (item == obj4)
                                {
                                    return;
                                }
                            }
                            code = "Sch_DupGlobalAttribute";
                        }
                        else if (item is XmlSchemaSimpleType)
                        {
                            if (this.IsValidTypeRedefine(existingObject, item, table))
                            {
                                return;
                            }
                            code = "Sch_DupSimpleType";
                        }
                        else if (item is XmlSchemaComplexType)
                        {
                            if (this.IsValidTypeRedefine(existingObject, item, table))
                            {
                                return;
                            }
                            code = "Sch_DupComplexType";
                        }
                        else if (item is XmlSchemaGroup)
                        {
                            if (this.IsValidGroupRedefine(existingObject, item, table))
                            {
                                return;
                            }
                            code = "Sch_DupGroup";
                        }
                        else if (item is XmlSchemaNotation)
                        {
                            code = "Sch_DupNotation";
                        }
                        else if (item is XmlSchemaIdentityConstraint)
                        {
                            code = "Sch_DupIdentityConstraint";
                        }
                        this.SendValidationEvent(code, qname.ToString(), item);
                    }
                }
                else
                {
                    table.Add(qname, item);
                }
            }
        }

        private bool IsValidAttributeGroupRedefine(XmlSchemaObject existingObject, XmlSchemaObject item, XmlSchemaObjectTable table)
        {
            XmlSchemaAttributeGroup group = item as XmlSchemaAttributeGroup;
            XmlSchemaAttributeGroup group2 = existingObject as XmlSchemaAttributeGroup;
            if (group2 == group.Redefined)
            {
                if (group2.AttributeUses.Count == 0)
                {
                    table.Insert(group.QualifiedName, group);
                    return true;
                }
            }
            else if (group2.Redefined == group)
            {
                return true;
            }
            return false;
        }

        private bool IsValidGroupRedefine(XmlSchemaObject existingObject, XmlSchemaObject item, XmlSchemaObjectTable table)
        {
            XmlSchemaGroup group = item as XmlSchemaGroup;
            XmlSchemaGroup group2 = existingObject as XmlSchemaGroup;
            if (group2 == group.Redefined)
            {
                if (group2.CanonicalParticle == null)
                {
                    table.Insert(group.QualifiedName, group);
                    return true;
                }
            }
            else if (group2.Redefined == group)
            {
                return true;
            }
            return false;
        }

        private bool IsValidTypeRedefine(XmlSchemaObject existingObject, XmlSchemaObject item, XmlSchemaObjectTable table)
        {
            XmlSchemaType type = item as XmlSchemaType;
            XmlSchemaType type2 = existingObject as XmlSchemaType;
            if (type2 == type.Redefined)
            {
                if (type2.ElementDecl == null)
                {
                    table.Insert(type.QualifiedName, type);
                    return true;
                }
            }
            else if (type2.Redefined == type)
            {
                return true;
            }
            return false;
        }

        protected void SendValidationEvent(XmlSchemaException e)
        {
            this.SendValidationEvent(e, XmlSeverityType.Error);
        }

        protected void SendValidationEvent(string code, XmlSchemaObject source)
        {
            this.SendValidationEvent(new XmlSchemaException(code, source), XmlSeverityType.Error);
        }

        protected void SendValidationEvent(XmlSchemaException e, XmlSeverityType severity)
        {
            if (severity == XmlSeverityType.Error)
            {
                this.errorCount++;
            }
            if (this.eventHandler != null)
            {
                this.eventHandler(null, new ValidationEventArgs(e, severity));
            }
            else if (severity == XmlSeverityType.Error)
            {
                throw e;
            }
        }

        protected void SendValidationEvent(string code, string msg, XmlSchemaObject source)
        {
            this.SendValidationEvent(new XmlSchemaException(code, msg, source), XmlSeverityType.Error);
        }

        protected void SendValidationEvent(string code, XmlSchemaObject source, XmlSeverityType severity)
        {
            this.SendValidationEvent(new XmlSchemaException(code, source), severity);
        }

        protected void SendValidationEvent(string code, string[] args, Exception innerException, XmlSchemaObject source)
        {
            this.SendValidationEvent(new XmlSchemaException(code, args, innerException, source.SourceUri, source.LineNumber, source.LinePosition, source), XmlSeverityType.Error);
        }

        protected void SendValidationEvent(string code, string msg1, string msg2, XmlSchemaObject source)
        {
            this.SendValidationEvent(new XmlSchemaException(code, new string[] { msg1, msg2 }, source), XmlSeverityType.Error);
        }

        protected void SendValidationEvent(string code, string msg, XmlSchemaObject source, XmlSeverityType severity)
        {
            this.SendValidationEvent(new XmlSchemaException(code, msg, source), severity);
        }

        protected void SendValidationEvent(string code, string msg1, string msg2, string sourceUri, int lineNumber, int linePosition)
        {
            this.SendValidationEvent(new XmlSchemaException(code, new string[] { msg1, msg2 }, sourceUri, lineNumber, linePosition), XmlSeverityType.Error);
        }

        protected void SendValidationEventNoThrow(XmlSchemaException e, XmlSeverityType severity)
        {
            if (severity == XmlSeverityType.Error)
            {
                this.errorCount++;
            }
            if (this.eventHandler != null)
            {
                this.eventHandler(null, new ValidationEventArgs(e, severity));
            }
        }

        protected XmlSchemaCompilationSettings CompilationSettings
        {
            get
            {
                return this.compilationSettings;
            }
        }

        protected ValidationEventHandler EventHandler
        {
            get
            {
                return this.eventHandler;
            }
        }

        protected bool HasErrors
        {
            get
            {
                return (this.errorCount != 0);
            }
        }

        protected XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }

        protected System.Xml.Schema.SchemaNames SchemaNames
        {
            get
            {
                if (this.schemaNames == null)
                {
                    this.schemaNames = new System.Xml.Schema.SchemaNames(this.nameTable);
                }
                return this.schemaNames;
            }
        }
    }
}

