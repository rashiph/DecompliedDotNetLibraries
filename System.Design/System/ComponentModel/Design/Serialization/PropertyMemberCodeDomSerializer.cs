namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Design;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class PropertyMemberCodeDomSerializer : MemberCodeDomSerializer
    {
        private static PropertyMemberCodeDomSerializer _default;

        private object GetPropertyValue(IDesignerSerializationManager manager, PropertyDescriptor property, object value, out bool validValue)
        {
            object obj2 = null;
            validValue = true;
            try
            {
                if (!property.ShouldSerializeValue(value))
                {
                    AmbientValueAttribute attribute = (AmbientValueAttribute) property.Attributes[typeof(AmbientValueAttribute)];
                    if (attribute != null)
                    {
                        return attribute.Value;
                    }
                    DefaultValueAttribute attribute2 = (DefaultValueAttribute) property.Attributes[typeof(DefaultValueAttribute)];
                    if (attribute2 != null)
                    {
                        return attribute2.Value;
                    }
                    validValue = false;
                }
                obj2 = property.GetValue(value);
            }
            catch (Exception exception)
            {
                validValue = false;
                manager.ReportError(System.Design.SR.GetString("SerializerPropertyGenFailed", new object[] { property.Name, exception.Message }));
            }
            return obj2;
        }

        public override void Serialize(IDesignerSerializationManager manager, object value, MemberDescriptor descriptor, CodeStatementCollection statements)
        {
            PropertyDescriptor property = descriptor as PropertyDescriptor;
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (property == null)
            {
                throw new ArgumentNullException("descriptor");
            }
            if (statements == null)
            {
                throw new ArgumentNullException("statements");
            }
            try
            {
                ExtenderProvidedPropertyAttribute attribute = (ExtenderProvidedPropertyAttribute) property.Attributes[typeof(ExtenderProvidedPropertyAttribute)];
                bool isExtender = (attribute != null) && (attribute.Provider != null);
                if (property.Attributes.Contains(DesignerSerializationVisibilityAttribute.Content))
                {
                    this.SerializeContentProperty(manager, value, property, isExtender, statements);
                }
                else if (isExtender)
                {
                    this.SerializeExtenderProperty(manager, value, property, statements);
                }
                else
                {
                    this.SerializeNormalProperty(manager, value, property, statements);
                }
            }
            catch (Exception innerException)
            {
                if (innerException is TargetInvocationException)
                {
                    innerException = innerException.InnerException;
                }
                manager.ReportError(System.Design.SR.GetString("SerializerPropertyGenFailed", new object[] { property.Name, innerException.Message }));
            }
        }

        private void SerializeContentProperty(IDesignerSerializationManager manager, object value, PropertyDescriptor property, bool isExtender, CodeStatementCollection statements)
        {
            bool flag;
            object presetValue = this.GetPropertyValue(manager, property, value, out flag);
            CodeDomSerializer serializer = null;
            if (presetValue == null)
            {
                string name = manager.GetName(value);
                if (name == null)
                {
                    name = value.GetType().FullName;
                }
                manager.ReportError(System.Design.SR.GetString("SerializerNullNestedProperty", new object[] { name, property.Name }));
            }
            else
            {
                serializer = (CodeDomSerializer) manager.GetSerializer(presetValue.GetType(), typeof(CodeDomSerializer));
                if (serializer != null)
                {
                    CodeExpression targetObject = base.SerializeToExpression(manager, value);
                    if (targetObject != null)
                    {
                        CodeExpression expression = null;
                        if (isExtender)
                        {
                            ExtenderProvidedPropertyAttribute attribute = (ExtenderProvidedPropertyAttribute) property.Attributes[typeof(ExtenderProvidedPropertyAttribute)];
                            CodeExpression expression3 = base.SerializeToExpression(manager, attribute.Provider);
                            CodeExpression expression4 = base.SerializeToExpression(manager, value);
                            if ((expression3 != null) && (expression4 != null))
                            {
                                CodeMethodReferenceExpression expression5 = new CodeMethodReferenceExpression(expression3, "Get" + property.Name);
                                CodeMethodInvokeExpression expression6 = new CodeMethodInvokeExpression {
                                    Method = expression5
                                };
                                expression6.Parameters.Add(expression4);
                                expression = expression6;
                            }
                        }
                        else
                        {
                            expression = new CodePropertyReferenceExpression(targetObject, property.Name);
                        }
                        if (expression != null)
                        {
                            ExpressionContext context = new ExpressionContext(expression, property.PropertyType, value, presetValue);
                            manager.Context.Push(context);
                            object obj3 = null;
                            try
                            {
                                SerializeAbsoluteContext context2 = (SerializeAbsoluteContext) manager.Context[typeof(SerializeAbsoluteContext)];
                                if (base.IsSerialized(manager, presetValue, context2 != null))
                                {
                                    obj3 = base.GetExpression(manager, presetValue);
                                }
                                else
                                {
                                    obj3 = serializer.Serialize(manager, presetValue);
                                }
                            }
                            finally
                            {
                                manager.Context.Pop();
                            }
                            CodeStatementCollection statements2 = obj3 as CodeStatementCollection;
                            if (statements2 == null)
                            {
                                CodeStatement statement2 = obj3 as CodeStatement;
                                if (statement2 != null)
                                {
                                    statements.Add(statement2);
                                }
                            }
                            else
                            {
                                foreach (CodeStatement statement in statements2)
                                {
                                    statements.Add(statement);
                                }
                            }
                        }
                    }
                }
                else
                {
                    manager.ReportError(System.Design.SR.GetString("SerializerNoSerializerForComponent", new object[] { property.PropertyType.FullName }));
                }
            }
        }

        private void SerializeExtenderProperty(IDesignerSerializationManager manager, object value, PropertyDescriptor property, CodeStatementCollection statements)
        {
            AttributeCollection attributes = property.Attributes;
            using (CodeDomSerializerBase.TraceScope("PropertyMemberCodeDomSerializer::SerializeExtenderProperty"))
            {
                ExtenderProvidedPropertyAttribute attribute = (ExtenderProvidedPropertyAttribute) attributes[typeof(ExtenderProvidedPropertyAttribute)];
                CodeExpression targetObject = base.SerializeToExpression(manager, attribute.Provider);
                CodeExpression expression2 = base.SerializeToExpression(manager, value);
                if ((targetObject != null) && (expression2 != null))
                {
                    bool flag;
                    CodeMethodReferenceExpression expression = new CodeMethodReferenceExpression(targetObject, "Set" + property.Name);
                    object obj2 = this.GetPropertyValue(manager, property, value, out flag);
                    CodeExpression expression4 = null;
                    if (flag)
                    {
                        ExpressionContext context = null;
                        if (obj2 != value)
                        {
                            context = new ExpressionContext(expression, property.PropertyType, value);
                            manager.Context.Push(context);
                        }
                        try
                        {
                            expression4 = base.SerializeToExpression(manager, obj2);
                        }
                        finally
                        {
                            if (context != null)
                            {
                                manager.Context.Pop();
                            }
                        }
                    }
                    if (expression4 != null)
                    {
                        CodeMethodInvokeExpression expression5 = new CodeMethodInvokeExpression {
                            Method = expression
                        };
                        expression5.Parameters.Add(expression2);
                        expression5.Parameters.Add(expression4);
                        statements.Add(expression5);
                    }
                }
            }
        }

        private void SerializeNormalProperty(IDesignerSerializationManager manager, object value, PropertyDescriptor property, CodeStatementCollection statements)
        {
            using (CodeDomSerializerBase.TraceScope("CodeDomSerializer::SerializeProperty"))
            {
                CodeExpression targetObject = base.SerializeToExpression(manager, value);
                if (targetObject != null)
                {
                    CodeExpression expression = new CodePropertyReferenceExpression(targetObject, property.Name);
                    CodeExpression right = null;
                    MemberRelationshipService service = manager.GetService(typeof(MemberRelationshipService)) as MemberRelationshipService;
                    if (service != null)
                    {
                        MemberRelationship relationship = service[value, property];
                        if (relationship != MemberRelationship.Empty)
                        {
                            CodeExpression expression4 = base.SerializeToExpression(manager, relationship.Owner);
                            if (expression4 != null)
                            {
                                right = new CodePropertyReferenceExpression(expression4, relationship.Member.Name);
                            }
                        }
                    }
                    if (right == null)
                    {
                        bool flag;
                        object obj2 = this.GetPropertyValue(manager, property, value, out flag);
                        if (flag)
                        {
                            ExpressionContext context = null;
                            if (obj2 != value)
                            {
                                context = new ExpressionContext(expression, property.PropertyType, value);
                                manager.Context.Push(context);
                            }
                            try
                            {
                                right = base.SerializeToExpression(manager, obj2);
                            }
                            finally
                            {
                                if (context != null)
                                {
                                    manager.Context.Pop();
                                }
                            }
                        }
                    }
                    if (right != null)
                    {
                        CodeAssignStatement statement = new CodeAssignStatement(expression, right);
                        statements.Add(statement);
                    }
                }
            }
        }

        public override bool ShouldSerialize(IDesignerSerializationManager manager, object value, MemberDescriptor descriptor)
        {
            PropertyDescriptor member = descriptor as PropertyDescriptor;
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (member == null)
            {
                throw new ArgumentNullException("descriptor");
            }
            bool flag = member.ShouldSerializeValue(value);
            if (!flag)
            {
                SerializeAbsoluteContext context = (SerializeAbsoluteContext) manager.Context[typeof(SerializeAbsoluteContext)];
                if ((context != null) && context.ShouldSerialize(member))
                {
                    if (!member.Attributes.Contains(DesignerSerializationVisibilityAttribute.Content))
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            if (flag && !member.Attributes.Contains(DesignOnlyAttribute.Yes))
            {
                return true;
            }
            MemberRelationshipService service = manager.GetService(typeof(MemberRelationshipService)) as MemberRelationshipService;
            if (service != null)
            {
                MemberRelationship relationship = service[value, descriptor];
                if (relationship != MemberRelationship.Empty)
                {
                    return true;
                }
            }
            return false;
        }

        internal static PropertyMemberCodeDomSerializer Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new PropertyMemberCodeDomSerializer();
                }
                return _default;
            }
        }
    }
}

