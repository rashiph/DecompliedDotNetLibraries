namespace System.Linq.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal static class Error
    {
        internal static Exception AccessorsCannotHaveByRefArgs()
        {
            return new ArgumentException(Strings.AccessorsCannotHaveByRefArgs);
        }

        internal static Exception AccessorsCannotHaveVarArgs()
        {
            return new ArgumentException(Strings.AccessorsCannotHaveVarArgs);
        }

        internal static Exception AmbiguousJump(object p0)
        {
            return new InvalidOperationException(Strings.AmbiguousJump(p0));
        }

        internal static Exception AmbiguousMatchInExpandoObject(object p0)
        {
            return new AmbiguousMatchException(Strings.AmbiguousMatchInExpandoObject(p0));
        }

        internal static Exception ArgCntMustBeGreaterThanNameCnt()
        {
            return new ArgumentException(Strings.ArgCntMustBeGreaterThanNameCnt);
        }

        internal static Exception ArgumentCannotBeOfTypeVoid()
        {
            return new ArgumentException(Strings.ArgumentCannotBeOfTypeVoid);
        }

        internal static Exception ArgumentMemberNotDeclOnType(object p0, object p1)
        {
            return new ArgumentException(Strings.ArgumentMemberNotDeclOnType(p0, p1));
        }

        internal static Exception ArgumentMustBeArray()
        {
            return new ArgumentException(Strings.ArgumentMustBeArray);
        }

        internal static Exception ArgumentMustBeArrayIndexType()
        {
            return new ArgumentException(Strings.ArgumentMustBeArrayIndexType);
        }

        internal static Exception ArgumentMustBeBoolean()
        {
            return new ArgumentException(Strings.ArgumentMustBeBoolean);
        }

        internal static Exception ArgumentMustBeFieldInfoOrPropertInfo()
        {
            return new ArgumentException(Strings.ArgumentMustBeFieldInfoOrPropertInfo);
        }

        internal static Exception ArgumentMustBeFieldInfoOrPropertInfoOrMethod()
        {
            return new ArgumentException(Strings.ArgumentMustBeFieldInfoOrPropertInfoOrMethod);
        }

        internal static Exception ArgumentMustBeInstanceMember()
        {
            return new ArgumentException(Strings.ArgumentMustBeInstanceMember);
        }

        internal static Exception ArgumentMustBeInteger()
        {
            return new ArgumentException(Strings.ArgumentMustBeInteger);
        }

        internal static Exception ArgumentMustBeSingleDimensionalArrayType()
        {
            return new ArgumentException(Strings.ArgumentMustBeSingleDimensionalArrayType);
        }

        internal static Exception ArgumentMustNotHaveValueType()
        {
            return new ArgumentException(Strings.ArgumentMustNotHaveValueType);
        }

        internal static Exception ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        internal static Exception ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        internal static Exception ArgumentTypeCannotBeVoid()
        {
            return new ArgumentException(Strings.ArgumentTypeCannotBeVoid);
        }

        internal static Exception ArgumentTypeDoesNotMatchMember(object p0, object p1)
        {
            return new ArgumentException(Strings.ArgumentTypeDoesNotMatchMember(p0, p1));
        }

        internal static Exception ArgumentTypesMustMatch()
        {
            return new ArgumentException(Strings.ArgumentTypesMustMatch);
        }

        internal static Exception ArrayTypeMustBeArray()
        {
            return new ArgumentException(Strings.ArrayTypeMustBeArray);
        }

        internal static Exception BinaryOperatorNotDefined(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.BinaryOperatorNotDefined(p0, p1, p2));
        }

        internal static Exception BinderNotCompatibleWithCallSite(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.BinderNotCompatibleWithCallSite(p0, p1, p2));
        }

        internal static Exception BindingCannotBeNull()
        {
            return new InvalidOperationException(Strings.BindingCannotBeNull);
        }

        internal static Exception BodyOfCatchMustHaveSameTypeAsBodyOfTry()
        {
            return new ArgumentException(Strings.BodyOfCatchMustHaveSameTypeAsBodyOfTry);
        }

        internal static Exception BothAccessorsMustBeStatic()
        {
            return new ArgumentException(Strings.BothAccessorsMustBeStatic);
        }

        internal static Exception BoundsCannotBeLessThanOne()
        {
            return new ArgumentException(Strings.BoundsCannotBeLessThanOne);
        }

        internal static Exception CannotAutoInitializeValueTypeElementThroughProperty(object p0)
        {
            return new InvalidOperationException(Strings.CannotAutoInitializeValueTypeElementThroughProperty(p0));
        }

        internal static Exception CannotAutoInitializeValueTypeMemberThroughProperty(object p0)
        {
            return new InvalidOperationException(Strings.CannotAutoInitializeValueTypeMemberThroughProperty(p0));
        }

        internal static Exception CannotCloseOverByRef(object p0, object p1)
        {
            return new InvalidOperationException(Strings.CannotCloseOverByRef(p0, p1));
        }

        internal static Exception CannotCompileConstant(object p0)
        {
            return new InvalidOperationException(Strings.CannotCompileConstant(p0));
        }

        internal static Exception CannotCompileDynamic()
        {
            return new NotSupportedException(Strings.CannotCompileDynamic);
        }

        internal static Exception CoalesceUsedOnNonNullType()
        {
            return new InvalidOperationException(Strings.CoalesceUsedOnNonNullType);
        }

        internal static Exception CoercionOperatorNotDefined(object p0, object p1)
        {
            return new InvalidOperationException(Strings.CoercionOperatorNotDefined(p0, p1));
        }

        internal static Exception CollectionModifiedWhileEnumerating()
        {
            return new InvalidOperationException(Strings.CollectionModifiedWhileEnumerating);
        }

        internal static Exception CollectionReadOnly()
        {
            return new NotSupportedException(Strings.CollectionReadOnly);
        }

        internal static Exception ControlCannotEnterExpression()
        {
            return new InvalidOperationException(Strings.ControlCannotEnterExpression);
        }

        internal static Exception ControlCannotEnterTry()
        {
            return new InvalidOperationException(Strings.ControlCannotEnterTry);
        }

        internal static Exception ControlCannotLeaveFilterTest()
        {
            return new InvalidOperationException(Strings.ControlCannotLeaveFilterTest);
        }

        internal static Exception ControlCannotLeaveFinally()
        {
            return new InvalidOperationException(Strings.ControlCannotLeaveFinally);
        }

        internal static Exception ConversionIsNotSupportedForArithmeticTypes()
        {
            return new InvalidOperationException(Strings.ConversionIsNotSupportedForArithmeticTypes);
        }

        internal static Exception CountCannotBeNegative()
        {
            return new ArgumentException(Strings.CountCannotBeNegative);
        }

        internal static Exception DefaultBodyMustBeSupplied()
        {
            return new ArgumentException(Strings.DefaultBodyMustBeSupplied);
        }

        internal static Exception DuplicateVariable(object p0)
        {
            return new ArgumentException(Strings.DuplicateVariable(p0));
        }

        internal static Exception DynamicBinderResultNotAssignable(object p0, object p1, object p2)
        {
            return new InvalidCastException(Strings.DynamicBinderResultNotAssignable(p0, p1, p2));
        }

        internal static Exception DynamicBindingNeedsRestrictions(object p0, object p1)
        {
            return new InvalidOperationException(Strings.DynamicBindingNeedsRestrictions(p0, p1));
        }

        internal static Exception DynamicObjectResultNotAssignable(object p0, object p1, object p2, object p3)
        {
            return new InvalidCastException(Strings.DynamicObjectResultNotAssignable(p0, p1, p2, p3));
        }

        internal static Exception ElementInitializerMethodNoRefOutParam(object p0, object p1)
        {
            return new ArgumentException(Strings.ElementInitializerMethodNoRefOutParam(p0, p1));
        }

        internal static Exception ElementInitializerMethodNotAdd()
        {
            return new ArgumentException(Strings.ElementInitializerMethodNotAdd);
        }

        internal static Exception ElementInitializerMethodStatic()
        {
            return new ArgumentException(Strings.ElementInitializerMethodStatic);
        }

        internal static Exception ElementInitializerMethodWithZeroArgs()
        {
            return new ArgumentException(Strings.ElementInitializerMethodWithZeroArgs);
        }

        internal static Exception EnumerationIsDone()
        {
            return new InvalidOperationException(Strings.EnumerationIsDone);
        }

        internal static Exception EqualityMustReturnBoolean(object p0)
        {
            return new ArgumentException(Strings.EqualityMustReturnBoolean(p0));
        }

        internal static Exception ExpressionTypeCannotInitializeArrayType(object p0, object p1)
        {
            return new InvalidOperationException(Strings.ExpressionTypeCannotInitializeArrayType(p0, p1));
        }

        internal static Exception ExpressionTypeDoesNotMatchAssignment(object p0, object p1)
        {
            return new ArgumentException(Strings.ExpressionTypeDoesNotMatchAssignment(p0, p1));
        }

        internal static Exception ExpressionTypeDoesNotMatchConstructorParameter(object p0, object p1)
        {
            return new ArgumentException(Strings.ExpressionTypeDoesNotMatchConstructorParameter(p0, p1));
        }

        internal static Exception ExpressionTypeDoesNotMatchLabel(object p0, object p1)
        {
            return new ArgumentException(Strings.ExpressionTypeDoesNotMatchLabel(p0, p1));
        }

        internal static Exception ExpressionTypeDoesNotMatchMethodParameter(object p0, object p1, object p2)
        {
            return new ArgumentException(Strings.ExpressionTypeDoesNotMatchMethodParameter(p0, p1, p2));
        }

        internal static Exception ExpressionTypeDoesNotMatchParameter(object p0, object p1)
        {
            return new ArgumentException(Strings.ExpressionTypeDoesNotMatchParameter(p0, p1));
        }

        internal static Exception ExpressionTypeDoesNotMatchReturn(object p0, object p1)
        {
            return new ArgumentException(Strings.ExpressionTypeDoesNotMatchReturn(p0, p1));
        }

        internal static Exception ExpressionTypeNotInvocable(object p0)
        {
            return new ArgumentException(Strings.ExpressionTypeNotInvocable(p0));
        }

        internal static Exception ExtensionNodeMustOverrideProperty(object p0)
        {
            return new InvalidOperationException(Strings.ExtensionNodeMustOverrideProperty(p0));
        }

        internal static Exception ExtensionNotReduced()
        {
            return new InvalidOperationException(Strings.ExtensionNotReduced);
        }

        internal static Exception FaultCannotHaveCatchOrFinally()
        {
            return new ArgumentException(Strings.FaultCannotHaveCatchOrFinally);
        }

        internal static Exception FieldInfoNotDefinedForType(object p0, object p1, object p2)
        {
            return new ArgumentException(Strings.FieldInfoNotDefinedForType(p0, p1, p2));
        }

        internal static Exception FieldNotDefinedForType(object p0, object p1)
        {
            return new ArgumentException(Strings.FieldNotDefinedForType(p0, p1));
        }

        internal static Exception FirstArgumentMustBeCallSite()
        {
            return new ArgumentException(Strings.FirstArgumentMustBeCallSite);
        }

        internal static Exception GenericMethodWithArgsDoesNotExistOnType(object p0, object p1)
        {
            return new InvalidOperationException(Strings.GenericMethodWithArgsDoesNotExistOnType(p0, p1));
        }

        internal static Exception HomogenousAppDomainRequired()
        {
            return new InvalidOperationException(Strings.HomogenousAppDomainRequired);
        }

        internal static Exception IllegalNewGenericParams(object p0)
        {
            return new ArgumentException(Strings.IllegalNewGenericParams(p0));
        }

        internal static Exception IncorrectNumberOfArgumentsForMembers()
        {
            return new ArgumentException(Strings.IncorrectNumberOfArgumentsForMembers);
        }

        internal static Exception IncorrectNumberOfConstructorArguments()
        {
            return new ArgumentException(Strings.IncorrectNumberOfConstructorArguments);
        }

        internal static Exception IncorrectNumberOfIndexes()
        {
            return new ArgumentException(Strings.IncorrectNumberOfIndexes);
        }

        internal static Exception IncorrectNumberOfLambdaArguments()
        {
            return new InvalidOperationException(Strings.IncorrectNumberOfLambdaArguments);
        }

        internal static Exception IncorrectNumberOfLambdaDeclarationParameters()
        {
            return new ArgumentException(Strings.IncorrectNumberOfLambdaDeclarationParameters);
        }

        internal static Exception IncorrectNumberOfMembersForGivenConstructor()
        {
            return new ArgumentException(Strings.IncorrectNumberOfMembersForGivenConstructor);
        }

        internal static Exception IncorrectNumberOfMethodCallArguments(object p0)
        {
            return new ArgumentException(Strings.IncorrectNumberOfMethodCallArguments(p0));
        }

        internal static Exception IncorrectNumberOfTypeArgsForAction()
        {
            return new ArgumentException(Strings.IncorrectNumberOfTypeArgsForAction);
        }

        internal static Exception IncorrectNumberOfTypeArgsForFunc()
        {
            return new ArgumentException(Strings.IncorrectNumberOfTypeArgsForFunc);
        }

        internal static Exception IncorrectTypeForTypeAs(object p0)
        {
            return new ArgumentException(Strings.IncorrectTypeForTypeAs(p0));
        }

        internal static Exception IndexesOfSetGetMustMatch()
        {
            return new ArgumentException(Strings.IndexesOfSetGetMustMatch);
        }

        internal static Exception InstanceAndMethodTypeMismatch(object p0, object p1, object p2)
        {
            return new ArgumentException(Strings.InstanceAndMethodTypeMismatch(p0, p1, p2));
        }

        internal static Exception InstanceFieldNotDefinedForType(object p0, object p1)
        {
            return new ArgumentException(Strings.InstanceFieldNotDefinedForType(p0, p1));
        }

        internal static Exception InstancePropertyNotDefinedForType(object p0, object p1)
        {
            return new ArgumentException(Strings.InstancePropertyNotDefinedForType(p0, p1));
        }

        internal static Exception InstancePropertyWithoutParameterNotDefinedForType(object p0, object p1)
        {
            return new ArgumentException(Strings.InstancePropertyWithoutParameterNotDefinedForType(p0, p1));
        }

        internal static Exception InstancePropertyWithSpecifiedParametersNotDefinedForType(object p0, object p1, object p2)
        {
            return new ArgumentException(Strings.InstancePropertyWithSpecifiedParametersNotDefinedForType(p0, p1, p2));
        }

        internal static Exception InvalidAsmNameOrExtension()
        {
            return new ArgumentException(Strings.InvalidAsmNameOrExtension);
        }

        internal static Exception InvalidCast(object p0, object p1)
        {
            return new InvalidOperationException(Strings.InvalidCast(p0, p1));
        }

        internal static Exception InvalidLvalue(object p0)
        {
            return new InvalidOperationException(Strings.InvalidLvalue(p0));
        }

        internal static Exception InvalidMemberType(object p0)
        {
            return new InvalidOperationException(Strings.InvalidMemberType(p0));
        }

        internal static Exception InvalidMetaObjectCreated(object p0)
        {
            return new InvalidOperationException(Strings.InvalidMetaObjectCreated(p0));
        }

        internal static Exception InvalidOperation(object p0)
        {
            return new ArgumentException(Strings.InvalidOperation(p0));
        }

        internal static Exception InvalidOutputDir()
        {
            return new ArgumentException(Strings.InvalidOutputDir);
        }

        internal static Exception InvalidUnboxType()
        {
            return new ArgumentException(Strings.InvalidUnboxType);
        }

        internal static Exception KeyDoesNotExistInExpando(object p0)
        {
            return new KeyNotFoundException(Strings.KeyDoesNotExistInExpando(p0));
        }

        internal static Exception LabelMustBeVoidOrHaveExpression()
        {
            return new ArgumentException(Strings.LabelMustBeVoidOrHaveExpression);
        }

        internal static Exception LabelTargetAlreadyDefined(object p0)
        {
            return new InvalidOperationException(Strings.LabelTargetAlreadyDefined(p0));
        }

        internal static Exception LabelTargetUndefined(object p0)
        {
            return new InvalidOperationException(Strings.LabelTargetUndefined(p0));
        }

        internal static Exception LabelTypeMustBeVoid()
        {
            return new ArgumentException(Strings.LabelTypeMustBeVoid);
        }

        internal static Exception LambdaTypeMustBeDerivedFromSystemDelegate()
        {
            return new ArgumentException(Strings.LambdaTypeMustBeDerivedFromSystemDelegate);
        }

        internal static Exception ListInitializerWithZeroMembers()
        {
            return new ArgumentException(Strings.ListInitializerWithZeroMembers);
        }

        internal static Exception LogicalOperatorMustHaveBooleanOperators(object p0, object p1)
        {
            return new ArgumentException(Strings.LogicalOperatorMustHaveBooleanOperators(p0, p1));
        }

        internal static Exception MemberNotFieldOrProperty(object p0)
        {
            return new ArgumentException(Strings.MemberNotFieldOrProperty(p0));
        }

        internal static Exception MethodBuilderDoesNotHaveTypeBuilder()
        {
            return new ArgumentException(Strings.MethodBuilderDoesNotHaveTypeBuilder);
        }

        internal static Exception MethodContainsGenericParameters(object p0)
        {
            return new ArgumentException(Strings.MethodContainsGenericParameters(p0));
        }

        internal static Exception MethodDoesNotExistOnType(object p0, object p1)
        {
            return new InvalidOperationException(Strings.MethodDoesNotExistOnType(p0, p1));
        }

        internal static Exception MethodIsGeneric(object p0)
        {
            return new ArgumentException(Strings.MethodIsGeneric(p0));
        }

        internal static Exception MethodNotPropertyAccessor(object p0, object p1)
        {
            return new ArgumentException(Strings.MethodNotPropertyAccessor(p0, p1));
        }

        internal static Exception MethodWithArgsDoesNotExistOnType(object p0, object p1)
        {
            return new InvalidOperationException(Strings.MethodWithArgsDoesNotExistOnType(p0, p1));
        }

        internal static Exception MethodWithMoreThanOneMatch(object p0, object p1)
        {
            return new InvalidOperationException(Strings.MethodWithMoreThanOneMatch(p0, p1));
        }

        internal static Exception MustBeReducible()
        {
            return new ArgumentException(Strings.MustBeReducible);
        }

        internal static Exception MustReduceToDifferent()
        {
            return new ArgumentException(Strings.MustReduceToDifferent);
        }

        internal static Exception MustRewriteChildToSameType(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.MustRewriteChildToSameType(p0, p1, p2));
        }

        internal static Exception MustRewriteToSameNode(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.MustRewriteToSameNode(p0, p1, p2));
        }

        internal static Exception MustRewriteWithoutMethod(object p0, object p1)
        {
            return new InvalidOperationException(Strings.MustRewriteWithoutMethod(p0, p1));
        }

        internal static Exception NonLocalJumpWithValue(object p0)
        {
            return new InvalidOperationException(Strings.NonLocalJumpWithValue(p0));
        }

        internal static Exception NoOrInvalidRuleProduced()
        {
            return new InvalidOperationException(Strings.NoOrInvalidRuleProduced);
        }

        internal static Exception NotAMemberOfType(object p0, object p1)
        {
            return new ArgumentException(Strings.NotAMemberOfType(p0, p1));
        }

        internal static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        internal static Exception NotSupported()
        {
            return new NotSupportedException();
        }

        internal static Exception OnlyStaticMethodsHaveNullInstance()
        {
            return new ArgumentException(Strings.OnlyStaticMethodsHaveNullInstance);
        }

        internal static Exception OperandTypesDoNotMatchParameters(object p0, object p1)
        {
            return new InvalidOperationException(Strings.OperandTypesDoNotMatchParameters(p0, p1));
        }

        internal static Exception OperatorNotImplementedForType(object p0, object p1)
        {
            return new NotImplementedException(Strings.OperatorNotImplementedForType(p0, p1));
        }

        internal static Exception OutOfRange(object p0, object p1)
        {
            return new ArgumentOutOfRangeException(Strings.OutOfRange(p0, p1));
        }

        internal static Exception OverloadOperatorTypeDoesNotMatchConversionType(object p0, object p1)
        {
            return new InvalidOperationException(Strings.OverloadOperatorTypeDoesNotMatchConversionType(p0, p1));
        }

        internal static Exception ParameterExpressionNotValidAsDelegate(object p0, object p1)
        {
            return new ArgumentException(Strings.ParameterExpressionNotValidAsDelegate(p0, p1));
        }

        internal static Exception PdbGeneratorNeedsExpressionCompiler()
        {
            return new NotSupportedException(Strings.PdbGeneratorNeedsExpressionCompiler);
        }

        internal static Exception PropertyCannotHaveRefType()
        {
            return new ArgumentException(Strings.PropertyCannotHaveRefType);
        }

        internal static Exception PropertyDoesNotHaveAccessor(object p0)
        {
            return new ArgumentException(Strings.PropertyDoesNotHaveAccessor(p0));
        }

        internal static Exception PropertyDoesNotHaveGetter(object p0)
        {
            return new ArgumentException(Strings.PropertyDoesNotHaveGetter(p0));
        }

        internal static Exception PropertyDoesNotHaveSetter(object p0)
        {
            return new ArgumentException(Strings.PropertyDoesNotHaveSetter(p0));
        }

        internal static Exception PropertyNotDefinedForType(object p0, object p1)
        {
            return new ArgumentException(Strings.PropertyNotDefinedForType(p0, p1));
        }

        internal static Exception PropertyTyepMustMatchSetter()
        {
            return new ArgumentException(Strings.PropertyTyepMustMatchSetter);
        }

        internal static Exception PropertyTypeCannotBeVoid()
        {
            return new ArgumentException(Strings.PropertyTypeCannotBeVoid);
        }

        internal static Exception PropertyWithMoreThanOneMatch(object p0, object p1)
        {
            return new InvalidOperationException(Strings.PropertyWithMoreThanOneMatch(p0, p1));
        }

        internal static Exception QueueEmpty()
        {
            return new InvalidOperationException(Strings.QueueEmpty);
        }

        internal static Exception QuotedExpressionMustBeLambda()
        {
            return new ArgumentException(Strings.QuotedExpressionMustBeLambda);
        }

        internal static Exception ReducedNotCompatible()
        {
            return new ArgumentException(Strings.ReducedNotCompatible);
        }

        internal static Exception ReducibleMustOverrideReduce()
        {
            return new ArgumentException(Strings.ReducibleMustOverrideReduce);
        }

        internal static Exception ReferenceEqualityNotDefined(object p0, object p1)
        {
            return new InvalidOperationException(Strings.ReferenceEqualityNotDefined(p0, p1));
        }

        internal static Exception RethrowRequiresCatch()
        {
            return new InvalidOperationException(Strings.RethrowRequiresCatch);
        }

        internal static Exception SameKeyExistsInExpando(object p0)
        {
            return new ArgumentException(Strings.SameKeyExistsInExpando(p0));
        }

        internal static Exception SetterHasNoParams()
        {
            return new ArgumentException(Strings.SetterHasNoParams);
        }

        internal static Exception SetterMustBeVoid()
        {
            return new ArgumentException(Strings.SetterMustBeVoid);
        }

        internal static Exception StartEndMustBeOrdered()
        {
            return new ArgumentException(Strings.StartEndMustBeOrdered);
        }

        internal static Exception SwitchValueTypeDoesNotMatchComparisonMethodParameter(object p0, object p1)
        {
            return new ArgumentException(Strings.SwitchValueTypeDoesNotMatchComparisonMethodParameter(p0, p1));
        }

        internal static Exception TestValueTypeDoesNotMatchComparisonMethodParameter(object p0, object p1)
        {
            return new ArgumentException(Strings.TestValueTypeDoesNotMatchComparisonMethodParameter(p0, p1));
        }

        internal static Exception TryMustHaveCatchFinallyOrFault()
        {
            return new ArgumentException(Strings.TryMustHaveCatchFinallyOrFault);
        }

        internal static Exception TryNotAllowedInFilter()
        {
            return new InvalidOperationException(Strings.TryNotAllowedInFilter);
        }

        internal static Exception TryNotSupportedForMethodsWithRefArgs(object p0)
        {
            return new NotSupportedException(Strings.TryNotSupportedForMethodsWithRefArgs(p0));
        }

        internal static Exception TryNotSupportedForValueTypeInstances(object p0)
        {
            return new NotSupportedException(Strings.TryNotSupportedForValueTypeInstances(p0));
        }

        internal static Exception TypeContainsGenericParameters(object p0)
        {
            return new ArgumentException(Strings.TypeContainsGenericParameters(p0));
        }

        internal static Exception TypeDoesNotHaveConstructorForTheSignature()
        {
            return new ArgumentException(Strings.TypeDoesNotHaveConstructorForTheSignature);
        }

        internal static Exception TypeIsGeneric(object p0)
        {
            return new ArgumentException(Strings.TypeIsGeneric(p0));
        }

        internal static Exception TypeMissingDefaultConstructor(object p0)
        {
            return new ArgumentException(Strings.TypeMissingDefaultConstructor(p0));
        }

        internal static Exception TypeMustBeDerivedFromSystemDelegate()
        {
            return new ArgumentException(Strings.TypeMustBeDerivedFromSystemDelegate);
        }

        internal static Exception TypeMustNotBeByRef()
        {
            return new ArgumentException(Strings.TypeMustNotBeByRef);
        }

        internal static Exception TypeNotIEnumerable(object p0)
        {
            return new ArgumentException(Strings.TypeNotIEnumerable(p0));
        }

        internal static Exception TypeParameterIsNotDelegate(object p0)
        {
            return new InvalidOperationException(Strings.TypeParameterIsNotDelegate(p0));
        }

        internal static Exception UnaryOperatorNotDefined(object p0, object p1)
        {
            return new InvalidOperationException(Strings.UnaryOperatorNotDefined(p0, p1));
        }

        internal static Exception UndefinedVariable(object p0, object p1, object p2)
        {
            return new InvalidOperationException(Strings.UndefinedVariable(p0, p1, p2));
        }

        internal static Exception UnexpectedCoalesceOperator()
        {
            return new InvalidOperationException(Strings.UnexpectedCoalesceOperator);
        }

        internal static Exception UnexpectedVarArgsCall(object p0)
        {
            return new InvalidOperationException(Strings.UnexpectedVarArgsCall(p0));
        }

        internal static Exception UnhandledBinary(object p0)
        {
            return new ArgumentException(Strings.UnhandledBinary(p0));
        }

        internal static Exception UnhandledBinding()
        {
            return new ArgumentException(Strings.UnhandledBinding);
        }

        internal static Exception UnhandledBindingType(object p0)
        {
            return new ArgumentException(Strings.UnhandledBindingType(p0));
        }

        internal static Exception UnhandledConvert(object p0)
        {
            return new ArgumentException(Strings.UnhandledConvert(p0));
        }

        internal static Exception UnhandledExpressionType(object p0)
        {
            return new ArgumentException(Strings.UnhandledExpressionType(p0));
        }

        internal static Exception UnhandledUnary(object p0)
        {
            return new ArgumentException(Strings.UnhandledUnary(p0));
        }

        internal static Exception UnknownBindingType()
        {
            return new ArgumentException(Strings.UnknownBindingType);
        }

        internal static Exception UnknownLiftType(object p0)
        {
            return new InvalidOperationException(Strings.UnknownLiftType(p0));
        }

        internal static Exception UserDefinedOperatorMustBeStatic(object p0)
        {
            return new ArgumentException(Strings.UserDefinedOperatorMustBeStatic(p0));
        }

        internal static Exception UserDefinedOperatorMustNotBeVoid(object p0)
        {
            return new ArgumentException(Strings.UserDefinedOperatorMustNotBeVoid(p0));
        }

        internal static Exception UserDefinedOpMustHaveConsistentTypes(object p0, object p1)
        {
            return new ArgumentException(Strings.UserDefinedOpMustHaveConsistentTypes(p0, p1));
        }

        internal static Exception UserDefinedOpMustHaveValidReturnType(object p0, object p1)
        {
            return new ArgumentException(Strings.UserDefinedOpMustHaveValidReturnType(p0, p1));
        }

        internal static Exception VariableMustNotBeByRef(object p0, object p1)
        {
            return new ArgumentException(Strings.VariableMustNotBeByRef(p0, p1));
        }
    }
}

