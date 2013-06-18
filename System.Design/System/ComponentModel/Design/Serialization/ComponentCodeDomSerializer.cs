namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Configuration;
    using System.Reflection;

    internal class ComponentCodeDomSerializer : CodeDomSerializer
    {
        private Type[] _containerConstructor;
        private static WeakReference _defaultSerializerRef;
        private static readonly Attribute[] _designTimeFilter = new Attribute[] { DesignOnlyAttribute.Yes };
        private static readonly Attribute[] _runTimeFilter = new Attribute[] { DesignOnlyAttribute.No };

        private bool CanCacheComponent(IDesignerSerializationManager manager, object value, PropertyDescriptorCollection props)
        {
            IComponent component = value as IComponent;
            if (component != null)
            {
                if (component.Site != null)
                {
                    INestedSite site = component.Site as INestedSite;
                    if ((site != null) && !string.IsNullOrEmpty(site.FullName))
                    {
                        return false;
                    }
                }
                if (props == null)
                {
                    props = TypeDescriptor.GetProperties(component);
                }
                foreach (PropertyDescriptor descriptor in props)
                {
                    if (typeof(IComponent).IsAssignableFrom(descriptor.PropertyType) && !descriptor.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden))
                    {
                        MemberCodeDomSerializer serializer = (MemberCodeDomSerializer) manager.GetSerializer(descriptor.GetType(), typeof(MemberCodeDomSerializer));
                        if ((serializer != null) && serializer.ShouldSerialize(manager, value, descriptor))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        protected override object DeserializeInstance(IDesignerSerializationManager manager, Type type, object[] parameters, string name, bool addToContainer)
        {
            object obj2 = base.DeserializeInstance(manager, type, parameters, name, addToContainer);
            if (obj2 != null)
            {
                base.DeserializePropertiesFromResources(manager, obj2, _designTimeFilter);
            }
            return obj2;
        }

        private Type[] GetContainerConstructor(IDesignerSerializationManager manager)
        {
            if (this._containerConstructor == null)
            {
                this._containerConstructor = new Type[] { CodeDomSerializerBase.GetReflectionTypeFromTypeHelper(manager, typeof(IContainer)) };
            }
            return this._containerConstructor;
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            CodeStatementCollection statements = null;
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(value);
            using (CodeDomSerializerBase.TraceScope("ComponentCodeDomSerializer::Serialize"))
            {
                if ((manager == null) || (value == null))
                {
                    throw new ArgumentNullException((manager == null) ? "manager" : "value");
                }
                if (base.IsSerialized(manager, value))
                {
                    return base.GetExpression(manager, value);
                }
                InheritanceLevel notInherited = InheritanceLevel.NotInherited;
                InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(value)[typeof(InheritanceAttribute)];
                if (attribute != null)
                {
                    notInherited = attribute.InheritanceLevel;
                }
                if (notInherited == InheritanceLevel.InheritedReadOnly)
                {
                    return statements;
                }
                statements = new CodeStatementCollection();
                CodeTypeDeclaration declaration = manager.Context[typeof(CodeTypeDeclaration)] as CodeTypeDeclaration;
                RootContext context = manager.Context[typeof(RootContext)] as RootContext;
                CodeExpression left = null;
                bool flag = false;
                bool flag2 = true;
                bool flag3 = true;
                bool flag4 = false;
                left = base.GetExpression(manager, value);
                if (left != null)
                {
                    flag = false;
                    flag2 = false;
                    flag3 = false;
                    IComponent component = value as IComponent;
                    if ((component != null) && (component.Site == null))
                    {
                        ExpressionContext context2 = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                        if ((context2 == null) || (context2.PresetValue != value))
                        {
                            flag4 = true;
                        }
                    }
                }
                else
                {
                    if (notInherited == InheritanceLevel.NotInherited)
                    {
                        PropertyDescriptor descriptor = properties["GenerateMember"];
                        if (((descriptor != null) && (descriptor.PropertyType == typeof(bool))) && !((bool) descriptor.GetValue(value)))
                        {
                            flag = true;
                            flag2 = false;
                        }
                    }
                    else
                    {
                        flag3 = false;
                    }
                    if (context == null)
                    {
                        flag = true;
                        flag2 = false;
                    }
                }
                manager.Context.Push(value);
                manager.Context.Push(statements);
                try
                {
                    try
                    {
                        string name = manager.GetName(value);
                        string className = TypeDescriptor.GetClassName(value);
                        if ((flag2 || flag) && (name != null))
                        {
                            if (flag2)
                            {
                                if (notInherited == InheritanceLevel.NotInherited)
                                {
                                    MemberAttributes @private;
                                    CodeMemberField field = new CodeMemberField(className, name);
                                    PropertyDescriptor descriptor2 = properties["Modifiers"];
                                    if (descriptor2 == null)
                                    {
                                        descriptor2 = properties["DefaultModifiers"];
                                    }
                                    if ((descriptor2 != null) && (descriptor2.PropertyType == typeof(MemberAttributes)))
                                    {
                                        @private = (MemberAttributes) descriptor2.GetValue(value);
                                    }
                                    else
                                    {
                                        @private = MemberAttributes.Private;
                                    }
                                    field.Attributes = @private;
                                    declaration.Members.Add(field);
                                }
                                left = new CodeFieldReferenceExpression(context.Expression, name);
                            }
                            else
                            {
                                if (notInherited == InheritanceLevel.NotInherited)
                                {
                                    CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(className, name);
                                    statements.Add(statement);
                                }
                                left = new CodeVariableReferenceExpression(name);
                            }
                        }
                        if (flag3)
                        {
                            CodeExpression expression2;
                            IContainer service = manager.GetService(typeof(IContainer)) as IContainer;
                            ConstructorInfo info = null;
                            if (service != null)
                            {
                                info = CodeDomSerializerBase.GetReflectionTypeHelper(manager, value).GetConstructor(BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, this.GetContainerConstructor(manager), null);
                            }
                            if (info != null)
                            {
                                expression2 = new CodeObjectCreateExpression(className, new CodeExpression[] { base.SerializeToExpression(manager, service) });
                            }
                            else
                            {
                                bool flag5;
                                expression2 = base.SerializeCreationExpression(manager, value, out flag5);
                            }
                            if (expression2 != null)
                            {
                                if (left == null)
                                {
                                    if (flag4)
                                    {
                                        left = expression2;
                                    }
                                }
                                else
                                {
                                    CodeAssignStatement statement2 = new CodeAssignStatement(left, expression2);
                                    statements.Add(statement2);
                                }
                            }
                        }
                        if (left != null)
                        {
                            base.SetExpression(manager, value, left);
                        }
                        if ((left != null) && !flag4)
                        {
                            bool flag6 = value is ISupportInitialize;
                            if (flag6)
                            {
                                string fullName = typeof(ISupportInitialize).FullName;
                                flag6 = manager.GetType(fullName) != null;
                            }
                            Type c = null;
                            if (flag6)
                            {
                                c = CodeDomSerializerBase.GetReflectionTypeHelper(manager, value);
                                flag6 = CodeDomSerializerBase.GetReflectionTypeFromTypeHelper(manager, typeof(ISupportInitialize)).IsAssignableFrom(c);
                            }
                            bool flag7 = (value is IPersistComponentSettings) && ((IPersistComponentSettings) value).SaveSettings;
                            if (flag7)
                            {
                                string typeName = typeof(IPersistComponentSettings).FullName;
                                flag7 = manager.GetType(typeName) != null;
                            }
                            if (flag7)
                            {
                                c = c ?? CodeDomSerializerBase.GetReflectionTypeHelper(manager, value);
                                flag7 = CodeDomSerializerBase.GetReflectionTypeFromTypeHelper(manager, typeof(IPersistComponentSettings)).IsAssignableFrom(c);
                            }
                            IDesignerSerializationManager manager2 = (IDesignerSerializationManager) manager.GetService(typeof(IDesignerSerializationManager));
                            if (flag6)
                            {
                                this.SerializeSupportInitialize(manager, statements, left, value, "BeginInit");
                            }
                            base.SerializePropertiesToResources(manager, statements, value, _designTimeFilter);
                            ComponentCache serviceInstance = (ComponentCache) manager.GetService(typeof(ComponentCache));
                            ComponentCache.Entry entry = null;
                            if (serviceInstance == null)
                            {
                                IServiceContainer container2 = (IServiceContainer) manager.GetService(typeof(IServiceContainer));
                                if (container2 != null)
                                {
                                    serviceInstance = new ComponentCache(manager);
                                    container2.AddService(typeof(ComponentCache), serviceInstance);
                                }
                            }
                            else if (((manager == manager2) && (serviceInstance != null)) && serviceInstance.Enabled)
                            {
                                entry = serviceInstance[value];
                            }
                            if ((entry == null) || entry.Tracking)
                            {
                                if (entry == null)
                                {
                                    entry = new ComponentCache.Entry(serviceInstance);
                                    ComponentCache.Entry entryAll = null;
                                    entryAll = serviceInstance.GetEntryAll(value);
                                    if (((entryAll != null) && (entryAll.Dependencies != null)) && (entryAll.Dependencies.Count > 0))
                                    {
                                        foreach (object obj2 in entryAll.Dependencies)
                                        {
                                            entry.AddDependency(obj2);
                                        }
                                    }
                                }
                                entry.Component = value;
                                bool flag8 = manager == manager2;
                                entry.Valid = flag8 && this.CanCacheComponent(manager, value, properties);
                                if ((flag8 && (serviceInstance != null)) && serviceInstance.Enabled)
                                {
                                    manager.Context.Push(serviceInstance);
                                    manager.Context.Push(entry);
                                }
                                try
                                {
                                    entry.Statements = new CodeStatementCollection();
                                    base.SerializeProperties(manager, entry.Statements, value, _runTimeFilter);
                                    base.SerializeEvents(manager, entry.Statements, value, null);
                                    foreach (CodeStatement statement3 in entry.Statements)
                                    {
                                        if (statement3 is CodeVariableDeclarationStatement)
                                        {
                                            entry.Tracking = true;
                                            break;
                                        }
                                    }
                                    if (entry.Statements.Count > 0)
                                    {
                                        entry.Statements.Insert(0, new CodeCommentStatement(string.Empty));
                                        entry.Statements.Insert(0, new CodeCommentStatement(name));
                                        entry.Statements.Insert(0, new CodeCommentStatement(string.Empty));
                                        if ((flag8 && (serviceInstance != null)) && serviceInstance.Enabled)
                                        {
                                            serviceInstance[value] = entry;
                                        }
                                    }
                                }
                                finally
                                {
                                    if ((flag8 && (serviceInstance != null)) && serviceInstance.Enabled)
                                    {
                                        manager.Context.Pop();
                                        manager.Context.Pop();
                                    }
                                }
                            }
                            else if (((entry.Resources != null) || (entry.Metadata != null)) && ((serviceInstance != null) && serviceInstance.Enabled))
                            {
                                ResourceCodeDomSerializer.Default.ApplyCacheEntry(manager, entry);
                            }
                            statements.AddRange(entry.Statements);
                            if (flag7)
                            {
                                this.SerializeLoadComponentSettings(manager, statements, left, value);
                            }
                            if (flag6)
                            {
                                this.SerializeSupportInitialize(manager, statements, left, value, "EndInit");
                            }
                        }
                        return statements;
                    }
                    catch (CheckoutException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        manager.ReportError(exception);
                    }
                    return statements;
                }
                finally
                {
                    manager.Context.Pop();
                    manager.Context.Pop();
                }
            }
            return statements;
        }

        private void SerializeLoadComponentSettings(IDesignerSerializationManager manager, CodeStatementCollection statements, CodeExpression valueExpression, object value)
        {
            CodeTypeReference targetType = new CodeTypeReference(typeof(IPersistComponentSettings));
            CodeCastExpression targetObject = new CodeCastExpression(targetType, valueExpression);
            CodeMethodReferenceExpression expression2 = new CodeMethodReferenceExpression(targetObject, "LoadComponentSettings");
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = expression2
            };
            CodeExpressionStatement statement = new CodeExpressionStatement(expression);
            statement.UserData["statement-ordering"] = "end";
            statements.Add(statement);
        }

        private void SerializeSupportInitialize(IDesignerSerializationManager manager, CodeStatementCollection statements, CodeExpression valueExpression, object value, string methodName)
        {
            CodeTypeReference targetType = new CodeTypeReference(typeof(ISupportInitialize));
            CodeCastExpression targetObject = new CodeCastExpression(targetType, valueExpression);
            CodeMethodReferenceExpression expression2 = new CodeMethodReferenceExpression(targetObject, methodName);
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = expression2
            };
            CodeExpressionStatement statement = new CodeExpressionStatement(expression);
            if (methodName == "BeginInit")
            {
                statement.UserData["statement-ordering"] = "begin";
            }
            else
            {
                statement.UserData["statement-ordering"] = "end";
            }
            statements.Add(statement);
        }

        internal static ComponentCodeDomSerializer Default
        {
            get
            {
                ComponentCodeDomSerializer target;
                if (_defaultSerializerRef != null)
                {
                    target = _defaultSerializerRef.Target as ComponentCodeDomSerializer;
                    if (target != null)
                    {
                        return target;
                    }
                }
                target = new ComponentCodeDomSerializer();
                _defaultSerializerRef = new WeakReference(target);
                return target;
            }
        }
    }
}

