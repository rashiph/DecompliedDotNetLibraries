namespace System.ServiceModel.Dispatcher
{
    using System;

    internal abstract class JumpOpcode : Opcode
    {
        private Opcode jump;

        internal JumpOpcode(OpcodeID id, Opcode jump) : base(id)
        {
            this.Jump = jump;
            base.flags |= OpcodeFlags.Jump;
        }

        internal void AddJump(BlockEndOpcode jumpTo)
        {
            bool flag = base.IsReachableFromConditional();
            if (flag)
            {
                base.prev.DelinkFromConditional(this);
            }
            if (this.jump == null)
            {
                this.jump = jumpTo;
            }
            else
            {
                BranchOpcode jump;
                if (this.jump.ID == OpcodeID.Branch)
                {
                    jump = (BranchOpcode) this.jump;
                }
                else
                {
                    BlockEndOpcode opcode = (BlockEndOpcode) this.jump;
                    jump = new BranchOpcode();
                    jump.Branches.Add(opcode);
                    this.jump = jump;
                }
                jump.Branches.Add(jumpTo);
            }
            jumpTo.LinkJump(this);
            if (flag && (this.jump != null))
            {
                base.prev.LinkToConditional(this);
            }
        }

        internal override void Remove()
        {
            if (this.jump == null)
            {
                base.Remove();
            }
        }

        internal void RemoveJump(BlockEndOpcode jumpTo)
        {
            bool flag = base.IsReachableFromConditional();
            if (flag)
            {
                base.prev.DelinkFromConditional(this);
            }
            if (this.jump.ID == OpcodeID.Branch)
            {
                BranchOpcode jump = (BranchOpcode) this.jump;
                jumpTo.DeLinkJump(this);
                jump.RemoveChild(jumpTo);
                if (jump.Branches.Count == 0)
                {
                    this.jump = null;
                }
            }
            else
            {
                jumpTo.DeLinkJump(this);
                this.jump = null;
            }
            if (flag && (this.jump != null))
            {
                base.prev.LinkToConditional(this);
            }
        }

        internal override void Trim()
        {
            if (this.jump.ID == OpcodeID.Branch)
            {
                this.jump.Trim();
            }
        }

        internal Opcode Jump
        {
            get
            {
                return this.jump;
            }
            set
            {
                this.AddJump((BlockEndOpcode) value);
            }
        }
    }
}

