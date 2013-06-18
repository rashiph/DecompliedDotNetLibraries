namespace System.Windows.Forms.Design
{
    using Microsoft.CSharp;
    using Microsoft.Win32;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    public class AxWrapperGen
    {
        private string aboutBoxMethod;
        internal static BooleanSwitch AxCodeGen = new BooleanSwitch("AxCodeGen", "ActiveX WFW property generation.");
        private string axctl;
        private string axctlEvents;
        private System.Type axctlEventsType;
        private string axctlIface;
        private static string axctlNS;
        private System.Type axctlType;
        private Hashtable axctlTypeMembers;
        private Hashtable axHostMembers;
        private static Hashtable axHostPropDescs;
        internal static BooleanSwitch AxWrapper = new BooleanSwitch("AxWrapper", "ActiveX WFW wrapper generation.");
        private static CodeAttributeDeclaration bindable = null;
        private static CodeAttributeDeclaration browse = null;
        private static Hashtable classesInNamespace;
        private Guid clsidAx;
        private Hashtable conflictableThings;
        private string cookie;
        private CodeFieldReferenceExpression cookieRef;
        private ArrayList dataSourceProps = new ArrayList();
        private static CodeAttributeDeclaration defaultBind = null;
        private string defMember;
        private bool dispInterface;
        private bool enumerableInterface;
        private ArrayList events;
        public static ArrayList GeneratedSources = new ArrayList();
        private static Guid Guid_DataSource = new Guid("{7C0FFAB3-CD84-11D0-949A-00A0C91110ED}");
        private string memIface;
        private CodeFieldReferenceExpression memIfaceRef;
        private string multicaster;
        private CodeFieldReferenceExpression multicasterRef;
        private static CodeAttributeDeclaration nobrowse = null;
        private static CodeAttributeDeclaration nopersist = null;

        public AxWrapperGen(System.Type axType)
        {
            this.axctl = axType.Name;
            this.axctl = this.axctl.TrimStart(new char[] { '_', '1' });
            this.axctl = "Ax" + this.axctl;
            this.clsidAx = axType.GUID;
            CustomAttributeData[] attributeData = GetAttributeData(axType, typeof(ComSourceInterfacesAttribute));
            if ((attributeData.Length == 0) && axType.BaseType.GUID.Equals(axType.GUID))
            {
                attributeData = GetAttributeData(axType.BaseType, typeof(ComSourceInterfacesAttribute));
            }
            if (attributeData.Length > 0)
            {
                CustomAttributeData data = attributeData[0];
                CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
                string str = argument.Value.ToString();
                int length = str.IndexOfAny(new char[1]);
                string name = str.Substring(0, length);
                this.axctlEventsType = axType.Module.Assembly.GetType(name);
                if (this.axctlEventsType == null)
                {
                    this.axctlEventsType = System.Type.GetType(name, false);
                }
                if (this.axctlEventsType != null)
                {
                    this.axctlEvents = this.axctlEventsType.FullName;
                }
            }
            System.Type[] interfaces = axType.GetInterfaces();
            this.axctlType = interfaces[0];
            foreach (System.Type type in interfaces)
            {
                if (GetAttributeData(type, typeof(CoClassAttribute)).Length > 0)
                {
                    System.Type[] typeArray2 = type.GetInterfaces();
                    if ((typeArray2 != null) && (typeArray2.Length > 0))
                    {
                        this.axctl = "Ax" + type.Name;
                        this.axctlType = typeArray2[0];
                        break;
                    }
                }
            }
            this.axctlIface = this.axctlType.Name;
            foreach (System.Type type2 in interfaces)
            {
                if (type2 == typeof(IEnumerable))
                {
                    this.enumerableInterface = true;
                    break;
                }
            }
            try
            {
                attributeData = GetAttributeData(this.axctlType, typeof(InterfaceTypeAttribute));
                if (attributeData.Length > 0)
                {
                    CustomAttributeData data2 = attributeData[0];
                    CustomAttributeTypedArgument argument2 = data2.ConstructorArguments[0];
                    this.dispInterface = argument2.Value.Equals(ComInterfaceType.InterfaceIsIDispatch);
                }
            }
            catch (MissingMethodException)
            {
            }
        }

        private void AddClassToNamespace(CodeNamespace ns, CodeTypeDeclaration cls)
        {
            if (classesInNamespace == null)
            {
                classesInNamespace = new Hashtable();
            }
            try
            {
                ns.Types.Add(cls);
                classesInNamespace.Add(cls.Name, cls);
            }
            catch (Exception)
            {
            }
        }

        private EventEntry AddEvent(string name, string eventCls, string eventHandlerCls, System.Type retType, AxParameterData[] parameters)
        {
            if (this.events == null)
            {
                this.events = new ArrayList();
            }
            if (this.axctlTypeMembers == null)
            {
                this.axctlTypeMembers = new Hashtable();
                foreach (MemberInfo info in this.axctlType.GetMembers())
                {
                    string key = info.Name;
                    if (!this.axctlTypeMembers.Contains(key))
                    {
                        this.axctlTypeMembers.Add(key, info);
                    }
                }
            }
            bool conflict = (this.axctlTypeMembers.Contains(name) || this.AxHostMembers.Contains(name)) || this.ConflictableThings.Contains(name);
            EventEntry entry = new EventEntry(name, eventCls, eventHandlerCls, retType, parameters, conflict);
            this.events.Add(entry);
            return entry;
        }

        private bool ClassAlreadyExistsInNamespace(CodeNamespace ns, string clsName)
        {
            return classesInNamespace.Contains(clsName);
        }

        private static string Compile(AxImporter importer, CodeNamespace ns, string[] refAssemblies, DateTime tlbTimeStamp, Version version)
        {
            CompilerResults results;
            ICodeGenerator codegen = new CSharpCodeProvider().CreateGenerator();
            string outputName = importer.options.outputName;
            string path = Path.Combine(importer.options.outputDirectory, outputName);
            string fileName = Path.ChangeExtension(path, ".cs");
            CompilerParameters options = new CompilerParameters(refAssemblies, path) {
                IncludeDebugInformation = importer.options.genSources
            };
            CodeCompileUnit cu = new CodeCompileUnit();
            cu.Namespaces.Add(ns);
            CodeAttributeDeclarationCollection assemblyCustomAttributes = cu.AssemblyCustomAttributes;
            assemblyCustomAttributes.Add(new CodeAttributeDeclaration("System.Reflection.AssemblyVersion", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(version.ToString())) }));
            assemblyCustomAttributes.Add(new CodeAttributeDeclaration("System.Windows.Forms.AxHost.TypeLibraryTimeStamp", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(tlbTimeStamp.ToString())) }));
            if (importer.options.delaySign)
            {
                assemblyCustomAttributes.Add(new CodeAttributeDeclaration("System.Reflection.AssemblyDelaySign", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(true)) }));
            }
            if ((importer.options.keyFile != null) && (importer.options.keyFile.Length > 0))
            {
                assemblyCustomAttributes.Add(new CodeAttributeDeclaration("System.Reflection.AssemblyKeyFile", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(importer.options.keyFile)) }));
            }
            if ((importer.options.keyContainer != null) && (importer.options.keyContainer.Length > 0))
            {
                assemblyCustomAttributes.Add(new CodeAttributeDeclaration("System.Reflection.AssemblyKeyName", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(importer.options.keyContainer)) }));
            }
            if (importer.options.genSources)
            {
                SaveCompileUnit(codegen, cu, fileName);
                results = ((ICodeCompiler) codegen).CompileAssemblyFromFile(options, fileName);
            }
            else
            {
                results = ((ICodeCompiler) codegen).CompileAssemblyFromDom(options, cu);
            }
            if ((results.Errors != null) && (results.Errors.Count > 0))
            {
                string str4 = null;
                foreach (CompilerError error in results.Errors)
                {
                    if (!error.IsWarning)
                    {
                        str4 = str4 + error.ToString() + "\r\n";
                    }
                }
                if (str4 != null)
                {
                    SaveCompileUnit(codegen, cu, fileName);
                    throw new Exception(System.Design.SR.GetString("AXCompilerError", new object[] { ns.Name, fileName }) + "\r\n" + str4);
                }
            }
            return path;
        }

        private string CreateDataSourceFieldName(string propName)
        {
            return ("ax" + propName);
        }

        private CodeStatement CreateInvalidStateException(string name, string kind)
        {
            CodeBinaryOperatorExpression expression = new CodeBinaryOperatorExpression(this.memIfaceRef, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = expression
            };
            CodeExpression[] parameters = new CodeExpression[] { new CodePrimitiveExpression(name), new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(null, typeof(AxHost).FullName + ".ActiveXInvokeKind"), kind) };
            CodeObjectCreateExpression toThrow = new CodeObjectCreateExpression(typeof(AxHost.InvalidActiveXStateException).FullName, parameters);
            statement.TrueStatements.Add(new CodeThrowExceptionStatement(toThrow));
            return statement;
        }

        private CodeParameterDeclarationExpression CreateParamDecl(string type, string name, bool isOptional)
        {
            CodeParameterDeclarationExpression expression = new CodeParameterDeclarationExpression(type, name);
            if (isOptional)
            {
                CodeAttributeDeclarationCollection declarations = new CodeAttributeDeclarationCollection();
                declarations.Add(new CodeAttributeDeclaration("System.Runtime.InteropServices.Optional", new CodeAttributeArgument[0]));
                expression.CustomAttributes = declarations;
            }
            return expression;
        }

        private CodeConditionStatement CreateValidStateCheck()
        {
            CodeConditionStatement statement = new CodeConditionStatement();
            CodeBinaryOperatorExpression left = new CodeBinaryOperatorExpression(this.memIfaceRef, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
            CodeBinaryOperatorExpression right = new CodeBinaryOperatorExpression(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "PropsValid", new CodeExpression[0]), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(true));
            CodeBinaryOperatorExpression expression3 = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.BooleanAnd, right);
            return new CodeConditionStatement { Condition = expression3 };
        }

        private void FillAxHostMembers()
        {
            if (this.axHostMembers == null)
            {
                this.axHostMembers = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                System.Type type = typeof(AxHost);
                foreach (MemberInfo info in type.GetMembers())
                {
                    string name = info.Name;
                    if (!this.axHostMembers.Contains(name))
                    {
                        FieldInfo info2 = info as FieldInfo;
                        if ((info2 != null) && !info2.IsPrivate)
                        {
                            this.axHostMembers.Add(name, info);
                        }
                        else
                        {
                            PropertyInfo info3 = info as PropertyInfo;
                            if (info3 != null)
                            {
                                this.axHostMembers.Add(name, info);
                            }
                            else
                            {
                                MethodBase base2 = info as MethodBase;
                                if ((base2 != null) && !base2.IsPrivate)
                                {
                                    this.axHostMembers.Add(name, info);
                                }
                                else
                                {
                                    EventInfo info4 = info as EventInfo;
                                    if (info4 != null)
                                    {
                                        this.axHostMembers.Add(name, info);
                                    }
                                    else
                                    {
                                        System.Type type2 = info as System.Type;
                                        if ((type2 != null) && (type2.IsPublic || type2.IsNestedPublic))
                                        {
                                            this.axHostMembers.Add(name, info);
                                        }
                                        else
                                        {
                                            this.axHostMembers.Add(name, info);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FillConflicatableThings()
        {
            if (this.conflictableThings == null)
            {
                this.conflictableThings = new Hashtable();
                this.conflictableThings.Add("System", "System");
            }
        }

        private void GenerateAxHost(CodeNamespace ns, string[] refAssemblies)
        {
            CodeTypeDeclaration cls = new CodeTypeDeclaration {
                Name = this.axctl
            };
            cls.BaseTypes.Add(typeof(AxHost).FullName);
            if (this.enumerableInterface)
            {
                cls.BaseTypes.Add(typeof(IEnumerable));
            }
            CodeAttributeDeclarationCollection declarations = new CodeAttributeDeclarationCollection();
            CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(typeof(AxHost.ClsidAttribute).FullName, new CodeAttributeArgument[] { new CodeAttributeArgument(new CodeSnippetExpression("\"{" + this.clsidAx.ToString() + "}\"")) });
            declarations.Add(declaration2);
            CodeAttributeDeclaration declaration3 = new CodeAttributeDeclaration(typeof(DesignTimeVisibleAttribute).FullName, new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(true)) });
            declarations.Add(declaration3);
            cls.CustomAttributes = declarations;
            CustomAttributeData[] attributeData = GetAttributeData(this.axctlType, typeof(DefaultMemberAttribute));
            if ((attributeData != null) && (attributeData.Length > 0))
            {
                CustomAttributeTypedArgument argument = attributeData[0].ConstructorArguments[0];
                this.defMember = argument.Value.ToString();
            }
            this.AddClassToNamespace(ns, cls);
            this.WriteMembersDecl(cls);
            if (this.axctlEventsType != null)
            {
                this.WriteEventMembersDecl(ns, cls);
            }
            CodeConstructor constructor = this.WriteConstructor(cls);
            this.WriteProperties(cls);
            this.WriteMethods(cls);
            this.WriteHookupMethods(cls);
            if (this.aboutBoxMethod != null)
            {
                CodeObjectCreateExpression expression = new CodeObjectCreateExpression("AboutBoxDelegate", new CodeExpression[0]);
                expression.Parameters.Add(new CodeFieldReferenceExpression(null, this.aboutBoxMethod));
                CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SetAboutBoxDelegate", new CodeExpression[0]);
                expression2.Parameters.Add(expression);
                constructor.Statements.Add(new CodeExpressionStatement(expression2));
            }
            if (this.axctlEventsType != null)
            {
                this.WriteEvents(ns, cls);
            }
            if (this.dataSourceProps.Count > 0)
            {
                this.WriteOnInPlaceActive(cls);
            }
        }

        internal static string GenerateWrappers(AxImporter importer, Guid axClsid, Assembly rcwAssem, string[] refAssemblies, DateTime tlbTimeStamp, out string assem)
        {
            assem = null;
            bool flag = false;
            CodeNamespace ns = null;
            string axctl = null;
            try
            {
                System.Type[] types = rcwAssem.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    if (IsTypeActiveXControl(types[i]))
                    {
                        flag = true;
                        if (ns == null)
                        {
                            axctlNS = "Ax" + types[i].Namespace;
                            ns = new CodeNamespace(axctlNS);
                        }
                        AxWrapperGen gen = new AxWrapperGen(types[i]);
                        gen.GenerateAxHost(ns, refAssemblies);
                        if (!axClsid.Equals(Guid.Empty) && axClsid.Equals(types[i].GUID))
                        {
                            axctl = gen.axctl;
                        }
                        else if (axClsid.Equals(Guid.Empty) && (axctl == null))
                        {
                            axctl = gen.axctl;
                        }
                    }
                }
            }
            finally
            {
                if (classesInNamespace != null)
                {
                    classesInNamespace.Clear();
                    classesInNamespace = null;
                }
            }
            AssemblyName name = rcwAssem.GetName();
            if (flag)
            {
                Version version = name.Version;
                assem = Compile(importer, ns, refAssemblies, tlbTimeStamp, version);
                if (assem != null)
                {
                    if (axctl == null)
                    {
                        throw new Exception(System.Design.SR.GetString("AXNotValidControl", new object[] { "{" + axClsid + "}" }));
                    }
                    return (axctlNS + "." + axctl + "," + axctlNS);
                }
            }
            return null;
        }

        internal static CustomAttributeData[] GetAttributeData(ICustomAttributeProvider attributeProvider, System.Type attributeType)
        {
            List<CustomAttributeData> list = new List<CustomAttributeData>();
            IList<CustomAttributeData> customAttributes = null;
            if (attributeProvider is Assembly)
            {
                customAttributes = CustomAttributeData.GetCustomAttributes(attributeProvider as Assembly);
            }
            else if (attributeProvider is MemberInfo)
            {
                customAttributes = CustomAttributeData.GetCustomAttributes(attributeProvider as MemberInfo);
            }
            else if (attributeProvider is Module)
            {
                customAttributes = CustomAttributeData.GetCustomAttributes(attributeProvider as Module);
            }
            else if (attributeProvider is ParameterInfo)
            {
                customAttributes = CustomAttributeData.GetCustomAttributes(attributeProvider as ParameterInfo);
            }
            if (customAttributes != null)
            {
                foreach (CustomAttributeData data in customAttributes)
                {
                    if (data.ToString().Contains(attributeType.ToString()))
                    {
                        list.Add(data);
                    }
                }
            }
            return list.ToArray();
        }

        private CodeExpression GetInitializer(System.Type type)
        {
            if (type != null)
            {
                if ((((type == typeof(int)) || (type == typeof(short))) || ((type == typeof(long)) || (type == typeof(float)))) || ((type == typeof(double)) || typeof(Enum).IsAssignableFrom(type)))
                {
                    return new CodePrimitiveExpression(0);
                }
                if (type == typeof(char))
                {
                    return new CodeCastExpression("System.Character", new CodePrimitiveExpression(0));
                }
                if (type == typeof(bool))
                {
                    return new CodePrimitiveExpression(false);
                }
            }
            return new CodePrimitiveExpression(null);
        }

        private CodeMethodReturnStatement GetPropertyGetRValue(PropertyInfo pinfo, CodeExpression reference, ComAliasEnum alias, AxParameterData[] parameters, bool fMethodSyntax)
        {
            CodeExpression expression = null;
            if (fMethodSyntax)
            {
                expression = new CodeMethodInvokeExpression(reference, pinfo.GetGetMethod().Name, new CodeExpression[0]);
                for (int i = 0; i < parameters.Length; i++)
                {
                    ((CodeMethodInvokeExpression) expression).Parameters.Add(new CodeFieldReferenceExpression(null, parameters[i].Name));
                }
            }
            else if (parameters.Length > 0)
            {
                expression = new CodeIndexerExpression(reference, new CodeExpression[0]);
                for (int j = 0; j < parameters.Length; j++)
                {
                    ((CodeIndexerExpression) expression).Indices.Add(new CodeFieldReferenceExpression(null, parameters[j].Name));
                }
            }
            else
            {
                expression = new CodePropertyReferenceExpression(reference, (parameters.Length == 0) ? pinfo.Name : "");
            }
            if (alias == ComAliasEnum.None)
            {
                return new CodeMethodReturnStatement(expression);
            }
            string comToManagedConverter = ComAliasConverter.GetComToManagedConverter(alias);
            string comToWFParamConverter = ComAliasConverter.GetComToWFParamConverter(alias);
            CodeExpression[] expressionArray = null;
            if (comToWFParamConverter.Length == 0)
            {
                expressionArray = new CodeExpression[] { expression };
            }
            else
            {
                CodeCastExpression expression2 = new CodeCastExpression(comToWFParamConverter, expression);
                expressionArray = new CodeExpression[] { expression2 };
            }
            return new CodeMethodReturnStatement(new CodeMethodInvokeExpression(null, comToManagedConverter, expressionArray));
        }

        private CodeExpression GetPropertySetRValue(ComAliasEnum alias, System.Type propertyType)
        {
            CodeExpression expression = new CodePropertySetValueReferenceExpression();
            if (alias == ComAliasEnum.None)
            {
                return expression;
            }
            string wFToComConverter = ComAliasConverter.GetWFToComConverter(alias);
            string wFToComParamConverter = ComAliasConverter.GetWFToComParamConverter(alias, propertyType);
            CodeExpression[] parameters = new CodeExpression[] { expression };
            CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression(null, wFToComConverter, parameters);
            if (wFToComParamConverter.Length == 0)
            {
                return expression2;
            }
            return new CodeCastExpression(wFToComParamConverter, expression2);
        }

        private bool IsDispidKnown(int dp, string propName)
        {
            return (((((dp == -513) || (dp == -501)) || ((dp == -512) || (dp == -514))) || (((dp == -516) || (dp == -611)) || ((dp == -517) || (dp == -515)))) || ((dp == 0) && propName.Equals(this.defMember)));
        }

        private bool IsEventPresent(MethodInfo mievent)
        {
            return false;
        }

        private bool IsPropertyBindable(PropertyInfo pinfo, out bool isDefaultBind)
        {
            isDefaultBind = false;
            MethodInfo getMethod = pinfo.GetGetMethod();
            if (getMethod != null)
            {
                CustomAttributeData[] attributeData = GetAttributeData(getMethod, typeof(TypeLibFuncAttribute));
                if ((attributeData != null) && (attributeData.Length > 0))
                {
                    CustomAttributeTypedArgument argument = attributeData[0].ConstructorArguments[0];
                    int num = int.Parse(argument.Value.ToString());
                    isDefaultBind = (num & 0x20) != 0;
                    if (isDefaultBind || ((num & 4) != 0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsPropertyBrowsable(PropertyInfo pinfo, ComAliasEnum alias)
        {
            MethodInfo getMethod = pinfo.GetGetMethod();
            if (getMethod == null)
            {
                return false;
            }
            CustomAttributeData[] attributeData = GetAttributeData(getMethod, typeof(TypeLibFuncAttribute));
            if ((attributeData != null) && (attributeData.Length > 0))
            {
                CustomAttributeTypedArgument argument = attributeData[0].ConstructorArguments[0];
                int num = int.Parse(argument.Value.ToString());
                if (((num & 0x400) != 0) || ((num & 0x40) != 0))
                {
                    return false;
                }
            }
            System.Type propertyType = pinfo.PropertyType;
            if (((alias == ComAliasEnum.None) && propertyType.IsInterface) && !propertyType.GUID.Equals(Guid_DataSource))
            {
                return false;
            }
            return true;
        }

        private bool IsPropertySignature(PropertyInfo pinfo, out bool useLet)
        {
            int nParams = 0;
            bool flag = true;
            useLet = false;
            string str = (this.defMember == null) ? "Item" : this.defMember;
            if (pinfo.Name.Equals(str))
            {
                nParams = pinfo.GetIndexParameters().Length;
            }
            if (pinfo.GetGetMethod() != null)
            {
                flag = this.IsPropertySignature(pinfo.GetGetMethod(), pinfo.PropertyType, true, nParams);
            }
            if (pinfo.GetSetMethod() != null)
            {
                flag = flag && this.IsPropertySignature(pinfo.GetSetMethod(), pinfo.PropertyType, false, nParams + 1);
                if (!flag)
                {
                    MethodInfo method = pinfo.DeclaringType.GetMethod("let_" + pinfo.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    if (method != null)
                    {
                        flag = this.IsPropertySignature(method, pinfo.PropertyType, false, nParams + 1);
                        useLet = true;
                    }
                }
            }
            return flag;
        }

        private bool IsPropertySignature(MethodInfo method, out bool hasPropInfo, out bool useLet)
        {
            useLet = false;
            hasPropInfo = false;
            if ((!method.Name.StartsWith("get_") && !method.Name.StartsWith("set_")) && !method.Name.StartsWith("let_"))
            {
                return false;
            }
            string name = method.Name.Substring(4, method.Name.Length - 4);
            PropertyInfo property = this.axctlType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (property == null)
            {
                return false;
            }
            return this.IsPropertySignature(property, out useLet);
        }

        private bool IsPropertySignature(MethodInfo method, System.Type returnType, bool getter, int nParams)
        {
            if (!method.IsConstructor)
            {
                if (getter)
                {
                    string name = method.Name.Substring(4);
                    if ((this.axctlType.GetProperty(name) != null) && (method.GetParameters().Length == nParams))
                    {
                        return (method.ReturnType == returnType);
                    }
                }
                else
                {
                    string str2 = method.Name.Substring(4);
                    ParameterInfo[] parameters = method.GetParameters();
                    if ((this.axctlType.GetProperty(str2) != null) && (parameters.Length == nParams))
                    {
                        return ((parameters.Length <= 0) || ((parameters[parameters.Length - 1].ParameterType == returnType) || (method.Name.StartsWith("let_") && (parameters[parameters.Length - 1].ParameterType == typeof(object)))));
                    }
                }
            }
            return false;
        }

        private static bool IsTypeActiveXControl(System.Type type)
        {
            if ((type.IsClass && type.IsCOMObject) && (type.IsPublic && !type.GUID.Equals(Guid.Empty)))
            {
                try
                {
                    CustomAttributeData[] attributeData = GetAttributeData(type, typeof(ComVisibleAttribute));
                    if (attributeData.Length != 0)
                    {
                        CustomAttributeTypedArgument argument = attributeData[0].ConstructorArguments[0];
                        if (argument.Value.Equals(false))
                        {
                            return false;
                        }
                    }
                }
                catch
                {
                    return false;
                }
                string name = @"CLSID\{" + type.GUID.ToString() + @"}\Control";
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(name);
                if (key == null)
                {
                    return false;
                }
                key.Close();
                System.Type[] interfaces = type.GetInterfaces();
                if ((interfaces != null) && (interfaces.Length >= 1))
                {
                    return true;
                }
            }
            return false;
        }

        internal static string MapTypeName(System.Type type)
        {
            bool isArray = type.IsArray;
            System.Type elementType = type.GetElementType();
            if (elementType != null)
            {
                type = elementType;
            }
            string fullName = type.FullName;
            if (!isArray)
            {
                return fullName;
            }
            return (fullName + "[]");
        }

        private bool OptionalsPresent(MethodInfo method)
        {
            AxParameterData[] dataArray = AxParameterData.Convert(method.GetParameters());
            if ((dataArray != null) && (dataArray.Length > 0))
            {
                for (int i = 0; i < dataArray.Length; i++)
                {
                    if (dataArray[i].IsOptional)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string ResolveConflict(string name, System.Type returnType, out bool fOverride, out bool fUseNew)
        {
            fOverride = false;
            fUseNew = false;
            string str = "";
            try
            {
                if (axHostPropDescs == null)
                {
                    axHostPropDescs = new Hashtable();
                    foreach (PropertyInfo info in typeof(AxHost).GetProperties())
                    {
                        string key = info.Name + info.PropertyType.GetHashCode();
                        if (!axHostPropDescs.Contains(key))
                        {
                            axHostPropDescs.Add(key, info);
                        }
                    }
                }
                PropertyInfo info2 = (PropertyInfo) axHostPropDescs[name + returnType.GetHashCode()];
                if (info2 != null)
                {
                    if (returnType.Equals(info2.PropertyType))
                    {
                        if (info2.CanRead ? info2.GetGetMethod().IsVirtual : false)
                        {
                            fOverride = true;
                            return str;
                        }
                        fUseNew = true;
                        return str;
                    }
                    return "Ctl";
                }
                if (this.AxHostMembers.Contains(name) || this.ConflictableThings.Contains(name))
                {
                    return "Ctl";
                }
                if (!name.StartsWith("get_") && !name.StartsWith("set_"))
                {
                    return str;
                }
                if (TypeDescriptor.GetProperties(typeof(AxHost))[name.Substring(4)] != null)
                {
                    str = "Ctl";
                }
            }
            catch (AmbiguousMatchException)
            {
                str = "Ctl";
            }
            return str;
        }

        private static void SaveCompileUnit(ICodeGenerator codegen, CodeCompileUnit cu, string fileName)
        {
            try
            {
                try
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
                catch
                {
                }
                FileStream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter w = new StreamWriter(stream, new UTF8Encoding(false));
                codegen.GenerateCodeFromCompileUnit(cu, w, null);
                w.Flush();
                w.Close();
                stream.Close();
                GeneratedSources.Add(fileName);
            }
            catch (Exception)
            {
            }
        }

        private CodeConstructor WriteConstructor(CodeTypeDeclaration cls)
        {
            CodeConstructor constructor = new CodeConstructor {
                Attributes = MemberAttributes.Public
            };
            constructor.BaseConstructorArgs.Add(new CodeSnippetExpression("\"" + this.clsidAx.ToString() + "\""));
            cls.Members.Add(constructor);
            return constructor;
        }

        private void WriteDataSourcePropertySetter(CodeMemberProperty prop, PropertyInfo pinfo, string dataSourceName)
        {
            CodeExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), dataSourceName);
            CodeExpression right = new CodeFieldReferenceExpression(null, "value");
            CodeAssignStatement statement = new CodeAssignStatement(left, right);
            prop.SetStatements.Add(statement);
            CodeConditionStatement statement2 = this.CreateValidStateCheck();
            left = new CodeFieldReferenceExpression(this.memIfaceRef, pinfo.Name);
            statement2.TrueStatements.Add(new CodeAssignStatement(left, right));
            prop.SetStatements.Add(statement2);
        }

        private string WriteEventClass(CodeNamespace ns, MethodInfo mi, ParameterInfo[] pinfos)
        {
            string clsName = this.axctlEventsType.Name + "_" + mi.Name + "Event";
            if (!this.ClassAlreadyExistsInNamespace(ns, clsName))
            {
                CodeTypeDeclaration cls = new CodeTypeDeclaration {
                    Name = clsName
                };
                AxParameterData[] dataArray = AxParameterData.Convert(pinfos);
                for (int i = 0; i < dataArray.Length; i++)
                {
                    CodeMemberField field = new CodeMemberField(dataArray[i].TypeName, dataArray[i].Name) {
                        Attributes = MemberAttributes.Public | MemberAttributes.Final
                    };
                    cls.Members.Add(field);
                }
                CodeConstructor constructor = new CodeConstructor {
                    Attributes = MemberAttributes.Public
                };
                for (int j = 0; j < dataArray.Length; j++)
                {
                    if (dataArray[j].Direction != FieldDirection.Out)
                    {
                        constructor.Parameters.Add(this.CreateParamDecl(dataArray[j].TypeName, dataArray[j].Name, false));
                        CodeFieldReferenceExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), dataArray[j].Name);
                        CodeFieldReferenceExpression right = new CodeFieldReferenceExpression(null, dataArray[j].Name);
                        CodeAssignStatement statement = new CodeAssignStatement(left, right);
                        constructor.Statements.Add(statement);
                    }
                }
                cls.Members.Add(constructor);
                this.AddClassToNamespace(ns, cls);
            }
            return clsName;
        }

        private string WriteEventHandlerClass(CodeNamespace ns, MethodInfo mi)
        {
            string clsName = this.axctlEventsType.Name + "_" + mi.Name + "EventHandler";
            if (!this.ClassAlreadyExistsInNamespace(ns, clsName))
            {
                CodeTypeDelegate cls = new CodeTypeDelegate {
                    Name = clsName
                };
                cls.Parameters.Add(this.CreateParamDecl(typeof(object).FullName, "sender", false));
                cls.Parameters.Add(this.CreateParamDecl(this.axctlEventsType.Name + "_" + mi.Name + "Event", "e", false));
                cls.ReturnType = new CodeTypeReference(mi.ReturnType);
                this.AddClassToNamespace(ns, cls);
            }
            return clsName;
        }

        private void WriteEventMembersDecl(CodeNamespace ns, CodeTypeDeclaration cls)
        {
            bool flag = false;
            MethodInfo[] methods = this.axctlEventsType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int i = 0; i < methods.Length; i++)
            {
                EventEntry entry = null;
                if (!this.IsEventPresent(methods[i]))
                {
                    ParameterInfo[] parameters = methods[i].GetParameters();
                    if ((parameters.Length > 0) || (methods[i].ReturnType != typeof(void)))
                    {
                        string eventHandlerCls = this.WriteEventHandlerClass(ns, methods[i]);
                        string eventCls = this.WriteEventClass(ns, methods[i], parameters);
                        entry = this.AddEvent(methods[i].Name, eventCls, eventHandlerCls, methods[i].ReturnType, AxParameterData.Convert(parameters));
                    }
                    else
                    {
                        entry = this.AddEvent(methods[i].Name, "System.EventArgs", "System.EventHandler", typeof(void), new AxParameterData[0]);
                    }
                }
                if (!flag)
                {
                    CustomAttributeData[] attributeData = GetAttributeData(methods[i], typeof(DispIdAttribute));
                    if ((attributeData != null) && (attributeData.Length != 0))
                    {
                        CustomAttributeData data = attributeData[0];
                        CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
                        if (int.Parse(argument.Value.ToString()) == 1)
                        {
                            string str3 = (entry != null) ? entry.resovledEventName : methods[i].Name;
                            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration("System.ComponentModel.DefaultEvent", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(str3)) });
                            cls.CustomAttributes.Add(declaration);
                            flag = true;
                        }
                    }
                }
            }
        }

        private string WriteEventMulticaster(CodeNamespace ns)
        {
            string clsName = this.axctl + "EventMulticaster";
            if (!this.ClassAlreadyExistsInNamespace(ns, clsName))
            {
                CodeTypeDeclaration cls = new CodeTypeDeclaration {
                    Name = clsName
                };
                cls.BaseTypes.Add(this.axctlEvents);
                CodeAttributeDeclarationCollection declarations = new CodeAttributeDeclarationCollection();
                CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration("System.Runtime.InteropServices.ClassInterface", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(null, "System.Runtime.InteropServices.ClassInterfaceType"), "None")) });
                declarations.Add(declaration2);
                cls.CustomAttributes = declarations;
                CodeMemberField field = new CodeMemberField(this.axctl, "parent") {
                    Attributes = MemberAttributes.Private | MemberAttributes.Final
                };
                cls.Members.Add(field);
                CodeConstructor constructor = new CodeConstructor {
                    Attributes = MemberAttributes.Public
                };
                constructor.Parameters.Add(this.CreateParamDecl(this.axctl, "parent", false));
                CodeFieldReferenceExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "parent");
                CodeFieldReferenceExpression right = new CodeFieldReferenceExpression(null, "parent");
                constructor.Statements.Add(new CodeAssignStatement(left, right));
                cls.Members.Add(constructor);
                MethodInfo[] methods = this.axctlEventsType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                int num = 0;
                for (int i = 0; i < methods.Length; i++)
                {
                    AxParameterData[] dataArray = AxParameterData.Convert(methods[i].GetParameters());
                    CodeMemberMethod method = new CodeMemberMethod {
                        Name = methods[i].Name,
                        Attributes = MemberAttributes.Public,
                        ReturnType = new CodeTypeReference(MapTypeName(methods[i].ReturnType))
                    };
                    for (int j = 0; j < dataArray.Length; j++)
                    {
                        CodeParameterDeclarationExpression expression3 = this.CreateParamDecl(MapTypeName(dataArray[j].ParameterType), dataArray[j].Name, dataArray[j].IsOptional);
                        expression3.Direction = dataArray[j].Direction;
                        method.Parameters.Add(expression3);
                    }
                    CodeFieldReferenceExpression expression4 = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "parent");
                    if (!this.IsEventPresent(methods[i]))
                    {
                        EventEntry entry = (EventEntry) this.events[num++];
                        CodeExpressionCollection expressions = new CodeExpressionCollection();
                        expressions.Add(expression4);
                        if (entry.eventCls.Equals("EventArgs"))
                        {
                            expressions.Add(new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(null, "EventArgs"), "Empty"));
                            CodeExpression[] array = new CodeExpression[expressions.Count];
                            ((ICollection) expressions).CopyTo(array, 0);
                            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(expression4, entry.invokeMethodName, array);
                            if (methods[i].ReturnType == typeof(void))
                            {
                                method.Statements.Add(new CodeExpressionStatement(expression));
                            }
                            else
                            {
                                method.Statements.Add(new CodeMethodReturnStatement(expression));
                            }
                        }
                        else
                        {
                            CodeObjectCreateExpression expression6 = new CodeObjectCreateExpression(entry.eventCls, new CodeExpression[0]);
                            for (int k = 0; k < entry.parameters.Length; k++)
                            {
                                if (!entry.parameters[k].IsOut)
                                {
                                    expression6.Parameters.Add(new CodeFieldReferenceExpression(null, entry.parameters[k].Name));
                                }
                            }
                            CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(entry.eventCls, entry.eventParam) {
                                InitExpression = expression6
                            };
                            method.Statements.Add(statement);
                            expressions.Add(new CodeFieldReferenceExpression(null, entry.eventParam));
                            CodeExpression[] expressionArray2 = new CodeExpression[expressions.Count];
                            ((ICollection) expressions).CopyTo(expressionArray2, 0);
                            CodeMethodInvokeExpression expression7 = new CodeMethodInvokeExpression(expression4, entry.invokeMethodName, expressionArray2);
                            if (methods[i].ReturnType == typeof(void))
                            {
                                method.Statements.Add(new CodeExpressionStatement(expression7));
                            }
                            else
                            {
                                CodeVariableDeclarationStatement statement2 = new CodeVariableDeclarationStatement(entry.retType, entry.invokeMethodName);
                                method.Statements.Add(statement2);
                                method.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, statement2.Name), expression7));
                            }
                            for (int m = 0; m < dataArray.Length; m++)
                            {
                                if (dataArray[m].IsByRef)
                                {
                                    method.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, dataArray[m].Name), new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(null, statement.Name), dataArray[m].Name)));
                                }
                            }
                            if (methods[i].ReturnType != typeof(void))
                            {
                                method.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, entry.invokeMethodName)));
                            }
                        }
                    }
                    else
                    {
                        CodeExpressionCollection expressions2 = new CodeExpressionCollection();
                        for (int n = 0; n < dataArray.Length; n++)
                        {
                            expressions2.Add(new CodeFieldReferenceExpression(null, dataArray[n].Name));
                        }
                        CodeExpression[] expressionArray3 = new CodeExpression[expressions2.Count];
                        ((ICollection) expressions2).CopyTo(expressionArray3, 0);
                        CodeMethodInvokeExpression expression8 = new CodeMethodInvokeExpression(expression4, "RaiseOn" + methods[i].Name, expressionArray3);
                        if (methods[i].ReturnType == typeof(void))
                        {
                            method.Statements.Add(new CodeExpressionStatement(expression8));
                        }
                        else
                        {
                            method.Statements.Add(new CodeMethodReturnStatement(expression8));
                        }
                    }
                    cls.Members.Add(method);
                }
                this.AddClassToNamespace(ns, cls);
            }
            return clsName;
        }

        private void WriteEvents(CodeNamespace ns, CodeTypeDeclaration cls)
        {
            for (int i = 0; (this.events != null) && (i < this.events.Count); i++)
            {
                EventEntry entry = (EventEntry) this.events[i];
                CodeMemberEvent event2 = new CodeMemberEvent {
                    Name = entry.resovledEventName,
                    Attributes = entry.eventFlags,
                    Type = new CodeTypeReference(entry.eventHandlerCls)
                };
                cls.Members.Add(event2);
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = entry.invokeMethodName,
                    ReturnType = new CodeTypeReference(entry.retType),
                    Attributes = MemberAttributes.Assembly | MemberAttributes.Final
                };
                method.Parameters.Add(this.CreateParamDecl(MapTypeName(typeof(object)), "sender", false));
                method.Parameters.Add(this.CreateParamDecl(entry.eventCls, "e", false));
                CodeFieldReferenceExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), entry.resovledEventName);
                CodeBinaryOperatorExpression expression2 = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
                CodeConditionStatement statement = new CodeConditionStatement {
                    Condition = expression2
                };
                CodeExpressionCollection expressions = new CodeExpressionCollection();
                expressions.Add(new CodeFieldReferenceExpression(null, "sender"));
                expressions.Add(new CodeFieldReferenceExpression(null, "e"));
                CodeExpression[] array = new CodeExpression[expressions.Count];
                ((ICollection) expressions).CopyTo(array, 0);
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), entry.resovledEventName, array);
                if (entry.retType == typeof(void))
                {
                    statement.TrueStatements.Add(new CodeExpressionStatement(expression));
                }
                else
                {
                    statement.TrueStatements.Add(new CodeMethodReturnStatement(expression));
                    statement.FalseStatements.Add(new CodeMethodReturnStatement(this.GetInitializer(entry.retType)));
                }
                method.Statements.Add(statement);
                cls.Members.Add(method);
            }
            this.WriteEventMulticaster(ns);
        }

        private void WriteHookupMethods(CodeTypeDeclaration cls)
        {
            if (this.axctlEventsType != null)
            {
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "CreateSink",
                    Attributes = MemberAttributes.Family | MemberAttributes.Override
                };
                CodeObjectCreateExpression expression = new CodeObjectCreateExpression(this.axctl + "EventMulticaster", new CodeExpression[0]);
                expression.Parameters.Add(new CodeThisReferenceExpression());
                CodeAssignStatement statement = new CodeAssignStatement(this.multicasterRef, expression);
                CodeObjectCreateExpression expression2 = new CodeObjectCreateExpression(typeof(AxHost.ConnectionPointCookie).FullName, new CodeExpression[0]);
                expression2.Parameters.Add(this.memIfaceRef);
                expression2.Parameters.Add(this.multicasterRef);
                expression2.Parameters.Add(new CodeTypeOfExpression(this.axctlEvents));
                CodeAssignStatement statement2 = new CodeAssignStatement(this.cookieRef, expression2);
                CodeTryCatchFinallyStatement statement3 = new CodeTryCatchFinallyStatement();
                statement3.TryStatements.Add(statement);
                statement3.TryStatements.Add(statement2);
                statement3.CatchClauses.Add(new CodeCatchClause("", new CodeTypeReference(typeof(Exception))));
                method.Statements.Add(statement3);
                cls.Members.Add(method);
                CodeMemberMethod method2 = new CodeMemberMethod {
                    Name = "DetachSink",
                    Attributes = MemberAttributes.Family | MemberAttributes.Override
                };
                CodeFieldReferenceExpression targetObject = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.cookie);
                CodeMethodInvokeExpression expression4 = new CodeMethodInvokeExpression(targetObject, "Disconnect", new CodeExpression[0]);
                statement3 = new CodeTryCatchFinallyStatement();
                statement3.TryStatements.Add(expression4);
                statement3.CatchClauses.Add(new CodeCatchClause("", new CodeTypeReference(typeof(Exception))));
                method2.Statements.Add(statement3);
                cls.Members.Add(method2);
            }
            CodeMemberMethod method3 = new CodeMemberMethod {
                Name = "AttachInterfaces",
                Attributes = MemberAttributes.Family | MemberAttributes.Override
            };
            CodeCastExpression right = new CodeCastExpression(this.axctlType.FullName, new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "GetOcx", new CodeExpression[0]));
            CodeAssignStatement statement4 = new CodeAssignStatement(this.memIfaceRef, right);
            CodeTryCatchFinallyStatement statement5 = new CodeTryCatchFinallyStatement();
            statement5.TryStatements.Add(statement4);
            statement5.CatchClauses.Add(new CodeCatchClause("", new CodeTypeReference(typeof(Exception))));
            method3.Statements.Add(statement5);
            cls.Members.Add(method3);
        }

        private void WriteMembersDecl(CodeTypeDeclaration cls)
        {
            this.memIface = "ocx";
            this.memIfaceRef = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.memIface);
            cls.Members.Add(new CodeMemberField(MapTypeName(this.axctlType), this.memIface));
            if (this.axctlEventsType != null)
            {
                this.multicaster = "eventMulticaster";
                this.multicasterRef = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.multicaster);
                cls.Members.Add(new CodeMemberField(this.axctl + "EventMulticaster", this.multicaster));
                this.cookie = "cookie";
                this.cookieRef = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.cookie);
                cls.Members.Add(new CodeMemberField(typeof(AxHost.ConnectionPointCookie).FullName, this.cookie));
            }
        }

        private void WriteMethod(CodeTypeDeclaration cls, MethodInfo method, bool hasPropInfo, bool removeOptionals)
        {
            AxMethodGenerator generator = AxMethodGenerator.Create(method, removeOptionals);
            generator.ControlType = this.axctlType;
            string name = method.Name;
            bool fOverride = false;
            bool fUseNew = false;
            this.ResolveConflict(method.Name, method.ReturnType, out fOverride, out fUseNew);
            if (fOverride)
            {
                name = "Ctl" + name;
            }
            CodeMemberMethod method2 = generator.CreateMethod(name);
            method2.Statements.Add(this.CreateInvalidStateException(method2.Name, "MethodInvoke"));
            List<CodeExpression> parameters = generator.GenerateAndMarshalParameters(method2);
            CodeExpression returnExpression = generator.DoMethodInvoke(method2, method.Name, this.memIfaceRef, parameters);
            generator.UnmarshalParameters(method2, parameters);
            generator.GenerateReturn(method2, returnExpression);
            cls.Members.Add(method2);
            CustomAttributeData[] attributeData = GetAttributeData(method, typeof(DispIdAttribute));
            if ((attributeData != null) && (attributeData.Length > 0))
            {
                CustomAttributeTypedArgument argument = attributeData[0].ConstructorArguments[0];
                if ((int.Parse(argument.Value.ToString()) == -552) && (method.GetParameters().Length == 0))
                {
                    this.aboutBoxMethod = method2.Name;
                }
            }
        }

        private void WriteMethods(CodeTypeDeclaration cls)
        {
            MethodInfo[] methods = this.axctlType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int i = 0; i < methods.Length; i++)
            {
                bool flag;
                bool flag2;
                if (!this.IsPropertySignature(methods[i], out flag, out flag2))
                {
                    if (this.OptionalsPresent(methods[i]))
                    {
                        this.WriteMethod(cls, methods[i], flag, true);
                    }
                    this.WriteMethod(cls, methods[i], flag, false);
                }
            }
        }

        private void WriteOnInPlaceActive(CodeTypeDeclaration cls)
        {
            CodeMemberMethod method = new CodeMemberMethod {
                Name = "OnInPlaceActive",
                Attributes = MemberAttributes.Family | MemberAttributes.Override
            };
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "OnInPlaceActive", new CodeExpression[0]);
            method.Statements.Add(new CodeExpressionStatement(expression));
            foreach (PropertyInfo info in this.dataSourceProps)
            {
                string fieldName = this.CreateDataSourceFieldName(info.Name);
                CodeBinaryOperatorExpression expression2 = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
                CodeConditionStatement statement = new CodeConditionStatement {
                    Condition = expression2
                };
                CodeExpression left = new CodeFieldReferenceExpression(this.memIfaceRef, info.Name);
                CodeExpression right = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);
                statement.TrueStatements.Add(new CodeAssignStatement(left, right));
                method.Statements.Add(statement);
            }
            cls.Members.Add(method);
        }

        private void WriteProperties(CodeTypeDeclaration cls)
        {
            PropertyInfo[] properties = this.axctlType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int i = 0; i < properties.Length; i++)
            {
                bool flag;
                if (this.IsPropertySignature(properties[i], out flag))
                {
                    this.WriteProperty(cls, properties[i], flag);
                }
            }
        }

        private void WriteProperty(CodeTypeDeclaration cls, PropertyInfo pinfo, bool useLet)
        {
            CodeAttributeDeclarationCollection declarations;
            CodeAttributeDeclaration declaration = null;
            CustomAttributeData data = null;
            if (nopersist == null)
            {
                nopersist = new CodeAttributeDeclaration("System.ComponentModel.DesignerSerializationVisibility", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(null, "System.ComponentModel.DesignerSerializationVisibility"), "Hidden")) });
                nobrowse = new CodeAttributeDeclaration("System.ComponentModel.Browsable", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(false)) });
                browse = new CodeAttributeDeclaration("System.ComponentModel.Browsable", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(true)) });
                bindable = new CodeAttributeDeclaration("System.ComponentModel.Bindable", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(null, "System.ComponentModel.BindableSupport"), "Yes")) });
                defaultBind = new CodeAttributeDeclaration("System.ComponentModel.Bindable", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(null, "System.ComponentModel.BindableSupport"), "Default")) });
            }
            ComAliasEnum alias = ComAliasConverter.GetComAliasEnum(pinfo, pinfo.PropertyType, pinfo);
            System.Type propertyType = pinfo.PropertyType;
            if (alias != ComAliasEnum.None)
            {
                propertyType = ComAliasConverter.GetWFTypeFromComType(propertyType, alias);
            }
            bool dataSourceProp = propertyType.GUID.Equals(Guid_DataSource);
            if (dataSourceProp)
            {
                CodeMemberField field = new CodeMemberField(propertyType.FullName, this.CreateDataSourceFieldName(pinfo.Name)) {
                    Attributes = MemberAttributes.Private | MemberAttributes.Final
                };
                cls.Members.Add(field);
                this.dataSourceProps.Add(pinfo);
            }
            CustomAttributeData[] attributeData = GetAttributeData(pinfo, typeof(DispIdAttribute));
            if ((attributeData != null) && (attributeData.Length > 0))
            {
                data = attributeData[0];
                CodeAttributeArgument[] arguments = new CodeAttributeArgument[1];
                CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
                arguments[0] = new CodeAttributeArgument(new CodePrimitiveExpression(int.Parse(argument.Value.ToString())));
                declaration = new CodeAttributeDeclaration(typeof(DispIdAttribute).FullName, arguments);
            }
            bool fOverride = false;
            bool fUseNew = false;
            string str = this.ResolveConflict(pinfo.Name, propertyType, out fOverride, out fUseNew);
            if (fOverride || fUseNew)
            {
                if (data == null)
                {
                    return;
                }
                CustomAttributeTypedArgument argument2 = data.ConstructorArguments[0];
                if (!this.IsDispidKnown(int.Parse(argument2.Value.ToString()), pinfo.Name))
                {
                    str = "Ctl";
                    fOverride = false;
                    fUseNew = false;
                }
            }
            CodeMemberProperty prop = new CodeMemberProperty {
                Type = new CodeTypeReference(MapTypeName(propertyType)),
                Name = str + pinfo.Name,
                Attributes = MemberAttributes.Public
            };
            if (fOverride)
            {
                prop.Attributes |= MemberAttributes.Override;
            }
            else if (fUseNew)
            {
                prop.Attributes |= MemberAttributes.New;
            }
            bool isDefaultBind = false;
            bool flag5 = this.IsPropertyBrowsable(pinfo, alias);
            bool flag6 = this.IsPropertyBindable(pinfo, out isDefaultBind);
            if (!flag5 || (alias == ComAliasEnum.Handle))
            {
                declarations = new CodeAttributeDeclarationCollection(new CodeAttributeDeclaration[] { nobrowse, nopersist, declaration });
            }
            else if (dataSourceProp)
            {
                declarations = new CodeAttributeDeclarationCollection(new CodeAttributeDeclaration[] { declaration });
            }
            else if (fOverride || fUseNew)
            {
                declarations = new CodeAttributeDeclarationCollection(new CodeAttributeDeclaration[] { browse, nopersist, declaration });
            }
            else
            {
                declarations = new CodeAttributeDeclarationCollection(new CodeAttributeDeclaration[] { nopersist, declaration });
            }
            if (alias != ComAliasEnum.None)
            {
                CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration(typeof(ComAliasNameAttribute).FullName, new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(pinfo.PropertyType.FullName)) });
                declarations.Add(declaration2);
            }
            if (isDefaultBind)
            {
                declarations.Add(defaultBind);
            }
            else if (flag6)
            {
                declarations.Add(bindable);
            }
            prop.CustomAttributes = declarations;
            AxParameterData[] parameters = AxParameterData.Convert(pinfo.GetIndexParameters());
            if ((parameters != null) && (parameters.Length > 0))
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    CodeParameterDeclarationExpression expression = this.CreateParamDecl(parameters[i].TypeName, parameters[i].Name, false);
                    expression.Direction = parameters[i].Direction;
                    prop.Parameters.Add(expression);
                }
            }
            bool fMethodSyntax = useLet;
            if (pinfo.CanWrite)
            {
                MethodInfo method;
                if (useLet)
                {
                    method = pinfo.DeclaringType.GetMethod("let_" + pinfo.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                }
                else
                {
                    method = pinfo.GetSetMethod();
                }
                System.Type parameterType = method.GetParameters()[0].ParameterType;
                System.Type elementType = parameterType.GetElementType();
                if ((elementType != null) && (parameterType != elementType))
                {
                    fMethodSyntax = true;
                }
            }
            if (pinfo.CanRead)
            {
                this.WritePropertyGetter(prop, pinfo, alias, parameters, fMethodSyntax, fOverride, dataSourceProp);
            }
            if (pinfo.CanWrite)
            {
                this.WritePropertySetter(prop, pinfo, alias, parameters, fMethodSyntax, fOverride, useLet, dataSourceProp);
            }
            if ((parameters.Length > 0) && (prop.Name != "Item"))
            {
                CodeAttributeDeclaration declaration3 = new CodeAttributeDeclaration("System.Runtime.CompilerServices.IndexerName", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(prop.Name)) });
                prop.Name = "Item";
                prop.CustomAttributes.Add(declaration3);
            }
            if ((this.defMember != null) && this.defMember.Equals(pinfo.Name))
            {
                CodeAttributeDeclaration declaration4 = new CodeAttributeDeclaration("System.ComponentModel.DefaultProperty", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(prop.Name)) });
                cls.CustomAttributes.Add(declaration4);
            }
            cls.Members.Add(prop);
        }

        private void WritePropertyGetter(CodeMemberProperty prop, PropertyInfo pinfo, ComAliasEnum alias, AxParameterData[] parameters, bool fMethodSyntax, bool fOverride, bool dataSourceProp)
        {
            if (dataSourceProp)
            {
                string fieldName = this.CreateDataSourceFieldName(pinfo.Name);
                CodeMethodReturnStatement statement = new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName));
                prop.GetStatements.Add(statement);
            }
            else if (fOverride)
            {
                CodeConditionStatement statement2 = this.CreateValidStateCheck();
                statement2.TrueStatements.Add(this.GetPropertyGetRValue(pinfo, this.memIfaceRef, alias, parameters, fMethodSyntax));
                statement2.FalseStatements.Add(this.GetPropertyGetRValue(pinfo, new CodeBaseReferenceExpression(), ComAliasEnum.None, parameters, false));
                prop.GetStatements.Add(statement2);
            }
            else
            {
                prop.GetStatements.Add(this.CreateInvalidStateException(prop.Name, "PropertyGet"));
                prop.GetStatements.Add(this.GetPropertyGetRValue(pinfo, this.memIfaceRef, alias, parameters, fMethodSyntax));
            }
        }

        private void WritePropertySetter(CodeMemberProperty prop, PropertyInfo pinfo, ComAliasEnum alias, AxParameterData[] parameters, bool fMethodSyntax, bool fOverride, bool useLet, bool dataSourceProp)
        {
            if (!fOverride && !dataSourceProp)
            {
                prop.SetStatements.Add(this.CreateInvalidStateException(prop.Name, "PropertySet"));
            }
            if (dataSourceProp)
            {
                string dataSourceName = this.CreateDataSourceFieldName(pinfo.Name);
                this.WriteDataSourcePropertySetter(prop, pinfo, dataSourceName);
            }
            else if (!fMethodSyntax)
            {
                this.WritePropertySetterProp(prop, pinfo, alias, parameters, fOverride, useLet);
            }
            else
            {
                this.WritePropertySetterMethod(prop, pinfo, alias, parameters, fOverride, useLet);
            }
        }

        private void WritePropertySetterMethod(CodeMemberProperty prop, PropertyInfo pinfo, ComAliasEnum alias, AxParameterData[] parameters, bool fOverride, bool useLet)
        {
            CodeExpression left = null;
            CodeBinaryOperatorExpression expression2 = null;
            CodeConditionStatement statement = null;
            CodeFieldReferenceExpression expression3;
            if (fOverride)
            {
                if (parameters.Length > 0)
                {
                    left = new CodeIndexerExpression(this.memIfaceRef, new CodeExpression[0]);
                }
                else
                {
                    left = new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), pinfo.Name);
                }
                expression2 = new CodeBinaryOperatorExpression(this.memIfaceRef, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
                statement = new CodeConditionStatement {
                    Condition = expression2
                };
            }
            string methodName = useLet ? ("let_" + pinfo.Name) : pinfo.GetSetMethod().Name;
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(this.memIfaceRef, methodName, new CodeExpression[0]);
            for (int i = 0; i < parameters.Length; i++)
            {
                if (fOverride)
                {
                    ((CodeIndexerExpression) left).Indices.Add(new CodeFieldReferenceExpression(null, parameters[i].Name));
                }
                expression.Parameters.Add(new CodeFieldReferenceExpression(null, parameters[i].Name));
            }
            CodeFieldReferenceExpression right = new CodeFieldReferenceExpression(null, "value");
            CodeExpression propertySetRValue = this.GetPropertySetRValue(alias, pinfo.PropertyType);
            if (alias != ComAliasEnum.None)
            {
                CodeParameterDeclarationExpression expression7;
                string wFToComParamConverter = ComAliasConverter.GetWFToComParamConverter(alias, pinfo.PropertyType);
                if (wFToComParamConverter.Length == 0)
                {
                    expression7 = this.CreateParamDecl(MapTypeName(pinfo.PropertyType), "paramTemp", false);
                }
                else
                {
                    expression7 = this.CreateParamDecl(wFToComParamConverter, "paramTemp", false);
                }
                prop.SetStatements.Add(new CodeAssignStatement(expression7, propertySetRValue));
                expression3 = new CodeFieldReferenceExpression(null, "paramTemp");
            }
            else
            {
                expression3 = right;
            }
            expression.Parameters.Add(new CodeDirectionExpression(useLet ? FieldDirection.In : FieldDirection.Ref, expression3));
            if (fOverride)
            {
                prop.SetStatements.Add(new CodeAssignStatement(left, right));
                statement.TrueStatements.Add(new CodeExpressionStatement(expression));
                prop.SetStatements.Add(statement);
            }
            else
            {
                prop.SetStatements.Add(new CodeExpressionStatement(expression));
            }
        }

        private void WritePropertySetterProp(CodeMemberProperty prop, PropertyInfo pinfo, ComAliasEnum alias, AxParameterData[] parameters, bool fOverride, bool useLet)
        {
            CodeExpression left = null;
            CodeBinaryOperatorExpression expression2 = null;
            CodeConditionStatement statement = null;
            CodeExpression expression3;
            if (fOverride)
            {
                if (parameters.Length > 0)
                {
                    left = new CodeIndexerExpression(this.memIfaceRef, new CodeExpression[0]);
                }
                else
                {
                    left = new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), pinfo.Name);
                }
                expression2 = new CodeBinaryOperatorExpression(this.memIfaceRef, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
                statement = new CodeConditionStatement {
                    Condition = expression2
                };
            }
            if (parameters.Length > 0)
            {
                expression3 = new CodeIndexerExpression(this.memIfaceRef, new CodeExpression[0]);
            }
            else
            {
                expression3 = new CodePropertyReferenceExpression(this.memIfaceRef, pinfo.Name);
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                if (fOverride)
                {
                    ((CodeIndexerExpression) left).Indices.Add(new CodeFieldReferenceExpression(null, parameters[i].Name));
                }
                ((CodeIndexerExpression) expression3).Indices.Add(new CodeFieldReferenceExpression(null, parameters[i].Name));
            }
            CodeFieldReferenceExpression right = new CodeFieldReferenceExpression(null, "value");
            CodeExpression propertySetRValue = this.GetPropertySetRValue(alias, pinfo.PropertyType);
            if (fOverride)
            {
                prop.SetStatements.Add(new CodeAssignStatement(left, right));
                statement.TrueStatements.Add(new CodeAssignStatement(expression3, propertySetRValue));
                prop.SetStatements.Add(statement);
            }
            else
            {
                prop.SetStatements.Add(new CodeAssignStatement(expression3, propertySetRValue));
            }
        }

        private Hashtable AxHostMembers
        {
            get
            {
                if (this.axHostMembers == null)
                {
                    this.FillAxHostMembers();
                }
                return this.axHostMembers;
            }
        }

        private Hashtable ConflictableThings
        {
            get
            {
                if (this.conflictableThings == null)
                {
                    this.FillConflicatableThings();
                }
                return this.conflictableThings;
            }
        }

        private class AxMethodGenerator
        {
            private System.Type _controlType;
            private MethodInfo _method;
            private AxParameterData[] _params;
            private bool _removeOptionals;
            protected static object OriginalParamNameKey = new object();
            protected static string ReturnValueVariableName = "returnValue";

            internal AxMethodGenerator(MethodInfo method, bool removeOpts)
            {
                this._method = method;
                this._removeOptionals = removeOpts;
            }

            public static AxWrapperGen.AxMethodGenerator Create(MethodInfo method, bool removeOptionals)
            {
                if (removeOptionals && NonPrimitiveOptionalsOrMissingPresent(method))
                {
                    return new AxWrapperGen.AxReflectionInvokeMethodGenerator(method, removeOptionals);
                }
                return new AxWrapperGen.AxMethodGenerator(method, removeOptionals);
            }

            public CodeMemberMethod CreateMethod(string methodName)
            {
                return new CodeMemberMethod { Name = methodName, Attributes = MemberAttributes.Public, ReturnType = new CodeTypeReference(AxWrapperGen.MapTypeName(this._method.ReturnType)) };
            }

            public CodeExpression DoMethodInvoke(CodeMemberMethod method, string methodName, CodeExpression targetObject, List<CodeExpression> parameters)
            {
                return this.DoMethodInvokeCore(method, methodName, this._method.ReturnType, targetObject, parameters);
            }

            public virtual CodeExpression DoMethodInvokeCore(CodeMemberMethod method, string methodName, System.Type returnType, CodeExpression targetObject, List<CodeExpression> parameters)
            {
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(targetObject, methodName, new CodeExpression[0]);
                foreach (CodeExpression expression2 in parameters)
                {
                    AxParameterData data = (AxParameterData) expression2.UserData[typeof(AxParameterData)];
                    CodeExpression expression3 = expression2;
                    if (data != null)
                    {
                        expression3 = new CodeDirectionExpression(data.Direction, expression2);
                    }
                    expression.Parameters.Add(expression3);
                }
                if (returnType == typeof(void))
                {
                    method.Statements.Add(new CodeExpressionStatement(expression));
                    return null;
                }
                CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(returnType, ReturnValueVariableName, new CodeCastExpression(returnType, expression));
                method.Statements.Add(statement);
                return new CodeVariableReferenceExpression(ReturnValueVariableName);
            }

            public List<CodeExpression> GenerateAndMarshalParameters(CodeMemberMethod method)
            {
                List<CodeExpression> list = new List<CodeExpression>();
                foreach (AxParameterData data in this.Parameters)
                {
                    if (data.IsOptional && this._removeOptionals)
                    {
                        CodeExpression defaultExpressionForInvoke = GetDefaultExpressionForInvoke(this._method, data);
                        list.Add(defaultExpressionForInvoke);
                    }
                    else
                    {
                        System.Type parameterBaseType = data.ParameterBaseType;
                        AxWrapperGen.ComAliasEnum alias = AxWrapperGen.ComAliasConverter.GetComAliasEnum(this._method, parameterBaseType, null);
                        CodeVariableReferenceExpression item = new CodeVariableReferenceExpression(data.Name);
                        item.UserData[typeof(AxParameterData)] = data;
                        if (alias != AxWrapperGen.ComAliasEnum.None)
                        {
                            CodeParameterDeclarationExpression expression3 = new CodeParameterDeclarationExpression(AxWrapperGen.ComAliasConverter.GetWFTypeFromComType(parameterBaseType, alias).FullName, data.Name) {
                                Direction = data.Direction
                            };
                            method.Parameters.Add(expression3);
                            string wFToComConverter = AxWrapperGen.ComAliasConverter.GetWFToComConverter(alias);
                            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(null, wFToComConverter, new CodeExpression[0]);
                            expression.Parameters.Add(new CodeVariableReferenceExpression(data.Name));
                            item.UserData[OriginalParamNameKey] = data.Name;
                            item.VariableName = "_" + data.Name;
                            CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(parameterBaseType.FullName, item.VariableName, new CodeCastExpression(parameterBaseType, expression));
                            method.Statements.Add(statement);
                        }
                        else
                        {
                            CodeParameterDeclarationExpression expression5 = new CodeParameterDeclarationExpression(data.TypeName, data.Name) {
                                Direction = data.Direction
                            };
                            method.Parameters.Add(expression5);
                        }
                        list.Add(item);
                    }
                }
                return list;
            }

            public void GenerateReturn(CodeMemberMethod method, CodeExpression returnExpression)
            {
                if (returnExpression != null)
                {
                    AxWrapperGen.ComAliasEnum alias = AxWrapperGen.ComAliasConverter.GetComAliasEnum(this._method, this._method.ReturnType, this._method.ReturnTypeCustomAttributes);
                    if (alias != AxWrapperGen.ComAliasEnum.None)
                    {
                        string comToManagedConverter = AxWrapperGen.ComAliasConverter.GetComToManagedConverter(alias);
                        CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(null, comToManagedConverter, new CodeExpression[0]);
                        expression.Parameters.Add(returnExpression);
                        returnExpression = expression;
                        method.ReturnType = new CodeTypeReference(AxWrapperGen.ComAliasConverter.GetWFTypeFromComType(this._method.ReturnType, alias));
                    }
                    method.Statements.Add(new CodeMethodReturnStatement(returnExpression));
                }
            }

            private static object GetClsPrimitiveValue(object value)
            {
                if (value is uint)
                {
                    return Convert.ChangeType(value, typeof(int), CultureInfo.InvariantCulture);
                }
                if (value is ushort)
                {
                    return Convert.ChangeType(value, typeof(short), CultureInfo.InvariantCulture);
                }
                if (value is ulong)
                {
                    return Convert.ChangeType(value, typeof(long), CultureInfo.InvariantCulture);
                }
                if (value is sbyte)
                {
                    return Convert.ChangeType(value, typeof(byte), CultureInfo.InvariantCulture);
                }
                return value;
            }

            private static CodeExpression GetDefaultExpressionForInvoke(MethodInfo method, AxParameterData parameterInfo)
            {
                object rawDefaultValue = parameterInfo.ParameterInfo.RawDefaultValue;
                System.Type parameterBaseType = parameterInfo.ParameterBaseType;
                if (rawDefaultValue == Missing.Value)
                {
                    if (!parameterBaseType.IsPrimitive)
                    {
                        if (!parameterBaseType.IsEnum)
                        {
                            if (parameterBaseType == typeof(object))
                            {
                                return new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(null, "System.Reflection.Missing"), "Value");
                            }
                            if (parameterBaseType.IsValueType)
                            {
                                if (parameterBaseType.GetConstructor(new System.Type[0]) != null)
                                {
                                    return new CodeObjectCreateExpression(parameterBaseType, new CodeExpression[0]);
                                }
                                if (parameterBaseType == typeof(decimal))
                                {
                                    return new CodeObjectCreateExpression(typeof(decimal), new CodeExpression[] { new CodePrimitiveExpression(0.0) });
                                }
                                if (parameterBaseType != typeof(DateTime))
                                {
                                    throw new Exception(System.Design.SR.GetString("AxImpNoDefaultValue", new object[] { method.Name, parameterInfo.Name, parameterBaseType.FullName }));
                                }
                                return new CodeObjectCreateExpression(typeof(DateTime), new CodeExpression[] { new CodePrimitiveExpression(0L) });
                            }
                            if (parameterBaseType == typeof(string))
                            {
                                rawDefaultValue = "";
                            }
                            else
                            {
                                rawDefaultValue = null;
                            }
                            parameterBaseType = null;
                        }
                        else
                        {
                            rawDefaultValue = 0;
                            if (!Enum.IsDefined(parameterBaseType, 0) && (Enum.GetValues(parameterBaseType).Length > 0))
                            {
                                rawDefaultValue = Enum.GetValues(parameterBaseType).GetValue(0);
                            }
                        }
                    }
                    else
                    {
                        rawDefaultValue = GetPrimitiveDefaultValue(parameterBaseType);
                    }
                }
                else if (parameterBaseType.IsPrimitive)
                {
                    rawDefaultValue = GetClsPrimitiveValue(rawDefaultValue);
                    rawDefaultValue = GetDefaultValueForUnsignedType(parameterBaseType, rawDefaultValue);
                }
                else if (((rawDefaultValue != null) && parameterBaseType.IsInstanceOfType(rawDefaultValue)) && (((rawDefaultValue is DateTime) || (rawDefaultValue is decimal)) || (rawDefaultValue is bool)))
                {
                    if (rawDefaultValue is DateTime)
                    {
                        CodeExpression[] parameters = new CodeExpression[1];
                        DateTime time = (DateTime) rawDefaultValue;
                        parameters[0] = new CodeCastExpression(typeof(long), new CodePrimitiveExpression(time.Ticks));
                        return new CodeObjectCreateExpression(typeof(DateTime), parameters);
                    }
                    if (rawDefaultValue is decimal)
                    {
                        return new CodeObjectCreateExpression(typeof(decimal), new CodeExpression[] { new CodeCastExpression(typeof(double), new CodePrimitiveExpression(decimal.ToDouble((decimal) rawDefaultValue))) });
                    }
                    if (rawDefaultValue is bool)
                    {
                        return new CodePrimitiveExpression((bool) rawDefaultValue);
                    }
                    if (!(rawDefaultValue is string))
                    {
                        throw new Exception(System.Design.SR.GetString("AxImpUnrecognizedDefaultValueType", new object[] { method.Name, parameterInfo.Name, parameterBaseType.FullName }));
                    }
                    parameterBaseType = null;
                }
                else if (!parameterBaseType.IsValueType)
                {
                    if (rawDefaultValue is DispatchWrapper)
                    {
                        rawDefaultValue = null;
                    }
                    if ((rawDefaultValue != null) && !(rawDefaultValue is string))
                    {
                        throw new Exception(System.Design.SR.GetString("AxImpUnrecognizedDefaultValueType", new object[] { method.Name, parameterInfo.Name, parameterBaseType.FullName }));
                    }
                    parameterBaseType = null;
                    return new CodePrimitiveExpression(rawDefaultValue);
                }
                if ((parameterBaseType != null) && parameterBaseType.IsEnum)
                {
                    rawDefaultValue = (int) rawDefaultValue;
                }
                CodeExpression expression = new CodePrimitiveExpression(rawDefaultValue);
                if (parameterBaseType != null)
                {
                    expression = new CodeCastExpression(parameterBaseType, expression);
                }
                return expression;
            }

            private static object GetDefaultValueForUnsignedType(System.Type parameterType, object value)
            {
                if (parameterType == typeof(uint))
                {
                    int num = 0;
                    if (value is short)
                    {
                        num = (short) value;
                    }
                    if (value is int)
                    {
                        num = (int) value;
                    }
                    if (value is long)
                    {
                        num = (int) value;
                    }
                    return Convert.ToUInt32(Convert.ToString(num, 0x10), 0x10);
                }
                if (parameterType == typeof(ushort))
                {
                    short num2 = (short) value;
                    return Convert.ToUInt16(Convert.ToString(num2, 0x10), 0x10);
                }
                if (!(parameterType == typeof(ulong)))
                {
                    return value;
                }
                long num3 = 0L;
                if (value is short)
                {
                    num3 = (short) value;
                }
                if (value is int)
                {
                    num3 = (int) value;
                }
                if (value is long)
                {
                    num3 = (long) value;
                }
                return Convert.ToUInt64(Convert.ToString(num3, 0x10), 0x10);
            }

            private static object GetPrimitiveDefaultValue(System.Type destType)
            {
                if (!(destType == typeof(IntPtr)) && !(destType == typeof(UIntPtr)))
                {
                    return GetClsPrimitiveValue(Convert.ChangeType(0, destType, CultureInfo.InvariantCulture));
                }
                return 0;
            }

            private static bool NonPrimitiveOptionalsOrMissingPresent(MethodInfo method)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if ((parameters != null) && (parameters.Length > 0))
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].IsOptional && ((!parameters[i].ParameterType.IsPrimitive && !parameters[i].ParameterType.IsEnum) || (parameters[i].RawDefaultValue == Missing.Value)))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public void UnmarshalParameters(CodeMemberMethod method, List<CodeExpression> parameters)
            {
                foreach (CodeExpression expression in parameters)
                {
                    if (expression is CodeVariableReferenceExpression)
                    {
                        AxParameterData data = (AxParameterData) expression.UserData[typeof(AxParameterData)];
                        string variableName = (string) expression.UserData[OriginalParamNameKey];
                        if ((data.Direction != FieldDirection.In) && (variableName != null))
                        {
                            CodeExpression left = new CodeVariableReferenceExpression(variableName);
                            CodeExpression expression3 = new CodeCastExpression(data.ParameterBaseType, expression);
                            AxWrapperGen.ComAliasEnum alias = AxWrapperGen.ComAliasConverter.GetComAliasEnum(this._method, data.ParameterBaseType, null);
                            if (alias != AxWrapperGen.ComAliasEnum.None)
                            {
                                string comToManagedConverter = AxWrapperGen.ComAliasConverter.GetComToManagedConverter(alias);
                                CodeMethodInvokeExpression expression4 = new CodeMethodInvokeExpression(null, comToManagedConverter, new CodeExpression[0]);
                                expression4.Parameters.Add(expression3);
                                expression3 = expression4;
                            }
                            CodeAssignStatement statement = new CodeAssignStatement(left, expression3);
                            method.Statements.Add(statement);
                        }
                    }
                }
            }

            public System.Type ControlType
            {
                get
                {
                    return this._controlType;
                }
                set
                {
                    this._controlType = value;
                }
            }

            private AxParameterData[] Parameters
            {
                get
                {
                    if ((this._params == null) && (this._method != null))
                    {
                        this._params = AxParameterData.Convert(this._method.GetParameters());
                        if (this._params == null)
                        {
                            this._params = new AxParameterData[0];
                        }
                    }
                    return this._params;
                }
            }
        }

        private class AxReflectionInvokeMethodGenerator : AxWrapperGen.AxMethodGenerator
        {
            internal AxReflectionInvokeMethodGenerator(MethodInfo method, bool removeOpts) : base(method, removeOpts)
            {
            }

            public override CodeExpression DoMethodInvokeCore(CodeMemberMethod method, string methodName, System.Type returnType, CodeExpression targetObject, List<CodeExpression> parameters)
            {
                CodeExpression[] initializers = parameters.ToArray();
                for (int i = 0; i < initializers.Length; i++)
                {
                    CodeVariableReferenceExpression expression = initializers[i] as CodeVariableReferenceExpression;
                    if (expression != null)
                    {
                        AxParameterData data = expression.UserData[typeof(AxParameterData)] as AxParameterData;
                        if ((data != null) && (data.Direction == FieldDirection.Out))
                        {
                            initializers[i] = new CodePrimitiveExpression(null);
                        }
                    }
                }
                CodeArrayCreateExpression initExpression = new CodeArrayCreateExpression(typeof(object), initializers);
                CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(typeof(object[]), "paramArray", initExpression);
                method.Statements.Add(statement);
                CodeTypeOfExpression expression3 = new CodeTypeOfExpression(base.ControlType);
                CodeVariableDeclarationStatement statement2 = new CodeVariableDeclarationStatement(typeof(System.Type), "typeVar", expression3);
                method.Statements.Add(statement2);
                CodeMethodInvokeExpression expression4 = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("typeVar"), "GetMethod", new CodeExpression[] { new CodePrimitiveExpression(methodName) });
                CodeVariableDeclarationStatement statement3 = new CodeVariableDeclarationStatement(typeof(MethodInfo), "methodToInvoke", expression4);
                method.Statements.Add(statement3);
                List<CodeExpression> list = new List<CodeExpression> {
                    targetObject
                };
                CodeVariableReferenceExpression item = new CodeVariableReferenceExpression("paramArray");
                list.Add(item);
                CodeExpression expression6 = base.DoMethodInvokeCore(method, "Invoke", returnType, new CodeVariableReferenceExpression("methodToInvoke"), list);
                for (int j = 0; j < parameters.Count; j++)
                {
                    CodeVariableReferenceExpression left = parameters[j] as CodeVariableReferenceExpression;
                    if (left != null)
                    {
                        AxParameterData data2 = left.UserData[typeof(AxParameterData)] as AxParameterData;
                        if ((data2 != null) && (data2.Direction != FieldDirection.In))
                        {
                            CodeExpression right = new CodeCastExpression(data2.TypeName, new CodeArrayIndexerExpression(item, new CodeExpression[] { new CodePrimitiveExpression(j) }));
                            CodeAssignStatement statement4 = new CodeAssignStatement(left, right);
                            method.Statements.Add(statement4);
                        }
                    }
                }
                return expression6;
            }
        }

        private static class ComAliasConverter
        {
            private static Guid Guid_IFont = new Guid("{BEF6E002-A874-101A-8BBA-00AA00300CAB}");
            private static Guid Guid_IFontDisp = new Guid("{BEF6E003-A874-101A-8BBA-00AA00300CAB}");
            private static Guid Guid_IPicture = new Guid("{7BF80980-BF32-101A-8BBB-00AA00300CAB}");
            private static Guid Guid_IPictureDisp = new Guid("{7BF80981-BF32-101A-8BBB-00AA00300CAB}");
            private static Hashtable typeGuids;

            public static AxWrapperGen.ComAliasEnum GetComAliasEnum(MemberInfo memberInfo, System.Type type, ICustomAttributeProvider attrProvider)
            {
                string str = null;
                int num = -1;
                CustomAttributeData[] attributeData = new CustomAttributeData[0];
                if (attrProvider != null)
                {
                    attributeData = AxWrapperGen.GetAttributeData(attrProvider, typeof(ComAliasNameAttribute));
                }
                if ((attributeData != null) && (attributeData.Length > 0))
                {
                    CustomAttributeData data = attributeData[0];
                    CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
                    str = argument.Value.ToString();
                }
                if ((str != null) && (str.Length != 0))
                {
                    if (str.EndsWith(".OLE_COLOR") && IsValidType(AxWrapperGen.ComAliasEnum.Color, type))
                    {
                        return AxWrapperGen.ComAliasEnum.Color;
                    }
                    if (str.EndsWith(".OLE_HANDLE") && IsValidType(AxWrapperGen.ComAliasEnum.Handle, type))
                    {
                        return AxWrapperGen.ComAliasEnum.Handle;
                    }
                }
                if (((memberInfo is PropertyInfo) && string.Equals(memberInfo.Name, "hWnd", StringComparison.OrdinalIgnoreCase)) && IsValidType(AxWrapperGen.ComAliasEnum.Handle, type))
                {
                    return AxWrapperGen.ComAliasEnum.Handle;
                }
                if (attrProvider != null)
                {
                    attributeData = AxWrapperGen.GetAttributeData(attrProvider, typeof(DispIdAttribute));
                    if ((attributeData != null) && (attributeData.Length > 0))
                    {
                        CustomAttributeData data2 = attributeData[0];
                        CustomAttributeTypedArgument argument2 = data2.ConstructorArguments[0];
                        num = int.Parse(argument2.Value.ToString());
                    }
                }
                if ((((num == -501) || (num == -513)) || ((num == -510) || (num == -503))) && IsValidType(AxWrapperGen.ComAliasEnum.Color, type))
                {
                    return AxWrapperGen.ComAliasEnum.Color;
                }
                if ((num == -512) && IsValidType(AxWrapperGen.ComAliasEnum.Font, type))
                {
                    return AxWrapperGen.ComAliasEnum.Font;
                }
                if ((num == -523) && IsValidType(AxWrapperGen.ComAliasEnum.Picture, type))
                {
                    return AxWrapperGen.ComAliasEnum.Picture;
                }
                if ((num == -515) && IsValidType(AxWrapperGen.ComAliasEnum.Handle, type))
                {
                    return AxWrapperGen.ComAliasEnum.Handle;
                }
                if (IsValidType(AxWrapperGen.ComAliasEnum.Font, type))
                {
                    return AxWrapperGen.ComAliasEnum.Font;
                }
                if (IsValidType(AxWrapperGen.ComAliasEnum.FontDisp, type))
                {
                    return AxWrapperGen.ComAliasEnum.FontDisp;
                }
                if (IsValidType(AxWrapperGen.ComAliasEnum.Picture, type))
                {
                    return AxWrapperGen.ComAliasEnum.Picture;
                }
                if (IsValidType(AxWrapperGen.ComAliasEnum.PictureDisp, type))
                {
                    return AxWrapperGen.ComAliasEnum.PictureDisp;
                }
                return AxWrapperGen.ComAliasEnum.None;
            }

            public static string GetComToManagedConverter(AxWrapperGen.ComAliasEnum alias)
            {
                if (alias == AxWrapperGen.ComAliasEnum.Color)
                {
                    return "GetColorFromOleColor";
                }
                if (IsFont(alias))
                {
                    return "GetFontFromIFont";
                }
                if (IsPicture(alias))
                {
                    return "GetPictureFromIPicture";
                }
                return "";
            }

            public static string GetComToWFParamConverter(AxWrapperGen.ComAliasEnum alias)
            {
                if (alias == AxWrapperGen.ComAliasEnum.Color)
                {
                    return typeof(uint).FullName;
                }
                return "";
            }

            private static Guid GetGuid(System.Type t)
            {
                Guid empty = Guid.Empty;
                if (typeGuids == null)
                {
                    typeGuids = new Hashtable();
                }
                else if (typeGuids.Contains(t))
                {
                    return (Guid) typeGuids[t];
                }
                empty = t.GUID;
                typeGuids.Add(t, empty);
                return empty;
            }

            public static string GetWFToComConverter(AxWrapperGen.ComAliasEnum alias)
            {
                if (alias == AxWrapperGen.ComAliasEnum.Color)
                {
                    return "GetOleColorFromColor";
                }
                if (IsFont(alias))
                {
                    return "GetIFontFromFont";
                }
                if (IsPicture(alias))
                {
                    return "GetIPictureFromPicture";
                }
                return "";
            }

            public static string GetWFToComParamConverter(AxWrapperGen.ComAliasEnum alias, System.Type t)
            {
                return t.FullName;
            }

            public static System.Type GetWFTypeFromComType(System.Type t, AxWrapperGen.ComAliasEnum alias)
            {
                if (IsValidType(alias, t))
                {
                    if (alias == AxWrapperGen.ComAliasEnum.Color)
                    {
                        return typeof(System.Drawing.Color);
                    }
                    if (IsFont(alias))
                    {
                        return typeof(Font);
                    }
                    if (IsPicture(alias))
                    {
                        return typeof(Image);
                    }
                }
                return t;
            }

            public static bool IsFont(AxWrapperGen.ComAliasEnum e)
            {
                if (e != AxWrapperGen.ComAliasEnum.Font)
                {
                    return (e == AxWrapperGen.ComAliasEnum.FontDisp);
                }
                return true;
            }

            public static bool IsPicture(AxWrapperGen.ComAliasEnum e)
            {
                if (e != AxWrapperGen.ComAliasEnum.Picture)
                {
                    return (e == AxWrapperGen.ComAliasEnum.PictureDisp);
                }
                return true;
            }

            private static bool IsValidType(AxWrapperGen.ComAliasEnum e, System.Type t)
            {
                switch (e)
                {
                    case AxWrapperGen.ComAliasEnum.Color:
                        return ((((t == typeof(ushort)) || (t == typeof(uint))) || (t == typeof(int))) || (t == typeof(short)));

                    case AxWrapperGen.ComAliasEnum.Font:
                        return GetGuid(t).Equals(Guid_IFont);

                    case AxWrapperGen.ComAliasEnum.FontDisp:
                        return GetGuid(t).Equals(Guid_IFontDisp);

                    case AxWrapperGen.ComAliasEnum.Handle:
                        return ((((t == typeof(uint)) || (t == typeof(int))) || (t == typeof(IntPtr))) || (t == typeof(UIntPtr)));

                    case AxWrapperGen.ComAliasEnum.Picture:
                        return GetGuid(t).Equals(Guid_IPicture);

                    case AxWrapperGen.ComAliasEnum.PictureDisp:
                        return GetGuid(t).Equals(Guid_IPictureDisp);
                }
                return false;
            }
        }

        private enum ComAliasEnum
        {
            None,
            Color,
            Font,
            FontDisp,
            Handle,
            Picture,
            PictureDisp
        }

        private class EventEntry
        {
            public string eventCls;
            public MemberAttributes eventFlags;
            public string eventHandlerCls;
            public string eventName;
            public string eventParam;
            public string invokeMethodName;
            public AxParameterData[] parameters;
            public string resovledEventName;
            public System.Type retType;

            public EventEntry(string eventName, string eventCls, string eventHandlerCls, System.Type retType, AxParameterData[] parameters, bool conflict)
            {
                this.eventName = eventName;
                this.eventCls = eventCls;
                this.eventHandlerCls = eventHandlerCls;
                this.retType = retType;
                this.parameters = parameters;
                this.eventParam = eventName.ToLower(CultureInfo.InvariantCulture) + "Event";
                this.resovledEventName = conflict ? (eventName + "Event") : eventName;
                this.invokeMethodName = "RaiseOn" + this.resovledEventName;
                this.eventFlags = MemberAttributes.Public | MemberAttributes.Final;
            }
        }
    }
}

