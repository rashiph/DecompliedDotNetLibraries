namespace System.Workflow.ComponentModel
{
    using System;

    internal interface IDependencyObjectAccessor
    {
        T[] GetInvocationList<T>(DependencyProperty dependencyEvent);
        void InitializeActivatingInstanceForRuntime(DependencyObject parentDependencyObject, IWorkflowCoreRuntime workflowCoreRuntime);
        void InitializeDefinitionForRuntime(DependencyObject parentDependencyObject);
        void InitializeInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime);
    }
}

