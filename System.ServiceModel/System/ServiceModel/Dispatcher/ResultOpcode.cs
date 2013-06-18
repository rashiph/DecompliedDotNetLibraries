namespace System.ServiceModel.Dispatcher
{
    using System;

    internal abstract class ResultOpcode : Opcode
    {
        internal ResultOpcode(OpcodeID id) : base(id)
        {
            base.flags |= OpcodeFlags.Result;
        }
    }
}

