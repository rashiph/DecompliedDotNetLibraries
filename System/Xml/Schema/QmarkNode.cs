namespace System.Xml.Schema
{
    using System;

    internal sealed class QmarkNode : InteriorNode
    {
        public override void ConstructPos(BitSet firstpos, BitSet lastpos, BitSet[] followpos)
        {
            base.LeftChild.ConstructPos(firstpos, lastpos, followpos);
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

