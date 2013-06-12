namespace System.Xml.Schema
{
    using System;

    internal class AxisElement
    {
        internal int curDepth;
        internal DoubleLinkAxis curNode;
        internal bool isMatch;
        internal int rootDepth;

        internal AxisElement(DoubleLinkAxis node, int depth)
        {
            this.curNode = node;
            this.rootDepth = this.curDepth = depth;
            this.isMatch = false;
        }

        internal bool MoveToChild(string name, string URN, int depth, ForwardAxis parent)
        {
            if (!Asttree.IsAttribute(this.curNode))
            {
                if (this.isMatch)
                {
                    this.isMatch = false;
                }
                if (!AxisStack.Equal(this.curNode.Name, this.curNode.Urn, name, URN))
                {
                    return false;
                }
                if (this.curDepth == -1)
                {
                    this.SetDepth(depth);
                }
                else if (depth > this.curDepth)
                {
                    return false;
                }
                if (this.curNode == parent.TopNode)
                {
                    this.isMatch = true;
                    return true;
                }
                DoubleLinkAxis next = (DoubleLinkAxis) this.curNode.Next;
                if (Asttree.IsAttribute(next))
                {
                    this.isMatch = true;
                    return false;
                }
                this.curNode = next;
                this.curDepth++;
            }
            return false;
        }

        internal void MoveToParent(int depth, ForwardAxis parent)
        {
            if (depth == (this.curDepth - 1))
            {
                if ((this.curNode.Input == parent.RootNode) && parent.IsDss)
                {
                    this.curNode = parent.RootNode;
                    this.rootDepth = this.curDepth = -1;
                }
                else if (this.curNode.Input != null)
                {
                    this.curNode = (DoubleLinkAxis) this.curNode.Input;
                    this.curDepth--;
                }
            }
            else if ((depth == this.curDepth) && this.isMatch)
            {
                this.isMatch = false;
            }
        }

        internal void SetDepth(int depth)
        {
            this.rootDepth = this.curDepth = depth;
        }

        internal DoubleLinkAxis CurNode
        {
            get
            {
                return this.curNode;
            }
        }
    }
}

