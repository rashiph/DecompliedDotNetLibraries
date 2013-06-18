namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    internal sealed class EventQueueState
    {
        private List<ActivityExecutorDelegateInfo<QueueEventArgs>> asynchronousListeners = new List<ActivityExecutorDelegateInfo<QueueEventArgs>>();
        private Queue deliveredMessages = new Queue();
        [NonSerialized]
        private bool dirty;
        private bool enabled = true;
        [NonSerialized]
        internal IComparable queueName;
        private List<ActivityExecutorDelegateInfo<QueueEventArgs>> synchronousListeners = new List<ActivityExecutorDelegateInfo<QueueEventArgs>>();
        private bool transactional = true;

        internal EventQueueState()
        {
        }

        internal void CopyFrom(EventQueueState copyFromState)
        {
            this.deliveredMessages = new Queue(copyFromState.Messages);
            this.asynchronousListeners.AddRange(copyFromState.AsynchronousListeners.ToArray());
            this.synchronousListeners.AddRange(copyFromState.SynchronousListeners.ToArray());
            this.enabled = copyFromState.Enabled;
            this.transactional = copyFromState.Transactional;
            this.dirty = false;
        }

        internal List<ActivityExecutorDelegateInfo<QueueEventArgs>> AsynchronousListeners
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.asynchronousListeners;
            }
        }

        internal bool Dirty
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dirty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.dirty = value;
            }
        }

        internal bool Enabled
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enabled;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.enabled = value;
            }
        }

        internal Queue Messages
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.deliveredMessages;
            }
        }

        internal List<ActivityExecutorDelegateInfo<QueueEventArgs>> SynchronousListeners
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.synchronousListeners;
            }
        }

        internal bool Transactional
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.transactional;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.transactional = value;
            }
        }
    }
}

