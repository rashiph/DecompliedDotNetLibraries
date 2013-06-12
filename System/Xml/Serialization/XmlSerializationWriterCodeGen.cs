namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    internal class XmlSerializationWriterCodeGen : XmlSerializationCodeGen
    {
        internal XmlSerializationWriterCodeGen(IndentedWriter writer, TypeScope[] scopes, string access, string className) : base(writer, scopes, access, className)
        {
        }

        private bool CanOptimizeWriteListSequence(TypeDesc listElementTypeDesc)
        {
            return ((listElementTypeDesc != null) && (listElementTypeDesc != base.QnameTypeDesc));
        }

        private string FindChoiceEnumValue(ElementAccessor element, EnumMapping choiceMapping, bool useReflection)
        {
            string ident = null;
            for (int i = 0; i < choiceMapping.Constants.Length; i++)
            {
                string xmlName = choiceMapping.Constants[i].XmlName;
                if (element.Any && (element.Name.Length == 0))
                {
                    if (!(xmlName == "##any:"))
                    {
                        continue;
                    }
                    if (useReflection)
                    {
                        ident = choiceMapping.Constants[i].Value.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        ident = choiceMapping.Constants[i].Name;
                    }
                    break;
                }
                int length = xmlName.LastIndexOf(':');
                string str3 = (length < 0) ? choiceMapping.Namespace : xmlName.Substring(0, length);
                string str4 = (length < 0) ? xmlName : xmlName.Substring(length + 1);
                if ((element.Name == str4) && (((element.Form == XmlSchemaForm.Unqualified) && string.IsNullOrEmpty(str3)) || (element.Namespace == str3)))
                {
                    if (useReflection)
                    {
                        ident = choiceMapping.Constants[i].Value.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        ident = choiceMapping.Constants[i].Name;
                    }
                    break;
                }
            }
            if ((ident == null) || (ident.Length == 0))
            {
                if (element.Any && (element.Name.Length == 0))
                {
                    throw new InvalidOperationException(Res.GetString("XmlChoiceMissingAnyValue", new object[] { choiceMapping.TypeDesc.FullName }));
                }
                throw new InvalidOperationException(Res.GetString("XmlChoiceMissingValue", new object[] { choiceMapping.TypeDesc.FullName, element.Namespace + ":" + element.Name, element.Name, element.Namespace }));
            }
            if (!useReflection)
            {
                CodeIdentifier.CheckValidIdentifier(ident);
            }
            return ident;
        }

        private int FindXmlnsIndex(MemberMapping[] members)
        {
            for (int i = 0; i < members.Length; i++)
            {
                if (members[i].Xmlns != null)
                {
                    return i;
                }
            }
            return -1;
        }

        internal void GenerateBegin()
        {
            base.Writer.Write(base.Access);
            base.Writer.Write(" class ");
            base.Writer.Write(base.ClassName);
            base.Writer.Write(" : ");
            base.Writer.Write(typeof(XmlSerializationWriter).FullName);
            base.Writer.WriteLine(" {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            foreach (TypeScope scope in base.Scopes)
            {
                foreach (TypeMapping mapping in scope.TypeMappings)
                {
                    if ((mapping is StructMapping) || (mapping is EnumMapping))
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
                    }
                }
            }
        }

        internal string GenerateElement(XmlMapping xmlMapping)
        {
            if (!xmlMapping.IsWriteable)
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

        internal void GenerateEnd()
        {
            base.GenerateReferencedMethods();
            this.GenerateInitCallbacksMethod();
            IndentedWriter writer = base.Writer;
            writer.Indent--;
            base.Writer.WriteLine("}");
        }

        private void GenerateInitCallbacksMethod()
        {
            base.Writer.WriteLine();
            base.Writer.WriteLine("protected override void InitCallbacks() {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            foreach (TypeScope scope in base.Scopes)
            {
                foreach (TypeMapping mapping in scope.TypeMappings)
                {
                    if ((mapping.IsSoap && ((mapping is StructMapping) || (mapping is EnumMapping))) && !mapping.TypeDesc.IsRoot)
                    {
                        string s = (string) base.MethodNames[mapping];
                        base.Writer.Write("AddWriteCallback(");
                        base.Writer.Write(base.RaCodeGen.GetStringForTypeof(mapping.TypeDesc.CSharpName, mapping.TypeDesc.UseReflection));
                        base.Writer.Write(", ");
                        base.WriteQuotedCSharpString(mapping.TypeName);
                        base.Writer.Write(", ");
                        base.WriteQuotedCSharpString(mapping.Namespace);
                        base.Writer.Write(", new ");
                        base.Writer.Write(typeof(XmlSerializationWriteCallback).FullName);
                        base.Writer.Write("(this.");
                        base.Writer.Write(s);
                        base.Writer.WriteLine("));");
                    }
                }
            }
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
        }

        private string GenerateMembersElement(XmlMembersMapping xmlMembersMapping)
        {
            ElementAccessor accessor = xmlMembersMapping.Accessor;
            MembersMapping mapping = (MembersMapping) accessor.Mapping;
            bool hasWrapperElement = mapping.HasWrapperElement;
            bool writeAccessors = mapping.WriteAccessors;
            bool flag3 = xmlMembersMapping.IsSoap && writeAccessors;
            string s = this.NextMethodName(accessor.Name);
            base.Writer.WriteLine();
            base.Writer.Write("public void ");
            base.Writer.Write(s);
            base.Writer.WriteLine("(object[] p) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine("WriteStartDocument();");
            if (!mapping.IsSoap)
            {
                base.Writer.WriteLine("TopLevelElement();");
            }
            base.Writer.WriteLine("int pLength = p.Length;");
            if (hasWrapperElement)
            {
                this.WriteStartElement(accessor.Name, (accessor.Form == XmlSchemaForm.Qualified) ? accessor.Namespace : "", mapping.IsSoap);
                int index = this.FindXmlnsIndex(mapping.Members);
                if (index >= 0)
                {
                    MemberMapping mapping1 = mapping.Members[index];
                    string source = "((" + typeof(XmlSerializerNamespaces).FullName + ")p[" + index.ToString(CultureInfo.InvariantCulture) + "])";
                    base.Writer.Write("if (pLength > ");
                    base.Writer.Write(index.ToString(CultureInfo.InvariantCulture));
                    base.Writer.WriteLine(") {");
                    IndentedWriter writer2 = base.Writer;
                    writer2.Indent++;
                    this.WriteNamespaces(source);
                    IndentedWriter writer3 = base.Writer;
                    writer3.Indent--;
                    base.Writer.WriteLine("}");
                }
                for (int j = 0; j < mapping.Members.Length; j++)
                {
                    MemberMapping mapping2 = mapping.Members[j];
                    if ((mapping2.Attribute != null) && !mapping2.Ignore)
                    {
                        string str3 = "p[" + j.ToString(CultureInfo.InvariantCulture) + "]";
                        string str4 = null;
                        int num3 = 0;
                        if (mapping2.CheckSpecified != SpecifiedAccessor.None)
                        {
                            string str5 = mapping2.Name + "Specified";
                            for (int k = 0; k < mapping.Members.Length; k++)
                            {
                                if (mapping.Members[k].Name == str5)
                                {
                                    str4 = "((bool) p[" + k.ToString(CultureInfo.InvariantCulture) + "])";
                                    num3 = k;
                                    break;
                                }
                            }
                        }
                        base.Writer.Write("if (pLength > ");
                        base.Writer.Write(j.ToString(CultureInfo.InvariantCulture));
                        base.Writer.WriteLine(") {");
                        IndentedWriter writer4 = base.Writer;
                        writer4.Indent++;
                        if (str4 != null)
                        {
                            base.Writer.Write("if (pLength <= ");
                            base.Writer.Write(num3.ToString(CultureInfo.InvariantCulture));
                            base.Writer.Write(" || ");
                            base.Writer.Write(str4);
                            base.Writer.WriteLine(") {");
                            IndentedWriter writer5 = base.Writer;
                            writer5.Indent++;
                        }
                        this.WriteMember(str3, mapping2.Attribute, mapping2.TypeDesc, "p");
                        if (str4 != null)
                        {
                            IndentedWriter writer6 = base.Writer;
                            writer6.Indent--;
                            base.Writer.WriteLine("}");
                        }
                        IndentedWriter writer7 = base.Writer;
                        writer7.Indent--;
                        base.Writer.WriteLine("}");
                    }
                }
            }
            for (int i = 0; i < mapping.Members.Length; i++)
            {
                MemberMapping mapping3 = mapping.Members[i];
                if ((mapping3.Xmlns == null) && !mapping3.Ignore)
                {
                    string str6 = null;
                    int num6 = 0;
                    if (mapping3.CheckSpecified != SpecifiedAccessor.None)
                    {
                        string str7 = mapping3.Name + "Specified";
                        for (int m = 0; m < mapping.Members.Length; m++)
                        {
                            if (mapping.Members[m].Name == str7)
                            {
                                str6 = "((bool) p[" + m.ToString(CultureInfo.InvariantCulture) + "])";
                                num6 = m;
                                break;
                            }
                        }
                    }
                    base.Writer.Write("if (pLength > ");
                    base.Writer.Write(i.ToString(CultureInfo.InvariantCulture));
                    base.Writer.WriteLine(") {");
                    IndentedWriter writer8 = base.Writer;
                    writer8.Indent++;
                    if (str6 != null)
                    {
                        base.Writer.Write("if (pLength <= ");
                        base.Writer.Write(num6.ToString(CultureInfo.InvariantCulture));
                        base.Writer.Write(" || ");
                        base.Writer.Write(str6);
                        base.Writer.WriteLine(") {");
                        IndentedWriter writer9 = base.Writer;
                        writer9.Indent++;
                    }
                    string str8 = "p[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                    string choiceSource = null;
                    if (mapping3.ChoiceIdentifier != null)
                    {
                        for (int n = 0; n < mapping.Members.Length; n++)
                        {
                            if (mapping.Members[n].Name == mapping3.ChoiceIdentifier.MemberName)
                            {
                                if (mapping3.ChoiceIdentifier.Mapping.TypeDesc.UseReflection)
                                {
                                    choiceSource = "p[" + n.ToString(CultureInfo.InvariantCulture) + "]";
                                }
                                else
                                {
                                    choiceSource = "((" + mapping.Members[n].TypeDesc.CSharpName + ")p[" + n.ToString(CultureInfo.InvariantCulture) + "])";
                                }
                                break;
                            }
                        }
                    }
                    if ((flag3 && mapping3.IsReturnValue) && (mapping3.Elements.Length > 0))
                    {
                        base.Writer.Write("WriteRpcResult(");
                        base.WriteQuotedCSharpString(mapping3.Elements[0].Name);
                        base.Writer.Write(", ");
                        base.WriteQuotedCSharpString("");
                        base.Writer.WriteLine(");");
                    }
                    this.WriteMember(str8, choiceSource, mapping3.ElementsSortedByDerivation, mapping3.Text, mapping3.ChoiceIdentifier, mapping3.TypeDesc, writeAccessors || hasWrapperElement);
                    if (str6 != null)
                    {
                        IndentedWriter writer10 = base.Writer;
                        writer10.Indent--;
                        base.Writer.WriteLine("}");
                    }
                    IndentedWriter writer11 = base.Writer;
                    writer11.Indent--;
                    base.Writer.WriteLine("}");
                }
            }
            if (hasWrapperElement)
            {
                this.WriteEndElement();
            }
            if (accessor.IsSoap)
            {
                if (!hasWrapperElement && !writeAccessors)
                {
                    base.Writer.Write("if (pLength > ");
                    int length = mapping.Members.Length;
                    base.Writer.Write(length.ToString(CultureInfo.InvariantCulture));
                    base.Writer.WriteLine(") {");
                    IndentedWriter writer12 = base.Writer;
                    writer12.Indent++;
                    this.WriteExtraMembers(mapping.Members.Length.ToString(CultureInfo.InvariantCulture), "pLength");
                    IndentedWriter writer13 = base.Writer;
                    writer13.Indent--;
                    base.Writer.WriteLine("}");
                }
                base.Writer.WriteLine("WriteReferencedElements();");
            }
            IndentedWriter writer14 = base.Writer;
            writer14.Indent--;
            base.Writer.WriteLine("}");
            return s;
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
            }
        }

        private string GenerateTypeElement(XmlTypeMapping xmlTypeMapping)
        {
            ElementAccessor accessor = xmlTypeMapping.Accessor;
            TypeMapping mapping = accessor.Mapping;
            string s = this.NextMethodName(accessor.Name);
            base.Writer.WriteLine();
            base.Writer.Write("public void ");
            base.Writer.Write(s);
            base.Writer.WriteLine("(object o) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine("WriteStartDocument();");
            base.Writer.WriteLine("if (o == null) {");
            IndentedWriter writer2 = base.Writer;
            writer2.Indent++;
            if (accessor.IsNullable)
            {
                if (mapping.IsSoap)
                {
                    this.WriteEncodedNullTag(accessor.Name, (accessor.Form == XmlSchemaForm.Qualified) ? accessor.Namespace : "");
                }
                else
                {
                    this.WriteLiteralNullTag(accessor.Name, (accessor.Form == XmlSchemaForm.Qualified) ? accessor.Namespace : "");
                }
            }
            else
            {
                this.WriteEmptyTag(accessor.Name, (accessor.Form == XmlSchemaForm.Qualified) ? accessor.Namespace : "");
            }
            base.Writer.WriteLine("return;");
            IndentedWriter writer3 = base.Writer;
            writer3.Indent--;
            base.Writer.WriteLine("}");
            if ((!mapping.IsSoap && !mapping.TypeDesc.IsValueType) && !mapping.TypeDesc.Type.IsPrimitive)
            {
                base.Writer.WriteLine("TopLevelElement();");
            }
            this.WriteMember("o", null, new ElementAccessor[] { accessor }, null, null, mapping.TypeDesc, !accessor.IsSoap);
            if (mapping.IsSoap)
            {
                base.Writer.WriteLine("WriteReferencedElements();");
            }
            IndentedWriter writer4 = base.Writer;
            writer4.Indent--;
            base.Writer.WriteLine("}");
            return s;
        }

        private string NextMethodName(string name)
        {
            int num2 = ++base.NextMethodNumber;
            return ("Write" + num2.ToString(null, NumberFormatInfo.InvariantInfo) + "_" + CodeIdentifier.MakeValidInternal(name));
        }

        private void WriteArray(string source, string choiceSource, ElementAccessor[] elements, TextAccessor text, ChoiceIdentifierAccessor choice, TypeDesc arrayTypeDesc)
        {
            if ((elements.Length != 0) || (text != null))
            {
                base.Writer.WriteLine("{");
                IndentedWriter writer = base.Writer;
                writer.Indent++;
                string cSharpName = arrayTypeDesc.CSharpName;
                this.WriteArrayLocalDecl(cSharpName, "a", source, arrayTypeDesc);
                if (arrayTypeDesc.IsNullable)
                {
                    base.Writer.WriteLine("if (a != null) {");
                    IndentedWriter writer2 = base.Writer;
                    writer2.Indent++;
                }
                if (choice != null)
                {
                    bool useReflection = choice.Mapping.TypeDesc.UseReflection;
                    this.WriteArrayLocalDecl(choice.Mapping.TypeDesc.CSharpName + "[]", "c", choiceSource, choice.Mapping.TypeDesc);
                    base.Writer.WriteLine("if (c == null || c.Length < a.Length) {");
                    IndentedWriter writer3 = base.Writer;
                    writer3.Indent++;
                    base.Writer.Write("throw CreateInvalidChoiceIdentifierValueException(");
                    base.WriteQuotedCSharpString(choice.Mapping.TypeDesc.FullName);
                    base.Writer.Write(", ");
                    base.WriteQuotedCSharpString(choice.MemberName);
                    base.Writer.Write(");");
                    IndentedWriter writer4 = base.Writer;
                    writer4.Indent--;
                    base.Writer.WriteLine("}");
                }
                this.WriteArrayItems(elements, text, choice, arrayTypeDesc, "a", "c");
                if (arrayTypeDesc.IsNullable)
                {
                    IndentedWriter writer5 = base.Writer;
                    writer5.Indent--;
                    base.Writer.WriteLine("}");
                }
                IndentedWriter writer6 = base.Writer;
                writer6.Indent--;
                base.Writer.WriteLine("}");
            }
        }

        private void WriteArrayItems(ElementAccessor[] elements, TextAccessor text, ChoiceIdentifierAccessor choice, TypeDesc arrayTypeDesc, string arrayName, string choiceName)
        {
            TypeDesc arrayElementTypeDesc = arrayTypeDesc.ArrayElementTypeDesc;
            if (arrayTypeDesc.IsEnumerable)
            {
                base.Writer.Write(typeof(IEnumerator).FullName);
                base.Writer.Write(" e = ");
                if (arrayTypeDesc.IsPrivateImplementation)
                {
                    base.Writer.Write("((");
                    base.Writer.Write(typeof(IEnumerable).FullName);
                    base.Writer.Write(")");
                    base.Writer.Write(arrayName);
                    base.Writer.WriteLine(").GetEnumerator();");
                }
                else if (arrayTypeDesc.IsGenericInterface)
                {
                    if (arrayTypeDesc.UseReflection)
                    {
                        base.Writer.Write("(");
                        base.Writer.Write(typeof(IEnumerator).FullName);
                        base.Writer.Write(")");
                        base.Writer.Write(base.RaCodeGen.GetReflectionVariable(arrayTypeDesc.CSharpName, "System.Collections.Generic.IEnumerable*"));
                        base.Writer.Write(".Invoke(");
                        base.Writer.Write(arrayName);
                        base.Writer.WriteLine(", new object[0]);");
                    }
                    else
                    {
                        base.Writer.Write("((System.Collections.Generic.IEnumerable<");
                        base.Writer.Write(arrayElementTypeDesc.CSharpName);
                        base.Writer.Write(">)");
                        base.Writer.Write(arrayName);
                        base.Writer.WriteLine(").GetEnumerator();");
                    }
                }
                else
                {
                    if (arrayTypeDesc.UseReflection)
                    {
                        base.Writer.Write("(");
                        base.Writer.Write(typeof(IEnumerator).FullName);
                        base.Writer.Write(")");
                    }
                    base.Writer.Write(base.RaCodeGen.GetStringForMethodInvoke(arrayName, arrayTypeDesc.CSharpName, "GetEnumerator", arrayTypeDesc.UseReflection, new string[0]));
                    base.Writer.WriteLine(";");
                }
                base.Writer.WriteLine("if (e != null)");
                base.Writer.WriteLine("while (e.MoveNext()) {");
                IndentedWriter writer1 = base.Writer;
                writer1.Indent++;
                string cSharpName = arrayElementTypeDesc.CSharpName;
                this.WriteLocalDecl(cSharpName, arrayName + "i", "e.Current", arrayElementTypeDesc.UseReflection);
                this.WriteElements(arrayName + "i", choiceName + "i", elements, text, choice, arrayName + "a", true, true);
            }
            else
            {
                base.Writer.Write("for (int i");
                base.Writer.Write(arrayName);
                base.Writer.Write(" = 0; i");
                base.Writer.Write(arrayName);
                base.Writer.Write(" < ");
                if (arrayTypeDesc.IsArray)
                {
                    base.Writer.Write(arrayName);
                    base.Writer.Write(".Length");
                }
                else
                {
                    base.Writer.Write("((");
                    base.Writer.Write(typeof(ICollection).FullName);
                    base.Writer.Write(")");
                    base.Writer.Write(arrayName);
                    base.Writer.Write(").Count");
                }
                base.Writer.Write("; i");
                base.Writer.Write(arrayName);
                base.Writer.WriteLine("++) {");
                IndentedWriter writer2 = base.Writer;
                writer2.Indent++;
                int num = elements.Length + ((text == null) ? 0 : 1);
                if (num > 1)
                {
                    string typeName = arrayElementTypeDesc.CSharpName;
                    this.WriteLocalDecl(typeName, arrayName + "i", base.RaCodeGen.GetStringForArrayMember(arrayName, "i" + arrayName, arrayTypeDesc), arrayElementTypeDesc.UseReflection);
                    if (choice != null)
                    {
                        string str3 = choice.Mapping.TypeDesc.CSharpName;
                        this.WriteLocalDecl(str3, choiceName + "i", base.RaCodeGen.GetStringForArrayMember(choiceName, "i" + arrayName, choice.Mapping.TypeDesc), choice.Mapping.TypeDesc.UseReflection);
                    }
                    this.WriteElements(arrayName + "i", choiceName + "i", elements, text, choice, arrayName + "a", true, arrayElementTypeDesc.IsNullable);
                }
                else
                {
                    this.WriteElements(base.RaCodeGen.GetStringForArrayMember(arrayName, "i" + arrayName, arrayTypeDesc), elements, text, choice, arrayName + "a", true, arrayElementTypeDesc.IsNullable);
                }
            }
            IndentedWriter writer = base.Writer;
            writer.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteArrayLocalDecl(string typeName, string variableName, string initValue, TypeDesc arrayTypeDesc)
        {
            base.RaCodeGen.WriteArrayLocalDecl(typeName, variableName, initValue, arrayTypeDesc);
        }

        private void WriteArrayTypeCompare(string variable, string escapedTypeName, string elementTypeName, bool useReflection)
        {
            base.RaCodeGen.WriteArrayTypeCompare(variable, escapedTypeName, elementTypeName, useReflection);
        }

        private void WriteAttribute(string source, AttributeAccessor attribute, string parent)
        {
            if (attribute.Mapping is SpecialMapping)
            {
                SpecialMapping mapping = (SpecialMapping) attribute.Mapping;
                if ((mapping.TypeDesc.Kind != TypeKind.Attribute) && !mapping.TypeDesc.CanBeAttributeValue)
                {
                    throw new InvalidOperationException(Res.GetString("XmlInternalError"));
                }
                base.Writer.Write("WriteXmlAttribute(");
                base.Writer.Write(source);
                base.Writer.Write(", ");
                base.Writer.Write(parent);
                base.Writer.WriteLine(");");
            }
            else
            {
                TypeDesc typeDesc = attribute.Mapping.TypeDesc;
                if (!typeDesc.UseReflection)
                {
                    source = "((" + typeDesc.CSharpName + ")" + source + ")";
                }
                this.WritePrimitive("WriteAttribute", attribute.Name, (attribute.Form == XmlSchemaForm.Qualified) ? attribute.Namespace : "", attribute.Default, source, attribute.Mapping, false, false, false);
            }
        }

        private void WriteCheckDefault(string source, object value, bool isNullable)
        {
            base.Writer.Write("if (");
            if ((value is string) && (((string) value).Length == 0))
            {
                base.Writer.Write("(");
                base.Writer.Write(source);
                if (isNullable)
                {
                    base.Writer.Write(" == null) || (");
                }
                else
                {
                    base.Writer.Write(" != null) && (");
                }
                base.Writer.Write(source);
                base.Writer.Write(".Length != 0)");
            }
            else
            {
                base.Writer.Write(source);
                base.Writer.Write(" != ");
                this.WriteValue(value);
            }
            base.Writer.Write(")");
        }

        private void WriteChoiceTypeCheck(string source, string fullTypeName, bool useReflection, ChoiceIdentifierAccessor choice, string enumName, TypeDesc typeDesc)
        {
            base.Writer.Write("if (((object)");
            base.Writer.Write(source);
            base.Writer.Write(") != null && !(");
            this.WriteInstanceOf(source, fullTypeName, useReflection);
            base.Writer.Write(")) throw CreateMismatchChoiceException(");
            base.WriteQuotedCSharpString(typeDesc.FullName);
            base.Writer.Write(", ");
            base.WriteQuotedCSharpString(choice.MemberName);
            base.Writer.Write(", ");
            base.WriteQuotedCSharpString(enumName);
            base.Writer.WriteLine(");");
        }

        private void WriteDerivedTypes(StructMapping mapping)
        {
            for (StructMapping mapping2 = mapping.DerivedMappings; mapping2 != null; mapping2 = mapping2.NextDerivedMapping)
            {
                string cSharpName = mapping2.TypeDesc.CSharpName;
                base.Writer.Write("else if (");
                this.WriteTypeCompare("t", cSharpName, mapping2.TypeDesc.UseReflection);
                base.Writer.WriteLine(") {");
                IndentedWriter writer = base.Writer;
                writer.Indent++;
                string s = base.ReferenceMapping(mapping2);
                base.Writer.Write(s);
                base.Writer.Write("(n, ns,");
                if (!mapping2.TypeDesc.UseReflection)
                {
                    base.Writer.Write("(" + cSharpName + ")");
                }
                base.Writer.Write("o");
                if (mapping2.TypeDesc.IsNullable)
                {
                    base.Writer.Write(", isNullable");
                }
                base.Writer.Write(", true");
                base.Writer.WriteLine(");");
                base.Writer.WriteLine("return;");
                IndentedWriter writer2 = base.Writer;
                writer2.Indent--;
                base.Writer.WriteLine("}");
                this.WriteDerivedTypes(mapping2);
            }
        }

        private void WriteElement(string source, ElementAccessor element, string arrayName, bool writeAccessor)
        {
            string str = writeAccessor ? element.Name : element.Mapping.TypeName;
            string str2 = (element.Any && (element.Name.Length == 0)) ? null : ((element.Form == XmlSchemaForm.Qualified) ? (writeAccessor ? element.Namespace : element.Mapping.Namespace) : "");
            if (element.Mapping is NullableMapping)
            {
                base.Writer.Write("if (");
                base.Writer.Write(source);
                base.Writer.WriteLine(" != null) {");
                IndentedWriter writer = base.Writer;
                writer.Indent++;
                string cSharpName = element.Mapping.TypeDesc.BaseTypeDesc.CSharpName;
                string str4 = source;
                if (!element.Mapping.TypeDesc.BaseTypeDesc.UseReflection)
                {
                    str4 = "((" + cSharpName + ")" + source + ")";
                }
                ElementAccessor accessor = element.Clone();
                accessor.Mapping = ((NullableMapping) element.Mapping).BaseMapping;
                this.WriteElement(accessor.Any ? source : str4, accessor, arrayName, writeAccessor);
                IndentedWriter writer2 = base.Writer;
                writer2.Indent--;
                base.Writer.WriteLine("}");
                if (element.IsNullable)
                {
                    base.Writer.WriteLine("else {");
                    IndentedWriter writer3 = base.Writer;
                    writer3.Indent++;
                    this.WriteLiteralNullTag(element.Name, (element.Form == XmlSchemaForm.Qualified) ? element.Namespace : "");
                    IndentedWriter writer4 = base.Writer;
                    writer4.Indent--;
                    base.Writer.WriteLine("}");
                }
            }
            else if (element.Mapping is ArrayMapping)
            {
                ArrayMapping mapping = (ArrayMapping) element.Mapping;
                if (mapping.IsSoap)
                {
                    base.Writer.Write("WritePotentiallyReferencingElement(");
                    base.WriteQuotedCSharpString(str);
                    base.Writer.Write(", ");
                    base.WriteQuotedCSharpString(str2);
                    base.Writer.Write(", ");
                    base.Writer.Write(source);
                    if (!writeAccessor)
                    {
                        base.Writer.Write(", ");
                        base.Writer.Write(base.RaCodeGen.GetStringForTypeof(mapping.TypeDesc.CSharpName, mapping.TypeDesc.UseReflection));
                        base.Writer.Write(", true, ");
                    }
                    else
                    {
                        base.Writer.Write(", null, false, ");
                    }
                    this.WriteValue(element.IsNullable);
                    base.Writer.WriteLine(");");
                }
                else if (element.IsUnbounded)
                {
                    TypeDesc desc = mapping.TypeDesc.CreateArrayTypeDesc();
                    string typeName = desc.CSharpName;
                    string variableName = "el" + arrayName;
                    string s = "c" + variableName;
                    base.Writer.WriteLine("{");
                    IndentedWriter writer5 = base.Writer;
                    writer5.Indent++;
                    this.WriteArrayLocalDecl(typeName, variableName, source, mapping.TypeDesc);
                    if (element.IsNullable)
                    {
                        this.WriteNullCheckBegin(variableName, element);
                    }
                    else
                    {
                        if (mapping.TypeDesc.IsNullable)
                        {
                            base.Writer.Write("if (");
                            base.Writer.Write(variableName);
                            base.Writer.Write(" != null)");
                        }
                        base.Writer.WriteLine("{");
                        IndentedWriter writer6 = base.Writer;
                        writer6.Indent++;
                    }
                    base.Writer.Write("for (int ");
                    base.Writer.Write(s);
                    base.Writer.Write(" = 0; ");
                    base.Writer.Write(s);
                    base.Writer.Write(" < ");
                    if (desc.IsArray)
                    {
                        base.Writer.Write(variableName);
                        base.Writer.Write(".Length");
                    }
                    else
                    {
                        base.Writer.Write("((");
                        base.Writer.Write(typeof(ICollection).FullName);
                        base.Writer.Write(")");
                        base.Writer.Write(variableName);
                        base.Writer.Write(").Count");
                    }
                    base.Writer.Write("; ");
                    base.Writer.Write(s);
                    base.Writer.WriteLine("++) {");
                    IndentedWriter writer7 = base.Writer;
                    writer7.Indent++;
                    element.IsUnbounded = false;
                    this.WriteElement(variableName + "[" + s + "]", element, arrayName, writeAccessor);
                    element.IsUnbounded = true;
                    IndentedWriter writer8 = base.Writer;
                    writer8.Indent--;
                    base.Writer.WriteLine("}");
                    IndentedWriter writer9 = base.Writer;
                    writer9.Indent--;
                    base.Writer.WriteLine("}");
                    IndentedWriter writer10 = base.Writer;
                    writer10.Indent--;
                    base.Writer.WriteLine("}");
                }
                else
                {
                    string str8 = mapping.TypeDesc.CSharpName;
                    base.Writer.WriteLine("{");
                    IndentedWriter writer11 = base.Writer;
                    writer11.Indent++;
                    this.WriteArrayLocalDecl(str8, arrayName, source, mapping.TypeDesc);
                    if (element.IsNullable)
                    {
                        this.WriteNullCheckBegin(arrayName, element);
                    }
                    else
                    {
                        if (mapping.TypeDesc.IsNullable)
                        {
                            base.Writer.Write("if (");
                            base.Writer.Write(arrayName);
                            base.Writer.Write(" != null)");
                        }
                        base.Writer.WriteLine("{");
                        IndentedWriter writer12 = base.Writer;
                        writer12.Indent++;
                    }
                    this.WriteStartElement(str, str2, false);
                    this.WriteArrayItems(mapping.ElementsSortedByDerivation, null, null, mapping.TypeDesc, arrayName, null);
                    this.WriteEndElement();
                    IndentedWriter writer13 = base.Writer;
                    writer13.Indent--;
                    base.Writer.WriteLine("}");
                    IndentedWriter writer14 = base.Writer;
                    writer14.Indent--;
                    base.Writer.WriteLine("}");
                }
            }
            else if (element.Mapping is EnumMapping)
            {
                if (element.Mapping.IsSoap)
                {
                    string str9 = (string) base.MethodNames[element.Mapping];
                    base.Writer.Write("Writer.WriteStartElement(");
                    base.WriteQuotedCSharpString(str);
                    base.Writer.Write(", ");
                    base.WriteQuotedCSharpString(str2);
                    base.Writer.WriteLine(");");
                    base.Writer.Write(str9);
                    base.Writer.Write("(");
                    base.Writer.Write(source);
                    base.Writer.WriteLine(");");
                    this.WriteEndElement();
                }
                else
                {
                    this.WritePrimitive("WriteElementString", str, str2, element.Default, source, element.Mapping, false, true, element.IsNullable);
                }
            }
            else if (element.Mapping is PrimitiveMapping)
            {
                PrimitiveMapping mapping2 = (PrimitiveMapping) element.Mapping;
                if (mapping2.TypeDesc == base.QnameTypeDesc)
                {
                    this.WriteQualifiedNameElement(str, str2, element.Default, source, element.IsNullable, mapping2.IsSoap, mapping2);
                }
                else
                {
                    string str10 = mapping2.IsSoap ? "Encoded" : "Literal";
                    string str11 = mapping2.TypeDesc.XmlEncodingNotRequired ? "Raw" : "";
                    this.WritePrimitive(element.IsNullable ? ("WriteNullableString" + str10 + str11) : ("WriteElementString" + str11), str, str2, element.Default, source, mapping2, mapping2.IsSoap, true, element.IsNullable);
                }
            }
            else if (element.Mapping is StructMapping)
            {
                StructMapping mapping3 = (StructMapping) element.Mapping;
                if (mapping3.IsSoap)
                {
                    base.Writer.Write("WritePotentiallyReferencingElement(");
                    base.WriteQuotedCSharpString(str);
                    base.Writer.Write(", ");
                    base.WriteQuotedCSharpString(str2);
                    base.Writer.Write(", ");
                    base.Writer.Write(source);
                    if (!writeAccessor)
                    {
                        base.Writer.Write(", ");
                        base.Writer.Write(base.RaCodeGen.GetStringForTypeof(mapping3.TypeDesc.CSharpName, mapping3.TypeDesc.UseReflection));
                        base.Writer.Write(", true, ");
                    }
                    else
                    {
                        base.Writer.Write(", null, false, ");
                    }
                    this.WriteValue(element.IsNullable);
                }
                else
                {
                    string str12 = base.ReferenceMapping(mapping3);
                    base.Writer.Write(str12);
                    base.Writer.Write("(");
                    base.WriteQuotedCSharpString(str);
                    base.Writer.Write(", ");
                    if (str2 == null)
                    {
                        base.Writer.Write("null");
                    }
                    else
                    {
                        base.WriteQuotedCSharpString(str2);
                    }
                    base.Writer.Write(", ");
                    base.Writer.Write(source);
                    if (mapping3.TypeDesc.IsNullable)
                    {
                        base.Writer.Write(", ");
                        this.WriteValue(element.IsNullable);
                    }
                    base.Writer.Write(", false");
                }
                base.Writer.WriteLine(");");
            }
            else
            {
                if (!(element.Mapping is SpecialMapping))
                {
                    throw new InvalidOperationException(Res.GetString("XmlInternalError"));
                }
                SpecialMapping mapping4 = (SpecialMapping) element.Mapping;
                bool useReflection = mapping4.TypeDesc.UseReflection;
                string text1 = mapping4.TypeDesc.CSharpName;
                if (element.Mapping is SerializableMapping)
                {
                    this.WriteElementCall("WriteSerializable", typeof(IXmlSerializable), source, str, str2, element.IsNullable, !element.Any);
                }
                else
                {
                    base.Writer.Write("if ((");
                    base.Writer.Write(source);
                    base.Writer.Write(") is ");
                    base.Writer.Write(typeof(XmlNode).FullName);
                    base.Writer.Write(" || ");
                    base.Writer.Write(source);
                    base.Writer.Write(" == null");
                    base.Writer.WriteLine(") {");
                    IndentedWriter writer15 = base.Writer;
                    writer15.Indent++;
                    this.WriteElementCall("WriteElementLiteral", typeof(XmlNode), source, str, str2, element.IsNullable, element.Any);
                    IndentedWriter writer16 = base.Writer;
                    writer16.Indent--;
                    base.Writer.WriteLine("}");
                    base.Writer.WriteLine("else {");
                    IndentedWriter writer17 = base.Writer;
                    writer17.Indent++;
                    base.Writer.Write("throw CreateInvalidAnyTypeException(");
                    base.Writer.Write(source);
                    base.Writer.WriteLine(");");
                    IndentedWriter writer18 = base.Writer;
                    writer18.Indent--;
                    base.Writer.WriteLine("}");
                }
            }
        }

        private void WriteElementCall(string func, Type cast, string source, string name, string ns, bool isNullable, bool isAny)
        {
            base.Writer.Write(func);
            base.Writer.Write("((");
            base.Writer.Write(cast.FullName);
            base.Writer.Write(")");
            base.Writer.Write(source);
            base.Writer.Write(", ");
            base.WriteQuotedCSharpString(name);
            base.Writer.Write(", ");
            base.WriteQuotedCSharpString(ns);
            base.Writer.Write(", ");
            this.WriteValue(isNullable);
            base.Writer.Write(", ");
            this.WriteValue(isAny);
            base.Writer.WriteLine(");");
        }

        private void WriteElements(string source, ElementAccessor[] elements, TextAccessor text, ChoiceIdentifierAccessor choice, string arrayName, bool writeAccessors, bool isNullable)
        {
            this.WriteElements(source, null, elements, text, choice, arrayName, writeAccessors, isNullable);
        }

        private void WriteElements(string source, string enumSource, ElementAccessor[] elements, TextAccessor text, ChoiceIdentifierAccessor choice, string arrayName, bool writeAccessors, bool isNullable)
        {
            if ((elements.Length != 0) || (text != null))
            {
                if ((elements.Length == 1) && (text == null))
                {
                    TypeDesc desc = elements[0].IsUnbounded ? elements[0].Mapping.TypeDesc.CreateArrayTypeDesc() : elements[0].Mapping.TypeDesc;
                    if ((!elements[0].Any && !elements[0].Mapping.TypeDesc.UseReflection) && !elements[0].Mapping.TypeDesc.IsOptionalValue)
                    {
                        source = "((" + desc.CSharpName + ")" + source + ")";
                    }
                    this.WriteElement(source, elements[0], arrayName, writeAccessors);
                }
                else
                {
                    if (isNullable && (choice == null))
                    {
                        base.Writer.Write("if ((object)(");
                        base.Writer.Write(source);
                        base.Writer.Write(") != null)");
                    }
                    base.Writer.WriteLine("{");
                    IndentedWriter writer = base.Writer;
                    writer.Indent++;
                    int num = 0;
                    ArrayList list = new ArrayList();
                    ElementAccessor element = null;
                    bool flag = false;
                    string str = (choice == null) ? null : choice.Mapping.TypeDesc.FullName;
                    for (int i = 0; i < elements.Length; i++)
                    {
                        ElementAccessor accessor2 = elements[i];
                        if (accessor2.Any)
                        {
                            num++;
                            if ((accessor2.Name != null) && (accessor2.Name.Length > 0))
                            {
                                list.Add(accessor2);
                            }
                            else if (element == null)
                            {
                                element = accessor2;
                            }
                        }
                        else if (choice != null)
                        {
                            bool useReflection = accessor2.Mapping.TypeDesc.UseReflection;
                            string cSharpName = accessor2.Mapping.TypeDesc.CSharpName;
                            bool flag3 = choice.Mapping.TypeDesc.UseReflection;
                            string s = (flag3 ? "" : (str + ".@")) + this.FindChoiceEnumValue(accessor2, (EnumMapping) choice.Mapping, flag3);
                            if (flag)
                            {
                                base.Writer.Write("else ");
                            }
                            else
                            {
                                flag = true;
                            }
                            base.Writer.Write("if (");
                            base.Writer.Write(flag3 ? base.RaCodeGen.GetStringForEnumLongValue(enumSource, flag3) : enumSource);
                            base.Writer.Write(" == ");
                            base.Writer.Write(s);
                            if (isNullable && !accessor2.IsNullable)
                            {
                                base.Writer.Write(" && ((object)(");
                                base.Writer.Write(source);
                                base.Writer.Write(") != null)");
                            }
                            base.Writer.WriteLine(") {");
                            IndentedWriter writer2 = base.Writer;
                            writer2.Indent++;
                            this.WriteChoiceTypeCheck(source, cSharpName, useReflection, choice, s, accessor2.Mapping.TypeDesc);
                            string str4 = source;
                            if (!useReflection)
                            {
                                str4 = "((" + cSharpName + ")" + source + ")";
                            }
                            this.WriteElement(accessor2.Any ? source : str4, accessor2, arrayName, writeAccessors);
                            IndentedWriter writer3 = base.Writer;
                            writer3.Indent--;
                            base.Writer.WriteLine("}");
                        }
                        else
                        {
                            bool flag4 = accessor2.Mapping.TypeDesc.UseReflection;
                            TypeDesc desc2 = accessor2.IsUnbounded ? accessor2.Mapping.TypeDesc.CreateArrayTypeDesc() : accessor2.Mapping.TypeDesc;
                            string escapedTypeName = desc2.CSharpName;
                            if (flag)
                            {
                                base.Writer.Write("else ");
                            }
                            else
                            {
                                flag = true;
                            }
                            base.Writer.Write("if (");
                            this.WriteInstanceOf(source, escapedTypeName, flag4);
                            base.Writer.WriteLine(") {");
                            IndentedWriter writer4 = base.Writer;
                            writer4.Indent++;
                            string str6 = source;
                            if (!flag4)
                            {
                                str6 = "((" + escapedTypeName + ")" + source + ")";
                            }
                            this.WriteElement(accessor2.Any ? source : str6, accessor2, arrayName, writeAccessors);
                            IndentedWriter writer5 = base.Writer;
                            writer5.Indent--;
                            base.Writer.WriteLine("}");
                        }
                    }
                    if (num > 0)
                    {
                        if ((elements.Length - num) > 0)
                        {
                            base.Writer.Write("else ");
                        }
                        string fullName = typeof(XmlElement).FullName;
                        base.Writer.Write("if (");
                        base.Writer.Write(source);
                        base.Writer.Write(" is ");
                        base.Writer.Write(fullName);
                        base.Writer.WriteLine(") {");
                        IndentedWriter writer6 = base.Writer;
                        writer6.Indent++;
                        base.Writer.Write(fullName);
                        base.Writer.Write(" elem = (");
                        base.Writer.Write(fullName);
                        base.Writer.Write(")");
                        base.Writer.Write(source);
                        base.Writer.WriteLine(";");
                        int num3 = 0;
                        foreach (ElementAccessor accessor3 in list)
                        {
                            if (num3++ > 0)
                            {
                                base.Writer.Write("else ");
                            }
                            string str8 = null;
                            bool flag1 = accessor3.Mapping.TypeDesc.UseReflection;
                            if (choice != null)
                            {
                                bool flag5 = choice.Mapping.TypeDesc.UseReflection;
                                str8 = (flag5 ? "" : (str + ".@")) + this.FindChoiceEnumValue(accessor3, (EnumMapping) choice.Mapping, flag5);
                                base.Writer.Write("if (");
                                base.Writer.Write(flag5 ? base.RaCodeGen.GetStringForEnumLongValue(enumSource, flag5) : enumSource);
                                base.Writer.Write(" == ");
                                base.Writer.Write(str8);
                                if (isNullable && !accessor3.IsNullable)
                                {
                                    base.Writer.Write(" && ((object)(");
                                    base.Writer.Write(source);
                                    base.Writer.Write(") != null)");
                                }
                                base.Writer.WriteLine(") {");
                                IndentedWriter writer7 = base.Writer;
                                writer7.Indent++;
                            }
                            base.Writer.Write("if (elem.Name == ");
                            base.WriteQuotedCSharpString(accessor3.Name);
                            base.Writer.Write(" && elem.NamespaceURI == ");
                            base.WriteQuotedCSharpString(accessor3.Namespace);
                            base.Writer.WriteLine(") {");
                            IndentedWriter writer8 = base.Writer;
                            writer8.Indent++;
                            this.WriteElement("elem", accessor3, arrayName, writeAccessors);
                            if (choice != null)
                            {
                                IndentedWriter writer9 = base.Writer;
                                writer9.Indent--;
                                base.Writer.WriteLine("}");
                                base.Writer.WriteLine("else {");
                                IndentedWriter writer10 = base.Writer;
                                writer10.Indent++;
                                base.Writer.WriteLine("// throw Value '{0}' of the choice identifier '{1}' does not match element '{2}' from namespace '{3}'.");
                                base.Writer.Write("throw CreateChoiceIdentifierValueException(");
                                base.WriteQuotedCSharpString(str8);
                                base.Writer.Write(", ");
                                base.WriteQuotedCSharpString(choice.MemberName);
                                base.Writer.WriteLine(", elem.Name, elem.NamespaceURI);");
                                IndentedWriter writer11 = base.Writer;
                                writer11.Indent--;
                                base.Writer.WriteLine("}");
                            }
                            IndentedWriter writer12 = base.Writer;
                            writer12.Indent--;
                            base.Writer.WriteLine("}");
                        }
                        if (num3 > 0)
                        {
                            base.Writer.WriteLine("else {");
                            IndentedWriter writer13 = base.Writer;
                            writer13.Indent++;
                        }
                        if (element != null)
                        {
                            this.WriteElement("elem", element, arrayName, writeAccessors);
                        }
                        else
                        {
                            base.Writer.WriteLine("throw CreateUnknownAnyElementException(elem.Name, elem.NamespaceURI);");
                        }
                        if (num3 > 0)
                        {
                            IndentedWriter writer14 = base.Writer;
                            writer14.Indent--;
                            base.Writer.WriteLine("}");
                        }
                        IndentedWriter writer15 = base.Writer;
                        writer15.Indent--;
                        base.Writer.WriteLine("}");
                    }
                    if (text != null)
                    {
                        bool flag6 = text.Mapping.TypeDesc.UseReflection;
                        string str9 = text.Mapping.TypeDesc.CSharpName;
                        if (elements.Length > 0)
                        {
                            base.Writer.Write("else ");
                            base.Writer.Write("if (");
                            this.WriteInstanceOf(source, str9, flag6);
                            base.Writer.WriteLine(") {");
                            IndentedWriter writer16 = base.Writer;
                            writer16.Indent++;
                            string str10 = source;
                            if (!flag6)
                            {
                                str10 = "((" + str9 + ")" + source + ")";
                            }
                            this.WriteText(str10, text);
                            IndentedWriter writer17 = base.Writer;
                            writer17.Indent--;
                            base.Writer.WriteLine("}");
                        }
                        else
                        {
                            string str11 = source;
                            if (!flag6)
                            {
                                str11 = "((" + str9 + ")" + source + ")";
                            }
                            this.WriteText(str11, text);
                        }
                    }
                    if (elements.Length > 0)
                    {
                        base.Writer.Write("else ");
                        if (isNullable)
                        {
                            base.Writer.Write(" if ((object)(");
                            base.Writer.Write(source);
                            base.Writer.Write(") != null)");
                        }
                        base.Writer.WriteLine("{");
                        IndentedWriter writer18 = base.Writer;
                        writer18.Indent++;
                        base.Writer.Write("throw CreateUnknownTypeException(");
                        base.Writer.Write(source);
                        base.Writer.WriteLine(");");
                        IndentedWriter writer19 = base.Writer;
                        writer19.Indent--;
                        base.Writer.WriteLine("}");
                    }
                    IndentedWriter writer20 = base.Writer;
                    writer20.Indent--;
                    base.Writer.WriteLine("}");
                }
            }
        }

        private void WriteEmptyTag(string name, string ns)
        {
            this.WriteTag("WriteEmptyTag", name, ns);
        }

        private void WriteEncodedNullTag(string name, string ns)
        {
            this.WriteTag("WriteNullTagEncoded", name, ns);
        }

        private void WriteEndElement()
        {
            base.Writer.WriteLine("WriteEndElement();");
        }

        private void WriteEndElement(string source)
        {
            base.Writer.Write("WriteEndElement(");
            base.Writer.Write(source);
            base.Writer.WriteLine(");");
        }

        private void WriteEnumAndArrayTypes()
        {
            foreach (TypeScope scope in base.Scopes)
            {
                foreach (Mapping mapping in scope.TypeMappings)
                {
                    if ((mapping is EnumMapping) && !mapping.IsSoap)
                    {
                        EnumMapping mapping2 = (EnumMapping) mapping;
                        string cSharpName = mapping2.TypeDesc.CSharpName;
                        base.Writer.Write("else if (");
                        this.WriteTypeCompare("t", cSharpName, mapping2.TypeDesc.UseReflection);
                        base.Writer.WriteLine(") {");
                        IndentedWriter writer = base.Writer;
                        writer.Indent++;
                        string s = base.ReferenceMapping(mapping2);
                        base.Writer.WriteLine("Writer.WriteStartElement(n, ns);");
                        base.Writer.Write("WriteXsiType(");
                        base.WriteQuotedCSharpString(mapping2.TypeName);
                        base.Writer.Write(", ");
                        base.WriteQuotedCSharpString(mapping2.Namespace);
                        base.Writer.WriteLine(");");
                        base.Writer.Write("Writer.WriteString(");
                        base.Writer.Write(s);
                        base.Writer.Write("(");
                        if (!mapping2.TypeDesc.UseReflection)
                        {
                            base.Writer.Write("(" + cSharpName + ")");
                        }
                        base.Writer.WriteLine("o));");
                        base.Writer.WriteLine("Writer.WriteEndElement();");
                        base.Writer.WriteLine("return;");
                        IndentedWriter writer2 = base.Writer;
                        writer2.Indent--;
                        base.Writer.WriteLine("}");
                    }
                    else if ((mapping is ArrayMapping) && !mapping.IsSoap)
                    {
                        ArrayMapping mapping3 = mapping as ArrayMapping;
                        if ((mapping3 != null) && !mapping.IsSoap)
                        {
                            string escapedTypeName = mapping3.TypeDesc.CSharpName;
                            base.Writer.Write("else if (");
                            if (mapping3.TypeDesc.IsArray)
                            {
                                this.WriteArrayTypeCompare("t", escapedTypeName, mapping3.TypeDesc.ArrayElementTypeDesc.CSharpName, mapping3.TypeDesc.UseReflection);
                            }
                            else
                            {
                                this.WriteTypeCompare("t", escapedTypeName, mapping3.TypeDesc.UseReflection);
                            }
                            base.Writer.WriteLine(") {");
                            IndentedWriter writer3 = base.Writer;
                            writer3.Indent++;
                            base.Writer.WriteLine("Writer.WriteStartElement(n, ns);");
                            base.Writer.Write("WriteXsiType(");
                            base.WriteQuotedCSharpString(mapping3.TypeName);
                            base.Writer.Write(", ");
                            base.WriteQuotedCSharpString(mapping3.Namespace);
                            base.Writer.WriteLine(");");
                            this.WriteMember("o", null, mapping3.ElementsSortedByDerivation, null, null, mapping3.TypeDesc, true);
                            base.Writer.WriteLine("Writer.WriteEndElement();");
                            base.Writer.WriteLine("return;");
                            IndentedWriter writer4 = base.Writer;
                            writer4.Indent--;
                            base.Writer.WriteLine("}");
                        }
                    }
                }
            }
        }

        private void WriteEnumCase(string fullTypeName, ConstantMapping c, bool useReflection)
        {
            base.RaCodeGen.WriteEnumCase(fullTypeName, c, useReflection);
        }

        private void WriteEnumMethod(EnumMapping mapping)
        {
            string s = (string) base.MethodNames[mapping];
            base.Writer.WriteLine();
            string cSharpName = mapping.TypeDesc.CSharpName;
            if (mapping.IsSoap)
            {
                base.Writer.Write("void ");
                base.Writer.Write(s);
                base.Writer.WriteLine("(object e) {");
                this.WriteLocalDecl(cSharpName, "v", "e", mapping.TypeDesc.UseReflection);
            }
            else
            {
                base.Writer.Write("string ");
                base.Writer.Write(s);
                base.Writer.Write("(");
                base.Writer.Write(mapping.TypeDesc.UseReflection ? "object" : cSharpName);
                base.Writer.WriteLine(" v) {");
            }
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine("string s = null;");
            ConstantMapping[] constants = mapping.Constants;
            if (constants.Length > 0)
            {
                Hashtable hashtable = new Hashtable();
                if (mapping.TypeDesc.UseReflection)
                {
                    base.Writer.WriteLine("switch (" + base.RaCodeGen.GetStringForEnumLongValue("v", mapping.TypeDesc.UseReflection) + " ){");
                }
                else
                {
                    base.Writer.WriteLine("switch (v) {");
                }
                IndentedWriter writer2 = base.Writer;
                writer2.Indent++;
                for (int i = 0; i < constants.Length; i++)
                {
                    ConstantMapping c = constants[i];
                    if (hashtable[c.Value] == null)
                    {
                        this.WriteEnumCase(cSharpName, c, mapping.TypeDesc.UseReflection);
                        base.Writer.Write("s = ");
                        base.WriteQuotedCSharpString(c.XmlName);
                        base.Writer.WriteLine("; break;");
                        hashtable.Add(c.Value, c.Value);
                    }
                }
                if (mapping.IsFlags)
                {
                    base.Writer.Write("default: s = FromEnum(");
                    base.Writer.Write(base.RaCodeGen.GetStringForEnumLongValue("v", mapping.TypeDesc.UseReflection));
                    base.Writer.Write(", new string[] {");
                    IndentedWriter writer3 = base.Writer;
                    writer3.Indent++;
                    for (int j = 0; j < constants.Length; j++)
                    {
                        ConstantMapping mapping3 = constants[j];
                        if (j > 0)
                        {
                            base.Writer.WriteLine(", ");
                        }
                        base.WriteQuotedCSharpString(mapping3.XmlName);
                    }
                    base.Writer.Write("}, new ");
                    base.Writer.Write(typeof(long).FullName);
                    base.Writer.Write("[] {");
                    for (int k = 0; k < constants.Length; k++)
                    {
                        ConstantMapping mapping4 = constants[k];
                        if (k > 0)
                        {
                            base.Writer.WriteLine(", ");
                        }
                        base.Writer.Write("(long)");
                        if (mapping.TypeDesc.UseReflection)
                        {
                            base.Writer.Write(mapping4.Value.ToString(CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            base.Writer.Write(cSharpName);
                            base.Writer.Write(".@");
                            CodeIdentifier.CheckValidIdentifier(mapping4.Name);
                            base.Writer.Write(mapping4.Name);
                        }
                    }
                    IndentedWriter writer4 = base.Writer;
                    writer4.Indent--;
                    base.Writer.Write("}, ");
                    base.WriteQuotedCSharpString(mapping.TypeDesc.FullName);
                    base.Writer.WriteLine("); break;");
                }
                else
                {
                    base.Writer.Write("default: throw CreateInvalidEnumValueException(");
                    base.Writer.Write(base.RaCodeGen.GetStringForEnumLongValue("v", mapping.TypeDesc.UseReflection));
                    base.Writer.Write(".ToString(System.Globalization.CultureInfo.InvariantCulture), ");
                    base.WriteQuotedCSharpString(mapping.TypeDesc.FullName);
                    base.Writer.WriteLine(");");
                }
                IndentedWriter writer5 = base.Writer;
                writer5.Indent--;
                base.Writer.WriteLine("}");
            }
            if (mapping.IsSoap)
            {
                base.Writer.Write("WriteXsiType(");
                base.WriteQuotedCSharpString(mapping.TypeName);
                base.Writer.Write(", ");
                base.WriteQuotedCSharpString(mapping.Namespace);
                base.Writer.WriteLine(");");
                base.Writer.WriteLine("Writer.WriteString(s);");
            }
            else
            {
                base.Writer.WriteLine("return s;");
            }
            IndentedWriter writer6 = base.Writer;
            writer6.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteEnumValue(EnumMapping mapping, string source)
        {
            string s = base.ReferenceMapping(mapping);
            base.Writer.Write(s);
            base.Writer.Write("(");
            base.Writer.Write(source);
            base.Writer.Write(")");
        }

        private void WriteExtraMembers(string loopStartSource, string loopEndSource)
        {
            base.Writer.Write("for (int i = ");
            base.Writer.Write(loopStartSource);
            base.Writer.Write("; i < ");
            base.Writer.Write(loopEndSource);
            base.Writer.WriteLine("; i++) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            base.Writer.WriteLine("if (p[i] != null) {");
            IndentedWriter writer2 = base.Writer;
            writer2.Indent++;
            base.Writer.WriteLine("WritePotentiallyReferencingElement(null, null, p[i], p[i].GetType(), true, false);");
            IndentedWriter writer3 = base.Writer;
            writer3.Indent--;
            base.Writer.WriteLine("}");
            IndentedWriter writer4 = base.Writer;
            writer4.Indent--;
            base.Writer.WriteLine("}");
        }

        private void WriteInstanceOf(string source, string escapedTypeName, bool useReflection)
        {
            base.RaCodeGen.WriteInstanceOf(source, escapedTypeName, useReflection);
        }

        private void WriteLiteralNullTag(string name, string ns)
        {
            this.WriteTag("WriteNullTagLiteral", name, ns);
        }

        private void WriteLocalDecl(string typeName, string variableName, string initValue, bool useReflection)
        {
            base.RaCodeGen.WriteLocalDecl(typeName, variableName, initValue, useReflection);
        }

        private void WriteMember(string source, AttributeAccessor attribute, TypeDesc memberTypeDesc, string parent)
        {
            if (!memberTypeDesc.IsAbstract)
            {
                if (memberTypeDesc.IsArrayLike)
                {
                    base.Writer.WriteLine("{");
                    IndentedWriter writer = base.Writer;
                    writer.Indent++;
                    string cSharpName = memberTypeDesc.CSharpName;
                    this.WriteArrayLocalDecl(cSharpName, "a", source, memberTypeDesc);
                    if (memberTypeDesc.IsNullable)
                    {
                        base.Writer.WriteLine("if (a != null) {");
                        IndentedWriter writer2 = base.Writer;
                        writer2.Indent++;
                    }
                    if (attribute.IsList)
                    {
                        if (this.CanOptimizeWriteListSequence(memberTypeDesc.ArrayElementTypeDesc))
                        {
                            base.Writer.Write("Writer.WriteStartAttribute(null, ");
                            base.WriteQuotedCSharpString(attribute.Name);
                            base.Writer.Write(", ");
                            string str2 = (attribute.Form == XmlSchemaForm.Qualified) ? attribute.Namespace : string.Empty;
                            if (str2 != null)
                            {
                                base.WriteQuotedCSharpString(str2);
                            }
                            else
                            {
                                base.Writer.Write("null");
                            }
                            base.Writer.WriteLine(");");
                        }
                        else
                        {
                            base.Writer.Write(typeof(StringBuilder).FullName);
                            base.Writer.Write(" sb = new ");
                            base.Writer.Write(typeof(StringBuilder).FullName);
                            base.Writer.WriteLine("();");
                        }
                    }
                    TypeDesc arrayElementTypeDesc = memberTypeDesc.ArrayElementTypeDesc;
                    if (memberTypeDesc.IsEnumerable)
                    {
                        base.Writer.Write(" e = ");
                        base.Writer.Write(typeof(IEnumerator).FullName);
                        if (memberTypeDesc.IsPrivateImplementation)
                        {
                            base.Writer.Write("((");
                            base.Writer.Write(typeof(IEnumerable).FullName);
                            base.Writer.WriteLine(").GetEnumerator();");
                        }
                        else if (memberTypeDesc.IsGenericInterface)
                        {
                            if (memberTypeDesc.UseReflection)
                            {
                                base.Writer.Write("(");
                                base.Writer.Write(typeof(IEnumerator).FullName);
                                base.Writer.Write(")");
                                base.Writer.Write(base.RaCodeGen.GetReflectionVariable(memberTypeDesc.CSharpName, "System.Collections.Generic.IEnumerable*"));
                                base.Writer.WriteLine(".Invoke(a, new object[0]);");
                            }
                            else
                            {
                                base.Writer.Write("((System.Collections.Generic.IEnumerable<");
                                base.Writer.Write(arrayElementTypeDesc.CSharpName);
                                base.Writer.WriteLine(">)a).GetEnumerator();");
                            }
                        }
                        else
                        {
                            if (memberTypeDesc.UseReflection)
                            {
                                base.Writer.Write("(");
                                base.Writer.Write(typeof(IEnumerator).FullName);
                                base.Writer.Write(")");
                            }
                            base.Writer.Write(base.RaCodeGen.GetStringForMethodInvoke("a", memberTypeDesc.CSharpName, "GetEnumerator", memberTypeDesc.UseReflection, new string[0]));
                            base.Writer.WriteLine(";");
                        }
                        base.Writer.WriteLine("if (e != null)");
                        base.Writer.WriteLine("while (e.MoveNext()) {");
                        IndentedWriter writer3 = base.Writer;
                        writer3.Indent++;
                        string typeName = arrayElementTypeDesc.CSharpName;
                        this.WriteLocalDecl(typeName, "ai", "e.Current", arrayElementTypeDesc.UseReflection);
                    }
                    else
                    {
                        base.Writer.Write("for (int i = 0; i < ");
                        if (memberTypeDesc.IsArray)
                        {
                            base.Writer.WriteLine("a.Length; i++) {");
                        }
                        else
                        {
                            base.Writer.Write("((");
                            base.Writer.Write(typeof(ICollection).FullName);
                            base.Writer.WriteLine(")a).Count; i++) {");
                        }
                        IndentedWriter writer4 = base.Writer;
                        writer4.Indent++;
                        string str4 = arrayElementTypeDesc.CSharpName;
                        this.WriteLocalDecl(str4, "ai", base.RaCodeGen.GetStringForArrayMember("a", "i", memberTypeDesc), arrayElementTypeDesc.UseReflection);
                    }
                    if (attribute.IsList)
                    {
                        if (this.CanOptimizeWriteListSequence(memberTypeDesc.ArrayElementTypeDesc))
                        {
                            base.Writer.WriteLine("if (i != 0) Writer.WriteString(\" \");");
                            base.Writer.Write("WriteValue(");
                        }
                        else
                        {
                            base.Writer.WriteLine("if (i != 0) sb.Append(\" \");");
                            base.Writer.Write("sb.Append(");
                        }
                        if (attribute.Mapping is EnumMapping)
                        {
                            this.WriteEnumValue((EnumMapping) attribute.Mapping, "ai");
                        }
                        else
                        {
                            this.WritePrimitiveValue(arrayElementTypeDesc, "ai", true);
                        }
                        base.Writer.WriteLine(");");
                    }
                    else
                    {
                        this.WriteAttribute("ai", attribute, parent);
                    }
                    IndentedWriter writer5 = base.Writer;
                    writer5.Indent--;
                    base.Writer.WriteLine("}");
                    if (attribute.IsList)
                    {
                        if (this.CanOptimizeWriteListSequence(memberTypeDesc.ArrayElementTypeDesc))
                        {
                            base.Writer.WriteLine("Writer.WriteEndAttribute();");
                        }
                        else
                        {
                            base.Writer.WriteLine("if (sb.Length != 0) {");
                            IndentedWriter writer6 = base.Writer;
                            writer6.Indent++;
                            base.Writer.Write("WriteAttribute(");
                            base.WriteQuotedCSharpString(attribute.Name);
                            base.Writer.Write(", ");
                            string str5 = (attribute.Form == XmlSchemaForm.Qualified) ? attribute.Namespace : string.Empty;
                            if (str5 != null)
                            {
                                base.WriteQuotedCSharpString(str5);
                                base.Writer.Write(", ");
                            }
                            base.Writer.WriteLine("sb.ToString());");
                            IndentedWriter writer7 = base.Writer;
                            writer7.Indent--;
                            base.Writer.WriteLine("}");
                        }
                    }
                    if (memberTypeDesc.IsNullable)
                    {
                        IndentedWriter writer8 = base.Writer;
                        writer8.Indent--;
                        base.Writer.WriteLine("}");
                    }
                    IndentedWriter writer9 = base.Writer;
                    writer9.Indent--;
                    base.Writer.WriteLine("}");
                }
                else
                {
                    this.WriteAttribute(source, attribute, parent);
                }
            }
        }

        private void WriteMember(string source, string choiceSource, ElementAccessor[] elements, TextAccessor text, ChoiceIdentifierAccessor choice, TypeDesc memberTypeDesc, bool writeAccessors)
        {
            if (memberTypeDesc.IsArrayLike && ((elements.Length != 1) || !(elements[0].Mapping is ArrayMapping)))
            {
                this.WriteArray(source, choiceSource, elements, text, choice, memberTypeDesc);
            }
            else
            {
                this.WriteElements(source, choiceSource, elements, text, choice, "a", writeAccessors, memberTypeDesc.IsNullable);
            }
        }

        private void WriteNamespaces(string source)
        {
            base.Writer.Write("WriteNamespaceDeclarations(");
            base.Writer.Write(source);
            base.Writer.WriteLine(");");
        }

        private void WriteNullCheckBegin(string source, ElementAccessor element)
        {
            base.Writer.Write("if ((object)(");
            base.Writer.Write(source);
            base.Writer.WriteLine(") == null) {");
            IndentedWriter writer = base.Writer;
            writer.Indent++;
            this.WriteLiteralNullTag(element.Name, (element.Form == XmlSchemaForm.Qualified) ? element.Namespace : "");
            IndentedWriter writer2 = base.Writer;
            writer2.Indent--;
            base.Writer.WriteLine("}");
            base.Writer.WriteLine("else {");
            IndentedWriter writer3 = base.Writer;
            writer3.Indent++;
        }

        private void WritePrimitive(string method, string name, string ns, object defaultValue, string source, TypeMapping mapping, bool writeXsiType, bool isElement, bool isNullable)
        {
            TypeDesc typeDesc = mapping.TypeDesc;
            bool flag = ((defaultValue != null) && (defaultValue != DBNull.Value)) && mapping.TypeDesc.HasDefaultSupport;
            if (flag)
            {
                if (mapping is EnumMapping)
                {
                    base.Writer.Write("if (");
                    if (mapping.TypeDesc.UseReflection)
                    {
                        base.Writer.Write(base.RaCodeGen.GetStringForEnumLongValue(source, mapping.TypeDesc.UseReflection));
                    }
                    else
                    {
                        base.Writer.Write(source);
                    }
                    base.Writer.Write(" != ");
                    if (((EnumMapping) mapping).IsFlags)
                    {
                        base.Writer.Write("(");
                        string[] strArray = ((string) defaultValue).Split(null);
                        for (int i = 0; i < strArray.Length; i++)
                        {
                            if ((strArray[i] != null) && (strArray[i].Length != 0))
                            {
                                if (i > 0)
                                {
                                    base.Writer.WriteLine(" | ");
                                }
                                base.Writer.Write(base.RaCodeGen.GetStringForEnumCompare((EnumMapping) mapping, strArray[i], mapping.TypeDesc.UseReflection));
                            }
                        }
                        base.Writer.Write(")");
                    }
                    else
                    {
                        base.Writer.Write(base.RaCodeGen.GetStringForEnumCompare((EnumMapping) mapping, (string) defaultValue, mapping.TypeDesc.UseReflection));
                    }
                    base.Writer.Write(")");
                }
                else
                {
                    this.WriteCheckDefault(source, defaultValue, isNullable);
                }
                base.Writer.WriteLine(" {");
                IndentedWriter writer = base.Writer;
                writer.Indent++;
            }
            base.Writer.Write(method);
            base.Writer.Write("(");
            base.WriteQuotedCSharpString(name);
            if (ns != null)
            {
                base.Writer.Write(", ");
                base.WriteQuotedCSharpString(ns);
            }
            base.Writer.Write(", ");
            if (mapping is EnumMapping)
            {
                this.WriteEnumValue((EnumMapping) mapping, source);
            }
            else
            {
                this.WritePrimitiveValue(typeDesc, source, isElement);
            }
            if (writeXsiType)
            {
                base.Writer.Write(", new System.Xml.XmlQualifiedName(");
                base.WriteQuotedCSharpString(mapping.TypeName);
                base.Writer.Write(", ");
                base.WriteQuotedCSharpString(mapping.Namespace);
                base.Writer.Write(")");
            }
            base.Writer.WriteLine(");");
            if (flag)
            {
                IndentedWriter writer2 = base.Writer;
                writer2.Indent--;
                base.Writer.WriteLine("}");
            }
        }

        private void WritePrimitiveValue(TypeDesc typeDesc, string source, bool isElement)
        {
            if ((typeDesc == base.StringTypeDesc) || (typeDesc.FormatterName == "String"))
            {
                base.Writer.Write(source);
            }
            else if (!typeDesc.HasCustomFormatter)
            {
                base.Writer.Write(typeof(XmlConvert).FullName);
                base.Writer.Write(".ToString((");
                base.Writer.Write(typeDesc.CSharpName);
                base.Writer.Write(")");
                base.Writer.Write(source);
                base.Writer.Write(")");
            }
            else
            {
                base.Writer.Write("From");
                base.Writer.Write(typeDesc.FormatterName);
                base.Writer.Write("(");
                base.Writer.Write(source);
                base.Writer.Write(")");
            }
        }

        private void WriteQualifiedNameElement(string name, string ns, object defaultValue, string source, bool nullable, bool IsSoap, TypeMapping mapping)
        {
            bool flag = (defaultValue != null) && (defaultValue != DBNull.Value);
            if (flag)
            {
                this.WriteCheckDefault(source, defaultValue, nullable);
                base.Writer.WriteLine(" {");
                IndentedWriter writer = base.Writer;
                writer.Indent++;
            }
            string str = IsSoap ? "Encoded" : "Literal";
            base.Writer.Write(nullable ? ("WriteNullableQualifiedName" + str) : "WriteElementQualifiedName");
            base.Writer.Write("(");
            base.WriteQuotedCSharpString(name);
            if (ns != null)
            {
                base.Writer.Write(", ");
                base.WriteQuotedCSharpString(ns);
            }
            base.Writer.Write(", ");
            base.Writer.Write(source);
            if (IsSoap)
            {
                base.Writer.Write(", new System.Xml.XmlQualifiedName(");
                base.WriteQuotedCSharpString(mapping.TypeName);
                base.Writer.Write(", ");
                base.WriteQuotedCSharpString(mapping.Namespace);
                base.Writer.Write(")");
            }
            base.Writer.WriteLine(");");
            if (flag)
            {
                IndentedWriter writer2 = base.Writer;
                writer2.Indent--;
                base.Writer.WriteLine("}");
            }
        }

        private void WriteStartElement(string name, string ns, bool writePrefixed)
        {
            this.WriteTag("WriteStartElement", name, ns, writePrefixed);
        }

        private void WriteStructMethod(StructMapping mapping)
        {
            if (!mapping.IsSoap || !mapping.TypeDesc.IsRoot)
            {
                string s = (string) base.MethodNames[mapping];
                base.Writer.WriteLine();
                base.Writer.Write("void ");
                base.Writer.Write(s);
                string cSharpName = mapping.TypeDesc.CSharpName;
                if (mapping.IsSoap)
                {
                    base.Writer.WriteLine("(object s) {");
                    IndentedWriter writer1 = base.Writer;
                    writer1.Indent++;
                    this.WriteLocalDecl(cSharpName, "o", "s", mapping.TypeDesc.UseReflection);
                }
                else
                {
                    base.Writer.Write("(string n, string ns, ");
                    base.Writer.Write(mapping.TypeDesc.UseReflection ? "object" : cSharpName);
                    base.Writer.Write(" o");
                    if (mapping.TypeDesc.IsNullable)
                    {
                        base.Writer.Write(", bool isNullable");
                    }
                    base.Writer.WriteLine(", bool needType) {");
                    IndentedWriter writer2 = base.Writer;
                    writer2.Indent++;
                    if (mapping.TypeDesc.IsNullable)
                    {
                        base.Writer.WriteLine("if ((object)o == null) {");
                        IndentedWriter writer3 = base.Writer;
                        writer3.Indent++;
                        base.Writer.WriteLine("if (isNullable) WriteNullTagLiteral(n, ns);");
                        base.Writer.WriteLine("return;");
                        IndentedWriter writer4 = base.Writer;
                        writer4.Indent--;
                        base.Writer.WriteLine("}");
                    }
                    base.Writer.WriteLine("if (!needType) {");
                    IndentedWriter writer5 = base.Writer;
                    writer5.Indent++;
                    base.Writer.Write(typeof(Type).FullName);
                    base.Writer.WriteLine(" t = o.GetType();");
                    base.Writer.Write("if (");
                    this.WriteTypeCompare("t", cSharpName, mapping.TypeDesc.UseReflection);
                    base.Writer.WriteLine(") {");
                    base.Writer.WriteLine("}");
                    this.WriteDerivedTypes(mapping);
                    if (mapping.TypeDesc.IsRoot)
                    {
                        this.WriteEnumAndArrayTypes();
                    }
                    base.Writer.WriteLine("else {");
                    IndentedWriter writer6 = base.Writer;
                    writer6.Indent++;
                    if (mapping.TypeDesc.IsRoot)
                    {
                        base.Writer.WriteLine("WriteTypedPrimitive(n, ns, o, true);");
                        base.Writer.WriteLine("return;");
                    }
                    else
                    {
                        base.Writer.WriteLine("throw CreateUnknownTypeException(o);");
                    }
                    IndentedWriter writer7 = base.Writer;
                    writer7.Indent--;
                    base.Writer.WriteLine("}");
                    IndentedWriter writer8 = base.Writer;
                    writer8.Indent--;
                    base.Writer.WriteLine("}");
                }
                if (!mapping.TypeDesc.IsAbstract)
                {
                    if ((mapping.TypeDesc.Type != null) && typeof(XmlSchemaObject).IsAssignableFrom(mapping.TypeDesc.Type))
                    {
                        base.Writer.WriteLine("EscapeName = false;");
                    }
                    string str3 = null;
                    MemberMapping[] allMembers = TypeScope.GetAllMembers(mapping);
                    int index = this.FindXmlnsIndex(allMembers);
                    if (index >= 0)
                    {
                        MemberMapping mapping2 = allMembers[index];
                        CodeIdentifier.CheckValidIdentifier(mapping2.Name);
                        str3 = base.RaCodeGen.GetStringForMember("o", mapping2.Name, mapping.TypeDesc);
                        if (mapping.TypeDesc.UseReflection)
                        {
                            str3 = "((" + mapping2.TypeDesc.CSharpName + ")" + str3 + ")";
                        }
                    }
                    if (!mapping.IsSoap)
                    {
                        base.Writer.Write("WriteStartElement(n, ns, o, false, ");
                        if (str3 == null)
                        {
                            base.Writer.Write("null");
                        }
                        else
                        {
                            base.Writer.Write(str3);
                        }
                        base.Writer.WriteLine(");");
                        if (!mapping.TypeDesc.IsRoot)
                        {
                            base.Writer.Write("if (needType) WriteXsiType(");
                            base.WriteQuotedCSharpString(mapping.TypeName);
                            base.Writer.Write(", ");
                            base.WriteQuotedCSharpString(mapping.Namespace);
                            base.Writer.WriteLine(");");
                        }
                    }
                    else if (str3 != null)
                    {
                        this.WriteNamespaces(str3);
                    }
                    for (int i = 0; i < allMembers.Length; i++)
                    {
                        MemberMapping mapping3 = allMembers[i];
                        if (mapping3.Attribute != null)
                        {
                            CodeIdentifier.CheckValidIdentifier(mapping3.Name);
                            if (mapping3.CheckShouldPersist)
                            {
                                base.Writer.Write("if (");
                                string str4 = base.RaCodeGen.GetStringForMethodInvoke("o", cSharpName, "ShouldSerialize" + mapping3.Name, mapping.TypeDesc.UseReflection, new string[0]);
                                if (mapping.TypeDesc.UseReflection)
                                {
                                    str4 = "((" + typeof(bool).FullName + ")" + str4 + ")";
                                }
                                base.Writer.Write(str4);
                                base.Writer.WriteLine(") {");
                                IndentedWriter writer9 = base.Writer;
                                writer9.Indent++;
                            }
                            if (mapping3.CheckSpecified != SpecifiedAccessor.None)
                            {
                                base.Writer.Write("if (");
                                string str5 = base.RaCodeGen.GetStringForMember("o", mapping3.Name + "Specified", mapping.TypeDesc);
                                if (mapping.TypeDesc.UseReflection)
                                {
                                    str5 = "((" + typeof(bool).FullName + ")" + str5 + ")";
                                }
                                base.Writer.Write(str5);
                                base.Writer.WriteLine(") {");
                                IndentedWriter writer10 = base.Writer;
                                writer10.Indent++;
                            }
                            this.WriteMember(base.RaCodeGen.GetStringForMember("o", mapping3.Name, mapping.TypeDesc), mapping3.Attribute, mapping3.TypeDesc, "o");
                            if (mapping3.CheckSpecified != SpecifiedAccessor.None)
                            {
                                IndentedWriter writer11 = base.Writer;
                                writer11.Indent--;
                                base.Writer.WriteLine("}");
                            }
                            if (mapping3.CheckShouldPersist)
                            {
                                IndentedWriter writer12 = base.Writer;
                                writer12.Indent--;
                                base.Writer.WriteLine("}");
                            }
                        }
                    }
                    for (int j = 0; j < allMembers.Length; j++)
                    {
                        MemberMapping mapping4 = allMembers[j];
                        if (mapping4.Xmlns == null)
                        {
                            CodeIdentifier.CheckValidIdentifier(mapping4.Name);
                            bool flag = mapping4.CheckShouldPersist && ((mapping4.Elements.Length > 0) || (mapping4.Text != null));
                            if (flag)
                            {
                                base.Writer.Write("if (");
                                string str6 = base.RaCodeGen.GetStringForMethodInvoke("o", cSharpName, "ShouldSerialize" + mapping4.Name, mapping.TypeDesc.UseReflection, new string[0]);
                                if (mapping.TypeDesc.UseReflection)
                                {
                                    str6 = "((" + typeof(bool).FullName + ")" + str6 + ")";
                                }
                                base.Writer.Write(str6);
                                base.Writer.WriteLine(") {");
                                IndentedWriter writer13 = base.Writer;
                                writer13.Indent++;
                            }
                            if (mapping4.CheckSpecified != SpecifiedAccessor.None)
                            {
                                base.Writer.Write("if (");
                                string str7 = base.RaCodeGen.GetStringForMember("o", mapping4.Name + "Specified", mapping.TypeDesc);
                                if (mapping.TypeDesc.UseReflection)
                                {
                                    str7 = "((" + typeof(bool).FullName + ")" + str7 + ")";
                                }
                                base.Writer.Write(str7);
                                base.Writer.WriteLine(") {");
                                IndentedWriter writer14 = base.Writer;
                                writer14.Indent++;
                            }
                            string choiceSource = null;
                            if (mapping4.ChoiceIdentifier != null)
                            {
                                CodeIdentifier.CheckValidIdentifier(mapping4.ChoiceIdentifier.MemberName);
                                choiceSource = base.RaCodeGen.GetStringForMember("o", mapping4.ChoiceIdentifier.MemberName, mapping.TypeDesc);
                            }
                            this.WriteMember(base.RaCodeGen.GetStringForMember("o", mapping4.Name, mapping.TypeDesc), choiceSource, mapping4.ElementsSortedByDerivation, mapping4.Text, mapping4.ChoiceIdentifier, mapping4.TypeDesc, true);
                            if (mapping4.CheckSpecified != SpecifiedAccessor.None)
                            {
                                IndentedWriter writer15 = base.Writer;
                                writer15.Indent--;
                                base.Writer.WriteLine("}");
                            }
                            if (flag)
                            {
                                IndentedWriter writer16 = base.Writer;
                                writer16.Indent--;
                                base.Writer.WriteLine("}");
                            }
                        }
                    }
                    if (!mapping.IsSoap)
                    {
                        this.WriteEndElement("o");
                    }
                }
                IndentedWriter writer = base.Writer;
                writer.Indent--;
                base.Writer.WriteLine("}");
            }
        }

        private void WriteTag(string methodName, string name, string ns)
        {
            base.Writer.Write(methodName);
            base.Writer.Write("(");
            base.WriteQuotedCSharpString(name);
            base.Writer.Write(", ");
            if (ns == null)
            {
                base.Writer.Write("null");
            }
            else
            {
                base.WriteQuotedCSharpString(ns);
            }
            base.Writer.WriteLine(");");
        }

        private void WriteTag(string methodName, string name, string ns, bool writePrefixed)
        {
            base.Writer.Write(methodName);
            base.Writer.Write("(");
            base.WriteQuotedCSharpString(name);
            base.Writer.Write(", ");
            if (ns == null)
            {
                base.Writer.Write("null");
            }
            else
            {
                base.WriteQuotedCSharpString(ns);
            }
            base.Writer.Write(", null, ");
            if (writePrefixed)
            {
                base.Writer.Write("true");
            }
            else
            {
                base.Writer.Write("false");
            }
            base.Writer.WriteLine(");");
        }

        private void WriteText(string source, TextAccessor text)
        {
            if (text.Mapping is PrimitiveMapping)
            {
                PrimitiveMapping mapping = (PrimitiveMapping) text.Mapping;
                base.Writer.Write("WriteValue(");
                if (text.Mapping is EnumMapping)
                {
                    this.WriteEnumValue((EnumMapping) text.Mapping, source);
                }
                else
                {
                    this.WritePrimitiveValue(mapping.TypeDesc, source, false);
                }
                base.Writer.WriteLine(");");
            }
            else if (text.Mapping is SpecialMapping)
            {
                SpecialMapping mapping2 = (SpecialMapping) text.Mapping;
                if (mapping2.TypeDesc.Kind != TypeKind.Node)
                {
                    throw new InvalidOperationException(Res.GetString("XmlInternalError"));
                }
                base.Writer.Write(source);
                base.Writer.WriteLine(".WriteTo(Writer);");
            }
        }

        private void WriteTypeCompare(string variable, string escapedTypeName, bool useReflection)
        {
            base.RaCodeGen.WriteTypeCompare(variable, escapedTypeName, useReflection);
        }

        private void WriteValue(object value)
        {
            if (value == null)
            {
                base.Writer.Write("null");
            }
            else
            {
                Type type = value.GetType();
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        base.Writer.Write(((bool) value) ? "true" : "false");
                        return;

                    case TypeCode.Char:
                    {
                        base.Writer.Write('\'');
                        char c = (char) value;
                        if (c != '\'')
                        {
                            base.Writer.Write(c);
                            break;
                        }
                        base.Writer.Write("'");
                        break;
                    }
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        base.Writer.Write("(");
                        base.Writer.Write(type.FullName);
                        base.Writer.Write(")");
                        base.Writer.Write("(");
                        base.Writer.Write(Convert.ToString(value, NumberFormatInfo.InvariantInfo));
                        base.Writer.Write(")");
                        return;

                    case TypeCode.Int32:
                    {
                        int num = (int) value;
                        base.Writer.Write(num.ToString(null, NumberFormatInfo.InvariantInfo));
                        return;
                    }
                    case TypeCode.Single:
                    {
                        float num3 = (float) value;
                        base.Writer.Write(num3.ToString("R", NumberFormatInfo.InvariantInfo));
                        base.Writer.Write("f");
                        return;
                    }
                    case TypeCode.Double:
                    {
                        double num2 = (double) value;
                        base.Writer.Write(num2.ToString("R", NumberFormatInfo.InvariantInfo));
                        return;
                    }
                    case TypeCode.Decimal:
                    {
                        decimal num4 = (decimal) value;
                        base.Writer.Write(num4.ToString(null, NumberFormatInfo.InvariantInfo));
                        base.Writer.Write("m");
                        return;
                    }
                    case TypeCode.DateTime:
                    {
                        base.Writer.Write(" new ");
                        base.Writer.Write(type.FullName);
                        base.Writer.Write("(");
                        DateTime time = (DateTime) value;
                        base.Writer.Write(time.Ticks.ToString(CultureInfo.InvariantCulture));
                        base.Writer.Write(")");
                        return;
                    }
                    case TypeCode.String:
                    {
                        string str = (string) value;
                        base.WriteQuotedCSharpString(str);
                        return;
                    }
                    default:
                    {
                        if (!type.IsEnum)
                        {
                            throw new InvalidOperationException(Res.GetString("XmlUnsupportedDefaultType", new object[] { type.FullName }));
                        }
                        int num6 = (int) value;
                        base.Writer.Write(num6.ToString(null, NumberFormatInfo.InvariantInfo));
                        return;
                    }
                }
                base.Writer.Write('\'');
            }
        }
    }
}

