namespace System.Xml.Schema
{
    using System;

    internal sealed class StarNode : InteriorNode
    {
        public override void ConstructPos(BitSet firstpos, BitSet lastpos, BitSet[] followpos)
        {
            base.LeftChild.ConstructPos(firstpos, lastpos, followpos);
            for (int i = lastpos.NextSet(-1); i != -1; i = lastpos.NextSet(i))
            {
                followpos[i].Or(firstpos);
            }
        }

        public override bool IsNullable
        {
            get
            {
                return true;
            }
        }
    }
}

