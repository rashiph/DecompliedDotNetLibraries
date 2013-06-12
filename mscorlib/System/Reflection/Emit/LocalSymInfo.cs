namespace System.Reflection.Emit
{
    using System;
    using System.Diagnostics.SymbolStore;
    using System.Reflection;

    internal class LocalSymInfo
    {
        internal const int InitialSize = 0x10;
        internal int[] m_iEndOffset;
        internal int[] m_iLocalSlot;
        internal int m_iLocalSymCount = 0;
        internal int m_iNameSpaceCount = 0;
        internal int[] m_iStartOffset;
        internal string[] m_namespace;
        internal string[] m_strName;
        internal byte[][] m_ubSignature;

        internal LocalSymInfo()
        {
        }

        internal void AddLocalSymInfo(string strName, byte[] signature, int slot, int startOffset, int endOffset)
        {
            this.EnsureCapacity();
            this.m_iStartOffset[this.m_iLocalSymCount] = startOffset;
            this.m_iEndOffset[this.m_iLocalSymCount] = endOffset;
            this.m_iLocalSlot[this.m_iLocalSymCount] = slot;
            this.m_strName[this.m_iLocalSymCount] = strName;
            this.m_ubSignature[this.m_iLocalSymCount] = signature;
            this.m_iLocalSymCount++;
        }

        internal void AddUsingNamespace(string strNamespace)
        {
            this.EnsureCapacityNamespace();
            this.m_namespace[this.m_iNameSpaceCount] = strNamespace;
            this.m_iNameSpaceCount++;
        }

        internal virtual void EmitLocalSymInfo(ISymbolWriter symWriter)
        {
            int num;
            for (num = 0; num < this.m_iLocalSymCount; num++)
            {
                symWriter.DefineLocalVariable(this.m_strName[num], FieldAttributes.PrivateScope, this.m_ubSignature[num], SymAddressKind.ILOffset, this.m_iLocalSlot[num], 0, 0, this.m_iStartOffset[num], this.m_iEndOffset[num]);
            }
            for (num = 0; num < this.m_iNameSpaceCount; num++)
            {
                symWriter.UsingNamespace(this.m_namespace[num]);
            }
        }

        private void EnsureCapacity()
        {
            if (this.m_iLocalSymCount == 0)
            {
                this.m_strName = new string[0x10];
                this.m_ubSignature = new byte[0x10][];
                this.m_iLocalSlot = new int[0x10];
                this.m_iStartOffset = new int[0x10];
                this.m_iEndOffset = new int[0x10];
            }
            else if (this.m_iLocalSymCount == this.m_strName.Length)
            {
                int num = this.m_iLocalSymCount * 2;
                int[] destinationArray = new int[num];
                Array.Copy(this.m_iLocalSlot, destinationArray, this.m_iLocalSymCount);
                this.m_iLocalSlot = destinationArray;
                destinationArray = new int[num];
                Array.Copy(this.m_iStartOffset, destinationArray, this.m_iLocalSymCount);
                this.m_iStartOffset = destinationArray;
                destinationArray = new int[num];
                Array.Copy(this.m_iEndOffset, destinationArray, this.m_iLocalSymCount);
                this.m_iEndOffset = destinationArray;
                string[] strArray = new string[num];
                Array.Copy(this.m_strName, strArray, this.m_iLocalSymCount);
                this.m_strName = strArray;
                byte[][] bufferArray = new byte[num][];
                Array.Copy(this.m_ubSignature, bufferArray, this.m_iLocalSymCount);
                this.m_ubSignature = bufferArray;
            }
        }

        private void EnsureCapacityNamespace()
        {
            if (this.m_iNameSpaceCount == 0)
            {
                this.m_namespace = new string[0x10];
            }
            else if (this.m_iNameSpaceCount == this.m_namespace.Length)
            {
                string[] destinationArray = new string[this.m_iNameSpaceCount * 2];
                Array.Copy(this.m_namespace, destinationArray, this.m_iNameSpaceCount);
                this.m_namespace = destinationArray;
            }
        }
    }
}

