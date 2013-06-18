namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class InstanceLock
    {
        private Guid m_instanceId;
        private string m_name;
        private LockPriorityOperator m_operator;
        private int m_priority;
        [ThreadStatic]
        private static List<InstanceLock> t_heldLocks;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal InstanceLock(Guid id, string name, int priority, LockPriorityOperator lockOperator)
        {
            this.m_instanceId = id;
            this.m_name = name;
            this.m_priority = priority;
            this.m_operator = lockOperator;
        }

        [Conditional("DEBUG")]
        internal static void AssertIsLocked(InstanceLock theLock)
        {
        }

        [Conditional("DEBUG")]
        internal static void AssertNoLocksHeld()
        {
        }

        internal InstanceLockGuard Enter()
        {
            return new InstanceLockGuard(this);
        }

        internal void Exit()
        {
            try
            {
                HeldLocks.Remove(this);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        internal bool TryEnter()
        {
            InstanceLockGuard.EnforceGuard(this);
            bool lockTaken = false;
            bool flag2 = false;
            try
            {
                Monitor.TryEnter(this, ref lockTaken);
                if (lockTaken)
                {
                    HeldLocks.Add(this);
                    flag2 = true;
                }
            }
            finally
            {
                if (lockTaken && !flag2)
                {
                    Monitor.Exit(this);
                }
            }
            return flag2;
        }

        private static List<InstanceLock> HeldLocks
        {
            get
            {
                List<InstanceLock> list = t_heldLocks;
                if (list == null)
                {
                    t_heldLocks = new List<InstanceLock>();
                    list = t_heldLocks;
                }
                return list;
            }
        }

        internal Guid InstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_instanceId;
            }
        }

        internal LockPriorityOperator Operator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_operator;
            }
        }

        internal int Priority
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_priority;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct InstanceLockGuard : IDisposable
        {
            private readonly InstanceLock m_lock;
            internal static void EnforceGuard(InstanceLock theLock)
            {
                foreach (InstanceLock @lock in InstanceLock.HeldLocks)
                {
                    switch (theLock.Operator)
                    {
                        case LockPriorityOperator.GreaterThan:
                            if ((@lock.InstanceId == theLock.InstanceId) && (@lock.Priority <= theLock.Priority))
                            {
                                throw new InvalidOperationException(ExecutionStringManager.InstanceOperationNotValidinWorkflowThread);
                            }
                            break;

                        case LockPriorityOperator.GreaterThanOrReentrant:
                            if ((@lock.InstanceId == theLock.InstanceId) && (@lock.Priority < theLock.Priority))
                            {
                                throw new InvalidOperationException(ExecutionStringManager.InstanceOperationNotValidinWorkflowThread);
                            }
                            break;
                    }
                }
            }

            internal InstanceLockGuard(InstanceLock theLock)
            {
                this.m_lock = theLock;
                EnforceGuard(theLock);
                try
                {
                }
                finally
                {
                    bool flag = false;
                    Monitor.Enter(this.m_lock);
                    try
                    {
                        InstanceLock.HeldLocks.Add(this.m_lock);
                        flag = true;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            Monitor.Exit(this.m_lock);
                        }
                    }
                }
            }

            internal void Pulse()
            {
                Monitor.Pulse(this.m_lock);
            }

            internal void Wait()
            {
                Monitor.Wait(this.m_lock);
            }

            public void Dispose()
            {
                try
                {
                    InstanceLock.HeldLocks.Remove(this.m_lock);
                }
                finally
                {
                    Monitor.Exit(this.m_lock);
                }
            }
        }
    }
}

