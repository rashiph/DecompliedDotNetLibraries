namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class BlockEndOpcode : Opcode
    {
        private QueryBuffer<Opcode> sourceJumps;

        internal BlockEndOpcode() : base(OpcodeID.BlockEnd)
        {
            this.sourceJumps = new QueryBuffer<Opcode>(1);
        }

        internal void DeLinkJump(Opcode jump)
        {
            this.sourceJumps.Remove(jump);
        }

        internal void LinkJump(Opcode jump)
        {
            this.sourceJumps.Add(jump);
        }

        internal override void Remove()
        {
            while (this.sourceJumps.Count > 0)
            {
                ((JumpOpcode) this.sourceJumps[0]).RemoveJump(this);
            }
            base.Remove();
        }
    }
}

