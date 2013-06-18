namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel.Compiler;

    internal abstract class Literal
    {
        protected internal Type m_type;
        internal static MethodInfo ObjectEquality = typeof(DefaultOperators).GetMethod("ObjectEquality");
        private static Dictionary<Type, TypeFlags> supportedTypes = CreateTypesDictionary();
        private static Dictionary<Type, LiteralMaker> types = CreateMakersDictionary();

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected Literal()
        {
        }

        private static void AddLiftedOperators(Type type, string methodName, OperatorGrouping group, Type arg1, Type arg2, List<MethodInfo> candidates)
        {
            int num = 0;
            foreach (MethodInfo info in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                ParameterInfo[] parameters = info.GetParameters();
                if ((info.Name == methodName) && (parameters.Length == 2))
                {
                    MethodInfo item = EvaluateLiftedMethod(info, parameters, group, arg1, arg2);
                    if (item != null)
                    {
                        num++;
                        if (!candidates.Contains(item))
                        {
                            candidates.Add(item);
                        }
                    }
                }
            }
            if ((num <= 0) && (type != typeof(object)))
            {
                type = type.BaseType;
                if (type != null)
                {
                    foreach (MethodInfo info3 in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        ParameterInfo[] infoArray3 = info3.GetParameters();
                        if ((info3.Name == methodName) && (infoArray3.Length == 2))
                        {
                            MethodInfo info4 = EvaluateLiftedMethod(info3, infoArray3, group, arg1, arg2);
                            if ((info4 != null) && !candidates.Contains(info4))
                            {
                                candidates.Add(info4);
                            }
                        }
                    }
                }
            }
        }

        private static void AddOperatorOverloads(Type type, string methodName, Type arg1, Type arg2, List<MethodInfo> candidates)
        {
            int num = 0;
            foreach (MethodInfo info in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                ParameterInfo[] parameters = info.GetParameters();
                if (((info.Name == methodName) && (parameters.Length == 2)) && EvaluateMethod(parameters, arg1, arg2))
                {
                    num++;
                    if (!candidates.Contains(info))
                    {
                        candidates.Add(info);
                    }
                }
            }
            if ((num <= 0) && (type != typeof(object)))
            {
                type = type.BaseType;
                if (type != null)
                {
                    foreach (MethodInfo info2 in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        ParameterInfo[] infoArray3 = info2.GetParameters();
                        if (((info2.Name == methodName) && (infoArray3.Length == 2)) && (EvaluateMethod(infoArray3, arg1, arg2) && !candidates.Contains(info2)))
                        {
                            candidates.Add(info2);
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static RuleBinaryExpressionInfo AllowedComparison(Type lhs, CodeExpression lhsExpression, Type rhs, CodeExpression rhsExpression, CodeBinaryOperatorType comparison, RuleValidation validator, out ValidationError error)
        {
            TypeFlags flags;
            TypeFlags flags2;
            if (!supportedTypes.TryGetValue(lhs, out flags) || !supportedTypes.TryGetValue(rhs, out flags2))
            {
                MethodInfo mi = MapOperatorToMethod(comparison, lhs, lhsExpression, rhs, rhsExpression, validator, out error);
                if (mi != null)
                {
                    return new RuleBinaryExpressionInfo(lhs, rhs, mi);
                }
                return null;
            }
            if (flags == flags2)
            {
                if ((flags == TypeFlags.Bool) && (comparison != CodeBinaryOperatorType.ValueEquality))
                {
                    string str = string.Format(CultureInfo.CurrentCulture, Messages.RelationalOpBadTypes, new object[] { comparison.ToString(), RuleDecompiler.DecompileType(lhs), RuleDecompiler.DecompileType(rhs) });
                    error = new ValidationError(str, 0x545);
                    return null;
                }
                error = null;
                return new RuleBinaryExpressionInfo(lhs, rhs, typeof(bool));
            }
            switch ((flags | flags2))
            {
                case (TypeFlags.ULong | TypeFlags.UnsignedNumbers):
                case (TypeFlags.Float | TypeFlags.SignedNumbers):
                case (TypeFlags.Float | TypeFlags.UnsignedNumbers):
                case (TypeFlags.Float | TypeFlags.ULong):
                case (TypeFlags.UnsignedNumbers | TypeFlags.SignedNumbers):
                case (TypeFlags.Decimal | TypeFlags.SignedNumbers):
                case (TypeFlags.Decimal | TypeFlags.UnsignedNumbers):
                case (TypeFlags.Decimal | TypeFlags.ULong):
                    error = null;
                    return new RuleBinaryExpressionInfo(lhs, rhs, typeof(bool));
            }
            string errorText = string.Format(CultureInfo.CurrentCulture, Messages.RelationalOpBadTypes, new object[] { comparison.ToString(), (lhs == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(lhs), (rhs == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(rhs) });
            error = new ValidationError(errorText, 0x545);
            return null;
        }

        private static Dictionary<Type, LiteralMaker> CreateMakersDictionary()
        {
            Dictionary<Type, LiteralMaker> dictionary = new Dictionary<Type, LiteralMaker>(0x20);
            dictionary.Add(typeof(byte), new LiteralMaker(Literal.MakeByte));
            dictionary.Add(typeof(sbyte), new LiteralMaker(Literal.MakeSByte));
            dictionary.Add(typeof(short), new LiteralMaker(Literal.MakeShort));
            dictionary.Add(typeof(int), new LiteralMaker(Literal.MakeInt));
            dictionary.Add(typeof(long), new LiteralMaker(Literal.MakeLong));
            dictionary.Add(typeof(ushort), new LiteralMaker(Literal.MakeUShort));
            dictionary.Add(typeof(uint), new LiteralMaker(Literal.MakeUInt));
            dictionary.Add(typeof(ulong), new LiteralMaker(Literal.MakeULong));
            dictionary.Add(typeof(float), new LiteralMaker(Literal.MakeFloat));
            dictionary.Add(typeof(double), new LiteralMaker(Literal.MakeDouble));
            dictionary.Add(typeof(char), new LiteralMaker(Literal.MakeChar));
            dictionary.Add(typeof(string), new LiteralMaker(Literal.MakeString));
            dictionary.Add(typeof(decimal), new LiteralMaker(Literal.MakeDecimal));
            dictionary.Add(typeof(bool), new LiteralMaker(Literal.MakeBool));
            dictionary.Add(typeof(byte?), new LiteralMaker(Literal.MakeByte));
            dictionary.Add(typeof(sbyte?), new LiteralMaker(Literal.MakeSByte));
            dictionary.Add(typeof(short?), new LiteralMaker(Literal.MakeShort));
            dictionary.Add(typeof(int?), new LiteralMaker(Literal.MakeInt));
            dictionary.Add(typeof(long?), new LiteralMaker(Literal.MakeLong));
            dictionary.Add(typeof(ushort?), new LiteralMaker(Literal.MakeUShort));
            dictionary.Add(typeof(uint?), new LiteralMaker(Literal.MakeUInt));
            dictionary.Add(typeof(ulong?), new LiteralMaker(Literal.MakeULong));
            dictionary.Add(typeof(float?), new LiteralMaker(Literal.MakeFloat));
            dictionary.Add(typeof(double?), new LiteralMaker(Literal.MakeDouble));
            dictionary.Add(typeof(char?), new LiteralMaker(Literal.MakeChar));
            dictionary.Add(typeof(decimal?), new LiteralMaker(Literal.MakeDecimal));
            dictionary.Add(typeof(bool?), new LiteralMaker(Literal.MakeBool));
            return dictionary;
        }

        private static Dictionary<Type, TypeFlags> CreateTypesDictionary()
        {
            Dictionary<Type, TypeFlags> dictionary = new Dictionary<Type, TypeFlags>(0x20);
            dictionary.Add(typeof(byte), TypeFlags.UnsignedNumbers);
            dictionary.Add(typeof(byte?), TypeFlags.UnsignedNumbers);
            dictionary.Add(typeof(sbyte), TypeFlags.SignedNumbers);
            dictionary.Add(typeof(sbyte?), TypeFlags.SignedNumbers);
            dictionary.Add(typeof(short), TypeFlags.SignedNumbers);
            dictionary.Add(typeof(short?), TypeFlags.SignedNumbers);
            dictionary.Add(typeof(int), TypeFlags.SignedNumbers);
            dictionary.Add(typeof(int?), TypeFlags.SignedNumbers);
            dictionary.Add(typeof(long), TypeFlags.SignedNumbers);
            dictionary.Add(typeof(long?), TypeFlags.SignedNumbers);
            dictionary.Add(typeof(ushort), TypeFlags.UnsignedNumbers);
            dictionary.Add(typeof(ushort?), TypeFlags.UnsignedNumbers);
            dictionary.Add(typeof(uint), TypeFlags.UnsignedNumbers);
            dictionary.Add(typeof(uint?), TypeFlags.UnsignedNumbers);
            dictionary.Add(typeof(ulong), TypeFlags.ULong);
            dictionary.Add(typeof(ulong?), TypeFlags.ULong);
            dictionary.Add(typeof(float), TypeFlags.Float);
            dictionary.Add(typeof(float?), TypeFlags.Float);
            dictionary.Add(typeof(double), TypeFlags.Float);
            dictionary.Add(typeof(double?), TypeFlags.Float);
            dictionary.Add(typeof(char), TypeFlags.UnsignedNumbers);
            dictionary.Add(typeof(char?), TypeFlags.UnsignedNumbers);
            dictionary.Add(typeof(string), TypeFlags.String);
            dictionary.Add(typeof(decimal), TypeFlags.Decimal);
            dictionary.Add(typeof(decimal?), TypeFlags.Decimal);
            dictionary.Add(typeof(bool), TypeFlags.Bool);
            dictionary.Add(typeof(bool?), TypeFlags.Bool);
            return dictionary;
        }

        private static bool DecimalIntegerLiteralZero(Type type, CodePrimitiveExpression expression)
        {
            if (expression != null)
            {
                if (type == typeof(int))
                {
                    return expression.Value.Equals(0);
                }
                if (type == typeof(uint))
                {
                    return expression.Value.Equals(0);
                }
                if (type == typeof(long))
                {
                    return expression.Value.Equals(0L);
                }
                if (type == typeof(ulong))
                {
                    return expression.Value.Equals((ulong) 0L);
                }
            }
            return false;
        }

        internal virtual bool Equal(bool literalValue)
        {
            return false;
        }

        internal virtual bool Equal(byte literalValue)
        {
            return false;
        }

        internal virtual bool Equal(char literalValue)
        {
            return false;
        }

        internal virtual bool Equal(decimal literalValue)
        {
            return false;
        }

        internal virtual bool Equal(double literalValue)
        {
            return false;
        }

        internal virtual bool Equal(short literalValue)
        {
            return false;
        }

        internal virtual bool Equal(int literalValue)
        {
            return false;
        }

        internal virtual bool Equal(long literalValue)
        {
            return false;
        }

        internal virtual bool Equal(sbyte literalValue)
        {
            return false;
        }

        internal virtual bool Equal(float literalValue)
        {
            return false;
        }

        internal virtual bool Equal(string literalValue)
        {
            return false;
        }

        internal virtual bool Equal(ushort literalValue)
        {
            return false;
        }

        internal virtual bool Equal(uint literalValue)
        {
            return false;
        }

        internal virtual bool Equal(ulong literalValue)
        {
            return false;
        }

        internal abstract bool Equal(Literal rhs);
        private static MethodInfo EvaluateLiftedMethod(MethodInfo mi, ParameterInfo[] parameters, OperatorGrouping group, Type arg1, Type arg2)
        {
            Type parameterType = parameters[0].ParameterType;
            Type type = parameters[1].ParameterType;
            if (ConditionHelper.IsNonNullableValueType(parameterType) && ConditionHelper.IsNonNullableValueType(type))
            {
                parameterType = typeof(Nullable<>).MakeGenericType(new Type[] { parameterType });
                type = typeof(Nullable<>).MakeGenericType(new Type[] { type });
                switch (group)
                {
                    case OperatorGrouping.Arithmetic:
                        if ((!ConditionHelper.IsNonNullableValueType(mi.ReturnType) || !RuleValidation.ImplicitConversion(arg1, parameterType)) || !RuleValidation.ImplicitConversion(arg2, type))
                        {
                            break;
                        }
                        return new LiftedArithmeticOperatorMethodInfo(mi);

                    case OperatorGrouping.Equality:
                        if ((!(mi.ReturnType == typeof(bool)) || !RuleValidation.ImplicitConversion(arg1, parameterType)) || !RuleValidation.ImplicitConversion(arg2, type))
                        {
                            break;
                        }
                        return new LiftedEqualityOperatorMethodInfo(mi);

                    case OperatorGrouping.Relational:
                        if ((!(mi.ReturnType == typeof(bool)) || !RuleValidation.ImplicitConversion(arg1, parameterType)) || !RuleValidation.ImplicitConversion(arg2, type))
                        {
                            break;
                        }
                        return new LiftedRelationalOperatorMethodInfo(mi);
                }
            }
            return null;
        }

        private static bool EvaluateMethod(ParameterInfo[] parameters, Type arg1, Type arg2)
        {
            Type parameterType = parameters[0].ParameterType;
            Type toType = parameters[1].ParameterType;
            return (RuleValidation.ImplicitConversion(arg1, parameterType) && RuleValidation.ImplicitConversion(arg2, toType));
        }

        internal virtual bool GreaterThan()
        {
            return false;
        }

        internal virtual bool GreaterThan(bool literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(byte literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(char literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(decimal literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(double literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(short literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(int literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(long literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(sbyte literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(float literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(string literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(ushort literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(uint literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal virtual bool GreaterThan(ulong literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThan, this.m_type);
        }

        internal abstract bool GreaterThan(Literal rhs);
        internal virtual bool GreaterThanOrEqual()
        {
            return false;
        }

        internal virtual bool GreaterThanOrEqual(bool literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(byte literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(char literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(decimal literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(double literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(short literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(int literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(long literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(sbyte literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(float literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(string literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(ushort literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(uint literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal virtual bool GreaterThanOrEqual(ulong literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.GreaterThanOrEqual, this.m_type);
        }

        internal abstract bool GreaterThanOrEqual(Literal rhs);
        internal virtual bool LessThan()
        {
            return false;
        }

        internal virtual bool LessThan(bool literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(byte literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(char literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(decimal literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(double literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(short literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(int literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(long literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(sbyte literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(float literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(string literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(ushort literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(uint literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal virtual bool LessThan(ulong literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThan, this.m_type);
        }

        internal abstract bool LessThan(Literal rhs);
        internal virtual bool LessThanOrEqual()
        {
            return false;
        }

        internal virtual bool LessThanOrEqual(bool literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(byte literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(char literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(decimal literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(double literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(short literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(int literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(long literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(sbyte literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(float literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(string literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(ushort literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(uint literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal virtual bool LessThanOrEqual(ulong literalValue)
        {
            throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleComparisonTypes, new object[] { literalValue.GetType(), this.m_type }), literalValue.GetType(), CodeBinaryOperatorType.LessThanOrEqual, this.m_type);
        }

        internal abstract bool LessThanOrEqual(Literal rhs);
        private static Literal MakeBool(object literalValue)
        {
            return new BoolLiteral((bool) literalValue);
        }

        private static Literal MakeByte(object literalValue)
        {
            return new ByteLiteral((byte) literalValue);
        }

        private static Literal MakeChar(object literalValue)
        {
            return new CharLiteral((char) literalValue);
        }

        private static Literal MakeDecimal(object literalValue)
        {
            return new DecimalLiteral((decimal) literalValue);
        }

        private static Literal MakeDouble(object literalValue)
        {
            return new DoubleLiteral((double) literalValue);
        }

        private static Literal MakeFloat(object literalValue)
        {
            return new FloatLiteral((float) literalValue);
        }

        private static Literal MakeInt(object literalValue)
        {
            return new IntLiteral((int) literalValue);
        }

        internal static Literal MakeLiteral(Type literalType, object literalValue)
        {
            LiteralMaker maker;
            if (literalValue == null)
            {
                return new NullLiteral(literalType);
            }
            if (types.TryGetValue(literalType, out maker))
            {
                try
                {
                    return maker(literalValue);
                }
                catch (InvalidCastException exception)
                {
                    throw new RuleEvaluationIncompatibleTypesException(string.Format(CultureInfo.CurrentCulture, Messages.InvalidCast, new object[] { RuleDecompiler.DecompileType(literalValue.GetType()), RuleDecompiler.DecompileType(literalType) }), literalType, CodeBinaryOperatorType.Assign, literalValue.GetType(), exception);
                }
            }
            return null;
        }

        private static Literal MakeLong(object literalValue)
        {
            return new LongLiteral((long) literalValue);
        }

        private static Literal MakeSByte(object literalValue)
        {
            return new SByteLiteral((sbyte) literalValue);
        }

        private static Literal MakeShort(object literalValue)
        {
            return new ShortLiteral((short) literalValue);
        }

        private static Literal MakeString(object literalValue)
        {
            return new StringLiteral((string) literalValue);
        }

        private static Literal MakeUInt(object literalValue)
        {
            return new UIntLiteral((uint) literalValue);
        }

        private static Literal MakeULong(object literalValue)
        {
            return new ULongLiteral((ulong) literalValue);
        }

        private static Literal MakeUShort(object literalValue)
        {
            return new UShortLiteral((ushort) literalValue);
        }

        [SuppressMessage("Microsoft.Performance", "CA1803:AvoidCostlyCallsWherePossible"), SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal static MethodInfo MapOperatorToMethod(CodeBinaryOperatorType op, Type lhs, CodeExpression lhsExpression, Type rhs, CodeExpression rhsExpression, RuleValidation validator, out ValidationError error)
        {
            string str;
            string str2;
            OperatorGrouping arithmetic;
            switch (op)
            {
                case CodeBinaryOperatorType.Add:
                    str = "op_Addition";
                    arithmetic = OperatorGrouping.Arithmetic;
                    break;

                case CodeBinaryOperatorType.Subtract:
                    str = "op_Subtraction";
                    arithmetic = OperatorGrouping.Arithmetic;
                    break;

                case CodeBinaryOperatorType.Multiply:
                    str = "op_Multiply";
                    arithmetic = OperatorGrouping.Arithmetic;
                    break;

                case CodeBinaryOperatorType.Divide:
                    str = "op_Division";
                    arithmetic = OperatorGrouping.Arithmetic;
                    break;

                case CodeBinaryOperatorType.Modulus:
                    str = "op_Modulus";
                    arithmetic = OperatorGrouping.Arithmetic;
                    break;

                case CodeBinaryOperatorType.ValueEquality:
                    str = "op_Equality";
                    arithmetic = OperatorGrouping.Equality;
                    break;

                case CodeBinaryOperatorType.BitwiseOr:
                    str = "op_BitwiseOr";
                    arithmetic = OperatorGrouping.Arithmetic;
                    break;

                case CodeBinaryOperatorType.BitwiseAnd:
                    str = "op_BitwiseAnd";
                    arithmetic = OperatorGrouping.Arithmetic;
                    break;

                case CodeBinaryOperatorType.LessThan:
                    str = "op_LessThan";
                    arithmetic = OperatorGrouping.Relational;
                    break;

                case CodeBinaryOperatorType.LessThanOrEqual:
                    str = "op_LessThanOrEqual";
                    arithmetic = OperatorGrouping.Relational;
                    break;

                case CodeBinaryOperatorType.GreaterThan:
                    str = "op_GreaterThan";
                    arithmetic = OperatorGrouping.Relational;
                    break;

                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    str = "op_GreaterThanOrEqual";
                    arithmetic = OperatorGrouping.Relational;
                    break;

                default:
                    str2 = string.Format(CultureInfo.CurrentCulture, Messages.BinaryOpNotSupported, new object[] { op.ToString() });
                    error = new ValidationError(str2, 0x548);
                    return null;
            }
            List<MethodInfo> candidates = new List<MethodInfo>();
            bool flag = ConditionHelper.IsNullableValueType(lhs);
            bool flag2 = ConditionHelper.IsNullableValueType(rhs);
            Type rhsType = flag ? Nullable.GetUnderlyingType(lhs) : lhs;
            Type type = flag2 ? Nullable.GetUnderlyingType(rhs) : rhs;
            if (!rhsType.IsEnum)
            {
                if (type.IsEnum)
                {
                    Type underlyingType;
                    switch (op)
                    {
                        case CodeBinaryOperatorType.Add:
                            underlyingType = EnumHelper.GetUnderlyingType(type);
                            if ((underlyingType == null) || !RuleValidation.TypesAreAssignable(rhsType, underlyingType, lhsExpression, out error))
                            {
                                break;
                            }
                            error = null;
                            return new EnumOperationMethodInfo(lhs, op, rhs, false);

                        case CodeBinaryOperatorType.Subtract:
                        {
                            underlyingType = EnumHelper.GetUnderlyingType(type);
                            if (underlyingType == null)
                            {
                                break;
                            }
                            CodePrimitiveExpression expression = lhsExpression as CodePrimitiveExpression;
                            if (!DecimalIntegerLiteralZero(lhs, expression))
                            {
                                if (!RuleValidation.TypesAreAssignable(rhsType, underlyingType, lhsExpression, out error))
                                {
                                    break;
                                }
                                error = null;
                                return new EnumOperationMethodInfo(lhs, op, rhs, false);
                            }
                            error = null;
                            return new EnumOperationMethodInfo(lhs, op, rhs, true);
                        }
                        case CodeBinaryOperatorType.ValueEquality:
                        case CodeBinaryOperatorType.LessThan:
                        case CodeBinaryOperatorType.LessThanOrEqual:
                        case CodeBinaryOperatorType.GreaterThan:
                        case CodeBinaryOperatorType.GreaterThanOrEqual:
                            if (!flag2 || !(lhs == typeof(NullLiteral)))
                            {
                                if (DecimalIntegerLiteralZero(lhs, lhsExpression as CodePrimitiveExpression))
                                {
                                    error = null;
                                    return new EnumOperationMethodInfo(lhs, op, rhs, true);
                                }
                                break;
                            }
                            error = null;
                            return new EnumOperationMethodInfo(rhs, op, rhs, false);
                    }
                }
            }
            else
            {
                Type type3;
                switch (op)
                {
                    case CodeBinaryOperatorType.Add:
                        type3 = EnumHelper.GetUnderlyingType(rhsType);
                        if ((type3 == null) || !RuleValidation.TypesAreAssignable(type, type3, rhsExpression, out error))
                        {
                            break;
                        }
                        error = null;
                        return new EnumOperationMethodInfo(lhs, op, rhs, false);

                    case CodeBinaryOperatorType.Subtract:
                        type3 = EnumHelper.GetUnderlyingType(rhsType);
                        if (type3 == null)
                        {
                            break;
                        }
                        if (!(rhsType == type))
                        {
                            if (DecimalIntegerLiteralZero(rhs, rhsExpression as CodePrimitiveExpression))
                            {
                                error = null;
                                return new EnumOperationMethodInfo(lhs, op, rhs, true);
                            }
                            if (!RuleValidation.TypesAreAssignable(type, type3, rhsExpression, out error))
                            {
                                break;
                            }
                            error = null;
                            return new EnumOperationMethodInfo(lhs, op, rhs, false);
                        }
                        error = null;
                        return new EnumOperationMethodInfo(lhs, op, rhs, false);

                    case CodeBinaryOperatorType.ValueEquality:
                    case CodeBinaryOperatorType.LessThan:
                    case CodeBinaryOperatorType.LessThanOrEqual:
                    case CodeBinaryOperatorType.GreaterThan:
                    case CodeBinaryOperatorType.GreaterThanOrEqual:
                        if (!(rhsType == type))
                        {
                            if (flag && (rhs == typeof(NullLiteral)))
                            {
                                error = null;
                                return new EnumOperationMethodInfo(lhs, op, lhs, false);
                            }
                            if (!DecimalIntegerLiteralZero(rhs, rhsExpression as CodePrimitiveExpression))
                            {
                                break;
                            }
                            error = null;
                            return new EnumOperationMethodInfo(lhs, op, rhs, true);
                        }
                        error = null;
                        return new EnumOperationMethodInfo(lhs, op, rhs, false);
                }
            }
            AddOperatorOverloads(rhsType, str, lhs, rhs, candidates);
            AddOperatorOverloads(type, str, lhs, rhs, candidates);
            if ((flag || flag2) || ((lhs == typeof(NullLiteral)) || (rhs == typeof(NullLiteral))))
            {
                AddLiftedOperators(rhsType, str, arithmetic, rhsType, type, candidates);
                AddLiftedOperators(type, str, arithmetic, rhsType, type, candidates);
            }
            if (candidates.Count == 0)
            {
                str = str.Substring(3);
                foreach (MethodInfo info in typeof(DefaultOperators).GetMethods())
                {
                    if (info.Name == str)
                    {
                        ParameterInfo[] parameters = info.GetParameters();
                        Type parameterType = parameters[0].ParameterType;
                        Type toType = parameters[1].ParameterType;
                        if (RuleValidation.ImplicitConversion(lhs, parameterType) && RuleValidation.ImplicitConversion(rhs, toType))
                        {
                            candidates.Add(info);
                        }
                    }
                }
                if ((((candidates.Count == 0) && ("Equality" == str)) && (!lhs.IsValueType && !rhs.IsValueType)) && (((lhs == typeof(NullLiteral)) || (rhs == typeof(NullLiteral))) || (lhs.IsAssignableFrom(rhs) || rhs.IsAssignableFrom(lhs))))
                {
                    candidates.Add(ObjectEquality);
                }
                if ((candidates.Count == 0) && ((flag || flag2) || ((lhs == typeof(NullLiteral)) || (rhs == typeof(NullLiteral)))))
                {
                    foreach (MethodInfo info2 in typeof(DefaultOperators).GetMethods())
                    {
                        if (info2.Name == str)
                        {
                            ParameterInfo[] infoArray2 = info2.GetParameters();
                            MethodInfo item = EvaluateLiftedMethod(info2, infoArray2, arithmetic, rhsType, type);
                            if (item != null)
                            {
                                candidates.Add(item);
                            }
                        }
                    }
                }
            }
            if (candidates.Count == 1)
            {
                error = null;
                return candidates[0];
            }
            if (candidates.Count == 0)
            {
                str2 = string.Format(CultureInfo.CurrentCulture, (arithmetic == OperatorGrouping.Arithmetic) ? Messages.ArithOpBadTypes : Messages.RelationalOpBadTypes, new object[] { op.ToString(), (lhs == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(lhs), (rhs == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(rhs) });
                error = new ValidationError(str2, 0x545);
                return null;
            }
            MethodInfo info4 = validator.FindBestCandidate(null, candidates, new Type[] { lhs, rhs });
            if (info4 != null)
            {
                error = null;
                return info4;
            }
            str2 = string.Format(CultureInfo.CurrentCulture, Messages.AmbiguousOperator, new object[] { op.ToString(), RuleDecompiler.DecompileMethod(candidates[0]), RuleDecompiler.DecompileMethod(candidates[1]) });
            error = new ValidationError(str2, 0x545);
            return null;
        }

        internal abstract object Value { get; }

        internal static class DefaultOperators
        {
            public static decimal Addition(decimal x, decimal y)
            {
                return (x + y);
            }

            public static double Addition(double x, double y)
            {
                return (x + y);
            }

            public static int Addition(int x, int y)
            {
                return (x + y);
            }

            public static long Addition(long x, long y)
            {
                return (x + y);
            }

            public static string Addition(object x, string y)
            {
                return (x + y);
            }

            public static float Addition(float x, float y)
            {
                return (x + y);
            }

            public static string Addition(string x, object y)
            {
                return (x + y);
            }

            public static string Addition(string x, string y)
            {
                return (x + y);
            }

            public static uint Addition(uint x, uint y)
            {
                return (x + y);
            }

            public static ulong Addition(ulong x, ulong y)
            {
                return (x + y);
            }

            public static bool BitwiseAnd(bool x, bool y)
            {
                return (x & y);
            }

            public static int BitwiseAnd(int x, int y)
            {
                return (x & y);
            }

            public static long BitwiseAnd(long x, long y)
            {
                return (x & y);
            }

            public static uint BitwiseAnd(uint x, uint y)
            {
                return (x & y);
            }

            public static ulong BitwiseAnd(ulong x, ulong y)
            {
                return (x & y);
            }

            public static bool BitwiseOr(bool x, bool y)
            {
                return (x | y);
            }

            public static int BitwiseOr(int x, int y)
            {
                return (x | y);
            }

            public static long BitwiseOr(long x, long y)
            {
                return (x | y);
            }

            public static uint BitwiseOr(uint x, uint y)
            {
                return (x | y);
            }

            public static ulong BitwiseOr(ulong x, ulong y)
            {
                return (x | y);
            }

            public static decimal Division(decimal x, decimal y)
            {
                return (x / y);
            }

            public static double Division(double x, double y)
            {
                return (x / y);
            }

            public static int Division(int x, int y)
            {
                return (x / y);
            }

            public static long Division(long x, long y)
            {
                return (x / y);
            }

            public static float Division(float x, float y)
            {
                return (x / y);
            }

            public static uint Division(uint x, uint y)
            {
                return (x / y);
            }

            public static ulong Division(ulong x, ulong y)
            {
                return (x / y);
            }

            public static bool Equality(bool x, bool y)
            {
                return (x == y);
            }

            public static bool Equality(decimal x, decimal y)
            {
                return (x == y);
            }

            public static bool Equality(double x, double y)
            {
                return (x == y);
            }

            public static bool Equality(int x, int y)
            {
                return (x == y);
            }

            public static bool Equality(long x, long y)
            {
                return (x == y);
            }

            public static bool Equality(float x, float y)
            {
                return (x == y);
            }

            public static bool Equality(string x, string y)
            {
                return (x == y);
            }

            public static bool Equality(uint x, uint y)
            {
                return (x == y);
            }

            public static bool Equality(ulong x, ulong y)
            {
                return (x == y);
            }

            public static bool GreaterThan(decimal x, decimal y)
            {
                return (x > y);
            }

            public static bool GreaterThan(double x, double y)
            {
                return (x > y);
            }

            public static bool GreaterThan(int x, int y)
            {
                return (x > y);
            }

            public static bool GreaterThan(long x, long y)
            {
                return (x > y);
            }

            public static bool GreaterThan(float x, float y)
            {
                return (x > y);
            }

            public static bool GreaterThan(uint x, uint y)
            {
                return (x > y);
            }

            public static bool GreaterThan(ulong x, ulong y)
            {
                return (x > y);
            }

            public static bool GreaterThanOrEqual(decimal x, decimal y)
            {
                return (x >= y);
            }

            public static bool GreaterThanOrEqual(double x, double y)
            {
                return (x >= y);
            }

            public static bool GreaterThanOrEqual(int x, int y)
            {
                return (x >= y);
            }

            public static bool GreaterThanOrEqual(long x, long y)
            {
                return (x >= y);
            }

            public static bool GreaterThanOrEqual(float x, float y)
            {
                return (x >= y);
            }

            public static bool GreaterThanOrEqual(uint x, uint y)
            {
                return (x >= y);
            }

            public static bool GreaterThanOrEqual(ulong x, ulong y)
            {
                return (x >= y);
            }

            public static bool LessThan(decimal x, decimal y)
            {
                return (x < y);
            }

            public static bool LessThan(double x, double y)
            {
                return (x < y);
            }

            public static bool LessThan(int x, int y)
            {
                return (x < y);
            }

            public static bool LessThan(long x, long y)
            {
                return (x < y);
            }

            public static bool LessThan(float x, float y)
            {
                return (x < y);
            }

            public static bool LessThan(uint x, uint y)
            {
                return (x < y);
            }

            public static bool LessThan(ulong x, ulong y)
            {
                return (x < y);
            }

            public static bool LessThanOrEqual(decimal x, decimal y)
            {
                return (x <= y);
            }

            public static bool LessThanOrEqual(double x, double y)
            {
                return (x <= y);
            }

            public static bool LessThanOrEqual(int x, int y)
            {
                return (x <= y);
            }

            public static bool LessThanOrEqual(long x, long y)
            {
                return (x <= y);
            }

            public static bool LessThanOrEqual(float x, float y)
            {
                return (x <= y);
            }

            public static bool LessThanOrEqual(uint x, uint y)
            {
                return (x <= y);
            }

            public static bool LessThanOrEqual(ulong x, ulong y)
            {
                return (x <= y);
            }

            public static decimal Modulus(decimal x, decimal y)
            {
                return (x % y);
            }

            public static double Modulus(double x, double y)
            {
                return (x % y);
            }

            public static int Modulus(int x, int y)
            {
                return (x % y);
            }

            public static long Modulus(long x, long y)
            {
                return (x % y);
            }

            public static float Modulus(float x, float y)
            {
                return (x % y);
            }

            public static uint Modulus(uint x, uint y)
            {
                return (x % y);
            }

            public static ulong Modulus(ulong x, ulong y)
            {
                return (x % y);
            }

            public static decimal Multiply(decimal x, decimal y)
            {
                return (x * y);
            }

            public static double Multiply(double x, double y)
            {
                return (x * y);
            }

            public static int Multiply(int x, int y)
            {
                return (x * y);
            }

            public static long Multiply(long x, long y)
            {
                return (x * y);
            }

            public static float Multiply(float x, float y)
            {
                return (x * y);
            }

            public static uint Multiply(uint x, uint y)
            {
                return (x * y);
            }

            public static ulong Multiply(ulong x, ulong y)
            {
                return (x * y);
            }

            public static bool ObjectEquality(object x, object y)
            {
                return (x == y);
            }

            public static decimal Subtraction(decimal x, decimal y)
            {
                return (x - y);
            }

            public static double Subtraction(double x, double y)
            {
                return (x - y);
            }

            public static int Subtraction(int x, int y)
            {
                return (x - y);
            }

            public static long Subtraction(long x, long y)
            {
                return (x - y);
            }

            public static float Subtraction(float x, float y)
            {
                return (x - y);
            }

            public static uint Subtraction(uint x, uint y)
            {
                return (x - y);
            }

            public static ulong Subtraction(ulong x, ulong y)
            {
                return (x - y);
            }
        }

        private delegate Literal LiteralMaker(object literalValue);

        internal enum OperatorGrouping
        {
            Arithmetic,
            Equality,
            Relational
        }

        [Flags]
        private enum TypeFlags
        {
            Bool = 0x40,
            Decimal = 0x10,
            Float = 8,
            SignedNumbers = 1,
            String = 0x20,
            ULong = 4,
            UnsignedNumbers = 2
        }
    }
}

