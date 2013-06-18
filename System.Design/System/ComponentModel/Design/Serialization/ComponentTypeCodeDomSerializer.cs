namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.CodeDom;

    internal class ComponentTypeCodeDomSerializer : TypeCodeDomSerializer
    {
        private static ComponentTypeCodeDomSerializer _default;
        private static object _initMethodKey = new object();
        private const string _initMethodName = "InitializeComponent";

        protected override CodeMemberMethod GetInitializeMethod(IDesignerSerializationManager manager, CodeTypeDeclaration typeDecl, object value)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (typeDecl == null)
            {
                throw new ArgumentNullException("typeDecl");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            CodeMemberMethod method = typeDecl.UserData[_initMethodKey] as CodeMemberMethod;
            if (method == null)
            {
                method = new CodeMemberMethod {
                    Name = "InitializeComponent",
                    Attributes = MemberAttributes.Private
                };
                typeDecl.UserData[_initMethodKey] = method;
                CodeConstructor constructor = new CodeConstructor();
                constructor.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InitializeComponent", new CodeExpression[0]));
                typeDecl.Members.Add(constructor);
            }
            return method;
        }

        protected override CodeMemberMethod[] GetInitializeMethods(IDesignerSerializationManager manager, CodeTypeDeclaration typeDecl)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (typeDecl == null)
            {
                throw new ArgumentNullException("typeDecl");
            }
            foreach (CodeTypeMember member in typeDecl.Members)
            {
                CodeMemberMethod method = member as CodeMemberMethod;
                if (((method != null) && method.Name.Equals("InitializeComponent")) && (method.Parameters.Count == 0))
                {
                    return new CodeMemberMethod[] { method };
                }
            }
            return new CodeMemberMethod[0];
        }

        internal static ComponentTypeCodeDomSerializer Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new ComponentTypeCodeDomSerializer();
                }
                return _default;
            }
        }
    }
}

