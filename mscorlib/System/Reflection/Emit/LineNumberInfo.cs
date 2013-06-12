namespace System.Reflection.Emit
{
    using System;
    using System.Diagnostics.SymbolStore;

    internal sealed class LineNumberInfo
    {
        private const int InitialSize = 0x10;
        private int m_DocumentCount = 0;
        private REDocument[] m_Documents;
        private int m_iLastFound = 0;

        internal LineNumberInfo()
        {
        }

        internal void AddLineNumberInfo(ISymbolDocumentWriter document, int iOffset, int iStartLine, int iStartColumn, int iEndLine, int iEndColumn)
        {
            int index = this.FindDocument(document);
            this.m_Documents[index].AddLineNumberInfo(document, iOffset, iStartLine, iStartColumn, iEndLine, iEndColumn);
        }

        internal void EmitLineNumberInfo(ISymbolWriter symWriter)
        {
            for (int i = 0; i < this.m_DocumentCount; i++)
            {
                this.m_Documents[i].EmitLineNumberInfo(symWriter);
            }
        }

        private void EnsureCapacity()
        {
            if (this.m_DocumentCount == 0)
            {
                this.m_Documents = new REDocument[0x10];
            }
            else if (this.m_DocumentCount == this.m_Documents.Length)
            {
                REDocument[] destinationArray = new REDocument[this.m_DocumentCount * 2];
                Array.Copy(this.m_Documents, destinationArray, this.m_DocumentCount);
                this.m_Documents = destinationArray;
            }
        }

        private int FindDocument(ISymbolDocumentWriter document)
        {
            if ((this.m_iLastFound >= this.m_DocumentCount) || (this.m_Documents[this.m_iLastFound].m_document != document))
            {
                for (int i = 0; i < this.m_DocumentCount; i++)
                {
                    if (this.m_Documents[i].m_document == document)
                    {
                        this.m_iLastFound = i;
                        return this.m_iLastFound;
                    }
                }
                this.EnsureCapacity();
                this.m_iLastFound = this.m_DocumentCount;
                this.m_Documents[this.m_iLastFound] = new REDocument(document);
                this.m_DocumentCount++;
            }
            return this.m_iLastFound;
        }
    }
}

