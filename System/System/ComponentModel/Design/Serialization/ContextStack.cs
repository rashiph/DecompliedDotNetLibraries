namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public sealed class ContextStack
    {
        private ArrayList contextStack;

        public void Append(object context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (this.contextStack == null)
            {
                this.contextStack = new ArrayList();
            }
            this.contextStack.Insert(0, context);
        }

        public object Pop()
        {
            object obj2 = null;
            if ((this.contextStack != null) && (this.contextStack.Count > 0))
            {
                int index = this.contextStack.Count - 1;
                obj2 = this.contextStack[index];
                this.contextStack.RemoveAt(index);
            }
            return obj2;
        }

        public void Push(object context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (this.contextStack == null)
            {
                this.contextStack = new ArrayList();
            }
            this.contextStack.Add(context);
        }

        public object Current
        {
            get
            {
                if ((this.contextStack != null) && (this.contextStack.Count > 0))
                {
                    return this.contextStack[this.contextStack.Count - 1];
                }
                return null;
            }
        }

        public object this[int level]
        {
            get
            {
                if (level < 0)
                {
                    throw new ArgumentOutOfRangeException("level");
                }
                if ((this.contextStack != null) && (level < this.contextStack.Count))
                {
                    return this.contextStack[(this.contextStack.Count - 1) - level];
                }
                return null;
            }
        }

        public object this[Type type]
        {
            get
            {
                if (type == null)
                {
                    throw new ArgumentNullException("type");
                }
                if (this.contextStack != null)
                {
                    int count = this.contextStack.Count;
                    while (count > 0)
                    {
                        object o = this.contextStack[--count];
                        if (type.IsInstanceOfType(o))
                        {
                            return o;
                        }
                    }
                }
                return null;
            }
        }
    }
}

