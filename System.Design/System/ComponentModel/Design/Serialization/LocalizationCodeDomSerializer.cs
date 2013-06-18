namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel;
    using System.Resources;

    internal class LocalizationCodeDomSerializer : CodeDomSerializer
    {
        private CodeDomSerializer _currentSerializer;
        private CodeDomLocalizationModel _model;

        internal LocalizationCodeDomSerializer(CodeDomLocalizationModel model, object currentSerializer)
        {
            this._model = model;
            this._currentSerializer = currentSerializer as CodeDomSerializer;
        }

        private bool EmitApplyMethod(IDesignerSerializationManager manager, object owner)
        {
            ApplyMethodTable context = (ApplyMethodTable) manager.Context[typeof(ApplyMethodTable)];
            if (context == null)
            {
                context = new ApplyMethodTable();
                manager.Context.Append(context);
            }
            if (!context.Contains(owner))
            {
                context.Add(owner);
                return true;
            }
            return false;
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            PropertyDescriptor descriptor = (PropertyDescriptor) manager.Context[typeof(PropertyDescriptor)];
            ExpressionContext context = (ExpressionContext) manager.Context[typeof(ExpressionContext)];
            bool flag = (value != null) ? CodeDomSerializerBase.GetReflectionTypeHelper(manager, value).IsSerializable : true;
            bool flag2 = !flag;
            bool flag3 = (descriptor != null) && descriptor.Attributes.Contains(DesignerSerializationVisibilityAttribute.Content);
            if (!flag2)
            {
                flag2 = ((context != null) && (context.PresetValue != null)) && (context.PresetValue == value);
            }
            if (((this._model == CodeDomLocalizationModel.PropertyReflection) && !flag3) && !flag2)
            {
                CodeStatementCollection statements = (CodeStatementCollection) manager.Context[typeof(CodeStatementCollection)];
                bool flag4 = false;
                ExtenderProvidedPropertyAttribute attribute = null;
                if (descriptor != null)
                {
                    attribute = descriptor.Attributes[typeof(ExtenderProvidedPropertyAttribute)] as ExtenderProvidedPropertyAttribute;
                    if ((attribute != null) && (attribute.ExtenderProperty != null))
                    {
                        flag4 = true;
                    }
                }
                if ((!flag4 && (context != null)) && (statements != null))
                {
                    string name = manager.GetName(context.Owner);
                    CodeExpression expression = base.SerializeToExpression(manager, context.Owner);
                    if ((name != null) && (expression != null))
                    {
                        RootContext context2 = manager.Context[typeof(RootContext)] as RootContext;
                        if ((context2 != null) && (context2.Value == context.Owner))
                        {
                            name = "$this";
                        }
                        base.SerializeToResourceExpression(manager, value, false);
                        if (this.EmitApplyMethod(manager, context.Owner))
                        {
                            ResourceManager manager2 = manager.Context[typeof(ResourceManager)] as ResourceManager;
                            CodeMethodReferenceExpression expression3 = new CodeMethodReferenceExpression(base.GetExpression(manager, manager2), "ApplyResources");
                            CodeMethodInvokeExpression expression4 = new CodeMethodInvokeExpression {
                                Method = expression3
                            };
                            expression4.Parameters.Add(expression);
                            expression4.Parameters.Add(new CodePrimitiveExpression(name));
                            statements.Add(expression4);
                        }
                        return null;
                    }
                }
            }
            if (flag2)
            {
                return this._currentSerializer.Serialize(manager, value);
            }
            return base.SerializeToResourceExpression(manager, value);
        }

        private class ApplyMethodTable
        {
            private Hashtable _table = new Hashtable();

            internal void Add(object value)
            {
                this._table.Add(value, value);
            }

            internal bool Contains(object value)
            {
                return this._table.ContainsKey(value);
            }
        }
    }
}

