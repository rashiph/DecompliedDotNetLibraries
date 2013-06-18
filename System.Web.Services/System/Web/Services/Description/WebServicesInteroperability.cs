namespace System.Web.Services.Description
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.Services;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public sealed class WebServicesInteroperability
    {
        private WebServicesInteroperability()
        {
        }

        private static void AddSignature(Hashtable wireSignatures, string name, string ns, string message, string messageNs, BasicProfileViolationCollection violations)
        {
            if (wireSignatures != null)
            {
                string str = ns + ":" + name;
                string element = (string) wireSignatures[str];
                string str3 = ((ns == null) && (name == null)) ? System.Web.Services.Res.GetString("WireSignatureEmpty", new object[] { message, messageNs }) : System.Web.Services.Res.GetString("WireSignature", new object[] { message, messageNs, ns, name });
                if (element != null)
                {
                    if (element.Length > 0)
                    {
                        violations.Add("R2710", element);
                        violations.Add("R2710", str3);
                        wireSignatures[str] = string.Empty;
                    }
                }
                else
                {
                    wireSignatures[str] = str3;
                }
            }
        }

        internal static bool AnalyzeBinding(Binding binding, ServiceDescription description, ServiceDescriptionCollection descriptions, BasicProfileViolationCollection violations)
        {
            bool flag = false;
            bool flag2 = false;
            SoapBinding binding2 = (SoapBinding) binding.Extensions.Find(typeof(SoapBinding));
            if ((binding2 == null) || (binding2.GetType() != typeof(SoapBinding)))
            {
                return false;
            }
            SoapBindingStyle style = (binding2.Style == SoapBindingStyle.Default) ? SoapBindingStyle.Document : binding2.Style;
            if (binding2.Transport.Length == 0)
            {
                violations.Add("R2701", System.Web.Services.Res.GetString("BindingMissingAttribute", new object[] { binding.Name, description.TargetNamespace, "transport" }));
            }
            else if (binding2.Transport != "http://schemas.xmlsoap.org/soap/http")
            {
                violations.Add("R2702", System.Web.Services.Res.GetString("BindingInvalidAttribute", new object[] { binding.Name, description.TargetNamespace, "transport", binding2.Transport }));
            }
            PortType portType = descriptions.GetPortType(binding.Type);
            Hashtable hashtable = new Hashtable();
            if (portType != null)
            {
                foreach (Operation operation in portType.Operations)
                {
                    if (operation.Messages.Flow == OperationFlow.Notification)
                    {
                        violations.Add("R2303", System.Web.Services.Res.GetString("OperationFlowNotification", new object[] { operation.Name, binding.Type.Namespace, binding.Type.Namespace }));
                    }
                    if (operation.Messages.Flow == OperationFlow.SolicitResponse)
                    {
                        violations.Add("R2303", System.Web.Services.Res.GetString("OperationFlowSolicitResponse", new object[] { operation.Name, binding.Type.Namespace, binding.Type.Namespace }));
                    }
                    if (hashtable[operation.Name] != null)
                    {
                        violations.Add("R2304", System.Web.Services.Res.GetString("Operation", new object[] { operation.Name, binding.Type.Name, binding.Type.Namespace }));
                    }
                    else
                    {
                        OperationBinding binding3 = null;
                        foreach (OperationBinding binding4 in binding.Operations)
                        {
                            if (operation.IsBoundBy(binding4))
                            {
                                if (binding3 != null)
                                {
                                    violations.Add("R2304", System.Web.Services.Res.GetString("OperationBinding", new object[] { binding3.Name, binding3.Parent.Name, description.TargetNamespace }));
                                }
                                binding3 = binding4;
                            }
                        }
                        if (binding3 == null)
                        {
                            violations.Add("R2718", System.Web.Services.Res.GetString("OperationMissingBinding", new object[] { operation.Name, binding.Type.Name, binding.Type.Namespace }));
                        }
                        else
                        {
                            hashtable.Add(operation.Name, operation);
                        }
                    }
                }
            }
            Hashtable wireSignatures = new Hashtable();
            SoapBindingStyle style2 = SoapBindingStyle.Default;
            foreach (OperationBinding binding5 in binding.Operations)
            {
                SoapBindingStyle style3 = style;
                string name = binding5.Name;
                if (name != null)
                {
                    if (hashtable[name] == null)
                    {
                        violations.Add("R2718", System.Web.Services.Res.GetString("PortTypeOperationMissing", new object[] { binding5.Name, binding.Name, description.TargetNamespace, binding.Type.Name, binding.Type.Namespace }));
                    }
                    Operation operation2 = FindOperation(portType.Operations, binding5);
                    SoapOperationBinding binding6 = (SoapOperationBinding) binding5.Extensions.Find(typeof(SoapOperationBinding));
                    if (binding6 != null)
                    {
                        if (style2 == SoapBindingStyle.Default)
                        {
                            style2 = binding6.Style;
                        }
                        flag |= style2 != binding6.Style;
                        style3 = (binding6.Style != SoapBindingStyle.Default) ? binding6.Style : style;
                    }
                    if (binding5.Input != null)
                    {
                        SoapBodyBinding binding7 = FindSoapBodyBinding(true, binding5.Input.Extensions, violations, style3 == SoapBindingStyle.Document, binding5.Name, binding.Name, description.TargetNamespace);
                        if ((binding7 != null) && (binding7.Use != SoapBindingUse.Encoded))
                        {
                            Message message = (operation2 == null) ? null : ((operation2.Messages.Input == null) ? null : descriptions.GetMessage(operation2.Messages.Input.Message));
                            if (style3 == SoapBindingStyle.Rpc)
                            {
                                CheckMessageParts(message, binding7.Parts, false, binding5.Name, binding.Name, description.TargetNamespace, wireSignatures, violations);
                            }
                            else
                            {
                                flag2 = flag2 || ((binding7.Parts != null) && (binding7.Parts.Length > 1));
                                int num = (binding7.Parts == null) ? 0 : binding7.Parts.Length;
                                CheckMessageParts(message, binding7.Parts, true, binding5.Name, binding.Name, description.TargetNamespace, wireSignatures, violations);
                                if (((num == 0) && (message != null)) && (message.Parts.Count > 1))
                                {
                                    violations.Add("R2210", System.Web.Services.Res.GetString("OperationBinding", new object[] { binding5.Name, binding.Name, description.TargetNamespace }));
                                }
                            }
                        }
                    }
                    if (binding5.Output != null)
                    {
                        SoapBodyBinding binding8 = FindSoapBodyBinding(false, binding5.Output.Extensions, violations, style3 == SoapBindingStyle.Document, binding5.Name, binding.Name, description.TargetNamespace);
                        if ((binding8 != null) && (binding8.Use != SoapBindingUse.Encoded))
                        {
                            Message message2 = (operation2 == null) ? null : ((operation2.Messages.Output == null) ? null : descriptions.GetMessage(operation2.Messages.Output.Message));
                            if (style3 == SoapBindingStyle.Rpc)
                            {
                                CheckMessageParts(message2, binding8.Parts, false, binding5.Name, binding.Name, description.TargetNamespace, null, violations);
                            }
                            else
                            {
                                flag2 = flag2 || ((binding8.Parts != null) && (binding8.Parts.Length > 1));
                                int num2 = (binding8.Parts == null) ? 0 : binding8.Parts.Length;
                                CheckMessageParts(message2, binding8.Parts, true, binding5.Name, binding.Name, description.TargetNamespace, null, violations);
                                if (((num2 == 0) && (message2 != null)) && (message2.Parts.Count > 1))
                                {
                                    violations.Add("R2210", System.Web.Services.Res.GetString("OperationBinding", new object[] { binding5.Name, binding.Name, description.TargetNamespace }));
                                }
                            }
                        }
                    }
                    foreach (FaultBinding binding9 in binding5.Faults)
                    {
                        foreach (ServiceDescriptionFormatExtension extension in binding9.Extensions)
                        {
                            if (extension is SoapFaultBinding)
                            {
                                SoapFaultBinding item = (SoapFaultBinding) extension;
                                if (item.Use == SoapBindingUse.Encoded)
                                {
                                    violations.Add("R2706", MessageString(item, binding5.Name, binding.Name, description.TargetNamespace, false, null));
                                }
                                else
                                {
                                    if ((item.Name == null) || (item.Name.Length == 0))
                                    {
                                        violations.Add("R2721", System.Web.Services.Res.GetString("FaultBinding", new object[] { binding9.Name, binding5.Name, binding.Name, description.TargetNamespace }));
                                    }
                                    else if (item.Name != binding9.Name)
                                    {
                                        violations.Add("R2754", System.Web.Services.Res.GetString("FaultBinding", new object[] { binding9.Name, binding5.Name, binding.Name, description.TargetNamespace }));
                                    }
                                    if ((item.Namespace != null) && (item.Namespace.Length > 0))
                                    {
                                        violations.Add((style3 == SoapBindingStyle.Document) ? "R2716" : "R2726", MessageString(item, binding5.Name, binding.Name, description.TargetNamespace, false, null));
                                    }
                                }
                            }
                        }
                    }
                    if (hashtable[binding5.Name] == null)
                    {
                        violations.Add("R2718", System.Web.Services.Res.GetString("PortTypeOperationMissing", new object[] { binding5.Name, binding.Name, description.TargetNamespace, binding.Type.Name, binding.Type.Namespace }));
                    }
                }
            }
            if (flag2)
            {
                violations.Add("R2201", System.Web.Services.Res.GetString("BindingMultipleParts", new object[] { binding.Name, description.TargetNamespace, "parts" }));
            }
            if (flag)
            {
                violations.Add("R2705", System.Web.Services.Res.GetString("Binding", new object[] { binding.Name, description.TargetNamespace }));
            }
            return true;
        }

        internal static void AnalyzeDescription(ServiceDescriptionCollection descriptions, BasicProfileViolationCollection violations)
        {
            bool flag = false;
            foreach (ServiceDescription description in descriptions)
            {
                SchemaCompiler.Compile(description.Types.Schemas);
                CheckWsdlImports(description, violations);
                CheckTypes(description, violations);
                foreach (string str in description.ValidationWarnings)
                {
                    violations.Add("R2028, R2029", str);
                }
                foreach (Binding binding in description.Bindings)
                {
                    flag |= AnalyzeBinding(binding, description, descriptions, violations);
                }
            }
            if (flag)
            {
                CheckExtensions(descriptions, violations);
            }
            else
            {
                violations.Add("Rxxxx");
            }
        }

        public static bool CheckConformance(WsiProfiles claims, ServiceDescription description, BasicProfileViolationCollection violations)
        {
            if (description == null)
            {
                throw new ArgumentNullException("description");
            }
            ServiceDescriptionCollection descriptions = new ServiceDescriptionCollection();
            descriptions.Add(description);
            return CheckConformance(claims, descriptions, violations);
        }

        public static bool CheckConformance(WsiProfiles claims, ServiceDescriptionCollection descriptions, BasicProfileViolationCollection violations)
        {
            if ((claims & WsiProfiles.BasicProfile1_1) == WsiProfiles.None)
            {
                return true;
            }
            if (descriptions == null)
            {
                throw new ArgumentNullException("descriptions");
            }
            if (violations == null)
            {
                throw new ArgumentNullException("violations");
            }
            int count = violations.Count;
            AnalyzeDescription(descriptions, violations);
            return (count == violations.Count);
        }

        public static bool CheckConformance(WsiProfiles claims, WebReference webReference, BasicProfileViolationCollection violations)
        {
            if ((claims & WsiProfiles.BasicProfile1_1) == WsiProfiles.None)
            {
                return true;
            }
            if (webReference == null)
            {
                return true;
            }
            if (violations == null)
            {
                throw new ArgumentNullException("violations");
            }
            XmlSchemas schemas = new XmlSchemas();
            ServiceDescriptionCollection descriptions = new ServiceDescriptionCollection();
            StringCollection warnings = new StringCollection();
            foreach (DictionaryEntry entry in webReference.Documents)
            {
                ServiceDescriptionImporter.AddDocument((string) entry.Key, entry.Value, schemas, descriptions, warnings);
            }
            int count = violations.Count;
            AnalyzeDescription(descriptions, violations);
            return (count == violations.Count);
        }

        private static bool CheckExtensions(ServiceDescriptionFormatExtensionCollection extensions)
        {
            foreach (ServiceDescriptionFormatExtension extension in extensions)
            {
                if (extension.Required)
                {
                    return false;
                }
            }
            return true;
        }

        private static void CheckExtensions(ServiceDescriptionCollection descriptions, BasicProfileViolationCollection violations)
        {
            Hashtable hashtable = new Hashtable();
            foreach (ServiceDescription description in descriptions)
            {
                if ((ServiceDescription.GetConformanceClaims(description.Types.DocumentationElement) == WsiProfiles.BasicProfile1_1) && !CheckExtensions(description.Extensions))
                {
                    violations.Add("R2026", System.Web.Services.Res.GetString("Element", new object[] { "wsdl:types", description.TargetNamespace }));
                }
                foreach (Service service in description.Services)
                {
                    foreach (Port port in service.Ports)
                    {
                        if (ServiceDescription.GetConformanceClaims(port.DocumentationElement) == WsiProfiles.BasicProfile1_1)
                        {
                            if (!CheckExtensions(port.Extensions))
                            {
                                violations.Add("R2026", System.Web.Services.Res.GetString("Port", new object[] { port.Name, service.Name, description.TargetNamespace }));
                            }
                            Binding binding = descriptions.GetBinding(port.Binding);
                            if (hashtable[binding] != null)
                            {
                                CheckExtensions(binding, description, violations);
                                hashtable.Add(binding, binding);
                            }
                        }
                    }
                }
                foreach (Binding binding2 in description.Bindings)
                {
                    SoapBinding binding3 = (SoapBinding) binding2.Extensions.Find(typeof(SoapBinding));
                    if (((binding3 != null) && !(binding3.GetType() != typeof(SoapBinding))) && ((hashtable[binding2] == null) && (ServiceDescription.GetConformanceClaims(binding2.DocumentationElement) == WsiProfiles.BasicProfile1_1)))
                    {
                        CheckExtensions(binding2, description, violations);
                        hashtable.Add(binding2, binding2);
                    }
                }
            }
        }

        private static void CheckExtensions(Binding binding, ServiceDescription description, BasicProfileViolationCollection violations)
        {
            SoapBinding binding2 = (SoapBinding) binding.Extensions.Find(typeof(SoapBinding));
            if (((binding2 != null) && (binding2.GetType() == typeof(SoapBinding))) && !CheckExtensions(binding.Extensions))
            {
                violations.Add("R2026", System.Web.Services.Res.GetString("BindingInvalidAttribute", new object[] { binding.Name, description.TargetNamespace, "wsdl:required", "true" }));
            }
        }

        private static void CheckMessagePart(MessagePart part, bool element, string message, string operation, string binding, string ns, Hashtable wireSignatures, BasicProfileViolationCollection violations)
        {
            if (part == null)
            {
                if (!element)
                {
                    AddSignature(wireSignatures, operation, ns, message, ns, violations);
                }
                else
                {
                    AddSignature(wireSignatures, null, null, message, ns, violations);
                }
            }
            else
            {
                if (((part.Type != null) && !part.Type.IsEmpty) && ((part.Element != null) && !part.Element.IsEmpty))
                {
                    violations.Add("R2306", System.Web.Services.Res.GetString("Part", new object[] { part.Name, message, ns }));
                }
                else
                {
                    XmlQualifiedName name = ((part.Type == null) || part.Type.IsEmpty) ? part.Element : part.Type;
                    if ((name.Namespace == null) || (name.Namespace.Length == 0))
                    {
                        violations.Add("R1014", System.Web.Services.Res.GetString("Part", new object[] { part.Name, message, ns }));
                    }
                }
                if (!element && ((part.Type == null) || part.Type.IsEmpty))
                {
                    violations.Add("R2203", System.Web.Services.Res.GetString("Part", new object[] { part.Name, message, ns }));
                }
                if (element && ((part.Element == null) || part.Element.IsEmpty))
                {
                    violations.Add("R2204", System.Web.Services.Res.GetString("Part", new object[] { part.Name, message, ns }));
                }
                if (!element)
                {
                    AddSignature(wireSignatures, operation, ns, message, ns, violations);
                }
                else if (part.Element != null)
                {
                    AddSignature(wireSignatures, part.Element.Name, part.Element.Namespace, message, ns, violations);
                }
            }
        }

        private static void CheckMessageParts(Message message, string[] parts, bool element, string operation, string binding, string ns, Hashtable wireSignatures, BasicProfileViolationCollection violations)
        {
            if (message != null)
            {
                if ((message.Parts == null) || (message.Parts.Count == 0))
                {
                    if (!element)
                    {
                        AddSignature(wireSignatures, operation, ns, message.Name, ns, violations);
                    }
                    else
                    {
                        AddSignature(wireSignatures, null, null, message.Name, ns, violations);
                    }
                }
                else if ((parts == null) || (parts.Length == 0))
                {
                    for (int i = 0; i < message.Parts.Count; i++)
                    {
                        CheckMessagePart(message.Parts[i], element, message.Name, operation, binding, ns, (i == 0) ? wireSignatures : null, violations);
                    }
                }
                else
                {
                    for (int j = 0; j < parts.Length; j++)
                    {
                        if (parts[j] != null)
                        {
                            MessagePart part1 = message.Parts[parts[j]];
                            CheckMessagePart(message.Parts[j], element, message.Name, operation, binding, ns, (j == 0) ? wireSignatures : null, violations);
                        }
                    }
                }
            }
        }

        private static void CheckTypes(ServiceDescription description, BasicProfileViolationCollection violations)
        {
            foreach (XmlSchema schema in description.Types.Schemas)
            {
                if ((schema.TargetNamespace == null) || (schema.TargetNamespace.Length == 0))
                {
                    using (XmlSchemaObjectEnumerator enumerator2 = schema.Items.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            if (!(enumerator2.Current is XmlSchemaAnnotation))
                            {
                                violations.Add("R2105", System.Web.Services.Res.GetString("Element", new object[] { "schema", description.TargetNamespace }));
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static void CheckWsdlImports(ServiceDescription description, BasicProfileViolationCollection violations)
        {
            foreach (Import import in description.Imports)
            {
                Uri uri;
                if ((import.Location == null) || (import.Location.Length == 0))
                {
                    violations.Add("R2007", System.Web.Services.Res.GetString("Description", new object[] { description.TargetNamespace }));
                }
                string uriString = import.Namespace;
                if ((uriString.Length != 0) && !Uri.TryCreate(uriString, UriKind.Absolute, out uri))
                {
                    violations.Add("R2803", System.Web.Services.Res.GetString("Description", new object[] { description.TargetNamespace }));
                }
            }
        }

        private static Operation FindOperation(OperationCollection operations, OperationBinding bindingOperation)
        {
            foreach (Operation operation in operations)
            {
                if (operation.IsBoundBy(bindingOperation))
                {
                    return operation;
                }
            }
            return null;
        }

        private static SoapBodyBinding FindSoapBodyBinding(bool input, ServiceDescriptionFormatExtensionCollection extensions, BasicProfileViolationCollection violations, bool documentBinding, string operationName, string bindingName, string bindingNs)
        {
            SoapBodyBinding binding = null;
            for (int i = 0; i < extensions.Count; i++)
            {
                object item = extensions[i];
                string uriString = null;
                bool flag = false;
                bool flag2 = false;
                if (item is SoapBodyBinding)
                {
                    flag = true;
                    binding = (SoapBodyBinding) item;
                    uriString = binding.Namespace;
                    flag2 = binding.Use == SoapBindingUse.Encoded;
                }
                else if (item is SoapHeaderBinding)
                {
                    flag = true;
                    SoapHeaderBinding binding2 = (SoapHeaderBinding) item;
                    uriString = binding2.Namespace;
                    flag2 = binding2.Use == SoapBindingUse.Encoded;
                    if (!flag2 && ((binding2.Part == null) || (binding2.Part.Length == 0)))
                    {
                        violations.Add("R2720", MessageString(binding2, operationName, bindingName, bindingNs, input, null));
                    }
                    if (binding2.Fault != null)
                    {
                        flag2 |= binding2.Fault.Use == SoapBindingUse.Encoded;
                        if (!flag2)
                        {
                            if ((binding2.Fault.Part == null) || (binding2.Fault.Part.Length == 0))
                            {
                                violations.Add("R2720", MessageString(binding2.Fault, operationName, bindingName, bindingNs, input, null));
                            }
                            if ((binding2.Fault.Namespace != null) && (binding2.Fault.Namespace.Length > 0))
                            {
                                violations.Add(documentBinding ? "R2716" : "R2726", MessageString(item, operationName, bindingName, bindingNs, input, null));
                            }
                        }
                    }
                }
                if (flag2)
                {
                    violations.Add("R2706", MessageString(item, operationName, bindingName, bindingNs, input, null));
                }
                else if (flag)
                {
                    if ((uriString == null) || (uriString.Length == 0))
                    {
                        if (!documentBinding && (item is SoapBodyBinding))
                        {
                            violations.Add("R2717", MessageString(item, operationName, bindingName, bindingNs, input, null));
                        }
                    }
                    else if (documentBinding || !(item is SoapBodyBinding))
                    {
                        violations.Add(documentBinding ? "R2716" : "R2726", MessageString(item, operationName, bindingName, bindingNs, input, null));
                    }
                    else
                    {
                        Uri uri;
                        if (!Uri.TryCreate(uriString, UriKind.Absolute, out uri))
                        {
                            violations.Add("R2717", MessageString(item, operationName, bindingName, bindingNs, input, System.Web.Services.Res.GetString("UriValueRelative", new object[] { uriString })));
                        }
                    }
                }
            }
            return binding;
        }

        private static string MessageString(object item, string operation, string binding, string ns, bool input, string details)
        {
            string name = null;
            string str2 = null;
            if (item is SoapBodyBinding)
            {
                name = input ? "InputElement" : "OutputElement";
                str2 = "soapbind:body";
            }
            else if (item is SoapHeaderBinding)
            {
                name = input ? "InputElement" : "OutputElement";
                str2 = "soapbind:header";
            }
            else if (item is SoapFaultBinding)
            {
                name = "Fault";
                str2 = ((SoapFaultBinding) item).Name;
            }
            else if (item is SoapHeaderFaultBinding)
            {
                name = "HeaderFault";
                str2 = "soapbind:headerfault";
            }
            if (name == null)
            {
                return null;
            }
            return System.Web.Services.Res.GetString(name, new object[] { str2, operation, binding, ns, details });
        }
    }
}

