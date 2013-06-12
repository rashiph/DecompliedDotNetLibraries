namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Reflection;
    using System.Web.UI;

    internal class DataBindingExpressionBuilder : ExpressionBuilder
    {
        private const string EvalMethodName = "Eval";
        private static EventInfo eventInfo;
        private const string GetDataItemMethodName = "GetDataItem";

        internal static void BuildEvalExpression(string field, string formatString, string propertyName, Type propertyType, ControlBuilder controlBuilder, CodeStatementCollection methodStatements, CodeStatementCollection statements, CodeLinePragma linePragma, ref bool hasTempObject)
        {
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "Eval" }
            };
            expression.Parameters.Add(new CodePrimitiveExpression(field));
            if (!string.IsNullOrEmpty(formatString))
            {
                expression.Parameters.Add(new CodePrimitiveExpression(formatString));
            }
            CodeStatementCollection statements2 = new CodeStatementCollection();
            BuildPropertySetExpression(expression, propertyName, propertyType, controlBuilder, methodStatements, statements2, linePragma, ref hasTempObject);
            CodeMethodInvokeExpression left = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Page"), MethodName = "GetDataItem" }
            };
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null))
            };
            statement.TrueStatements.AddRange(statements2);
            statements.Add(statement);
        }

        internal override void BuildExpression(BoundPropertyEntry bpe, ControlBuilder controlBuilder, CodeExpression controlReference, CodeStatementCollection methodStatements, CodeStatementCollection statements, CodeLinePragma linePragma, ref bool hasTempObject)
        {
            BuildExpressionStatic(bpe, controlBuilder, controlReference, methodStatements, statements, linePragma, ref hasTempObject);
        }

        internal static void BuildExpressionSetup(ControlBuilder controlBuilder, CodeStatementCollection methodStatements, CodeStatementCollection statements)
        {
            CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(controlBuilder.ControlType, "dataBindingExpressionBuilderTarget");
            methodStatements.Add(statement);
            CodeVariableReferenceExpression left = new CodeVariableReferenceExpression(statement.Name);
            CodeAssignStatement statement2 = new CodeAssignStatement(left, new CodeCastExpression(controlBuilder.ControlType, new CodeArgumentReferenceExpression("sender")));
            statements.Add(statement2);
            Type bindingContainerType = controlBuilder.BindingContainerType;
            CodeVariableDeclarationStatement statement3 = new CodeVariableDeclarationStatement(bindingContainerType, "Container");
            methodStatements.Add(statement3);
            CodeAssignStatement statement4 = new CodeAssignStatement(new CodeVariableReferenceExpression(statement3.Name), new CodeCastExpression(bindingContainerType, new CodePropertyReferenceExpression(left, "BindingContainer")));
            statements.Add(statement4);
        }

        internal static void BuildExpressionStatic(BoundPropertyEntry bpe, ControlBuilder controlBuilder, CodeExpression controlReference, CodeStatementCollection methodStatements, CodeStatementCollection statements, CodeLinePragma linePragma, ref bool hasTempObject)
        {
            CodeExpression expression = new CodeSnippetExpression(bpe.Expression);
            BuildPropertySetExpression(expression, bpe.Name, bpe.Type, controlBuilder, methodStatements, statements, linePragma, ref hasTempObject);
        }

        private static void BuildPropertySetExpression(CodeExpression expression, string propertyName, Type propertyType, ControlBuilder controlBuilder, CodeStatementCollection methodStatements, CodeStatementCollection statements, CodeLinePragma linePragma, ref bool hasTempObject)
        {
            CodeDomUtility.CreatePropertySetStatements(methodStatements, statements, new CodeVariableReferenceExpression("dataBindingExpressionBuilderTarget"), propertyName, propertyType, expression, linePragma);
        }

        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            return null;
        }

        internal static EventInfo Event
        {
            get
            {
                if (eventInfo == null)
                {
                    eventInfo = typeof(Control).GetEvent("DataBinding");
                }
                return eventInfo;
            }
        }
    }
}

