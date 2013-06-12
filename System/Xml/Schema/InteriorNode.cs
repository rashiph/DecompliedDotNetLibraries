namespace System.Xml.Schema
{
    using System;

    internal abstract class InteriorNode : SyntaxTreeNode
    {
        private SyntaxTreeNode leftChild;
        private SyntaxTreeNode rightChild;

        protected InteriorNode()
        {
        }

        public override SyntaxTreeNode Clone(Positions positions)
        {
            InteriorNode node = (InteriorNode) base.MemberwiseClone();
            node.LeftChild = this.leftChild.Clone(positions);
            if (this.rightChild != null)
            {
                node.RightChild = this.rightChild.Clone(positions);
            }
            return node;
        }

        public override void ExpandTree(InteriorNode parent, SymbolsDictionary symbols, Positions positions)
        {
            this.leftChild.ExpandTree(this, symbols, positions);
            if (this.rightChild != null)
            {
                this.rightChild.ExpandTree(this, symbols, positions);
            }
        }

        public SyntaxTreeNode LeftChild
        {
            get
            {
                return this.leftChild;
            }
            set
            {
                this.leftChild = value;
            }
        }

        public SyntaxTreeNode RightChild
        {
            get
            {
                return this.rightChild;
            }
            set
            {
                this.rightChild = value;
            }
        }
    }
}

