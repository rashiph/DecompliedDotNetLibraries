namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;

    internal sealed class SerializableTypeCodeDomSerializer : CodeDomSerializer
    {
        private CodeDomSerializer originalSerializer;

        public SerializableTypeCodeDomSerializer(CodeDomSerializer originalSerializer)
        {
            if (originalSerializer == null)
            {
                throw new ArgumentNullException("originalSerializer");
            }
            this.originalSerializer = originalSerializer;
        }

        private CodeVariableReferenceExpression AddVariableExpression(IDesignerSerializationManager manager, CodeStatementCollection statements, object value)
        {
            string name = base.GetUniqueName(manager, value).Replace('`', '_');
            CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(TypeDescriptor.GetClassName(value), name) {
                InitExpression = new CodeObjectCreateExpression(TypeDescriptor.GetClassName(value), new CodeExpression[0])
            };
            statements.Add(statement);
            CodeVariableReferenceExpression expression = new CodeVariableReferenceExpression(name);
            base.SetExpression(manager, value, expression);
            return expression;
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            object obj2 = null;
            if (value != null)
            {
                CodeStatementCollection statements = null;
                ExpressionContext context = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                if (value.GetType().GetConstructor(new Type[0]) != null)
                {
                    if (value is ICollection)
                    {
                        ExpressionContext context2 = null;
                        if (context == null)
                        {
                            return obj2;
                        }
                        if (context.PresetValue != value)
                        {
                            try
                            {
                                statements = new CodeStatementCollection();
                                CodeVariableReferenceExpression expression = this.AddVariableExpression(manager, statements, value);
                                context2 = new ExpressionContext(expression, value.GetType(), context.Owner, value);
                                manager.Context.Push(context2);
                                obj2 = this.originalSerializer.Serialize(manager, value);
                                if (obj2 is CodeStatementCollection)
                                {
                                    statements.AddRange(obj2 as CodeStatementCollection);
                                    return statements;
                                }
                                if (obj2 is CodeStatement)
                                {
                                    statements.Add(obj2 as CodeStatement);
                                    return statements;
                                }
                                if (obj2 is CodeExpression)
                                {
                                    statements.Add(new CodeAssignStatement(expression, obj2 as CodeExpression));
                                }
                                return statements;
                            }
                            finally
                            {
                                if (context2 != null)
                                {
                                    manager.Context.Pop();
                                }
                            }
                        }
                        return this.originalSerializer.Serialize(manager, value);
                    }
                    statements = new CodeStatementCollection();
                    this.AddVariableExpression(manager, statements, value);
                    base.SerializeProperties(manager, statements, value, new Attribute[] { DesignOnlyAttribute.No });
                    base.SerializeEvents(manager, statements, value, new Attribute[] { DesignOnlyAttribute.No });
                    return statements;
                }
                if (context != null)
                {
                    obj2 = this.originalSerializer.Serialize(manager, value);
                }
            }
            return obj2;
        }
    }
}

