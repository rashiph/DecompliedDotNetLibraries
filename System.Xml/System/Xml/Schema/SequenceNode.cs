namespace System.Xml.Schema
{
    using System;

    internal sealed class SequenceNode : InteriorNode
    {
        public override void ConstructPos(BitSet firstpos, BitSet lastpos, BitSet[] followpos)
        {
            BitSet set = new BitSet(lastpos.Count);
            base.LeftChild.ConstructPos(firstpos, set, followpos);
            BitSet set2 = new BitSet(firstpos.Count);
            base.RightChild.ConstructPos(set2, lastpos, followpos);
            if (base.LeftChild.IsNullable && !base.RightChild.IsRangeNode)
            {
                firstpos.Or(set2);
            }
            if (base.RightChild.IsNullable)
            {
                lastpos.Or(set);
            }
            for (int i = set.NextSet(-1); i != -1; i = set.NextSet(i))
            {
                followpos[i].Or(set2);
            }
            if (base.RightChild.IsRangeNode)
            {
                ((LeafRangeNode) base.RightChild).NextIteration = firstpos.Clone();
            }
        }

        public override bool IsNullable
        {
            get
            {
                return ((base.LeftChild.IsNullable && (base.RightChild.IsNullable || base.RightChild.IsRangeNode)) || (base.RightChild.IsRangeNode && (((LeafRangeNode) base.RightChild).Min == 0M)));
            }
        }
    }
}

