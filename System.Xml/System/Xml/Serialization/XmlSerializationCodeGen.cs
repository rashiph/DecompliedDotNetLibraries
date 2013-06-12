namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class XmlSerializationCodeGen
    {
        private string access;
        private string className;
        private Hashtable generatedMethods = new Hashtable();
        private Hashtable methodNames = new Hashtable();
        private int nextMethodNumber;
        private TypeDesc qnameTypeDesc;
        private ReflectionAwareCodeGen raCodeGen;
        private TypeMapping[] referencedMethods;
        private int references;
        private TypeScope[] scopes;
        private TypeDesc stringTypeDesc;
        private IndentedWriter writer;

        internal XmlSerializationCodeGen(IndentedWriter writer, TypeScope[] scopes, string access, string className)
        {
            this.writer = writer;
            this.scopes = scopes;
            if (scopes.Length > 0)
            {
                this.stringTypeDesc = scopes[0].GetTypeDesc(typeof(string));
                this.qnameTypeDesc = scopes[0].GetTypeDesc(typeof(XmlQualifiedName));
            }
            this.raCodeGen = new ReflectionAwareCodeGen(writer);
            this.className = className;
            this.access = access;
        }

        private TypeMapping[] EnsureArrayIndex(TypeMapping[] a, int index)
        {
            if (a == null)
            {
                return new TypeMapping[0x20];
            }
            if (index < a.Length)
            {
                return a;
            }
            TypeMapping[] destinationArray = new TypeMapping[a.Length + 0x20];
            Array.Copy(a, destinationArray, index);
            return destinationArray;
        }

        internal string GenerateBaseSerializer(string baseSerializer, string readerClass, string writerClass, CodeIdentifiers classes)
        {
            baseSerializer = CodeIdentifier.MakeValid(baseSerializer);
            baseSerializer = classes.AddUnique(baseSerializer, baseSerializer);
            this.writer.WriteLine();
            this.writer.Write("public abstract class ");
            this.writer.Write(CodeIdentifier.GetCSharpName(baseSerializer));
            this.writer.Write(" : ");
            this.writer.Write(typeof(XmlSerializer).FullName);
            this.writer.WriteLine(" {");
            this.writer.Indent++;
            this.writer.Write("protected override ");
            this.writer.Write(typeof(XmlSerializationReader).FullName);
            this.writer.WriteLine(" CreateReader() {");
            this.writer.Indent++;
            this.writer.Write("return new ");
            this.writer.Write(readerClass);
            this.writer.WriteLine("();");
            this.writer.Indent--;
            this.writer.WriteLine("}");
            this.writer.Write("protected override ");
            this.writer.Write(typeof(XmlSerializationWriter).FullName);
            this.writer.WriteLine(" CreateWriter() {");
            this.writer.Indent++;
            this.writer.Write("return new ");
            this.writer.Write(writerClass);
            this.writer.WriteLine("();");
            this.writer.Indent--;
            this.writer.WriteLine("}");
            this.writer.Indent--;
            this.writer.WriteLine("}");
            return baseSerializer;
        }

        private void GenerateGetSerializer(Hashtable serializers, XmlMapping[] xmlMappings)
        {
            this.writer.Write("public override ");
            this.writer.Write(typeof(XmlSerializer).FullName);
            this.writer.Write(" GetSerializer(");
            this.writer.Write(typeof(Type).FullName);
            this.writer.WriteLine(" type) {");
            this.writer.Indent++;
            for (int i = 0; i < xmlMappings.Length; i++)
            {
                if (xmlMappings[i] is XmlTypeMapping)
                {
                    Type type = xmlMappings[i].Accessor.Mapping.TypeDesc.Type;
                    if (((type != null) && (type.IsPublic || type.IsNestedPublic)) && ((!DynamicAssemblies.IsTypeDynamic(type) && !type.IsGenericType) && (!type.ContainsGenericParameters || !DynamicAssemblies.IsTypeDynamic(type.GetGenericArguments()))))
                    {
                        this.writer.Write("if (type == typeof(");
                        this.writer.Write(CodeIdentifier.GetCSharpName(type));
                        this.writer.Write(")) return new ");
                        this.writer.Write((string) serializers[xmlMappings[i].Key]);
                        this.writer.WriteLine("();");
                    }
                }
            }
            this.writer.WriteLine("return null;");
            this.writer.Indent--;
            this.writer.WriteLine("}");
        }

        internal void GenerateHashtableGetBegin(string privateName, string publicName)
        {
            this.writer.Write(typeof(Hashtable).FullName);
            this.writer.Write(" ");
            this.writer.Write(privateName);
            this.writer.WriteLine(" = null;");
            this.writer.Write("public override ");
            this.writer.Write(typeof(Hashtable).FullName);
            this.writer.Write(" ");
            this.writer.Write(publicName);
            this.writer.WriteLine(" {");
            this.writer.Indent++;
            this.writer.WriteLine("get {");
            this.writer.Indent++;
            this.writer.Write("if (");
            this.writer.Write(privateName);
            this.writer.WriteLine(" == null) {");
            this.writer.Indent++;
            this.writer.Write(typeof(Hashtable).FullName);
            this.writer.Write(" _tmp = new ");
            this.writer.Write(typeof(Hashtable).FullName);
            this.writer.WriteLine("();");
        }

        internal void GenerateHashtableGetEnd(string privateName)
        {
            this.writer.Write("if (");
            this.writer.Write(privateName);
            this.writer.Write(" == null) ");
            this.writer.Write(privateName);
            this.writer.WriteLine(" = _tmp;");
            this.writer.Indent--;
            this.writer.WriteLine("}");
            this.writer.Write("return ");
            this.writer.Write(privateName);
            this.writer.WriteLine(";");
            this.writer.Indent--;
            this.writer.WriteLine("}");
            this.writer.Indent--;
            this.writer.WriteLine("}");
        }

        internal virtual void GenerateMethod(TypeMapping mapping)
        {
        }

        internal void GeneratePublicMethods(string privateName, string publicName, string[] methods, XmlMapping[] xmlMappings)
        {
            this.GenerateHashtableGetBegin(privateName, publicName);
            if (((methods != null) && (methods.Length != 0)) && ((xmlMappings != null) && (xmlMappings.Length == methods.Length)))
            {
                for (int i = 0; i < methods.Length; i++)
                {
                    if (methods[i] != null)
                    {
                        this.writer.Write("_tmp[");
                        this.WriteQuotedCSharpString(xmlMappings[i].Key);
                        this.writer.Write("] = ");
                        this.WriteQuotedCSharpString(methods[i]);
                        this.writer.WriteLine(";");
                    }
                }
            }
            this.GenerateHashtableGetEnd(privateName);
        }

        internal void GenerateReferencedMethods()
        {
            while (this.references > 0)
            {
                TypeMapping mapping = this.referencedMethods[--this.references];
                this.GenerateMethod(mapping);
            }
        }

        internal void GenerateSerializerContract(string className, XmlMapping[] xmlMappings, Type[] types, string readerType, string[] readMethods, string writerType, string[] writerMethods, Hashtable serializers)
        {
            this.writer.WriteLine();
            this.writer.Write("public class XmlSerializerContract : global::");
            this.writer.Write(typeof(XmlSerializerImplementation).FullName);
            this.writer.WriteLine(" {");
            this.writer.Indent++;
            this.writer.Write("public override global::");
            this.writer.Write(typeof(XmlSerializationReader).FullName);
            this.writer.Write(" Reader { get { return new ");
            this.writer.Write(readerType);
            this.writer.WriteLine("(); } }");
            this.writer.Write("public override global::");
            this.writer.Write(typeof(XmlSerializationWriter).FullName);
            this.writer.Write(" Writer { get { return new ");
            this.writer.Write(writerType);
            this.writer.WriteLine("(); } }");
            this.GeneratePublicMethods("readMethods", "ReadMethods", readMethods, xmlMappings);
            this.GeneratePublicMethods("writeMethods", "WriteMethods", writerMethods, xmlMappings);
            this.GenerateTypedSerializers(serializers);
            this.GenerateSupportedTypes(types);
            this.GenerateGetSerializer(serializers, xmlMappings);
            this.writer.Indent--;
            this.writer.WriteLine("}");
        }

        internal void GenerateSupportedTypes(Type[] types)
        {
            this.writer.Write("public override ");
            this.writer.Write(typeof(bool).FullName);
            this.writer.Write(" CanSerialize(");
            this.writer.Write(typeof(Type).FullName);
            this.writer.WriteLine(" type) {");
            this.writer.Indent++;
            Hashtable hashtable = new Hashtable();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (((type != null) && (type.IsPublic || type.IsNestedPublic)) && ((((hashtable[type] == null) && !DynamicAssemblies.IsTypeDynamic(type)) && !type.IsGenericType) && (!type.ContainsGenericParameters || !DynamicAssemblies.IsTypeDynamic(type.GetGenericArguments()))))
                {
                    hashtable[type] = type;
                    this.writer.Write("if (type == typeof(");
                    this.writer.Write(CodeIdentifier.GetCSharpName(type));
                    this.writer.WriteLine(")) return true;");
                }
            }
            this.writer.WriteLine("return false;");
            this.writer.Indent--;
            this.writer.WriteLine("}");
        }

        internal string GenerateTypedSerializer(string readMethod, string writeMethod, XmlMapping mapping, CodeIdentifiers classes, string baseSerializer, string readerClass, string writerClass)
        {
            string name = CodeIdentifier.MakeValid(Accessor.UnescapeName(mapping.Accessor.Mapping.TypeDesc.Name));
            name = classes.AddUnique(name + "Serializer", mapping);
            this.writer.WriteLine();
            this.writer.Write("public sealed class ");
            this.writer.Write(CodeIdentifier.GetCSharpName(name));
            this.writer.Write(" : ");
            this.writer.Write(baseSerializer);
            this.writer.WriteLine(" {");
            this.writer.Indent++;
            this.writer.WriteLine();
            this.writer.Write("public override ");
            this.writer.Write(typeof(bool).FullName);
            this.writer.Write(" CanDeserialize(");
            this.writer.Write(typeof(XmlReader).FullName);
            this.writer.WriteLine(" xmlReader) {");
            this.writer.Indent++;
            if (mapping.Accessor.Any)
            {
                this.writer.WriteLine("return true;");
            }
            else
            {
                this.writer.Write("return xmlReader.IsStartElement(");
                this.WriteQuotedCSharpString(mapping.Accessor.Name);
                this.writer.Write(", ");
                this.WriteQuotedCSharpString(mapping.Accessor.Namespace);
                this.writer.WriteLine(");");
            }
            this.writer.Indent--;
            this.writer.WriteLine("}");
            if (writeMethod != null)
            {
                this.writer.WriteLine();
                this.writer.Write("protected override void Serialize(object objectToSerialize, ");
                this.writer.Write(typeof(XmlSerializationWriter).FullName);
                this.writer.WriteLine(" writer) {");
                this.writer.Indent++;
                this.writer.Write("((");
                this.writer.Write(writerClass);
                this.writer.Write(")writer).");
                this.writer.Write(writeMethod);
                this.writer.Write("(");
                if (mapping is XmlMembersMapping)
                {
                    this.writer.Write("(object[])");
                }
                this.writer.WriteLine("objectToSerialize);");
                this.writer.Indent--;
                this.writer.WriteLine("}");
            }
            if (readMethod != null)
            {
                this.writer.WriteLine();
                this.writer.Write("protected override object Deserialize(");
                this.writer.Write(typeof(XmlSerializationReader).FullName);
                this.writer.WriteLine(" reader) {");
                this.writer.Indent++;
                this.writer.Write("return ((");
                this.writer.Write(readerClass);
                this.writer.Write(")reader).");
                this.writer.Write(readMethod);
                this.writer.WriteLine("();");
                this.writer.Indent--;
                this.writer.WriteLine("}");
            }
            this.writer.Indent--;
            this.writer.WriteLine("}");
            return name;
        }

        private void GenerateTypedSerializers(Hashtable serializers)
        {
            string privateName = "typedSerializers";
            this.GenerateHashtableGetBegin(privateName, "TypedSerializers");
            foreach (string str2 in serializers.Keys)
            {
                this.writer.Write("_tmp.Add(");
                this.WriteQuotedCSharpString(str2);
                this.writer.Write(", new ");
                this.writer.Write((string) serializers[str2]);
                this.writer.WriteLine("());");
            }
            this.GenerateHashtableGetEnd("typedSerializers");
        }

        internal static bool IsWildcard(SpecialMapping mapping)
        {
            if (mapping is SerializableMapping)
            {
                return ((SerializableMapping) mapping).IsAny;
            }
            return mapping.TypeDesc.CanBeElementValue;
        }

        internal string ReferenceMapping(TypeMapping mapping)
        {
            if (!mapping.IsSoap && (this.generatedMethods[mapping] == null))
            {
                this.referencedMethods = this.EnsureArrayIndex(this.referencedMethods, this.references);
                this.referencedMethods[this.references++] = mapping;
            }
            return (string) this.methodNames[mapping];
        }

        internal void WriteQuotedCSharpString(string value)
        {
            this.raCodeGen.WriteQuotedCSharpString(value);
        }

        internal string Access
        {
            get
            {
                return this.access;
            }
        }

        internal string ClassName
        {
            get
            {
                return this.className;
            }
        }

        internal Hashtable GeneratedMethods
        {
            get
            {
                return this.generatedMethods;
            }
        }

        internal Hashtable MethodNames
        {
            get
            {
                return this.methodNames;
            }
        }

        internal int NextMethodNumber
        {
            get
            {
                return this.nextMethodNumber;
            }
            set
            {
                this.nextMethodNumber = value;
            }
        }

        internal TypeDesc QnameTypeDesc
        {
            get
            {
                return this.qnameTypeDesc;
            }
        }

        internal ReflectionAwareCodeGen RaCodeGen
        {
            get
            {
                return this.raCodeGen;
            }
        }

        internal TypeScope[] Scopes
        {
            get
            {
                return this.scopes;
            }
        }

        internal TypeDesc StringTypeDesc
        {
            get
            {
                return this.stringTypeDesc;
            }
        }

        internal IndentedWriter Writer
        {
            get
            {
                return this.writer;
            }
        }
    }
}

