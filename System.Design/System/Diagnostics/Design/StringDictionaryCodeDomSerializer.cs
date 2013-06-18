namespace System.Diagnostics.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;

    internal class StringDictionaryCodeDomSerializer : CodeDomSerializer
    {
        public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
        {
            return null;
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            object obj2 = null;
            StringDictionary dictionary = value as StringDictionary;
            if (dictionary == null)
            {
                return obj2;
            }
            object current = manager.Context.Current;
            ExpressionContext context = current as ExpressionContext;
            if ((context != null) && (context.Owner == value))
            {
                current = context.Expression;
            }
            CodePropertyReferenceExpression targetObject = current as CodePropertyReferenceExpression;
            if (targetObject == null)
            {
                return obj2;
            }
            object component = base.DeserializeExpression(manager, null, targetObject.TargetObject);
            if ((component == null) || (TypeDescriptor.GetProperties(component)[targetObject.PropertyName] == null))
            {
                return obj2;
            }
            CodeStatementCollection statements = new CodeStatementCollection();
            CodeMethodReferenceExpression expression2 = new CodeMethodReferenceExpression(targetObject, "Add");
            foreach (DictionaryEntry entry in dictionary)
            {
                CodeExpression expression3 = base.SerializeToExpression(manager, entry.Key);
                CodeExpression expression4 = base.SerializeToExpression(manager, entry.Value);
                if ((expression3 != null) && (expression4 != null))
                {
                    CodeMethodInvokeExpression expression5 = new CodeMethodInvokeExpression {
                        Method = expression2
                    };
                    expression5.Parameters.Add(expression3);
                    expression5.Parameters.Add(expression4);
                    statements.Add(expression5);
                }
            }
            return statements;
        }
    }
}

