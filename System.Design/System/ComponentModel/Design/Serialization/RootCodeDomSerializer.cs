namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Design;
    using System.Globalization;
    using System.Runtime.Serialization;

    internal sealed class RootCodeDomSerializer : ComponentCodeDomSerializer
    {
        private bool containerRequired;
        private static readonly Attribute[] designTimeProperties = new Attribute[] { DesignOnlyAttribute.Yes };
        private CodeMemberMethod initMethod;
        private IDictionary nameTable;
        private static readonly Attribute[] runTimeProperties = new Attribute[] { DesignOnlyAttribute.No };
        private IDictionary statementTable;

        private void AddStatement(string name, CodeStatement statement)
        {
            CodeDomSerializerBase.OrderedCodeStatementCollection statements = (CodeDomSerializerBase.OrderedCodeStatementCollection) this.statementTable[name];
            if (statements == null)
            {
                statements = new CodeDomSerializerBase.OrderedCodeStatementCollection {
                    Order = this.statementTable.Count,
                    Name = name
                };
                this.statementTable[name] = statements;
            }
            statements.Add(statement);
        }

        public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
        {
            if ((manager == null) || (codeObject == null))
            {
                throw new ArgumentNullException((manager == null) ? "manager" : "codeObject");
            }
            object obj2 = null;
            using (CodeDomSerializerBase.TraceScope("RootCodeDomSerializer::Deserialize"))
            {
                if (!(codeObject is CodeTypeDeclaration))
                {
                    throw new ArgumentException(System.Design.SR.GetString("SerializerBadElementType", new object[] { typeof(CodeTypeDeclaration).FullName }));
                }
                bool caseInsensitive = false;
                CodeDomProvider service = manager.GetService(typeof(CodeDomProvider)) as CodeDomProvider;
                if (service != null)
                {
                    caseInsensitive = (service.LanguageOptions & LanguageOptions.CaseInsensitive) != LanguageOptions.None;
                }
                CodeTypeDeclaration declaration = (CodeTypeDeclaration) codeObject;
                CodeTypeReference reference = null;
                Type type = null;
                foreach (CodeTypeReference reference2 in declaration.BaseTypes)
                {
                    Type type2 = manager.GetType(CodeDomSerializerBase.GetTypeNameFromCodeTypeReference(manager, reference2));
                    if ((type2 != null) && !type2.IsInterface)
                    {
                        reference = reference2;
                        type = type2;
                        break;
                    }
                }
                if (type == null)
                {
                    Exception exception = new SerializationException(System.Design.SR.GetString("SerializerTypeNotFound", new object[] { reference.BaseType })) {
                        HelpLink = "SerializerTypeNotFound"
                    };
                    throw exception;
                }
                if (type.IsAbstract)
                {
                    Exception exception2 = new SerializationException(System.Design.SR.GetString("SerializerTypeAbstract", new object[] { type.FullName })) {
                        HelpLink = "SerializerTypeAbstract"
                    };
                    throw exception2;
                }
                ResolveNameEventHandler handler = new ResolveNameEventHandler(this.OnResolveName);
                manager.ResolveName += handler;
                if (!(manager is DesignerSerializationManager))
                {
                    manager.AddSerializationProvider(new CodeDomSerializationProvider());
                }
                obj2 = manager.CreateInstance(type, null, declaration.Name, true);
                this.nameTable = new HybridDictionary(declaration.Members.Count, caseInsensitive);
                this.statementTable = new HybridDictionary(declaration.Members.Count, caseInsensitive);
                this.initMethod = null;
                RootContext context = new RootContext(new CodeThisReferenceExpression(), obj2);
                manager.Context.Push(context);
                try
                {
                    foreach (CodeTypeMember member in declaration.Members)
                    {
                        if (member is CodeMemberField)
                        {
                            if (string.Compare(member.Name, declaration.Name, caseInsensitive, CultureInfo.InvariantCulture) != 0)
                            {
                                this.nameTable[member.Name] = member;
                            }
                        }
                        else if ((this.initMethod == null) && (member is CodeMemberMethod))
                        {
                            CodeMemberMethod method = (CodeMemberMethod) member;
                            if ((string.Compare(method.Name, this.InitMethodName, caseInsensitive, CultureInfo.InvariantCulture) == 0) && (method.Parameters.Count == 0))
                            {
                                this.initMethod = method;
                            }
                        }
                    }
                    if (this.initMethod != null)
                    {
                        foreach (CodeStatement statement in this.initMethod.Statements)
                        {
                            CodeVariableDeclarationStatement statement2 = statement as CodeVariableDeclarationStatement;
                            if (statement2 != null)
                            {
                                this.nameTable[statement2.Name] = statement;
                            }
                        }
                    }
                    if (this.nameTable[declaration.Name] != null)
                    {
                        this.nameTable[declaration.Name] = obj2;
                    }
                    if (this.initMethod != null)
                    {
                        this.FillStatementTable(this.initMethod, declaration.Name);
                    }
                    PropertyDescriptor descriptor = manager.Properties["SupportsStatementGeneration"];
                    if (((descriptor != null) && (descriptor.PropertyType == typeof(bool))) && ((bool) descriptor.GetValue(manager)))
                    {
                        foreach (string str in this.nameTable.Keys)
                        {
                            CodeDomSerializerBase.OrderedCodeStatementCollection statements = (CodeDomSerializerBase.OrderedCodeStatementCollection) this.statementTable[str];
                            if (statements != null)
                            {
                                bool flag2 = false;
                                foreach (CodeStatement statement3 in statements)
                                {
                                    object obj3 = statement3.UserData["GeneratedStatement"];
                                    if (((obj3 == null) || !(obj3 is bool)) || !((bool) obj3))
                                    {
                                        flag2 = true;
                                        break;
                                    }
                                }
                                if (!flag2)
                                {
                                    this.statementTable.Remove(str);
                                }
                            }
                        }
                    }
                    IContainer container = (IContainer) manager.GetService(typeof(IContainer));
                    if (container != null)
                    {
                        foreach (object obj4 in container.Components)
                        {
                            base.DeserializePropertiesFromResources(manager, obj4, designTimeProperties);
                        }
                    }
                    object[] array = new object[this.statementTable.Values.Count];
                    this.statementTable.Values.CopyTo(array, 0);
                    Array.Sort(array, StatementOrderComparer.Default);
                    foreach (CodeDomSerializerBase.OrderedCodeStatementCollection statements2 in array)
                    {
                        string name = statements2.Name;
                        if ((name != null) && !name.Equals(declaration.Name))
                        {
                            this.DeserializeName(manager, name);
                        }
                    }
                    CodeStatementCollection statements3 = (CodeStatementCollection) this.statementTable[declaration.Name];
                    if ((statements3 != null) && (statements3.Count > 0))
                    {
                        foreach (CodeStatement statement4 in statements3)
                        {
                            base.DeserializeStatement(manager, statement4);
                        }
                    }
                    return obj2;
                }
                finally
                {
                    manager.ResolveName -= handler;
                    this.initMethod = null;
                    this.nameTable = null;
                    this.statementTable = null;
                    manager.Context.Pop();
                }
            }
            return obj2;
        }

        private object DeserializeName(IDesignerSerializationManager manager, string name)
        {
            string typeName = null;
            Type objectType = null;
            object obj2 = this.nameTable[name];
            using (CodeDomSerializerBase.TraceScope("RootCodeDomSerializer::DeserializeName"))
            {
                CodeMemberField field = null;
                CodeObject obj3 = obj2 as CodeObject;
                if (obj3 != null)
                {
                    obj2 = null;
                    this.nameTable[name] = null;
                    if (obj3 is CodeVariableDeclarationStatement)
                    {
                        CodeVariableDeclarationStatement statement = (CodeVariableDeclarationStatement) obj3;
                        typeName = CodeDomSerializerBase.GetTypeNameFromCodeTypeReference(manager, statement.Type);
                    }
                    else if (obj3 is CodeMemberField)
                    {
                        field = (CodeMemberField) obj3;
                        typeName = CodeDomSerializerBase.GetTypeNameFromCodeTypeReference(manager, field.Type);
                    }
                }
                else
                {
                    if (obj2 != null)
                    {
                        return obj2;
                    }
                    IContainer service = (IContainer) manager.GetService(typeof(IContainer));
                    if (service != null)
                    {
                        IComponent component = service.Components[name];
                        if (component != null)
                        {
                            typeName = component.GetType().FullName;
                            this.nameTable[name] = component;
                        }
                    }
                }
                if (name.Equals(this.ContainerName))
                {
                    IContainer container2 = (IContainer) manager.GetService(typeof(IContainer));
                    if (container2 != null)
                    {
                        obj2 = container2;
                    }
                }
                else if (typeName != null)
                {
                    objectType = manager.GetType(typeName);
                    if (objectType == null)
                    {
                        manager.ReportError(new SerializationException(System.Design.SR.GetString("SerializerTypeNotFound", new object[] { typeName })));
                    }
                    else
                    {
                        CodeStatementCollection codeObject = (CodeStatementCollection) this.statementTable[name];
                        if ((codeObject != null) && (codeObject.Count > 0))
                        {
                            CodeDomSerializer serializer = (CodeDomSerializer) manager.GetSerializer(objectType, typeof(CodeDomSerializer));
                            if (serializer == null)
                            {
                                manager.ReportError(System.Design.SR.GetString("SerializerNoSerializerForComponent", new object[] { objectType.FullName }));
                            }
                            else
                            {
                                try
                                {
                                    obj2 = serializer.Deserialize(manager, codeObject);
                                    if ((obj2 != null) && (field != null))
                                    {
                                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(obj2)["Modifiers"];
                                        if ((descriptor != null) && (descriptor.PropertyType == typeof(MemberAttributes)))
                                        {
                                            MemberAttributes attributes = field.Attributes & MemberAttributes.AccessMask;
                                            descriptor.SetValue(obj2, attributes);
                                        }
                                    }
                                }
                                catch (Exception exception)
                                {
                                    manager.ReportError(exception);
                                }
                            }
                        }
                    }
                }
                this.nameTable[name] = obj2;
            }
            return obj2;
        }

        private void FillStatementTable(CodeMemberMethod method, string className)
        {
            using (CodeDomSerializerBase.TraceScope("RootCodeDomSerializer::FillStatementTable"))
            {
                foreach (CodeStatement statement in method.Statements)
                {
                    CodeExpression left = null;
                    if (statement is CodeAssignStatement)
                    {
                        left = ((CodeAssignStatement) statement).Left;
                    }
                    else if (statement is CodeAttachEventStatement)
                    {
                        left = ((CodeAttachEventStatement) statement).Event;
                    }
                    else if (statement is CodeRemoveEventStatement)
                    {
                        left = ((CodeRemoveEventStatement) statement).Event;
                    }
                    else if (statement is CodeExpressionStatement)
                    {
                        left = ((CodeExpressionStatement) statement).Expression;
                    }
                    else if (statement is CodeVariableDeclarationStatement)
                    {
                        CodeVariableDeclarationStatement statement2 = (CodeVariableDeclarationStatement) statement;
                        if ((statement2.InitExpression != null) && this.nameTable.Contains(statement2.Name))
                        {
                            this.AddStatement(statement2.Name, statement2);
                        }
                        left = null;
                    }
                    if (left == null)
                    {
                        continue;
                    }
                Label_00C4:
                    while (left is CodeCastExpression)
                    {
                        left = ((CodeCastExpression) left).Expression;
                    }
                    if (left is CodeDelegateCreateExpression)
                    {
                        left = ((CodeDelegateCreateExpression) left).TargetObject;
                        goto Label_00C4;
                    }
                    if (left is CodeDelegateInvokeExpression)
                    {
                        left = ((CodeDelegateInvokeExpression) left).TargetObject;
                        goto Label_00C4;
                    }
                    if (left is CodeDirectionExpression)
                    {
                        left = ((CodeDirectionExpression) left).Expression;
                        goto Label_00C4;
                    }
                    if (left is CodeEventReferenceExpression)
                    {
                        left = ((CodeEventReferenceExpression) left).TargetObject;
                        goto Label_00C4;
                    }
                    if (left is CodeMethodInvokeExpression)
                    {
                        left = ((CodeMethodInvokeExpression) left).Method;
                        goto Label_00C4;
                    }
                    if (left is CodeMethodReferenceExpression)
                    {
                        left = ((CodeMethodReferenceExpression) left).TargetObject;
                        goto Label_00C4;
                    }
                    if (left is CodeArrayIndexerExpression)
                    {
                        left = ((CodeArrayIndexerExpression) left).TargetObject;
                        goto Label_00C4;
                    }
                    if (left is CodeFieldReferenceExpression)
                    {
                        CodeFieldReferenceExpression expression2 = (CodeFieldReferenceExpression) left;
                        if (expression2.TargetObject is CodeThisReferenceExpression)
                        {
                            this.AddStatement(expression2.FieldName, statement);
                            continue;
                        }
                        left = expression2.TargetObject;
                        goto Label_00C4;
                    }
                    if (left is CodePropertyReferenceExpression)
                    {
                        CodePropertyReferenceExpression expression3 = (CodePropertyReferenceExpression) left;
                        if ((expression3.TargetObject is CodeThisReferenceExpression) && this.nameTable.Contains(expression3.PropertyName))
                        {
                            this.AddStatement(expression3.PropertyName, statement);
                            continue;
                        }
                        left = expression3.TargetObject;
                        goto Label_00C4;
                    }
                    if (left is CodeVariableReferenceExpression)
                    {
                        CodeVariableReferenceExpression expression4 = (CodeVariableReferenceExpression) left;
                        if (this.nameTable.Contains(expression4.VariableName))
                        {
                            this.AddStatement(expression4.VariableName, statement);
                        }
                    }
                    else if ((left is CodeThisReferenceExpression) || (left is CodeBaseReferenceExpression))
                    {
                        this.AddStatement(className, statement);
                    }
                }
            }
        }

        private string GetMethodName(object statement)
        {
            string str = null;
            while (str == null)
            {
                if (statement is CodeExpressionStatement)
                {
                    statement = ((CodeExpressionStatement) statement).Expression;
                }
                else
                {
                    if (statement is CodeMethodInvokeExpression)
                    {
                        statement = ((CodeMethodInvokeExpression) statement).Method;
                        continue;
                    }
                    if (statement is CodeMethodReferenceExpression)
                    {
                        return ((CodeMethodReferenceExpression) statement).MethodName;
                    }
                    return str;
                }
            }
            return str;
        }

        private void OnResolveName(object sender, ResolveNameEventArgs e)
        {
            using (CodeDomSerializerBase.TraceScope("RootCodeDomSerializer::OnResolveName"))
            {
                if (e.Value == null)
                {
                    IDesignerSerializationManager manager = (IDesignerSerializationManager) sender;
                    object obj2 = this.DeserializeName(manager, e.Name);
                    e.Value = obj2;
                }
            }
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            if ((manager == null) || (value == null))
            {
                throw new ArgumentNullException((manager == null) ? "manager" : "value");
            }
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(manager.GetName(value));
            RootContext context = new RootContext(new CodeThisReferenceExpression(), value);
            using (CodeDomSerializerBase.TraceScope("RootCodeDomSerializer::Serialize"))
            {
                declaration.BaseTypes.Add(value.GetType());
                this.containerRequired = false;
                manager.Context.Push(context);
                manager.Context.Push(this);
                manager.Context.Push(declaration);
                if (!(manager is DesignerSerializationManager))
                {
                    manager.AddSerializationProvider(new CodeDomSerializationProvider());
                }
                try
                {
                    if (value is IComponent)
                    {
                        ISite site = ((IComponent) value).Site;
                        if (site == null)
                        {
                            return declaration;
                        }
                        ICollection components = site.Container.Components;
                        StatementContext context2 = new StatementContext();
                        context2.StatementCollection.Populate(components);
                        manager.Context.Push(context2);
                        try
                        {
                            foreach (IComponent component in components)
                            {
                                if ((component != value) && !base.IsSerialized(manager, component))
                                {
                                    if (base.GetSerializer(manager, component) != null)
                                    {
                                        base.SerializeToExpression(manager, component);
                                    }
                                    else
                                    {
                                        manager.ReportError(System.Design.SR.GetString("SerializerNoSerializerForComponent", new object[] { component.GetType().FullName }));
                                    }
                                }
                            }
                            manager.Context.Push(value);
                            try
                            {
                                if ((base.GetSerializer(manager, value) != null) && !base.IsSerialized(manager, value))
                                {
                                    base.SerializeToExpression(manager, value);
                                }
                                else
                                {
                                    manager.ReportError(System.Design.SR.GetString("SerializerNoSerializerForComponent", new object[] { value.GetType().FullName }));
                                }
                            }
                            finally
                            {
                                manager.Context.Pop();
                            }
                        }
                        finally
                        {
                            manager.Context.Pop();
                        }
                        CodeMemberMethod method = new CodeMemberMethod {
                            Name = this.InitMethodName,
                            Attributes = MemberAttributes.Private
                        };
                        declaration.Members.Add(method);
                        ArrayList elements = new ArrayList();
                        foreach (object obj2 in components)
                        {
                            if (obj2 != value)
                            {
                                elements.Add(context2.StatementCollection[obj2]);
                            }
                        }
                        if (context2.StatementCollection[value] != null)
                        {
                            elements.Add(context2.StatementCollection[value]);
                        }
                        if (this.ContainerRequired)
                        {
                            this.SerializeContainerDeclaration(manager, method.Statements);
                        }
                        this.SerializeElementsToStatements(elements, method.Statements);
                    }
                    return declaration;
                }
                finally
                {
                    manager.Context.Pop();
                    manager.Context.Pop();
                    manager.Context.Pop();
                }
            }
            return declaration;
        }

        private void SerializeContainerDeclaration(IDesignerSerializationManager manager, CodeStatementCollection statements)
        {
            CodeTypeDeclaration declaration = (CodeTypeDeclaration) manager.Context[typeof(CodeTypeDeclaration)];
            if (declaration != null)
            {
                Type type = typeof(IContainer);
                CodeTypeReference reference = new CodeTypeReference(type);
                CodeMemberField field = new CodeMemberField(reference, this.ContainerName) {
                    Attributes = MemberAttributes.Private
                };
                declaration.Members.Add(field);
                type = typeof(Container);
                reference = new CodeTypeReference(type);
                CodeObjectCreateExpression right = new CodeObjectCreateExpression {
                    CreateType = reference
                };
                CodeFieldReferenceExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), this.ContainerName);
                CodeAssignStatement statement = new CodeAssignStatement(left, right);
                statements.Add(statement);
            }
        }

        private void SerializeElementsToStatements(ArrayList elements, CodeStatementCollection statements)
        {
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            ArrayList list3 = new ArrayList();
            ArrayList list4 = new ArrayList();
            ArrayList list5 = new ArrayList();
            foreach (object obj2 in elements)
            {
                if ((obj2 is CodeAssignStatement) && (((CodeAssignStatement) obj2).Left is CodeFieldReferenceExpression))
                {
                    list4.Add(obj2);
                }
                else if (obj2 is CodeVariableDeclarationStatement)
                {
                    list3.Add(obj2);
                }
                else
                {
                    if (!(obj2 is CodeStatement))
                    {
                        goto Label_0110;
                    }
                    string str = ((CodeObject) obj2).UserData["statement-ordering"] as string;
                    if (str == null)
                    {
                        goto Label_0101;
                    }
                    string str3 = str;
                    if (str3 == null)
                    {
                        goto Label_00F2;
                    }
                    if (!(str3 == "begin"))
                    {
                        if (str3 == "end")
                        {
                            goto Label_00E4;
                        }
                        if (str3 == "default")
                        {
                        }
                        goto Label_00F2;
                    }
                    list.Add(obj2);
                }
                continue;
            Label_00E4:
                list2.Add(obj2);
                continue;
            Label_00F2:
                list5.Add(obj2);
                continue;
            Label_0101:
                list5.Add(obj2);
                continue;
            Label_0110:
                if (obj2 is CodeStatementCollection)
                {
                    CodeStatementCollection statements2 = (CodeStatementCollection) obj2;
                    foreach (CodeStatement statement in statements2)
                    {
                        if ((statement is CodeAssignStatement) && (((CodeAssignStatement) statement).Left is CodeFieldReferenceExpression))
                        {
                            list4.Add(statement);
                        }
                        else if (statement is CodeVariableDeclarationStatement)
                        {
                            list3.Add(statement);
                        }
                        else
                        {
                            string str2 = statement.UserData["statement-ordering"] as string;
                            if (str2 == null)
                            {
                                goto Label_01F0;
                            }
                            string str4 = str2;
                            if (str4 == null)
                            {
                                goto Label_01E4;
                            }
                            if (!(str4 == "begin"))
                            {
                                if (str4 == "end")
                                {
                                    goto Label_01D9;
                                }
                                if (str4 == "default")
                                {
                                }
                                goto Label_01E4;
                            }
                            list.Add(statement);
                        }
                        continue;
                    Label_01D9:
                        list2.Add(statement);
                        continue;
                    Label_01E4:
                        list5.Add(statement);
                        continue;
                    Label_01F0:
                        list5.Add(statement);
                    }
                }
            }
            statements.AddRange((CodeStatement[]) list3.ToArray(typeof(CodeStatement)));
            statements.AddRange((CodeStatement[]) list4.ToArray(typeof(CodeStatement)));
            statements.AddRange((CodeStatement[]) list.ToArray(typeof(CodeStatement)));
            statements.AddRange((CodeStatement[]) list5.ToArray(typeof(CodeStatement)));
            statements.AddRange((CodeStatement[]) list2.ToArray(typeof(CodeStatement)));
        }

        private CodeStatementCollection SerializeRootObject(IDesignerSerializationManager manager, object value, bool designTime)
        {
            if (((CodeTypeDeclaration) manager.Context[typeof(CodeTypeDeclaration)]) == null)
            {
                return null;
            }
            CodeStatementCollection statements = new CodeStatementCollection();
            using (CodeDomSerializerBase.TraceScope("RootCodeDomSerializer::SerializeRootObject"))
            {
                if (designTime)
                {
                    base.SerializeProperties(manager, statements, value, designTimeProperties);
                    return statements;
                }
                base.SerializeProperties(manager, statements, value, runTimeProperties);
                base.SerializeEvents(manager, statements, value, null);
            }
            return statements;
        }

        public string ContainerName
        {
            get
            {
                return "components";
            }
        }

        public bool ContainerRequired
        {
            get
            {
                return this.containerRequired;
            }
            set
            {
                this.containerRequired = value;
            }
        }

        public string InitMethodName
        {
            get
            {
                return "InitializeComponent";
            }
        }

        private class ComponentComparer : IComparer
        {
            public static readonly RootCodeDomSerializer.ComponentComparer Default = new RootCodeDomSerializer.ComponentComparer();

            private ComponentComparer()
            {
            }

            public int Compare(object left, object right)
            {
                int num = string.Compare(((IComponent) left).GetType().Name, ((IComponent) right).GetType().Name, false, CultureInfo.InvariantCulture);
                if (num == 0)
                {
                    num = string.Compare(((IComponent) left).Site.Name, ((IComponent) right).Site.Name, true, CultureInfo.InvariantCulture);
                }
                return num;
            }
        }

        private class StatementOrderComparer : IComparer
        {
            public static readonly RootCodeDomSerializer.StatementOrderComparer Default = new RootCodeDomSerializer.StatementOrderComparer();

            private StatementOrderComparer()
            {
            }

            public int Compare(object left, object right)
            {
                CodeDomSerializerBase.OrderedCodeStatementCollection statements = left as CodeDomSerializerBase.OrderedCodeStatementCollection;
                CodeDomSerializerBase.OrderedCodeStatementCollection statements2 = right as CodeDomSerializerBase.OrderedCodeStatementCollection;
                if (left == null)
                {
                    return 1;
                }
                if (right == null)
                {
                    return -1;
                }
                if (right == left)
                {
                    return 0;
                }
                return (statements.Order - statements2.Order);
            }
        }
    }
}

