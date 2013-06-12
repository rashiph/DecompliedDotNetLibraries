namespace System.Xml.Schema
{
    using System;

    internal sealed class ChoiceNode : InteriorNode
    {
        public override void ConstructPos(BitSet firstpos, BitSet lastpos, BitSet[] followpos)
        {
            base.LeftChild.ConstructPos(firstpos, lastpos, followpos);
            BitSet set = new BitSet(firstpos.Count);
            BitSet set2 = new BitSet(lastpos.Count);
            base.RightChild.ConstructPos(set, set2, followpos);
            firstpos.Or(set);
            lastpos.Or(set2);
        }

        public override bool IsNullable
        {
            get
            {
                if (!base.LeftChild.IsNullable)
                {
                    return base.RightChild.IsNullable;
                }
                return true;
            }
        }
    }
}

