namespace System.Xml.Schema
{
    using System;

    internal abstract class SyntaxTreeNode
    {
        protected SyntaxTreeNode()
        {
        }

        public abstract SyntaxTreeNode Clone(Positions positions);
        public abstract void ConstructPos(BitSet firstpos, BitSet lastpos, BitSet[] followpos);
        public abstract void ExpandTree(InteriorNode parent, SymbolsDictionary symbols, Positions positions);

        public abstract bool IsNullable { get; }

        public virtual bool IsRangeNode
        {
            get
            {
                return false;
            }
        }
    }
}

