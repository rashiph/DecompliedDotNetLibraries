namespace System.Activities
{
    using System;
    using System.Globalization;
    using System.Resources;

    internal class SR
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceManager;

        private SR()
        {
        }

        internal static string ActivityCannotBeReferenced(object param0, object param1, object param2, object param3)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityCannotBeReferenced", Culture), new object[] { param0, param1, param2, param3 });
        }

        internal static string ActivityCannotBeReferencedWithoutTarget(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityCannotBeReferencedWithoutTarget", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ActivityCannotReferenceItself(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityCannotReferenceItself", Culture), new object[] { param0 });
        }

        internal static string ActivityDefinitionCannotBeShared(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityDefinitionCannotBeShared", Culture), new object[] { param0 });
        }

        internal static string ActivityDelegateAlreadyOpened(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityDelegateAlreadyOpened", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ActivityDelegateCannotBeReferenced(object param0, object param1, object param2, object param3)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityDelegateCannotBeReferenced", Culture), new object[] { param0, param1, param2, param3 });
        }

        internal static string ActivityDelegateCannotBeReferencedNoHandler(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityDelegateCannotBeReferencedNoHandler", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ActivityDelegateCannotBeReferencedWithoutTarget(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityDelegateCannotBeReferencedWithoutTarget", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ActivityDelegateCannotBeReferencedWithoutTargetNoHandler(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityDelegateCannotBeReferencedWithoutTargetNoHandler", Culture), new object[] { param0, param1 });
        }

        internal static string ActivityDelegateHandlersMustBeDeclarations(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityDelegateHandlersMustBeDeclarations", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ActivityDelegateNotOpened(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityDelegateNotOpened", Culture), new object[] { param0 });
        }

        internal static string ActivityDelegateOwnerEnvironmentMissing(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityDelegateOwnerEnvironmentMissing", Culture), new object[] { param0, param1 });
        }

        internal static string ActivityDelegateOwnerMissing(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityDelegateOwnerMissing", Culture), new object[] { param0 });
        }

        internal static string ActivityDelegateOwnerNotInParentScope(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityDelegateOwnerNotInParentScope", Culture), new object[] { param0, param1 });
        }

        internal static string ActivityNotPartOfThisTree(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityNotPartOfThisTree", Culture), new object[] { param0, param1 });
        }

        internal static string ActivityPropertyMustBeSet(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityPropertyMustBeSet", Culture), new object[] { param0, param1 });
        }

        internal static string ActivityPropertyNotSet(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityPropertyNotSet", Culture), new object[] { param0, param1 });
        }

        internal static string ActivityPropertyRequiresName(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityPropertyRequiresName", Culture), new object[] { param0 });
        }

        internal static string ActivityPropertyRequiresType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityPropertyRequiresType", Culture), new object[] { param0 });
        }

        internal static string ActivityTypeMismatch(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityTypeMismatch", Culture), new object[] { param0, param1 });
        }

        internal static string ActivityXamlServicesRequiresActivity(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ActivityXamlServicesRequiresActivity", Culture), new object[] { param0 });
        }

        internal static string AddValidationErrorMustBeCalledFromConstraint(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("AddValidationErrorMustBeCalledFromConstraint", Culture), new object[] { param0 });
        }

        internal static string AmbiguousVBVariableReference(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("AmbiguousVBVariableReference", Culture), new object[] { param0 });
        }

        internal static string ArgumentAlreadyInUse(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentAlreadyInUse", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ArgumentDirectionMismatch(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentDirectionMismatch", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ArgumentDoesNotExist(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentDoesNotExist", Culture), new object[] { param0 });
        }

        internal static string ArgumentDoesNotExistInEnvironment(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentDoesNotExistInEnvironment", Culture), new object[] { param0 });
        }

        internal static string ArgumentIsAddedMoreThanOnce(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentIsAddedMoreThanOnce", Culture), new object[] { param0, param1 });
        }

        internal static string ArgumentLocationExpressionTypeMismatch(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentLocationExpressionTypeMismatch", Culture), new object[] { param0, param1 });
        }

        internal static string ArgumentNotFound(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentNotFound", Culture), new object[] { param0 });
        }

        internal static string ArgumentNotInTree(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentNotInTree", Culture), new object[] { param0 });
        }

        internal static string ArgumentNumberRequiresTheSameAsParameterNumber(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentNumberRequiresTheSameAsParameterNumber", Culture), new object[] { param0 });
        }

        internal static string ArgumentRequired(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentRequired", Culture), new object[] { param0, param1 });
        }

        internal static string ArgumentTypeMismatch(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentTypeMismatch", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ArgumentTypeMustBeCompatible(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentTypeMustBeCompatible", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ArgumentValueExpressionTypeMismatch(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentValueExpressionTypeMismatch", Culture), new object[] { param0, param1 });
        }

        internal static string ArgumentViolationsFound(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentViolationsFound", Culture), new object[] { param0, param1 });
        }

        internal static string BinaryExpressionActivityRequiresArgument(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("BinaryExpressionActivityRequiresArgument", Culture), new object[] { param0, param1, param2 });
        }

        internal static string BookmarkAlreadyExists(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("BookmarkAlreadyExists", Culture), new object[] { param0 });
        }

        internal static string BookmarkNotRegistered(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("BookmarkNotRegistered", Culture), new object[] { param0 });
        }

        internal static string BookmarkScopeNotFound(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("BookmarkScopeNotFound", Culture), new object[] { param0 });
        }

        internal static string BookmarkScopeWithIdAlreadyExists(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("BookmarkScopeWithIdAlreadyExists", Culture), new object[] { param0 });
        }

        internal static string CallbackExceptionFromHostAbort(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CallbackExceptionFromHostAbort", Culture), new object[] { param0 });
        }

        internal static string CallbackExceptionFromHostGetExtension(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CallbackExceptionFromHostGetExtension", Culture), new object[] { param0 });
        }

        internal static string CancellationHandlerFatalException(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CancellationHandlerFatalException", Culture), new object[] { param0 });
        }

        internal static string CanInduceIdleActivityInArgumentExpression(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("CanInduceIdleActivityInArgumentExpression", Culture), new object[] { param0, param1, param2 });
        }

        internal static string CanInduceIdleNotSpecified(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CanInduceIdleNotSpecified", Culture), new object[] { param0 });
        }

        internal static string CannotDereferenceNull(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotDereferenceNull", Culture), new object[] { param0 });
        }

        internal static string CannotNestTransactionScopeWhenAmbientHandleIsSuppressed(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotNestTransactionScopeWhenAmbientHandleIsSuppressed", Culture), new object[] { param0 });
        }

        internal static string CannotPropagateExceptionWhileCanceling(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotPropagateExceptionWhileCanceling", Culture), new object[] { param0, param1 });
        }

        internal static string CannotSerializeExpression(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotSerializeExpression", Culture), new object[] { param0 });
        }

        internal static string CannotSetValueToLocation(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotSetValueToLocation", Culture), new object[] { param0, param1, param2 });
        }

        internal static string CannotValidateNullObject(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotValidateNullObject", Culture), new object[] { param0, param1 });
        }

        internal static string CanOnlyGetOwnedArguments(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("CanOnlyGetOwnedArguments", Culture), new object[] { param0, param1, param2 });
        }

        internal static string CanOnlyScheduleDirectChildren(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("CanOnlyScheduleDirectChildren", Culture), new object[] { param0, param1, param2 });
        }

        internal static string CatchOrFinallyExpected(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CatchOrFinallyExpected", Culture), new object[] { param0 });
        }

        internal static string CollectionActivityRequiresCollection(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CollectionActivityRequiresCollection", Culture), new object[] { param0 });
        }

        internal static string CompensateWithoutCompensableActivity(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CompensateWithoutCompensableActivity", Culture), new object[] { param0 });
        }

        internal static string CompensationHandlerFatalException(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CompensationHandlerFatalException", Culture), new object[] { param0 });
        }

        internal static string CompilerErrorSpecificExpression(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("CompilerErrorSpecificExpression", Culture), new object[] { param0, param1 });
        }

        internal static string CompletionConditionSetButNoBody(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CompletionConditionSetButNoBody", Culture), new object[] { param0 });
        }

        internal static string ConfirmationHandlerFatalException(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ConfirmationHandlerFatalException", Culture), new object[] { param0 });
        }

        internal static string ConfirmWithoutCompensableActivity(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ConfirmWithoutCompensableActivity", Culture), new object[] { param0 });
        }

        internal static string ConstructorInfoNotFound(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ConstructorInfoNotFound", Culture), new object[] { param0 });
        }

        internal static string ConvertVariableToValueExpressionFailed(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ConvertVariableToValueExpressionFailed", Culture), new object[] { param0, param1 });
        }

        internal static string DebugInfoCannotEvaluateExpression(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DebugInfoCannotEvaluateExpression", Culture), new object[] { param0 });
        }

        internal static string DebugInfoExceptionCaught(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("DebugInfoExceptionCaught", Culture), new object[] { param0, param1 });
        }

        internal static string DebugInstrumentationFailed(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DebugInstrumentationFailed", Culture), new object[] { param0 });
        }

        internal static string DelegateArgumentAlreadyInUseOnActivity(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("DelegateArgumentAlreadyInUseOnActivity", Culture), new object[] { param0, param1, param2 });
        }

        internal static string DelegateArgumentDoesNotExist(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DelegateArgumentDoesNotExist", Culture), new object[] { param0 });
        }

        internal static string DelegateArgumentMustBeReferenced(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DelegateArgumentMustBeReferenced", Culture), new object[] { param0 });
        }

        internal static string DelegateArgumentNotVisible(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DelegateArgumentNotVisible", Culture), new object[] { param0 });
        }

        internal static string DelegateArgumentTypeInvalid(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("DelegateArgumentTypeInvalid", Culture), new object[] { param0, param1, param2 });
        }

        internal static string DelegateHandlersCannotBeScheduledDirectly(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("DelegateHandlersCannotBeScheduledDirectly", Culture), new object[] { param0, param1 });
        }

        internal static string DelegateInArgumentTypeMismatch(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("DelegateInArgumentTypeMismatch", Culture), new object[] { param0, param1, param2 });
        }

        internal static string DelegateOutArgumentTypeMismatch(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("DelegateOutArgumentTypeMismatch", Culture), new object[] { param0, param1, param2 });
        }

        internal static string DelegateParameterCannotBeModifiedAfterOpen(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DelegateParameterCannotBeModifiedAfterOpen", Culture), new object[] { param0 });
        }

        internal static string DelegateParameterDirectionalityMismatch(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("DelegateParameterDirectionalityMismatch", Culture), new object[] { param0, param1, param2 });
        }

        internal static string DoNotSupportArrayIndexerOnNonArrayType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DoNotSupportArrayIndexerOnNonArrayType", Culture), new object[] { param0 });
        }

        internal static string DoNotSupportArrayIndexerReferenceWithDifferentArrayTypeAndResultType(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("DoNotSupportArrayIndexerReferenceWithDifferentArrayTypeAndResultType", Culture), new object[] { param0, param1 });
        }

        internal static string DoNotSupportArrayIndexerValueWithIncompatibleArrayTypeAndResultType(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("DoNotSupportArrayIndexerValueWithIncompatibleArrayTypeAndResultType", Culture), new object[] { param0, param1 });
        }

        internal static string DoNotSupportArrayIndexerWithDifferentArrayTypeAndResultType(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("DoNotSupportArrayIndexerWithDifferentArrayTypeAndResultType", Culture), new object[] { param0, param1 });
        }

        internal static string DoNotSupportArrayIndexerWithNonIntIndex(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DoNotSupportArrayIndexerWithNonIntIndex", Culture), new object[] { param0 });
        }

        internal static string DoWhileRequiresCondition(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DoWhileRequiresCondition", Culture), new object[] { param0 });
        }

        internal static string DuplicateAnnotationName(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DuplicateAnnotationName", Culture), new object[] { param0 });
        }

        internal static string DuplicateCatchClause(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DuplicateCatchClause", Culture), new object[] { param0 });
        }

        internal static string DuplicateEvaluationOrderValues(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("DuplicateEvaluationOrderValues", Culture), new object[] { param0, param1 });
        }

        internal static string DuplicateInstrumentation(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DuplicateInstrumentation", Culture), new object[] { param0 });
        }

        internal static string DuplicateMethodFound(object param0, object param1, object param2, object param3)
        {
            return string.Format(Culture, ResourceManager.GetString("DuplicateMethodFound", Culture), new object[] { param0, param1, param2, param3 });
        }

        internal static string DurationIsNegative(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DurationIsNegative", Culture), new object[] { param0 });
        }

        internal static string DynamicActivityDuplicatePropertyDetected(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DynamicActivityDuplicatePropertyDetected", Culture), new object[] { param0 });
        }

        internal static string EmptyIdReturnedFromHost(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("EmptyIdReturnedFromHost", Culture), new object[] { param0 });
        }

        internal static string ErrorExtractingValuesForLambdaRewrite(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ErrorExtractingValuesForLambdaRewrite", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ExecutionPropertyAlreadyDefined(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ExecutionPropertyAlreadyDefined", Culture), new object[] { param0 });
        }

        internal static string ExtraOverloadGroupPropertiesConfigured(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ExtraOverloadGroupPropertiesConfigured", Culture), new object[] { param0, param1, param2 });
        }

        internal static string FaultContextNotFound(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("FaultContextNotFound", Culture), new object[] { param0 });
        }

        internal static string FinalStateCannotHaveProperty(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("FinalStateCannotHaveProperty", Culture), new object[] { param0, param1 });
        }

        internal static string FinalStateCannotHaveTransition(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("FinalStateCannotHaveTransition", Culture), new object[] { param0 });
        }

        internal static string FlowchartMissingStartNode(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("FlowchartMissingStartNode", Culture), new object[] { param0 });
        }

        internal static string FlowDecisionRequiresCondition(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("FlowDecisionRequiresCondition", Culture), new object[] { param0 });
        }

        internal static string FlowNodeCannotBeShared(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("FlowNodeCannotBeShared", Culture), new object[] { param0, param1 });
        }

        internal static string FlowNodeLockedForRuntime(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("FlowNodeLockedForRuntime", Culture), new object[] { param0 });
        }

        internal static string FlowSwitchRequiresExpression(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("FlowSwitchRequiresExpression", Culture), new object[] { param0 });
        }

        internal static string ForEachRequiresNonNullValues(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ForEachRequiresNonNullValues", Culture), new object[] { param0 });
        }

        internal static string HostIdDoesNotMatchInstance(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("HostIdDoesNotMatchInstance", Culture), new object[] { param0, param1 });
        }

        internal static string IdNotFoundInWorkflow(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("IdNotFoundInWorkflow", Culture), new object[] { param0 });
        }

        internal static string IncompatibleTypeForMultidimensionalArrayItemReference(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("IncompatibleTypeForMultidimensionalArrayItemReference", Culture), new object[] { param0, param1 });
        }

        internal static string IncorrectIndexForArgument(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("IncorrectIndexForArgument", Culture), new object[] { param0, param1, param2 });
        }

        internal static string IndexOutOfBounds(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("IndexOutOfBounds", Culture), new object[] { param0, param1 });
        }

        internal static string IndicesAreNeeded(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("IndicesAreNeeded", Culture), new object[] { param0, param1 });
        }

        internal static string InitialStateCannotBeFinalState(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InitialStateCannotBeFinalState", Culture), new object[] { param0 });
        }

        internal static string InitialStateNotInStatesCollection(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InitialStateNotInStatesCollection", Culture), new object[] { param0 });
        }

        internal static string InlinedLocationReferenceOnlyAccessibleByOwner(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InlinedLocationReferenceOnlyAccessibleByOwner", Culture), new object[] { param0, param1 });
        }

        internal static string InputParametersCountMismatch(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InputParametersCountMismatch", Culture), new object[] { param0, param1 });
        }

        internal static string InputParametersMissing(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InputParametersMissing", Culture), new object[] { param0 });
        }

        internal static string InputParametersTypeMismatch(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InputParametersTypeMismatch", Culture), new object[] { param0, param1 });
        }

        internal static string InsufficientArraySize(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InsufficientArraySize", Culture), new object[] { param0 });
        }

        internal static string InternalConstraintException(object param0, object param1, object param2, object param3)
        {
            return string.Format(Culture, ResourceManager.GetString("InternalConstraintException", Culture), new object[] { param0, param1, param2, param3 });
        }

        internal static string InvalidArgumentExpression(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidArgumentExpression", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidAsyncBeginMethodSignature(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidAsyncBeginMethodSignature", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidAsyncCancelMethodSignature(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidAsyncCancelMethodSignature", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidAsyncEndMethodSignature(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidAsyncEndMethodSignature", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidCallbackState(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidCallbackState", Culture), new object[] { param0 });
        }

        internal static string InvalidCompensateActivityUsage(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidCompensateActivityUsage", Culture), new object[] { param0 });
        }

        internal static string InvalidCompensationToken(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidCompensationToken", Culture), new object[] { param0 });
        }

        internal static string InvalidConfirmActivityUsage(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidConfirmActivityUsage", Culture), new object[] { param0 });
        }

        internal static string InvalidDirectionForArgument(object param0, object param1, object param2, object param3)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidDirectionForArgument", Culture), new object[] { param0, param1, param2, param3 });
        }

        internal static string InvalidDynamicActivityProperty(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidDynamicActivityProperty", Culture), new object[] { param0 });
        }

        internal static string InvalidExecutionCallback(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidExecutionCallback", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidExpressionForLocation(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidExpressionForLocation", Culture), new object[] { param0 });
        }

        internal static string InvalidExpressionProperty(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidExpressionProperty", Culture), new object[] { param0 });
        }

        internal static string InvalidGenericTypeInfo(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidGenericTypeInfo", Culture), new object[] { param0 });
        }

        internal static string InvalidParameterInfo(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidParameterInfo", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidProperty(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidProperty", Culture), new object[] { param0 });
        }

        internal static string InvalidPropertyType(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidPropertyType", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidSourceLocationColumn(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidSourceLocationColumn", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidSourceLocationLineNumber(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidSourceLocationLineNumber", Culture), new object[] { param0, param1 });
        }

        internal static string InvalidTypeForArgument(object param0, object param1, object param2, object param3)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidTypeForArgument", Culture), new object[] { param0, param1, param2, param3 });
        }

        internal static string InvalidXamlMember(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidXamlMember", Culture), new object[] { param0 });
        }

        internal static string LiteralsMustBeValueTypesOrImmutableTypes(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("LiteralsMustBeValueTypesOrImmutableTypes", Culture), new object[] { param0, param1 });
        }

        internal static string LocationExpressionCouldNotBeResolved(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("LocationExpressionCouldNotBeResolved", Culture), new object[] { param0 });
        }

        internal static string LocationTypeMismatch(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("LocationTypeMismatch", Culture), new object[] { param0, param1, param2 });
        }

        internal static string MemberCannotBeNull(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("MemberCannotBeNull", Culture), new object[] { param0, param1, param2 });
        }

        internal static string MemberIsReadOnly(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("MemberIsReadOnly", Culture), new object[] { param0, param1 });
        }

        internal static string MemberNotFound(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("MemberNotFound", Culture), new object[] { param0, param1 });
        }

        internal static string MemberNotSupportedByActivityXamlServices(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("MemberNotSupportedByActivityXamlServices", Culture), new object[] { param0 });
        }

        internal static string MethodInfoRequired(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("MethodInfoRequired", Culture), new object[] { param0 });
        }

        internal static string MethodNameRequired(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("MethodNameRequired", Culture), new object[] { param0 });
        }

        internal static string MissingArgument(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("MissingArgument", Culture), new object[] { param0, param1 });
        }

        internal static string MissingNameProperty(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("MissingNameProperty", Culture), new object[] { param0 });
        }

        internal static string MissingSetAccessorForIndexer(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("MissingSetAccessorForIndexer", Culture), new object[] { param0, param1 });
        }

        internal static string MultipleOverloadGroupsConfigured(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("MultipleOverloadGroupsConfigured", Culture), new object[] { param0 });
        }

        internal static string NoNamespace(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("NoNamespace", Culture), new object[] { param0 });
        }

        internal static string NoOutputLocationWasFound(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("NoOutputLocationWasFound", Culture), new object[] { param0 });
        }

        internal static string NullReferencedMemberAccess(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("NullReferencedMemberAccess", Culture), new object[] { param0, param1 });
        }

        internal static string OneOfTwoPropertiesMustBeSet(object param0, object param1, object param2, object param3)
        {
            return string.Format(Culture, ResourceManager.GetString("OneOfTwoPropertiesMustBeSet", Culture), new object[] { param0, param1, param2, param3 });
        }

        internal static string OptionalExtensionTypeMatchedMultiple(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("OptionalExtensionTypeMatchedMultiple", Culture), new object[] { param0 });
        }

        internal static string OutArgumentCannotHaveInputValue(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("OutArgumentCannotHaveInputValue", Culture), new object[] { param0 });
        }

        internal static string OutOfRangeSourceLocationEndColumn(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("OutOfRangeSourceLocationEndColumn", Culture), new object[] { param0 });
        }

        internal static string OutOfRangeSourceLocationEndLine(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("OutOfRangeSourceLocationEndLine", Culture), new object[] { param0 });
        }

        internal static string OverloadGroupHasSubsets(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("OverloadGroupHasSubsets", Culture), new object[] { param0, param1 });
        }

        internal static string OverloadGroupsAreEquivalent(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("OverloadGroupsAreEquivalent", Culture), new object[] { param0 });
        }

        internal static string ParallelForEachRequiresNonNullValues(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ParallelForEachRequiresNonNullValues", Culture), new object[] { param0 });
        }

        internal static string PickBranchRequiresTrigger(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("PickBranchRequiresTrigger", Culture), new object[] { param0 });
        }

        internal static string PrivateActionsShouldNotChange(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("PrivateActionsShouldNotChange", Culture), new object[] { param0 });
        }

        internal static string PrivateChildrenShouldNotChange(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("PrivateChildrenShouldNotChange", Culture), new object[] { param0 });
        }

        internal static string PrivateVariablesShouldNotChange(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("PrivateVariablesShouldNotChange", Culture), new object[] { param0 });
        }

        internal static string PropertyCannotBeModified(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("PropertyCannotBeModified", Culture), new object[] { param0 });
        }

        internal static string PropertyMemberNotSupportedByActivityXamlServices(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("PropertyMemberNotSupportedByActivityXamlServices", Culture), new object[] { param0 });
        }

        internal static string PropertyReadOnlyInWorkflowDataContext(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("PropertyReadOnlyInWorkflowDataContext", Culture), new object[] { param0 });
        }

        internal static string PropertyReferenceNotFound(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("PropertyReferenceNotFound", Culture), new object[] { param0 });
        }

        internal static string PublicMethodWithMatchingParameterDoesNotExist(object param0, object param1, object param2, object param3)
        {
            return string.Format(Culture, ResourceManager.GetString("PublicMethodWithMatchingParameterDoesNotExist", Culture), new object[] { param0, param1, param2, param3 });
        }

        internal static string ReadonlyPropertyCannotBeSet(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ReadonlyPropertyCannotBeSet", Culture), new object[] { param0, param1 });
        }

        internal static string RequiredArgumentValueNotSupplied(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("RequiredArgumentValueNotSupplied", Culture), new object[] { param0 });
        }

        internal static string RequiredExtensionTypeNotFound(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("RequiredExtensionTypeNotFound", Culture), new object[] { param0 });
        }

        internal static string RequiredVariableCoundNotBeExtracted(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("RequiredVariableCoundNotBeExtracted", Culture), new object[] { param0, param1 });
        }

        internal static string RequireExtensionOnlyAcceptsReferenceTypes(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("RequireExtensionOnlyAcceptsReferenceTypes", Culture), new object[] { param0 });
        }

        internal static string ResultArgumentHasRequiredTypeAndDirection(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("ResultArgumentHasRequiredTypeAndDirection", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ResultArgumentMustBeSpecificType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ResultArgumentMustBeSpecificType", Culture), new object[] { param0 });
        }

        internal static string RethrowMustBeAPublicChild(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("RethrowMustBeAPublicChild", Culture), new object[] { param0 });
        }

        internal static string RethrowNotInATryCatch(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("RethrowNotInATryCatch", Culture), new object[] { param0 });
        }

        internal static string ReturnTypeIncompatible(object param0, object param1, object param2, object param3, object param4)
        {
            return string.Format(Culture, ResourceManager.GetString("ReturnTypeIncompatible", Culture), new object[] { param0, param1, param2, param3, param4 });
        }

        internal static string RootActivityAlreadyAssociatedWithInstance(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("RootActivityAlreadyAssociatedWithInstance", Culture), new object[] { param0 });
        }

        internal static string RootActivityCannotBeReferenced(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("RootActivityCannotBeReferenced", Culture), new object[] { param0, param1 });
        }

        internal static string RuntimeArgumentBindingInvalid(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("RuntimeArgumentBindingInvalid", Culture), new object[] { param0, param1 });
        }

        internal static string RuntimeArgumentNotOpen(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("RuntimeArgumentNotOpen", Culture), new object[] { param0 });
        }

        internal static string RuntimeTransactionHandleNotRegisteredAsExecutionProperty(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("RuntimeTransactionHandleNotRegisteredAsExecutionProperty", Culture), new object[] { param0 });
        }

        internal static string SimpleStateMustHaveOneTransition(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("SimpleStateMustHaveOneTransition", Culture), new object[] { param0 });
        }

        internal static string SpecialMethodNotFound(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("SpecialMethodNotFound", Culture), new object[] { param0, param1 });
        }

        internal static string StateCannotBeAddedTwice(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("StateCannotBeAddedTwice", Culture), new object[] { param0 });
        }

        internal static string StateMachineMustHaveInitialState(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("StateMachineMustHaveInitialState", Culture), new object[] { param0 });
        }

        internal static string StateNotBelongToAnyParent(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("StateNotBelongToAnyParent", Culture), new object[] { param0, param1 });
        }

        internal static string SubexpressionResultWasNotVisible(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("SubexpressionResultWasNotVisible", Culture), new object[] { param0 });
        }

        internal static string SubexpressionResultWasNull(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("SubexpressionResultWasNull", Culture), new object[] { param0 });
        }

        internal static string SwitchCaseKeyTypesMustMatchExpressionType(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("SwitchCaseKeyTypesMustMatchExpressionType", Culture), new object[] { param0, param1, param2 });
        }

        internal static string SwitchCaseNullWithValueType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("SwitchCaseNullWithValueType", Culture), new object[] { param0 });
        }

        internal static string SwitchCaseTypeMismatch(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("SwitchCaseTypeMismatch", Culture), new object[] { param0, param1 });
        }

        internal static string SymbolNamesMustBeUnique(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("SymbolNamesMustBeUnique", Culture), new object[] { param0 });
        }

        internal static string SymbolResolverDoesNotHaveSymbol(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("SymbolResolverDoesNotHaveSymbol", Culture), new object[] { param0, param1 });
        }

        internal static string TargetTypeAndTargetObjectAreMutuallyExclusive(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("TargetTypeAndTargetObjectAreMutuallyExclusive", Culture), new object[] { param0, param1 });
        }

        internal static string TargetTypeCannotBeEnum(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("TargetTypeCannotBeEnum", Culture), new object[] { param0, param1 });
        }

        internal static string TargetTypeIsValueType(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("TargetTypeIsValueType", Culture), new object[] { param0, param1 });
        }

        internal static string TimeoutOnOperation(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TimeoutOnOperation", Culture), new object[] { param0 });
        }

        internal static string TransitionCannotBeAddedTwice(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("TransitionCannotBeAddedTwice", Culture), new object[] { param0, param1, param2 });
        }

        internal static string TransitionTargetCannotBeNull(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("TransitionTargetCannotBeNull", Culture), new object[] { param0, param1 });
        }

        internal static string TypeConverterHelperCacheAddFailed(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TypeConverterHelperCacheAddFailed", Culture), new object[] { param0 });
        }

        internal static string TypeMismatchForAssign(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("TypeMismatchForAssign", Culture), new object[] { param0, param1, param2 });
        }

        internal static string TypeMustbeValueType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TypeMustbeValueType", Culture), new object[] { param0 });
        }

        internal static string TypeNotAssignableTo(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("TypeNotAssignableTo", Culture), new object[] { param0, param1 });
        }

        internal static string UnconditionalTransitionShouldNotShareNullTriggersWithOthers(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("UnconditionalTransitionShouldNotShareNullTriggersWithOthers", Culture), new object[] { param0, param1 });
        }

        internal static string UnconditionalTransitionShouldNotShareTriggersWithOthers(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("UnconditionalTransitionShouldNotShareTriggersWithOthers", Culture), new object[] { param0, param1, param2 });
        }

        internal static string UnexpectedArgumentCount(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("UnexpectedArgumentCount", Culture), new object[] { param0, param1, param2 });
        }

        internal static string UnknownExpressionCompilationError(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("UnknownExpressionCompilationError", Culture), new object[] { param0 });
        }

        internal static string UnknownLanguage(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("UnknownLanguage", Culture), new object[] { param0 });
        }

        internal static string UnopenedActivitiesCannotBeExecuted(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("UnopenedActivitiesCannotBeExecuted", Culture), new object[] { param0 });
        }

        internal static string UnsupportedExpressionType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("UnsupportedExpressionType", Culture), new object[] { param0 });
        }

        internal static string UnsupportedMemberExpressionWithType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("UnsupportedMemberExpressionWithType", Culture), new object[] { param0 });
        }

        internal static string UnsupportedReferenceExpressionType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("UnsupportedReferenceExpressionType", Culture), new object[] { param0 });
        }

        internal static string UnusedInputArguments(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("UnusedInputArguments", Culture), new object[] { param0 });
        }

        internal static string ValidationContextCannotBeNull(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ValidationContextCannotBeNull", Culture), new object[] { param0, param1 });
        }

        internal static string ValidationErrorPrefixForHiddenActivity(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ValidationErrorPrefixForHiddenActivity", Culture), new object[] { param0 });
        }

        internal static string ValidationErrorPrefixForPublicActivityWithHiddenParent(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ValidationErrorPrefixForPublicActivityWithHiddenParent", Culture), new object[] { param0, param1 });
        }

        internal static string VariableAlreadyInUseOnActivity(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableAlreadyInUseOnActivity", Culture), new object[] { param0, param1, param2 });
        }

        internal static string VariableCannotBePopulatedInLocationEnvironment(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableCannotBePopulatedInLocationEnvironment", Culture), new object[] { param0 });
        }

        internal static string VariableDoesNotExist(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableDoesNotExist", Culture), new object[] { param0 });
        }

        internal static string VariableExpressionTypeMismatch(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableExpressionTypeMismatch", Culture), new object[] { param0, param1, param2 });
        }

        internal static string VariableIsReadOnly(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableIsReadOnly", Culture), new object[] { param0 });
        }

        internal static string VariableNameNotAnIdentifier(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableNameNotAnIdentifier", Culture), new object[] { param0 });
        }

        internal static string VariableNotOpen(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableNotOpen", Culture), new object[] { param0, param1 });
        }

        internal static string VariableNotVisible(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableNotVisible", Culture), new object[] { param0 });
        }

        internal static string VariableOnlyAccessibleAtScopeOfDeclaration(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableOnlyAccessibleAtScopeOfDeclaration", Culture), new object[] { param0, param1 });
        }

        internal static string VariableOrArgumentDoesNotExist(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableOrArgumentDoesNotExist", Culture), new object[] { param0 });
        }

        internal static string VariableShouldBeOpen(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableShouldBeOpen", Culture), new object[] { param0 });
        }

        internal static string VariableTypeInvalid(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableTypeInvalid", Culture), new object[] { param0, param1, param2 });
        }

        internal static string VariableTypeNotMatchLocationType(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("VariableTypeNotMatchLocationType", Culture), new object[] { param0, param1 });
        }

        internal static string WhileRequiresCondition(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WhileRequiresCondition", Culture), new object[] { param0 });
        }

        internal static string WorkflowAbortedReason(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowAbortedReason", Culture), new object[] { param0, param1 });
        }

        internal static string WorkflowApplicationAborted(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowApplicationAborted", Culture), new object[] { param0 });
        }

        internal static string WorkflowApplicationCompleted(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowApplicationCompleted", Culture), new object[] { param0 });
        }

        internal static string WorkflowApplicationTerminated(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowApplicationTerminated", Culture), new object[] { param0 });
        }

        internal static string WorkflowApplicationUnloaded(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowApplicationUnloaded", Culture), new object[] { param0 });
        }

        internal static string WorkflowInstanceAborted(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowInstanceAborted", Culture), new object[] { param0 });
        }

        internal static string WorkflowInstanceIsReadOnly(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowInstanceIsReadOnly", Culture), new object[] { param0 });
        }

        internal static string WorkflowInstanceNotFoundInStore(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowInstanceNotFoundInStore", Culture), new object[] { param0 });
        }

        internal static string WorkflowInstanceUnlocked(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowInstanceUnlocked", Culture), new object[] { param0 });
        }

        internal static string WriteonlyPropertyCannotBeRead(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("WriteonlyPropertyCannotBeRead", Culture), new object[] { param0, param1 });
        }

        internal static string WrongArgumentType(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("WrongArgumentType", Culture), new object[] { param0, param1 });
        }

        internal static string XamlElementExpectedAt(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("XamlElementExpectedAt", Culture), new object[] { param0, param1 });
        }

        internal static string AbortingDueToInstanceTimeout
        {
            get
            {
                return ResourceManager.GetString("AbortingDueToInstanceTimeout", Culture);
            }
        }

        internal static string AbortingDueToLoadFailure
        {
            get
            {
                return ResourceManager.GetString("AbortingDueToLoadFailure", Culture);
            }
        }

        internal static string AbortInstanceOnTransactionFailureDoesNotMatch
        {
            get
            {
                return ResourceManager.GetString("AbortInstanceOnTransactionFailureDoesNotMatch", Culture);
            }
        }

        internal static string ActivityFailedToOpenBefore
        {
            get
            {
                return ResourceManager.GetString("ActivityFailedToOpenBefore", Culture);
            }
        }

        internal static string ActivityInstanceFixupFailed
        {
            get
            {
                return ResourceManager.GetString("ActivityInstanceFixupFailed", Culture);
            }
        }

        internal static string ActivityMapIsCorrupt
        {
            get
            {
                return ResourceManager.GetString("ActivityMapIsCorrupt", Culture);
            }
        }

        internal static string AECDisposed
        {
            get
            {
                return ResourceManager.GetString("AECDisposed", Culture);
            }
        }

        internal static string AECForPropertiesHasBeenDisposed
        {
            get
            {
                return ResourceManager.GetString("AECForPropertiesHasBeenDisposed", Culture);
            }
        }

        internal static string AlreadySetupNoPersist
        {
            get
            {
                return ResourceManager.GetString("AlreadySetupNoPersist", Culture);
            }
        }

        internal static string ArgumentMustbePropertyofWorkflowElement
        {
            get
            {
                return ResourceManager.GetString("ArgumentMustbePropertyofWorkflowElement", Culture);
            }
        }

        internal static string ArgumentNameRequired
        {
            get
            {
                return ResourceManager.GetString("ArgumentNameRequired", Culture);
            }
        }

        internal static string ArgumentsChangedUpdateError
        {
            get
            {
                return ResourceManager.GetString("ArgumentsChangedUpdateError", Culture);
            }
        }

        internal static string ArgumentTypeCannotBeNull
        {
            get
            {
                return ResourceManager.GetString("ArgumentTypeCannotBeNull", Culture);
            }
        }

        internal static string AsyncMethodsMustAllBeStaticOrInstance
        {
            get
            {
                return ResourceManager.GetString("AsyncMethodsMustAllBeStaticOrInstance", Culture);
            }
        }

        internal static string AsyncMethodsMustFromSameType
        {
            get
            {
                return ResourceManager.GetString("AsyncMethodsMustFromSameType", Culture);
            }
        }

        internal static string BadCopyToArray
        {
            get
            {
                return ResourceManager.GetString("BadCopyToArray", Culture);
            }
        }

        internal static string BeginExecuteMustNotReturnANullAsyncResult
        {
            get
            {
                return ResourceManager.GetString("BeginExecuteMustNotReturnANullAsyncResult", Culture);
            }
        }

        internal static string BeginExecuteMustUseProvidedStateAsAsyncResultState
        {
            get
            {
                return ResourceManager.GetString("BeginExecuteMustUseProvidedStateAsAsyncResultState", Culture);
            }
        }

        internal static string BookmarkNotFoundGeneric
        {
            get
            {
                return ResourceManager.GetString("BookmarkNotFoundGeneric", Culture);
            }
        }

        internal static string BookmarkScopeAlreadyInitialized
        {
            get
            {
                return ResourceManager.GetString("BookmarkScopeAlreadyInitialized", Culture);
            }
        }

        internal static string BookmarkScopeHasBookmarks
        {
            get
            {
                return ResourceManager.GetString("BookmarkScopeHasBookmarks", Culture);
            }
        }

        internal static string BookmarkScopeNotRegisteredForInitialize
        {
            get
            {
                return ResourceManager.GetString("BookmarkScopeNotRegisteredForInitialize", Culture);
            }
        }

        internal static string BookmarkScopeNotRegisteredForUnregister
        {
            get
            {
                return ResourceManager.GetString("BookmarkScopeNotRegisteredForUnregister", Culture);
            }
        }

        internal static string BookmarkScopesRequireKeys
        {
            get
            {
                return ResourceManager.GetString("BookmarkScopesRequireKeys", Culture);
            }
        }

        internal static string BookmarksOnlyResumableWhileIdle
        {
            get
            {
                return ResourceManager.GetString("BookmarksOnlyResumableWhileIdle", Culture);
            }
        }

        internal static string CannotAddHandlesUpdateError
        {
            get
            {
                return ResourceManager.GetString("CannotAddHandlesUpdateError", Culture);
            }
        }

        internal static string CannotAddOrRemoveWithChildren
        {
            get
            {
                return ResourceManager.GetString("CannotAddOrRemoveWithChildren", Culture);
            }
        }

        internal static string CannotCallAbortInstanceFromWorkflowThread
        {
            get
            {
                return ResourceManager.GetString("CannotCallAbortInstanceFromWorkflowThread", Culture);
            }
        }

        internal static string CannotChangeAbortInstanceFlagAfterPropertyRegistration
        {
            get
            {
                return ResourceManager.GetString("CannotChangeAbortInstanceFlagAfterPropertyRegistration", Culture);
            }
        }

        internal static string CannotCompleteRuntimeOwnedTransaction
        {
            get
            {
                return ResourceManager.GetString("CannotCompleteRuntimeOwnedTransaction", Culture);
            }
        }

        internal static string CannotCreateEnvironmentUpdateError
        {
            get
            {
                return ResourceManager.GetString("CannotCreateEnvironmentUpdateError", Culture);
            }
        }

        internal static string CannotEnlistMultipleTransactions
        {
            get
            {
                return ResourceManager.GetString("CannotEnlistMultipleTransactions", Culture);
            }
        }

        internal static string CanNotFindSymbolResolverInWorkflowInstanceExtensions
        {
            get
            {
                return ResourceManager.GetString("CanNotFindSymbolResolverInWorkflowInstanceExtensions", Culture);
            }
        }

        internal static string CannotGetValueOfOutArgument
        {
            get
            {
                return ResourceManager.GetString("CannotGetValueOfOutArgument", Culture);
            }
        }

        internal static string CannotInvokeOpenedActivity
        {
            get
            {
                return ResourceManager.GetString("CannotInvokeOpenedActivity", Culture);
            }
        }

        internal static string CannotModifyCatchAfterOpen
        {
            get
            {
                return ResourceManager.GetString("CannotModifyCatchAfterOpen", Culture);
            }
        }

        internal static string CannotPerformOperationFromHandlerThread
        {
            get
            {
                return ResourceManager.GetString("CannotPerformOperationFromHandlerThread", Culture);
            }
        }

        internal static string CannotPerformOperationOnHandle
        {
            get
            {
                return ResourceManager.GetString("CannotPerformOperationOnHandle", Culture);
            }
        }

        internal static string CannotPersistInsideIsolation
        {
            get
            {
                return ResourceManager.GetString("CannotPersistInsideIsolation", Culture);
            }
        }

        internal static string CannotPersistInsideNoPersist
        {
            get
            {
                return ResourceManager.GetString("CannotPersistInsideNoPersist", Culture);
            }
        }

        internal static string CannotPersistWhileDetached
        {
            get
            {
                return ResourceManager.GetString("CannotPersistWhileDetached", Culture);
            }
        }

        internal static string CannotRemoveExecutingActivityUpdateError
        {
            get
            {
                return ResourceManager.GetString("CannotRemoveExecutingActivityUpdateError", Culture);
            }
        }

        internal static string CannotResetPropertyInDataContext
        {
            get
            {
                return ResourceManager.GetString("CannotResetPropertyInDataContext", Culture);
            }
        }

        internal static string CannotScheduleChildrenWhileEnteringIsolation
        {
            get
            {
                return ResourceManager.GetString("CannotScheduleChildrenWhileEnteringIsolation", Culture);
            }
        }

        internal static string CannotSerializeVariableExpression
        {
            get
            {
                return ResourceManager.GetString("CannotSerializeVariableExpression", Culture);
            }
        }

        internal static string CannotSetRuntimeTransactionInNoPersist
        {
            get
            {
                return ResourceManager.GetString("CannotSetRuntimeTransactionInNoPersist", Culture);
            }
        }

        internal static string CannotSetupIsolationInsideIsolation
        {
            get
            {
                return ResourceManager.GetString("CannotSetupIsolationInsideIsolation", Culture);
            }
        }

        internal static string CannotSetupIsolationInsideNoPersist
        {
            get
            {
                return ResourceManager.GetString("CannotSetupIsolationInsideNoPersist", Culture);
            }
        }

        internal static string CannotSetupIsolationWithChildren
        {
            get
            {
                return ResourceManager.GetString("CannotSetupIsolationWithChildren", Culture);
            }
        }

        internal static string CannotSetValueOfInArgument
        {
            get
            {
                return ResourceManager.GetString("CannotSetValueOfInArgument", Culture);
            }
        }

        internal static string CannotSuppressAlreadyRegisteredHandle
        {
            get
            {
                return ResourceManager.GetString("CannotSuppressAlreadyRegisteredHandle", Culture);
            }
        }

        internal static string CannotUnregisterDefaultBookmarkScope
        {
            get
            {
                return ResourceManager.GetString("CannotUnregisterDefaultBookmarkScope", Culture);
            }
        }

        internal static string CannotUnregisterNullBookmarkScope
        {
            get
            {
                return ResourceManager.GetString("CannotUnregisterNullBookmarkScope", Culture);
            }
        }

        internal static string CannotUseInputsWithLoad
        {
            get
            {
                return ResourceManager.GetString("CannotUseInputsWithLoad", Culture);
            }
        }

        internal static string CannotWaitForIdleSynchronously
        {
            get
            {
                return ResourceManager.GetString("CannotWaitForIdleSynchronously", Culture);
            }
        }

        internal static string CanOnlyAbortDirectChildren
        {
            get
            {
                return ResourceManager.GetString("CanOnlyAbortDirectChildren", Culture);
            }
        }

        internal static string CanOnlyCancelDirectChildren
        {
            get
            {
                return ResourceManager.GetString("CanOnlyCancelDirectChildren", Culture);
            }
        }

        internal static string CantFindTimerExtension
        {
            get
            {
                return ResourceManager.GetString("CantFindTimerExtension", Culture);
            }
        }

        internal static string CompensableActivityAlreadyConfirmedOrCompensated
        {
            get
            {
                return ResourceManager.GetString("CompensableActivityAlreadyConfirmedOrCompensated", Culture);
            }
        }

        internal static string CompensableActivityInsideTransactionScopeActivity
        {
            get
            {
                return ResourceManager.GetString("CompensableActivityInsideTransactionScopeActivity", Culture);
            }
        }

        internal static string CompensateWithNoTargetConstraint
        {
            get
            {
                return ResourceManager.GetString("CompensateWithNoTargetConstraint", Culture);
            }
        }

        internal static string CompilerError
        {
            get
            {
                return ResourceManager.GetString("CompilerError", Culture);
            }
        }

        internal static string ConfirmWithNoTargetConstraint
        {
            get
            {
                return ResourceManager.GetString("ConfirmWithNoTargetConstraint", Culture);
            }
        }

        internal static string ConstVariableCannotBeSet
        {
            get
            {
                return ResourceManager.GetString("ConstVariableCannotBeSet", Culture);
            }
        }

        internal static string ControllerInvalidBeforeInitialize
        {
            get
            {
                return ResourceManager.GetString("ControllerInvalidBeforeInitialize", Culture);
            }
        }

        internal static string CopyToIndexOutOfRange
        {
            get
            {
                return ResourceManager.GetString("CopyToIndexOutOfRange", Culture);
            }
        }

        internal static string CopyToNotEnoughSpaceInArray
        {
            get
            {
                return ResourceManager.GetString("CopyToNotEnoughSpaceInArray", Culture);
            }
        }

        internal static string CopyToRankMustBeOne
        {
            get
            {
                return ResourceManager.GetString("CopyToRankMustBeOne", Culture);
            }
        }

        internal static string CreateBookmarkScopeFailed
        {
            get
            {
                return ResourceManager.GetString("CreateBookmarkScopeFailed", Culture);
            }
        }

        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        internal static string DebugInfoNoLambda
        {
            get
            {
                return ResourceManager.GetString("DebugInfoNoLambda", Culture);
            }
        }

        internal static string DebugInfoNotAnIExpressionContainer
        {
            get
            {
                return ResourceManager.GetString("DebugInfoNotAnIExpressionContainer", Culture);
            }
        }

        internal static string DebugInfoTryGetValueFailed
        {
            get
            {
                return ResourceManager.GetString("DebugInfoTryGetValueFailed", Culture);
            }
        }

        internal static string DefaultAbortReason
        {
            get
            {
                return ResourceManager.GetString("DefaultAbortReason", Culture);
            }
        }

        internal static string DefaultCancelationRequiresCancelHasBeenRequested
        {
            get
            {
                return ResourceManager.GetString("DefaultCancelationRequiresCancelHasBeenRequested", Culture);
            }
        }

        internal static string DefaultInvalidWorkflowExceptionMessage
        {
            get
            {
                return ResourceManager.GetString("DefaultInvalidWorkflowExceptionMessage", Culture);
            }
        }

        internal static string DefaultWorkflowApplicationExceptionMessage
        {
            get
            {
                return ResourceManager.GetString("DefaultWorkflowApplicationExceptionMessage", Culture);
            }
        }

        internal static string DelegateArgumentMustBeSet
        {
            get
            {
                return ResourceManager.GetString("DelegateArgumentMustBeSet", Culture);
            }
        }

        internal static string DictionaryIsReadOnly
        {
            get
            {
                return ResourceManager.GetString("DictionaryIsReadOnly", Culture);
            }
        }

        internal static string DirectLambdaParameterReference
        {
            get
            {
                return ResourceManager.GetString("DirectLambdaParameterReference", Culture);
            }
        }

        internal static string EmptyGuidOnDeserializedInstance
        {
            get
            {
                return ResourceManager.GetString("EmptyGuidOnDeserializedInstance", Culture);
            }
        }

        internal static string EnlistedTransactionPropertiesRequireIsolationBlocks
        {
            get
            {
                return ResourceManager.GetString("EnlistedTransactionPropertiesRequireIsolationBlocks", Culture);
            }
        }

        internal static string EnumeratorNotStarted
        {
            get
            {
                return ResourceManager.GetString("EnumeratorNotStarted", Culture);
            }
        }

        internal static string EnvironmentDisposed
        {
            get
            {
                return ResourceManager.GetString("EnvironmentDisposed", Culture);
            }
        }

        internal static string ErrorsEncounteredWhileProcessingTree
        {
            get
            {
                return ResourceManager.GetString("ErrorsEncounteredWhileProcessingTree", Culture);
            }
        }

        internal static string ExclusiveHandleRegisterBookmarkScopeFailed
        {
            get
            {
                return ResourceManager.GetString("ExclusiveHandleRegisterBookmarkScopeFailed", Culture);
            }
        }

        internal static string ExclusiveHandleReinitializeFailed
        {
            get
            {
                return ResourceManager.GetString("ExclusiveHandleReinitializeFailed", Culture);
            }
        }

        internal static string ExpressionRequiredForConversion
        {
            get
            {
                return ResourceManager.GetString("ExpressionRequiredForConversion", Culture);
            }
        }

        internal static string ExtensionsCannotBeModified
        {
            get
            {
                return ResourceManager.GetString("ExtensionsCannotBeModified", Culture);
            }
        }

        internal static string ExternalLocationsGetOnly
        {
            get
            {
                return ResourceManager.GetString("ExternalLocationsGetOnly", Culture);
            }
        }

        internal static string HandleInitializationContextDisposed
        {
            get
            {
                return ResourceManager.GetString("HandleInitializationContextDisposed", Culture);
            }
        }

        internal static string HandleNotInitialized
        {
            get
            {
                return ResourceManager.GetString("HandleNotInitialized", Culture);
            }
        }

        internal static string HasExecutingChildrenNoPersist
        {
            get
            {
                return ResourceManager.GetString("HasExecutingChildrenNoPersist", Culture);
            }
        }

        internal static string InitializationIncomplete
        {
            get
            {
                return ResourceManager.GetString("InitializationIncomplete", Culture);
            }
        }

        internal static string InstanceMethodCallRequiresTargetObject
        {
            get
            {
                return ResourceManager.GetString("InstanceMethodCallRequiresTargetObject", Culture);
            }
        }

        internal static string InstanceMustBePaused
        {
            get
            {
                return ResourceManager.GetString("InstanceMustBePaused", Culture);
            }
        }

        internal static string InstanceMustNotBePaused
        {
            get
            {
                return ResourceManager.GetString("InstanceMustNotBePaused", Culture);
            }
        }

        internal static string InstanceStoreFailed
        {
            get
            {
                return ResourceManager.GetString("InstanceStoreFailed", Culture);
            }
        }

        internal static string InstanceStoreRequiredToPersist
        {
            get
            {
                return ResourceManager.GetString("InstanceStoreRequiredToPersist", Culture);
            }
        }

        internal static string InvalidActivityIdFormat
        {
            get
            {
                return ResourceManager.GetString("InvalidActivityIdFormat", Culture);
            }
        }

        internal static string InvalidEvaluationOrderValue
        {
            get
            {
                return ResourceManager.GetString("InvalidEvaluationOrderValue", Culture);
            }
        }

        internal static string InvalidIdleAction
        {
            get
            {
                return ResourceManager.GetString("InvalidIdleAction", Culture);
            }
        }

        internal static string InvalidLocationExpression
        {
            get
            {
                return ResourceManager.GetString("InvalidLocationExpression", Culture);
            }
        }

        internal static string InvalidLValueExpression
        {
            get
            {
                return ResourceManager.GetString("InvalidLValueExpression", Culture);
            }
        }

        internal static string InvalidRuntimeState
        {
            get
            {
                return ResourceManager.GetString("InvalidRuntimeState", Culture);
            }
        }

        internal static string InvalidStateForAsyncCallback
        {
            get
            {
                return ResourceManager.GetString("InvalidStateForAsyncCallback", Culture);
            }
        }

        internal static string InvalidTypeConverterUsage
        {
            get
            {
                return ResourceManager.GetString("InvalidTypeConverterUsage", Culture);
            }
        }

        internal static string InvalidUnhandledExceptionAction
        {
            get
            {
                return ResourceManager.GetString("InvalidUnhandledExceptionAction", Culture);
            }
        }

        internal static string InvalidVisualBasicSettingsValue
        {
            get
            {
                return ResourceManager.GetString("InvalidVisualBasicSettingsValue", Culture);
            }
        }

        internal static string IsolationLevelValidation
        {
            get
            {
                return ResourceManager.GetString("IsolationLevelValidation", Culture);
            }
        }

        internal static string KeyCollectionUpdatesNotAllowed
        {
            get
            {
                return ResourceManager.GetString("KeyCollectionUpdatesNotAllowed", Culture);
            }
        }

        internal static string LambdaNotXamlSerializable
        {
            get
            {
                return ResourceManager.GetString("LambdaNotXamlSerializable", Culture);
            }
        }

        internal static string LoadingWorkflowApplicationRequiresInstanceStore
        {
            get
            {
                return ResourceManager.GetString("LoadingWorkflowApplicationRequiresInstanceStore", Culture);
            }
        }

        internal static string MarkCanceledOnlyCallableIfCancelRequested
        {
            get
            {
                return ResourceManager.GetString("MarkCanceledOnlyCallableIfCancelRequested", Culture);
            }
        }

        internal static string MultiDimensionalArraysNotSupported
        {
            get
            {
                return ResourceManager.GetString("MultiDimensionalArraysNotSupported", Culture);
            }
        }

        internal static string MustMatchReferenceExpressionReturnType
        {
            get
            {
                return ResourceManager.GetString("MustMatchReferenceExpressionReturnType", Culture);
            }
        }

        internal static string NewArrayBoundsRequiresIntegralArguments
        {
            get
            {
                return ResourceManager.GetString("NewArrayBoundsRequiresIntegralArguments", Culture);
            }
        }

        internal static string NewArrayRequiresArrayTypeAsResultType
        {
            get
            {
                return ResourceManager.GetString("NewArrayRequiresArrayTypeAsResultType", Culture);
            }
        }

        internal static string NoCAInSecondaryRoot
        {
            get
            {
                return ResourceManager.GetString("NoCAInSecondaryRoot", Culture);
            }
        }

        internal static string NoOverloadGroupsAreConfigured
        {
            get
            {
                return ResourceManager.GetString("NoOverloadGroupsAreConfigured", Culture);
            }
        }

        internal static string NoRunnableInstances
        {
            get
            {
                return ResourceManager.GetString("NoRunnableInstances", Culture);
            }
        }

        internal static string NoRuntimeTransactionExists
        {
            get
            {
                return ResourceManager.GetString("NoRuntimeTransactionExists", Culture);
            }
        }

        internal static string NullKeyAlreadyPresent
        {
            get
            {
                return ResourceManager.GetString("NullKeyAlreadyPresent", Culture);
            }
        }

        internal static string OnlyBookmarkOwnerCanRemove
        {
            get
            {
                return ResourceManager.GetString("OnlyBookmarkOwnerCanRemove", Culture);
            }
        }

        internal static string OnlyOneOperationPerActivity
        {
            get
            {
                return ResourceManager.GetString("OnlyOneOperationPerActivity", Culture);
            }
        }

        internal static string OnlyOneRequireTransactionContextAllowed
        {
            get
            {
                return ResourceManager.GetString("OnlyOneRequireTransactionContextAllowed", Culture);
            }
        }

        internal static string OnlySingleCastDelegatesAllowed
        {
            get
            {
                return ResourceManager.GetString("OnlySingleCastDelegatesAllowed", Culture);
            }
        }

        internal static string OperationAlreadyCompleted
        {
            get
            {
                return ResourceManager.GetString("OperationAlreadyCompleted", Culture);
            }
        }

        internal static string OutOfIdSpaceIds
        {
            get
            {
                return ResourceManager.GetString("OutOfIdSpaceIds", Culture);
            }
        }

        internal static string OutOfInstanceIds
        {
            get
            {
                return ResourceManager.GetString("OutOfInstanceIds", Culture);
            }
        }

        internal static string OutOfInternalBookmarks
        {
            get
            {
                return ResourceManager.GetString("OutOfInternalBookmarks", Culture);
            }
        }

        internal static string OverloadingMethodMustBeStatic
        {
            get
            {
                return ResourceManager.GetString("OverloadingMethodMustBeStatic", Culture);
            }
        }

        internal static string OverloadOnlyCallableFromWorkflowThread
        {
            get
            {
                return ResourceManager.GetString("OverloadOnlyCallableFromWorkflowThread", Culture);
            }
        }

        internal static string PauseWhenPersistableInvalidIfPersistable
        {
            get
            {
                return ResourceManager.GetString("PauseWhenPersistableInvalidIfPersistable", Culture);
            }
        }

        internal static string PrepareForSerializationRequiresPersistability
        {
            get
            {
                return ResourceManager.GetString("PrepareForSerializationRequiresPersistability", Culture);
            }
        }

        internal static string ProvidedStateInitializedForExecution
        {
            get
            {
                return ResourceManager.GetString("ProvidedStateInitializedForExecution", Culture);
            }
        }

        internal static string ReadonlyNameScopeCannotBeUpdated
        {
            get
            {
                return ResourceManager.GetString("ReadonlyNameScopeCannotBeUpdated", Culture);
            }
        }

        internal static string RegisteredBookmarkScopeRequired
        {
            get
            {
                return ResourceManager.GetString("RegisteredBookmarkScopeRequired", Culture);
            }
        }

        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceManager, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.Activities.SR", typeof(System.Activities.SR).Assembly);
                    resourceManager = manager;
                }
                return resourceManager;
            }
        }

        internal static string ResultArgumentMustBeBoundToResultProperty
        {
            get
            {
                return ResourceManager.GetString("ResultArgumentMustBeBoundToResultProperty", Culture);
            }
        }

        internal static string ResultCannotBeSetOnArgumentExpressions
        {
            get
            {
                return ResourceManager.GetString("ResultCannotBeSetOnArgumentExpressions", Culture);
            }
        }

        internal static string RootArgumentViolationsFound
        {
            get
            {
                return ResourceManager.GetString("RootArgumentViolationsFound", Culture);
            }
        }

        internal static string RootArgumentViolationsFoundNoInputs
        {
            get
            {
                return ResourceManager.GetString("RootArgumentViolationsFoundNoInputs", Culture);
            }
        }

        internal static string RuntimeArgumentNotCreated
        {
            get
            {
                return ResourceManager.GetString("RuntimeArgumentNotCreated", Culture);
            }
        }

        internal static string RuntimeDelegateArgumentDirectionIncorrect
        {
            get
            {
                return ResourceManager.GetString("RuntimeDelegateArgumentDirectionIncorrect", Culture);
            }
        }

        internal static string RuntimeDelegateArgumentTypeIncorrect
        {
            get
            {
                return ResourceManager.GetString("RuntimeDelegateArgumentTypeIncorrect", Culture);
            }
        }

        internal static string RuntimeOperationInProgress
        {
            get
            {
                return ResourceManager.GetString("RuntimeOperationInProgress", Culture);
            }
        }

        internal static string RuntimeRunning
        {
            get
            {
                return ResourceManager.GetString("RuntimeRunning", Culture);
            }
        }

        internal static string RuntimeTransactionAlreadyExists
        {
            get
            {
                return ResourceManager.GetString("RuntimeTransactionAlreadyExists", Culture);
            }
        }

        internal static string RuntimeTransactionIsSuppressed
        {
            get
            {
                return ResourceManager.GetString("RuntimeTransactionIsSuppressed", Culture);
            }
        }

        internal static string SameUserStateUsedForMultipleInvokes
        {
            get
            {
                return ResourceManager.GetString("SameUserStateUsedForMultipleInvokes", Culture);
            }
        }

        internal static string SavingActivityToXamlNotSupported
        {
            get
            {
                return ResourceManager.GetString("SavingActivityToXamlNotSupported", Culture);
            }
        }

        internal static string SendNotSupported
        {
            get
            {
                return ResourceManager.GetString("SendNotSupported", Culture);
            }
        }

        internal static string SetupOrCleanupWorkflowThreadThrew
        {
            get
            {
                return ResourceManager.GetString("SetupOrCleanupWorkflowThreadThrew", Culture);
            }
        }

        internal static string SymbolResolverAlreadyExists
        {
            get
            {
                return ResourceManager.GetString("SymbolResolverAlreadyExists", Culture);
            }
        }

        internal static string SymbolResolverMustBeSingleton
        {
            get
            {
                return ResourceManager.GetString("SymbolResolverMustBeSingleton", Culture);
            }
        }

        internal static string TimerExtensionAlreadyAttached
        {
            get
            {
                return ResourceManager.GetString("TimerExtensionAlreadyAttached", Culture);
            }
        }

        internal static string TimerExtensionRequiresWorkflowInstance
        {
            get
            {
                return ResourceManager.GetString("TimerExtensionRequiresWorkflowInstance", Culture);
            }
        }

        internal static string TooManyViolationsForExceptionMessage
        {
            get
            {
                return ResourceManager.GetString("TooManyViolationsForExceptionMessage", Culture);
            }
        }

        internal static string TrackingRelatedWorkflowAbort
        {
            get
            {
                return ResourceManager.GetString("TrackingRelatedWorkflowAbort", Culture);
            }
        }

        internal static string TransactionHandleAlreadyHasTransaction
        {
            get
            {
                return ResourceManager.GetString("TransactionHandleAlreadyHasTransaction", Culture);
            }
        }

        internal static string TryLoadRequiresOwner
        {
            get
            {
                return ResourceManager.GetString("TryLoadRequiresOwner", Culture);
            }
        }

        internal static string UnInitializedRuntimeTransactionHandle
        {
            get
            {
                return ResourceManager.GetString("UnInitializedRuntimeTransactionHandle", Culture);
            }
        }

        internal static string UnmatchedNoPersistExit
        {
            get
            {
                return ResourceManager.GetString("UnmatchedNoPersistExit", Culture);
            }
        }

        internal static string ValueCollectionUpdatesNotAllowed
        {
            get
            {
                return ResourceManager.GetString("ValueCollectionUpdatesNotAllowed", Culture);
            }
        }

        internal static string ValueMustBeAssignableToType
        {
            get
            {
                return ResourceManager.GetString("ValueMustBeAssignableToType", Culture);
            }
        }

        internal static string VariableMustBeSet
        {
            get
            {
                return ResourceManager.GetString("VariableMustBeSet", Culture);
            }
        }

        internal static string WorkflowApplicationAlreadyHasId
        {
            get
            {
                return ResourceManager.GetString("WorkflowApplicationAlreadyHasId", Culture);
            }
        }

        internal static string WorkflowTerminatedExceptionDefaultMessage
        {
            get
            {
                return ResourceManager.GetString("WorkflowTerminatedExceptionDefaultMessage", Culture);
            }
        }

        internal static string WorkItemAbortedInstance
        {
            get
            {
                return ResourceManager.GetString("WorkItemAbortedInstance", Culture);
            }
        }

        internal static string WrongCacheMetadataForCodeActivity
        {
            get
            {
                return ResourceManager.GetString("WrongCacheMetadataForCodeActivity", Culture);
            }
        }

        internal static string WrongCacheMetadataForNativeActivity
        {
            get
            {
                return ResourceManager.GetString("WrongCacheMetadataForNativeActivity", Culture);
            }
        }

        internal static string WrongNumberOfArgumentsForActivityDelegate
        {
            get
            {
                return ResourceManager.GetString("WrongNumberOfArgumentsForActivityDelegate", Culture);
            }
        }

        internal static string XamlElementExpected
        {
            get
            {
                return ResourceManager.GetString("XamlElementExpected", Culture);
            }
        }
    }
}

