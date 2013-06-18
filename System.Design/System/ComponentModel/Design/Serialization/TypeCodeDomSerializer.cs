namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Design;

    [DefaultSerializationProvider(typeof(CodeDomSerializationProvider))]
    public class TypeCodeDomSerializer : CodeDomSerializerBase
    {
        private static TypeCodeDomSerializer _default;
        private static readonly Attribute[] _designTimeFilter = new Attribute[] { DesignOnlyAttribute.Yes };
        private static object _initMethodKey = new object();
        private IDictionary _nameTable;
        private static readonly Attribute[] _runTimeFilter = new Attribute[] { DesignOnlyAttribute.No };
        private Dictionary<string, CodeDomSerializerBase.OrderedCodeStatementCollection> _statementTable;

        public virtual object Deserialize(IDesignerSerializationManager manager, CodeTypeDeclaration declaration)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (declaration == null)
            {
                throw new ArgumentNullException("declaration");
            }
            object obj2 = null;
            using (CodeDomSerializerBase.TraceScope("TypeCodeDomSerializer::Deserialize"))
            {
                bool caseInsensitive = false;
                CodeDomProvider service = manager.GetService(typeof(CodeDomProvider)) as CodeDomProvider;
                if (service != null)
                {
                    caseInsensitive = (service.LanguageOptions & LanguageOptions.CaseInsensitive) != LanguageOptions.None;
                }
                Type type = null;
                string name = declaration.Name;
                foreach (CodeTypeReference reference in declaration.BaseTypes)
                {
                    Type type2 = manager.GetType(CodeDomSerializerBase.GetTypeNameFromCodeTypeReference(manager, reference));
                    name = reference.BaseType;
                    if ((type2 != null) && !type2.IsInterface)
                    {
                        type = type2;
                        break;
                    }
                }
                if (type == null)
                {
                    CodeDomSerializerBase.Error(manager, System.Design.SR.GetString("SerializerTypeNotFound", new object[] { name }), "SerializerTypeNotFound");
                }
                if (CodeDomSerializerBase.GetReflectionTypeFromTypeHelper(manager, type).IsAbstract)
                {
                    CodeDomSerializerBase.Error(manager, System.Design.SR.GetString("SerializerTypeAbstract", new object[] { type.FullName }), "SerializerTypeAbstract");
                }
                ResolveNameEventHandler handler = new ResolveNameEventHandler(this.OnResolveName);
                manager.ResolveName += handler;
                obj2 = manager.CreateInstance(type, null, declaration.Name, true);
                int count = declaration.Members.Count;
                this._nameTable = new HybridDictionary(count, caseInsensitive);
                this._statementTable = new Dictionary<string, CodeDomSerializerBase.OrderedCodeStatementCollection>(count);
                Dictionary<string, string> names = new Dictionary<string, string>(count);
                RootContext context = new RootContext(new CodeThisReferenceExpression(), obj2);
                manager.Context.Push(context);
                try
                {
                    StringComparison comparisonType = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                    foreach (CodeTypeMember member in declaration.Members)
                    {
                        CodeMemberField field = member as CodeMemberField;
                        if ((field != null) && !string.Equals(field.Name, declaration.Name, comparisonType))
                        {
                            this._nameTable[field.Name] = field;
                            if ((field.Type != null) && !string.IsNullOrEmpty(field.Type.BaseType))
                            {
                                names[field.Name] = CodeDomSerializerBase.GetTypeNameFromCodeTypeReference(manager, field.Type);
                            }
                        }
                    }
                    CodeMemberMethod[] initializeMethods = this.GetInitializeMethods(manager, declaration);
                    if (initializeMethods == null)
                    {
                        throw new InvalidOperationException();
                    }
                    foreach (CodeMemberMethod method in initializeMethods)
                    {
                        foreach (CodeStatement statement in method.Statements)
                        {
                            CodeVariableDeclarationStatement statement2 = statement as CodeVariableDeclarationStatement;
                            if (statement2 != null)
                            {
                                this._nameTable[statement2.Name] = statement;
                            }
                        }
                    }
                    this._nameTable[declaration.Name] = context.Expression;
                    foreach (CodeMemberMethod method2 in initializeMethods)
                    {
                        CodeDomSerializerBase.FillStatementTable(manager, this._statementTable, names, method2.Statements, declaration.Name);
                    }
                    PropertyDescriptor descriptor = manager.Properties["SupportsStatementGeneration"];
                    if (((descriptor != null) && (descriptor.PropertyType == typeof(bool))) && ((bool) descriptor.GetValue(manager)))
                    {
                        foreach (string str2 in this._nameTable.Keys)
                        {
                            if (!this._statementTable.ContainsKey(str2))
                            {
                                continue;
                            }
                            CodeStatementCollection statements = this._statementTable[str2];
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
                                this._statementTable.Remove(str2);
                            }
                        }
                    }
                    base.DeserializePropertiesFromResources(manager, obj2, _designTimeFilter);
                    CodeDomSerializerBase.OrderedCodeStatementCollection[] array = new CodeDomSerializerBase.OrderedCodeStatementCollection[this._statementTable.Count];
                    this._statementTable.Values.CopyTo(array, 0);
                    Array.Sort(array, StatementOrderComparer.Default);
                    CodeDomSerializerBase.OrderedCodeStatementCollection statements2 = null;
                    foreach (CodeDomSerializerBase.OrderedCodeStatementCollection statements3 in array)
                    {
                        if (statements3.Name.Equals(declaration.Name))
                        {
                            statements2 = statements3;
                        }
                        else
                        {
                            this.DeserializeName(manager, statements3.Name, statements3);
                        }
                    }
                    if (statements2 != null)
                    {
                        this.DeserializeName(manager, statements2.Name, statements2);
                    }
                }
                finally
                {
                    this._nameTable = null;
                    this._statementTable = null;
                    manager.ResolveName -= handler;
                    manager.Context.Pop();
                }
            }
            return obj2;
        }

        private object DeserializeName(IDesignerSerializationManager manager, string name, CodeStatementCollection statements)
        {
            object obj2 = null;
            using (CodeDomSerializerBase.TraceScope("RootCodeDomSerializer::DeserializeName"))
            {
                obj2 = this._nameTable[name];
                CodeObject obj3 = obj2 as CodeObject;
                string typeName = null;
                CodeMemberField field = null;
                if (obj3 != null)
                {
                    obj2 = null;
                    this._nameTable[name] = null;
                    CodeVariableDeclarationStatement statement = obj3 as CodeVariableDeclarationStatement;
                    if (statement != null)
                    {
                        typeName = CodeDomSerializerBase.GetTypeNameFromCodeTypeReference(manager, statement.Type);
                    }
                    else
                    {
                        field = obj3 as CodeMemberField;
                        if (field != null)
                        {
                            typeName = CodeDomSerializerBase.GetTypeNameFromCodeTypeReference(manager, field.Type);
                        }
                        else
                        {
                            CodeExpression expression = obj3 as CodeExpression;
                            RootContext context = manager.Context[typeof(RootContext)] as RootContext;
                            if (((context != null) && (expression != null)) && (context.Expression == expression))
                            {
                                obj2 = context.Value;
                                typeName = TypeDescriptor.GetClassName(obj2);
                            }
                        }
                    }
                }
                else if (obj2 == null)
                {
                    IContainer service = (IContainer) manager.GetService(typeof(IContainer));
                    if (service != null)
                    {
                        IComponent component = service.Components[name];
                        if (component != null)
                        {
                            typeName = component.GetType().FullName;
                            this._nameTable[name] = component;
                        }
                    }
                }
                if (typeName == null)
                {
                    return obj2;
                }
                Type valueType = manager.GetType(typeName);
                if (valueType == null)
                {
                    manager.ReportError(new CodeDomSerializerException(System.Design.SR.GetString("SerializerTypeNotFound", new object[] { typeName }), manager));
                    return obj2;
                }
                if ((statements == null) && this._statementTable.ContainsKey(name))
                {
                    statements = this._statementTable[name];
                }
                if ((statements == null) || (statements.Count <= 0))
                {
                    return obj2;
                }
                CodeDomSerializer serializer = base.GetSerializer(manager, valueType);
                if (serializer == null)
                {
                    manager.ReportError(new CodeDomSerializerException(System.Design.SR.GetString("SerializerNoSerializerForComponent", new object[] { valueType.FullName }), manager));
                    return obj2;
                }
                try
                {
                    obj2 = serializer.Deserialize(manager, statements);
                    if ((obj2 != null) && (field != null))
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(obj2)["Modifiers"];
                        if ((descriptor != null) && (descriptor.PropertyType == typeof(MemberAttributes)))
                        {
                            MemberAttributes attributes = field.Attributes & MemberAttributes.AccessMask;
                            descriptor.SetValue(obj2, attributes);
                        }
                    }
                    this._nameTable[name] = obj2;
                }
                catch (Exception exception)
                {
                    manager.ReportError(exception);
                }
            }
            return obj2;
        }

        protected virtual CodeMemberMethod GetInitializeMethod(IDesignerSerializationManager manager, CodeTypeDeclaration declaration, object value)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (declaration == null)
            {
                throw new ArgumentNullException("declaration");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            CodeConstructor constructor = declaration.UserData[_initMethodKey] as CodeConstructor;
            if (constructor == null)
            {
                constructor = new CodeConstructor();
                declaration.UserData[_initMethodKey] = constructor;
            }
            return constructor;
        }

        protected virtual CodeMemberMethod[] GetInitializeMethods(IDesignerSerializationManager manager, CodeTypeDeclaration declaration)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (declaration == null)
            {
                throw new ArgumentNullException("declaration");
            }
            foreach (CodeTypeMember member in declaration.Members)
            {
                CodeConstructor constructor = member as CodeConstructor;
                if ((constructor != null) && (constructor.Parameters.Count == 0))
                {
                    return new CodeMemberMethod[] { constructor };
                }
            }
            return new CodeMemberMethod[0];
        }

        private void IntegrateStatements(IDesignerSerializationManager manager, object root, ICollection members, StatementContext statementCxt, CodeTypeDeclaration typeDecl)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            List<CodeMethodMap> list = new List<CodeMethodMap>();
            if (members != null)
            {
                foreach (object obj2 in members)
                {
                    if (obj2 != root)
                    {
                        CodeStatementCollection statements = statementCxt.StatementCollection[obj2];
                        if (statements != null)
                        {
                            CodeMethodMap map;
                            int num;
                            CodeMemberMethod method = this.GetInitializeMethod(manager, typeDecl, obj2);
                            if (method == null)
                            {
                                throw new InvalidOperationException();
                            }
                            if (dictionary.TryGetValue(method.Name, out num))
                            {
                                map = list[num];
                            }
                            else
                            {
                                map = new CodeMethodMap(method);
                                list.Add(map);
                                dictionary[method.Name] = list.Count - 1;
                            }
                            if (statements.Count > 0)
                            {
                                map.Add(statements);
                            }
                        }
                    }
                }
            }
            CodeStatementCollection statements2 = statementCxt.StatementCollection[root];
            if (statements2 != null)
            {
                CodeMethodMap map2;
                int num2;
                CodeMemberMethod method2 = this.GetInitializeMethod(manager, typeDecl, root);
                if (method2 == null)
                {
                    throw new InvalidOperationException();
                }
                if (dictionary.TryGetValue(method2.Name, out num2))
                {
                    map2 = list[num2];
                }
                else
                {
                    map2 = new CodeMethodMap(method2);
                    list.Add(map2);
                    dictionary[method2.Name] = list.Count - 1;
                }
                if (statements2.Count > 0)
                {
                    map2.Add(statements2);
                }
            }
            foreach (CodeMethodMap map3 in list)
            {
                map3.Combine();
                typeDecl.Members.Add(map3.Method);
            }
        }

        private void OnResolveName(object sender, ResolveNameEventArgs e)
        {
            using (CodeDomSerializerBase.TraceScope("RootCodeDomSerializer::OnResolveName"))
            {
                if (e.Value == null)
                {
                    IDesignerSerializationManager manager = (IDesignerSerializationManager) sender;
                    e.Value = this.DeserializeName(manager, e.Name, null);
                }
            }
        }

        public virtual CodeTypeDeclaration Serialize(IDesignerSerializationManager manager, object root, ICollection members)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }
            CodeTypeDeclaration declaration = new CodeTypeDeclaration(manager.GetName(root));
            CodeThisReferenceExpression expression = new CodeThisReferenceExpression();
            RootContext context = new RootContext(expression, root);
            StatementContext context2 = new StatementContext();
            context2.StatementCollection.Populate(root);
            if (members != null)
            {
                context2.StatementCollection.Populate(members);
            }
            declaration.BaseTypes.Add(root.GetType());
            manager.Context.Push(declaration);
            manager.Context.Push(context);
            manager.Context.Push(context2);
            try
            {
                if (members != null)
                {
                    foreach (object obj2 in members)
                    {
                        if (obj2 != root)
                        {
                            base.SerializeToExpression(manager, obj2);
                        }
                    }
                }
                base.SerializeToExpression(manager, root);
                this.IntegrateStatements(manager, root, members, context2, declaration);
            }
            finally
            {
                manager.Context.Pop();
                manager.Context.Pop();
                manager.Context.Pop();
            }
            return declaration;
        }

        internal static TypeCodeDomSerializer Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new TypeCodeDomSerializer();
                }
                return _default;
            }
        }

        private class StatementOrderComparer : IComparer
        {
            public static readonly TypeCodeDomSerializer.StatementOrderComparer Default = new TypeCodeDomSerializer.StatementOrderComparer();

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

