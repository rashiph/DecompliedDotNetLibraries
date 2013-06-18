namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class StringEqualsBranchOpcode : QueryConditionalBranchOpcode
    {
        internal StringEqualsBranchOpcode() : base(OpcodeID.StringEqualsBranch, new StringBranchIndex())
        {
        }

        internal override LiteralRelationOpcode ValidateOpcode(Opcode opcode)
        {
            StringEqualsOpcode opcode2 = opcode as StringEqualsOpcode;
            if (opcode2 != null)
            {
                return opcode2;
            }
            return null;
        }
    }
}

