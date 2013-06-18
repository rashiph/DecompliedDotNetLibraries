namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    public static class RuleExpressionWalker
    {
        private static TypeWrapperTuple[] typeWrappers = new TypeWrapperTuple[] { new TypeWrapperTuple(typeof(CodeThisReferenceExpression), new ThisExpression()), new TypeWrapperTuple(typeof(CodePrimitiveExpression), new PrimitiveExpression()), new TypeWrapperTuple(typeof(CodeFieldReferenceExpression), new FieldReferenceExpression()), new TypeWrapperTuple(typeof(CodePropertyReferenceExpression), new PropertyReferenceExpression()), new TypeWrapperTuple(typeof(CodeBinaryOperatorExpression), new BinaryExpression()), new TypeWrapperTuple(typeof(CodeMethodInvokeExpression), new MethodInvokeExpression()), new TypeWrapperTuple(typeof(CodeIndexerExpression), new IndexerPropertyExpression()), new TypeWrapperTuple(typeof(CodeArrayIndexerExpression), new ArrayIndexerExpression()), new TypeWrapperTuple(typeof(CodeDirectionExpression), new DirectionExpression()), new TypeWrapperTuple(typeof(CodeTypeReferenceExpression), new TypeReferenceExpression()), new TypeWrapperTuple(typeof(CodeCastExpression), new CastExpression()), new TypeWrapperTuple(typeof(CodeObjectCreateExpression), new ObjectCreateExpression()), new TypeWrapperTuple(typeof(CodeArrayCreateExpression), new ArrayCreateExpression()) };

        public static void AnalyzeUsage(RuleAnalysis analysis, CodeExpression expression, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            if (analysis == null)
            {
                throw new ArgumentNullException("analysis");
            }
            GetExpression(expression).AnalyzeUsage(expression, analysis, isRead, isWritten, qualifier);
        }

        public static CodeExpression Clone(CodeExpression originalExpression)
        {
            if (originalExpression == null)
            {
                return null;
            }
            CodeExpression result = GetExpression(originalExpression).Clone(originalExpression);
            ConditionHelper.CloneUserData(originalExpression, result);
            return result;
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId="0#")]
        public static void Decompile(StringBuilder stringBuilder, CodeExpression expression, CodeExpression parentExpression)
        {
            GetExpression(expression).Decompile(expression, stringBuilder, parentExpression);
        }

        public static RuleExpressionResult Evaluate(RuleExecution execution, CodeExpression expression)
        {
            if (execution == null)
            {
                throw new ArgumentNullException("execution");
            }
            return GetExpression(expression).Evaluate(expression, execution);
        }

        private static RuleExpressionInternal GetExpression(CodeExpression expression)
        {
            Type type = expression.GetType();
            int length = typeWrappers.Length;
            for (int i = 0; i < length; i++)
            {
                TypeWrapperTuple tuple = typeWrappers[i];
                if (type == tuple.codeDomType)
                {
                    return tuple.internalExpression;
                }
            }
            IRuleExpression ruleExpr = expression as IRuleExpression;
            if (ruleExpr != null)
            {
                return new CustomExpressionWrapper(ruleExpr);
            }
            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public static bool Match(CodeExpression firstExpression, CodeExpression secondExpression)
        {
            if ((firstExpression == null) && (secondExpression == null))
            {
                return true;
            }
            if ((firstExpression == null) || (secondExpression == null))
            {
                return false;
            }
            if (firstExpression.GetType() != secondExpression.GetType())
            {
                return false;
            }
            return GetExpression(firstExpression).Match(firstExpression, secondExpression);
        }

        public static RuleExpressionInfo Validate(RuleValidation validation, CodeExpression expression, bool isWritten)
        {
            if (validation == null)
            {
                throw new ArgumentNullException("validation");
            }
            RuleExpressionInfo info = null;
            if (!isWritten)
            {
                info = validation.ExpressionInfo(expression);
            }
            if (info != null)
            {
                return info;
            }
            RuleExpressionInternal ruleExpr = GetExpression(expression);
            if (ruleExpr == null)
            {
                ValidationError item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, new object[] { expression.GetType().FullName }), 0x548);
                item.UserData["ErrorObject"] = expression;
                if (validation.Errors == null)
                {
                    string name = string.Empty;
                    if ((validation.ThisType != null) && (validation.ThisType.Name != null))
                    {
                        name = validation.ThisType.Name;
                    }
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.ErrorsCollectionMissing, new object[] { name }));
                }
                validation.Errors.Add(item);
                return null;
            }
            return validation.ValidateSubexpression(expression, ruleExpr, isWritten);
        }

        private class CustomExpressionWrapper : RuleExpressionInternal
        {
            private IRuleExpression ruleExpr;

            internal CustomExpressionWrapper(IRuleExpression ruleExpr)
            {
                this.ruleExpr = ruleExpr;
            }

            internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
            {
                this.ruleExpr.AnalyzeUsage(analysis, isRead, isWritten, qualifier);
            }

            internal override CodeExpression Clone(CodeExpression expression)
            {
                return this.ruleExpr.Clone();
            }

            internal override void Decompile(CodeExpression expression, StringBuilder decompilation, CodeExpression parentExpression)
            {
                this.ruleExpr.Decompile(decompilation, parentExpression);
            }

            internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
            {
                return this.ruleExpr.Evaluate(execution);
            }

            internal override bool Match(CodeExpression leftExpression, CodeExpression rightExpression)
            {
                return this.ruleExpr.Match(rightExpression);
            }

            internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
            {
                return this.ruleExpr.Validate(validation, isWritten);
            }
        }

        private class TypeWrapperTuple
        {
            internal Type codeDomType;
            internal RuleExpressionInternal internalExpression;

            internal TypeWrapperTuple(Type type, RuleExpressionInternal internalExpression)
            {
                this.codeDomType = type;
                this.internalExpression = internalExpression;
            }
        }
    }
}

