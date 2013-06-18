namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class IdSpace
    {
        private int lastId;
        private IList<Activity> members;

        public IdSpace()
        {
        }

        public IdSpace(IdSpace parent, int parentId)
        {
            this.Parent = parent;
            this.ParentId = parentId;
        }

        public void AddMember(Activity element)
        {
            if (this.members == null)
            {
                this.members = new List<Activity>();
            }
            if (this.lastId == 0x7fffffff)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.OutOfIdSpaceIds));
            }
            this.lastId++;
            element.InternalId = this.lastId;
            this.members.Add(element);
        }

        public void Dispose()
        {
            if (this.members != null)
            {
                this.members.Clear();
            }
            this.lastId = 0;
            this.Parent = null;
            this.ParentId = 0;
        }

        public Activity this[int id]
        {
            get
            {
                int num = id - 1;
                if (((this.members != null) && (num >= 0)) && (num < this.members.Count))
                {
                    return this.members[num];
                }
                return null;
            }
        }

        public int MemberCount
        {
            get
            {
                if (this.members == null)
                {
                    return 0;
                }
                return this.members.Count;
            }
        }

        public Activity Owner
        {
            get
            {
                if (this.Parent != null)
                {
                    return this.Parent[this.ParentId];
                }
                return null;
            }
        }

        public IdSpace Parent { get; private set; }

        public int ParentId { get; private set; }
    }
}

