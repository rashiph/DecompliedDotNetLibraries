namespace System.Web.Services.Description
{
    using System;
    using System.Reflection;
    using System.Web.Services.Configuration;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Schema;

    internal abstract class HttpProtocolReflector : ProtocolReflector
    {
        private MimeReflector[] reflectors;

        protected HttpProtocolReflector()
        {
            Type[] mimeReflectorTypes = WebServicesSection.Current.MimeReflectorTypes;
            this.reflectors = new MimeReflector[mimeReflectorTypes.Length];
            for (int i = 0; i < this.reflectors.Length; i++)
            {
                MimeReflector reflector = (MimeReflector) Activator.CreateInstance(mimeReflectorTypes[i]);
                reflector.ReflectionContext = this;
                this.reflectors[i] = reflector;
            }
        }

        protected bool ReflectMimeParameters()
        {
            bool flag = false;
            for (int i = 0; i < this.reflectors.Length; i++)
            {
                if (this.reflectors[i].ReflectParameters())
                {
                    flag = true;
                }
            }
            return flag;
        }

        protected bool ReflectMimeReturn()
        {
            if (base.Method.ReturnType == typeof(void))
            {
                Message outputMessage = base.OutputMessage;
                return true;
            }
            for (int i = 0; i < this.reflectors.Length; i++)
            {
                if (this.reflectors[i].ReflectReturn())
                {
                    return true;
                }
            }
            return false;
        }

        internal void ReflectStringParametersMessage()
        {
            Message inputMessage = base.InputMessage;
            foreach (ParameterInfo info in base.Method.InParameters)
            {
                MessagePart messagePart = new MessagePart {
                    Name = XmlConvert.EncodeLocalName(info.Name)
                };
                if (info.ParameterType.IsArray)
                {
                    string defaultNamespace = base.DefaultNamespace;
                    if (defaultNamespace.EndsWith("/", StringComparison.Ordinal))
                    {
                        defaultNamespace = defaultNamespace + "AbstractTypes";
                    }
                    else
                    {
                        defaultNamespace = defaultNamespace + "/AbstractTypes";
                    }
                    string name = "StringArray";
                    if (!base.ServiceDescription.Types.Schemas.Contains(defaultNamespace))
                    {
                        XmlSchema schema = new XmlSchema {
                            TargetNamespace = defaultNamespace
                        };
                        base.ServiceDescription.Types.Schemas.Add(schema);
                        XmlSchemaElement item = new XmlSchemaElement {
                            Name = "String",
                            SchemaTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema"),
                            MinOccurs = 0M,
                            MaxOccurs = 79228162514264337593543950335M
                        };
                        XmlSchemaSequence sequence = new XmlSchemaSequence();
                        sequence.Items.Add(item);
                        XmlSchemaComplexContentRestriction restriction = new XmlSchemaComplexContentRestriction {
                            BaseTypeName = new XmlQualifiedName("Array", "http://schemas.xmlsoap.org/soap/encoding/"),
                            Particle = sequence
                        };
                        XmlSchemaImport import = new XmlSchemaImport {
                            Namespace = restriction.BaseTypeName.Namespace
                        };
                        XmlSchemaComplexContent content = new XmlSchemaComplexContent {
                            Content = restriction
                        };
                        XmlSchemaComplexType type = new XmlSchemaComplexType {
                            Name = name,
                            ContentModel = content
                        };
                        schema.Items.Add(type);
                        schema.Includes.Add(import);
                    }
                    messagePart.Type = new XmlQualifiedName(name, defaultNamespace);
                }
                else
                {
                    messagePart.Type = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
                }
                inputMessage.Parts.Add(messagePart);
            }
        }

        protected bool ReflectUrlParameters()
        {
            if (!HttpServerProtocol.AreUrlParametersSupported(base.Method))
            {
                return false;
            }
            this.ReflectStringParametersMessage();
            base.OperationBinding.Input.Extensions.Add(new HttpUrlEncodedBinding());
            return true;
        }

        internal string MethodUrl
        {
            get
            {
                string messageName = base.Method.MethodAttribute.MessageName;
                if (messageName.Length == 0)
                {
                    messageName = base.Method.Name;
                }
                return ("/" + messageName);
            }
        }
    }
}

