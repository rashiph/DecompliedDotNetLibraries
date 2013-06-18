namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class TypeReferenceExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeTypeReferenceExpression expression2 = (CodeTypeReferenceExpression) expression;
            return new CodeTypeReferenceExpression(CloneType(expression2.Type));
        }

        internal static CodeTypeReference CloneType(CodeTypeReference oldType)
        {
            if (oldType == null)
            {
                return null;
            }
            CodeTypeReference result = new CodeTypeReference {
                ArrayElementType = CloneType(oldType.ArrayElementType),
                ArrayRank = oldType.ArrayRank,
                BaseType = oldType.BaseType
            };
            foreach (CodeTypeReference reference2 in oldType.TypeArguments)
            {
                result.TypeArguments.Add(CloneType(reference2));
            }
            ConditionHelper.CloneUserData(oldType, result);
            return result;
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeTypeReferenceExpression expression2 = (CodeTypeReferenceExpression) expression;
            RuleDecompiler.DecompileType(stringBuilder, expression2.Type);
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            return new RuleLiteralResult(null);
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeTypeReferenceExpression expression2 = (CodeTypeReferenceExpression) expression;
            CodeTypeReferenceExpression expression3 = (CodeTypeReferenceExpression) comperand;
            return MatchType(expression2.Type, expression3.Type);
        }

        internal static bool MatchType(CodeTypeReference typeRef1, CodeTypeReference typeRef2)
        {
            if (typeRef1.BaseType != typeRef2.BaseType)
            {
                return false;
            }
            if (typeRef1.TypeArguments.Count != typeRef2.TypeArguments.Count)
            {
                return false;
            }
            for (int i = 0; i < typeRef1.TypeArguments.Count; i++)
            {
                CodeTypeReference reference = typeRef1.TypeArguments[i];
                CodeTypeReference reference2 = typeRef2.TypeArguments[i];
                if (!MatchType(reference, reference2))
                {
                    return false;
                }
            }
            return true;
        }

        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            CodeTypeReferenceExpression expression2 = (CodeTypeReferenceExpression) expression;
            if (expression2.Type == null)
            {
                ValidationError item = new ValidationError(Messages.NullTypeType, 0x53d);
                item.UserData["ErrorObject"] = expression2;
                validation.Errors.Add(item);
                return null;
            }
            if (isWritten)
            {
                ValidationError error2 = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, new object[] { typeof(CodeTypeReferenceExpression).ToString() }), 0x17a);
                error2.UserData["ErrorObject"] = expression2;
                validation.Errors.Add(error2);
                return null;
            }
            return new RuleExpressionInfo(validation.ResolveType(expression2.Type));
        }
    }
}

