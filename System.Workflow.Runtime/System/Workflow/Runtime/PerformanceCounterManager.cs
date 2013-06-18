namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class PerformanceCounterManager
    {
        private static string c_PerformanceCounterCategoryName = ExecutionStringManager.PerformanceCounterCategory;
        private Dictionary<PerformanceCounterAction, List<PerformanceCounterStatement>> m_actionStatements;
        private string m_instanceName;
        private static PerformanceCounterData[] s_DefaultPerformanceCounters;

        static PerformanceCounterManager()
        {
            PerformanceCounterData[] dataArray = new PerformanceCounterData[20];
            PerformanceCounterActionMapping[] mappings = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Creation, PerformanceCounterOperation.Increment) };
            dataArray[0] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesCreatedName, ExecutionStringManager.PerformanceCounterSchedulesCreatedDescription, PerformanceCounterType.NumberOfItems64, mappings);
            PerformanceCounterActionMapping[] mappingArray2 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Creation, PerformanceCounterOperation.Increment) };
            dataArray[1] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesCreatedRateName, ExecutionStringManager.PerformanceCounterSchedulesCreatedRateDescription, PerformanceCounterType.RateOfCountsPerSecond64, mappingArray2);
            PerformanceCounterActionMapping[] mappingArray3 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Unloading, PerformanceCounterOperation.Increment) };
            dataArray[2] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesUnloadedName, ExecutionStringManager.PerformanceCounterSchedulesUnloadedDescription, PerformanceCounterType.NumberOfItems64, mappingArray3);
            PerformanceCounterActionMapping[] mappingArray4 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Unloading, PerformanceCounterOperation.Increment) };
            dataArray[3] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesUnloadedRateName, ExecutionStringManager.PerformanceCounterSchedulesUnloadedRateDescription, PerformanceCounterType.RateOfCountsPerSecond64, mappingArray4);
            PerformanceCounterActionMapping[] mappingArray5 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Loading, PerformanceCounterOperation.Increment) };
            dataArray[4] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesLoadedName, ExecutionStringManager.PerformanceCounterSchedulesLoadedDescription, PerformanceCounterType.NumberOfItems64, mappingArray5);
            PerformanceCounterActionMapping[] mappingArray6 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Loading, PerformanceCounterOperation.Increment) };
            dataArray[5] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesLoadedRateName, ExecutionStringManager.PerformanceCounterSchedulesLoadedRateDescription, PerformanceCounterType.RateOfCountsPerSecond64, mappingArray6);
            PerformanceCounterActionMapping[] mappingArray7 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Completion, PerformanceCounterOperation.Increment) };
            dataArray[6] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesCompletedName, ExecutionStringManager.PerformanceCounterSchedulesCompletedDescription, PerformanceCounterType.NumberOfItems64, mappingArray7);
            PerformanceCounterActionMapping[] mappingArray8 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Completion, PerformanceCounterOperation.Increment) };
            dataArray[7] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesCompletedRateName, ExecutionStringManager.PerformanceCounterSchedulesCompletedRateDescription, PerformanceCounterType.RateOfCountsPerSecond64, mappingArray8);
            PerformanceCounterActionMapping[] mappingArray9 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Suspension, PerformanceCounterOperation.Increment), new PerformanceCounterActionMapping(PerformanceCounterAction.Resumption, PerformanceCounterOperation.Decrement) };
            dataArray[8] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesSuspendedName, ExecutionStringManager.PerformanceCounterSchedulesSuspendedDescription, PerformanceCounterType.NumberOfItems64, mappingArray9);
            PerformanceCounterActionMapping[] mappingArray10 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Suspension, PerformanceCounterOperation.Increment) };
            dataArray[9] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesSuspendedRateName, ExecutionStringManager.PerformanceCounterSchedulesSuspendedRateDescription, PerformanceCounterType.RateOfCountsPerSecond64, mappingArray10);
            PerformanceCounterActionMapping[] mappingArray11 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Termination, PerformanceCounterOperation.Increment) };
            dataArray[10] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesTerminatedName, ExecutionStringManager.PerformanceCounterSchedulesTerminatedDescription, PerformanceCounterType.NumberOfItems64, mappingArray11);
            PerformanceCounterActionMapping[] mappingArray12 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Termination, PerformanceCounterOperation.Increment) };
            dataArray[11] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesTerminatedRateName, ExecutionStringManager.PerformanceCounterSchedulesTerminatedRateDescription, PerformanceCounterType.RateOfCountsPerSecond64, mappingArray12);
            PerformanceCounterActionMapping[] mappingArray13 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Creation, PerformanceCounterOperation.Increment), new PerformanceCounterActionMapping(PerformanceCounterAction.Loading, PerformanceCounterOperation.Increment), new PerformanceCounterActionMapping(PerformanceCounterAction.Unloading, PerformanceCounterOperation.Decrement), new PerformanceCounterActionMapping(PerformanceCounterAction.Completion, PerformanceCounterOperation.Decrement), new PerformanceCounterActionMapping(PerformanceCounterAction.Termination, PerformanceCounterOperation.Decrement), new PerformanceCounterActionMapping(PerformanceCounterAction.Aborted, PerformanceCounterOperation.Decrement) };
            dataArray[12] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesInMemoryName, ExecutionStringManager.PerformanceCounterSchedulesInMemoryDescription, PerformanceCounterType.NumberOfItems64, mappingArray13);
            PerformanceCounterActionMapping[] mappingArray14 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Executing, PerformanceCounterOperation.Increment), new PerformanceCounterActionMapping(PerformanceCounterAction.NotExecuting, PerformanceCounterOperation.Decrement) };
            dataArray[13] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesExecutingName, ExecutionStringManager.PerformanceCounterSchedulesExecutingDescription, PerformanceCounterType.NumberOfItems64, mappingArray14);
            PerformanceCounterActionMapping[] mappingArray15 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Idle, PerformanceCounterOperation.Increment) };
            dataArray[14] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesIdleRateName, ExecutionStringManager.PerformanceCounterSchedulesIdleRateDescription, PerformanceCounterType.RateOfCountsPerSecond64, mappingArray15);
            PerformanceCounterActionMapping[] mappingArray16 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Runnable, PerformanceCounterOperation.Increment), new PerformanceCounterActionMapping(PerformanceCounterAction.NotExecuting, PerformanceCounterOperation.Decrement) };
            dataArray[15] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesRunnableName, ExecutionStringManager.PerformanceCounterSchedulesRunnableDescription, PerformanceCounterType.NumberOfItems64, mappingArray16);
            PerformanceCounterActionMapping[] mappingArray17 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Aborted, PerformanceCounterOperation.Increment) };
            dataArray[0x10] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesAbortedName, ExecutionStringManager.PerformanceCounterSchedulesAbortedDescription, PerformanceCounterType.NumberOfItems64, mappingArray17);
            PerformanceCounterActionMapping[] mappingArray18 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Aborted, PerformanceCounterOperation.Increment) };
            dataArray[0x11] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesAbortedRateName, ExecutionStringManager.PerformanceCounterSchedulesAbortedRateDescription, PerformanceCounterType.RateOfCountsPerSecond64, mappingArray18);
            PerformanceCounterActionMapping[] mappingArray19 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Persisted, PerformanceCounterOperation.Increment) };
            dataArray[0x12] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesPersistedName, ExecutionStringManager.PerformanceCounterSchedulesPersistedDescription, PerformanceCounterType.NumberOfItems64, mappingArray19);
            PerformanceCounterActionMapping[] mappingArray20 = new PerformanceCounterActionMapping[] { new PerformanceCounterActionMapping(PerformanceCounterAction.Persisted, PerformanceCounterOperation.Increment) };
            dataArray[0x13] = new PerformanceCounterData(ExecutionStringManager.PerformanceCounterSchedulesPersistedRateName, ExecutionStringManager.PerformanceCounterSchedulesPersistedRateDescription, PerformanceCounterType.RateOfCountsPerSecond64, mappingArray20);
            s_DefaultPerformanceCounters = dataArray;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal PerformanceCounterManager()
        {
        }

        internal List<PerformanceCounter> CreateCounters(string name)
        {
            List<PerformanceCounter> list = new List<PerformanceCounter> {
                new PerformanceCounter(c_PerformanceCounterCategoryName, name, "_Global_", 0)
            };
            if (!string.IsNullOrEmpty(this.m_instanceName))
            {
                list.Add(new PerformanceCounter(c_PerformanceCounterCategoryName, name, this.m_instanceName, false));
            }
            return list;
        }

        internal void Initialize(WorkflowRuntime runtime)
        {
            runtime.WorkflowExecutorInitializing += new EventHandler<WorkflowRuntime.WorkflowExecutorInitializingEventArgs>(this.WorkflowExecutorInitializing);
        }

        private void Notify(PerformanceCounterAction action, WorkflowExecutor executor)
        {
            List<PerformanceCounterStatement> list;
            if (this.m_actionStatements.TryGetValue(action, out list))
            {
                foreach (PerformanceCounterStatement statement in list)
                {
                    this.NotifyCounter(action, statement, executor);
                }
            }
        }

        private void NotifyCounter(PerformanceCounterAction action, PerformanceCounterStatement statement, WorkflowExecutor executor)
        {
            foreach (PerformanceCounter counter in statement.Counters)
            {
                switch (statement.Operation)
                {
                    case PerformanceCounterOperation.Increment:
                        counter.Increment();
                        break;

                    case PerformanceCounterOperation.Decrement:
                        counter.Decrement();
                        break;
                }
            }
        }

        internal void SetInstanceName(string instanceName)
        {
            PerformanceCounterData[] dataArray = s_DefaultPerformanceCounters;
            if (string.IsNullOrEmpty(instanceName))
            {
                try
                {
                    new SecurityPermission(PermissionState.Unrestricted).Assert();
                    instanceName = Process.GetCurrentProcess().MainModule.ModuleName;
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            this.m_instanceName = instanceName;
            Dictionary<PerformanceCounterAction, List<PerformanceCounterStatement>> dictionary = new Dictionary<PerformanceCounterAction, List<PerformanceCounterStatement>>();
            if (PerformanceCounterCategory.Exists(c_PerformanceCounterCategoryName))
            {
                for (int i = 0; i < dataArray.Length; i++)
                {
                    PerformanceCounterData data = dataArray[i];
                    for (int j = 0; j < data.Mappings.Length; j++)
                    {
                        PerformanceCounterActionMapping mapping = data.Mappings[j];
                        if (!dictionary.ContainsKey(mapping.Action))
                        {
                            dictionary.Add(mapping.Action, new List<PerformanceCounterStatement>());
                        }
                        List<PerformanceCounterStatement> list = dictionary[mapping.Action];
                        PerformanceCounterStatement item = new PerformanceCounterStatement(this.CreateCounters(data.Name), mapping.Operation);
                        list.Add(item);
                    }
                }
            }
            this.m_actionStatements = dictionary;
        }

        internal void Uninitialize(WorkflowRuntime runtime)
        {
            runtime.WorkflowExecutorInitializing -= new EventHandler<WorkflowRuntime.WorkflowExecutorInitializingEventArgs>(this.WorkflowExecutorInitializing);
        }

        private void WorkflowExecutionEvent(object sender, WorkflowExecutor.WorkflowExecutionEventArgs e)
        {
            PerformanceCounterAction creation;
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidArgumentType, new object[] { "sender", typeof(WorkflowExecutor).ToString() }));
            }
            WorkflowExecutor executor = (WorkflowExecutor) sender;
            switch (e.EventType)
            {
                case WorkflowEventInternal.Created:
                    creation = PerformanceCounterAction.Creation;
                    break;

                case WorkflowEventInternal.Completing:
                case WorkflowEventInternal.Idle:
                case WorkflowEventInternal.Suspending:
                case WorkflowEventInternal.Resuming:
                case WorkflowEventInternal.Persisting:
                case WorkflowEventInternal.Unloading:
                case WorkflowEventInternal.Exception:
                case WorkflowEventInternal.Terminating:
                case WorkflowEventInternal.Aborting:
                    return;

                case WorkflowEventInternal.Completed:
                    creation = PerformanceCounterAction.Completion;
                    break;

                case WorkflowEventInternal.SchedulerEmpty:
                    creation = PerformanceCounterAction.Idle;
                    break;

                case WorkflowEventInternal.Suspended:
                    creation = PerformanceCounterAction.Suspension;
                    break;

                case WorkflowEventInternal.Resumed:
                    creation = PerformanceCounterAction.Resumption;
                    break;

                case WorkflowEventInternal.Persisted:
                    creation = PerformanceCounterAction.Persisted;
                    break;

                case WorkflowEventInternal.Unloaded:
                    creation = PerformanceCounterAction.Unloading;
                    break;

                case WorkflowEventInternal.Loaded:
                    creation = PerformanceCounterAction.Loading;
                    break;

                case WorkflowEventInternal.Terminated:
                    creation = PerformanceCounterAction.Termination;
                    break;

                case WorkflowEventInternal.Aborted:
                    creation = PerformanceCounterAction.Aborted;
                    break;

                case WorkflowEventInternal.Runnable:
                    creation = PerformanceCounterAction.Runnable;
                    break;

                case WorkflowEventInternal.Executing:
                    creation = PerformanceCounterAction.Executing;
                    break;

                case WorkflowEventInternal.NotExecuting:
                    creation = PerformanceCounterAction.NotExecuting;
                    break;

                case WorkflowEventInternal.Started:
                    creation = PerformanceCounterAction.Starting;
                    break;

                default:
                    return;
            }
            this.Notify(creation, executor);
        }

        private void WorkflowExecutorInitializing(object sender, WorkflowRuntime.WorkflowExecutorInitializingEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
            {
                throw new ArgumentException("sender");
            }
            WorkflowExecutor executor = (WorkflowExecutor) sender;
            executor.WorkflowExecutionEvent += new EventHandler<WorkflowExecutor.WorkflowExecutionEventArgs>(this.WorkflowExecutionEvent);
        }
    }
}

