namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Services.Description;
    using System.Web.Services.Discovery;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class DiscoveryServerType : ServerType
    {
        private ServiceDescription description;
        private DiscoveryDocument discoDoc;
        private LogicalMethodInfo methodInfo;
        private Hashtable schemaTable;
        private Hashtable wsdlTable;

        internal DiscoveryServerType(Type type, string uri) : base(typeof(DiscoveryServerProtocol))
        {
            this.schemaTable = new Hashtable();
            this.wsdlTable = new Hashtable();
            uri = new Uri(uri, true).GetLeftPart(UriPartial.Path);
            this.methodInfo = new LogicalMethodInfo(typeof(DiscoveryServerProtocol).GetMethod("Discover", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
            ServiceDescriptionReflector reflector = new ServiceDescriptionReflector();
            reflector.Reflect(type, uri);
            XmlSchemas schemas = reflector.Schemas;
            this.description = reflector.ServiceDescription;
            XmlSerializer serializer = ServiceDescription.Serializer;
            this.AddSchemaImports(schemas, uri, reflector.ServiceDescriptions);
            for (int i = 1; i < reflector.ServiceDescriptions.Count; i++)
            {
                ServiceDescription description = reflector.ServiceDescriptions[i];
                Import import = new Import {
                    Namespace = description.TargetNamespace
                };
                string key = "wsdl" + i.ToString(CultureInfo.InvariantCulture);
                import.Location = uri + "?wsdl=" + key;
                reflector.ServiceDescription.Imports.Add(import);
                this.wsdlTable.Add(key, description);
            }
            this.discoDoc = new DiscoveryDocument();
            this.discoDoc.References.Add(new ContractReference(uri + "?wsdl", uri));
            foreach (Service service in reflector.ServiceDescription.Services)
            {
                foreach (Port port in service.Ports)
                {
                    SoapAddressBinding binding = (SoapAddressBinding) port.Extensions.Find(typeof(SoapAddressBinding));
                    if (binding != null)
                    {
                        System.Web.Services.Discovery.SoapBinding binding2 = new System.Web.Services.Discovery.SoapBinding {
                            Binding = port.Binding,
                            Address = binding.Location
                        };
                        this.discoDoc.References.Add(binding2);
                    }
                }
            }
        }

        internal void AddExternal(XmlSchema schema, string ns, string location)
        {
            if (schema != null)
            {
                if (schema.TargetNamespace == ns)
                {
                    XmlSchemaInclude item = new XmlSchemaInclude {
                        SchemaLocation = location
                    };
                    schema.Includes.Add(item);
                }
                else
                {
                    XmlSchemaImport import = new XmlSchemaImport {
                        SchemaLocation = location,
                        Namespace = ns
                    };
                    schema.Includes.Add(import);
                }
            }
        }

        private void AddSchemaImports(XmlSchemas schemas, string uri, ServiceDescriptionCollection descriptions)
        {
            int num = 0;
            foreach (XmlSchema schema in schemas)
            {
                if (schema != null)
                {
                    if ((schema.Id == null) || (schema.Id.Length == 0))
                    {
                        schema.Id = "schema" + ++num.ToString(CultureInfo.InvariantCulture);
                    }
                    string location = uri + "?schema=" + schema.Id;
                    foreach (ServiceDescription description in descriptions)
                    {
                        if (description.Types.Schemas.Count == 0)
                        {
                            XmlSchema schema2 = new XmlSchema {
                                TargetNamespace = description.TargetNamespace
                            };
                            schema.ElementFormDefault = XmlSchemaForm.Qualified;
                            this.AddExternal(schema2, schema.TargetNamespace, location);
                            description.Types.Schemas.Add(schema2);
                        }
                        else
                        {
                            this.AddExternal(description.Types.Schemas[0], schema.TargetNamespace, location);
                        }
                    }
                    this.schemaTable.Add(schema.Id, schema);
                }
            }
        }

        internal XmlSchema GetSchema(string id)
        {
            return (XmlSchema) this.schemaTable[id];
        }

        internal ServiceDescription GetServiceDescription(string id)
        {
            return (ServiceDescription) this.wsdlTable[id];
        }

        internal ServiceDescription Description
        {
            get
            {
                return this.description;
            }
        }

        internal DiscoveryDocument Disco
        {
            get
            {
                return this.discoDoc;
            }
        }

        internal LogicalMethodInfo MethodInfo
        {
            get
            {
                return this.methodInfo;
            }
        }
    }
}

