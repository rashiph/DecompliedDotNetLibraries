namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;

    internal class ContainerCodeDomSerializer : CodeDomSerializer
    {
        private const string _containerName = "components";
        private static ContainerCodeDomSerializer _defaultSerializer;

        protected override object DeserializeInstance(IDesignerSerializationManager manager, Type type, object[] parameters, string name, bool addToContainer)
        {
            if (typeof(IContainer).IsAssignableFrom(type))
            {
                object service = manager.GetService(typeof(IContainer));
                if (service != null)
                {
                    manager.SetName(service, name);
                    return service;
                }
            }
            return base.DeserializeInstance(manager, type, parameters, name, addToContainer);
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            CodeExpression expression;
            CodeTypeDeclaration declaration = manager.Context[typeof(CodeTypeDeclaration)] as CodeTypeDeclaration;
            RootContext context = manager.Context[typeof(RootContext)] as RootContext;
            CodeStatementCollection statements = new CodeStatementCollection();
            if ((declaration != null) && (context != null))
            {
                CodeMemberField field = new CodeMemberField(typeof(IContainer), "components") {
                    Attributes = MemberAttributes.Private
                };
                declaration.Members.Add(field);
                expression = new CodeFieldReferenceExpression(context.Expression, "components");
            }
            else
            {
                CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(typeof(IContainer), "components");
                statements.Add(statement);
                expression = new CodeVariableReferenceExpression("components");
            }
            base.SetExpression(manager, value, expression);
            CodeObjectCreateExpression right = new CodeObjectCreateExpression(typeof(Container), new CodeExpression[0]);
            CodeAssignStatement statement2 = new CodeAssignStatement(expression, right);
            statement2.UserData["IContainer"] = "IContainer";
            statements.Add(statement2);
            return statements;
        }

        internal static ContainerCodeDomSerializer Default
        {
            get
            {
                if (_defaultSerializer == null)
                {
                    _defaultSerializer = new ContainerCodeDomSerializer();
                }
                return _defaultSerializer;
            }
        }
    }
}

