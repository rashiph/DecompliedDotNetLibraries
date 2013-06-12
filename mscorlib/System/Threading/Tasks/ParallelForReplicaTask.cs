namespace System.Threading.Tasks
{
    using System;

    internal class ParallelForReplicaTask : Task
    {
        internal Task m_handedOverChildReplica;
        internal object m_stateForNextReplica;
        internal object m_stateFromPreviousReplica;

        internal ParallelForReplicaTask(Action<object> taskReplicaDelegate, object stateObject, Task parentTask, TaskScheduler taskScheduler, TaskCreationOptions creationOptionsForReplica, InternalTaskOptions internalOptionsForReplica) : base(taskReplicaDelegate, stateObject, parentTask, CancellationToken.None, creationOptionsForReplica, internalOptionsForReplica, taskScheduler)
        {
        }

        internal void SaveStateForNextReplica(object stateForNextReplica)
        {
            this.m_stateForNextReplica = stateForNextReplica;
        }

        internal override Task HandedOverChildReplica
        {
            get
            {
                return this.m_handedOverChildReplica;
            }
            set
            {
                this.m_handedOverChildReplica = value;
            }
        }

        internal override object SavedStateForNextReplica
        {
            get
            {
                return this.m_stateForNextReplica;
            }
            set
            {
                this.m_stateForNextReplica = value;
            }
        }

        internal override object SavedStateFromPreviousReplica
        {
            get
            {
                return this.m_stateFromPreviousReplica;
            }
            set
            {
                this.m_stateFromPreviousReplica = value;
            }
        }
    }
}

