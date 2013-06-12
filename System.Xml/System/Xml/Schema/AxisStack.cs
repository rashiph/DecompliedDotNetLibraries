namespace System.Xml.Schema
{
    using System;
    using System.Collections;

    internal class AxisStack
    {
        private ActiveAxis parent;
        private ArrayList stack;
        private ForwardAxis subtree;

        public AxisStack(ForwardAxis faxis, ActiveAxis parent)
        {
            this.subtree = faxis;
            this.stack = new ArrayList();
            this.parent = parent;
            if (!faxis.IsDss)
            {
                this.Push(1);
            }
        }

        internal static bool Equal(string thisname, string thisURN, string name, string URN)
        {
            if (thisURN == null)
            {
                if ((URN != null) && (URN.Length != 0))
                {
                    return false;
                }
            }
            else if ((thisURN.Length != 0) && (thisURN != URN))
            {
                return false;
            }
            if ((thisname.Length != 0) && (thisname != name))
            {
                return false;
            }
            return true;
        }

        internal bool MoveToAttribute(string name, string URN, int depth)
        {
            if (!this.subtree.IsAttribute)
            {
                return false;
            }
            if (!Equal(this.subtree.TopNode.Name, this.subtree.TopNode.Urn, name, URN))
            {
                return false;
            }
            bool flag = false;
            if (this.subtree.TopNode.Input == null)
            {
                if (!this.subtree.IsDss)
                {
                    return (depth == 1);
                }
                return true;
            }
            for (int i = 0; i < this.stack.Count; i++)
            {
                AxisElement element = (AxisElement) this.stack[i];
                if (element.isMatch && (element.CurNode == this.subtree.TopNode.Input))
                {
                    flag = true;
                }
            }
            return flag;
        }

        internal bool MoveToChild(string name, string URN, int depth)
        {
            bool flag = false;
            if (this.subtree.IsDss && Equal(this.subtree.RootNode.Name, this.subtree.RootNode.Urn, name, URN))
            {
                this.Push(-1);
            }
            for (int i = 0; i < this.stack.Count; i++)
            {
                if (((AxisElement) this.stack[i]).MoveToChild(name, URN, depth, this.subtree))
                {
                    flag = true;
                }
            }
            return flag;
        }

        internal void MoveToParent(string name, string URN, int depth)
        {
            if (!this.subtree.IsSelfAxis)
            {
                for (int i = 0; i < this.stack.Count; i++)
                {
                    ((AxisElement) this.stack[i]).MoveToParent(depth, this.subtree);
                }
                if (this.subtree.IsDss && Equal(this.subtree.RootNode.Name, this.subtree.RootNode.Urn, name, URN))
                {
                    this.Pop();
                }
            }
        }

        internal void Pop()
        {
            this.stack.RemoveAt(this.Length - 1);
        }

        internal void Push(int depth)
        {
            AxisElement element = new AxisElement(this.subtree.RootNode, depth);
            this.stack.Add(element);
        }

        internal int Length
        {
            get
            {
                return this.stack.Count;
            }
        }

        internal ForwardAxis Subtree
        {
            get
            {
                return this.subtree;
            }
        }
    }
}

