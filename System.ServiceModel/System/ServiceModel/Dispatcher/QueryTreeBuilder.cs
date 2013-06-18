namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    internal class QueryTreeBuilder
    {
        private Diverger diverger;
        private Opcode lastOpcode;

        internal QueryTreeBuilder()
        {
        }

        internal Opcode Build(Opcode tree, OpcodeBlock newBlock)
        {
            if (tree == null)
            {
                this.lastOpcode = newBlock.Last;
                return newBlock.First;
            }
            this.diverger = new Diverger(tree, newBlock.First);
            if (!this.diverger.Find())
            {
                this.lastOpcode = this.diverger.TreePath[this.diverger.TreePath.Count - 1];
                return tree;
            }
            if (this.diverger.TreeOpcode == null)
            {
                this.diverger.TreePath[this.diverger.TreePath.Count - 1].Attach(this.diverger.InsertOpcode);
            }
            else
            {
                this.diverger.TreeOpcode.Add(this.diverger.InsertOpcode);
            }
            this.lastOpcode = newBlock.Last;
            if (this.diverger.InsertOpcode.IsMultipleResult())
            {
                if (OpcodeID.Branch == this.diverger.TreeOpcode.ID)
                {
                    OpcodeList branches = ((BranchOpcode) this.diverger.TreeOpcode).Branches;
                    int num = 0;
                    int count = branches.Count;
                    while (num < count)
                    {
                        if (branches[num].IsMultipleResult())
                        {
                            this.lastOpcode = branches[num];
                            break;
                        }
                        num++;
                    }
                }
                else if (this.diverger.TreeOpcode.IsMultipleResult())
                {
                    this.lastOpcode = this.diverger.TreeOpcode;
                }
            }
            this.FixupJumps();
            return tree;
        }

        private void FixupJumps()
        {
            QueryBuffer<Opcode> treePath = this.diverger.TreePath;
            QueryBuffer<Opcode> insertPath = this.diverger.InsertPath;
            for (int i = 0; i < insertPath.Count; i++)
            {
                if (insertPath[i].TestFlag(OpcodeFlags.Jump))
                {
                    JumpOpcode opcode = (JumpOpcode) insertPath[i];
                    if (-1 == insertPath.IndexOf(opcode.Jump, i + 1))
                    {
                        BlockEndOpcode jump = (BlockEndOpcode) opcode.Jump;
                        opcode.RemoveJump(jump);
                        ((JumpOpcode) treePath[i]).AddJump(jump);
                    }
                }
            }
        }

        internal Opcode LastOpcode
        {
            get
            {
                return this.lastOpcode;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Diverger
        {
            private Opcode treeOpcode;
            private QueryBuffer<Opcode> treePath;
            private QueryBuffer<Opcode> insertPath;
            private Opcode insertOpcode;
            internal Diverger(Opcode tree, Opcode insert)
            {
                this.treePath = new QueryBuffer<Opcode>(0x10);
                this.insertPath = new QueryBuffer<Opcode>(0x10);
                this.treeOpcode = tree;
                this.insertOpcode = insert;
            }

            internal Opcode InsertOpcode
            {
                get
                {
                    return this.insertOpcode;
                }
            }
            internal QueryBuffer<Opcode> InsertPath
            {
                get
                {
                    return this.insertPath;
                }
            }
            internal Opcode TreeOpcode
            {
                get
                {
                    return this.treeOpcode;
                }
            }
            internal QueryBuffer<Opcode> TreePath
            {
                get
                {
                    return this.treePath;
                }
            }
            internal bool Find()
            {
                Opcode next = null;
                while ((this.treeOpcode != null) || (this.insertOpcode != null))
                {
                    if (this.insertOpcode == null)
                    {
                        return false;
                    }
                    if (this.treeOpcode == null)
                    {
                        return true;
                    }
                    if (this.treeOpcode.TestFlag(OpcodeFlags.Branch))
                    {
                        next = this.treeOpcode.Locate(this.insertOpcode);
                        if (next == null)
                        {
                            return true;
                        }
                        this.treeOpcode = next;
                        next = next.Next;
                    }
                    else
                    {
                        if (!this.treeOpcode.Equals(this.insertOpcode))
                        {
                            return true;
                        }
                        next = this.treeOpcode.Next;
                    }
                    this.treePath.Add(this.treeOpcode);
                    this.insertPath.Add(this.insertOpcode);
                    this.insertOpcode = this.insertOpcode.Next;
                    this.treeOpcode = next;
                }
                return false;
            }
        }
    }
}

