namespace System.Xml.Schema
{
    using System;
    using System.Collections;

    internal class NamespaceListNode : SyntaxTreeNode
    {
        protected NamespaceList namespaceList;
        protected object particle;

        public NamespaceListNode(NamespaceList namespaceList, object particle)
        {
            this.namespaceList = namespaceList;
            this.particle = particle;
        }

        public override SyntaxTreeNode Clone(Positions positions)
        {
            throw new InvalidOperationException();
        }

        public override void ConstructPos(BitSet firstpos, BitSet lastpos, BitSet[] followpos)
        {
            throw new InvalidOperationException();
        }

        public override void ExpandTree(InteriorNode parent, SymbolsDictionary symbols, Positions positions)
        {
            SyntaxTreeNode node = null;
            foreach (int num in this.GetResolvedSymbols(symbols))
            {
                if (symbols.GetParticle(num) != this.particle)
                {
                    symbols.IsUpaEnforced = false;
                }
                LeafNode node2 = new LeafNode(positions.Add(num, this.particle));
                if (node == null)
                {
                    node = node2;
                }
                else
                {
                    InteriorNode node3 = new ChoiceNode {
                        LeftChild = node,
                        RightChild = node2
                    };
                    node = node3;
                }
            }
            if (parent.LeftChild == this)
            {
                parent.LeftChild = node;
            }
            else
            {
                parent.RightChild = node;
            }
        }

        public virtual ICollection GetResolvedSymbols(SymbolsDictionary symbols)
        {
            return symbols.GetNamespaceListSymbols(this.namespaceList);
        }

        public override bool IsNullable
        {
            get
            {
                throw new InvalidOperationException();
            }
        }
    }
}

