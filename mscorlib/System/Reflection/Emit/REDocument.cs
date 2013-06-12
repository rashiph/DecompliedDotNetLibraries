namespace System.Reflection.Emit
{
    using System;
    using System.Diagnostics.SymbolStore;

    internal sealed class REDocument
    {
        private const int InitialSize = 0x10;
        internal ISymbolDocumentWriter m_document;
        private int[] m_iColumns;
        private int[] m_iEndColumns;
        private int[] m_iEndLines;
        private int m_iLineNumberCount = 0;
        private int[] m_iLines;
        private int[] m_iOffsets;

        internal REDocument(ISymbolDocumentWriter document)
        {
            this.m_document = document;
        }

        internal void AddLineNumberInfo(ISymbolDocumentWriter document, int iOffset, int iStartLine, int iStartColumn, int iEndLine, int iEndColumn)
        {
            this.EnsureCapacity();
            this.m_iOffsets[this.m_iLineNumberCount] = iOffset;
            this.m_iLines[this.m_iLineNumberCount] = iStartLine;
            this.m_iColumns[this.m_iLineNumberCount] = iStartColumn;
            this.m_iEndLines[this.m_iLineNumberCount] = iEndLine;
            this.m_iEndColumns[this.m_iLineNumberCount] = iEndColumn;
            this.m_iLineNumberCount++;
        }

        internal void EmitLineNumberInfo(ISymbolWriter symWriter)
        {
            if (this.m_iLineNumberCount != 0)
            {
                int[] destinationArray = new int[this.m_iLineNumberCount];
                Array.Copy(this.m_iOffsets, destinationArray, this.m_iLineNumberCount);
                int[] numArray2 = new int[this.m_iLineNumberCount];
                Array.Copy(this.m_iLines, numArray2, this.m_iLineNumberCount);
                int[] numArray3 = new int[this.m_iLineNumberCount];
                Array.Copy(this.m_iColumns, numArray3, this.m_iLineNumberCount);
                int[] numArray4 = new int[this.m_iLineNumberCount];
                Array.Copy(this.m_iEndLines, numArray4, this.m_iLineNumberCount);
                int[] numArray5 = new int[this.m_iLineNumberCount];
                Array.Copy(this.m_iEndColumns, numArray5, this.m_iLineNumberCount);
                symWriter.DefineSequencePoints(this.m_document, destinationArray, numArray2, numArray3, numArray4, numArray5);
            }
        }

        private void EnsureCapacity()
        {
            if (this.m_iLineNumberCount == 0)
            {
                this.m_iOffsets = new int[0x10];
                this.m_iLines = new int[0x10];
                this.m_iColumns = new int[0x10];
                this.m_iEndLines = new int[0x10];
                this.m_iEndColumns = new int[0x10];
            }
            else if (this.m_iLineNumberCount == this.m_iOffsets.Length)
            {
                int num = this.m_iLineNumberCount * 2;
                int[] destinationArray = new int[num];
                Array.Copy(this.m_iOffsets, destinationArray, this.m_iLineNumberCount);
                this.m_iOffsets = destinationArray;
                destinationArray = new int[num];
                Array.Copy(this.m_iLines, destinationArray, this.m_iLineNumberCount);
                this.m_iLines = destinationArray;
                destinationArray = new int[num];
                Array.Copy(this.m_iColumns, destinationArray, this.m_iLineNumberCount);
                this.m_iColumns = destinationArray;
                destinationArray = new int[num];
                Array.Copy(this.m_iEndLines, destinationArray, this.m_iLineNumberCount);
                this.m_iEndLines = destinationArray;
                destinationArray = new int[num];
                Array.Copy(this.m_iEndColumns, destinationArray, this.m_iLineNumberCount);
                this.m_iEndColumns = destinationArray;
            }
        }
    }
}

