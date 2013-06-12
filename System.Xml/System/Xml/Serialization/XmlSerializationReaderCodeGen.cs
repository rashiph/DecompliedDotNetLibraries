namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Xml;
    using System.Xml.Schema;

    internal class XmlSerializationReaderCodeGen : XmlSerializationCodeGen
    {
        private Hashtable createMethods;
        private Hashtable enums;
        private Hashtable idNames;
        private int nextCreateMethodNumber;
        private int nextIdNumber;
        private int nextWhileLoopIndex;

        internal XmlSerializationReaderCodeGen(IndentedWriter writer, TypeScope[] scopes, string access, string className) : base(writer, scopes, access, className)
        {
            this.idNames = new Hashtable();
            this.createMethods = new Hashtable();
        }

        private string ExpectedElements(Member[] members)
        {
            if (this.IsSequence(members))
            {
                return "null";
            }
            string str = string.Empty;
            bool flag = true;
            for (int i = 0; i < members.Length; i++)
            {
                Member member = members[i];
                if (((member.Mapping.Xmlns == null) && !member.Mapping.Ignore) && (!member.Mapping.IsText && !member.Mapping.IsAttribute))
                {
                    foreach (ElementAccessor accessor in member.Mapping.Elements)
                    {
                        string str2 = (accessor.Form == XmlSchemaForm.Qualified) ? accessor.Namespace : "";
                        if (!accessor.Any || ((accessor.Name != null) && (accessor.Name.Length != 0)))
                        {
                            if (!flag)
                            {
                                str = str + ", ";
                            }
                            str = str + str2 + ":" + accessor.Name;
                            flag = false;
                        }
                    }
                }
            }
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            ReflectionAwareCodeGen.WriteQuotedCSharpString(new IndentedWriter(writer, true), str);
            return writer.ToString();
        }

        internal void GenerateBegin()
        {
            base.Writer.Write(base.Access);
            base.Writer.Write(" class ");
            base.Writer.Write(base.ClassName);
            base.Writer.Write(" : ");
            base.Writer.Write(typeof(XmlSerializationReader).FullName);
            base.Writer.WriteLine(" {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            foreach (TypeScope scope in base.Scopes)
            {
                foreach (TypeMapping mapping in scope.TypeMappings)
                {
                    if (((mapping is StructMapping) || (mapping is EnumMapping)) || (mapping is NullableMapping))
                    {
                        base.MethodNames.Add(mapping, this.NextMethodName(mapping.TypeDesc.Name));
                    }
                }
                base.RaCodeGen.WriteReflectionInit(scope);
            }
            foreach (TypeScope scope2 in base.Scopes)
            {
                foreach (TypeMapping mapping2 in scope2.TypeMappings)
                {
                    if (mapping2.IsSoap)
                    {
                        if (mapping2 is StructMapping)
                        {
                            this.WriteStructMethod((StructMapping) mapping2);
                        }
                        else if (mapping2 is EnumMapping)
                        {
                            this.WriteEnumMethod((EnumMapping) mapping2);
                        }
                        else if (mapping2 is NullableMapping)
                        {
                            this.WriteNullableMethod((NullableMapping) mapping2);
                        }
                    }
                }
            }
        }

        internal string GenerateElement(XmlMapping xmlMapping)
        {
            if (!xmlMapping.IsReadable)
            {
                return null;
            }
            if (!xmlMapping.GenerateSerializer)
            {
                throw new ArgumentException(Res.GetString("XmlInternalError"), "xmlMapping");
            }
            if (xmlMapping is XmlTypeMapping)
            {
                return this.GenerateTypeElement((XmlTypeMapping) xmlMapping);
            }
            if (!(xmlMapping is XmlMembersMapping))
            {
                throw new ArgumentException(Res.GetString("XmlInternalError"), "xmlMapping");
            }
            return this.GenerateMembersElement((XmlMembersMapping) xmlMapping);
        }

        private string GenerateEncodedMembersElement(XmlMembersMapping xmlMembersMapping)
        {
            ElementAccessor accessor = xmlMembersMapping.Accessor;
            MembersMapping mapping = (MembersMapping) accessor.Mapping;
            MemberMapping[] members = mapping.Members;
            bool hasWrapperElement = mapping.HasWrapperElement;
            bool writeAccessors = mapping.WriteAccessors;
            string s = this.NextMethodName(accessor.Name);
            base.Writer.WriteLine();
            base.Writer.Write("public object[] ");
            base.Writer.Write(s);
            base.Writer.WriteLine("() {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine("Reader.MoveToContent();");
            base.Writer.Write("object[] p = new object[");
            int length = members.Length;
            base.Writer.Write(length.ToString(CultureInfo.InvariantCulture));
            base.Writer.WriteLine("];");
            this.InitializeValueTypes("p", members);
            if (hasWrapperElement)
            {
                this.WriteReadNonRoots();
                if (mapping.ValidateRpcWrapperElement)
                {
                    base.Writer.Write("if (!");
                    this.WriteXmlNodeEqual("Reader", accessor.Name, (accessor.Form == XmlSchemaForm.Qualified) ? accessor.Namespace : "");
                    base.Writer.WriteLine(") throw CreateUnknownNodeException();");
                }
                base.Writer.WriteLine("bool isEmptyWrapper = Reader.IsEmptyElement;");
                base.Writer.WriteLine("Reader.ReadStartElement();");
            }
            Member[] memberArray = new Member[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                MemberMapping mapping2 = members[i];
                string str2 = "p[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                string arraySource = str2;
                if (mapping2.Xmlns != null)
                {
                    arraySource = "((" + mapping2.TypeDesc.CSharpName + ")" + str2 + ")";
                }
                Member member = new Member(this, str2, arraySource, "a", i, mapping2);
                if (!mapping2.IsSequence)
                {
                    member.ParamsReadSource = "paramsRead[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                }
                memberArray[i] = member;
                if (mapping2.CheckSpecified == SpecifiedAccessor.ReadWrite)
                {
                    string str4 = mapping2.Name + "Specified";
                    for (int j = 0; j < members.Length; j++)
                    {
                        if (members[j].Name == str4)
                        {
                            member.CheckSpecifiedSource = "p[" + j.ToString(CultureInfo.InvariantCulture) + "]";
                            break;
                        }
                    }
                }
            }
            string fixupMethodName = "fixup_" + s;
            bool flag3 = this.WriteMemberFixupBegin(memberArray, fixupMethodName, "p");
            if ((memberArray.Length > 0) && memberArray[0].Mapping.IsReturnValue)
            {
                base.Writer.WriteLine("IsReturnValue = true;");
            }
            string source = (!hasWrapperElement && !writeAccessors) ? "hrefList" : null;
            if (source != null)
            {
                this.WriteInitCheckTypeHrefList(source);
            }
            this.WriteParamsRead(members.Length);
            int loopIndex = this.WriteWhileNotLoopStart();
            IndentedWriter writer2 = base.Writer;
            writer2.Indent++;
            string elementElseString = (source == null) ? "UnknownNode((object)p);" : "if (Reader.GetAttribute(\"id\", null) != null) { ReadReferencedElement(); } else { UnknownNode((object)p); }";
            this.WriteMemberElements(memberArray, elementElseString, "UnknownNode((object)p);", null, null, source);
            base.Writer.WriteLine("Reader.MoveToContent();");
            this.WriteWhileLoopEnd(loopIndex);
            if (hasWrapperElement)
            {
                base.Writer.WriteLine("if (!isEmptyWrapper) ReadEndElement();");
            }
            if (source != null)
            {
                this.WriteHandleHrefList(memberArray, source);
            }
            base.Writer.WriteLine("ReadReferencedElements();");
            base.Writer.WriteLine("return p;");
            IndentedWriter writer3 = base.Writer;
            writer3.Indent--;
            base.Writer.WriteLine("}");
            if (flag3)
            {
                this.WriteFixupMethod(fixupMethodName, memberArray, "object[]", false, false, "p");
            }
            return s;
        }

        internal void GenerateEnd()
        {
            this.GenerateEnd(new string[0], new XmlMapping[0], new Type[0]);
        }

        internal void GenerateEnd(string[] methods, XmlMapping[] xmlMappings, Type[] types)
        {
            base.GenerateReferencedMethods();
            this.GenerateInitCallbacksMethod();
            foreach (CreateCollectionInfo info in this.createMethods.Values)
            {
                this.WriteCreateCollectionMethod(info);
            }
            base.Writer.WriteLine();
            foreach (string str in this.idNames.Values)
            {
                base.Writer.Write("string ");
                base.Writer.Write(str);
                base.Writer.WriteLine(";");
            }
            base.Writer.WriteLine();
            base.Writer.WriteLine("protected override void InitIDs() {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            foreach (string str2 in this.idNames.Keys)
            {
                string s = (string) this.idNames[str2];
                base.Writer.Write(s);
                base.Writer.Write(" = Reader.NameTable.Add(");
                base.WriteQuotedCSharpString(str2);
                base.Writer.WriteLine(");");
            }
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
            IndentedWriter writer3 = base.Writer;
            writer3.Indent--;
            base.Writer.WriteLine("}");
        }

        private void GenerateInitCallbacksMethod()
        {
            base.Writer.WriteLine();
            base.Writer.WriteLine("protected override void InitCallbacks() {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            string s = this.NextMethodName("Array");
            bool flag = false;
            foreach (TypeScope scope in base.Scopes)
            {
                foreach (TypeMapping mapping in scope.TypeMappings)
                {
                    if ((mapping.IsSoap && (((mapping is StructMapping) || (mapping is EnumMapping)) || ((mapping is ArrayMapping) || (mapping is NullableMapping)))) && !mapping.TypeDesc.IsRoot)
                    {
                        string str2;
                        if (mapping is ArrayMapping)
                        {
                            str2 = s;
                            flag = true;
                        }
                        else
                        {
                            str2 = (string) base.MethodNames[mapping];
                        }
                        base.Writer.Write("AddReadCallback(");
                        this.WriteID(mapping.TypeName);
                        base.Writer.Write(", ");
                        this.WriteID(mapping.Namespace);
                        base.Writer.Write(", ");
                        base.Writer.Write(base.RaCodeGen.GetStringForTypeof(mapping.TypeDesc.CSharpName, mapping.TypeDesc.UseReflection));
                        base.Writer.Write(", new ");
                        base.Writer.Write(typeof(XmlSerializationReadCallback).FullName);
                        base.Writer.Write("(this.");
                        base.Writer.Write(str2);
                        base.Writer.WriteLine("));");
                    }
                }
            }
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
            if (flag)
            {
                base.Writer.WriteLine();
                base.Writer.Write("object ");
                base.Writer.Write(s);
                base.Writer.WriteLine("() {");
                IndentedWriter writer3 = base.Writer;
                writer3.Indent++;
                base.Writer.WriteLine("// dummy array method");
                base.Writer.WriteLine("UnknownNode(null);");
                base.Writer.WriteLine("return null;");
                IndentedWriter writer4 = base.Writer;
                writer4.Indent--;
                base.Writer.WriteLine("}");
            }
        }

        private string GenerateLiteralMembersElement(XmlMembersMapping xmlMembersMapping)
        {
            ElementAccessor e = xmlMembersMapping.Accessor;
            MemberMapping[] members = ((MembersMapping) e.Mapping).Members;
            bool hasWrapperElement = ((MembersMapping) e.Mapping).HasWrapperElement;
            string s = this.NextMethodName(e.Name);
            base.Writer.WriteLine();
            base.Writer.Write("public object[] ");
            base.Writer.Write(s);
            base.Writer.WriteLine("() {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine("Reader.MoveToContent();");
            base.Writer.Write("object[] p = new object[");
            int length = members.Length;
            base.Writer.Write(length.ToString(CultureInfo.InvariantCulture));
            base.Writer.WriteLine("];");
            this.InitializeValueTypes("p", members);
            int loopIndex = 0;
            if (hasWrapperElement)
            {
                loopIndex = this.WriteWhileNotLoopStart();
                IndentedWriter writer2 = base.Writer;
                writer2.Indent++;
                this.WriteIsStartTag(e.Name, (e.Form == XmlSchemaForm.Qualified) ? e.Namespace : "");
            }
            Member anyText = null;
            Member anyElement = null;
            Member anyAttribute = null;
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            ArrayList list3 = new ArrayList();
            for (int i = 0; i < members.Length; i++)
            {
                MemberMapping member = members[i];
                string source = "p[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                string arraySource = source;
                if (member.Xmlns != null)
                {
                    arraySource = "((" + member.TypeDesc.CSharpName + ")" + source + ")";
                }
                string choiceIdentifierSource = this.GetChoiceIdentifierSource(members, member);
                Member member4 = new Member(this, source, arraySource, "a", i, member, choiceIdentifierSource);
                Member member5 = new Member(this, source, null, "a", i, member, choiceIdentifierSource);
                if (!member.IsSequence)
                {
                    member4.ParamsReadSource = "paramsRead[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                }
                if (member.CheckSpecified == SpecifiedAccessor.ReadWrite)
                {
                    string str5 = member.Name + "Specified";
                    for (int j = 0; j < members.Length; j++)
                    {
                        if (members[j].Name == str5)
                        {
                            member4.CheckSpecifiedSource = "p[" + j.ToString(CultureInfo.InvariantCulture) + "]";
                            break;
                        }
                    }
                }
                bool flag2 = false;
                if (member.Text != null)
                {
                    anyText = member5;
                }
                if ((member.Attribute != null) && member.Attribute.Any)
                {
                    anyAttribute = member5;
                }
                if ((member.Attribute != null) || (member.Xmlns != null))
                {
                    list3.Add(member4);
                }
                else if (member.Text != null)
                {
                    list2.Add(member4);
                }
                if (!member.IsSequence)
                {
                    for (int k = 0; k < member.Elements.Length; k++)
                    {
                        if (member.Elements[k].Any && (member.Elements[k].Name.Length == 0))
                        {
                            anyElement = member5;
                            if ((member.Attribute == null) && (member.Text == null))
                            {
                                list2.Add(member5);
                            }
                            flag2 = true;
                            break;
                        }
                    }
                }
                if (((member.Attribute != null) || (member.Text != null)) || flag2)
                {
                    list.Add(member5);
                }
                else if (member.TypeDesc.IsArrayLike && ((member.Elements.Length != 1) || !(member.Elements[0].Mapping is ArrayMapping)))
                {
                    list.Add(member5);
                    list2.Add(member5);
                }
                else
                {
                    if (member.TypeDesc.IsArrayLike && !member.TypeDesc.IsArray)
                    {
                        member4.ParamsReadSource = null;
                    }
                    list.Add(member4);
                }
            }
            Member[] memberArray = (Member[]) list.ToArray(typeof(Member));
            Member[] memberArray2 = (Member[]) list2.ToArray(typeof(Member));
            if ((memberArray.Length > 0) && memberArray[0].Mapping.IsReturnValue)
            {
                base.Writer.WriteLine("IsReturnValue = true;");
            }
            this.WriteParamsRead(members.Length);
            if (list3.Count > 0)
            {
                Member[] memberArray3 = (Member[]) list3.ToArray(typeof(Member));
                this.WriteMemberBegin(memberArray3);
                this.WriteAttributes(memberArray3, anyAttribute, "UnknownNode", "(object)p");
                this.WriteMemberEnd(memberArray3);
                base.Writer.WriteLine("Reader.MoveToElement();");
            }
            this.WriteMemberBegin(memberArray2);
            if (hasWrapperElement)
            {
                base.Writer.WriteLine("if (Reader.IsEmptyElement) { Reader.Skip(); Reader.MoveToContent(); continue; }");
                base.Writer.WriteLine("Reader.ReadStartElement();");
            }
            if (this.IsSequence(memberArray))
            {
                base.Writer.WriteLine("int state = 0;");
            }
            int num5 = this.WriteWhileNotLoopStart();
            IndentedWriter writer3 = base.Writer;
            writer3.Indent++;
            string elementElseString = "UnknownNode((object)p, " + this.ExpectedElements(memberArray) + ");";
            this.WriteMemberElements(memberArray, elementElseString, elementElseString, anyElement, anyText, null);
            base.Writer.WriteLine("Reader.MoveToContent();");
            this.WriteWhileLoopEnd(num5);
            this.WriteMemberEnd(memberArray2);
            if (hasWrapperElement)
            {
                base.Writer.WriteLine("ReadEndElement();");
                IndentedWriter writer4 = base.Writer;
                writer4.Indent--;
                base.Writer.WriteLine("}");
                this.WriteUnknownNode("UnknownNode", "null", e, true);
                base.Writer.WriteLine("Reader.MoveToContent();");
                this.WriteWhileLoopEnd(loopIndex);
            }
            base.Writer.WriteLine("return p;");
            IndentedWriter writer5 = base.Writer;
            writer5.Indent--;
            base.Writer.WriteLine("}");
            return s;
        }

        private string GenerateMembersElement(XmlMembersMapping xmlMembersMapping)
        {
            if (xmlMembersMapping.Accessor.IsSoap)
            {
                return this.GenerateEncodedMembersElement(xmlMembersMapping);
            }
            return this.GenerateLiteralMembersElement(xmlMembersMapping);
        }

        internal override void GenerateMethod(TypeMapping mapping)
        {
            if (!base.GeneratedMethods.Contains(mapping))
            {
                base.GeneratedMethods[mapping] = mapping;
                if (mapping is StructMapping)
                {
                    this.WriteStructMethod((StructMapping) mapping);
                }
                else if (mapping is EnumMapping)
                {
                    this.WriteEnumMethod((EnumMapping) mapping);
                }
                else if (mapping is NullableMapping)
                {
                    this.WriteNullableMethod((NullableMapping) mapping);
                }
            }
        }

        private string GenerateTypeElement(XmlTypeMapping xmlTypeMapping)
        {
            ElementAccessor accessor = xmlTypeMapping.Accessor;
            TypeMapping mapping = accessor.Mapping;
            string s = this.NextMethodName(accessor.Name);
            base.Writer.WriteLine();
            base.Writer.Write("public object ");
            base.Writer.Write(s);
            base.Writer.WriteLine("() {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine("object o = null;");
            MemberMapping mapping2 = new MemberMapping {
                TypeDesc = mapping.TypeDesc,
                Elements = new ElementAccessor[] { accessor }
            };
            Member[] members = new Member[] { new Member(this, "o", "o", "a", 0, mapping2) };
            base.Writer.WriteLine("Reader.MoveToContent();");
            string elseString = "UnknownNode(null, " + this.ExpectedElements(members) + ");";
            this.WriteMemberElements(members, "throw CreateUnknownNodeException();", elseString, accessor.Any ? members[0] : null, null, null);
            if (accessor.IsSoap)
            {
                base.Writer.WriteLine("Referenced(o);");
                base.Writer.WriteLine("ReadReferencedElements();");
            }
            base.Writer.WriteLine("return (object)o;");
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
            return s;
        }

        private string GetArraySource(TypeDesc typeDesc, string arrayName)
        {
            return this.GetArraySource(typeDesc, arrayName, false);
        }

        private string GetArraySource(TypeDesc typeDesc, string arrayName, bool multiRef)
        {
            string str = arrayName;
            string str2 = "c" + str;
            string str3 = "";
            if (multiRef)
            {
                str3 = "soap = (System.Object[])EnsureArrayIndex(soap, " + str2 + "+2, typeof(System.Object)); ";
            }
            bool useReflection = typeDesc.UseReflection;
            if (!typeDesc.IsArray)
            {
                return base.RaCodeGen.GetStringForMethod(arrayName, typeDesc.CSharpName, "Add", useReflection);
            }
            string cSharpName = typeDesc.ArrayElementTypeDesc.CSharpName;
            bool flag2 = typeDesc.ArrayElementTypeDesc.UseReflection;
            string str5 = useReflection ? "" : ("(" + cSharpName + "[])");
            str3 = str3 + str + " = " + str5 + "EnsureArrayIndex(" + str + ", " + str2 + ", " + base.RaCodeGen.GetStringForTypeof(cSharpName, flag2) + ");";
            string str6 = base.RaCodeGen.GetStringForArrayMember(str, str2 + "++", typeDesc);
            if (multiRef)
            {
                str3 = str3 + " soap[1] = " + str + ";";
                str3 = str3 + " if (ReadReference(out soap[" + str2 + "+2])) " + str6 + " = null; else ";
            }
            return (str3 + str6);
        }

        private string GetChoiceIdentifierSource(MemberMapping[] mappings, MemberMapping member)
        {
            if (member.ChoiceIdentifier != null)
            {
                for (int i = 0; i < mappings.Length; i++)
                {
                    if (mappings[i].Name == member.ChoiceIdentifier.MemberName)
                    {
                        return ("p[" + i.ToString(CultureInfo.InvariantCulture) + "]");
                    }
                }
            }
            return null;
        }

        private string GetChoiceIdentifierSource(MemberMapping mapping, string parent, TypeDesc parentTypeDesc)
        {
            if (mapping.ChoiceIdentifier == null)
            {
                return "";
            }
            CodeIdentifier.CheckValidIdentifier(mapping.ChoiceIdentifier.MemberName);
            return base.RaCodeGen.GetStringForMember(parent, mapping.ChoiceIdentifier.MemberName, parentTypeDesc);
        }

        private void InitializeValueTypes(string arrayName, MemberMapping[] mappings)
        {
            for (int i = 0; i < mappings.Length; i++)
            {
                if (mappings[i].TypeDesc.IsValueType)
                {
                    base.Writer.Write(arrayName);
                    base.Writer.Write("[");
                    base.Writer.Write(i.ToString(CultureInfo.InvariantCulture));
                    base.Writer.Write("] = ");
                    if (mappings[i].TypeDesc.IsOptionalValue && mappings[i].TypeDesc.BaseTypeDesc.UseReflection)
                    {
                        base.Writer.Write("null");
                    }
                    else
                    {
                        base.Writer.Write(base.RaCodeGen.GetStringForCreateInstance(mappings[i].TypeDesc.CSharpName, mappings[i].TypeDesc.UseReflection, false, false));
                    }
                    base.Writer.WriteLine(";");
                }
            }
        }

        private bool IsSequence(Member[] members)
        {
            for (int i = 0; i < members.Length; i++)
            {
                if (members[i].Mapping.IsParticle && members[i].Mapping.IsSequence)
                {
                    return true;
                }
            }
            return false;
        }

        private string MakeUnique(EnumMapping mapping, string name)
        {
            string key = name;
            object obj2 = this.Enums[key];
            if (obj2 != null)
            {
                if (obj2 == mapping)
                {
                    return null;
                }
                int num = 0;
                while (obj2 != null)
                {
                    num++;
                    key = name + num.ToString(CultureInfo.InvariantCulture);
                    obj2 = this.Enums[key];
                }
            }
            this.Enums.Add(key, mapping);
            return key;
        }

        private string NextIdName(string name)
        {
            int num2 = ++this.nextIdNumber;
            return ("id" + num2.ToString(CultureInfo.InvariantCulture) + "_" + CodeIdentifier.MakeValidInternal(name));
        }

        private string NextMethodName(string name)
        {
            int num2 = ++base.NextMethodNumber;
            return ("Read" + num2.ToString(CultureInfo.InvariantCulture) + "_" + CodeIdentifier.MakeValidInternal(name));
        }

        private void WriteAddCollectionFixup(TypeDesc typeDesc, bool readOnly, string memberSource, string targetSource)
        {
            base.Writer.WriteLine("// get array of the collection items");
            bool useReflection = typeDesc.UseReflection;
            CreateCollectionInfo info = (CreateCollectionInfo) this.createMethods[typeDesc];
            if (info == null)
            {
                int num2 = ++this.nextCreateMethodNumber;
                info = new CreateCollectionInfo("create" + num2.ToString(CultureInfo.InvariantCulture) + "_" + typeDesc.Name, typeDesc);
                this.createMethods.Add(typeDesc, info);
            }
            base.Writer.Write("if ((object)(");
            base.Writer.Write(memberSource);
            base.Writer.WriteLine(") == null) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            if (readOnly)
            {
                base.Writer.Write("throw CreateReadOnlyCollectionException(");
                base.WriteQuotedCSharpString(typeDesc.CSharpName);
                base.Writer.WriteLine(");");
            }
            else
            {
                base.Writer.Write(memberSource);
                base.Writer.Write(" = ");
                base.Writer.Write(base.RaCodeGen.GetStringForCreateInstance(typeDesc.CSharpName, typeDesc.UseReflection, typeDesc.CannotNew, true));
                base.Writer.WriteLine(";");
            }
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
            base.Writer.Write("CollectionFixup collectionFixup = new CollectionFixup(");
            base.Writer.Write(memberSource);
            base.Writer.Write(", ");
            base.Writer.Write("new ");
            base.Writer.Write(typeof(XmlSerializationCollectionFixupCallback).FullName);
            base.Writer.Write("(this.");
            base.Writer.Write(info.Name);
            base.Writer.Write("), ");
            base.Writer.Write(targetSource);
            base.Writer.WriteLine(");");
            base.Writer.WriteLine("AddFixup(collectionFixup);");
        }

        private void WriteArray(string source, string arrayName, ArrayMapping arrayMapping, bool readOnly, bool isNullable, int fixupIndex)
        {
            if (arrayMapping.IsSoap)
            {
                base.Writer.Write("object rre = ");
                base.Writer.Write((fixupIndex >= 0) ? "ReadReferencingElement" : "ReadReferencedElement");
                base.Writer.Write("(");
                this.WriteID(arrayMapping.TypeName);
                base.Writer.Write(", ");
                this.WriteID(arrayMapping.Namespace);
                if (fixupIndex >= 0)
                {
                    base.Writer.Write(", ");
                    base.Writer.Write("out fixup.Ids[");
                    base.Writer.Write(fixupIndex.ToString(CultureInfo.InvariantCulture));
                    base.Writer.Write("]");
                }
                base.Writer.WriteLine(");");
                TypeDesc typeDesc = arrayMapping.TypeDesc;
                if (typeDesc.IsEnumerable || typeDesc.IsCollection)
                {
                    base.Writer.WriteLine("if (rre != null) {");
                    IndentedWriter writer = base.Writer;
                    writer.Indent++;
                    this.WriteAddCollectionFixup(typeDesc, readOnly, source, "rre");
                    IndentedWriter writer2 = base.Writer;
                    writer2.Indent--;
                    base.Writer.WriteLine("}");
                }
                else
                {
                    base.Writer.WriteLine("try {");
                    IndentedWriter writer3 = base.Writer;
                    writer3.Indent++;
                    this.WriteSourceBeginTyped(source, arrayMapping.TypeDesc);
                    base.Writer.Write("rre");
                    this.WriteSourceEnd(source);
                    base.Writer.WriteLine(";");
                    this.WriteCatchCastException(arrayMapping.TypeDesc, "rre", null);
                }
            }
            else
            {
                base.Writer.WriteLine("if (!ReadNull()) {");
                IndentedWriter writer4 = base.Writer;
                writer4.Indent++;
                MemberMapping mapping = new MemberMapping {
                    Elements = arrayMapping.Elements,
                    TypeDesc = arrayMapping.TypeDesc,
                    ReadOnly = readOnly
                };
                Member member = new Member(this, source, arrayName, 0, mapping, false) {
                    IsNullable = false
                };
                Member[] members = new Member[] { member };
                this.WriteMemberBegin(members);
                if (readOnly)
                {
                    base.Writer.Write("if (((object)(");
                    base.Writer.Write(member.ArrayName);
                    base.Writer.Write(") == null) || ");
                }
                else
                {
                    base.Writer.Write("if (");
                }
                base.Writer.WriteLine("(Reader.IsEmptyElement)) {");
                IndentedWriter writer5 = base.Writer;
                writer5.Indent++;
                base.Writer.WriteLine("Reader.Skip();");
                IndentedWriter writer6 = base.Writer;
                writer6.Indent--;
                base.Writer.WriteLine("}");
                base.Writer.WriteLine("else {");
                IndentedWriter writer7 = base.Writer;
                writer7.Indent++;
                base.Writer.WriteLine("Reader.ReadStartElement();");
                int loopIndex = this.WriteWhileNotLoopStart();
                IndentedWriter writer8 = base.Writer;
                writer8.Indent++;
                string elementElseString = "UnknownNode(null, " + this.ExpectedElements(members) + ");";
                this.WriteMemberElements(members, elementElseString, elementElseString, null, null, null);
                base.Writer.WriteLine("Reader.MoveToContent();");
                this.WriteWhileLoopEnd(loopIndex);
                IndentedWriter writer9 = base.Writer;
                writer9.Indent--;
                base.Writer.WriteLine("ReadEndElement();");
                base.Writer.WriteLine("}");
                this.WriteMemberEnd(members, false);
                IndentedWriter writer10 = base.Writer;
                writer10.Indent--;
                base.Writer.WriteLine("}");
                if (isNullable)
                {
                    base.Writer.WriteLine("else {");
                    IndentedWriter writer11 = base.Writer;
                    writer11.Indent++;
                    member.IsNullable = true;
                    this.WriteMemberBegin(members);
                    this.WriteMemberEnd(members);
                    IndentedWriter writer12 = base.Writer;
                    writer12.Indent--;
                    base.Writer.WriteLine("}");
                }
            }
        }

        private void WriteArrayLocalDecl(string typeName, string variableName, string initValue, TypeDesc arrayTypeDesc)
        {
            base.RaCodeGen.WriteArrayLocalDecl(typeName, variableName, initValue, arrayTypeDesc);
        }

        private void WriteAttribute(Member member)
        {
            AttributeAccessor attribute = member.Mapping.Attribute;
            if (attribute.Mapping is SpecialMapping)
            {
                SpecialMapping mapping = (SpecialMapping) attribute.Mapping;
                if (mapping.TypeDesc.Kind != TypeKind.Attribute)
                {
                    if (!mapping.TypeDesc.CanBeAttributeValue)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInternalError"));
                    }
                    base.Writer.Write("if (attr is ");
                    base.Writer.Write(typeof(XmlAttribute).FullName);
                    base.Writer.WriteLine(") {");
                    IndentedWriter writer = base.Writer;
                    writer.Indent++;
                    this.WriteSourceBegin(member.ArraySource);
                    base.Writer.Write("(");
                    base.Writer.Write(typeof(XmlAttribute).FullName);
                    base.Writer.Write(")attr");
                    this.WriteSourceEnd(member.ArraySource);
                    base.Writer.WriteLine(";");
                    IndentedWriter writer2 = base.Writer;
                    writer2.Indent--;
                    base.Writer.WriteLine("}");
                }
                else
                {
                    this.WriteSourceBegin(member.ArraySource);
                    base.Writer.Write("attr");
                    this.WriteSourceEnd(member.ArraySource);
                    base.Writer.WriteLine(";");
                }
            }
            else if (attribute.IsList)
            {
                base.Writer.WriteLine("string listValues = Reader.Value;");
                base.Writer.WriteLine("string[] vals = listValues.Split(null);");
                base.Writer.WriteLine("for (int i = 0; i < vals.Length; i++) {");
                IndentedWriter writer3 = base.Writer;
                writer3.Indent++;
                string arraySource = this.GetArraySource(member.Mapping.TypeDesc, member.ArrayName);
                this.WriteSourceBegin(arraySource);
                this.WritePrimitive(attribute.Mapping, "vals[i]");
                this.WriteSourceEnd(arraySource);
                base.Writer.WriteLine(";");
                IndentedWriter writer4 = base.Writer;
                writer4.Indent--;
                base.Writer.WriteLine("}");
            }
            else
            {
                this.WriteSourceBegin(member.ArraySource);
                this.WritePrimitive(attribute.Mapping, attribute.IsList ? "vals[i]" : "Reader.Value");
                this.WriteSourceEnd(member.ArraySource);
                base.Writer.WriteLine(";");
            }
            if (((member.Mapping.CheckSpecified == SpecifiedAccessor.ReadWrite) && (member.CheckSpecifiedSource != null)) && (member.CheckSpecifiedSource.Length > 0))
            {
                base.Writer.Write(member.CheckSpecifiedSource);
                base.Writer.WriteLine(" = true;");
            }
            if (member.ParamsReadSource != null)
            {
                base.Writer.Write(member.ParamsReadSource);
                base.Writer.WriteLine(" = true;");
            }
        }

        private void WriteAttributes(Member[] members, Member anyAttribute, string elseCall, string firstParam)
        {
            int num = 0;
            Member member = null;
            ArrayList list = new ArrayList();
            base.Writer.WriteLine("while (Reader.MoveToNextAttribute()) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            for (int i = 0; i < members.Length; i++)
            {
                Member member2 = members[i];
                if (member2.Mapping.Xmlns != null)
                {
                    member = member2;
                }
                else if (!member2.Mapping.Ignore)
                {
                    AttributeAccessor attribute = member2.Mapping.Attribute;
                    if ((attribute != null) && !attribute.Any)
                    {
                        list.Add(attribute);
                        if (num++ > 0)
                        {
                            base.Writer.Write("else ");
                        }
                        base.Writer.Write("if (");
                        if (member2.ParamsReadSource != null)
                        {
                            base.Writer.Write("!");
                            base.Writer.Write(member2.ParamsReadSource);
                            base.Writer.Write(" && ");
                        }
                        if (attribute.IsSpecialXmlNamespace)
                        {
                            this.WriteXmlNodeEqual("Reader", attribute.Name, "http://www.w3.org/XML/1998/namespace");
                        }
                        else
                        {
                            this.WriteXmlNodeEqual("Reader", attribute.Name, (attribute.Form == XmlSchemaForm.Qualified) ? attribute.Namespace : "");
                        }
                        base.Writer.WriteLine(") {");
                        IndentedWriter writer2 = base.Writer;
                        writer2.Indent++;
                        this.WriteAttribute(member2);
                        IndentedWriter writer3 = base.Writer;
                        writer3.Indent--;
                        base.Writer.WriteLine("}");
                    }
                }
            }
            if (num > 0)
            {
                base.Writer.Write("else ");
            }
            if (member != null)
            {
                base.Writer.WriteLine("if (IsXmlnsAttribute(Reader.Name)) {");
                IndentedWriter writer4 = base.Writer;
                writer4.Indent++;
                base.Writer.Write("if (");
                base.Writer.Write(member.Source);
                base.Writer.Write(" == null) ");
                base.Writer.Write(member.Source);
                base.Writer.Write(" = new ");
                base.Writer.Write(member.Mapping.TypeDesc.CSharpName);
                base.Writer.WriteLine("();");
                base.Writer.Write("((" + member.Mapping.TypeDesc.CSharpName + ")" + member.ArraySource + ")");
                base.Writer.WriteLine(".Add(Reader.Name.Length == 5 ? \"\" : Reader.LocalName, Reader.Value);");
                IndentedWriter writer5 = base.Writer;
                writer5.Indent--;
                base.Writer.WriteLine("}");
                base.Writer.WriteLine("else {");
                IndentedWriter writer6 = base.Writer;
                writer6.Indent++;
            }
            else
            {
                base.Writer.WriteLine("if (!IsXmlnsAttribute(Reader.Name)) {");
                IndentedWriter writer7 = base.Writer;
                writer7.Indent++;
            }
            if (anyAttribute != null)
            {
                base.Writer.Write(typeof(XmlAttribute).FullName);
                base.Writer.Write(" attr = ");
                base.Writer.Write("(");
                base.Writer.Write(typeof(XmlAttribute).FullName);
                base.Writer.WriteLine(") Document.ReadNode(Reader);");
                base.Writer.WriteLine("ParseWsdlArrayType(attr);");
                this.WriteAttribute(anyAttribute);
            }
            else
            {
                base.Writer.Write(elseCall);
                base.Writer.Write("(");
                base.Writer.Write(firstParam);
                if (list.Count > 0)
                {
                    base.Writer.Write(", ");
                    string str = "";
                    for (int j = 0; j < list.Count; j++)
                    {
                        AttributeAccessor accessor2 = (AttributeAccessor) list[j];
                        if (j > 0)
                        {
                            str = str + ", ";
                        }
                        str = str + (accessor2.IsSpecialXmlNamespace ? "http://www.w3.org/XML/1998/namespace" : (((accessor2.Form == XmlSchemaForm.Qualified) ? accessor2.Namespace : "") + ":" + accessor2.Name));
                    }
                    base.WriteQuotedCSharpString(str);
                }
                base.Writer.WriteLine(");");
            }
            IndentedWriter writer8 = base.Writer;
            writer8.Indent--;
            base.Writer.WriteLine("}");
            IndentedWriter writer9 = base.Writer;
            writer9.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteBooleanValue(bool value)
        {
            base.Writer.Write(value ? "true" : "false");
        }

        private void WriteCatchCastException(TypeDesc typeDesc, string source, string id)
        {
            this.WriteCatchException(typeof(InvalidCastException));
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.Write("throw CreateInvalidCastException(");
            base.Writer.Write(base.RaCodeGen.GetStringForTypeof(typeDesc.CSharpName, typeDesc.UseReflection));
            base.Writer.Write(", ");
            base.Writer.Write(source);
            if (id == null)
            {
                base.Writer.WriteLine(", null);");
            }
            else
            {
                base.Writer.Write(", (string)");
                base.Writer.Write(id);
                base.Writer.WriteLine(");");
            }
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteCatchException(Type exceptionType)
        {
            IndentedWriter writer = base.Writer;
            writer.Indent--;
            base.Writer.WriteLine("}");
            base.Writer.Write("catch (");
            base.Writer.Write(exceptionType.FullName);
            base.Writer.WriteLine(") {");
        }

        private void WriteCreateCollection(TypeDesc td, string source)
        {
            bool useReflection = td.UseReflection;
            string s = ((td.ArrayElementTypeDesc == null) ? "object" : td.ArrayElementTypeDesc.CSharpName) + "[]";
            bool flag2 = (td.ArrayElementTypeDesc != null) && td.ArrayElementTypeDesc.UseReflection;
            if (flag2)
            {
                s = typeof(Array).FullName;
            }
            base.Writer.Write(s);
            base.Writer.Write(" ");
            base.Writer.Write("ci =");
            base.Writer.Write("(" + s + ")");
            base.Writer.Write(source);
            base.Writer.WriteLine(";");
            base.Writer.WriteLine("for (int i = 0; i < ci.Length; i++) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.Write(base.RaCodeGen.GetStringForMethod("c", td.CSharpName, "Add", useReflection));
            if (!flag2)
            {
                base.Writer.Write("ci[i]");
            }
            else
            {
                base.Writer.Write(base.RaCodeGen.GetReflectionVariable(typeof(Array).FullName, "0") + "[ci , i]");
            }
            if (useReflection)
            {
                base.Writer.WriteLine("}");
            }
            base.Writer.WriteLine(");");
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteCreateCollectionMethod(CreateCollectionInfo c)
        {
            base.Writer.Write("void ");
            base.Writer.Write(c.Name);
            base.Writer.WriteLine("(object collection, object collectionItems) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine("if (collectionItems == null) return;");
            base.Writer.WriteLine("if (collection == null) return;");
            TypeDesc typeDesc = c.TypeDesc;
            bool useReflection = typeDesc.UseReflection;
            string cSharpName = typeDesc.CSharpName;
            this.WriteLocalDecl(cSharpName, "c", "collection", useReflection);
            this.WriteCreateCollection(typeDesc, "collectionItems");
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteCreateInstance(string escapedName, string source, bool useReflection, bool ctorInaccessible)
        {
            base.RaCodeGen.WriteCreateInstance(escapedName, source, useReflection, ctorInaccessible);
        }

        private void WriteCreateMapping(TypeMapping mapping, string local)
        {
            string cSharpName = mapping.TypeDesc.CSharpName;
            bool useReflection = mapping.TypeDesc.UseReflection;
            bool cannotNew = mapping.TypeDesc.CannotNew;
            base.Writer.Write(useReflection ? "object" : cSharpName);
            base.Writer.Write(" ");
            base.Writer.Write(local);
            base.Writer.WriteLine(";");
            if (cannotNew)
            {
                base.Writer.WriteLine("try {");
                IndentedWriter writer = base.Writer;
                writer.Indent++;
            }
            base.Writer.Write(local);
            base.Writer.Write(" = ");
            base.Writer.Write(base.RaCodeGen.GetStringForCreateInstance(cSharpName, useReflection, mapping.TypeDesc.CannotNew, true));
            base.Writer.WriteLine(";");
            if (cannotNew)
            {
                this.WriteCatchException(typeof(MissingMethodException));
                IndentedWriter writer2 = base.Writer;
                writer2.Indent++;
                base.Writer.Write("throw CreateInaccessibleConstructorException(");
                base.WriteQuotedCSharpString(cSharpName);
                base.Writer.WriteLine(");");
                this.WriteCatchException(typeof(SecurityException));
                IndentedWriter writer3 = base.Writer;
                writer3.Indent++;
                base.Writer.Write("throw CreateCtorHasSecurityException(");
                base.WriteQuotedCSharpString(cSharpName);
                base.Writer.WriteLine(");");
                IndentedWriter writer4 = base.Writer;
                writer4.Indent--;
                base.Writer.WriteLine("}");
            }
        }

        private void WriteDerivedSerializable(SerializableMapping head, SerializableMapping mapping, string source, bool isWrappedAny)
        {
            if (mapping != null)
            {
                for (SerializableMapping mapping2 = mapping.DerivedMappings; mapping2 != null; mapping2 = mapping2.NextDerivedMapping)
                {
                    base.Writer.Write("else if (tser == null");
                    base.Writer.Write(" || ");
                    this.WriteQNameEqual("tser", mapping2.XsiType.Name, mapping2.XsiType.Namespace);
                    base.Writer.WriteLine(") {");
                    IndentedWriter writer = base.Writer;
                    writer.Indent++;
                    if (mapping2.Type != null)
                    {
                        if (head.Type.IsAssignableFrom(mapping2.Type))
                        {
                            this.WriteSourceBeginTyped(source, head.TypeDesc);
                            base.Writer.Write("ReadSerializable(( ");
                            base.Writer.Write(typeof(IXmlSerializable).FullName);
                            base.Writer.Write(")");
                            base.Writer.Write(base.RaCodeGen.GetStringForCreateInstance(mapping2.TypeDesc.CSharpName, mapping2.TypeDesc.UseReflection, mapping2.TypeDesc.CannotNew, false));
                            if (isWrappedAny)
                            {
                                base.Writer.WriteLine(", true");
                            }
                            base.Writer.Write(")");
                            this.WriteSourceEnd(source);
                            base.Writer.WriteLine(";");
                        }
                        else
                        {
                            base.Writer.Write("throw CreateBadDerivationException(");
                            base.WriteQuotedCSharpString(mapping2.XsiType.Name);
                            base.Writer.Write(", ");
                            base.WriteQuotedCSharpString(mapping2.XsiType.Namespace);
                            base.Writer.Write(", ");
                            base.WriteQuotedCSharpString(head.XsiType.Name);
                            base.Writer.Write(", ");
                            base.WriteQuotedCSharpString(head.XsiType.Namespace);
                            base.Writer.Write(", ");
                            base.WriteQuotedCSharpString(mapping2.Type.FullName);
                            base.Writer.Write(", ");
                            base.WriteQuotedCSharpString(head.Type.FullName);
                            base.Writer.WriteLine(");");
                        }
                    }
                    else
                    {
                        base.Writer.WriteLine("// missing real mapping for " + mapping2.XsiType);
                        base.Writer.Write("throw CreateMissingIXmlSerializableType(");
                        base.WriteQuotedCSharpString(mapping2.XsiType.Name);
                        base.Writer.Write(", ");
                        base.WriteQuotedCSharpString(mapping2.XsiType.Namespace);
                        base.Writer.Write(", ");
                        base.WriteQuotedCSharpString(head.Type.FullName);
                        base.Writer.WriteLine(");");
                    }
                    IndentedWriter writer2 = base.Writer;
                    writer2.Indent--;
                    base.Writer.WriteLine("}");
                    this.WriteDerivedSerializable(head, mapping2, source, isWrappedAny);
                }
            }
        }

        private void WriteDerivedTypes(StructMapping mapping, bool isTypedReturn, string returnTypeName)
        {
            for (StructMapping mapping2 = mapping.DerivedMappings; mapping2 != null; mapping2 = mapping2.NextDerivedMapping)
            {
                base.Writer.Write("else if (");
                this.WriteQNameEqual("xsiType", mapping2.TypeName, mapping2.Namespace);
                base.Writer.WriteLine(")");
                IndentedWriter writer = base.Writer;
                writer.Indent++;
                string s = base.ReferenceMapping(mapping2);
                base.Writer.Write("return ");
                if (mapping2.TypeDesc.UseReflection && isTypedReturn)
                {
                    base.Writer.Write("(" + returnTypeName + ")");
                }
                base.Writer.Write(s);
                base.Writer.Write("(");
                if (mapping2.TypeDesc.IsNullable)
                {
                    base.Writer.Write("isNullable, ");
                }
                base.Writer.WriteLine("false);");
                IndentedWriter writer2 = base.Writer;
                writer2.Indent--;
                this.WriteDerivedTypes(mapping2, isTypedReturn, returnTypeName);
            }
        }

        private void WriteElement(string source, string arrayName, string choiceSource, ElementAccessor element, ChoiceIdentifierAccessor choice, string checkSpecified, bool checkForNull, bool readOnly, int fixupIndex, int elementIndex)
        {
            if ((checkSpecified != null) && (checkSpecified.Length > 0))
            {
                base.Writer.Write(checkSpecified);
                base.Writer.WriteLine(" = true;");
            }
            if (element.Mapping is ArrayMapping)
            {
                this.WriteArray(source, arrayName, (ArrayMapping) element.Mapping, readOnly, element.IsNullable, fixupIndex);
            }
            else if (element.Mapping is NullableMapping)
            {
                string s = base.ReferenceMapping(element.Mapping);
                this.WriteSourceBegin(source);
                base.Writer.Write(s);
                base.Writer.Write("(true)");
                this.WriteSourceEnd(source);
                base.Writer.WriteLine(";");
            }
            else if (!element.Mapping.IsSoap && (element.Mapping is PrimitiveMapping))
            {
                if (element.IsNullable)
                {
                    base.Writer.WriteLine("if (ReadNull()) {");
                    IndentedWriter writer1 = base.Writer;
                    writer1.Indent++;
                    this.WriteSourceBegin(source);
                    if (element.Mapping.TypeDesc.IsValueType)
                    {
                        base.Writer.Write(base.RaCodeGen.GetStringForCreateInstance(element.Mapping.TypeDesc.CSharpName, element.Mapping.TypeDesc.UseReflection, false, false));
                    }
                    else
                    {
                        base.Writer.Write("null");
                    }
                    this.WriteSourceEnd(source);
                    base.Writer.WriteLine(";");
                    IndentedWriter writer2 = base.Writer;
                    writer2.Indent--;
                    base.Writer.WriteLine("}");
                    base.Writer.Write("else ");
                }
                if (((element.Default != null) && (element.Default != DBNull.Value)) && element.Mapping.TypeDesc.IsValueType)
                {
                    base.Writer.WriteLine("if (Reader.IsEmptyElement) {");
                    IndentedWriter writer3 = base.Writer;
                    writer3.Indent++;
                    base.Writer.WriteLine("Reader.Skip();");
                    IndentedWriter writer4 = base.Writer;
                    writer4.Indent--;
                    base.Writer.WriteLine("}");
                    base.Writer.WriteLine("else {");
                }
                else
                {
                    base.Writer.WriteLine("{");
                }
                IndentedWriter writer = base.Writer;
                writer.Indent++;
                this.WriteSourceBegin(source);
                if (element.Mapping.TypeDesc == base.QnameTypeDesc)
                {
                    base.Writer.Write("ReadElementQualifiedName()");
                }
                else
                {
                    string str2;
                    string str5;
                    if (((str5 = element.Mapping.TypeDesc.FormatterName) != null) && ((str5 == "ByteArrayBase64") || (str5 == "ByteArrayHex")))
                    {
                        str2 = "false";
                    }
                    else
                    {
                        str2 = "Reader.ReadElementString()";
                    }
                    this.WritePrimitive(element.Mapping, str2);
                }
                this.WriteSourceEnd(source);
                base.Writer.WriteLine(";");
                IndentedWriter writer6 = base.Writer;
                writer6.Indent--;
                base.Writer.WriteLine("}");
            }
            else if ((element.Mapping is StructMapping) || (element.Mapping.IsSoap && (element.Mapping is PrimitiveMapping)))
            {
                TypeMapping mapping = element.Mapping;
                if (mapping.IsSoap)
                {
                    base.Writer.Write("object rre = ");
                    base.Writer.Write((fixupIndex >= 0) ? "ReadReferencingElement" : "ReadReferencedElement");
                    base.Writer.Write("(");
                    this.WriteID(mapping.TypeName);
                    base.Writer.Write(", ");
                    this.WriteID(mapping.Namespace);
                    if (fixupIndex >= 0)
                    {
                        base.Writer.Write(", out fixup.Ids[");
                        base.Writer.Write(fixupIndex.ToString(CultureInfo.InvariantCulture));
                        base.Writer.Write("]");
                    }
                    base.Writer.Write(")");
                    this.WriteSourceEnd(source);
                    base.Writer.WriteLine(";");
                    if (mapping.TypeDesc.IsValueType)
                    {
                        base.Writer.WriteLine("if (rre != null) {");
                        IndentedWriter writer7 = base.Writer;
                        writer7.Indent++;
                    }
                    base.Writer.WriteLine("try {");
                    IndentedWriter writer8 = base.Writer;
                    writer8.Indent++;
                    this.WriteSourceBeginTyped(source, mapping.TypeDesc);
                    base.Writer.Write("rre");
                    this.WriteSourceEnd(source);
                    base.Writer.WriteLine(";");
                    this.WriteCatchCastException(mapping.TypeDesc, "rre", null);
                    base.Writer.Write("Referenced(");
                    base.Writer.Write(source);
                    base.Writer.WriteLine(");");
                    if (mapping.TypeDesc.IsValueType)
                    {
                        IndentedWriter writer9 = base.Writer;
                        writer9.Indent--;
                        base.Writer.WriteLine("}");
                    }
                }
                else
                {
                    string str3 = base.ReferenceMapping(mapping);
                    if (checkForNull)
                    {
                        base.Writer.Write("if ((object)(");
                        base.Writer.Write(arrayName);
                        base.Writer.Write(") == null) Reader.Skip(); else ");
                    }
                    this.WriteSourceBegin(source);
                    base.Writer.Write(str3);
                    base.Writer.Write("(");
                    if (mapping.TypeDesc.IsNullable)
                    {
                        this.WriteBooleanValue(element.IsNullable);
                        base.Writer.Write(", ");
                    }
                    base.Writer.Write("true");
                    base.Writer.Write(")");
                    this.WriteSourceEnd(source);
                    base.Writer.WriteLine(";");
                }
            }
            else
            {
                if (!(element.Mapping is SpecialMapping))
                {
                    throw new InvalidOperationException(Res.GetString("XmlInternalError"));
                }
                SpecialMapping mapping2 = (SpecialMapping) element.Mapping;
                switch (mapping2.TypeDesc.Kind)
                {
                    case TypeKind.Node:
                    {
                        bool flag = mapping2.TypeDesc.FullName == typeof(XmlDocument).FullName;
                        this.WriteSourceBeginTyped(source, mapping2.TypeDesc);
                        base.Writer.Write(flag ? "ReadXmlDocument(" : "ReadXmlNode(");
                        base.Writer.Write(element.Any ? "false" : "true");
                        base.Writer.Write(")");
                        this.WriteSourceEnd(source);
                        base.Writer.WriteLine(";");
                        goto Label_08BC;
                    }
                    case TypeKind.Serializable:
                    {
                        SerializableMapping mapping3 = (SerializableMapping) element.Mapping;
                        if (mapping3.DerivedMappings != null)
                        {
                            base.Writer.Write(typeof(XmlQualifiedName).FullName);
                            base.Writer.WriteLine(" tser = GetXsiType();");
                            base.Writer.Write("if (tser == null");
                            base.Writer.Write(" || ");
                            this.WriteQNameEqual("tser", mapping3.XsiType.Name, mapping3.XsiType.Namespace);
                            base.Writer.WriteLine(") {");
                            IndentedWriter writer10 = base.Writer;
                            writer10.Indent++;
                        }
                        this.WriteSourceBeginTyped(source, mapping3.TypeDesc);
                        base.Writer.Write("ReadSerializable(( ");
                        base.Writer.Write(typeof(IXmlSerializable).FullName);
                        base.Writer.Write(")");
                        base.Writer.Write(base.RaCodeGen.GetStringForCreateInstance(mapping3.TypeDesc.CSharpName, mapping3.TypeDesc.UseReflection, mapping3.TypeDesc.CannotNew, false));
                        bool isWrappedAny = !element.Any && XmlSerializationCodeGen.IsWildcard(mapping3);
                        if (isWrappedAny)
                        {
                            base.Writer.WriteLine(", true");
                        }
                        base.Writer.Write(")");
                        this.WriteSourceEnd(source);
                        base.Writer.WriteLine(";");
                        if (mapping3.DerivedMappings != null)
                        {
                            IndentedWriter writer11 = base.Writer;
                            writer11.Indent--;
                            base.Writer.WriteLine("}");
                            this.WriteDerivedSerializable(mapping3, mapping3, source, isWrappedAny);
                            this.WriteUnknownNode("UnknownNode", "null", null, true);
                        }
                        goto Label_08BC;
                    }
                }
                throw new InvalidOperationException(Res.GetString("XmlInternalError"));
            }
        Label_08BC:
            if (choice != null)
            {
                string cSharpName = choice.Mapping.TypeDesc.CSharpName;
                base.Writer.Write(choiceSource);
                base.Writer.Write(" = ");
                CodeIdentifier.CheckValidIdentifier(choice.MemberIds[elementIndex]);
                base.Writer.Write(base.RaCodeGen.GetStringForEnumMember(cSharpName, choice.MemberIds[elementIndex], choice.Mapping.TypeDesc.UseReflection));
                base.Writer.WriteLine(";");
            }
        }

        private void WriteEncodedStructMethod(StructMapping structMapping)
        {
            if (!structMapping.TypeDesc.IsRoot)
            {
                Member[] memberArray;
                bool flag;
                string str2;
                bool useReflection = structMapping.TypeDesc.UseReflection;
                string s = (string) base.MethodNames[structMapping];
                base.Writer.WriteLine();
                base.Writer.Write("object");
                base.Writer.Write(" ");
                base.Writer.Write(s);
                base.Writer.Write("(");
                base.Writer.WriteLine(") {");
                IndentedWriter writer = base.Writer;
                writer.Indent++;
                if (structMapping.TypeDesc.IsAbstract)
                {
                    base.Writer.Write("throw CreateAbstractTypeException(");
                    base.WriteQuotedCSharpString(structMapping.TypeName);
                    base.Writer.Write(", ");
                    base.WriteQuotedCSharpString(structMapping.Namespace);
                    base.Writer.WriteLine(");");
                    memberArray = new Member[0];
                    flag = false;
                    str2 = null;
                }
                else
                {
                    this.WriteCreateMapping(structMapping, "o");
                    MemberMapping[] allMembers = TypeScope.GetAllMembers(structMapping);
                    memberArray = new Member[allMembers.Length];
                    for (int i = 0; i < allMembers.Length; i++)
                    {
                        MemberMapping mapping = allMembers[i];
                        CodeIdentifier.CheckValidIdentifier(mapping.Name);
                        string source = base.RaCodeGen.GetStringForMember("o", mapping.Name, structMapping.TypeDesc);
                        Member member = new Member(this, source, source, "a", i, mapping, this.GetChoiceIdentifierSource(mapping, "o", structMapping.TypeDesc));
                        if (mapping.CheckSpecified == SpecifiedAccessor.ReadWrite)
                        {
                            member.CheckSpecifiedSource = base.RaCodeGen.GetStringForMember("o", mapping.Name + "Specified", structMapping.TypeDesc);
                        }
                        if (!mapping.IsSequence)
                        {
                            member.ParamsReadSource = "paramsRead[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                        }
                        memberArray[i] = member;
                    }
                    str2 = "fixup_" + s;
                    flag = this.WriteMemberFixupBegin(memberArray, str2, "o");
                    this.WriteParamsRead(allMembers.Length);
                    this.WriteAttributes(memberArray, null, "UnknownNode", "(object)o");
                    base.Writer.WriteLine("Reader.MoveToElement();");
                    base.Writer.WriteLine("if (Reader.IsEmptyElement) { Reader.Skip(); return o; }");
                    base.Writer.WriteLine("Reader.ReadStartElement();");
                    int loopIndex = this.WriteWhileNotLoopStart();
                    IndentedWriter writer2 = base.Writer;
                    writer2.Indent++;
                    this.WriteMemberElements(memberArray, "UnknownNode((object)o);", "UnknownNode((object)o);", null, null, null);
                    base.Writer.WriteLine("Reader.MoveToContent();");
                    this.WriteWhileLoopEnd(loopIndex);
                    base.Writer.WriteLine("ReadEndElement();");
                    base.Writer.WriteLine("return o;");
                }
                IndentedWriter writer3 = base.Writer;
                writer3.Indent--;
                base.Writer.WriteLine("}");
                if (flag)
                {
                    this.WriteFixupMethod(str2, memberArray, structMapping.TypeDesc.CSharpName, structMapping.TypeDesc.UseReflection, true, "o");
                }
            }
        }

        private void WriteEnumAndArrayTypes()
        {
            foreach (TypeScope scope in base.Scopes)
            {
                foreach (Mapping mapping in scope.TypeMappings)
                {
                    if (!mapping.IsSoap)
                    {
                        if (mapping is EnumMapping)
                        {
                            EnumMapping mapping2 = (EnumMapping) mapping;
                            base.Writer.Write("else if (");
                            this.WriteQNameEqual("xsiType", mapping2.TypeName, mapping2.Namespace);
                            base.Writer.WriteLine(") {");
                            IndentedWriter writer = base.Writer;
                            writer.Indent++;
                            base.Writer.WriteLine("Reader.ReadStartElement();");
                            string s = base.ReferenceMapping(mapping2);
                            base.Writer.Write("object e = ");
                            base.Writer.Write(s);
                            base.Writer.WriteLine("(CollapseWhitespace(Reader.ReadString()));");
                            base.Writer.WriteLine("ReadEndElement();");
                            base.Writer.WriteLine("return e;");
                            IndentedWriter writer2 = base.Writer;
                            writer2.Indent--;
                            base.Writer.WriteLine("}");
                        }
                        else if (mapping is ArrayMapping)
                        {
                            ArrayMapping arrayMapping = (ArrayMapping) mapping;
                            if (arrayMapping.TypeDesc.HasDefaultConstructor)
                            {
                                base.Writer.Write("else if (");
                                this.WriteQNameEqual("xsiType", arrayMapping.TypeName, arrayMapping.Namespace);
                                base.Writer.WriteLine(") {");
                                IndentedWriter writer3 = base.Writer;
                                writer3.Indent++;
                                MemberMapping mapping4 = new MemberMapping {
                                    TypeDesc = arrayMapping.TypeDesc,
                                    Elements = arrayMapping.Elements
                                };
                                Member member = new Member(this, "a", "z", 0, mapping4);
                                TypeDesc typeDesc = arrayMapping.TypeDesc;
                                string cSharpName = arrayMapping.TypeDesc.CSharpName;
                                if (typeDesc.UseReflection)
                                {
                                    if (typeDesc.IsArray)
                                    {
                                        base.Writer.Write(typeof(Array).FullName);
                                    }
                                    else
                                    {
                                        base.Writer.Write("object");
                                    }
                                }
                                else
                                {
                                    base.Writer.Write(cSharpName);
                                }
                                base.Writer.Write(" a = ");
                                if (arrayMapping.TypeDesc.IsValueType)
                                {
                                    base.Writer.Write(base.RaCodeGen.GetStringForCreateInstance(cSharpName, typeDesc.UseReflection, false, false));
                                    base.Writer.WriteLine(";");
                                }
                                else
                                {
                                    base.Writer.WriteLine("null;");
                                }
                                this.WriteArray(member.Source, member.ArrayName, arrayMapping, false, false, -1);
                                base.Writer.WriteLine("return a;");
                                IndentedWriter writer4 = base.Writer;
                                writer4.Indent--;
                                base.Writer.WriteLine("}");
                            }
                        }
                    }
                }
            }
        }

        private void WriteEnumMethod(EnumMapping mapping)
        {
            string s = null;
            if (mapping.IsFlags)
            {
                s = this.WriteHashtable(mapping, mapping.TypeDesc.Name);
            }
            string str2 = (string) base.MethodNames[mapping];
            base.Writer.WriteLine();
            bool useReflection = mapping.TypeDesc.UseReflection;
            string cSharpName = mapping.TypeDesc.CSharpName;
            if (mapping.IsSoap)
            {
                base.Writer.Write("object");
                base.Writer.Write(" ");
                base.Writer.Write(str2);
                base.Writer.WriteLine("() {");
                IndentedWriter writer1 = base.Writer;
                writer1.Indent++;
                base.Writer.WriteLine("string s = Reader.ReadElementString();");
            }
            else
            {
                base.Writer.Write(useReflection ? "object" : cSharpName);
                base.Writer.Write(" ");
                base.Writer.Write(str2);
                base.Writer.WriteLine("(string s) {");
                IndentedWriter writer2 = base.Writer;
                writer2.Indent++;
            }
            ConstantMapping[] constants = mapping.Constants;
            if (mapping.IsFlags)
            {
                if (useReflection)
                {
                    base.Writer.Write("return ");
                    base.Writer.Write(typeof(Enum).FullName);
                    base.Writer.Write(".ToObject(");
                    base.Writer.Write(base.RaCodeGen.GetStringForTypeof(cSharpName, useReflection));
                    base.Writer.Write(", ToEnum(s, ");
                    base.Writer.Write(s);
                    base.Writer.Write(", ");
                    base.WriteQuotedCSharpString(cSharpName);
                    base.Writer.WriteLine("));");
                }
                else
                {
                    base.Writer.Write("return (");
                    base.Writer.Write(cSharpName);
                    base.Writer.Write(")ToEnum(s, ");
                    base.Writer.Write(s);
                    base.Writer.Write(", ");
                    base.WriteQuotedCSharpString(cSharpName);
                    base.Writer.WriteLine(");");
                }
            }
            else
            {
                base.Writer.WriteLine("switch (s) {");
                IndentedWriter writer3 = base.Writer;
                writer3.Indent++;
                Hashtable hashtable = new Hashtable();
                for (int i = 0; i < constants.Length; i++)
                {
                    ConstantMapping mapping2 = constants[i];
                    CodeIdentifier.CheckValidIdentifier(mapping2.Name);
                    if (hashtable[mapping2.XmlName] == null)
                    {
                        base.Writer.Write("case ");
                        base.WriteQuotedCSharpString(mapping2.XmlName);
                        base.Writer.Write(": return ");
                        base.Writer.Write(base.RaCodeGen.GetStringForEnumMember(cSharpName, mapping2.Name, useReflection));
                        base.Writer.WriteLine(";");
                        hashtable[mapping2.XmlName] = mapping2.XmlName;
                    }
                }
                base.Writer.Write("default: throw CreateUnknownConstantException(s, ");
                base.Writer.Write(base.RaCodeGen.GetStringForTypeof(cSharpName, useReflection));
                base.Writer.WriteLine(");");
                IndentedWriter writer4 = base.Writer;
                writer4.Indent--;
                base.Writer.WriteLine("}");
            }
            IndentedWriter writer = base.Writer;
            writer.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteFixupMethod(string fixupMethodName, Member[] members, string typeName, bool useReflection, bool typed, string source)
        {
            base.Writer.WriteLine();
            base.Writer.Write("void ");
            base.Writer.Write(fixupMethodName);
            base.Writer.WriteLine("(object objFixup) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine("Fixup fixup = (Fixup)objFixup;");
            this.WriteLocalDecl(typeName, source, "fixup.Source", useReflection);
            base.Writer.WriteLine("string[] ids = fixup.Ids;");
            for (int i = 0; i < members.Length; i++)
            {
                Member member = members[i];
                if (member.MultiRef)
                {
                    string s = member.FixupIndex.ToString(CultureInfo.InvariantCulture);
                    base.Writer.Write("if (ids[");
                    base.Writer.Write(s);
                    base.Writer.WriteLine("] != null) {");
                    IndentedWriter writer2 = base.Writer;
                    writer2.Indent++;
                    string arraySource = member.ArraySource;
                    string targetSource = "GetTarget(ids[" + s + "])";
                    TypeDesc typeDesc = member.Mapping.TypeDesc;
                    if (typeDesc.IsCollection || typeDesc.IsEnumerable)
                    {
                        this.WriteAddCollectionFixup(typeDesc, member.Mapping.ReadOnly, arraySource, targetSource);
                    }
                    else
                    {
                        if (typed)
                        {
                            base.Writer.WriteLine("try {");
                            IndentedWriter writer3 = base.Writer;
                            writer3.Indent++;
                            this.WriteSourceBeginTyped(arraySource, member.Mapping.TypeDesc);
                        }
                        else
                        {
                            this.WriteSourceBegin(arraySource);
                        }
                        base.Writer.Write(targetSource);
                        this.WriteSourceEnd(arraySource);
                        base.Writer.WriteLine(";");
                        if (((member.Mapping.CheckSpecified == SpecifiedAccessor.ReadWrite) && (member.CheckSpecifiedSource != null)) && (member.CheckSpecifiedSource.Length > 0))
                        {
                            base.Writer.Write(member.CheckSpecifiedSource);
                            base.Writer.WriteLine(" = true;");
                        }
                        if (typed)
                        {
                            this.WriteCatchCastException(member.Mapping.TypeDesc, targetSource, "ids[" + s + "]");
                        }
                    }
                    IndentedWriter writer4 = base.Writer;
                    writer4.Indent--;
                    base.Writer.WriteLine("}");
                }
            }
            IndentedWriter writer5 = base.Writer;
            writer5.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteHandleHrefList(Member[] members, string listSource)
        {
            base.Writer.WriteLine("int isObjectIndex = 0;");
            base.Writer.Write("foreach (object obj in ");
            base.Writer.Write(listSource);
            base.Writer.WriteLine(") {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine("bool isReferenced = true;");
            base.Writer.Write("bool isObject = (bool)");
            base.Writer.Write(listSource);
            base.Writer.WriteLine("IsObject[isObjectIndex++];");
            base.Writer.WriteLine("object refObj = isObject ? obj : GetTarget((string)obj);");
            base.Writer.WriteLine("if (refObj == null) continue;");
            base.Writer.Write(typeof(Type).FullName);
            base.Writer.WriteLine(" refObjType = refObj.GetType();");
            base.Writer.WriteLine("string refObjId = null;");
            this.WriteMemberElementsIf(members, null, "isReferenced = false;", "refObj");
            base.Writer.WriteLine("if (isObject && isReferenced) Referenced(refObj); // need to mark this obj as ref'd since we didn't do GetTarget");
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
        }

        private string WriteHashtable(EnumMapping mapping, string typeName)
        {
            CodeIdentifier.CheckValidIdentifier(typeName);
            string name = this.MakeUnique(mapping, typeName + "Values");
            if (name == null)
            {
                return CodeIdentifier.GetCSharpName(typeName);
            }
            string s = this.MakeUnique(mapping, "_" + name);
            name = CodeIdentifier.GetCSharpName(name);
            base.Writer.WriteLine();
            base.Writer.Write(typeof(Hashtable).FullName);
            base.Writer.Write(" ");
            base.Writer.Write(s);
            base.Writer.WriteLine(";");
            base.Writer.WriteLine();
            base.Writer.Write("internal ");
            base.Writer.Write(typeof(Hashtable).FullName);
            base.Writer.Write(" ");
            base.Writer.Write(name);
            base.Writer.WriteLine(" {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine("get {");
            IndentedWriter writer2 = base.Writer;
            writer2.Indent++;
            base.Writer.Write("if ((object)");
            base.Writer.Write(s);
            base.Writer.WriteLine(" == null) {");
            IndentedWriter writer3 = base.Writer;
            writer3.Indent++;
            base.Writer.Write(typeof(Hashtable).FullName);
            base.Writer.Write(" h = new ");
            base.Writer.Write(typeof(Hashtable).FullName);
            base.Writer.WriteLine("();");
            ConstantMapping[] constants = mapping.Constants;
            for (int i = 0; i < constants.Length; i++)
            {
                base.Writer.Write("h.Add(");
                base.WriteQuotedCSharpString(constants[i].XmlName);
                if (!mapping.TypeDesc.UseReflection)
                {
                    base.Writer.Write(", (long)");
                    base.Writer.Write(mapping.TypeDesc.CSharpName);
                    base.Writer.Write(".@");
                    CodeIdentifier.CheckValidIdentifier(constants[i].Name);
                    base.Writer.Write(constants[i].Name);
                }
                else
                {
                    base.Writer.Write(", ");
                    base.Writer.Write(constants[i].Value.ToString(CultureInfo.InvariantCulture) + "L");
                }
                base.Writer.WriteLine(");");
            }
            base.Writer.Write(s);
            base.Writer.WriteLine(" = h;");
            IndentedWriter writer4 = base.Writer;
            writer4.Indent--;
            base.Writer.WriteLine("}");
            base.Writer.Write("return ");
            base.Writer.Write(s);
            base.Writer.WriteLine(";");
            IndentedWriter writer5 = base.Writer;
            writer5.Indent--;
            base.Writer.WriteLine("}");
            IndentedWriter writer6 = base.Writer;
            writer6.Indent--;
            base.Writer.WriteLine("}");
            return name;
        }

        private void WriteID(string name)
        {
            if (name == null)
            {
                name = "";
            }
            string str = (string) this.idNames[name];
            if (str == null)
            {
                str = this.NextIdName(name);
                this.idNames.Add(name, str);
            }
            base.Writer.Write(str);
        }

        private void WriteIfNotSoapRoot(string source)
        {
            base.Writer.Write("if (Reader.GetAttribute(\"root\", \"");
            base.Writer.Write("http://schemas.xmlsoap.org/soap/encoding/");
            base.Writer.WriteLine("\") == \"0\") {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine(source);
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteInitCheckTypeHrefList(string source)
        {
            base.Writer.Write(typeof(ArrayList).FullName);
            base.Writer.Write(" ");
            base.Writer.Write(source);
            base.Writer.Write(" = new ");
            base.Writer.Write(typeof(ArrayList).FullName);
            base.Writer.WriteLine("();");
            base.Writer.Write(typeof(ArrayList).FullName);
            base.Writer.Write(" ");
            base.Writer.Write(source);
            base.Writer.Write("IsObject = new ");
            base.Writer.Write(typeof(ArrayList).FullName);
            base.Writer.WriteLine("();");
        }

        private void WriteIsStartTag(string name, string ns)
        {
            base.Writer.Write("if (Reader.IsStartElement(");
            this.WriteID(name);
            base.Writer.Write(", ");
            this.WriteID(ns);
            base.Writer.WriteLine(")) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
        }

        private void WriteLiteralStructMethod(StructMapping structMapping)
        {
            string s = (string) base.MethodNames[structMapping];
            bool useReflection = structMapping.TypeDesc.UseReflection;
            string str2 = useReflection ? "object" : structMapping.TypeDesc.CSharpName;
            base.Writer.WriteLine();
            base.Writer.Write(str2);
            base.Writer.Write(" ");
            base.Writer.Write(s);
            base.Writer.Write("(");
            if (structMapping.TypeDesc.IsNullable)
            {
                base.Writer.Write("bool isNullable, ");
            }
            base.Writer.WriteLine("bool checkType) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.Write(typeof(XmlQualifiedName).FullName);
            base.Writer.WriteLine(" xsiType = checkType ? GetXsiType() : null;");
            base.Writer.WriteLine("bool isNull = false;");
            if (structMapping.TypeDesc.IsNullable)
            {
                base.Writer.WriteLine("if (isNullable) isNull = ReadNull();");
            }
            base.Writer.WriteLine("if (checkType) {");
            if (structMapping.TypeDesc.IsRoot)
            {
                IndentedWriter writer2 = base.Writer;
                writer2.Indent++;
                base.Writer.WriteLine("if (isNull) {");
                IndentedWriter writer3 = base.Writer;
                writer3.Indent++;
                base.Writer.WriteLine("if (xsiType != null) return (" + str2 + ")ReadTypedNull(xsiType);");
                base.Writer.Write("else return ");
                if (structMapping.TypeDesc.IsValueType)
                {
                    base.Writer.Write(base.RaCodeGen.GetStringForCreateInstance(structMapping.TypeDesc.CSharpName, useReflection, false, false));
                    base.Writer.WriteLine(";");
                }
                else
                {
                    base.Writer.WriteLine("null;");
                }
                IndentedWriter writer4 = base.Writer;
                writer4.Indent--;
                base.Writer.WriteLine("}");
            }
            base.Writer.Write("if (xsiType == null");
            if (!structMapping.TypeDesc.IsRoot)
            {
                base.Writer.Write(" || ");
                this.WriteQNameEqual("xsiType", structMapping.TypeName, structMapping.Namespace);
            }
            base.Writer.WriteLine(") {");
            if (structMapping.TypeDesc.IsRoot)
            {
                IndentedWriter writer5 = base.Writer;
                writer5.Indent++;
                base.Writer.WriteLine("return ReadTypedPrimitive(new System.Xml.XmlQualifiedName(\"anyType\", \"http://www.w3.org/2001/XMLSchema\"));");
                IndentedWriter writer6 = base.Writer;
                writer6.Indent--;
            }
            base.Writer.WriteLine("}");
            this.WriteDerivedTypes(structMapping, !useReflection && !structMapping.TypeDesc.IsRoot, str2);
            if (structMapping.TypeDesc.IsRoot)
            {
                this.WriteEnumAndArrayTypes();
            }
            base.Writer.WriteLine("else");
            IndentedWriter writer7 = base.Writer;
            writer7.Indent++;
            if (structMapping.TypeDesc.IsRoot)
            {
                base.Writer.Write("return ReadTypedPrimitive((");
            }
            else
            {
                base.Writer.Write("throw CreateUnknownTypeException((");
            }
            base.Writer.Write(typeof(XmlQualifiedName).FullName);
            base.Writer.WriteLine(")xsiType);");
            IndentedWriter writer8 = base.Writer;
            writer8.Indent--;
            base.Writer.WriteLine("}");
            if (structMapping.TypeDesc.IsNullable)
            {
                base.Writer.WriteLine("if (isNull) return null;");
            }
            if (structMapping.TypeDesc.IsAbstract)
            {
                base.Writer.Write("throw CreateAbstractTypeException(");
                base.WriteQuotedCSharpString(structMapping.TypeName);
                base.Writer.Write(", ");
                base.WriteQuotedCSharpString(structMapping.Namespace);
                base.Writer.WriteLine(");");
            }
            else
            {
                if ((structMapping.TypeDesc.Type != null) && typeof(XmlSchemaObject).IsAssignableFrom(structMapping.TypeDesc.Type))
                {
                    base.Writer.WriteLine("DecodeName = false;");
                }
                this.WriteCreateMapping(structMapping, "o");
                MemberMapping[] allMembers = TypeScope.GetAllMembers(structMapping);
                Member member = null;
                Member member2 = null;
                Member anyAttribute = null;
                bool flag2 = structMapping.HasExplicitSequence();
                ArrayList list = new ArrayList(allMembers.Length);
                ArrayList list2 = new ArrayList(allMembers.Length);
                ArrayList list3 = new ArrayList(allMembers.Length);
                for (int i = 0; i < allMembers.Length; i++)
                {
                    MemberMapping mapping = allMembers[i];
                    CodeIdentifier.CheckValidIdentifier(mapping.Name);
                    string source = base.RaCodeGen.GetStringForMember("o", mapping.Name, structMapping.TypeDesc);
                    Member member4 = new Member(this, source, "a", i, mapping, this.GetChoiceIdentifierSource(mapping, "o", structMapping.TypeDesc));
                    if (!mapping.IsSequence)
                    {
                        member4.ParamsReadSource = "paramsRead[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                    }
                    member4.IsNullable = mapping.TypeDesc.IsNullable;
                    if (mapping.CheckSpecified == SpecifiedAccessor.ReadWrite)
                    {
                        member4.CheckSpecifiedSource = base.RaCodeGen.GetStringForMember("o", mapping.Name + "Specified", structMapping.TypeDesc);
                    }
                    if (mapping.Text != null)
                    {
                        member = member4;
                    }
                    if ((mapping.Attribute != null) && mapping.Attribute.Any)
                    {
                        anyAttribute = member4;
                    }
                    if (!flag2)
                    {
                        for (int j = 0; j < mapping.Elements.Length; j++)
                        {
                            if (mapping.Elements[j].Any && ((mapping.Elements[j].Name == null) || (mapping.Elements[j].Name.Length == 0)))
                            {
                                member2 = member4;
                                break;
                            }
                        }
                    }
                    else if (mapping.IsParticle && !mapping.IsSequence)
                    {
                        StructMapping mapping2;
                        structMapping.FindDeclaringMapping(mapping, out mapping2, structMapping.TypeName);
                        throw new InvalidOperationException(Res.GetString("XmlSequenceHierarchy", new object[] { structMapping.TypeDesc.FullName, mapping.Name, mapping2.TypeDesc.FullName, "Order" }));
                    }
                    if (((mapping.Attribute == null) && (mapping.Elements.Length == 1)) && (mapping.Elements[0].Mapping is ArrayMapping))
                    {
                        Member member5 = new Member(this, source, source, "a", i, mapping, this.GetChoiceIdentifierSource(mapping, "o", structMapping.TypeDesc)) {
                            CheckSpecifiedSource = member4.CheckSpecifiedSource
                        };
                        list3.Add(member5);
                    }
                    else
                    {
                        list3.Add(member4);
                    }
                    if (mapping.TypeDesc.IsArrayLike)
                    {
                        list.Add(member4);
                        if (mapping.TypeDesc.IsArrayLike && ((mapping.Elements.Length != 1) || !(mapping.Elements[0].Mapping is ArrayMapping)))
                        {
                            member4.ParamsReadSource = null;
                            if ((member4 != member) && (member4 != member2))
                            {
                                list2.Add(member4);
                            }
                        }
                        else if (!mapping.TypeDesc.IsArray)
                        {
                            member4.ParamsReadSource = null;
                        }
                    }
                }
                if (member2 != null)
                {
                    list2.Add(member2);
                }
                if ((member != null) && (member != member2))
                {
                    list2.Add(member);
                }
                Member[] members = (Member[]) list.ToArray(typeof(Member));
                Member[] memberArray2 = (Member[]) list2.ToArray(typeof(Member));
                Member[] memberArray3 = (Member[]) list3.ToArray(typeof(Member));
                this.WriteMemberBegin(members);
                this.WriteParamsRead(allMembers.Length);
                this.WriteAttributes(memberArray3, anyAttribute, "UnknownNode", "(object)o");
                if (anyAttribute != null)
                {
                    this.WriteMemberEnd(members);
                }
                base.Writer.WriteLine("Reader.MoveToElement();");
                base.Writer.WriteLine("if (Reader.IsEmptyElement) {");
                IndentedWriter writer9 = base.Writer;
                writer9.Indent++;
                base.Writer.WriteLine("Reader.Skip();");
                this.WriteMemberEnd(memberArray2);
                base.Writer.WriteLine("return o;");
                IndentedWriter writer10 = base.Writer;
                writer10.Indent--;
                base.Writer.WriteLine("}");
                base.Writer.WriteLine("Reader.ReadStartElement();");
                if (this.IsSequence(memberArray3))
                {
                    base.Writer.WriteLine("int state = 0;");
                }
                int loopIndex = this.WriteWhileNotLoopStart();
                IndentedWriter writer11 = base.Writer;
                writer11.Indent++;
                string elementElseString = "UnknownNode((object)o, " + this.ExpectedElements(memberArray3) + ");";
                this.WriteMemberElements(memberArray3, elementElseString, elementElseString, member2, member, null);
                base.Writer.WriteLine("Reader.MoveToContent();");
                this.WriteWhileLoopEnd(loopIndex);
                this.WriteMemberEnd(memberArray2);
                base.Writer.WriteLine("ReadEndElement();");
                base.Writer.WriteLine("return o;");
            }
            IndentedWriter writer12 = base.Writer;
            writer12.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteLocalDecl(string typeFullName, string variableName, string initValue, bool useReflection)
        {
            base.RaCodeGen.WriteLocalDecl(typeFullName, variableName, initValue, useReflection);
        }

        private void WriteMemberBegin(Member[] members)
        {
            for (int i = 0; i < members.Length; i++)
            {
                Member member = members[i];
                if (member.IsArrayLike)
                {
                    string arrayName = member.ArrayName;
                    string s = "c" + arrayName;
                    TypeDesc typeDesc = member.Mapping.TypeDesc;
                    string cSharpName = typeDesc.CSharpName;
                    if (member.Mapping.TypeDesc.IsArray)
                    {
                        this.WriteArrayLocalDecl(typeDesc.CSharpName, arrayName, "null", typeDesc);
                        base.Writer.Write("int ");
                        base.Writer.Write(s);
                        base.Writer.WriteLine(" = 0;");
                        if (member.Mapping.ChoiceIdentifier != null)
                        {
                            this.WriteArrayLocalDecl(member.Mapping.ChoiceIdentifier.Mapping.TypeDesc.CSharpName + "[]", member.ChoiceArrayName, "null", member.Mapping.ChoiceIdentifier.Mapping.TypeDesc);
                            base.Writer.Write("int c");
                            base.Writer.Write(member.ChoiceArrayName);
                            base.Writer.WriteLine(" = 0;");
                        }
                    }
                    else
                    {
                        bool useReflection = typeDesc.UseReflection;
                        if ((member.Source[member.Source.Length - 1] == '(') || (member.Source[member.Source.Length - 1] == '{'))
                        {
                            this.WriteCreateInstance(cSharpName, arrayName, useReflection, typeDesc.CannotNew);
                            base.Writer.Write(member.Source);
                            base.Writer.Write(arrayName);
                            if (member.Source[member.Source.Length - 1] == '{')
                            {
                                base.Writer.WriteLine("});");
                            }
                            else
                            {
                                base.Writer.WriteLine(");");
                            }
                        }
                        else
                        {
                            if ((member.IsList && !member.Mapping.ReadOnly) && member.Mapping.TypeDesc.IsNullable)
                            {
                                base.Writer.Write("if ((object)(");
                                base.Writer.Write(member.Source);
                                base.Writer.Write(") == null) ");
                                if (!member.Mapping.TypeDesc.HasDefaultConstructor)
                                {
                                    base.Writer.Write("throw CreateReadOnlyCollectionException(");
                                    base.WriteQuotedCSharpString(member.Mapping.TypeDesc.CSharpName);
                                    base.Writer.WriteLine(");");
                                }
                                else
                                {
                                    base.Writer.Write(member.Source);
                                    base.Writer.Write(" = ");
                                    base.Writer.Write(base.RaCodeGen.GetStringForCreateInstance(cSharpName, useReflection, typeDesc.CannotNew, true));
                                    base.Writer.WriteLine(";");
                                }
                            }
                            this.WriteLocalDecl(cSharpName, arrayName, member.Source, useReflection);
                        }
                    }
                }
            }
        }

        private void WriteMemberElements(Member[] members, string elementElseString, string elseString, Member anyElement, Member anyText, string checkTypeHrefsSource)
        {
            bool flag = (checkTypeHrefsSource != null) && (checkTypeHrefsSource.Length > 0);
            if (anyText != null)
            {
                base.Writer.WriteLine("string tmp = null;");
            }
            base.Writer.Write("if (Reader.NodeType == ");
            base.Writer.Write(typeof(XmlNodeType).FullName);
            base.Writer.WriteLine(".Element) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            if (flag)
            {
                this.WriteIfNotSoapRoot(elementElseString + " continue;");
                this.WriteMemberElementsCheckType(checkTypeHrefsSource);
            }
            else
            {
                this.WriteMemberElementsIf(members, anyElement, elementElseString, null);
            }
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
            if (anyText != null)
            {
                this.WriteMemberText(anyText, elseString);
            }
            base.Writer.WriteLine("else {");
            IndentedWriter writer3 = base.Writer;
            writer3.Indent++;
            base.Writer.WriteLine(elseString);
            IndentedWriter writer4 = base.Writer;
            writer4.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteMemberElementsCheckType(string checkTypeHrefsSource)
        {
            base.Writer.WriteLine("string refElemId = null;");
            base.Writer.WriteLine("object refElem = ReadReferencingElement(null, null, true, out refElemId);");
            base.Writer.WriteLine("if (refElemId != null) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.Write(checkTypeHrefsSource);
            base.Writer.WriteLine(".Add(refElemId);");
            base.Writer.Write(checkTypeHrefsSource);
            base.Writer.WriteLine("IsObject.Add(false);");
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
            base.Writer.WriteLine("else if (refElem != null) {");
            IndentedWriter writer3 = base.Writer;
            writer3.Indent++;
            base.Writer.Write(checkTypeHrefsSource);
            base.Writer.WriteLine(".Add(refElem);");
            base.Writer.Write(checkTypeHrefsSource);
            base.Writer.WriteLine("IsObject.Add(true);");
            IndentedWriter writer4 = base.Writer;
            writer4.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteMemberElementsElse(Member anyElement, string elementElseString)
        {
            if (anyElement != null)
            {
                ElementAccessor[] elements = anyElement.Mapping.Elements;
                for (int i = 0; i < elements.Length; i++)
                {
                    ElementAccessor element = elements[i];
                    if (element.Any && (element.Name.Length == 0))
                    {
                        this.WriteElement(anyElement.ArraySource, anyElement.ArrayName, anyElement.ChoiceArraySource, element, anyElement.Mapping.ChoiceIdentifier, (anyElement.Mapping.CheckSpecified == SpecifiedAccessor.ReadWrite) ? anyElement.CheckSpecifiedSource : null, false, false, -1, i);
                        return;
                    }
                }
            }
            else
            {
                base.Writer.WriteLine(elementElseString);
            }
        }

        private void WriteMemberElementsIf(Member[] members, Member anyElement, string elementElseString, string checkTypeSource)
        {
            bool flag = (checkTypeSource != null) && (checkTypeSource.Length > 0);
            int num = 0;
            bool flag2 = this.IsSequence(members);
            if (flag2)
            {
                base.Writer.WriteLine("switch (state) {");
            }
            int num2 = 0;
            for (int i = 0; i < members.Length; i++)
            {
                Member member = members[i];
                if (((member.Mapping.Xmlns == null) && !member.Mapping.Ignore) && (!flag2 || (!member.Mapping.IsText && !member.Mapping.IsAttribute)))
                {
                    bool flag3 = true;
                    ChoiceIdentifierAccessor choiceIdentifier = member.Mapping.ChoiceIdentifier;
                    ElementAccessor[] elements = member.Mapping.Elements;
                    for (int j = 0; j < elements.Length; j++)
                    {
                        ElementAccessor element = elements[j];
                        string ns = (element.Form == XmlSchemaForm.Qualified) ? element.Namespace : "";
                        if ((flag2 || !element.Any) || ((element.Name != null) && (element.Name.Length != 0)))
                        {
                            if (!flag3 || (!flag2 && (num > 0)))
                            {
                                base.Writer.Write("else ");
                            }
                            else if (flag2)
                            {
                                base.Writer.Write("case ");
                                base.Writer.Write(num2.ToString(CultureInfo.InvariantCulture));
                                base.Writer.WriteLine(":");
                                IndentedWriter writer1 = base.Writer;
                                writer1.Indent++;
                            }
                            num++;
                            flag3 = false;
                            base.Writer.Write("if (");
                            if (member.ParamsReadSource != null)
                            {
                                base.Writer.Write("!");
                                base.Writer.Write(member.ParamsReadSource);
                                base.Writer.Write(" && ");
                            }
                            if (flag)
                            {
                                if (element.Mapping is NullableMapping)
                                {
                                    TypeDesc typeDesc = ((NullableMapping) element.Mapping).BaseMapping.TypeDesc;
                                    base.Writer.Write(base.RaCodeGen.GetStringForTypeof(typeDesc.CSharpName, typeDesc.UseReflection));
                                }
                                else
                                {
                                    base.Writer.Write(base.RaCodeGen.GetStringForTypeof(element.Mapping.TypeDesc.CSharpName, element.Mapping.TypeDesc.UseReflection));
                                }
                                base.Writer.Write(".IsAssignableFrom(");
                                base.Writer.Write(checkTypeSource);
                                base.Writer.Write("Type)");
                            }
                            else
                            {
                                if (member.Mapping.IsReturnValue)
                                {
                                    base.Writer.Write("(IsReturnValue || ");
                                }
                                if ((flag2 && element.Any) && (element.AnyNamespaces == null))
                                {
                                    base.Writer.Write("true");
                                }
                                else
                                {
                                    this.WriteXmlNodeEqual("Reader", element.Name, ns);
                                }
                                if (member.Mapping.IsReturnValue)
                                {
                                    base.Writer.Write(")");
                                }
                            }
                            base.Writer.WriteLine(") {");
                            IndentedWriter writer = base.Writer;
                            writer.Indent++;
                            if (flag)
                            {
                                if (element.Mapping.TypeDesc.IsValueType || (element.Mapping is NullableMapping))
                                {
                                    base.Writer.Write("if (");
                                    base.Writer.Write(checkTypeSource);
                                    base.Writer.WriteLine(" != null) {");
                                    IndentedWriter writer3 = base.Writer;
                                    writer3.Indent++;
                                }
                                if (element.Mapping is NullableMapping)
                                {
                                    this.WriteSourceBegin(member.ArraySource);
                                    TypeDesc desc2 = ((NullableMapping) element.Mapping).BaseMapping.TypeDesc;
                                    base.Writer.Write(base.RaCodeGen.GetStringForCreateInstance(element.Mapping.TypeDesc.CSharpName, element.Mapping.TypeDesc.UseReflection, false, true, "(" + desc2.CSharpName + ")" + checkTypeSource));
                                }
                                else
                                {
                                    this.WriteSourceBeginTyped(member.ArraySource, element.Mapping.TypeDesc);
                                    base.Writer.Write(checkTypeSource);
                                }
                                this.WriteSourceEnd(member.ArraySource);
                                base.Writer.WriteLine(";");
                                if (element.Mapping.TypeDesc.IsValueType)
                                {
                                    IndentedWriter writer4 = base.Writer;
                                    writer4.Indent--;
                                    base.Writer.WriteLine("}");
                                }
                                if (member.FixupIndex >= 0)
                                {
                                    base.Writer.Write("fixup.Ids[");
                                    base.Writer.Write(member.FixupIndex.ToString(CultureInfo.InvariantCulture));
                                    base.Writer.Write("] = ");
                                    base.Writer.Write(checkTypeSource);
                                    base.Writer.WriteLine("Id;");
                                }
                            }
                            else
                            {
                                this.WriteElement(member.ArraySource, member.ArrayName, member.ChoiceArraySource, element, choiceIdentifier, (member.Mapping.CheckSpecified == SpecifiedAccessor.ReadWrite) ? member.CheckSpecifiedSource : null, member.IsList && member.Mapping.TypeDesc.IsNullable, member.Mapping.ReadOnly, member.FixupIndex, j);
                            }
                            if (member.Mapping.IsReturnValue)
                            {
                                base.Writer.WriteLine("IsReturnValue = false;");
                            }
                            if (member.ParamsReadSource != null)
                            {
                                base.Writer.Write(member.ParamsReadSource);
                                base.Writer.WriteLine(" = true;");
                            }
                            IndentedWriter writer5 = base.Writer;
                            writer5.Indent--;
                            base.Writer.WriteLine("}");
                        }
                    }
                    if (flag2)
                    {
                        if (member.IsArrayLike)
                        {
                            base.Writer.WriteLine("else {");
                            IndentedWriter writer6 = base.Writer;
                            writer6.Indent++;
                        }
                        num2++;
                        base.Writer.Write("state = ");
                        base.Writer.Write(num2.ToString(CultureInfo.InvariantCulture));
                        base.Writer.WriteLine(";");
                        if (member.IsArrayLike)
                        {
                            IndentedWriter writer7 = base.Writer;
                            writer7.Indent--;
                            base.Writer.WriteLine("}");
                        }
                        base.Writer.WriteLine("break;");
                        IndentedWriter writer8 = base.Writer;
                        writer8.Indent--;
                    }
                }
            }
            if (num > 0)
            {
                if (flag2)
                {
                    base.Writer.WriteLine("default:");
                }
                else
                {
                    base.Writer.WriteLine("else {");
                }
                IndentedWriter writer9 = base.Writer;
                writer9.Indent++;
            }
            this.WriteMemberElementsElse(anyElement, elementElseString);
            if (num > 0)
            {
                if (flag2)
                {
                    base.Writer.WriteLine("break;");
                }
                IndentedWriter writer10 = base.Writer;
                writer10.Indent--;
                base.Writer.WriteLine("}");
            }
        }

        private void WriteMemberEnd(Member[] members)
        {
            this.WriteMemberEnd(members, false);
        }

        private void WriteMemberEnd(Member[] members, bool soapRefs)
        {
            for (int i = 0; i < members.Length; i++)
            {
                Member member = members[i];
                if (member.IsArrayLike)
                {
                    TypeDesc typeDesc = member.Mapping.TypeDesc;
                    if (typeDesc.IsArray)
                    {
                        this.WriteSourceBegin(member.Source);
                        if (soapRefs)
                        {
                            base.Writer.Write(" soap[1] = ");
                        }
                        string arrayName = member.ArrayName;
                        string s = "c" + arrayName;
                        bool useReflection = typeDesc.ArrayElementTypeDesc.UseReflection;
                        string cSharpName = typeDesc.ArrayElementTypeDesc.CSharpName;
                        if (!useReflection)
                        {
                            base.Writer.Write("(" + cSharpName + "[])");
                        }
                        base.Writer.Write("ShrinkArray(");
                        base.Writer.Write(arrayName);
                        base.Writer.Write(", ");
                        base.Writer.Write(s);
                        base.Writer.Write(", ");
                        base.Writer.Write(base.RaCodeGen.GetStringForTypeof(cSharpName, useReflection));
                        base.Writer.Write(", ");
                        this.WriteBooleanValue(member.IsNullable);
                        base.Writer.Write(")");
                        this.WriteSourceEnd(member.Source);
                        base.Writer.WriteLine(";");
                        if (member.Mapping.ChoiceIdentifier != null)
                        {
                            this.WriteSourceBegin(member.ChoiceSource);
                            arrayName = member.ChoiceArrayName;
                            s = "c" + arrayName;
                            bool flag2 = member.Mapping.ChoiceIdentifier.Mapping.TypeDesc.UseReflection;
                            string typeFullName = member.Mapping.ChoiceIdentifier.Mapping.TypeDesc.CSharpName;
                            if (!flag2)
                            {
                                base.Writer.Write("(" + typeFullName + "[])");
                            }
                            base.Writer.Write("ShrinkArray(");
                            base.Writer.Write(arrayName);
                            base.Writer.Write(", ");
                            base.Writer.Write(s);
                            base.Writer.Write(", ");
                            base.Writer.Write(base.RaCodeGen.GetStringForTypeof(typeFullName, flag2));
                            base.Writer.Write(", ");
                            this.WriteBooleanValue(member.IsNullable);
                            base.Writer.Write(")");
                            this.WriteSourceEnd(member.ChoiceSource);
                            base.Writer.WriteLine(";");
                        }
                    }
                    else if (typeDesc.IsValueType)
                    {
                        base.Writer.Write(member.Source);
                        base.Writer.Write(" = ");
                        base.Writer.Write(member.ArrayName);
                        base.Writer.WriteLine(";");
                    }
                }
            }
        }

        private bool WriteMemberFixupBegin(Member[] members, string fixupMethodName, string source)
        {
            int num = 0;
            for (int i = 0; i < members.Length; i++)
            {
                Member member = members[i];
                if (member.Mapping.Elements.Length != 0)
                {
                    TypeMapping mapping = member.Mapping.Elements[0].Mapping;
                    if (((mapping is StructMapping) || (mapping is ArrayMapping)) || ((mapping is PrimitiveMapping) || (mapping is NullableMapping)))
                    {
                        member.MultiRef = true;
                        member.FixupIndex = num++;
                    }
                }
            }
            if (num > 0)
            {
                base.Writer.Write("Fixup fixup = new Fixup(");
                base.Writer.Write(source);
                base.Writer.Write(", ");
                base.Writer.Write("new ");
                base.Writer.Write(typeof(XmlSerializationFixupCallback).FullName);
                base.Writer.Write("(this.");
                base.Writer.Write(fixupMethodName);
                base.Writer.Write("), ");
                base.Writer.Write(num.ToString(CultureInfo.InvariantCulture));
                base.Writer.WriteLine(");");
                base.Writer.WriteLine("AddFixup(fixup);");
                return true;
            }
            return false;
        }

        private void WriteMemberText(Member anyText, string elseString)
        {
            base.Writer.Write("else if (Reader.NodeType == ");
            base.Writer.Write(typeof(XmlNodeType).FullName);
            base.Writer.WriteLine(".Text || ");
            base.Writer.Write("Reader.NodeType == ");
            base.Writer.Write(typeof(XmlNodeType).FullName);
            base.Writer.WriteLine(".CDATA || ");
            base.Writer.Write("Reader.NodeType == ");
            base.Writer.Write(typeof(XmlNodeType).FullName);
            base.Writer.WriteLine(".Whitespace || ");
            base.Writer.Write("Reader.NodeType == ");
            base.Writer.Write(typeof(XmlNodeType).FullName);
            base.Writer.WriteLine(".SignificantWhitespace) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            if (anyText != null)
            {
                this.WriteText(anyText);
            }
            else
            {
                base.Writer.Write(elseString);
                base.Writer.WriteLine(";");
            }
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteNullableMethod(NullableMapping nullableMapping)
        {
            string s = (string) base.MethodNames[nullableMapping];
            bool useReflection = nullableMapping.BaseMapping.TypeDesc.UseReflection;
            string str2 = useReflection ? "object" : nullableMapping.TypeDesc.CSharpName;
            base.Writer.WriteLine();
            base.Writer.Write(str2);
            base.Writer.Write(" ");
            base.Writer.Write(s);
            base.Writer.WriteLine("(bool checkType) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.Write(str2);
            base.Writer.Write(" o = ");
            if (useReflection)
            {
                base.Writer.Write("null");
            }
            else
            {
                base.Writer.Write("default(");
                base.Writer.Write(str2);
                base.Writer.Write(")");
            }
            base.Writer.WriteLine(";");
            base.Writer.WriteLine("if (ReadNull())");
            IndentedWriter writer2 = base.Writer;
            writer2.Indent++;
            base.Writer.WriteLine("return o;");
            IndentedWriter writer3 = base.Writer;
            writer3.Indent--;
            ElementAccessor element = new ElementAccessor {
                Mapping = nullableMapping.BaseMapping,
                Any = false,
                IsNullable = nullableMapping.BaseMapping.TypeDesc.IsNullable
            };
            this.WriteElement("o", null, null, element, null, null, false, false, -1, -1);
            base.Writer.WriteLine("return o;");
            IndentedWriter writer4 = base.Writer;
            writer4.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteParamsRead(int length)
        {
            base.Writer.Write("bool[] paramsRead = new bool[");
            base.Writer.Write(length.ToString(CultureInfo.InvariantCulture));
            base.Writer.WriteLine("];");
        }

        private void WritePrimitive(TypeMapping mapping, string source)
        {
            if (mapping is EnumMapping)
            {
                string s = base.ReferenceMapping(mapping);
                if (s == null)
                {
                    throw new InvalidOperationException(Res.GetString("XmlMissingMethodEnum", new object[] { mapping.TypeDesc.Name }));
                }
                if (mapping.IsSoap)
                {
                    base.Writer.Write("(");
                    base.Writer.Write(mapping.TypeDesc.CSharpName);
                    base.Writer.Write(")");
                }
                base.Writer.Write(s);
                base.Writer.Write("(");
                if (!mapping.IsSoap)
                {
                    base.Writer.Write(source);
                }
                base.Writer.Write(")");
            }
            else if (mapping.TypeDesc == base.StringTypeDesc)
            {
                base.Writer.Write(source);
            }
            else if (mapping.TypeDesc.FormatterName == "String")
            {
                if (mapping.TypeDesc.CollapseWhitespace)
                {
                    base.Writer.Write("CollapseWhitespace(");
                    base.Writer.Write(source);
                    base.Writer.Write(")");
                }
                else
                {
                    base.Writer.Write(source);
                }
            }
            else
            {
                if (!mapping.TypeDesc.HasCustomFormatter)
                {
                    base.Writer.Write(typeof(XmlConvert).FullName);
                    base.Writer.Write(".");
                }
                base.Writer.Write("To");
                base.Writer.Write(mapping.TypeDesc.FormatterName);
                base.Writer.Write("(");
                base.Writer.Write(source);
                base.Writer.Write(")");
            }
        }

        private void WriteQNameEqual(string source, string name, string ns)
        {
            base.Writer.Write("((object) ((");
            base.Writer.Write(typeof(XmlQualifiedName).FullName);
            base.Writer.Write(")");
            base.Writer.Write(source);
            base.Writer.Write(").Name == (object)");
            this.WriteID(name);
            base.Writer.Write(" && (object) ((");
            base.Writer.Write(typeof(XmlQualifiedName).FullName);
            base.Writer.Write(")");
            base.Writer.Write(source);
            base.Writer.Write(").Namespace == (object)");
            this.WriteID(ns);
            base.Writer.Write(")");
        }

        private void WriteReadNonRoots()
        {
            base.Writer.WriteLine("Reader.MoveToContent();");
            int loopIndex = this.WriteWhileLoopStartCheck();
            base.Writer.Write("while (Reader.NodeType == ");
            base.Writer.Write(typeof(XmlNodeType).FullName);
            base.Writer.WriteLine(".Element) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.Write("string root = Reader.GetAttribute(\"root\", \"");
            base.Writer.Write("http://schemas.xmlsoap.org/soap/encoding/");
            base.Writer.WriteLine("\");");
            base.Writer.Write("if (root == null || ");
            base.Writer.Write(typeof(XmlConvert).FullName);
            base.Writer.WriteLine(".ToBoolean(root)) break;");
            base.Writer.WriteLine("ReadReferencedElement();");
            base.Writer.WriteLine("Reader.MoveToContent();");
            this.WriteWhileLoopEnd(loopIndex);
        }

        private void WriteSourceBegin(string source)
        {
            base.Writer.Write(source);
            if ((source[source.Length - 1] != '(') && (source[source.Length - 1] != '{'))
            {
                base.Writer.Write(" = ");
            }
        }

        private void WriteSourceBeginTyped(string source, TypeDesc typeDesc)
        {
            this.WriteSourceBegin(source);
            if ((typeDesc != null) && !typeDesc.UseReflection)
            {
                base.Writer.Write("(");
                base.Writer.Write(typeDesc.CSharpName);
                base.Writer.Write(")");
            }
        }

        private void WriteSourceEnd(string source)
        {
            if (source[source.Length - 1] == '(')
            {
                base.Writer.Write(")");
            }
            else if (source[source.Length - 1] == '{')
            {
                base.Writer.Write("})");
            }
        }

        private void WriteStructMethod(StructMapping structMapping)
        {
            if (structMapping.IsSoap)
            {
                this.WriteEncodedStructMethod(structMapping);
            }
            else
            {
                this.WriteLiteralStructMethod(structMapping);
            }
        }

        private void WriteText(Member member)
        {
            TextAccessor text = member.Mapping.Text;
            if (text.Mapping is SpecialMapping)
            {
                SpecialMapping mapping = (SpecialMapping) text.Mapping;
                this.WriteSourceBeginTyped(member.ArraySource, mapping.TypeDesc);
                if (mapping.TypeDesc.Kind != TypeKind.Node)
                {
                    throw new InvalidOperationException(Res.GetString("XmlInternalError"));
                }
                base.Writer.Write("Document.CreateTextNode(Reader.ReadString())");
                this.WriteSourceEnd(member.ArraySource);
            }
            else
            {
                if (member.IsArrayLike)
                {
                    this.WriteSourceBegin(member.ArraySource);
                    if (text.Mapping.TypeDesc.CollapseWhitespace)
                    {
                        base.Writer.Write("CollapseWhitespace(Reader.ReadString())");
                    }
                    else
                    {
                        base.Writer.Write("Reader.ReadString()");
                    }
                }
                else if ((text.Mapping.TypeDesc == base.StringTypeDesc) || (text.Mapping.TypeDesc.FormatterName == "String"))
                {
                    base.Writer.Write("tmp = ReadString(tmp, ");
                    if (text.Mapping.TypeDesc.CollapseWhitespace)
                    {
                        base.Writer.WriteLine("true);");
                    }
                    else
                    {
                        base.Writer.WriteLine("false);");
                    }
                    this.WriteSourceBegin(member.ArraySource);
                    base.Writer.Write("tmp");
                }
                else
                {
                    this.WriteSourceBegin(member.ArraySource);
                    this.WritePrimitive(text.Mapping, "Reader.ReadString()");
                }
                this.WriteSourceEnd(member.ArraySource);
            }
            base.Writer.WriteLine(";");
        }

        private void WriteUnknownNode(string func, string node, ElementAccessor e, bool anyIfs)
        {
            if (anyIfs)
            {
                base.Writer.WriteLine("else {");
                IndentedWriter writer = base.Writer;
                writer.Indent++;
            }
            base.Writer.Write(func);
            base.Writer.Write("(");
            base.Writer.Write(node);
            if (e != null)
            {
                base.Writer.Write(", ");
                string str = (e.Form == XmlSchemaForm.Qualified) ? e.Namespace : "";
                str = str + ":" + e.Name;
                ReflectionAwareCodeGen.WriteQuotedCSharpString(base.Writer, str);
            }
            base.Writer.WriteLine(");");
            if (anyIfs)
            {
                IndentedWriter writer2 = base.Writer;
                writer2.Indent--;
                base.Writer.WriteLine("}");
            }
        }

        private void WriteWhileLoopEnd(int loopIndex)
        {
            this.WriteWhileLoopEndCheck(loopIndex);
            IndentedWriter writer = base.Writer;
            writer.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteWhileLoopEndCheck(int loopIndex)
        {
            base.Writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "CheckReaderCount(ref whileIterations{0}, ref readerCount{1});", new object[] { loopIndex, loopIndex }));
        }

        private int WriteWhileLoopStartCheck()
        {
            base.Writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "int whileIterations{0} = 0;", new object[] { this.nextWhileLoopIndex }));
            base.Writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "int readerCount{0} = ReaderCount;", new object[] { this.nextWhileLoopIndex }));
            return this.nextWhileLoopIndex++;
        }

        private int WriteWhileNotLoopStart()
        {
            base.Writer.WriteLine("Reader.MoveToContent();");
            int num = this.WriteWhileLoopStartCheck();
            base.Writer.Write("while (Reader.NodeType != ");
            base.Writer.Write(typeof(XmlNodeType).FullName);
            base.Writer.Write(".EndElement && Reader.NodeType != ");
            base.Writer.Write(typeof(XmlNodeType).FullName);
            base.Writer.WriteLine(".None) {");
            return num;
        }

        private void WriteXmlNodeEqual(string source, string name, string ns)
        {
            base.Writer.Write("(");
            if ((name != null) && (name.Length > 0))
            {
                base.Writer.Write("(object) ");
                base.Writer.Write(source);
                base.Writer.Write(".LocalName == (object)");
                this.WriteID(name);
                base.Writer.Write(" && ");
            }
            base.Writer.Write("(object) ");
            base.Writer.Write(source);
            base.Writer.Write(".NamespaceURI == (object)");
            this.WriteID(ns);
            base.Writer.Write(")");
        }

        internal Hashtable Enums
        {
            get
            {
                if (this.enums == null)
                {
                    this.enums = new Hashtable();
                }
                return this.enums;
            }
        }

        private class CreateCollectionInfo
        {
            private string name;
            private System.Xml.Serialization.TypeDesc td;

            internal CreateCollectionInfo(string name, System.Xml.Serialization.TypeDesc td)
            {
                this.name = name;
                this.td = td;
            }

            internal string Name
            {
                get
                {
                    return this.name;
                }
            }

            internal System.Xml.Serialization.TypeDesc TypeDesc
            {
                get
                {
                    return this.td;
                }
            }
        }

        private class Member
        {
            private string arrayName;
            private string arraySource;
            private string checkSpecifiedSource;
            private string choiceArrayName;
            private string choiceArraySource;
            private string choiceSource;
            private int fixupIndex;
            private bool isArray;
            private bool isList;
            private bool isNullable;
            private MemberMapping mapping;
            private bool multiRef;
            private string paramsReadSource;
            private string source;

            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arrayName, int i, MemberMapping mapping) : this(outerClass, source, null, arrayName, i, mapping, false, null)
            {
            }

            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arrayName, int i, MemberMapping mapping, bool multiRef) : this(outerClass, source, null, arrayName, i, mapping, multiRef, null)
            {
            }

            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arrayName, int i, MemberMapping mapping, string choiceSource) : this(outerClass, source, null, arrayName, i, mapping, false, choiceSource)
            {
            }

            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arraySource, string arrayName, int i, MemberMapping mapping) : this(outerClass, source, arraySource, arrayName, i, mapping, false, null)
            {
            }

            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arraySource, string arrayName, int i, MemberMapping mapping, string choiceSource) : this(outerClass, source, arraySource, arrayName, i, mapping, false, choiceSource)
            {
            }

            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arraySource, string arrayName, int i, MemberMapping mapping, bool multiRef, string choiceSource)
            {
                this.fixupIndex = -1;
                this.source = source;
                this.arrayName = arrayName + "_" + i.ToString(CultureInfo.InvariantCulture);
                this.choiceArrayName = "choice_" + this.arrayName;
                this.choiceSource = choiceSource;
                ElementAccessor[] elements = mapping.Elements;
                if (mapping.TypeDesc.IsArrayLike)
                {
                    if (arraySource != null)
                    {
                        this.arraySource = arraySource;
                    }
                    else
                    {
                        this.arraySource = outerClass.GetArraySource(mapping.TypeDesc, this.arrayName, multiRef);
                    }
                    this.isArray = mapping.TypeDesc.IsArray;
                    this.isList = !this.isArray;
                    if (mapping.ChoiceIdentifier != null)
                    {
                        this.choiceArraySource = outerClass.GetArraySource(mapping.TypeDesc, this.choiceArrayName, multiRef);
                        string choiceArrayName = this.choiceArrayName;
                        string str2 = "c" + choiceArrayName;
                        bool useReflection = mapping.ChoiceIdentifier.Mapping.TypeDesc.UseReflection;
                        string cSharpName = mapping.ChoiceIdentifier.Mapping.TypeDesc.CSharpName;
                        string str4 = useReflection ? "" : ("(" + cSharpName + "[])");
                        string str5 = choiceArrayName + " = " + str4 + "EnsureArrayIndex(" + choiceArrayName + ", " + str2 + ", " + outerClass.RaCodeGen.GetStringForTypeof(cSharpName, useReflection) + ");";
                        this.choiceArraySource = str5 + outerClass.RaCodeGen.GetStringForArrayMember(choiceArrayName, str2 + "++", mapping.ChoiceIdentifier.Mapping.TypeDesc);
                    }
                    else
                    {
                        this.choiceArraySource = this.choiceSource;
                    }
                }
                else
                {
                    this.arraySource = (arraySource == null) ? source : arraySource;
                    this.choiceArraySource = this.choiceSource;
                }
                this.mapping = mapping;
            }

            internal string ArrayName
            {
                get
                {
                    return this.arrayName;
                }
            }

            internal string ArraySource
            {
                get
                {
                    return this.arraySource;
                }
            }

            internal string CheckSpecifiedSource
            {
                get
                {
                    return this.checkSpecifiedSource;
                }
                set
                {
                    this.checkSpecifiedSource = value;
                }
            }

            internal string ChoiceArrayName
            {
                get
                {
                    return this.choiceArrayName;
                }
            }

            internal string ChoiceArraySource
            {
                get
                {
                    return this.choiceArraySource;
                }
            }

            internal string ChoiceSource
            {
                get
                {
                    return this.choiceSource;
                }
            }

            internal int FixupIndex
            {
                get
                {
                    return this.fixupIndex;
                }
                set
                {
                    this.fixupIndex = value;
                }
            }

            internal bool IsArrayLike
            {
                get
                {
                    if (!this.isArray)
                    {
                        return this.isList;
                    }
                    return true;
                }
            }

            internal bool IsList
            {
                get
                {
                    return this.isList;
                }
            }

            internal bool IsNullable
            {
                get
                {
                    return this.isNullable;
                }
                set
                {
                    this.isNullable = value;
                }
            }

            internal MemberMapping Mapping
            {
                get
                {
                    return this.mapping;
                }
            }

            internal bool MultiRef
            {
                get
                {
                    return this.multiRef;
                }
                set
                {
                    this.multiRef = value;
                }
            }

            internal string ParamsReadSource
            {
                get
                {
                    return this.paramsReadSource;
                }
                set
                {
                    this.paramsReadSource = value;
                }
            }

            internal string Source
            {
                get
                {
                    return this.source;
                }
            }
        }
    }
}

