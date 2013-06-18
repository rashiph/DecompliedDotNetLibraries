namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime.InteropServices;

    internal sealed class PDBReader : IDisposable
    {
        private const string IMetaDataImportGuid = "7DAC8207-D3AE-4c75-9B67-92801A497D44";
        private ISymUnmanagedReader symReader;

        public PDBReader(string assemblyPath)
        {
            object unknown = null;
            System.Workflow.ComponentModel.Compiler.IMetaDataDispenser o = null;
            try
            {
                Guid riid = new Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44");
                o = (System.Workflow.ComponentModel.Compiler.IMetaDataDispenser) new System.Workflow.ComponentModel.Compiler.MetaDataDispenser();
                o.OpenScope(assemblyPath, 0, ref riid, out unknown);
                this.symReader = (ISymUnmanagedReader) new CorSymReader_SxS();
                this.symReader.Initialize(unknown, assemblyPath, null, null);
            }
            finally
            {
                if (unknown != null)
                {
                    Marshal.ReleaseComObject(unknown);
                }
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
        }

        private void Dispose()
        {
            if (this.symReader != null)
            {
                Marshal.ReleaseComObject(this.symReader);
                this.symReader = null;
            }
        }

        ~PDBReader()
        {
            this.Dispose();
        }

        public void GetSourceLocationForOffset(uint methodDef, uint offset, out string fileLocation, out uint line, out uint column)
        {
            fileLocation = null;
            line = 0;
            column = 0;
            ISymUnmanagedMethod o = null;
            ISymUnmanagedDocument[] documents = null;
            uint pointsCount = 0;
            try
            {
                o = this.symReader.GetMethod(methodDef);
                pointsCount = o.GetSequencePointCount();
                documents = new ISymUnmanagedDocument[pointsCount];
                uint[] offsets = new uint[pointsCount];
                uint[] lines = new uint[pointsCount];
                uint[] columns = new uint[pointsCount];
                uint[] endLines = new uint[pointsCount];
                uint[] endColumns = new uint[pointsCount];
                o.GetSequencePoints(pointsCount, out pointsCount, offsets, documents, lines, columns, endLines, endColumns);
                uint index = 1;
                while (index < pointsCount)
                {
                    if (offsets[index] > offset)
                    {
                        break;
                    }
                    index++;
                }
                index--;
                while ((columns[index] == 0) && (index > 0))
                {
                    index--;
                }
                while ((index < pointsCount) && (columns[index] == 0))
                {
                    index++;
                }
                if ((index >= pointsCount) || (columns[index] == 0))
                {
                    index = 0;
                }
                line = lines[index];
                column = columns[index];
                ISymUnmanagedDocument document = documents[index];
                uint urlLength = 0x105;
                string url = new string('\0', (int) urlLength);
                document.GetURL(urlLength, out urlLength, url);
                fileLocation = url.Substring(0, ((int) urlLength) - 1);
            }
            finally
            {
                for (uint i = 0; i < pointsCount; i++)
                {
                    if (documents[i] != null)
                    {
                        Marshal.ReleaseComObject(documents[i]);
                    }
                }
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

