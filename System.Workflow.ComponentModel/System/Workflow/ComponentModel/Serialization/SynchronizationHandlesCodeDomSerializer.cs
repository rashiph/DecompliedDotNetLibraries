namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel;

    internal sealed class SynchronizationHandlesCodeDomSerializer : CodeDomSerializer
    {
        public override object Serialize(IDesignerSerializationManager manager, object obj)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            CodeExpression expression = null;
            CodeStatementCollection statements = manager.Context[typeof(CodeStatementCollection)] as CodeStatementCollection;
            if (statements == null)
            {
                return expression;
            }
            Activity activity = (Activity) manager.Context[typeof(Activity)];
            base.SerializeToExpression(manager, activity);
            ICollection<string> is2 = obj as ICollection<string>;
            if (is2 == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(StringCollection).FullName }), "obj");
            }
            string uniqueName = base.GetUniqueName(manager, new StringCollection());
            statements.Add(new CodeVariableDeclarationStatement(obj.GetType(), uniqueName, new CodeObjectCreateExpression(obj.GetType(), new CodeExpression[0])));
            foreach (string str2 in is2)
            {
                statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(uniqueName), "Add"), new CodeExpression[] { new CodePrimitiveExpression(str2) }));
            }
            return new CodeVariableReferenceExpression(uniqueName);
        }
    }
}

