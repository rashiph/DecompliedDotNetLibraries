namespace System.Runtime.Remoting.MetadataServices
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Serialization;
    using System.Text;

    internal class WsdlGenerator
    {
        private Assembly _dynamicAssembly;
        private string _name;
        private ArrayList _namespaces;
        internal Queue _queue;
        private string _serviceEndpoint;
        private string _targetNS;
        private string _targetNSPrefix;
        private TextWriter _textWriter;
        internal Hashtable _typeToInteropNS;
        internal Hashtable _typeToServiceEndpoint;
        private XsdVersion _xsdVersion;
        private static SchemaBlockType blockDefault = SchemaBlockType.SEQUENCE;
        private static Type s_charType = typeof(char);
        private static Type s_contextBoundType = typeof(ContextBoundObject);
        private static Type s_delegateType = typeof(Delegate);
        private static Type s_marshalByRefType = typeof(MarshalByRefObject);
        private static Type s_objectType = typeof(object);
        private static Type s_remotingClientProxyType = typeof(RemotingClientProxy);
        private static Type s_valueType = typeof(ValueType);
        private static Type s_voidType = typeof(void);

        internal WsdlGenerator(Type[] types, TextWriter output)
        {
            this._typeToInteropNS = new Hashtable();
            this._textWriter = output;
            this._queue = new Queue();
            this._name = null;
            this._namespaces = new ArrayList();
            this._dynamicAssembly = null;
            this._serviceEndpoint = null;
            for (int i = 0; i < types.Length; i++)
            {
                if ((types[i] != null) && (types[i].BaseType != null))
                {
                    this.ProcessTypeAttributes(types[i]);
                    this._queue.Enqueue(types[i]);
                }
            }
        }

        internal WsdlGenerator(ServiceType[] serviceTypes, SdlType sdlType, TextWriter output)
        {
            this._typeToInteropNS = new Hashtable();
            this._textWriter = output;
            this._queue = new Queue();
            this._name = null;
            this._namespaces = new ArrayList();
            this._dynamicAssembly = null;
            this._serviceEndpoint = null;
            for (int i = 0; i < serviceTypes.Length; i++)
            {
                if ((serviceTypes[i] != null) && (serviceTypes[i].ObjectType.BaseType != null))
                {
                    this.ProcessTypeAttributes(serviceTypes[i].ObjectType);
                    this._queue.Enqueue(serviceTypes[i].ObjectType);
                }
                if (serviceTypes[i].Url != null)
                {
                    if (this._typeToServiceEndpoint == null)
                    {
                        this._typeToServiceEndpoint = new Hashtable(10);
                    }
                    if (this._typeToServiceEndpoint.ContainsKey(serviceTypes[i].ObjectType.Name))
                    {
                        ((ArrayList) this._typeToServiceEndpoint[serviceTypes[i].ObjectType.Name]).Add(serviceTypes[i].Url);
                    }
                    else
                    {
                        ArrayList list2 = new ArrayList(10);
                        list2.Add(serviceTypes[i].Url);
                        this._typeToServiceEndpoint[serviceTypes[i].ObjectType.Name] = list2;
                    }
                }
            }
        }

        internal WsdlGenerator(Type[] types, SdlType sdlType, TextWriter output)
        {
            this._typeToInteropNS = new Hashtable();
            this._textWriter = output;
            this._queue = new Queue();
            this._name = null;
            this._namespaces = new ArrayList();
            this._dynamicAssembly = null;
            this._serviceEndpoint = null;
            for (int i = 0; i < types.Length; i++)
            {
                if ((types[i] != null) && (types[i].BaseType != null))
                {
                    this.ProcessTypeAttributes(types[i]);
                    this._queue.Enqueue(types[i]);
                }
            }
        }

        internal WsdlGenerator(Type[] types, TextWriter output, Assembly assembly, string url) : this(types, output)
        {
            this._dynamicAssembly = assembly;
            this._serviceEndpoint = url;
        }

        internal WsdlGenerator(Type[] types, SdlType sdlType, TextWriter output, Assembly assembly, string url) : this(types, output)
        {
            this._dynamicAssembly = assembly;
            this._serviceEndpoint = url;
        }

        private XMLNamespace AddNamespace(string name, Assembly assem)
        {
            return this.AddNamespace(name, assem, false);
        }

        private XMLNamespace AddNamespace(string name, Assembly assem, bool bInteropType)
        {
            XMLNamespace namespace2 = new XMLNamespace(name, assem, this._serviceEndpoint, this._typeToServiceEndpoint, "ns" + this._namespaces.Count, bInteropType, this);
            this._namespaces.Add(namespace2);
            return namespace2;
        }

        private void AddType(Type type, XMLNamespace xns)
        {
            Type elementType = type.GetElementType();
            Type type3 = elementType;
            while (type3 != null)
            {
                type3 = elementType.GetElementType();
                if (type3 != null)
                {
                    elementType = type3;
                }
            }
            if (elementType != null)
            {
                this.EnqueueType(elementType, xns);
            }
            if (!type.IsArray && !type.IsByRef)
            {
                this.EnqueueType(type, xns);
            }
            if (!type.IsPublic && !type.IsNotPublic)
            {
                string fullName = type.FullName;
                int index = fullName.IndexOf("+");
                if (index > 0)
                {
                    string name = fullName.Substring(0, index);
                    Type type4 = type.Module.Assembly.GetType(name, true);
                    bool flag1 = type4 == null;
                    this.EnqueueType(type4, xns);
                }
            }
        }

        private void EnqueueReachableTypes(RealSchemaType rsType)
        {
            XMLNamespace xNS = rsType.XNS;
            if ((rsType.Type.BaseType != null) && ((rsType.Type.BaseType != s_valueType) || (rsType.Type.BaseType != s_objectType)))
            {
                this.AddType(rsType.Type.BaseType, this.GetNamespace(rsType.Type.BaseType));
            }
            if ((rsType.Type.IsInterface || s_marshalByRefType.IsAssignableFrom(rsType.Type)) || s_delegateType.IsAssignableFrom(rsType.Type))
            {
                FieldInfo[] instanceFields = rsType.GetInstanceFields();
                for (int i = 0; i < instanceFields.Length; i++)
                {
                    if (instanceFields[i].FieldType != null)
                    {
                        this.AddType(instanceFields[i].FieldType, xNS);
                    }
                }
                Type[] introducedInterfaces = rsType.GetIntroducedInterfaces();
                if (introducedInterfaces.Length > 0)
                {
                    for (int j = 0; j < introducedInterfaces.Length; j++)
                    {
                        this.AddType(introducedInterfaces[j], xNS);
                    }
                }
                this.ProcessMethods(rsType);
            }
            else
            {
                FieldInfo[] infoArray2 = rsType.GetInstanceFields();
                for (int k = 0; k < infoArray2.Length; k++)
                {
                    if (infoArray2[k].FieldType != null)
                    {
                        this.AddType(infoArray2[k].FieldType, xNS);
                    }
                }
            }
        }

        private void EnqueueType(Type type, XMLNamespace xns)
        {
            if (!type.IsPrimitive || (type == s_charType))
            {
                string str;
                Assembly assembly;
                XMLNamespace namespace2 = null;
                bool bInteropType = GetNSAndAssembly(type, out str, out assembly);
                namespace2 = this.LookupNamespace(str, assembly);
                if (namespace2 == null)
                {
                    namespace2 = this.AddNamespace(str, assembly, bInteropType);
                }
                string str2 = SudsConverter.MapClrTypeToXsdType(type);
                if ((type.IsInterface || (str2 != null)) || (type == s_voidType))
                {
                    xns.DependsOnSchemaNS(namespace2, false);
                }
                else
                {
                    xns.DependsOnSchemaNS(namespace2, true);
                }
                if (!type.FullName.StartsWith("System."))
                {
                    this._queue.Enqueue(type);
                }
            }
        }

        internal void Generate()
        {
            while (this._queue.Count > 0)
            {
                Type type = (Type) this._queue.Dequeue();
                this.ProcessType(type);
            }
            this.Resolve();
            this.PrintWsdl();
            this._textWriter.Flush();
        }

        private XMLNamespace GetNamespace(Type type)
        {
            string ns = null;
            Assembly assem = null;
            bool bInteropType = GetNSAndAssembly(type, out ns, out assem);
            XMLNamespace namespace2 = this.LookupNamespace(ns, assem);
            if (namespace2 == null)
            {
                namespace2 = this.AddNamespace(ns, assem, bInteropType);
            }
            return namespace2;
        }

        private static bool GetNSAndAssembly(Type type, out string ns, out Assembly assem)
        {
            string xmlNamespace = null;
            string xmlElement = null;
            SoapServices.GetXmlElementForInteropType(type, out xmlElement, out xmlNamespace);
            if (xmlNamespace != null)
            {
                ns = xmlNamespace;
                assem = type.Module.Assembly;
                return true;
            }
            ns = type.Namespace;
            assem = type.Module.Assembly;
            return false;
        }

        internal static string IndentP(string indentStr)
        {
            return (indentStr + "    ");
        }

        private XMLNamespace LookupNamespace(string name, Assembly assem)
        {
            for (int i = 0; i < this._namespaces.Count; i++)
            {
                XMLNamespace namespace2 = (XMLNamespace) this._namespaces[i];
                if (name == namespace2.Name)
                {
                    return namespace2;
                }
            }
            return null;
        }

        internal void PrintServiceWsdl(TextWriter textWriter, StringBuilder sb, string indent, ArrayList refNames)
        {
            string indentStr = IndentP(indent);
            string str2 = IndentP(indentStr);
            IndentP(str2);
            sb.Length = 0;
            sb.Append("\n");
            sb.Append(indent);
            sb.Append("<service name='");
            sb.Append(this._name);
            sb.Append("Service'");
            sb.Append(">");
            textWriter.WriteLine(sb);
            for (int i = 0; i < refNames.Count; i++)
            {
                if (((this._typeToServiceEndpoint != null) && this._typeToServiceEndpoint.ContainsKey(refNames[i])) || (this._serviceEndpoint != null))
                {
                    sb.Length = 0;
                    sb.Append(indentStr);
                    sb.Append("<port name='");
                    sb.Append(refNames[i]);
                    sb.Append("Port'");
                    sb.Append(" ");
                    sb.Append("binding='tns:");
                    sb.Append(refNames[i]);
                    sb.Append("Binding");
                    sb.Append("'>");
                    textWriter.WriteLine(sb);
                    if ((this._typeToServiceEndpoint != null) && this._typeToServiceEndpoint.ContainsKey(refNames[i]))
                    {
                        foreach (string str3 in (ArrayList) this._typeToServiceEndpoint[refNames[i]])
                        {
                            sb.Length = 0;
                            sb.Append(str2);
                            sb.Append("<soap:address location='");
                            sb.Append(this.UrlEncode(str3));
                            sb.Append("'/>");
                            textWriter.WriteLine(sb);
                        }
                    }
                    else if (this._serviceEndpoint != null)
                    {
                        sb.Length = 0;
                        sb.Append(str2);
                        sb.Append("<soap:address location='");
                        sb.Append(this._serviceEndpoint);
                        sb.Append("'/>");
                        textWriter.WriteLine(sb);
                    }
                    sb.Length = 0;
                    sb.Append(indentStr);
                    sb.Append("</port>");
                    textWriter.WriteLine(sb);
                }
            }
            sb.Length = 0;
            sb.Append(indent);
            sb.Append("</service>");
            textWriter.WriteLine(sb);
        }

        private void PrintTypesBeginWsdl(TextWriter textWriter, StringBuilder sb, string indent)
        {
            sb.Length = 0;
            sb.Append(indent);
            sb.Append("<types>");
            textWriter.WriteLine(sb);
        }

        private void PrintTypesEndWsdl(TextWriter textWriter, StringBuilder sb, string indent)
        {
            sb.Length = 0;
            sb.Append(indent);
            sb.Append("</types>");
            textWriter.WriteLine(sb);
        }

        private void PrintWsdl()
        {
            if ((this._targetNS == null) || (this._targetNS.Length == 0))
            {
                if (this._namespaces.Count > 0)
                {
                    this._targetNS = ((XMLNamespace) this._namespaces[0]).Namespace;
                }
                else
                {
                    this._targetNS = "http://schemas.xmlsoap.org/wsdl/";
                }
            }
            string indentStr = "";
            string str2 = IndentP(indentStr);
            string str3 = IndentP(str2);
            string str4 = IndentP(str3);
            IndentP(str4);
            StringBuilder builder = new StringBuilder(0x100);
            this._textWriter.WriteLine("<?xml version='1.0' encoding='UTF-8'?>");
            builder.Length = 0;
            builder.Append("<definitions ");
            if (this._name != null)
            {
                builder.Append("name='");
                builder.Append(this._name);
                builder.Append("' ");
            }
            builder.Append("targetNamespace='");
            builder.Append(this._targetNS);
            builder.Append("'");
            this._textWriter.WriteLine(builder);
            this.PrintWsdlNamespaces(this._textWriter, builder, str4);
            bool flag = false;
            for (int i = 0; i < this._namespaces.Count; i++)
            {
                if (((XMLNamespace) this._namespaces[i]).CheckForSchemaContent())
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                this.PrintTypesBeginWsdl(this._textWriter, builder, str2);
                for (int k = 0; k < this._namespaces.Count; k++)
                {
                    if (((XMLNamespace) this._namespaces[k]).CheckForSchemaContent())
                    {
                        ((XMLNamespace) this._namespaces[k]).PrintSchemaWsdl(this._textWriter, builder, str3);
                    }
                }
                this.PrintTypesEndWsdl(this._textWriter, builder, str2);
            }
            ArrayList refNames = new ArrayList(0x19);
            for (int j = 0; j < this._namespaces.Count; j++)
            {
                ((XMLNamespace) this._namespaces[j]).PrintMessageWsdl(this._textWriter, builder, str2, refNames);
            }
            this.PrintServiceWsdl(this._textWriter, builder, str2, refNames);
            this._textWriter.WriteLine("</definitions>");
        }

        private void PrintWsdlNamespaces(TextWriter textWriter, StringBuilder sb, string indent)
        {
            sb.Length = 0;
            sb.Append(indent);
            sb.Append("xmlns='http://schemas.xmlsoap.org/wsdl/'");
            textWriter.WriteLine(sb);
            sb.Length = 0;
            sb.Append(indent);
            sb.Append("xmlns:tns='");
            sb.Append(this._targetNS);
            sb.Append("'");
            textWriter.WriteLine(sb);
            sb.Length = 0;
            sb.Append(indent);
            sb.Append("xmlns:xsd='");
            sb.Append(SudsConverter.GetXsdVersion(this._xsdVersion));
            sb.Append("'");
            textWriter.WriteLine(sb);
            sb.Length = 0;
            sb.Append(indent);
            sb.Append("xmlns:xsi='");
            sb.Append(SudsConverter.GetXsiVersion(this._xsdVersion));
            sb.Append("'");
            textWriter.WriteLine(sb);
            sb.Length = 0;
            sb.Append(indent);
            sb.Append("xmlns:suds='http://www.w3.org/2000/wsdl/suds'");
            textWriter.WriteLine(sb);
            sb.Length = 0;
            sb.Append(indent);
            sb.Append("xmlns:wsdl='http://schemas.xmlsoap.org/wsdl/'");
            textWriter.WriteLine(sb);
            sb.Length = 0;
            sb.Append(indent);
            sb.Append("xmlns:soapenc='http://schemas.xmlsoap.org/soap/encoding/'");
            textWriter.WriteLine(sb);
            Hashtable usedNames = new Hashtable(10);
            for (int i = 0; i < this._namespaces.Count; i++)
            {
                ((XMLNamespace) this._namespaces[i]).PrintDependsOnWsdl(this._textWriter, sb, indent, usedNames);
            }
            sb.Length = 0;
            sb.Append(indent);
            sb.Append("xmlns:soap='http://schemas.xmlsoap.org/wsdl/soap/'>");
            textWriter.WriteLine(sb);
        }

        private void ProcessMethods(RealSchemaType rsType)
        {
            XMLNamespace xNS = rsType.XNS;
            MethodInfo[] introducedMethods = rsType.GetIntroducedMethods();
            if (introducedMethods.Length > 0)
            {
                string name = null;
                XMLNamespace xns = null;
                if (xNS.IsInteropType)
                {
                    name = xNS.Name;
                    xns = xNS;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    QualifyName(sb, xNS.Name, rsType.Name);
                    name = sb.ToString();
                    xns = this.AddNamespace(name, xNS.Assem);
                    xNS.DependsOnSchemaNS(xns, false);
                }
                for (int i = 0; i < introducedMethods.Length; i++)
                {
                    MethodInfo info = introducedMethods[i];
                    this.AddType(info.ReturnType, xns);
                    ParameterInfo[] parameters = info.GetParameters();
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        this.AddType(parameters[j].ParameterType, xns);
                    }
                }
            }
        }

        internal void ProcessType(Type type)
        {
            string str;
            Assembly assembly;
            bool bInteropType = GetNSAndAssembly(type, out str, out assembly);
            XMLNamespace xns = this.LookupNamespace(str, assembly);
            if (xns != null)
            {
                string name = RefName(type);
                if (xns.LookupSchemaType(name) != null)
                {
                    return;
                }
            }
            else
            {
                xns = this.AddNamespace(str, assembly, bInteropType);
            }
            this._typeToInteropNS[type] = xns;
            if (!type.IsArray)
            {
                SimpleSchemaType ssType = SimpleSchemaType.GetSimpleSchemaType(type, xns, false);
                if (ssType != null)
                {
                    xns.AddSimpleSchemaType(ssType);
                }
                else
                {
                    bool bUnique = false;
                    string serviceEndpoint = null;
                    Hashtable typeToServiceEndpoint = null;
                    if ((this._name == null) && s_marshalByRefType.IsAssignableFrom(type))
                    {
                        this._name = type.Name;
                        this._targetNS = xns.Namespace;
                        this._targetNSPrefix = xns.Prefix;
                        serviceEndpoint = this._serviceEndpoint;
                        typeToServiceEndpoint = this._typeToServiceEndpoint;
                        bUnique = true;
                    }
                    RealSchemaType rsType = new RealSchemaType(type, xns, serviceEndpoint, typeToServiceEndpoint, bUnique, this);
                    xns.AddRealSchemaType(rsType);
                    this.EnqueueReachableTypes(rsType);
                }
            }
        }

        internal void ProcessTypeAttributes(Type type)
        {
            SoapTypeAttribute cachedSoapAttribute = InternalRemotingServices.GetCachedSoapAttribute(type) as SoapTypeAttribute;
            if (cachedSoapAttribute != null)
            {
                SoapOption option;
                if ((option = cachedSoapAttribute.SoapOptions & SoapOption.Option1) == SoapOption.Option1)
                {
                    this._xsdVersion = XsdVersion.V1999;
                }
                else if ((option &= SoapOption.Option2) == SoapOption.Option2)
                {
                    this._xsdVersion = XsdVersion.V2000;
                }
                else
                {
                    this._xsdVersion = XsdVersion.V2001;
                }
            }
        }

        internal static void QualifyName(StringBuilder sb, string ns, string name)
        {
            if ((ns != null) && (ns.Length != 0))
            {
                sb.Append(ns);
                sb.Append('.');
            }
            sb.Append(name);
        }

        internal static string RefName(Type type)
        {
            string name = type.Name;
            if (type.IsPublic || type.IsNotPublic)
            {
                return name;
            }
            name = type.FullName;
            int num = name.LastIndexOf('.');
            if (num > 0)
            {
                name = name.Substring(num + 1);
            }
            return name.Replace('+', '.');
        }

        private void Resolve()
        {
            for (int i = 0; i < this._namespaces.Count; i++)
            {
                ((XMLNamespace) this._namespaces[i]).Resolve();
            }
        }

        private string UrlEncode(string url)
        {
            if (((url != null) && (url.Length != 0)) && ((url.IndexOf("&amp;") <= -1) && (url.IndexOf('&') > -1)))
            {
                return url.Replace("&", "&amp;");
            }
            return url;
        }

        private class ArraySchemaType : WsdlGenerator.ComplexSchemaType
        {
            private System.Type _type;

            internal ArraySchemaType(System.Type type, string name, SchemaBlockType blockType, bool bSealed) : base(name, blockType, bSealed)
            {
                this._type = type;
            }

            internal override void PrintSchemaType(TextWriter textWriter, StringBuilder sb, string indent, bool bAnonymous)
            {
                string str = WsdlGenerator.IndentP(indent);
                sb.Length = 0;
                sb.Append(indent);
                sb.Append("<complexType name='");
                sb.Append(base.FullRefName);
                sb.Append("'>");
                textWriter.WriteLine(sb);
                base.PrintBody(textWriter, sb, str);
                sb.Length = 0;
                sb.Append(indent);
                sb.Append("</complexType>");
                textWriter.WriteLine(sb);
            }

            internal System.Type Type
            {
                get
                {
                    return this._type;
                }
            }
        }

        private abstract class ComplexSchemaType : WsdlGenerator.SchemaType
        {
            private ArrayList _abstractElms;
            private string _baseName;
            private SchemaBlockType _blockType;
            private bool _bSealed;
            private string _elementName;
            private string _fullRefName;
            private string _name;
            private ArrayList _particles;
            private Type _type;
            private static string[] schemaBlockBegin = new string[] { "<all>", "<sequence>", "<choice>", "<complexContent>" };
            private static string[] schemaBlockEnd = new string[] { "</all>", "</sequence>", "</choice>", "</complexContent>" };

            internal ComplexSchemaType(Type type)
            {
                this._blockType = SchemaBlockType.ALL;
                this._type = type;
                this.Init();
            }

            internal ComplexSchemaType(string name, bool bSealed)
            {
                this._name = name;
                this._fullRefName = this._name;
                this._blockType = SchemaBlockType.ALL;
                this._baseName = null;
                this._elementName = name;
                this._bSealed = bSealed;
                this._particles = new ArrayList();
                this._abstractElms = new ArrayList();
            }

            internal ComplexSchemaType(string name, SchemaBlockType blockType, bool bSealed)
            {
                this._name = name;
                this._fullRefName = this._name;
                this._blockType = blockType;
                this._baseName = null;
                this._elementName = name;
                this._bSealed = bSealed;
                this._particles = new ArrayList();
                this._abstractElms = new ArrayList();
            }

            internal void AddParticle(WsdlGenerator.Particle particle)
            {
                this._particles.Add(particle);
            }

            private void Init()
            {
                this._name = this._type.Name;
                this._bSealed = this._type.IsSealed;
                this._baseName = null;
                this._elementName = this._name;
                this._particles = new ArrayList();
                this._abstractElms = new ArrayList();
                this._fullRefName = WsdlGenerator.RefName(this._type);
            }

            protected void PrintBody(TextWriter textWriter, StringBuilder sb, string indent)
            {
                int count = this._particles.Count;
                string indentStr = WsdlGenerator.IndentP(indent);
                string str2 = WsdlGenerator.IndentP(indentStr);
                if (count > 0)
                {
                    bool flag = WsdlGenerator.blockDefault != this._blockType;
                    if (flag)
                    {
                        sb.Length = 0;
                        sb.Append(indentStr);
                        sb.Append(schemaBlockBegin[(int) this._blockType]);
                        textWriter.WriteLine(sb);
                    }
                    for (int j = 0; j < count; j++)
                    {
                        ((WsdlGenerator.Particle) this._particles[j]).Print(textWriter, sb, WsdlGenerator.IndentP(str2));
                    }
                    if (flag)
                    {
                        sb.Length = 0;
                        sb.Append(indentStr);
                        sb.Append(schemaBlockEnd[(int) this._blockType]);
                        textWriter.WriteLine(sb);
                    }
                }
                int num3 = this._abstractElms.Count;
                for (int i = 0; i < num3; i++)
                {
                    ((WsdlGenerator.IAbstractElement) this._abstractElms[i]).Print(textWriter, sb, WsdlGenerator.IndentP(indent));
                }
            }

            protected string BaseName
            {
                get
                {
                    return this._baseName;
                }
                set
                {
                    this._baseName = value;
                }
            }

            internal string ElementName
            {
                get
                {
                    return this._elementName;
                }
                set
                {
                    this._elementName = value;
                }
            }

            internal string FullRefName
            {
                get
                {
                    return this._fullRefName;
                }
            }

            protected bool IsEmpty
            {
                get
                {
                    return ((this._abstractElms.Count == 0) && (this._particles.Count == 0));
                }
            }

            protected bool IsSealed
            {
                get
                {
                    return this._bSealed;
                }
            }

            internal string Name
            {
                get
                {
                    return this._name;
                }
            }
        }

        private class EnumElement : WsdlGenerator.IAbstractElement
        {
            private string _value;

            internal EnumElement(string value)
            {
                this._value = value;
            }

            public void Print(TextWriter textWriter, StringBuilder sb, string indent)
            {
                sb.Length = 0;
                sb.Append(indent);
                sb.Append("<enumeration value='");
                sb.Append(this._value);
                sb.Append("'/>");
                textWriter.WriteLine(sb);
            }
        }

        private interface IAbstractElement
        {
            void Print(TextWriter textWriter, StringBuilder sb, string indent);
        }

        private abstract class Particle : WsdlGenerator.IAbstractElement
        {
            protected Particle()
            {
            }

            public abstract string Name();
            public abstract void Print(TextWriter textWriter, StringBuilder sb, string indent);
        }

        private class PhonySchemaType : WsdlGenerator.ComplexSchemaType
        {
            internal ArrayList _inParamNames;
            internal ArrayList _inParamTypes;
            private int _numOverloadedTypes;
            internal ArrayList _outParamNames;
            internal ArrayList _outParamTypes;
            internal ArrayList _paramNamesOrder;
            internal string _returnName;
            internal string _returnType;

            internal PhonySchemaType(string name) : base(name, true)
            {
                this._numOverloadedTypes = 0;
            }

            internal int OverloadedType()
            {
                return ++this._numOverloadedTypes;
            }

            internal override void PrintSchemaType(TextWriter textWriter, StringBuilder sb, string indent, bool bAnonymous)
            {
            }
        }

        private class RealSchemaType : WsdlGenerator.ComplexSchemaType
        {
            private bool _bStruct;
            private bool _bUnique;
            private FieldInfo[] _fields;
            private System.Type[] _iFaces;
            private string[] _implIFaces;
            private string[] _methodAttributes;
            private MethodInfo[] _methods;
            private string[] _methodTypes;
            internal System.Type[] _nestedTypes;
            private WsdlGenerator.PhonySchemaType[] _phony;
            private string _serviceEndpoint;
            private System.Type _type;
            private Hashtable _typeToServiceEndpoint;
            private WsdlGenerator _WsdlGenerator;
            private WsdlGenerator.XMLNamespace _xns;
            private static FieldInfo[] emptyFieldSet = new FieldInfo[0];
            private static MethodInfo[] emptyMethodSet = new MethodInfo[0];
            private static System.Type[] emptyTypeSet = new System.Type[0];

            internal RealSchemaType(System.Type type, WsdlGenerator.XMLNamespace xns, string serviceEndpoint, Hashtable typeToServiceEndpoint, bool bUnique, WsdlGenerator WsdlGenerator) : base(type)
            {
                this._type = type;
                this._serviceEndpoint = serviceEndpoint;
                this._typeToServiceEndpoint = typeToServiceEndpoint;
                this._bUnique = bUnique;
                this._WsdlGenerator = WsdlGenerator;
                this._bStruct = type.IsValueType;
                this._xns = xns;
                this._implIFaces = null;
                this._iFaces = null;
                this._methods = null;
                this._fields = null;
                this._methodTypes = null;
                this._nestedTypes = type.GetNestedTypes();
                if (this._nestedTypes != null)
                {
                    foreach (System.Type type2 in this._nestedTypes)
                    {
                        this._WsdlGenerator.AddType(type2, xns);
                    }
                }
            }

            private static void FindMethodAttributes(System.Type type, MethodInfo[] infos, ref string[] methodAttributes, BindingFlags bFlags)
            {
                System.Type baseType = type;
                ArrayList list = new ArrayList();
                while (true)
                {
                    baseType = baseType.BaseType;
                    if ((baseType == null) || baseType.FullName.StartsWith("System."))
                    {
                        break;
                    }
                    list.Add(baseType);
                }
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < infos.Length; i++)
                {
                    MethodBase base2 = infos[i];
                    builder.Length = 0;
                    MethodAttributes attributes = base2.Attributes;
                    bool isVirtual = base2.IsVirtual;
                    bool flag2 = (attributes & MethodAttributes.NewSlot) == MethodAttributes.NewSlot;
                    if (base2.IsPublic)
                    {
                        builder.Append("public");
                    }
                    else if (base2.IsFamily)
                    {
                        builder.Append("protected");
                    }
                    else if (base2.IsAssembly)
                    {
                        builder.Append("internal");
                    }
                    bool flag3 = false;
                    for (int j = 0; j < list.Count; j++)
                    {
                        baseType = (System.Type) list[j];
                        ParameterInfo[] parameters = base2.GetParameters();
                        System.Type[] types = new System.Type[parameters.Length];
                        for (int k = 0; k < types.Length; k++)
                        {
                            types[k] = parameters[k].ParameterType;
                        }
                        MethodInfo method = baseType.GetMethod(base2.Name, types);
                        if (method != null)
                        {
                            if (builder.Length > 0)
                            {
                                builder.Append(" ");
                            }
                            if (flag2 || method.IsFinal)
                            {
                                builder.Append("new");
                            }
                            else if (method.IsVirtual && isVirtual)
                            {
                                builder.Append("override");
                            }
                            else
                            {
                                builder.Append("new");
                            }
                            flag3 = true;
                            break;
                        }
                    }
                    if (!flag3 && isVirtual)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(" ");
                        }
                        builder.Append("virtual");
                    }
                    if (builder.Length > 0)
                    {
                        methodAttributes[i] = builder.ToString();
                    }
                }
            }

            internal FieldInfo[] GetInstanceFields()
            {
                this._fields = GetInstanceFields(this._type);
                return this._fields;
            }

            private static FieldInfo[] GetInstanceFields(System.Type type)
            {
                BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                if (!WsdlGenerator.s_marshalByRefType.IsAssignableFrom(type))
                {
                    bindingAttr |= BindingFlags.NonPublic;
                }
                FieldInfo[] fields = type.GetFields(bindingAttr);
                int length = fields.Length;
                if (length == 0)
                {
                    return emptyFieldSet;
                }
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].IsStatic)
                    {
                        length--;
                        fields[i] = fields[length];
                        fields[length] = null;
                    }
                }
                if (length < fields.Length)
                {
                    FieldInfo[] destinationArray = new FieldInfo[length];
                    Array.Copy(fields, destinationArray, length);
                    return destinationArray;
                }
                return fields;
            }

            internal System.Type[] GetIntroducedInterfaces()
            {
                this._iFaces = GetIntroducedInterfaces(this._type);
                return this._iFaces;
            }

            private static System.Type[] GetIntroducedInterfaces(System.Type type)
            {
                ArrayList list = new ArrayList();
                foreach (System.Type type2 in type.GetInterfaces())
                {
                    if (!type2.FullName.StartsWith("System."))
                    {
                        list.Add(type2);
                    }
                }
                System.Type[] typeArray2 = new System.Type[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    typeArray2[i] = (System.Type) list[i];
                }
                return typeArray2;
            }

            internal MethodInfo[] GetIntroducedMethods()
            {
                this._methods = GetIntroducedMethods(this._type, ref this._methodAttributes);
                this._methodTypes = new string[2 * this._methods.Length];
                return this._methods;
            }

            private static MethodInfo[] GetIntroducedMethods(System.Type type, ref string[] methodAttributes)
            {
                BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                MethodInfo[] methods = type.GetMethods(bindingAttr);
                if (!type.IsInterface)
                {
                    methodAttributes = new string[methods.Length];
                    FindMethodAttributes(type, methods, ref methodAttributes, bindingAttr);
                    ArrayList list = new ArrayList();
                    foreach (System.Type type2 in type.GetInterfaces())
                    {
                        foreach (MethodInfo info in type.GetInterfaceMap(type2).TargetMethods)
                        {
                            if (!info.IsPublic && (type.GetMethod(info.Name, bindingAttr | BindingFlags.NonPublic) != null))
                            {
                                list.Add(info);
                            }
                        }
                    }
                    MethodInfo[] infoArray2 = null;
                    if (list.Count > 0)
                    {
                        infoArray2 = new MethodInfo[methods.Length + list.Count];
                        for (int i = 0; i < methods.Length; i++)
                        {
                            infoArray2[i] = methods[i];
                        }
                        for (int j = 0; j < list.Count; j++)
                        {
                            infoArray2[methods.Length + j] = (MethodInfo) list[j];
                        }
                        return infoArray2;
                    }
                }
                return methods;
            }

            private bool IsNotSystemDefinedRoot(System.Type type, System.Type baseType)
            {
                return ((((!type.IsInterface && !type.IsValueType) && ((baseType != null) && (baseType.BaseType != null))) && (((baseType != WsdlGenerator.s_marshalByRefType) && (baseType != WsdlGenerator.s_valueType)) && ((baseType != WsdlGenerator.s_objectType) && (baseType != WsdlGenerator.s_contextBoundType)))) && (((baseType != WsdlGenerator.s_remotingClientProxyType) && (baseType.FullName != "System.EnterpriseServices.ServicedComponent")) && (baseType.FullName != "System.__ComObject")));
            }

            private void ParamInOut(ParameterInfo param, out bool bMarshalIn, out bool bMarshalOut)
            {
                bool isIn = param.IsIn;
                bool isOut = param.IsOut;
                bool isByRef = param.ParameterType.IsByRef;
                bMarshalIn = false;
                bMarshalOut = false;
                if (isByRef)
                {
                    if (isIn == isOut)
                    {
                        bMarshalIn = true;
                        bMarshalOut = true;
                    }
                    else
                    {
                        bMarshalIn = isIn;
                        bMarshalOut = isOut;
                    }
                }
                else
                {
                    bMarshalIn = true;
                    bMarshalOut = isOut;
                }
            }

            internal void PrintMessageWsdl(TextWriter textWriter, StringBuilder sb, string indent, ArrayList refNames)
            {
                string indentStr = WsdlGenerator.IndentP(indent);
                string str2 = WsdlGenerator.IndentP(indentStr);
                string str3 = WsdlGenerator.IndentP(str2);
                string str4 = null;
                MethodInfo method = null;
                string str5 = null;
                string str6 = null;
                bool flag = false;
                string ns = null;
                if (this._xns.IsInteropType)
                {
                    ns = this._xns.Name;
                }
                else
                {
                    sb.Length = 0;
                    WsdlGenerator.QualifyName(sb, this._xns.Name, base.Name);
                    ns = sb.ToString();
                }
                WsdlGenerator.XMLNamespace schemaNamespace = this._xns.LookupSchemaNamespace(ns, this._xns.Assem);
                int length = 0;
                if (this._methods != null)
                {
                    length = this._methods.Length;
                }
                if (length > 0)
                {
                    str4 = schemaNamespace.Namespace;
                    string prefix = schemaNamespace.Prefix;
                }
                refNames.Add(base.Name);
                for (int i = 0; i < length; i++)
                {
                    method = this._methods[i];
                    flag = RemotingServices.IsOneWay(method);
                    str5 = PrintMethodName(method);
                    sb.Length = 0;
                    WsdlGenerator.QualifyName(sb, base.Name, this._methodTypes[2 * i]);
                    str6 = sb.ToString();
                    sb.Length = 0;
                    sb.Append("\n");
                    sb.Append(indent);
                    sb.Append("<message name='");
                    sb.Append(str6 + "Input");
                    sb.Append("'>");
                    textWriter.WriteLine(sb);
                    WsdlGenerator.PhonySchemaType type = this._phony[i];
                    if (type._inParamTypes != null)
                    {
                        for (int m = 0; m < type._inParamTypes.Count; m++)
                        {
                            sb.Length = 0;
                            sb.Append(indentStr);
                            sb.Append("<part name='");
                            sb.Append(type._inParamNames[m]);
                            sb.Append("' type='");
                            sb.Append(type._inParamTypes[m]);
                            sb.Append("'/>");
                            textWriter.WriteLine(sb);
                        }
                        sb.Length = 0;
                        sb.Append(indent);
                        sb.Append("</message>");
                        textWriter.WriteLine(sb);
                        if (!flag)
                        {
                            sb.Length = 0;
                            sb.Append(indent);
                            sb.Append("<message name='");
                            sb.Append(str6 + "Output");
                            sb.Append("'>");
                            textWriter.WriteLine(sb);
                            if ((type._returnType != null) || (type._outParamTypes != null))
                            {
                                if (type._returnType != null)
                                {
                                    sb.Length = 0;
                                    sb.Append(indentStr);
                                    sb.Append("<part name='");
                                    sb.Append(type._returnName);
                                    sb.Append("' type='");
                                    sb.Append(type._returnType);
                                    sb.Append("'/>");
                                    textWriter.WriteLine(sb);
                                }
                                if (type._outParamTypes != null)
                                {
                                    for (int n = 0; n < type._outParamTypes.Count; n++)
                                    {
                                        sb.Length = 0;
                                        sb.Append(indentStr);
                                        sb.Append("<part name='");
                                        sb.Append(type._outParamNames[n]);
                                        sb.Append("' type='");
                                        sb.Append(type._outParamTypes[n]);
                                        sb.Append("'/>");
                                        textWriter.WriteLine(sb);
                                    }
                                }
                            }
                            sb.Length = 0;
                            sb.Append(indent);
                            sb.Append("</message>");
                            textWriter.WriteLine(sb);
                        }
                    }
                }
                sb.Length = 0;
                sb.Append("\n");
                sb.Append(indent);
                sb.Append("<portType name='");
                sb.Append(base.Name);
                sb.Append("PortType");
                sb.Append("'>");
                textWriter.WriteLine(sb);
                for (int j = 0; j < length; j++)
                {
                    method = this._methods[j];
                    WsdlGenerator.PhonySchemaType type2 = this._phony[j];
                    flag = RemotingServices.IsOneWay(method);
                    str5 = PrintMethodName(method);
                    sb.Length = 0;
                    sb.Append("tns:");
                    WsdlGenerator.QualifyName(sb, base.Name, this._methodTypes[2 * j]);
                    str6 = sb.ToString();
                    sb.Length = 0;
                    sb.Append(indentStr);
                    sb.Append("<operation name='");
                    sb.Append(str5);
                    sb.Append("'");
                    if ((type2 != null) && (type2._paramNamesOrder.Count > 0))
                    {
                        sb.Append(" parameterOrder='");
                        bool flag2 = true;
                        foreach (string str8 in type2._paramNamesOrder)
                        {
                            if (!flag2)
                            {
                                sb.Append(" ");
                            }
                            sb.Append(str8);
                            flag2 = false;
                        }
                        sb.Append("'");
                    }
                    sb.Append(">");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str2);
                    sb.Append("<input name='");
                    sb.Append(this._methodTypes[2 * j]);
                    sb.Append("Request' ");
                    sb.Append("message='");
                    sb.Append(str6);
                    sb.Append("Input");
                    sb.Append("'/>");
                    textWriter.WriteLine(sb);
                    if (!flag)
                    {
                        sb.Length = 0;
                        sb.Append(str2);
                        sb.Append("<output name='");
                        sb.Append(this._methodTypes[2 * j]);
                        sb.Append("Response' ");
                        sb.Append("message='");
                        sb.Append(str6);
                        sb.Append("Output");
                        sb.Append("'/>");
                        textWriter.WriteLine(sb);
                    }
                    sb.Length = 0;
                    sb.Append(indentStr);
                    sb.Append("</operation>");
                    textWriter.WriteLine(sb);
                }
                sb.Length = 0;
                sb.Append(indent);
                sb.Append("</portType>");
                textWriter.WriteLine(sb);
                sb.Length = 0;
                sb.Append("\n");
                sb.Append(indent);
                sb.Append("<binding name='");
                sb.Append(base.Name);
                sb.Append("Binding");
                sb.Append("' ");
                sb.Append("type='tns:");
                sb.Append(base.Name);
                sb.Append("PortType");
                sb.Append("'>");
                textWriter.WriteLine(sb);
                sb.Length = 0;
                sb.Append(indentStr);
                sb.Append("<soap:binding style='rpc' transport='http://schemas.xmlsoap.org/soap/http'/>");
                textWriter.WriteLine(sb);
                if (this._type.IsInterface || this.IsSUDSType)
                {
                    this.PrintSuds(this._type, this._implIFaces, this._nestedTypes, textWriter, sb, indent);
                }
                if (!this._xns.IsClassesPrinted)
                {
                    for (int num6 = 0; num6 < this._xns._realSchemaTypes.Count; num6++)
                    {
                        WsdlGenerator.RealSchemaType type3 = (WsdlGenerator.RealSchemaType) this._xns._realSchemaTypes[num6];
                        System.Type type4 = type3._type;
                        if (!type3.Type.IsInterface && !type3.IsSUDSType)
                        {
                            System.Type[] introducedInterfaces = GetIntroducedInterfaces(type3._type);
                            string[] implIFaces = null;
                            bool flag3 = false;
                            if (introducedInterfaces.Length > 0)
                            {
                                implIFaces = new string[introducedInterfaces.Length];
                                int index = 0;
                                while (num6 < introducedInterfaces.Length)
                                {
                                    string str9;
                                    Assembly assembly;
                                    WsdlGenerator.GetNSAndAssembly(introducedInterfaces[index], out str9, out assembly);
                                    WsdlGenerator.XMLNamespace namespace3 = this._xns.LookupSchemaNamespace(str9, assembly);
                                    sb.Length = 0;
                                    sb.Append(namespace3.Prefix);
                                    sb.Append(':');
                                    sb.Append(introducedInterfaces[index].Name);
                                    implIFaces[index] = sb.ToString();
                                    if (implIFaces[index].Length > 0)
                                    {
                                        flag3 = true;
                                    }
                                    num6++;
                                }
                            }
                            if (!flag3)
                            {
                                implIFaces = null;
                            }
                            this.PrintSuds(type4, implIFaces, type3._nestedTypes, textWriter, sb, indent);
                        }
                    }
                    this._xns.IsClassesPrinted = true;
                }
                for (int k = 0; k < length; k++)
                {
                    method = this._methods[k];
                    str5 = PrintMethodName(method);
                    flag = RemotingServices.IsOneWay(method);
                    sb.Length = 0;
                    sb.Append(indentStr);
                    sb.Append("<operation name='");
                    sb.Append(str5);
                    sb.Append("'>");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str2);
                    sb.Append("<soap:operation soapAction='");
                    string soapActionFromMethodBase = SoapServices.GetSoapActionFromMethodBase(method);
                    if ((soapActionFromMethodBase != null) || (soapActionFromMethodBase.Length > 0))
                    {
                        sb.Append(soapActionFromMethodBase);
                    }
                    else
                    {
                        sb.Append(str4);
                        sb.Append('#');
                        sb.Append(str5);
                    }
                    sb.Append("'/>");
                    textWriter.WriteLine(sb);
                    if (((this._methodAttributes != null) && (k < this._methodAttributes.Length)) && (this._methodAttributes[k] != null))
                    {
                        sb.Length = 0;
                        sb.Append(str2);
                        sb.Append("<suds:method attributes='");
                        sb.Append(this._methodAttributes[k]);
                        sb.Append("'/>");
                        textWriter.WriteLine(sb);
                    }
                    sb.Length = 0;
                    sb.Append(str2);
                    sb.Append("<input name='");
                    sb.Append(this._methodTypes[2 * k]);
                    sb.Append("Request'>");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str3);
                    sb.Append("<soap:body use='encoded' encodingStyle='http://schemas.xmlsoap.org/soap/encoding/' namespace='");
                    string xmlNamespaceForMethodCall = SoapServices.GetXmlNamespaceForMethodCall(method);
                    if (xmlNamespaceForMethodCall == null)
                    {
                        sb.Append(str4);
                    }
                    else
                    {
                        sb.Append(xmlNamespaceForMethodCall);
                    }
                    sb.Append("'/>");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str2);
                    sb.Append("</input>");
                    textWriter.WriteLine(sb);
                    if (!flag)
                    {
                        sb.Length = 0;
                        sb.Append(str2);
                        sb.Append("<output name='");
                        sb.Append(this._methodTypes[2 * k]);
                        sb.Append("Response'>");
                        textWriter.WriteLine(sb);
                        sb.Length = 0;
                        sb.Append(str3);
                        sb.Append("<soap:body use='encoded' encodingStyle='http://schemas.xmlsoap.org/soap/encoding/' namespace='");
                        xmlNamespaceForMethodCall = SoapServices.GetXmlNamespaceForMethodResponse(method);
                        if (xmlNamespaceForMethodCall == null)
                        {
                            sb.Append(str4);
                        }
                        else
                        {
                            sb.Append(xmlNamespaceForMethodCall);
                        }
                        sb.Append("'/>");
                        textWriter.WriteLine(sb);
                        sb.Length = 0;
                        sb.Append(str2);
                        sb.Append("</output>");
                        textWriter.WriteLine(sb);
                    }
                    sb.Length = 0;
                    sb.Append(indentStr);
                    sb.Append("</operation>");
                    textWriter.WriteLine(sb);
                }
                sb.Length = 0;
                sb.Append(indent);
                sb.Append("</binding>");
                textWriter.WriteLine(sb);
            }

            internal static string PrintMethodName(MethodInfo methodInfo)
            {
                string name = methodInfo.Name;
                int num = 0;
                int num2 = 0;
                for (int i = 0; i < name.Length; i++)
                {
                    if (name[i] == '.')
                    {
                        num2 = num;
                        num = i;
                    }
                }
                string str2 = name;
                if (num2 > 0)
                {
                    str2 = name.Substring(num2 + 1);
                }
                return str2;
            }

            internal override void PrintSchemaType(TextWriter textWriter, StringBuilder sb, string indent, bool bAnonymous)
            {
                if (!bAnonymous)
                {
                    sb.Length = 0;
                    sb.Append(indent);
                    sb.Append("<element name='");
                    sb.Append(base.ElementName);
                    sb.Append("' type='");
                    sb.Append(this._xns.Prefix);
                    sb.Append(':');
                    sb.Append(base.FullRefName);
                    sb.Append("'/>");
                    textWriter.WriteLine(sb);
                }
                sb.Length = 0;
                sb.Append(indent);
                if (!bAnonymous)
                {
                    sb.Append("<complexType name='");
                    sb.Append(base.FullRefName);
                    sb.Append('\'');
                }
                else
                {
                    sb.Append("<complexType ");
                }
                if (base.BaseName != null)
                {
                    sb.Append(" base='");
                    sb.Append(base.BaseName);
                    sb.Append('\'');
                }
                if (base.IsSealed && !bAnonymous)
                {
                    sb.Append(" final='#all'");
                }
                bool isEmpty = base.IsEmpty;
                if (isEmpty)
                {
                    sb.Append("/>");
                }
                else
                {
                    sb.Append('>');
                }
                textWriter.WriteLine(sb);
                if (!isEmpty)
                {
                    base.PrintBody(textWriter, sb, indent);
                    textWriter.Write(indent);
                    textWriter.WriteLine("</complexType>");
                }
            }

            private void PrintSuds(System.Type type, string[] implIFaces, System.Type[] nestedTypes, TextWriter textWriter, StringBuilder sb, string indent)
            {
                string indentStr = WsdlGenerator.IndentP(indent);
                string str2 = WsdlGenerator.IndentP(indentStr);
                WsdlGenerator.IndentP(str2);
                string str3 = null;
                sb.Length = 0;
                sb.Append(indentStr);
                if (type.IsInterface)
                {
                    sb.Append("<suds:interface type='");
                    str3 = "</suds:interface>";
                }
                else if (type.IsValueType)
                {
                    sb.Append("<suds:struct type='");
                    str3 = "</suds:struct>";
                }
                else
                {
                    sb.Append("<suds:class type='");
                    str3 = "</suds:class>";
                }
                sb.Append(this._xns.Prefix);
                sb.Append(':');
                sb.Append(WsdlGenerator.RefName(type));
                sb.Append("'");
                System.Type baseType = type.BaseType;
                if (this.IsNotSystemDefinedRoot(type, baseType))
                {
                    WsdlGenerator.XMLNamespace namespace2 = this._WsdlGenerator.GetNamespace(baseType);
                    sb.Append(" extends='");
                    sb.Append(namespace2.Prefix);
                    sb.Append(':');
                    sb.Append(baseType.Name);
                    sb.Append("'");
                }
                if ((baseType != null) && (baseType.FullName == "System.EnterpriseServices.ServicedComponent"))
                {
                    sb.Append(" rootType='ServicedComponent'");
                }
                else if (typeof(Delegate).IsAssignableFrom(type) || typeof(MulticastDelegate).IsAssignableFrom(type))
                {
                    sb.Append(" rootType='Delegate'");
                }
                else if (typeof(MarshalByRefObject).IsAssignableFrom(type))
                {
                    sb.Append(" rootType='MarshalByRefObject'");
                }
                else if (typeof(ISerializable).IsAssignableFrom(type))
                {
                    sb.Append(" rootType='ISerializable'");
                }
                if ((implIFaces == null) && (nestedTypes == null))
                {
                    sb.Append("/>");
                }
                else
                {
                    sb.Append(">");
                }
                textWriter.WriteLine(sb);
                string str4 = null;
                if (type.IsInterface)
                {
                    str4 = "<suds:extends type='";
                }
                else
                {
                    str4 = "<suds:implements type='";
                }
                if (implIFaces != null)
                {
                    for (int i = 0; i < implIFaces.Length; i++)
                    {
                        if ((implIFaces[i] != null) && !(implIFaces[i] == string.Empty))
                        {
                            sb.Length = 0;
                            sb.Append(str2);
                            sb.Append(str4);
                            sb.Append(implIFaces[i]);
                            sb.Append("'/>");
                            textWriter.WriteLine(sb);
                        }
                    }
                }
                if (nestedTypes != null)
                {
                    for (int j = 0; j < nestedTypes.Length; j++)
                    {
                        sb.Length = 0;
                        sb.Append(str2);
                        sb.Append("<suds:nestedType name='");
                        sb.Append(nestedTypes[j].Name);
                        sb.Append("' type='");
                        sb.Append(this._xns.Prefix);
                        sb.Append(':');
                        sb.Append(WsdlGenerator.RefName(nestedTypes[j]));
                        sb.Append("'/>");
                        textWriter.WriteLine(sb);
                    }
                }
                if ((implIFaces != null) || (nestedTypes != null))
                {
                    sb.Length = 0;
                    sb.Append(indentStr);
                    sb.Append(str3);
                    textWriter.WriteLine(sb);
                }
            }

            private static string ProcessArray(System.Type type, WsdlGenerator.XMLNamespace xns)
            {
                string wireQname = null;
                bool flag = false;
                System.Type elementType = type.GetElementType();
                string str2 = "ArrayOf";
                while (elementType.IsArray)
                {
                    str2 = str2 + "ArrayOf";
                    elementType = elementType.GetElementType();
                }
                wireQname = TypeName(elementType, true, xns);
                int index = wireQname.IndexOf(":");
                wireQname.Substring(0, index);
                string str3 = wireQname.Substring(index + 1);
                int arrayRank = type.GetArrayRank();
                string str4 = "";
                if (arrayRank > 1)
                {
                    str4 = arrayRank.ToString(CultureInfo.InvariantCulture);
                }
                string name = (str2 + str3.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture) + str3.Substring(1) + str4).Replace('+', 'N');
                if (xns.LookupArraySchemaType(name) == null)
                {
                    WsdlGenerator.ArraySchemaType asType = new WsdlGenerator.ArraySchemaType(type, name, SchemaBlockType.ComplexContent, false);
                    WsdlGenerator.Restriction particle = new WsdlGenerator.Restriction();
                    WsdlGenerator.SchemaAttribute attribute = new WsdlGenerator.SchemaAttribute();
                    if (flag)
                    {
                        attribute.AddArray(wireQname);
                    }
                    else
                    {
                        string str6 = type.Name;
                        index = str6.IndexOf("[");
                        attribute.AddArray(wireQname + str6.Substring(index));
                    }
                    particle.AddArray(attribute);
                    asType.AddParticle(particle);
                    xns.AddArraySchemaType(asType);
                }
                return (xns.Prefix + ":" + name);
            }

            internal void Resolve(StringBuilder sb)
            {
                sb.Length = 0;
                bool isSUDSType = this.IsSUDSType;
                System.Type baseType = this._type.BaseType;
                if (this.IsNotSystemDefinedRoot(this._type, baseType))
                {
                    WsdlGenerator.XMLNamespace xns = this._WsdlGenerator.GetNamespace(baseType);
                    sb.Append(xns.Prefix);
                    sb.Append(':');
                    sb.Append(baseType.Name);
                    base.BaseName = sb.ToString();
                    if (isSUDSType)
                    {
                        this._xns.DependsOnSUDSNS(xns);
                    }
                    System.Type type = this._type;
                    for (System.Type type3 = type.BaseType; (type3 != null) && this.IsNotSystemDefinedRoot(type, type3); type3 = type.BaseType)
                    {
                        if (((this._typeToServiceEndpoint != null) && !this._typeToServiceEndpoint.ContainsKey(type3.Name)) && this._typeToServiceEndpoint.ContainsKey(type.Name))
                        {
                            this._typeToServiceEndpoint[type3.Name] = this._typeToServiceEndpoint[type.Name];
                        }
                        type = type3;
                    }
                }
                this._xns.DependsOnSchemaNS(this._xns, false);
                if (isSUDSType)
                {
                    this._xns.AddRealSUDSType(this);
                    if (this._iFaces.Length > 0)
                    {
                        this._implIFaces = new string[this._iFaces.Length];
                        for (int i = 0; i < this._iFaces.Length; i++)
                        {
                            string str;
                            Assembly assembly;
                            WsdlGenerator.GetNSAndAssembly(this._iFaces[i], out str, out assembly);
                            WsdlGenerator.XMLNamespace schemaNamespace = this._xns.LookupSchemaNamespace(str, assembly);
                            sb.Length = 0;
                            sb.Append(schemaNamespace.Prefix);
                            sb.Append(':');
                            sb.Append(this._iFaces[i].Name);
                            this._implIFaces[i] = sb.ToString();
                            this._xns.DependsOnSUDSNS(schemaNamespace);
                        }
                    }
                    if (this._methods.Length > 0)
                    {
                        string ns = null;
                        if (this._xns.IsInteropType)
                        {
                            ns = this._xns.Name;
                        }
                        else
                        {
                            sb.Length = 0;
                            WsdlGenerator.QualifyName(sb, this._xns.Name, base.Name);
                            ns = sb.ToString();
                        }
                        WsdlGenerator.XMLNamespace namespace4 = this._xns.LookupSchemaNamespace(ns, this._xns.Assem);
                        this._xns.DependsOnSUDSNS(namespace4);
                        this._phony = new WsdlGenerator.PhonySchemaType[this._methods.Length];
                        for (int j = 0; j < this._methods.Length; j++)
                        {
                            MethodInfo method = this._methods[j];
                            string name = method.Name;
                            ParameterInfo[] parameters = method.GetParameters();
                            WsdlGenerator.PhonySchemaType phType = new WsdlGenerator.PhonySchemaType(name) {
                                _inParamTypes = new ArrayList(10),
                                _inParamNames = new ArrayList(10),
                                _outParamTypes = new ArrayList(10),
                                _outParamNames = new ArrayList(10),
                                _paramNamesOrder = new ArrayList(10)
                            };
                            int num3 = 0;
                            foreach (ParameterInfo info2 in parameters)
                            {
                                bool bMarshalIn = false;
                                bool bMarshalOut = false;
                                phType._paramNamesOrder.Add(info2.Name);
                                this.ParamInOut(info2, out bMarshalIn, out bMarshalOut);
                                System.Type parameterType = info2.ParameterType;
                                string str4 = info2.Name;
                                if ((str4 == null) || (str4.Length == 0))
                                {
                                    str4 = "param" + num3++;
                                }
                                string str5 = TypeName(parameterType, true, namespace4);
                                if (bMarshalIn)
                                {
                                    phType._inParamNames.Add(str4);
                                    phType._inParamTypes.Add(str5);
                                }
                                if (bMarshalOut)
                                {
                                    phType._outParamNames.Add(str4);
                                    phType._outParamTypes.Add(str5);
                                }
                            }
                            namespace4.AddPhonySchemaType(phType);
                            this._phony[j] = phType;
                            this._methodTypes[2 * j] = phType.ElementName;
                            if (!RemotingServices.IsOneWay(method))
                            {
                                string returnXmlElementName = null;
                                SoapMethodAttribute cachedSoapAttribute = (SoapMethodAttribute) InternalRemotingServices.GetCachedSoapAttribute(method);
                                if (cachedSoapAttribute.ReturnXmlElementName != null)
                                {
                                    returnXmlElementName = cachedSoapAttribute.ReturnXmlElementName;
                                }
                                else
                                {
                                    returnXmlElementName = "return";
                                }
                                string responseXmlElementName = null;
                                if (cachedSoapAttribute.ResponseXmlElementName != null)
                                {
                                    responseXmlElementName = cachedSoapAttribute.ResponseXmlElementName;
                                }
                                else
                                {
                                    responseXmlElementName = name + "Response";
                                }
                                WsdlGenerator.PhonySchemaType type6 = new WsdlGenerator.PhonySchemaType(responseXmlElementName);
                                phType._returnName = returnXmlElementName;
                                System.Type returnType = method.ReturnType;
                                if ((returnType != null) && !(returnType == typeof(void)))
                                {
                                    phType._returnType = TypeName(returnType, true, namespace4);
                                }
                                namespace4.AddPhonySchemaType(type6);
                                this._methodTypes[(2 * j) + 1] = type6.ElementName;
                            }
                        }
                    }
                }
                if (this._fields != null)
                {
                    for (int k = 0; k < this._fields.Length; k++)
                    {
                        FieldInfo info3 = this._fields[k];
                        System.Type fieldType = info3.FieldType;
                        if (fieldType == null)
                        {
                            fieldType = typeof(object);
                        }
                        base.AddParticle(new WsdlGenerator.SchemaElement(info3.Name, fieldType, false, this._xns));
                    }
                }
            }

            internal static string TypeName(System.Type type, bool bEmbedded, WsdlGenerator.XMLNamespace thisxns)
            {
                string str = null;
                if (type.IsArray)
                {
                    return ProcessArray(type, thisxns);
                }
                string str2 = WsdlGenerator.RefName(type);
                System.Type elementType = type;
                if (type.IsByRef)
                {
                    elementType = type.GetElementType();
                    str2 = WsdlGenerator.RefName(elementType);
                    if (elementType.IsArray)
                    {
                        return ProcessArray(elementType, thisxns);
                    }
                }
                str = SudsConverter.MapClrTypeToXsdType(elementType);
                if (str != null)
                {
                    return str;
                }
                string ns = type.Namespace;
                Assembly assem = type.Module.Assembly;
                WsdlGenerator.XMLNamespace xns = null;
                xns = (WsdlGenerator.XMLNamespace) thisxns.Generator._typeToInteropNS[type];
                if (xns == null)
                {
                    xns = thisxns.LookupSchemaNamespace(ns, assem);
                    if (xns == null)
                    {
                        xns = thisxns.Generator.LookupNamespace(ns, assem);
                        if (xns == null)
                        {
                            xns = thisxns.Generator.AddNamespace(ns, assem);
                        }
                        thisxns.DependsOnSchemaNS(xns, false);
                    }
                }
                StringBuilder builder = new StringBuilder(0x100);
                builder.Append(xns.Prefix);
                builder.Append(':');
                builder.Append(str2);
                return builder.ToString();
            }

            internal bool IsSUDSType
            {
                get
                {
                    if ((((this._iFaces == null) || (this._iFaces.Length <= 0)) && ((this._methods == null) || (this._methods.Length <= 0))) && ((this._type == null) || !this._type.IsInterface))
                    {
                        return ((WsdlGenerator.s_delegateType != null) && WsdlGenerator.s_delegateType.IsAssignableFrom(this._type));
                    }
                    return true;
                }
            }

            internal bool IsUnique
            {
                get
                {
                    return this._bUnique;
                }
            }

            internal System.Type Type
            {
                get
                {
                    return this._type;
                }
            }

            internal WsdlGenerator.XMLNamespace XNS
            {
                get
                {
                    return this._xns;
                }
            }
        }

        private class Restriction : WsdlGenerator.Particle
        {
            internal ArrayList _abstractElms;
            private WsdlGenerator.SchemaAttribute _attribute;
            private string _baseName;
            private WsdlGenerator.XMLNamespace _baseNS;
            internal RestrictionType _rtype;

            internal Restriction()
            {
                this._abstractElms = new ArrayList();
            }

            internal Restriction(string baseName, WsdlGenerator.XMLNamespace baseNS)
            {
                this._abstractElms = new ArrayList();
                this._baseName = baseName;
                this._baseNS = baseNS;
            }

            internal void AddArray(WsdlGenerator.SchemaAttribute attribute)
            {
                this._rtype = RestrictionType.Array;
                this._attribute = attribute;
            }

            public override string Name()
            {
                return this._baseName;
            }

            public override void Print(TextWriter textWriter, StringBuilder sb, string indent)
            {
                string indentStr = WsdlGenerator.IndentP(indent);
                sb.Length = 0;
                sb.Append(indent);
                if (this._rtype == RestrictionType.Array)
                {
                    sb.Append("<restriction base='soapenc:Array'>");
                }
                else if (this._rtype == RestrictionType.Enum)
                {
                    sb.Append("<restriction base='xsd:string'>");
                }
                else
                {
                    sb.Append("<restriction base='");
                    sb.Append(this._baseNS.Prefix);
                    sb.Append(':');
                    sb.Append(this._baseName);
                    sb.Append("'>");
                }
                textWriter.WriteLine(sb);
                foreach (WsdlGenerator.IAbstractElement element in this._abstractElms)
                {
                    element.Print(textWriter, sb, WsdlGenerator.IndentP(indentStr));
                }
                if (this._attribute != null)
                {
                    this._attribute.Print(textWriter, sb, WsdlGenerator.IndentP(indentStr));
                }
                sb.Length = 0;
                sb.Append(indent);
                sb.Append("</restriction>");
                textWriter.WriteLine(sb);
            }

            internal enum RestrictionType
            {
                None,
                Array,
                Enum
            }
        }

        private class SchemaAttribute : WsdlGenerator.IAbstractElement
        {
            private string _wireQname;

            internal SchemaAttribute()
            {
            }

            internal void AddArray(string wireQname)
            {
                this._wireQname = wireQname;
            }

            public void Print(TextWriter textWriter, StringBuilder sb, string indent)
            {
                sb.Length = 0;
                sb.Append(indent);
                sb.Append("<attribute ref='soapenc:arrayType'");
                sb.Append(" wsdl:arrayType ='");
                sb.Append(this._wireQname);
                sb.Append("'/>");
                textWriter.WriteLine(sb);
            }
        }

        private class SchemaElement : WsdlGenerator.Particle
        {
            private string _name;
            private WsdlGenerator.SchemaType _schemaType;
            private string _typeString;

            internal SchemaElement(string name, Type type, bool bEmbedded, WsdlGenerator.XMLNamespace xns)
            {
                this._name = name;
                this._typeString = null;
                this._schemaType = WsdlGenerator.SimpleSchemaType.GetSimpleSchemaType(type, xns, true);
                this._typeString = WsdlGenerator.RealSchemaType.TypeName(type, bEmbedded, xns);
            }

            public override string Name()
            {
                return this._name;
            }

            public override void Print(TextWriter textWriter, StringBuilder sb, string indent)
            {
                string indentStr = WsdlGenerator.IndentP(indent);
                sb.Length = 0;
                sb.Append(indent);
                sb.Append("<element name='");
                sb.Append(this._name);
                if ((this._schemaType != null) && (!(this._schemaType is WsdlGenerator.SimpleSchemaType) || !((WsdlGenerator.SimpleSchemaType) this._schemaType).Type.IsEnum))
                {
                    sb.Append("'>");
                    textWriter.WriteLine(sb);
                    this._schemaType.PrintSchemaType(textWriter, sb, WsdlGenerator.IndentP(indentStr), true);
                    sb.Length = 0;
                    sb.Append(indent);
                    sb.Append("</element>");
                }
                else
                {
                    if (this._typeString != null)
                    {
                        sb.Append("' type='");
                        sb.Append(this._typeString);
                        sb.Append('\'');
                    }
                    sb.Append("/>");
                }
                textWriter.WriteLine(sb);
            }
        }

        private abstract class SchemaType
        {
            protected SchemaType()
            {
            }

            internal abstract void PrintSchemaType(TextWriter textWriter, StringBuilder sb, string indent, bool bAnonymous);
        }

        private class SimpleSchemaType : WsdlGenerator.SchemaType
        {
            private ArrayList _abstractElms = new ArrayList();
            internal string _baseName;
            private string _fullRefName;
            internal WsdlGenerator.Restriction _restriction;
            private System.Type _type;
            private WsdlGenerator.XMLNamespace _xns;

            private SimpleSchemaType(System.Type type, WsdlGenerator.XMLNamespace xns)
            {
                this._type = type;
                this._xns = xns;
                this._abstractElms = new ArrayList();
                this._fullRefName = WsdlGenerator.RefName(type);
            }

            internal static WsdlGenerator.SimpleSchemaType GetSimpleSchemaType(System.Type type, WsdlGenerator.XMLNamespace xns, bool fInline)
            {
                WsdlGenerator.SimpleSchemaType type2 = null;
                if (type.IsEnum)
                {
                    type2 = new WsdlGenerator.SimpleSchemaType(type, xns);
                    string baseName = WsdlGenerator.RealSchemaType.TypeName(Enum.GetUnderlyingType(type), true, xns);
                    type2._restriction = new WsdlGenerator.Restriction(baseName, xns);
                    string[] names = Enum.GetNames(type);
                    for (int i = 0; i < names.Length; i++)
                    {
                        type2._restriction._abstractElms.Add(new WsdlGenerator.EnumElement(names[i]));
                    }
                    type2._restriction._rtype = WsdlGenerator.Restriction.RestrictionType.Enum;
                }
                return type2;
            }

            internal override void PrintSchemaType(TextWriter textWriter, StringBuilder sb, string indent, bool bAnonymous)
            {
                sb.Length = 0;
                sb.Append(indent);
                if (!bAnonymous)
                {
                    sb.Append("<simpleType name='");
                    sb.Append(this.FullRefName);
                    sb.Append("'");
                    if (this.BaseName != null)
                    {
                        sb.Append(" base='");
                        sb.Append(this.BaseName);
                        sb.Append("'");
                    }
                    if (this._restriction._rtype == WsdlGenerator.Restriction.RestrictionType.Enum)
                    {
                        sb.Append(" suds:enumType='");
                        sb.Append(this._restriction.Name());
                        sb.Append("'");
                    }
                }
                else if (this.BaseName != null)
                {
                    sb.Append("<simpleType base='");
                    sb.Append(this.BaseName);
                    sb.Append("'");
                }
                else
                {
                    sb.Append("<simpleType");
                }
                bool flag = (this._abstractElms.Count == 0) && (this._restriction == null);
                if (flag)
                {
                    sb.Append("/>");
                }
                else
                {
                    sb.Append(">");
                }
                textWriter.WriteLine(sb);
                if (!flag)
                {
                    if (this._abstractElms.Count > 0)
                    {
                        for (int i = 0; i < this._abstractElms.Count; i++)
                        {
                            ((WsdlGenerator.IAbstractElement) this._abstractElms[i]).Print(textWriter, sb, WsdlGenerator.IndentP(indent));
                        }
                    }
                    if (this._restriction != null)
                    {
                        this._restriction.Print(textWriter, sb, WsdlGenerator.IndentP(indent));
                    }
                    textWriter.Write(indent);
                    textWriter.WriteLine("</simpleType>");
                }
            }

            internal string BaseName
            {
                get
                {
                    return this._baseName;
                }
            }

            internal string FullRefName
            {
                get
                {
                    return this._fullRefName;
                }
            }

            internal System.Type Type
            {
                get
                {
                    return this._type;
                }
            }
        }

        private class XMLNamespace
        {
            private ArrayList _arraySchemaTypes;
            private Assembly _assem;
            private bool _bClassesPrinted;
            private bool _bInteropType;
            internal bool _bUnique;
            private ArrayList _dependsOnSchemaNS;
            private ArrayList _dependsOnSUDSNS;
            private WsdlGenerator _generator;
            private string _name;
            private string _namespace;
            private ArrayList _phonySchemaTypes;
            private string _prefix;
            internal ArrayList _realSchemaTypes;
            private ArrayList _realSUDSTypes;
            private string _serviceEndpoint;
            private ArrayList _simpleSchemaTypes;
            private Hashtable _typeToServiceEndpoint;
            private ArrayList _xnsImports;

            internal XMLNamespace(string name, Assembly assem, string serviceEndpoint, Hashtable typeToServiceEndpoint, string prefix, bool bInteropType, WsdlGenerator generator)
            {
                this._name = name;
                this._assem = assem;
                this._bUnique = false;
                this._bInteropType = bInteropType;
                this._generator = generator;
                StringBuilder builder = new StringBuilder(0x100);
                Assembly assembly = typeof(string).Module.Assembly;
                if (!this._bInteropType)
                {
                    if (assem == assembly)
                    {
                        builder.Append(SoapServices.CodeXmlNamespaceForClrTypeNamespace(name, null));
                    }
                    else if (assem != null)
                    {
                        builder.Append(SoapServices.CodeXmlNamespaceForClrTypeNamespace(name, assem.FullName));
                    }
                }
                else
                {
                    builder.Append(name);
                }
                this._namespace = builder.ToString();
                this._prefix = prefix;
                this._dependsOnSchemaNS = new ArrayList();
                this._realSUDSTypes = new ArrayList();
                this._dependsOnSUDSNS = new ArrayList();
                this._realSchemaTypes = new ArrayList();
                this._phonySchemaTypes = new ArrayList();
                this._simpleSchemaTypes = new ArrayList();
                this._arraySchemaTypes = new ArrayList();
                this._xnsImports = new ArrayList();
                this._serviceEndpoint = serviceEndpoint;
                this._typeToServiceEndpoint = typeToServiceEndpoint;
            }

            internal void AddArraySchemaType(WsdlGenerator.ArraySchemaType asType)
            {
                this._arraySchemaTypes.Add(asType);
            }

            internal void AddPhonySchemaType(WsdlGenerator.PhonySchemaType phType)
            {
                WsdlGenerator.PhonySchemaType phonySchemaType = this.LookupPhonySchemaType(phType.Name);
                if (phonySchemaType != null)
                {
                    phType.ElementName = phType.Name + phonySchemaType.OverloadedType();
                }
                this._phonySchemaTypes.Add(phType);
            }

            internal void AddRealSchemaType(WsdlGenerator.RealSchemaType rsType)
            {
                this._realSchemaTypes.Add(rsType);
                if (rsType.IsUnique)
                {
                    this._bUnique = true;
                }
            }

            internal void AddRealSUDSType(WsdlGenerator.RealSchemaType rsType)
            {
                this._realSUDSTypes.Add(rsType);
            }

            internal void AddSimpleSchemaType(WsdlGenerator.SimpleSchemaType ssType)
            {
                this._simpleSchemaTypes.Add(ssType);
            }

            internal bool CheckForSchemaContent()
            {
                if ((this._arraySchemaTypes.Count > 0) || (this._simpleSchemaTypes.Count > 0))
                {
                    return true;
                }
                if (this._realSchemaTypes.Count == 0)
                {
                    return false;
                }
                bool flag = false;
                for (int i = 0; i < this._realSchemaTypes.Count; i++)
                {
                    WsdlGenerator.RealSchemaType type = (WsdlGenerator.RealSchemaType) this._realSchemaTypes[i];
                    if (!type.Type.IsInterface && !type.IsSUDSType)
                    {
                        flag = true;
                        break;
                    }
                }
                return flag;
            }

            internal void DependsOnSchemaNS(WsdlGenerator.XMLNamespace xns, bool bImport)
            {
                if (this.LookupSchemaNamespace(xns.Name, xns.Assem) == null)
                {
                    this._dependsOnSchemaNS.Add(xns);
                    if (bImport && (this.Namespace != xns.Namespace))
                    {
                        this._xnsImports.Add(xns);
                    }
                }
            }

            internal void DependsOnSUDSNS(WsdlGenerator.XMLNamespace xns)
            {
                if (this.LookupSUDSNamespace(xns.Name, xns.Assem) == null)
                {
                    this._dependsOnSUDSNS.Add(xns);
                }
            }

            internal WsdlGenerator.ArraySchemaType LookupArraySchemaType(string name)
            {
                for (int i = 0; i < this._arraySchemaTypes.Count; i++)
                {
                    WsdlGenerator.ArraySchemaType type = (WsdlGenerator.ArraySchemaType) this._arraySchemaTypes[i];
                    if (type.Name == name)
                    {
                        return type;
                    }
                }
                return null;
            }

            internal WsdlGenerator.PhonySchemaType LookupPhonySchemaType(string name)
            {
                for (int i = 0; i < this._phonySchemaTypes.Count; i++)
                {
                    WsdlGenerator.PhonySchemaType type = (WsdlGenerator.PhonySchemaType) this._phonySchemaTypes[i];
                    if (type.Name == name)
                    {
                        return type;
                    }
                }
                return null;
            }

            internal WsdlGenerator.RealSchemaType LookupRealSchemaType(string name)
            {
                for (int i = 0; i < this._realSchemaTypes.Count; i++)
                {
                    WsdlGenerator.RealSchemaType type = (WsdlGenerator.RealSchemaType) this._realSchemaTypes[i];
                    if (type.FullRefName == name)
                    {
                        return type;
                    }
                }
                return null;
            }

            internal WsdlGenerator.XMLNamespace LookupSchemaNamespace(string ns, Assembly assem)
            {
                for (int i = 0; i < this._dependsOnSchemaNS.Count; i++)
                {
                    WsdlGenerator.XMLNamespace namespace2 = (WsdlGenerator.XMLNamespace) this._dependsOnSchemaNS[i];
                    if ((namespace2.Name == ns) && (namespace2.Assem == assem))
                    {
                        return namespace2;
                    }
                }
                return null;
            }

            internal Type LookupSchemaType(string name)
            {
                Type type = null;
                WsdlGenerator.RealSchemaType realSchemaType = this.LookupRealSchemaType(name);
                if (realSchemaType != null)
                {
                    type = realSchemaType.Type;
                }
                WsdlGenerator.SimpleSchemaType simpleSchemaType = this.LookupSimpleSchemaType(name);
                if (simpleSchemaType != null)
                {
                    type = simpleSchemaType.Type;
                }
                WsdlGenerator.ArraySchemaType arraySchemaType = this.LookupArraySchemaType(name);
                if (arraySchemaType != null)
                {
                    type = arraySchemaType.Type;
                }
                return type;
            }

            internal WsdlGenerator.SimpleSchemaType LookupSimpleSchemaType(string name)
            {
                for (int i = 0; i < this._simpleSchemaTypes.Count; i++)
                {
                    WsdlGenerator.SimpleSchemaType type = (WsdlGenerator.SimpleSchemaType) this._simpleSchemaTypes[i];
                    if (type.FullRefName == name)
                    {
                        return type;
                    }
                }
                return null;
            }

            private WsdlGenerator.XMLNamespace LookupSUDSNamespace(string ns, Assembly assem)
            {
                for (int i = 0; i < this._dependsOnSUDSNS.Count; i++)
                {
                    WsdlGenerator.XMLNamespace namespace2 = (WsdlGenerator.XMLNamespace) this._dependsOnSUDSNS[i];
                    if ((namespace2.Name == ns) && (namespace2.Assem == assem))
                    {
                        return namespace2;
                    }
                }
                return null;
            }

            internal void PrintDependsOnWsdl(TextWriter textWriter, StringBuilder sb, string indent, Hashtable usedNames)
            {
                if (this._dependsOnSchemaNS.Count > 0)
                {
                    for (int i = 0; i < this._dependsOnSchemaNS.Count; i++)
                    {
                        WsdlGenerator.XMLNamespace namespace2 = (WsdlGenerator.XMLNamespace) this._dependsOnSchemaNS[i];
                        if (!usedNames.ContainsKey(namespace2.Prefix))
                        {
                            usedNames[namespace2.Prefix] = null;
                            sb.Length = 0;
                            sb.Append(indent);
                            sb.Append("xmlns:");
                            sb.Append(namespace2.Prefix);
                            sb.Append("='");
                            sb.Append(namespace2.Namespace);
                            sb.Append("'");
                            textWriter.WriteLine(sb);
                        }
                    }
                }
            }

            internal void PrintMessageWsdl(TextWriter textWriter, StringBuilder sb, string indent, ArrayList refNames)
            {
                for (int i = 0; i < this._realSUDSTypes.Count; i++)
                {
                    ((WsdlGenerator.RealSchemaType) this._realSUDSTypes[i]).PrintMessageWsdl(textWriter, sb, indent, refNames);
                }
                if ((this._realSUDSTypes.Count == 0) && (this._realSchemaTypes.Count > 0))
                {
                    ((WsdlGenerator.RealSchemaType) this._realSchemaTypes[0]).PrintMessageWsdl(textWriter, sb, indent, new ArrayList());
                }
            }

            internal void PrintSchemaWsdl(TextWriter textWriter, StringBuilder sb, string indent)
            {
                bool flag = false;
                if (((this._simpleSchemaTypes.Count > 0) || (this._realSchemaTypes.Count > 0)) || (this._arraySchemaTypes.Count > 0))
                {
                    flag = true;
                }
                if (flag)
                {
                    string indentStr = WsdlGenerator.IndentP(indent);
                    string str2 = WsdlGenerator.IndentP(indentStr);
                    WsdlGenerator.IndentP(WsdlGenerator.IndentP(str2));
                    sb.Length = 0;
                    sb.Append(indent);
                    sb.Append("<schema ");
                    sb.Append("targetNamespace='");
                    sb.Append(this.Namespace);
                    sb.Append("'");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str2);
                    sb.Append("xmlns='");
                    sb.Append(SudsConverter.GetXsdVersion(this._generator._xsdVersion));
                    sb.Append("'");
                    textWriter.WriteLine(sb);
                    sb.Length = 0;
                    sb.Append(str2);
                    sb.Append("elementFormDefault='unqualified' attributeFormDefault='unqualified'>");
                    textWriter.WriteLine(sb);
                    foreach (WsdlGenerator.XMLNamespace namespace2 in this._xnsImports)
                    {
                        sb.Length = 0;
                        sb.Append(indentStr);
                        sb.Append("<import namespace='");
                        sb.Append(namespace2.Namespace);
                        sb.Append("'/>");
                        textWriter.WriteLine(sb);
                    }
                    for (int i = 0; i < this._simpleSchemaTypes.Count; i++)
                    {
                        ((WsdlGenerator.SimpleSchemaType) this._simpleSchemaTypes[i]).PrintSchemaType(textWriter, sb, indentStr, false);
                    }
                    for (int j = 0; j < this._realSchemaTypes.Count; j++)
                    {
                        WsdlGenerator.RealSchemaType type2 = (WsdlGenerator.RealSchemaType) this._realSchemaTypes[j];
                        if (!type2.Type.IsInterface && !type2.IsSUDSType)
                        {
                            type2.PrintSchemaType(textWriter, sb, indentStr, false);
                        }
                    }
                    for (int k = 0; k < this._arraySchemaTypes.Count; k++)
                    {
                        ((WsdlGenerator.ArraySchemaType) this._arraySchemaTypes[k]).PrintSchemaType(textWriter, sb, indentStr, false);
                    }
                    sb.Length = 0;
                    sb.Append(indent);
                    sb.Append("</schema>");
                    textWriter.WriteLine(sb);
                }
            }

            internal void Resolve()
            {
                StringBuilder sb = new StringBuilder(0x100);
                for (int i = 0; i < this._realSchemaTypes.Count; i++)
                {
                    ((WsdlGenerator.RealSchemaType) this._realSchemaTypes[i]).Resolve(sb);
                }
            }

            internal Assembly Assem
            {
                get
                {
                    return this._assem;
                }
            }

            internal WsdlGenerator Generator
            {
                get
                {
                    return this._generator;
                }
            }

            internal bool IsClassesPrinted
            {
                get
                {
                    return this._bClassesPrinted;
                }
                set
                {
                    this._bClassesPrinted = value;
                }
            }

            internal bool IsInteropType
            {
                get
                {
                    return this._bInteropType;
                }
            }

            internal string Name
            {
                get
                {
                    return this._name;
                }
            }

            internal string Namespace
            {
                get
                {
                    return this._namespace;
                }
            }

            internal string Prefix
            {
                get
                {
                    return this._prefix;
                }
            }
        }
    }
}

