namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class ArrayIndexerExpression : RuleExpressionInternal
    {
        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeArrayIndexerExpression expression2 = (CodeArrayIndexerExpression) expression;
            RuleExpressionWalker.AnalyzeUsage(analysis, expression2.TargetObject, isRead, isWritten, qualifier);
            for (int i = 0; i < expression2.Indices.Count; i++)
            {
                RuleExpressionWalker.AnalyzeUsage(analysis, expression2.Indices[i], true, false, null);
            }
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeArrayIndexerExpression expression2 = (CodeArrayIndexerExpression) expression;
            CodeExpression targetObject = RuleExpressionWalker.Clone(expression2.TargetObject);
            CodeExpression[] indices = new CodeExpression[expression2.Indices.Count];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = RuleExpressionWalker.Clone(expression2.Indices[i]);
            }
            return new CodeArrayIndexerExpression(targetObject, indices);
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeArrayIndexerExpression expression2 = (CodeArrayIndexerExpression) expression;
            CodeExpression targetObject = expression2.TargetObject;
            if (targetObject == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.NullIndexerTarget, new object[0]));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            if ((expression2.Indices == null) || (expression2.Indices.Count == 0))
            {
                RuleEvaluationException exception2 = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.MissingIndexExpressions, new object[0]));
                exception2.Data["ErrorObject"] = expression2;
                throw exception2;
            }
            RuleExpressionWalker.Decompile(stringBuilder, targetObject, expression2);
            stringBuilder.Append('[');
            RuleExpressionWalker.Decompile(stringBuilder, expression2.Indices[0], null);
            for (int i = 1; i < expression2.Indices.Count; i++)
            {
                stringBuilder.Append(", ");
                RuleExpressionWalker.Decompile(stringBuilder, expression2.Indices[i], null);
            }
            stringBuilder.Append(']');
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodeArrayIndexerExpression expression2 = (CodeArrayIndexerExpression) expression;
            object obj2 = RuleExpressionWalker.Evaluate(execution, expression2.TargetObject).Value;
            if (obj2 == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullIndexer, new object[0]));
                exception.Data["ErrorObject"] = expression2;
                throw exception;
            }
            int count = expression2.Indices.Count;
            long[] indexerArguments = new long[count];
            for (int i = 0; i < count; i++)
            {
                Type expressionType = execution.Validation.ExpressionInfo(expression2.Indices[i]).ExpressionType;
                object operandValue = RuleExpressionWalker.Evaluate(execution, expression2.Indices[i]).Value;
                indexerArguments[i] = (long) Executor.AdjustType(expressionType, operandValue, typeof(long));
            }
            return new RuleArrayElementResult((Array) obj2, indexerArguments);
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeArrayIndexerExpression expression2 = (CodeArrayIndexerExpression) expression;
            CodeArrayIndexerExpression expression3 = (CodeArrayIndexerExpression) comperand;
            if (!RuleExpressionWalker.Match(expression2.TargetObject, expression3.TargetObject))
            {
                return false;
            }
            if (expression2.Indices.Count != expression3.Indices.Count)
            {
                return false;
            }
            for (int i = 0; i < expression2.Indices.Count; i++)
            {
                if (!RuleExpressionWalker.Match(expression2.Indices[i], expression3.Indices[i]))
                {
                    return false;
                }
            }
            return true;
        }

        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            ValidationError item = null;
            Type expressionType = null;
            CodeArrayIndexerExpression newParent = (CodeArrayIndexerExpression) expression;
            CodeExpression targetObject = newParent.TargetObject;
            if (targetObject == null)
            {
                item = new ValidationError(Messages.NullIndexerTarget, 0x53d);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
                return null;
            }
            if (targetObject is CodeTypeReferenceExpression)
            {
                item = new ValidationError(Messages.IndexersCannotBeStatic, 0x53d);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
                return null;
            }
            if ((newParent.Indices == null) || (newParent.Indices.Count == 0))
            {
                item = new ValidationError(Messages.MissingIndexExpressions, 0x53d);
                item.UserData["ErrorObject"] = newParent;
                validation.Errors.Add(item);
                return null;
            }
            try
            {
                if (!validation.PushParentExpression(newParent))
                {
                    return null;
                }
                RuleExpressionInfo info = RuleExpressionWalker.Validate(validation, newParent.TargetObject, false);
                if (info == null)
                {
                    return null;
                }
                expressionType = info.ExpressionType;
                if (expressionType == null)
                {
                    return null;
                }
                if (expressionType == typeof(NullLiteral))
                {
                    item = new ValidationError(Messages.NullIndexerTarget, 0x53d);
                    item.UserData["ErrorObject"] = newParent;
                    validation.Errors.Add(item);
                    return null;
                }
                if (!expressionType.IsArray)
                {
                    item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CannotIndexType, new object[] { RuleDecompiler.DecompileType(expressionType) }), 0x19b);
                    item.UserData["ErrorObject"] = newParent;
                    validation.Errors.Add(item);
                    return null;
                }
                int arrayRank = expressionType.GetArrayRank();
                if (newParent.Indices.Count != arrayRank)
                {
                    item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.ArrayIndexBadRank, new object[] { arrayRank }), 0x19c);
                    item.UserData["ErrorObject"] = newParent;
                    validation.Errors.Add(item);
                    return null;
                }
                bool flag = false;
                for (int i = 0; i < newParent.Indices.Count; i++)
                {
                    CodeExpression expression4 = newParent.Indices[i];
                    if (expression4 == null)
                    {
                        item = new ValidationError(Messages.NullIndexExpression, 0x53d);
                        item.UserData["ErrorObject"] = newParent;
                        validation.Errors.Add(item);
                        flag = true;
                        continue;
                    }
                    if (expression4 is CodeDirectionExpression)
                    {
                        item = new ValidationError(Messages.IndexerArgCannotBeRefOrOut, 0x19d);
                        item.UserData["ErrorObject"] = expression4;
                        validation.Errors.Add(item);
                        flag = true;
                    }
                    if (expression4 is CodeTypeReferenceExpression)
                    {
                        item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, new object[] { expression4.GetType().FullName }), 0x548);
                        item.UserData["ErrorObject"] = expression4;
                        validation.AddError(item);
                        flag = true;
                    }
                    RuleExpressionInfo info2 = RuleExpressionWalker.Validate(validation, expression4, false);
                    if (info2 != null)
                    {
                        Type type = info2.ExpressionType;
                        switch (Type.GetTypeCode(type))
                        {
                            case TypeCode.Char:
                            case TypeCode.SByte:
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            {
                                continue;
                            }
                        }
                        item = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.ArrayIndexBadType, new object[] { RuleDecompiler.DecompileType(type) }), 0x19e);
                        item.UserData["ErrorObject"] = expression4;
                        validation.Errors.Add(item);
                        flag = true;
                        continue;
                    }
                    flag = true;
                }
                if (flag)
                {
                    return null;
                }
            }
            finally
            {
                validation.PopParentExpression();
            }
            return new RuleExpressionInfo(expressionType.GetElementType());
        }
    }
}

