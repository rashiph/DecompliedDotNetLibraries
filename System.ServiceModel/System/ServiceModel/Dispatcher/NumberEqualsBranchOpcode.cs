namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class NumberEqualsBranchOpcode : QueryConditionalBranchOpcode
    {
        internal NumberEqualsBranchOpcode() : base(OpcodeID.NumberEqualsBranch, new NumberBranchIndex())
        {
        }

        internal override LiteralRelationOpcode ValidateOpcode(Opcode opcode)
        {
            NumberEqualsOpcode opcode2 = opcode as NumberEqualsOpcode;
            if (opcode2 != null)
            {
                return opcode2;
            }
            return null;
        }
    }
}

