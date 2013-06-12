namespace System.Reflection.Emit
{
    using System;
    using System.Diagnostics.SymbolStore;

    internal sealed class ScopeTree
    {
        internal const int InitialSize = 0x10;
        internal int m_iCount = 0;
        internal int[] m_iOffsets;
        internal int m_iOpenScopeCount = 0;
        internal LocalSymInfo[] m_localSymInfos;
        internal ScopeAction[] m_ScopeActions;

        internal ScopeTree()
        {
        }

        internal void AddLocalSymInfoToCurrentScope(string strName, byte[] signature, int slot, int startOffset, int endOffset)
        {
            int currentActiveScopeIndex = this.GetCurrentActiveScopeIndex();
            if (this.m_localSymInfos[currentActiveScopeIndex] == null)
            {
                this.m_localSymInfos[currentActiveScopeIndex] = new LocalSymInfo();
            }
            this.m_localSymInfos[currentActiveScopeIndex].AddLocalSymInfo(strName, signature, slot, startOffset, endOffset);
        }

        internal void AddScopeInfo(ScopeAction sa, int iOffset)
        {
            if ((sa == ScopeAction.Close) && (this.m_iOpenScopeCount <= 0))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_UnmatchingSymScope"));
            }
            this.EnsureCapacity();
            this.m_ScopeActions[this.m_iCount] = sa;
            this.m_iOffsets[this.m_iCount] = iOffset;
            this.m_localSymInfos[this.m_iCount] = null;
            this.m_iCount++;
            if (sa == ScopeAction.Open)
            {
                this.m_iOpenScopeCount++;
            }
            else
            {
                this.m_iOpenScopeCount--;
            }
        }

        internal void AddUsingNamespaceToCurrentScope(string strNamespace)
        {
            int currentActiveScopeIndex = this.GetCurrentActiveScopeIndex();
            if (this.m_localSymInfos[currentActiveScopeIndex] == null)
            {
                this.m_localSymInfos[currentActiveScopeIndex] = new LocalSymInfo();
            }
            this.m_localSymInfos[currentActiveScopeIndex].AddUsingNamespace(strNamespace);
        }

        internal void EmitScopeTree(ISymbolWriter symWriter)
        {
            for (int i = 0; i < this.m_iCount; i++)
            {
                if (this.m_ScopeActions[i] == ScopeAction.Open)
                {
                    symWriter.OpenScope(this.m_iOffsets[i]);
                }
                else
                {
                    symWriter.CloseScope(this.m_iOffsets[i]);
                }
                if (this.m_localSymInfos[i] != null)
                {
                    this.m_localSymInfos[i].EmitLocalSymInfo(symWriter);
                }
            }
        }

        internal void EnsureCapacity()
        {
            if (this.m_iCount == 0)
            {
                this.m_iOffsets = new int[0x10];
                this.m_ScopeActions = new ScopeAction[0x10];
                this.m_localSymInfos = new LocalSymInfo[0x10];
            }
            else if (this.m_iCount == this.m_iOffsets.Length)
            {
                int num = this.m_iCount * 2;
                int[] destinationArray = new int[num];
                Array.Copy(this.m_iOffsets, destinationArray, this.m_iCount);
                this.m_iOffsets = destinationArray;
                ScopeAction[] actionArray = new ScopeAction[num];
                Array.Copy(this.m_ScopeActions, actionArray, this.m_iCount);
                this.m_ScopeActions = actionArray;
                LocalSymInfo[] infoArray = new LocalSymInfo[num];
                Array.Copy(this.m_localSymInfos, infoArray, this.m_iCount);
                this.m_localSymInfos = infoArray;
            }
        }

        internal int GetCurrentActiveScopeIndex()
        {
            int num = 0;
            int index = this.m_iCount - 1;
            if (this.m_iCount != 0)
            {
                while ((num > 0) || (this.m_ScopeActions[index] == ScopeAction.Close))
                {
                    if (this.m_ScopeActions[index] == ScopeAction.Open)
                    {
                        num--;
                    }
                    else
                    {
                        num++;
                    }
                    index--;
                }
                return index;
            }
            return -1;
        }
    }
}

