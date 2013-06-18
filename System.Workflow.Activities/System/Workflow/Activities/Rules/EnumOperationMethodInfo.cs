namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;

    internal class EnumOperationMethodInfo : MethodInfo
    {
        private ParameterInfo[] expectedParameters;
        private Type lhsBaseType;
        private Type lhsRootType;
        private CodeBinaryOperatorType op;
        private Type resultBaseType;
        private bool resultIsNullable;
        private Type resultRootType;
        private Type resultType;
        private Type rhsBaseType;
        private Type rhsRootType;

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public EnumOperationMethodInfo(Type lhs, CodeBinaryOperatorType operation, Type rhs, bool isZero)
        {
            this.op = operation;
            this.expectedParameters = new ParameterInfo[] { new SimpleParameterInfo(lhs), new SimpleParameterInfo(rhs) };
            bool flag = ConditionHelper.IsNullableValueType(lhs);
            bool flag2 = ConditionHelper.IsNullableValueType(rhs);
            this.lhsBaseType = flag ? Nullable.GetUnderlyingType(lhs) : lhs;
            this.rhsBaseType = flag2 ? Nullable.GetUnderlyingType(rhs) : rhs;
            if (this.lhsBaseType.IsEnum)
            {
                this.lhsRootType = EnumHelper.GetUnderlyingType(this.lhsBaseType);
            }
            else
            {
                this.lhsRootType = this.lhsBaseType;
            }
            if (this.rhsBaseType.IsEnum)
            {
                this.rhsRootType = EnumHelper.GetUnderlyingType(this.rhsBaseType);
            }
            else
            {
                this.rhsRootType = this.rhsBaseType;
            }
            switch (this.op)
            {
                case CodeBinaryOperatorType.Add:
                    if (!this.lhsBaseType.IsEnum || !rhs.IsEnum)
                    {
                        if (this.lhsBaseType.IsEnum)
                        {
                            this.resultBaseType = this.lhsBaseType;
                        }
                        else
                        {
                            this.resultBaseType = this.rhsBaseType;
                        }
                        break;
                    }
                    this.resultBaseType = this.lhsRootType;
                    break;

                case CodeBinaryOperatorType.Subtract:
                    if (!this.rhsBaseType.IsEnum || !this.lhsBaseType.IsEnum)
                    {
                        if (this.lhsBaseType.IsEnum)
                        {
                            this.resultRootType = this.lhsRootType;
                            if (isZero && (this.rhsBaseType != this.lhsRootType))
                            {
                                this.resultBaseType = this.lhsRootType;
                            }
                            else
                            {
                                this.resultBaseType = this.lhsBaseType;
                            }
                        }
                        else
                        {
                            this.resultRootType = this.rhsRootType;
                            if (isZero)
                            {
                                this.resultBaseType = this.rhsRootType;
                            }
                            else
                            {
                                this.resultBaseType = this.rhsBaseType;
                            }
                        }
                    }
                    else
                    {
                        this.resultRootType = this.rhsRootType;
                        this.resultBaseType = this.rhsRootType;
                    }
                    this.resultIsNullable = flag || flag2;
                    this.resultType = this.resultIsNullable ? typeof(Nullable<>).MakeGenericType(new Type[] { this.resultBaseType }) : this.resultBaseType;
                    return;

                case CodeBinaryOperatorType.ValueEquality:
                case CodeBinaryOperatorType.LessThan:
                case CodeBinaryOperatorType.LessThanOrEqual:
                case CodeBinaryOperatorType.GreaterThan:
                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    this.resultType = typeof(bool);
                    return;

                case CodeBinaryOperatorType.BitwiseOr:
                case CodeBinaryOperatorType.BitwiseAnd:
                case CodeBinaryOperatorType.BooleanOr:
                case CodeBinaryOperatorType.BooleanAnd:
                    return;

                default:
                    return;
            }
            this.resultIsNullable = flag || flag2;
            this.resultType = this.resultIsNullable ? typeof(Nullable<>).MakeGenericType(new Type[] { this.resultBaseType }) : this.resultBaseType;
        }

        public override MethodInfo GetBaseDefinition()
        {
            return null;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new object[0];
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return MethodImplAttributes.CodeTypeMask;
        }

        public override ParameterInfo[] GetParameters()
        {
            return this.expectedParameters;
        }

        [SuppressMessage("Microsoft.Performance", "CA1803:AvoidCostlyCallsWherePossible")]
        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            object obj2;
            ArithmeticLiteral literal;
            ArithmeticLiteral literal2;
            Literal literal3;
            Literal literal4;
            if (this.lhsRootType == null)
            {
                this.lhsRootType = Enum.GetUnderlyingType(this.lhsBaseType);
            }
            if (this.rhsRootType == null)
            {
                this.rhsRootType = Enum.GetUnderlyingType(this.rhsBaseType);
            }
            switch (this.op)
            {
                case CodeBinaryOperatorType.Add:
                    if ((parameters[0] != null) && (parameters[1] != null))
                    {
                        literal = ArithmeticLiteral.MakeLiteral(this.lhsRootType, parameters[0]);
                        literal2 = ArithmeticLiteral.MakeLiteral(this.rhsRootType, parameters[1]);
                        obj2 = literal.Add(literal2);
                        obj2 = Executor.AdjustType(obj2.GetType(), obj2, this.resultBaseType);
                        if (this.resultIsNullable)
                        {
                            obj2 = Activator.CreateInstance(this.resultType, new object[] { obj2 });
                        }
                        return obj2;
                    }
                    return null;

                case CodeBinaryOperatorType.Subtract:
                    if ((parameters[0] != null) && (parameters[1] != null))
                    {
                        literal = ArithmeticLiteral.MakeLiteral(this.resultRootType, Executor.AdjustType(this.lhsRootType, parameters[0], this.resultRootType));
                        literal2 = ArithmeticLiteral.MakeLiteral(this.resultRootType, Executor.AdjustType(this.rhsRootType, parameters[1], this.resultRootType));
                        obj2 = literal.Subtract(literal2);
                        obj2 = Executor.AdjustType(obj2.GetType(), obj2, this.resultBaseType);
                        if (this.resultIsNullable)
                        {
                            obj2 = Activator.CreateInstance(this.resultType, new object[] { obj2 });
                        }
                        return obj2;
                    }
                    return null;

                case CodeBinaryOperatorType.ValueEquality:
                    literal3 = Literal.MakeLiteral(this.lhsRootType, parameters[0]);
                    literal4 = Literal.MakeLiteral(this.rhsRootType, parameters[1]);
                    return literal3.Equal(literal4);

                case CodeBinaryOperatorType.LessThan:
                    literal3 = Literal.MakeLiteral(this.lhsRootType, parameters[0]);
                    literal4 = Literal.MakeLiteral(this.rhsRootType, parameters[1]);
                    return literal3.LessThan(literal4);

                case CodeBinaryOperatorType.LessThanOrEqual:
                    literal3 = Literal.MakeLiteral(this.lhsRootType, parameters[0]);
                    literal4 = Literal.MakeLiteral(this.rhsRootType, parameters[1]);
                    return literal3.LessThanOrEqual(literal4);

                case CodeBinaryOperatorType.GreaterThan:
                    literal3 = Literal.MakeLiteral(this.lhsRootType, parameters[0]);
                    literal4 = Literal.MakeLiteral(this.rhsRootType, parameters[1]);
                    return literal3.GreaterThan(literal4);

                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    literal3 = Literal.MakeLiteral(this.lhsRootType, parameters[0]);
                    literal4 = Literal.MakeLiteral(this.rhsRootType, parameters[1]);
                    return literal3.GreaterThanOrEqual(literal4);
            }
            throw new RuleEvaluationException(string.Format(CultureInfo.CurrentCulture, Messages.BinaryOpNotSupported, new object[] { this.op.ToString() }));
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return true;
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return MethodAttributes.Static;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return typeof(Enum);
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                return new RuntimeMethodHandle();
            }
        }

        public override string Name
        {
            get
            {
                return "op_Enum";
            }
        }

        public override Type ReflectedType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.resultType;
            }
        }

        public override Type ReturnType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.resultType;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return null;
            }
        }
    }
}

