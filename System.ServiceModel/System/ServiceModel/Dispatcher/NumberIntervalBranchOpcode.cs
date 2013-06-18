namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class NumberIntervalBranchOpcode : QueryConditionalBranchOpcode
    {
        internal NumberIntervalBranchOpcode() : base(OpcodeID.NumberIntervalBranch, new IntervalBranchIndex())
        {
        }

        internal override LiteralRelationOpcode ValidateOpcode(Opcode opcode)
        {
            NumberIntervalOpcode opcode2 = opcode as NumberIntervalOpcode;
            if (opcode2 != null)
            {
                return opcode2;
            }
            return null;
        }
    }
}

