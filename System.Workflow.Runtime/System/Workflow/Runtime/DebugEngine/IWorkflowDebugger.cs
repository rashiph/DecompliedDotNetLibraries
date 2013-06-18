namespace System.Workflow.Runtime.DebugEngine
{
    using System;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel;

    public interface IWorkflowDebugger
    {
        void ActivityStatusChanged(Guid programId, Guid scheduleTypeId, Guid instanceId, string activityQualifiedName, string hierarchicalActivityId, ActivityExecutionStatus status, int stateReaderId);
        void AssemblyLoaded(Guid programId, string assemblyPath, bool fromGlobalAssemblyCache);
        void BeforeActivityStatusChanged(Guid programId, Guid scheduleTypeId, Guid instanceId, string activityQualifiedName, string hierarchicalActivityId, ActivityExecutionStatus status, int stateReaderId);
        void BeforeHandlerInvoked(Guid programId, Guid scheduleTypeId, string activityQualifiedName, ActivityHandlerDescriptor handlerMethod);
        void HandlerInvoked(Guid programId, Guid instanceId, int threadId, string activityQualifiedName);
        void InstanceCompleted(Guid programId, Guid instanceId);
        void InstanceCreated(Guid programId, Guid instanceId, Guid scheduleTypeId);
        void InstanceDynamicallyUpdated(Guid programId, Guid instanceId, Guid scheduleTypeId);
        void ScheduleTypeLoaded(Guid programId, Guid scheduleTypeId, string assemblyFullName, string fileName, string md5Digest, bool isDynamic, string scheduleNamespace, string scheduleName, string workflowMarkup);
        void SetInitialActivityStatus(Guid programId, Guid scheduleTypeId, Guid instanceId, string activityQualifiedName, string hierarchicalActivityId, ActivityExecutionStatus status, int stateReaderId);
        void UpdateHandlerMethodsForActivity(Guid programId, Guid scheduleTypeId, string activityQualifiedName, List<ActivityHandlerDescriptor> handlerMethods);
    }
}

