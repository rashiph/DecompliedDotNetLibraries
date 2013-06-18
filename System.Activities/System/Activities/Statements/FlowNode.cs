namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public abstract class FlowNode
    {
        private int cacheId;
        private Flowchart owner;

        internal FlowNode()
        {
            this.Index = -1;
        }

        internal virtual void GetChildActivities(ICollection<Activity> children)
        {
        }

        internal abstract void GetConnectedNodes(IList<FlowNode> connections);
        internal abstract void OnOpen(Flowchart owner, NativeActivityMetadata metadata);
        internal bool Open(Flowchart owner, NativeActivityMetadata metadata)
        {
            if (this.cacheId == owner.CacheId)
            {
                if (!object.ReferenceEquals(this.owner, owner))
                {
                    metadata.AddValidationError(System.Activities.SR.FlowNodeCannotBeShared(this.owner.DisplayName, owner.DisplayName));
                }
                return false;
            }
            this.OnOpen(owner, metadata);
            this.owner = owner;
            this.cacheId = owner.CacheId;
            this.Index = -1;
            return true;
        }

        internal int Index { get; set; }

        internal bool IsOpen
        {
            get
            {
                return (this.owner != null);
            }
        }

        internal Flowchart Owner
        {
            get
            {
                return this.owner;
            }
        }
    }
}

