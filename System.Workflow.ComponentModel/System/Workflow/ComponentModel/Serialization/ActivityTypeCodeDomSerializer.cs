namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel;

    public class ActivityTypeCodeDomSerializer : TypeCodeDomSerializer
    {
        private static object _initMethodKey = new object();
        private const string _initMethodName = "InitializeComponent";

        public override object Deserialize(IDesignerSerializationManager manager, CodeTypeDeclaration declaration)
        {
            return base.Deserialize(manager, declaration);
        }

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
                CodeConstructor constructor = new CodeConstructor {
                    Attributes = MemberAttributes.Public
                };
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

        public override CodeTypeDeclaration Serialize(IDesignerSerializationManager manager, object root, ICollection members)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }
            Activity activity = root as Activity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(Activity).FullName }), "root");
            }
            CodeTypeDeclaration declaration = base.Serialize(manager, root, members);
            CodeMemberMethod method = declaration.UserData[_initMethodKey] as CodeMemberMethod;
            if ((method != null) && (activity is CompositeActivity))
            {
                CodeStatement[] array = new CodeStatement[method.Statements.Count];
                method.Statements.CopyTo(array, 0);
                method.Statements.Clear();
                CodeAssignStatement statement = new CodeAssignStatement {
                    Left = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "CanModifyActivities"),
                    Right = new CodePrimitiveExpression(true)
                };
                method.Statements.Add(statement);
                foreach (CodeStatement statement2 in array)
                {
                    method.Statements.Add(statement2);
                }
                CodeAssignStatement statement3 = new CodeAssignStatement {
                    Left = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "CanModifyActivities"),
                    Right = new CodePrimitiveExpression(false)
                };
                method.Statements.Add(statement3);
            }
            foreach (CodeTypeMember member in declaration.Members)
            {
                CodeMemberField field = member as CodeMemberField;
                if (field != null)
                {
                    foreach (object obj2 in members)
                    {
                        if (!(obj2 is Activity))
                        {
                            throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(Activity).FullName }), "members");
                        }
                        Activity activity2 = obj2 as Activity;
                        if (((field.Name == manager.GetName(activity2)) && (((int) activity2.GetValue(ActivityMarkupSerializer.StartLineProperty)) != -1)) && (activity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty) != null))
                        {
                            field.LinePragma = new CodeLinePragma((string) activity.GetValue(ActivityCodeDomSerializer.MarkupFileNameProperty), Math.Max((int) activity2.GetValue(ActivityMarkupSerializer.StartLineProperty), 1));
                        }
                    }
                }
            }
            return declaration;
        }
    }
}

