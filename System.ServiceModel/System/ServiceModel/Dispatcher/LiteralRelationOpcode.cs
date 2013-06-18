namespace System.ServiceModel.Dispatcher
{
    using System;

    internal abstract class LiteralRelationOpcode : Opcode
    {
        internal LiteralRelationOpcode(OpcodeID id) : base(id)
        {
            base.flags |= OpcodeFlags.Literal;
        }

        internal abstract object Literal { get; }
    }
}

