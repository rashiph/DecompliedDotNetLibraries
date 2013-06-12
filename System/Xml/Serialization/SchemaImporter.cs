namespace System.Xml.Serialization
{
    using Microsoft.CSharp;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Configuration;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.Serialization.Advanced;
    using System.Xml.Serialization.Configuration;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class SchemaImporter
    {
        private CodeDomProvider codeProvider;
        private System.Xml.Serialization.ImportContext context;
        private SchemaImporterExtensionCollection extensions;
        private System.Xml.Serialization.NameTable groupsInUse;
        private CodeGenerationOptions options;
        private StructMapping root;
        private bool rootImported;
        private XmlSchemas schemas;
        private TypeScope scope;
        private System.Xml.Serialization.NameTable typesInUse;

        internal SchemaImporter(XmlSchemas schemas, CodeGenerationOptions options, CodeDomProvider codeProvider, System.Xml.Serialization.ImportContext context)
        {
            if (!schemas.Contains("http://www.w3.org/2001/XMLSchema"))
            {
                schemas.AddReference(XmlSchemas.XsdSchema);
                schemas.SchemaSet.Add(XmlSchemas.XsdSchema);
            }
            if (!schemas.Contains("http://www.w3.org/XML/1998/namespace"))
            {
                schemas.AddReference(XmlSchemas.XmlSchema);
                schemas.SchemaSet.Add(XmlSchemas.XmlSchema);
            }
            this.schemas = schemas;
            this.options = options;
            this.codeProvider = codeProvider;
            this.context = context;
            this.Schemas.SetCache(this.Context.Cache, this.Context.ShareTypes);
            SchemaImporterExtensionsSection section = System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.SchemaImporterExtensionsSectionPath) as SchemaImporterExtensionsSection;
            if (section != null)
            {
                this.extensions = section.SchemaImporterExtensionsInternal;
            }
            else
            {
                this.extensions = new SchemaImporterExtensionCollection();
            }
        }

        internal void AddReference(XmlQualifiedName name, System.Xml.Serialization.NameTable references, string error)
        {
            if (name.Namespace != "http://www.w3.org/2001/XMLSchema")
            {
                if (references[name] != null)
                {
                    throw new InvalidOperationException(Res.GetString(error, new object[] { name.Name, name.Namespace }));
                }
                references[name] = name;
            }
        }

        internal void AddReservedIdentifiersForDataBinding(CodeIdentifiers scope)
        {
            if ((this.options & CodeGenerationOptions.EnableDataBinding) != CodeGenerationOptions.None)
            {
                scope.AddReserved(CodeExporter.PropertyChangedEvent.Name);
                scope.AddReserved(CodeExporter.RaisePropertyChangedEventMethod.Name);
            }
        }

        private StructMapping CreateRootMapping()
        {
            TypeDesc typeDesc = this.Scope.GetTypeDesc(typeof(object));
            return new StructMapping { TypeDesc = typeDesc, Members = new MemberMapping[0], IncludeInSchema = false, TypeName = "anyType", Namespace = "http://www.w3.org/2001/XMLSchema" };
        }

        internal string GenerateUniqueTypeName(string typeName)
        {
            typeName = CodeIdentifier.MakeValid(typeName);
            return this.TypeIdentifiers.AddUnique(typeName, typeName);
        }

        internal StructMapping GetRootMapping()
        {
            if (this.root == null)
            {
                this.root = this.CreateRootMapping();
            }
            return this.root;
        }

        internal abstract void ImportDerivedTypes(XmlQualifiedName baseName);
        internal StructMapping ImportRootMapping()
        {
            if (!this.rootImported)
            {
                this.rootImported = true;
                this.ImportDerivedTypes(XmlQualifiedName.Empty);
            }
            return this.GetRootMapping();
        }

        internal void MakeDerived(StructMapping structMapping, Type baseType, bool baseTypeCanBeIndirect)
        {
            structMapping.ReferencedByTopLevelElement = true;
            if (baseType != null)
            {
                TypeDesc typeDesc = this.Scope.GetTypeDesc(baseType);
                if (typeDesc != null)
                {
                    TypeDesc baseTypeDesc = structMapping.TypeDesc;
                    if (baseTypeCanBeIndirect)
                    {
                        while ((baseTypeDesc.BaseTypeDesc != null) && (baseTypeDesc.BaseTypeDesc != typeDesc))
                        {
                            baseTypeDesc = baseTypeDesc.BaseTypeDesc;
                        }
                    }
                    if ((baseTypeDesc.BaseTypeDesc != null) && (baseTypeDesc.BaseTypeDesc != typeDesc))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInvalidBaseType", new object[] { structMapping.TypeDesc.FullName, baseType.FullName, baseTypeDesc.BaseTypeDesc.FullName }));
                    }
                    baseTypeDesc.BaseTypeDesc = typeDesc;
                }
            }
        }

        internal void RemoveReference(XmlQualifiedName name, System.Xml.Serialization.NameTable references)
        {
            references[name] = null;
        }

        internal CodeDomProvider CodeProvider
        {
            get
            {
                if (this.codeProvider == null)
                {
                    this.codeProvider = new CSharpCodeProvider();
                }
                return this.codeProvider;
            }
        }

        internal System.Xml.Serialization.ImportContext Context
        {
            get
            {
                if (this.context == null)
                {
                    this.context = new System.Xml.Serialization.ImportContext();
                }
                return this.context;
            }
        }

        public SchemaImporterExtensionCollection Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new SchemaImporterExtensionCollection();
                }
                return this.extensions;
            }
        }

        internal System.Xml.Serialization.NameTable GroupsInUse
        {
            get
            {
                if (this.groupsInUse == null)
                {
                    this.groupsInUse = new System.Xml.Serialization.NameTable();
                }
                return this.groupsInUse;
            }
        }

        internal Hashtable ImportedElements
        {
            get
            {
                return this.Context.Elements;
            }
        }

        internal Hashtable ImportedMappings
        {
            get
            {
                return this.Context.Mappings;
            }
        }

        internal CodeGenerationOptions Options
        {
            get
            {
                return this.options;
            }
        }

        internal XmlSchemas Schemas
        {
            get
            {
                if (this.schemas == null)
                {
                    this.schemas = new XmlSchemas();
                }
                return this.schemas;
            }
        }

        internal TypeScope Scope
        {
            get
            {
                if (this.scope == null)
                {
                    this.scope = new TypeScope();
                }
                return this.scope;
            }
        }

        internal CodeIdentifiers TypeIdentifiers
        {
            get
            {
                return this.Context.TypeIdentifiers;
            }
        }

        internal System.Xml.Serialization.NameTable TypesInUse
        {
            get
            {
                if (this.typesInUse == null)
                {
                    this.typesInUse = new System.Xml.Serialization.NameTable();
                }
                return this.typesInUse;
            }
        }
    }
}

