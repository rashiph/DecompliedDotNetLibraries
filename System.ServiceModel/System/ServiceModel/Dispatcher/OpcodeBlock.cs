namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct OpcodeBlock
    {
        private Opcode first;
        private Opcode last;
        internal OpcodeBlock(Opcode first)
        {
            this.first = first;
            this.first.Prev = null;
            this.last = this.first;
            while (this.last.Next != null)
            {
                this.last = this.last.Next;
            }
        }

        internal Opcode First
        {
            get
            {
                return this.first;
            }
        }
        internal Opcode Last
        {
            get
            {
                return this.last;
            }
        }
        internal void Append(Opcode opcode)
        {
            if (this.last == null)
            {
                this.first = opcode;
                this.last = opcode;
            }
            else
            {
                this.last.Attach(opcode);
                opcode.Next = null;
                this.last = opcode;
            }
        }

        internal void Append(OpcodeBlock block)
        {
            if (this.last == null)
            {
                this.first = block.first;
                this.last = block.last;
            }
            else
            {
                this.last.Attach(block.first);
                this.last = block.last;
            }
        }

        internal void DetachLast()
        {
            if (this.last != null)
            {
                Opcode prev = this.last.Prev;
                this.last.Prev = null;
                this.last = prev;
                if (this.last != null)
                {
                    this.last.Next = null;
                }
            }
        }
    }
}

