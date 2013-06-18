namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal class Executor
    {
        internal static object AdjustType(Type operandType, object operandValue, Type toType)
        {
            object obj2;
            ValidationError error;
            object obj3;
            if (operandType == toType)
            {
                return operandValue;
            }
            if (AdjustValueStandard(operandType, operandValue, toType, out obj2))
            {
                return obj2;
            }
            MethodInfo info = RuleValidation.FindImplicitConversion(operandType, toType, out error);
            if (info == null)
            {
                if (error != null)
                {
                    throw new RuleEvaluationException(error.ErrorText);
                }
                throw new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.CastIncompatibleTypes, new object[] { RuleDecompiler.DecompileType(operandType), RuleDecompiler.DecompileType(toType) }));
            }
            Type parameterType = info.GetParameters()[0].ParameterType;
            Type returnType = info.ReturnType;
            if (AdjustValueStandard(operandType, operandValue, parameterType, out obj3))
            {
                object obj5;
                object obj4 = info.Invoke(null, new object[] { obj3 });
                if (AdjustValueStandard(returnType, obj4, toType, out obj5))
                {
                    return obj5;
                }
            }
            throw new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.CastIncompatibleTypes, new object[] { RuleDecompiler.DecompileType(operandType), RuleDecompiler.DecompileType(toType) }));
        }

        internal static object AdjustTypeWithCast(Type operandType, object operandValue, Type toType)
        {
            object obj2;
            ValidationError error;
            object obj3;
            if (operandType == toType)
            {
                return operandValue;
            }
            if (AdjustValueStandard(operandType, operandValue, toType, out obj2))
            {
                return obj2;
            }
            MethodInfo info = RuleValidation.FindExplicitConversion(operandType, toType, out error);
            if (info == null)
            {
                if (error != null)
                {
                    throw new RuleEvaluationException(error.ErrorText);
                }
                throw new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.CastIncompatibleTypes, new object[] { RuleDecompiler.DecompileType(operandType), RuleDecompiler.DecompileType(toType) }));
            }
            Type parameterType = info.GetParameters()[0].ParameterType;
            Type returnType = info.ReturnType;
            if (AdjustValueStandard(operandType, operandValue, parameterType, out obj3))
            {
                object obj5;
                object obj4 = info.Invoke(null, new object[] { obj3 });
                if (AdjustValueStandard(returnType, obj4, toType, out obj5))
                {
                    return obj5;
                }
            }
            throw new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.CastIncompatibleTypes, new object[] { RuleDecompiler.DecompileType(operandType), RuleDecompiler.DecompileType(toType) }));
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static bool AdjustValueStandard(Type operandType, object operandValue, Type toType, out object converted)
        {
            converted = operandValue;
            if (operandValue == null)
            {
                ValidationError error;
                if (!toType.IsValueType)
                {
                    return true;
                }
                if (!ConditionHelper.IsNullableValueType(toType))
                {
                    throw new InvalidCastException(string.Format(CultureInfo.CurrentCulture, Messages.CannotCastNullToValueType, new object[] { RuleDecompiler.DecompileType(toType) }));
                }
                converted = Activator.CreateInstance(toType);
                return RuleValidation.StandardImplicitConversion(operandType, toType, null, out error);
            }
            Type c = operandValue.GetType();
            if (c == toType)
            {
                return true;
            }
            if (toType.IsAssignableFrom(c))
            {
                return true;
            }
            if (c.IsValueType && toType.IsValueType)
            {
                if (c.IsEnum)
                {
                    c = Enum.GetUnderlyingType(c);
                    operandValue = ArithmeticLiteral.MakeLiteral(c, operandValue).Value;
                }
                bool flag = ConditionHelper.IsNullableValueType(toType);
                Type enumType = flag ? Nullable.GetUnderlyingType(toType) : toType;
                if (enumType.IsEnum)
                {
                    object obj2;
                    Type underlyingType = Enum.GetUnderlyingType(enumType);
                    if (AdjustValueStandard(c, operandValue, underlyingType, out obj2))
                    {
                        converted = Enum.ToObject(enumType, obj2);
                        if (flag)
                        {
                            converted = Activator.CreateInstance(toType, new object[] { converted });
                        }
                        return true;
                    }
                }
                else if (enumType.IsPrimitive || (enumType == typeof(decimal)))
                {
                    if (c == typeof(char))
                    {
                        char ch = (char) operandValue;
                        if (enumType == typeof(float))
                        {
                            converted = (float) ch;
                        }
                        else if (enumType == typeof(double))
                        {
                            converted = (double) ch;
                        }
                        else if (enumType == typeof(decimal))
                        {
                            converted = ch;
                        }
                        else
                        {
                            converted = ((IConvertible) ch).ToType(enumType, CultureInfo.CurrentCulture);
                        }
                        if (flag)
                        {
                            converted = Activator.CreateInstance(toType, new object[] { converted });
                        }
                        return true;
                    }
                    if (c == typeof(float))
                    {
                        float num = (float) operandValue;
                        if (enumType == typeof(char))
                        {
                            converted = (char) ((ushort) num);
                        }
                        else
                        {
                            converted = ((IConvertible) num).ToType(enumType, CultureInfo.CurrentCulture);
                        }
                        if (flag)
                        {
                            converted = Activator.CreateInstance(toType, new object[] { converted });
                        }
                        return true;
                    }
                    if (c == typeof(double))
                    {
                        double num2 = (double) operandValue;
                        if (enumType == typeof(char))
                        {
                            converted = (char) ((ushort) num2);
                        }
                        else
                        {
                            converted = ((IConvertible) num2).ToType(enumType, CultureInfo.CurrentCulture);
                        }
                        if (flag)
                        {
                            converted = Activator.CreateInstance(toType, new object[] { converted });
                        }
                        return true;
                    }
                    if (c == typeof(decimal))
                    {
                        decimal num3 = (decimal) operandValue;
                        if (enumType == typeof(char))
                        {
                            converted = (char) num3;
                        }
                        else
                        {
                            converted = ((IConvertible) num3).ToType(enumType, CultureInfo.CurrentCulture);
                        }
                        if (flag)
                        {
                            converted = Activator.CreateInstance(toType, new object[] { converted });
                        }
                        return true;
                    }
                    IConvertible convertible = operandValue as IConvertible;
                    if (convertible != null)
                    {
                        try
                        {
                            converted = convertible.ToType(enumType, CultureInfo.CurrentCulture);
                            if (flag)
                            {
                                converted = Activator.CreateInstance(toType, new object[] { converted });
                            }
                            return true;
                        }
                        catch (InvalidCastException)
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        private static RuleSymbolInfo AnalyzeRule(RuleChainingBehavior behavior, Rule rule, RuleValidation validator, Tracer tracer)
        {
            RuleSymbolInfo info = new RuleSymbolInfo();
            if (rule.Condition != null)
            {
                info.conditionDependencies = rule.Condition.GetDependencies(validator);
                if ((info.conditionDependencies != null) && (tracer != null))
                {
                    tracer.TraceConditionSymbols(rule.Name, info.conditionDependencies);
                }
            }
            if (rule.thenActions != null)
            {
                info.thenSideEffects = GetActionSideEffects(behavior, rule.thenActions, validator);
                if ((info.thenSideEffects != null) && (tracer != null))
                {
                    tracer.TraceThenSymbols(rule.Name, info.thenSideEffects);
                }
            }
            if (rule.elseActions != null)
            {
                info.elseSideEffects = GetActionSideEffects(behavior, rule.elseActions, validator);
                if ((info.elseSideEffects != null) && (tracer != null))
                {
                    tracer.TraceElseSymbols(rule.Name, info.elseSideEffects);
                }
            }
            return info;
        }

        private static void AnalyzeRules(RuleChainingBehavior behavior, List<RuleState> ruleStates, RuleValidation validation, Tracer tracer)
        {
            int count = ruleStates.Count;
            if (behavior != RuleChainingBehavior.None)
            {
                RuleSymbolInfo[] ruleSymbols = new RuleSymbolInfo[count];
                int index = 0;
                while (index < count)
                {
                    ruleSymbols[index] = AnalyzeRule(behavior, ruleStates[index].Rule, validation, tracer);
                    index++;
                }
                for (index = 0; index < count; index++)
                {
                    RuleState state = ruleStates[index];
                    if (ruleSymbols[index].thenSideEffects != null)
                    {
                        state.ThenActionsActiveRules = AnalyzeSideEffects(ruleSymbols[index].thenSideEffects, ruleSymbols);
                        if ((state.ThenActionsActiveRules != null) && (tracer != null))
                        {
                            tracer.TraceThenTriggers(state.Rule.Name, state.ThenActionsActiveRules, ruleStates);
                        }
                    }
                    if (ruleSymbols[index].elseSideEffects != null)
                    {
                        state.ElseActionsActiveRules = AnalyzeSideEffects(ruleSymbols[index].elseSideEffects, ruleSymbols);
                        if ((state.ElseActionsActiveRules != null) && (tracer != null))
                        {
                            tracer.TraceElseTriggers(state.Rule.Name, state.ElseActionsActiveRules, ruleStates);
                        }
                    }
                }
            }
        }

        private static ICollection<int> AnalyzeSideEffects(ICollection<string> sideEffects, RuleSymbolInfo[] ruleSymbols)
        {
            Dictionary<int, object> dictionary = new Dictionary<int, object>();
            for (int i = 0; i < ruleSymbols.Length; i++)
            {
                ICollection<string> conditionDependencies = ruleSymbols[i].conditionDependencies;
                if (conditionDependencies != null)
                {
                    foreach (string str in sideEffects)
                    {
                        bool flag = false;
                        if (str.EndsWith("*", StringComparison.Ordinal))
                        {
                            foreach (string str2 in conditionDependencies)
                            {
                                if (str2.EndsWith("*", StringComparison.Ordinal))
                                {
                                    string str5;
                                    string str6;
                                    string str3 = str2.Substring(0, str2.Length - 2);
                                    string str4 = str.Substring(0, str.Length - 1);
                                    if (str3.Length < str4.Length)
                                    {
                                        str5 = str3;
                                        str6 = str4;
                                    }
                                    else
                                    {
                                        str5 = str4;
                                        str6 = str3;
                                    }
                                    if (!str6.StartsWith(str5, StringComparison.Ordinal))
                                    {
                                        continue;
                                    }
                                    flag = true;
                                    break;
                                }
                                string str7 = str.Substring(0, str.Length - 1);
                                string str8 = str2;
                                if (str8.EndsWith("/", StringComparison.Ordinal))
                                {
                                    str8 = str8.Substring(0, str8.Length - 1);
                                }
                                if (str8.StartsWith(str7, StringComparison.Ordinal))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (string str9 in conditionDependencies)
                            {
                                if (str9.EndsWith("*", StringComparison.Ordinal))
                                {
                                    string str11;
                                    string str12;
                                    string str10 = str9.Substring(0, str9.Length - 2);
                                    if (str10.Length < str.Length)
                                    {
                                        str11 = str10;
                                        str12 = str;
                                    }
                                    else
                                    {
                                        str11 = str;
                                        str12 = str10;
                                    }
                                    if (!str12.StartsWith(str11, StringComparison.Ordinal))
                                    {
                                        continue;
                                    }
                                    flag = true;
                                    break;
                                }
                                if (str9.StartsWith(str, StringComparison.Ordinal))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            dictionary[i] = null;
                            break;
                        }
                    }
                }
            }
            return dictionary.Keys;
        }

        internal static bool EvaluateBool(CodeExpression expression, RuleExecution context)
        {
            object operandValue = RuleExpressionWalker.Evaluate(context, expression).Value;
            if (operandValue is bool)
            {
                return (bool) operandValue;
            }
            Type expressionType = context.Validation.ExpressionInfo(expression).ExpressionType;
            if (expressionType == null)
            {
                InvalidOperationException exception = new InvalidOperationException(Messages.ConditionMustBeBoolean);
                exception.Data["ErrorObject"] = expression;
                throw exception;
            }
            return (bool) AdjustType(expressionType, operandValue, typeof(bool));
        }

        internal static void ExecuteRuleSet(IList<RuleState> orderedRules, RuleExecution ruleExecution, Tracer tracer, string trackingKey)
        {
            long[] numArray = new long[orderedRules.Count];
            bool[] flagArray = new bool[orderedRules.Count];
            ruleExecution.Halted = false;
            ActivityExecutionContext activityExecutionContext = ruleExecution.ActivityExecutionContext;
            int index = 0;
            while (index < orderedRules.Count)
            {
                RuleState state = orderedRules[index];
                if (!flagArray[index])
                {
                    if (tracer != null)
                    {
                        tracer.StartRule(state.Rule.Name);
                    }
                    flagArray[index] = true;
                    bool result = state.Rule.Condition.Evaluate(ruleExecution);
                    if (tracer != null)
                    {
                        tracer.RuleResult(state.Rule.Name, result);
                    }
                    if ((activityExecutionContext != null) && (state.Rule.Name != null))
                    {
                        activityExecutionContext.TrackData(trackingKey, new RuleActionTrackingEvent(state.Rule.Name, result));
                    }
                    ICollection<RuleAction> is2 = result ? state.Rule.thenActions : state.Rule.elseActions;
                    ICollection<int> is3 = result ? state.ThenActionsActiveRules : state.ElseActionsActiveRules;
                    if ((is2 != null) && (is2.Count > 0))
                    {
                        numArray[index] += 1L;
                        string name = state.Rule.Name;
                        if (tracer != null)
                        {
                            tracer.StartActions(name, result);
                        }
                        foreach (RuleAction action in is2)
                        {
                            action.Execute(ruleExecution);
                            if (ruleExecution.Halted)
                            {
                                break;
                            }
                        }
                        if (ruleExecution.Halted)
                        {
                            return;
                        }
                        if (is3 != null)
                        {
                            foreach (int num2 in is3)
                            {
                                RuleState state2 = orderedRules[num2];
                                if (flagArray[num2] && ((numArray[num2] == 0L) || (state2.Rule.ReevaluationBehavior == RuleReevaluationBehavior.Always)))
                                {
                                    if (tracer != null)
                                    {
                                        tracer.TraceUpdate(name, state2.Rule.Name);
                                    }
                                    flagArray[num2] = false;
                                    if (num2 < index)
                                    {
                                        index = num2;
                                    }
                                }
                            }
                        }
                        continue;
                    }
                }
                index++;
            }
        }

        private static ICollection<string> GetActionSideEffects(RuleChainingBehavior behavior, IList<RuleAction> actions, RuleValidation validation)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (RuleAction action in actions)
            {
                if ((behavior == RuleChainingBehavior.Full) || ((behavior == RuleChainingBehavior.UpdateOnly) && (action is RuleUpdateAction)))
                {
                    ICollection<string> sideEffects = action.GetSideEffects(validation);
                    if (sideEffects != null)
                    {
                        foreach (string str in sideEffects)
                        {
                            dictionary[str] = null;
                        }
                    }
                }
            }
            return dictionary.Keys;
        }

        internal static IList<RuleState> Preprocess(RuleChainingBehavior behavior, ICollection<Rule> rules, RuleValidation validation, Tracer tracer)
        {
            List<RuleState> ruleStates = new List<RuleState>(rules.Count);
            foreach (Rule rule in rules)
            {
                if (rule.Active)
                {
                    ruleStates.Add(new RuleState(rule));
                }
            }
            ruleStates.Sort();
            AnalyzeRules(behavior, ruleStates, validation, tracer);
            return ruleStates;
        }

        private class RuleSymbolInfo
        {
            internal ICollection<string> conditionDependencies;
            internal ICollection<string> elseSideEffects;
            internal ICollection<string> thenSideEffects;
        }
    }
}

