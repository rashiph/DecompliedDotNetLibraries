using System;
using System.Globalization;
using System.Resources;
using System.Runtime;
using System.Workflow.Activities;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Tracking;

internal sealed class SR
{
    internal const string ActivationDescr = "ActivationDescr";
    internal const string Activity = "Activity";
    internal const string Base = "Base";
    internal const string CallExternalMethodActivityDescription = "CallExternalMethodActivityDescription";
    internal const string ChangingVariable = "ChangingVariable";
    internal const string CodeActivityDescription = "CodeActivityDescription";
    internal const string CodeConditionDisplayName = "CodeConditionDisplayName";
    internal const string CompensatableSequenceActivityDescription = "CompensatableSequenceActivityDescription";
    internal const string CompletedStateDescription = "CompletedStateDescription";
    internal const string CompletedStateImagePathDescription = "CompletedStateImagePathDescription";
    internal const string ConditionalActivityDescription = "ConditionalActivityDescription";
    internal const string ConditionDescr = "ConditionDescr";
    internal const string ConditionedActivityConditions = "ConditionedActivityConditions";
    internal const string Conditions = "Conditions";
    internal const string ConnectorColorDescription = "ConnectorColorDescription";
    internal const string ConstrainedGroupActivityDescription = "ConstrainedGroupActivityDescription";
    internal const string CorrelationSet = "CorrelationSet";
    internal const string CorrelationSetDescr = "CorrelationSetDescr";
    internal const string DelayActivityDescription = "DelayActivityDescription";
    internal const string DynamicUpdateConditionDescr = "DynamicUpdateConditionDescr";
    internal const string Error_ActivationActivityInsideLoop = "Error_ActivationActivityInsideLoop";
    internal const string Error_ActivationActivityNotFirst = "Error_ActivationActivityNotFirst";
    private const string Error_BlackBoxCustomStateNotSupported = "Error_BlackBoxCustomStateNotSupported";
    internal const string Error_CAGChildNotFound = "Error_CAGChildNotFound";
    internal const string Error_CAGDynamicUpdateNotAllowed = "Error_CAGDynamicUpdateNotAllowed";
    internal const string Error_CAGNotExecuting = "Error_CAGNotExecuting";
    internal const string Error_CAGQuiet = "Error_CAGQuiet";
    internal const string Error_CallExternalMethodArgsSerializationException = "Error_CallExternalMethodArgsSerializationException";
    internal const string Error_CanNotChangeAtRuntime = "Error_CanNotChangeAtRuntime";
    internal const string Error_CannotConnectToRequest = "Error_CannotConnectToRequest";
    private const string Error_CannotExecuteStateMachineWithoutInitialState = "Error_CannotExecuteStateMachineWithoutInitialState";
    internal const string Error_CannotNestThisActivity = "Error_CannotNestThisActivity";
    internal const string Error_CannotResolveWebServiceInput = "Error_CannotResolveWebServiceInput";
    internal const string Error_CantFindInstance = "Error_CantFindInstance";
    internal const string Error_CantInvokeDesignTimeTypes = "Error_CantInvokeDesignTimeTypes";
    internal const string Error_CantInvokeSelf = "Error_CantInvokeSelf";
    private const string Error_CantRemoveEventDrivenFromExecutingState = "Error_CantRemoveEventDrivenFromExecutingState";
    private const string Error_CantRemoveState = "Error_CantRemoveState";
    internal const string Error_CompletedStateCannotContainActivities = "Error_CompletedStateCannotContainActivities";
    internal const string Error_ConditionalBranchParentNotConditional = "Error_ConditionalBranchParentNotConditional";
    internal const string Error_ConditionalBranchUpdateAtRuntime = "Error_ConditionalBranchUpdateAtRuntime";
    internal const string Error_ConditionalDeclNotAllConditionalBranchDecl = "Error_ConditionalDeclNotAllConditionalBranchDecl";
    internal const string Error_ConditionalLessThanOneChildren = "Error_ConditionalLessThanOneChildren";
    internal const string Error_ConfigurationSectionNotFound = "Error_ConfigurationSectionNotFound";
    internal const string Error_ContextStackItemMissing = "Error_ContextStackItemMissing";
    internal const string Error_CorrelationAttributeInvalid = "Error_CorrelationAttributeInvalid";
    internal const string Error_CorrelationInitializerNotDefinied = "Error_CorrelationInitializerNotDefinied";
    internal const string Error_CorrelationInvalid = "Error_CorrelationInvalid";
    internal const string Error_CorrelationNotInitialized = "Error_CorrelationNotInitialized";
    internal const string Error_CorrelationParameterException = "Error_CorrelationParameterException";
    internal const string Error_CorrelationParameterNotFound = "Error_CorrelationParameterNotFound";
    internal const string Error_CorrelationTokenInReplicator = "Error_CorrelationTokenInReplicator";
    internal const string Error_CorrelationTokenMissing = "Error_CorrelationTokenMissing";
    internal const string Error_CorrelationTokenSpecifiedForUncorrelatedInterface = "Error_CorrelationTokenSpecifiedForUncorrelatedInterface";
    internal const string Error_CorrelationTypeNotConsistent = "Error_CorrelationTypeNotConsistent";
    internal const string Error_CorrelationViolationException = "Error_CorrelationViolationException";
    internal const string Error_DuplicateCorrelation = "Error_DuplicateCorrelation";
    internal const string Error_DuplicateCorrelationAttribute = "Error_DuplicateCorrelationAttribute";
    internal const string Error_DuplicatedActivityID = "Error_DuplicatedActivityID";
    internal const string Error_DuplicateParameter = "Error_DuplicateParameter";
    internal const string Error_DuplicateWebServiceFaultFound = "Error_DuplicateWebServiceFaultFound";
    internal const string Error_DuplicateWebServiceResponseFound = "Error_DuplicateWebServiceResponseFound";
    internal const string Error_DynamicActivity = "Error_DynamicActivity";
    internal const string Error_DynamicActivity2 = "Error_DynamicActivity2";
    internal const string Error_DynamicActivity3 = "Error_DynamicActivity3";
    internal const string Error_EventActivityIsImmutable = "Error_EventActivityIsImmutable";
    private const string Error_EventActivityNotValidInStateHandler = "Error_EventActivityNotValidInStateHandler";
    internal const string Error_EventArgumentSerializationException = "Error_EventArgumentSerializationException";
    internal const string Error_EventArgumentValidationException = "Error_EventArgumentValidationException";
    internal const string Error_EventDeliveryFailedException = "Error_EventDeliveryFailedException";
    private const string Error_EventDrivenInvalidFirstActivity = "Error_EventDrivenInvalidFirstActivity";
    internal const string Error_EventDrivenMultipleEventActivity = "Error_EventDrivenMultipleEventActivity";
    internal const string Error_EventDrivenNoFirstActivity = "Error_EventDrivenNoFirstActivity";
    private const string Error_EventDrivenParentNotListen = "Error_EventDrivenParentNotListen";
    internal const string Error_EventHandlersChildNotFound = "Error_EventHandlersChildNotFound";
    internal const string Error_EventHandlersDeclParentNotScope = "Error_EventHandlersDeclParentNotScope";
    internal const string Error_EventNameMissing = "Error_EventNameMissing";
    internal const string Error_ExecInAtomicScope = "Error_ExecInAtomicScope";
    internal const string Error_ExecWithActivationReceive = "Error_ExecWithActivationReceive";
    internal const string Error_ExternalDataExchangeException = "Error_ExternalDataExchangeException";
    internal const string Error_ExternalDataExchangeServiceExists = "Error_ExternalDataExchangeServiceExists";
    internal const string Error_ExternalRuntimeContainerNotFound = "Error_ExternalRuntimeContainerNotFound";
    internal const string Error_FailedToStartTheWorkflow = "Error_FailedToStartTheWorkflow";
    internal const string Error_FieldNotExists = "Error_FieldNotExists";
    internal const string Error_GeneratorShouldContainSingleActivity = "Error_GeneratorShouldContainSingleActivity";
    internal const string Error_GenericMethodsNotSupported = "Error_GenericMethodsNotSupported";
    internal const string Error_GetCalleeWorkflow = "Error_GetCalleeWorkflow";
    internal const string Error_InitializerFollowerInTxnlScope = "Error_InitializerFollowerInTxnlScope";
    internal const string Error_InitializerInReplicator = "Error_InitializerInReplicator";
    private const string Error_InitialStateMustBeDifferentThanCompletedState = "Error_InitialStateMustBeDifferentThanCompletedState";
    internal const string Error_InsufficientArrayPassedIn = "Error_InsufficientArrayPassedIn";
    internal const string Error_InterfaceTypeNeedsExternalDataExchangeAttribute = "Error_InterfaceTypeNeedsExternalDataExchangeAttribute";
    internal const string Error_InterfaceTypeNotInterface = "Error_InterfaceTypeNotInterface";
    internal const string Error_InvalidCAGActivityType = "Error_InvalidCAGActivityType";
    private const string Error_InvalidCompositeStateChild = "Error_InvalidCompositeStateChild";
    internal const string Error_InvalidEventArgsSignature = "Error_InvalidEventArgsSignature";
    internal const string Error_InvalidEventMessage = "Error_InvalidEventMessage";
    internal const string Error_InvalidEventPropertyName = "Error_InvalidEventPropertyName";
    internal const string Error_InvalidIdentifier = "Error_InvalidIdentifier";
    internal const string Error_InvalidLanguageIdentifier = "Error_InvalidLanguageIdentifier";
    private const string Error_InvalidLeafStateChild = "Error_InvalidLeafStateChild";
    internal const string Error_InvalidLocalServiceMessage = "Error_InvalidLocalServiceMessage";
    internal const string Error_InvalidMethodPropertyName = "Error_InvalidMethodPropertyName";
    private const string Error_InvalidStateActivityParent = "Error_InvalidStateActivityParent";
    private const string Error_InvalidTargetStateInStateInitialization = "Error_InvalidTargetStateInStateInitialization";
    internal const string Error_ListenLessThanTwoChildren = "Error_ListenLessThanTwoChildren";
    internal const string Error_ListenNotAllEventDriven = "Error_ListenNotAllEventDriven";
    internal const string Error_MethodNotExists = "Error_MethodNotExists";
    internal const string Error_MisMatchCorrelationTokenOwnerNameProperty = "Error_MisMatchCorrelationTokenOwnerNameProperty";
    internal const string Error_MissingConditionName = "Error_MissingConditionName";
    internal const string Error_MissingCorrelationParameterAttribute = "Error_MissingCorrelationParameterAttribute";
    internal const string Error_MissingCorrelationTokenOwnerNameProperty = "Error_MissingCorrelationTokenOwnerNameProperty";
    internal const string Error_MissingCorrelationTokenProperty = "Error_MissingCorrelationTokenProperty";
    internal const string Error_MissingEventName = "Error_MissingEventName";
    internal const string Error_MissingInterfaceType = "Error_MissingInterfaceType";
    internal const string Error_MissingMethodName = "Error_MissingMethodName";
    internal const string Error_MissingRuleConditions = "Error_MissingRuleConditions";
    internal const string Error_MissingValidationProperty = "Error_MissingValidationProperty";
    internal const string Error_MoreThanOneEventHandlersDecl = "Error_MoreThanOneEventHandlersDecl";
    internal const string Error_MoreThanTwoActivitiesInEventHandlingScope = "Error_MoreThanTwoActivitiesInEventHandlingScope";
    internal const string Error_MultiDimensionalArray = "Error_MultiDimensionalArray";
    private const string Error_MultipleStateHandlerActivities = "Error_MultipleStateHandlerActivities";
    internal const string Error_MustHaveParent = "Error_MustHaveParent";
    internal const string Error_NegativeValue = "Error_NegativeValue";
    internal const string Error_NestedConstrainedGroupConditions = "Error_NestedConstrainedGroupConditions";
    internal const string Error_NoInstanceInSession = "Error_NoInstanceInSession";
    internal const string Error_NoMatchingActiveDirectoryEntry = "Error_NoMatchingActiveDirectoryEntry";
    internal const string Error_OutRefParameterNotSupported = "Error_OutRefParameterNotSupported";
    internal const string Error_OwnerActivityIsNotParent = "Error_OwnerActivityIsNotParent";
    internal const string Error_ParallelLessThanTwoChildren = "Error_ParallelLessThanTwoChildren";
    internal const string Error_ParallelNotAllSequence = "Error_ParallelNotAllSequence";
    internal const string Error_ParameterNotFound = "Error_ParameterNotFound";
    internal const string Error_ParameterPropertyNotSet = "Error_ParameterPropertyNotSet";
    internal const string Error_ParameterTypeNotFound = "Error_ParameterTypeNotFound";
    internal const string Error_ParameterTypeResolution = "Error_ParameterTypeResolution";
    internal const string Error_PropertyNotSet = "Error_PropertyNotSet";
    internal const string Error_ReplicatorCannotCancelChild = "Error_ReplicatorCannotCancelChild";
    internal const string Error_ReplicatorChildRunning = "Error_ReplicatorChildRunning";
    internal const string Error_ReplicatorDisconnected = "Error_ReplicatorDisconnected";
    internal const string Error_ReplicatorInvalidExecutionType = "Error_ReplicatorInvalidExecutionType";
    internal const string Error_ReplicatorNotExecuting = "Error_ReplicatorNotExecuting";
    internal const string Error_ReplicatorNotInitialized = "Error_ReplicatorNotInitialized";
    internal const string Error_ReturnTypeNotFound = "Error_ReturnTypeNotFound";
    internal const string Error_ReturnTypeNotVoid = "Error_ReturnTypeNotVoid";
    internal const string Error_RoleProviderNotAvailableOrEnabled = "Error_RoleProviderNotAvailableOrEnabled";
    internal const string Error_ServiceMissingExternalDataExchangeInterface = "Error_ServiceMissingExternalDataExchangeInterface";
    internal const string Error_ServiceNotFound = "Error_ServiceNotFound";
    private const string Error_SetStateMustPointToALeafNodeState = "Error_SetStateMustPointToALeafNodeState";
    private const string Error_SetStateMustPointToAState = "Error_SetStateMustPointToAState";
    private const string Error_SetStateOnlyWorksOnStateMachineWorkflow = "Error_SetStateOnlyWorksOnStateMachineWorkflow";
    private const string Error_StateActivityMustBeContainedInAStateMachine = "Error_StateActivityMustBeContainedInAStateMachine";
    internal const string Error_StateChildNotFound = "Error_StateChildNotFound";
    private const string Error_StateHandlerParentNotState = "Error_StateHandlerParentNotState";
    private const string Error_StateMachineWorkflowMustBeARootActivity = "Error_StateMachineWorkflowMustBeARootActivity";
    internal const string Error_TypeIsNotRootActivity = "Error_TypeIsNotRootActivity";
    internal const string Error_TypeNotExist = "Error_TypeNotExist";
    internal const string Error_TypeNotPublic = "Error_TypeNotPublic";
    internal const string Error_TypeNotPublicSerializable = "Error_TypeNotPublicSerializable";
    internal const string Error_TypeNotResolved = "Error_TypeNotResolved";
    internal const string Error_TypePropertyInvalid = "Error_TypePropertyInvalid";
    internal const string Error_UnexpectedArgumentType = "Error_UnexpectedArgumentType";
    internal const string Error_UninitializedCorrelation = "Error_UninitializedCorrelation";
    internal const string Error_UnknownConfigurationParameter = "Error_UnknownConfigurationParameter";
    internal const string Error_WebServiceFaultNotNeeded = "Error_WebServiceFaultNotNeeded";
    internal const string Error_WebServiceInputNotProcessed = "Error_WebServiceInputNotProcessed";
    internal const string Error_WebServiceReceiveNotConfigured = "Error_WebServiceReceiveNotConfigured";
    internal const string Error_WebServiceReceiveNotFound = "Error_WebServiceReceiveNotFound";
    internal const string Error_WebServiceReceiveNotMarkedActivate = "Error_WebServiceReceiveNotMarkedActivate";
    internal const string Error_WebServiceReceiveNotValid = "Error_WebServiceReceiveNotValid";
    internal const string Error_WebServiceResponseNotFound = "Error_WebServiceResponseNotFound";
    internal const string Error_WebServiceResponseNotNeeded = "Error_WebServiceResponseNotNeeded";
    internal const string Error_WhileShouldHaveOneChild = "Error_WhileShouldHaveOneChild";
    internal const string Error_WorkflowCompleted = "Error_WorkflowCompleted";
    internal const string Error_WorkflowInstanceDehydratedBeforeSendingResponse = "Error_InstanceDehydratedBeforeSendingResponse";
    internal const string Error_WorkflowTerminated = "Error_WorkflowTerminated";
    internal const string EventDrivenActivityDescription = "EventDrivenActivityDescription";
    internal const string EventHandlingScopeActivityDescription = "EventHandlingScopeActivityDescription";
    internal const string EventInfoMissing = "EventInfoMissing";
    internal const string EventNameMissing = "EventNameMissing";
    internal const string EventSink = "EventSink";
    internal const string ExecutionTypeDescr = "ExecutionTypeDescr";
    internal const string ExpressionDescr = "ExpressionDescr";
    internal const string ExternalEventNameDescr = "ExternalEventNameDescr";
    internal const string ExternalMethodNameDescr = "ExternalMethodNameDescr";
    internal const string FilterDescription_InvokeWorkflow = "FilterDescription_InvokeWorkflow";
    internal const string ForegroundCategory = "ForegroundCategory";
    internal const string General_MissingService = "General_MissingService";
    internal const string HandleExternalEventActivityDescription = "HandleExternalEventActivityDescription";
    internal const string Handlers = "Handlers";
    internal const string HelperExternalDataExchangeDesc = "HelperExternalDataExchangeDesc";
    internal const string In = "In";
    internal const string InitialChildDataDescr = "InitialChildDataDescr";
    internal const string InitializeCaleeDescr = "InitializeCaleeDescr";
    internal const string InitialStateDescription = "InitialStateDescription";
    internal const string InitialStateImagePathDescription = "InitialStateImagePathDescription";
    internal const string InterfaceTypeDescription = "InterfaceTypeDescription";
    internal const string InterfaceTypeFilterDescription = "InterfaceTypeFilterDescription";
    internal const string InterfaceTypeMissing = "InterfaceTypeMissing";
    private const string InvalidActivityStatus = "InvalidActivityStatus";
    private const string InvalidSetStateInStateInitialization = "InvalidSetStateInStateInitialization";
    private const string InvalidStateMachineAction = "InvalidStateMachineAction";
    private const string InvalidStateTransitionPath = "InvalidStateTransitionPath";
    internal const string InvalidTimespanFormat = "InvalidTimespanFormat";
    private const string InvalidUserDataInStateChangeTrackingRecord = "InvalidUserDataInStateChangeTrackingRecord";
    internal const string InvokeParameterDescription = "InvokeParameterDescription";
    internal const string InvokeWebServiceActivityDescription = "InvokeWebServiceActivityDescription";
    internal const string InvokeWorkflowActivityDescription = "InvokeWorkflowActivityDescription";
    internal const string ListenActivityDescription = "ListenActivityDescription";
    private static SR loader;
    internal const string MethodInfoMissing = "MethodInfoMissing";
    internal const string MethodNameDescr = "MethodNameDescr";
    internal const string MethodNameMissing = "MethodNameMissing";
    private const string MoveSetState = "MoveSetState";
    internal const string NameDescr = "NameDescr";
    internal const string OnAfterMethodInvokeDescr = "OnAfterMethodInvokeDescr";
    internal const string OnAfterReceiveDescr = "OnAfterReceiveDescr";
    internal const string OnBeforeFaultingDescr = "OnBeforeFaultingDescr";
    internal const string OnBeforeMethodInvokeDescr = "OnBeforeMethodInvokeDescr";
    internal const string OnBeforeResponseDescr = "OnBeforeResponseDescr";
    internal const string OnCompletedDescr = "OnCompletedDescr";
    internal const string OnGeneratorChildCompletedDescr = "OnGeneratorChildCompletedDescr";
    internal const string OnGeneratorChildInitializedDescr = "OnGeneratorChildInitializedDescr";
    internal const string OnInitializedDescr = "OnInitializedDescr";
    internal const string Optional = "Optional";
    internal const string Out = "Out";
    internal const string ParallelActivityDescription = "ParallelActivityDescription";
    internal const string ParameterDescription = "ParameterDescription";
    internal const string Parameters = "Parameters";
    internal const string PolicyActivityDescription = "PolicyActivityDescription";
    internal const string Properties = "Properties";
    internal const string ProxyClassDescr = "ProxyClassDescr";
    internal const string ReceiveActivityNameDescription = "ReceiveActivityNameDescription";
    internal const string Ref = "Ref";
    internal const string ReplicatorActivityDescription = "ReplicatorActivityDescription";
    internal const string ReplicatorUntilConditionDescr = "ReplicatorUntilConditionDescr";
    internal const string Required = "Required";
    private ResourceManager resources = new ResourceManager("System.Workflow.Activities.StringResources", Assembly.GetExecutingAssembly());
    internal const string RoleDescr = "RoleDescr";
    internal const string RuleConditionDisplayName = "RuleConditionDisplayName";
    internal const string RuleSetDefinitionDescription = "RuleSetDefinitionDescription";
    internal const string RuleSetDescription = "RuleSetDescription";
    internal const string ScopeActivityDescription = "ScopeActivityDescription";
    internal const string SequenceActivityDescription = "SequenceActivityDescription";
    internal const string SequentialWorkflow = "SequentialWorkflow";
    internal const string SetStateActivityDescription = "SetStateActivityDescription";
    internal const string ShowingExternalDataExchangeService = "ShowingExternalDataExchangeService";
    private const string SqlTrackingServiceRequired = "SqlTrackingServiceRequired";
    internal const string Standard = "Standard";
    internal const string StateActivityDescription = "StateActivityDescription";
    private const string StateAlreadySubscribesToThisEvent = "StateAlreadySubscribesToThisEvent";
    internal const string StateFinalizationActivityDescription = "StateFinalizationActivityDescription";
    internal const string StateInitializationActivityDescription = "StateInitializationActivityDescription";
    internal const string StateMachineWorkflow = "StateMachineWorkflow";
    internal const string StateMachineWorkflowActivityDescription = "StateMachineWorkflowActivityDescription";
    private const string StateMachineWorkflowMustHaveACurrentState = "StateMachineWorkflowMustHaveACurrentState";
    internal const string StateMachineWorkflowRequired = "StateMachineWorkflowRequired";
    internal const string TargetStateDescription = "TargetStateDescription";
    internal const string TargetWorkflowDescr = "TargetWorkflowDescr";
    internal const string TimeoutDurationDescription = "TimeoutDurationDescription";
    internal const string TimeoutInitializerDescription = "TimeoutInitializerDescription";
    internal const string Type = "Type";
    internal const string TypeDescr = "TypeDescr";
    private const string UnableToTransitionToState = "UnableToTransitionToState";
    private const string UndoSetAsCompletedState = "UndoSetAsCompletedState";
    private const string UndoSetAsInitialState = "UndoSetAsInitialState";
    internal const string UndoSwitchViews = "UndoSwitchViews";
    internal const string UntilConditionDescr = "UntilConditionDescr";
    internal const string URLDescr = "URLDescr";
    internal const string UserCodeHandlerDescr = "UserCodeHandlerDescr";
    internal const string Warning_AdditionalBindingsFound = "Warning_AdditionalBindingsFound";
    internal const string WebServiceFaultActivityDescription = "WebServiceFaultActivityDescription";
    internal const string WebServiceMethodDescription = "WebServiceMethodDescription";
    internal const string WebServiceReceiveActivityDescription = "WebServiceReceiveActivityDescription";
    internal const string WebServiceResponseActivityDescription = "WebServiceResponseActivityDescription";
    internal const string WebServiceSessionIDDescr = "WebServiceSessionIDDescr";
    internal const string WhenConditionDescr = "WhenConditionDescr";
    internal const string WhileActivityDescription = "WhileActivityDescription";
    internal const string WhileConditionDescr = "WhileConditionDescr";
    internal const string WorkflowAuthorizationException = "WorkflowAuthorizationException";

    internal SR()
    {
    }

    internal static string GetError_BlackBoxCustomStateNotSupported()
    {
        return GetString("Error_BlackBoxCustomStateNotSupported", new object[] { typeof(StateActivity).Name });
    }

    internal static string GetError_CannotExecuteStateMachineWithoutInitialState()
    {
        return GetString("Error_CannotExecuteStateMachineWithoutInitialState", new object[] { typeof(StateMachineWorkflowActivity).Name, "InitialStateName" });
    }

    internal static string GetError_CantRemoveEventDrivenFromExecutingState(string eventDrivenName, string parentStateName)
    {
        return GetString("Error_CantRemoveEventDrivenFromExecutingState", new object[] { typeof(EventDrivenActivity).Name, eventDrivenName, typeof(StateActivity).Name, parentStateName });
    }

    internal static string GetError_CantRemoveState(string stateName)
    {
        return GetString("Error_CantRemoveState", new object[] { typeof(StateActivity).Name, stateName });
    }

    internal static string GetError_CompletedStateMustPointToALeafNodeState()
    {
        return GetString("Error_SetStateMustPointToALeafNodeState", new object[] { "CompletedStateName", typeof(StateActivity).Name });
    }

    internal static string GetError_CompletedStateMustPointToAState()
    {
        return GetString("Error_SetStateMustPointToAState", new object[] { "CompletedStateName", typeof(StateActivity).Name });
    }

    internal static string GetError_EventActivityNotValidInStateFinalization()
    {
        return GetString("Error_EventActivityNotValidInStateHandler", new object[] { typeof(StateFinalizationActivity).Name, typeof(IEventActivity).FullName });
    }

    internal static string GetError_EventActivityNotValidInStateInitialization()
    {
        return GetString("Error_EventActivityNotValidInStateHandler", new object[] { typeof(StateInitializationActivity).Name, typeof(IEventActivity).FullName });
    }

    internal static string GetError_EventDrivenInvalidFirstActivity()
    {
        return GetString("Error_EventDrivenInvalidFirstActivity", new object[] { typeof(EventDrivenActivity).Name, typeof(IEventActivity).FullName, typeof(HandleExternalEventActivity).Name, typeof(DelayActivity).Name });
    }

    internal static string GetError_EventDrivenParentNotListen()
    {
        return GetString("Error_EventDrivenParentNotListen", new object[] { typeof(EventDrivenActivity).Name, typeof(ListenActivity).Name, typeof(EventHandlersActivity).Name, typeof(StateActivity).Name, typeof(StateMachineWorkflowActivity).Name });
    }

    internal static string GetError_InitialStateMustBeDifferentThanCompletedState()
    {
        return GetString("Error_InitialStateMustBeDifferentThanCompletedState", new object[] { StateMachineWorkflowActivity.InitialStateNameProperty, StateMachineWorkflowActivity.CompletedStateNameProperty });
    }

    internal static string GetError_InitialStateMustPointToALeafNodeState()
    {
        return GetString("Error_SetStateMustPointToALeafNodeState", new object[] { StateMachineWorkflowActivity.InitialStateNameProperty, typeof(StateActivity).Name });
    }

    internal static string GetError_InitialStateMustPointToAState()
    {
        return GetString("Error_SetStateMustPointToAState", new object[] { "InitialStateName", typeof(StateActivity).Name });
    }

    internal static string GetError_InvalidCompositeStateChild()
    {
        return GetString("Error_InvalidCompositeStateChild", new object[] { typeof(StateMachineWorkflowActivity).Name, typeof(StateActivity).Name, typeof(EventDrivenActivity).Name });
    }

    internal static string GetError_InvalidLeafStateChild()
    {
        return GetString("Error_InvalidLeafStateChild", new object[] { typeof(StateActivity).Name, typeof(EventDrivenActivity).Name, typeof(StateInitializationActivity).Name, typeof(StateFinalizationActivity).Name });
    }

    internal static string GetError_InvalidStateActivityParent()
    {
        return GetString("Error_InvalidStateActivityParent", new object[] { typeof(StateActivity).Name });
    }

    internal static string GetError_InvalidTargetStateInStateInitialization()
    {
        return GetString("Error_InvalidTargetStateInStateInitialization", new object[] { typeof(SetStateActivity).Name, "TargetStateName", typeof(StateActivity).Name, typeof(StateInitializationActivity).Name });
    }

    internal static string GetError_MultipleStateFinalizationActivities()
    {
        return GetString("Error_MultipleStateHandlerActivities", new object[] { typeof(StateFinalizationActivity).Name, typeof(StateActivity).Name });
    }

    internal static string GetError_MultipleStateInitializationActivities()
    {
        return GetString("Error_MultipleStateHandlerActivities", new object[] { typeof(StateInitializationActivity).Name, typeof(StateActivity).Name });
    }

    internal static string GetError_SetStateMustPointToALeafNodeState()
    {
        return GetString("Error_SetStateMustPointToALeafNodeState", new object[] { SetStateActivity.TargetStateNameProperty, typeof(StateActivity).Name });
    }

    internal static string GetError_SetStateMustPointToAState()
    {
        return GetString("Error_SetStateMustPointToAState", new object[] { SetStateActivity.TargetStateNameProperty, typeof(StateActivity).Name });
    }

    internal static string GetError_SetStateOnlyWorksOnStateMachineWorkflow()
    {
        return GetString("Error_SetStateOnlyWorksOnStateMachineWorkflow", new object[] { typeof(SetStateActivity).Name, typeof(EventDrivenActivity).Name, typeof(StateInitializationActivity).Name, typeof(StateMachineWorkflowActivity).Name, typeof(StateActivity).Name });
    }

    internal static string GetError_StateActivityMustBeContainedInAStateMachine()
    {
        return GetString("Error_StateActivityMustBeContainedInAStateMachine", new object[] { typeof(StateActivity).Name, typeof(StateMachineWorkflowActivity).Name, "InitialStateName" });
    }

    internal static string GetError_StateFinalizationParentNotState()
    {
        return GetString("Error_StateHandlerParentNotState", new object[] { typeof(StateFinalizationActivity).Name, typeof(StateActivity).Name });
    }

    internal static string GetError_StateInitializationParentNotState()
    {
        return GetString("Error_StateHandlerParentNotState", new object[] { typeof(StateInitializationActivity).Name, typeof(StateActivity).Name });
    }

    internal static string GetError_StateMachineWorkflowMustBeARootActivity()
    {
        return GetString("Error_StateMachineWorkflowMustBeARootActivity", new object[] { typeof(StateMachineWorkflowActivity).Name });
    }

    internal static string GetInvalidActivityStatus(System.Workflow.ComponentModel.Activity activity)
    {
        return GetString("InvalidActivityStatus", new object[] { activity.ExecutionStatus, activity.QualifiedName });
    }

    internal static string GetInvalidSetStateInStateInitialization()
    {
        return GetString("InvalidSetStateInStateInitialization", new object[] { typeof(SetStateActivity).Name, typeof(StateInitializationActivity).Name });
    }

    internal static string GetInvalidStateMachineAction(string stateName)
    {
        return GetString("InvalidStateMachineAction", new object[] { typeof(StateActivity).Name, typeof(StateMachineAction).Name, stateName });
    }

    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
    internal static string GetInvalidStateTransitionPath()
    {
        return GetString("InvalidStateTransitionPath");
    }

    internal static string GetInvalidUserDataInStateChangeTrackingRecord()
    {
        return GetString("InvalidUserDataInStateChangeTrackingRecord", new object[] { "StateActivity.StateChange", typeof(StateActivity).Name });
    }

    private static SR GetLoader()
    {
        if (loader == null)
        {
            loader = new SR();
        }
        return loader;
    }

    internal static string GetMoveSetState()
    {
        return GetString("MoveSetState", new object[] { typeof(SetStateActivity).Name });
    }

    internal static string GetSqlTrackingServiceRequired()
    {
        return GetString("SqlTrackingServiceRequired", new object[] { "StateHistory", typeof(SqlTrackingService).FullName });
    }

    internal static string GetStateAlreadySubscribesToThisEvent(string stateName, IComparable queueName)
    {
        return GetString("StateAlreadySubscribesToThisEvent", new object[] { typeof(StateActivity).Name, stateName, queueName });
    }

    internal static string GetStateMachineWorkflowMustHaveACurrentState()
    {
        return GetString("StateMachineWorkflowMustHaveACurrentState", new object[] { typeof(StateMachineWorkflowActivity).Name });
    }

    internal static string GetStateMachineWorkflowRequired()
    {
        return GetString("StateMachineWorkflowRequired", new object[] { typeof(StateMachineWorkflowInstance).Name, typeof(StateMachineWorkflowActivity).Name });
    }

    internal static string GetString(string name)
    {
        return GetString(Culture, name);
    }

    internal static string GetString(CultureInfo culture, string name)
    {
        SR loader = GetLoader();
        if (loader == null)
        {
            return null;
        }
        return loader.resources.GetString(name, culture);
    }

    internal static string GetString(string name, params object[] args)
    {
        return GetString(Culture, name, args);
    }

    internal static string GetString(CultureInfo culture, string name, params object[] args)
    {
        SR loader = GetLoader();
        if (loader == null)
        {
            return null;
        }
        string format = loader.resources.GetString(name, culture);
        if ((args != null) && (args.Length > 0))
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }
        return format;
    }

    internal static string GetUnableToTransitionToState(string stateName)
    {
        return GetString("UnableToTransitionToState", new object[] { stateName });
    }

    internal static string GetUndoSetAsCompletedState(string stateName)
    {
        return GetString("UndoSetAsCompletedState", new object[] { stateName });
    }

    internal static string GetUndoSetAsInitialState(string stateName)
    {
        return GetString("UndoSetAsInitialState", new object[] { stateName });
    }

    private static CultureInfo Culture
    {
        get
        {
            return null;
        }
    }

    internal static string Error_SenderMustBeActivityExecutionContext
    {
        get
        {
            return GetString("Error_SenderMustBeActivityExecutionContext", new object[] { typeof(ActivityExecutionContext).FullName });
        }
    }
}

