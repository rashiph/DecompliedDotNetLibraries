namespace System.Threading
{
    using System;
    using System.Diagnostics.Eventing;

    internal sealed class TplEtwProvider : EventProviderBase
    {
        public static TplEtwProvider Log = new TplEtwProvider();

        private TplEtwProvider() : base(new Guid(0x2e5dba47, 0xa3d2, 0x4d16, 0x8e, 0xe0, 0x66, 0x71, 0xff, 220, 0xd7, 0xb5))
        {
        }

        [Event(5, Level=EventLevel.LogAlways)]
        public void ParallelFork(int OriginatingTaskManager, int OriginatingTaskID, int ForkJoinContextID)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(5, OriginatingTaskManager, OriginatingTaskID, ForkJoinContextID);
            }
        }

        [Event(3, Level=EventLevel.LogAlways)]
        public void ParallelInvokeBegin(int OriginatingTaskSchedulerID, int OriginatingTaskID, int ForkJoinContextID, ForkJoinOperationType OperationType, int ActionCount)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(3, new object[] { OriginatingTaskSchedulerID, OriginatingTaskID, ForkJoinContextID, (int) OperationType, ActionCount });
            }
        }

        [Event(4, Level=EventLevel.LogAlways)]
        public void ParallelInvokeEnd(int OriginatingTaskSchedulerID, int OriginatingTaskID, int ForkJoinContextID)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(4, OriginatingTaskSchedulerID, OriginatingTaskID, ForkJoinContextID);
            }
        }

        [Event(6, Level=EventLevel.LogAlways)]
        public void ParallelJoin(int OriginatingTaskSchedulerID, int OriginatingTaskID, int ForkJoinContextID)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(6, OriginatingTaskSchedulerID, OriginatingTaskID, ForkJoinContextID);
            }
        }

        [Event(1, Level=EventLevel.LogAlways)]
        public void ParallelLoopBegin(int OriginatingTaskSchedulerID, int OriginatingTaskID, int ForkJoinContextID, ForkJoinOperationType OperationType, long InclusiveFrom, long ExclusiveTo)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(1, new object[] { OriginatingTaskSchedulerID, OriginatingTaskID, ForkJoinContextID, (int) OperationType, InclusiveFrom, ExclusiveTo });
            }
        }

        [Event(2, Level=EventLevel.LogAlways)]
        public void ParallelLoopEnd(int OriginatingTaskSchedulerID, int OriginatingTaskID, int ForkJoinContextID, long TotalIterations)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(2, new object[] { OriginatingTaskSchedulerID, OriginatingTaskID, ForkJoinContextID, TotalIterations });
            }
        }

        [Event(9, Level=EventLevel.Verbose)]
        public void TaskCompleted(int OriginatingTaskSchedulerID, int OriginatingTaskID, int TaskID, bool IsExceptional)
        {
            if (base.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                base.WriteEvent(9, new object[] { OriginatingTaskSchedulerID, OriginatingTaskID, TaskID, IsExceptional });
            }
        }

        [Event(7, Level=EventLevel.Verbose)]
        public void TaskScheduled(int OriginatingTaskSchedulerID, int OriginatingTaskID, int TaskID, int CreatingTaskID, int TaskCreationOptions)
        {
            if (base.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                base.WriteEvent(7, new object[] { OriginatingTaskSchedulerID, OriginatingTaskID, TaskID, CreatingTaskID, TaskCreationOptions });
            }
        }

        [Event(8, Level=EventLevel.Verbose)]
        public void TaskStarted(int OriginatingTaskSchedulerID, int OriginatingTaskID, int TaskID)
        {
            if (base.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                base.WriteEvent(8, OriginatingTaskSchedulerID, OriginatingTaskID, TaskID);
            }
        }

        [Event(10, Level=EventLevel.Verbose)]
        public void TaskWaitBegin(int OriginatingTaskSchedulerID, int OriginatingTaskID, int TaskID)
        {
            if (base.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                base.WriteEvent(10, OriginatingTaskSchedulerID, OriginatingTaskID, TaskID);
            }
        }

        [Event(11, Level=EventLevel.Verbose)]
        public void TaskWaitEnd(int OriginatingTaskSchedulerID, int OriginatingTaskID, int TaskID)
        {
            if (base.IsEnabled(EventLevel.Verbose, ~EventKeywords.None))
            {
                base.WriteEvent(11, OriginatingTaskSchedulerID, OriginatingTaskID, TaskID);
            }
        }

        public enum ForkJoinOperationType
        {
            ParallelFor = 2,
            ParallelForEach = 3,
            ParallelInvoke = 1
        }
    }
}

