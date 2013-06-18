namespace System.Workflow.Runtime
{
    using System;
    using System.Threading;
    using System.Workflow.ComponentModel;

    internal class WorkflowDefinitionLock : IDisposable
    {
        private object _syncObj;
        internal static readonly DependencyProperty WorkflowDefinitionLockObjectProperty = DependencyProperty.RegisterAttached("WorkflowDefinitionLockObject", typeof(object), typeof(WorkflowDefinitionLock), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));

        public WorkflowDefinitionLock(Activity definition)
        {
            this._syncObj = GetWorkflowDefinitionLockObject(definition);
            Monitor.Enter(this._syncObj);
        }

        public void Dispose()
        {
            Monitor.Exit(this._syncObj);
        }

        internal static object GetWorkflowDefinitionLockObject(DependencyObject dependencyObject)
        {
            lock (dependencyObject)
            {
                return dependencyObject.GetValue(WorkflowDefinitionLockObjectProperty);
            }
        }

        internal static void SetWorkflowDefinitionLockObject(DependencyObject dependencyObject, object value)
        {
            lock (dependencyObject)
            {
                if (dependencyObject.GetValue(WorkflowDefinitionLockObjectProperty) == null)
                {
                    dependencyObject.SetValue(WorkflowDefinitionLockObjectProperty, value);
                }
            }
        }
    }
}

