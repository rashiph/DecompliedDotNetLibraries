namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    internal class ReflectionAwareCodeGen
    {
        private const string arrayMemberKey = "0";
        private static string helperClassesForUseReflection = "\r\n    sealed class XSFieldInfo {{\r\n       {3} fieldInfo;\r\n        public XSFieldInfo({2} t, {1} memberName){{\r\n            fieldInfo = t.GetField(memberName);\r\n        }}\r\n        public {0} this[{0} o] {{\r\n            get {{\r\n                return fieldInfo.GetValue(o);\r\n            }}\r\n            set {{\r\n                fieldInfo.SetValue(o, value);\r\n            }}\r\n        }}\r\n\r\n    }}\r\n    sealed class XSPropInfo {{\r\n        {4} propInfo;\r\n        public XSPropInfo({2} t, {1} memberName){{\r\n            propInfo = t.GetProperty(memberName);\r\n        }}\r\n        public {0} this[{0} o] {{\r\n            get {{\r\n                return propInfo.GetValue(o, null);\r\n            }}\r\n            set {{\r\n                propInfo.SetValue(o, value, null);\r\n            }}\r\n        }}\r\n    }}\r\n    sealed class XSArrayInfo {{\r\n        {4} propInfo;\r\n        public XSArrayInfo({4} propInfo){{\r\n            this.propInfo = propInfo;\r\n        }}\r\n        public {0} this[{0} a, int i] {{\r\n            get {{\r\n                return propInfo.GetValue(a, new {0}[]{{i}});\r\n            }}\r\n            set {{\r\n                propInfo.SetValue(a, value, new {0}[]{{i}});\r\n            }}\r\n        }}\r\n    }}\r\n";
        private const string hexDigits = "0123456789ABCDEF";
        private int nextReflectionVariableNumber;
        private Hashtable reflectionVariables;
        private IndentedWriter writer;

        internal ReflectionAwareCodeGen(IndentedWriter writer)
        {
            this.writer = writer;
        }

        private string GenerateVariableName(string prefix, string fullName)
        {
            this.nextReflectionVariableNumber++;
            return string.Concat(new object[] { prefix, this.nextReflectionVariableNumber, "_", CodeIdentifier.MakeValidInternal(fullName.Replace('.', '_')) });
        }

        internal string GetReflectionVariable(string typeFullName, string memberName)
        {
            string str;
            if (memberName == null)
            {
                str = typeFullName;
            }
            else
            {
                str = memberName + ":" + typeFullName;
            }
            return (string) this.reflectionVariables[str];
        }

        internal string GetStringForArrayMember(string arrayName, string subscript, TypeDesc arrayTypeDesc)
        {
            if (!arrayTypeDesc.UseReflection)
            {
                return (arrayName + "[" + subscript + "]");
            }
            string typeFullName = arrayTypeDesc.IsCollection ? arrayTypeDesc.CSharpName : typeof(Array).FullName;
            string reflectionVariable = this.GetReflectionVariable(typeFullName, "0");
            return (reflectionVariable + "[" + arrayName + ", " + subscript + "]");
        }

        internal string GetStringForCreateInstance(string escapedTypeName, bool useReflection, bool ctorInaccessible, bool cast)
        {
            return this.GetStringForCreateInstance(escapedTypeName, useReflection, ctorInaccessible, cast, string.Empty);
        }

        internal string GetStringForCreateInstance(string type, string cast, bool nonPublic, string arg)
        {
            StringBuilder builder = new StringBuilder();
            if ((cast != null) && (cast.Length > 0))
            {
                builder.Append("(");
                builder.Append(cast);
                builder.Append(")");
            }
            builder.Append(typeof(Activator).FullName);
            builder.Append(".CreateInstance(");
            builder.Append(type);
            builder.Append(", ");
            string fullName = typeof(BindingFlags).FullName;
            builder.Append(fullName);
            builder.Append(".Instance | ");
            builder.Append(fullName);
            builder.Append(".Public | ");
            builder.Append(fullName);
            builder.Append(".CreateInstance");
            if (nonPublic)
            {
                builder.Append(" | ");
                builder.Append(fullName);
                builder.Append(".NonPublic");
            }
            if ((arg == null) || (arg.Length == 0))
            {
                builder.Append(", null, new object[0], null)");
            }
            else
            {
                builder.Append(", null, new object[] { ");
                builder.Append(arg);
                builder.Append(" }, null)");
            }
            return builder.ToString();
        }

        internal string GetStringForCreateInstance(string escapedTypeName, bool useReflection, bool ctorInaccessible, bool cast, string arg)
        {
            if (!useReflection && !ctorInaccessible)
            {
                return ("new " + escapedTypeName + "(" + arg + ")");
            }
            return this.GetStringForCreateInstance(this.GetStringForTypeof(escapedTypeName, useReflection), (cast && !useReflection) ? escapedTypeName : null, ctorInaccessible, arg);
        }

        internal string GetStringForEnumCompare(EnumMapping mapping, string memberName, bool useReflection)
        {
            if (!useReflection)
            {
                CodeIdentifier.CheckValidIdentifier(memberName);
                return (mapping.TypeDesc.CSharpName + ".@" + memberName);
            }
            string variable = this.GetStringForEnumMember(mapping.TypeDesc.CSharpName, memberName, useReflection);
            return this.GetStringForEnumLongValue(variable, useReflection);
        }

        internal string GetStringForEnumLongValue(string variable, bool useReflection)
        {
            if (useReflection)
            {
                return (typeof(Convert).FullName + ".ToInt64(" + variable + ")");
            }
            return ("((" + typeof(long).FullName + ")" + variable + ")");
        }

        internal string GetStringForEnumMember(string typeFullName, string memberName, bool useReflection)
        {
            if (!useReflection)
            {
                return (typeFullName + ".@" + memberName);
            }
            return (this.GetReflectionVariable(typeFullName, memberName) + "[null]");
        }

        internal string GetStringForMember(string obj, string memberName, TypeDesc typeDesc)
        {
            if (typeDesc.UseReflection)
            {
                while (typeDesc != null)
                {
                    string cSharpName = typeDesc.CSharpName;
                    string reflectionVariable = this.GetReflectionVariable(cSharpName, memberName);
                    if (reflectionVariable != null)
                    {
                        return (reflectionVariable + "[" + obj + "]");
                    }
                    typeDesc = typeDesc.BaseTypeDesc;
                    if ((typeDesc != null) && !typeDesc.UseReflection)
                    {
                        return ("((" + typeDesc.CSharpName + ")" + obj + ").@" + memberName);
                    }
                }
                return ("[" + obj + "]");
            }
            return (obj + ".@" + memberName);
        }

        internal string GetStringForMethod(string obj, string typeFullName, string memberName, bool useReflection)
        {
            if (!useReflection)
            {
                return (obj + "." + memberName + "(");
            }
            return (this.GetReflectionVariable(typeFullName, memberName) + ".Invoke(" + obj + ", new object[]{");
        }

        internal string GetStringForMethodInvoke(string obj, string escapedTypeName, string methodName, bool useReflection, params string[] args)
        {
            StringBuilder builder = new StringBuilder();
            if (useReflection)
            {
                builder.Append(this.GetReflectionVariable(escapedTypeName, methodName));
                builder.Append(".Invoke(");
                builder.Append(obj);
                builder.Append(", new object[] {");
            }
            else
            {
                builder.Append(obj);
                builder.Append(".@");
                builder.Append(methodName);
                builder.Append("(");
            }
            for (int i = 0; i < args.Length; i++)
            {
                if (i != 0)
                {
                    builder.Append(", ");
                }
                builder.Append(args[i]);
            }
            if (useReflection)
            {
                builder.Append("})");
            }
            else
            {
                builder.Append(")");
            }
            return builder.ToString();
        }

        internal string GetStringForTypeof(string typeFullName, bool useReflection)
        {
            if (useReflection)
            {
                return this.GetReflectionVariable(typeFullName, null);
            }
            return ("typeof(" + typeFullName + ")");
        }

        private void InitTheFirstTime()
        {
            if (this.reflectionVariables == null)
            {
                this.reflectionVariables = new Hashtable();
                this.writer.Write(string.Format(CultureInfo.InvariantCulture, helperClassesForUseReflection, new object[] { "object", "string", typeof(Type).FullName, typeof(FieldInfo).FullName, typeof(PropertyInfo).FullName, typeof(MemberInfo).FullName, typeof(MemberTypes).FullName }));
                this.WriteDefaultIndexerInit(typeof(IList), typeof(Array).FullName, false, false);
            }
        }

        internal void WriteArrayLocalDecl(string typeName, string variableName, string initValue, TypeDesc arrayTypeDesc)
        {
            if (arrayTypeDesc.UseReflection)
            {
                if (arrayTypeDesc.IsEnumerable)
                {
                    typeName = typeof(IEnumerable).FullName;
                }
                else if (arrayTypeDesc.IsCollection)
                {
                    typeName = typeof(ICollection).FullName;
                }
                else
                {
                    typeName = typeof(Array).FullName;
                }
            }
            this.writer.Write(typeName);
            this.writer.Write(" ");
            this.writer.Write(variableName);
            if (initValue != null)
            {
                this.writer.Write(" = ");
                if (initValue != "null")
                {
                    this.writer.Write("(" + typeName + ")");
                }
                this.writer.Write(initValue);
            }
            this.writer.WriteLine(";");
        }

        internal void WriteArrayTypeCompare(string variable, string escapedTypeName, string elementTypeName, bool useReflection)
        {
            if (!useReflection)
            {
                this.writer.Write(variable);
                this.writer.Write(" == typeof(");
                this.writer.Write(escapedTypeName);
                this.writer.Write(")");
            }
            else
            {
                this.writer.Write(variable);
                this.writer.Write(".IsArray ");
                this.writer.Write(" && ");
                this.WriteTypeCompare(variable + ".GetElementType()", elementTypeName, useReflection);
            }
        }

        private string WriteAssemblyInfo(Type type)
        {
            string fullName = type.Assembly.FullName;
            string str2 = (string) this.reflectionVariables[fullName];
            if (str2 == null)
            {
                int index = fullName.IndexOf(',');
                string str3 = (index > -1) ? fullName.Substring(0, index) : fullName;
                str2 = this.GenerateVariableName("assembly", str3);
                this.writer.Write("static " + typeof(Assembly).FullName + " " + str2 + " = ResolveDynamicAssembly(");
                this.WriteQuotedCSharpString(DynamicAssemblies.GetName(type.Assembly));
                this.writer.WriteLine(");");
                this.reflectionVariables.Add(fullName, str2);
            }
            return str2;
        }

        private void WriteCollectionInfo(string typeVariable, TypeDesc typeDesc, Type type)
        {
            string cSharpName = CodeIdentifier.GetCSharpName(type);
            string typeFullName = typeDesc.ArrayElementTypeDesc.CSharpName;
            bool useReflection = typeDesc.ArrayElementTypeDesc.UseReflection;
            if (typeDesc.IsCollection)
            {
                this.WriteDefaultIndexerInit(type, cSharpName, typeDesc.UseReflection, useReflection);
            }
            else if (typeDesc.IsEnumerable)
            {
                if (typeDesc.IsGenericInterface)
                {
                    this.WriteMethodInfo(cSharpName, typeVariable, "System.Collections.Generic.IEnumerable*", true, new string[0]);
                }
                else if (!typeDesc.IsPrivateImplementation)
                {
                    this.WriteMethodInfo(cSharpName, typeVariable, "GetEnumerator", true, new string[0]);
                }
            }
            this.WriteMethodInfo(cSharpName, typeVariable, "Add", false, new string[] { this.GetStringForTypeof(typeFullName, useReflection) });
        }

        internal void WriteCreateInstance(string escapedName, string source, bool useReflection, bool ctorInaccessible)
        {
            this.writer.Write(useReflection ? "object" : escapedName);
            this.writer.Write(" ");
            this.writer.Write(source);
            this.writer.Write(" = ");
            this.writer.Write(this.GetStringForCreateInstance(escapedName, useReflection, ctorInaccessible, !useReflection && ctorInaccessible));
            this.writer.WriteLine(";");
        }

        private string WriteDefaultIndexerInit(Type type, string escapedName, bool collectionUseReflection, bool elementUseReflection)
        {
            string s = this.GenerateVariableName("item", escapedName);
            PropertyInfo defaultIndexer = TypeScope.GetDefaultIndexer(type, null);
            this.writer.Write("static XSArrayInfo ");
            this.writer.Write(s);
            this.writer.Write("= new XSArrayInfo(");
            this.writer.Write(this.GetStringForTypeof(CodeIdentifier.GetCSharpName(type), collectionUseReflection));
            this.writer.Write(".GetProperty(");
            this.WriteQuotedCSharpString(defaultIndexer.Name);
            this.writer.Write(",");
            this.writer.Write(this.GetStringForTypeof(CodeIdentifier.GetCSharpName(defaultIndexer.PropertyType), elementUseReflection));
            this.writer.Write(",new ");
            this.writer.Write(typeof(Type[]).FullName);
            this.writer.WriteLine("{typeof(int)}));");
            this.reflectionVariables.Add("0:" + escapedName, s);
            return s;
        }

        internal void WriteEnumCase(string fullTypeName, ConstantMapping c, bool useReflection)
        {
            this.writer.Write("case ");
            if (useReflection)
            {
                this.writer.Write(c.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                this.writer.Write(fullTypeName);
                this.writer.Write(".@");
                CodeIdentifier.CheckValidIdentifier(c.Name);
                this.writer.Write(c.Name);
            }
            this.writer.Write(": ");
        }

        internal void WriteInstanceOf(string source, string escapedTypeName, bool useReflection)
        {
            if (!useReflection)
            {
                this.writer.Write(source);
                this.writer.Write(" is ");
                this.writer.Write(escapedTypeName);
            }
            else
            {
                this.writer.Write(this.GetReflectionVariable(escapedTypeName, null));
                this.writer.Write(".IsAssignableFrom(");
                this.writer.Write(source);
                this.writer.Write(".GetType())");
            }
        }

        internal void WriteLocalDecl(string typeFullName, string variableName, string initValue, bool useReflection)
        {
            if (useReflection)
            {
                typeFullName = "object";
            }
            this.writer.Write(typeFullName);
            this.writer.Write(" ");
            this.writer.Write(variableName);
            if (initValue != null)
            {
                this.writer.Write(" = ");
                if (!useReflection && (initValue != "null"))
                {
                    this.writer.Write("(" + typeFullName + ")");
                }
                this.writer.Write(initValue);
            }
            this.writer.WriteLine(";");
        }

        private void WriteMappingInfo(TypeMapping mapping, string typeVariable, Type type)
        {
            string cSharpName = mapping.TypeDesc.CSharpName;
            if (mapping is StructMapping)
            {
                StructMapping mapping2 = mapping as StructMapping;
                for (int i = 0; i < mapping2.Members.Length; i++)
                {
                    MemberMapping mapping3 = mapping2.Members[i];
                    this.WriteMemberInfo(type, cSharpName, typeVariable, mapping3.Name);
                    if (mapping3.CheckShouldPersist)
                    {
                        string memberName = "ShouldSerialize" + mapping3.Name;
                        this.WriteMethodInfo(cSharpName, typeVariable, memberName, false, new string[0]);
                    }
                    if (mapping3.CheckSpecified != SpecifiedAccessor.None)
                    {
                        string str3 = mapping3.Name + "Specified";
                        this.WriteMemberInfo(type, cSharpName, typeVariable, str3);
                    }
                    if (mapping3.ChoiceIdentifier != null)
                    {
                        string str4 = mapping3.ChoiceIdentifier.MemberName;
                        this.WriteMemberInfo(type, cSharpName, typeVariable, str4);
                    }
                }
            }
            else if (mapping is EnumMapping)
            {
                FieldInfo[] fields = type.GetFields();
                for (int j = 0; j < fields.Length; j++)
                {
                    this.WriteMemberInfo(type, cSharpName, typeVariable, fields[j].Name);
                }
            }
        }

        private string WriteMemberInfo(Type type, string escapedName, string typeVariable, string memberName)
        {
            MemberInfo[] member = type.GetMember(memberName);
            for (int i = 0; i < member.Length; i++)
            {
                switch (member[i].MemberType)
                {
                    case MemberTypes.Property:
                    {
                        string str = this.GenerateVariableName("prop", memberName);
                        this.writer.Write("static XSPropInfo " + str + " = new XSPropInfo(" + typeVariable + ", ");
                        this.WriteQuotedCSharpString(memberName);
                        this.writer.WriteLine(");");
                        this.reflectionVariables.Add(memberName + ":" + escapedName, str);
                        return str;
                    }
                    case MemberTypes.Field:
                    {
                        string str2 = this.GenerateVariableName("field", memberName);
                        this.writer.Write("static XSFieldInfo " + str2 + " = new XSFieldInfo(" + typeVariable + ", ");
                        this.WriteQuotedCSharpString(memberName);
                        this.writer.WriteLine(");");
                        this.reflectionVariables.Add(memberName + ":" + escapedName, str2);
                        return str2;
                    }
                }
            }
            throw new InvalidOperationException(Res.GetString("XmlSerializerUnsupportedType", new object[] { member[0].ToString() }));
        }

        private string WriteMethodInfo(string escapedName, string typeVariable, string memberName, bool isNonPublic, params string[] paramTypes)
        {
            string str = this.GenerateVariableName("method", memberName);
            this.writer.Write("static " + typeof(MethodInfo).FullName + " " + str + " = " + typeVariable + ".GetMethod(");
            this.WriteQuotedCSharpString(memberName);
            this.writer.Write(", ");
            string fullName = typeof(BindingFlags).FullName;
            this.writer.Write(fullName);
            this.writer.Write(".Public | ");
            this.writer.Write(fullName);
            this.writer.Write(".Instance | ");
            this.writer.Write(fullName);
            this.writer.Write(".Static");
            if (isNonPublic)
            {
                this.writer.Write(" | ");
                this.writer.Write(fullName);
                this.writer.Write(".NonPublic");
            }
            this.writer.Write(", null, ");
            this.writer.Write("new " + typeof(Type).FullName + "[] { ");
            for (int i = 0; i < paramTypes.Length; i++)
            {
                this.writer.Write(paramTypes[i]);
                if (i < (paramTypes.Length - 1))
                {
                    this.writer.Write(", ");
                }
            }
            this.writer.WriteLine("}, null);");
            this.reflectionVariables.Add(memberName + ":" + escapedName, str);
            return str;
        }

        internal void WriteQuotedCSharpString(string value)
        {
            WriteQuotedCSharpString(this.writer, value);
        }

        internal static void WriteQuotedCSharpString(IndentedWriter writer, string value)
        {
            if (value == null)
            {
                writer.Write("null");
            }
            else
            {
                writer.Write("@\"");
                foreach (char ch in value)
                {
                    if (ch < ' ')
                    {
                        if (ch == '\r')
                        {
                            writer.Write(@"\r");
                        }
                        else if (ch == '\n')
                        {
                            writer.Write(@"\n");
                        }
                        else if (ch == '\t')
                        {
                            writer.Write(@"\t");
                        }
                        else
                        {
                            byte num = (byte) ch;
                            writer.Write(@"\x");
                            writer.Write("0123456789ABCDEF"[num >> 4]);
                            writer.Write("0123456789ABCDEF"[num & 15]);
                        }
                    }
                    else if (ch == '"')
                    {
                        writer.Write("\"\"");
                    }
                    else
                    {
                        writer.Write(ch);
                    }
                }
                writer.Write("\"");
            }
        }

        internal void WriteReflectionInit(TypeScope scope)
        {
            foreach (Type type in scope.Types)
            {
                TypeDesc typeDesc = scope.GetTypeDesc(type);
                if (typeDesc.UseReflection)
                {
                    this.WriteTypeInfo(scope, typeDesc, type);
                }
            }
        }

        internal void WriteTypeCompare(string variable, string escapedTypeName, bool useReflection)
        {
            this.writer.Write(variable);
            this.writer.Write(" == ");
            this.writer.Write(this.GetStringForTypeof(escapedTypeName, useReflection));
        }

        private string WriteTypeInfo(TypeScope scope, TypeDesc typeDesc, Type type)
        {
            this.InitTheFirstTime();
            string cSharpName = typeDesc.CSharpName;
            string str2 = (string) this.reflectionVariables[cSharpName];
            if (str2 == null)
            {
                if (type.IsArray)
                {
                    str2 = this.GenerateVariableName("array", typeDesc.CSharpName);
                    TypeDesc arrayElementTypeDesc = typeDesc.ArrayElementTypeDesc;
                    if (arrayElementTypeDesc.UseReflection)
                    {
                        string str3 = this.WriteTypeInfo(scope, arrayElementTypeDesc, scope.GetTypeFromTypeDesc(arrayElementTypeDesc));
                        this.writer.WriteLine("static " + typeof(Type).FullName + " " + str2 + " = " + str3 + ".MakeArrayType();");
                    }
                    else
                    {
                        string str4 = this.WriteAssemblyInfo(type);
                        this.writer.Write("static " + typeof(Type).FullName + " " + str2 + " = " + str4 + ".GetType(");
                        this.WriteQuotedCSharpString(type.FullName);
                        this.writer.WriteLine(");");
                    }
                }
                else
                {
                    str2 = this.GenerateVariableName("type", typeDesc.CSharpName);
                    Type underlyingType = Nullable.GetUnderlyingType(type);
                    if (underlyingType != null)
                    {
                        string str5 = this.WriteTypeInfo(scope, scope.GetTypeDesc(underlyingType), underlyingType);
                        this.writer.WriteLine("static " + typeof(Type).FullName + " " + str2 + " = typeof(System.Nullable<>).MakeGenericType(new " + typeof(Type).FullName + "[] {" + str5 + "});");
                    }
                    else
                    {
                        string str6 = this.WriteAssemblyInfo(type);
                        this.writer.Write("static " + typeof(Type).FullName + " " + str2 + " = " + str6 + ".GetType(");
                        this.WriteQuotedCSharpString(type.FullName);
                        this.writer.WriteLine(");");
                    }
                }
                this.reflectionVariables.Add(cSharpName, str2);
                TypeMapping typeMappingFromTypeDesc = scope.GetTypeMappingFromTypeDesc(typeDesc);
                if (typeMappingFromTypeDesc != null)
                {
                    this.WriteMappingInfo(typeMappingFromTypeDesc, str2, type);
                }
                if (typeDesc.IsCollection || typeDesc.IsEnumerable)
                {
                    TypeDesc desc2 = typeDesc.ArrayElementTypeDesc;
                    if (desc2.UseReflection)
                    {
                        this.WriteTypeInfo(scope, desc2, scope.GetTypeFromTypeDesc(desc2));
                    }
                    this.WriteCollectionInfo(str2, typeDesc, type);
                }
            }
            return str2;
        }
    }
}

