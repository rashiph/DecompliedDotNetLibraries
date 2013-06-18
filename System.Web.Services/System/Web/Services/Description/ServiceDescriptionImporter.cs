namespace System.Web.Services.Description
{
    using Microsoft.CSharp;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web.Services;
    using System.Web.Services.Configuration;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class ServiceDescriptionImporter
    {
        private XmlSchemas abstractSchemas;
        private XmlSchemas allSchemas;
        private System.CodeDom.CodeCompileUnit codeCompileUnit;
        private CodeDomProvider codeProvider;
        private XmlSchemas concreteSchemas;
        private List<Type> extensions;
        private ProtocolImporter[] importers;
        private System.Xml.Serialization.CodeGenerationOptions options;
        private string protocolName;
        private XmlSchemas schemas;
        private ServiceDescriptionCollection serviceDescriptions;
        private ServiceDescriptionImportStyle style;

        public ServiceDescriptionImporter()
        {
            this.serviceDescriptions = new ServiceDescriptionCollection();
            this.schemas = new XmlSchemas();
            this.allSchemas = new XmlSchemas();
            this.options = System.Xml.Serialization.CodeGenerationOptions.GenerateOldAsync;
            this.abstractSchemas = new XmlSchemas();
            this.concreteSchemas = new XmlSchemas();
            Type[] protocolImporterTypes = WebServicesSection.Current.ProtocolImporterTypes;
            this.importers = new ProtocolImporter[protocolImporterTypes.Length];
            for (int i = 0; i < this.importers.Length; i++)
            {
                this.importers[i] = (ProtocolImporter) Activator.CreateInstance(protocolImporterTypes[i]);
                this.importers[i].Initialize(this);
            }
        }

        internal ServiceDescriptionImporter(System.CodeDom.CodeCompileUnit codeCompileUnit) : this()
        {
            this.codeCompileUnit = codeCompileUnit;
        }

        internal static void AddDocument(string path, object document, XmlSchemas schemas, ServiceDescriptionCollection descriptions, StringCollection warnings)
        {
            ServiceDescription serviceDescription = document as ServiceDescription;
            if (serviceDescription != null)
            {
                descriptions.Add(serviceDescription);
            }
            else
            {
                XmlSchema schema = document as XmlSchema;
                if (schema != null)
                {
                    schemas.Add(schema);
                }
            }
        }

        private void AddImport(XmlSchema schema, Hashtable imports)
        {
            if ((schema != null) && (imports[schema] == null))
            {
                imports.Add(schema, schema);
                foreach (XmlSchemaExternal external in schema.Includes)
                {
                    if (external is XmlSchemaImport)
                    {
                        XmlSchemaImport import = (XmlSchemaImport) external;
                        foreach (XmlSchema schema2 in this.allSchemas.GetSchemas(import.Namespace))
                        {
                            this.AddImport(schema2, imports);
                        }
                    }
                }
            }
        }

        private static void AddSchema(XmlSchema schema, bool isEncoded, bool isLiteral, XmlSchemas abstractSchemas, XmlSchemas concreteSchemas, Hashtable references)
        {
            if (schema != null)
            {
                if (isEncoded && !abstractSchemas.Contains(schema))
                {
                    if (references.Contains(schema))
                    {
                        abstractSchemas.AddReference(schema);
                    }
                    else
                    {
                        abstractSchemas.Add(schema);
                    }
                }
                if (isLiteral && !concreteSchemas.Contains(schema))
                {
                    if (references.Contains(schema))
                    {
                        concreteSchemas.AddReference(schema);
                    }
                    else
                    {
                        concreteSchemas.Add(schema);
                    }
                }
            }
        }

        public void AddServiceDescription(ServiceDescription serviceDescription, string appSettingUrlKey, string appSettingBaseUrl)
        {
            if (serviceDescription == null)
            {
                throw new ArgumentNullException("serviceDescription");
            }
            serviceDescription.AppSettingUrlKey = appSettingUrlKey;
            serviceDescription.AppSettingBaseUrl = appSettingBaseUrl;
            this.ServiceDescriptions.Add(serviceDescription);
        }

        internal static System.Xml.Serialization.ImportContext Context(CodeNamespace ns, Hashtable namespaces, bool verbose)
        {
            if (namespaces[ns.Name] == null)
            {
                namespaces[ns.Name] = new System.Xml.Serialization.ImportContext(new CodeIdentifiers(), true);
            }
            return (System.Xml.Serialization.ImportContext) namespaces[ns.Name];
        }

        private ProtocolImporter FindImporterByName(string protocolName)
        {
            for (int i = 0; i < this.importers.Length; i++)
            {
                ProtocolImporter importer = this.importers[i];
                if (string.Compare(this.ProtocolName, importer.ProtocolName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return importer;
                }
            }
            throw new ArgumentException(System.Web.Services.Res.GetString("ProtocolWithNameIsNotRecognized1", new object[] { protocolName }), "protocolName");
        }

        private void FindUse(MessagePart part, out bool isEncoded, out bool isLiteral)
        {
            isEncoded = false;
            isLiteral = false;
            string name = part.Message.Name;
            Operation operation = null;
            ServiceDescription serviceDescription = part.Message.ServiceDescription;
            foreach (PortType type in serviceDescription.PortTypes)
            {
                foreach (Operation operation2 in type.Operations)
                {
                    foreach (OperationMessage message in operation2.Messages)
                    {
                        if (message.Message.Equals(new XmlQualifiedName(part.Message.Name, serviceDescription.TargetNamespace)))
                        {
                            operation = operation2;
                            this.FindUse(operation, serviceDescription, name, ref isEncoded, ref isLiteral);
                        }
                    }
                }
            }
            if (operation == null)
            {
                this.FindUse(null, serviceDescription, name, ref isEncoded, ref isLiteral);
            }
        }

        private void FindUse(Operation operation, ServiceDescription description, string messageName, ref bool isEncoded, ref bool isLiteral)
        {
            string targetNamespace = description.TargetNamespace;
            foreach (Binding binding in description.Bindings)
            {
                if ((operation == null) || new XmlQualifiedName(operation.PortType.Name, targetNamespace).Equals(binding.Type))
                {
                    foreach (OperationBinding binding2 in binding.Operations)
                    {
                        if (binding2.Input != null)
                        {
                            foreach (object obj2 in binding2.Input.Extensions)
                            {
                                if (operation != null)
                                {
                                    SoapBodyBinding binding3 = obj2 as SoapBodyBinding;
                                    if ((binding3 != null) && operation.IsBoundBy(binding2))
                                    {
                                        if (binding3.Use == SoapBindingUse.Encoded)
                                        {
                                            isEncoded = true;
                                        }
                                        else if (binding3.Use == SoapBindingUse.Literal)
                                        {
                                            isLiteral = true;
                                        }
                                    }
                                }
                                else
                                {
                                    SoapHeaderBinding binding4 = obj2 as SoapHeaderBinding;
                                    if ((binding4 != null) && (binding4.Message.Name == messageName))
                                    {
                                        if (binding4.Use == SoapBindingUse.Encoded)
                                        {
                                            isEncoded = true;
                                        }
                                        else if (binding4.Use == SoapBindingUse.Literal)
                                        {
                                            isLiteral = true;
                                        }
                                    }
                                }
                            }
                        }
                        if (binding2.Output != null)
                        {
                            foreach (object obj3 in binding2.Output.Extensions)
                            {
                                if (operation != null)
                                {
                                    if (operation.IsBoundBy(binding2))
                                    {
                                        SoapBodyBinding binding5 = obj3 as SoapBodyBinding;
                                        if (binding5 != null)
                                        {
                                            if (binding5.Use == SoapBindingUse.Encoded)
                                            {
                                                isEncoded = true;
                                            }
                                            else if (binding5.Use == SoapBindingUse.Literal)
                                            {
                                                isLiteral = true;
                                            }
                                        }
                                        else if (obj3 is MimeXmlBinding)
                                        {
                                            isLiteral = true;
                                        }
                                    }
                                }
                                else
                                {
                                    SoapHeaderBinding binding6 = obj3 as SoapHeaderBinding;
                                    if ((binding6 != null) && (binding6.Message.Name == messageName))
                                    {
                                        if (binding6.Use == SoapBindingUse.Encoded)
                                        {
                                            isEncoded = true;
                                        }
                                        else if (binding6.Use == SoapBindingUse.Literal)
                                        {
                                            isLiteral = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static StringCollection GenerateWebReferences(WebReferenceCollection webReferences, CodeDomProvider codeProvider, System.CodeDom.CodeCompileUnit codeCompileUnit, WebReferenceOptions options)
        {
            if (codeCompileUnit != null)
            {
                codeCompileUnit.ReferencedAssemblies.Add("System.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.Xml.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.Web.Services.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.EnterpriseServices.dll");
            }
            Hashtable namespaces = new Hashtable();
            Hashtable exportContext = new Hashtable();
            foreach (WebReference reference in webReferences)
            {
                ServiceDescriptionImporter importer = new ServiceDescriptionImporter(codeCompileUnit);
                XmlSchemas schemas = new XmlSchemas();
                ServiceDescriptionCollection descriptions = new ServiceDescriptionCollection();
                foreach (DictionaryEntry entry in reference.Documents)
                {
                    AddDocument((string) entry.Key, entry.Value, schemas, descriptions, reference.ValidationWarnings);
                }
                importer.Schemas.Add(schemas);
                foreach (ServiceDescription description in descriptions)
                {
                    importer.AddServiceDescription(description, reference.AppSettingUrlKey, reference.AppSettingBaseUrl);
                }
                importer.CodeGenerator = codeProvider;
                importer.ProtocolName = reference.ProtocolName;
                importer.Style = options.Style;
                importer.CodeGenerationOptions = options.CodeGenerationOptions;
                foreach (string str in options.SchemaImporterExtensions)
                {
                    importer.Extensions.Add(Type.GetType(str, true));
                }
                System.Xml.Serialization.ImportContext importContext = Context(reference.ProxyCode, namespaces, options.Verbose);
                reference.Warnings = importer.Import(reference.ProxyCode, importContext, exportContext, reference.ValidationWarnings);
                if (reference.ValidationWarnings.Count != 0)
                {
                    reference.Warnings |= ServiceDescriptionImportWarnings.SchemaValidation;
                }
            }
            StringCollection strings = new StringCollection();
            if (options.Verbose)
            {
                foreach (System.Xml.Serialization.ImportContext context2 in namespaces.Values)
                {
                    foreach (string str2 in context2.Warnings)
                    {
                        strings.Add(str2);
                    }
                }
            }
            return strings;
        }

        public ServiceDescriptionImportWarnings Import(CodeNamespace codeNamespace, System.CodeDom.CodeCompileUnit codeCompileUnit)
        {
            if (codeCompileUnit != null)
            {
                codeCompileUnit.ReferencedAssemblies.Add("System.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.Xml.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.Web.Services.dll");
                codeCompileUnit.ReferencedAssemblies.Add("System.EnterpriseServices.dll");
            }
            return this.Import(codeNamespace, new System.Xml.Serialization.ImportContext(new CodeIdentifiers(), false), new Hashtable(), new StringCollection());
        }

        private ServiceDescriptionImportWarnings Import(CodeNamespace codeNamespace, System.Xml.Serialization.ImportContext importContext, Hashtable exportContext, StringCollection warnings)
        {
            Hashtable hashtable2;
            this.allSchemas = new XmlSchemas();
            foreach (XmlSchema schema in this.schemas)
            {
                this.allSchemas.Add(schema);
            }
            foreach (ServiceDescription description in this.serviceDescriptions)
            {
                foreach (XmlSchema schema2 in description.Types.Schemas)
                {
                    this.allSchemas.Add(schema2);
                }
            }
            Hashtable references = new Hashtable();
            if (!this.allSchemas.Contains("http://schemas.xmlsoap.org/wsdl/"))
            {
                this.allSchemas.AddReference(ServiceDescription.Schema);
                references[ServiceDescription.Schema] = ServiceDescription.Schema;
            }
            if (!this.allSchemas.Contains("http://schemas.xmlsoap.org/soap/encoding/"))
            {
                this.allSchemas.AddReference(ServiceDescription.SoapEncodingSchema);
                references[ServiceDescription.SoapEncodingSchema] = ServiceDescription.SoapEncodingSchema;
            }
            this.allSchemas.Compile(null, false);
            foreach (ServiceDescription description2 in this.serviceDescriptions)
            {
                foreach (Message message in description2.Messages)
                {
                    foreach (MessagePart part in message.Parts)
                    {
                        bool flag;
                        bool flag2;
                        this.FindUse(part, out flag, out flag2);
                        if ((part.Element != null) && !part.Element.IsEmpty)
                        {
                            if (flag)
                            {
                                throw new InvalidOperationException(System.Web.Services.Res.GetString("CanTSpecifyElementOnEncodedMessagePartsPart", new object[] { part.Name, message.Name }));
                            }
                            XmlSchemaElement element = (XmlSchemaElement) this.allSchemas.Find(part.Element, typeof(XmlSchemaElement));
                            if (element != null)
                            {
                                AddSchema(element.Parent as XmlSchema, flag, flag2, this.abstractSchemas, this.concreteSchemas, references);
                                if ((element.SchemaTypeName != null) && !element.SchemaTypeName.IsEmpty)
                                {
                                    XmlSchemaType type = (XmlSchemaType) this.allSchemas.Find(element.SchemaTypeName, typeof(XmlSchemaType));
                                    if (type != null)
                                    {
                                        AddSchema(type.Parent as XmlSchema, flag, flag2, this.abstractSchemas, this.concreteSchemas, references);
                                    }
                                }
                            }
                        }
                        if ((part.Type != null) && !part.Type.IsEmpty)
                        {
                            XmlSchemaType type2 = (XmlSchemaType) this.allSchemas.Find(part.Type, typeof(XmlSchemaType));
                            if (type2 != null)
                            {
                                AddSchema(type2.Parent as XmlSchema, flag, flag2, this.abstractSchemas, this.concreteSchemas, references);
                            }
                        }
                    }
                }
            }
            foreach (XmlSchemas schemas in new XmlSchemas[] { this.abstractSchemas, this.concreteSchemas })
            {
                hashtable2 = new Hashtable();
                foreach (XmlSchema schema3 in schemas)
                {
                    this.AddImport(schema3, hashtable2);
                }
                foreach (XmlSchema schema4 in hashtable2.Keys)
                {
                    if ((references[schema4] == null) && !schemas.Contains(schema4))
                    {
                        schemas.Add(schema4);
                    }
                }
            }
            hashtable2 = new Hashtable();
            foreach (XmlSchema schema5 in this.allSchemas)
            {
                if (!this.abstractSchemas.Contains(schema5) && !this.concreteSchemas.Contains(schema5))
                {
                    this.AddImport(schema5, hashtable2);
                }
            }
            foreach (XmlSchema schema6 in hashtable2.Keys)
            {
                if (references[schema6] == null)
                {
                    if (!this.abstractSchemas.Contains(schema6))
                    {
                        this.abstractSchemas.Add(schema6);
                    }
                    if (!this.concreteSchemas.Contains(schema6))
                    {
                        this.concreteSchemas.Add(schema6);
                    }
                }
            }
            if (this.abstractSchemas.Count > 0)
            {
                foreach (XmlSchema schema7 in references.Values)
                {
                    this.abstractSchemas.AddReference(schema7);
                }
                foreach (string str in SchemaCompiler.Compile(this.abstractSchemas))
                {
                    warnings.Add(str);
                }
            }
            if (this.concreteSchemas.Count > 0)
            {
                foreach (XmlSchema schema8 in references.Values)
                {
                    this.concreteSchemas.AddReference(schema8);
                }
                foreach (string str2 in SchemaCompiler.Compile(this.concreteSchemas))
                {
                    warnings.Add(str2);
                }
            }
            if (this.ProtocolName.Length > 0)
            {
                ProtocolImporter importer = this.FindImporterByName(this.ProtocolName);
                if (importer.GenerateCode(codeNamespace, importContext, exportContext))
                {
                    return importer.Warnings;
                }
            }
            else
            {
                for (int i = 0; i < this.importers.Length; i++)
                {
                    ProtocolImporter importer2 = this.importers[i];
                    if (importer2.GenerateCode(codeNamespace, importContext, exportContext))
                    {
                        return importer2.Warnings;
                    }
                }
            }
            return ServiceDescriptionImportWarnings.NoCodeGenerated;
        }

        internal XmlSchemas AbstractSchemas
        {
            get
            {
                return this.abstractSchemas;
            }
        }

        internal XmlSchemas AllSchemas
        {
            get
            {
                return this.allSchemas;
            }
        }

        internal System.CodeDom.CodeCompileUnit CodeCompileUnit
        {
            get
            {
                return this.codeCompileUnit;
            }
        }

        [ComVisible(false)]
        public System.Xml.Serialization.CodeGenerationOptions CodeGenerationOptions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.options;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.options = value;
            }
        }

        [ComVisible(false)]
        public CodeDomProvider CodeGenerator
        {
            get
            {
                if (this.codeProvider == null)
                {
                    this.codeProvider = new CSharpCodeProvider();
                }
                return this.codeProvider;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.codeProvider = value;
            }
        }

        internal XmlSchemas ConcreteSchemas
        {
            get
            {
                return this.concreteSchemas;
            }
        }

        internal List<Type> Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new List<Type>();
                }
                return this.extensions;
            }
        }

        public string ProtocolName
        {
            get
            {
                if (this.protocolName != null)
                {
                    return this.protocolName;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.protocolName = value;
            }
        }

        public XmlSchemas Schemas
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.schemas;
            }
        }

        public ServiceDescriptionCollection ServiceDescriptions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serviceDescriptions;
            }
        }

        public ServiceDescriptionImportStyle Style
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.style;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.style = value;
            }
        }
    }
}

