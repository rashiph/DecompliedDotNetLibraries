namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;

    [DefaultSerializationProvider(typeof(CodeDomSerializationProvider))]
    public class CodeDomSerializer : CodeDomSerializerBase
    {
        private static CodeDomSerializer _default;
        private static readonly Attribute[] _designTimeFilter = new Attribute[] { DesignOnlyAttribute.Yes };
        private static readonly Attribute[] _runTimeFilter = new Attribute[] { DesignOnlyAttribute.No };
        private static CodeThisReferenceExpression _thisRef = new CodeThisReferenceExpression();

        public virtual object Deserialize(IDesignerSerializationManager manager, object codeObject)
        {
            object obj2 = null;
            if ((manager == null) || (codeObject == null))
            {
                throw new ArgumentNullException((manager == null) ? "manager" : "codeObject");
            }
            using (CodeDomSerializerBase.TraceScope("CodeDomSerializer::Deserialize"))
            {
                CodeExpression expression = codeObject as CodeExpression;
                if (expression != null)
                {
                    return base.DeserializeExpression(manager, null, expression);
                }
                CodeStatementCollection statements = codeObject as CodeStatementCollection;
                if (statements != null)
                {
                    foreach (CodeStatement statement in statements)
                    {
                        if (obj2 == null)
                        {
                            obj2 = this.DeserializeStatementToInstance(manager, statement);
                        }
                        else
                        {
                            base.DeserializeStatement(manager, statement);
                        }
                    }
                    return obj2;
                }
                if (!(codeObject is CodeStatement))
                {
                    string str = string.Format(CultureInfo.CurrentCulture, "{0}, {1}, {2}", new object[] { typeof(CodeExpression).Name, typeof(CodeStatement).Name, typeof(CodeStatementCollection).Name });
                    throw new ArgumentException(System.Design.SR.GetString("SerializerBadElementTypes", new object[] { codeObject.GetType().Name, str }));
                }
            }
            return obj2;
        }

        protected object DeserializeStatementToInstance(IDesignerSerializationManager manager, CodeStatement statement)
        {
            object obj2 = null;
            CodeVariableDeclarationStatement statement3;
            CodeAssignStatement statement2 = statement as CodeAssignStatement;
            if (statement2 != null)
            {
                CodeFieldReferenceExpression left = statement2.Left as CodeFieldReferenceExpression;
                if (left != null)
                {
                    return base.DeserializeExpression(manager, left.FieldName, statement2.Right);
                }
                CodeVariableReferenceExpression expression2 = statement2.Left as CodeVariableReferenceExpression;
                if (expression2 != null)
                {
                    return base.DeserializeExpression(manager, expression2.VariableName, statement2.Right);
                }
                base.DeserializeStatement(manager, statement2);
                return obj2;
            }
            if (((statement3 = statement as CodeVariableDeclarationStatement) != null) && (statement3.InitExpression != null))
            {
                return base.DeserializeExpression(manager, statement3.Name, statement3.InitExpression);
            }
            base.DeserializeStatement(manager, statement);
            return obj2;
        }

        public virtual string GetTargetComponentName(CodeStatement statement, CodeExpression expression, Type targetType)
        {
            string fieldName = null;
            CodeVariableReferenceExpression expression2 = expression as CodeVariableReferenceExpression;
            if (expression2 != null)
            {
                return expression2.VariableName;
            }
            CodeFieldReferenceExpression expression3 = expression as CodeFieldReferenceExpression;
            if (expression3 != null)
            {
                fieldName = expression3.FieldName;
            }
            return fieldName;
        }

        public virtual object Serialize(IDesignerSerializationManager manager, object value)
        {
            object obj2 = null;
            if ((manager == null) || (value == null))
            {
                throw new ArgumentNullException((manager == null) ? "manager" : "value");
            }
            using (CodeDomSerializerBase.TraceScope("CodeDomSerializer::Serialize"))
            {
                bool flag2;
                bool flag3;
                if (value is Type)
                {
                    return new CodeTypeOfExpression((Type) value);
                }
                bool flag = false;
                CodeExpression expression = base.SerializeCreationExpression(manager, value, out flag2);
                if (!(value is IComponent))
                {
                    flag = flag2;
                }
                ExpressionContext context = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                if ((context != null) && object.ReferenceEquals(context.PresetValue, value))
                {
                    flag3 = true;
                }
                else
                {
                    flag3 = false;
                }
                if (expression == null)
                {
                    return obj2;
                }
                if (flag)
                {
                    return expression;
                }
                CodeStatementCollection statements = new CodeStatementCollection();
                if (flag3)
                {
                    base.SetExpression(manager, value, expression, true);
                }
                else
                {
                    string uniqueName = base.GetUniqueName(manager, value);
                    CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(TypeDescriptor.GetClassName(value), uniqueName) {
                        InitExpression = expression
                    };
                    statements.Add(statement);
                    CodeExpression expression2 = new CodeVariableReferenceExpression(uniqueName);
                    base.SetExpression(manager, value, expression2);
                }
                base.SerializePropertiesToResources(manager, statements, value, _designTimeFilter);
                base.SerializeProperties(manager, statements, value, _runTimeFilter);
                base.SerializeEvents(manager, statements, value, _runTimeFilter);
                return statements;
            }
        }

        public virtual object SerializeAbsolute(IDesignerSerializationManager manager, object value)
        {
            object obj2;
            SerializeAbsoluteContext context = new SerializeAbsoluteContext();
            manager.Context.Push(context);
            try
            {
                obj2 = this.Serialize(manager, value);
            }
            finally
            {
                manager.Context.Pop();
            }
            return obj2;
        }

        public virtual CodeStatementCollection SerializeMember(IDesignerSerializationManager manager, object owningObject, MemberDescriptor member)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (owningObject == null)
            {
                throw new ArgumentNullException("owningObject");
            }
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            CodeStatementCollection statements = new CodeStatementCollection();
            if (base.GetExpression(manager, owningObject) == null)
            {
                CodeExpression expression = new CodeVariableReferenceExpression(base.GetUniqueName(manager, owningObject));
                base.SetExpression(manager, owningObject, expression);
            }
            PropertyDescriptor propertyToSerialize = member as PropertyDescriptor;
            if (propertyToSerialize != null)
            {
                base.SerializeProperty(manager, statements, owningObject, propertyToSerialize);
                return statements;
            }
            EventDescriptor descriptor = member as EventDescriptor;
            if (descriptor == null)
            {
                throw new NotSupportedException(System.Design.SR.GetString("SerializerMemberTypeNotSerializable", new object[] { member.GetType().FullName }));
            }
            base.SerializeEvent(manager, statements, owningObject, descriptor);
            return statements;
        }

        public virtual CodeStatementCollection SerializeMemberAbsolute(IDesignerSerializationManager manager, object owningObject, MemberDescriptor member)
        {
            CodeStatementCollection statements;
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (owningObject == null)
            {
                throw new ArgumentNullException("owningObject");
            }
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            SerializeAbsoluteContext context = new SerializeAbsoluteContext(member);
            manager.Context.Push(context);
            try
            {
                statements = this.SerializeMember(manager, owningObject, member);
            }
            finally
            {
                manager.Context.Pop();
            }
            return statements;
        }

        [Obsolete("This method has been deprecated. Use SerializeToExpression or GetExpression instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        protected CodeExpression SerializeToReferenceExpression(IDesignerSerializationManager manager, object value)
        {
            CodeExpression expression = null;
            using (CodeDomSerializerBase.TraceScope("CodeDomSerializer::SerializeToReferenceExpression"))
            {
                expression = base.GetExpression(manager, value);
                if ((expression != null) || !(value is IComponent))
                {
                    return expression;
                }
                string name = manager.GetName(value);
                bool flag = false;
                if (name == null)
                {
                    IReferenceService service = (IReferenceService) manager.GetService(typeof(IReferenceService));
                    if (service != null)
                    {
                        name = service.GetName(value);
                        flag = name != null;
                    }
                }
                if (name == null)
                {
                    return expression;
                }
                RootContext context = (RootContext) manager.Context[typeof(RootContext)];
                if ((context != null) && (context.Value == value))
                {
                    return context.Expression;
                }
                if (flag && (name.IndexOf('.') != -1))
                {
                    int index = name.IndexOf('.');
                    return new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(_thisRef, name.Substring(0, index)), name.Substring(index + 1));
                }
                return new CodeFieldReferenceExpression(_thisRef, name);
            }
        }

        internal static CodeDomSerializer Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new CodeDomSerializer();
                }
                return _default;
            }
        }
    }
}

